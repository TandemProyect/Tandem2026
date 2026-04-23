 function WallAddControl_90() {
    //if (IconActive_180_90 === true) {
    //    return;
    //}
    if (obWall === null) {
        obWall = obWallMouseMove;
    }
    document.getElementById("DivInputDim").style.display = "none";
    ActionDbl = "";
    var obWallScalex = obWall.scale.x * 10;
    objectsMoveZ.pop();
    //if (obWall.IdWall_180 !== "0") { return; }
    var YPosition = obWall.position.z ;
    var XPosition = obWall.position.x - (obWallScalex * 100) / 2;
    meshControl_Move_90.position.x = XPosition;
    meshControl_Move_90.position.z = YPosition;
    objectsMoveZ.push(meshControl_Move_90);
    meshControl_Move_90.visible = true;
/*    IconActive_180_90 = true;*/
    InsertWall = 102;
};
function WallAddControl_270() {
    //if (IconActive_180_90 === true) {
    //    return;
    //}
    if (obWall === null) {
        obWall = obWallMouseMove;
    }
    document.getElementById("DivInputDim").style.display = "none";
    ActionDbl = "";
    var obWallScaley = obWall.scale.z * 10;
    var obWallScalex = obWall.scale.x * 10;
    objectsMoveZ.pop();
    //if (obWall.IdWall_180 !== "0") { return; }
    var YPosition = obWall.position.z + (obWallScaley * 100);
    var XPosition = obWall.position.x - (obWallScalex * 100) / 2;
    meshControl_Move_270.position.x = XPosition;
    meshControl_Move_270.position.z = YPosition;
    objectsMoveZ.push(meshControl_Move_270);
    meshControl_Move_270.visible = true;
    InsertWall = 102;
};
function WallAddControl_180() {
    //if (IconActive_180_90 === true) {
    //    return;
    //}
    if (obWall === null) {
        obWall = obWallMouseMove;
    }
    document.getElementById("DivInputDim").style.display = "none";
    ActionDbl = "";
    var obWallScaleY = obWall.scale.y * 10;
    objectsMoveXEnd.pop();
    if (obWall.IdWall_180 !== "0") { return; }
    var YPosition = obWall.position.z - (obWallScaleY * 100) / 2;
    var XPosition = obWall.position.x;
    meshControl_Move_180.position.x = XPosition;
    meshControl_Move_180.position.z = YPosition;
    meshControl_Move_180.position.y = 0;
    objectsMoveXEnd.push(meshControl_Move_180);
    meshControl_Move_180.visible = true;
    IconActive_180_90 = true;
    InsertWall = 102;
};
function WallAddControl_0() {
    ActionDbl = "";
    var obWallScaleY = obWall.scale.y * 10;
    var obWallScaleX = obWall.scale.x * 10;
    objectsMoveX.pop();
    var YPosition = obWall.position.z - (obWallScaleY * 100) / 2;
    var XPosition = obWall.position.x + (obWallScaleX * 100);
    meshControl_Move_0.position.x = XPosition;
    meshControl_Move_0.position.z = YPosition;
    objectsMoveX.push(meshControl_Move_0);
    meshControl_Move_0.visible = true;
    IconActive_0_270 = true;
    InsertWall = 102;
};
function AddDimControl(Panel, x, y, z, NameDim) {
    var loader = new THREE.STLLoader();
    var materialTextDim = new THREE.MeshBasicMaterial({ color: 0xefb608, opacity: 0, transparent: true });
    var radius = 1;
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = Panel;
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "45pt Arial";
    ctx.fillStyle = '#000000';
    ctx.textAlign = "center";
    ctx.fillText(_dim.name, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x;
    sprite.position.y = y;
    sprite.position.z = z;
    _dim.add(sprite);
    _dim.name = NameDim;
    scene.add(_dim);

    //loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
    //    var meshTextDim = new THREE.Mesh(geometry, materialTextDim);
    //    meshTextDim.position.set(x + 10, 15, z - 20);
    //    meshTextDim.name = "CtrTextCube";
    //    meshTextDim.scale.set(0.03, 0.015, 0.03);
    //    scene.add(meshTextDim);
    //});
};
function AddDimText(NameTextDim, x, y) {
    scene.remove(_dim);
    var radius = 1;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    geom.name = "Dim_DimAtos";
    var mat = new THREE.MeshBasicMaterial({ color: Math.random() * 0xFFFFFF, wireframe: true });
    _dim = new THREE.Mesh(geom, mat);
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    TextDim = canvas.getContext("2d");
    TextDim.font = "35pt Arial";
    TextDim.fillStyle = '#000000';
    if (LinkEnvironment === 9) { TextDim.fillStyle = '#FFFFFF'; }
    TextDim.textAlign = "center";
    TextDim.fillText(NameTextDim, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({ map: tex });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x;
    sprite.position.y = -10;
    sprite.position.z = y;
    _dim.add(sprite);
    _dim.name = "Dim_DimAtos_0";
    _dim.visible = true;
    scene.add(_dim);
};
function AddDimTextGrill(NameTextDim, x, y) {
    var radius = 1;
    var geom = new THREE.SphereGeometry(radius, 64, 24);
    geom.name = "Geo_Grill_90";
    var mat = new THREE.MeshBasicMaterial({ color: Math.random() * 0x0AA0F7, wireframe: true });
    var GrillNumber = new THREE.Mesh(geom, mat);
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    TextDim = canvas.getContext("2d");
    TextDim.font = "50pt Arial";
    TextDim.fillStyle = '#0AA0F7';
    if (LinkEnvironment === 9) { TextDim.fillStyle = '#FFFFFF'; }
    TextDim.textAlign = "center";
    TextDim.fillText(NameTextDim, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({ map: tex });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(150, 150, 1.5);
    sprite.position.x = x;
    sprite.position.y = -10;
    sprite.position.z = y;
    GrillNumber.add(sprite);
    GrillNumber.name = "Grill_90";
    GrillNumber.visible = true;
    scene.add(GrillNumber);
};
function AddDimWall_90(ob) {
    ConeRight.visible = false
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    var obWallScaleY = ob.scale.z * 10;
    var YPosition = ob.position.z/* - (obWallScaleY * 100) / 2*/;
    var XPosition = ob.position.x;
    ConeTop.position.x = XPosition - 200;
    ConeTop.position.z = YPosition + 10;
    ConeTop.visible = true;

    LineDimTop.position.x = XPosition;
    LineDimTop.position.z = YPosition;
    LineDimTop.visible = true;

    ConeDown.position.x = XPosition - 200;
    ConeDown.position.z = (YPosition + (obWallScaleY * 100)) - 10;
    ConeDown.visible = true;

    LineDimDown.position.x = XPosition;
    LineDimDown.position.z = YPosition + (obWallScaleY * 100);
    LineDimDown.visible = true;

    LineDimTopToRDown.position.x = XPosition - 200;
    //LineDimTopToRDown.position.z = (YPosition + (obWallScaleY * 100)) - 10;
    LineDimTopToRDown.position.z = YPosition + (obWallScaleY * 100);
    LineDimTopToRDown.scale.y = ob.scale.z;
    LineDimTopToRDown.rotation.z = Math.PI;
    LineDimTopToRDown.visible = true;
    _dim.visible = true;
    _dim.position.x = XPosition + ((ob.scale.x * 1000) / 2);
    _dim.position.z = YPosition + 200;
    NameTextDim = (ob.scale.z * 10).toFixed(3);
    var x = ob.position.x -190;
    var y = ob.position.z + ((ob.scale.z * 1000) / 2);
    AddDimText(NameTextDim, x, y);
    ActionDbl = "Wall_R900";
};
function AddDimWall_0(ob)
{
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    var obWallScaleY = ob.scale.y * 10;
    var YPosition = ob.position.z - (obWallScaleY * 100) / 2;
    var XPosition = ob.position.x;
    ConeRight.position.x = XPosition + ((ob.scale.x * 1000) - 10);
    ConeRight.position.z = YPosition + 200;
    ConeRight.visible = true;
    ConeLeft.position.x = XPosition + 10;
    ConeLeft.position.z = YPosition + 200;
    ConeLeft.visible = true;
    LineDimLef.position.x = XPosition;
    LineDimLef.position.z = YPosition;
    LineDimLef.visible = true;
    LineRightToLeft.position.x = XPosition;
    LineRightToLeft.position.z = YPosition + 200;
    LineRightToLeft.scale.x = ob.scale.x;
    LineRightToLeft.visible = true;
    LineDimRight.position.x = XPosition + (ob.scale.x * 1000);
    LineDimRight.position.z = YPosition;
    LineDimRight.visible = true;
    _dim.visible = true;
    _dim.name = "Dim_DimAtos";
    _dim.position.x = XPosition + ((ob.scale.x * 1000) / 2);
    _dim.position.z = YPosition + 200;
    NameTextDim = (ob.scale.x * 10).toFixed(3);
    var x = ob.position.x + ((ob.scale.x * 1000) / 2);
    var y = ob.position.z + 190;
    AddDimText(NameTextDim, x, y);
    ActionDbl = "Wall_R000";
};
function AddDimTemporal(NameTextDim, x, y) {
    const loaderFonds = new THREE.FontLoader()
    loaderFonds.load("../../Content/DesignTools/Fonts/optimer_regular.typeface.json", function (font) {
     var geometryFond = new THREE.TextGeometry(NameTextDim, {
            font: font,
            size: 12,
            height: 0.2,
            curveSegments: 12,
            bevelEnabled: false,
            bevelThickness: 0.5,
            bevelSize: 0.3,
            bevelOffset: 0,
            bevelSegments: 5,
        })
      var  meshFonds = new THREE.Mesh(geometryFond, material);
        meshFonds.position.x = x;
        meshFonds.position.y = 5;
        meshFonds.position.z = y;
        scene.add(meshFonds);
    })
};
function AddDim20(ob) {
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    var obWallScaleY = ob.scale.y * 10;

    //Position DimLineRight
    var XPositionLineRight = meshEsq20.position.x;
    var YPositionLineRight = ob.position.z - (obWallScaleY * 100) / 2;
    LineDimRight.visible = true;
    LineDimRight.position.x = XPositionLineRight;
    LineDimRight.position.z = YPositionLineRight;
    ConeRight.position.x = XPositionLineRight - 10;
    ConeRight.position.z = YPositionLineRight + 200;
    ConeRight.visible = true;

    var YPosition = ob.position.z - (obWallScaleY * 100) / 2;
    var XPosition = ob.position.x;
    LineDimLef.position.x = XPosition;
    LineDimLef.position.z = YPosition;
    LineDimLef.visible = true;

    ConeLeft.position.x = XPosition + 10;
    ConeLeft.position.z = YPosition + 200;
    ConeLeft.visible = true;

    var dLine = XPositionLineRight - XPosition;
    LineRightToLeft.position.x = XPosition;
    LineRightToLeft.position.z = YPosition + 200;
    LineRightToLeft.scale.x = dLine / 1000;
    LineRightToLeft.visible = true;
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_20";
};
function AddDim60(ob) {
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    var obWallScaleY = ob.scale.y * 10;

    //Position DimLineRight
    var XPositionLineRight = meshEsq60.position.x;
    var YPositionLineRight = ob.position.z - (obWallScaleY * 100) / 2;
    LineDimRight.visible = true;
    LineDimRight.position.x = XPositionLineRight;
    LineDimRight.position.z = YPositionLineRight;
    ConeRight.position.x = XPositionLineRight -10;
    ConeRight.position.z = YPositionLineRight + 200;
    ConeRight.visible = true;


    var YPosition = ob.position.z - (obWallScaleY * 100) / 2;
    var XPosition = ob.position.x;

    LineDimLef.position.x = XPosition;
    LineDimLef.position.z = YPosition;
    LineDimLef.visible = true;


 
    ConeLeft.position.x = XPosition + 10;
    ConeLeft.position.z = YPosition + 200;
    ConeLeft.visible = true;


    var dLine = XPositionLineRight - XPosition;
    LineRightToLeft.position.x = XPosition;
    LineRightToLeft.position.z = YPosition + 200;
    LineRightToLeft.scale.x = dLine/1000;
    LineRightToLeft.visible = true;
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine /100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_60";
};
function AddDim40(ob) {
    var YPositionLineRight = meshEsq40.position.z;
    ConeRight.visible = false
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    var obWallScaleY = ob.scale.z * 10;
    var YPosition = ob.position.z + (obWallScaleY * 100);
    var XPosition = ob.position.x;
    ConeTop.position.x = XPosition - 200;
    ConeTop.position.z = meshEsq40.position.z + 10;
    ConeTop.visible = true;
    LineDimTop.position.x = XPosition;
    LineDimTop.position.z = meshEsq40.position.z;
    LineDimTop.visible = true;

    ConeDown.position.x = XPosition - 200;
    ConeDown.position.z = YPosition - 10;
    ConeDown.visible = true;

    LineDimDown.position.x = XPosition;
    LineDimDown.position.z = YPosition;
    LineDimDown.visible = true;

    var dLine = YPositionLineRight - YPosition;
    LineDimTopToRDown.rotation.z = Math.PI;
    LineDimTopToRDown.position.x = ob.position.x - 200;
    LineDimTopToRDown.position.z = meshEsq40.position.z;
    LineDimTopToRDown.scale.y = dLine / 1000;
    LineDimTopToRDown.visible = true;
    if (dLine < 0) { dLine = dLine * -1; }
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_40";
};
function AddDim80(ob) {
    var YPositionLineRight = meshEsq80.position.z;
    ConeRight.visible = false
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    var obWallScaleY = ob.scale.z * 10;
    var YPosition = ob.position.z + (obWallScaleY * 100);
    var XPosition = ob.position.x;
    ConeTop.position.x = XPosition - 200;
    ConeTop.position.z = meshEsq80.position.z + 10;
    ConeTop.visible = true;
    LineDimTop.position.x = XPosition;
    LineDimTop.position.z = meshEsq80.position.z;
    LineDimTop.visible = true;

    ConeDown.position.x = XPosition - 200;
    ConeDown.position.z = YPosition - 10;
    ConeDown.visible = true;

    LineDimDown.position.x = XPosition;
    LineDimDown.position.z = YPosition;
    LineDimDown.visible = true;

    var dLine = YPositionLineRight - YPosition;
    LineDimTopToRDown.rotation.z = Math.PI;
    LineDimTopToRDown.position.x = ob.position.x - 200;
    LineDimTopToRDown.position.z = meshEsq80.position.z;
    LineDimTopToRDown.scale.y = dLine / 1000;
    LineDimTopToRDown.visible = true;
    if (dLine < 0) { dLine = dLine * -1; }
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_80";
};
function AddDimX_90(ob) {
    var YPositionLineRight = meshEsqX.position.z;
    ConeRight.visible = false
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    var obWallScaleY = ob.scale.z * 10;
    var YPosition = ob.position.z + (obWallScaleY * 100);
    var XPosition = ob.position.x;
    ConeTop.position.x = XPosition - 200;
    ConeTop.position.z = meshEsqX.position.z + 10;
    ConeTop.visible = true;
    LineDimTop.position.x = XPosition;
    LineDimTop.position.z = meshEsqX.position.z;
    LineDimTop.visible = true;

    ConeDown.position.x = XPosition - 200;
    ConeDown.position.z = YPosition - 10;
    ConeDown.visible = true;

    LineDimDown.position.x = XPosition;
    LineDimDown.position.z = YPosition;
    LineDimDown.visible = true;

    var dLine = YPositionLineRight - YPosition;
    LineDimTopToRDown.rotation.z = Math.PI;
    LineDimTopToRDown.position.x = ob.position.x - 200;
    LineDimTopToRDown.position.z = meshEsqX.position.z;
    LineDimTopToRDown.scale.y = dLine / 1000;
    LineDimTopToRDown.visible = true;
    if (dLine < 0) { dLine = dLine * -1; }
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_X";
};
function AddDimX_00(ob) {
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    var obWallScaleY = ob.scale.y * 10;

    //Position DimLineRight
    var XPositionLineRight = meshEsqX.position.x;
    var YPositionLineRight = ob.position.z - (obWallScaleY * 100) / 2;
    LineDimRight.visible = true;
    LineDimRight.position.x = XPositionLineRight;
    LineDimRight.position.z = YPositionLineRight;
    ConeRight.position.x = XPositionLineRight - 10;
    ConeRight.position.z = YPositionLineRight + 200;
    ConeRight.visible = true;

    var YPosition = ob.position.z - (obWallScaleY * 100) / 2;
    var XPosition = ob.position.x;
    LineDimLef.position.x = XPosition;
    LineDimLef.position.z = YPosition;
    LineDimLef.visible = true;

    ConeLeft.position.x = XPosition + 10;
    ConeLeft.position.z = YPosition + 200;
    ConeLeft.visible = true;

    var dLine = XPositionLineRight - XPosition;
    LineRightToLeft.position.x = XPosition;
    LineRightToLeft.position.z = YPosition + 200;
    LineRightToLeft.scale.x = dLine / 1000;
    LineRightToLeft.visible = true;
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Esq_X_0";
};
function AddDParalles_00(ob) {
    var YPositionLineRight = meshParall.position.z;
    ConeRight.visible = false
    ConeLeft.visible = false;
    LineDimLef.visible = false;
    LineRightToLeft.visible = false;
    LineDimRight.visible = false;
    var obWallScaleY = ob.scale.z * 10;
    var YPosition = ob.position.z;
    var XPosition = ob.position.x;
    ConeTop.position.x = XPosition - 200;
    ConeTop.position.z = meshParall.position.z + 10;
    ConeTop.visible = true;
    LineDimTop.position.x = XPosition;
    LineDimTop.position.z = meshParall.position.z;
    LineDimTop.visible = true;

    ConeDown.position.x = XPosition - 200;
    ConeDown.position.z = YPosition - 10;
    ConeDown.visible = true;

    LineDimDown.position.x = XPosition;
    LineDimDown.position.z = YPosition;
    LineDimDown.visible = true;

    var dLine = YPositionLineRight - YPosition;
    LineDimTopToRDown.rotation.z = Math.PI;
    LineDimTopToRDown.position.x = ob.position.x - 200;
    LineDimTopToRDown.position.z = meshParall.position.z;
    LineDimTopToRDown.scale.y = dLine / 1000;
    LineDimTopToRDown.visible = true;
    if (dLine < 0) { dLine = dLine * -1; }
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    //ActionDbl = "Control_Move_Esq_X";
    ActionDbl = "Control_Move_Parall";
    
};
function AddDParalles_90(ob) {
    ConeTop.visible = false;
    LineDimTop.visible = false;
    ConeDown.visible = false;
    LineDimDown.visible = false;
    LineDimTopToRDown.visible = false;
    var obWallScaleY = ob.scale.y * 10;
    //Position DimLineRight
    var XPositionLineRight = meshParall90.position.x;
    var YPositionLineRight = ob.position.z   /*+ (obWallScaleY * 100) / 2*/;
    LineDimRight.visible = true;
    LineDimRight.position.x = XPositionLineRight;
    LineDimRight.position.z = YPositionLineRight - 200;
    ConeRight.position.x = XPositionLineRight - 10;
    ConeRight.position.z = YPositionLineRight - 200;
    ConeRight.visible = true;
    var YPosition = ob.position.z /*+ (obWallScaleY * 100) / 2*/;
    var XPosition = ob.position.x;
    LineDimLef.position.x = XPosition;
    LineDimLef.position.z = YPosition - 200;
    LineDimLef.visible = true;
    ConeLeft.position.x = XPosition + 10;
    ConeLeft.position.z = YPosition - 200;
    ConeLeft.visible = true;
    var dLine = XPositionLineRight - XPosition;
    LineRightToLeft.position.x = XPosition;
    LineRightToLeft.position.z = YPosition - 200;
    LineRightToLeft.scale.x = dLine / 1000;
    LineRightToLeft.visible = true;
    AddDivDim = true;
    KeyActive = false;
    NameTextDim = dLine / 100;
    document.getElementById("DivInputDim").style.display = "inline";
    ActionDbl = "Control_Move_Parall_90";
};