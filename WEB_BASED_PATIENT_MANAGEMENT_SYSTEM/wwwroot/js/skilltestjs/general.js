// ============================================================
// skilltestjs/general.js — SKILL TEST features para sa TANANG page.
// Gi-load sa Views/Shared/_Layout.cshtml.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// CSS (skilltestcss/general.css) ug C# nga parehas og numero kung naa.
//
// I-UNCOMMENT UNA ANG [ST-GENERAL-0] (SETUP) sa Views/Shared/_Layout.cshtml
// aron ma-load kini nga file.
// ============================================================


// ==== [ST-GENERAL-1] START — Close (X) button + auto-hide sa success/error alerts ====
// Gamit: ang green/red nga message sa taas sa page (TempData) ma-close, ug ang
//        success message kusang mawala human sa 5 seconds.
/*
document.querySelectorAll('.page-alerts .alert').forEach(alert => {
    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'st-alert-close';
    close.setAttribute('aria-label', 'Close');
    close.innerHTML = '&times;';
    close.addEventListener('click', () => alert.remove());
    alert.appendChild(close);

    // Ang success ra ang kusang mawala; ang error magpabilin hangtod i-close.
    if (alert.classList.contains('alert-success')) {
        setTimeout(() => {
            alert.classList.add('st-alert-hide');
            setTimeout(() => alert.remove(), 400);
        }, 5000);
    }
});
*/
// ==== [ST-GENERAL-1] END ====


// ==== [ST-GENERAL-2] START — Pula nga asterisk (*) sa label sa required nga fields ====
// Gamit: makita dayon sa user kung unsang field ang kinahanglan sudlan.
/*
document.querySelectorAll('input[required], select[required], textarea[required]').forEach(field => {
    if (!field.id) return;
    const label = document.querySelector(`label[for="${field.id}"]`);
    if (!label || label.querySelector('.st-required')) return;

    const star = document.createElement('span');
    star.className = 'st-required';
    star.textContent = ' *';
    star.setAttribute('aria-hidden', 'true');
    label.appendChild(star);
});
*/
// ==== [ST-GENERAL-2] END ====


// ==== [ST-GENERAL-3] START — Likayan ang double-click sa Save (disable + spinner) ====
// Gamit: inig submit sa form, ma-disable ang Save button aron dili ma-save kaduha.
/*
document.addEventListener('submit', event => {
    const form = event.target;
    // setTimeout(0): human sa validation sa module. Kung na-cancel (preventDefault),
    // pananglitan sayop ang input o AJAX ang form, ayaw i-disable.
    setTimeout(() => {
        if (event.defaultPrevented) return;
        form.querySelectorAll('button[type="submit"]').forEach(button => {
            button.disabled = true;
            if (!button.querySelector('.st-spinner')) {
                button.insertAdjacentHTML('afterbegin', '<span class="spinner-border spinner-border-sm st-spinner" aria-hidden="true"></span>');
            }
        });
    }, 0);
});

// Inig Back sa browser, i-enable balik ang mga button.
window.addEventListener('pageshow', () => {
    document.querySelectorAll('.st-spinner').forEach(spinner => {
        spinner.closest('button').disabled = false;
        spinner.remove();
    });
});
*/
// ==== [ST-GENERAL-3] END ====


// ==== [ST-GENERAL-4] START — Live nga petsa ug oras sa topbar ====
// Gamit: makita ang petsa ug oras karon (mo-update matag segundo).
/*
(() => {
    const topbarRight = document.querySelector('.topbar-right');
    if (!topbarRight) return;

    const clock = document.createElement('div');
    clock.className = 'st-clock';
    clock.setAttribute('aria-live', 'off');
    topbarRight.prepend(clock);

    const tick = () => {
        clock.textContent = new Date().toLocaleString('en-PH', {
            weekday: 'short', month: 'short', day: 'numeric', year: 'numeric',
            hour: 'numeric', minute: '2-digit', second: '2-digit'
        });
    };
    tick();
    setInterval(tick, 1000);
})();
*/
// ==== [ST-GENERAL-4] END ====


// ==== [ST-GENERAL-5] START — Confirmation inig Logout ====
// Gamit: mangutana una "Are you sure you want to log out?" sa dili pa mo-logout.
// Gigamit ang existing nga confirm modal sa _Layout (showConfirmModal sa site.js).
/*
document.querySelector('.sidebar-footer form')?.addEventListener('submit', function (event) {
    event.preventDefault();
    const form = this;
    const title = document.getElementById('confirmActionModalLabel');
    const button = document.getElementById('confirmActionConfirmBtn');
    const oldTitle = title.textContent;
    const oldButton = button.innerHTML;

    // Usbon ang "Delete" nga text ngadto sa "Logout" samtang abli ang modal.
    title.textContent = 'Confirm Logout';
    button.innerHTML = '<i class="bi bi-box-arrow-left" aria-hidden="true"></i> Logout';
    document.getElementById('confirmActionModal').addEventListener('hidden.bs.modal', () => {
        title.textContent = oldTitle;
        button.innerHTML = oldButton;
    }, { once: true });

    showConfirmModal('Are you sure you want to log out?', () => form.submit());
});
*/
// ==== [ST-GENERAL-5] END ====


// ==== [ST-GENERAL-6] START — Auto-logout kung walay activity (15 minutes) ====
// Gamit: security — kung gibiyaan ang computer nga naka-login, kusang mo-logout.
// (Naa sab C# part sa Program.cs: ang cookie kay mo-expire sa 30 minutes.)
/*
(() => {
    const IDLE_MINUTES = 15;
    const logoutForm = document.querySelector('.sidebar-footer form');
    if (!logoutForm) return;

    let timer;
    const reset = () => {
        clearTimeout(timer);
        timer = setTimeout(() => logoutForm.submit(), IDLE_MINUTES * 60 * 1000);
    };

    ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'].forEach(type =>
        document.addEventListener(type, reset, { passive: true }));
    reset();
})();
*/
// ==== [ST-GENERAL-6] END ====


// ==== [ST-GENERAL-7] START — Dark mode toggle (button sa topbar) ====
// Gamit: mangitngit nga theme para dili sakit sa mata; gi-remember sa browser.
/*
(() => {
    const KEY = 'st-dark-mode';
    const topbarRight = document.querySelector('.topbar-right');
    if (!topbarRight) return;

    const apply = on => document.documentElement.classList.toggle('st-dark', on);
    try { apply(localStorage.getItem(KEY) === '1'); } catch { }

    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'st-dark-toggle';
    button.title = 'Toggle dark mode';
    button.setAttribute('aria-label', 'Toggle dark mode');
    button.innerHTML = '<i class="bi bi-moon-stars" aria-hidden="true"></i>';
    button.addEventListener('click', () => {
        const on = !document.documentElement.classList.contains('st-dark');
        apply(on);
        try { localStorage.setItem(KEY, on ? '1' : '0'); } catch { }
    });
    topbarRight.prepend(button);
})();
*/
// ==== [ST-GENERAL-7] END ====
