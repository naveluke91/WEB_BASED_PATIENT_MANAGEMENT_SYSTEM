// ============================================================
// reports.js — Reports module (Views/Reports/Index.cshtml)
// Print Report opens the browser's print dialog; reports.css hides the
// navigation and filters when printing.
// ============================================================

document.getElementById('printReport')?.addEventListener('click', function () {
    window.print();
});

// ---- Validation sa Generate Reports ----
(() => {
    const form = document.querySelector('.report-filters form');
    const reportType = document.getElementById('reportType');
    const dateFrom = document.getElementById('dateFrom');
    const dateTo = document.getElementById('dateTo');
    const FV = window.FormValidation;
    if (!form || !reportType || !dateFrom || !dateTo || !FV) return;

    const validator = FV.create(form, () => [
        { field: reportType, test: el => el.value ? '' : 'Pilia ang klase sa report.' },
        { field: dateFrom, test: el => el.validity?.badInput ? FV.MSG.date : FV.rules.dateYear(el.value) },
        {
            field: dateTo, test: el => {
                const yearError = el.validity?.badInput ? FV.MSG.date : FV.rules.dateYear(el.value);
                if (yearError) return yearError;
                const from = FV.parseDate(dateFrom.value);
                const to = FV.parseDate(el.value);
                return from && to && to < from ? 'Dili pwede una sa Date From.' : '';
            }
        }
    ]);

    form.addEventListener('submit', event => {
        if (!validator.validate()) event.preventDefault();
    });
})();
