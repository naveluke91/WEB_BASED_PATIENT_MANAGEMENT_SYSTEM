// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function confirmDelete(event, formElement) {
    const message = formElement.dataset.deleteMessage || 'Are you sure you want to delete this record?';
    if (!window.confirm(message)) {
        event.preventDefault();
    }
}

// ---- Validation sa mga form (gamiton sa matag module) ----
// Pula nga border + mubo nga mensahe ubos sa field; walay alert() o browser popup.
window.FormValidation = (() => {
    const ERROR_CLASS = 'input-validation-error';

    // Mga mensahe (parehas sa server).
    const MSG = {
        required: 'Kinahanglan kini nga field.',
        name: 'Dili valid ang ngalan.',
        contact: 'Dili valid ang contact number.',
        futureDate: 'Dili pwede future date.',
        date: 'Dili valid ang petsa.',
        option: 'Pilia ang valid nga opsyon.'
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
            if (text.length > max) return `Hangtod ${max} ka karakter lang.`;
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
            if (text.length > max) return `Hangtod ${max} ka karakter lang.`;
            return TEXT.test(text) ? '' : message;
        },
        address(value) {
            const text = String(value ?? '').trim();
            if (!text) return MSG.required;
            if (text.length > 250) return 'Hangtod 250 ka karakter lang.';
            return /[\p{L}\p{N}]/u.test(text) ? '' : 'Dili valid ang address.';
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
                    return ageOn(date) > 130 ? 'Dili pwede lapas 130 ang edad.' : '';
                }
            },
            { field: field('MaritalStatus'), test: el => isBlank(el.value) ? MSG.required : (MARITAL_STATUSES.includes(el.value) ? '' : MSG.option) },
            { field: field('Religion'), test: el => rules.text(el.value, 100, 'Dili valid ang relihiyon.') },
            { field: field('Occupation'), test: el => rules.text(el.value, 100, 'Dili valid ang trabaho.') },
            { field: field('ContactNo'), test: el => rules.contact(el.value) },
            {
                field: field('LMP'), test: el => {
                    if (isBlank(el.value)) return MSG.required;
                    const date = parseDate(el.value);
                    if (!date) return MSG.date;
                    if (date > today()) return MSG.futureDate;
                    const dob = dobValue();
                    return dob && date < dob ? 'Dili pwede una sa petsa sa pagkatawo.' : '';
                }
            },
            {
                field: field('AOG'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    const match = AOG.exec(text);
                    return text.length <= 50 && match && Number(match[1]) <= 45 ? '' : 'Dili valid ang AOG (pananglitan: 14 weeks).';
                }
            },
            {
                field: field('EDC'), test: el => {
                    if (isBlank(el.value)) return MSG.required;
                    const date = parseDate(el.value);
                    if (!date) return MSG.date;
                    const lmp = lmpValue();
                    if (lmp && date <= lmp) return 'Kinahanglan human sa LMP.';
                    const latest = lmp ? new Date(lmp.getFullYear(), lmp.getMonth(), lmp.getDate() + 315) : null;
                    return latest && date > latest ? 'Layo ra kaayo sa LMP.' : '';
                }
            },
            {
                field: field('Menarche'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    if (!WHOLE.test(text) || Number(text) < 5 || Number(text) > 30) return 'Dili valid ang edad sa menarche.';
                    const dob = dobValue();
                    return dob && Number(text) > ageOn(dob) ? 'Dili pwede lapas sa edad sa pasyente.' : '';
                }
            },
            {
                field: field('Gravida'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    return WHOLE.test(text) ? '' : 'Numero lang (0-99), walay decimal.';
                }
            },
            {
                field: field('TFAL'), test: el => {
                    const text = el.value.trim();
                    if (!text) return MSG.required;
                    return TFAL.test(text) ? '' : 'Pormat: 2-1-0-1.';
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
            showErrors(errors, findField) {
                let first = null;
                Object.entries(errors || {}).forEach(([key, message]) => {
                    const field = findField(key);
                    if (!field) return;
                    setError(field, message);
                    first = firstInPage(first, field);
                });
                if (first) focusField(first);
                return !!first;
            }
        };
    }

    return { MSG, rules, isBlank, parseDate, today, patientChecks, setError, clearError, clearAll, create };
})();

// Phone bottom bar: when the tabs scroll sideways, keep the current page's tab in view.
(() => {
    const nav = document.querySelector('.sidebar-nav');
    const activeTab = nav?.querySelector('.nav-item.active');
    if (!activeTab || !window.matchMedia('(max-width: 575.98px)').matches || nav.scrollWidth <= nav.clientWidth) return;

    activeTab.scrollIntoView({ block: 'nearest', inline: 'nearest' });
})();
