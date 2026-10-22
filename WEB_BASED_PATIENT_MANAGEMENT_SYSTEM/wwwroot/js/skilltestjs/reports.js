// ============================================================
// skilltestjs/reports.js — SKILL TEST features sa REPORTS page
// (Views/Reports/Index.cshtml). Gi-load human sa js/reports/reports.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, ReportsController.cs, ReportsPageViewModel.cs ug
// skilltestcss/reports.css nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-REPORT-0] (SETUP) sa Index.cshtml.
// ============================================================


// ==== [ST-REPORT-2] START — Export Excel (.xls) gikan sa report table nga makita ====
// Gamit: ma-download ang report ug maablihan sa Microsoft Excel.
/*
document.getElementById('stExportExcel')?.addEventListener('click', () => {
    const title = document.querySelector('.report-results .card-title')?.textContent.trim() || 'Report';
    const tables = Array.from(document.querySelectorAll('.report-results .report-section'))
        .filter(section => section.offsetParent !== null)
        .map(section => section.querySelector('table')?.outerHTML || '')
        .join('<br />');

    const html = `<html><head><meta charset="utf-8" /></head><body><h3>${title}</h3>${tables}</body></html>`;
    const blob = new Blob(['﻿' + html], { type: 'application/vnd.ms-excel' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `${title.replace(/[^\w]+/g, '-')}-${STDate.iso()}.xls`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(link.href), 1000);
});
*/
// ==== [ST-REPORT-2] END ====


// ==== [ST-REPORT-6] START — Quick dates: Today / This Week / This Month / This Year ====
// Gamit: dili na kinahanglan mopili og Date From ug Date To matag usa.
/*
document.querySelectorAll('.st-date-presets [data-preset]').forEach(button => {
    button.addEventListener('click', () => {
        const from = document.getElementById('dateFrom');
        const to = document.getElementById('dateTo');
        const today = new Date();
        let start;
        let end;

        switch (button.dataset.preset) {
            case 'today':
                start = today;
                end = today;
                break;
            case 'week':
                start = STDate.startOfWeek(today);
                end = STDate.addDays(start, 6);
                break;
            case 'month':
                start = STDate.startOfMonth(today);
                end = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                break;
            case 'year':
                start = new Date(today.getFullYear(), 0, 1);
                end = new Date(today.getFullYear(), 11, 31);
                break;
            default:
                from.value = '';
                to.value = '';
                return;
        }

        from.value = STDate.iso(start);
        to.value = STDate.iso(end);
    });
});
*/
// ==== [ST-REPORT-6] END ====


// ==== [ST-REPORT-7] START — Ipakita ra ang filter nga angay sa napili nga report ====
/*
(() => {
    const type = document.getElementById('reportType');
    const filter = document.getElementById('stReportFilter');
    if (!type || !filter) return;

    const sync = () => {
        let anyVisible = false;
        filter.querySelectorAll('optgroup').forEach(group => {
            const show = group.dataset.report === type.value;
            group.hidden = !show;
            group.disabled = !show;
            if (show) anyVisible = true;
        });
        if (filter.selectedOptions[0]?.parentElement?.disabled) filter.value = '';
        filter.closest('div').hidden = !anyVisible;
    };

    type.addEventListener('change', sync);
    sync();
})();
*/
// ==== [ST-REPORT-7] END ====


// ==== [ST-REPORT-8] START — Sort sa report: i-click ang column header (▲ ascending / ▼ descending) ====
/*
document.querySelectorAll('.report-table').forEach(table => {
    const headers = Array.from(table.querySelectorAll('thead th'));

    const valueOf = (row, index) => {
        const text = row.cells[index]?.textContent.trim() ?? '';
        const number = Number(text.replace(/[₱,\s]/g, ''));
        if (text && text !== '—' && !Number.isNaN(number)) return number;
        const date = Date.parse(text);
        if (!Number.isNaN(date) && /\d{4}/.test(text)) return date;
        return text.toLowerCase();
    };

    headers.forEach((header, index) => {
        header.classList.add('st-sortable');
        header.title = 'Click to sort';
        header.addEventListener('click', () => {
            const body = table.tBodies[0];
            const rows = Array.from(body.rows).filter(row => !row.querySelector('.table-empty'));
            const direction = header.dataset.sortDir === 'asc' ? 'desc' : 'asc';

            headers.forEach(other => delete other.dataset.sortDir);
            header.dataset.sortDir = direction;

            rows.sort((a, b) => {
                const left = valueOf(a, index);
                const right = valueOf(b, index);
                const result = left > right ? 1 : left < right ? -1 : 0;
                return direction === 'asc' ? result : -result;
            });
            rows.forEach(row => body.appendChild(row));
        });
    });
});
*/
// ==== [ST-REPORT-8] END ====


// ==== [ST-REPORT-12] START — Chart sa report (Chart.js) ====
// Ang datos kay gikan sa data-chart sa <canvas> (gi-compute sa Index.cshtml).
/*
(() => {
    const canvas = document.getElementById('stReportChart');
    if (!canvas || typeof Chart === 'undefined') return;

    const data = JSON.parse(canvas.dataset.chart);
    new Chart(canvas, {
        type: 'bar',
        data: {
            labels: Object.keys(data),
            datasets: [{
                label: canvas.dataset.title,
                data: Object.values(data),
                backgroundColor: '#468403',
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true } }
        }
    });
})();
*/
// ==== [ST-REPORT-12] END ====
