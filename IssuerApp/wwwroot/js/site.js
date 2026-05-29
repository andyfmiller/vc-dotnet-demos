// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

// Display an animated loading image during every navigation event

var hideLoader = function() {
    $("#loader").hide();
}

var showLoader = function() {
    $("#loader").show();
}

$(function() {
    var submitActor = null;

    // Find all the submit buttons
    var $submitActors = $(document).find("[type=submit]");

    // Capture the button when it is clicked
    $submitActors.click(function() {
        submitActor = this;
    });

    // Show the loader unless the actor is a download button
    $("form").each(function() {
        $(this).submit(function() {
            if (null === submitActor) {
                submitActor = $submitActors[0];
            }
        });
    });

    $(window).on("beforeunload",
        function(e) {
            if (submitActor === null || typeof submitActor === "undefined") {
                submitActor = e.target.activeElement;
            }

            if (!submitActor.classList.contains('btn-download')) {
                showLoader();
            }
        });
});

/**
 * Display the message in a bootstrap toast element.
 *
 * @param {string} message
 */
function showToast(message) {
    const el = $("#toast");
    $(".toast-body", el).text(message);
    el.parent().css("z-index", 1051); // Above modals
    el.toast("show");
    el.on("hidden.bs.toast",
        function() {
            el.parent().css("z-index", -1); // Below everything
        });
}

/**
 * Always show tooltips
 */
$(function() {
    $('[data-toggle="tooltip"]').tooltip({trigger: "hover"});
    $('[rel="tooltip"]').tooltip({trigger: "hover"});
});
