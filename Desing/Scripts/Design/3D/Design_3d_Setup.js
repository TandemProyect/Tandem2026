//Tabulador Grill
var ChangeConection = false;
var ChangeConectionPosition = 0;
//Print

//Ayuda
var CurrentInsertWall = null;
let videoElem = "";
/*let control = null;*/
//Conexiones

var Wall_Conexion_1 = null;
var Wall_Conexion_2 = null;
//Nucleo


//Wizard
var ActionWizard = 0;
var ParallePositionY = null;
var ParallePositionX = null;
var ObParalle = null;
//Conexion
var FirstWallConexion = null
var SecontWallConexion = null;
var ValueNewWall = 0;

//Dim
var TestDimVertical = [];
var currentPosition = new Position(
    x = 0,
    y = 0,
    z = 0);
var currentTarget = new Position(
    x = 0,
    y = 0,
    z = 0);
// Setup
var IdUndoRedo = 1;
var IdRedoUndo = 0;
var _ListRedo_Undo = []
var _ListUndo_Redo = [];
var KeyActive = false;
var OrbitDesign = 1;
var IsMiddleOfConnecting = false;
var IsMiddleOfConnectingX = false;
var evt = null;

//Event
document.addEventListener('contextmenu', (event) => {
    if (event.button === 2) {
        ResetControl();
        if (ActionWizard !== 500) {
            if (ActionDbl === "Wall_R000") {
                obWall = obWallMouseMove;
                obWallScaleX = (obWall.scale.x * 10).toFixed(3);
                obWallScaleY = (obWall.scale.y * 10).toFixed(3);
                obWallScaleZ = (obWall.scale.z * 10).toFixed(3);
                let _dinText = parseFloat(obWallScaleX).toFixed(3);
                if (obWall.IdWall_180.trim() === '0') {
                    WallAddControl_180();
                }
                if (obWall.IdWall_0.trim() === '0') {
                    WallAddControl_0();
                }

            }
            if (ActionDbl === "Wall_R900") {
                obWall = obWallMouseMove;
                if (obWall != null) {
                    obWallScaleX = (obWall.scale.x * 10).toFixed(3);
                    obWallScaleY = (obWall.scale.y * 10).toFixed(3);
                    obWallScaleZ = (obWall.scale.z * 10).toFixed(3);
                    let _dinText = parseFloat(obWallScaleX).toFixed(3);

                    if (obWall.IdWall_90.trim() === '0') {
                        WallAddControl_90();
                    }
                    if (obWall.IdWall_270.trim() === '0') {
                        WallAddControl_270();
                    }
                }
            }
        }
    }
})


 



document.onkeyup = function (evt) {
    if (evt.keyCode === 32) {
        ResetGrill();
    }
}
document.onkeydown = function (evt) {
    if (evt.keyCode === 32) {

        CurrentInsertWall = InsertWall;
        isYColision = false;
        isXColision = false;
        //DimHorizontal
        ConeRight.visible = false;
        ConeLeft.visible = false;
        LineDimLef.visible = false;
        LineRightToLeft.visible = false;
        LineDimRight.visible = false;
        TextDim.visible = false;
        _dim.visible = false;

        //DimVertical
        ConeTop.visible = false;
        LineDimTop.visible = false;
        ConeDown.visible = false;
        LineDimDown.visible = false;
        LineDimTopToRDown.visible = false;
    }
    if (evt.keyCode === 27) {
        document.getElementById("IdShowMuros").checked = false;
        CurrentInsertWall = null;
        ResetSetup();
    }
    if (evt.keyCode === 13) {
        //if (IsMiddleOfConnecting === true) {
        //    CreateConection90x0(obd, obwalld, ValueNewWall);
        //}
        if (ActionDbl === "Control_Move_270") {
            var value = ($("#InputDim").val() / 10).toFixed(3);
            var value2 = obWall.scale.z.toFixed(3);
            if (value !== value2) {
                var Difz = (value - value2) * 1000;
                obWall.scale.z = $("#InputDim").val() / 10;
                AddDimWall_90(obWall);
            }
        }
        if (ActionDbl === "Control_Move_90") {
            var value = ($("#InputDim").val() / 10).toFixed(3);
            var value2 = obWall.scale.z.toFixed(3);
            if (value !== value2) {
                var Difz = (value - value2) * 1000;
                obWall.scale.z = $("#InputDim").val() / 10;
                obWall.position.z = obWall.position.z - Difz;
                AddDimWall_90(obWall);
            }
        }
        if (ActionDbl === "Control_Move_0") {
            obWall.scale.x = $("#InputDim").val() / 10;
            AddDimWall_0(obWall);
        }
        if (ActionDbl === "Control_Move_180") {
            var value = ($("#InputDim").val() / 10).toFixed(3);
            var value2 = obWall.scale.x.toFixed(3);
            if (value !== value2) {
                var Difz = (value - value2) * 1000;
                obWall.scale.x = $("#InputDim").val() / 10;
                obWall.position.x = obWall.position.x - Difz;
                AddDimWall_0(obWall);
            }
        }
        if (ActionDbl === "Control_Move_Esq_60") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCorner60(value);
        }
        if (ActionDbl === "Control_Move_Esq_80") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCorner80(value);
        }
        if (ActionDbl === "Control_Move_Esq_X") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCornerX(value);
        }
        if (ActionDbl === "Control_Move_Parall") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCornerParall(value, true);
        }

        if (ActionDbl === "Control_Move_Parall_90") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCornerParall_90(value, true);
        }



        if (ActionDbl === "Control_Move_Esq_40") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCorner40(value);
        }

        if (ActionDbl === "Control_Move_Esq_20") {
            controls = ReturnControlsForCamera(camera, 2);
            var value = ($("#InputDim").val() / 10).toFixed(3);
            document.getElementById("DivInputDim").style.display = "none";
            AddCorner20(value);
        }
        controls = ReturnControlsForCamera(camera, 1)
        KeyActive = false;
        AddDivDim = true;
        ResetSetup();
    }
    if (evt.keyCode === 17) {
        if (ChangeConection === true)
        {
            /*if (ChangeConectionPosition === -30) { ChangeConectionPosition = 0; } */
            switch (ChangeConectionPosition) { 
                case 0:
                    ChangeConectionPosition = 15;
                    break;
                case 15:
                    ChangeConectionPosition = 30;
                    break;
                case 30:
                    ChangeConectionPosition = 0;
                    break;
                default:
                    break;
            }
         }

        AddDivDim = false;
        KeyActive = true;
        controls = ReturnControlsForCamera(camera, 2)
        document.getElementById("InputDim").focus();
    }
    if (evt.keyCode === 17) {
        if (ChangeConection === true) {
            switch (ChangeConectionPosition) {
                case 0:
                    ChangeConectionPosition = -15;
                    break;
                case -15:
                    ChangeConectionPosition = -30;
                    break;
                case -30:
                    ChangeConectionPosition = 0;
                    break;
                default:
                    break;
            }
            
        }

        AddDivDim = false;
        KeyActive = true;
        controls = ReturnControlsForCamera(camera, 2);
        document.getElementById("InputDim").focus();
    }
};
function ResetGrill() {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].MeshTypeWall === "Grill_900") {
                scene.children[i].material = materialGrill;
            }
            if (scene.children[i].MeshTypeWall === "Grill_000") {
                scene.children[i].material = materialGrill;
            }

        }
    }
}
function ResetControl() {
    meshControl_Move_180.visible = false;
    meshControl_Move_180.material = MaterialUnSelectIcon;
    meshControl_Move_0.visible = false;
    meshControl_Move_0.material = MaterialUnSelectIcon;
    meshControl_Move_90.visible = false;
    meshControl_Move_90.material = MaterialUnSelectIcon;
    meshControl_Move_270.visible = false;
    meshControl_Move_270.material = MaterialUnSelectIcon;
    meshControl_Move_180.position.y = 0;
    meshControl_Move_0.position.y = 0;
    meshControl_Move_90.position.y = 0;
    meshControl_Move_270.position.y = 0;
}

function ResetView() {
    meshWall_0.visible = false;
    meshWall_90.visible = false;
    meshEsq20.visible = false;
    meshEsq20Conexion.visible = false;
    meshEsqXConexion.visible = false;
    meshEsq10.visible = false;
    meshNucleo.visible = false;
    meshEsq30.visible = false;
    meshEsq70.visible = false;
    meshEsq50.visible = false;
    meshEsq40.visible = false;
    meshEsq60.visible = false;
    meshEsq80.visible = false;
    meshEsqX.visible = false;
    meshParall.visible = false;
    meshParall90.visible = false;
    InsertWall = CurrentInsertWall;
    switch (InsertWall) {
        case 1:
            meshWall_0.visible = true;
        case 102:
            meshParall.visible = true;
            break;
        case 2:
            meshWall_90.visible = true;
            break;
        case 15:
            meshEsqX.visible = true;
            break;

        case 10:
            meshEsq10.visible = true;
            break;
        case 20:
            meshEsq20.visible = true;
            break;
        case 30:
            meshEsq30.visible = true;
            break;
        case 40:
            meshEsq40.visible = true;
            break;
        case 50:
            meshEsq50.visible = true;
            break;
        case 60:
            meshEsq60.visible = true;
            break;
        case 70:
            meshEsq70.visible = true;
            break;
        case 80:
            meshEsq80.visible = true;
            break;
        default:
            break;
    }
}
function ResetSetup(v) {
    if (OtherCornerObject === undefined) {
        OtherCornerObject = null;
    }
    if (v !== false) {
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
        RemoveFaces();
    }
    ResetGrill();
    ChangeConection = false;
    ChangeConectionPosition = 0;
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ObParalle = null;
    ActionWizard = 0;
    ParallePositionY = null;
    ParallePositionX = null;
    renderer.domElement.style.cursor = "auto";
    IsMiddleOfConnecting = false;
    IsMiddleOfConnectingX = false;
    FirstWallConexion = null
    SecontWallConexion = null;
    ValueNewWall = 0;
    TypeConetion = "";
    //Control
    AddDivDim = false;
    $("#InputDim").val()
    controls = ReturnControlsForCamera(camera, 1);
    document.getElementById("DivInputDim").style.display = "none";
    ActionDbl = "";
    IconActive_180_90 = false;
    IconActive_0_270 = false;
    ImDraw = false
    meshControl_Move_180.position.y = 0;
    meshControl_Move_180.visible = false;
    meshControl_Move_180.material = MaterialUnSelectIcon;
    meshControl_Move_0.position.y = 0;
    meshControl_Move_0.visible = false;
    meshControl_Move_0.material = MaterialUnSelectIcon;
    meshControl_Move_90.position.y = 0;
    meshControl_Move_90.visible = false;
    meshControl_Move_90.material = MaterialUnSelectIcon;
    meshControl_Move_270.position.y = 0;
    meshControl_Move_270.visible = false;
    meshControl_Move_270.material = MaterialUnSelectIcon;
    meshWall_0.visible = false;
    meshWall_90.visible = false;
    meshEsq20.visible = false;
    meshEsq20Conexion.visible = false;
    meshEsqXConexion.visible = false;
    meshEsq10.visible = false;
    meshNucleo.visible = false;
    meshEsq30.visible = false;
    meshEsq70.visible = false;
    meshEsq50.visible = false;
    meshEsq40.visible = false;
    meshEsq60.visible = false;
    meshEsq80.visible = false;
    meshEsqX.visible = false;
    meshParall.visible = false;
    meshParall90.visible = false;
    isYColision = false;
    isXColision = false;
    yColision = 0;
    if (obWallMouseMove !== null) {
        obWallMouseMove.material = materialWall;
    }
    if (OtherCornerObject !== null) {

        OtherCornerObject.material = materialWall;
    }
    obWall = null;
    obWallMouseMove = null;

    //DimHorizontal
    ConeRight.visible = false;
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    TextDim.visible = false;
    _dim.visible = false;

    //DimVertical
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    InsertWall = 102;
    IsFormArtive = false;
}

$('#sidebar').toggleClass('active');
$(this).toggleClass('active');
$("#leftMenu").hide();
$("#View").hide();
//Test 
var _listWalls = [];
var testActive = false;
var mouse = null;
//
var IdNameTemporal = "";
var ListWalls = [];
var LinkEnvironment = 2;
//MenuWall
var MenuWallActive = "_TapMuro";
// CornerLeftTop
var obWallX = null;
var obWallY = null;
var obEsqX = null;
var obEsqY = null;
var IsFormArtive = false;
var obWall = null;
var obWallMouseMove = null;
var OtherCornerObject = null;
var obWallMouseMoveSecontObject = null;
var obWallMouseMoveOldMaterial = null;
var obWallActive = null;
var materialUnion1 = null;
var materialBase = null;
var materialDim = null;
var materialDimWall = null;
var materialIcon = null;
var materialBaseShow = null;
var SelectMaterial = null;
SelectMaterialConexion_1 = null;
SelectMaterialConexion_2 = null;

var EraseMaterial = null;
var materialEsq = null;
var materialSup = null;
var materialWallAct = null;
var materialWall = null;
var materialGrill = null;
var materialGrillAct = null;
var MaterialUnSelectIcon = null
var MaterialSelectIcon = null
//wall
//Draw
//Control 

//Object
var AddDivDim = false;


var ObPosibleWallXDownRight = null;
var ObPosibleWallXTopLeft = null;
var ObPosibleWallXDownLeft = null;
var ObPosibleWallXFrom = null;
var ObDinTest = null;


//TWDown0CDown1
var ObDimLine_AddCorner70 = null
var ObText_AddCorner70 = null;
var ObFistArrowHelper_AddCorner70 = null;
var ObSecontArrowHelper_AddCorner70 = null;
var ObDimLine_TWDown0CDown1 = null;


//Desaing three
//Cube
let Tower;
var controls = null;
var controlsorthographic = null;
var cameraTypeId = 1;
let perspectiveCamera = null;
let ortogonalCamera = null;
let rollOverMesh, rollOverMaterial;
//Corner
let ActiveAddCorner = "";
let isYColision = false;
let isXColision = false;
let xColision = 0;
let yColision = 0;
let meshEsq10;
let meshNucleo;

let meshEsq30;
let meshEsq70;
let meshEsq50;
let meshEsq20;
let meshEsq20Conexion;
let meshEsqXConexion;
let meshEsq40;
let meshEsq60;
let meshEsq80;
let meshEsqX;
let meshParall;
let meshParall90;

//Dim v
var geometryConoTop;
var ConeTop;

var geometryConoDown;
var ConeDown;

var pointsDimTop = [];
var LineDimTop = null;

var pointsDimDown = [];
var LineDimDown = null;

var pointsDimTopToRDown = [];
var LineDimTopToRDown = null;




//Dim h 
var size = 256;
var radius = 1;
var geometryConoLeft;
var ConeLeft;

var geometryConoRight;
var ConeRight;

var pointsDimLef = [];
var LineDimLef = null;

var pointsDimRight = [];
var LineDimRight = null;

var pointsDimLefToRight = [];
var LineDimLefToRight = null;

var TypeConetion = "";
//Controls move
var meshFonds = null;
var geometryFond = "";
var ImDraw = false;
var IconActive_180_90 = false;
var IconActive_0_270 = false;
let meshControl_Move_180;
let meshControl_Move_0;

let meshControl_Move_90;
let meshControl_Move_270;


var _dim;
var TextDim;
var NameTextDim = "";

let meshCtrMXStr;
let meshCtrMXEnd;

let meshWall_0;
let cubeGeo, cubeMaterial;
const objects = [];
var screenWidth = screen.width;
var screenHeight = screen.height;
var windowWidth = window.innerWidth;
var windowHeight = window.innerHeight - 160;

let mouseWall;
var buildStarted = false;
var ActualControl = "";
var Control_S_x = false;
var Control_Add = false;
var ExistWall = 0;
var InsertWall = 0;
var Edit_Wall = 0;
// Move
var rollOverGeo = null;
var ActionDbl = "";
var objectsMoveX = [];
var objectsMoveZ = [];
var objectsMoveXEnd = [];
var obcontrolx = null;
var obcontroladd = null;
/*document.getElementById('Top_Menu').style.display = "none";*/
var ob = null;
var renderer, scene, camera, controls, control, world;
/*var renderer2, background;*/
var material, mesh, canvas,/* rayCaster,*/ mousePosition;
let meshes = [];
var controledObject;
let isShiftDown = false;
function Position(_x, _y, _z) {
    this.x = _x;
    this.y = _y;
    this.z = _z;
}

