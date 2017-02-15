
//(function () {
//    window.addEventListener('load', function () {

//        var inputs = document.getElementsByClassName('text-input');


//        $(".text-input").parent().closest('div').click(function () {

//            if (!$(this).find("span").hasClass('spanDown')) {
//                $(".spanDown").animate({ "opacity": "0" }, "slow").removeClass('spanDown');
//                $(".divDwon").animate({ "margin-bottom": '30px' }, "slow").removeClass('divDwon');

//                if ($(this).find("span").hasClass('desc')) {
//                    $(this).closest('div').addClass("divDwon").animate({ "margin-bottom": '45px' }, "slow");
//                    $(this).find(".desc").addClass("spanDown").animate({ "top": 40, "opacity": "1" }, "slow");
//                }
//            }
//        });

//        $(inputs).focus(function () {
//            if (!$(this).hasClass('dirty')) {
//                $(this).addClass('dirty');
//            }
//        });
//        $(inputs).blur(function () {
//            if ($(this).val().length === 0) {
//                $(this).removeClass('dirty');
//            }
//        });


//        $('.modal').on('shown.bs.modal', function () {


//            $(".text-input").parent().closest('div').click(function () {

//                if (!$(this).find("span").hasClass('spanDown')) {
//                    $(".spanDown").animate({ "opacity": "0" }, "slow").removeClass('spanDown');
//                    $(".divDwon").animate({ "margin-bottom": '30px' }, "slow").removeClass('divDwon');

//                    if ($(this).find("span").hasClass('desc')) {
//                        $(this).closest('div').addClass("divDwon").animate({ "margin-bottom": '45px' }, "slow");
//                        $(this).find(".desc").addClass("spanDown").animate({ "top": 40, "opacity": "1" }, "slow");
//                    }
//                }
//            });


//            $(inputs).focus(function () {
//                if (!$(this).hasClass('dirty')) {
//                    $(this).addClass('dirty');
//                }
//            });
//            $(inputs).blur(function () {
//                if ($(this).val().length === 0) {
//                    $(this).removeClass('dirty');
//                }
//            });

//            for (var i = 0; i < inputs.length; i++) {

//                var input = inputs[i];

//                if (inputs[i].value != '') {
//                    addClass(inputs[i], 'dirty');
//                } else {
//                    removeClass(inputs[i], 'dirty');
//                }

//                input.addEventListener('keyup', function (e) {
//                    if (this.value != '') {
//                        addClass(this, 'dirty');

//                    } else {
//                        removeClass(this, 'dirty');
//                    }
//                });
//            }
//        })


//        for (var i = 0; i < inputs.length; i++) {
//            var input = inputs[i];

//            /*Form yüklendiðinde eðer value alaný setliyse placeholderlarý kaldýrýr*/
//            if (inputs[i].value != '') {
//                addClass(inputs[i], 'dirty');
//            } else {
//                removeClass(inputs[i], 'dirty');
//            }



//            $(inputs).focus(function () {
//                if (!$(this).hasClass('dirty')) {
//                    $(this).addClass('dirty');
//                }
//            });
//            $(inputs).blur(function () {
//                if ($(this).val().length === 0) {
//                    $(this).removeClass('dirty');
//                }
//            });
//            $(inputs).bind('paste input',function(){
//                if ($(this).val().length === 0) {
//                    $(this).removeClass('dirty');
//                }
//            });


//            input.addEventListener('keyup', function (e) {
//                if (this.value != '') {
//                    addClass(this, 'dirty');

//                } else {
//                    removeClass(this, 'dirty');
//                }
//            });
//        }
//    });
//})();

$(document).ready(function () {

    $(".text-input").parent().closest('div').click(function () {

                    if (!$(this).find("span").hasClass('spanDown')) {
                        $(".spanDown").animate({ "opacity": "0" }, "slow").removeClass('spanDown');
                        $(".divDwon").animate({ "margin-bottom": '30px' }, "slow").removeClass('divDwon');

                        if ($(this).find("span").hasClass('desc')) {
                            $(this).closest('div').addClass("divDwon").animate({ "margin-bottom": '45px' }, "slow");
                            $(this).find(".desc").addClass("spanDown").animate({ "top": 40, "opacity": "1" }, "slow");
                        }
                    }
                });


    // Function to update labels of text fields
    updateTextFields = function () {
        var input_selector = 'input[type=text], input[type=password], input[type=email], input[type=url], input[type=tel], input[type=number], input[type=search], textarea';
        $(input_selector).each(function (index, element) {
            if ($(element).val().length > 0 || $(this).attr('placeholder') !== undefined || $(element)[0].validity.badInput === true) {
                $(this).addClass('dirty');
            }
            else {
                $(this).removeClass('dirty');
            }
        });
    };

    // Text based inputs
    var input_selector = 'input[type=text], input[type=password], input[type=email], input[type=url], input[type=tel], input[type=number], input[type=search], textarea';

    // Handle HTML5 autofocus
    $('input[autofocus]').addClass('dirty');

    // Add dirty if form auto complete
    $(document).on('change', input_selector, function () {
        if ($(this).val().length !== 0 || $(this).attr('placeholder') !== undefined) {
            $(this).addClass('dirty');
        }
   //     validate_field($(this));
    });

    // Add dirty if input element has been pre-populated on document ready
    $(document).ready(function () {
        updateTextFields();
    });

    // HTML DOM FORM RESET handling
    $(document).on('reset', function (e) {
        var formReset = $(e.target);
        if (formReset.is('form')) {
            formReset.find(input_selector).removeClass('valid').removeClass('invalid');
            formReset.find(input_selector).each(function () {
                if ($(this).attr('value') === '') {
                    $(this).removeClass('dirty');
                }
            });

            // Reset select
            formReset.find('select.initialized').each(function () {

                var reset_text = formReset.find('option[selected]').text();
                formReset.siblings('input.select-dropdown').val(reset_text);
            });
        }
    });

    // Add dirty when element has focus
    $(document).on('focus', input_selector, function () {
        $(this).addClass('dirty');
    });

    $(document).on('blur', input_selector, function () {
        var $inputElement = $(this);
        if ($inputElement.val().length === 0 && $inputElement[0].validity.badInput !== true && $inputElement.attr('placeholder') === undefined) {
            $inputElement.removeClass('dirty');
        }
    //    validate_field($inputElement);
    });
});