$(document).ready(function () {
    // Search modal window
    $('.functions .functions__search').click(function () {
        $('.search__window, .search__window-form').toggleClass('active');
    })
    
    $('.search__window-close').click(function () {
        $('.search__window, .search__window-form').removeClass('active');
    })
    
    // Scrolling navbar
    let lastScrollTop = 0;
    let navbar = document.getElementById('navbar');
    let navbarHeight = $('#navbar').css('height');

    window.addEventListener("scroll", function () {
        let scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        if (scrollTop > lastScrollTop) {
            navbar.style.top = `-${navbarHeight}`;
        } else {
            navbar.style.top = "0";
        }

        lastScrollTop = scrollTop;
    })

    // Burger
    $('.navbar__burger').click(function () {
        $('.navbar__burger, .navbar__menu, .account__menu').toggleClass('active')
    })

    // Comment user selector
    $('.post__comments-wrapper').click(function () {
        let userName = $(this).find('.post__comments-username').text();
        let postCommentInput = $('.post__comments-input');

        postCommentInput.val(userName + ', ');
    })
})