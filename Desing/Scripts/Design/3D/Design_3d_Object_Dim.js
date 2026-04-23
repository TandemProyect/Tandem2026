//DIM
//DIM
function InsertVertical(IdWall, x, y,z, LongDimTypeHorizontal, Filter, LongWood) {
    if (Filter === 'Vertical50') {
        InsertVertical270(IdWall, x, y,z, LongDimTypeHorizontal, Filter, LongWood);
    }
}
function InsertVertical270(IdWall, x, y,z, LongDimTypeHorizontal, Filter, DimTex) {
    x = parseInt(x);
    y = parseInt(y);
    var radius = 1;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01 + z, y));
    pointsDim.push(new THREE.Vector3(x, 0.01 + z, y + 300));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDim);
    LineDimLeft.name = IdWall + "_LineDimLeft_";
    LineDimLeft.idwall = IdWall;
    LineDimLeft.typeDim = "Dim";
    LineDimLeft.visible = false;
    scene.add(LineDimLeft);
    DrawDimDot(x, 0.01 + z, y + 300, LineDimLeft.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, ((DimTex / 10)) + z, y));
    pointsDim.push(new THREE.Vector3(x, (DimTex / 10) + z, y + 310));
    const LineTop = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineTop, materialDim);
    LineDimRight.name = IdWall + "_LineDimRight_";
    LineDimRight.idwall = IdWall;
    LineDimRight.typeDim = "Dim";
    LineDimRight.visible = false;
    scene.add(LineDimRight);
    DrawDimDot(x, (DimTex / 10) + z, y + 300, LineDimRight.name);

    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01 + z, y + 300));
    pointsDim.push(new THREE.Vector3(x , ((DimTex / 10) / 2 - 15) + z, y + 300));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
    LineTopLeft.name = IdWall + "_LineTopLeft";
    LineTopLeft.idwall = IdWall;
    LineTopLeft.typeDim = "Dim";
    LineTopLeft.visible = false;
    scene.add(LineTopLeft);

    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, ((DimTex / 10) / 2 + 15) + z, y + 300));
    pointsDim.push(new THREE.Vector3(x, (DimTex / 10) + z, y + 300));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
    LineTopRight.name = IdWall + "_LineTopRight";
    LineTopRight.idwall = IdWall;
    LineTopRight.typeDim = "Dim";
    LineTopRight.visible = false;
    scene.add(LineTopRight)

    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = (DimTex / 1000).toFixed(3);
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "25pt Arial";
    ctx.fillStyle = '#000000';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#FFFFFF';
    }
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
    sprite.position.y = (((DimTex / 10) / 2) - 17) + z;
    sprite.position.z = y + 300;
    _dim.add(sprite);
    _dim.name = IdWall + "_dim";
    _dim.idwall = IdWall;
    _dim.typeDim = "Dim";
    _dim.visible = false;
    scene.add(_dim);
};


function InsertDimHorizontal_0(IdWall, x, y, LongDimTypeHorizontal, Filter, LongWood) {
    var ob = getCHeckimWall(IdWall);
    var obScaleY = ob.scale.y * 1000;
    var YPosition = GetDimPosition(ob);
    if (YPosition === 90) {
        y = y - (50 + obScaleY);
    }
    var DistLineMedium = ((LongDimTypeHorizontal / 10) / 2) - 5;
    x = parseInt(x);
    y = parseInt(y);
    var z = 0.01;
    var radius = 1;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y));
    var PositionDimPoint = 110; if (YPosition === 90) { PositionDimPoint = -110; }
    var PositionDimPoint2 = 100; if (YPosition === 90) { PositionDimPoint2 = -100; }

    if (Filter !== "") {
        var PositionDimPoint = 60; if (YPosition === 90) { PositionDimPoint = -60; }
        var PositionDimPoint2 = 50; if (YPosition === 90) { PositionDimPoint2 = -50; }
    }
    if (LongWood !== 0) {
        var PositionDimPoint = 140; if (YPosition === 90) { PositionDimPoint = -140; }
        var PositionDimPoint2 = 130; if (YPosition === 90) { PositionDimPoint2 = -130; }
    }


    pointsDim.push(new THREE.Vector3(x, 0.01, y + PositionDimPoint));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDim);
    LineDimLeft.name = IdWall + "_LineDimLeft_";

    LineDimLeft.idwall = IdWall;
    LineDimLeft.typeDim = "Dim";
    LineDimLeft.visible = false;
    scene.add(LineDimLeft);
    DrawDimDot(x, 0.01 + z, y + PositionDimPoint2, LineDimLeft.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y));
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y + PositionDimPoint));
    const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineRight, materialDim);

    LineDimRight.name = IdWall + "_LineDimRight_";
    LineDimRight.idwall = IdWall;
    LineDimRight.typeDim = "Dim";
    LineDimRight.visible = false;
    scene.add(LineDimRight);
    DrawDimDot(x + LongDimTypeHorizontal / 10, 0.01 + z, y + PositionDimPoint2, LineDimRight.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y + PositionDimPoint2));
    pointsDim.push(new THREE.Vector3(x + DistLineMedium, 0.01, y + PositionDimPoint2));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
    LineTopLeft.name = IdWall + "_LineTopLeft";
    LineTopLeft.idwall = IdWall;
    LineTopLeft.typeDim = "Dim";
    LineTopLeft.visible = false;

    scene.add(LineTopLeft);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + DistLineMedium + 10, 0.01, y + PositionDimPoint2));
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y + PositionDimPoint2));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
    LineTopRight.name = IdWall + "_LineTopRight";
    LineTopRight.idwall = IdWall;
    LineTopRight.typeDim = "Dim";
    LineTopRight.visible = false;

    scene.add(LineTopRight)
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    if (Filter !== "") {
        _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    if (LongWood !== 0) {
        _dim.name = (LongDimTypeHorizontal / 1000).toFixed(3);
    }

    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "25pt Arial";
    ctx.fillStyle = '#000000';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#FFFFFF';
    }
    ctx.textAlign = "center";
    ctx.fillText(_dim.name, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x + (LongDimTypeHorizontal / 10) / 2;
    sprite.position.y = -10;
    sprite.position.z = y + PositionDimPoint;
    _dim.add(sprite);
    _dim.name = IdWall + "_dim";
    _dim.idwall = IdWall;
    _dim.typeDim = "Dim";
    _dim.visible = false;
    scene.add(_dim);
};
function InsertDimHorizontal_90(IdWall, x, y, LongDimTypeHorizontal, Filter, LongWood) {
    var ob = getCHeckimWall(IdWall);
    var obScaleY = ob.scale.y * 1000;
    var XPosition = GetDimPosition(ob);
    if (XPosition === 180) {
        x = x - 50;
    }
    var z = 0.01;
    var DistLineMedium = ((LongDimTypeHorizontal / 10) / 2) - 5;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y));
    var PositionDimPoint = 110; if (XPosition === 90) { PositionDimPoint = -110; }
    var PositionDimPoint2 = 100; if (XPosition === 90) { PositionDimPoint2 = -100; }

    if (Filter !== "") {
        var PositionDimPoint = 60; if (XPosition === 90) { PositionDimPoint = -60; }
        var PositionDimPoint2 = 50; if (XPosition === 90) { PositionDimPoint2 = -50; }
    }

    if (LongWood !== 0) {
        var PositionDimPoint = 140; if (XPosition === 90) { PositionDimPoint = -140; }
        var PositionDimPoint2 = 130; if (XPosition === 90) { PositionDimPoint2 = -130; }
    }


    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDim);
    LineDimLeft.name = IdWall + "_LineDimLeft";
    LineDimLeft.idwall = IdWall;
    LineDimLeft.typeDim = "Dim";
    LineDimLeft.visible = false;
    scene.add(LineDimLeft);
    DrawDimDot(x - PositionDimPoint2, 0.01 + z, y, LineDimLeft.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y - LongDimTypeHorizontal / 10));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint, 0.01, y - LongDimTypeHorizontal / 10));
    const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineRight, materialDim);
    LineDimRight.name = IdWall + "_LineDimRight";
    LineDimRight.idwall = IdWall;
    LineDimRight.typeDim = "Dim";
    LineDimRight.visible = false;
    scene.add(LineDimRight);
    DrawDimDot(x - PositionDimPoint2, 0.01 + z, y - LongDimTypeHorizontal / 10, LineDimRight.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y - 40));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
    LineTopLeft.name = IdWall + "_LineTopLeft";
    LineTopLeft.idwall = IdWall;
    LineTopLeft.typeDim = "Dim";
    LineTopLeft.visible = false;
    scene.add(LineTopLeft);

    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y - DistLineMedium));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y - LongDimTypeHorizontal / 10));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
    LineTopRight.name = IdWall + "_LineTopRight";
    LineTopRight.idwall = IdWall;
    LineTopRight.typeDim = "Dim";
    LineTopRight.visible = false;
    scene.add(LineTopRight)
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    if (Filter !== "") {
        _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    if (LongWood !== 0) {
        _dim.name = (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "25pt Arial";

    ctx.fillStyle = '#000000';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#FFFFFF';
    }
    ctx.textAlign = "center";
    ctx.fillText(_dim.name, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x - PositionDimPoint;
    sprite.position.y = -10;
    sprite.position.z = y - (LongDimTypeHorizontal / 10) / 2;
    _dim.add(sprite);
    _dim.name = IdWall + "_dim";
    _dim.idwall = IdWall;
    _dim.typeDim = "Dim";
    _dim.visible = false;
    scene.add(_dim);
};
function getTipeRetumWallR000(ob) {
    var Conexion0 = ob.IdWall_0.substr(0, 6);
    var Conexion180 = ob.IdWall_180.substr(0, 6);
    if (Conexion0 === 'Esq_30') { return 90; }
    if (Conexion0 === 'Esq_20') { return 90; }
    if (Conexion180 === 'Esq_10') { return 90; }
    if (Conexion180 === 'Esq_20') { return 90; }
    return 270;
}

function getTipeRetumWallR900(ob) {
    //'Esq_50'
    /* IdWall_90 = 'Esq_40_901727950212568'*/
    var typeOb = ob.idWall.substr(0, 6);
    var Conexion90 = ob.IdWall_90.substr(0, 6);
    var Conexion270 = ob.IdWall_270.substr(0, 6);
    if (Conexion90 === 'Esq_30') { return 90; }
    if (Conexion90 === 'Esq_40') { return 90; }
    if (Conexion270 === 'Esq_40') { return 90; }
    if (Conexion270 === 'Esq_50') { return 90; }
    return 180;
}

function GetDimPosition(ob) {
    var a = 1;
    switch (ob.idWall.substr(0, 9)) {
        case 'Wall_R000':
            return getTipeRetumWallR000(ob);
            break;
        case 'Wall_R900':
            var a = getTipeRetumWallR900(ob);
            return a;
            break;
        case 'Esq_50_00':
            return 270;
            break;
        case 'Esq_50_90':
            return 90;
            break;
        case 'Esq_40_90':
            return 90;
        case 'Esq_30_00':
            return 90;
            break;
        case 'Esq_30_90':
            return 90;
            break;
        case 'Esq_20_00':
            return 90;
            break;
        case 'Esq_10_00':
            return 90;
            break;
        default:
            break;
    }
    return 180;
}
function DrawDimDot(x, y, z, name) {
    const geometry = new THREE.SphereGeometry(1, 10, 60);
    var color = 0x000000;
    if (LinkEnvironment === 9) {
        color = 0xFFFFFF;
    }
    const pt = new THREE.Points(
        geometry,
        new THREE.PointsMaterial({
            color: color,
            size: 0.5
        }));
    pt.position.x = x;
    pt.position.y = y;
    pt.position.z = z;
    pt.name = name;
    pt.typeDim = "Dim";
    pt.visible = false;
    scene.add(pt);
};

function DrawDimDotWall(x, y, z, name) {
    const geometry = new THREE.SphereGeometry(1, 10, 60);
    var color = 0xffae00;
    if (LinkEnvironment === 9) {
        color = 0xffae00;
    }
    const pt = new THREE.Points(
        geometry,
        new THREE.PointsMaterial({
            color: color,
            size: 0.9
        }));
    pt.position.x = x;
    pt.position.y = y;
    pt.position.z = z;
    pt.name = name;
    pt.typeDim = "DimWall";
    pt.visible = false;
    scene.add(pt);
};


function AddDimWall_0Static(IdWall, x, y, LongDimTypeHorizontal, Filter, Dist) {
 
    var obScaleY = IdWall.scale.y * 1000;
    var YPosition = GetDimPosition(IdWall);
    if (YPosition === 90) {
        y = y - (50 + obScaleY);
    }
    var DistLineMedium = ((LongDimTypeHorizontal / 10) / 2) - 5;
    x = parseInt(x);
    y = parseInt(y);
    var z = 0.01;
    var radius = 1;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y));
    var PositionDimPoint = 180; if (YPosition === 90) { PositionDimPoint = -180; }
    var PositionDimPoint2 = 170; if (YPosition === 90) { PositionDimPoint2 = -170; }

    if (Filter !== "") {
        var PositionDimPoint = 90; if (YPosition === 90) { PositionDimPoint = -90; }
        var PositionDimPoint2 = 80; if (YPosition === 90) { PositionDimPoint2 = -80; }
    }
    if (Dist !== 0) {
        var PositionDimPoint = 190; if (YPosition === 90) { PositionDimPoint = -190; }
        var PositionDimPoint2 = 180; if (YPosition === 90) { PositionDimPoint2 = -180; }
    }


    pointsDim.push(new THREE.Vector3(x, 0.01, y + PositionDimPoint));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDimWall);
    LineDimLeft.name = IdWall + "_LineDimLeftWall_";

    LineDimLeft.idwall = IdWall;
    LineDimLeft.typeDim = "DimWall";
    LineDimLeft.visible = false;
    scene.add(LineDimLeft);
    DrawDimDotWall(x, 0.01 + z, y + PositionDimPoint2, LineDimLeft.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y));
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y + PositionDimPoint));
    const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineRight, materialDimWall);

    LineDimRight.name = IdWall + "_LineDimRightWall_";
    LineDimRight.idwall = IdWall;
    LineDimRight.typeDim = "DimWall";
    LineDimRight.visible = false;
    scene.add(LineDimRight);
    DrawDimDotWall(x + LongDimTypeHorizontal / 10, 0.01 + z, y + PositionDimPoint2, LineDimRight.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y + PositionDimPoint2));
    pointsDim.push(new THREE.Vector3(x + DistLineMedium, 0.01, y + PositionDimPoint2));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDimWall);
    LineTopLeft.name = IdWall + "_LineTopLeftWall";
    LineTopLeft.idwall = IdWall;
    LineTopLeft.typeDim = "DimWall";
    LineTopLeft.visible = false;

    scene.add(LineTopLeft);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + DistLineMedium + 10, 0.01, y + PositionDimPoint2));
    pointsDim.push(new THREE.Vector3(x + LongDimTypeHorizontal / 10, 0.01, y + PositionDimPoint2));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDimWall);
    LineTopRight.name = IdWall + "_LineTopRight";
    LineTopRight.idwall = IdWall;
    LineTopRight.typeDim = "DimWall";
    LineTopRight.visible = false;

    scene.add(LineTopRight)
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xffae00,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    if (Filter !== "") {
        _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    if (Dist !== 0) {
        _dim.name = (LongDimTypeHorizontal / 1000).toFixed(3);
    }

    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "30pt Arial";
    ctx.fillStyle = '#2300ff';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#ffe000';
    }
    if (LinkEnvironment === 2) {
        ctx.fillStyle = '#ffe000';
    }
    ctx.textAlign = "center";
    ctx.fillText(_dim.name, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x + (LongDimTypeHorizontal / 10) / 2;
    sprite.position.y = -10;
    sprite.position.z = y + PositionDimPoint;
    _dim.add(sprite);
    _dim.name = IdWall + "_dimWall";
    _dim.idwall = IdWall;
    _dim.typeDim = "DimWall";
    _dim.visible = false;
    scene.add(_dim);
};
function AddDimWall_90Static(IdWall, x, y, LongDimTypeHorizontal, Filter, LongWood) {
    var obScaleY = IdWall.scale.y * 1000;
    var XPosition = GetDimPosition(IdWall);
    if (XPosition === 180) {
        x = x - 50;
    }
    var z = 0.01;
    var DistLineMedium = ((LongDimTypeHorizontal / 10) / 2) - 5;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y));
    var PositionDimPoint = 180; if (XPosition === 90) { PositionDimPoint = -180; }
    var PositionDimPoint2 = 170; if (XPosition === 90) { PositionDimPoint2 = -170; }
    if (Filter !== "") {
        var PositionDimPoint = 90; if (XPosition === 90) { PositionDimPoint = -90; }
        var PositionDimPoint2 = 80; if (XPosition === 90) { PositionDimPoint2 = -80; }
    }

    if (LongWood !== 0) {
        var PositionDimPoint = 190; if (XPosition === 90) { PositionDimPoint = -190; }
        var PositionDimPoint2 = 180; if (XPosition === 90) { PositionDimPoint2 = -180; }
    }


    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDimWall);
    LineDimLeft.name = IdWall + "_LineDimLeftWall";
    LineDimLeft.idwall = IdWall;
    LineDimLeft.typeDim = "DimWall";
    LineDimLeft.visible = false;
    scene.add(LineDimLeft);
    DrawDimDotWall(x - PositionDimPoint2, 0.01 + z, y, LineDimLeft.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y + LongDimTypeHorizontal / 10));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint, 0.01, y + LongDimTypeHorizontal / 10));
    const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineRight, materialDimWall);
    LineDimRight.name = IdWall + "_LineDimRightWall";
    LineDimRight.idwall = IdWall;
    LineDimRight.typeDim = "DimWall";
    LineDimRight.visible = false;
    scene.add(LineDimRight);
    DrawDimDotWall(x - PositionDimPoint2, 0.01 + z, y + LongDimTypeHorizontal / 10, LineDimRight.name);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y + DistLineMedium));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDimWall);
    LineTopLeft.name = IdWall + "_LineTopLeftWall";
    LineTopLeft.idwall = IdWall;
    LineTopLeft.typeDim = "DimWall";
    LineTopLeft.visible = false;
    scene.add(LineTopLeft);

    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y + DistLineMedium));
    pointsDim.push(new THREE.Vector3(x - PositionDimPoint2, 0.01, y + LongDimTypeHorizontal / 10));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDimWall);
    LineTopRight.name = IdWall + "_LineTopRightWall";
    LineTopRight.idwall = IdWall;
    LineTopRight.typeDim = "DimWall";
    LineTopRight.visible = false;
    scene.add(LineTopRight)
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xffae00,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    if (Filter !== "") {
        _dim.name = Filter + " " + (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    if (LongWood !== 0) {
        _dim.name = (LongDimTypeHorizontal / 1000).toFixed(3);
    }
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    var ctx = canvas.getContext("2d");
    ctx.font = "30pt Arial";

    ctx.fillStyle = '#2300ff';
    if (LinkEnvironment === 9) {
        ctx.fillStyle = '#ffe000';
    }
    if (LinkEnvironment === 2) {
        ctx.fillStyle = '#ffe000';
    }
    ctx.textAlign = "center";
    ctx.fillText(_dim.name, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x - PositionDimPoint;
    sprite.position.y = -10;
    sprite.position.z = y + (LongDimTypeHorizontal / 10) / 2;
    _dim.add(sprite);
    _dim.name = IdWall + "_dimWall";
    _dim.idwall = IdWall;
    _dim.typeDim = "DimWall";
    _dim.visible = false;
    scene.add(_dim);
};

 