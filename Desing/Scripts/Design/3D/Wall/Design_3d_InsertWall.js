//angel
function DrawDesign() {
    var addMesh0 = false;
    var addMesh90 = false;
    var addMeshWorker = false;
    var addMeshGrill_0 = false;
    var addMeshGrill_90 = false;
    for (var i = 0; i < ListWalls.length; i++) {
        switch (ListWalls[i].TypeWall.substr(0, 9)) {
            case 'Grill_900':
                addMeshGrill_90 = true;
                break;
            case 'Grill_000':
                addMeshGrill_0 = true;
                break;
            case 'Wall_R000':
                addMesh0 = true;
                break;
            case 'Wall_R900':
                addMesh90 = true;
                break;
            case 'Esq_10_00':
                addMesh0 = true;
                break;
            case 'Esq_10_90':
                addMesh90 = true;
                break;
            case 'Esq_30_00':
                addMesh0 = true;
                break;
            case 'Esq_30_90':
                addMesh90 = true;
                break;
            case 'Esq_40_00':
                addMesh0 = true;
                break;
            case 'Esq_40_90':
                addMesh90 = true;
                break;
            case 'Esq_50_00':
                addMesh0 = true;
                break;
            case 'Esq_50_90':
                addMesh90 = true;
                break;
            case 'Esq_60_00':
                addMesh0 = true;
                break;
            case 'Esq_60_90':
                addMesh90 = true;
                break;
            case 'Esq_20_00':
                addMesh0 = true;
                break;
            case 'Esq_20_90':
                addMesh90 = true;
                break;
            case 'Esq_70_00':
                addMesh0 = true;
                break;
            case 'Esq_70_90':
                addMesh90 = true;
                break;
            case 'Esq_80_90':
                addMesh90 = true;
                break;
            case 'Esq_80_00':
                addMesh0 = true;
                break;
            case 'Esq_X_00':
                addMesh0 = true;
                break;
            case 'Esq_X_90':
                addMesh90 = true;
                break;
            case 'Pilar':
                addMesh0 = true;
                break;
            case 'Warker':
                addMeshWorker = true;
                break;
        }
        var partName = new Date().valueOf();
        var NameWall = ListWalls[i].TypeWall + partName;
        var _longWall = parseFloat(ListWalls[i].ScaleX.replace(',', '.'));
        var _widthWall = parseFloat(ListWalls[i].ScaleY.replace(',', '.'));
        var _heightWall = parseFloat(ListWalls[i].ScaleZ.replace(',', '.'));
        var loader = new THREE.STLLoader();
        var x = parseFloat(ListWalls[i].PositionX.replace(',', '.'));
        var z = parseFloat(ListWalls[i].PositionY.replace(',', '.'));
        var y = parseFloat(ListWalls[i].PositionZ.replace(',', '.'));
        var meshWall = new THREE.Mesh();
        meshWall.position.set(x, 0, y);
        meshWall.idWall = ListWalls[i].IdWall;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = ListWalls[i].AddName;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = _longWall;
        meshWall.scale.y = _widthWall;
        meshWall.scale.z = _heightWall;
        meshWall.Iniciall_Wall = ListWalls[i].Iniciall_Wall;
        meshWall.End_Wall = ListWalls[i].End_Wall;
        meshWall.MeshTypeWall = ListWalls[i].TypeWall;
        meshWall.MeshTypeWallLeft = ListWalls[i].TypeWallLeft;
        meshWall.MeshTypeWallRight = ListWalls[i].TypeWallRight;
        meshWall.MeshTypeWall_180 = ListWalls[i].TypeWallLeft;
        meshWall.MeshTypeWall_0 = ListWalls[i].TypeWallRight;
        meshWall.IdCornerDown = ListWalls[i].IDCornerDown;
        meshWall.IdCornerLeft = ListWalls[i].IDCornerLeft;
        meshWall.ScaleEsqy = ListWalls[i].ScaleEsqy;
        meshWall.CHeckDimWall = ListWalls[i].CHeckDimWall;
        meshWall.CHeckBracketInside = ListWalls[i].CHeckBracketInside;
        meshWall.CHeckBracketOutside = ListWalls[i].CHeckBracketOutside;
        meshWall.CHeckRijiInside = ListWalls[i].CHeckRijiInside;
        meshWall.CHeckRijiOutside = ListWalls[i].CHeckRijiOutside;
        meshWall.CHeckPropInside = ListWalls[i].CHeckPropInside;
        meshWall.CHeckPropOutside = ListWalls[i].CHeckPropOutside;
        meshWall.CHeckPropInsideInf = ListWalls[i].CHeckPropInsideInf;
        meshWall.CHeckPropOutsideInf = ListWalls[i].CHeckPropOutsideInf;
        meshWall.CHeck750R = ListWalls[i].CHeck750R;
        meshWall.LongLeft = ListWalls[i].LongLeft;
        meshWall.LongRight = ListWalls[i].LongRight;
        meshWall.IsSolutionCornerYUniversalPanelCorner = ListWalls[i].IsSolutionCornerXUniversalPanelCorner;
        meshWall.IsSolutionCornerXUniversalPanelCorner = ListWalls[i].IsSolutionCornerXUniversalPanelCorner;
        meshWall.Tape_0 = ListWalls[i].Tape_0;
        meshWall.Tape_180 = ListWalls[i].Tape_180;
        meshWall.Tape_90 = ListWalls[i].Tape_90;
        meshWall.Tape_270 = ListWalls[i].Tape_270;
        meshWall.Grupo = ListWalls[i].Grupo;
        meshWall.material = materialWall;
        meshWall.Sub_Long_0 = ListWalls[i].Sub_Long_0;
        meshWall.Sub_Long_180 = ListWalls[i].Sub_Long_180;
        meshWall.Sub_Long_90 = ListWalls[i].Sub_Long_90;
        meshWall.Sub_Long_270 = ListWalls[i].Sub_Long_270;
        meshWall.IdWall_270 = ListWalls[i].IdWall_270;
        meshWall.IdWall_0 = ListWalls[i].IdWall_0;
        meshWall.IdWall_180 = ListWalls[i].IdWall_180;
        meshWall.IdWall_90 = ListWalls[i].IdWall_90;
        meshWall.TypeWall_0 = ListWalls[i].TypeWall_0;
        meshWall.TypeWall_180 = ListWalls[i].TypeWall_180;
        meshWall.TypeWall_90 = ListWalls[i].TypeWall_90;
        meshWall.TypeWall_270 = ListWalls[i].TypeWall_270;
        meshWall.IsTypeConexion = null;
        meshWall.IdTypeFormworkMode = ListWalls[i].IdTypeFormworkMode;
        meshWall.IsFormwork = ListWalls[i].IsFormwork;
        if (meshWall.IsFormwork === undefined || meshWall.IsFormwork === null || meshWall.IsFormwork === "") {
            meshWall.IsFormwork = true;
        }
        if (addMesh0 === true) { insertwall_00(meshWall); addMesh0 = false; }
        if (addMesh90 === true) { insertwall_90(meshWall); addMesh90 = false; }
        if (addMeshWorker === true) { insertWorker_00(meshWall); addMesh90 = false; }
        if (addMeshGrill_0 === true) { AddGrill_0(meshWall); addMeshGrill_0 = false; }
        if (addMeshGrill_90 === true) { AddGrill_90(meshWall); addMeshGrill_90 = false; }
    }
};
function AddNucleo_Wall() {
    var H = document.getElementById("NucleoH").value;
    var esp10_00 = document.getElementById("EWS").value / 10;
    var esp70_00 = document.getElementById("ELI").value;
    var esp70_90 = document.getElementById("EWI").value;
    var esp50_90 = document.getElementById("ELD").value;
    var L = document.getElementById("NucleoL").value / 10;
    var W = document.getElementById("NucleoW").value / 10;
    var xSubSuperior = GetXsub(esp10_00);
    var EsqTopLong_10 = xSubSuperior;
    var xSubRight = GetXsub(esp50_90)
    var xSub = GetXsub(esp70_00);
    var xSub_50 = GetXsub(esp50_90);
    var EsqLefLong_E_50 = xSub_50 * 10;
    var EsqLefLong_Esq_30 = xSub;
    var IdpartName = new Date().valueOf();
    var IdWall_10_00_D = "Wall_R000" + IdpartName;
    var IdEsq_10_00_D = "Esq_10_00" + IdpartName;
    var IdEsq_10_90_D = "Esq_10_90" + IdpartName;
    var IdWall_30_90_D = "Wall_R900" + IdpartName + 30;
    var IdEsq_30_00_D = "Esq_30_00" + IdpartName + 30;
    var IdEsq_30_90_D = "Esq_30_90" + IdpartName + 30;
    var IdWall_50_00_D = "Wall_R000" + IdpartName + 50;
    var IdEsq_50_00_D = "Esq_50_00" + IdpartName + 50;
    var IdEsq_50_90_D = "Esq_50_90" + IdpartName + 50;
    var IdWall_70_90_D = "Wall_R900" + IdpartName + 70;
    var IdEsq_70_00_D = "Esq_70_00" + IdpartName + 70;
    var IdEsq_70_90_D = "Esq_70_90" + IdpartName + 70;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    meshNucleo.visible = false;
    //WallEsqInf_70
    var EsqLefXPosition = meshNucleo.position.x;
    var EsqLefYPosition = meshNucleo.position.z;
    var EsqLefLong_Esq_70 = xSub;
    var EsqLefWidth = esp70_00;
    var EsqLefHeigh = H;
    AddWall_R000(EsqLefXPosition, EsqLefYPosition, EsqLefLong_Esq_70 * 10, EsqLefWidth, EsqLefHeigh,
        "Esq_70_00",
        IdEsq_70_00_D,
        IdWall_50_00_D,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        false,
    );
    //WallEsq_Left_70_90
    var EsqTopXPosition = meshNucleo.position.x + (esp70_90 * 100);
    var subY = (xSub * 1000);
    var EsqTopYPosition = meshNucleo.position.z - subY;
    var EsqTopLong = xSub;
    var EsqTopWidth = esp70_90 / 10;
    var EsqTopHeigh = H / 10;
    //Esq_70
    AddWall_R900(
        EsqTopXPosition,
        EsqTopYPosition,
        EsqTopLong,
        EsqTopWidth,
        EsqTopHeigh,
        "Esq_70_90",
        IdEsq_70_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_70_90_D,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
    );
    /*WallTop_*/
    var subSY = xSub + xSubSuperior;
    var WallopLong = W - subSY;
    var WallTopXPosition = EsqTopXPosition;
    var WallTopYPosition = EsqTopYPosition - (WallopLong * 1000);
    var WallTopWidth = EsqTopWidth;
    var WallTopHeigh = H / 10;
    //Wall70_90
    AddWall_R900(
        WallTopXPosition,
        WallTopYPosition,
        WallopLong,
        WallTopWidth,
        WallTopHeigh,
        "Wall_R900",
        IdWall_70_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdEsq_70_90_D,
        EsqTopLong,
        IdEsq_70_90_D,
        EsqTopLong,
        IdUndoRedo,
    );
    //WallInf_0 
    var WallXPosition = meshNucleo.position.x + (xSub * 1000);
    var WallYPosition = EsqLefYPosition;
    var subSXD = xSub + xSubRight;
    var WallLong = (L - subSXD) * 10;
    var WallWidth = EsqLefWidth;
    var Wallheigh = H;
    var idUndoRedoTemp = IdUndoRedo;
    //Wall_50
    AddWall_R000(WallXPosition, WallYPosition, WallLong, WallWidth, Wallheigh,
        "Wall_R000",
        IdWall_50_00_D,
        IdEsq_50_00_D,
        EsqLefLong_E_50,
        EsqLefLong_Esq_70,
        IdEsq_70_00_D,
        EsqLefLong_Esq_70,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        idUndoRedoTemp,
        false,
    );
    //WallEsqInf_50
    var EsqRightXPosition = WallXPosition + (WallLong * 100);
    var EsqRightYPosition = WallYPosition;
    var EsqLefWidth = esp70_00;
    var EsqLefHeigh = H;
    AddWall_R000(EsqRightXPosition, EsqRightYPosition, EsqLefLong_E_50, EsqLefWidth, EsqLefHeigh,
        "Esq_50_00",
        IdEsq_50_00_D,
        "",
        Sub_Long_0,
        Sub_Long_180,
        IdWall_50_00_D,
        "0",
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        false,
    );
    //WallEsq_Inf_Top_50
    var EsqTopLong = EsqTopLong;
    var suXEsqInf = EsqTopLong * 1000;
    var EsqTopXPosition = EsqRightXPosition + suXEsqInf;
    var EsqTopYPosition = EsqTopYPosition;
    var EsqTopWidth = esp50_90 / 10;
    var EsqTopHeigh = H / 10;
    AddWall_R900(
        EsqTopXPosition,
        EsqTopYPosition,
        EsqTopLong,
        EsqTopWidth,
        EsqTopHeigh,
        "Esq_50_90",
        IdEsq_50_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_30_90_D,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
    );
    //Wall_90_50
    WallTopX_50_Position = EsqTopXPosition;
    AddWall_R900(
        WallTopX_50_Position,
        WallTopYPosition,
        WallopLong,
        WallTopWidth,
        WallTopHeigh,
        "Wall_R900",
        IdWall_30_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdEsq_30_90_D,
        xSubSuperior,
        IdEsq_50_90_D,
        xSubSuperior,
        IdUndoRedo,
    );
    //WallEsqSup_10_00
    var EsqLefXPosition = meshNucleo.position.x;
    var supX_10 = (W - esp10_00) * 1000;
    var EsqLefYPosition = meshNucleo.position.z - supX_10;
    var EsqLefLong_Esq_10 = xSub;
    var EsqLefWidth = esp10_00 * 10;
    var EsqLefHeigh = H;
    AddWall_R000(EsqLefXPosition, EsqLefYPosition, EsqLefLong_Esq_10 * 10, EsqLefWidth, EsqLefHeigh,
        "Esq_10_00",
        IdEsq_10_00_D,
        IdWall_10_00_D,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        false,
    );
    //WallEsq_Sup_10_90
    var supX_10 = W * 1000;
    var EsqLefXPosition_10 = meshNucleo.position.x + (esp70_90 * 100);
    var EsqLefY_10_Position = meshNucleo.position.z - supX_10;
    var EsqTopWidth = esp70_90 / 10;
    var EsqTopHeigh = H / 10;
    AddWall_R900(
        EsqLefXPosition_10,
        EsqLefY_10_Position,
        EsqTopLong_10,
        EsqTopWidth,
        EsqTopHeigh,
        "Esq_10_90",
        IdEsq_10_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        "",
        Sub_Long_90,
        IdEsq_70_90_D,
        Sub_Long_270,
        IdUndoRedo,
    );
    //Wall_Wall_10 
    var idUndoRedoTemp = IdUndoRedo;
    AddWall_R000(WallXPosition, EsqLefYPosition, WallLong, WallWidth, Wallheigh,
        "Wall_R000",
        IdWall_10_00_D,
        IdEsq_30_00_D,
        EsqLefLong_Esq_30,
        EsqLefLong_Esq_10,
        IdEsq_10_00_D,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        idUndoRedoTemp,
        false,
    );

    //Esq 30
    //WallEsqSup_30_00
    var EsqLefXPosition = WallXPosition + (WallLong * 100);
    var supX_10 = (W - esp10_00) * 1000;
    var EsqLefYPosition = meshNucleo.position.z - supX_10;

    var EsqLefWidth = esp10_00 * 10;
    var EsqLefHeigh = H;
    AddWall_R000(EsqLefXPosition, EsqLefYPosition, EsqLefLong_Esq_30 * 10, EsqLefWidth, EsqLefHeigh,
        "Esq_30_00",
        IdEsq_30_00_D,
        "0",
        Sub_Long_0,
        Sub_Long_180,
        IdWall_10_00_D,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo,
        false,
    );
    //WallEsq_Sup_30_90
    var supX_10 = W * 1000;
    var EsqLefXPosition_10 = meshNucleo.position.x + (esp70_90 * 100);
    var EsqLefY_10_Position = meshNucleo.position.z - supX_10;
    var EsqTopLong = xSubSuperior;
    var EsqTopWidth = esp70_90 / 10;
    var EsqTopHeigh = H / 10;
    AddWall_R900(
        EsqTopXPosition,
        EsqLefY_10_Position,
        EsqTopLong,
        EsqTopWidth,
        EsqTopHeigh,
        "Esq_30_90",
        IdEsq_30_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        "",
        Sub_Long_90,
        IdWall_30_90_D,
        Sub_Long_270,
        IdUndoRedo,
    );
    InsertWall = 102;
};
function AddWall_R000(
/*1*/  x,
/*2*/  y,
/*3*/  _longWall,
/*4*/  _widthWall,
/*5*/  _heightWall,
/*6*/  TypeWall,
/*7*/  IdWall,
/*8*/  IdWall_0,
/*9*/  Sub_Long_0,
/*10*/ Sub_Long_180,
/*11*/ IdWall_180,
/*12*/ IdWall_90,
/*13*/ Sub_Long_90,
/*14*/ IdWall_270,
/*15*/ Sub_Long_270,
/*16*/ IdUndoRedo,
/*17*/ IsFirstWall,
/*18*/ OldWall,
/*19*/ IdTypeFormworkMode,
) {
    _ListUndo_Redo.push({
        Type: "AddWall_R000",
        x: x,
        y: y,
        _longWall: _longWall,
        _widthWall: _widthWall,
        _heightWall: _heightWall,
        TypeWall: TypeWall,
        IdWall: IdWall,
        IdWall_0: IdWall_0,
        Sub_Long_0: Sub_Long_0,
        Sub_Long_180: Sub_Long_180,
        IdWall_180: IdWall_180,
        IdWall_90: IdWall_90,
        Sub_Long_90: Sub_Long_90,
        IdWall_270: IdWall_270,
        Sub_Long_270: Sub_Long_270,
        MeshActive: true,
        IdUndoRedo: IdUndoRedo,
        IsFirstWall: IsFirstWall,
        IdTypeFormworkMode: IdTypeFormworkMode,
    });
    IdNameTemporal = IdWall;
    Edit_Wall = 20;
    InsertWall = 0;
    var subInicioAndFin = 0;
    var meshWall = null;
    var loaderMesh_0 = new THREE.STLLoader();
    var ElementMesh_0 = "../../Content/DesignTools/Control/Cube.stl";
    loaderMesh_0.load(ElementMesh_0, function (geometry) {
        meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.receiveShadow = true;
        meshWall.MeshTypeWall = TypeWall;
        meshWall.idWall = IdWall;
        meshWall.name = IdWall;
        meshWall.IdWall_0 = IdWall_0;
        meshWall.IdWall_180 = IdWall_180;
        meshWall.IdWall_90 = IdWall_90;
        meshWall.IdWall_270 = IdWall_270;
        meshWall.Sub_Long_0 = Sub_Long_0;
        meshWall.Sub_Long_180 = Sub_Long_180;
        meshWall.Sub_Long_90 = Sub_Long_90;
        meshWall.Sub_Long_270 = Sub_Long_270;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Variable que se utiliza con conexiones;
        meshWall.IsTypeConexion = null;
        //Add Tape
        if (TypeWall === 'Wall_R000') {
            var w = _widthWall;
            if (w >= 0.3) {
                if (w === 0.3 || w === 0.45 || w === 0.60 || w === 0.75 || w === 0.90) {
                    meshWall.Tape_0 = "TapeS4";
                }
                else {
                    if (w === 0.35 || w === 0.40 || w === 0.55)
                    {
                        meshWall.Tape_0 = "TapeS6";
                    }
                    else {
                        if (Number.isInteger(w / 0.05)) {
                            meshWall.Tape_0 = "TapeS9";
                        }
                        else {
                            meshWall.Tape_0 = "TapeS12";
                        }
                    }
                }
            }
            else {
                meshWall.Tape_0 = "TapeS3";
            }
        }



        meshWall.Grupo = 0;
        meshWall.IsFormwork = true;
        if (OldWall === false) {
            OldWall = undefined;
        }
        if (OldWall === '0') {
            OldWall = undefined;
        }
        if (OldWall !== undefined) {
            meshWall.CHeckDimWall = OldWall.CHeckDimWall;
            meshWall.CHeckBracketInside = OldWall.CHeckBracketInside;
            meshWall.CHeckBracketOutside = OldWall.CHeckBracketOutside;
            meshWall.CHeckRijiInside = OldWall.CHeckRijiInside;
            meshWall.CHeckRijiOutside = OldWall.CHeckRijiOutside
            meshWall.CHeckPropInside = OldWall.CHeckPropInside
            meshWall.CHeckPropOutside = OldWall.CHeckPropOutside
            meshWall.CHeckPropInsideInf = OldWall.CHeckPropInsideInf
            meshWall.CHeckPropOutsideInf = OldWall.CHeckPropOutsideInf
            meshWall.CHeck750R = OldWall.CHeck750R;
            meshWall.IsSolutionCornerYUniversalPanelCorner = OldWall.IsSolutionCornerYUniversalPanelCorner;
            meshWall.IsSolutionCornerXUniversalPanelCorner = OldWall.IsSolutionCornerXUniversalPanelCorner;
            meshWall.Tape_0 = OldWall.Tape_0;
            meshWall.Tape_180 = OldWall.Tape_180;
            meshWall.name = OldWall.name;
            meshWall.idWall = OldWall.idWall;
            meshWall.IsFormwork = OldWall.IsFormwork;
        }

        if (meshWall.IsFormwork === undefined) {
            meshWall.IsFormwork = true;
        }

        if (meshWall.IdWall_0 !== "0") {
            meshWall.Tape_0 = "0";
        }
        meshWall.position.set(x, 0, y);
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = _longWall / 10;
        meshWall.scale.y = _widthWall / 10;
        meshWall.scale.z = _heightWall / 10;
        meshWall.Iniciall_Wall = parseInt(subInicioAndFin / 2);
        meshWall.End_Wall = parseInt(subInicioAndFin / 2);
        meshWall.castShadow = true;
        meshWall.receiveShadow = true;
        idWalltoAdd = meshWall.id;
        meshWall.IdUndoRedo = IdUndoRedo;
        if (IdTypeFormworkMode == undefined) {
            meshWall.IdTypeFormworkMode = true;
        }
        else {
            meshWall.IdTypeFormworkMode = IdTypeFormworkMode;
        }
        scene.add(meshWall);
        //    DrawPoint(x, 275, y);
    });
    scene.traverse(function (child) {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
        }
    });
    InsertWall = 102;
    IsFormArtive = false;
};
function AddWall_R900(
    /*1*/  x,
    /*2*/  y,
    /*3*/  _longWall,
    /*4*/  _widthWall,
    /*5*/  _heightWall,
    /*6*/  TypeWall,
    /*7*/  IdWall,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo,
    /*17*/ OldWall,
    /*18*/ IdTypeFormworkMode,

) {
    _ListUndo_Redo.push({
        Type: "AddWall_R900",
        x: x,
        y: y,
        _longWall: _longWall,
        _widthWall: _widthWall,
        _heightWall: _heightWall,
        TypeWall: TypeWall,
        IdWall: IdWall,
        IdWall_0: IdWall_0,
        Sub_Long_0: Sub_Long_0,
        Sub_Long_180: Sub_Long_180,
        IdWall_180: IdWall_180,
        IdWall_90: IdWall_90,
        Sub_Long_90: Sub_Long_90,
        IdWall_270: IdWall_270,
        Sub_Long_270: Sub_Long_270,
        MeshActive: true,
        IdUndoRedo: IdUndoRedo,
        IdTypeFormworkMode: IdTypeFormworkMode,
    });
    IdNameTemporal = IdWall;
    Edit_Wall = 20;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(x, _heightWall * 1000, y);
        meshWall.idWall = IdWall;
        meshWall.name = IdWall;
        meshWall.MeshTypeWall = TypeWall;
        meshWall.IdWall_0 = IdWall_0;
        meshWall.IdWall_180 = IdWall_180;
        meshWall.IdWall_270 = IdWall_270;
        meshWall.Sub_Long_0 = Sub_Long_0;
        meshWall.Sub_Long_180 = Sub_Long_180;
        meshWall.Sub_Long_90 = Sub_Long_90;
        meshWall.Sub_Long_270 = Sub_Long_270;
        meshWall.rotation.x = 0;
        meshWall.rotation.z = 0;
        meshWall.CHeck750R = true;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = _widthWall;
        meshWall.scale.y = _heightWall;
        meshWall.scale.z = _longWall;
        meshWall.castShadow = true;
        meshWall.receiveShadow = true;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.IsFormwork = true;
        if (OldWall === false)
        {
            OldWall = undefined;
        }
        if (OldWall === '0') {
            OldWall = undefined;
        }
        if (OldWall !== undefined) {
            meshWall.CHeckDimWall = OldWall.CHeckDimWall;
            meshWall.CHeckBracketInside = OldWall.CHeckBracketInside;
            meshWall.CHeckBracketOutside = OldWall.CHeckBracketOutside;
            meshWall.CHeckRijiInside = OldWall.CHeckRijiInside;
            meshWall.CHeckRijiOutside = OldWall.CHeckRijiOutside
            meshWall.CHeckPropInside = OldWall.CHeckPropInside
            meshWall.CHeckPropOutside = OldWall.CHeckPropOutside
            meshWall.CHeckPropInsideInf = OldWall.CHeckPropInsideInf
            meshWall.CHeckPropOutsideInf = OldWall.CHeckPropOutsideInf
            meshWall.CHeck750R = OldWall.CHeck750R;
            meshWall.IsSolutionCornerYUniversalPanelCorner = OldWall.IsSolutionCornerYUniversalPanelCorner;
            meshWall.IsSolutionCornerXUniversalPanelCorner = OldWall.IsSolutionCornerXUniversalPanelCorner;
            meshWall.Tape_90 = OldWall.Tape_90;
            meshWall.Tape_270 = OldWall.Tape_270;
            meshWall.name = OldWall.name;
            meshWall.idWall = OldWall.idWall;
            meshWall.IsFormwork = OldWall.IsFormwork;
        }
        if (meshWall.IsFormwork === undefined) {
            meshWall.IsFormwork = true;
        }
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        meshWall.Tape_90 = "";
        meshWall.Tape_270 = "";
        meshWall.IdWall_90 = IdWall_90;
        meshWall.IdWall_270 = IdWall_270;
        meshWall.Sub_Long_90 = Sub_Long_90;
        meshWall.Sub_Long_270 = Sub_Long_270;
        meshWall.Grupo = 0;
        meshWall.IdUndoRedo = IdUndoRedo;
        meshWall.IsTypeConexion = null;
        if (IdTypeFormworkMode == undefined) {
            meshWall.IdTypeFormworkMode = true;
        }
        else {
            meshWall.IdTypeFormworkMode = IdTypeFormworkMode;
        }
        scene.add(meshWall);
        //DrawPoint(x, 275, y);
        InsertWall = 102;
        IsFormArtive = false;
    });
    const Wall_texture = new THREE.TextureLoader().load("../../Content/DesignTools/Material/concrete.png");
    Wall_texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
    Wall_texture.colorSpace = THREE.SRGBColorSpace;
    const wallmeshMaterial = new THREE.MeshPhongMaterial({ map: Wall_texture });
    var LoadFace1 = new THREE.BoxGeometry(1000, 1000, 1000);
    meshFace1 = new THREE.Mesh(LoadFace1, wallmeshMaterial);
    meshFace1.position.set(x, 0, y);
    meshFace1.rotation.x = -0.5 * Math.PI;
    meshFace1.rotation.z = Math.PI;
    meshFace1.scale.set(1, 1, 1);
    meshFace1.scale.x = _longWall / 10;
    meshFace1.scale.z = _heightWall / 10;
    meshFace1.position.x = x + (_longWall * 100) / 2;
    meshFace1.scale.y = 0.001;
    meshFace1.position.y = 0 + (_heightWall * 100) / 2;
    meshFace1.name = "Waal_Face1_" + IdWall;
    meshFace1.visible = true;
    //scene.add(meshFace1);
    var LoadFace2 = new THREE.BoxGeometry(1000, 1000, 1000);
    LoadFace2 = new THREE.Mesh(LoadFace1, wallmeshMaterial);
    LoadFace2.position.set(x, 0, y);
    LoadFace2.rotation.x = -0.5 * Math.PI;
    LoadFace2.rotation.z = Math.PI;
    LoadFace2.scale.set(1, 1, 1);
    LoadFace2.scale.x = _longWall / 10;
    LoadFace2.scale.z = _heightWall / 10;
    LoadFace2.scale.y = 0.001;
    LoadFace2.position.x = x + (_longWall * 100) / 2;
    LoadFace2.position.y = 0 + (_heightWall * 100) / 2;
    LoadFace2.position.z = y - (_widthWall * 100);
    LoadFace2.name = "Waal_Face1_" + IdWall;
    LoadFace2.visible = true;
    //    scene.add(LoadFace2);
};
function InsertPilar(x, y, ZRotate, _longWall, _widthWall, _heightWall) {
    Edit_Wall = 20;
    InsertWall = 0;
    var partName = new Date().valueOf();
    var NameWall = "Pilar" + partName;
    var loader = new THREE.STLLoader();
    var _milllongWall = parseInt(_longWall * 1000);
    var subInicioAndFin = 0;
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.castShadow = true;
        meshWall.receiveShadow = true;
        meshWall.scale.x = _longWall / 10;
        meshWall.scale.y = _widthWall / 10;
        meshWall.scale.z = _heightWall / 10;
        meshWall.Iniciall_Wall = parseInt(subInicioAndFin / 2);
        meshWall.End_Wall = parseInt(subInicioAndFin / 2);
        meshWall.MeshTypeWall = "Pilar";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = true;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;


        meshWall.CHeckPropInside = document.getElementById("CHeckPropInsidepilar").checked;
        meshWall.CHeckPropOutside = null;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInfPilar").checked;
        meshWall.CHeckPropOutsideInf = null;
        meshWall.IsTypeConexion = null;

        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = partName;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        meshWall.Tape_0 = "0";
        meshWall.Tape_180 = "0";
        meshWall.Tape_90 = "";
        meshWall.Tape_270 = "";
        meshWall.Grupo = 0;
        scene.add(meshWall);
        /*      HelpSelectMesh();*/
        InsertWall = 102;
        $("#MenubottomDesign").hide("slide", { direction: "left" }, 400);
        controls = ReturnControlsForCamera(camera, 1);
        IsFormArtive = false;
    });
};
function InsertWorker(x, y) {
    rollOverMesh.visible = false;
    var materialWorker = new THREE.MeshLambertMaterial({ color: 0xFAD7A0 });
    var partName = new Date().valueOf();
    var NameWall = "Worker" + partName;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Stl/ATK60/Worker.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorker);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerCasco = new THREE.MeshLambertMaterial({ color: 0xFDFEFE });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerCasco.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerCasco);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerGuantes = new THREE.MeshLambertMaterial({ color: 0x512202 });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerGuantes.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerGuantes);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerChaleco = new THREE.MeshLambertMaterial({ color: 0xE1FA05 });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerChaleco.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerChaleco);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerCamiseta = new THREE.MeshLambertMaterial({ color: 0x1F05FA });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerCamiseta.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerCamiseta);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    InsertWall = 102;
};
function insertWorker_00(mesh) {
    var materialWorker = new THREE.MeshLambertMaterial({ color: 0xFAD7A0 });
    var partName = new Date().valueOf();
    var NameWall = "Worker" + partName;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Stl/ATK60/Worker.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorker);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerCasco = new THREE.MeshLambertMaterial({ color: 0xFDFEFE });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerCasco.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerCasco);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerGuantes = new THREE.MeshLambertMaterial({ color: 0x512202 });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerGuantes.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerGuantes);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerChaleco = new THREE.MeshLambertMaterial({ color: 0xE1FA05 });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerChaleco.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerChaleco);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    var materialWorkerCamiseta = new THREE.MeshLambertMaterial({ color: 0x1F05FA });
    loader.load("../../Content/DesignTools/Stl/ATK60/WorkerCamiseta.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWorkerCamiseta);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = 10;
        meshWall.scale.y = 10;
        meshWall.scale.z = 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Warker";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = document.getElementById("CHeckDimWall").checked;
        meshWall.CHeckBracketInside = document.getElementById("CHeckBracketInside").checked;
        meshWall.CHeckBracketOutside = document.getElementById("CHeckBracketOutside").checked;
        meshWall.CHeckRijiInside = document.getElementById("CHeckRijiInside").checked;
        meshWall.CHeckRijiOutside = document.getElementById("CHeckRijiOutside").checked;
        meshWall.CHeckPropInside = document.getElementById("CHeckPropInside").checked;
        meshWall.CHeckPropOutside = document.getElementById("CHeckPropOutside").checked;
        meshWall.CHeckPropInsideInf = document.getElementById("CHeckPropInsideInf").checked;
        meshWall.CHeckPropOutsideInf = document.getElementById("CHeckPropOutsideInf").checked;
        meshWall.CHeck750R = document.getElementById("CHeck750R").checked;
        meshWall.idWall = 0;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        meshWall.IsSolutionCornerYUniversalPanelCorner = false;
        meshWall.IsSolutionCornerXUniversalPanelCorner = false;
        //Add Tape
        meshWall.Tape_0 = "";
        meshWall.Tape_180 = "";
        meshWall.Grupo = 0;
        meshWall.IdWall_0 = "0";
        meshWall.Sub_Long_0 = "0";
        meshWall.Sub_Long_180 = "0";
        meshWall.IdWall_180 = "0";
        meshWall.IdWall_90 = "0";
        meshWall.Sub_Long_90 = "0";
        meshWall.IdWall_270 = "0";
        meshWall.Sub_Long_270 = "0";
        meshWall.IsTypeConexion = null;
        scene.add(meshWall);
    });
    InsertWall = 102;
    $("#MenubottomDesign").hide("slide", { direction: "left" }, 400);
    controls = ReturnControlsForCamera(camera, 1);
    IsFormArtive = false;
}
function insertwall_00(mesh) {
    Edit_Wall = 20;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(mesh.position.x, 0, mesh.position.z);
        meshWall.idWall = mesh.idWall;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = mesh.name;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = mesh.scale.x;
        meshWall.scale.y = mesh.scale.y;
        meshWall.scale.z = mesh.scale.z;
        meshWall.Iniciall_Wall = mesh.Iniciall_Wall;
        meshWall.End_Wall = mesh.End_Wall;
        meshWall.MeshTypeWall = mesh.MeshTypeWall;
        meshWall.MeshTypeWallLeft = mesh.TypeWallLeft;
        meshWall.MeshTypeWallRight = mesh.MeshTypeWallRight;
        meshWall.MeshTypeWall_180 = mesh.MeshTypeWall_180;
        meshWall.MeshTypeWall_0 = mesh.MeshTypeWall_0;
        meshWall.IdCornerDown = mesh.IdCornerDown;
        meshWall.IdCornerLeft = mesh.IdCornerLeft;
        meshWall.ScaleEsqy = mesh.ScaleEsqy;
        meshWall.CHeckDimWall = mesh.CHeckDimWall;
        meshWall.CHeckBracketInside = mesh.CHeckBracketInside;
        meshWall.CHeckBracketOutside = mesh.CHeckBracketOutside;
        meshWall.CHeckRijiInside = mesh.CHeckRijiInside;
        meshWall.CHeckRijiOutside = mesh.CHeckRijiOutside;
        meshWall.CHeckPropInside = mesh.CHeckPropInside;
        meshWall.CHeckPropOutside = mesh.CHeckPropOutside;
        meshWall.CHeckPropInsideInf = mesh.CHeckPropInsideInf;
        meshWall.CHeckPropOutsideInf = mesh.CHeckPropOutsideInf;
        meshWall.CHeck750R = mesh.CHeck750R;
        meshWall.idWall = mesh.idWall;
        meshWall.LongLeft = mesh.LongLeft;
        meshWall.LongRight = mesh.LongRight;
        meshWall.IsSolutionCornerYUniversalPanelCorner = mesh.IsSolutionCornerYUniversalPanelCorner;
        meshWall.IsSolutionCornerXUniversalPanelCorner = mesh.IsSolutionCornerXUniversalPanelCorner;
        meshWall.Tape_0 = mesh.Tape_0;
        meshWall.Tape_180 = mesh.Tape_180;
        meshWall.Tape_90 = mesh.Tape_90;
        meshWall.Tape_270 = mesh.Tape_270;
        meshWall.Grupo = mesh.Grupo;
        meshWall.Sub_Long_0 = mesh.Sub_Long_0;
        meshWall.Sub_Long_180 = mesh.Sub_Long_180;
        meshWall.Sub_Long_90 = mesh.Sub_Long_90;
        meshWall.Sub_Long_270 = mesh.Sub_Long_270;
        meshWall.IdWall_270 = mesh.IdWall_270;
        meshWall.IdWall_0 = mesh.IdWall_0;
        meshWall.IdWall_180 = mesh.IdWall_180;
        meshWall.IdWall_90 = mesh.IdWall_90;
        meshWall.TypeWall_0 = mesh.TypeWall_0;
        meshWall.TypeWall_180 = mesh.TypeWall_180;
        meshWall.TypeWall_90 = mesh.TypeWall_90;
        meshWall.TypeWall_270 = mesh.TypeWall_270;
        meshWall.IsTypeConexion = null;
        meshWall.IdTypeFormworkMode = mesh.IdTypeFormworkMode;
        scene.add(meshWall);
    });
    InsertWall = 102;
    $("#MenubottomDesign").hide("slide", { direction: "left" }, 400);
    controls = ReturnControlsForCamera(camera, 1);
    IsFormArtive = false;
}
function insertwall_90(mesh) {
    var _heightWall = mesh.scale.y;
    Edit_Wall = 20;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(mesh.position.x, _heightWall * 1000, mesh.position.z);
        meshWall.idWall = mesh.idWall;
        meshWall.rotation.x = 0;
        meshWall.rotation.z = 0;
        meshWall.name = mesh.name;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = mesh.scale.x;
        meshWall.scale.y = mesh.scale.y;
        meshWall.scale.z = mesh.scale.z;
        meshWall.Iniciall_Wall = mesh.Iniciall_Wall;
        meshWall.End_Wall = mesh.End_Wall;
        meshWall.MeshTypeWall = mesh.MeshTypeWall;
        meshWall.MeshTypeWallLeft = mesh.TypeWallLeft;
        meshWall.MeshTypeWallRight = mesh.MeshTypeWallRight;
        meshWall.MeshTypeWall_180 = mesh.MeshTypeWall_180;
        meshWall.MeshTypeWall_0 = mesh.MeshTypeWall_0;
        meshWall.IdCornerDown = mesh.IdCornerDown;
        meshWall.IdCornerLeft = mesh.IdCornerLeft;
        meshWall.ScaleEsqy = mesh.ScaleEsqy;
        meshWall.CHeckDimWall = mesh.CHeckDimWall;
        meshWall.CHeckBracketInside = mesh.CHeckBracketInside;
        meshWall.CHeckBracketOutside = mesh.CHeckBracketOutside;
        meshWall.CHeckRijiInside = mesh.CHeckRijiInside;
        meshWall.CHeckRijiOutside = mesh.CHeckRijiOutside;
        meshWall.CHeckPropInside = mesh.CHeckPropInside;
        meshWall.CHeckPropOutside = mesh.CHeckPropOutside;
        meshWall.CHeckPropInsideInf = mesh.CHeckPropInsideInf;
        meshWall.CHeckPropOutsideInf = mesh.CHeckPropOutsideInf;
        meshWall.CHeck750R = mesh.CHeck750R;
        meshWall.idWall = mesh.idWall;
        meshWall.LongLeft = mesh.LongLeft;
        meshWall.LongRight = mesh.LongRight;
        meshWall.IsSolutionCornerYUniversalPanelCorner = mesh.IsSolutionCornerYUniversalPanelCorner;
        meshWall.IsSolutionCornerXUniversalPanelCorner = mesh.IsSolutionCornerXUniversalPanelCorner;
        meshWall.Tape_0 = mesh.Tape_0;
        meshWall.Tape_180 = mesh.Tape_180;
        meshWall.Tape_90 = mesh.Tape_90;
        meshWall.Tape_270 = mesh.Tape_270;
        meshWall.Grupo = mesh.Grupo;
        meshWall.Sub_Long_0 = mesh.Sub_Long_0;
        meshWall.Sub_Long_180 = mesh.Sub_Long_180;
        meshWall.Sub_Long_90 = mesh.Sub_Long_90;
        meshWall.Sub_Long_270 = mesh.Sub_Long_270;
        meshWall.IdWall_270 = mesh.IdWall_270;
        meshWall.IdWall_0 = mesh.IdWall_0;
        meshWall.IdWall_180 = mesh.IdWall_180;
        meshWall.IdWall_90 = mesh.IdWall_90;
        meshWall.TypeWall_0 = mesh.TypeWall_0;
        meshWall.TypeWall_180 = mesh.TypeWall_180;
        meshWall.TypeWall_90 = mesh.TypeWall_90;
        meshWall.TypeWall_270 = mesh.TypeWall_270;
        meshWall.IsTypeConexion = null;
        meshWall.IdTypeFormworkMode = mesh.IdTypeFormworkMode;
        scene.add(meshWall);
    });
    InsertWall = 102;
    $("#MenubottomDesign").hide("slide", { direction: "left" }, 400);
    controls = ReturnControlsForCamera(camera, 1);
    IsFormArtive = false;
}
function getMesh(IdWall) {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].idWall === undefined) {
                continue;
            }
            if (scene.children[i].name === IdWall) {
                return scene.children[i];
            }
        }
    }
    return "";
};

function getallMeshHelp() {
    var l = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            l.push
                ({
                    objList: scene.children[i],
                });
        }
    }
    return l;
};


function getallMesh() {
    var l = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].IdUndoRedo > 0) {
                l.push
                    ({
                        objList: scene.children[i].IdUndoRedo,
                    });
            }
        }
    }
    return l;
};
function DrawPoint(x, y, z) {
    var dotGeometry = new THREE.Geometry();
    dotGeometry.vertices.push(new THREE.Vector3(x, y, z));
    var dotMaterial = new THREE.PointsMaterial({ size: 8, sizeAttenuation: false });
    var dot = new THREE.Points(dotGeometry, dotMaterial);
    scene.add(dot);
};

function DrawPointText(x, y, z) {
    var dotGeometry = new THREE.Geometry();
    dotGeometry.vertices.push(new THREE.Vector3(x, y, z));
    var dotMaterial = new THREE.PointsMaterial({ size: 8, sizeAttenuation: false });
    var dot = new THREE.Points(dotGeometry, dotMaterial);
    scene.add(dot);
};

function AddGrill_R000(x, y, _longWall, _widthWall, _heightWall, TypeWall, IdWall) {
    var loaderGrill_V = new THREE.STLLoader();
    loaderGrill_V.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshGrill_V = new THREE.Mesh(geometry, materialGrill);
        meshGrill_V.MeshTypeWall = TypeWall;
        meshGrill_V.idWall = IdWall;
        meshGrill_V.name = IdWall;
        meshGrill_V.position.set(x, 0, y);
        meshGrill_V.rotation.x = -0.5 * Math.PI;
        meshGrill_V.rotation.z = Math.PI;
        meshGrill_V.scale.set(1, 1, 1);
        meshGrill_V.scale.x = _longWall;
        meshGrill_V.scale.y = _widthWall;
        meshGrill_V.scale.z = _heightWall;
        scene.add(meshGrill_V);
    });
    InsertWall = 102;
    IsFormArtive = false;
};
function AddGrill_R900(
    /*1*/  x,
    /*2*/  y,
    /*3*/  _longWall,
    /*4*/  _widthWall,
    /*5*/  _heightWall,
    /*6*/  TypeWall,
    /*7*/  IdWall,

) {
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshGrill = new THREE.Mesh(geometry, materialGrill);
        meshGrill.position.set(x, _heightWall * 1000, y);
        meshGrill.idWall = IdWall;
        meshGrill.name = IdWall;
        meshGrill.MeshTypeWall = TypeWall;
        meshGrill.scale.set(1, 1, 1);
        meshGrill.scale.x = _widthWall;
        meshGrill.scale.y = _heightWall;
        meshGrill.scale.z = _longWall;
        meshGrill.material.opacity = 0.3;
        scene.add(meshGrill);
        InsertWall = 102;
        IsFormArtive = false;
    });

};