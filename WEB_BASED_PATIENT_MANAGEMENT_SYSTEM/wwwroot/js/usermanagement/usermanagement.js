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
    const role = document.getElementById('addUserRole');
    const password = document.getElementById('addUserPassword');
    const confirmPassword = document.getElementById('addUserConfirmPassword');
    const submitButton = document.getElementById('addUserSubmit');
    const error = document.getElementById('addUserError');

    if (!data || !modal || !form || !fullName || !username || !role || !password || !confirmPassword || !submitButton || !error) return;

    // The browser shows this message on Save while the passwords differ
    // (the server checks it again).
    function checkPasswordsMatch() {
        const mismatch = confirmPassword.value !== '' && confirmPassword.value !== password.value;
        confirmPassword.setCustomValidity(mismatch ? 'Passwords do not match.' : '');
    }

    password.addEventListener('input', checkPasswordsMatch);
    confirmPassword.addEventListener('input', checkPasswordsMatch);

    // A second click would submit the same account twice.
    form.addEventListener('submit', () => {
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        checkPasswordsMatch();
        error.textContent = '';
        error.classList.add('d-none');
        submitButton.disabled = false;
    });

    // Reopened after the server rejected the form (passwords are never sent back).
    const reopen = data.reopenForm;
    if (reopen && reopen.mode === 'add') {
        fullName.value = reopen.fullName || '';
        username.value = reopen.username || '';
        role.value = reopen.role || '';
        error.textContent = reopen.error;
        error.classList.remove('d-none');
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// ---- Edit User modal ----

(() => {
    const data = window.userManagementPageData;
    const modal = document.getElementById('editUserModal');
    const form = document.getElementById('editUserForm');
    const id = document.getElementById('editUserId');
    const fullName = document.getElementById('editUserFullName');
    const username = document.getElementById('editUserUsername');
    const role = document.getElementById('editUserRole');
    const roleLocked = document.getElementById('editUserRoleLocked');
    const roleNote = document.getElementById('editUserRoleNote');
    const submitButton = document.getElementById('editUserSubmit');
    const error = document.getElementById('editUserError');

    if (!data || !modal || !form || !id || !fullName || !username || !role || !roleLocked || !roleNote || !submitButton || !error) return;

    // The signed-in Admin cannot change their own role, so the list is locked
    // and the current role is sent in the hidden field instead.
    function fill(user) {
        const isSelf = String(user.id) === String(data.currentUserId);
        id.value = user.id;
        fullName.value = user.fullName || '';
        username.value = user.username || '';
        role.value = user.role || '';
        roleLocked.value = user.role || '';
        role.disabled = isSelf;
        roleLocked.disabled = !isSelf;
        roleNote.hidden = !isSelf;
    }

    function hideError() {
        error.textContent = '';
        error.classList.add('d-none');
    }

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
    });

    form.addEventListener('submit', () => {
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        hideError();
        submitButton.disabled = false;
    });

    // Reopened after the server rejected the form.
    const reopen = data.reopenForm;
    if (reopen && reopen.mode === 'edit') {
        fill(reopen);
        error.textContent = reopen.error;
        error.classList.remove('d-none');
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
