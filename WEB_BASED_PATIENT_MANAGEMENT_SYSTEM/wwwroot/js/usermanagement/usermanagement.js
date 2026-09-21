// ============================================================
// usermanagement.js — User Management module (Views/UserManagement/Index.cshtml)
// Add / Edit User modals and table search. Server-rendered values are
// provided by the view as window.userManagementPageData before this file loads.
// ============================================================

// ---- Add User modal ----

(() => {
    const data = window.userManagementPageData;
    const modal = document.getElementById('addUserModal');
    const form = document.getElementById('addUserForm');
    const fullName = document.getElementById('addUserFullName');
    const username = document.getElementById('addUserUsername');
    const password = document.getElementById('addUserPassword');
    const confirmPassword = document.getElementById('addUserConfirmPassword');
    const submitButton = document.getElementById('addUserSubmit');
    const error = document.getElementById('addUserError');

    if (!data || !modal || !form || !fullName || !username || !password || !confirmPassword || !submitButton || !error) return;

    // Validation (parehas sa server); ang password dili ibalik sa page.
    const validator = FormValidation.create(form, () => [
        { field: fullName, test: el => FormValidation.rules.personName(el.value) },
        { field: username, test: el => checkUsername(el.value) },
        {
            field: password, test: el => !el.value ? 'Kinahanglan kini nga field.'
                : el.value.length < 8 ? 'Labing menos 8 ka karakter.'
                : el.value.length > 100 ? 'Hangtod 100 ka karakter lang.' : ''
        },
        { field: confirmPassword, test: el => !el.value ? 'Kinahanglan kini nga field.' : (el.value !== password.value ? 'Dili parehas ang password.' : '') }
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
        showServerError(modal, error, reopen, { FullName: fullName, Username: username, Password: password, ConfirmPassword: confirmPassword });
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// Username: 3-50 ka letra, numero, . _ - lang.
function checkUsername(value) {
    const text = String(value ?? '').trim();
    if (!text) return 'Kinahanglan kini nga field.';
    if (text.length < 3 || text.length > 50) return 'Gikan 3 hangtod 50 ka karakter.';
    return /^[A-Za-z0-9._-]+$/.test(text) ? '' : 'Letra, numero, . _ - lang.';
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
    const submitButton = document.getElementById('editUserSubmit');
    const error = document.getElementById('editUserError');

    if (!data || !modal || !form || !id || !fullName || !username || !submitButton || !error) return;

    // Ngalan ug username ra; ang role dili usbon.
    function fill(user) {
        id.value = user.id;
        fullName.value = user.fullName || '';
        username.value = user.username || '';
    }

    function hideError() {
        error.textContent = '';
        error.classList.add('d-none');
    }

    // Validation (parehas sa server).
    const validator = FormValidation.create(form, () => [
        { field: fullName, test: el => FormValidation.rules.personName(el.value) },
        { field: username, test: el => checkUsername(el.value) }
    ]);

    modal.addEventListener('show.bs.modal', event => {
        const button = event.relatedTarget;
        if (!button) return;

        fill({
            id: button.dataset.userId,
            fullName: button.dataset.fullName,
            username: button.dataset.username
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
        showServerError(modal, error, reopen, { FullName: fullName, Username: username });
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
