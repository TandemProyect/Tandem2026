
function EraseDesign() {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name === undefined) {
                continue;
            }
            if (scene.children[i].name === null) {
                continue;
            }
            if (scene.children[i].name.substr(0, 10) === "Face_Wall_") {
                var ob = scene.children[i];
                scene.remove(ob);
            }
            if (scene.children[i].name.substr(0, 5) === "Wall_") {
                var ob = scene.children[i];
                ob.material = new THREE.MeshLambertMaterial({ color: 0x839192 });
            }
            if (scene.children[i].name.substr(0, 5) === "Atk60") {
                var obPanel = scene.children[i];
                scene.remove(obPanel);
            }
            InsertWall = 0;
        }
    }
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name.substr(0, 5) === "Atk60") {
                EraseDesign();
                EraseDesign();
            }
        }
    }
};
function EraseDimensionWall1() {
    for (var i = 0; i < scene.children.length; i++) {
        //AddCorner70
        if (scene.children[i].name.substr(0, 28) === "FistArrowHelper_AddCorner70") {
            scene.remove(scene.children[i]);
        }
        if (scene.children[i].name.substr(0, 30) === "SecontArrowHelper_AddCorner70") {
            scene.remove(scene.children[i]);
        }
        if (scene.children[i].name.substr(0, 20) === "DimLine_AddCorner70") {
            scene.remove(scene.children[i]);
        }
        if (scene.children[i].name.substr(0, 21) === "DimLine_TWDown0CDown1") {
            scene.remove(scene.children[i]);
        }
        if (scene.children[i].name.substr(0, 17) === "Test_AddCorner70") {
            scene.remove(scene.children[i]);
        }
    }
};
function EraseDimensionWall() {
    return;
};
function EraseDimension() {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].typeDim === "Dim") {
            var ob = scene.children[i];
            scene.remove(ob);
        }
    }
};

$("#Open_Test").on("click", function () {
    $("#MenuLeftDesign").hide("slide", { direction: "left" }, 400);
    $("#ViewTest").show("slide", { direction: "left" }, 400);
});
$("#btnClouseTest").on("click", function () {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
});
$("#BtnCloseTest").on("click", function () {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
});
$("#IdShowDim").on("click", function () {
    var Mesh = getallMeshHelp();
    var value = document.getElementById("IdShowDim").checked;
    if (document.getElementById("IdShowDim").checked === true) {
        {
            for (var i = 0; i < scene.children.length; i++) {
                if (scene.children[i].type === "Mesh") {
                    if (scene.children[i].name === "") {
                        continue;
                    }
                }
                if (scene.children[i].MeshTypeWall === "Wall_R000") {
                    var IdWall = scene.children[i].idWall;
                    if (scene.children[i].CHeckDimWall === true) {
                        for (var i2 = 0; i2 < scene.children.length; i2++) {
                            if (scene.children[i2].typeDim !== "Dim") {
                                continue;
                            }
                            var nam = scene.children[i2].name.substr(0, 22);
                            if (scene.children[i2].name.substr(0, 22) === IdWall) {
                                if (scene.children[i2].typeDim === "Dim") {
                                    scene.children[i2].visible = true;
                                }
                                else {
                                    scene.children[i2].visible = false;
                                }
                            }
                        }
                    }

                }

                if (scene.children[i].MeshTypeWall === "Wall_R900") {
                    var IdWall = scene.children[i].idWall;
                    if (scene.children[i].CHeckDimWall === true) {
                        for (var i3 = 0; i3 < scene.children.length; i3++) {
                            if (scene.children[i3].typeDim !== "Dim") {
                                continue;
                            }
                            var nam = scene.children[i3].name.substr(0, 22);
                            if (scene.children[i3].name.substr(0, 22) === IdWall) {
                                if (scene.children[i3].typeDim === "Dim") {
                                    scene.children[i3].visible = true;
                                }
                                else {
                                    scene.children[i3].visible = false;
                                }
                            }
                        }
                    }

                }


                if (scene.children[i].Type === "AddWall_R000") {
                    if (scene.children[i].CHeckDimWall === true) {
                        for (var i2 = 0; i2 < scene.children.length; i2++) {
                            if (scene.children[i2].typeDim === "Dim") {
                                scene.children[i2].visible = true;
                            }
                            else {
                                scene.children[i2].visible = false;
                            }
                        }
                    }

                }
            }
        }
    }
    else {
        for (var i = 0; i < scene.children.length; i++) {

            if (scene.children[i].typeDim === "Dim") {
                scene.children[i].visible = false;
            }
        }
    }
});



$("#IdShowDimAll").on("click", function () {
    var value = document.getElementById("IdShowDimAll").checked;
    if (document.getElementById("IdShowDimAll").checked === true) {
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].typeDim === "Dim") {
                scene.children[i].visible = true;
            }
        }
    }
    else {
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].typeDim === "Dim") {
                scene.children[i].visible = false;
            }
        }
    }
});


$("#View_Wall").on("click", function () {
    ResetSetup();
    document.getElementById("IdShowMuros").checked = false;
    document.getElementById("IdShowDim").checked = false;
    document.getElementById("IdShowDimAll").checked = false;
    $("#btnExportCad").hide("slide", { direction: "right" }, 100);
    document.getElementById("IdShowDim").checked = false;
    EraseDesign();
    EraseDimension();
    EraseDimension();
    EraseDimension();
    EraseDimension();
    EraseDimension();
    EraseDimension();
    EraseDimension();
    ResertDimAndControl();
    InsertWall = 102;
});



$("#IdShowMuros").on("click", function () {
    if (document.getElementById("IdShowMuros").checked === true) {
        AddFaces();
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].typeDim === "DimWall") {
                scene.children[i].visible = true;
            }
        }
    }
    else {
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].typeDim === "DimWall") {
                scene.children[i].visible = false;
            }
        }

        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
        RemoveFaces();
    }
});
 
$("#btnConerDeleteDimensionCorner").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    obWall.material = new THREE.MeshLambertMaterial({ color: 0x839192 });
    $("#EdiCorner").hide("slide", { direction: "right" }, 400);
    scene.remove(obWall);
    scene.remove(obWallMouseMoveSecontObject);
    ResertDimAndControl()
});
$("#btnDeleteDimension").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    obWall.material = new THREE.MeshLambertMaterial({ color: 0x839192 });
    $("#EditDim").hide("slide", { direction: "right" }, 400);

    $("#compassContainer").show("slide", { direction: "right" }, 400);
    $("#EditTape").hide("slide", { direction: "right" }, 400);
    scene.remove(obWall);
    //ResertDimAndControl()
    ResetSetup();
    IsFormArtive = false;

});


$("#MaterialList_filter").on("mouseenter", function () {
    document.getElementById("MaterialList_filter").focus();
});


$("#btnDeleteDimension").on("mouseenter", function () {
    document.getElementById("btnDeleteDimension").focus();
});
function EditDimClouse() {
    IsFormArtive = false;
    if (obWall !== null) {
        obWall.material = new THREE.MeshLambertMaterial({ color: 0xA3A196 });
    }
    ReturnControlsForCamera(camera, 1);
    $("#EditDim").hide("slide", { direction: "right" }, 400);
    $("#EditTape").hide("slide", { direction: "right" }, 400);
    $("#compassContainer").show("slide", { direction: "right" }, 400);
    ResertDimAndControl();
    IsFormArtive = false;
};

$("#DivConfigEnvironment").on("mouseenter", function () {
    ReturnControlsForCamera(camera, 2);
});
$("#DivConfigEnvironment").on("mouseleave", function () {
    ReturnControlsForCamera(camera, 1);
});
$("#DivWallDimension").on("mouseleave", function () {
    UpdateControl();
    document.getElementById("DivWallDimension").style.visibility = "hidden";

});
$("#DivWallDimension").on("mouseenter", function () {
    ReturnControlsForCamera(camera, 2);
})
$("#DataWallDimension").on("click", function () {
    objectsMoveX = [];
    document.getElementById("DataWallDimension").placeholder = "";
    document.getElementById("DataWallDimension").value = "";
});
$("#DataWallDimension").on("change", function () {
    var Value = parseFloat($("#DataWallDimension").val()).toFixed(3);
    var oldValue = obWall.scale.x * 10;
    var valueMove = 0;
    if (oldValue < Value) {
        valueMove = (Value - oldValue);
        obWall.position.x -= valueMove * 100;
    }
    else {
        valueMove = (oldValue - Value);
        obWall.position.x += valueMove * 100;
    }
    obWall.scale.x = $("#DataWallDimension").val() / 10;
    $("#Datalong").val(Value);
});





$("#BtnCloseDimension").on("click", function () {
    EditDimClouse();
    IsFormArtive = false;
    $("#compassContainer").show("slide", { direction: "right" }, 400);
});
$("#BtnClosePilar").on("click", function () {
    if (obWall !== null) {
        obWall.material = new THREE.MeshLambertMaterial({ color: 0xA3A196 });
    }
    $("#EditPilar").hide("slide", { direction: "right" }, 400);
    obWall = null;
    ActionDbl = null;
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
});
$("#btnDeletePilar").on("click", function () {
    IsFormArtive = false;
    ReturnControlsForCamera(camera, 1);
    obWall.material = new THREE.MeshLambertMaterial({ color: 0x839192 });
    $("#EditPilar").hide("slide", { direction: "right" }, 400);
    scene.remove(obWall);
    IsFormArtive = false;
});
$("#btnDeletePilar").on("mouseenter", function () {
    document.getElementById("btnDeletePilar").focus();
});
$('#MenuRight').on('click', function () {
    $("#MenuRightDesign").show("slide", { direction: "right" }, 400);
});
$('#CloseBtnRecuento').on('click', function () {
    IsFormArtive = false;
    $("#ViewMaterialList").hide("slide", { direction: "left" }, 400);
});
$('#lestBtnRecuento').on('click', function () {
    $("#plussBtnRecuento").show();
    $("#lestBtnRecuento").hide();
    $("#MenuTopDesaign").show("slide", { direction: "right" }, 100);
    document.getElementById("ViewMaterialList").style.top = "60px";
    document.getElementById("ViewMaterialList").style.left = "0px";
    $('#ViewMaterialList').css('height', "630px");
    $('#ViewMaterialList').css('width', "1100px");
    $('#DivData').css('width', '1100px');
    $('#MaterialList').css('width', '1100px');

});
$('#plussBtnRecuento').on('click', function () {
    $("#plussBtnRecuento").hide();
    $("#lestBtnRecuento").show();
    $("#MenuTopDesaign").hide("slide", { direction: "right" }, 400);
    var windowH = $(window).height();
    var windowW = $(window).width();
    document.getElementById("ViewMaterialList").style.top = "0px";
    document.getElementById("ViewMaterialList").style.left = "0px";
    $('#ViewMaterialList').css('height', (windowH) + 'px');
    $('#ViewMaterialList').css('width', (windowW) + 'px');
    $('#DivData').css('width', (windowW) + 'px');
    $('#MaterialList').css('width', (windowW) + 'px');
});
//Config
$('#btnConfigStop').on('click', function () {
    IsFormArtive = false;
    $("#DivConfigEnvironment").hide();
    $("#DivConfigWall").hide();
    $("#DivConfigStok").show("slide", { direction: "right" }, 400);
    document.getElementById("btnConfigStop").style.color = "gold";
    document.getElementById("btnConfigWall").style.color = "white";
    document.getElementById("btnConfigEnvironment").style.color = "white";
});
$('#btnConfigWall').on('click', function () {
    IsFormArtive = false;
    $("#DivConfigStok").hide();
    $("#DivConfigEnvironment").hide();
    $("#DivConfigWall").show("slide", { direction: "right" }, 400);
    document.getElementById("btnConfigStop").style.color = "white";
    document.getElementById("btnConfigEnvironment").style.color = "white";
    document.getElementById("btnConfigWall").style.color = "gold";
});
$('#btnConfigEnvironment').on('click', function () {
    IsFormArtive = false;
    $("#DivConfigStok").hide();
    $("#DivConfigWall").hide();
    $("#DivConfigEnvironment").show("slide", { direction: "right" }, 400);
    document.getElementById("btnConfigEnvironment").style.color = "gold";
    document.getElementById("btnConfigStop").style.color = "white";
    document.getElementById("btnConfigWall").style.color = "white";
});
$('#CloseBtnMenu').on('click', function () {
    IsFormArtive = false;
    $("#MenuRightDesign").hide("slide", { direction: "right" }, 400);
});
$('#MenuLeft').on('click', function () {

    $("#MenuLeftDesign").show("slide", { direction: "left" }, 400);
});
$('#Menubottom').on('click', function () {
    $("#EditDim").hide("slide", { direction: "right" }, 400);
    document.getElementById("IdShowMuros").checked = false;
    ResetSetup();
    $("#EdiCorner_10").hide("slide", { direction: "left" }, 400);
    $("#EdiCorner_30").hide("slide", { direction: "left" }, 400);
    $("#EdiCorner_50").hide("slide", { direction: "left" }, 400);
    $("#EdiCorner_70").hide("slide", { direction: "left" }, 400);
    $("#MenubottomDesign").show("slide", { direction: "left" }, 400);
    controls = ReturnControlsForCamera(camera, 2);

});
$('#CloseMenubottom').on('click', function () {
    IsFormArtive = false;
    $("#MenubottomDesign").hide("slide", { direction: "left" }, 400);
    controls = ReturnControlsForCamera(camera, 1);
});
$('#CloseBtnMenuLeft').on('click', function () {
    IsFormArtive = false;
    $("#MenuLeftDesign").hide("slide", { direction: "left" }, 400);
});
$("#BtnCloseConfig").on("click", function () {
    IsFormArtive = false;
    $("#ViewConfig").hide("slide", 400);
});
$("#btnConfig").on("click", function () {
    $("#ViewConfig").show("slide", 400);
});
$("#btWizarNucleo").on("click", function () {
    $("#MenubottomDesign").hide("slide", 200);
    controls = ReturnControlsForCamera(camera, 1);
    $("#DivNucleo").show("slide", 400);
});
$("#CloseDivNucleo").on("click", function () {
    $("#DivNucleo").hide("slide", 300);
});
//Help
$("#IdHelp").on("click", function () {
    if (document.getElementById("IdHelp").checked === true) {
        $("#LabelHelp").show();
        $("#IdHelpVideoConten").show();
        $("#IdHelpImgConten").show();
    }
    else {
        $("#LabelHelp").hide();
        $("#IdHelpVideoConten").hide();
        $("#IdHelpImgConten").hide();
    }
});

$("#IdHelpVideo").on("click", function () {
    if (document.getElementById("IdHelpVideo").checked === true) {
        document.getElementById("IdHelpImg").checked = false;
    }
    else {
        document.getElementById("IdHelpImg").checked = true;
    }
});
$("#IdHelpImg").on("click", function () {
    if (document.getElementById("IdHelpImg").checked === true) {
        document.getElementById("IdHelpVideo").checked = false;
    }
    else {
        document.getElementById("IdHelpVideo").checked = true;
    }
});



//Wall
$("#Insert_Wall").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    InsertWall = 1;
})

$("#Insert_Wall_D").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    InsertWall = 111;
})

$("#Insert_Paralelas").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    ActionWizard = 500;
})
//Conexion_T

$("#Insert_Wall_90").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    InsertWall = 2;
})
$("#Insert_Wall_90_D").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    InsertWall = 222;
})

$("#Insert_Pilar").on('click', function (e) {
    ResetSetup();
    rollOverMesh.visible = true;
    IsFormArtive = false;
    ActionDbl = null;
    InsertWall = 14;
})
$("#Insert_Wall_m1").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner10";
    InsertWall = 10;
})
$("#Insert_Wall_m3").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner30";
    InsertWall = 30;
})
$("#Insert_Wall_m7").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner70";
    InsertWall = 70;
})
$("#Insert_Wall_m5").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner50";
    InsertWall = 50;
})
$("#Insert_Wall_m60").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner60";
    InsertWall = 60;
})
$("#Insert_Wall_m20").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner20";
    InsertWall = 20;
})
$("#Insert_Wall_m80").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner80";
    InsertWall = 80;
})
$("#Insert_Wall_X").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCornerX";
    InsertWall = 15;
})
$("#Insert_Wall_m40").on('click', function (e) {
    ResetSetup();
    ActiveAddCorner = "AddCorner40";
    InsertWall = 40;
})
$("#Insert_Worker").on('click', function (e) {
    ResetSetup();
    rollOverMesh.visible = true;
    InsertWall = 5;
})
$("#Insert_Puntal").on('click', function (e) {
    ResetSetup();
    InsertWall = 200;
})
$("#Insert_Cruce").on('click', function (e) {
    CleanEsqAndCruce();
    var obOld = null;
    InsertWall = 55;
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name.substr(0, 5) === "Wall_") {
                /*    camera.controls.update();*/
                obWall = scene.children[i];
                var _longWall = obWall.scale.x * 10;
                var _heightWall = obWall.scale.z * 10;
                var _widthWall = obWall.scale.y * 10;
                var x = obWall.position.x + ((_longWall / 2) * 100);
                var y = obWall.position.z;
                var z = obWall.position.y + ((_heightWall / 2) * 100);
                NameWall = obWall.name;
                obWall.material = new THREE.MeshBasicMaterial({ color: 0x839192, opacity: 0.5, transparent: true });
                CreateFacesWall(_longWall, x, y, z, _heightWall, NameWall, _widthWall, "Face_Wall_From");
                CreateFacesWall(_longWall, x, y - _widthWall * 100, z, _heightWall, NameWall, _widthWall, "Face_Wall_Back");
            }
        }
    }
});
$("#Insert_Esq").on('click', function (e) {
    CleanEsqAndCruce();
    InsertWall = 56;
});
function ResertDimAndControl() {
    Edit_Wall = 1;
    ActionDbl = null;
    obWallMouseMove = null;
    obWallMouseMoveSecontObject = null;
    obWall = null;
    ActionDbl = null;
    EraseDimensionWall();
    EraseDimensionWall();
    EraseDimensionWall();
    Edit_Wall = 1;

}
$('#btnModulos').on('click', function () {
    IsFormArtive = false;
    $("#TapCorner").hide();
    $("#TapCombinados").hide();
    $("#TapModulos").show("slide", { direction: "right" }, 400);
    document.getElementById("btnModulos").style.color = "gold";
    document.getElementById("btnModulosCombinados").style.color = "white";
    document.getElementById("btnCorner").style.color = "white";
});
$('#btnModulosCombinados').on('click', function () {
    IsFormArtive = false;
    $("#TapCorner").hide();
    $("#TapModulos").hide();
    $("#TapCombinados").show("slide", { direction: "right" }, 400);
    document.getElementById("btnCorner").style.color = "white";
    document.getElementById("btnModulos").style.color = "white";
    document.getElementById("btnModulosCombinados").style.color = "gold";
});
$('#btnCorner').on('click', function () {
    IsFormArtive = false;
    $("#TapModulos").hide();
    $("#TapCombinados").hide();
    $("#TapCorner").show("slide", { direction: "right" }, 400);
    document.getElementById("btnCorner").style.color = "gold";
    document.getElementById("btnModulos").style.color = "white";
    document.getElementById("btnModulosCombinados").style.color = "white";
});
$("#IdUndo").on("click", function () {
    ResetSetup();
    var l = getallMesh();
    var lMest = getallMesh();
    var j = _ListUndo_Redo;
    var jl1 = _ListUndo_Redo.sort();
    var j2 = _ListUndo_Redo.filter(function (element) { return element.IdUndoRedo >= IdUndoRedo; })
    var IdRedoUndo = IdRedoUndo + 1;
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].IdUndoRedo === IdUndoRedo) {
            scene.remove(scene.children[i]);
            _ListRedo_Undo.push({
                Type: scene.children[i].Type,
                x: scene.children[i].x,
                y: scene.children[i].y,
                _longWall: scene.children[i]._longWall,
                _widthWall: scene.children[i]._widthWall,
                _heightWall: scene.children[i]._heightWall,
                TypeWall: scene.children[i].TypeWall,
                IdWall: scene.children[i].IdWall,
                IdWall_0: scene.children[i].IdWall_0,
                Sub_Long_0: scene.children[i].Sub_Long_0,
                Sub_Long_180: scene.children[i].Sub_Long_180,
                IdWall_180: scene.children[i].IdWall_180,
                IdWall_90: scene.children[i].IdWall_90,
                Sub_Long_90: scene.children[i].Sub_Long_90,
                IdWall_270: scene.children[i].IdWall_270,
                Sub_Long_270: scene.children[i].Sub_Long_270,
                MeshActive: true,
                IdUndoRedo: scene.children[i].IdRedoUndo,
            });
        }
    }
    for (var i2 = 0; i2 < scene.children.length; i2++) {
        if (scene.children[i2].IdUndoRedo === IdUndoRedo) {
            scene.remove(scene.children[i2]);
            _ListRedo_Undo.push({
                Type: scene.children[i2].Type,
                x: scene.children[i2].x,
                y: scene.children[i2].y,
                _longWall: scene.children[i2]._longWall,
                _widthWall: scene.children[i2]._widthWall,
                _heightWall: scene.children[i2]._heightWall,
                TypeWall: scene.children[i2].TypeWall,
                IdWall: scene.children[i2].IdWall,
                IdWall_0: scene.children[i2].IdWall_0,
                Sub_Long_0: scene.children[i2].Sub_Long_0,
                Sub_Long_180: scene.children[i2].Sub_Long_180,
                IdWall_180: scene.children[i2].IdWall_180,
                IdWall_90: scene.children[i2].IdWall_90,
                Sub_Long_90: scene.children[i2].Sub_Long_90,
                IdWall_270: scene.children[i2].IdWall_270,
                Sub_Long_270: scene.children[i2].Sub_Long_270,
                MeshActive: true,
                IdUndoRedo: scene.children[i2].IdRedoUndo,
            });
        }
    }


    for (var i3 = 0; i3 < scene.children.length; i3++) {
        if (scene.children[i3].IdUndoRedo === IdUndoRedo) {
            scene.remove(scene.children[i3]);
            _ListRedo_Undo.push({
                Type: scene.children[i3].Type,
                x: scene.children[i3].x,
                y: scene.children[i3].y,
                _longWall: scene.children[i3]._longWall,
                _widthWall: scene.children[i3]._widthWall,
                _heightWall: scene.children[i3]._heightWall,
                TypeWall: scene.children[i3].TypeWall,
                IdWall: scene.children[i3].IdWall,
                IdWall_0: scene.children[i3].IdWall_0,
                Sub_Long_0: scene.children[i3].Sub_Long_0,
                Sub_Long_180: scene.children[i3].Sub_Long_180,
                IdWall_180: scene.children[i3].IdWall_180,
                IdWall_90: scene.children[i3].IdWall_90,
                Sub_Long_90: scene.children[i3].Sub_Long_90,
                IdWall_270: scene.children[i3].IdWall_270,
                Sub_Long_270: scene.children[i3].Sub_Long_270,
                MeshActive: true,
                IdUndoRedo: scene.children[i3].IdRedoUndo,
            });
        }
    }
    IdUndoRedo = IdUndoRedo - 1;
});
$("#IdRedo").on("click", function () {
});

$("#Insert_Conexion").on('click', function (e) {
    ResetSetup();
    IsFormArtive = false;
    ActionDbl = null;
    ActionWizard = 600;
    if (document.getElementById("IdHelp").checked === true) {
        if (document.getElementById("IdHelpVideo").checked === true) {
            $("#ToasMesaje").show("slide", { direction: "right" }, 400);
            videoElem = document.getElementById("Video_1_Conexion");
            videoElem.src = "../../Content/DesignTools/Help/Ayuda_Conexion_1.mp4";
            videoElem.play();
        }
        else {
            $("#ToasMesajeImg").show("slide", { direction: "right" }, 400);
        }
    }
})

$("#Insert_Grill").on("click", function () {
    $("#MenubottomDesign").hide("slide", 200);
    controls = ReturnControlsForCamera(camera, 1);
    $("#DivGrill").show("slide", 400);
});


$("#CloseDivDivGrill").on("click", function () {
    $("#DivGrill").hide("slide", { direction: "left" }, 400);
});




$("#btAddGrillVertical").on("click", function () {
    AddGrill_90(false);
});

$("#btAddGrillHorizontal").on("click", function () {
    AddGrill_0(false);
});

$("#GrilVertical").on("mouseenter", function () {
    document.getElementById("GrilVertical").focus();
});

$("#GrilHorizontal").on("mouseenter", function () {
    document.getElementById("GrilHorizontal").focus();
});

function AddGrill_90(Mesh) {
    var valueText = parseFloat($("#GrilVertical").val());
    if (Mesh !== false) {
        valueText = Mesh.position.x / 100;
    }
    var Value = valueText * 100;
    InsertWall = 0;
    let longWall = 800;
    var widthWall = 0.003;
    var heightWall = 0.001;
    var IdpartName = new Date().valueOf();
    var IdWall = "Grill_900" + IdpartName;
    var IdCono = "Cono" + IdpartName;
    var Position_Y = (longWall * 1000) * -1;
    AddGrill_R900(Value, Position_Y + 8000, longWall, widthWall, heightWall, "Grill_900", IdWall);

    var geometryGrillConoRight = new THREE.ConeGeometry(5, 20, 32);
    ConeGrillVertical = new THREE.Mesh(geometryGrillConoRight, materialDimWall);
    ConeGrillVertical.scale.x = 3;
    ConeGrillVertical.scale.y = 3;
    ConeGrillVertical.rotation.x = Math.PI * 1.5;
    ConeGrillVertical.rotation.z = Math.PI * 1.5;
    ConeGrillVertical.position.x = (Value - 30);
    ConeGrillVertical.position.y = 1;
    ConeGrillVertical.position.z = - 100;
    ConeGrillVertical.name = "IdCono";
    ConeGrillVertical.visible = true;
    scene.add(ConeGrillVertical);
    NameTextDim = valueText.toFixed(3);
    AddDimTextGrill(NameTextDim, Value - 150, - 150);
};

function AddGrill_0(Mesh) {
    var valueText = parseFloat($("#GrilHorizontal").val());
    if (Mesh !== false) {
        valueText = Mesh.position.z / 100;
        valueText = valueText * -1;
    }


    var Value = (valueText * 100) * -1;
    InsertWall = 0;
    let longWall = 800;
    var widthWall = 0.003;
    var heightWall = 0.001;
    var IdpartName = new Date().valueOf();
    var IdWall = "Grill_000" + IdpartName;
    var IdCono = "Cono" + IdpartName;
    AddGrill_R000(0 - 8000, Value, longWall, widthWall, heightWall, "Grill_000", IdWall);
    var geometryGrillConoTop = new THREE.ConeGeometry(5, 20, 32);
    ConeGrillHorizontal = new THREE.Mesh(geometryGrillConoTop, materialDimWall);
    ConeGrillHorizontal.scale.x = 3;
    ConeGrillHorizontal.scale.y = 3;
    ConeGrillHorizontal.rotation.x = - Math.PI * 1.5;
    ConeGrillHorizontal.rotation.z = - Math.PI;
    ConeGrillHorizontal.position.x = + 100
    ConeGrillHorizontal.position.y = 1;
    ConeGrillHorizontal.position.z = (Value + 30);
    ConeGrillHorizontal.name = "IdCono";
    ConeGrillHorizontal.visible = true;
    scene.add(ConeGrillHorizontal);
    NameTextDim = valueText.toFixed(3);
    AddDimTextGrill(NameTextDim, 150, Value + 150);
};


function CloseFormEdit() {
    $("#EdiCorner_10").hide("slide", { direction: "right" }, 400);
    $("#EdiCorner_30").hide("slide", { direction: "right" }, 400);
    $("#EdiCorner_50").hide("slide", { direction: "right" }, 400);
    $("#EdiCorner_70").hide("slide", { direction: "right" }, 400);
    $("#EditDim").hide("slide", { direction: "right" }, 400);
    ReturnControlsForCamera(camera, 1);
    $("#EditPilar").hide("slide", { direction: "right" }, 400);
};
function CheckConectión(Conexion_0, Conexion_90, Conexion_180, Conexion_270)
{
    if (Conexion_180 !== null && Conexion_0 !== null || Conexion_90 !== null && Conexion_270) {
        // bloqueo y estilos de longitud
        document.getElementById("Datalong").disabled = true;
        //document.getElementById("wallEditionNoticeLong").style.display = "inline";
        document.getElementById("Datalong").style.border = "0px solid #ffe000";
        document.getElementById("Datalong").style.opacity = "0.5";
        document.getElementById("Datalong").style.pointerEvents = "none";
        // bloqueo y estilos anchura 
        document.getElementById("DataWith").disabled = true;
        //document.getElementById("wallEditionNoticeWith").style.display = "inline";
        document.getElementById("DataWith").style.border = "0px solid #ffe000";
        document.getElementById("DataWith").style.opacity = "0.5";
        document.getElementById("DataWith").style.pointerEvents = "none";
        // bloqueo y estilos cordenada X
        document.getElementById("DataCordenadX").disabled = true;
        document.getElementById("DataCordenadX").style.border = "0px solid #ffe000";
        document.getElementById("DataCordenadX").style.opacity = "0.5";
        document.getElementById("DataCordenadX").style.pointerEvents = "none";
        // bloqueo y estilos cordenada Y
        document.getElementById("DataCordenadY").disabled = true;
        document.getElementById("DataCordenadY").style.border = "0px solid #ffe000";
        document.getElementById("DataCordenadY").style.opacity = "0.5";
        document.getElementById("DataCordenadY").style.pointerEvents = "none";
        document.getElementById("TapTape").style.display = "none";
        document.getElementById("TapPuntal").style.display = "none";
    }
    if (Conexion_0 !== null || Conexion_180 !== null) {
        // bloqueo y estilos anchura 
        document.getElementById("DataWith").disabled = true;
        //document.getElementById("wallEditionNoticeWith").style.display = "inline";
        document.getElementById("DataWith").style.border = "0px solid #ffe000";
        document.getElementById("DataWith").style.opacity = "0.5";
        document.getElementById("DataWith").style.pointerEvents = "none";
    }
    
};

function getValorbydefect() {
    // longitud
    document.getElementById("Datalong").disabled = false;
    document.getElementById("Datalong").style.border = "1px solid #ffe000";
    document.getElementById("Datalong").style.borderLeft = "4px solid #ffe000";
    // anchura
    document.getElementById("DataWith").disabled = false;
    document.getElementById("DataWith").style.border = "1px solid #ffe000";
    document.getElementById("DataWith").style.borderLeft = "4px solid #ffe000";
    //cordenada X
    document.getElementById("DataCordenadX").disabled = false;
    document.getElementById("DataCordenadX").style.border = "1px solid #ffe000";
    document.getElementById("DataCordenadX").style.borderLeft = "4px solid #ffe000";
    // Cordenada Y
    document.getElementById("DataCordenadY").disabled = false;
    document.getElementById("DataCordenadY").style.border = "1px solid #ffe000";
    document.getElementById("DataCordenadY").style.borderLeft = "4px solid #ffe000";
}
