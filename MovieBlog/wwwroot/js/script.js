$(document).ready(function () {
    // Search window
    $('.functions .functions__search').click(function () {
        $('.search__window, .search__window-form').toggleClass('active');
    })

    $('.search__window-close').click(function () {
        $('.search__window, .search__window-form').removeClass('active');
    })


    // Scrolling navbar
    let lastScrollTop = 0;
    let navbar = document.getElementById('navbar');

    window.addEventListener("scroll", function () {
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        if (scrollTop > lastScrollTop) {
            navbar.style.top = "-6vw";
        } else {
            navbar.style.top = "0";
        }

        lastScrollTop = scrollTop;
    })
})