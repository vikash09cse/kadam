$(function () { // shorthand document.ready

    // Make Menu Active on Click 
    var path = window.location.href;

    $('#navbar-nav .simplebar-content .nav-item > a').each(function () {
        if (this.href === path) {
            $(this).closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
            $(this).closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
            $(this).addClass('active');
        } else {
            $(this).removeClass('active');
        }
    });

    if ($(".page-content").hasClass("pc-viewclient")) {
        $('#viewclient-link').closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
        $('#viewclient-link').closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
    	$('#viewclient-link').addClass('active');
    }

    else if ($(".page-content").hasClass("pc-backofficeusers")) {
        $('#backoffice-link').closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
        $('#backoffice-link').closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
        $('#backoffice-link').addClass('active');
    }
    else if ($(".page-content").hasClass("pc-ordermaster")) {
        $('#view-new-orders').closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
        $('#view-new-orders').closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
        $('#view-new-orders').addClass('active');
    }

    else if ($(".page-content").hasClass("pc-returnitems")) {
        $('#update-return-items').closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
        $('#update-return-items').closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
        $('#update-return-items').addClass('active');
    }

    else if ($(".page-content").hasClass("pc-shippingaddress")) {
        $('#shipping-address').closest('.menu-dropdown').removeClass('collapsed').addClass('collapse show');
        $('#shipping-address').closest('.menu-dropdown').siblings('.menu-link').attr('aria-expanded', 'true');
        $('#shipping-address').addClass('active');
    }

})

$(window).on("load", function () {

    // jquery close icon not appearing due to bootstrap. fixed via below code
    $.fn.bootstrapBtn = $.fn.button.noConflict();

});
