// ============================================================
// appointment.js — Appointments module (Views/Appointments/Index.cshtml)
// Time slots, New (patient autocomplete + Register Patient) / View / Edit modals,
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
    if (FV.isBlank(el.value)) return 'Please select a date.';
    const date = FV.parseDate(el.value);
    if (!date) return FV.MSG.date;
    return date > FV.today() ? '' : 'Please select a date from tomorrow onward.';
}

// Valid ra nga oras nga dili pa booked.
function checkApptTime(el) {
    if (!el.value) return 'Please select a time.';
    if (!ALL_SLOTS.some(slot => slot.value === el.value)) return 'Please select a valid time.';
    return el.selectedOptions[0]?.disabled ? 'This schedule is already taken.' : '';
}

// Mga field sa form base sa name (para sa sayop gikan sa server).
const fieldByName = form => name => form.querySelector(`[name="${name}"]`);

const newApptForm = document.getElementById('formNewAppointment');
const newApptValidator = FV.create(newApptForm, () => [
    // Napili nga rehistradong pasyente: ang ngalan ug contact gikan sa Patient record, dili na i-validate ang format.
    { field: document.getElementById('newPatientName'), test: el => document.getElementById('newPatientId').value ? '' : FV.rules.personName(el.value) },
    { field: document.getElementById('newContactNo'), test: el => document.getElementById('newPatientId').value ? '' : FV.rules.contact(el.value) },
    { field: document.getElementById('newDate'), test: checkApptDate },
    { field: document.getElementById('newTimeSelect'), test: checkApptTime }
]);
newApptForm.addEventListener('submit', e => {
    if (!newApptValidator.validate()) e.preventDefault();
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
        { field: document.getElementById('editStatus'), test: el => ['Pending', 'Confirmed', 'Cancelled', 'Rescheduled'].includes(el.value) ? '' : 'Please select a valid status.' },
        {
            field: document.getElementById('editChoiceCards'),
            highlight: [document.getElementById('editExistingBtn'), document.getElementById('editNewBtn')],
            test: () => status === 'Confirmed' && choiceVisible && !document.getElementById('editPatientMode').value
                ? 'Please select Existing or New Patient.' : ''
        },
        { field: document.getElementById('editPatientName'), test: el => FV.rules.personName(el.value) },
        { field: document.getElementById('editContactNo'), test: el => FV.rules.contact(el.value) },
        { field: document.getElementById('editDate'), test: el => isCancelled ? '' : (checkApptDate(el) || (unchangedReschedule() ? 'Please change the date or time.' : '')) },
        { field: document.getElementById('editTimeSelect'), test: el => isCancelled ? '' : (checkApptTime(el) || (unchangedReschedule() ? 'Please change the date or time.' : '')) }
    ];
});

const confirmNewForm = document.getElementById('formConfirmNewPatient');
const confirmNewValidator = FV.create(confirmNewForm, () => FV.patientChecks(confirmNewForm));

// ---------------------------------------------------------------
// NEW APPOINTMENT modal — populate time slots on date change
// ---------------------------------------------------------------
document.getElementById('modalNewAppointment').addEventListener('show.bs.modal', function () {
    // Balik gikan sa Register Patient: ayaw i-reset aron magpabilin ang gi-enter (ngalan, contact, petsa, oras).
    if (returningFromRegistration) { returningFromRegistration = false; return; }

    newApptValidator.reset();
    hidePatientSuggestions();
    setPatientStatus('');
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
// NEW APPOINTMENT modal — Patient Name autocomplete (Patients table).
// Ang napili nga pasyente mo-fill sa contact ug sa hidden PatientId; kung walay
// match, ipakita ang "not registered" ug ang Register Patient shortcut.
// Ang server mo-verify gihapon sa PatientId pag-save.
// ---------------------------------------------------------------
const newPatientNameInput = document.getElementById('newPatientName');
const newContactNoInput = document.getElementById('newContactNo');
const newPatientIdInput = document.getElementById('newPatientId');
const newPatientSuggestions = document.getElementById('newPatientSuggestions');
const newPatientStatus = document.getElementById('newPatientStatus');
const newPatientStatusText = document.getElementById('newPatientStatusText');
const registerPatientLink = document.getElementById('btnRegisterPatient');
let patientSearchTimer = null;
let patientSearchSeq = 0;
let contactAutofilled = false;

function setPatientStatus(message, showRegister = false) {
    newPatientStatusText.textContent = message;
    registerPatientLink.style.display = showRegister ? 'inline' : 'none';
    newPatientStatus.style.display = message ? 'block' : 'none';
}

function hidePatientSuggestions() {
    newPatientSuggestions.style.display = 'none';
    newPatientSuggestions.innerHTML = '';
}

// Napili ang pasyente: ngalan, contact (dili na usbon) ug PatientId gikan sa Patient record.
function applyPatientToAppointment(patient) {
    newPatientNameInput.value = patient.fullName;
    newContactNoInput.value = patient.contactNo;
    newContactNoInput.readOnly = true;
    contactAutofilled = true;
    newPatientIdInput.value = patient.id;
    hidePatientSuggestions();
    newApptValidator.recheck();
}

// Wala nay napili nga pasyente: tangtangon ang PatientId ug ang na-autofill nga contact.
function clearSelectedPatient() {
    newPatientIdInput.value = '';
    if (contactAutofilled) newContactNoInput.value = '';
    contactAutofilled = false;
    newContactNoInput.readOnly = false;
}

async function searchPatients(q) {
    const seq = ++patientSearchSeq; // i-ignore ang tigulang nga tubag
    try {
        const res = await fetch(`/Appointments/SearchPatients?q=${encodeURIComponent(q)}`);
        if (!res.ok) throw new Error('Patient search failed');
        const patients = await res.json();
        if (seq !== patientSearchSeq) return;
        renderPatientSuggestions(patients, q);
    } catch (e) {
        if (seq !== patientSearchSeq) return;
        hidePatientSuggestions();
        console.warn('Patient search failed:', e);
    }
}

function renderPatientSuggestions(patients, q) {
    hidePatientSuggestions();
    if (!patients.length) {
        // Walay match sa Patients table (ug valid ang ngalan): wala pa ni-register.
        if (!FV.rules.personName(q)) setPatientStatus('This patient is not currently registered.', true);
        return;
    }
    patients.forEach((p, i) => {
        const li = document.createElement('li');
        li.className = 'appt-suggestion-item';
        li.textContent = `${i + 1}. ${p.fullName}`;
        li.addEventListener('click', () => applyPatientToAppointment(p));
        newPatientSuggestions.appendChild(li);
    });
    newPatientSuggestions.style.display = 'block';
}

// Bisan unsa nga usab sa ngalan = wala nay napili nga pasyente; pangitaa pag-usab kung mihunong na ang user.
newPatientNameInput.addEventListener('input', function () {
    clearTimeout(patientSearchTimer);
    patientSearchSeq++;
    clearSelectedPatient();
    hidePatientSuggestions();
    setPatientStatus('');
    const q = this.value.trim();
    if (q) patientSearchTimer = setTimeout(() => searchPatients(q), 400);
});

newPatientNameInput.addEventListener('focus', function () {
    const q = this.value.trim();
    if (q && !newPatientIdInput.value) searchPatients(q);
});

document.addEventListener('click', function (e) {
    if (!e.target.closest('#newPatientName') && !e.target.closest('#newPatientSuggestions')) hidePatientSuggestions();
});

// ---------------------------------------------------------------
// REGISTER PATIENT modal (gikan sa New Appointment) — Add New Patient nga form;
// Patient ra ang ma-save, dili ang appointment.
// ---------------------------------------------------------------
const newAppointmentModalEl = document.getElementById('modalNewAppointment');
const registerPatientModalEl = document.getElementById('modalRegisterPatient');
const registerPatientForm = document.getElementById('formRegisterPatient');
const registerPatientValidator = FV.create(registerPatientForm, () => FV.patientChecks(registerPatientForm));
let returningFromRegistration = false;
let registeredPatient = null;

// Auto-calculate Age gikan sa Date of Birth
document.getElementById('regDateOfBirth').addEventListener('change', function () {
    const ageInput = document.getElementById('regAge');
    if (!this.value) { ageInput.value = ''; return; }
    const dob = new Date(this.value);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const m = today.getMonth() - dob.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--;
    ageInput.value = age >= 0 ? age : '';
});

// Register Patient: i-hide ang New Appointment (magpabilin ang mga value) ug ablihi ang Add New Patient.
registerPatientLink.addEventListener('click', function () {
    registerPatientForm.reset();
    document.getElementById('regAge').value = '';
    registerPatientValidator.reset();
    hideModalError('regPatientInlineError');
    // Ang gi-type nga ngalan ug contact (kung naa) ma-prefill.
    document.getElementById('regFullName').value = newPatientNameInput.value.trim();
    document.getElementById('regContactNo').value = newContactNoInput.value.trim();
    registeredPatient = null;

    newAppointmentModalEl.addEventListener('hidden.bs.modal', () => {
        bootstrap.Modal.getOrCreateInstance(registerPatientModalEl).show();
    }, { once: true });
    bootstrap.Modal.getOrCreateInstance(newAppointmentModalEl).hide();
});

// Inig sira sa Register Patient (Save, Cancel o X): balik sa New Appointment nga wala mabag-o ang mga value.
registerPatientModalEl.addEventListener('hidden.bs.modal', function () {
    returningFromRegistration = true;
    if (registeredPatient) {
        applyPatientToAppointment(registeredPatient);
        setPatientStatus('Patient registered successfully.');
        registeredPatient = null;
    }
    bootstrap.Modal.getOrCreateInstance(newAppointmentModalEl).show();
});

registerPatientForm.addEventListener('submit', async function (e) {
    e.preventDefault();
    hideModalError('regPatientInlineError');
    if (!registerPatientValidator.validate()) return;

    const submitBtn = this.querySelector('[type="submit"]');
    submitBtn.disabled = true; // dili mag-doble nga Patient kung ma-double click
    try {
        const res = await fetch(this.action, {
            method: 'POST',
            body: new FormData(this),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const data = await res.json();
        if (data.success) {
            registeredPatient = { id: data.patientId, fullName: data.fullName, contactNo: data.contactNo };
            bootstrap.Modal.getOrCreateInstance(registerPatientModalEl).hide();
        } else if (!registerPatientValidator.showErrors(data.errors, fieldByName(this))) {
            showModalError('regPatientInlineError', data.message || 'Failed to save patient. Please check the fields.');
        }
    } catch (err) {
        console.error('Register patient failed:', err);
        showModalError('regPatientInlineError', 'An unexpected error occurred while saving. Please try again.');
    } finally {
        submitBtn.disabled = false;
    }
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
