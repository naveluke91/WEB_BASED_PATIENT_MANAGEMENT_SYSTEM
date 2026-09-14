using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Controllers;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Data;
using WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models;

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

// Sign-in is switched off for now: no page requires it and AccountController is
// disabled, so this cookie setup never redirects anyone. Kept for the later
// Admin/Staff sign-in task.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;

        // The account is checked on every request, so a deleted account is signed
        // out and a changed name or role takes effect straight away.
        options.Events.OnValidatePrincipal = async context =>
        {
            var idValue = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var account = int.TryParse(idValue, out var accountId)
                ? await db.UserAccounts.AsNoTracking().FirstOrDefaultAsync(u => u.Id == accountId)
                : null;

            if (account == null)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var principal = context.Principal!;
            if (principal.FindFirst(ClaimTypes.Role)?.Value != account.Role
                || principal.FindFirst(ClaimTypes.Name)?.Value != account.Username
                || principal.FindFirst(ClaimTypes.GivenName)?.Value != account.FullName)
            {
                context.ReplacePrincipal(AccountController.CreatePrincipal(account));
                context.ShouldRenew = true;
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Patients/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Default route — mag-sugod sa Patients Index page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Patients}/{action=Index}/{id?}");

app.Run();
