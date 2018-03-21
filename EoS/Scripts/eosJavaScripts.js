function toggleDIvDisplay(e, SwedishCountryId) {
    //alert(e);
    //alert(SwedishCountryId);
    //alert('@(ViewBag.SwedishCountryIdBag)');
    if (e == SwedishCountryId) { //Changed from e === SwedishCountryId <-----???
        $('#divtwo').show();
    }
    else {
        $('#divtwo').hide();
        $('#RegionID').val(null);
    }
}


function toggleExtraDomainDisplay(e, DomainId) {
    //alert($('#ExtraProjectDomains').val());
    if (e == DomainId) {
        $('#ExtraDomainDiv').show();
    }
    else {
        $('#ExtraDomainDiv').hide();
        $('#ExtraProjectDomains').val(null);
    }
}


//This function to count how many letters has been wrriten in project summary of the IdeaCarrier project
function countChar(val) {
    var len = val.value.length;
    var remChars = 1500 - len;
    $('#charcounter').text(remChars);  // assign the length to a label

}

function showMe(e,i) {
    // i am spammy!
    alert(e.value+" "+i);
    //alert($('#FundingDivisionsList[' + i + '].Percentage').text());
    //$('#myVar').val(e.value);
    //return e.value
    //$('#FundingDivisionsList[' + i + '].Percentage').val(e.value);
    //return e.value
}

//This section for both IdeaCarrier and Investors for where they can navigate within the form content
$(document).ready(function () {
    $('#wizard').bootstrapWizard({ 'nextSelector': '.button-next', 'previousSelector': '.button-previous', 'firstSelector': '.button-first', 'lastSelector': '.button-last' });
});


//var form = $('accountForm');
//if (!form.valid()) {
//    $('#wizard').bootstrapWizard({ 'nextSelector': '.button-next', 'previousSelector': '.button-previous', 'firstSelector': '.button-first', 'lastSelector': '.button-last' });
//}
   

//$("#wizard").bootstrapWizard({
//    onNext: function (tab, navigation, index) {
//        if (index == 0) {
//            // Validate content of tab and 
//            // return false to prevent navigation
//            return true;
//        }
//    }
//});

//$(document).ready(function () {
//    $('#wizard').bootstrapWizard({
//        onNext: function (tab, navigation, index) {
//            if (index == 0) {
//                return true;
//                }
//            }
//    });
//});

//Go to Specific Tab on Page Reload or Hyperlink
var hash = window.location.hash;
hash && $('ul.nav a[href="' + hash + '"]').tab('show');


//This for Date Picker function
$(document).ready(function () {
    $('.date-picker').datetimepicker({
        format: 'YYYY-MM-DD',
        minDate: new Date()
    });
});


//This section related to Admin page and side bar 
function htmlbodyHeightUpdate() {
    var height3 = $(window).height();
    var height1 = $('.nav').height() + 50;
    height2 = $('.main').height();
    if (height2 > height3) {
        $('html').height(Math.max(height1, height3, height2) + 10);
        $('body').height(Math.max(height1, height3, height2) + 10);
    }
    else {
        $('html').height(Math.max(height1, height3, height2));
        $('body').height(Math.max(height1, height3, height2));
    }

}
$(document).ready(function () {
    htmlbodyHeightUpdate();
    $(window).resize(function () {
        htmlbodyHeightUpdate();
    });
    $(window).scroll(function () {
        height2 = $('.main').height();
        htmlbodyHeightUpdate();
    });
}); 

$(document).ready(function () {
    $(".dropdown-toggle").dropdown();
});

