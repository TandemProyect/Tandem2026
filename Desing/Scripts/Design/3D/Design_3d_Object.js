/// <reference path="design-3d-cameras.js" />
/// <reference path="design-3d-cameras.js" />
// Object
var materialReturm = new THREE.MeshLambertMaterial({ color: 0xFFFFFF });
var material = new THREE.MeshLambertMaterial({ color: 0xFFFFFF });

function CleanEsqAndCruce() {
    var obOld = null;
    InsertWall = 0;
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
        }
    }

}
function CreateFacesWall(_longWall, x, y, z, _heightWall, NameWall, _widthWall, _name) {
    const geometryRight = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x839192 });
    const Face = new THREE.Mesh(geometryRight, material);
    Face.geometry.name = NameWall;
    Face.scale.x = _longWall * 100;
    Face.name = _name + NameWall;
    Face.scale.z = 0.001;
    Face.scale.y = _heightWall * 100;
    Face.position.set(x, z, y);
    Face.visible = visible = true;
    scene.add(Face);
}
function deleteOb() {
    scene.remove(ob);
    ob.geometry.dispose();
    ob.material.dispose();
    $("#EditWall").hide("slow", function () {
    });
}
function changePositionY() {
    var value = $('iframe[name=DivLefEdit]').contents().find('#YValuePosition').val();
    var obX = ob.position.x;
    var obz = ob.position.z;
    ob.position.set(obX, 0, value);
}
function changePositionX() {
    var value = $('iframe[name=DivLefEdit]').contents().find('#XValueNumberPosition').val() * 100;
    var oby = ob.position.x;
    var obz = ob.position.z;
    ob.position.set(value, 0, obz);
}
//Change Size
function changeX() {
    var value = $('iframe[name=DivLefEdit]').contents().find('#XValue').val() * 10;
    var oby = ob.scale.y;
    var obz = ob.scale.z;
    ob.scale.set(value, oby, obz);
}
function changeThickness() {
    var value = $('iframe[name=DivLefEdit]').contents().find('#ThicknessValue').val() * 100;
    var oby = value;
    var obz = ob.scale.z;
    var obx = ob.scale.x;
    ob.scale.set(obx, oby, obz);
}
function changeHeight() {
    var value = $('iframe[name=DivLefEdit]').contents().find('#HeightValue').val() * 10;
    var oby = ob.scale.y;
    var obz = value;
    var obx = ob.scale.x;
    ob.scale.set(obx, oby, obz);
}
function Create_Face_X(_longWall, x, y, _heightWall, NameWall, _widthWall) {
    var n = (_longWall / 0.25) - 2;
    for (var i = 0; i < n; i++) {
        const geometry = new THREE.BoxGeometry(1, 1, 1);
        const material = new THREE.MeshBasicMaterial({ color: 0x839192 });
        const cube = new THREE.Mesh(geometry, material);
        cube.scale.x = _widthWall * 100;
        cube.name = "Face_Wall_Rect" + NameWall;
        cube.scale.z = 0.001;
        cube.scale.y = _heightWall * 100;
        cube.position.set(x + 25, (_heightWall * 100) / 2, y - (_widthWall * 100 / 2));
        cube.visible = visible = true;
        scene.add(cube);
        x = x + _widthWall * 100;
    }
}
function Create_Face_Esq_Left(_longWall, x, y, _heightWall, NameWall, _widthWall) {
    const geometryLeft = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x839192 });
    const cubeEsqLeft = new THREE.Mesh(geometryLeft, material);
    cubeEsqLeft.scale.x = _widthWall * 100;
    cubeEsqLeft.name = "Face_Esq_Left" + NameWall;
    cubeEsqLeft.scale.z = 0.001;
    cubeEsqLeft.scale.y = _heightWall * 100;
    cubeEsqLeft.position.set(x, (_heightWall * 100) / 2, y - (_widthWall * 100 / 2));
    cubeEsqLeft.visible = visible = true;
    scene.add(cubeEsqLeft);
}
function Create_Face_Esq_Right(_longWall, x, y, _heightWall, NameWall, _widthWall) {
    const geometryRight = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x839192 });
    const cubeEsqRight = new THREE.Mesh(geometryRight, material);
    cubeEsqRight.scale.x = _widthWall * 100;
    cubeEsqRight.name = "Face_Esq_Right" + NameWall;
    cubeEsqRight.scale.z = 0.001;
    cubeEsqRight.scale.y = _heightWall * 100;
    cubeEsqRight.position.set((x + _longWall * 100) - (_widthWall * 100), (_heightWall * 100) / 2, y - (_widthWall * 100 / 2));
    cubeEsqRight.visible = visible = true;
    scene.add(cubeEsqRight);
}
//839192 Color Muro
function FilerRigiSExS01(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, ParameteerFilter) {
    if (CodeName === "1850164") {
        return;
    }
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    if (XRotate === 0) {
        XRotate = 0;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }
    x = x + ParameteerFilter;
    x = parseInt(x);
    y = parseInt(y);
    var h = z;
    var material = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var NameMesh = "Atk60_" + CodeName;
    var loaderMesh = new THREE.STLLoader();
    loaderMesh.load(Element, function (geometry) {
        var mesh = new THREE.Mesh(geometry, material);
        mesh.position.set(x, z, y);
        mesh.rotation.x = -0.5 * Math.PI;
        mesh.name = NameMesh;
        mesh.rotation.x = XRotate;
        mesh.rotation.y = ZRotate;
        mesh.rotation.z = YRotate;
        mesh.scale.set(1, 1, 1);
        mesh.scale.x = ScaleX;
        mesh.scale.y = ScaleY;
        mesh.scale.z = ScaleZ;
        scene.add(mesh);
    });
    return;
};
function FilerSExS01(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {

    var l = 0.0022;
    var w = longWood / 10000;
    if (XRotate === 0) {
        l = 0.0022;
        w = longWood / 10000;
        y = y - longWood / 10;
        x = x + 2.2;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    loaderWood.load(Element, function (geometry) {
        var meshWood = new THREE.Mesh(geometry, material);
        meshWood.position.set(x, 0, y);
        meshWood.rotation.x = -0.5 * Math.PI;
        meshWood.name = NameWood;
        meshWood.rotation.z = ZRotate;
        meshWood.scale.set(1, 1, 1);
        meshWood.scale.x = l;
        meshWood.scale.y = w;
        meshWood.scale.z = h;
        scene.add(meshWood);
    });
    var material2 = new THREE.MeshLambertMaterial({ color: 0xA78344 });
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial90 = new THREE.Mesh(geometry, material2);
        meshWoodLaterial90.position.set(x + (ParametFilter - 2.2), 0, y);
        meshWoodLaterial90.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial90.name = NameWood;
        meshWoodLaterial90.rotation.z = ZRotate;
        meshWoodLaterial90.scale.set(1, 1, 1);
        meshWoodLaterial90.scale.x = ((ParametFilter - 2.2) / 1000);
        meshWoodLaterial90.scale.y = 0.0075;
        meshWoodLaterial90.scale.z = h;
        scene.add(meshWoodLaterial90);
    });
    var x2 = x + ParametFilter - 2.2;
    var y2 = y + (w * 1000) - 7.5;
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material2);
        meshWoodLaterial270.position.set(x2, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.rotation.z = ZRotate;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = ((ParametFilter - 2.2) / 1000);
        meshWoodLaterial270.scale.y = 0.0075;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
function FilerSEMA03(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {
    var l = 0.0022;
    var w = ((longWood + 240) / 10000);
    if (XRotate === 0) {
        l = 0.0022;
        w = ((longWood + 240) / 10000);
        y = (y - longWood / 10) - 12;
        x = x;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    var y2 = y + ((w * 1000) - (12 + ParametFilter));
    var scaley = 0.012 + (ParametFilter / 1000);
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material);
        meshWoodLaterial270.position.set(x + 12, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.rotation.z = ZRotate;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = 0.012;
        meshWoodLaterial270.scale.y = scaley;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
function FilerSEMeS01(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {
    var l = 0.0022;
    var w = ((longWood + 240) / 10000);
    if (XRotate === 0) {
        l = 0.0022;
        w = ((longWood + 240) / 10000);
        y = (y - longWood / 10) - 12;
        x = x + 2.2;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    loaderWood.load(Element, function (geometry) {
        var meshWood = new THREE.Mesh(geometry, material);
        meshWood.position.set(x, 0, y);
        meshWood.rotation.x = -0.5 * Math.PI;
        meshWood.name = NameWood;
        meshWood.rotation.z = ZRotate;
        meshWood.scale.set(1, 1, 1);
        meshWood.scale.x = l;
        meshWood.scale.y = w;
        meshWood.scale.z = h;
        scene.add(meshWood);
    });
    var material2 = new THREE.MeshLambertMaterial({ color: 0xA78344 });
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial90 = new THREE.Mesh(geometry, material2);
        meshWoodLaterial90.position.set(x + 7.3, 0, y);
        meshWoodLaterial90.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial90.name = NameWood;
        meshWoodLaterial90.rotation.z = ZRotate;
        meshWoodLaterial90.scale.set(1, 1, 1);
        meshWoodLaterial90.scale.x = 0.0075;
        meshWoodLaterial90.scale.y = 0.012;
        meshWoodLaterial90.scale.z = h;
        scene.add(meshWoodLaterial90);
    });
    var y2 = y + (w * 1000) - 12;
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material2);
        meshWoodLaterial270.position.set(x + 7.3, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.rotation.z = ZRotate;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = 0.0075;
        meshWoodLaterial270.scale.y = 0.012;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
function FilerTapeS2(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {
    var l = 0.0022;
    var w = longWood / 10000;
    if (XRotate === 0) {
        l = 0.0022;
        w = longWood / 10000;
        y = y - longWood / 10;
        x = x + 2.2;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    loaderWood.load(Element, function (geometry) {
        var meshWood = new THREE.Mesh(geometry, material);
        meshWood.position.set(x, 0, y);
        meshWood.rotation.x = -0.5 * Math.PI;
        meshWood.name = NameWood;
        meshWood.rotation.z = ZRotate;
        meshWood.scale.set(1, 1, 1);
        meshWood.scale.x = l;
        meshWood.scale.y = w;
        meshWood.scale.z = h;
        scene.add(meshWood);
    });
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial90 = new THREE.Mesh(geometry, material);
        meshWoodLaterial90.position.set(x + (ParametFilter - 2.2), 0, y);
        meshWoodLaterial90.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial90.name = NameWood;
        meshWoodLaterial90.rotation.z = ZRotate;
        meshWoodLaterial90.scale.set(1, 1, 1);
        meshWoodLaterial90.scale.x = ((ParametFilter - 2.2) / 1000);
        meshWoodLaterial90.scale.y = 0.0022;
        meshWoodLaterial90.scale.z = h;
        scene.add(meshWoodLaterial90);
    });
    var x2 = x + ParametFilter - 2.2;
    var y2 = y + (w * 1000) - 2.2;
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material);
        meshWoodLaterial270.position.set(x2, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.rotation.z = ZRotate;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = ((ParametFilter - 2.2) / 1000);
        meshWoodLaterial270.scale.y = 0.0022;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
function InsertWoodPlank(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {

    if (Filter === "SExS01") {
        FilerSExS01(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter);
        return;
    }
    if (Filter === "TapeS2") {
        FilerTapeS2(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter);
        return;
    }
    if (Filter === "SEMeS01") {
        FilerSEMeS01(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter);
        return;
    }
    if (Filter === "SEMA03") {
        FilerSEMA03(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter);
        return;
    }
    if (Filter === "Remate90") {
        Remate90(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, longWood);
        return;
    }

    if (Filter === "Remate0") {
        Remate0(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, longWood);
        return;
    }
    if (XRotate === 180) {
        ZRotate = + Math.PI * - 0.5;
        y = y + 12;
    }
    if (XRotate === 90) {
        ZRotate = + Math.PI * - 0.5;
    }
    if (XRotate === 0) {
        y = parseInt(y) - 12;
    }
    x = parseInt(x);

    var l = longWood / 10000;
    var w = 0.012;
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    loaderWood.load(Element, function (geometry) {
        var meshWood = new THREE.Mesh(geometry, material);
        meshWood.position.set(x, 0, y);
        meshWood.rotation.x = -0.5 * Math.PI;
        meshWood.name = NameWood;
        meshWood.rotation.z = ZRotate;
        meshWood.scale.set(1, 1, 1);
        meshWood.scale.x = l;
        meshWood.scale.y = w;
        meshWood.scale.z = h;
        scene.add(meshWood);
    });
    return;
};
//Remate90
function Remate90(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {
    var l = 0.0022;
    var w = ((longWood + 240) / 10000);
    if (XRotate === 90) {
        l = 0.0022;
        w = ((longWood + 240) / 10000);
        y = (y - longWood / 10) - 12;
        x = x - 12;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var MeshTypeWallName = "Remate90_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    var y2 = y /*+ ((w * 1000) - (12 + ParametFilter))*/;
    var scaley = (ParametFilter / 1000);
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material);
        meshWoodLaterial270.position.set(x + 12, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.MeshTypeWall = MeshTypeWallName;
        meshWoodLaterial270.rotation.z = ZRotate;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = 0.012;
        meshWoodLaterial270.scale.y = scaley * 0.98;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
function Remate0(Filter, Id, CodeName, XRotate, Element, x, y, ZRotate, longWood, widthWood, heightWood, ParametFilter) {
    var l = 0.0022;
    var w = ((longWood + 240) / 10000);
    if (XRotate === 0) {
        l = 0.0022;
        w = ((longWood + 240) / 10000);
        x = (x - longWood / 10) - 12;
        y = y - 12;
    }
    x = parseInt(x);
    y = parseInt(y);
    var h = heightWood / 10000;
    var material = new THREE.MeshLambertMaterial({ color: 0xD35400 });
    var partName = new Date().valueOf();
    var NameWood = "Atk60_" + CodeName;
    var loaderWood = new THREE.STLLoader();
    var y2 = y + ((w * 1000) - (12 + ParametFilter));
    var scaley = (ParametFilter / 1000);
    loaderWood.load(Element, function (geometry) {
        var meshWoodLaterial270 = new THREE.Mesh(geometry, material);
        meshWoodLaterial270.position.set(x + 12, 0, y2);
        meshWoodLaterial270.rotation.x = -0.5 * Math.PI;
        meshWoodLaterial270.name = NameWood;
        meshWoodLaterial270.rotation.z = 0.5 * Math.PI;
        meshWoodLaterial270.scale.set(1, 1, 1);
        meshWoodLaterial270.scale.x = 0.012;
        meshWoodLaterial270.scale.y = scaley;
        meshWoodLaterial270.scale.z = h;
        scene.add(meshWoodLaterial270);
    });
    return;
};
//Insert wall
function GetYXDef(CodeName, y) {
    switch (CodeName) {
        case "230050":
            return y + 25;
            break;
        case "230060":
            return y + 30;
            break;
        case "230070":
            return y + 37.5;
            break;
        case "230080":
            return y + 40;
            break;
        case "230090":
            return y + 45;
            break;
        case "230100":
            return y + 50;
            break;
        case "230110":
            return y + 55;
            break;
        case "230120":
            return y + 60;
            break;
        case "230130":
            return y + 65;
            break;
        case "230140":
            return y + 70;
            break;
        case "230150":
            return y + 75;
            break;
        case "230160":
            return y + 80;
            break;
        case "230170":
            return y + 85;
            break;
        case "230180":
            return y + 90;
            break;
        case "230190":
            return y + 95;
            break;
        case "230200":
            return y + 100;
            break;
        case "230210":
            return y + 105;
            break;
        case "230220":
            return y + 110;
            break;
        case "230230":
            return y + 115;
            break;
        case "230240":
            return y + 120;
            break;
        case "230250":
            return y + 125;
            break;
        case "230260":
            return y + 130;
            break;
        case "230270":
            return y + 135;
            break;
        case "230280":
            return y + 140;
            break;
        case "230290":
            return y + 145;
            break;
    }
    return y;
}
function GetScaleY(CodeName) {
    switch (CodeName) {
        case "230050":
            return 50;
            break;
        case "230060":
            return 60;
            break;
        case "230070":
            return 70;
            break;
        case "230080":
            return 80;
            break;
        case "230090":
            return 90;
            break;
        case "230100":
            return 100;
            break;
        case "230110":
            return 110;
            break;
        case "230120":
            return 120;
            break;
        case "230130":
            return 130;
            break;
        case "230140":
            return 140;
            break;
        case "230150":
            return 150;
            break;
        case "230160":
            return 160;
            break;
        case "230170":
            return 170;
            break;
        case "230180":
            return 180;
            break;
        case "230190":
            return 190;
            break;
        case "230200":
            return 200;
            break;
        case "230210":
            return 210;
            break;
        case "230220":
            return 220;
            break;
        case "230230":
            return 230;
            break;
        case "230240":
            return 240;
            break;
        case "230250":
            return 250;
            break;
        case "230260":
            return 260;
            break;
        case "230270":
            return 270;
            break;
        case "230280":
            return 280;
            break;
        case "230290":
            return 290;
            break;
    }
    return 100;
}
function InsertDywidag(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    var YRotate = 0;
    var yDef = 0;
    var xDef = 0;
    if (XRotate === 0) {
        yDef = GetYXDef(CodeName, y);
        x = parseInt(x);
        y = parseInt(yDef);
        XRotate = -0.5 * Math.PI;
        if (ZRotate === "90") {
            ZRotate = - (Math.PI * 0.5);
        }
    }
    if (XRotate === 90) {
        xDef = GetYXDef(CodeName, x);
        XRotate = -0.5 * Math.PI;
        YRotate = Math.PI * 0.5;
        x = parseInt(xDef);
        y = parseInt(y);
    }
    Edit_Wall = 20;
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;

        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertDywidagPlaca(Filter, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var _getCHeckRijiOutside = getCHeckRijiOutside(IdWall);
    var _getCHeckRijiInside = getCHeckRijiInside(IdWall);

    if (Filter === "Tape") {
        if (_getCHeckRijiInside === false) {
            return;
        }
        if (_getCHeckRijiOutside === false) {
            return;
        }
    }
    var YRotate = 0;
    var PaintRigi = true;
    if (CodeName === "1850162") {
        PaintRigi = false;
    }
    if (CodeName === "1850164") {
        PaintRigi = false;
    }
    if (CodeName === "1850163") {
        PaintRigi = false;
    }
    if (CodeName === "10443020-2") {
        PaintRigi = false;
    }
    if (XRotate === 271) {
        if (PaintRigi === false) {
            if (_getCHeckRijiOutside === false) { return; }
        }

        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = 0;
    }
    if (XRotate === 181) {
        if (PaintRigi === false) {
            if (_getCHeckRijiOutside === false) { return; }
        }
        XRotate = Math.PI * 0.50;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
    if (XRotate === 91) {
        if (PaintRigi === false) {
            if (_getCHeckRijiInside === false) { return; }
        }
        XRotate = Math.PI * 0.5;
        YRotate = 0;
        ZRotate = 0;
    }
    if (XRotate === 1) {
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }

    //Muro Recto
    if (XRotate === 90) {
        XRotate = Math.PI;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }
    if (XRotate === 270) {
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
    if (XRotate === 180) {
        XRotate = Math.PI * 0.5;
        YRotate = 0;
        ZRotate = 0;
    }
    if (XRotate === 0) {
        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = 0;
    }
    if (CodeName === "10443020-2") { CodeName = "10443020"; }
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    if (XRotate === 180) {
        XRotate = Math.PI * 0.5;
        YRotate = 0;
        ZRotate = 0;
    }
    if (XRotate === 270) {
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI;
        ZRotate = Math.PI * -0.5;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionRegulable(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var XRotateDef = 0;
    var YRotateDef = 0;
    var ZRotateDef = 0;

    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var yDef = GetYXDef(CodeName, y);
    ScaleY = GetScaleY(CodeName);

    YRotate = 0;
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    if (XRotate === 0)
    {
        XRotateDef = Math.PI * 1.5;
        YRotateDef = - Math.PI * 1.5;
        ZRotateDef = 0;
    }

    if (XRotate === 270) {
        XRotateDef = Math.PI * 0.5;
        YRotateDef = Math.PI;
        ZRotateDef = 0;
    }

    if (XRotate === 180) {
        XRotateDef = Math.PI * 1.5;
        YRotateDef = Math.PI * 1.5;
        ZRotateDef = 0;
    }
    if (XRotate === 90) {
        XRotateDef = Math.PI * 1.5;
        YRotateDef = Math.PI * 1.5;
        ZRotateDef = 0;
    }

    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    // No Recontar
    if (CodeName === "10000221B") {
        NameUnion = "Atk60";
    }

    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotateDef;
        meshUnion1.rotation.y = ZRotateDef;
        meshUnion1.rotation.z = YRotateDef;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionTape1850164(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter) {
    materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    ZRotate = 0;
    YRotate = 0;
    if (XRotate === 2) {
        XRotate = 0;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionVertical1850164(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter) {
    if (Filter === "SEMA03") {
        InsertUnionTape1850164(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter);
        return;
    }
    var _getCHeckRijiOutside = getCHeckRijiOutside(IdWall);
    var _getCHeckRijiInside = getCHeckRijiInside(IdWall);
    materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    ZRotate = 0;
    YRotate = 0;
    if (XRotate === 180) {
        if (_getCHeckRijiOutside === false) { return; }
        //function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate) {
        //Helpd(Element, CodeName, 250, Math.PI * 0.5, Math.PI * 1.5, 0);
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
    }

    if (XRotate === 0) {
        if (Filter === null) {
            if (_getCHeckRijiInside === false) {
                return;
            }
        }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
    }
    if (XRotate === 270) {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        XRotate = -0.5 * Math.PI;
    }
    if (XRotate === 90) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        //function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate) {
        //Helpd(Element, CodeName, 150, Math.PI * 0.5, 0, 0);
        XRotate = Math.PI * 0.5;
        YRotate = 0;
        ZRotate = 0;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionVertical1850162(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var _getCHeckRijiOutside = getCHeckRijiOutside(IdWall);
    var _getCHeckRijiInside = getCHeckRijiInside(IdWall);
    materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    ZRotate = 0;
    YRotate = 0;
    if (XRotate === 180) {
        if (_getCHeckRijiOutside === false) { return; }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
    }
    if (XRotate === 0) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
    }
    if (XRotate === 270) {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        XRotate = -0.5 * Math.PI;
    }
    if (XRotate === 90) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionVertical(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var _getCHeckRijiOutside = getCHeckRijiOutside(IdWall);
    var _getCHeckRijiInside = getCHeckRijiInside(IdWall);
    materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    ZRotate = 0;
    if (XRotate === 0) {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        XRotate = -0.5 * Math.PI;
        if (ZRotate === "90") {
            ZRotate = - (Math.PI * 0.5);
        }
    }
    if (XRotate === 1) {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        XRotate = -0.5 * Math.PI;
        if (ZRotate === "90") {
            ZRotate = - (Math.PI * 0.5);
        }
    }
    if (XRotate === 180) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
        if (ZRotate === "90") {
            ZRotate = (Math.PI * 0.5);
        }
    }
    if (ZRotate === "90") {
        ZRotate = Math.PI;
    }
    if (ZRotate === "2701") {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        //Helpd(Element, CodeName, 250, Math.PI * 0.5, Math.PI * 1.5, 0);

        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
    if (ZRotate === "270") {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnionVertical120(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var _getCHeckRijiOutside = getCHeckRijiOutside(IdWall);
    var _getCHeckRijiInside = getCHeckRijiInside(IdWall);
    materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    ZRotate = 0;
    YRotate = 0;
    if (XRotate === 180) {
        if (_getCHeckRijiOutside === false) { return; }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
    }
    if (XRotate === 0) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 1.5;
    }
    if (XRotate === 270) {
        if (_getCHeckRijiOutside === false) {
            return;
        }
        XRotate = -0.5 * Math.PI;
    }
    if (XRotate === 90) {
        if (_getCHeckRijiInside === false) {
            return;
        }
        XRotate = Math.PI * 0.5;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
//function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate) { 
function InsertUnionGancho1920811(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var YRotate = 0;
    if (XRotate === 90) {
        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = 0;
    }

    if (XRotate === 270) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI;
        ZRotate = 0;
    }

    if (XRotate === 180) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }

    if (XRotate === 0) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
 
    if (XRotate === 1) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }
    if (XRotate === 181) {
        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = Math.PI;
    }





    /*    function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate)*/




    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x4C8DF8 });

    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertUnion(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    /*    var yDef = GetYXDef(CodeName, y);*/
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });

    //function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate) {
    //Helpd(Element, CodeName, 200, Math.PI * 0.5, Math.PI * 0.5, 0);
    if (ZRotate === "0Coodinate") {
        XRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
        ZRotate = 0;
    }
    if (ZRotate === "270") {
        XRotate = Math.PI * 1.5;
        ZRotate = 0;
        YRotate = 0;
    }
    if (ZRotate === "0") {
        XRotate = Math.PI * 1.5;
        ZRotate = 0;
        YRotate = Math.PI * 0.5;
    }
    if (ZRotate === "90") {
        XRotate = Math.PI * 0.5;
        ZRotate = 0;
        YRotate = 0;
    }
    if (ZRotate === "180") {
        XRotate = Math.PI * 0.5;
        ZRotate = 0;
        YRotate = Math.PI * 1.5;
    }
    if (ZRotate === "270M") {
        XRotate = 0;
        ZRotate = Math.PI * 1.5;
        YRotate = Math.PI * 0.5;
    }
    if (ZRotate === "0M") {
        XRotate = 0;
        ZRotate = 0;
        YRotate = Math.PI * 0.5;
    }
    if (ZRotate === "90M") {
        XRotate = 0;
        ZRotate = Math.PI * 0.5;
        YRotate = Math.PI * 0.5;
    }
    if (ZRotate === "180M") {
        XRotate = Math.PI;
        ZRotate = 0;
        YRotate = Math.PI * 1.5;
    }
    if (ZRotate === "180S") {
        XRotate = Math.PI * 1.5;
        ZRotate = 0;
        YRotate = Math.PI * 1.5;
    }

    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    /*    y = parseInt(yDef);*/

    InsertWall = 0;
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
}
function InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate) {
    var partName = Id;
    var _getCheckIfIsPilar = getCheckIfIsPilar(IdWall);
    var _getCHeckPropInside = getCHeckPropInside(IdWall);
    var _getCHeckPropOutside = getCHeckPropOutside(IdWall);
    var _getCHeckPropInsideInf = getCHeckPropInsideInf(IdWall);
    var _getCHeckPropOutsideInf = getCHeckPropOutsideInf(IdWall);
    //var texInf = Element.substring(11, 24);

    if (_getCheckIfIsPilar === false)
    {
        //""
        if (Filter === "PuntalInf0") { if (_getCHeckPropInsideInf !== true) { return; } }
        if (Filter === "PuntalInf180") { if (_getCHeckPropInsideInf !== true) { return; } }
        if (Filter === "PuntalInf270") { if (_getCHeckPropOutsideInf !== true) { return; } }
        if (Filter === "PuntalInf90") { if (_getCHeckPropInsideInf !== true) { return; } }
        if (Filter === "PuntalInf270") { if (_getCHeckPropOutsideInf !== true) { return; } }
        if (Filter === "Puntal0") { if (_getCHeckPropInside !== true) { return; } }
        if (Filter === "Puntal180") { if (_getCHeckPropInside !== true) { return; } }
        if (Filter === "Puntal90") { if (_getCHeckPropInside !== true) { return; } }
        if (Filter === "Puntal270") {
            if (_getCHeckPropOutside !== true) {
                return;
            }
        }

    }
    else {
        if (Filter === "Puntal270") {
            if (_getCHeckPropInside !== true) {
                return;
            }
        }
        if (Filter === "Puntal0") {
            if (_getCHeckPropInside !== true) {
                return;
            }
        }
        if (Filter === "PuntalInf270") {
            if (_getCHeckPropInsideInf !== true) {
                return;
            }
        }
        if (Filter === "PuntalInf0") {
            if (_getCHeckPropInsideInf !== true) {
                return;
            }
        }
        if (Filter === "PuntalInf180") {
            if (_getCHeckPropInsideInf !== true) {
                return;
            }
        }
    }


    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    var yDef = GetYXDef(CodeName, y);
    ScaleY = GetScaleY(CodeName);

    if (XRotate === 270) {
        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = 0;
    }

    if (XRotate === 0) {
        XRotate = Math.PI * 1.5;
        YRotate = - Math.PI * 1.5;
        ZRotate = 0;
    }
    if (XRotate === 180) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
    }
    if (XRotate === 90) {
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI;
        ZRotate = 0;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(yDef);
    InsertWall = 0;
    var material = new THREE.MeshLambertMaterial({ color: 0x908F8E });
    if (CodeName.substr(0, 4) === "H0_2") {
        material = new THREE.MeshLambertMaterial({ color: 0xDCDBDA });
    }
    var NameUnion = "Atk60_" + CodeName + "_" + partName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, material);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
};
function InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, filter) {

    if (CodeName === "Tablon10x15x5") {
        var j = 1;
    }

    var name = CodeName.substr(0, 3);
    var _getCHeckBracketInside = getCHeckBracketInside(IdWall);
    var _getCHeckBracketOutside = getCHeckBracketOutside(IdWall);
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var YRotate = 0;
    var CadRotation = 0;
    var yDef = GetYXDef(CodeName, y);
    ScaleY = GetScaleY(CodeName);
    var filterd = (parseInt(filter) / 10);
    var filterd2 = ((parseInt(filter) / 10) / 2) - 10;

    if (XRotate === 270) {
        if (_getCHeckBracketOutside === false) {
            return;
        }
        XRotate = Math.PI * 1.5;
        YRotate = 0;
        ZRotate = 0;
        if (CodeName === "Tablon10x15x5") {
            x = x - filterd2;
            ScaleX = filterd;
        }

        if (CodeName === "Tablon10x15x5-2") {
            x = x - filterd2;
            ScaleX = filterd;
        }
        CadRotation = 90;
    }
    if (XRotate === 90) {
        if (_getCHeckBracketInside === false) {
            return;
        }
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI;
        ZRotate = 0;
        if (CodeName === "Tablon10x15x5") {
            x = x - filterd2;
        }
        if (CodeName === "Tablon10x15x5-2") {
            x = x - filterd2;
        }
        CadRotation = 270;
    }

    if (XRotate === 0) {
        if (_getCHeckBracketInside === false) {
            return;
        }
        XRotate = Math.PI * 1.5;
        YRotate = - Math.PI * 1.5;
        ZRotate = 0;
        if (CodeName === "Tablon10x15x5") {
            y = y - filterd2;
            ScaleX = filterd;
        }
        if (CodeName === "Tablon10x15x5-2") {
            y = y - filterd2 + 3;
            ScaleX = filterd;
        }
    }
    if (XRotate === 180) {
        if (_getCHeckBracketOutside === false) {
            return;
        }
        XRotate = Math.PI * 1.5;
        YRotate = Math.PI * 1.5;
        ZRotate = 0;
        if (CodeName === "Tablon10x15x5") {
            y = y - filterd2;
            ScaleX = filterd;
        }
        if (CodeName === "Tablon10x15x5-2") {
            y = y - filterd2 + 3;
            ScaleX = filterd;
        }
    }

    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;

    var material = new THREE.MeshLambertMaterial({ color: 0xF9B02E });
    if (CodeName === "4120000042") {
        material = new THREE.MeshLambertMaterial({ color: 0x0060D3 });
    }
    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, material);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.rotation.z = YRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        meshUnion1.CadRotation = CadRotation;
        scene.add(meshUnion1);
    });
    return;
};
function InsertUnion1(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, ParameteerFilter) {
    if (Filter === "SExS01") {
        FilerRigiSExS01(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, ParameteerFilter);
    }

    if (Filter === "SEMA03") {
        FilerRigiSExS01(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, ParameteerFilter);
    }

    var name = CodeName.substr(0, 3);
    if (name === "230") {
        InsertDywidag(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "10443020" || CodeName === "10443020-2") {
        InsertDywidagPlaca(Filter, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "7238001") {
        InsertDywidagPlaca(Filter, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "1920894") {
        InsertDywidagPlaca(Filter, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "10000221") {
        InsertUnionRegulable(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "10000221B") {
        InsertUnionRegulable(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "10004220") {
        InsertUnion(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "1850162") {
        InsertUnionVertical1850162(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }


    if (CodeName === "1850164") {
        InsertUnionVertical1850164(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter);
        return;
    }

    if (CodeName === "10443020") {
        InsertUnionVertical(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "1850163") {
        InsertUnionVertical120(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "HTipo1") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "HTipo0") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "HTipo2") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "HTipo3") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "HTipo3-2") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "HTipo4-41") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "Puntal27") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "Puntal27G") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName.substr(0, 2) === "H0") {
        InsertPuntal(Filter, Id, IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }

    if (CodeName === "Tablon10x15x5") {
        InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter);
        return;
    }
    if (CodeName === "Tablon10x15x5-2") {
        InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, Filter);
        return;
    }

    if (CodeName === "Tablon24x15x5") {
        InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, 0);
        return;
    }
    if (CodeName === "Tablon24x15x5-2") {
        InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, 0);
        return;
    }
    if (CodeName === "4120000042") {
        InsertBracket(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate, 0);
        return;
    }

    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;
    var yDef = GetYXDef(CodeName, y);
    ScaleY = GetScaleY(CodeName);
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    if (XRotate === 0) {
        XRotate = -0.5 * Math.PI;
        if (ZRotate === "90") {
            ZRotate = - (Math.PI * 0.5);
        }
    }
    if (XRotate === 180) {
        XRotate = Math.PI * 0.5;
        if (ZRotate === "90") {
            ZRotate = (Math.PI * 0.5);
        }
    }
    if (ZRotate === 90) {
        ZRotate = Math.PI;
        //if (ZRotate === "90") {
        // ZRotate = - (Math.PI * 0.5);
        //}
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(yDef);
    InsertWall = 0;

    var NameUnion = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshUnion1 = new THREE.Mesh(geometry, materialUnion1);
        meshUnion1.position.set(x, z, y);
        meshUnion1.rotation.x = XRotate;
        meshUnion1.rotation.y = ZRotate;
        meshUnion1.name = NameUnion;
        meshUnion1.scale.set(1, 1, 1);
        meshUnion1.scale.x = ScaleX;
        meshUnion1.scale.z = ScaleZ;
        meshUnion1.scale.y = ScaleY;
        meshUnion1.IdWall = IdWall;
        scene.add(meshUnion1);
    });
    return;
};
function InsertPanel(Filer, Id, IdWall, _type, LongDimTypeHorizontal, LongDimTypeHorizontalT, LongDimTypeVertical, CodeName, XRotate, ZRotate, Element, x, y, z, material, ElementMirrow) {
    var CadRotation = 0;
    if (CodeName === "27000000" || CodeName === "12000000" || CodeName === "PanelExt240") {
        material = new THREE.MeshLambertMaterial({ color: 0xF8F84C });
    }
    if (CodeName === "1920811") {
        InsertUnionGancho1920811(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "dywidag02") {
        InsertUnionGancho1920811(IdWall, CodeName, XRotate, Element, x, y, z, ZRotate);
        return;
    }
    if (CodeName === "1920811") {
        material = new THREE.MeshLambertMaterial({ color: 0x4C8DF8 });
    }

    var _getCHeckDimWall = getCHeckDimWall(IdWall);
    if (_getCHeckDimWall === false) {
        LongDimTypeHorizontal = null;
        LongDimTypeHorizontalT = null;
        LongDimTypeVertical = null;
    }
    if (_type === "WallEsqTLe") {
        ZRotate = Math.PI * 0.5;
    }
    if (XRotate === 180) {
        ZRotate = + Math.PI;
    }
    if (XRotate === 90) {
        ZRotate = + Math.PI * 1.5;

    }
    if (XRotate === 270) {
        ZRotate = - Math.PI * 1.5;

    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NamePanel = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshPanel = new THREE.Mesh(geometry, material);
        meshPanel.position.set(x, z, y);
        meshPanel.rotation.x = -0.5 * Math.PI;
        meshPanel.name = NamePanel;
        meshPanel.rotation.z = ZRotate;
        meshPanel.scale.set(1, 1, 1);

        meshPanel.castShadow = true;
        meshPanel.receiveShadow = true;

        meshPanel.scale.x = 100;
        meshPanel.scale.z = 100;
        meshPanel.scale.y = 100;
        meshPanel.LongDimTypeHorizontalT = LongDimTypeHorizontalT;
        meshPanel.LongDimTypeHorizontal = LongDimTypeHorizontal;
        meshPanel.LongDimTypeVertical = LongDimTypeVertical;
        meshPanel.DimType = _type;
        meshPanel.TypeElement = "Panel_ATK60";
        meshPanel.IdWall = IdWall;
        meshPanel.CadRotation = XRotate;
        scene.add(meshPanel);
    });
    return;
};
function InsertPanelF(Filter, Id, _type, DimType, CodeName, XRotate, ZRotate, Element, x, y, z, material, ElementMirrow) {

    if (_type === "WallEsqTLe") {
        ZRotate = Math.PI * 0.5;
    }

    if (XRotate === 180) {
        ZRotate = + Math.PI;
    }
    if (XRotate === 90) {
        ZRotate = + Math.PI * 1.5;
    }
    if (XRotate === 270) {
        ZRotate = - Math.PI * 1.5;
    }
    Edit_Wall = 20;
    x = parseInt(x);
    y = parseInt(y);
    InsertWall = 0;
    var NamePanel = "Atk60_" + CodeName;
    var loaderPanel = new THREE.STLLoader();
    loaderPanel.load(Element, function (geometry) {
        var meshPanel = new THREE.Mesh(geometry, material);
        meshPanel.position.set(x, z, y);
        meshPanel.rotation.x = -0.5 * Math.PI;
        meshPanel.name = NamePanel;
        meshPanel.rotation.z = ZRotate;
        meshPanel.scale.set(1, 1, 1);
        meshPanel.scale.x = 100;
        meshPanel.scale.z = 100;
        meshPanel.scale.y = 100;
        meshPanel.horizontalDim = false;
        meshPanel.TypeElement = "Panel_ATK60_Fenolico";
        scene.add(meshPanel);
    });
    return;
};
function InsertDimVerticalT(Panel, DimType, XRotate, ZRotate, x, z, y,) {
    x = parseInt(x);
    y = parseInt(y);
    //add Dimension horizontal

    if (DimType === "HorizontalT") {
        var radius = 1;
        var x2 = x + 150;
        var y2 = y + 150;

        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y));
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 160));
        const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimLeft = new THREE.Line(LineLeft, materialDim);
        LineDimLeft.name = "DinHorizontal";
        scene.add(LineDimLeft);
        DrawDot(x, 0.01 + z, y + 150, "DinHorizontal");
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + Panel, 0.01, y));
        pointsDim.push(new THREE.Vector3(x + Panel, 0.01, y + 160));
        const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimRight = new THREE.Line(LineRight, materialDim);
        LineDimRight.name = "DinHorizontal";
        scene.add(LineDimRight);

        DrawDot(x + Panel, 0.01 + z, y + 150, "DinHorizontal");

        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 150));
        pointsDim.push(new THREE.Vector3(x + 40, 0.01, y + 150));
        const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
        LineTopLeft.name = "DinHorizontal";
        scene.add(LineTopLeft);

        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + 50, 0.01, y + 150));
        pointsDim.push(new THREE.Vector3(x + 90, 0.01, y + 150));
        const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
        LineTopRight.name = "DinHorizontal";
        scene.add(LineTopRight)

        var size = 256;
        var geom = new THREE.SphereGeometry(radius, 32, 16);
        var mat = new THREE.MeshBasicMaterial({
            color: Math.random() * 0xFFFFFF,
            wireframe: true
        });
        var _dim = new THREE.Mesh(geom, mat);
        _dim.name = "2,70";
        var canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        var ctx = canvas.getContext("2d");
        ctx.font = "25pt Arial";
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
        sprite.position.x = x + Panel / 2;
        sprite.position.y = -10;
        sprite.position.z = y2 + 10;
        _dim.add(sprite);
        _dim.name = "DinHorizontal";
        scene.add(_dim);
    }
};
function InsertDimHorizontalT(Type, Panel, x, z, y) {
    x = parseInt(x);
    y = parseInt(y);
    z = 0;
    var pointsDim = [];
    var radius = 1;
    if (Type === "0") {
        pointsDim.push(new THREE.Vector3(x, 0.01, y));
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 210));
        const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimLeft = new THREE.Line(LineLeft, materialDim);
        LineDimLeft.name = "DinHorizontal";
        scene.add(LineDimLeft);
        DrawDot(x, 0.01 + z, y + 200, "DinHorizontal");

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + (Panel / 10), 0.01, y));
        pointsDim.push(new THREE.Vector3(x + (Panel / 10), 0.01, y + 210));
        const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimRight = new THREE.Line(LineRight, materialDim);
        LineDimRight.name = "DinHorizontal";
        scene.add(LineDimRight);
        DrawDot(x + (Panel / 10), 0.01 + z, y + 200, "DinHorizontal");

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 200));
        pointsDim.push(new THREE.Vector3(x + ((Panel / 10) / 2) - 15, 0.01, y + 200));
        const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
        LineTopLeft.name = "DinHorizontal";
        scene.add(LineTopLeft);

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + ((Panel / 10) / 2) + 15, 0.01, y + 200));
        pointsDim.push(new THREE.Vector3(x + (Panel / 10), 0.01, y + 200));
        const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
        LineTopRight.name = "DinHorizontal";
        scene.add(LineTopRight)

        var size = 256;
        var geom = new THREE.SphereGeometry(radius, 32, 16);
        var mat = new THREE.MeshBasicMaterial({
            color: Math.random() * 0xFFFFFF,
            wireframe: true
        });
        var _dimT = new THREE.Mesh(geom, mat);
        _dimT.name = (Panel / 1000).toFixed(3);
        var canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        var ctx = canvas.getContext("2d");
        ctx.font = "25pt Arial";
        ctx.fillStyle = '#000000';
        ctx.textAlign = "center";
        ctx.fillText(_dimT.name, size / 2, size / 3);
        var tex = new THREE.Texture(canvas);
        tex.needsUpdate = true;
        var spriteMat = new THREE.SpriteMaterial({
            map: tex
        });
        var spriteT = new THREE.Sprite(spriteMat);
        spriteT.scale.set(100, 100, 1);
        spriteT.position.x = x + (Panel / 10) / 2;
        spriteT.position.y = -10;
        spriteT.position.z = y + 210;
        _dimT.add(spriteT);
        _dimT.name = "DinHorizontal";
        scene.add(_dimT);
    }
    if (Type === "90") {
        pointsDim.push(new THREE.Vector3(x, 0.01, y));
        pointsDim.push(new THREE.Vector3(x - 210, 0.01, y));
        const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimLeft = new THREE.Line(LineLeft, materialDim);
        LineDimLeft.name = "DinHorizontal";
        scene.add(LineDimLeft);
        DrawDot(x - 200, 0.01 + z, y, "DinHorizontal");

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y - (Panel / 10)));
        pointsDim.push(new THREE.Vector3(x - 210, 0.01, y - (Panel / 10)));
        const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimRight = new THREE.Line(LineRight, materialDim);
        LineDimRight.name = "DinHorizontal";
        scene.add(LineDimRight);
        DrawDot(x - 200, 0.01 + z, y - (Panel / 10), "DinHorizontal");

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x - 200, 0.01, y));
        pointsDim.push(new THREE.Vector3(x - 200, 0.01, y - ((Panel / 10) / 2) + 15));
        const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
        LineTopLeft.name = "DinHorizontal";
        scene.add(LineTopLeft);

        pointsDim = [];
        pointsDim.push(new THREE.Vector3(x - 200, 0.01, y - ((Panel / 10) / 2) - 15));
        pointsDim.push(new THREE.Vector3(x - 200, 0.01, y - (Panel / 10)));
        const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
        LineTopRight.name = "DinHorizontal";
        scene.add(LineTopRight)

        var size = 256;
        var geom = new THREE.SphereGeometry(radius, 32, 16);
        var mat = new THREE.MeshBasicMaterial({
            color: Math.random() * 0xFFFFFF,
            wireframe: true
        });
        var _dimT = new THREE.Mesh(geom, mat);
        _dimT.name = (Panel / 1000).toFixed(3);
        var canvas = document.createElement('canvas');
        canvas.width = size;
        canvas.height = size;
        var ctx = canvas.getContext("2d");
        ctx.font = "25pt Arial";
        ctx.fillStyle = '#000000';
        ctx.textAlign = "center";
        ctx.fillText(_dimT.name, size / 2, size / 3);
        var tex = new THREE.Texture(canvas);
        tex.needsUpdate = true;
        var spriteMat = new THREE.SpriteMaterial({
            map: tex
        });
        var spriteT = new THREE.Sprite(spriteMat);
        spriteT.scale.set(100, 100, 1);
        spriteT.position.x = x - 210;
        spriteT.position.y = -10;
        spriteT.position.z = y - (Panel / 10) / 2;
        _dimT.add(spriteT);
        _dimT.name = "DinHorizontal";
        scene.add(_dimT);
    }
};
function InsertDimWallH(Panel, DimType, x, z, y) {
    x = parseInt(x);
    y = parseInt(y);
    //add Dimension horizontal
    var radius = 1;
    var y2 = y + 150;
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y));
    pointsDim.push(new THREE.Vector3(x, 0.01, y + 110));
    const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimLeft = new THREE.Line(LineLeft, materialDim);
    LineDimLeft.name = "Line_Wall";
    scene.add(LineDimLeft);
    DrawDot(x, 0.01 + z, y + 100, "Line_Wall");
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + Panel, 0.01, y));
    pointsDim.push(new THREE.Vector3(x + Panel, 0.01, y + 110));
    const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineDimRight = new THREE.Line(LineRight, materialDim);
    LineDimRight.name = "Line_Wall";
    scene.add(LineDimRight);
    DrawDot(x + Panel, 0.01 + z, y + 100, "Line_Wall");
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x, 0.01, y + 100));
    pointsDim.push(new THREE.Vector3(x + 40, 0.01, y + 100));
    const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
    LineTopLeft.name = "Line_Wall";
    scene.add(LineTopLeft);
    var pointsDim = [];
    pointsDim.push(new THREE.Vector3(x + 50, 0.01, y + 100));
    pointsDim.push(new THREE.Vector3(x + Panel, 0.01, y + 100));
    const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
    const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
    LineTopRight.name = "Line_Wall";
    scene.add(LineTopRight)
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = (Panel / 100).toFixed(3);
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
    ctx.fillText(_dim.name, size / 2, size / 3);

    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = x + (Panel) / 2;
    sprite.position.y = -10;
    sprite.position.z = y2 + 10;
    _dim.add(sprite);
    _dim.name = "Line_Wall";
    scene.add(_dim);
};
function InsertDim(Type, Panel, x, z, y,) {
    x = parseInt(x);
    y = parseInt(y);
    var radius = 1;
    if (Type === "0") {
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y));
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 110));
        const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimLeft = new THREE.Line(LineLeft, materialDim);
        LineDimLeft.name = "DinHorizontal";
        scene.add(LineDimLeft);
        DrawDot(x, 0.01 + z, y + 100, "DinHorizontal");
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + Panel / 10, 0.01, y));
        pointsDim.push(new THREE.Vector3(x + Panel / 10, 0.01, y + 110));
        const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimRight = new THREE.Line(LineRight, materialDim);
        LineDimRight.name = "DinHorizontal";
        scene.add(LineDimRight);
        DrawDot(x + Panel / 10, 0.01 + z, y + 100, "DinHorizontal");
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y + 100));
        pointsDim.push(new THREE.Vector3(x + 40, 0.01, y + 100));
        const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
        LineTopLeft.name = "DinHorizontal";
        scene.add(LineTopLeft);

        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x + 50, 0.01, y + 100));
        pointsDim.push(new THREE.Vector3(x + Panel / 10, 0.01, y + 100));
        const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
        LineTopRight.name = "DinHorizontal";
        scene.add(LineTopRight)
        var size = 256;
        var geom = new THREE.SphereGeometry(radius, 32, 16);
        var mat = new THREE.MeshBasicMaterial({
            color: Math.random() * 0xFFFFFF,
            wireframe: true
        });
        var _dim = new THREE.Mesh(geom, mat);
        _dim.name = (Panel / 1000).toFixed(3);
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
        sprite.position.x = x + (Panel / 10) / 2;
        sprite.position.y = -10;
        sprite.position.z = y + 110;
        _dim.add(sprite);
        _dim.name = "DinHorizontal";
        scene.add(_dim);
    }
    if (Type === "90") {
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y));
        pointsDim.push(new THREE.Vector3(x - 100, 0.01, y));
        const LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimLeft = new THREE.Line(LineLeft, materialDim);
        LineDimLeft.name = "DinHorizontal";
        scene.add(LineDimLeft);
        DrawDot(x - 100, 0.01 + z, y, "DinHorizontal");
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x, 0.01, y - Panel / 10));
        pointsDim.push(new THREE.Vector3(x - 110, 0.01, y - Panel / 10));
        const LineRight = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineDimRight = new THREE.Line(LineRight, materialDim);
        LineDimRight.name = "DinHorizontal";
        scene.add(LineDimRight);
        DrawDot(x - 100, 0.01 + z, y - Panel / 10, "DinHorizontal");
        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x - 100, 0.01, y));
        pointsDim.push(new THREE.Vector3(x - 100, 0.01, y - 40));
        const LineTopLeftPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopLeft = new THREE.Line(LineTopLeftPoint, materialDim);
        LineTopLeft.name = "DinHorizontal";
        scene.add(LineTopLeft);

        var pointsDim = [];
        pointsDim.push(new THREE.Vector3(x - 100, 0.01, y - 50));
        pointsDim.push(new THREE.Vector3(x - 100, 0.01, y - Panel / 10));
        const LineTopRightPoint = new THREE.BufferGeometry().setFromPoints(pointsDim);
        const LineTopRight = new THREE.Line(LineTopRightPoint, materialDim);
        LineTopRight.name = "DinHorizontal";
        scene.add(LineTopRight)
        var size = 256;
        var geom = new THREE.SphereGeometry(radius, 32, 16);
        var mat = new THREE.MeshBasicMaterial({
            color: Math.random() * 0xFFFFFF,
            wireframe: true
        });
        var _dim = new THREE.Mesh(geom, mat);
        _dim.name = (Panel / 1000).toFixed(3);
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
        sprite.position.x = x - 110;
        sprite.position.y = -10;
        sprite.position.z = y - (Panel / 10) / 2;
        _dim.add(sprite);
        _dim.name = "DinHorizontal";
        scene.add(_dim);
    }
};
 
function DrawDot(x, y, z, name) {
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
    scene.add(pt);
};
//Check





