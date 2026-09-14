// ============================================================
// appointment.js — Appointments module (Views/Appointments/Index.cshtml)
// Time slots, New / Existing / View / Edit appointment modals,
// confirm-as-new-patient modal, table search and delete confirmation.
// ============================================================

// ---------------------------------------------------------------
// Time slots
// ---------------------------------------------------------------
const ALL_SLOTS = [
    { value: "09:00", label: "9:00 AM" },
    { value: "10:00", label: "10:00 AM" },
    { value: "11:00", label: "11:00 AM" },
    { value: "13:00", label: "1:00 PM" },
    { value: "14:00", label: "2:00 PM" },
    { value: "15:00", label: "3:00 PM" }
];

function populateTimeSelect(selectEl, bookedTimes, currentValue = null) {
    selectEl.innerHTML = '<option value="">-- Select Time --</option>';
    ALL_SLOTS.forEach(slot => {
        const isBooked = bookedTimes.includes(slot.value) && slot.value !== currentValue;
        const opt = document.createElement('option');
        opt.value = slot.value;
        opt.textContent = isBooked ? `${slot.label} (Booked - Unavailable)` : slot.label;
        opt.disabled = isBooked;
        if (isBooked) opt.style.color = '#aaa';
        if (slot.value === currentValue) opt.selected = true;
        selectEl.appendChild(opt);
    });
}

async function refreshTimeDropdown(selectEl, loadingEl, date, excludeId = null, currentValue = null) {
    if (!date) { selectEl.innerHTML = '<option value="">-- Select Time --</option>'; return; }
    loadingEl.style.display = 'block';
    selectEl.disabled = true;
    try {
        let url = `/Appointments/GetBookedTimes?date=${date}`;
        if (excludeId) url += `&excludeId=${excludeId}`;
        const res = await fetch(url);
        const booked = await res.json();
        populateTimeSelect(selectEl, booked, currentValue);
    } catch (e) {
        populateTimeSelect(selectEl, [], currentValue);
    } finally {
        loadingEl.style.display = 'none';
        selectEl.disabled = false;
    }
}

// ---------------------------------------------------------------
// Inline modal error helpers (NO browser alert)
// ---------------------------------------------------------------
function showModalError(id, msg) {
    const el = document.getElementById(id);
    if (!el) return;
    el.textContent = msg;
    el.style.display = 'block';
}

function hideModalError(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.textContent = '';
    el.style.display = 'none';
}

// ---------------------------------------------------------------
// NEW APPOINTMENT modal — populate time slots on date change
// ---------------------------------------------------------------
document.getElementById('modalNewAppointment').addEventListener('show.bs.modal', function () {
    const dateInput = document.getElementById('newDate');
    if (dateInput && dateInput.value) {
        refreshTimeDropdown(
            document.getElementById('newTimeSelect'),
            document.getElementById('newTimeLoading'),
            dateInput.value
        );
    } else {
        document.getElementById('newTimeSelect').innerHTML = '<option value="">-- Select Time --</option>';
    }
});

document.getElementById('newDate').addEventListener('change', function () {
    refreshTimeDropdown(
        document.getElementById('newTimeSelect'),
        document.getElementById('newTimeLoading'),
        this.value
    );
});

// ---------------------------------------------------------------
// EXISTING APPOINTMENT modal — search + autofill name & contact
// ---------------------------------------------------------------
let searchTimeout = null;

document.getElementById('existSearch').addEventListener('focus', function () {
    clearTimeout(searchTimeout);
    fetchExistSuggestions(this.value.trim());
});

document.getElementById('existSearch').addEventListener('input', function () {
    clearTimeout(searchTimeout);
    const q = this.value.trim();
    searchTimeout = setTimeout(() => fetchExistSuggestions(q), 250);
});

async function fetchExistSuggestions(q) {
    try {
        const res = await fetch(`/Appointments/SearchAppointments?q=${encodeURIComponent(q)}`);
        const results = await res.json();
        renderExistSuggestions(results, q);
    } catch (e) {
        console.warn('Search failed:', e);
    }
}

function renderExistSuggestions(results, q = '') {
    const ul = document.getElementById('existSuggestions');
    ul.innerHTML = '';
    if (!results.length) {
        if (q.length > 0) {
            const li = document.createElement('li');
            li.className = 'appt-exist-suggestion-empty';
            li.textContent = 'No patient found';
            ul.appendChild(li);
            ul.style.display = 'block';
        } else {
            ul.style.display = 'none';
        }
        return;
    }
    results.forEach(r => {
        const li = document.createElement('li');
        li.className = 'appt-exist-suggestion-item';
        li.textContent = r.patientName;
        li.addEventListener('click', () => selectExistPatient(r));
        ul.appendChild(li);
    });
    ul.style.display = 'block';
}

function selectExistPatient(r) {
    document.getElementById('existSuggestions').style.display = 'none';
    document.getElementById('existSearch').value   = r.patientName;
    document.getElementById('existPatientName').value = r.patientName;
    document.getElementById('existContactNo').value   = r.contactNo;
    document.getElementById('existPatientId').value   = r.patientId ?? '';
}

document.addEventListener('click', function (e) {
    if (!e.target.closest('#existSearch') && !e.target.closest('#existSuggestions')) {
        document.getElementById('existSuggestions').style.display = 'none';
    }
});

document.getElementById('modalExistingAppointment').addEventListener('show.bs.modal', function () {
    document.getElementById('existSearch').value = '';
    document.getElementById('existPatientName').value = '';
    document.getElementById('existContactNo').value = '';
    document.getElementById('existDate').value = '';
    document.getElementById('existPatientId').value = '';
    document.getElementById('existTimeSelect').innerHTML = '<option value="">-- Select Time --</option>';
    document.getElementById('existSuggestions').style.display = 'none';
});

document.getElementById('existDate').addEventListener('change', function () {
    refreshTimeDropdown(
        document.getElementById('existTimeSelect'),
        document.getElementById('existTimeLoading'),
        this.value
    );
});

// ---------------------------------------------------------------
// 3. VIEW APPOINTMENT modal
// ---------------------------------------------------------------
async function openViewModal(id) {
    try {
        const res = await fetch(`/Appointments/GetById/${id}`);
        if (!res.ok) throw new Error('Not found');
        const appt = await res.json();

        document.getElementById('viewPatientName').textContent = appt.patientName;
        document.getElementById('viewContactNo').textContent   = appt.contactNo;
        document.getElementById('viewDate').textContent        = appt.appointmentDate;
        document.getElementById('viewTime').textContent        = appt.appointmentTime;

        const badge = document.getElementById('viewStatusBadge');
        if (badge) {
            badge.textContent = appt.status;
            badge.className = `status-badge status-${appt.status.toLowerCase()}`;
        }

        new bootstrap.Modal(document.getElementById('modalViewAppointment')).show();
    } catch (e) {
        console.error('Could not load appointment details:', e);
    }
}

// ---------------------------------------------------------------
// 4. EDIT APPOINTMENT modal (Inline Existing Patient Search on Confirmed)
// ---------------------------------------------------------------
async function openEditModal(id) {
    try {
        const res = await fetch(`/Appointments/GetById/${id}`);
        if (!res.ok) throw new Error('Not found');
        const appt = await res.json();

        document.getElementById('editId').value          = appt.id;
        document.getElementById('editPatientName').value = appt.patientName;
        document.getElementById('editContactNo').value   = appt.contactNo;
        document.getElementById('editDate').value        = appt.appointmentDate;
        document.getElementById('editPatientId').value   = appt.patientId ?? '';

        // Remember ORIGINAL date/time so we can verify that a
        // "Rescheduled" save really changed the date or the time
        // (client-side check; server re-validates against the DB record).
        const editForm = document.getElementById('formEditAppointment');
        editForm.dataset.origDate = appt.appointmentDate;  // "yyyy-MM-dd"
        editForm.dataset.origTime = appt.appointmentTime;  // "HH:mm"

        const statusSelect = document.getElementById('editStatus');
        statusSelect.value = appt.status;

        hideModalError('editInlineError');

        // Reset active modes
        backToEditChoiceCards();

        // If Confirmed, always show choice cards and pre-highlight Existing if patient already linked
        handleEditStatusChange();
        if (appt.status === 'Confirmed' && appt.patientId) {
            document.getElementById('editExistingBtn').classList.add('active');
            document.getElementById('editPatientMode').value = 'existing';
        }

        // Refresh time dropdown
        await refreshTimeDropdown(
            document.getElementById('editTimeSelect'),
            document.getElementById('editTimeLoading'),
            appt.appointmentDate,
            appt.id,
            appt.appointmentTime
        );

        new bootstrap.Modal(document.getElementById('modalEditAppointment')).show();
    } catch (e) {
        console.error('Could not load edit appointment data:', e);
    }
}

// Pop up Choice Cards (Pic 2) directly below Status when Confirmed is chosen
const editStatusSelect        = document.getElementById('editStatus');
const editConfirmChoiceSection = document.getElementById('editConfirmChoiceSection');
const editChoiceCards          = document.getElementById('editChoiceCards');

function handleEditStatusChange() {
    const isCancelled  = editStatusSelect.value === 'Cancelled';
    const isConfirmed  = editStatusSelect.value === 'Confirmed';
    const editDateInput = document.getElementById('editDate');
    const editTimeInput = document.getElementById('editTimeSelect');

    hideModalError('editInlineError');

    if (isCancelled) {
        // Keep visible but remove required + min so browser won't block saving
        editDateInput.removeAttribute('required');
        editDateInput.removeAttribute('min');
        editTimeInput.removeAttribute('required');
        editConfirmChoiceSection.style.display = 'none';
    } else {
        // Restore required + min
        editDateInput.setAttribute('required', 'required');
        const tomorrow = new Date(); tomorrow.setDate(tomorrow.getDate() + 1);
        editDateInput.setAttribute('min', tomorrow.toISOString().split('T')[0]);
        editTimeInput.setAttribute('required', 'required');

        // Show choice cards when Confirmed, unless this appointment is
        // already linked to a registered Patient — it already has its
        // Patient, so don't ask Existing/New Patient again.
        const alreadyHasPatient = !!document.getElementById('editPatientId').value;
        if (isConfirmed && !alreadyHasPatient) {
            editConfirmChoiceSection.style.display = 'block';
        } else {
            editConfirmChoiceSection.style.display = 'none';
        }
    }
}

// Toggle: Existing Patient card — highlights when selected, untoggles if clicked again
function toggleEditExistingCard() {
    const existingBtn = document.getElementById('editExistingBtn');
    const modeInput   = document.getElementById('editPatientMode');


    if (existingBtn.classList.contains('active')) {
        existingBtn.classList.remove('active');
        modeInput.value = '';
    } else {
        existingBtn.classList.add('active');
        modeInput.value = 'existing';
    }
}

// Open separate New Patient modal from Edit modal
function openNewPatientModalFromEdit() {
    const apptId  = document.getElementById('editId').value;
    const name    = document.getElementById('editPatientName').value;
    const contact = document.getElementById('editContactNo').value;

    document.getElementById('confirmNewApptId').value   = apptId;
    document.getElementById('confirmNewFullName').value = name;
    document.getElementById('confirmNewContactNo').value = contact;



    // Hide edit modal and show new patient modal
    const editModalEl = document.getElementById('modalEditAppointment');
    const editModal = bootstrap.Modal.getInstance(editModalEl);
    if (editModal) editModal.hide();

    const newModalEl = document.getElementById('modalConfirmNewPatient');
    const newModal = new bootstrap.Modal(newModalEl);
    newModal.show();
}

function backToEditModalFromNewPatient() {
    const newModalEl = document.getElementById('modalConfirmNewPatient');
    const newModal = bootstrap.Modal.getInstance(newModalEl);
    if (newModal) newModal.hide();

    const editModalEl = document.getElementById('modalEditAppointment');
    const editModal = new bootstrap.Modal(editModalEl);
    editModal.show();
}

function backToEditChoiceCards() {
    document.getElementById('editPatientMode').value = '';
    document.getElementById('editExistingBtn').classList.remove('active');
}

// Auto-calculate Age on Confirm New Patient DOB
const confirmNewDobInput = document.getElementById('confirmNewDOB');
if (confirmNewDobInput) {
    confirmNewDobInput.addEventListener('change', function () {
        if (!this.value) { document.getElementById('confirmNewAge').value = ''; return; }
        const dob = new Date(this.value);
        const today = new Date();
        let age = today.getFullYear() - dob.getFullYear();
        const m = today.getMonth() - dob.getMonth();
        if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--;
        document.getElementById('confirmNewAge').value = age >= 0 ? age : '';
    });
}

// Handle Confirm New Patient form submission via AJAX
document.getElementById('formConfirmNewPatient').addEventListener('submit', async function (e) {
    e.preventDefault();
    hideModalError('confirmNewInlineError');
    try {
        const formData = new FormData(this);
        const res = await fetch(this.action, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const data = await res.json();
        if (data.success) {
            window.location.reload();
        } else {
            // Keep the modal open and show the message inline
            showModalError('confirmNewInlineError', data.message || 'Failed to save patient. Please check the fields.');
        }
    } catch (err) {
        console.error('Save failed:', err);
        showModalError('confirmNewInlineError', 'An unexpected error occurred while saving. Please try again.');
    }
});

editStatusSelect.addEventListener('change', handleEditStatusChange);

document.getElementById('editDate').addEventListener('change', async function () {
    const id = document.getElementById('editId').value;
    const timeSelect = document.getElementById('editTimeSelect');
    const selectedTime = timeSelect.value;

    await refreshTimeDropdown(
        timeSelect,
        document.getElementById('editTimeLoading'),
        this.value,
        id || null
    );

    // Keep the chosen time when that slot is still free on the new date,
    // so a reschedule can change only the date.
    const stillFree = Array.from(timeSelect.options)
        .some(option => option.value === selectedTime && !option.disabled);
    if (selectedTime && stillFree) timeSelect.value = selectedTime;
});

// Handle Edit Form Submission via AJAX
document.getElementById('formEditAppointment').addEventListener('submit', async function (e) {
    e.preventDefault();
    hideModalError('editInlineError');

    // Client-side pre-check, same rule as the server (which re-validates
    // against the saved record): Rescheduled needs a new date OR a new time.
    // Both sides use one format each — "yyyy-MM-dd" (date input / GetById)
    // and "HH:mm" (time slot values / GetById).
    const status = document.getElementById('editStatus').value;
    if (status === 'Rescheduled') {
        const origDate = this.dataset.origDate || '';
        const origTime = this.dataset.origTime || '';
        const newDate  = document.getElementById('editDate').value;
        const newTime  = document.getElementById('editTimeSelect').value;

        if (origDate === newDate && origTime === newTime) {
            showModalError('editInlineError', 'Please change the appointment date or time before rescheduling.');
            return;
        }
    }

    try {
        const formData = new FormData(this);
        const res = await fetch(this.action, {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const data = await res.json();
        if (data.success) {
            window.location.reload();
        } else {
            // Keep the modal open and show the message inline
            showModalError('editInlineError', data.message || 'Failed to save appointment. Please check the fields.');
        }
    } catch (err) {
        console.error('Edit save failed:', err);
        showModalError('editInlineError', 'An unexpected error occurred while saving. Please try again.');
    }
});

// ---------------------------------------------------------------
// Table search filter
// ---------------------------------------------------------------
document.getElementById('apptSearch').addEventListener('input', function () {
    const q = this.value.toLowerCase();
    document.querySelectorAll('#apptTableBody .patient-row').forEach(function (row) {
        const name    = (row.dataset.name    || '').toLowerCase();
        const contact = (row.dataset.contact || '').toLowerCase();
        row.style.display = (name.includes(q) || contact.includes(q)) ? '' : 'none';
    });
});

// ---------------------------------------------------------------
// Delete confirm
// ---------------------------------------------------------------
function confirmApptDelete(event, form, patientName) {
    event.preventDefault();
    if (confirm('Delete appointment for "' + patientName + '"?')) {
        form.submit();
    }
    return false;
}
