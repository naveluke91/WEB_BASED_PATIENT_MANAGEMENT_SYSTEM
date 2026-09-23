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
// Validation (pula nga field + mensahe; walay alert)
// ---------------------------------------------------------------
const FV = window.FormValidation;

// Petsa sugod ugma lang.
function checkApptDate(el) {
    if (FV.isBlank(el.value)) return 'Pilia ang petsa.';
    const date = FV.parseDate(el.value);
    if (!date) return FV.MSG.date;
    return date > FV.today() ? '' : 'Pilia ang petsa sugod ugma.';
}

// Valid ra nga oras nga dili pa booked.
function checkApptTime(el) {
    if (!el.value) return 'Pilia ang oras.';
    if (!ALL_SLOTS.some(slot => slot.value === el.value)) return 'Pilia ang valid nga oras.';
    return el.selectedOptions[0]?.disabled ? 'Nagamit na kini nga schedule.' : '';
}

// Mga field sa form base sa name (para sa sayop gikan sa server).
const fieldByName = form => name => form.querySelector(`[name="${name}"]`);

const newApptForm = document.getElementById('formNewAppointment');
const newApptValidator = FV.create(newApptForm, () => [
    { field: document.getElementById('newPatientName'), test: el => FV.rules.personName(el.value) },
    { field: document.getElementById('newContactNo'), test: el => FV.rules.contact(el.value) },
    { field: document.getElementById('newDate'), test: checkApptDate },
    { field: document.getElementById('newTimeSelect'), test: checkApptTime }
]);
newApptForm.addEventListener('submit', e => {
    if (!newApptValidator.validate()) e.preventDefault();
});

const existApptForm = document.getElementById('formExistingAppointment');
const existApptValidator = FV.create(existApptForm, () => [
    {
        // Kinahanglan napili gikan sa listahan.
        field: document.getElementById('existSearch'), test: el => {
            const pickedName = document.getElementById('existPatientName').value;
            const pickedId = document.getElementById('existPatientId').value;
            return pickedId && pickedName && el.value.trim() === pickedName ? '' : 'Pilia ang pasyente gikan sa listahan.';
        }
    },
    { field: document.getElementById('existContactNo'), test: el => FV.rules.contact(el.value) },
    { field: document.getElementById('existDate'), test: checkApptDate },
    { field: document.getElementById('existTimeSelect'), test: checkApptTime }
]);
existApptForm.addEventListener('submit', e => {
    if (!existApptValidator.validate()) e.preventDefault();
});

const editApptForm = document.getElementById('formEditAppointment');
const editApptValidator = FV.create(editApptForm, () => {
    const status = document.getElementById('editStatus').value;
    const isCancelled = status === 'Cancelled';
    const choiceVisible = document.getElementById('editConfirmChoiceSection').style.display !== 'none';

    // Rescheduled: kinahanglan mausab ang petsa o oras.
    const unchangedReschedule = () => status === 'Rescheduled'
        && document.getElementById('editDate').value === (editApptForm.dataset.origDate || '')
        && document.getElementById('editTimeSelect').value === (editApptForm.dataset.origTime || '');

    return [
        { field: document.getElementById('editStatus'), test: el => ['Pending', 'Confirmed', 'Cancelled', 'Rescheduled'].includes(el.value) ? '' : 'Pilia ang valid nga status.' },
        {
            field: document.getElementById('editChoiceCards'),
            highlight: [document.getElementById('editExistingBtn'), document.getElementById('editNewBtn')],
            test: () => status === 'Confirmed' && choiceVisible && !document.getElementById('editPatientMode').value
                ? 'Pilia ang Existing o New Patient.' : ''
        },
        { field: document.getElementById('editPatientName'), test: el => FV.rules.personName(el.value) },
        { field: document.getElementById('editContactNo'), test: el => FV.rules.contact(el.value) },
        { field: document.getElementById('editDate'), test: el => isCancelled ? '' : (checkApptDate(el) || (unchangedReschedule() ? 'Usba ang petsa o oras.' : '')) },
        { field: document.getElementById('editTimeSelect'), test: el => isCancelled ? '' : (checkApptTime(el) || (unchangedReschedule() ? 'Usba ang petsa o oras.' : '')) }
    ];
});

const confirmNewForm = document.getElementById('formConfirmNewPatient');
const confirmNewValidator = FV.create(confirmNewForm, () => FV.patientChecks(confirmNewForm));

// ---------------------------------------------------------------
// NEW APPOINTMENT modal — populate time slots on date change
// ---------------------------------------------------------------
document.getElementById('modalNewAppointment').addEventListener('show.bs.modal', function () {
    newApptValidator.reset();
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
    // Bag-ong type = wala pay napili nga pasyente.
    document.getElementById('existPatientId').value = '';
    document.getElementById('existPatientName').value = '';
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
    existApptValidator.recheck();
}

document.addEventListener('click', function (e) {
    if (!e.target.closest('#existSearch') && !e.target.closest('#existSuggestions')) {
        document.getElementById('existSuggestions').style.display = 'none';
    }
});

document.getElementById('modalExistingAppointment').addEventListener('show.bs.modal', function () {
    existApptValidator.reset();
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
        editApptValidator.reset();

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
    editApptValidator.recheck();
}

// Open separate New Patient modal from Edit modal
function openNewPatientModalFromEdit() {
    const apptId  = document.getElementById('editId').value;
    const name    = document.getElementById('editPatientName').value;
    const contact = document.getElementById('editContactNo').value;

    document.getElementById('confirmNewApptId').value   = apptId;
    document.getElementById('confirmNewFullName').value = name;
    document.getElementById('confirmNewContactNo').value = contact;
    confirmNewValidator.reset();
    hideModalError('confirmNewInlineError');



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
    // I-validate una; ayaw i-send kung naay sayop.
    if (!confirmNewValidator.validate()) return;
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
        } else if (!confirmNewValidator.showErrors(data.errors, fieldByName(this))) {
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

    // Client-side pre-check, same rules as the server (which re-validates
    // against the saved record), including: Rescheduled needs a new date OR
    // a new time.
    if (!editApptValidator.validate()) return;

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
        } else if (!editApptValidator.showErrors(data.errors, fieldByName(this))) {
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
    showConfirmModal('Delete appointment for "' + patientName + '"?', () => form.submit());
    return false;
}
