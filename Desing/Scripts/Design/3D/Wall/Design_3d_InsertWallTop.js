function AddEsq10() {
    meshEsq10.visible = false;

    var CoodMarca = new THREE.Vector3(obWall.position.x, 275, obWall.position.z);
    var Marca = new THREE.ArrowHelper(new THREE.Vector3(0, 0, -1), CoodMarca, 1, 0x000000, 20, 10);
    Marca.name = "";
    //scene.add(Marca);


    var ySub = obWall.position.z - 30;
    var xSub = 0.06;
    if (obWall.scale.x + 0.025 > 0.060) { xSub = 0.075; }
    if (obWall.scale.x + 0.025 > 0.075) { xSub = 0.090; }
    if (obWall.scale.x + 0.025 > 0.090) { xSub = 0.105; }
    if (obWall.scale.x + 0.025 > 0.105) { xSub = 0.12; }
    if (obWall.scale.x + 0.025 > 0.120) { xSub = 0.135; }
    if (obWall.scale.x + 0.025 > 0.135) { xSub = 0.150; }
    if (obWall.scale.x + 0.025 > 0.150) { xSub = 0.165; }
    if (obWall.scale.x + 0.025 > 0.165) { xSub = 0.180; }
    var PointxSub = xSub * 1000;
    var MarStarx = obWall.position.x - obWall.scale.x * 1000;
    var MarStary = obWall.position.z;

    var CoodMarca = new THREE.Vector3(MarStarx, 275, MarStary);
    var Marca = new THREE.ArrowHelper(new THREE.Vector3(0, 0, -1), CoodMarca, 1, 0x000000, 20, 10);
    Marca.name = "";
    //scene.add(Marca);

     //marca old wall
    var CoodMarcaOld = new THREE.Vector3(MarStarx + PointxSub, 275, MarStary);
    var MarcaOld = new THREE.ArrowHelper(new THREE.Vector3(0, 0, -1), CoodMarcaOld, 1, 0x000000, 20, 10);
    Marca.name = "";
    //scene.add(MarcaOld);
    var CooXWall = MarStarx + PointxSub;
    var CooYWall = MarStary;

    var PositionEsqy = y;
    var ScaleEsqy = obWall.scale.y + 0.030;
    var EscaleZ = obWall.scale.z;
    var partName = new Date().valueOf();
    var loader = new THREE.STLLoader();
    var UniversalPanel = true;
    var _vScaleEsqy = 0;
    //Corner
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked === false) {
        UniversalPanel = false;
        if (ScaleEsqy > 0.03) { _vScaleEsqy = 0.045; }
        if (ScaleEsqy > 0.045) { _vScaleEsqy = 0.060; }
        if (ScaleEsqy > 0.06) { _vScaleEsqy = 0.075; }
        if (ScaleEsqy > 0.075) { _vScaleEsqy = 0.090; }
        if (ScaleEsqy > 0.090) { _vScaleEsqy = 0.105; }
        if (ScaleEsqy > 0.105) { _vScaleEsqy = 0.120; }
        if (ScaleEsqy > 0.12) { _vScaleEsqy = 0.135; }
        if (ScaleEsqy > 0.135) { _vScaleEsqy = 0.15; }
        if (ScaleEsqy > 0.15) { _vScaleEsqy = 0.165; }
        if (ScaleEsqy > 0.165) { _vScaleEsqy = 0.18; }
        ScaleEsqy = _vScaleEsqy;
        PositionEsqy = obWall.position.z - _vScaleEsqy * 1000;
    }
    //NewWall
    AddWall_R000(CooXWall, CooYWall, 0, obWall.scale.y * 10, obWall.scale.x * 10, obWall.scale.z * 10, "Wall_R000");
 
    //AddWall_Esq_1(x, y, ZRotate, _longWall, _widthWall, _heightWall, TypeWall)
    //Corner sent direccion
    AddWall_Esq_1(MarStarx, CooYWall, 0, xSub * 10, obWall.scale.x * 10, obWall.scale.z * 10, "Esq10");
    var CoordYEsq_2Wall = (MarStary - (obWall.scale.y * 100));
    var longEsq_2Wall =  xSub * 10;
    var CoordXEsq_2Wall = MarStarx + (obWall.scale.x * 1000);
    var WidthEsq_2Wall = (obWall.scale.x * 10);
    var HeightEsq_2Wall = obWall.scale.z * 10;
    AddWall_Esq_2(CoordXEsq_2Wall, CoordYEsq_2Wall, 0, WidthEsq_2Wall, HeightEsq_2Wall, longEsq_2Wall, "Esq10_2");
    // new old Wall
    var CoodMarca2 = new THREE.Vector3(MarStarx, 275, MarStary - (obWall.scale.y * 100));
    var Marca2 = new THREE.ArrowHelper(new THREE.Vector3(0, 0, -1), CoodMarca2, 1, 0x000000, 20, 10);
    Marca.name = "";
    //scene.add(Marca2);
    var CoordYOldWall = (MarStary - (obWall.scale.y * 100)) + (xSub * 1000);
    var longOldWall = ((obWall.scale.y * 10) - ((xSub*10) -0.3));
    var CoordXOldWall = MarStarx + (obWall.scale.x * 1000);
    var WidthOldWall = (obWall.scale.x * 10);
    var HeightOldWall = obWall.scale.z * 10;
    AddWall_R900(CoordXOldWall, CoordYOldWall, 0, WidthOldWall, HeightOldWall, longOldWall, "Wall_R900");
    scene.remove(obWall);
    InsertWall = 102;
};



function AddEsq70() {
    meshEsq70.visible = false;
    var ySub = obWall.position.z - obWall.scale.y * 1000;
    var xSub = 0.06;
    if (obWall.scale.y + 0.025 > 0.060) { xSub = 0.075; }
    if (obWall.scale.y + 0.025 > 0.075) { xSub = 0.090; }
    if (obWall.scale.y + 0.025 > 0.090) { xSub = 0.105; }
    if (obWall.scale.y + 0.025 > 0.105) { xSub = 0.12; }
    if (obWall.scale.y + 0.025 > 0.120) { xSub = 0.135; }
    if (obWall.scale.y + 0.025 > 0.135) { xSub = 0.150; }
    if (obWall.scale.y + 0.025 > 0.150) { xSub = 0.165; }
    if (obWall.scale.y + 0.025 > 0.165) { xSub = 0.180; }
    var x = obWall.position.x + obWall.scale.y * 1000;
    var y = obWall.position.z - obWall.scale.y * 1000 - 30;
    var PositionEsqy = obWall.position.z - obWall.scale.y * 1000 - 30;
    var ScaleEsqy = obWall.scale.y + 0.030;
    var EscaleZ = obWall.scale.z;
    var partName = new Date().valueOf();
    var loader = new THREE.STLLoader();
    var UniversalPanel = true;
    var _vScaleEsqy = 0;
    //Corner
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked === false) {
        UniversalPanel = false;
        if (ScaleEsqy > 0.03) { _vScaleEsqy = 0.045; }
        if (ScaleEsqy > 0.045) { _vScaleEsqy = 0.060; }
        if (ScaleEsqy > 0.06) { _vScaleEsqy = 0.075; }
        if (ScaleEsqy > 0.075) { _vScaleEsqy = 0.090; }
        if (ScaleEsqy > 0.090) { _vScaleEsqy = 0.105; }
        if (ScaleEsqy > 0.105) { _vScaleEsqy = 0.120; }
        if (ScaleEsqy > 0.12) { _vScaleEsqy = 0.135; }
        if (ScaleEsqy > 0.135) { _vScaleEsqy = 0.15; }
        if (ScaleEsqy > 0.15) { _vScaleEsqy = 0.165; }
        if (ScaleEsqy > 0.165) { _vScaleEsqy = 0.18; }
        ScaleEsqy = _vScaleEsqy;
        PositionEsqy = obWall.position.z - _vScaleEsqy * 1000;
    }

    //WallEPanelExTop_
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshEsqTop = new THREE.Mesh(geometry, materialEsq);
        meshEsqTop.position.set(x, EscaleZ * 1000, PositionEsqy);
        //meshEsqTop.rotation.x = -Math.PI;
        meshEsqTop.name = "WallEsq70_New_" + partName;
        //meshEsqTop.rotation.y = Math.PI;
        meshEsqTop.scale.set(obWall.scale.y, EscaleZ, ScaleEsqy);
        meshEsqTop.UniversalPanel = UniversalPanel;
        meshEsqTop.CHeckDimWall = obWall.CHeckDimWall;
        meshEsqTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshEsqTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshEsqTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshEsqTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshEsqTop.CHeckPropInside = obWall.CHeckPropInside;
        meshEsqTop.CHeckPropOutside = obWall.CHeckPropOutside;
        meshEsqTop.CHeck750R = obWall.CHeck750R;
        scene.add(meshEsqTop);
    });
     //WallEsqTLe_
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshSupbTop = new THREE.Mesh(geometry, materialSup);
        var SupPositionX = (xSub - obWall.scale.y) * 1000;
        meshSupbTop.position.set(x + SupPositionX, EscaleZ * 1000, ySub);
        meshSupbTop.name = "WallEsq70_Old_" + partName;
        meshSupbTop.UniversalPanel = UniversalPanel;
        meshSupbTop.scale.set(xSub, EscaleZ, obWall.scale.y);
        meshSupbTop.XWith = obWall.scale.y;
        meshSupbTop.YWith = obWall.scale.y;
        meshSupbTop.CHeckDimWall = obWall.CHeckDimWall;
        meshSupbTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshSupbTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshSupbTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshSupbTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshSupbTop.CHeckPropInside = obWall.CHeckPropInside;
        meshSupbTop.CHeckPropOutside = obWall.CHeckPropOutside;
        scene.add(meshSupbTop);
    });
    //Wall
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWallTop = new THREE.Mesh(geometry, materialSup);
        meshWallTop.name = "Wall_R900" + partName;
        meshWallTop.position.set(x, EscaleZ * 1000, PositionEsqy - 270);
        meshWallTop.scale.set(obWall.scale.y, EscaleZ, 0.27);
        meshWallTop.MeshTypeWall = "Wall_R900";
        meshWallTop.IdCornerDown = partName.toString();
        meshWallTop.ScaleEsqy = ScaleEsqy * 1000;
        meshWallTop.CHeckDimWall = obWall.CHeckDimWall;
        meshWallTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshWallTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshWallTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshWallTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshWallTop.CHeckPropInside = obWall.CHeckPropInside;
        meshWallTop.CHeckPropOutside = obWall.CHeckPropOutside;
        meshWallTop.idWall = partName;
        meshWallTop.tape_180 = "";
        meshWallTop.tape_0 = "";
        scene.add(meshWallTop);
    });

    //AddDimWall_Top(partName);
    //AddDimWall_EsqDown(obWall.position.x, obWall.position.z, xSub * 1000, partName);
    //AddDimWall_EsqTop(ScaleEsqy * 1000, x, PositionEsqy, obWall.position.z, partName);
    //Add Control
    //AddWallControlTop()
    //Change Form
    //document.getElementById("ImgEditDim").src = "../../Content/DesignTools/Form/EditWall/AddEsq70.png";
    //var Datalong = (EscaleZ * 10).toFixed(2);
    //var DataWith = (obWall.scale.y * 10).toFixed(2);
    //var DataHeight = (obWall.scale.z * 10).toFixed(2);
    //$("#Datalong").val(DataWith);
    //$("#DataWith").val(Datalong);
    //$("#DataHeight").val(DataHeight);
    //$("#DataSupInicial").val(0);
    //$("#DataSupEnd").val(0);
    //$("#DataSupEnd").prop("disabled", true);
    //$("#DataCordenadX").val(x);
    //$("#DataCordenadY").val(PositionEsqy - 270);
    //$("#DataCordenadX").prop("disabled", true);
    //$("#DataCordenadY").prop("disabled", true);
    //Change master
    /*    ResetSetup();*/


    //Chaange obWall
    obWall.ScaleEsqy = ScaleEsqy * 1000;
    obWall.Iniciall_Wall = 0;
    obWall.scale.x = obWall.scale.x - xSub;
    obWall.position.x = obWall.position.x + (xSub * 1000);
    obWall.name = "Wall_R000" + partName;
    obWall.MeshTypeWall = "Wall_R000";
    obWall.IdCornerLeft = partName.toString();
    obWall.LongLeft = xSub;
    obWall.LongRight = 0;
    obWall.IsSolutionCornerYUniversalPanelCorner = document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked;
    obWall.IsSolutionCornerXUniversalPanelCorner = document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked;
    obWall.tape_180 = "";
    obWall.Iniciall_Wall = "WallEsq70_Old_" + partName;
    InsertWall = 102;
};


function AddEsq90() {
    meshEsq90.visible = false;
    var ySub = obWall.position.z - obWall.scale.y * 1000;
    var xSub = 0.06;
    if (obWall.scale.y + 0.025 > 0.060) { xSub = 0.075; }
    if (obWall.scale.y + 0.025 > 0.075) { xSub = 0.090; }
    if (obWall.scale.y + 0.025 > 0.090) { xSub = 0.105; }
    if (obWall.scale.y + 0.025 > 0.105) { xSub = 0.12; }
    if (obWall.scale.y + 0.025 > 0.120) { xSub = 0.135; }
    if (obWall.scale.y + 0.025 > 0.135) { xSub = 0.150; }
    if (obWall.scale.y + 0.025 > 0.150) { xSub = 0.165; }
    if (obWall.scale.y + 0.025 > 0.165) { xSub = 0.180; }
    var x = (obWall.position.x + obWall.scale.x * 1000) + (obWall.scale.y * 1000);
    var y = obWall.position.z - obWall.scale.y * 1000 - 30;
    var PositionEsqy = obWall.position.z - obWall.scale.y * 1000 - 30;
    var ScaleEsqy = obWall.scale.y + 0.030;
    var EscaleZ = obWall.scale.z;
    var partName = new Date().valueOf();
    var loader = new THREE.STLLoader();
    var UniversalPanel = true;
    var _vScaleEsqy = 0;
    //Corner
    if (document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked === false) {
        UniversalPanel = false;
        if (ScaleEsqy > 0.03) { _vScaleEsqy = 0.045; }
        if (ScaleEsqy > 0.045) { _vScaleEsqy = 0.060; }
        if (ScaleEsqy > 0.06) { _vScaleEsqy = 0.075; }
        if (ScaleEsqy > 0.075) { _vScaleEsqy = 0.090; }
        if (ScaleEsqy > 0.090) { _vScaleEsqy = 0.105; }
        if (ScaleEsqy > 0.105) { _vScaleEsqy = 0.120; }
        if (ScaleEsqy > 0.12) { _vScaleEsqy = 0.135; }
        if (ScaleEsqy > 0.135) { _vScaleEsqy = 0.15; }
        if (ScaleEsqy > 0.15) { _vScaleEsqy = 0.165; }
        if (ScaleEsqy > 0.165) { _vScaleEsqy = 0.18; }
        ScaleEsqy = _vScaleEsqy;
        PositionEsqy = obWall.position.z - _vScaleEsqy * 1000;
    }
    //WallEPanelExTop_
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshEsqTop = new THREE.Mesh(geometry, materialEsq);
        var xEsqWall = x - (obWall.scale.y * 1000);
        meshEsqTop.position.set(xEsqWall, EscaleZ * 1000, PositionEsqy);
        //meshEsqTop.rotation.x = -Math.PI;
        meshEsqTop.name = "WallEsq90_New_" + partName;
        //meshEsqTop.rotation.y = Math.PI;
        meshEsqTop.scale.set(obWall.scale.y, EscaleZ, ScaleEsqy);
        meshEsqTop.UniversalPanel = UniversalPanel;
        meshEsqTop.CHeckDimWall = obWall.CHeckDimWall;
        meshEsqTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshEsqTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshEsqTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshEsqTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshEsqTop.CHeckPropInside = obWall.CHeckPropInside;
        meshEsqTop.CHeckPropOutside = obWall.CHeckPropOutside;
        meshEsqTop.CHeck750R = obWall.CHeck750R;
        scene.add(meshEsqTop);
    });
    //WallEsqTLe_
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshSupbTop = new THREE.Mesh(geometry, materialSup);
        var SupPositionX = (xSub - obWall.scale.y) * 1000;
        meshSupbTop.position.set(x - SupPositionX, EscaleZ * 1000, ySub);
        meshSupbTop.name = "WallEsq90_Old_" + partName;
        meshSupbTop.UniversalPanel = UniversalPanel;
        meshSupbTop.scale.set(xSub, EscaleZ, obWall.scale.y);
        meshSupbTop.XWith = obWall.scale.y;
        meshSupbTop.YWith = obWall.scale.y;
        meshSupbTop.CHeckDimWall = obWall.CHeckDimWall;
        meshSupbTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshSupbTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshSupbTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshSupbTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshSupbTop.CHeckPropInside = obWall.CHeckPropInside;
        meshSupbTop.CHeckPropOutside = obWall.CHeckPropOutside;
        scene.add(meshSupbTop);
    });

    // Wall
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var xWall = x - (obWall.scale.y * 1000);
        var meshWallTop = new THREE.Mesh(geometry, materialSup);
        meshWallTop.name = "Wall_R900" + partName;
        meshWallTop.position.set(xWall, EscaleZ * 1000, PositionEsqy - 270);
        meshWallTop.scale.set(obWall.scale.y, EscaleZ, 0.27);
        meshWallTop.MeshTypeWall = "Wall_R900";
        meshWallTop.IdCornerDown = partName.toString();
        meshWallTop.ScaleEsqy = ScaleEsqy * 1000;
        meshWallTop.CHeckDimWall = obWall.CHeckDimWall;
        meshWallTop.CHeckBracketInside = obWall.CHeckBracketInside;
        meshWallTop.CHeckBracketOutside = obWall.CHeckBracketOutside;
        meshWallTop.CHeckRijiInside = obWall.CHeckRijiInside;
        meshWallTop.CHeckRijiOutside = obWall.CHeckRijiOutside;
        meshWallTop.CHeckPropInside = obWall.CHeckPropInside;
        meshWallTop.CHeckPropOutside = obWall.CHeckPropOutside;
        meshWallTop.idWall = partName;
        scene.add(meshWallTop);
    });
    obWall.ScaleEsqy = ScaleEsqy * 1000; 
    obWall.Iniciall_Wall = 0;
    obWall.scale.x = (obWall.scale.x) - (xSub);
    //obWall.position.x = obWall.position.x + (xSub * 1000);
    obWall.name = "Wall_R000" + partName;
    obWall.MeshTypeWall = "Wall_R000";
    obWall.IdCornerLeft = partName.toString();
    obWall.LongLeft = xSub;
    obWall.LongRight = 0;
    obWall.IsSolutionCornerYUniversalPanelCorner = document.getElementById("IsSolutionCornerYUniversalPanelCorner").checked;
    obWall.IsSolutionCornerXUniversalPanelCorner = document.getElementById("IsSolutionCornerXUniversalPanelCorner").checked;
    obWall.End_Wall = "WallEsq90_Old_" + partName;
    obWall.tape_0 = "";
    InsertWall = 102;

};



function AddDimWall_EsqTop(_scaleEsqy, x, _positionEsqy, y, IdName) {
    var arrowPos = new THREE.Vector3(x - 200, 0, _positionEsqy + _scaleEsqy / 2);
    var arrowPosCone = new THREE.ArrowHelper(new THREE.Vector3(0, 0, -1), arrowPos, _scaleEsqy / 2, 0x000000, 20, 10);
    arrowPosCone.name = "FistArrowHelper_EsqTop";
    scene.add(arrowPosCone);
    var arrowPosS = new THREE.Vector3(x - 200, 0, y - _scaleEsqy / 2);
    var arrowPosSCone = new THREE.ArrowHelper(new THREE.Vector3(0, 0, 1), arrowPosS, _scaleEsqy / 2, 0x000000, 20, 10);
    arrowPosSCone.name = "SecontArrowHelper_EsqTop";
    scene.add(arrowPosSCone);
    var pointsDimDown = [];
    pointsDimDown.push(new THREE.Vector3(x, 0, _positionEsqy + _scaleEsqy));
    pointsDimDown.push(new THREE.Vector3(x - 220, 0, _positionEsqy + _scaleEsqy));
    const LineDown = new THREE.BufferGeometry().setFromPoints(pointsDimDown);
    const LineDimDown = new THREE.Line(LineDown, materialDimWall);
    LineDimDown.name = "DimLineEsq_Down" + IdName;
    scene.add(LineDimDown);
    var pointsDimTop = [];
    pointsDimTop.push(new THREE.Vector3(x, 0, _positionEsqy));
    pointsDimTop.push(new THREE.Vector3(x - 220, 0, _positionEsqy));
    const LineTop = new THREE.BufferGeometry().setFromPoints(pointsDimTop);
    const LineDimTop = new THREE.Line(LineTop, materialDimWall);
    LineDimTop.name = "DimLineEsq_Top" + IdName;
    scene.add(LineDimTop);
    //AddTest
    var radius = 1;
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = (_scaleEsqy / 100).toFixed(2);
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
    sprite.position.x = x - 190;
    sprite.position.y = -10;
    sprite.position.z = y - _scaleEsqy / 2;
    _dim.add(sprite);
    _dim.name = "TextDim" + IdName;
    scene.add(_dim);

};
function AddDimWall_EsqDown(x, y, xSub, IdName) {
    var arrowPos = new THREE.Vector3(x - xSub / 2, 0, y + 200);
    var arrowPosCone = new THREE.ArrowHelper(new THREE.Vector3(-1, 0, 0), arrowPos, xSub / 2, 0x000000, 20, 10);
    arrowPosCone.name = "EsqRight_ArrowPosCone";
    scene.add(arrowPosCone);
    var arrowPosRight = new THREE.Vector3(x - xSub / 2, 0, y + 200);
    var arrowPosConeRight = new THREE.ArrowHelper(new THREE.Vector3(1, 0, 0), arrowPosRight, xSub / 2, 0x000000, 20, 10);
    arrowPosConeRight.name = "EsqRight_ArrowPosSecontCone";
    scene.add(arrowPosConeRight);

    var pointsDimDown = [];
    pointsDimDown.push(new THREE.Vector3(x - xSub, 0, y));
    pointsDimDown.push(new THREE.Vector3(x - xSub, 0, y + 220));
    const LineDown = new THREE.BufferGeometry().setFromPoints(pointsDimDown);
    const LineDimDown = new THREE.Line(LineDown, materialDimWall);
    LineDimDown.name = "DimLineEsq" + IdName;
    scene.add(LineDimDown);

    var pointsDimTop = [];
    pointsDimTop.push(new THREE.Vector3(x, 0, y));
    pointsDimTop.push(new THREE.Vector3(x, 0, y + 220));
    const LineTop = new THREE.BufferGeometry().setFromPoints(pointsDimTop);
    const LineDimTop = new THREE.Line(LineTop, materialDimWall);
    LineDimTop.name = "DimLineEsq" + IdName;
    scene.add(LineDimTop);

    //AddTest
    var radius = 1;
    var size = 256;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    var mat = new THREE.MeshBasicMaterial({
        color: Math.random() * 0xFFFFFF,
        wireframe: true
    });
    var _dim = new THREE.Mesh(geom, mat);
    _dim.name = (xSub / 100).toFixed(2);
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
    sprite.position.x = x - (xSub / 2);
    sprite.position.y = -10;
    sprite.position.z = y + 190;
    _dim.add(sprite);
    _dim.name = "TextDim" + IdName;
    scene.add(_dim);
};
function AddDimWall_Top(IdName) {
    var x = obWall.position.x - obWall.ScaleEsqy;
    var y = obWall.position.z;
    const geometryCono = new THREE.ConeGeometry(5, 20, 32);
    const Fistcone = new THREE.Mesh(geometryCono, materialDimWall);
    Fistcone.rotation.x = Math.PI * -0.5;
    Fistcone.position.x = x - 200 + obWall.scale.z * 100;
    Fistcone.position.y = 0;
    Fistcone.position.z = y - (obWall.ScaleEsqy + 270 - 10);
    Fistcone.name = "FistArrowHelper_AddEsq70" + IdName;
    scene.add(Fistcone);

    const SecontCone = new THREE.Mesh(geometryCono, materialDimWall);
    SecontCone.rotation.x = Math.PI * 0.5;
    SecontCone.position.x = x - 200 + obWall.scale.z * 100;
    SecontCone.position.y = 0;
    SecontCone.position.z = y - (obWall.ScaleEsqy + 10);
    SecontCone.name = "SecontArrowHelper_AddEsq70" + IdName;
    scene.add(SecontCone);


    var pointsDimTop = [];
    pointsDimTop.push(new THREE.Vector3(x, 0, y - (obWall.ScaleEsqy + 270)));
    pointsDimTop.push(new THREE.Vector3(x - 200 + obWall.scale.z * 100 - 10, 0, y - (obWall.ScaleEsqy + 270)));
    const LineTop = new THREE.BufferGeometry().setFromPoints(pointsDimTop);
    const LineDimTop = new THREE.Line(LineTop, materialDimWall);
    LineDimTop.name = "DimLine_AddEsq70" + IdName;
    scene.add(LineDimTop);

    var pointsDimDown = [];
    pointsDimDown.push(new THREE.Vector3(x - 200 + obWall.scale.z * 100, 0, y - (obWall.ScaleEsqy + 270)));
    pointsDimDown.push(new THREE.Vector3(x - 200 + obWall.scale.z * 100, 0, y - obWall.ScaleEsqy));
    const LineDown = new THREE.BufferGeometry().setFromPoints(pointsDimDown);
    const LineDimDown = new THREE.Line(LineDown, materialDimWall);
    LineDimDown.name = "DimLine_TWDown0CDown1" + IdName;
    scene.add(LineDimDown);
    //AddTest
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
    ctx.fillText("2,70", size / 2, size / 3);

    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({
        map: tex
    });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = (x - 200) + (obWall.scale.z * 100 + 10);
    sprite.position.y = -10;
    sprite.position.z = y - (obWall.ScaleEsqy + (270 / 2));
    _dim.add(sprite);
    _dim.name = "Test_AddEsq70" + IdName;
    scene.add(_dim);
};





