// ============================================================
// skilltestjs/patient.js — SKILL TEST features sa PATIENTS page
// (Views/Patients/Index.cshtml). Gi-load human sa js/patient/patient.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, PatientsController.cs, Patient.cs ug skilltestcss/patient.css
// nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-PATIENT-0] (SETUP) sa tanang file nga naay #0.
// Ang mga table feature mogamit sa STTable (skilltestjs/st-core.js).
// ============================================================

// ==== [ST-PATIENT-0] START — SETUP: config sa Patients table (i-uncomment una ni) ====
// keys = mga data-* sa <tr> nga gi-search sa search box (parehas sa original nga search).
// Ang <span class="st-data"> sa matag row (Index.cshtml) i-copy ngadto sa <tr>.
/*
const ST_PATIENT_TABLE = { body: '#patientTableBody', search: '#patientSearch', keys: ['name', 'contact', 'id'] };

document.querySelectorAll('#patientTableBody .st-data').forEach(span => {
    Object.assign(span.closest('tr').dataset, span.dataset);
});
*/
// ==== [ST-PATIENT-0] END ====


// ==== [ST-PATIENT-1] START — Filter by Marital Status ====
// Gamit: ipakita ra ang pasyente nga Single, Married, Widowed o Separated.
/*
STTable.filter(ST_PATIENT_TABLE, 'marital', row => {
    const value = document.getElementById('stMaritalFilter').value;
    return !value || row.dataset.marital === value;
}, '#stMaritalFilter');
*/
// ==== [ST-PATIENT-1] END ====


// ==== [ST-PATIENT-3] START — Patient No.: ma-search sab ang "PT-0001" ====
/*
STTable.searchKeys(ST_PATIENT_TABLE, ['name', 'contact', 'id', 'patientno']);
*/
// ==== [ST-PATIENT-3] END ====


// ==== [ST-PATIENT-4] START — Pagination (10 / 25 / 50 / All kada page) ====
// Gamit: dili kaayo taas ang table kung daghan na ang pasyente.
/*
(() => {
    const size = document.getElementById('stPatientPageSize');
    const apply = () => STTable.paginate(ST_PATIENT_TABLE, size.value, '#stPatientPager');
    size.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-PATIENT-4] END ====


// ==== [ST-PATIENT-5] START — Count "Showing X of Y patients" + "No matching records" ====
// Gamit: makita pila ang resulta sa search/filter; kung walay makita, naay message.
/*
STTable.onRender(ST_PATIENT_TABLE, state => {
    const label = document.getElementById('stPatientCount');
    if (label) label.textContent = `Showing ${state.matched} of ${state.total} patient${state.total === 1 ? '' : 's'}`;
});
*/
// ==== [ST-PATIENT-5] END ====


// ==== [ST-PATIENT-6] START — Filter by Trimester / Due this month ====
// Gamit: pangitaon dayon ang pasyente sa 1st, 2nd o 3rd trimester (gikan sa LMP),
//        o ang manganak karong bulana (EDC).
/*
(() => {
    const weeksSinceLmp = text => {
        const lmp = STDate.parse(text);
        if (!lmp) return null;
        return Math.floor((STDate.parse(STDate.iso()) - lmp) / (7 * 86400000));
    };

    STTable.filter(ST_PATIENT_TABLE, 'trimester', row => {
        const value = document.getElementById('stTrimesterFilter').value;
        if (!value) return true;
        if (value === 'due-month') return STDate.inPreset(row.dataset.edc, 'month');

        const weeks = weeksSinceLmp(row.dataset.lmp);
        if (weeks === null) return false;
        if (value === '1') return weeks >= 0 && weeks <= 13;
        if (value === '2') return weeks >= 14 && weeks <= 27;
        return weeks >= 28 && weeks <= 42;
    }, '#stTrimesterFilter');
})();
*/
// ==== [ST-PATIENT-6] END ====


// ==== [ST-PATIENT-7] START — "Due soon" badge sa EDC (sulod sa 14 days) ====
// Gamit: makita dayon kinsa ang hapit na manganak o lapas na sa EDC.
/*
document.querySelectorAll('#patientTableBody .patient-row').forEach(row => {
    const edc = STDate.parse(row.dataset.edc);
    const cell = row.querySelector('.st-edc-badge');
    if (!edc || !cell) return;

    const days = Math.round((edc - STDate.parse(STDate.iso())) / 86400000);
    let badge = null;
    if (days >= 0 && days <= 14) {
        badge = { text: days === 0 ? 'Due today' : `Due in ${days} day${days === 1 ? '' : 's'}`, className: 'st-badge-warning' };
    } else if (days < 0 && days >= -14) {
        badge = { text: 'Overdue', className: 'st-badge-danger' };
    }

    if (badge) cell.innerHTML = `<span class="st-badge ${badge.className}">${badge.text}</span>`;
});
*/
// ==== [ST-PATIENT-7] END ====


// ==== [ST-PATIENT-8] START — "High-risk age" badge (ubos sa 18 o labaw sa 35) ====
// Gamit: ang batan-on kaayo o edaran nga pagmabdos kay high-risk, so kinahanglan bantayan.
/*
document.querySelectorAll('#patientTableBody .patient-row').forEach(row => {
    if (!row.dataset.age) return;
    const age = Number(row.dataset.age);
    if (age >= 18 && age <= 35) return;
    row.querySelector('.cell-name')?.insertAdjacentHTML('beforeend',
        '<span class="st-badge st-badge-danger" title="Age below 18 or above 35">High-risk age</span>');
});
*/
// ==== [ST-PATIENT-8] END ====


// ==== [ST-PATIENT-9] START — Filter by Date Registered (From – To) ====
/*
STTable.filter(ST_PATIENT_TABLE, 'registered', row => {
    const from = document.getElementById('stRegisteredFrom').value; // "yyyy-MM-dd"
    const to = document.getElementById('stRegisteredTo').value;
    const date = row.dataset.registered;
    if (!from && !to) return true;
    if (!date) return false;
    return (!from || date >= from) && (!to || date <= to);
}, '#stRegisteredFrom', '#stRegisteredTo');
*/
// ==== [ST-PATIENT-9] END ====


// ==== [ST-PATIENT-12] START — Blood Type sa Edit ug View Details modal ====
// I-wrap ang original nga function sa patient.js: human ma-abli ang modal,
// kuhaon ang Blood Type gikan sa /Patients/StBloodType/{id}.
/*
(() => {
    const getBloodType = async id => {
        const response = await fetch(`/Patients/StBloodType/${encodeURIComponent(id)}`);
        return response.ok ? (await response.json()).bloodType : '';
    };

    const originalEdit = window.openPatientEditModal;
    window.openPatientEditModal = async function (id, state) {
        await originalEdit(id, state);
        document.getElementById('editBloodType').value = await getBloodType(id);
    };

    const originalView = window.openPatientDetailsModal;
    window.openPatientDetailsModal = async function (id) {
        await originalView(id);
        document.getElementById('viewDetailBloodType').textContent = (await getBloodType(id)) || '—';
    };
})();
*/
// ==== [ST-PATIENT-12] END ====


// ==== [ST-PATIENT-13] START — Email sa Edit ug View Details modal ====
/*
(() => {
    const getEmail = async id => {
        const response = await fetch(`/Patients/StEmail/${encodeURIComponent(id)}`);
        return response.ok ? (await response.json()).email : '';
    };

    const originalEdit = window.openPatientEditModal;
    window.openPatientEditModal = async function (id, state) {
        await originalEdit(id, state);
        document.getElementById('editEmail').value = await getEmail(id);
    };

    const originalView = window.openPatientDetailsModal;
    window.openPatientDetailsModal = async function (id) {
        await originalView(id);
        document.getElementById('viewDetailEmail').textContent = (await getEmail(id)) || '—';
    };
})();
*/
// ==== [ST-PATIENT-13] END ====


// ==== [ST-PATIENT-14] START — Patient History sa View Details modal ====
// Gamit: makita sa usa ka lugar ang tanang appointments, services, consultations ug payments.
/*
(() => {
    const escape = text => String(text ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    const peso = value => '₱ ' + Number(value).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    const section = (title, columns, rows) => {
        if (!rows.length) return `<h6 class="st-history-title">${title}</h6><p class="st-history-empty">No ${title.toLowerCase()} yet.</p>`;
        const head = columns.map(column => `<th>${column}</th>`).join('');
        const body = rows.map(row => `<tr>${row.map(cell => `<td>${escape(cell)}</td>`).join('')}</tr>`).join('');
        return `<h6 class="st-history-title">${title} (${rows.length})</h6>
                <div class="table-responsive"><table class="st-history-table"><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table></div>`;
    };

    const original = window.openPatientDetailsModal;
    window.openPatientDetailsModal = async function (id) {
        const box = document.getElementById('stPatientHistory');
        if (box) box.textContent = 'Loading...';
        await original(id);
        if (!box) return;

        try {
            const response = await fetch(`/Patients/History/${encodeURIComponent(id)}`);
            if (!response.ok) throw new Error(`History request failed (${response.status}).`);
            const history = await response.json();

            box.innerHTML =
                section('Appointments', ['Date', 'Time', 'Service', 'Status'],
                    history.appointments.map(a => [a.date, a.time, a.service, a.status])) +
                section('Services', ['Service', 'Type', 'Price'],
                    history.services.map(s => [s.service, s.type, peso(s.price)])) +
                section('Consultations', ['Date', 'Service', 'Visit Type', 'Status'],
                    history.consultations.map(c => [c.date, c.service, c.visitType, c.status])) +
                section('Payments', ['Date', 'Service', 'Amount', 'Method'],
                    history.payments.map(p => [p.date, p.service, peso(p.amount), p.method]));
        } catch (error) {
            console.error(error);
            box.textContent = 'Unable to load the patient history.';
        }
    };
})();
*/
// ==== [ST-PATIENT-14] END ====


// ==== [ST-PATIENT-15] START — Print Patient Record (gikan sa View Details modal) ====
/*
document.getElementById('stPrintPatient')?.addEventListener('click', () => {
    const name = document.getElementById('viewDetailFullName').textContent;
    STPrint.element(document.querySelector('#modalViewPatientDetails .modal-body'), `Patient Record — ${name}`);
});
*/
// ==== [ST-PATIENT-15] END ====
