// ============================================================
// skilltestjs/appointment.js — SKILL TEST features sa APPOINTMENTS page
// (Views/Appointments/Index.cshtml). Gi-load human sa js/appointment/appointment.js.
//
// PARA MA-ON ANG FEATURE: tangtanga ang "/*" nga linya ubos sa START
// marker ug ang "*/" nga linya ibabaw sa END marker. Buhata sab sa
// Index.cshtml, AppointmentsController.cs ug skilltestcss/appointment.css
// nga parehas og numero (kung naa).
//
// I-UNCOMMENT UNA ANG [ST-APPT-0] (SETUP) sa tanang file nga naay #0.
// Ang mga table feature mogamit sa STTable (skilltestjs/st-core.js).
// ============================================================


// ==== [ST-APPT-0] START — SETUP: config sa Appointments table (i-uncomment una ni) ====
// Ang <span class="st-data"> sa matag row (Index.cshtml) i-copy ngadto sa <tr>.
/*
const ST_APPT_TABLE = { body: '#apptTableBody', search: '#apptSearch', keys: ['name', 'contact'] };

document.querySelectorAll('#apptTableBody .st-data').forEach(span => {
    Object.assign(span.closest('tr').dataset, span.dataset);
});
*/
// ==== [ST-APPT-0] END ====


// ==== [ST-APPT-1] START — Filter by Status ====
// Gamit: ipakita ra ang Pending, Confirmed, Cancelled, etc.
/*
STTable.filter(ST_APPT_TABLE, 'status', row => {
    const value = document.getElementById('stApptStatusFilter').value;
    return !value || row.dataset.status === value;
}, '#stApptStatusFilter');
*/
// ==== [ST-APPT-1] END ====


// ==== [ST-APPT-2] START — Filter by Date (Today, This Week, Upcoming, Past o specific nga petsa) ====
/*
STTable.filter(ST_APPT_TABLE, 'date', row => {
    const preset = document.getElementById('stApptDatePreset').value;
    const exact = document.getElementById('stApptDate').value; // "yyyy-MM-dd"
    if (exact) return row.dataset.date === exact;
    return STDate.inPreset(row.dataset.date, preset);
}, '#stApptDatePreset', '#stApptDate');
*/
// ==== [ST-APPT-2] END ====


// ==== [ST-APPT-3] START — Sort (Ascending / Descending) ====
// Ang value sa option = "data-key:type:direction", e.g. "datetime:text:desc".
/*
(() => {
    const select = document.getElementById('stApptSort');
    const apply = () => {
        const [key, type, direction] = select.value.split(':');
        STTable.sort(ST_APPT_TABLE, STTable.by(key, type, direction));
    };
    select.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-APPT-3] END ====


// ==== [ST-APPT-4] START — Count chips matag status (i-click para i-filter) ====
// Gamit: makita dayon pila ang Pending, Confirmed, etc.; i-click aron i-filter.
/*
(() => {
    const box = document.getElementById('stApptChips');
    if (!box) return;

    const rows = Array.from(document.querySelectorAll('#apptTableBody .patient-row'));
    const statuses = ['Pending', 'Confirmed', 'Rescheduled', 'Cancelled', 'Completed', 'No-Show'];
    let active = '';

    const render = () => {
        const chips = [`<button type="button" class="st-chip${active ? '' : ' active'}" data-status="">All <strong>${rows.length}</strong></button>`];
        statuses.forEach(status => {
            const count = rows.filter(row => row.dataset.status === status).length;
            if (count) chips.push(`<button type="button" class="st-chip${active === status ? ' active' : ''}" data-status="${status}">${status} <strong>${count}</strong></button>`);
        });
        box.innerHTML = chips.join('');
    };

    box.addEventListener('click', event => {
        const chip = event.target.closest('.st-chip');
        if (!chip) return;
        active = chip.dataset.status;
        render();
        STTable.refresh(ST_APPT_TABLE);
    });

    STTable.filter(ST_APPT_TABLE, 'chip', row => !active || row.dataset.status === active);
    render();
})();
*/
// ==== [ST-APPT-4] END ====


// ==== [ST-APPT-5] START — I-highlight ang mga appointment karon nga adlaw ====
/*
document.querySelectorAll('#apptTableBody .patient-row').forEach(row => {
    if (row.dataset.date === STDate.iso()) row.classList.add('st-today');
});
*/
// ==== [ST-APPT-5] END ====


// ==== [ST-APPT-7] START — Pagination (10 / 25 / 50 / All kada page) ====
/*
(() => {
    const size = document.getElementById('stApptPageSize');
    const apply = () => STTable.paginate(ST_APPT_TABLE, size.value, '#stApptPager');
    size.addEventListener('change', apply);
    apply();
})();
*/
// ==== [ST-APPT-7] END ====


// ==== [ST-APPT-8] START — Notes: i-fill ang Notes sa Edit modal ====
// (Ang New Appointment modal kay ordinaryo nga textarea ra, walay JS.)
/*
(() => {
    const original = window.openEditModal;
    window.openEditModal = async function (id) {
        await original(id);
        const response = await fetch(`/Appointments/GetById/${encodeURIComponent(id)}`);
        if (!response.ok) return;
        const appointment = await response.json();
        document.getElementById('editNotes').value = appointment.notes || '';
    };
})();
*/
// ==== [ST-APPT-8] END ====


// ==== [ST-APPT-9] START — View modal: Service, Booked On ug Notes ====
/*
(() => {
    const original = window.openViewModal;
    window.openViewModal = async function (id) {
        await original(id);
        const response = await fetch(`/Appointments/StDetails/${encodeURIComponent(id)}`);
        if (!response.ok) return;
        const details = await response.json();
        document.getElementById('stViewService').textContent = details.service;
        document.getElementById('stViewCreatedAt').textContent = details.createdAt;
        document.getElementById('stViewNotes').textContent = details.notes;
    };
})();
*/
// ==== [ST-APPT-9] END ====


// ==== [ST-APPT-10] START — Bag-ong time slot: 4:00 PM (naa sab sa AppointmentsController.AllowedTimeSlots) ====
/*
ALL_SLOTS.push({ value: '16:00', label: '4:00 PM' });
*/
// ==== [ST-APPT-10] END ====


// ==== [ST-APPT-11] START — Same-day booking: pwede na mopili sa petsa karon ====
// Ang mga oras nga lapas na karon nga adlaw kay dili mapili.
/*
(() => {
    const today = STDate.iso();
    ['newDate', 'editDate'].forEach(id => {
        const input = document.getElementById(id);
        ['focus', 'click'].forEach(type => input.addEventListener(type, () => { input.min = today; }));
        input.min = today;
    });

    // Validation sa date (appointment.js): karon nga adlaw pwede na.
    checkApptDate = function (el) {
        if (FV.isBlank(el.value)) return 'Please select a date.';
        const date = FV.parseDate(el.value);
        if (!date) return FV.MSG.date;
        return date >= FV.today() ? '' : 'Please select a date from today onward.';
    };

    // I-disable ang mga oras nga lapas na kung karon ang petsa.
    const originalPopulate = populateTimeSelect;
    populateTimeSelect = function (selectEl, bookedTimes, currentValue = null) {
        originalPopulate(selectEl, bookedTimes, currentValue);
        const dateInput = document.getElementById(selectEl.id === 'editTimeSelect' ? 'editDate' : 'newDate');
        if (dateInput.value !== today) return;
        const now = new Date();
        const nowText = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
        Array.from(selectEl.options).forEach(option => {
            if (option.value && option.value <= nowText && !option.disabled) {
                option.disabled = true;
                option.textContent += ' (Passed)';
            }
        });
    };
})();
*/
// ==== [ST-APPT-11] END ====


// ==== [ST-APPT-12] START — Sirado kung Sunday ug holiday (client-side check) ====
// (Parehas nga lista sa holidays sa AppointmentsController.ValidateAppointmentInput.)
/*
(() => {
    const holidays = ['01-01', '04-09', '05-01', '06-12', '08-21', '11-01', '11-30', '12-25', '12-30', '12-31'];
    const previousCheck = checkApptDate;
    checkApptDate = function (el) {
        const message = previousCheck(el);
        if (message) return message;
        const date = FV.parseDate(el.value);
        if (date.getDay() === 0) return 'The clinic is closed on Sundays. Please choose another date.';
        if (holidays.includes(el.value.slice(5, 10))) return 'The clinic is closed on this holiday. Please choose another date.';
        return '';
    };
})();
*/
// ==== [ST-APPT-12] END ====


// ==== [ST-APPT-17] START — Ipakita ang "Reason for Cancellation" kung Cancelled ====
/*
(() => {
    const status = document.getElementById('editStatus');
    const section = document.getElementById('stCancelReasonSection');
    const reason = document.getElementById('stCancelReason');
    const sync = () => { section.hidden = status.value !== 'Cancelled'; };

    status.addEventListener('change', sync);

    const original = window.openEditModal;
    window.openEditModal = async function (id) {
        reason.value = '';
        await original(id);
        sync();
    };
})();
*/
// ==== [ST-APPT-17] END ====


// ==== [ST-APPT-18] START — Calendar View (bulan) ====
// Gamit: makita ang mga appointment sa calendar; i-click ang ngalan aron makita ang detalye.
/*
(() => {
    const card = document.getElementById('stCalendar');
    const grid = document.getElementById('stCalendarGrid');
    const title = document.getElementById('stCalendarTitle');
    const toggle = document.getElementById('stCalendarToggle');
    if (!card || !grid || !title || !toggle) return;

    const escape = text => String(text ?? '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
    const appointments = Array.from(document.querySelectorAll('#apptTableBody .patient-row')).map(row => ({
        id: row.dataset.id,
        date: row.dataset.date,
        datetime: row.dataset.datetime,
        time: row.dataset.time,
        status: row.dataset.status,
        name: row.querySelector('.cell-name').textContent.trim()
    }));
    let month = STDate.startOfMonth(new Date());

    const render = () => {
        title.textContent = month.toLocaleDateString('en-PH', { month: 'long', year: 'numeric' });
        const cells = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'].map(day => `<div class="st-cal-head">${day}</div>`);
        for (let i = 0; i < month.getDay(); i++) cells.push('<div class="st-cal-day st-cal-empty"></div>');

        const daysInMonth = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate();
        for (let day = 1; day <= daysInMonth; day++) {
            const iso = STDate.iso(new Date(month.getFullYear(), month.getMonth(), day));
            const items = appointments
                .filter(a => a.date === iso)
                .sort((a, b) => a.datetime.localeCompare(b.datetime))
                .map(a => `<button type="button" class="st-cal-item st-cal-${a.status.toLowerCase()}" data-id="${a.id}">${escape(a.time)} · ${escape(a.name)}</button>`)
                .join('');
            cells.push(`<div class="st-cal-day${iso === STDate.iso() ? ' st-cal-today' : ''}"><span class="st-cal-number">${day}</span>${items}</div>`);
        }
        grid.innerHTML = cells.join('');
    };

    toggle.addEventListener('click', () => {
        card.hidden = !card.hidden;
        if (!card.hidden) render();
    });
    document.getElementById('stCalendarPrev').addEventListener('click', () => {
        month = new Date(month.getFullYear(), month.getMonth() - 1, 1);
        render();
    });
    document.getElementById('stCalendarNext').addEventListener('click', () => {
        month = new Date(month.getFullYear(), month.getMonth() + 1, 1);
        render();
    });
    grid.addEventListener('click', event => {
        const item = event.target.closest('.st-cal-item');
        if (item) openViewModal(Number(item.dataset.id));
    });
})();
*/
// ==== [ST-APPT-18] END ====


// ==== [ST-APPT-20] START — Print Appointment Slip (gikan sa View modal) ====
/*
document.getElementById('stPrintAppointment')?.addEventListener('click', () => {
    const name = document.getElementById('viewPatientName').textContent;
    STPrint.element(document.querySelector('#modalViewAppointment .modal-body'), `Appointment Slip — ${name}`);
});
*/
// ==== [ST-APPT-20] END ====
