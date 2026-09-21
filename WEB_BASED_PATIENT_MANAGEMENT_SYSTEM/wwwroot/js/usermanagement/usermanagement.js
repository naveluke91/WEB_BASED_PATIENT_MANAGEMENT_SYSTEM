// ============================================================
// usermanagement.js — User Management module (Views/UserManagement/Index.cshtml)
// Add / Edit / Reset Password modals and table search. Server-rendered values are
// provided by the view as window.userManagementPageData before this file loads.
// Ang Role field makita sa SuperAdmin ra; ang Reset Password modal para sa Admin (Staff) ug SuperAdmin (Admin ug Staff).
// ============================================================

// ---- Validation messages (English, same rules as UserManagementController) ----
const UM_MSG = {
    required: 'This field is required.',
    invalidName: "Enter a valid name (letters, spaces, . ' and - only).",
    nameTooLong: 'Use 150 characters or fewer.',
    usernameLength: 'Username must be 3 to 50 characters.',
    usernameChars: 'Username can only use letters, numbers, dots, dashes and underscores.',
    selectRole: 'Select Admin or Staff.',
    passwordMin: 'Use at least 8 characters.',
    passwordMax: 'Use 100 characters or fewer.',
    mismatch: 'Passwords do not match.'
};

// Full name: letters, space, . ' - ; labing menos 2 ka letra.
function checkFullName(value) {
    const text = String(value ?? '').trim();
    if (!text) return UM_MSG.required;
    if (text.length > 150) return UM_MSG.nameTooLong;
    const validChars = /^[\p{L}\p{M} .'’\-]+$/u.test(text);
    const letters = (text.match(/\p{L}/gu) || []).length;
    return validChars && letters >= 2 ? '' : UM_MSG.invalidName;
}

// Confirm password: kinahanglan parehas sa password.
function checkConfirm(value, password) {
    if (!value) return UM_MSG.required;
    return value !== password ? UM_MSG.mismatch : '';
}

// ---- Add User modal ----

(() => {
    const data = window.userManagementPageData;
    const modal = document.getElementById('addUserModal');
    const form = document.getElementById('addUserForm');
    const fullName = document.getElementById('addUserFullName');
    const username = document.getElementById('addUserUsername');
    const role = document.getElementById('addUserRole'); // wala kini sa Admin
    const password = document.getElementById('addUserPassword');
    const confirmPassword = document.getElementById('addUserConfirmPassword');
    const submitButton = document.getElementById('addUserSubmit');
    const error = document.getElementById('addUserError');

    if (!data || !modal || !form || !fullName || !username || !password || !confirmPassword || !submitButton || !error) return;

    // Validation (parehas sa server); ang password dili ibalik sa page.
    const validator = FormValidation.create(form, () => [
        { field: fullName, test: el => checkFullName(el.value) },
        { field: username, test: el => checkUsername(el.value) },
        ...(role ? [{ field: role, test: el => checkRole(el.value) }] : []),
        { field: password, test: el => checkPassword(el.value) },
        { field: confirmPassword, test: el => checkConfirm(el.value, password.value) }
    ]);

    // A second click would submit the same account twice.
    form.addEventListener('submit', event => {
        if (!validator.validate()) {
            event.preventDefault();
            return;
        }
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        validator.reset();
        error.textContent = '';
        error.classList.add('d-none');
        submitButton.disabled = false;
    });

    // Reopened after the server rejected the form (passwords are never sent back).
    const reopen = data.reopenForm;
    if (reopen && reopen.mode === 'add') {
        fullName.value = reopen.fullName || '';
        username.value = reopen.username || '';
        if (role) role.value = reopen.role || '';
        showServerError(modal, error, reopen, { FullName: fullName, Username: username, Role: role, Password: password, ConfirmPassword: confirmPassword });
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// Username: 3-50 ka letra, numero, . _ - lang.
function checkUsername(value) {
    const text = String(value ?? '').trim();
    if (!text) return UM_MSG.required;
    if (text.length < 3 || text.length > 50) return UM_MSG.usernameLength;
    return /^[A-Za-z0-9._-]+$/.test(text) ? '' : UM_MSG.usernameChars;
}

// Role: Admin o Staff ra (SuperAdmin ra ang naay Role field).
function checkRole(value) {
    return ['Admin', 'Staff'].includes(value) ? '' : UM_MSG.selectRole;
}

// Password sa Admin/Staff: 8-100 ka karakter.
function checkPassword(value) {
    if (!value) return UM_MSG.required;
    if (value.length < 8) return UM_MSG.passwordMin;
    return value.length > 100 ? UM_MSG.passwordMax : '';
}

// Sayop gikan sa server: i-marka ang field kung nahibaw-an, kung dili ipakita sa taas.
function showServerError(modal, errorBox, reopen, fields) {
    const field = reopen.field ? fields[reopen.field] : null;
    if (field) {
        modal.addEventListener('shown.bs.modal', () => FormValidation.setError(field, reopen.error), { once: true });
    } else {
        errorBox.textContent = reopen.error;
        errorBox.classList.remove('d-none');
    }
}

// ---- Edit User modal ----

(() => {
    const data = window.userManagementPageData;
    const modal = document.getElementById('editUserModal');
    const form = document.getElementById('editUserForm');
    const id = document.getElementById('editUserId');
    const fullName = document.getElementById('editUserFullName');
    const username = document.getElementById('editUserUsername');
    const role = document.getElementById('editUserRole'); // wala kini sa Admin
    const submitButton = document.getElementById('editUserSubmit');
    const error = document.getElementById('editUserError');

    if (!data || !modal || !form || !id || !fullName || !username || !submitButton || !error) return;

    function fill(user) {
        id.value = user.id;
        fullName.value = user.fullName || '';
        username.value = user.username || '';
        if (role) role.value = user.role || '';
    }

    function hideError() {
        error.textContent = '';
        error.classList.add('d-none');
    }

    // Validation (parehas sa server).
    const validator = FormValidation.create(form, () => [
        { field: fullName, test: el => checkFullName(el.value) },
        { field: username, test: el => checkUsername(el.value) },
        ...(role ? [{ field: role, test: el => checkRole(el.value) }] : [])
    ]);

    modal.addEventListener('show.bs.modal', event => {
        const button = event.relatedTarget;
        if (!button) return;

        fill({
            id: button.dataset.userId,
            fullName: button.dataset.fullName,
            username: button.dataset.username,
            role: button.dataset.role
        });
        hideError();
        validator.reset();
    });

    form.addEventListener('submit', event => {
        if (!validator.validate()) {
            event.preventDefault();
            return;
        }
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        hideError();
        validator.reset();
        submitButton.disabled = false;
    });

    // Reopened after the server rejected the form.
    const reopen = data.reopenForm;
    if (reopen && reopen.mode === 'edit') {
        fill(reopen);
        showServerError(modal, error, reopen, { FullName: fullName, Username: username, Role: role });
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// ---- Reset Password modal (Admin: Staff ra; SupAdmin: Admin ug Staff) ----

(() => {
    const data = window.userManagementPageData;
    const modal = document.getElementById('resetUserModal');
    const form = document.getElementById('resetUserForm');
    const id = document.getElementById('resetUserId');
    const accountName = document.getElementById('resetUserName');
    const password = document.getElementById('resetUserPassword');
    const confirmPassword = document.getElementById('resetUserConfirmPassword');
    const submitButton = document.getElementById('resetUserSubmit');
    const error = document.getElementById('resetUserError');

    if (!data || !modal || !form || !id || !accountName || !password || !confirmPassword || !submitButton || !error) return;

    const validator = FormValidation.create(form, () => [
        { field: password, test: el => checkPassword(el.value) },
        { field: confirmPassword, test: el => checkConfirm(el.value, password.value) }
    ]);

    modal.addEventListener('show.bs.modal', event => {
        const button = event.relatedTarget;
        if (!button) return;

        id.value = button.dataset.userId || '';
        accountName.textContent = button.dataset.fullName || '';
        validator.reset();
    });

    form.addEventListener('submit', event => {
        if (!validator.validate()) {
            event.preventDefault();
            return;
        }
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        validator.reset();
        error.textContent = '';
        error.classList.add('d-none');
        submitButton.disabled = false;
    });

    // Reopened after the server rejected the form (passwords are never sent back).
    const reopen = data.reopenForm;
    if (reopen && reopen.mode === 'reset') {
        id.value = reopen.id || '';
        accountName.textContent = reopen.fullName || '';
        showServerError(modal, error, reopen, { NewPassword: password, ConfirmPassword: confirmPassword });
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// ---- User table search ----

document.getElementById('userSearch')?.addEventListener('input', function () {
    const query = this.value.trim().toLowerCase();
    document.querySelectorAll('#userTableBody .patient-row').forEach(function (row) {
        row.style.display = (row.dataset.search || '').includes(query) ? '' : 'none';
    });
});
