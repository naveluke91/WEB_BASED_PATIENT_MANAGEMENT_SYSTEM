// ============================================================
// st-core.js — [ST-CORE] Helper para sa SKILL TEST (ST) features.
//
// ACTIVE ni nga file pero WALAY EPEKTO sa system hangtod naay
// ST feature (e.g. [ST-PATIENT-1]) nga i-uncomment ug mo-tawag niini.
// DILI ni kinahanglan i-uncomment.
//
// Naa diri:
//   1. STTable — search + filter + sort + pagination + "No results" sa
//      bisan unsang list table (ang mga row kay <tr class="patient-row">).
//   2. STPrint — mag-print og usa ka bahin sa page (receipt, record, etc.).
//   3. STDate  — gagmay nga helpers para sa petsa (yyyy-MM-dd).
//
// Example nga paggamit (tan-awa ang skilltestjs/patient.js):
//   const T = { body: '#patientTableBody', search: '#patientSearch', keys: ['name', 'contact', 'id'] };
//   STTable.filter(T, 'marital', row => ..., '#stMaritalFilter');  // filter
//   STTable.sort(T, STTable.by('name', 'text', 'asc'));            // sort A–Z
//   STTable.paginate(T, 10, '#stPatientPager');                     // 10 kada page
// ============================================================

window.STTable = (() => {
    'use strict';

    // Usa ka state kada table (key = tbody selector).
    const tables = new Map();

    function setup(cfg) {
        let table = tables.get(cfg.body);
        if (table) return table;

        const tbody = document.querySelector(cfg.body);
        if (!tbody) return null;

        table = {
            cfg,
            tbody,
            keys: [...(cfg.keys || ['name'])], // data-* nga gi-search
            filters: new Map(),                // ngalan → function(row) → true/false
            sorter: null,                      // function(a, b) para sa Array.sort
            pageSize: 0,                       // 0 = walay pagination
            page: 1,
            pager: null,
            listeners: []
        };
        tables.set(cfg.body, table);

        // I-apply balik ang tanan matag type sa search box. Kini nga listener
        // modagan human sa original search sa module, so kini ang masunod.
        document.querySelector(cfg.search)?.addEventListener('input', () => {
            table.page = 1;
            render(table);
        });

        return table;
    }

    function rows(table) {
        return Array.from(table.tbody.querySelectorAll('tr.patient-row'));
    }

    function matchesSearch(table, row) {
        const query = (document.querySelector(table.cfg.search)?.value || '').toLowerCase().trim();
        if (!query) return true;
        return table.keys.some(key => (row.dataset[key] || '').toLowerCase().includes(query));
    }

    // "No matching records" nga row kung walay makita.
    function toggleNoResults(table, show) {
        let row = table.tbody.querySelector('tr.st-no-results');
        if (show && !row) {
            const columns = table.tbody.closest('table')?.querySelectorAll('thead th').length || 1;
            row = document.createElement('tr');
            row.className = 'st-no-results';
            row.innerHTML = `<td colspan="${columns}" class="table-empty">No matching records found.</td>`;
            table.tbody.appendChild(row);
        } else if (!show && row) {
            row.remove();
        }
    }

    function renderPager(table, state) {
        const container = table.pager ? document.querySelector(table.pager) : null;
        if (!container) return;

        if (!table.pageSize) {
            container.replaceChildren();
            return;
        }

        const info = document.createElement('span');
        info.className = 'st-pager-info';
        info.textContent = state.matched === 0
            ? 'Showing 0 records'
            : `Showing ${state.start + 1}–${state.end} of ${state.matched}`;

        const controls = document.createElement('div');
        controls.className = 'st-pager-controls';

        const addButton = (label, page, disabled, active) => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'st-page-btn' + (active ? ' active' : '');
            button.textContent = label;
            button.disabled = disabled;
            button.addEventListener('click', () => {
                table.page = page;
                render(table);
            });
            controls.appendChild(button);
        };

        addButton('‹', state.page - 1, state.page <= 1, false);
        for (let page = 1; page <= state.pages; page++) {
            addButton(String(page), page, false, page === state.page);
        }
        addButton('›', state.page + 1, state.page >= state.pages, false);

        container.replaceChildren(info, controls);
    }

    function render(table) {
        const all = rows(table);

        // 1. Sort: i-arrange balik ang mga <tr> sa tbody.
        if (table.sorter) {
            all.sort(table.sorter).forEach(row => table.tbody.appendChild(row));
        }

        // 2. Search + tanan nga filter.
        const filters = Array.from(table.filters.values());
        const matched = all.filter(row => matchesSearch(table, row) && filters.every(test => test(row)));

        // 3. Pagination.
        const pages = table.pageSize ? Math.max(1, Math.ceil(matched.length / table.pageSize)) : 1;
        if (table.page > pages) table.page = pages;
        const start = table.pageSize ? (table.page - 1) * table.pageSize : 0;
        const end = table.pageSize ? Math.min(start + table.pageSize, matched.length) : matched.length;
        const visible = new Set(matched.slice(start, end));

        all.forEach(row => {
            const show = visible.has(row);
            row.classList.toggle('hidden', !show);
            row.style.display = show ? '' : 'none';
        });

        toggleNoResults(table, all.length > 0 && matched.length === 0);

        const state = { total: all.length, matched: matched.length, matchedRows: matched, start, end, page: table.page, pages };
        renderPager(table, state);
        table.listeners.forEach(listener => listener(state));
        return state;
    }

    // Comparer gikan sa data-* attribute: type = 'text' | 'number' | 'date'.
    // Ang blangko nga value pirmi sa ubos.
    function by(key, type = 'text', direction = 'asc') {
        const sign = direction === 'desc' ? -1 : 1;
        return (a, b) => {
            const left = a.dataset[key] || '';
            const right = b.dataset[key] || '';
            if (!left && !right) return 0;
            if (!left) return 1;
            if (!right) return -1;
            if (type === 'number') return (Number(left) - Number(right)) * sign;
            // Ang date kay "yyyy-MM-dd" (o "yyyy-MM-dd HH:mm"), so mo-sort na as text.
            return left.localeCompare(right, undefined, { sensitivity: 'base' }) * sign;
        };
    }

    return {
        // Mag-add og filter. Ang triggers = mga selector (select/input) nga
        // inig usab mo-apply balik sa filter.
        filter(cfg, name, test, ...triggers) {
            const table = setup(cfg);
            if (!table) return;
            table.filters.set(name, test);
            triggers.forEach(selector => {
                document.querySelectorAll(selector).forEach(element => {
                    ['input', 'change'].forEach(type => element.addEventListener(type, () => {
                        table.page = 1;
                        render(table);
                    }));
                });
            });
            render(table);
        },
        removeFilter(cfg, name) {
            const table = setup(cfg);
            if (!table) return;
            table.filters.delete(name);
            render(table);
        },
        sort(cfg, comparer) {
            const table = setup(cfg);
            if (!table) return;
            table.sorter = comparer;
            render(table);
        },
        paginate(cfg, size, pagerSelector) {
            const table = setup(cfg);
            if (!table) return;
            table.pageSize = Number(size) || 0;
            table.page = 1;
            if (pagerSelector) table.pager = pagerSelector;
            render(table);
        },
        searchKeys(cfg, keys) {
            const table = setup(cfg);
            if (!table) return;
            table.keys = [...keys];
            render(table);
        },
        // Tawagon matag render (para sa count, total, chips, etc.).
        onRender(cfg, listener) {
            const table = setup(cfg);
            if (!table) return;
            table.listeners.push(listener);
            render(table);
        },
        refresh(cfg) {
            const table = setup(cfg);
            return table ? render(table) : null;
        },
        by
    };
})();

// ---- STPrint: i-print ang sulod sa usa ka element sa bag-ong window ----
window.STPrint = (() => {
    'use strict';

    function escape(text) {
        return String(text ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    }

    function html(rawTitle, bodyHtml) {
        const printWindow = window.open('', '_blank', 'width=900,height=700');
        if (!printWindow) {
            alert('Please allow pop-ups to print.');
            return;
        }

        const title = escape(rawTitle);
        const origin = window.location.origin;
        printWindow.document.write(`<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8" />
<title>${title}</title>
<link rel="stylesheet" href="${origin}/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="${origin}/css/site.css" />
<link rel="stylesheet" href="${origin}/css/consultation/consultation.css" />
<style>
  body { background: #fff; padding: 24px; font-family: 'Segoe UI', Arial, sans-serif; }
  .st-print-header { text-align: center; margin-bottom: 16px; }
  .st-print-header h1 { font-size: 20px; margin: 0; }
  .st-print-header p { margin: 2px 0; color: #555; font-size: 13px; }
  .st-no-print, button, .btn-close { display: none !important; }
  table { width: 100%; border-collapse: collapse; }
  @media print { body { padding: 0; } }
</style>
</head>
<body>
<div class="st-print-header">
  <h1>Española Birthing Home</h1>
  <p>${title}</p>
  <p>Printed ${new Date().toLocaleString()}</p>
</div>
${bodyHtml}
</body>
</html>`);
        printWindow.document.close();
        // Hulaton ma-load ang CSS una mo-print.
        printWindow.onload = () => {
            printWindow.focus();
            printWindow.print();
        };
    }

    return {
        html,
        element(element, title) {
            if (!element) return;
            // I-copy ang current values sa mga input aron makita sa print.
            const clone = element.cloneNode(true);
            const originals = element.querySelectorAll('input, select, textarea');
            clone.querySelectorAll('input, select, textarea').forEach((field, index) => {
                const source = originals[index];
                if (!source) return;
                if (field.type === 'checkbox' || field.type === 'radio') {
                    if (source.checked) field.setAttribute('checked', 'checked');
                    else field.removeAttribute('checked');
                } else if (field.tagName === 'SELECT') {
                    Array.from(field.options).forEach((option, i) => {
                        if (source.options[i]?.selected) option.setAttribute('selected', 'selected');
                    });
                } else if (field.tagName === 'TEXTAREA') {
                    field.textContent = source.value;
                } else {
                    field.setAttribute('value', source.value);
                }
            });
            html(title, clone.outerHTML);
        }
    };
})();

// ---- STDate: gagmay nga helpers sa petsa ----
window.STDate = {
    // Petsa karon as "yyyy-MM-dd" (local time).
    iso(date = new Date()) {
        return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    },
    addDays(date, days) {
        const copy = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        copy.setDate(copy.getDate() + days);
        return copy;
    },
    // Sugod sa semana (Sunday) ug sugod sa bulan.
    startOfWeek(date = new Date()) {
        return window.STDate.addDays(date, -date.getDay());
    },
    startOfMonth(date = new Date()) {
        return new Date(date.getFullYear(), date.getMonth(), 1);
    },
    // "yyyy-MM-dd" → Date (local), o null.
    parse(text) {
        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(text || '');
        return match ? new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3])) : null;
    },
    // Ang petsa sa row ba (yyyy-MM-dd) naa sulod sa preset? preset = today | tomorrow | week | month | upcoming | past
    inPreset(text, preset) {
        if (!preset) return true;
        const date = window.STDate.parse(text);
        if (!date) return false;
        const today = window.STDate.parse(window.STDate.iso());
        const day = 86400000;
        switch (preset) {
            case 'today': return date.getTime() === today.getTime();
            case 'tomorrow': return date.getTime() === today.getTime() + day;
            case 'week': {
                const start = window.STDate.startOfWeek(today);
                return date >= start && date < window.STDate.addDays(start, 7);
            }
            case 'month': return date.getFullYear() === today.getFullYear() && date.getMonth() === today.getMonth();
            case 'upcoming': return date >= today;
            case 'past': return date < today;
            default: return true;
        }
    }
};
