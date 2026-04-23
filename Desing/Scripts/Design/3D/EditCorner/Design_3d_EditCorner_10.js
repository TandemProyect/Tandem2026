//Coner 10
$("#btnCornerChangeDimensionCorner_10").on("click", function () {
    EdiCorner_10();
    IsFormArtive = false;
});
$("#btnCornerDeleteDimensionCorner_10").on("click", function () {
    DeleteCorner_10();
    IsFormArtive = false;
});
function DeleteCorner_10() {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i] === OtherCornerObject) {
                var ob = scene.children[i];
                scene.remove(ob);
            }
            if (scene.children[i] === obWallMouseMove) {
                var ob = scene.children[i];
                scene.remove(ob);
            }
        }
    }
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i] === OtherCornerObject) {
                var ob = scene.children[i];
                scene.remove(ob);
            }
            if (scene.children[i] === obWallMouseMove) {
                var ob = scene.children[i];
                scene.remove(ob);
            }
        }
    }
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_10").hide("slide", { direction: "right" }, 400);
};
function OpenFormEsq_10() {
    /*    Universal en Y*/
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_10_90') {
        var temporaWall = obWallMouseMove;
        obWallMouseMove = OtherCornerObject;
        OtherCornerObject = temporaWall;
    }
    if (obWallMouseMove.Tape_90 === undefined) { obWallMouseMove.Tape_90 = "Agular10"; }
    if (OtherCornerObject.Tape_180 === undefined) { OtherCornerObject.Tape_180 = "Agular10"; }
    if (obWallMouseMove.Tape_90 !== "Universal_Y") { obWallMouseMove.Tape_90 = "Agular10"; }
    if (OtherCornerObject.Tape_180 !== "Universal_X") { OtherCornerObject.Tape_0 = "Agular10"; }
    if (OtherCornerObject.Tape_180 === "Universal_X") {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked = true;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerAgular10").checked = false;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_3.png";

    }
    if (obWallMouseMove.Tape_90 === "Universal_Y") {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked = true;
        document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerAgular10").checked = false;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_2.png";
    }
    if (obWallMouseMove.Tape_90 === "Agular10" && OtherCornerObject.Tape_180 === "Agular10") {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerAgular10").checked = true;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_1.png";

    }
    CloseFormEdit();
    IsFormArtive = true;
    ReturnControlsForCamera(camera, 2);
    $("#MenubottomDesign").hide("slide", 200);
    $("#EdiCorner_10").show("slide", { direction: "right" }, 150);
};

$("#BtnCloseCornerDimension_10").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_10").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
});
function ResetAngular10() {
    obWallMouseMove.Tape_180 = "Agular10";
    obWallMouseMove.Tape_90 = "";
    OtherCornerObject.Tape_90 = "Agular10";
    OtherCornerObject.Tape_180 = "";
    //90
    var oldValue = OtherCornerObject.scale.z;
    var value = parseInt(GetXsub(obWallMouseMove.scale.y) * 1000) / 1000;
    OtherCornerObject.scale.z = value;
    var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
    OtherCornerObject.position.z = OtherCornerObject.position.z + (MoveValue * 1000);
    var ob180 = getCHeckWall90(OtherCornerObject.IdWall_270);
    ob180.scale.z = ob180.scale.z + MoveValue;
    //00
    var oldValue = obWallMouseMove.scale.x;
    var value = parseInt(GetXsub(obWallMouseMove.scale.y) * 1000) / 1000;
    obWallMouseMove.scale.x = value;
    var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
    var ob_0 = getCHeckWall0(obWallMouseMove.IdWall_0);
    ob_0.scale.x = ob_0.scale.x + MoveValue;
    ob_0.position.x = ob_0.position.x - (MoveValue * 1000);
};
function EdiCorner_10() {
    $("#MenubottomDesign").hide("slide", 200);
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_10_90') {
        var temporaWall = obWallMouseMove;
        obWallMouseMove = OtherCornerObject;
        OtherCornerObject = temporaWall;
    }
    ResetAngular10();
    obWallMouseMove.Tape_180 = "";
    OtherCornerObject.Tape_180 = "";
    obWallMouseMove.Tape_90 = "";
    OtherCornerObject.Tape_90 = "";
    if (IsSolutionCornerXUniversalPanelCorner10.checked === true) {
        OtherCornerObject.Tape_180 = "Universal_X";
        OtherCornerObject.Tape_90 = "";
        obWallMouseMove.Tape_90 = "Agular10";
        obWallMouseMove.Tape_180 = "";
        var oldValue = OtherCornerObject.scale.z;
        var value = parseInt(getNewDimensionWall(OtherCornerObject.scale.x) * 1000) / 1000;
        OtherCornerObject.scale.z = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        OtherCornerObject.position.z = OtherCornerObject.position.z + (MoveValue * 1000);
        var ob270 = getCHeckWall90(OtherCornerObject.IdWall_270);
        ob270.scale.z = ob270.scale.z + MoveValue;
    }
    if (IsSolutionCornerYUniversalPanelCorner10.checked === true) {
        obWallMouseMove.Tape_90 = "Universal_Y";
        obWallMouseMove.Tape_180 = "";
        OtherCornerObject.Tape_90 = "Agular10";
        OtherCornerObject.Tape_180 = "";
        var oldValue = obWallMouseMove.scale.x;
        var value = parseInt(getNewDimensionWall(obWallMouseMove.scale.y) * 1000) / 1000;
        obWallMouseMove.scale.x = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        var ob_0 = getCHeckWall0(obWallMouseMove.IdWall_0);
        ob_0.scale.x = ob_0.scale.x + MoveValue;
        ob_0.position.x = ob_0.position.x - (MoveValue * 1000);
    }
    $("#EdiCorner_10").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
};
//Esq 10
$("#IsSolutionCornerXUniversalPanelCorner10").on("click", function () {
    if (document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked === true) {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerAgular10").checked = false;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_3.png";
    }
});
$("#IsSolutionCornerYUniversalPanelCorner10").on("click", function () {
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerAgular10").checked = false;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_2.png";
    }
});
$("#IsSolutionCornerAgular10").on("click", function () {
    if (document.getElementById("IsSolutionCornerAgular10").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner10").checked = false;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner10").checked = false;
        document.getElementById("TypeSolucion_Esq_10").src = "../../Content/DesignTools/MenuIcon/Esq_10_S_1.png";
    }
});