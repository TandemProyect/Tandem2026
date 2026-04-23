//Solapes
$("#Id_Clouse_DivWork").on("click", function () {
    $("#DivWork").hide("slide", { direction: "right" }, 4);
});





$("#Id_Clouse_DivExport").on("click", function () {
    $("#DivExport").hide("slide", { direction: "right" }, 4);
});

$("#Id_Clouse_Toas").on("click", function () {
    $("#Toas").hide("slide", { direction: "right" }, 4);
});

$("#ChekSolapeEnd").on('click', function (e) {
    if (document.getElementById("ChekSolapeEnd").checked === true) {
        resetDivTapeFinal();
        $("#DivTape_End_01").show("slide", { direction: "right" }, 4);
        document.getElementById("TapeS1").checked = true;
        CalculationEndSolape();
    }
    else {
        document.getElementById("DataSupEnd").value = 0;
        resetDivTapeFinal();
        ChangeSolape0Final();
    }
});



$("#DataHeightCorner").on("change", function () {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name.substr(0, 4) === "Wall") {
                switch (scene.children[i].name.substr(0, 10)) {
                    case "Wall_R000":
                        scene.children[i].scale.z = $("#DataHeightCorner").val() / 10;
                        break;
                    case "Wall_R900":
                        scene.children[i].scale.y = $("#DataHeightCorner").val() / 10;
                        scene.children[i].position.y = $("#DataHeightCorner").val() * 100;
                        break;
                    case "WallEsqTLe":
                        scene.children[i].scale.y = $("#DataHeightCorner").val() / 10;
                        scene.children[i].position.y = $("#DataHeightCorner").val() * 100;
                        break;
                    case "WallEPanel":
                        scene.children[i].scale.y = $("#DataHeightCorner").val() / 10;
                        scene.children[i].position.y = $("#DataHeightCorner").val() * 100;
                        break;
                }
            }
        }
    }
});
$("#DataWXConer").on("change", function () {

    EraseDimensionWall();
    EraseDimensionWall();
    var OldValue = obWall.scale.z * 10;
    var MoveY = $("#DataWXConer").val() - OldValue;
    obWall.scale.z = $("#DataWXConer").val() / 10;
    obWall.XWith = obEsqY.scale.x;
    obWall.position.z = obWall.position.z - (MoveY * 100);
    obWall.XWith = obEsqY.scale.x;
    var scaley = obWall.scale.z + 0.03;
    obWallX.scale.y = $("#DataWXConer").val() / 10;
    obEsqY.scale.z = scaley;
    obEsqY.position.z = obEsqY.position.z - (MoveY * 100);
    obWallY.scale.z = obWallY.scale.z - (MoveY / 10);
    //    obWallY.position.z = obWallY.position.z + (MoveY * 100);
});
$("#DataWYConer").on("change", function () {
    var OldValue = obWallY.scale.x * 10;
    var Move = $("#DataWYConer").val() - OldValue;
    obEsqY.scale.x = $("#DataWYConer").val() / 10;
    obWallY.scale.x = $("#DataWYConer").val() / 10;
    obEsqY.position.x = obEsqY.position.x + (Move * 100);
    obWallY.position.x = obWallY.position.x + (Move * 100);
    obEsqX.scale.x = obEsqX.scale.x + (Move / 10);
    obEsqX.position.x = obEsqX.position.x + (Move * 100);
    obWallX.scale.x = obWallX.scale.x + (Move / 10);
    obWallX.position.x = obWallX.position.x + (Move * 100);
    obEsqX.YWith = obEsqY.scale.x;
});
//DataWith


function GetObToChange(Id) {
    if (Id === undefined) {
        return;
    }
    var id = Id.replace(/\s+/g, '');

    var list = HelpSelectMeshId();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name === id) {
                return scene.children[i];
            }
        }
    }
};

function CalculationEndSolape() {
    var l = parseFloat($("#Datalong").val());
    var l2 = Math.trunc((l / 0.15));
    var l3 = l2 * 0.15;
    var n = parseFloat((l - l3).toFixed(2));
    var n2 = parseFloat((0.15 - n).toFixed(2));
    document.getElementById("DataSupEnd").value = n2 + 0.15;
}
function ChangeSolape0Final() {
    resetDivTapeFinal();
    var w = $("#DataWith").val() * 1;
    var Iniciall_Wall = $("#DataSupInicial").val() * 1;
    var End_Wall = $("#DataSupEnd").val() * 1;
    ///Tape Inicial
    if (Iniciall_Wall === 0) {
        if (w >= 0.3) {
            if (w === 0.3 || w === 0.45 || w === 0.60 || w === 0.75 || w === 0.90) {
                resetDivTape();
                $("#DivTape_End_03").show("slide", { direction: "right" }, 4);
                document.getElementById("TapeS4").checked = true;

            }
            else {
                if (w === 0.35 || w === 0.40 || w === 0.55 || w === 0.65 || w === 0.70 || w === 0.80 || w === 0.85) {
                    resetDivTape();
                    $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
                    document.getElementById("TapeS6").checked = true;
                }
                else {
                    if (Number.isInteger(w / 0.05)) {
                        $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
                        document.getElementById("TapeS6").checked = true;
                    }
                    else {
                        $("#DivTape_End_05").show("slide", { direction: "right" }, 4);
                        document.getElementById("TapeS8").checked = true;
                    }
                }
            }
        }
        else {
            $("#DivTape_End_02").show("slide", { direction: "right" }, 4);
            document.getElementById("TapeS3").checked = true;
        }
    }
};

function resetDivTape() {
    //$("#DivInicialSinSolapeMenor030").hide("slide", { direction: "right" }, 4);
    //$("#DivInicialConSolapeMenor030S1").hide("slide", { direction: "right" }, 4);
    //$("#DivInicialSolapeMayor030Exacto").hide("slide", { direction: "right" }, 4);
    //$("#DivInicialSolapeMayor030NoExactoNoMultiplo").hide("slide", { direction: "right" }, 4);
    //$("#DivInicialSolapeMayor030Multiplo005").hide("slide", { direction: "right" }, 4);
    //$("#DivInicialSolapeMayor030Multiplo").hide("slide", { direction: "right" }, 4);
};
function resetDivTapeFinal() {
    $("#DivTape_End_01").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_02").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_03").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_04").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_05").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_06").hide("slide", { direction: "right" }, 4);
    $("#DivTape_End_07").hide("slide", { direction: "right" }, 4);
    OnclickResetFinalMenuTape();
};
function GetLongY(l) {
    var _vScaleEsqy = 0.3;
    if (l > 0.03) { _vScaleEsqy = 0.45; }
    if (l > 0.045) { _vScaleEsqy = 0.60; }
    if (l > 0.06) { _vScaleEsqy = 0.75; }
    if (l > 0.075) { _vScaleEsqy = 0.90; }
    if (l > 0.090) { _vScaleEsqy = 1.05; }
    if (l > 0.105) { _vScaleEsqy = 1.20; }
    if (l > 0.12) { _vScaleEsqy = 1.35; }
    if (l > 0.135) { _vScaleEsqy = 1.5; }
    if (l > 0.15) { _vScaleEsqy = 1.65; }
    if (l > 0.165) { _vScaleEsqy = 1.8; }
    return _vScaleEsqy;
}
//Muro 0 0
$("#Datalong").on("change", function () {
    if (obWall.MeshTypeWall === "Wall_R000") {
        if (ActionDbl === "CtrMXStr") {
            var OldValue = obWall.scale.x * 10;
            var MoveX = $("#Datalong").val() - OldValue;
            obWall.scale.x = $("#Datalong").val() / 10;
            obWall.position.x = obWall.position.x - (MoveX * 100);
        }
        if (ActionDbl === "CtrMXEnd") {
            obWall.scale.x = $("#Datalong").val() / 10;
        }
        if (ActionDbl === "") {
            obWall.scale.x = $("#Datalong").val() / 10;
        }
        obWall.scale.x = $("#Datalong").val() / 10;
        var _longWall = document.getElementById("Datalong").value;
        var _milllongWall = parseInt(_longWall * 1000);
        var sub = parseInt((_longWall * 1000) / 300) + 1;
        //var subInicioAndFin = (sub * 300);
        //subInicioAndFin = subInicioAndFin - _milllongWall;
        //if (subInicioAndFin < 150) {
        //    subInicioAndFin = 200
        //}
        //document.getElementById("DataSupInicial").value = subInicioAndFin / 1000;
        //document.getElementById("DataSupEnd").value = subInicioAndFin / 1000;
        //    InsertDimWallH(obWall.scale.x * 1000, "Wall", obWall.position.x, obWall.position.y, obWall.position.z);
    }
    if (obWall.MeshTypeWall === "Wall_R900") {
        var v = parseInt($("#Datalong").val() * 1000);
        let value = v / 10000;
        var distMoveWall = parseFloat((obWall.scale.z - value).toFixed(3)) * 1000;
        if (distMoveWall < -0.0001) { distMoveWall = distMoveWall * -1 };
        var LengthGreaterThanValue = true;
        if (obWall.scale.z < value) {
            LengthGreaterThanValue = false;
        }
        obWall.scale.z = value;
        var NewPositionValue = 0;
        if (obWall.IdWall_270 !== '0')
        {
            if (LengthGreaterThanValue == false) {
                NewPositionValue = obWall.position.z - distMoveWall;
            }
            else {
                NewPositionValue = obWall.position.z + distMoveWall;
            }
        }
        if (obWall.IdWall_90 !== '0')
        {
            if (LengthGreaterThanValue == false) {
                NewPositionValue = obWall.position.z - 0;
            }
            else {
                NewPositionValue = obWall.position.z + 0;
            }
        }
        if (obWall.IdWall_90 !== '0' && obWall.IdWall_270 !== '0') 
            {
                if (LengthGreaterThanValue == false) {
                    NewPositionValue = obWall.position.z + distMoveWall;
                }
                else {
                    NewPositionValue = obWall.position.z - distMoveWall;
                }
            }
            obWall.position.z = NewPositionValue;
        }
    });
$("#Datalong").on("mouseenter", function () {
    document.getElementById("Datalong").focus();
});
$("#DataWith").on("change", function () {
    ChangeDataWith();
});
function ChangeDataWith() {
    var para = 1;
    var value = $("#DataWith").val();
    if (value <= 0.099) {
        document.getElementById("ValidationWith").style.display = "inline";
        document.getElementById("DataWith").focus();
        document.getElementById("btnDeleteDimension").style.display = "none";
        document.getElementById("btnChangeDimension").style.display = "none";
        return;
    }
    else {
        document.getElementById("ValidationWith").style.display = "none";
        document.getElementById("btnDeleteDimension").style.display = "inline";
        document.getElementById("btnChangeDimension").style.display = "inline";
    }
    if (obWall.MeshTypeWall.substr(0, 9) === "Wall_R000") {
        var OldWith = obWall.scale.y;
        var NewWith = $("#DataWith").val() / 10;
        obWall.scale.y = $("#DataWith").val() / 10;
        var TypeIDWall0 = obWall.IdWall_0.substr(0, 6);
        if (TypeIDWall0 === "Esq_50") {
            ChangeEsq_50(obWall.IdWall_0, OldWith, NewWith);
        }
        ChangeSolape0Final();
    }
    if (obWall.MeshTypeWall.substr(0, 9) === "Wall_R900") {
        var OldWith = obWall.scale.x;
        var NewWith = $("#DataWith").val() / 10;
        obWall.scale.x = $("#DataWith").val() / 10;
        var TypeIDWall0 = obWall.IdWall_270.substr(0, 6);
        if (TypeIDWall0 === "Esq_50") {
            ChangeEsq_50_90(obWall.IdWall_270, OldWith, NewWith);
        }
        ChangeSolape0Final();
    }
};


$("#DataHeight").on("change", function () {
    changeDataHeight();
});
function changeDataHeight() {
    
    var value = $("#DataHeight").val(); 
    if (value < 0.5 || value == null || value.trim() === "" || value > 10) {
        document.getElementById("ValidationHeight").style.display = "inline";
        document.getElementById("DataHeight").focus();
        document.getElementById("DataHeight").value = "2.7"; // Limpiar el input
        
        return;
    } else {
        document.getElementById("ValidationHeight").style.display = "none";
    }
}

$("#DataWith").on("mouseenter", function () {
    document.getElementById("DataWith").focus();
});

$("#DataHeight").on("change", function () {
    _cHeckHeightAllWall = document.getElementById("CHeckHeightAllWall").checked;

    if ($("#DataHeight").val() <= 1.5) {
        document.getElementById("PuntaInfExt").style.display = "none";
        document.getElementById("PuntaInfInt").style.display = "none";
    }
    else {
        document.getElementById("PuntaInfExt").style.display = "inline";
        document.getElementById("PuntaInfInt").style.display = "inline";
    }

    if (_cHeckHeightAllWall === true) {
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].type === "Mesh") {
                if (scene.children[i].name === "") {
                    continue;
                }
                if (scene.children[i].MeshTypeWall === undefined) {
                    continue;
                }
                switch (scene.children[i].MeshTypeWall.substr(0, 9)) {
                    case 'Wall_R000':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Wall_R900':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_10_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_10_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_20_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_20_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_30_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_30_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_40_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_40_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_50_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_50_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_60_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_60_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_70_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_70_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_80_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_80_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;
                    case 'Esq_X_00':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                    case 'Esq_X_90':
                        scene.children[i].scale.y = $("#DataHeight").val() / 10;
                        scene.children[i].position.y = $("#DataHeight").val() * 100;
                        break;

                    case 'Pilar':
                        scene.children[i].scale.z = $("#DataHeight").val() / 10;
                        break;
                }
            }
        }
    }
    else {
        switch (obWall.MeshTypeWall) {
            case 'Wall_R000':
                obWall.scale.z = $("#DataHeight").val() / 10;
                break;
            case 'Wall_R900':
                obWall.scale.y = $("#DataHeight").val() / 10;
                obWall.position.y = $("#DataHeight").val() * 100;
                break;
            case 'Esq_10_00':
                scene.children[i].scale.z = $("#DataHeight").val() / 10;
                break;
            case 'Esq_10_90':
                obWall.scale.y = $("#DataHeight").val() / 10;
                obWall.position.y = $("#DataHeight").val() * 100;
                break;
            case 'Esq_30_00':
                obWall.scale.z = $("#DataHeight").val() / 10;
                break;
            case 'Esq_30_90':
                obWall.scale.y = $("#DataHeight").val() / 10;
                obWall.position.y = $("#DataHeight").val() * 100;
                break;
            case 'Esq_50_00':
                scene.children[i].scale.z = $("#DataHeight").val() / 10;
                break;
            case 'Esq_50_90':
                obWall.scale.y = $("#DataHeight").val() / 10;
                obWall.position.y = $("#DataHeight").val() * 100;
                break;
            case 'Esq_70_00':
                obWall.scale.z = $("#DataHeight").val() / 10;
                break;
            case 'Esq_70_90':
                obWall.scale.y = $("#DataHeight").val() / 10;
                obWall.position.y = $("#DataHeight").val() * 100;
                break;
            case 'Pilar':
                obWall.scale.z = $("#DataHeight").val() / 10;
                break;
        }
    }
});
$("#DataHeight").on("mouseenter", function () {
    document.getElementById("DataHeight").focus();
});
//Pilar
$("#DataWithPilar").on("change", function () {
    obWall.scale.y = $("#DataWithPilar").val() / 10;
});
$("#DataWithPilar").on("mouseenter", function () {
    document.getElementById("DataWithPilar").focus();
});
$("#DataCordenadXPilar").on("change", function () {
    obWall.position.x = $("#DataCordenadXPilar").val() / 10;
});
$("#DataCordenadXPilar").on("mouseenter", function () {
    document.getElementById("DataCordenadXPilar").focus();
});
$("#DataCordenadYPilar").on("change", function () {
    obWall.position.z = $("#DataCordenadYPilar").val() / 10;
});
$("#DataCordenadYPilar").on("mouseenter", function () {
    document.getElementById("DataCordenadYPilar").focus();
});
$("#DatalongPilar").on("change", function () {
    obWall.scale.x = $("#DatalongPilar").val() / 10;
});
$("#DatalongPilar").on("mouseenter", function () {
    document.getElementById("DatalongPilar").focus();
});
$("#DataHeightPilar").on("change", function () {
    obWall.scale.z = $("#DataHeightPilar").val() / 10;
});
$("#DataHeightPilar").on("mouseenter", function () {
    document.getElementById("DataHeightPilar").focus();
});





$("#btnChangePilar").on("click", function () {
    var testLong = $("#DatalongPilar").val();
    var testWith = $("#DataWithPilar").val();

    if (testLong > 0.56) {
        $("#DatalongPilar").focus();
        document.getElementById("Id_Validate_Pilar_Long").style.display = "inline";
        return;
    }
    if (testWith > 0.56) {
        $("#DataWithPilar").focus();
        return;
    }

    obWall.scale.y = $("#DataWithPilar").val() / 10;
    obWall.scale.x = $("#DatalongPilar").val() / 10;
    obWall.scale.z = $("#DataHeightPilar").val() / 10;
    obWall.position.x = $("#DataCordenadXPilar").val() / 10;
    obWall.position.z = $("#DataCordenadYPilar").val() / 10;
    obWall.CHeckDimWall = false;
    obWall.CHeckPropInside = false;
    if (document.getElementById("CHeckPropInsidepilar").checked === true) {
        obWall.CHeckPropInside = true;
        obWall.CHeckPropInsideInf = false;
        if (document.getElementById("CHeckPropInsideInfPilar").checked === true) {
            obWall.CHeckPropInsideInf = true;
        }
        else {
            obWall.CHeckPropInsideInf = false;
        }
    }
    else {
        document.getElementById("CHeckPropInsideInfPilar").checked = false;
        obWall.CHeckPropInside = false;
        obWall.CHeckPropInsideInf = false;
    }
    obWall.CHeck750R = false;
    if (document.getElementById("CHeck750RPilar").checked === true) { obWall.CHeck750R = true; }


    obWall.material = new THREE.MeshLambertMaterial({ color: 0xA3A196 });
    $("#EditPilar").hide("slide", { direction: "right" }, 400);
    Edit_Wall = 1;
    obWall.Grupo = document.getElementById("ÌdGrupPilar").value;
    obWall = null;
    ActionDbl = null;
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
});
$("#btnChangeDimensionPilar").on("mouseenter", function () {
    document.getElementById("btnChangeDimensionPilar").focus();
});
$("#ButtonClouseFormWall").on('click', function (e) {
    var materialReturm = new THREE.MeshLambertMaterial({ color: 0xFFFFFF });
    ob.material = materialReturm;
    ob.geometry.dispose();
    ob.material.dispose();
    $("#EditWall").hide("slow", function () {
        // Animation complete.
    });

});
$("#btnChangeDimension").on("click", function ()

{
    ///realizar OnChange
    if (obWall.MeshTypeWall === "Wall_R000") {
        obWall.position.z = $("#DataCordenadY").val() / 10;
        obWall.position.x = $("#DataCordenadX").val() / 10;
    }

    if (obWall.MeshTypeWall === "Wall_R900") {
        obWall.position.z = $("#DataCordenadY").val() / 10;
        obWall.position.x = $("#DataCordenadX").val() / 10;
    }
 
    if (obWall.CHeckDimWall !== null) {
        obWall.CHeckDimWall = false;
    }
    
    obWall.CHeck750R = false;
    obWall.IdTypeFormworkMode = false;
    if (document.getElementById("CHeck750R").checked === true) { obWall.CHeck750R = true; }
    if (document.getElementById("CHeckTypeFormworkMode").checked === true) { obWall.IdTypeFormworkMode = true; }

    if (document.getElementById("CHeckDimWall").checked === true) { obWall.CHeckDimWall = true; }
    obWall.CHeckBracketInside = false;
    if (document.getElementById("CHeckBracketInside").checked === true) { obWall.CHeckBracketInside = true; }
    obWall.CHeckBracketOutside = false;
    if (document.getElementById("CHeckBracketOutside").checked === true) { obWall.CHeckBracketOutside = true; }
    obWall.CHeckRijiInside = false;
    if (document.getElementById("CHeckRijiInside").checked === true) { obWall.CHeckRijiInside = true; }
    obWall.CHeckRijiOutside = false;
    if (document.getElementById("CHeckRijiOutside").checked === true) { obWall.CHeckRijiOutside = true; }
    obWall.CHeckPropInside = false;
    if (document.getElementById("CHeckPropInside").checked === true) { obWall.CHeckPropInside = true; }
    obWall.CHeckPropOutside = false;
    if (document.getElementById("CHeckPropOutside").checked === true) { obWall.CHeckPropOutside = true; }
    obWall.CHeckPropInsideInf = false;
    if (document.getElementById("CHeckPropInsideInf").checked === true) { obWall.CHeckPropInsideInf = true; }
    obWall.CHeckPropOutsideInf = false;
    if (document.getElementById("CHeckPropOutsideInf").checked === true) { obWall.CHeckPropOutsideInf = true; }

    var _dataSupInicial = parseInt($("#DataSupInicial").val() * 1000);
    var _dataSupEnd = parseInt($("#DataSupEnd").val() * 1000);
    obWall.material = new THREE.MeshLambertMaterial({ color: 0xA3A196 });
    $("#EditDim").hide("slide", { direction: "right" }, 400);
    ReturnControlsForCamera(camera, 1);
    $("#EditTape").hide("slide", { direction: "right" }, 400);
    $("#compassContainer").show("slide", { direction: "right" }, 400);
    Edit_Wall = 1;
    obWall.Iniciall_Wall = _dataSupInicial;
    obWall.End_Wall = _dataSupEnd;

    obWall.userData.name = "SupInicial_" + $("#DataSupInicial").val() + "SupEnd_" + $("#DataSupEnd").val();
    if (document.getElementById("TapeS1").checked === true) {
        obWall.Tape_0 = "TapeS1";
    }
    if (document.getElementById("TapeS2").checked === true) {
        obWall.Tape_0 = "TapeS2";
    }
    if (document.getElementById("TapeS3").checked === true) {
        obWall.Tape_0 = "TapeS3";
    }
    if (document.getElementById("TapeS4").checked === true) {
        obWall.Tape_0 = "TapeS4";
    }
    if (document.getElementById("TapeS5").checked === true) {
        obWall.Tape_0 = "TapeS5";
    }
    if (document.getElementById("TapeS6").checked === true) {
        obWall.Tape_0 = "TapeS5";
    }
    if (document.getElementById("TapeS9").checked === true) {
        obWall.Tape_0 = "TapeS5";
    }
    if (document.getElementById("TapeS7").checked === true) {
        obWall.Tape_0 = "TapeS7";
    }
    if (document.getElementById("TapeS8").checked === true) {
        obWall.Tape_0 = "TapeS7";
    }
    if (document.getElementById("TapeS10").checked === true) {
        obWall.Tape_0 = "TapeS7";
    }
    if (document.getElementById("TapeS12").checked === true) {
        obWall.Tape_0 = "TapeS7";
    }
    if (document.getElementById("TapeS11").checked === true) {
        obWall.Tape_0 = "TapeS11";
    }
    if (document.getElementById("TapeS13").checked === true) {
        obWall.Tape_0 = "TapeS13";
    }
    obWall.Grupo = document.getElementById("ÌdGrup").value;
    //Tape Inicial

    obWall = null;
    ActionDbl = null;
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
});
$("#btnChangeDimension").on("mouseenter", function () {
    document.getElementById("btnChangeDimension").focus();
});
$("#ÌdGrupPilar").on("mouseenter", function () {
    document.getElementById("ÌdGrupPilar").focus();
});
function UpdateDimWall_Top() {
    for (var iUp3 = 0; iUp3 < scene.children.length; iUp3++) {
        if (scene.children[iUp3].name.substr(0, 17) === "Test_AddCorner70") {
            scene.remove(scene.children[iUp3]);
        }
        if (scene.children[iUp3].name.substr(0, 21) === "DimLine_TWDown0CDown1") {
            scene.remove(scene.children[iUp3]);
        }
    }
    var pointsDimDown = [];
    pointsDimDown.push(new THREE.Vector3(obWall.position.x - 200, 0, obWall.position.z));
    pointsDimDown.push(new THREE.Vector3(obWall.position.x - 200, 0, obWall.position.z + obWall.scale.z * 1000));
    const LineDown = new THREE.BufferGeometry().setFromPoints(pointsDimDown);
    const LineDimDown = new THREE.Line(LineDown, materialDimWall);
    LineDimDown.name = "DimLine_TWDown0CDown1";
    scene.add(LineDimDown);

    var radius = 1;
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    geom.name = "";
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "35pt Arial";
    ctx.fillStyle = '#000000';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#FFFFFF';
    }
    ctx.textAlign = "center";
    var _textoInsert = (obWall.scale.z * 10).toFixed(3);
    ctx.fillText(_textoInsert, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = (obWall.position.x - 200);
    sprite.position.y = -10;
    sprite.position.z = obWall.position.z + (obWall.scale.z / 2) * 1000;
    _dim.add(sprite);
    _dim.name = "UpdateTest_AddCorner70";
    scene.add(_dim);
}
function EraseDim1() {
    for (var i5 = 0; i5 < scene.children.length; i5++) {
        if (scene.children[i5].name === "Test_AddCorner70") {
            scene.remove(scene.children[i5]);
        }
        if (scene.children[i5].name === "UpdateTest_AddCorner70") {
            scene.remove(scene.children[i5]);
        }
        if (scene.children[i5].name === "FistArrowHelper_AddCorner70") {
            scene.remove(scene.children[i5]);
        }
        if (scene.children[i5].name === "DimLine_AddCorner70") {
            scene.remove(scene.children[i5]);
        }
    }
};
$("#DataWXConer").on("change", function () {
    var OldValue = obWall.scale.z * 10;
    var MoveY = $("#DataWXConer").val() - OldValue;
    obWall.scale.z = $("#DataWXConer").val() / 10;
    obWall.XWith = obEsqY.scale.x;
    obWall.position.z = obWall.position.z - (MoveY * 100);
    obWall.XWith = obEsqY.scale.x;
    var scaley = obWall.scale.z + 0.03;
    obWallX.scale.y = $("#DataWXConer").val() / 10;
    obEsqY.scale.z = scaley;
    obEsqY.position.z = obEsqY.position.z - (MoveY * 100);
    obWallY.scale.z = obWallY.scale.z - (MoveY / 10);
    //    obWallY.position.z = obWallY.position.z + (MoveY * 100);
});
$("#DataWYConer").on("change", function () {
    var OldValue = obWallY.scale.x * 10;
    var Move = $("#DataWYConer").val() - OldValue;
    obEsqY.scale.x = $("#DataWYConer").val() / 10;
    obWallY.scale.x = $("#DataWYConer").val() / 10;
    obEsqY.position.x = obEsqY.position.x + (Move * 100);
    obWallY.position.x = obWallY.position.x + (Move * 100);
    obEsqX.scale.x = obEsqX.scale.x + (Move / 10);
    obEsqX.position.x = obEsqX.position.x + (Move * 100);
    obWallX.scale.x = obWallX.scale.x + (Move / 10);
    obWallX.position.x = obWallX.position.x + (Move * 100);
    obEsqX.YWith = obEsqY.scale.x;
});
///Menu Wall
$("#btnMuro").on("mouseenter", function () {
    ResetMenuWall();
    document.getElementById("btnMuro").style.backgroundColor = "#efb608";
});
$("#TapMuro").on("mouseleave", function () {
    OnclickResetMenuWall();
});
$("#btnMuro").on('click', function (e) {
    MenuWallActive = "_TapMuro";
    $("#EditTape").hide("slide", { direction: "right" }, 400);
    $("#EditPuntal").hide("slide", { direction: "right" }, 400);
    $("#Recto").show("slide", { direction: "right" }, 400);
    OnclickResetMenuWall();
});
$("#btnPuntal").on("mouseenter", function () {
    ResetMenuWall();
    document.getElementById("btnPuntal").style.backgroundColor = "#efb608";
});
$("#TapPuntal").on("mouseleave", function () {
    OnclickResetMenuWall();
});
$("#btnPuntal").on('click', function (e) {
    MenuWallActive = "_TapPuntal";
    $("#EditTape").hide("slide", { direction: "right" }, 400);
    $("#Recto").hide("slide", { direction: "right" }, 400);
    $("#EditPuntal").show("slide", { direction: "right" }, 400);
    OnclickResetMenuWall();
});
$("#btnTape").on("mouseenter", function () {
    ResetMenuWall();
    document.getElementById("btnTape").style.backgroundColor = "#efb608";
});
$("#btnTape").on("mouseleave", function () {
    OnclickResetMenuWall();
});
$("#TapTape").on('click', function (e) {
    MenuWallActive = "_TapTape";
    $("#Recto").hide("slide", { direction: "right" }, 400);
    $("#EditPuntal").hide("slide", { direction: "right" }, 400);
    $("#EditTape").show("slide", { direction: "right" }, 400);
    OnclickResetMenuWall();
});

function ResetMenuWall() {
    document.getElementById("btnMuro").style.backgroundColor = "#F0F6F7";
    document.getElementById("btnTape").style.backgroundColor = "#F0F6F7";
    document.getElementById("btnPuntal").style.backgroundColor = "#F0F6F7";

    document.getElementById("btnMuro").style.color = "#787C7C";
    document.getElementById("btnTape").style.color = "#787C7C";
    document.getElementById("btnPuntal").style.color = "#787C7C";

};
function OnclickResetMenuWall() {
    if (MenuWallActive === "_TapMuro") {
        document.getElementById("btnMuro").style.backgroundColor = "#0d6efd";
        document.getElementById("btnMuro").style.color = "#FFFFFF";


        document.getElementById("btnTape").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnPuntal").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnTape").style.color = "#787C7C";
        document.getElementById("btnPuntal").style.color = "#787C7C";
    }
    if (MenuWallActive === "_TapTape") {
        document.getElementById("btnTape").style.backgroundColor = "#0d6efd";
        document.getElementById("btnTape").style.color = "#FFFFFF";

        document.getElementById("btnMuro").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnPuntal").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnMuro").style.color = "#787C7C";
        document.getElementById("btnPuntal").style.color = "#787C7C";

    }
    if (MenuWallActive === "_TapPuntal") {
        document.getElementById("btnPuntal").style.backgroundColor = "#0d6efd";
        document.getElementById("btnPuntal").style.color = "#FFFFFF";

        document.getElementById("btnTape").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnMuro").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnMuro").style.color = "#787C7C";
        document.getElementById("btnTape").style.color = "#787C7C";
    }

};


$("#btnIdMaterial").on("mouseenter", function () {
    ResetMenuListMaterial();
    document.getElementById("btnIdMaterial").style.backgroundColor = "#efb608";
});
$("#btnIdMaterial").on("mouseleave", function () {
    OnclickResetMenuListMaterial();
});
$("#btnIdMaterial").on('click', function (e) {
    MenuWallActive = "_TapMaterial";
    //$("#Recto").hide("slide", { direction: "right" }, 400);
    //$("#EditPuntal").hide("slide", { direction: "right" }, 400);
    //$("#EditTape").show("slide", { direction: "right" }, 400);
    OnclickResetMenuListMaterial();
});

$("#btnIdGrup").on("mouseenter", function () {
    ResetMenuListMaterial();
    document.getElementById("btnIdGrup").style.backgroundColor = "#efb608";
});
$("#btnIdGrup").on("mouseleave", function () {
    OnclickResetMenuListMaterial();
});


function ResetMenuListMaterial() {
    document.getElementById("btnIdMaterial").style.backgroundColor = "#F0F6F7";
    document.getElementById("btnIdMaterial").style.color = "#787C7C";
    document.getElementById("btnIdGrup").style.backgroundColor = "#F0F6F7";
    document.getElementById("btnIdGrup").style.color = "#787C7C";
};
function OnclickResetMenuListMaterial() {
    if (MenuWallActive === "_TapMaterial") {
        document.getElementById("btnIdMaterial").style.backgroundColor = "#0d6efd";
        document.getElementById("btnIdMaterial").style.color = "#FFFFFF";
        document.getElementById("btnIdGrup").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnIdGrup").style.color = "#787C7C";

    }
    if (MenuWallActive === "_TapGrup") {
        document.getElementById("btnIdGrup").style.backgroundColor = "#0d6efd";
        document.getElementById("btnIdGrup").style.color = "#FFFFFF";
        document.getElementById("btnIdMaterial").style.backgroundColor = "#F0F6F7";
        document.getElementById("btnIdMaterial").style.color = "#787C7C";
    }
};
$("#ÌdGrup").on("mouseenter", function () {
    document.getElementById("ÌdGrup").focus();
});

$("#DataSupInicial").on("change", function () {
    ChangeSolape0Final();
});
$("#DataSupEnd").on("change", function () {
    ChangeSolape0Final();
});

$("#DataCordenadX").on("mouseenter", function () {
    document.getElementById("DataCordenadX").focus();
});
$("#DataCordenadY").on("mouseenter", function () {
    document.getElementById("DataCordenadY").focus();
});


$("#DataSupInicial").on("mouseenter", function () {
    document.getElementById("DataSupInicial").focus();
});
$("#DataSupEnd").on("mouseenter", function () {
    document.getElementById("DataSupEnd").focus();
});
//Tape buton Inicial and End
$("#TapeS1").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS1").checked = true;
});
$("#TapeS2").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS2").checked = true;
});
$("#TapeS3").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS3").checked = true;
});
$("#TapeS4").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS4").checked = true;
});
$("#TapeS5").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS5").checked = true;
});
$("#TapeS6").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS6").checked = true;
});
$("#TapeS7").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS7").checked = true;
});
$("#TapeS8").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS8").checked = true;
});
$("#TapeS9").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS9").checked = true;
});
$("#TapeS10").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS10").checked = true;
});
$("#TapeS11").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS11").checked = true;
});
$("#TapeS12").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS12").checked = true;
});
$("#TapeS13").on('click', function (e) {
    OnclickResetFinalMenuTape();
    document.getElementById("TapeS13").checked = true;
});

//$("#CHeckFinalMayorAnchoNoExactoSolucion1_2").on('click', function (e) {
//    OnclickResetFinalMenuTape();
//    document.getElementById("CHeckFinalMayorAnchoNoExactoSolucion1_2").checked = true;
//});


function OnclickResetInicialMenuTape() {
    obWall.Tape_180 = "";
    //document.getElementById("TapeS1").checked = false;
    //document.getElementById("TapeS2").checked = false;
    //document.getElementById("TapeS3").checked = false;
    //document.getElementById("TapeS4").checked = false;
    //document.getElementById("TapeS5").checked = false;
    //document.getElementById("TapeS6").checked = false;
    //document.getElementById("TapeS7").checked = false;
    //document.getElementById("TapeS8").checked = false;
    //document.getElementById("TapeS9").checked = false;
    //document.getElementById("TapeS10").checked = false;
    //document.getElementById("TapeS11").checked = false;
    //document.getElementById("TapeS12").checked = false;
    //document.getElementById("TapeS13").checked = false;
};
function OnclickResetFinalMenuTape() {
    document.getElementById("TapeS1").checked = false;
    document.getElementById("TapeS2").checked = false;
    document.getElementById("TapeS3").checked = false;
    document.getElementById("TapeS4").checked = false;
    document.getElementById("TapeS5").checked = false;
    document.getElementById("TapeS6").checked = false;
    document.getElementById("TapeS7").checked = false;
    document.getElementById("TapeS8").checked = false;
    document.getElementById("TapeS9").checked = false;
    document.getElementById("TapeS10").checked = false;
    document.getElementById("TapeS11").checked = false;
    document.getElementById("TapeS12").checked = false;
    document.getElementById("TapeS13").checked = false;
};

$("#CHeckPropInside").on('click', function (e) {
    if (document.getElementById("CHeckPropInside").checked === false) {
        document.getElementById("PuntaInfInt").style.display = "none";
    }
    else {
        if ($("#DataHeight").val() > 1.5) {
            document.getElementById("PuntaInfInt").style.display = "inline";
        }
        else {
            document.getElementById("PuntaInfInt").style.display = "none";
        }

    }
});

$("#CHeckPropOutside").on('click', function (e) {
    if (document.getElementById("CHeckPropOutside").checked === false) {

        document.getElementById("PuntaInfExt").style.display = "none";
    }
    else {
        if ($("#DataHeight").val() > 1.5) {
            document.getElementById("PuntaInfExt").style.display = "inline";
        }
        else {
            document.getElementById("PuntaInfExt").style.display = "none";
        }
    }
});
///Nucleo
$("#NucleoL").on("mouseenter", function () {
    document.getElementById("NucleoL").focus();
});
$("#NucleoW").on("mouseenter", function () {
    document.getElementById("NucleoW").focus();
});
$("#NucleoH").on("mouseenter", function () {
    document.getElementById("NucleoH").focus();
});
$("#EWS").on("mouseenter", function () {
    document.getElementById("EWS").focus();
});
$("#EWI").on("mouseenter", function () {
    document.getElementById("EWI").focus();
});
$("#ELI").on("mouseenter", function () {
    document.getElementById("ELI").focus();
});
$("#ELD").on("mouseenter", function () {
    document.getElementById("ELD").focus();
});
$("#NucleoL").on("click", function () {
    document.getElementById("NucleoL").value = "";
});
$("#NucleoW").on("click", function () {
    document.getElementById("NucleoW").value = "";
});
$("#NucleoH").on("click", function () {
    document.getElementById("NucleoH").value = "";
});
$("#EWS").on("click", function () {
    document.getElementById("EWS").value = "";
});
$("#EWI").on("click", function () {
    document.getElementById("EWI").value = "";
});
$("#ELI").on("click", function () {
    document.getElementById("ELI").value = "";
});
$("#ELD").on("click", function () {
    document.getElementById("ELD").value = "";
});

$("#btAddNucleo").on("click", function () {
    $("#DivNucleo").hide("slide", { direction: "left" }, 400);
    ResetSetup();
    meshNucleo.visible = true;
    meshNucleo.scale.x = document.getElementById("NucleoL").value;
    meshNucleo.scale.y = document.getElementById("NucleoW").value;
    meshNucleo.scale.z = document.getElementById("NucleoH").value;
    IsFormArtive = false;
    ActionDbl = "Add_Nucleo";
    InsertWall = 16;
});
//angel
$("#IdWidthDefault").on("mouseenter", function () {
    document.getElementById("IdWidthDefault").focus();
});
$("#IdHeightDefault").on("mouseenter", function () {
    document.getElementById("IdHeightDefault").focus();
});
