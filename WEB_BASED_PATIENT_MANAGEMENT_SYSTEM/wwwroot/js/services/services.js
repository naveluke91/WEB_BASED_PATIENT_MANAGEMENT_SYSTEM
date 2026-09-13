// ============================================================
// services.js — Services module (Views/Services/Index.cshtml)
// Register Service modal, View / Edit Service modals and table search.
// Server-rendered values are provided by the view as
// window.servicesPageData before this file loads.
// ============================================================

// ---- Register Service modal ----

(() => {
    const patients = window.servicesPageData.patients;
    const form = document.getElementById('serviceForm');
    const search = document.getElementById('patientSearchInput');
    const dropdown = document.getElementById('patientDropdown');
    const patientId = document.getElementById('selectedPatientIdInput');
    const appointmentGroup = document.getElementById('appointmentSelectionGroup');
    const appointmentId = document.getElementById('selectedAppointmentIdInput');
    const serviceName = document.getElementById('selectedServicesInput');
    const error = document.getElementById('serviceRegistrationError');
    const modal = document.getElementById('addServiceModal');

    if (!form || !search || !dropdown || !patientId || !appointmentGroup || !appointmentId || !serviceName || !error) return;

    let selectedPatient = null;
    let availableAppointments = [];

    function hideDropdown() {
        dropdown.style.display = 'none';
    }

    // Select Appointment is always visible. It is enabled only when the
    // selected patient has confirmed appointments that are still available
    // (Services/GetAvailableAppointments). Otherwise it stays disabled and
    // the service is registered as Walk-In, exactly as before.
    const appointmentPlaceholders = {
        noPatient: '-- Select a patient first --',
        loading: 'Loading appointments...',
        walkIn: 'Walk-In - No appointment required',
        choose: '-- Select a confirmed appointment --',
        unavailable: 'Appointments could not be loaded'
    };

    function lockAppointments(placeholder) {
        availableAppointments = [];
        appointmentId.replaceChildren(new Option(placeholder, ''));
        appointmentId.value = '';
        appointmentId.disabled = true;
    }

    function resetAppointments() {
        lockAppointments(appointmentPlaceholders.noPatient);
    }

    function showAppointments(appointments) {
        if (!appointments.length) {
            lockAppointments(appointmentPlaceholders.walkIn);
            return;
        }

        availableAppointments = appointments;
        appointmentId.replaceChildren(new Option(appointmentPlaceholders.choose, ''));

        appointments.forEach(appointment => {
            appointmentId.add(new Option(appointment.label, appointment.id));
        });

        appointmentId.disabled = false;

        if (appointments.length === 1) {
            appointmentId.value = String(appointments[0].id);
        }
    }

    async function loadAvailableAppointments(patient) {
        lockAppointments(appointmentPlaceholders.loading);

        try {
            const response = await fetch(`${window.servicesPageData.availableAppointmentsUrl}?patientId=${encodeURIComponent(patient.id)}`, {
                headers: { 'Accept': 'application/json' }
            });

            if (selectedPatient?.id !== patient.id) return;

            if (!response.ok) {
                lockAppointments(appointmentPlaceholders.unavailable);
                return;
            }

            const appointments = await response.json();

            // Another patient may have been picked while this request was running
            if (selectedPatient?.id === patient.id) {
                showAppointments(appointments);
            }
        } catch {
            if (selectedPatient?.id === patient.id) {
                lockAppointments(appointmentPlaceholders.unavailable);
            }
        }
    }

    function choosePatient(patient) {
        selectedPatient = patient;
        patientId.value = patient.id;
        search.value = patient.name;
        search.style.borderColor = '#468403';
        hideDropdown();
        loadAvailableAppointments(patient);
    }

    function showPatients(matches) {
        dropdown.replaceChildren();
        if (!matches.length) {
            hideDropdown();
            return;
        }

        matches.forEach(patient => {
            const option = document.createElement('button');
            option.type = 'button';
            option.className = 'w-100 text-start border-0 bg-white px-3 py-2';
            option.textContent = patient.name;
            option.addEventListener('click', () => choosePatient(patient));
            dropdown.appendChild(option);
        });
        dropdown.style.display = 'block';
    }

    search.addEventListener('focus', () => showPatients(patients));
    search.addEventListener('input', () => {
        selectedPatient = null;
        patientId.value = '';
        resetAppointments();
        search.style.borderColor = '#C8D5C0';
        const query = search.value.trim().toLowerCase();
        showPatients(patients.filter(patient => patient.name.toLowerCase().includes(query)));
    });

    document.addEventListener('click', event => {
        if (!search.contains(event.target) && !dropdown.contains(event.target)) hideDropdown();
    });

    document.querySelectorAll('#addServiceModal .svc-tile').forEach(tile => {
        tile.addEventListener('click', () => {
            document.querySelectorAll('#addServiceModal .svc-tile').forEach(item => item.classList.remove('selected'));
            tile.classList.add('selected');
            serviceName.value = tile.dataset.service;
        });
    });

    form.addEventListener('submit', event => {
        // Appointment selection is optional: leaving it blank registers a
        // Walk-In service; picking one applies it to that appointment.
        if (selectedPatient && patientId.value && serviceName.value) return;
        event.preventDefault();
        error.textContent = 'Please select a registered patient and one service.';
        error.classList.remove('d-none');
    });

    modal?.addEventListener('hidden.bs.modal', () => {
        selectedPatient = null;
        patientId.value = '';
        resetAppointments();
        serviceName.value = '';
        search.value = '';
        search.style.borderColor = '#C8D5C0';
        error.classList.add('d-none');
        document.querySelectorAll('#addServiceModal .svc-tile').forEach(tile => tile.classList.remove('selected'));
        hideDropdown();
    });
})();

// ---- View / Edit Service modals ----

(() => {
    const editModalElement = document.getElementById('editServiceModal');
    const viewModalElement = document.getElementById('viewServiceModal');
    const editForm = document.getElementById('editServiceForm');
    const editId = document.getElementById('editServiceId');
    const editPatientId = document.getElementById('editPatientId');
    const editServiceName = document.getElementById('editSelectedServiceInput');
    const editError = document.getElementById('editServiceError');
    const viewEditButton = document.getElementById('viewServiceEditButton');

    if (!editModalElement || !viewModalElement || !editForm || !editId || !editPatientId || !editServiceName || !editError) {
        return;
    }

    const editTiles = Array.from(editModalElement.querySelectorAll('.svc-tile'));
    let viewedService = null;

    function getServiceFromButton(button) {
        return {
            id: button.dataset.serviceId || '',
            patientId: button.dataset.patientId || '',
            patientName: button.dataset.patientName || 'Unknown',
            serviceName: button.dataset.serviceName || '',
            price: button.dataset.servicePrice || ''
        };
    }

    function selectEditService(serviceName) {
        editServiceName.value = serviceName || '';
        editTiles.forEach(tile => {
            const isSelected = tile.dataset.service === serviceName;
            tile.classList.toggle('selected', isSelected);
            tile.setAttribute('aria-pressed', isSelected ? 'true' : 'false');
        });
    }

    function populateEditModal(service) {
        editId.value = service.id;
        editPatientId.value = service.patientId;
        selectEditService(service.serviceName);
        editError.classList.add('d-none');
        editError.textContent = '';
    }

    editModalElement.addEventListener('show.bs.modal', event => {
        if (event.relatedTarget) {
            populateEditModal(getServiceFromButton(event.relatedTarget));
        }
    });

    editTiles.forEach(tile => {
        tile.addEventListener('click', () => selectEditService(tile.dataset.service));
    });

    editForm.addEventListener('submit', event => {
        if (editPatientId.value && editServiceName.value) {
            return;
        }

        event.preventDefault();
        editError.textContent = 'Please select a registered patient and one service.';
        editError.classList.remove('d-none');
    });

    editModalElement.addEventListener('hidden.bs.modal', () => {
        editForm.reset();
        editId.value = '';
        selectEditService('');
        editError.classList.add('d-none');
        editError.textContent = '';
    });

    viewModalElement.addEventListener('show.bs.modal', event => {
        if (!event.relatedTarget) {
            return;
        }

        viewedService = getServiceFromButton(event.relatedTarget);
        document.getElementById('viewServiceSubtitle').textContent = viewedService.patientName;
        document.getElementById('viewServicePatient').textContent = viewedService.patientName;
        document.getElementById('viewServiceName').textContent = viewedService.serviceName || '—';
        document.getElementById('viewServicePrice').textContent = viewedService.price ? `₱ ${viewedService.price}` : '—';
        viewEditButton?.removeAttribute('disabled');
    });

    viewModalElement.addEventListener('hidden.bs.modal', () => {
        document.getElementById('viewServiceSubtitle').textContent = '';
        document.getElementById('viewServicePatient').textContent = '—';
        document.getElementById('viewServiceName').textContent = '—';
        document.getElementById('viewServicePrice').textContent = '—';
        viewEditButton?.setAttribute('disabled', 'disabled');
    });

    viewEditButton?.addEventListener('click', () => {
        if (!viewedService) {
            return;
        }

        const serviceToEdit = viewedService;
        viewModalElement.addEventListener('hidden.bs.modal', () => {
            populateEditModal(serviceToEdit);
            bootstrap.Modal.getOrCreateInstance(editModalElement).show();
        }, { once: true });
    });
})();

// ---- Services table search ----

document.getElementById('servicesSearch')?.addEventListener('input', function () {
    const query = this.value.trim().toLowerCase();
    document.querySelectorAll('#servicesTableBody .patient-row').forEach(function (row) {
        row.style.display = row.dataset.name.includes(query) ? '' : 'none';
    });
});
