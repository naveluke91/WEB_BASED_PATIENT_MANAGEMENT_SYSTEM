// ============================================================
// skilltestjs/billing.js — SKILL TEST features sa PAYMENT/BILLING page
// (Views/Billing/Index.cshtml). Gi-load human sa js/billing/billing.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, Modals, BillingController.cs, Payment.cs ug
// skilltestcss/billing.css nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-BILLING-0] (SETUP) sa tanang file nga naay #0.
// Ang mga table feature mogamit sa STTable (skilltestjs/st-core.js).
// ============================================================


// ==== [ST-BILLING-0] START — SETUP: config sa Transactions table (i-uncomment una ni) ====
// Ang <span class="st-data"> sa matag row (Index.cshtml) i-copy ngadto sa <tr>.
/*
const ST_BILLING_TABLE = { body: '#billingTableBody', search: '#billingSearch', keys: ['search'] };
const stPeso = value => '₱ ' + Number(value || 0).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

document.querySelectorAll('#billingTableBody .st-data').forEach(span => {
    Object.assign(span.closest('tr').dataset, span.dataset);
});
*/
// ==== [ST-BILLING-0] END ====


// ==== [ST-BILLING-1] START — Filter by Payment Method ====
/*
STTable.filter(ST_BILLING_TABLE, 'method', row => {
    const value = document.getElementById('stPayMethodFilter').value;
    return !value || row.dataset.method === value;
}, '#stPayMethodFilter');
*/
// ==== [ST-BILLING-1] END ====


// ==== [ST-BILLING-2] START — Filter by Payment Date (Today / This Week / This Month o From–To) ====
/*
STTable.filter(ST_BILLING_TABLE, 'date', row => {
    const preset = document.getElementById('stPayDatePreset').value;
    const from = document.getElementById('stPayFrom').value;
    const to = document.getElementById('stPayTo').value;
    const date = row.dataset.date; // "yyyy-MM-dd"
    if (!STDate.inPreset(date, preset)) return false;
    return (!from || date >= from) && (!to || date <= to);
}, '#stPayDatePreset', '#stPayFrom', '#stPayTo');
*/
// ==== [ST-BILLING-2] END ====


// ==== [ST-BILLING-3] START — Sort (Ascending / Descending) ====
/*
(() => {
    const select = document.getElementById('stPaySort');
    const apply = () => {
        const [key, type, direction] = select.value.split(':');
        STTable.sort(ST_BILLING_TABLE, STTable.by(key, type, direction));
    };
    select.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-BILLING-3] END ====


// ==== [ST-BILLING-6] START — Pagination (10 / 25 / 50 / All kada page) ====
/*
(() => {
    const size = document.getElementById('stPayPageSize');
    const apply = () => STTable.paginate(ST_BILLING_TABLE, size.value, '#stPayPager');
    size.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-BILLING-6] END ====


// ==== [ST-BILLING-7] START — Amount validation: dili ₱0 ug dili ubos sa service fee ====
// Mo-check una sa original nga validation (capture = true).
/*
(() => {
    const form = document.getElementById('addPaymentForm');
    const consultation = document.getElementById('paymentConsultationId');
    const amount = document.getElementById('paymentAmount');

    form.addEventListener('submit', event => {
        const selected = window.billingPageData.consultations.find(c => String(c.id) === consultation.value);
        if (!selected || !amount.value) return; // ang original nga validation na ang bahala

        const fee = selected.serviceFee ?? 0;
        const discount = document.getElementById('stDiscountType')?.value;
        const minimum = discount ? Math.round(fee * 0.8 * 100) / 100 : fee;
        const value = Number(amount.value);

        let message = '';
        if (value <= 0) message = 'Amount must be more than ₱0.00.';
        else if (fee > 0 && value < minimum) message = `Amount cannot be less than ₱${minimum.toFixed(2)}.`;

        if (message) {
            event.preventDefault();
            event.stopImmediatePropagation();
            FormValidation.setError(amount, message);
        }
    }, true);
})();
*/
// ==== [ST-BILLING-7] END ====


// ==== [ST-BILLING-8] START — Cash: Amount Received ug Change (sukli) ====
// Gamit: i-type ang gihatag sa pasyente; ang sukli kusang ma-compute.
/*
(() => {
    const method = document.getElementById('paymentMethod');
    const amount = document.getElementById('paymentAmount');
    const tendered = document.getElementById('stAmountTendered');
    const change = document.getElementById('stChange');
    const cashOnly = document.querySelectorAll('.st-cash-only');
    if (!tendered || !change) return;

    const update = () => {
        cashOnly.forEach(element => { element.hidden = method.value !== 'Cash'; });
        const difference = Number(tendered.value) - Number(amount.value);
        change.textContent = tendered.value && amount.value ? stPeso(Math.max(difference, 0)) : stPeso(0);
        change.classList.toggle('st-change-short', tendered.value !== '' && difference < 0);
    };
    [method, amount, tendered].forEach(element => {
        element.addEventListener('input', update);
        element.addEventListener('change', update);
    });
    document.getElementById('addPaymentModal').addEventListener('shown.bs.modal', update);

    document.getElementById('addPaymentForm').addEventListener('submit', event => {
        if (method.value !== 'Cash') return;
        let message = '';
        if (!tendered.value) message = 'Enter the amount received from the patient.';
        else if (Number(tendered.value) < Number(amount.value)) message = 'Amount received cannot be less than the amount to pay.';
        if (message) {
            event.preventDefault();
            event.stopImmediatePropagation();
            FormValidation.setError(tendered, message);
        }
    }, true);

    // View modal: Amount Received ug Change.
    document.getElementById('viewPaymentModal').addEventListener('show.bs.modal', event => {
        const data = event.relatedTarget?.closest('tr')?.querySelector('.st-pay-tendered')?.dataset;
        document.getElementById('stViewTendered').textContent = data?.tendered ? `₱ ${data.tendered}` : '—';
        document.getElementById('stViewChange').textContent = data?.change ? `₱ ${data.change}` : '—';
    });

    update();
})();
*/
// ==== [ST-BILLING-8] END ====


// ==== [ST-BILLING-9] START — GCash Reference Number (13 digits) ====
/*
(() => {
    const method = document.getElementById('paymentMethod');
    const reference = document.getElementById('stReferenceNumber');
    const gcashOnly = document.querySelectorAll('.st-gcash-only');
    if (!reference) return;

    const update = () => gcashOnly.forEach(element => { element.hidden = method.value !== 'GCash'; });
    method.addEventListener('change', update);
    document.getElementById('addPaymentModal').addEventListener('shown.bs.modal', update);

    // Numero ra ang ma-type.
    reference.addEventListener('input', () => {
        reference.value = reference.value.replace(/\D/g, '').slice(0, 13);
    });

    document.getElementById('addPaymentForm').addEventListener('submit', event => {
        if (method.value !== 'GCash' || /^\d{13}$/.test(reference.value)) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        FormValidation.setError(reference, 'Enter the 13-digit GCash reference number.');
    }, true);

    // View modal: Reference No.
    document.getElementById('viewPaymentModal').addEventListener('show.bs.modal', event => {
        const data = event.relatedTarget?.closest('tr')?.querySelector('.st-pay-reference')?.dataset;
        document.getElementById('stViewReference').textContent = data?.reference || '—';
    });

    update();
})();
*/
// ==== [ST-BILLING-9] END ====


// ==== [ST-BILLING-10] START — Discount: Senior Citizen / PWD (20% off sa service fee) ====
/*
(() => {
    const discount = document.getElementById('stDiscountType');
    const consultation = document.getElementById('paymentConsultationId');
    const amount = document.getElementById('paymentAmount');
    if (!discount) return;

    discount.addEventListener('change', () => {
        const selected = window.billingPageData.consultations.find(c => String(c.id) === consultation.value);
        if (!selected || selected.serviceFee == null) return;
        const fee = Number(selected.serviceFee);
        amount.value = (discount.value ? Math.round(fee * 0.8 * 100) / 100 : fee).toFixed(2);
        amount.dispatchEvent(new Event('input', { bubbles: true }));
    });

    // Kung mag-ilis og consultation, i-reset ang discount.
    consultation.addEventListener('change', () => { discount.value = ''; });

    // View modal: Discount.
    const labels = { Senior: 'Senior Citizen (20%)', PWD: 'PWD (20%)' };
    document.getElementById('viewPaymentModal').addEventListener('show.bs.modal', event => {
        const data = event.relatedTarget?.closest('tr')?.querySelector('.st-pay-discount')?.dataset;
        document.getElementById('stViewDiscount').textContent = labels[data?.discount] || 'None';
    });
})();
*/
// ==== [ST-BILLING-10] END ====


// ==== [ST-BILLING-11] START — View modal: Received By ====
/*
document.getElementById('viewPaymentModal').addEventListener('show.bs.modal', event => {
    const data = event.relatedTarget?.closest('tr')?.querySelector('.st-pay-received')?.dataset;
    document.getElementById('stViewReceivedBy').textContent = data?.received || '—';
});
*/
// ==== [ST-BILLING-11] END ====


// ==== [ST-BILLING-13] START — Print Receipt (gikan sa View modal) ====
/*
document.getElementById('stPrintReceipt')?.addEventListener('click', () => {
    const name = document.getElementById('viewPaymentPatientName').textContent;
    STPrint.element(document.querySelector('#viewPaymentModal .modal-body'), `Official Receipt — ${name}`);
});
*/
// ==== [ST-BILLING-13] END ====


// ==== [ST-BILLING-14] START — OR No. sa View modal (OR-yyyy-00001 gikan sa Payment Id) ====
/*
document.getElementById('viewPaymentModal').addEventListener('show.bs.modal', event => {
    const button = event.relatedTarget;
    if (!button) return;
    const year = (button.dataset.paymentDate || '').match(/\d{4}/)?.[0] || new Date().getFullYear();
    document.getElementById('stViewOrNo').textContent = `OR-${year}-${String(button.dataset.paymentId).padStart(5, '0')}`;
});
*/
// ==== [ST-BILLING-14] END ====


// ==== [ST-BILLING-16] START — Edit Payment modal (Admin ra): i-fill gikan sa button ====
/*
document.getElementById('stEditPaymentModal')?.addEventListener('show.bs.modal', event => {
    const button = event.relatedTarget;
    if (!button) return;
    document.getElementById('stEditPaymentId').value = button.dataset.paymentId;
    document.getElementById('stEditPaymentAmount').value = Number(button.dataset.amount).toFixed(2);
    document.getElementById('stEditPaymentMethod').value = button.dataset.paymentMethod;
    document.getElementById('stEditPaymentSubtitle').textContent = button.dataset.patientName;
});
*/
// ==== [ST-BILLING-16] END ====


// ==== [ST-BILLING-12] START — Bag-ong payment method "Maya": i-validate diri ====
// Ang original nga validation (billing.js) nagdawat ra og Cash o GCash, so kung
// Maya ang napili, kini nga code ang mo-check (ug dili na ang original).
/*
document.getElementById('addPaymentForm').addEventListener('submit', event => {
    const method = document.getElementById('paymentMethod');
    if (method.value !== 'Maya') return;

    event.stopImmediatePropagation();
    const consultation = document.getElementById('paymentConsultationId');
    const amount = document.getElementById('paymentAmount');
    if (!consultation.value) {
        event.preventDefault();
        FormValidation.setError(consultation, 'Please select a consultation.');
        return;
    }
    if (!/^\d{1,8}(\.\d{1,2})?$/.test(amount.value.trim())) {
        event.preventDefault();
        FormValidation.setError(amount, 'Enter a valid amount.');
        return;
    }
    document.getElementById('addPaymentSubmit').disabled = true;
}, true);
*/
// ==== [ST-BILLING-12] END ====
