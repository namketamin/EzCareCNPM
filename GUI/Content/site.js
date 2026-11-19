document.querySelectorAll('[data-print]').forEach(function (button) {
    button.addEventListener('click', function () { window.print(); });
});

var role = document.getElementById('staff-role');
var doctorFields = document.getElementById('doctor-fields');
if (role && doctorFields) {
    function toggleDoctorFields() {
        doctorFields.hidden = role.value !== '2';
        doctorFields.disabled = doctorFields.hidden;
    }
    role.addEventListener('change', toggleDoctorFields);
    toggleDoctorFields();
}

var navigation = document.getElementById('public-navigation');
var menu = document.querySelector('.menu-toggle');
if (navigation && menu) {
    navigation.classList.add('is-collapsed');
    menu.addEventListener('click', function () {
        var expanded = menu.getAttribute('aria-expanded') === 'true';
        menu.setAttribute('aria-expanded', String(!expanded));
        navigation.classList.toggle('is-collapsed', expanded);
        menu.textContent = expanded ? 'Menu' : 'Đóng';
    });
}

document.querySelectorAll('.password-toggle').forEach(function (button) {
    button.addEventListener('click', function () {
        var input = document.getElementById(button.getAttribute('aria-controls'));
        var visible = input.type === 'password';
        input.type = visible ? 'text' : 'password';
        button.textContent = visible ? 'Ẩn' : 'Hiện';
        button.setAttribute('aria-pressed', String(visible));
    });
});

var bookingForm = document.querySelector('.booking-form');
if (bookingForm) {
    var summary = bookingForm.querySelector('[data-slot-summary]');
    function updateSlotSummary() {
        var selected = bookingForm.querySelector('input[name="slot"]:checked:not(:disabled)');
        var submit = bookingForm.querySelector('[data-book-submit]');
        if (submit) submit.disabled = !selected;
        if (summary && selected) {
            var label = selected.closest('label');
            summary.textContent = 'Giờ đã chọn ' + label.querySelector('strong').textContent +
                ' · ' + label.querySelector('small').textContent;
            var progress = document.querySelectorAll('.booking-progress li');
            progress.forEach(function (step) { step.removeAttribute('aria-current'); });
            if (progress.length > 1) progress[1].setAttribute('aria-current', 'step');
        }
    }
    bookingForm.querySelectorAll('input[name="slot"]').forEach(function (input) {
        input.addEventListener('change', updateSlotSummary);
    });
    updateSlotSummary();
}

var error = document.querySelector('.notice.error');
if (error) error.focus();

var backToTop = document.querySelector('.back-to-top');
if (backToTop) {
    function toggleBackToTop() {
        var hide = window.scrollY < 320;
        if (backToTop.hidden !== hide) backToTop.hidden = hide;
    }
    window.addEventListener('scroll', toggleBackToTop, { passive:true });
    window.addEventListener('pageshow', toggleBackToTop);
    toggleBackToTop();
}


