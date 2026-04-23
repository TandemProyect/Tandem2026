document.addEventListener('dblclick', ondblclick, false);
function GetAdMove_Wall_270_To_Grill() {
    var obZ = obWall.position.z + (obWall.scale.z * 1000);
    var g_Position_z = 1999999999;
    for (var g = 0; g < scene.children.length; g++) {
        if (scene.children[g].type === "Mesh") {
            if (scene.children[g].MeshTypeWall === "Grill_000") {
                if (obZ < scene.children[g].position.z) {
                    if (scene.children[g].position.z < g_Position_z)
                        g_Position_z = scene.children[g].position.z;
                }
            }
        }
    }
    if (g_Position_z !== 1999999999) {
        var distZ = (obZ - g_Position_z) / 1000;
        if (distZ < 0) { distZ = distZ * -1 }
        obWall.scale.z = obWall.scale.z + distZ;
        obWall.position.z = g_Position_z - (obWall.scale.z * 1000);
    }
    ResetSetup();
}
function GetAdMove_Wall_90_To_Grill() {
    var obZ = obWall.position.z;
    var g_Position_z = -1999999999;
    for (var g = 0; g < scene.children.length; g++) {
        if (scene.children[g].type === "Mesh") {
            if (scene.children[g].MeshTypeWall === "Grill_000") {
                if (obZ > scene.children[g].position.z) {
                    if (scene.children[g].position.z > g_Position_z)
                        g_Position_z = scene.children[g].position.z;
                }
            }
        }
    }
    if (g_Position_z !== -1999999999) {
        var distZ = (g_Position_z - obZ) / 1000;
        if (distZ < 0) { distZ = distZ * -1 }
        obWall.scale.z = obWall.scale.z + distZ;
        obWall.position.z = g_Position_z;
    }
    ResetSetup();
}
function GetAdMove_Wall_180_To_Grill() {
    var obX = obWall.position.x;
    var g_Position_x = 1999999999;
    for (var g = 0; g < scene.children.length; g++) {
        if (scene.children[g].type === "Mesh") {
            if (scene.children[g].MeshTypeWall === "Grill_900") {
                if (obX > scene.children[g].position.x) {
                    if (scene.children[g].position.x < g_Position_x)
                        g_Position_x = scene.children[g].position.x;
                }
            }
        }
    }
    if (g_Position_x !== -1999999999) {
        var distX = (obX - g_Position_x) / 1000;
        obWall.scale.x = obWall.scale.x + distX;
        obWall.position.x = g_Position_x;
    }
    ResetSetup();
}
function GetAdMove_Wall_0_To_Grill() {
    var obX = obWall.position.x + (obWall.scale.x * 1000);
    var g_Position_x = -1999999999;
    for (var g = 0; g < scene.children.length; g++) {
        if (scene.children[g].type === "Mesh") {
            if (scene.children[g].MeshTypeWall === "Grill_900") {
                if (obX < scene.children[g].position.x) {
                    if (scene.children[g].position.x > g_Position_x)
                        g_Position_x = scene.children[g].position.x;
                }
            }
        }
    }
    if (g_Position_x !== -1999999999) {
        var distX = (g_Position_x - obX) / 1000;
        obWall.scale.x = obWall.scale.x + distX;
    }
    ResetSetup();
}

function ondblclick(event) {

    CurrentInsertWall = null;

    if (ActionDbl === "Control_Move_0") {
        GetAdMove_Wall_0_To_Grill();
    }
    if (ActionDbl === "Control_Move_180") {
        GetAdMove_Wall_180_To_Grill();
    }
    if (ActionDbl === "Control_Move_90") {
        GetAdMove_Wall_90_To_Grill();
    }
    if (ActionDbl === "Control_Move_270") {
        GetAdMove_Wall_270_To_Grill();
    }
    if (ActionWizard === 600) {
        if (Wall_Conexion_1 === null) {
            if (document.getElementById("IdHelp").checked === true) {
                if (document.getElementById("IdHelpVideo").checked === true) {
                    videoElem.pause();
                    videoElem.src = "../../Content/DesignTools/Help/Ayuda_Conexion_2.mp4";
                    videoElem.play();
                }
                else {
                    $("#ToasMesajeImg").show("slide", { direction: "right" }, 400);
                }
            }
            Wall_Conexion_1 = obWallMouseMove;
            Wall_Conexion_1.material = SelectMaterialConexion_1;
            return;
        }
        else {
            if (document.getElementById("IdHelp").checked === true) {
                if (document.getElementById("IdHelpVideo").checked === true) {
                    videoElem.pause();
                    videoElem.src = "";
                    $("#ToasMesaje").hide("slide", { direction: "right" }, 400);
                }
                else {
                    // aqui help Img
                }
            }
            Wall_Conexion_2 = obWallMouseMove;
            Wall_Conexion_2.material = SelectMaterialConexion_2;
            AnalyzeConnection_T(Wall_Conexion_1, Wall_Conexion_2);

            if (Wall_Conexion_1 == null) { return; }
            if (Wall_Conexion_2 == null) { return; }
            if (Wall_Conexion_1.MeshTypeWall === 'Wall_R000' && Wall_Conexion_2.MeshTypeWall === 'Wall_R000') {
                var YCoordenateFirstWall = Wall_Conexion_1.position.z;
                var YCoordenateSecondWall = Wall_Conexion_2.position.z;
                if (YCoordenateFirstWall === YCoordenateSecondWall) {
                    ResolverConnection_0(Wall_Conexion_1, Wall_Conexion_2);
                }
                return;
            }

            if (Wall_Conexion_1.MeshTypeWall === 'Wall_R900' && Wall_Conexion_2.MeshTypeWall === 'Wall_R900') {
                var XCoordenateFirstWall = Wall_Conexion_1.position.x;
                var XCoordenateSecondWall = Wall_Conexion_2.position.x;
                if (XCoordenateFirstWall === XCoordenateSecondWall) {
                    ResolverConnection_90(Wall_Conexion_1, Wall_Conexion_2);
                }
            }
            return;
        }
    }
    if (IsMiddleOfConnecting === true) {
        CreateConection90x0(FirstWallConexion, SecontWallConexion, ValueNewWall, false);
    }
    if (IsMiddleOfConnectingX === true) {
        CreateConection90x0X(SecontWallConexion, FirstWallConexion, ValueNewWall);
    }
    //GetSceneListMaterial(scene.children);
    if (ActionDbl === "Add_Nucleo") {
        AddNucleo_Wall();
    }
    if (ActionDbl === "Control_Move_Esq_20") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCorner20(value);
    }
    if (ActionDbl === "Control_Move_Esq_80") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCorner80(value);
    }
    if (ActionDbl === "Control_Move_Esq_X") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCornerX(value);
    }
    if (ActionDbl === "Control_Move_Parall_90") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        ActionWizard = 0;
        AddCornerParall_90(value);
    }
    if (ActionDbl === "Control_Move_Parall") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        ActionWizard = 0;
        AddCornerParall(value);
    }
    if (ActionDbl === "Control_Move_Esq_X_0") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCornerX_0(value);
    }
    if (ActionDbl === "Control_Move_Esq_40") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCorner40(value);
    }
    if (ActionDbl === "Control_Move_Esq_60") {
        var value = ($("#InputDim").val() / 10).toFixed(3);
        AddCorner60(value);
    }
    if (ActionDbl === "AddCorner10_90") { AddCorner10_90(); }
    if (ActionDbl === "AddCorner10_00") { AddCorner10_00(); }
    if (ActionDbl === "AddCorner30_90") { AddCorner30_90(); }
    if (ActionDbl === "AddCorner30_00") { AddCorner30_00(); }
    if (ActionDbl === "AddCorner70_00") {
        AddCorner70_00();
    }
    if (ActionDbl === "AddCorner70_90") {
        AddCorner70_90();
    }
    if (ActionDbl === "AddCorner50_00") {
        AddCorner50_00();
    }
    if (ActionDbl === "AddCorner50_90") {
        AddCorner50_90();
    }
    if (ActionDbl === "Worker") {
        obWall = obWallMouseMove;
        EraseWorker(obWall.name);
        EraseWorker(obWall.name);
        EraseWorker(obWall.name);
    }
    if (ActionDbl === "Prop") { obWall = obWallMouseMove; EraseProp(obWall.name); }
    if (ActionDbl === "Wall_R900") {
        getValorbydefect();
        obWall = obWallMouseMove;
        //obWall.material = materialWallAct;
        MenuWallActive = "_TapMuro";
        $("#EditTape").hide("slide", { direction: "right" }, 400);
        $("#EditPuntal").hide("slide", { direction: "right" }, 400);
        $("#EditDim").show("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 2);
        $("#compassContainer").hide("slide", { direction: "right" }, 400);
        $("#MenubottomDesign").hide("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 1);
        $("#Recto").show("slide", { direction: "right" }, 400);
        $("#TapMuro").show();
        OnclickResetMenuWall();
        if (IsFormArtive === true) { return; }
        ActionDbl = null;
        obWallMouseMove = null;
        const Iniciall_Wall = 0;
        const End_Wall = 0;
        obWallScaleX = (obWall.scale.x * 10).toFixed(3);
        obWallScaleY = (obWall.scale.y * 10).toFixed(3);
        obWallScaleZ = (obWall.scale.z * 10).toFixed(3);
        $("#EditDim").css({ top: 65, left: 5, position: 'absolute' });
        $("#EditDim").show("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 2);
        $("#compassContainer").hide("slide", { direction: "right" }, 400);
        MenuWallActive = "_TapMuro";
        OnclickResetMenuWall();
        IsFormArtive = true;
        Edit_Wall = 0;
        ActionDbl = "";
        $("#ÌdGrup").val(obWall.Grupo);
        $("#Datalong").val(obWallScaleZ);
        $("#DataWith").val(obWallScaleX);
        $("#DataHeight").val(obWallScaleY);
        $("#DataSupInicial").val(Iniciall_Wall);
        $("#DataSupEnd").val(End_Wall);
        $("#DataCordenadX").val(obWall.position.x * 10);
        $("#DataCordenadY").val(obWall.position.z * 10);
        document.getElementById("CHeckDimWall").checked = false;
        if (obWall.CHeckDimWall === true) { document.getElementById("CHeckDimWall").checked = true; }
        document.getElementById("CHeckBracketInside").checked = false;
        if (obWall.CHeckBracketInside === true) { document.getElementById("CHeckBracketInside").checked = true; }
        if (obWall.CHeckBracketInside === "True") { document.getElementById("CHeckBracketInside").checked = true; }
        document.getElementById("CHeckBracketOutside").checked = false;
        if (obWall.CHeckBracketInside === true) { document.getElementById("CHeckBracketOutside").checked = true; }
        if (obWall.CHeckBracketInside === "True") { document.getElementById("CHeckBracketOutside").checked = true; }
        document.getElementById("CHeckRijiInside").checked = false;
        if (obWall.CHeckRijiInside === true) { document.getElementById("CHeckRijiInside").checked = true; }
        document.getElementById("CHeckRijiOutside").checked = false;
        if (obWall.CHeckRijiOutside === true) { document.getElementById("CHeckRijiOutside").checked = true; }
        document.getElementById("CHeckPropInside").checked = false;
        if (obWall.CHeckPropInside === "True" || obWall.CHeckPropInside === true) { document.getElementById("CHeckPropInside").checked = true; }
        document.getElementById("CHeckPropOutside").checked = false;
        if (obWall.CHeckPropOutside === "True" || obWall.CHeckPropOutside === true) { document.getElementById("CHeckPropOutside").checked = true; }
        document.getElementById("CHeckPropInsideInf").checked = false;
        if (obWall.CHeckPropInsideInf === "True" || obWall.CHeckPropInsideInf === true) { document.getElementById("CHeckPropInsideInf").checked = true; }
        document.getElementById("CHeckPropOutsideInf").checked = false;
        if (obWall.CHeckPropOutsideInf === "True" || obWall.CHeckPropOutsideInf === true) { document.getElementById("CHeckPropOutsideInf").checked = true; }
        if (document.getElementById("CHeckPropInside").checked === false) {
            document.getElementById("PuntaInfInt").style.display = "none";
        }
        else { document.getElementById("PuntaInfInt").style.display = "inline"; }
        if (document.getElementById("CHeckPropOutside").checked === false) {
            document.getElementById("PuntaInfExt").style.display = "none";
        }
        else { document.getElementById("PuntaInfExt").style.display = "inline"; }
        document.getElementById("CHeck750R").checked = false;

        if (obWall.CHeck750R === "True") { document.getElementById("CHeck750R").checked = true; }
        if (obWall.CHeck750R === true) { document.getElementById("CHeck750R").checked = true; }

        document.getElementById("CHeckTypeFormworkMode").checked = false;
        if (obWall.IdTypeFormworkMode === "True") { document.getElementById("CHeckTypeFormworkMode").checked = true; }
        if (obWall.IdTypeFormworkMode === true) { document.getElementById("CHeckTypeFormworkMode").checked = true; }


        //End Tape
        resetDivTapeFinal();
        CalculationEndSolape();
        //ChangeSolape0Final();
        if (obWall.Tape_0 === "TapeS1") {
            document.getElementById("ChekSolapeEnd").checked = true;
            document.getElementById("TapeS1").checked = true;
            $("#DivTape_End_01").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS2") {
            document.getElementById("ChekSolapeEnd").checked = true;
            document.getElementById("TapeS2").checked = true;
            $("#DivTape_End_01").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS3") {
            document.getElementById("TapeS3").checked = true;
            $("#DivTape_End_02").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS4") {
            document.getElementById("TapeS4").checked = true;
            $("#DivTape_End_03").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS5") {
            document.getElementById("TapeS5").checked = true;
            $("#DivTape_End_03").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS6") {
            document.getElementById("TapeS6").checked = true;
            $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS7") {
            document.getElementById("TapeS7").checked = true;
            $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS8") {
            document.getElementById("TapeS8").checked = true;
            $("#DivTape_End_05").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS9") {
            document.getElementById("TapeS9").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS10") {
            document.getElementById("TapeS10").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS11") {
            document.getElementById("TapeS11").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS12") {
            document.getElementById("TapeS12").checked = true;
            $("#DivTape_End_07").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS13") {
            document.getElementById("TapeS13").checked = true;
            $("#DivTape_End_07").show("slide", { direction: "right" }, 4);
        }
        $("#DataSupEnd").val(obWall.End_Wall);
    }
    if (ActionDbl === "Wall_R000") {
        getValorbydefect();
        obWall = obWallMouseMove;
        //obWall.material = materialWallAct;
        MenuWallActive = "_TapMuro";
        $("#EditTape").hide("slide", { direction: "right" }, 400);
        $("#EditPuntal").hide("slide", { direction: "right" }, 400);
        $("#EditDim").show("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 2);
        $("#compassContainer").hide("slide", { direction: "right" }, 400);
        $("#MenubottomDesign").hide("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 1);
        $("#Recto").show("slide", { direction: "right" }, 400);
        $("#TapMuro").show();
        OnclickResetMenuWall();
        if (IsFormArtive === true) { return; }
        ActionDbl = null;
        obWallMouseMove = null;
        const Iniciall_Wall = 0;
        const End_Wall = 0;
        obWallScaleX = (obWall.scale.x * 10).toFixed(3);
        obWallScaleY = (obWall.scale.y * 10).toFixed(3);
        obWallScaleZ = (obWall.scale.z * 10).toFixed(3);
        $("#EditDim").css({ top: 65, left: 5, position: 'absolute' });
 
        var Conexion_180 = getCHeckWallMaster(obWall.IdWall_180);
        var Conexion_0 = getCHeckWallMaster(obWall.IdWall_0);
        var Conexion_90 = getCHeckWallMaster(obWall.IdWall_90);
        var Conexion_270 = getCHeckWallMaster(obWall.IdWall_270);
        CheckConectión(Conexion_0, Conexion_90, Conexion_180, Conexion_270);

        $("#EditDim").show("slide", { direction: "right" }, 400);
        controls = ReturnControlsForCamera(camera, 2);
        $("#compassContainer").hide("slide", { direction: "right" }, 400);
        MenuWallActive = "_TapMuro";
        OnclickResetMenuWall();
        IsFormArtive = true;
        Edit_Wall = 0;
        ActionDbl = "";
        $("#ÌdGrup").val(obWall.Grupo);
        $("#Datalong").val(obWallScaleX);
        $("#DataWith").val(obWallScaleY);
        $("#DataHeight").val(obWallScaleZ);
        $("#DataSupInicial").val(Iniciall_Wall);
        $("#DataSupEnd").val(End_Wall);
        $("#DataCordenadX").val(obWall.position.x * 10);
        $("#DataCordenadY").val(obWall.position.z * 10);


        document.getElementById("CHeckDimWall").checked = false;
        if (obWall.CHeckDimWall === true) { document.getElementById("CHeckDimWall").checked = true; }
        document.getElementById("CHeckBracketInside").checked = false;
        if (obWall.CHeckBracketInside === true) { document.getElementById("CHeckBracketInside").checked = true; }
        if (obWall.CHeckBracketInside === "True") { document.getElementById("CHeckBracketInside").checked = true; }
        document.getElementById("CHeckBracketOutside").checked = false;
        if (obWall.CHeckBracketInside === true) { document.getElementById("CHeckBracketOutside").checked = true; }
        if (obWall.CHeckBracketInside === "True") { document.getElementById("CHeckBracketOutside").checked = true; }
        document.getElementById("CHeckRijiInside").checked = false;
        if (obWall.CHeckRijiInside === true) { document.getElementById("CHeckRijiInside").checked = true; }
        document.getElementById("CHeckRijiOutside").checked = false;
        if (obWall.CHeckRijiOutside === true) { document.getElementById("CHeckRijiOutside").checked = true; }
        document.getElementById("CHeckPropInside").checked = false;
        if (obWall.CHeckPropInside === "True" || obWall.CHeckPropInside === true) { document.getElementById("CHeckPropInside").checked = true; }
        document.getElementById("CHeckPropOutside").checked = false;
        if (obWall.CHeckPropOutside === "True" || obWall.CHeckPropOutside === true) { document.getElementById("CHeckPropOutside").checked = true; }
        document.getElementById("CHeckPropInsideInf").checked = false;
        if (obWall.CHeckPropInsideInf === "True" || obWall.CHeckPropInsideInf === true) { document.getElementById("CHeckPropInsideInf").checked = true; }
        document.getElementById("CHeckPropOutsideInf").checked = false;
        if (obWall.CHeckPropOutsideInf === "True" || obWall.CHeckPropOutsideInf === true) { document.getElementById("CHeckPropOutsideInf").checked = true; }
        if (document.getElementById("CHeckPropInside").checked === false) {
            document.getElementById("PuntaInfInt").style.display = "none";
        }
        else { document.getElementById("PuntaInfInt").style.display = "inline"; }
        if (document.getElementById("CHeckPropOutside").checked === false) {
            document.getElementById("PuntaInfExt").style.display = "none";
        }
        else { document.getElementById("PuntaInfExt").style.display = "inline"; }
        document.getElementById("CHeck750R").checked = false;
        if (obWall.CHeck750R === "True") { document.getElementById("CHeck750R").checked = true; }
        if (obWall.CHeck750R === true) { document.getElementById("CHeck750R").checked = true; }

        document.getElementById("CHeckTypeFormworkMode").checked = false;
        if (obWall.IdTypeFormworkMode === "True") { document.getElementById("CHeckTypeFormworkMode").checked = true; }
        if (obWall.IdTypeFormworkMode === true) { document.getElementById("CHeckTypeFormworkMode").checked = true; }




        //End Tape
        resetDivTapeFinal();
        CalculationEndSolape();
        //ChangeSolape0Final();
        if (obWall.Tape_0 === "TapeS1") {
            document.getElementById("ChekSolapeEnd").checked = true;
            document.getElementById("TapeS1").checked = true;
            $("#DivTape_End_01").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS2") {
            document.getElementById("ChekSolapeEnd").checked = true;
            document.getElementById("TapeS2").checked = true;
            $("#DivTape_End_01").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS3") {
            document.getElementById("TapeS3").checked = true;
            $("#DivTape_End_02").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS4") {
            document.getElementById("TapeS4").checked = true;
            $("#DivTape_End_03").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS5") {
            document.getElementById("TapeS5").checked = true;
            $("#DivTape_End_03").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS6") {
            document.getElementById("TapeS6").checked = true;
            $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS7") {
            document.getElementById("TapeS7").checked = true;
            $("#DivTape_End_04").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS8") {
            document.getElementById("TapeS8").checked = true;
            $("#DivTape_End_05").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS9") {
            document.getElementById("TapeS9").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS10") {
            document.getElementById("TapeS10").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS11") {
            document.getElementById("TapeS11").checked = true;
            $("#DivTape_End_06").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS12") {
            document.getElementById("TapeS12").checked = true;
            $("#DivTape_End_07").show("slide", { direction: "right" }, 4);
        }
        if (obWall.Tape_0 === "TapeS13") {
            document.getElementById("TapeS13").checked = true;
            $("#DivTape_End_07").show("slide", { direction: "right" }, 4);
        }
        $("#DataSupEnd").val(obWall.End_Wall);
    }
    if (ActionDbl === "Pilar") {
        obWall = obWallMouseMove;
        obWall.material = materialWallAct;
        $("#EditPilar").show("slide", { direction: "right" }, 400);
        OnclickResetMenuWall();
        if (IsFormArtive === true) { return; }
        ActionDbl = null;
        obWallMouseMove = null;
        const Iniciall_Wall = 0;
        const End_Wall = 0;
        obWallScaleX = (obWall.scale.x * 10).toFixed(3);
        obWallScaleY = (obWall.scale.y * 10).toFixed(3);
        obWallScaleZ = (obWall.scale.z * 10).toFixed(3);
        $("#EditPilar").css({ top: 65, left: 5, position: 'absolute' });
        OnclickResetMenuWall();
        IsFormArtive = true;
        Edit_Wall = 0;
        ActionDbl = "";
        $("#ÌdGrupPilar").val(obWall.Grupo);
        $("#DatalongPilar").val(obWallScaleX);
        $("#DataWithPilar").val(obWallScaleY);
        $("#DataHeightPilar").val(obWallScaleZ);
        $("#DataCordenadXPilar").val(obWall.position.x * 10);
        $("#DataCordenadYPilar").val(obWall.position.z * 10);
        document.getElementById("CHeckDimWall").checked = false;
        document.getElementById("CHeck750RPilar").checked = false;
        if (obWall.CHeck750R === "True") { document.getElementById("CHeck750RPilar").checked = true; }
        if (obWall.CHeck750R === true) { document.getElementById("CHeck750RPilar").checked = true; }
    }
    if (ActionDbl === "Esq_30") {
        OpenFormEsq_30();
    }

    if (ActionDbl === "Esq_10") {
        OpenFormEsq_10();
    }

    if (ActionDbl === "Esq_50") {
        OpenFormEsq_50();
    }
    if (ActionDbl === "Esq_70") {
        OpenFormEsq_70();
    }
    if (InsertWall === 1) { AddWall_R000_1And_111(); }
    if (InsertWall === 111) { AddWall_R000_1And_111(); }
    if (InsertWall === 2) { AddWall_R900_2And_222(); }
    if (InsertWall === 222) { AddWall_R900_2And_222(); }
    if (InsertWall === 10) {
        meshWall_0.visible = false;
        InsertWall = 0;
        let longWall = 1.5;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq10.position.x, meshEsq10.position.z + 30, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner10_00Temp(); }, 50);

    }

    



    if (InsertWall === 20) {
        meshWall_0.visible = false;
        InsertWall = 0;
        let longWall = 3;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
 
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq20.position.x - 150, meshEsq20.position.z, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () {
            AddCorner20_00Temp();
        }, 50);
    }
    if (InsertWall === 30) {
        meshEsq30.visible = false;
        InsertWall = 0;
        let longWall = 1.5;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq30.position.x - 150, meshEsq30.position.z + 30, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner30_00Temp(); }, 50);
    }
    if (InsertWall === 50) {
        meshEsq50.visible = false;
        InsertWall = 0;
        let longWall = 1.5;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq50.position.x - 150, meshEsq50.position.z, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner50_00Temp(); }, 50);
    }
    if (InsertWall === 60) {
        meshEsq60.visible = false;
        InsertWall = 0;
        let longWall = 3;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq60.position.x - 150, meshEsq60.position.z, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner60_00Temp(); }, 50);
    }
    if (InsertWall === 80) {
        meshEsq80.visible = false;
        InsertWall = 0;
        let longWall = 0.3;
        var widthWall = document.getElementById("IdWidthDefault").value/10;
        var heightWall = document.getElementById("IdHeightDefault").value/10;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R900" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R900(meshEsq80.position.x + widthWall, meshEsq80.position.z - 150, longWall, widthWall, heightWall, "Wall_R900", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner80_00Temp(); }, 50);
    }
    if (InsertWall === 15) {
        meshEsqX.visible = false;
        InsertWall = 0;
        let longWall = 4;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(
            meshEsqX.position.x - 150,
            meshEsqX.position.z,
            longWall, widthWall,
            heightWall,
            "Wall_R000",
            IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCornerX_00Temp(); }, 50);
    }
    if (InsertWall === 40) {
        meshEsq40.visible = false;
        InsertWall = 0;
        let longWall = 0.3;
        var widthWall = document.getElementById("IdWidthDefault").value/10;
        var heightWall = document.getElementById("IdHeightDefault").value/10;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R900" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R900(
            /*1*/ meshEsq40.position.x,
            /*2*/ meshEsq40.position.z - 150,
            /*3*/ longWall,
            /*4*/ widthWall,
           /* 5*/ heightWall,
           /* 6*/ "Wall_R900",
            /*7*/ IdWall,
            /*8*/ IdWall_0,
            /*9*/ Sub_Long_0,
            /*10*/ Sub_Long_180,
            /*11*/ IdWall_180,
            /*12*/ IdWall_90,
            /*13*/ Sub_Long_90,
            /*14*/ IdWall_270,
            /*15*/ Sub_Long_270,
            /*16*/ IdUndoRedo,
            /*17*/ false
        );
        setTimeout(function () { AddCorner40_00Temp(); }, 50);
    }
    if (InsertWall === 70) {
        meshWall_0.visible = false;
        InsertWall = 0;
        let longWall = 1.5;
        var widthWall = document.getElementById("IdWidthDefault").value;
        var heightWall = document.getElementById("IdHeightDefault").value;
        var IdpartName = new Date().valueOf();
        var IdWall = "Wall_R000" + IdpartName;
        var IdWall_0 = "0";
        var Sub_Long_0 = "0";
        var Sub_Long_180 = "0";
        var IdWall_180 = "0";
        var IdWall_90 = "0";
        var Sub_Long_90 = "0";
        var IdWall_270 = "0";
        var Sub_Long_270 = "0";
        AddWall_R000(meshEsq70.position.x, meshEsq70.position.z, longWall, widthWall, heightWall, "Wall_R000", IdWall,
            IdWall_0,
            Sub_Long_0,
            Sub_Long_180,
            IdWall_180,
            IdWall_90,
            Sub_Long_90,
            IdWall_270,
            Sub_Long_270,
            IdUndoRedo,
            false
        );
        setTimeout(function () { AddCorner70_00Temp(); }, 50);
    }
    if (InsertWall === 5) {
        InsertWall = 0;
        InsertWorker(rollOverMesh.position.x, rollOverMesh.position.z);
    }
    //Pilar
    if (InsertWall === 14) {
        InsertWall = 0;
        let longWall = 0.30;
        widthWall = 0.30;
        heightWall = 2.70;
        InsertPilar(rollOverMesh.position.x, rollOverMesh.position.z, 0, longWall, widthWall, heightWall);
    }
}
function AddWall_R900_2And_222() {
    meshWall_90.visible = false;
    let longWall = 0.960;
    var widthWall = document.getElementById("IdWidthDefault").value/10;
    var heightWall = document.getElementById("IdHeightDefault").value/10;

    var IdpartName = new Date().valueOf();
    var IdWall = "Wall_R900" + IdpartName;
    var IdWall_0 = "0";
    var Sub_Long_0 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_90 = "0";
    var Sub_Long_90 = "0";
    var IdWall_270 = "0";
    var Sub_Long_270 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    var meshWall_90PositionX = meshWall_90.position.x + 30;
    if (InsertWall === 222) {
        meshWall_90PositionX = meshWall_90.position.x;
    }
    InsertWall = 0;
    AddWall_R900(meshWall_90PositionX, (meshWall_90.position.z - 9.6 * 100), longWall, widthWall, heightWall, "Wall_R900", IdWall,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        false
    );
};
function AddWall_R000_1And_111() {
    meshWall_0.visible = false;
    let longWall = 9.60;
    //angel
    var widthWall = document.getElementById("IdWidthDefault").value;
    var heightWall = document.getElementById("IdHeightDefault").value;
 
    var IdpartName = new Date().valueOf();
    var IdWall = "Wall_R000" + IdpartName;
    var IdWall_0 = "0";
    var Sub_Long_0 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_90 = "0";
    var Sub_Long_90 = "0";
    var IdWall_270 = "0";
    var Sub_Long_270 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    var meshWall_0PositionZ = meshWall_0.position.z;
    if (InsertWall === 111) {
        meshWall_0PositionZ = meshWall_0.position.z + 30;
    }
    AddWall_R000(meshWall_0.position.x, meshWall_0PositionZ, longWall, widthWall, heightWall, "Wall_R000", IdWall,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        true
    );


};
function EraseWorker(Idname) {

    var IdSelecte = Idname.substring(6, 19);
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].name === "") {
            continue;
        }
        if (scene.children[i].name.substring(6, 19) === IdSelecte) {
            scene.remove(scene.children[i]);
        }
    }

};
function EraseProp(Idname) {
    //GetSceneListMaterial(scene.children);
    var IdSelecte = Idname.substring(14, 46);
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].name === "") {
            continue;
        }
        if (scene.children[i].name.substring(14, 46) === IdSelecte) {
            scene.remove(scene.children[i]);
        }
        if (scene.children[i].name.substring(12, 44) === IdSelecte) {
            scene.remove(scene.children[i]);
        }
    }
};
function AnalyzeConnection_T(Wall_Conexion_1, Wall_Conexion_2) {
    var TypeConexion_1 = Wall_Conexion_1.MeshTypeWall;
    var TypeConexion_2 = Wall_Conexion_2.MeshTypeWall;
    if (TypeConexion_1 === 'Wall_R900' & TypeConexion_2 === 'Wall_R000') {
        CreateConectionT_90_00(Wall_Conexion_1, Wall_Conexion_2);
        return;
    }
    if (TypeConexion_1 === 'Wall_R000' & TypeConexion_2 === 'Wall_R900') {
        CreateConectionT_90_00(Wall_Conexion_2, Wall_Conexion_1);
        return;
    }

};