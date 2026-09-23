// ============================================================
// billing.js — Payment/Billing module (Views/Billing/Index.cshtml)
// Add Payment modal, View Transaction modal and table search.
// Server-rendered values are provided by the view as
// window.billingPageData before this file loads.
// ============================================================

// ---- Add Payment modal ----

(() => {
    const data = window.billingPageData;
    const modal = document.getElementById('addPaymentModal');
    const form = document.getElementById('addPaymentForm');
    const consultationSelect = document.getElementById('paymentConsultationId');
    const amount = document.getElementById('paymentAmount');
    const submitButton = document.getElementById('addPaymentSubmit');
    const error = document.getElementById('addPaymentError');

    if (!data || !modal || !form || !consultationSelect || !amount || !submitButton || !error) return;

    // Consultation details come from the database (rendered by the server),
    // so the patient's name is displayed, never typed in.
    const consultations = new Map(data.consultations.map(consultation => [String(consultation.id), consultation]));

    // Modal element id → consultation property
    const detailFields = {
        payPatientName: 'patientName',
        payPatientId: 'patientId',
        payContactNo: 'contactNo',
        payConsultationDate: 'consultationDate',
        payServiceType: 'serviceType',
        payVisitType: 'visitType'
    };

    function showError(message) {
        error.textContent = message;
        error.classList.remove('d-none');
    }

    function hideError() {
        error.textContent = '';
        error.classList.add('d-none');
    }

    function formatAmount(value) {
        return Number(value).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function showConsultation(consultation) {
        Object.entries(detailFields).forEach(([elementId, key]) => {
            document.getElementById(elementId).textContent = consultation ? String(consultation[key]) : '—';
        });

        const hasFee = !!consultation && consultation.serviceFee !== null;
        document.getElementById('payServiceFee').textContent = hasFee ? `₱ ${formatAmount(consultation.serviceFee)}` : '—';

        // The registered service price is the suggested amount; staff can change it.
        amount.value = hasFee ? Number(consultation.serviceFee).toFixed(2) : '';
    }

    consultationSelect.addEventListener('change', () => {
        hideError();
        showConsultation(consultations.get(consultationSelect.value) ?? null);
    });

    // Kantidad: numero, dili negatibo, hangtod 2 ka decimal.
    function checkAmount(el) {
        const text = el.value.trim();
        if (el.validity?.badInput) return 'Enter a valid amount.';
        if (!text) return 'This field is required.';
        if (text.startsWith('-')) return 'Amount cannot be negative.';
        if (/\.\d{3,}$/.test(text)) return 'Amount can only have up to 2 decimal places.';
        if (!/^\d{1,8}(\.\d{1,2})?$/.test(text)) return 'Enter a valid amount.';
        return Number(text) <= 99999999.99 ? '' : 'Amount is too large.';
    }

    const paymentMethod = document.getElementById('paymentMethod');
    const validator = FormValidation.create(form, () => [
        { field: consultationSelect, test: el => consultations.has(el.value) ? '' : 'Please select a consultation.' },
        { field: amount, test: checkAmount },
        { field: paymentMethod, test: el => ['Cash', 'GCash'].includes(el.value) ? '' : 'Please select Cash or GCash.' }
    ]);

    // A second click would only reach the server's duplicate check, so block it.
    // I-disable ra kung valid ang form.
    form.addEventListener('submit', event => {
        if (!validator.validate()) {
            event.preventDefault();
            return;
        }
        submitButton.disabled = true;
    });

    modal.addEventListener('hidden.bs.modal', () => {
        form.reset();
        hideError();
        showConsultation(null);
        validator.reset();
        submitButton.disabled = consultations.size === 0;
    });

    // Opened from Consultation → Proceed to Billing, or reopened after the server rejected the form.
    const openId = data.openConsultationId == null ? '' : String(data.openConsultationId);
    if (consultations.has(openId)) {
        consultationSelect.value = openId;
        showConsultation(consultations.get(openId));
        if (data.addPaymentError) {
            // I-marka ang field kung nahibaw-an; kung dili, ipakita sa taas.
            const errorField = data.addPaymentErrorField ? form.querySelector(`[name="${data.addPaymentErrorField}"]`) : null;
            if (errorField) {
                modal.addEventListener('shown.bs.modal', () => FormValidation.setError(errorField, data.addPaymentError), { once: true });
            } else {
                showError(data.addPaymentError);
            }
        }
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

// ---- View Transaction modal ----

(() => {
    const data = window.billingPageData;
    const modal = document.getElementById('viewPaymentModal');
    const notice = document.getElementById('viewPaymentNotice');

    if (!data || !modal || !notice) return;

    // Modal element id → View button data attribute
    const fields = {
        viewPaymentPatientName: 'patientName',
        viewPaymentPatientId: 'patientId',
        viewPaymentConsultationDate: 'consultationDate',
        viewPaymentServiceType: 'serviceType',
        viewPaymentVisitType: 'visitType',
        viewPaymentAppointment: 'appointment',
        viewPaymentMethod: 'paymentMethod',
        viewPaymentDate: 'paymentDate'
    };

    modal.addEventListener('show.bs.modal', event => {
        const button = event.relatedTarget;
        if (!button) return;

        Object.entries(fields).forEach(([elementId, key]) => {
            document.getElementById(elementId).textContent = button.dataset[key] || '—';
        });
        document.getElementById('viewPaymentAmount').textContent = `₱ ${button.dataset.amount}`;
        document.getElementById('viewPaymentSubtitle').textContent = button.dataset.patientName || '';
    });

    modal.addEventListener('hidden.bs.modal', () => {
        notice.textContent = '';
        notice.classList.add('d-none');
    });

    // Opened from Consultation → View Payment, or after a duplicate payment was blocked.
    if (data.openPaymentId != null) {
        const button = document.querySelector(`.js-view-payment[data-payment-id="${data.openPaymentId}"]`);
        if (button) {
            if (data.paymentNotice) {
                notice.textContent = data.paymentNotice;
                notice.classList.remove('d-none');
            }
            bootstrap.Modal.getOrCreateInstance(modal).show(button);
        }
    }
})();

// ---- Transactions table search ----

document.getElementById('billingSearch')?.addEventListener('input', function () {
    const query = this.value.trim().toLowerCase();
    document.querySelectorAll('#billingTableBody .patient-row').forEach(function (row) {
        row.style.display = (row.dataset.search || '').includes(query) ? '' : 'none';
    });
});
