// ============================================================
// reports.js — Reports module (Views/Reports/Index.cshtml)
// Print Report opens the browser's print dialog; reports.css hides the
// navigation and filters when printing.
// ============================================================

document.getElementById('printReport')?.addEventListener('click', function () {
    window.print();
});
