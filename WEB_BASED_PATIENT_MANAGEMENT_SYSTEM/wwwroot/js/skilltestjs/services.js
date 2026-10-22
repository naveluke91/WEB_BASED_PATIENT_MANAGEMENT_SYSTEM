// ============================================================
// skilltestjs/services.js — SKILL TEST features sa SERVICES page
// (Views/Services/Index.cshtml). Gi-load human sa js/services/services.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, Modals, ServicesController.cs ug skilltestcss/services.css
// nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-SERVICE-0] (SETUP) sa tanang file nga naay #0.
// Ang mga table feature mogamit sa STTable (skilltestjs/st-core.js).
// ============================================================


// ==== [ST-SERVICE-0] START — SETUP: config sa Services table (i-uncomment una ni) ====
// Ang <span class="st-data"> sa matag row (Index.cshtml) i-copy ngadto sa <tr>.
/*
const ST_SERVICE_TABLE = { body: '#servicesTableBody', search: '#servicesSearch', keys: ['name'] };

document.querySelectorAll('#servicesTableBody .st-data').forEach(span => {
    Object.assign(span.closest('tr').dataset, span.dataset);
});
*/
// ==== [ST-SERVICE-0] END ====


// ==== [ST-SERVICE-1] START — Filter by Service ====
// Gamit: ipakita ra ang usa ka klase sa service (e.g. Prenatal ra).
/*
STTable.filter(ST_SERVICE_TABLE, 'service', row => {
    const value = document.getElementById('stServiceFilter').value;
    return !value || row.dataset.service === value;
}, '#stServiceFilter');
*/
// ==== [ST-SERVICE-1] END ====


// ==== [ST-SERVICE-2] START — Filter by Type (Walk-In / Appointment) ====
/*
STTable.filter(ST_SERVICE_TABLE, 'type', row => {
    const value = document.getElementById('stServiceTypeFilter').value;
    return !value || row.dataset.type === value;
}, '#stServiceTypeFilter');
*/
// ==== [ST-SERVICE-2] END ====


// ==== [ST-SERVICE-3] START — Sort (Ascending / Descending) ====
// Ang value sa option = "data-key:type:direction", e.g. "price:number:desc".
/*
(() => {
    const select = document.getElementById('stServiceSort');
    const apply = () => {
        const [key, type, direction] = select.value.split(':');
        STTable.sort(ST_SERVICE_TABLE, STTable.by(key, type, direction));
    };
    select.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-SERVICE-3] END ====


// ==== [ST-SERVICE-4] START — Search pati sa service name (dili lang patient name) ====
/*
STTable.searchKeys(ST_SERVICE_TABLE, ['name', 'service']);
document.getElementById('servicesSearch').placeholder = 'Search by patient or service...';
*/
// ==== [ST-SERVICE-4] END ====


// ==== [ST-SERVICE-7] START — Total sa presyo sa makita nga services ====
// Gamit: makita dayon ang kinatibuk-ang kantidad (mo-usab inig search/filter).
/*
STTable.onRender(ST_SERVICE_TABLE, state => {
    const label = document.getElementById('stServiceTotal');
    if (!label) return;
    const total = state.matchedRows.reduce((sum, row) => sum + Number(row.dataset.price || 0), 0);
    label.textContent = `${state.matched} service${state.matched === 1 ? '' : 's'} · Total ₱ `
        + total.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
});
*/
// ==== [ST-SERVICE-7] END ====


// ==== [ST-SERVICE-8] START — Pagination (10 / 25 / 50 / All kada page) ====
/*
(() => {
    const size = document.getElementById('stServicePageSize');
    const apply = () => STTable.paginate(ST_SERVICE_TABLE, size.value, '#stServicePager');
    size.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-SERVICE-8] END ====


// ==== [ST-SERVICE-10] START — Presyo sa mga tile gikan sa server (ServicePrices) ====
// Gamit: usbon ra ang presyo sa ServicesController.ServicePrices, ug mo-sunod na
//        ang tanang tile sa Add/Edit Service modal.
/*
(() => {
    const prices = window.stServicePrices || {};
    const peso = value => 'PHP ' + Number(value).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    document.querySelectorAll('.svc-tile[data-service]').forEach(tile => {
        const price = prices[tile.dataset.service];
        if (price === undefined) return;
        if (tile.dataset.price !== undefined) tile.dataset.price = String(price);
        const label = tile.querySelector('.svc-price');
        if (label) label.textContent = peso(price);
    });
})();
*/
// ==== [ST-SERVICE-10] END ====


// ==== [ST-SERVICE-11] START — Pagpili og daghang service (Walk-In ra) ====
// Gamit: kung naka-check ang "Select more than one service", ang matag click sa
//        tile mo-add/remove; ipadala sa server isip "Prenatal|Anti-Tetanus Injection".
/*
(() => {
    const toggle = document.getElementById('stMultiService');
    const hidden = document.getElementById('selectedServicesInput');
    const appointment = document.getElementById('selectedAppointmentIdInput');
    const error = document.getElementById('serviceRegistrationError');
    const tiles = Array.from(document.querySelectorAll('#addServiceModal .svc-tile'));
    if (!toggle || !hidden) return;

    const selected = new Set();
    const apply = () => {
        tiles.forEach(tile => tile.classList.toggle('selected', selected.has(tile.dataset.service)));
        hidden.value = Array.from(selected).join('|');
    };

    // Ang original nga click (services.js) mauna: usa ra ang mapili.
    // Kung naka-check ang toggle, i-add/remove hinuon ang tile sa listahan.
    tiles.forEach(tile => tile.addEventListener('click', () => {
        if (!toggle.checked) return;
        const name = tile.dataset.service;
        if (selected.has(name)) selected.delete(name);
        else selected.add(name);
        apply();
    }));

    toggle.addEventListener('change', () => {
        selected.clear();
        if (toggle.checked && hidden.value) selected.add(hidden.value);
        apply();
    });

    // Ang daghang service kay para sa Walk-In ra (walay napili nga appointment).
    document.getElementById('serviceForm').addEventListener('submit', event => {
        if (toggle.checked && selected.size > 1 && appointment.value) {
            event.preventDefault();
            event.stopImmediatePropagation();
            error.textContent = 'More than one service can only be registered for a Walk-In patient.';
            error.classList.remove('d-none');
        }
    }, true);

    document.getElementById('addServiceModal').addEventListener('hidden.bs.modal', () => {
        toggle.checked = false;
        selected.clear();
    });
})();
*/
// ==== [ST-SERVICE-11] END ====
