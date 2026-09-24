// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Shared confirmation modal (Views/Shared/_Layout.cshtml #confirmActionModal), used instead of
// window.confirm() for destructive actions across the app.
function showConfirmModal(message, onConfirm) {
    const modalElement = document.getElementById('confirmActionModal');
    if (!modalElement || typeof bootstrap === 'undefined') {
        // Falls back safely if the shared modal isn't on this page for some reason.
        if (window.confirm(message)) onConfirm();
        return;
    }

    document.getElementById('confirmActionMessage').textContent = message;
    const confirmButton = document.getElementById('confirmActionConfirmBtn');
    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);

    const handleConfirm = () => {
        confirmButton.removeEventListener('click', handleConfirm);
        modal.hide();
        onConfirm();
    };
    confirmButton.addEventListener('click', handleConfirm);
    modal.show();
}

function confirmDelete(event, formElement) {
    event.preventDefault();
    const message = formElement.dataset.deleteMessage || 'Are you sure you want to delete this record?';
    showConfirmModal(message, () => formElement.submit());
}

// ---- Validation sa mga form (gamiton sa matag module) ----
// Pula nga border + mubo nga mensahe ubos sa field; walay alert() o browser popup.
window.FormValidation = (() => {
    const ERROR_CLASS = 'input-validation-error';

    // Mga mensahe (parehas sa server).
    const MSG = {
        required: 'This field is required.',
        name: 'Enter a valid name.',
        contact: 'Contact number must contain 11 digits.',
        futureDate: 'Date cannot be in the future.',
        date: 'Enter a valid date.',
        option: 'Please select a valid option.'
    };

    const NAME = /^[\p{L}\p{M} .'’\-]+$/u;
    const CONTACT = /^(09\d{9}|\+639\d{9})$/;
    const TEXT = /^(?=.*\p{L})[\p{L}\p{M}\p{N} .,'’()&/\-]+$/u;
    const AOG = /^(\d{1,2})(\.\d{1,2})?\s*(weeks?|wks?|w)?(\s*(and\s+)?[0-6]\s*(days?|d))?$/i;
    const WHOLE = /^\d{1,2}$/;
    const TFAL = /^\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}\s*-\s*\d{1,2}$/;
    const MARITAL_STATUSES = ['Single', 'Married', 'Widowed', 'Separated'];

    // I-check kung walay sulod.
    const isBlank = value => !String(value ?? '').trim();

    // "yyyy-mm-dd" (o datetime-local) → lokal nga Date, o null.
    function parseDate(value) {
        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(String(value ?? ''));
        if (!match) return null;
        const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
        return Number.isNaN(date.getTime()) ? null : date;
    }

    function today() {
        const date = new Date();
        date.setHours(0, 0, 0, 0);
        return date;
    }

    function ageOn(dateOfBirth) {
        const now = today();
        let age = now.getFullYear() - dateOfBirth.getFullYear();
        const months = now.getMonth() - dateOfBirth.getMonth();
        if (months < 0 || (months === 0 && now.getDate() < dateOfBirth.getDate())) age--;
        return age;
    }

    // Mga rule nga magamit balik; ibalik ang mensahe o ''.
    const rules = {
        required: value => isBlank(value) ? MSG.required : '',
        personName(value, max = 150) {
            const text = String(value ?? '').trim();
            if (!text) return MSG.required;
            if (text.length > max) return `Use ${max} characters or fewer.`;
            return NAME.test(text) && (text.match(/\p{L}/gu) || []).length >= 2 ? '' : MSG.name;
        },
        contact(value) {
            const text = String(value ?? '').trim();
            if (!text) return MSG.required;
            return CONTACT.test(text) ? '' : MSG.contact;
        },
        text(value, max, message) {
            const text = String(value ?? '').trim();
            if (!text) return MSG.required;
            if (text.length > max) return `Use ${max} characters or fewer.`;
            return TEXT.test(text) ? '' : message;
        },
        address(value) {
            const text = String(value ?? '').trim();
            if (!text) return MSG.required;
            if (text.length > 250) return 'Use 250 characters or fewer.';
            return /[\p{L}\p{N}]/u.test(text) ? '' : 'Enter a valid address.';
        },
        // Valid nga tuig lang (1900-2100); blangko = OK.
        dateYear(value) {
            if (isBlank(value)) return '';
            const date = parseDate(value);
            return date && date.getFullYear() >= 1900 && date.getFullYear() <= 2100 ? '' : MSG.date;
        }
    };

    // Mga check sa patient form (Add, Edit, ug New Patient sa Appointments).
    function patientChecks(form) {
        const field = name => form.querySelector(`[name="${name}"]`);
        const dobValue = () => {
            const date = parseDate(field('DateOfBirth')?.value);
            return date && date <= today() && ageOn(date) <= 130 ? date : null;
        };
        const lmpValue = () => {
            const date = parseDate(field('LMP')?.value);
            const dob = dobValue();
            return date && date <= today() && (!dob || date >= dob) ? date : null;
        };

        return [
            { field: field('FullName'), test: el => rules.personName(el.value) },
            { field: field('Address'), test: el => rules.address(el.value) },
            {
                field: field('DateOfBirth'), test: el => {
                    if (isBlank(el.value)) return MSG.required;
                    const date = parseDate(el.value);
                    if (!date) return MSG.date;
                    if (date > today()) return MSG.futureDate;
                    return ageOn(date) > 130 ? 'Age cannot be more than 130 years.' : '';
                }
            },
            { field: field('MaritalStatus'), test: el => isBlank(el.value) ? MSG.required : (MARITAL_STATUSES.includes(el.value) ? '' : MSG.option) },
            { field: field('Religion'), test: el => rules.text(el.value, 100, 'Enter a valid religion.') },
            { field: field('Occupation'), test: el => rules.text(el.value, 100, 'Enter a valid occupation.') },
            { field: field('ContactNo'), test: el => rules.contact(el.value) },
            {
                field: field('LMP'), test: el => {
                    if (isBlank(el.value)) return MSG.required;
                    const date = parseDate(el.value);
                    if (!date) return MSG.date;
                    if (date > today()) return MSG.futureDate;
                    const dob = dobValue();
                    return dob && date < dob ? 'Date cannot be before the date of birth.' : '';
                }
            },
            {
                field: field('AOG'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    const match = AOG.exec(text);
                    return text.length <= 50 && match && Number(match[1]) <= 45 ? '' : 'Enter a valid AOG (example: 14 weeks).';
                }
            },
            {
                field: field('EDC'), test: el => {
                    if (isBlank(el.value)) return MSG.required;
                    const date = parseDate(el.value);
                    if (!date) return MSG.date;
                    const lmp = lmpValue();
                    if (lmp && date <= lmp) return 'Date must be after the LMP.';
                    const latest = lmp ? new Date(lmp.getFullYear(), lmp.getMonth(), lmp.getDate() + 315) : null;
                    return latest && date > latest ? 'Date is too far after the LMP.' : '';
                }
            },
            {
                field: field('Menarche'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    if (!WHOLE.test(text) || Number(text) < 5 || Number(text) > 30) return 'Enter a valid age at menarche.';
                    const dob = dobValue();
                    return dob && Number(text) > ageOn(dob) ? 'Cannot be more than the patient\'s age.' : '';
                }
            },
            {
                field: field('Gravida'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    return WHOLE.test(text) ? '' : 'Enter a whole number from 0 to 99.';
                }
            },
            {
                field: field('TFAL'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    return TFAL.test(text) ? '' : 'Use the format 2-1-0-1.';
                }
            }
        ];
    }

    // I-marka og pula ang field ug ipakita ang mensahe ubos niini.
    function setError(field, message, options = {}) {
        if (!field) return;
        const highlight = options.highlight || [field];
        highlight.forEach(el => el.classList.add(ERROR_CLASS));
        field._fvHighlight = highlight;
        field.style.removeProperty('border-color');
        field.setAttribute('aria-invalid', 'true');
        field.setAttribute('data-fv-invalid', '');

        if (options.quiet) {
            field.title = message;
            field._fvQuiet = true;
            return;
        }

        let messageElement = field._fvMessage;
        if (!messageElement) {
            messageElement = document.createElement('div');
            messageElement.className = 'field-validation-error text-danger' + (options.inline ? '' : ' fv-message');
            messageElement.setAttribute('data-fv-message', '');
            const anchor = field.parentElement;
            if (!options.inline && anchor && getComputedStyle(anchor).position === 'static') anchor.classList.add('fv-anchor');
            field.insertAdjacentElement('afterend', messageElement);
            field._fvMessage = messageElement;
            if (field.id) {
                messageElement.id = `${field.id}Error`;
                field.setAttribute('aria-describedby', messageElement.id);
            }
        }
        if (!options.inline) messageElement.style.left = `${field.offsetLeft}px`;
        messageElement.textContent = message;
    }

    // Tangtangon ang pula ug mensahe.
    function clearError(field) {
        if (!field) return;
        (field._fvHighlight || [field]).forEach(el => el.classList.remove(ERROR_CLASS));
        field.removeAttribute('aria-invalid');
        field.removeAttribute('data-fv-invalid');
        if (field._fvQuiet) {
            field.removeAttribute('title');
            field._fvQuiet = false;
        }
        if (field._fvMessage) {
            field.removeAttribute('aria-describedby');
            field._fvMessage.remove();
            field._fvMessage = null;
        }
    }

    function clearAll(root) {
        root?.querySelectorAll('[data-fv-invalid]').forEach(clearError);
    }

    function focusField(field) {
        const target = field.matches('input, select, textarea, button') ? field : field.querySelector('input, select, textarea, button');
        target?.focus({ preventScroll: true });
        field.scrollIntoView({ block: 'center', behavior: 'smooth' });
    }

    // Ang field nga mas una sa page.
    function firstInPage(current, field) {
        return !current || (field.compareDocumentPosition(current) & Node.DOCUMENT_POSITION_FOLLOWING) ? field : current;
    }

    // Validator para sa usa ka form; ang getChecks mobalik og [{ field, test }].
    function create(form, getChecks) {
        if (!form) return null;
        form.noValidate = true;

        // I-check pag-usab ang pula nga field samtang nag-usab ang user.
        const recheck = () => {
            getChecks().forEach(check => {
                if (!check.field || !check.field.hasAttribute('data-fv-invalid')) return;
                const message = check.test(check.field);
                if (message) setError(check.field, message, check);
                else clearError(check.field);
            });
        };
        form.addEventListener('input', recheck);
        form.addEventListener('change', recheck);

        return {
            validate() {
                let first = null;
                getChecks().forEach(check => {
                    if (!check.field) return;
                    const message = check.test(check.field);
                    if (message) {
                        setError(check.field, message, check);
                        first = firstInPage(first, check.field);
                    } else {
                        clearError(check.field);
                    }
                });
                if (first) focusField(first);
                return !first;
            },
            recheck,
            reset: () => clearAll(form),
            // Sayop gikan sa server: { ngalan: mensahe }.
            showErrors(errors, findField, options) {
                let first = null;
                Object.entries(errors || {}).forEach(([key, message]) => {
                    const field = findField(key);
                    if (!field) return;
                    setError(field, message, options);
                    first = firstInPage(first, field);
                });
                if (first) focusField(first);
                return !!first;
            }
        };
    }

    return { MSG, rules, isBlank, parseDate, today, patientChecks, setError, clearError, clearAll, create };
})();

// Contact number: digits only while typing, maximum 11 (PH mobile format).
document.querySelectorAll('input[name$="ContactNo"]').forEach(el => {
    el.addEventListener('input', () => {
        el.value = el.value.replace(/\D/g, '').slice(0, 11);
    });
});

// AOG and EDC from the LMP (Naegele's rule: EDC = LMP + 280 days).
(() => {
    const { parseDate, today } = window.FormValidation;
    const plural = (count, unit) => `${count} ${unit}${count === 1 ? '' : 's'}`;
    const iso = date => `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;

    function aogText(lmp, onDate) {
        const days = Math.round((onDate - lmp) / 86400000);
        if (days < 0) return '';
        return plural(Math.floor(days / 7), 'week') + (days % 7 ? ' ' + plural(days % 7, 'day') : '');
    }

    function setValue(field, value) {
        if (!field || !value || field.value === value) return;
        field.value = value;
        field.dispatchEvent(new Event('input', { bubbles: true }));
    }

    function fillFromLmp(lmpField, withEdc = true) {
        const scope = lmpField.form || document;
        const prefix = lmpField.name.slice(0, -3);
        const lmp = parseDate(lmpField.value);
        if (!lmp || lmp > today()) return;
        const onDate = parseDate(scope.querySelector(`[name="${prefix}RecordDate"]`)?.value) || today();
        setValue(scope.querySelector(`[name="${prefix}AOG"]`), aogText(lmp, onDate));
        if (withEdc) setValue(scope.querySelector(`[name="${prefix}EDC"]`), iso(new Date(lmp.getFullYear(), lmp.getMonth(), lmp.getDate() + 280)));
    }

    document.addEventListener('change', event => {
        const field = event.target;
        if (!(field instanceof HTMLInputElement) || field.type !== 'date') return;

        if (field.name.endsWith('LMP')) {
            fillFromLmp(field);
        } else if (/\.PrenatalVisits\[[^\]]+\]\.RecordDate$/.test(field.name)) {
            const lmp = parseDate(field.form?.querySelector('[name="PrenatalRecord.LMP"]')?.value);
            const visitDate = parseDate(field.value);
            if (lmp && visitDate) setValue(field.form.querySelector(`[name="${field.name.replace(/RecordDate$/, 'AOG')}"]`), aogText(lmp, visitDate));
        } else if (field.name.endsWith('RecordDate')) {
            const lmpField = field.form?.querySelector(`[name="${field.name.slice(0, -10)}LMP"]`);
            if (lmpField) fillFromLmp(lmpField, false);
        }
    });

    window.fillAogFromLmp = fillFromLmp;
})();

// Phone bottom bar: when the tabs scroll sideways, keep the current page's tab in view.
(() => {
    const nav = document.querySelector('.sidebar-nav');
    const activeTab = nav?.querySelector('.nav-item.active');
    if (!activeTab || !window.matchMedia('(max-width: 575.98px)').matches || nav.scrollWidth <= nav.clientWidth) return;

    activeTab.scrollIntoView({ block: 'nearest', inline: 'nearest' });
})();

// Notification bell: clicking an item marks it read on the server and removes it from
// the dropdown (does not touch the Appointment itself).
document.querySelectorAll('.notification-item[data-appointment-id]').forEach(item => {
    const markRead = () => {
        const token = document.querySelector('#notificationReadTokenForm input[name="__RequestVerificationToken"]')?.value;
        if (!token) return;

        fetch('/Appointments/MarkNotificationRead/' + item.dataset.appointmentId, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: '__RequestVerificationToken=' + encodeURIComponent(token)
        }).then(response => {
            if (!response.ok) return;

            const list = item.closest('.notification-list');
            item.remove();

            const remaining = list ? list.querySelectorAll('.notification-item').length : 0;
            const badge = document.querySelector('.notification-badge');
            if (badge) {
                if (remaining > 0) badge.textContent = remaining;
                else badge.remove();
            }
            if (remaining === 0 && list) {
                list.outerHTML = '<div class="notification-empty">No upcoming appointments or follow-up visits.</div>';
            }
        });
    };

    item.addEventListener('click', markRead);
    item.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            markRead();
        }
    });
});
