// ============================================================
// superadmin.js — SuperAdmin pages (Views/SuperAdmin, _SuperAdminLayout)
// Validation (same rules as SuperAdminController), modals on Manage Users,
// and reopening a form the server rejected. Passwords are never refilled.
// ============================================================

document.addEventListener('DOMContentLoaded', () => {
    const FV = window.FormValidation;
    if (!FV) return;

    const data = window.superAdminPageData || {};
    const ROLES = ['Admin', 'Staff'];

    // ---- Mga lagda (parehas sa server) ----
    const required = value => FV.isBlank(value) ? FV.MSG.required : '';

    function strongPassword(value) {
        const text = String(value ?? '');
        if (!text.trim()) return FV.MSG.required;
        if (text.length < 12) return 'Labing menos 12 ka karakter.';
        if (text.length > 100) return 'Hangtod 100 ka karakter lang.';
        const hasSymbol = /[^\p{L}\p{N}\s]/u.test(text);
        if (!/\p{Lu}/u.test(text) || !/\p{Ll}/u.test(text) || !/\p{N}/u.test(text) || !hasSymbol)
            return 'Gamiti og dako ug gamay nga letra, numero, ug simbolo.';
        return '';
    }

    function basicPassword(value) {
        const text = String(value ?? '');
        if (!text.trim()) return FV.MSG.required;
        if (text.length < 8) return 'Labing menos 8 ka karakter.';
        return text.length > 100 ? 'Hangtod 100 ka karakter lang.' : '';
    }

    function username(value) {
        const text = String(value ?? '').trim();
        if (!text) return FV.MSG.required;
        return /^[A-Za-z0-9._-]{3,50}$/.test(text) ? '' : '3-50 ka letra, numero, . _ - lang.';
    }

    function email(value) {
        const text = String(value ?? '').trim();
        if (!text) return FV.MSG.required;
        return /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(text) ? '' : 'Dili valid ang Gmail.';
    }

    const confirmOf = source => value => !value ? FV.MSG.required : (value !== source.value ? 'Dili parehas ang password.' : '');

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
        check(byId('saRecoveryCode'), value => /^\d{8}$/.test(String(value ?? '').trim()) ? '' : '8 ka numero ang code.')
    ]);

    // ---- Manage Users ----
    const role = value => ROLES.includes(value) ? '' : 'Pilia ang Admin o Staff.';

    const forms = {
        add: {
            modal: byId('saAddModal'),
            form: byId('saAddForm'),
            checks: () => [
                check(byId('saAddFullName'), FV.rules.personName),
                check(byId('saAddUsername'), username),
                check(byId('saAddRole'), role),
                check(byId('saAddPassword'), basicPassword),
                check(byId('saAddConfirmPassword'), confirmOf(byId('saAddPassword')))
            ]
        },
        edit: {
            modal: byId('saEditModal'),
            form: byId('saEditForm'),
            checks: () => [
                check(byId('saEditFullName'), FV.rules.personName),
                check(byId('saEditUsername'), username),
                check(byId('saEditRole'), role)
            ]
        },
        reset: {
            modal: byId('saResetModal'),
            form: byId('saResetForm'),
            checks: () => [
                check(byId('saResetNewPassword'), basicPassword),
                check(byId('saResetConfirmPassword'), confirmOf(byId('saResetNewPassword')))
            ]
        },
        // ---- Settings ----
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

    Object.values(forms).forEach(entry => {
        entry.validator = attach(entry.form, entry.checks);
        if (!entry.modal || !entry.form) return;

        // Limpyo human sirado ang modal.
        entry.modal.addEventListener('hidden.bs.modal', () => {
            entry.form.reset();
            entry.validator.reset();
            entry.form.querySelector('[type="submit"]')?.removeAttribute('disabled');
        });
    });

    // Punan ang modal gikan sa button (textContent ra, walay HTML).
    function fillFromButton(modalId, handler) {
        byId(modalId)?.addEventListener('show.bs.modal', event => {
            const button = event.relatedTarget;
            if (button) handler(button.dataset);
        });
    }

    fillFromButton('saEditModal', d => {
        byId('saEditId').value = d.id || '';
        byId('saEditFullName').value = d.fullName || '';
        byId('saEditUsername').value = d.username || '';
        byId('saEditRole').value = d.role || '';
    });

    fillFromButton('saResetModal', d => {
        byId('saResetId').value = d.id || '';
        byId('saResetName').textContent = d.fullName || '';
    });

    fillFromButton('saDeleteModal', d => {
        byId('saDeleteId').value = d.id || '';
        byId('saDeleteMessage').textContent = `Delete ${d.roleLabel || 'account'} "${d.fullName || ''}"?`;
    });

    // Gi-reject sa server: ablihan pag-usab ug markahan ang mga field.
    const rejected = data.form;
    const entry = rejected ? forms[rejected.form] : null;
    if (!entry?.form) return;

    const values = rejected.values || {};
    const setValue = (name, value) => {
        const field = entry.form.elements[name];
        if (field && value !== undefined && value !== null) field.value = value;
    };
    setValue('Id', values.id);
    setValue('FullName', values.fullName);
    setValue('Username', values.username);
    setValue('Role', values.role);
    setValue('NewUsername', values.newUsername);
    setValue('RecoveryEmail', values.recoveryEmail);
    if (rejected.form === 'reset') byId('saResetName').textContent = values.fullName || '';

    const showErrors = () => entry.validator.showErrors(rejected.errors, key => entry.form.elements[key]);
    if (entry.modal) {
        entry.modal.addEventListener('shown.bs.modal', showErrors, { once: true });
        bootstrap.Modal.getOrCreateInstance(entry.modal).show();
    } else {
        showErrors();
    }
});
