// ============================================================
// skilltestjs/consultation.js — SKILL TEST features sa CONSULTATION page
// (Views/Consultation/Index.cshtml). Gi-load human sa js/consultation/consultation.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, ConsultationController.cs ug skilltestcss/consultation.css
// nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-CONSULT-0] (SETUP) sa tanang file nga naay #0.
// Ang mga table feature mogamit sa STTable (skilltestjs/st-core.js).
// ============================================================


// ==== [ST-CONSULT-0] START — SETUP: config sa Consultation table (i-uncomment una ni) ====
// Ang <span class="st-data"> sa matag row (Index.cshtml) i-copy ngadto sa <tr>.
/*
const ST_CONSULT_TABLE = { body: '#consultationTableBody', search: '#consultationSearch', keys: ['name', 'contact'] };

document.querySelectorAll('#consultationTableBody .st-data').forEach(span => {
    Object.assign(span.closest('tr').dataset, span.dataset);
});
*/
// ==== [ST-CONSULT-0] END ====


// ==== [ST-CONSULT-1] START — Status tabs: All / Waiting / In Progress / Completed ====
// Gamit: ipakita ra ang naghulat, ang gina-consult, o ang nahuman na (naay ihap matag tab).
/*
(() => {
    const box = document.getElementById('stConsultTabs');
    if (!box) return;

    const rows = Array.from(document.querySelectorAll('#consultationTableBody .patient-row'));
    const statuses = ['Waiting', 'In Progress', 'Completed'];
    let active = '';

    const render = () => {
        box.innerHTML = [`<button type="button" class="st-chip${active ? '' : ' active'}" data-status="">All <strong>${rows.length}</strong></button>`]
            .concat(statuses.map(status => {
                const count = rows.filter(row => row.dataset.status === status).length;
                return `<button type="button" class="st-chip${active === status ? ' active' : ''}" data-status="${status}">${status} <strong>${count}</strong></button>`;
            }))
            .join('');
    };

    box.addEventListener('click', event => {
        const tab = event.target.closest('.st-chip');
        if (!tab) return;
        active = tab.dataset.status;
        render();
        STTable.refresh(ST_CONSULT_TABLE);
    });

    STTable.filter(ST_CONSULT_TABLE, 'status', row => !active || row.dataset.status === active);
    render();
})();
*/
// ==== [ST-CONSULT-1] END ====


// ==== [ST-CONSULT-2] START — Filter by Date (Today / This Week / This Month) ====
/*
STTable.filter(ST_CONSULT_TABLE, 'date', row => {
    return STDate.inPreset(row.dataset.date, document.getElementById('stConsultDatePreset').value);
}, '#stConsultDatePreset');
*/
// ==== [ST-CONSULT-2] END ====


// ==== [ST-CONSULT-3] START — Sort (Ascending / Descending) ====
/*
(() => {
    const select = document.getElementById('stConsultSort');
    const body = document.getElementById('consultationTableBody');
    const originalOrder = Array.from(body.querySelectorAll('.patient-row'));

    select.addEventListener('change', () => {
        if (!select.value) {
            // Balik sa default nga order (gikan sa server: In Progress → Waiting → Completed).
            originalOrder.forEach(row => body.appendChild(row));
            STTable.sort(ST_CONSULT_TABLE, null);
            return;
        }
        const [key, type, direction] = select.value.split(':');
        STTable.sort(ST_CONSULT_TABLE, STTable.by(key === 'date' ? 'sortdate' : key, type, direction));
    });
})();
*/
// ==== [ST-CONSULT-3] END ====


// ==== [ST-CONSULT-4] START — Filter by Visit Type ug Service ====
/*
STTable.filter(ST_CONSULT_TABLE, 'visit', row => {
    const visit = document.getElementById('stConsultVisitFilter').value;
    const service = document.getElementById('stConsultServiceFilter').value;
    return (!visit || row.dataset.visittype === visit) && (!service || row.dataset.service === service);
}, '#stConsultVisitFilter', '#stConsultServiceFilter');
*/
// ==== [ST-CONSULT-4] END ====


// ==== [ST-CONSULT-5] START — Pagination (10 / 25 / 50 / All kada page) ====
/*
(() => {
    const size = document.getElementById('stConsultPageSize');
    const apply = () => STTable.paginate(ST_CONSULT_TABLE, size.value, '#stConsultPager');
    size.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-CONSULT-5] END ====


// ==== [ST-CONSULT-6] START — Queue number (Q-001, Q-002...) sa mga naghulat karon ====
// Gamit: kinsa ang una sa pila. Ang una nga naghulat (pinakauna nga oras) = Q-001.
/*
(() => {
    const today = STDate.iso();
    Array.from(document.querySelectorAll('#consultationTableBody .patient-row'))
        .filter(row => row.dataset.status === 'Waiting' && row.dataset.date === today)
        .sort((a, b) => a.dataset.sortdate.localeCompare(b.dataset.sortdate))
        .forEach((row, index) => {
            row.querySelector('.cell-name').insertAdjacentHTML('afterbegin',
                `<span class="st-queue-no">Q-${String(index + 1).padStart(3, '0')}</span>`);
        });
})();
*/
// ==== [ST-CONSULT-6] END ====


// ==== [ST-CONSULT-9] START — BMI auto-compute sa Family Planning form (Height m, Weight kg) ====
// BMI = weight ÷ (height × height). < 18.5 Underweight, < 25 Normal, < 30 Overweight, 30+ Obese.
/*
(() => {
    const height = document.querySelector('[name="FamilyPlanningRecord.PE_Height"]');
    const weight = document.querySelector('[name="FamilyPlanningRecord.PE_Weight"]');
    if (!height || !weight) return;

    const output = document.createElement('span');
    output.className = 'st-bmi';
    weight.insertAdjacentElement('afterend', output);

    const update = () => {
        const h = parseFloat(height.value);
        const w = parseFloat(weight.value);
        if (!(h > 0) || !(w > 0)) {
            output.textContent = '';
            return;
        }
        const bmi = w / (h * h);
        const label = bmi < 18.5 ? 'Underweight' : bmi < 25 ? 'Normal' : bmi < 30 ? 'Overweight' : 'Obese';
        output.textContent = `BMI: ${bmi.toFixed(1)} (${label})`;
    };
    height.addEventListener('input', update);
    weight.addEventListener('input', update);
    update();
})();
*/
// ==== [ST-CONSULT-9] END ====


// ==== [ST-CONSULT-10] START — Warning kung abnormal ang vital signs (BP ug Temperature) ====
// High BP: 140/90 pataas. Low BP: ubos sa 90 systolic. Fever: 37.5°C pataas. Low temp: ubos sa 35.5°C.
/*
(() => {
    const warn = (input, message) => {
        let note = input.nextElementSibling?.classList.contains('st-vital-warning') ? input.nextElementSibling : null;
        if (!message) {
            note?.remove();
            return;
        }
        if (!note) {
            note = document.createElement('span');
            note.className = 'st-vital-warning';
            input.insertAdjacentElement('afterend', note);
        }
        note.textContent = message;
    };

    const checkBp = (systolic, diastolic) => {
        if (systolic >= 140 || diastolic >= 90) return '⚠ High BP';
        if (systolic > 0 && systolic < 90) return '⚠ Low BP';
        return '';
    };

    document.addEventListener('input', event => {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.name) return;
        const name = input.name;

        if (/BloodPressure$/.test(name) && !/_(Systolic|Diastolic)$/.test(name)) {
            const match = /^(\d{2,3})\s*\/\s*(\d{2,3})/.exec(input.value.trim());
            warn(input, match ? checkBp(Number(match[1]), Number(match[2])) : '');
        } else if (/PE_BloodPressure_(Systolic|Diastolic)$/.test(name)) {
            const systolic = Number(document.querySelector('[name="FamilyPlanningRecord.PE_BloodPressure_Systolic"]')?.value) || 0;
            const diastolic = Number(document.querySelector('[name="FamilyPlanningRecord.PE_BloodPressure_Diastolic"]')?.value) || 0;
            warn(input, checkBp(systolic, diastolic));
        } else if (/Temperature$/.test(name)) {
            const temperature = parseFloat(input.value);
            warn(input, temperature >= 37.5 ? '⚠ Fever' : temperature > 0 && temperature < 35.5 ? '⚠ Low temperature' : '');
        }
    });
})();
*/
// ==== [ST-CONSULT-10] END ====


// ==== [ST-CONSULT-11] START — Dropdown imbes text: Newborn Gender ug Prenatal Blood Type ====
// (Ang server validation kay naa sa ConsultationController.BuildClinicalRules, parehas nga #11.)
/*
(() => {
    const toSelect = (name, options) => {
        const input = document.querySelector(`input[name="${name}"]`);
        if (!input) return;
        const select = document.createElement('select');
        select.name = input.name;
        select.id = input.id;
        select.className = input.className;
        select.innerHTML = '<option value="">-- Select --</option>'
            + options.map(option => `<option value="${option}">${option}</option>`).join('');
        select.value = options.includes(input.value) ? input.value : '';
        input.replaceWith(select);
    };

    toSelect('NewbornRecord.Gender', ['Male', 'Female']);
    toSelect('PrenatalRecord.BloodType', ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-']);
})();
*/
// ==== [ST-CONSULT-11] END ====


// ==== [ST-CONSULT-14] START — Newborn consent: auto-fill sa midwife name ug petsa ====
// Ang ngalan gikan sa naka-login nga user (topbar); ang petsa = karon.
/*
(() => {
    const midwifeName = document.querySelector('[name="NewbornRecord.ConsentMidwifeName"]');
    const midwifeDate = document.querySelector('[name="NewbornRecord.ConsentMidwifeDate"]');
    const userName = document.querySelector('.topbar .user-name')?.textContent.trim();
    if (midwifeName && !midwifeName.value && userName) midwifeName.value = userName;
    if (midwifeDate && !midwifeDate.value) midwifeDate.value = STDate.iso();
})();
*/
// ==== [ST-CONSULT-14] END ====


// ==== [ST-CONSULT-15] START — Print Record (gikan sa View Record modal) ====
/*
document.getElementById('stPrintRecord')?.addEventListener('click', () => {
    const title = document.getElementById('consultationRecordSubtitle').textContent;
    STPrint.element(document.getElementById('consultationRecordBody'), `Consultation Record — ${title}`);
});
*/
// ==== [ST-CONSULT-15] END ====
