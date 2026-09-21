// ============================================================
// superadmin.js — SuperAdmin login / recovery pages (_SuperAdminLayout) and
// Settings (Views/SuperAdmin/Settings.cshtml). Same rules as SuperAdminController.
// Passwords are never refilled. Users management is in usermanagement.js.
// ============================================================

document.addEventListener('DOMContentLoaded', () => {
    const FV = window.FormValidation;
    if (!FV) return;

    const data = window.superAdminPageData || {};

    // ---- Mga lagda (parehas sa server) ; ang mga mensahe English ----
    const MSG = {
        required: 'This field is required.',
        usernameLength: 'Username must be 3 to 50 characters.',
        usernameChars: 'Username can only use letters, numbers, dots, dashes and underscores.',
        gmail: 'Enter a valid Gmail address.',
        code: 'The recovery code must be 8 digits.',
        min12: 'Use at least 12 characters.',
        max: 'Use 100 characters or fewer.',
        complexity: 'Use uppercase and lowercase letters, a number, and a symbol.',
        mismatch: 'Passwords do not match.'
    };

    const required = value => FV.isBlank(value) ? MSG.required : '';

    function strongPassword(value) {
        const text = String(value ?? '');
        if (!text.trim()) return MSG.required;
        if (text.length < 12) return MSG.min12;
        if (text.length > 100) return MSG.max;
        const hasSymbol = /[^\p{L}\p{N}\s]/u.test(text);
        if (!/\p{Lu}/u.test(text) || !/\p{Ll}/u.test(text) || !/\p{N}/u.test(text) || !hasSymbol)
            return MSG.complexity;
        return '';
    }

    function username(value) {
        const text = String(value ?? '').trim();
        if (!text) return MSG.required;
        if (text.length < 3 || text.length > 50) return MSG.usernameLength;
        return /^[A-Za-z0-9._-]+$/.test(text) ? '' : MSG.usernameChars;
    }

    function email(value) {
        const text = String(value ?? '').trim();
        if (!text) return MSG.required;
        return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(text) ? '' : MSG.gmail;
    }

    const confirmOf = source => value => !value ? MSG.required : (value !== source.value ? MSG.mismatch : '');

    // Mensahe ubos sa field (dili absolute), para dili matabunan.
    const check = (field, test) => ({ field, test: el => test(el.value), inline: true });

    // Validator sa usa ka form; i-disable ang submit human sa valid nga submit.
    function attach(form, getChecks) {
        if (!form) return null;
        const validator = FV.create(form, getChecks);
        form.addEventListener('submit', event => {
            if (!validator.validate()) {
                event.preventDefault();
                return;
            }
            form.querySelector('[type="submit"]')?.setAttribute('disabled', '');
        });
        return validator;
    }

    // Sayop nga gi-render sa server (tag helper): himuon nga FormValidation error.
    document.querySelectorAll('.sa-body span.field-validation-error[data-valmsg-for]').forEach(span => {
        const form = span.closest('form');
        const field = form?.elements[span.dataset.valmsgFor];
        const message = span.textContent.trim();
        if (!field || !message) return;
        span.remove();
        FV.setError(field, message, { inline: true });
    });

    const byId = id => document.getElementById(id);

    // ---- Sign-in ug recovery pages ----
    attach(byId('saLoginForm'), () => [
        check(byId('saLoginUsername'), required),
        check(byId('saLoginPassword'), required)
    ]);

    attach(byId('saChangePasswordForm'), () => [
        check(byId('saNewPassword'), strongPassword),
        check(byId('saConfirmPassword'), confirmOf(byId('saNewPassword')))
    ]);

    attach(byId('saResetPasswordForm'), () => [
        check(byId('saRecoveryNewPassword'), strongPassword),
        check(byId('saRecoveryConfirmPassword'), confirmOf(byId('saRecoveryNewPassword')))
    ]);

    attach(byId('saForgotForm'), () => [check(byId('saRecoveryEmail'), email)]);

    attach(byId('saVerifyForm'), () => [
        check(byId('saRecoveryCode'), value => /^\d{8}$/.test(String(value ?? '').trim()) ? '' : MSG.code)
    ]);

    // ---- Settings ----
    const forms = {
        username: {
            form: byId('saUsernameForm'),
            checks: () => [check(byId('saUsernameCurrent'), required), check(byId('saNewUsername'), username)]
        },
        password: {
            form: byId('saPasswordForm'),
            checks: () => [
                check(byId('saPasswordCurrent'), required),
                check(byId('saPasswordNew'), strongPassword),
                check(byId('saPasswordConfirm'), confirmOf(byId('saPasswordNew')))
            ]
        },
        recovery: {
            form: byId('saRecoveryForm'),
            checks: () => [check(byId('saRecoveryCurrent'), required), check(byId('saRecoveryEmailInput'), email)]
        }
    };

    Object.values(forms).forEach(entry => { entry.validator = attach(entry.form, entry.checks); });

    // Gi-reject sa server: markahan ang mga field.
    const rejected = data.form;
    const entry = rejected ? forms[rejected.form] : null;
    if (!entry?.form) return;

    const values = rejected.values || {};
    const setValue = (name, value) => {
        const field = entry.form.elements[name];
        if (field && value !== undefined && value !== null) field.value = value;
    };
    setValue('NewUsername', values.newUsername);
    setValue('RecoveryEmail', values.recoveryEmail);

    let first = null;
    Object.entries(rejected.errors || {}).forEach(([key, message]) => {
        const field = entry.form.elements[key];
        if (!field) return;
        FV.setError(field, message, { inline: true });
        first = first || field;
    });
    first?.focus();
});
