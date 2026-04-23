//Coner 30

$("#btnCornerChangeDimensionCorner_30").on("click", function () {
    EdiCorner_30();
    IsFormArtive = false;
});
$("#btnCornerDeleteDimensionCorner_30").on("click", function () {
    DeleteCorner_30();
    IsFormArtive = false;
});
function DeleteCorner_30() {
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
    $("#EdiCorner_30").hide("slide", { direction: "right" }, 400);
};
$("#BtnCloseCornerDimension_30").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_30").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
});
function OpenFormEsq_30() {
  
    //Universal en Y
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_30_90') {
        var temporaWall = obWallMouseMove;
        obWallMouseMove = OtherCornerObject;
        OtherCornerObject = temporaWall;
    }
    if (obWallMouseMove.Tape_90 !== "Universal_Y") { obWallMouseMove.Tape_90 = "Agular30"; }
    if (OtherCornerObject.Tape_0 !== "Universal_X") { OtherCornerObject.Tape_0 = "Agular30"; }
    if (OtherCornerObject.Tape_0 === "Universal_X") {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked = true;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerAgular30").checked = false;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_3.png";

    }
    if (obWallMouseMove.Tape_90 === "Universal_Y") {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked = true;
        document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerAgular30").checked = false;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_2.png";
    }
    if (obWallMouseMove.Tape_90 === "Agular30" && OtherCornerObject.Tape_0 === "Agular30") {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerAgular30").checked = true;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_1.png";

    }

    CloseFormEdit();
    IsFormArtive = true;
    ReturnControlsForCamera(camera, 2);
    $("#MenubottomDesign").hide("slide", 200);
    $("#EdiCorner_30").show("slide", { direction: "right" }, 150);
};
function EdiCorner_30() {

    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_30_90') {
        var temporaWall = obWallMouseMove;
        obWallMouseMove = OtherCornerObject;
        OtherCornerObject = temporaWall;
    }
    ResetAngular30();
    obWallMouseMove.Tape_0 = "";
    OtherCornerObject.Tape_0 = "";
    obWallMouseMove.Tape_90 = "";
    OtherCornerObject.Tape_90 = "";
    if (IsSolutionCornerXUniversalPanelCorner30.checked === true) {
        OtherCornerObject.Tape_0 = "Universal_X";
        OtherCornerObject.Tape_90 = "";
        obWallMouseMove.Tape_90 = "Agular30";
        obWallMouseMove.Tape_0 = "";
        var oldValue = OtherCornerObject.scale.z;
        var value = parseInt(getNewDimensionWall(OtherCornerObject.scale.x) * 1000) / 1000;
        OtherCornerObject.scale.z = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        OtherCornerObject.position.z = OtherCornerObject.position.z + (MoveValue * 1000);
        var ob270 = getCHeckWall90(OtherCornerObject.IdWall_270);
        ob270.scale.z = ob270.scale.z + MoveValue;
    }
    if (IsSolutionCornerYUniversalPanelCorner30.checked === true) {
        obWallMouseMove.Tape_90 = "Universal_Y";
        obWallMouseMove.Tape_0 = "";
        OtherCornerObject.Tape_90 = "Agular30";
        OtherCornerObject.Tape_0 = "";
        var oldValue = obWallMouseMove.scale.x;
        var value = parseInt(getNewDimensionWall(obWallMouseMove.scale.y) * 1000) / 1000;
        obWallMouseMove.scale.x = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        var ob_180 = getCHeckWall0(obWallMouseMove.IdWall_180);
        ob_180.scale.x = ob_180.scale.x + MoveValue;
        ob_180.position.x = ob_180.position.x - (MoveValue * 1000);
    }

    $("#EdiCorner_30").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
};
function ResetAngular30() {
    obWallMouseMove.Tape_0 = "Agular30";
    obWallMouseMove.Tape_90 = "";
    OtherCornerObject.Tape_90 = "Agular30";
    OtherCornerObject.Tape_0 = "";
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
    //obWallMouseMove.position.x = obWallMouseMove.position.x + (MoveValue * 1000);
    var ob_180 = getCHeckWall0(obWallMouseMove.IdWall_180);
    ob_180.scale.x = ob_180.scale.x + MoveValue;
    ob_180.position.x = ob_180.position.x - (MoveValue * 1000);
};


//Esq 30

$("#IsSolutionCornerXUniversalPanelCorner30").on("click", function () {
    if (document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked === true) {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerAgular30").checked = false;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_3.png";
    }
});
$("#IsSolutionCornerYUniversalPanelCorner30").on("click", function () {
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerAgular30").checked = false;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_2.png";
    }
});
$("#IsSolutionCornerAgular30").on("click", function () {
    if (document.getElementById("IsSolutionCornerAgular30").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner30").checked = false;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner30").checked = false;
        document.getElementById("TypeSolucion_Esq_30").src = "../../Content/DesignTools/MenuIcon/Esq_30_S_1.png";
    }
});