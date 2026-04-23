//Coner 70
$("#btnCornerChangeDimensionCorner_70").on("click", function () {
    EdiCorner_70();
    IsFormArtive = false;
});
$("#btnCornerDeleteDimensionCorner_70").on("click", function () {
    DeleteCorner_70();
    IsFormArtive = false;
});
function DeleteCorner_70() {
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
    $("#EdiCorner_70").hide("slide", { direction: "right" }, 400);
};
function OpenFormEsq_70() {
    $("#MenubottomDesign").hide("slide", { direction: "right" }, 400);
    //Universal en Y
    document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = false;
    document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = false;
    document.getElementById("IsSolutionCornerAgular70").checked = true;
    document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_1.png";
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_70_90') {
        if (obWallMouseMove.Tape_270 === 'Universal_X') {
            document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = true;
            document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = false;
            document.getElementById("IsSolutionCornerAgular70").checked = false;
            document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_2.png";
         
        }
       
    }

    if (OtherCornerObject !== null) {
        if (OtherCornerObject.idWall.substr(0, 9) === 'Esq_70_90') {
            if (OtherCornerObject.Tape_270 === "Universal_X") {
                document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = true;
                document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = false;
                document.getElementById("IsSolutionCornerAgular70").checked = false;
                document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_2.png";
              
            }
        }
       
    }
    if (obWallMouseMove !== null) {
        if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_70_00') {
            if (obWallMouseMove.Tape_270 === "Universal_Y") {
                document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = false;
                document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = true;
                document.getElementById("IsSolutionCornerAgular70").checked = false;
                document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_3.png";
             
            }
        }
       
    }
    if (OtherCornerObject !== null) {
        if (OtherCornerObject.idWall.substr(0, 9) === 'Esq_70_00') {
            if (OtherCornerObject.Tape_180 === "Universal_Y") {
                document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = false;
                document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = true;
                document.getElementById("IsSolutionCornerAgular70").checked = false;
                document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_3.png";
          
            }
        }
    }
    CloseFormEdit();
    IsFormArtive = true;
    ReturnControlsForCamera(camera, 2);
    $("#EdiCorner_70").show("slide", { direction: "right" }, 150);
};
$("#BtnCloseCornerDimension_70").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    $("#EdiCorner_70").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
});
function EdiCorner_70() {
    ResetAngular70();
    obWallMouseMove.Tape_180 = "";
    OtherCornerObject.Tape_180 = "";
    obWallMouseMove.Tape_270 = "";
    OtherCornerObject.Tape_270 = "";
    if (IsSolutionCornerXUniversalPanelCorner70.checked === true) {
        OtherCornerObject.Tape_180 = "";
        OtherCornerObject.Tape_270 = "Universal_X";
        obWallMouseMove.Tape_270 = "Universal_X";
        obWallMouseMove.Tape_180 = "";
        var oldValue = obWallMouseMove.scale.x;
        var value = parseInt(getNewDimensionWall(obWallMouseMove.scale.y) * 1000) / 1000;
        obWallMouseMove.scale.x = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        var ob_0 = getCHeckWall0(obWallMouseMove.IdWall_0);
        ob_0.scale.x = ob_0.scale.x + MoveValue;
        ob_0.position.x = ob_0.position.x - (MoveValue * 1000);
        
    }
    if (IsSolutionCornerYUniversalPanelCorner70.checked === true) {
        obWallMouseMove.Tape_270 = "";
        obWallMouseMove.Tape_180 = "Universal_Y";
        OtherCornerObject.Tape_180 = "Universal_Y";
        OtherCornerObject.Tape_270 = "";
        var oldValue = OtherCornerObject.scale.z;
        var value = parseInt(getNewDimensionWall(OtherCornerObject.scale.x) * 1000) / 1000;
        OtherCornerObject.scale.z = value;
        var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
        OtherCornerObject.position.z = OtherCornerObject.position.z + (MoveValue * 1000);
        var ob90 = getCHeckWall90(OtherCornerObject.IdWall_90);
        ob90.scale.z = ob90.scale.z + MoveValue;
    }
    if (IsSolutionCornerAgular70.checked === true) {
     
    }
    $("#EdiCorner_70").hide("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    CloseFormEdit();
};
function ResetAngular70()
{
    if (obWallMouseMove.idWall.substr(0, 9) === 'Esq_70_90') {
        var temporaWall = obWallMouseMove;
        obWallMouseMove = OtherCornerObject;
        OtherCornerObject = temporaWall;
    }
    obWallMouseMove.Tape_270 = "Agular70";
    obWallMouseMove.Tape_180 = "";
    OtherCornerObject.Tape_270 = "";
    OtherCornerObject.Tape_180 = "Agular70";
    //90
    var oldValue = OtherCornerObject.scale.z;
    var value = parseInt(GetXsub(obWallMouseMove.scale.y) * 1000) / 1000;
    OtherCornerObject.scale.z = value;
    var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
    OtherCornerObject.position.z = OtherCornerObject.position.z + (MoveValue * 1000);
    var ob90 = getCHeckWall90(OtherCornerObject.IdWall_90);
    ob90.scale.z = ob90.scale.z + MoveValue;
    //00
    var oldValue = obWallMouseMove.scale.x;
    var value = parseInt(GetXsub(obWallMouseMove.scale.y) * 1000) / 1000;
    obWallMouseMove.scale.x = value;
    var MoveValue = ((oldValue * 1000) - (value * 1000)) / 1000;
    //obWallMouseMove.position.x = obWallMouseMove.position.x + (MoveValue * 1000);
    var ob_0 = getCHeckWall0(obWallMouseMove.IdWall_0);
    ob_0.scale.x = ob_0.scale.x + MoveValue;
    ob_0.position.x = ob_0.position.x - (MoveValue * 1000);
};



//Esq 70

$("#IsSolutionCornerXUniversalPanelCorner70").on("click", function () {
    if (document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked === true) {
        document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = false;
        document.getElementById("IsSolutionCornerAgular70").checked = false;
        document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_2.png";
    }
});
$("#IsSolutionCornerYUniversalPanelCorner70").on("click", function () {
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = false;
        document.getElementById("IsSolutionCornerAgular70").checked = false;
        document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_3.png";
    }
});
$("#IsSolutionCornerAgular70").on("click", function () {
    if (document.getElementById("IsSolutionCornerAgular70").checked === true) {
        document.getElementById("IsSolutionCornerXUniversalPanelCorner70").checked = false;
        document.getElementById("IsSolutionCornerYUniversalPanelCorner70").checked = false;
        document.getElementById("TypeSolucion_Esq_70").src = "../../Content/DesignTools/MenuIcon/Esq_70_S_1.png";
    }
});