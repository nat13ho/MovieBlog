$(document).ready(function () {
    // Dark Mode
    const darkModeToggle = $('#toggle-mode__checkbox');
    let darkMode = localStorage.getItem('darkMode');
    let elements = $('body, .container, #navbar');
    let toggleButton = $('.toggle-mode__ball');

    const enableDarkMode = () => {
        elements.addClass('dark');
        localStorage.setItem('darkMode', 'enabled');
    };

    const disableDarkMode = () => {
        elements.removeClass('dark');
        localStorage.setItem('darkMode', null);
    };

    if (darkMode === 'enabled') {
        enableDarkMode();
        toggleButton.css('transform', 'translateX(24px)');
    }

    darkModeToggle.click(function () {
        darkMode = localStorage.getItem('darkMode');

        if (darkMode !== 'enabled') {
            enableDarkMode();
            toggleButton.css('transform', 'translateX(24px)');
        } else {
            disableDarkMode();
            toggleButton.css('transform', 'none');
        }
    })
})