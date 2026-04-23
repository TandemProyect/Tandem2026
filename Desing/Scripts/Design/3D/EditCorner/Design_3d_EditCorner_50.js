//Edit Corner 50
$("#btnCornerChangeDimensionCorner_50").on("click", function () {
    EdiCorner_50();
    IsFormArtive = false;
});
$("#btnCornerDeleteDimensionCorner_50").on("click", function () {
    DeleteCorner_50();
    IsFormArtive = false;
});

function DeleteCorner_50() {
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
    $("#EdiCorner_50").hide("slide", { direction: "right" }, 400);
};
function EdiCorner_50() {
    obWallMouseMove.Tape_0 = "";
    OtherCornerObject.Tape_0 = "";
    obWallMouseMove.Tape_270 = "";
    OtherCornerObject.Tape_270 = "";
    if (IsSolutionCornerXUniversalPanelCorner.checked === true) {
        if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_90') {
            obWallMouseMove.Tape_0 = "Universal_X";
            OtherCornerObject.Tape_270 = "Other_Universal_X";
            obWallMouseMove.Tape_270 = "Other_Universal_X";
            OtherCornerObject.Tape_0 = "Other_Universal_X";
            ChangeNewDimension();
        }
        else {
            OtherCornerObject.Tape_0 = "Universal_X";
            obWallMouseMove.Tape_270 = "Other_Universal_X";
            obWallMouseMove.Tape_0 = "Other_Universal_X";
            OtherCornerObject.Tape_270 = "Other_Universal_X";
        }
    }
    else {
        if (IsSolutionCornerYUniversalPanelCorner.checked === true) {
            if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_00') {
                obWallMouseMove.Tape_270 = "Universal_Y";
                OtherCornerObject.Tape_0 = "Other_Universal_Y";
                OtherCornerObject.Tape_0 = "Other_Universal_X";
                ChangeNewDimension00(obWallMouseMove);
            }
            else {
                obWallMouseMove.Tape_270 = "Other_Universal_Y";
                obWallMouseMove.Tape_0 = "Other_Universal_Y";
                OtherCornerObject.Tape_270 = "Universal_Y";
                OtherCornerObject.Tape_0 = "Other_Universal_Y";
                ChangeNewDimension00(OtherCornerObject);
            }
        }
    };
    if (IsSolutionCornerYUniversalPanelCorner.checked === false && IsSolutionCornerXUniversalPanelCorner.checked === false) {
        obWallMouseMove.Tape_270 = "Angular";
        OtherCornerObject.Tape_0 = "Angular";
        obWallMouseMove.Tape_270 = "Angular";
        OtherCornerObject.Tape_270 = "Angular";
        if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_90') {
            ChangeNewDimensionAngular90(obWallMouseMove, OtherCornerObject);
        }
        if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_00') {
            ChangeNewDimensionAngular00(OtherCornerObject, obWallMouseMove);
        }

    }
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_50").hide("slide", { direction: "right" }, 400);
};
function ChangeEsq_50(Id, OldWith, NewWith) {
    var ObToChangeEsq_0 = GetObToChange(Id);
    ObToChangeEsq_0.scale.y = $("#DataWith").val() / 10;
    AnalizeEsq90(ObToChangeEsq_0.IdWall_90, OldWith, NewWith);
};
function ChangeEsq_50_90(Id, OldWith, NewWith) {
    var ObToChangeEsq_270 = GetObToChange(Id);
    ObToChangeEsq_270.scale.x = $("#DataWith").val() / 10;
    AnalizeEsq00(ObToChangeEsq_270.IdWall_180, OldWith, NewWith);
};
function AnalizeEsq00(Id, OldWith, NewWith) {
    var ObToChangeEsq00 = GetObToChange(Id);
    var xSub = GetXsub(obWall.scale.x);
    var ObToChangeWall00 = GetObToChange(ObToChangeEsq00.IdWall_180);
    var dif = (xSub - ObToChangeEsq00.scale.x) * 1000;
    ObToChangeEsq00.scale.x = xSub;
    ObToChangeEsq00.position.x = ObToChangeEsq00.position.x - dif;
    ObToChangeWall00.scale.x = ObToChangeWall00.scale.x - (dif / 1000);
    ObToChangeEsq00.Sub_Long_0 = NewWith;
};
function AnalizeEsq90(Id, OldWith, NewWith) {
    var ObToChangeEsq90 = GetObToChange(Id);
    var xSub = GetXsub(obWall.scale.y);
    var ObToChangeWall90 = GetObToChange(ObToChangeEsq90.IdWall_90);
    var dif = (xSub - ObToChangeEsq90.scale.z) * 1000;
    ObToChangeEsq90.scale.z = xSub;
    ObToChangeEsq90.position.z = ObToChangeEsq90.position.z - dif;
    //ObToChangeWall90.position.z = ObToChangeWall90.position.z - dif;
    ObToChangeWall90.scale.z = ObToChangeWall90.scale.z - (dif / 1000);
};
function getNewDimensionWallAngular(v) {
    value = (v + 0.03).toFixed(3);
    return value;
};
function getNewDimensionWall(v)
{
    value = (v + 0.03).toFixed(3);
    return value;
};
function ChangeNewDimension() {
    var value = getNewDimensionWall(obWallMouseMove.scale.x);
    var MoveValue = obWallMouseMove.scale.z - value;
    obWallMouseMove.scale.z = value;
    obWallMouseMove.position.z = obWallMouseMove.position.z + (MoveValue * 1000);
    var wallToChange = getCHeckWall90(obWallMouseMove.IdWall_90);
    wallToChange.scale.z = wallToChange.scale.z + MoveValue;
};
function ChangeNewDimension00(ob) {

    var value = parseInt(getNewDimensionWall(ob.scale.y) * 1000) / 1000;
    var valueScaleWall00 = parseInt(ob.scale.x * 1000) / 1000;
    var MoveValue = parseInt((valueScaleWall00 - value) * 1000) / 1000;
    ob.scale.x = value;
    ob.position.x = ob.position.x + (MoveValue * 1000);
    var wallToChange = getCHeckWall180(ob.IdWall_180);
    wallToChange.scale.x = wallToChange.scale.x + MoveValue;
};
function ChangeNewDimensionAngular90(_ob1, _ob2) {
    var value = GetXsub(obWallMouseMove.scale.x);
    var MoveValue = _ob1.scale.z - value;
    _ob1.scale.z = value;
    _ob1.position.z = _ob1.position.z + (MoveValue * 1000);
    var wallToChange = getCHeckWall90(_ob1.IdWall_90);
    wallToChange.scale.z = wallToChange.scale.z + MoveValue;
    //Coner 270
    var wallToEsq00 = getCHeckWall90(_ob2.IdWall_180);
    var MoveValue = ChangeWallNewValue(_ob2.scale.x, value);
    _ob2.scale.x = value;
    _ob2.position.x = _ob2.position.x - (MoveValue * 1000);
    wallToEsq00.scale.x = wallToEsq00.scale.x - MoveValue;
};
function ChangeNewDimensionAngular00(_ob1, _ob2) {
    var value = GetXsub(obWallMouseMove.scale.y);
    var MoveValue = _ob2.scale.x - value;
    _ob1.scale.z = value;
    _ob1.position.z = _ob1.position.z + (MoveValue * 1000);
    var wallToChange = getCHeckWall90(_ob1.IdWall_90);
    wallToChange.scale.z = wallToChange.scale.z + MoveValue;
    //Coner 270
    var wallToEsq00 = getCHeckWall90(_ob2.IdWall_180);
    var MoveValue = ChangeWallNewValue(_ob2.scale.x, value);
    _ob2.scale.x = value;
    _ob2.position.x = _ob2.position.x - (MoveValue * 1000);
    wallToEsq00.scale.x = wallToEsq00.scale.x - MoveValue;
};
function OpenFormEsq_50() {
    $("#MenubottomDesign").hide("slide", 200);

    if (obWallMouseMove.MeshTypeWall === 'Esq_50_90') {
        if (obWallMouseMove.scale.x > 0.056) {
            document.getElementById("CornerValidation50").style.visibility = 'hidden';
        }
        else {
            document.getElementById("CornerValidation50").style.visibility = 'visible';
        }
    }
    else {
        if (OtherCornerObject.scale.x > 0.05) {
            document.getElementById("CornerValidation50").style.visibility = 'hidden';
        }
        else {
            document.getElementById("CornerValidation50").style.visibility = 'visible';
        }
    }

    //Universal en Y
    document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = false;
    document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
    document.getElementById("IsSolutionCornerAgular50").checked = true;
    document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_1.png";
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_90') {
        if (obWallMouseMove.Tape_0 === 'Universal_X') {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_3.png";

        }
    }
    if (OtherCornerObject.idWall.substr(0, 9) === 'Esq_50_90') {
        if (OtherCornerObject.Tape_0 === "Universal_X") {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_3.png";
        }
        if (OtherCornerObject.Tape_0 === "Other_Universal_X") {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_2.png";
        }

    }
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_50_00') {
        if (obWallMouseMove.Tape_270 === "Universal_Y") {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_2.png";
        }
    }
    if (OtherCornerObject.idWall.substr(0, 9) === 'Esq_50_00') {
        if (OtherCornerObject.Tape_0 === "Universal_Y") {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_2.png";
        }
        if (OtherCornerObject.Tape_0 === "Other_Universal_Y") {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = true;
            document.getElementById("IsSolutionCornerAgular50").checked = false;
            document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_2.png";
        }
    }
    CloseFormEdit();
    IsFormArtive = true;
    ReturnControlsForCamera(camera, 2);
    $("#EdiCorner_50").show("slide", { direction: "right" }, 150);
};
$("#BtnCloseCornerDimension_50").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_50").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
});

//Esq 50
$("#IsSolutionCornerXUniversalPanelCorner").on("click", function () {
    if (document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked === true) {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = false;
        document.getElementById("IsSolutionCornerAgular50").checked = false;
        document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_3.png";
    }
});
$("#IsSolutionCornerYUniversalPanelCorner").on("click", function () {
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
        document.getElementById("IsSolutionCornerAgular50").checked = false;
        document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_2.png";
    }
});
$("#IsSolutionCornerAgular50").on("click", function () {
    if (document.getElementById("IsSolutionCornerAgular50").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked = false;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked = false;
        document.getElementById("TypeSolucion_Esq_50").src = "../../Content/DesignTools/MenuIcon/Esq_50_S_1.png";
    }
});

 