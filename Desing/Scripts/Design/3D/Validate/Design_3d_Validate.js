
$("#IdWidthDefault").on("change", function () {
    var testValue = $("#IdWidthDefault").val();
    if (testValue < 0.05 || Value == null || value.trim() === "" || testValue > 1.0) {
        $("#IdWidthDefault").focus();
        document.getElementById("Id_Validate_WidthDefault").style.display = "inline";
        document.getElementById("IdWidthDefault").value = "0.3";
     }
    else {
        document.getElementById("Id_Validate_WidthDefault").style.display = "none";
     }
});


//IdHeightDefault
$("#IdHeightDefault").on("change", function () {
    var testValue = $("#IdHeightDefault").val();
    
    if (testValue < 0.5 || Value == null || Value.trim() === "" || testValue > 10) {
        $("#IdHeightDefault").focus();
        document.getElementById("Id_Validate_hightDefault").style.display = "inline";
        document.getElementById("IdHeightDefault").value = "2.7"; 

        
    } else {
        document.getElementById("Id_Validate_hightDefault").style.display = "none";
    }
});
$("#DatalongPilar").on("change", function () {
    var testLong = $("#DatalongPilar").val();
    if (testLong > 0.55) {
        $("#DatalongPilar").focus();
        document.getElementById("Id_Validate_Pilar_Long").style.display = "inline";
        document.getElementById("btnChangePilar").style.display = "none";
    }
    else {
        document.getElementById("Id_Validate_Pilar_Long").style.display = "none";
        document.getElementById("btnChangePilar").style.display = "inline";
    }
});

$("#DataWithPilar").on("change", function () {
    var testWith = $("#DataWithPilar").val();
    if (testWith > 0.55) {
        $("#DataWithPilar").focus();
        document.getElementById("Id_Validate_Pilar_With").style.display = "inline";
        document.getElementById("btnChangePilar").style.display = "none";

    }
    else {
        document.getElementById("Id_Validate_Pilar_With").style.display = "none";
        document.getElementById("btnChangePilar").style.display = "inline";
    }
});

// Validate Nucleo
$("#NucleoW").on("change", function () {
    var value = $("#NucleoW").val();
    if (value > 50) {
        $("#NucleoW").focus();
        document.getElementById("Id_Validate_NucleoW_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 3) {
            $("#NucleoW").focus();
            document.getElementById("Id_Validate_NucleoW_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoW_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }

});
$("#NucleoL").on("change", function () {
    var value = $("#NucleoL").val();
    if (value > 50) {
        $("#NucleoL").focus();
        document.getElementById("Id_Validate_NucleoL_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 3) {
            $("#NucleoL").focus();
            document.getElementById("Id_Validate_NucleoL_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoL_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }

});
$("#NucleoH").on("change", function () {
    var value = $("#NucleoH").val();
    if (value > 10) {
        $("#NucleoH").focus();
        document.getElementById("Id_Validate_NucleoH_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 0.2) {
            $("#NucleoH").focus();
            document.getElementById("Id_Validate_NucleoH_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoH_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }
});
$("#EWS").on("change", function () {
    var value = $("#EWS").val();
    if (value > 1) {
        $("#EWS").focus();
        document.getElementById("Id_Validate_NucleoEWS_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 0.1) {
            $("#EWS").focus();
            document.getElementById("Id_Validate_NucleoEWS_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoEWS_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }
});
$("#EWI").on("change", function () {
    var value = $("#EWI").val();
    if (value > 1) {
        $("#EWI").focus();
        document.getElementById("Id_Validate_NucleoEWI_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 0.1) {
            $("#EWI").focus();
            document.getElementById("Id_Validate_NucleoEWI_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoEWI_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }
});
$("#ELI").on("change", function () {
    var value = $("#ELI").val();
    if (value > 1) {
        $("#ELI").focus();
        document.getElementById("Id_Validate_NucleoELI_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 0.1) {
            $("#ELI").focus();
            document.getElementById("Id_Validate_NucleoELI_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoELI_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }
});
$("#ELD").on("change", function () {
    var value = $("#ELD").val();
    if (value > 1) {
        $("#ELD").focus();
        document.getElementById("Id_Validate_NucleoELD_Long").style.display = "inline";
        document.getElementById("btAddNucleo").style.display = "none";
    }
    else {
        if (value < 0.1) {
            $("#ELD").focus();
            document.getElementById("Id_Validate_NucleoELD_Long").style.display = "inline";
            document.getElementById("btAddNucleo").style.display = "none";
        }
        else {
            document.getElementById("Id_Validate_NucleoELD_Long").style.display = "none";
            document.getElementById("btAddNucleo").style.display = "inline";
        }
    }
});