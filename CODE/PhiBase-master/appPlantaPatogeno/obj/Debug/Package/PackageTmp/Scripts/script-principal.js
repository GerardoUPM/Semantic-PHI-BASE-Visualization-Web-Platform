//(function ($, undefined) {
//    'use stirict';
//    jQuery.fn.exits = function () { return this.length > 0; }

    $('#dc').change(function () {
        if ($(this).is(":checked")) {
            $('#fullDiagram').show();
            $('#localDiagram').css("height", "350px");
            $('.resultado').css("height", "350px");
            $('.resultado').css("top", "-346px");
            $('#res').css("height", "255px");
            if ($('#2Nodos').is(":checked")) {
                actualizarLocalDiagram();
                //alert("actualizar 2 Nodos");
            } else {
                actualizarLocalDiagram();
                //alert("actualizar 3 Nodos");
            }
        } else {
            $('#fullDiagram').hide();
            $('#localDiagram').css("height", "700px");
            $('.resultado').css("height", "700px");
            $('.resultado').css("top", "-696px");
            $('#res').css("height", "540px");
            if ($('#2Nodos').is(":checked")) {
                actualizarLocalDiagram();
                //alert("actualizar 2 Nodos");
            } else {
                actualizarLocalDiagram();
                //alert("actualizar 3 Nodos");
            }
        }
    });
    $('#2Nodos').change(function () {
        if ($(this).is(":checked")) {
            $("#3Nodos").prop("checked", "");
            showLocalOnFullClick()
        }
    });
    $('#3Nodos').change(function () {
        if ($(this).is(":checked")) {
            $("#2Nodos").prop("checked", "");
            showLocalOnFullClick3Nodos();
        }
    });
    var nbOptions = 8; // number of menus
    var angleStart = -360; // start angle

    // jquery rotate animation


    // show / hide the options
    function toggleOptions(s) {
        $(s).toggleClass('open');
        var li = $(s).find('li');
        var deg = $(s).hasClass('half') ? 180 / (li.length - 1) : 360 / li.length;
        for (var i = 0; i < li.length; i++) {
            var d = $(s).hasClass('half') ? (i * deg) - 90 : i * deg;
            $(s).hasClass('open') ? rotate(li[i], d) : rotate(li[i], angleStart);
        }
    }

    setTimeout(function () { toggleOptions('.selector'); }, 100);
//});