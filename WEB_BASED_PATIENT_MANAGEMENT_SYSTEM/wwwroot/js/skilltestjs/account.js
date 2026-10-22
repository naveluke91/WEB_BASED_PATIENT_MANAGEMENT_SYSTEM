// ============================================================
// skilltestjs/account.js — SKILL TEST features sa Login / Account pages
// (Login, Forgot/Reset Password, Change Password, Setup).
// Gi-load sa Views/Shared/_AuthLayout.cshtml ug sa Views/Account/MyPassword.cshtml.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// skilltestcss/account.css nga parehas og numero.
//
// I-UNCOMMENT UNA ANG [ST-ACCOUNT-0] (SETUP) sa Views/Shared/_AuthLayout.cshtml ug _Layout.cshtml
// aron ma-load kini nga file.
// ============================================================


// ==== [ST-ACCOUNT-1] START — Show/Hide password (eye icon) ====
// Gamit: makita sa user ang gi-type nga password aron dili masayop.
// (Parehas sa eye button sa Users → Add User modal.)
/*
document.querySelectorAll('input[type="password"]').forEach(input => {
    if (input.closest('.st-password-wrap')) return;

    const wrap = document.createElement('div');
    wrap.className = 'st-password-wrap';
    input.parentNode.insertBefore(wrap, input);
    wrap.appendChild(input);

    const toggle = document.createElement('button');
    toggle.type = 'button';
    toggle.className = 'st-password-toggle';
    toggle.setAttribute('aria-label', 'Show password');
    toggle.innerHTML = '<i class="bi bi-eye" aria-hidden="true"></i>';
    toggle.addEventListener('click', () => {
        const visible = input.type === 'password';
        input.type = visible ? 'text' : 'password';
        toggle.setAttribute('aria-label', visible ? 'Hide password' : 'Show password');
        toggle.querySelector('i').className = visible ? 'bi bi-eye-slash' : 'bi bi-eye';
    });
    wrap.appendChild(toggle);
});
*/
// ==== [ST-ACCOUNT-1] END ====


// ==== [ST-ACCOUNT-7] START — Password strength meter (Weak / Medium / Strong) ====
// Gamit: ipakita kung unsa ka lig-on ang bag-ong password samtang nag-type.
/*
document.querySelectorAll('input[name="NewPassword"], input[name="Password"]').forEach(input => {
    // Dili sa Login page (current password ra didto).
    if (input.autocomplete === 'current-password') return;

    const meter = document.createElement('div');
    meter.className = 'st-strength';
    meter.innerHTML = '<div class="st-strength-bar"><span></span></div><small class="st-strength-text"></small>';
    (input.closest('.st-password-wrap') || input).insertAdjacentElement('afterend', meter);

    const bar = meter.querySelector('span');
    const text = meter.querySelector('.st-strength-text');

    input.addEventListener('input', () => {
        const value = input.value;
        let score = 0;
        if (value.length >= 8) score++;
        if (/[A-Z]/.test(value) && /[a-z]/.test(value)) score++;
        if (/\d/.test(value)) score++;
        if (/[^A-Za-z0-9]/.test(value)) score++;

        const levels = [
            { label: '', width: 0, className: '' },
            { label: 'Weak', width: 25, className: 'weak' },
            { label: 'Fair', width: 50, className: 'fair' },
            { label: 'Good', width: 75, className: 'good' },
            { label: 'Strong', width: 100, className: 'strong' }
        ];
        const level = value ? levels[Math.max(score, 1)] : levels[0];
        bar.style.width = level.width + '%';
        bar.className = level.className;
        text.textContent = level.label ? `Password strength: ${level.label}` : '';
    });
});
*/
// ==== [ST-ACCOUNT-7] END ====
