function getCHeckPropInsideInf(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall.toString() === IdWall) {
                if (scene.children[i].CHeckPropInsideInf === "False") {
                    return false;
                }
                if (scene.children[i].CHeckPropInsideInf === "True") {
                    return true;
                }
                return scene.children[i].CHeckPropInsideInf;
            }
        }
    }
    return false;
};
function getCHeckPropOutsideInf(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall.toString() === IdWall) {
                if (scene.children[i].CHeckPropOutsideInf === "False") {
                    return false;
                }
                if (scene.children[i].CHeckPropOutsideInf === "True") {
                    return true;
                }
                return scene.children[i].CHeckPropOutsideInf;
            }
        }
    }
    return false;
};
function getCHeckPropInside(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall.toString() === IdWall) {
                if (scene.children[i].CHeckPropInside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckPropInside === "True") {
                    return true;
                }
                return scene.children[i].CHeckPropInside;
            }
        }
    }
    return false;
};
function getCHeckPropOutside(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall.toString() === IdWall) {
                if (scene.children[i].CHeckPropOutside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckPropOutside === "True") {
                    return true;
                }
                return scene.children[i].CHeckPropOutside;
            }
        }
    }
    return false;
};
function getCHeckBracketInside(IdWall) {
    /*   var _listMesh = HelpSelectMesh();*/
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                if (scene.children[i].CHeckBracketInside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckBracketInside === "True") {
                    return true;
                }
                return scene.children[i].CHeckBracketInside;
            }
        }
    }
    return false;
};
function getCHeckBracketOutside(IdWall) {
    /*   var _listMesh = HelpSelectMesh();*/
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                if (scene.children[i].CHeckBracketOutside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckBracketOutside === "True") {
                    return true;
                }
                return scene.children[i].CHeckBracketOutside;
            }
        }
    }
    return false;
};
function getCHeckRijiInside(IdWall) {
    /*   var _listMesh = HelpSelectMesh();*/
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                if (scene.children[i].CHeckRijiInside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckRijiInside === "True") {
                    return true;
                }
                return scene.children[i].CHeckRijiInside;
            }
        }
    }
    return false;
};
function getCHeckRijiOutside(IdWall) {
    /*   var _listMesh = HelpSelectMesh();*/
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                if (scene.children[i].CHeckRijiOutside === "False") {
                    return false;
                }
                if (scene.children[i].CHeckRijiOutside === "True") {
                    return true;
                }
                return scene.children[i].CHeckRijiOutside;
            }
        }
    }
    return false;
};
function getCHeckDimWall(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                return scene.children[i].CHeckDimWall;
            }
        }
    }
    return false;
}
function getCheckIfIsPilar(IdWall) {
    var toTest = "";
    for (var i = 0; i < scene.children.length; i++) {
        toTest = scene.children[i].idWall;
        if (toTest !== undefined) {
            toTest = scene.children[i].idWall.toString()
            if (toTest === IdWall) {
                if (scene.children[i].MeshTypeWall === "Pilar") {
                    return true;
                }
            }

        }
    }
    return false;
};
function getGrup(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                return scene.children[i].Grupo;
            }
        }
    }
    return "";
};
// Help

function Help1850162(x, XRotate, YRotate, ZRotate) {
    var Element = "../../Content/DesignTools/Stl/ATK60/Puntal270.stl";
    var CodeName = "Puntal27";
    /*    "270"*/
    Helpd(Element, CodeName, 0, Math.PI * 1.5, 0, 0);
    //90
    Helpd(Element, CodeName, 0, Math.PI * 1.5, Math.PI * 1.5, 0);
    //180
    Helpd(Element, CodeName, 0, Math.PI * 1.5, Math.PI, 0);
    //0
    Helpd(Element, CodeName, 0, Math.PI * 1.5, - Math.PI * 1.5, 0);
    Helpd(Element, CodeName, 200, - Math.PI * 1.5, 0, 0);
    XRotate = 0;
 /*1*/   Helpd(Element, CodeName, 0, 0, 0, 0);
 /*2*/    Helpd(Element, CodeName, 50, 0, Math.PI * 0.5, 0);
 /*3*/    Helpd(Element, CodeName, 100, 0, Math.PI * 1.5, 0);

// XRotate= Math.PI * 0.5;
 /*4*/    Helpd(Element, CodeName, 150, Math.PI * 0.5, 0, 0);
 /*5*/    Helpd(Element, CodeName, 200, Math.PI * 0.5, Math.PI * 0.5, 0);
 /*6*/    Helpd(Element, CodeName, 250, Math.PI * 0.5, Math.PI * 1.5, 0);

// XRotate= Math.PI;
 /*7*/    Helpd(Element, CodeName, 300, Math.PI, 0, 0);
 /*8*/   Helpd(Element, CodeName, 350, Math.PI, Math.PI * 0.5, 0);
 /*9*/    Helpd(Element, CodeName, 400, Math.PI, Math.PI * 1.5, 0);

// XRotate= Math.PI * 1.5;
 /*10*/    Helpd(Element, CodeName, 450, Math.PI * 1.5, 0, 0);
 /*11*/   Helpd(Element, CodeName, 500, Math.PI * 1.5, Math.PI * 0.5, 0);
 /*12*/  Helpd(Element, CodeName, 550, Math.PI * 1.5, Math.PI * 1.5, 0);
    // YRotate= 0;
};
function Helpd(Element, CodeName, x, XRotate, YRotate, ZRotate) {
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    var y = 0;
    var z = 0;
    var ScaleX = 100;
    var ScaleY = 100;
    var ScaleZ = 100;

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
        scene.add(meshUnion1);
    });

    return;
};
function getallMeshidWall() {
    var l = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].idWall > 0) {
                l.push
                    ({
                        objList: scene.children[i]
                    });
            }
        }
    }
    return l;
};
function getCHeckimWall(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === IdWall) {
                return scene.children[i];
            }
        }
    }
    return scene.children[i];
}
function getCHeckWall90(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall === IdWall.substr(0, 22)) {
                return scene.children[i];
            }
        }
    }
}

function getCHeckWall0(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            var v = scene.children[i].idWall;
            var v1 = IdWall;
            if (scene.children[i].idWall === IdWall) {
                return scene.children[i];
            }
        }
    }
}

/*Angel*/
function getCHeckWallMaster(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh")
        {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall === IdWall.substr(0, 22)) {
                return scene.children[i];
            }
        }
    }
    return null;
}


function getCHeckWall180(IdWall) {
    var _listMesh = HelpSelectMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].idWall === IdWall.substr(0, 22)) {
                return scene.children[i];
            }
        }
    }
}
function getallMeshToScene() {
    var l = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            l.push
                ({
                    objList: scene.children[i]
                });
        }
    }
    return l;
};
function getallMeshToSceneTipeMesh() {
    var l = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].MeshTypeWall !== undefined) {
            l.push
                ({
                    objList: scene.children[i]
                });
        }
    }
    return l;
};




function HelpElement() {
    var Element = "../../Content/DesignTools/Stl/ATK60/1920811.stl";
    var CodeName = "1920811";
    // XRotate=0;
    Helpd(Element, CodeName, -500, Math.PI * 1.5, Math.PI * 0.5, 0);

 /*1*/   Helpd(Element, CodeName, 0, 0, 0, 0);
 /*2*/    Helpd(Element, CodeName, 50, 0, Math.PI * 0.5, 0);
 /*3*/    Helpd(Element, CodeName, 100, 0, Math.PI * 1.5, 0);

// XRotate= Math.PI * 0.5;
 /*4*/    Helpd(Element, CodeName, 150, Math.PI * 0.5, 0, 0);
 /*5*/    Helpd(Element, CodeName, 200, Math.PI * 0.5, Math.PI * 0.5, 0);
 /*6*/    Helpd(Element, CodeName, 250, Math.PI * 0.5, Math.PI * 1.5, 0);

// XRotate= Math.PI;
 /*7*/    Helpd(Element, CodeName, 300, Math.PI, 0, 0);
 /*8*/   Helpd(Element, CodeName, 350, Math.PI, Math.PI * 0.5, 0);
 /*9*/    Helpd(Element, CodeName, 400, Math.PI, Math.PI * 1.5, 0);

// XRotate= Math.PI * 1.5;
 /*10*/    Helpd(Element, CodeName, 450, Math.PI * 1.5, 0, 0);
 /*11*/   Helpd(Element, CodeName, 500, Math.PI * 1.5, Math.PI * 0.5, 0);
 /*12*/  Helpd(Element, CodeName, 550, Math.PI * 1.5, Math.PI * 1.5, 0);
    // YRotate= 0;

 /*13*/    Helpd(Element, CodeName, 650, 0, Math.PI * 1.5, 0);
 /*14*/    Helpd(Element, CodeName, 700, 0, Math.PI * 0.5, 0);
 /*15*/    Helpd(Element, CodeName, 750, 0, Math.PI, 0);

 /*16*/    Helpd(Element, CodeName, 850, 0, Math.PI * 1.5, Math.PI * 1.5);
 /*17*/    Helpd(Element, CodeName, 900, 0, Math.PI * 0.5, Math.PI * 1.5);
 /*18*/    Helpd(Element, CodeName, 950, 0, Math.PI, Math.PI * 1.5);

 /*19*/    Helpd(Element, CodeName, 1050, 0, Math.PI * 1.5, Math.PI * 0.5);
 /*20*/    Helpd(Element, CodeName, 1100, 0, Math.PI * 0.5, Math.PI * 0.5);
 /*21*/    Helpd(Element, CodeName, 1200, 0, Math.PI, Math.PI * 0.5);

 /*19*/    Helpd(Element, CodeName, 1250, Math.PI * 1.5, 0, 0);
 /*20*/    Helpd(Element, CodeName, 1300, Math.PI * 1.5, 0, Math.PI);
 /*21*/    Helpd(Element, CodeName, 1350, Math.PI * 1.5, 0, Math.PI * 1.5);

 /*19*/    Helpd(Element, CodeName, 1400, Math.PI * 1.5, 0, Math.PI * 0.5);
 /*20*/    Helpd(Element, CodeName, 1450, Math.PI * 1.5, Math.PI * 0.5, Math.PI * 0.5);
 /*21*/    Helpd(Element, CodeName, 1500, Math.PI * 1.5, Math.PI * 0.5, Math.PI * 1.5);


};



function RemoveFaces() {
    ResetSetup(false);
    for (var i = 0; i < scene.children.length; i++)
    {
        if (scene.children[i].name === null) { continue; }
        if (scene.children[i].name === undefined) { continue; }
        if (scene.children[i].name.substr(0, 10) === "Waal_Face1") {
            var Obface = scene.children[i];
            scene.remove(Obface);
        }
        if (scene.children[i].typeDim === "DimWall") {
            var ObDinWall = scene.children[i];
            scene.remove(ObDinWall);
        }
    }
};
function AddFaces() {
    ResetSetup(false);
    var l = getallMeshToSceneTipeMesh();
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].MeshTypeWall === 'Remate90_Wood') {
        //    AddFaces_Remate90(scene.children[i]);
        }

        if (scene.children[i].MeshTypeWall === 'Wall_R000') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_10_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_20_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_30_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_40_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_50_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_60_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_70_00') {
            AddFaces_0(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_80_00') {
            AddFaces_0(scene.children[i]);
        }
 
        if (scene.children[i].MeshTypeWall === 'Wall_R900') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_10_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_20_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_30_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_40_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_50_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_60_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_70_90') {
            AddFaces_90(scene.children[i]);
        }
        if (scene.children[i].MeshTypeWall === 'Esq_80_90') {
            AddFaces_90(scene.children[i]);
        }
    }
};
function AddFaces_Remate90(Mesh) {
    //Top
    var j = 1;
    AddDimWall_90Static(Mesh, Mesh.position.x, Mesh.position.z, Mesh.scale.z * 10000, "", Mesh.scale.z * 10000);
/*    return;*/
    var RealPosition_X = Mesh.position.x - ((Mesh.scale.x * 1000) / 2);
    var RealPositionFrom_X = Mesh.position.x;
    var RealPosition_Y = Mesh.position.z + ((Mesh.scale.z * 1000) / 2);
    //From
    var RealPosition_Y_From = Mesh.position.z + (Mesh.scale.z * 1000 / 2);
    var RealPosition_Z_From = (Mesh.scale.y * 1000) / 2;
    //Back
    var RealPosition_X_Back = RealPositionFrom_X - (Mesh.scale.x * 1000 + 1);
    const Wall_texture = new THREE.TextureLoader().load("../../Content/DesignTools/Material/concrete.png");
    Wall_texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
    Wall_texture.colorSpace = THREE.SRGBColorSpace;
    const wallmeshMaterial = new THREE.MeshPhongMaterial({ map: Wall_texture });
    var LoadFace_000_Top = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_Top = new THREE.Mesh(LoadFace_000_Top, wallmeshMaterial);
    meshFace_000_Top.position.set(RealPosition_X, (Mesh.scale.y * 1000) + 1, RealPosition_Y);
    meshFace_000_Top.rotation.x = -0.5 * Math.PI;
    meshFace_000_Top.rotation.z = Math.PI;
    meshFace_000_Top.scale.set(1, 1, 1);
    meshFace_000_Top.scale.x = Mesh.scale.x;
    meshFace_000_Top.scale.y = Mesh.scale.z;
    meshFace_000_Top.scale.z = 0.001;
    meshFace_000_Top.name = "Waal_Face1_" + Mesh.IdWall;
    meshFace_000_Top.visible = true;
    scene.add(meshFace_000_Top);
    // From
    var LoadFace_000_From = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_From = new THREE.Mesh(LoadFace_000_From, wallmeshMaterial);
    meshFace_000_From.position.set(RealPositionFrom_X, RealPosition_Z_From, RealPosition_Y_From);
    meshFace_000_From.rotation.x = -0.5 * Math.PI;
    meshFace_000_From.rotation.z = Math.PI;
    meshFace_000_From.scale.set(1, 1, 1);
    meshFace_000_From.scale.x = 0.001;
    meshFace_000_From.scale.y = Mesh.scale.z;
    meshFace_000_From.scale.z = Mesh.scale.y;
    meshFace_000_From.name = "Waal_Face1_From" + Mesh.IdWall;
    meshFace_000_From.visible = true;
    scene.add(meshFace_000_From);

    //// Back
    var LoadFace_000_Back = new THREE.BoxGeometry(1000, 1000, 1000);
    LoadFace_000_Back = new THREE.Mesh(LoadFace_000_From, wallmeshMaterial);
    LoadFace_000_Back.position.set(RealPosition_X_Back, RealPosition_Z_From, RealPosition_Y_From);
    LoadFace_000_Back.rotation.x = -0.5 * Math.PI;
    LoadFace_000_Back.rotation.z = Math.PI;
    LoadFace_000_Back.scale.set(1, 1, 1);
    LoadFace_000_Back.scale.x = 0.001;
    LoadFace_000_Back.scale.y = Mesh.scale.z;
    LoadFace_000_Back.scale.z = Mesh.scale.y;
    LoadFace_000_Back.name = "Waal_Face1_Back" + Mesh.IdWall;
    LoadFace_000_Back.visible = true;
    scene.add(LoadFace_000_Back);
};
function AddFaces_90(Mesh)
{ 
    //Top
    AddDimWall_90Static(Mesh, Mesh.position.x, Mesh.position.z, Mesh.scale.z * 10000, "", Mesh.scale.z * 10000);
    return;
    var RealPosition_X = Mesh.position.x - ((Mesh.scale.x * 1000) / 2);
    var RealPositionFrom_X = Mesh.position.x;
    var RealPosition_Y = Mesh.position.z + ((Mesh.scale.z * 1000) / 2);
    //From
    var RealPosition_Y_From = Mesh.position.z + (Mesh.scale.z * 1000/2);
    var RealPosition_Z_From = (Mesh.scale.y * 1000) / 2;
    //Back
    var RealPosition_X_Back = RealPositionFrom_X - (Mesh.scale.x * 1000 + 1);
    const Wall_texture = new THREE.TextureLoader().load("../../Content/DesignTools/Material/concrete.png");
    Wall_texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
    Wall_texture.colorSpace = THREE.SRGBColorSpace;
    const wallmeshMaterial = new THREE.MeshPhongMaterial({ map: Wall_texture });
    var LoadFace_000_Top = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_Top = new THREE.Mesh(LoadFace_000_Top, wallmeshMaterial);
    meshFace_000_Top.position.set(RealPosition_X, (Mesh.scale.y * 1000) + 1, RealPosition_Y);
    meshFace_000_Top.rotation.x = -0.5 * Math.PI;
    meshFace_000_Top.rotation.z = Math.PI;
    meshFace_000_Top.scale.set(1, 1, 1);
    meshFace_000_Top.scale.x = Mesh.scale.x;
    meshFace_000_Top.scale.y = Mesh.scale.z;
    meshFace_000_Top.scale.z = 0.001;
    meshFace_000_Top.name = "Waal_Face1_" + Mesh.IdWall;
    meshFace_000_Top.visible = true;
    scene.add(meshFace_000_Top);
    // From
    var LoadFace_000_From = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_From = new THREE.Mesh(LoadFace_000_From, wallmeshMaterial);
    meshFace_000_From.position.set(RealPositionFrom_X, RealPosition_Z_From, RealPosition_Y_From);
    meshFace_000_From.rotation.x = -0.5 * Math.PI;
    meshFace_000_From.rotation.z = Math.PI;
    meshFace_000_From.scale.set(1, 1, 1);
    meshFace_000_From.scale.x = 0.001;
    meshFace_000_From.scale.y = Mesh.scale.z;
    meshFace_000_From.scale.z = Mesh.scale.y;
    meshFace_000_From.name = "Waal_Face1_From" + Mesh.IdWall;
    meshFace_000_From.visible = true;
    scene.add(meshFace_000_From);

    //// Back
    var LoadFace_000_Back = new THREE.BoxGeometry(1000, 1000, 1000);
    LoadFace_000_Back = new THREE.Mesh(LoadFace_000_From, wallmeshMaterial);
    LoadFace_000_Back.position.set(RealPosition_X_Back, RealPosition_Z_From, RealPosition_Y_From);
    LoadFace_000_Back.rotation.x = -0.5 * Math.PI;
    LoadFace_000_Back.rotation.z = Math.PI;
    LoadFace_000_Back.scale.set(1, 1, 1);
    LoadFace_000_Back.scale.x = 0.001;
    LoadFace_000_Back.scale.y = Mesh.scale.z;
    LoadFace_000_Back.scale.z = Mesh.scale.y;
    LoadFace_000_Back.name = "Waal_Face1_Back" + Mesh.IdWall;
    LoadFace_000_Back.visible = true;
    scene.add(LoadFace_000_Back);
};
function AddFaces_0(Mesh)
{
 
    AddDimWall_0Static(Mesh, Mesh.position.x, Mesh.position.z, Mesh.scale.x * 10000, "", Mesh.scale.x * 10000);
    return;
    //Top
    var RealPosition_X = Mesh.position.x + ((Mesh.scale.x * 1000) / 2);
    var RealPosition_Y = Mesh.position.z - ((Mesh.scale.y * 1000) / 2);
    //From
    var RealPosition_Y_From = Mesh.position.z + 1;
    var RealPosition_Z_From = (Mesh.scale.z * 1000) / 2;
    //Back
    var RealPosition_Y_Back = (RealPosition_Y_From - (Mesh.scale.y * 1000)) - 1;

    const Wall_texture = new THREE.TextureLoader().load("../../Content/DesignTools/Material/concrete.png");
    Wall_texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
    Wall_texture.colorSpace = THREE.SRGBColorSpace;
    const wallmeshMaterial = new THREE.MeshPhongMaterial({ map: Wall_texture });
    var LoadFace_000_Top = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_Top = new THREE.Mesh(LoadFace_000_Top, wallmeshMaterial);
    meshFace_000_Top.position.set(RealPosition_X, (Mesh.scale.z * 1000) + 1, RealPosition_Y);
    meshFace_000_Top.rotation.x = -0.5 * Math.PI;
    meshFace_000_Top.rotation.z = Math.PI;
    meshFace_000_Top.scale.set(1, 1, 1);
    meshFace_000_Top.scale.x = Mesh.scale.x;
    meshFace_000_Top.scale.y = Mesh.scale.y;
    meshFace_000_Top.scale.z = 0.001;
    meshFace_000_Top.name = "Waal_Face1_" + Mesh.IdWall;
    meshFace_000_Top.visible = true;
    scene.add(meshFace_000_Top);
    // From
    var LoadFace_000_From = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace_000_From = new THREE.Mesh(LoadFace_000_From, wallmeshMaterial);
    meshFace_000_From.position.set(RealPosition_X, RealPosition_Z_From, RealPosition_Y_From);
    meshFace_000_From.rotation.x = -0.5 * Math.PI;
    meshFace_000_From.rotation.z = Math.PI;
    meshFace_000_From.scale.set(1, 1, 1);
    meshFace_000_From.scale.x = Mesh.scale.x;
    meshFace_000_From.scale.y = 0.001;
    meshFace_000_From.scale.z = Mesh.scale.z;
    meshFace_000_From.name = "Waal_Face1_From" + Mesh.IdWall;
    meshFace_000_From.visible = true;
    scene.add(meshFace_000_From);

    // Back
    var LoadFace_000_Back = new THREE.BoxGeometry(1000, 1000, 1000);
    LoadFace_000_Back = new THREE.Mesh(LoadFace_000_Back, wallmeshMaterial);
    LoadFace_000_Back.position.set(RealPosition_X, RealPosition_Z_From, RealPosition_Y_Back);
    LoadFace_000_Back.rotation.x = -0.5 * Math.PI;
    LoadFace_000_Back.rotation.z = Math.PI;
    LoadFace_000_Back.scale.set(1, 1, 1);
    LoadFace_000_Back.scale.x = Mesh.scale.x;
    LoadFace_000_Back.scale.y = 0.001;
    LoadFace_000_Back.scale.z = Mesh.scale.z;
    LoadFace_000_Back.name = "Waal_Face1_Back" + Mesh.IdWall;
    LoadFace_000_Back.visible = true;
    scene.add(LoadFace_000_Back);
};
function AddTexHelp(NameTextDim, x, y) {
    var radius = 1;
    var geom = new THREE.SphereGeometry(radius, 64, 24);
    geom.name = "Geo_Grill_90";
    var mat = new THREE.MeshBasicMaterial({ color: Math.random() * 0x0AA0F7, wireframe: true });
    var TexHelp = new THREE.Mesh(geom, mat);
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    TextDim = canvas.getContext("2d");
    TextDim.font = "30pt Arial";
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
    TexHelp.add(sprite);
    TexHelp.name = "Grill_90";
    TexHelp.visible = true;
    scene.add(TexHelp);
};
function DrawPointHelp(x, y, z) {
    const geometry = new THREE.SphereGeometry(1, 10, 60);
    var color = 0x000000;
    const pt = new THREE.Points(
        geometry,
        new THREE.PointsMaterial({
            color: color,
            size: 15
        }));
    pt.position.x = x;
    pt.position.y = y;
    pt.position.z = z;
    pt.typeDim = "Dim";
    pt.visible = true;
    scene.add(pt);
};

function ChangeWallNewValue(value1, value2) {
    value1 = parseFloat((value1).toFixed(3));
    value2 = parseFloat((value2).toFixed(3));
    var value = parseFloat((value1 - value2).toFixed(5));
    if (value < 0.001) { value = value * -1}
    return value;
};
