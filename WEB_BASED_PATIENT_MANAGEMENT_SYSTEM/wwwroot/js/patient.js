/**
 * Patient Management — client-side helpers
 * - Derives age from date of birth on form pages
 * - Filters patient table rows on the index page
 */

(function () {
    'use strict';

    /**
     * Calculates age in years from a date-of-birth string (YYYY-MM-DD).
     */
    function calculateAge(dateOfBirth) {
        if (!dateOfBirth) return '';

        var dateParts = dateOfBirth.split('-');
        if (dateParts.length !== 3) return '';

        // Use local date parts so the selected birthday is not shifted by timezone.
        var dob = new Date(dateParts[0], dateParts[1] - 1, dateParts[2]);
        if (Number.isNaN(dob.getTime())) return '';

        var today = new Date();

        // A future date or an age above the allowed range must not produce an age.
        if (dob > today) return '';

        var age = today.getFullYear() - dob.getFullYear();
        var monthDiff = today.getMonth() - dob.getMonth();

        // Subtract one if birthday has not occurred yet this year
        var monthDiff = today.getMonth() - dob.getMonth();

        // Subtract one if birthday has not occurred yet this year
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
            age--;
        }

        return age >= 0 && age <= 130 ? age : '';
    }

    /**
     * Wires up the DOB → Age auto-calculation on Create/Edit forms.
     */
    function initAgeCalculation() {
        var dobInput = document.getElementById('dateOfBirth') || document.querySelector('input[name="PrenatalRecord.DateOfBirth"]');
        var ageField = document.getElementById('ageField');
        var ageDisplay = document.getElementById('ageDisplay');
        var ageDisplaySection2 = document.getElementById('ageDisplaySection2');

        if (!dobInput) return;

        function updateAge() {
            // I-calculate ang edad kung naa'y sulod sa DOB field
            var computedAge = '';
            if (dobInput.value) {
                computedAge = calculateAge(dobInput.value);
            }
            
            if (ageField) ageField.value = computedAge;
            if (ageDisplay) ageDisplay.value = computedAge;
            if (ageDisplaySection2) ageDisplaySection2.value = computedAge;
        }

        // Check kon anaa ba kita sa Edit page (naay '/Edit' sa URL) o Details page
        var isEditPage = window.location.pathname.indexOf('/Edit') > -1 || window.location.pathname.indexOf('/Details') > -1;

        if (isEditPage) {
            updateAge();
        } else {
            // Blanko ang Age field sa pagsugod (Create page)
            if (ageField) ageField.value = '';
            if (ageDisplay) ageDisplay.value = '';
            if (ageDisplaySection2) ageDisplaySection2.value = '';
        }

        // Mag-calculate lang kung hilabtan o usbon na sa user ang DOB field
        dobInput.addEventListener('change', updateAge);
        dobInput.addEventListener('input', updateAge);
    }

    /**
     * Filters patient table rows based on the search input value.
     */
    function initPatientSearch() {
        var searchInput = document.getElementById('patientSearch');
        var tableBody = document.getElementById('patientTableBody');
        var paginationInfo = document.getElementById('paginationInfo');

        if (!searchInput || !tableBody) return;

        searchInput.addEventListener('input', function () {
            var query = searchInput.value.toLowerCase().trim();
            var rows = tableBody.querySelectorAll('.patient-row');
            var visibleCount = 0;

            rows.forEach(function (row) {
                var name = row.getAttribute('data-name') || '';
                var contact = row.getAttribute('data-contact') || '';
                var id = row.getAttribute('data-id') || '';
                var match = !query ||
                    name.includes(query) ||
                    contact.includes(query) ||
                    id.includes(query);

                row.classList.toggle('hidden', !match);
                if (match) visibleCount++;
            });

            // Update pagination info text to reflect filtered count
            if (paginationInfo) {
                var total = rows.length;
                if (visibleCount === 0) {
                    paginationInfo.textContent = 'No patients found';
                } else {
                    paginationInfo.textContent =
                        'Showing 1 to ' + visibleCount + ' of ' + total + ' patients';
                }
            }
        });
    }

    // Initialize when the DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initAgeCalculation();
        initPatientSearch();
    });
})();
