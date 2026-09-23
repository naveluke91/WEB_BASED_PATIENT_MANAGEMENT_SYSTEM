using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services;

var builder = WebApplication.CreateBuilder(args);

// I-register ang MVC controllers ug views
builder.Services.AddControllersWithViews();

// I-register ang ApplicationDbContext gamit ang SQL Server connection string
// Ang connection string makita sa appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Sign-in: an encrypted cookie holds the account's id, username, name and role.
// Passwords are stored as salted hashes by ASP.NET Core's built-in PasswordHasher.
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();

// SMTP settings and sender credentials are supplied through configuration, user-secrets, or environment variables.
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<EmailSender>();

// Password-reset state is server-side and expires quickly.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.Name = ".EBH.PasswordReset";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Redirect unauthenticated users to /Account/Login.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Secure cookie kung HTTPS (production).
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        // The account is checked on every request, so a deleted account is signed
        // out and a changed name or role takes effect straight away.
        options.Events.OnValidatePrincipal = async context =>
        {
            var idValue = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var account = int.TryParse(idValue, out var accountId)
                ? await db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(u => u.Id == accountId)
                : null;

            // Bag-o nga password = logout sa daan nga session.
            var principal = context.Principal!;
            if (account == null || !AccountController.StampMatches(principal, account))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            if (account.Role != UserRoles.Admin && account.Role != UserRoles.Staff)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            if (principal.FindFirst(ClaimTypes.Role)?.Value != account.Role
                || principal.FindFirst(ClaimTypes.Name)?.Value != account.Username
                || principal.FindFirst(ClaimTypes.GivenName)?.Value != account.FullName)
            {
                context.ReplacePrincipal(AccountController.CreatePrincipal(account));
                context.ShouldRenew = true;
            }
        };
    });

// Kinahanglan naka-login sa tanan nga page; ang Login ug Setup [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var passwordResetPermitLimit = builder.Configuration.GetValue("PasswordResetRateLimit:PermitLimit", 10);
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(AccountController.PasswordResetRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = passwordResetPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Redirect("/Account/ForgotPassword?busy=1");
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Sample Staff1 ug Admin para sa local testing: Development ra, dili sa Production.
if (app.Environment.IsDevelopment())
    AccountController.EnsureSampleAccounts(app.Services, app.Logger);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Patients/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseRateLimiter();
app.UseAuthentication();

// Dili i-cache ang page sa naka-login, aron dili makita pinaagi sa Back human sa logout.
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        // Security headers: walay framing, walay MIME sniffing, walay referrer sa gawas.
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "SAMEORIGIN";
        context.Response.Headers["Referrer-Policy"] = "same-origin";
        if (context.User.Identity?.IsAuthenticated == true)
            context.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    });
    await next();
});

app.UseAuthorization();

// Temporary password: dili pa makasulod hangtod mausab ang password.
app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    var action = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
    if (context.User.Identity?.IsAuthenticated == true && action != null
        && endpoint!.Metadata.GetMetadata<IAllowAnonymous>() == null
        && action.ActionName is not ("ChangePassword" or "Logout" or "AccessDenied")
        && int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var accountId))
    {
        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
        if (await db.UserAccounts.AnyAsync(u => u.Id == accountId && u.MustChangePassword))
        {
            context.Response.Redirect("/Account/ChangePassword");
            return;
        }
    }

    await next();
});

// Default route — mag-sugod sa Patients Index page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Patients}/{action=Index}/{id?}");

app.Run();
