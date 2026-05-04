// Layout JavaScript functionality
(function() {
    'use strict';

    // Initialize layout when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeNavbar();
    });

    // Initialize navbar functionality
    function initializeNavbar() {
        const navbarToggler = document.querySelector('.navbar-toggler');
        const navbarCollapse = document.querySelector('.navbar-collapse');

        if (navbarToggler && navbarCollapse) {
            // Close mobile menu when clicking outside
            document.addEventListener('click', function(event) {
                const isClickInside = navbarCollapse.contains(event.target) || navbarToggler.contains(event.target);
                
                if (!isClickInside && navbarCollapse.classList.contains('show')) {
                    const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                    if (bsCollapse) {
                        bsCollapse.hide();
                    }
                }
            });

            // Close mobile menu when clicking on a nav link
            const navLinks = document.querySelectorAll('.nav-link-custom');
            navLinks.forEach(function(link) {
                link.addEventListener('click', function() {
                    if (window.innerWidth < 992) {
                        const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                        if (bsCollapse) {
                            bsCollapse.hide();
                        }
                    }
                });
            });
        }
    }
})();
