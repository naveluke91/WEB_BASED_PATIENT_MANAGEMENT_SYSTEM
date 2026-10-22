// ============================================================
// skilltestjs/notification.js — SKILL TEST features sa Notification bell
// (topbar). Gi-load sa Views/Shared/_Layout.cshtml.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// _Layout.cshtml, AppointmentsController.cs ug skilltestcss/notification.css
// nga parehas og numero kung naa.
//
// I-UNCOMMENT UNA ANG [ST-NOTIF-0] (SETUP) sa Views/Shared/_Layout.cshtml
// aron ma-load kini nga file.
// ============================================================


// ==== [ST-NOTIF-1] START — I-click ang notification → adto sa Appointments ug i-highlight ====
// Gamit: dili lang ma-mark as read; moadto dayon sa appointment nga gi-click.
/*
document.querySelectorAll('.notification-item[data-appointment-id]').forEach(item => {
    const goToAppointment = () => {
        // Hulata gamay aron ma-save una ang "read" (site.js) una mobalhin og page.
        setTimeout(() => {
            window.location.href = `/Appointments?highlight=${encodeURIComponent(item.dataset.appointmentId)}`;
        }, 400);
    };
    item.addEventListener('click', goToAppointment);
    item.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === ' ') goToAppointment();
    });
});

// Sa Appointments page: i-highlight ug i-scroll ang appointment nga gi-click.
(() => {
    const id = new URLSearchParams(window.location.search).get('highlight');
    if (!/^\d+$/.test(id || '') || !window.location.pathname.toLowerCase().startsWith('/appointments')) return;

    // Pangitaon ang row pinaagi sa View button niini (onclick="openViewModal(ID)").
    const row = document.querySelector(`#apptTableBody button[onclick="openViewModal(${id})"]`)?.closest('tr');
    if (!row) return;
    row.classList.add('st-row-highlight');
    row.scrollIntoView({ block: 'center', behavior: 'smooth' });
})();
*/
// ==== [ST-NOTIF-1] END ====


// ==== [ST-NOTIF-2] START — "Mark all as read" ====
// Gamit: usa ra ka click aron mawala tanan notification (para sa naka-login ra).
/*
document.getElementById('stMarkAllRead')?.addEventListener('click', async event => {
    event.stopPropagation(); // ayaw isira ang dropdown
    const button = event.currentTarget;
    const token = document.querySelector('#notificationReadTokenForm input[name="__RequestVerificationToken"]')?.value;
    if (!token) return;

    button.disabled = true;
    const response = await fetch('/Appointments/MarkAllNotificationsRead', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: '__RequestVerificationToken=' + encodeURIComponent(token)
    });

    if (!response.ok) {
        button.disabled = false;
        return;
    }

    const empty = document.createElement('div');
    empty.className = 'notification-empty';
    empty.textContent = 'No upcoming appointments or follow-up visits.';
    document.querySelector('.notification-list')?.replaceWith(empty);
    document.querySelector('.notification-badge')?.remove();
    button.remove();
});
*/
// ==== [ST-NOTIF-2] END ====
