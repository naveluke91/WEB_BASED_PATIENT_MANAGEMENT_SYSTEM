(function() {
    var searchInput = document.getElementById('patientSearchInput');
    var dropdown    = document.getElementById('patientDropdown');
    var patientIdEl = document.getElementById('selectedPatientIdInput');

    if (!searchInput || !dropdown) return;

    var patients = window.consultationPageData.patients;

    var selectedId = null;
    window.loadedPatientData = null;

    function showDropdown(list) {
        dropdown.innerHTML = '';
        if (list.length === 0) { dropdown.style.display = 'none'; return; }

        list.forEach(function(p) {
            var item = document.createElement('div');
            item.style.cssText = 'display:flex; align-items:center; gap:0.65rem; padding:0.65rem 1rem; cursor:pointer; border-bottom:1px solid #f3f4f6;';
            item.innerHTML =
                '<span style="color:#9ca3af; font-size:1.1rem;"><i class="bi bi-person-circle"></i></span>' +
                '<span style="font-weight:600; font-size:0.9rem; color:#111827;"></span>';
            // Ang ngalan isip text (dili HTML) para luwas.
            item.lastElementChild.textContent = p.name;

            item.addEventListener('mouseenter', function() { item.style.background = '#f3f4ff'; });
            item.addEventListener('mouseleave', function() { item.style.background = ''; });
            item.addEventListener('mousedown', function(e) {
                // mousedown fires before blur so we can capture the click
                e.preventDefault();
                pickPatient(p.id, p.name);
            });
            dropdown.appendChild(item);
        });

        dropdown.style.display = 'block';
    }

    async function pickPatient(id, name) {
        selectedId = id;
        patientIdEl.value = id;
        searchInput.value = name;
        searchInput.style.borderColor = '#16a34a';
        dropdown.style.display = 'none';

        try {
            const res = await fetch(`/Patients/GetById/${id}`);
            if (res.ok) {
                window.loadedPatientData = await res.json();
                applyPatientAutofill(window.loadedPatientData);
            }
        } catch (err) {
            console.error('Failed to fetch patient data for autofill:', err);
        }
    }

    window.clearPatientSelection = function() {
        selectedId = null;
        window.loadedPatientData = null;
        patientIdEl.value = '';
        searchInput.value = '';
        searchInput.style.borderColor = '#d1d5db';
        dropdown.style.display = 'none';
        resetPatientAutofill();
    };

    searchInput.addEventListener('focus', function() {
        var q = this.value.trim().toLowerCase();
        if (!q) {
            showDropdown(patients);
        }
    });

    searchInput.addEventListener('input', function() {
        selectedId = null;
        patientIdEl.value = '';
        searchInput.style.borderColor = '#d1d5db';
        var q = this.value.trim().toLowerCase();
        if (!q) { showDropdown(patients); return; }
        showDropdown(patients.filter(function(p) {
            return p.name.toLowerCase().includes(q);
        }));
    });

    document.addEventListener('click', function(e) {
        if (!searchInput.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.style.display = 'none';
            if (!selectedId) {
                searchInput.value = '';
                patientIdEl.value = '';
            }
        }
    });

    window.pickPatient = pickPatient;

    if (window.consultationPageData.initialPatientId != null) {
        pickPatient(window.consultationPageData.initialPatientId, window.consultationPageData.initialPatientName);
    }

    if (window.consultationPageData.initialService != null) {
        // A Consultation already knows its selected service. Activate the
        // matching panel after the shared form script has attached handlers.
        window.setTimeout(function () {
            var initialService = window.consultationPageData.initialService;
            var initialTile = Array.from(document.querySelectorAll('.svc-tile'))
                .find(function (tile) { return tile.dataset.service === initialService; });
            if (initialTile) initialTile.click();
        }, 0);
    }
})();

function calculateAge(dob) {
    if (!dob) return '';
    var parts = dob.split('-');
    if (parts.length !== 3) return '';
    var d = new Date(parts[0], parts[1] - 1, parts[2]);
    if (isNaN(d)) return '';
    var today = new Date();
    if (d > today) return '';
    var age = today.getFullYear() - d.getFullYear();
    var m = today.getMonth() - d.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < d.getDate())) age--;
    return (age >= 0 && age <= 130) ? age : '';
}

var dobInput = document.getElementById('prenatalDobInput');
var ageField = document.getElementById('prenatalAgeInput');

function updateAge() { 
    if (dobInput && ageField) {
        ageField.value = dobInput.value ? calculateAge(dobInput.value) : ''; 
    }
    syncAntenatalFields(); 
}

if (dobInput && ageField) {
    dobInput.addEventListener('change', updateAge);
    dobInput.addEventListener('input', updateAge);
}

function attachFpAgeListener(dobId, ageId) {
    var fpDob = document.getElementById(dobId);
    var fpAge = document.getElementById(ageId);
    if (fpDob && fpAge) {
        function updateFpAge() {
            fpAge.value = fpDob.value ? calculateAge(fpDob.value) : '';
        }
        fpDob.addEventListener('change', updateFpAge);
        fpDob.addEventListener('input', updateFpAge);
    }
}
attachFpAgeListener('fpDobInput', 'fpAgeInput');
attachFpAgeListener('fpSpouseDobInput', 'fpSpouseAgeInput');

function syncAntenatalFields() {
    var preName = document.getElementById('prenatalNameInput');
    var preAddress = document.getElementById('prenatalAddressInput');
    var preAge = document.getElementById('prenatalAgeInput');

    var antName = document.getElementById('PrenatalRecord_PatientName');
    var antAge = document.getElementById('ageDisplaySection2');
    var antAddress = document.getElementById('PrenatalRecord_Address');

    if (preName && antName) antName.value = preName.value;
    if (preAddress && antAddress) antAddress.value = preAddress.value;
    if (preAge && antAge) antAge.value = preAge.value;
}

document.addEventListener('DOMContentLoaded', function() {
    syncAntenatalFields();

    var preName = document.getElementById('prenatalNameInput');
    var preAddress = document.getElementById('prenatalAddressInput');
    var preAge = document.getElementById('prenatalAgeInput');

    if (preName) {
        preName.addEventListener('input', syncAntenatalFields);
        preName.addEventListener('change', syncAntenatalFields);
    }
    if (preAddress) {
        preAddress.addEventListener('input', syncAntenatalFields);
        preAddress.addEventListener('change', syncAntenatalFields);
    }
    if (preAge) {
        preAge.addEventListener('input', syncAntenatalFields);
        preAge.addEventListener('change', syncAntenatalFields);
    }
});

function buildVisitRow() {
    var ticks = new Date().getTime();
    return `<tr class="visit-row">
        <input type="hidden" name="PrenatalRecord.PrenatalVisits.Index" value="${ticks}" />
        <td><input type="date" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].RecordDate" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].AOG" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].Weight" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].BloodPressure" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].Temperature" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].FundalHeight" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].FetalHeartTone" /></td>
        <td><input type="text" class="pn-table-input" name="PrenatalRecord.PrenatalVisits[${ticks}].Remarks" /></td>
        <td class="td-remove"><button type="button" class="visit-remove-btn" onclick="removeVisitRow(this)" title="Remove"><i class="bi bi-x"></i></button></td>
    </tr>`;
}

var addVisitBtn = document.getElementById('addVisitBtn');
if (addVisitBtn) {
    addVisitBtn.addEventListener('click', function () {
        document.getElementById('visitTableBody').insertAdjacentHTML('beforeend', buildVisitRow());
    });
}

function removeVisitRow(btn) {
    var tbody = document.getElementById('visitTableBody');
    if (tbody.querySelectorAll('.visit-row').length > 1) {
        btn.closest('.visit-row').remove();
    }
}

var fpServices = ['Implant','Implant Removal','DEPO','NORIFAM','IUD Insertion','IUD Removal','Anti-Tetanus Injection'];

document.querySelectorAll('.svc-tile').forEach(function (tile) {
    tile.addEventListener('click', function () {
        var svc = tile.dataset.service;

        document.querySelectorAll('.svc-tile').forEach(function (t) {
            if (t !== tile) t.classList.remove('selected');
        });

        tile.classList.toggle('selected');
        var isSelected = tile.classList.contains('selected');

        var prPanel = document.getElementById('prenatalRecordPanel');
        var nbPanel = document.getElementById('newbornPanel');
        var fpPanel = document.getElementById('familyPlanningPanel');

        if (prPanel) prPanel.style.display = (svc === 'Prenatal' && isSelected) ? 'block' : 'none';
        if (nbPanel) nbPanel.style.display  = (svc === 'Normal Delivery Fee & Newborn Care Package' && isSelected) ? 'block' : 'none';
        if (fpPanel) fpPanel.style.display = (fpServices.indexOf(svc) !== -1 && isSelected) ? 'block' : 'none';

        var fpMethodDisplay = document.getElementById('fpMethodDisplay');
        if (fpMethodDisplay) {
            if (fpServices.indexOf(svc) !== -1 && isSelected) {
                fpMethodDisplay.value = svc;
            } else {
                fpMethodDisplay.value = '';
            }
        }

        var selSvcInput = document.getElementById('selectedServicesInput');
        if (selSvcInput) {
            selSvcInput.value = isSelected ? svc : '';
        }

        if (svc === 'Prenatal' && isSelected) {
            syncAntenatalFields();
        }

        if (isSelected && window.loadedPatientData) {
            applyPatientAutofill(window.loadedPatientData);
        }
    });
});

function applyPatientAutofill(p) {
    if (!p) return;

    var preName = document.getElementById('prenatalNameInput');
    if (preName) preName.value = p.fullName || '';
    var preAddress = document.getElementById('prenatalAddressInput');
    if (preAddress) preAddress.value = p.address || '';
    var preDob = document.getElementById('prenatalDobInput');
    if (preDob) preDob.value = p.dateOfBirthRaw || '';
    var preAge = document.getElementById('prenatalAgeInput');
    if (preAge) preAge.value = p.age || '';

    setInputValueByName('PrenatalRecord.MaritalStatus', p.maritalStatus === '—' ? '' : p.maritalStatus);
    setInputValueByName('PrenatalRecord.LMP', p.lmpRaw || '');
    setInputValueByName('PrenatalRecord.AOG', p.aogRaw || '');
    setInputValueByName('PrenatalRecord.EDC', p.edcRaw || '');
    var preLmp = document.querySelector('#consultationServiceForm [name="PrenatalRecord.LMP"]');
    if (preLmp && window.fillAogFromLmp) window.fillAogFromLmp(preLmp, false);
    setInputValueByName('PrenatalRecord.Menarche', p.menarcheRaw || '');
    setInputValueByName('PrenatalRecord.ContactNo', p.contactNoRaw || '');
    setInputValueByName('PrenatalRecord.Gravida', p.gravidaRaw || '');
    setInputValueByName('PrenatalRecord.TFAL', p.tfalRaw || '');
    setInputValueByName('PrenatalRecord.Occupation', p.occupationRaw || '');

    var antName = document.getElementById('PrenatalRecord_PatientName');
    if (antName) antName.value = p.fullName || '';
    var antAddress = document.getElementById('PrenatalRecord_Address');
    if (antAddress) antAddress.value = p.address || '';
    var antAge = document.getElementById('ageDisplaySection2');
    if (antAge) antAge.value = p.age || '';
    setInputValueByName('PrenatalRecord.G', p.gravidaRaw || '');

    var nameParts = (p.fullName || '').trim().split(/\s+/);
    var lastName = nameParts.length > 1 ? nameParts[nameParts.length - 1] : (nameParts[0] || '');
    var givenName = nameParts.length > 1 ? nameParts.slice(0, nameParts.length - 1).join(' ') : (nameParts[0] || '');

    setInputValueByName('FamilyPlanningRecord.ClientLastName', lastName);
    setInputValueByName('FamilyPlanningRecord.ClientGivenName', givenName);
    var fpDob = document.getElementById('fpDobInput');
    if (fpDob) fpDob.value = p.dateOfBirthRaw || '';
    var fpAge = document.getElementById('fpAgeInput');
    if (fpAge) fpAge.value = p.age || '';
    setInputValueByName('FamilyPlanningRecord.ClientOccupation', p.occupationRaw || '');
    setInputValueByName('FamilyPlanningRecord.AddressStreet', p.address || '');
    setInputValueByName('FamilyPlanningRecord.ContactNo', p.contactNoRaw || '');
    setInputValueByName('FamilyPlanningRecord.CivilStatus', p.maritalStatus === '—' ? '' : p.maritalStatus);
    setInputValueByName('FamilyPlanningRecord.Religion', p.religion === '—' ? '' : p.religion);

    setInputValueByName('NewbornRecord.MotherName', p.fullName || '');
    setInputValueByName('NewbornRecord.MotherAddress', p.address || '');
}

function resetPatientAutofill() {
    var preName = document.getElementById('prenatalNameInput');
    if (preName) preName.value = '';
    var preAddress = document.getElementById('prenatalAddressInput');
    if (preAddress) preAddress.value = '';
    var preDob = document.getElementById('prenatalDobInput');
    if (preDob) preDob.value = '';
    var preAge = document.getElementById('prenatalAgeInput');
    if (preAge) preAge.value = '';

    setInputValueByName('PrenatalRecord.MaritalStatus', '');
    setInputValueByName('PrenatalRecord.LMP', '');
    setInputValueByName('PrenatalRecord.AOG', '');
    setInputValueByName('PrenatalRecord.EDC', '');
    setInputValueByName('PrenatalRecord.Menarche', '');
    setInputValueByName('PrenatalRecord.ContactNo', '');
    setInputValueByName('PrenatalRecord.Gravida', '');
    setInputValueByName('PrenatalRecord.TFAL', '');
    setInputValueByName('PrenatalRecord.Occupation', '');

    var antName = document.getElementById('PrenatalRecord_PatientName');
    if (antName) antName.value = '';
    var antAddress = document.getElementById('PrenatalRecord_Address');
    if (antAddress) antAddress.value = '';
    var antAge = document.getElementById('ageDisplaySection2');
    if (antAge) antAge.value = '';
    setInputValueByName('PrenatalRecord.G', '');

    setInputValueByName('FamilyPlanningRecord.ClientLastName', '');
    setInputValueByName('FamilyPlanningRecord.ClientGivenName', '');
    var fpDob = document.getElementById('fpDobInput');
    if (fpDob) fpDob.value = '';
    var fpAge = document.getElementById('fpAgeInput');
    if (fpAge) fpAge.value = '';
    setInputValueByName('FamilyPlanningRecord.ClientOccupation', '');
    setInputValueByName('FamilyPlanningRecord.AddressStreet', '');
    setInputValueByName('FamilyPlanningRecord.ContactNo', '');
    setInputValueByName('FamilyPlanningRecord.CivilStatus', '');
    setInputValueByName('FamilyPlanningRecord.Religion', '');

    setInputValueByName('NewbornRecord.MotherName', '');
    setInputValueByName('NewbornRecord.MotherAddress', '');
}

function setInputValueByName(name, value) {
    var el = document.querySelector(`[name="${name}"]`);
    if (el && value !== undefined && value !== null) {
        el.value = value;
    }
}

function buildNbVitalsRow() {
    var ticks = new Date().getTime();
    return `<tr class="nb-vitals-row">
        <input type="hidden" name="newbornRecord.Vitals.Index" value="${ticks}" />
        <td><input type="datetime-local" class="pn-table-input" name="newbornRecord.Vitals[${ticks}].DateTime" /></td>
        <td><input type="text" class="pn-table-input" name="newbornRecord.Vitals[${ticks}].HeartRate" /></td>
        <td><input type="text" class="pn-table-input" name="newbornRecord.Vitals[${ticks}].RespiratoryRate" /></td>
        <td><input type="text" class="pn-table-input" name="newbornRecord.Vitals[${ticks}].Temperature" /></td>
        <td class="td-remove"><button type="button" class="visit-remove-btn" onclick="removeNbRow(this,'newbornVitalsBody','nb-vitals-row')"><i class="bi bi-x"></i></button></td>
    </tr>`;
}

var addNbVitalsBtn = document.getElementById('addNbVitalsBtn');
if (addNbVitalsBtn) {
    addNbVitalsBtn.addEventListener('click', function () {
        document.getElementById('newbornVitalsBody').insertAdjacentHTML('beforeend', buildNbVitalsRow());
    });
}

function buildNbMedsRow() {
    var ticks = new Date().getTime();
    return `<tr class="nb-meds-row">
        <input type="hidden" name="newbornRecord.Medications.Index" value="${ticks}" />
        <td><input type="datetime-local" class="pn-table-input" name="newbornRecord.Medications[${ticks}].DateTime" /></td>
        <td><input type="text" class="pn-table-input" name="newbornRecord.Medications[${ticks}].MedicineGiven" /></td>
        <td class="td-remove"><button type="button" class="visit-remove-btn" onclick="removeNbRow(this,'newbornMedsBody','nb-meds-row')"><i class="bi bi-x"></i></button></td>
    </tr>`;
}
var addNbMedsBtn = document.getElementById('addNbMedsBtn');
if (addNbMedsBtn) {
    addNbMedsBtn.addEventListener('click', function () {
        document.getElementById('newbornMedsBody').insertAdjacentHTML('beforeend', buildNbMedsRow());
    });
}

function removeNbRow(btn, tbodyId, rowClass) {
    var tbody = document.getElementById(tbodyId);
    if (tbody.querySelectorAll('.' + rowClass).length > 1) {
        btn.closest('.' + rowClass).remove();
    }
}

(() => {
    const form = document.getElementById('consultationServiceForm');
    const FV = window.FormValidation;
    if (!form || !FV) return;
    const rules = window.consultationPageData.fieldRules || [];
    let draft = false;

    function visiblePanel() {
        return ['prenatalRecordPanel', 'newbornPanel', 'familyPlanningPanel']
            .map(id => document.getElementById(id))
            .find(panel => panel && panel.style.display !== 'none');
    }

    function checkRule(rule, el, panel) {
        if (el.validity?.badInput) return 'Enter a valid value.';
        const text = el.value.trim();
        if (!text) return rule.Required && !draft ? FV.MSG.required : '';
        const range = `Enter a value from ${rule.Min} to ${rule.Max}.`;

        switch (rule.Type) {
            case 'whole':
                if (!/^\d{1,4}$/.test(text)) return rule.Message || 'Enter a whole number (no decimals or negative numbers).';
                return Number(text) < rule.Min || Number(text) > rule.Max ? (rule.Message || range) : '';
            case 'decimal':
                if (!/^\d{1,4}(\.\d{1,2})?$/.test(text)) return 'Enter a number (example: 36.5).';
                return Number(text) < rule.Min || Number(text) > rule.Max ? range : '';
            case 'pattern':
                return new RegExp(rule.Pattern, 'i').test(text) ? '' : rule.Message;
            case 'date': {
                const yearError = FV.rules.dateYear(text);
                if (yearError) return yearError;
                const date = FV.parseDate(text);
                if (rule.NotFuture && date > FV.today()) return FV.MSG.futureDate;
                if (rule.After) {
                    const prefix = rule.Field.split('.')[0];
                    const other = FV.parseDate(panel.querySelector(`[name="${prefix}.${rule.After}"]`)?.value);
                    if (other && date <= other) return rule.Message;
                }
                return '';
            }
            default:
                return '';
        }
    }

    function checks() {
        const panel = visiblePanel();
        if (!panel) return [];
        const list = [];
        const ruled = new Set();

        rules.filter(rule => rule.Type !== 'choice').forEach(rule => {
            const [head, tail] = rule.Field.split('[]');
            const selector = tail === undefined ? `[name="${rule.Field}"]` : `[name^="${head}[" i][name$="]${tail}" i]`;
            panel.querySelectorAll(selector).forEach(field => {
                ruled.add(field);
                list.push({ field, quiet: true, test: el => checkRule(rule, el, panel) });
            });
        });

        panel.querySelectorAll('input[type="date"], input[type="datetime-local"]').forEach(field => {
            if (!ruled.has(field)) list.push({ field, quiet: true, test: el => el.validity?.badInput ? FV.MSG.date : FV.rules.dateYear(el.value) });
        });

        return list;
    }

    const validator = FV.create(form, checks);
    const modal = document.getElementById('consultationServiceModal');
    const confirmModal = document.getElementById('saveRecordConfirmModal');
    const confirmButton = document.getElementById('saveRecordConfirmBtn');
    let confirming = false;
    let saving = false;

    form.addEventListener('submit', event => {
        event.preventDefault();
        draft = false;
        if (!validator.validate()) return;
        confirming = true;
        bootstrap.Modal.getOrCreateInstance(modal).hide();
    });

    document.getElementById('saveLaterBtn')?.addEventListener('click', event => {
        draft = true;
        if (!validator.validate()) return;
        document.getElementById('saveLaterInput').value = 'true';
        event.currentTarget.disabled = true;
        form.submit();
    });

    modal.addEventListener('hidden.bs.modal', () => {
        if (!confirming) return;
        confirming = false;
        bootstrap.Modal.getOrCreateInstance(confirmModal).show();
    });

    confirmButton.addEventListener('click', () => {
        saving = true;
        confirmButton.disabled = true;
        form.submit();
    });

    confirmModal.addEventListener('hidden.bs.modal', () => {
        if (!saving) bootstrap.Modal.getOrCreateInstance(modal).show();
    });

    const serverErrors = window.consultationPageData.clinicalErrors;
    if (serverErrors) {
        modal.addEventListener('shown.bs.modal', () => {
            validator.showErrors(serverErrors, name => form.querySelector(`[name="${name}"]`), { quiet: true });
        }, { once: true });
    }
})();

document.getElementById('consultationSearch')?.addEventListener('input', function () {
    const query = this.value.toLowerCase().trim();
    document.querySelectorAll('#consultationTableBody .patient-row').forEach(function (row) {
        const name = (row.dataset.name || '').toLowerCase();
        const contact = (row.dataset.contact || '').toLowerCase();
        row.style.display = (name.includes(query) || contact.includes(query)) ? '' : 'none';
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const beginModalElement = document.getElementById('beginConsultationModal');
    const appointmentId = document.getElementById('beginAppointmentId');
    const serviceId = document.getElementById('beginServiceId');
    const patientSelect = document.getElementById('beginPatientId');
    const serviceSelect = document.getElementById('beginServiceType');
    const confirmationMessage = document.getElementById('startConfirmationMessage');

    function configureBeginModal(button) {
        appointmentId.value = button.dataset.appointmentId || '';
        serviceId.value = button.dataset.serviceId || '';
        patientSelect.value = button.dataset.patientId || '';
        serviceSelect.value = button.dataset.service || '';
        const patientName = button.dataset.patientName || 'this patient';
        const serviceName = button.dataset.service || 'selected service';
        confirmationMessage.textContent = `Do you want to start the ${serviceName} consultation for ${patientName}?`;
    }

    // Bootstrap opens the modal through the data-bs attributes.  Using its
    // show event keeps Walk-In usable even when another page script fails.
    beginModalElement.addEventListener('show.bs.modal', function (event) {
        if (event.relatedTarget) configureBeginModal(event.relatedTarget);
    });

    const serviceModalElement = document.getElementById('consultationServiceModal');
    if (serviceModalElement.dataset.open === 'true') {
        new bootstrap.Modal(serviceModalElement).show();
    }
});

(() => {
    const modal = document.getElementById('consultationRecordModal');
    const body = document.getElementById('consultationRecordBody');
    const subtitle = document.getElementById('consultationRecordSubtitle');
    if (!modal || !body || !subtitle) return;

    let latestRequest = 0;

    function showMessage(text, className) {
        const message = document.createElement('p');
        message.className = className;
        message.textContent = text;
        body.replaceChildren(message);
    }

    modal.addEventListener('show.bs.modal', async event => {
        const button = event.relatedTarget;
        if (!button) return;

        // Ang katapusang gi-klik ra ang ipakita.
        const request = ++latestRequest;
        subtitle.textContent = button.dataset.recordTitle || '';
        showMessage('Loading record...', 'text-muted');

        try {
            const response = await fetch(button.dataset.url);
            if (!response.ok || response.redirected) throw new Error(`Record request failed (${response.status}).`);
            const html = await response.text();
            if (request === latestRequest) body.innerHTML = html;
        } catch (error) {
            console.error(error);
            if (request === latestRequest) showMessage('Unable to load the record. Please try again.', 'text-danger');
        }
    });

    modal.addEventListener('hidden.bs.modal', () => body.replaceChildren());
})();
