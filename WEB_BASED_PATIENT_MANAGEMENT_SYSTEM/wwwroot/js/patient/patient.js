/**
 * Patient Management — client-side helpers
 * - Derives age from date of birth on form pages
 * - Filters patient table rows on the index page
 */

(function () {
    'use strict';

    /**
     * Calculates age in years from a date-of-birth string (YYYY-MM-DD).
     */
    function calculateAge(dateOfBirth) {
        if (!dateOfBirth) return '';

        var dateParts = dateOfBirth.split('-');
        if (dateParts.length !== 3) return '';

        // Use local date parts so the selected birthday is not shifted by timezone.
        var dob = new Date(dateParts[0], dateParts[1] - 1, dateParts[2]);
        if (Number.isNaN(dob.getTime())) return '';

        var today = new Date();

        // A future date or an age above the allowed range must not produce an age.
        if (dob > today) return '';

        var age = today.getFullYear() - dob.getFullYear();
        var monthDiff = today.getMonth() - dob.getMonth();

        // Subtract one if birthday has not occurred yet this year
        var monthDiff = today.getMonth() - dob.getMonth();

        // Subtract one if birthday has not occurred yet this year
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
            age--;
        }

        return age >= 0 && age <= 130 ? age : '';
    }

    /**
     * Wires up the DOB → Age auto-calculation on Create/Edit forms.
     */
    function initAgeCalculation() {
        var dobInput = document.getElementById('dateOfBirth') || document.querySelector('input[name="PrenatalRecord.DateOfBirth"]');
        var ageField = document.getElementById('ageField');
        var ageDisplay = document.getElementById('ageDisplay');
        var ageDisplaySection2 = document.getElementById('ageDisplaySection2');

        if (!dobInput) return;

        function updateAge() {
            // I-calculate ang edad kung naa'y sulod sa DOB field
            var computedAge = '';
            if (dobInput.value) {
                computedAge = calculateAge(dobInput.value);
            }
            
            if (ageField) ageField.value = computedAge;
            if (ageDisplay) ageDisplay.value = computedAge;
            if (ageDisplaySection2) ageDisplaySection2.value = computedAge;
        }

        // Check kon anaa ba kita sa Edit page (naay '/Edit' sa URL) o Details page
        var isEditPage = window.location.pathname.indexOf('/Edit') > -1 || window.location.pathname.indexOf('/Details') > -1;

        if (isEditPage) {
            updateAge();
        } else {
            // Blanko ang Age field sa pagsugod (Create page)
            if (ageField) ageField.value = '';
            if (ageDisplay) ageDisplay.value = '';
            if (ageDisplaySection2) ageDisplaySection2.value = '';
        }

        // Mag-calculate lang kung hilabtan o usbon na sa user ang DOB field
        dobInput.addEventListener('change', updateAge);
        dobInput.addEventListener('input', updateAge);
    }

    /**
     * Filters patient table rows based on the search input value.
     */
    function initPatientSearch() {
        var searchInput = document.getElementById('patientSearch');
        var tableBody = document.getElementById('patientTableBody');
        var paginationInfo = document.getElementById('paginationInfo');

        if (!searchInput || !tableBody) return;

        searchInput.addEventListener('input', function () {
            var query = searchInput.value.toLowerCase().trim();
            var rows = tableBody.querySelectorAll('.patient-row');
            var visibleCount = 0;

            rows.forEach(function (row) {
                var name = row.getAttribute('data-name') || '';
                var contact = row.getAttribute('data-contact') || '';
                var id = row.getAttribute('data-id') || '';
                var match = !query ||
                    name.includes(query) ||
                    contact.includes(query) ||
                    id.includes(query);

                row.classList.toggle('hidden', !match);
                if (match) visibleCount++;
            });

            // Update pagination info text to reflect filtered count
            if (paginationInfo) {
                var total = rows.length;
                if (visibleCount === 0) {
                    paginationInfo.textContent = 'No patients found';
                } else {
                    paginationInfo.textContent =
                        'Showing 1 to ' + visibleCount + ' of ' + total + ' patients';
                }
            }
        });
    }

    // Initialize when the DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initAgeCalculation();
        initPatientSearch();
    });
})();

/* ------------------------------------------------------------
 * Patients list page (Views/Patients/Index.cshtml)
 * Add / Edit / View Details modals. These elements exist only on
 * the list page, so their listeners use ?. and stay inactive on the
 * standalone Create/Edit pages that also load this file.
 * ------------------------------------------------------------ */

// Auto-calculate Age from Date of Birth in the Add modal
document.getElementById('addDateOfBirth')?.addEventListener('change', function () {
    const dob = new Date(this.value);
    if (!this.value) { document.getElementById('addAge').value = ''; return; }
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    const m = today.getMonth() - dob.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--;
    document.getElementById('addAge').value = age >= 0 ? age : '';
});

// ---- Validation sa Add / Edit Patient ----

// Validator sa usa ka patient form; dili i-submit kung naay sayop.
function createPatientValidator(formId) {
    const form = document.getElementById(formId);
    if (!form || !window.FormValidation) return null;

    const validator = FormValidation.create(form, () => FormValidation.patientChecks(form));
    form.addEventListener('submit', event => {
        if (!validator.validate()) event.preventDefault();
    });
    return validator;
}

const addPatientValidator = createPatientValidator('formAddPatient');
const editPatientValidator = createPatientValidator('formEditPatient');

// I-fill balik ang form ug i-marka ang sayop gikan sa server.
function applyPatientFormState(formId, state) {
    const form = document.getElementById(formId);
    if (!form || !state) return;

    Object.entries(state.values || {}).forEach(([name, value]) => {
        const field = form.querySelector(`[name="${name}"]`);
        if (field) field.value = value ?? '';
    });
    // I-calculate balik ang edad gikan sa DOB.
    form.querySelector('[name="DateOfBirth"]')?.dispatchEvent(new Event('change'));

    const validator = formId === 'formAddPatient' ? addPatientValidator : editPatientValidator;
    validator?.showErrors(state.errors, name => form.querySelector(`[name="${name}"]`));
}

// Reset modal form on close
document.getElementById('modalAddPatient')?.addEventListener('hidden.bs.modal', function () {
    document.getElementById('formAddPatient').reset();
    document.getElementById('addAge').value = '';
    addPatientValidator?.reset();
});

// I-reset ang sayop inig sira sa Edit modal.
document.getElementById('modalEditPatient')?.addEventListener('hidden.bs.modal', function () {
    editPatientValidator?.reset();
});

function calculateEditAge(dateOfBirth) {
    if (!dateOfBirth) return '';

    const parts = dateOfBirth.split('-');
    if (parts.length !== 3) return '';

    const dob = new Date(parts[0], parts[1] - 1, parts[2]);
    const today = new Date();
    if (Number.isNaN(dob.getTime()) || dob > today) return '';

    let age = today.getFullYear() - dob.getFullYear();
    const monthDiff = today.getMonth() - dob.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) age--;

    return age >= 0 && age <= 130 ? age : '';
}

function setEditFieldValue(id, value) {
    document.getElementById(id).value = value ?? '';
}

// Fetch the existing patient data before showing the edit modal.
// Ang state = sayop gikan sa server (kung naa).
async function openPatientEditModal(id, state = null) {
    try {
        editPatientValidator?.reset();
        const response = await fetch(`/Patients/GetById/${encodeURIComponent(id)}`);
        if (!response.ok) throw new Error('Patient not found');

        const patient = await response.json();
        const normalize = value => value === '\u2014' ? '' : (value ?? '');
        const form = document.getElementById('formEditPatient');

        form.action = `/Patients/Edit/${encodeURIComponent(patient.id)}`;
        setEditFieldValue('editId', patient.id);
        setEditFieldValue('editFullName', patient.fullName);
        setEditFieldValue('editAddress', patient.address);
        setEditFieldValue('editDateOfBirth', patient.dateOfBirthRaw);
        setEditFieldValue('editAge', patient.age);
        setEditFieldValue('editMaritalStatus', normalize(patient.maritalStatus));
        setEditFieldValue('editReligion', normalize(patient.religion));
        setEditFieldValue('editLMP', patient.lmpRaw);
        setEditFieldValue('editAOG', patient.aogRaw);
        setEditFieldValue('editEDC', patient.edcRaw);
        window.fillAogFromLmp?.(document.getElementById('editLMP'), false);
        setEditFieldValue('editMenarche', patient.menarcheRaw);
        setEditFieldValue('editContactNo', patient.contactNoRaw);
        setEditFieldValue('editGravida', patient.gravidaRaw);
        setEditFieldValue('editTFAL', patient.tfalRaw);
        setEditFieldValue('editOccupation', patient.occupationRaw);
        document.getElementById('editPatientSubtitle').textContent = patient.fullName;

        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalEditPatient')).show();
        applyPatientFormState('formEditPatient', state);
    } catch (error) {
        console.error('Failed to load patient for editing:', error);
    }
}

document.getElementById('editDateOfBirth')?.addEventListener('change', function () {
    document.getElementById('editAge').value = calculateEditAge(this.value);
});

// View Patient Details in Modal
async function openPatientDetailsModal(id) {
    try {
        const res = await fetch(`/Patients/GetById/${id}`);
        if (!res.ok) throw new Error('Not found');
        const p = await res.json();

        document.getElementById('viewPatientHeaderSubtitle').textContent = p.fullName;
        document.getElementById('viewDetailFullName').textContent      = p.fullName;
        document.getElementById('viewDetailAddress').textContent       = p.address;
        document.getElementById('viewDetailDOB').textContent           = p.dateOfBirth;
        document.getElementById('viewDetailAge').textContent           = p.age;
        document.getElementById('viewDetailMaritalStatus').textContent = p.maritalStatus;
        document.getElementById('viewDetailReligion').textContent      = p.religion;
        document.getElementById('viewDetailLMP').textContent           = p.lmp;
        document.getElementById('viewDetailAOG').textContent           = p.aog;
        document.getElementById('viewDetailEDC').textContent           = p.edc;
        document.getElementById('viewDetailMenarche').textContent      = p.menarche;
        document.getElementById('viewDetailContactNo').textContent     = p.contactNo;
        document.getElementById('viewDetailGravida').textContent       = p.gravida;
        document.getElementById('viewDetailTFAL').textContent          = p.tfal;
        document.getElementById('viewDetailOccupation').textContent    = p.occupation;

        new bootstrap.Modal(document.getElementById('modalViewPatientDetails')).show();
    } catch (err) {
        console.error('Failed to load patient details:', err);
    }
}
