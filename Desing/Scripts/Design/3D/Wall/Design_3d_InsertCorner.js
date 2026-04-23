function AddCorner10_00(IsTemporal, ReferentPoint, FishConetionWall) {
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq10.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdEsq_00_D = "Esq_10_00" + IdpartName;
    var IdEsq_90_D = "Esq_10_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = obWall.position.x;
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub * 10;
    var EsqLefWidth = obWall.scale.y * 10;
    var EsqLefHeigh = obWall.scale.z * 10;
    //"Esq_10_00"
    AddWall_R000(
        /* 1*/    EsqLefXPosition,
        /* 2*/    EsqLefYPosition,
        /* 3*/    EsqLefLong,
        /* 4*/    EsqLefWidth,
        /* 5*/    EsqLefHeigh,
        /* 6*/   "Esq_10_00",
        /* 7*/    IdEsq_00_D,
        /* 8*/    IdWall_00_D,
        /* 9*/    Sub_Long_0,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdEsq_90_D,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );
    var EsqTopXPosition = obWall.position.x + (obWall.scale.y * 1000);
    var EsqTopYPosition = obWall.position.z - (obWall.scale.y * 1000);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    // "Esq_10_90"
    AddWall_R900(
 /*1*/   EsqTopXPosition,
 /*2*/   EsqTopYPosition,
 /*3*/   EsqTopLong,
 /*4*/   EsqTopWidth,
 /*5*/   EsqTopHeigh,
 /*6*/   "Esq_10_90",
 /*7*/   IdEsq_90_D,
 /*8*/   IdEsq_00_D,
 /*9*/   Sub_Long_0,
 /*10*/  Sub_Long_180,
 /*11*/  IdWall_180,
 /*12*/  IdWall_90,
 /*13*/  Sub_Long_90,
 /*14*/  IdWall_90_D,
 /*15*/  Sub_Long_270,
 /*16*/  IdUndoRedo,
        /*17*/
    );
    var EsqDownXPosition = obWall.position.x + (obWall.scale.y * 1000);
    var EsqDownYPosition = obWall.position.z + ((xSub * 1000) - (obWall.scale.y * 1000));
    var EsqDownLong = 0.15;
    if (ReferentPoint > 0) {

    }
    else {
        var EsqDownWidth = obWall.scale.y;
        var EsqDownHeigh = obWall.scale.z;
        AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition,
/*3*/   EsqDownLong,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  IdEsq_90_D,
/*13*/  xSub,
/*14*/  IdWall_270,
/*15*/  Sub_Long_270,
/*16*/  idUndoRedoTemp,
            /*17*/
        );
    }

    var WallTopXPosition = obWall.position.x + xSub * 1000;
    var WallTopYPosition = obWall.position.z;
    var WallopLong = (obWall.scale.x * 10) - xSub * 10;
    var WallTopWidth = obWall.scale.y * 10;
    var WallTopHeigh = obWall.scale.z * 10;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R000(
/*1*/  WallTopXPosition,
/*2*/  WallTopYPosition,
/*3*/  WallopLong,
/*4*/  WallTopWidth,
/*5*/  WallTopHeigh,
/*6*/  "Wall_R000",
/*7*/  IdWall_00_D,
/*8*/  IdWall_0,
/*9*/  Sub_Long_0,
/*10*/ xSub,
/*11*/ IdEsq_00_D,
/*12*/ IdWall_90,
/*13*/ Sub_Long_90,
/*14*/ IdWall_270,
/*15*/ Sub_Long_270,
/*16*/ IdUndoRedo,
/*17*/ false,
/*18*/ obWall,
    );
    //WallTop_
    scene.remove(obWall);
    InsertWall = 102;
};
function AddCorner10_90(IsTemporal) {
    meshEsq10.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdEsq_00_D = "Esq_10_00" + IdpartName;
    var IdEsq_90_D = "Esq_10_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = obWall.IdWall_270;
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.x);
    IdUndoRedo = IdUndoRedo + 1;
    var EsqLefXPosition = obWall.position.x - (obWall.scale.x * 1000);
    var EsqLefYPosition = obWall.position.z + (obWall.scale.x * 1000);
    var EsqLefWidth = obWall.scale.x * 10;
    var EsqLefHeigh = obWall.scale.y * 10;
    var EsqLefLong = xSub * 10;
    //"Esq_10_00",
    AddWall_R000(
        /* 1*/    EsqLefXPosition,
        /* 2*/    EsqLefYPosition,
        /* 3*/    EsqLefLong,
        /* 4*/    EsqLefWidth,
        /* 5*/    EsqLefHeigh,
        /* 6*/   "Esq_10_00",
        /* 7*/    IdEsq_00_D,
        /* 8*/    IdWall_00_D,
        /* 9*/    Sub_Long_0,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdEsq_90_D,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );
    //WallEsqTAbajo_
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = obWall.position.z;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;
    //"Esq_10_90",
    AddWall_R900(
 /*1*/   EsqTopXPosition,
 /*2*/   EsqTopYPosition,
 /*3*/   EsqTopLong,
 /*4*/   EsqTopWidth,
 /*5*/   EsqTopHeigh,
 /*6*/   "Esq_10_90",
 /*7*/   IdEsq_90_D,
 /*8*/   IdEsq_00_D,
 /*9*/   Sub_Long_0,
 /*10*/  Sub_Long_180,
 /*11*/  IdWall_180,
 /*12*/  IdWall_90,
 /*13*/  Sub_Long_90,
 /*14*/  IdWall_90_D,
 /*15*/  Sub_Long_270,
 /*16*/  IdUndoRedo,
        /*17*/
    );
    var WallTopXPosition = (obWall.position.x - (obWall.scale.x * 1000)) + (xSub * 1000);
    var WallTopYPosition = obWall.position.z + (obWall.scale.x * 1000);
    var WallopLong = 1.5;
    var WallTopWidth = obWall.scale.x * 10;
    var WallTopHeigh = obWall.scale.y * 10;
    AddWall_R000(
/*1*/  WallTopXPosition,
/*2*/  WallTopYPosition,
/*3*/  WallopLong,
/*4*/  WallTopWidth,
/*5*/  WallTopHeigh,
/*6*/  "Wall_R000",
/*7*/  IdWall_00_D,
/*8*/  IdWall_0,
/*9*/  Sub_Long_0,
/*10*/ xSub,
/*11*/ IdEsq_00_D,
/*12*/ IdWall_90,
/*13*/ Sub_Long_90,
/*14*/ IdWall_270,
/*15*/ Sub_Long_270,
/*16*/ IdUndoRedo,
/*17*/ false
    );
    //Old WallTop_
    var EsqDownXPosition = obWall.position.x;
    var EsqDownYPosition = obWall.position.z + (xSub * 1000);
    var EsqDownLong = obWall.scale.z - xSub;
    var EsqDownWidth = obWall.scale.x;
    var EsqDownHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition,
/*3*/   EsqDownLong,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  IdEsq_90_D,
/*13*/  xSub,
/*14*/  IdWall_270,
/*15*/  Sub_Long_270,
/*16*/  idUndoRedoTemp,
/*17*/  obWall,
    );
    scene.remove(obWall);
    InsertWall = 102;
}
function AddCorner20_00(Value, IsTemporal, wallD, ValueNewWall) {
    if (wallD !== undefined) {
        obWall = wallD;
    }
    var IdpartName = new Date().valueOf();
    var IdEsq_00_D = "Esq_20_00" + IdpartName;
    var IdEsq_90_D = "Esq_20_90" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_180_D = "Wall_R000_180" + IdpartName;
    var IdWall_00_D = "Wall_R000_0" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y + 0.03);
    IdUndoRedo = IdUndoRedo + 1;
    Value = parseFloat(Value);
    meshEsq20.visible = false;
    //WallEsqTLe_ oldWall
    var WallXPosition = obWall.position.x;
    var WallLong = Value - 0.03;
    var WallYPosition = obWall.position.z;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
     /*1*/   WallXPosition,
     /*2 */  WallYPosition,
     /*3 */  WallLong * 10,
     /*4 */  WallWidth * 10,
     /*5 */  Wallheigh * 10,
     /*6 */  "Wall_R000_180",
     /*7 */  IdWall_180_D,
     /*8 */  IdEsq_00_D,
     /*9 */  /*Sub_Long_0,*/ xSub,
     /*10 */ obWall.Sub_Long_180,
     /*11 */ obWall.IdWall_180,
     /*12 */ IdWall_90,
     /*13 */ Sub_Long_90,
     /*14 */ IdWall_270,
     /*15 */ Sub_Long_270,
     /*16 */ IdUndoRedo,
     /*17*/  false,
     /*18*/  obWall,
    );
    var WallXEsq = WallXPosition + WallLong * 1000;
    var WallXLong = xSub;
    /* Wall Right*/
    //getIdEsq30(obWall);
    AddWall_R000(
/* 1*/  WallXEsq,
/* 2*/  WallYPosition,
/* 3*/  WallXLong * 10,
/* 4*/  WallWidth * 10,
/* 5*/  Wallheigh * 10,
/* 6*/  "Esq_20_00",
/* 7*/  IdEsq_00_D,
/* 8*/  IdWall_00_D,
/* 9*/  Sub_Long_0,
/* 10*/ Sub_Long_180,
/* 11*/  IdWall_180_D,
/* 12*/  IdWall_90,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_00_D,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/  false
    );
    var Wall2XPosition = WallXEsq + (xSub * 1000);
    var res = (xSub + Value) - 0.03;
    var Wall2Long = obWall.scale.x - res;
    var Wall2YPosition = obWall.position.z;
    var Wall2Width = obWall.scale.y;
    var Wall2heigh = obWall.scale.z;
    AddWall_R000(
/* 1*/   Wall2XPosition,
/* 2*/   Wall2YPosition,
/* 3*/   Wall2Long * 10,
/* 4*/   Wall2Width * 10,
/* 5*/   Wall2heigh * 10,
/* 6*/   "Wall_R000_0",
/* 7*/   IdWall_00_D,
/* 8*/   obWall.IdWall_0,
/* 9*/   obWall.Sub_Long_0,
/* 10*/  /*Sub_Long_180*/ xSub,
/* 11*/  IdWall_180,
/* 12*/  IdEsq_00_D,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_270,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/   false
    );
    var restx = xSub / 2;
    restx = restx + obWall.scale.y / 2;
    var Wall90XPosition = WallXEsq + (restx * 1000);
    var Wall90YPosition = obWall.position.z - (obWall.scale.y * 1000);
    var Wall90Long = 0.03 + obWall.scale.y;
    var Wall90Width = obWall.scale.y;
    var Wall90Heigh = obWall.scale.z;
    AddWall_R900(
    /*1*/  Wall90XPosition,
    /*2*/  Wall90YPosition,
    /*3*/  Wall90Long,
    /*4*/  Wall90Width,
    /*5*/  Wall90Heigh,
    /*6*/  "Esq_20_90",
    /*7*/  IdEsq_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_90_D,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    var Wall90_2_Long = 0.15;
    if (ValueNewWall !== undefined) {
        Wall90_2_Long = ValueNewWall;
    }
    var Wall_2_90YPosition = obWall.position.z + 30;
    AddWall_R900(
    /*1*/ Wall90XPosition,
    /*1*/ Wall_2_90YPosition,
    /*1*/ Wall90_2_Long,
    /*1*/ Wall90Width,
    /*1*/ Wall90Heigh,
    /*1*/ "Wall_R900",
    /*1*/ IdWall_90_D,
    /*1*/ IdWall_0,
    /*1*/ Sub_Long_0,
    /*1*/ Sub_Long_180,
    /*1*/ IdWall_180,
    /*1*/ IdEsq_90_D,
    /*1*/ Wall90Long,
    /*1*/ IdWall_270,
    /*1*/ Sub_Long_270,
    /*1*/ IdUndoRedo
    );
    scene.remove(obWall);
    ResetSetup();
    InsertWall = 102;
};
function AddCorner70_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner70_90(true);
}
function AddCorner10_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner10_00();
}
function AddCorner20_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    var widthWall = document.getElementById("IdWidthDefault").value / 10 + 0.105;
    AddCorner20_00(parseFloat(widthWall.toFixed(3)));
}

function AddCorner30_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner30_00();
}
function AddCorner50_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner50_00();
}
function AddCorner60_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner60(0.135);
}
function AddCornerX_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCornerX_0(0.135);
}
function AddCorner40_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner40(0.135);
}
function AddCorner80_00Temp() {
    obWall = scene.getObjectByName(IdNameTemporal);
    AddCorner80(0.135);
}
function GetXsub(ScaleY) {
    var xSub = 0.045;
    if (ScaleY + 0.030 > 0.045) { xSub = 0.060; }
    if (ScaleY + 0.030 > 0.060) { xSub = 0.075; }
    if (ScaleY + 0.030 > 0.075) { xSub = 0.090; }
    if (ScaleY + 0.030 > 0.090) { xSub = 0.105; }
    if (ScaleY + 0.030 > 0.105) { xSub = 0.12; }
    if (ScaleY + 0.030 > 0.120) { xSub = 0.135; }
    if (ScaleY + 0.030 > 0.135) { xSub = 0.150; }
    if (ScaleY + 0.030 > 0.150) { xSub = 0.165; }
    if (ScaleY + 0.030 > 0.165) { xSub = 0.180; }
    return xSub;
}
function AddCorner70_90(IsTemporal) {
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = obWall.idWall;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdEsq_00_D = "Esq_70_00" + IdpartName;
    var IdEsq_90_D = "Esq_70_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = obWall.IdWall_270;
    var IdWall_0 = obWall.IdWall_0;
    var IdWall_90 = obWall.IdWall_90;
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq70.visible = false;
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = obWall.position.x;
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
    //"Esq_70_00"
    AddWall_R000(/*1*/EsqLefXPosition, /*2*/EsqLefYPosition,/*3*/ EsqLefLong * 10, /*4*/EsqLefWidth * 10,/*5*/ EsqLefHeigh * 10,
        /*6*/"Esq_70_00",
        /*7*/ IdEsq_00_D,
        /*8*/IdWall_00_D,
        /*9*/Sub_Long_0,
        /*10*/Sub_Long_180,
        /*11*/ IdWall_180,
        /*12*/IdEsq_90_D,
        /*13*/Sub_Long_90,
        /*14*/IdWall_270,
        /*15*/Sub_Long_270,
        /*16*/IdUndoRedo,
        /*17*/ false,
    );
    //WallEsqTTop_
    var EsqTopXPosition = obWall.position.x + obWall.scale.y * 1000;
    var EsqTopYPosition = obWall.position.z - xSub * 1000;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    //"Esq_70_90"
    AddWall_R900(/*1*/EsqTopXPosition,/*2*/EsqTopYPosition,/*3*/EsqTopLong,/*4*/EsqTopWidth,/*5*/EsqTopHeigh,
        /*6*/"Esq_70_90",
        /*7*/ IdEsq_90_D,
        /*8*/IdEsq_00_D,
        /*9*/Sub_Long_0,
        /*10*/Sub_Long_180,
        /*11*/IdWall_180,
        /*12*/IdWall_90_D,
        /*13*/Sub_Long_90,
        /*14*/IdWall_270,
        /*15*/Sub_Long_270,
        /*16*/IdUndoRedo,
    );
    //WallTop_
    var WallTopXPosition = obWall.position.x + (obWall.scale.y * 1000);
    var WallTopYPosition = obWall.position.z - ((xSub * 1000) + 150);
    var WallTopLong = 0.15;
    var WallTopWidth = obWall.scale.y;
    var WallTopHeigh = obWall.scale.z;
    // "Wall_R900",
    AddWall_R900(/*1*/ WallTopXPosition,/*2*/ WallTopYPosition,/*3*/ WallTopLong,/*4*/ WallTopWidth,/*5*/ WallTopHeigh,
        /*6*/ "Wall_R900",
        /*7*/ IdWall_90_D,
        /*8*/ IdWall_0,
        /*9*/ Sub_Long_0,
        /*10*/ Sub_Long_180,
        /*11*/ IdWall_180,
        /*12*/ IdWall_90,
        /*13*/ Sub_Long_90,
        /*14*/ IdEsq_90_D,
        /*15*/ xSub,
        /*16*/ IdUndoRedo,
    );
    var WallXPosition = obWall.position.x + (xSub * 1000);
    var WallYPosition = obWall.position.z;
    var WallLong = obWall.scale.x - xSub;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    //Old WallTop_ 
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) { idUndoRedoTemp = obWall.IdUndoRedo; }
    AddWall_R000(WallXPosition, WallYPosition, WallLong * 10, WallWidth * 10, Wallheigh * 10, "Wall_R000",
        obWall.IdWall,
        IdWall_0,
        Sub_Long_0,
        xSub,
        IdEsq_00_D,
        IdWall_90,
        Sub_Long_90,
        IdWall_270,
        Sub_Long_270,
        idUndoRedoTemp,
        false,
        obWall,
    );
    scene.remove(obWall);
    InsertWall = 102;
};
function AddCorner70_00(IsTemporal) {
    var xSub = GetXsub(obWall.scale.x);
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = obWall.idWall;
    var IdEsq_00_D = "Esq_70_00" + IdpartName;
    var IdEsq_90_D = "Esq_70_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_0 = "0";
    var IdWall_180 = obWall.IdWall_180;
    var IdWall_90 = "0";
    var IdWall_270 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq70.visible = false;
    var EsqLefXPosition = obWall.position.x - (obWall.scale.x * 1000);
    var EsqLefYPosition = obWall.position.z + (obWall.scale.z * 1000);
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.x;
    var EsqLefHeigh = obWall.scale.y;
    AddWall_R000(/*1*/EsqLefXPosition,/*2*/ EsqLefYPosition,/*3*/ EsqLefLong * 10,/*4*/ EsqLefWidth * 10,/*5*/ EsqLefHeigh * 10,
        /*6*/   "Esq_70_00", IdEsq_00_D,
        /*7*/   IdWall_00_D,
        /*8*/    Sub_Long_0,
        /*9*/    Sub_Long_180,
        /*10*/   IdWall_180,
        /*11*/   IdEsq_90_D,
        /*12*/   IdEsq_00_D,
        /*13*/   IdWall_270,
        /*14*/   Sub_Long_270,
        /*15*/   IdUndoRedo,
        /*16*/   false,
    );
    var EsqTopXPosition = obWall.position.x
    var EsqTopYPosition = obWall.position.z + obWall.scale.z * 1000 - (xSub * 1000);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;
    AddWall_R900(/*1*/EsqTopXPosition,/*2*/ EsqTopYPosition,/*3*/ EsqTopLong,/*4*/ EsqTopWidth,/*5*/ EsqTopHeigh,
        /*6*/   "Esq_70_90",
        /*7*/  IdEsq_90_D,
        /*8*/  IdEsq_00_D,
        /*9*/  Sub_Long_0,
        /*10*/   Sub_Long_180,
        /*11*/   IdWall_180,
        /*12*/   IdWall_90_D,
        /*13*/  Sub_Long_90,
        /*14*/  IdWall_270,
        /*15*/  Sub_Long_270,
        /*16*/   IdUndoRedo,
        /*17*/   false
    );
    // #endregion
    // #region R900 wall
    var WallTopXPosition = obWall.position.x
    var WallTopYPosition = obWall.position.z;
    var WallopLong = obWall.scale.z - xSub;
    var WallTopWidth = obWall.scale.x;
    var WallTopHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(/*1*/WallTopXPosition,/*2*/ WallTopYPosition,/*3*/ WallopLong,/*4*/ WallTopWidth,/*5*/ WallTopHeigh,
        /*6*/"Wall_R900",
        /*7*/IdWall_90_D,
        /*8*/IdWall_0,
        /*9*/Sub_Long_0,
        /*10*/Sub_Long_0,
        /*11*/Sub_Long_0,
        /*12*/Sub_Long_0,
        /*13*/Sub_Long_0,
        /*14*/Sub_Long_180,
        /*15*/IdWall_180,
        /*16*/IdWall_90,
        /*17*/Sub_Long_90,
        /*18*/IdEsq_90_D,
        /*19*/xSub,
        /*20*/IdUndoRedo,
        /*21*/obWall,
    );
    var WallXPosition = obWall.position.x + ((xSub * 1000) - (obWall.scale.x * 1000));
    var WallYPosition = obWall.position.z + (obWall.scale.z * 1000);
    var WallLong = 0.27;
    var WallWidth = obWall.scale.x;
    var Wallheigh = obWall.scale.y;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
        /*1*/   WallXPosition,
        /*2*/   WallYPosition, WallLong * 10,
        /*3*/   WallWidth * 10, Wallheigh * 10,
        /*4*/   "Wall_R000",
        /*5*/   IdWall_00_D,
        /*6*/   IdWall_0,
        /*7*/   Sub_Long_0,
        /* 8*/   xSub,
        /*9*/   IdEsq_00_D,
        /*10*/   IdWall_90,
        /*11*/   IdWall_180,
        /*12*/   IdWall_270,
        /*13*/   Sub_Long_270,
        /*14*/   IdUndoRedo,
        /*15*/   false,
    );
    scene.remove(obWall);
    InsertWall = 102;
};
function AddCorner30_00(IsTemporal) {
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq30.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = obWall.idWall;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdEsq_00_D = "Esq_30_00" + IdpartName;
    var IdEsq_90_D = "Esq_30_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = (obWall.position.x + (obWall.scale.x * 1000)) - (xSub * 1000);
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
    //"Esq_30_00",
    AddWall_R000(
     /*1*/   EsqLefXPosition,
     /*2*/   EsqLefYPosition,
     /*3*/   EsqLefLong * 10,
     /*4*/   EsqLefWidth * 10,
     /*5*/   EsqLefHeigh * 10,
     /*6*/   "Esq_30_00",
     /*7*/    IdEsq_00_D,
     /*8*/    IdWall_0,
     /*9*/    Sub_Long_0,
     /*10*/   Sub_Long_180,
     /*11*/   IdWall_00_D,
     /*12*/   IdWall_90,
     /*13*/   Sub_Long_90,
     /*14*/   IdEsq_90_D,
     /*15*/   Sub_Long_270,
     /*16*/   IdUndoRedo,
     /*17*/   false
    );
    var EsqTopXPosition = obWall.position.x + (obWall.scale.x * 1000);
    var EsqTopYPosition = obWall.position.z - (obWall.scale.y * 1000);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    // "Esq_30_90",
    AddWall_R900(
     /*1*/   EsqTopXPosition,
     /*2*/   EsqTopYPosition,
     /*3*/   EsqTopLong,
     /*4*/   EsqTopWidth,
     /*5*/   EsqTopHeigh,
     /*6*/   "Esq_30_90",
     /*7*/   IdEsq_90_D,
     /*8*/   IdWall_0,
     /*9*/   Sub_Long_0,
     /*10*/  Sub_Long_180,
     /*11*/  IdEsq_00_D,
     /*12*/  IdWall_90,
     /*13*/  Sub_Long_90,
     /*14*/  IdWall_90_D,
     /*15*/  Sub_Long_270,
     /*16*/  IdUndoRedo
    );
    var WallTopXPosition = obWall.position.x + (obWall.scale.x * 1000);
    var WallTopYPosition = obWall.position.z + ((xSub * 1000) - (obWall.scale.y * 1000));
    var WallopLong = 0.15;
    var WallTopWidth = obWall.scale.y;
    var WallTopHeigh = obWall.scale.z;
    AddWall_R900(
   /* 1 */   WallTopXPosition,
   /* 2 */ WallTopYPosition,
   /* 3 */ WallopLong,
   /* 4 */ WallTopWidth,
   /* 5 */ WallTopHeigh,
   /* 6 */ "Wall_R900",
   /* 7 */ IdWall_90_D,
   /* 8 */ IdWall_0,
   /* 9 */ Sub_Long_0,
   /* 10 */ Sub_Long_180,
   /* 11 */ IdWall_180,
   /* 12 */ IdEsq_90_D,
   /* 13 */ xSub,
   /* 14 */ IdWall_270,
   /* 15 */ Sub_Long_270,
   /* 16 */ IdUndoRedo
    );

    var WallXPosition = obWall.position.x;
    var WallYPosition = obWall.position.z;
    var WallLong = obWall.scale.x - xSub;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) { idUndoRedoTemp = obWall.IdUndoRedo; }
    AddWall_R000(
    /*1*/ WallXPosition,
    /*2*/ WallYPosition,
    /*3*/ WallLong * 10,
    /*4*/ WallWidth * 10,
    /*5*/ Wallheigh * 10,
    /*6*/ "Wall_R000",
    /*7*/ IdWall_00_D,
    /*8*/ IdEsq_00_D,
    /*9*/ xSub,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ idUndoRedoTemp,
    /*17*/ false,
    /*18*/ obWall
    );

    scene.remove(obWall);

    InsertWall = 102;
};
function AddCorner30_90(IsTemporal) {
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq30.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = obWall.idWall;
    var IdEsq_00_D = "Esq_30_00" + IdpartName;
    var IdEsq_90_D = "Esq_30_90" + IdpartName;
    var xSub = GetXsub(obWall.scale.x);
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var EsqLefXPosition = obWall.position.x - (xSub * 1000);
    var EsqLefYPosition = obWall.position.z + (obWall.scale.x * 1000);
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.x;
    var EsqLefHeigh = obWall.scale.y;
      /*6*/ "Esq_30_00",
        AddWall_R000(/*1*/ EsqLefXPosition,/*2*/ EsqLefYPosition,/*3*/ EsqLefLong * 10,/*4*/ EsqLefWidth * 10,/*5*/ EsqLefHeigh * 10,
    /*6*/ "Esq_30_00",
    /*7*/ IdEsq_00_D,
    /*8*/ IdWall_0,
    /*9*/ Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_00_D,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdEsq_90_D,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo,
    /*17*/ false
        );
    // #endregion
    // #region EsqR900
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = obWall.position.z;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;
    /*6*/    "Esq_30_90",
        AddWall_R900(/*1*/   EsqTopXPosition,/*2*/    EsqTopYPosition,/*3*/    EsqTopLong, /*4*/    EsqTopWidth,/*5*/    EsqTopHeigh,
    /*6*/    "Esq_30_90",
     /*7*/    IdEsq_90_D,
     /*8*/    IdWall_0,
     /*9*/    Sub_Long_0,
     /*10*/   Sub_Long_180,
     /*11*/   IdEsq_00_D,
     /*12*/   IdWall_90,
     /*13*/   Sub_Long_90,
     /*14*/   IdWall_90_D,
     /*15*/   Sub_Long_270,
     /*16*/   IdUndoRedo
        );
    // #endregion
    // #region Wall00
    var WallXPosition = obWall.position.x - (150 + (xSub * 1000));
    var WallYPosition = obWall.position.z + (obWall.scale.x * 1000);
    var WallLong = 0.15;
    var WallWidth = obWall.scale.x;
    var Wallheigh = obWall.scale.y;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
     /*1*/  WallXPosition,
     /*2*/  WallYPosition,
     /*3*/  WallLong * 10,
     /*4*/  WallWidth * 10,
     /*5*/  Wallheigh * 10,
     /*6*/  "Wall_R000",
     /*7*/  IdWall_00_D,
     /*8*/  IdEsq_00_D,
     /*9*/  xSub,
     /*10*/ Sub_Long_180,
     /*11*/ IdWall_180,
     /*12*/ IdWall_90,
     /*13*/ Sub_Long_90,
     /*14*/ IdWall_270,
     /*15*/ Sub_Long_270,
     /*16*/ IdUndoRedo,
     /*17*/ false
    );
    // #endregion
    // #region Wall900
    var WallTopXPosition = obWall.position.x;
    var WallTopYPosition = obWall.position.z + (xSub * 1000);
    var WallopLong = obWall.scale.z - xSub;
    var WallTopWidth = obWall.scale.x;
    var WallTopHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) { idUndoRedoTemp = obWall.IdUndoRedo; }
    AddWall_R900(
    /*1*/  WallTopXPosition,
    /*2*/  WallTopYPosition,
    /*3*/  WallopLong,
    /*4*/  WallTopWidth,
    /*5*/  WallTopHeigh,
    /*6*/  "Wall_R900",
    /*7*/  IdWall_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdEsq_90_D,
    /*13*/ xSub,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ idUndoRedoTemp,
    /*17*/ obWall
    );
    scene.remove(obWall);
    // #endregion_
    InsertWall = 102;
};
function AddCorner50_00(IsTemporal) {
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq50.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = obWall.idWall;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdEsq_00_D = "Esq_50_00" + IdpartName;
    var IdEsq_90_D = "Esq_50_90" + IdpartName;
    var xSub = GetXsub(obWall.scale.y);
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = obWall.IdWall_180;
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    //WallEsqTLe_
    var EsqLefXPosition = (obWall.position.x + (obWall.scale.x * 1000)) - (xSub * 1000);
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
    AddWall_R000(/*1*/ EsqLefXPosition,/*2*/ EsqLefYPosition,/*3*/ EsqLefLong * 10,/*4*/ EsqLefWidth * 10,/*5*/ EsqLefHeigh * 10,
        /*6*/ "Esq_50_00",
        /*7*/ IdEsq_00_D,
        /*8*/ "0",
        /*9*/ EsqLefWidth,
        /*10*/ Sub_Long_180,
        /*11*/ IdWall_00_D,
        /*12*/ IdEsq_90_D,
        /*13*/ Sub_Long_90,
        /*14*/ IdWall_270,
        /*15*/ Sub_Long_270,
        /*16*/ IdUndoRedo,
        /*17*/ false
    );
    var EsqTopXPosition = obWall.position.x + (obWall.scale.x * 1000);
    var EsqTopYPosition = obWall.position.z - xSub * 1000;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    AddWall_R900(/*1*/ EsqTopXPosition,/*2*/ EsqTopYPosition,/*3*/ EsqTopLong,/*4*/ EsqTopWidth,/*5*/ EsqTopHeigh,
      /*6*/ "Esq_50_90",
      /*7*/ IdEsq_90_D,
      /*8*/ IdWall_0,
      /*9*/ Sub_Long_0,
      /*10*/ Sub_Long_180,
      /*11*/ IdEsq_00_D,
      /*12*/ IdWall_90_D,
      /*13*/ Sub_Long_90,
      /*14*/ IdWall_270,
      /*15*/ Sub_Long_270,
      /*16*/ IdUndoRedo
    );
    //New Wall
    var WallXPosition = obWall.position.x;
    var WallYPosition = obWall.position.z;
    var WallLong = obWall.scale.x - xSub;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    var WallTopXPosition = obWall.position.x + (obWall.scale.x * 1000);
    var WallTopYPosition = obWall.position.z - ((xSub * 1000) + 150);
    var WallopLong = 0.15;
    var WallTopWidth = obWall.scale.y;
    var WallTopHeigh = obWall.scale.z;
    AddWall_R900(/*1*/ WallTopXPosition,/*2*/ WallTopYPosition,/*3*/ WallopLong,/*4*/ WallTopWidth,/*5*/ WallTopHeigh,
     /*6*/ "Wall_R900",
     /*7*/ IdWall_90_D,
     /*8*/ IdWall_0,
     /*9*/ Sub_Long_0,
     /*10*/ Sub_Long_180,
     /*11*/ IdWall_180,
     /*12*/ IdWall_90,
     /*13*/ Sub_Long_90,
     /*14*/ IdEsq_90_D,
     /*15*/ xSub,
     /*16*/ IdUndoRedo
    );
    var idUndoRedoTemp = IdUndoRedo;

    AddWall_R000(
      /*1*/ WallXPosition,
      /*2*/ WallYPosition,
      /*3*/ WallLong * 10,
      /*4*/ WallWidth * 10,
      /*5*/ Wallheigh * 10,
      /*6*/ "Wall_R000",
      /*7*/ IdWall_00_D,
      /*8*/ IdEsq_00_D,
      /*9*/ xSub,
      /*10*/ Sub_Long_180,
      /*11*/ IdWall_180,
      /*12*/ IdWall_90,
      /*13*/ Sub_Long_90,
      /*14*/ IdWall_270,
      /*15*/ Sub_Long_270,
      /*16*/ idUndoRedoTemp,
      /*17*/ false,
      /*18*/ obWall,
    );

    scene.remove(obWall);
    //WallTop_
    InsertWall = 102;
};
function AddCorner50_90(IsTemporal) {
    /*Muro90*/
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq50.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = obWall.idWall;
    var IdEsq_00_D = "Esq_50_00" + IdpartName;
    var IdEsq_90_D = "Esq_50_90" + IdpartName;
    var xSub = GetXsub(obWall.scale.x);
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var EsqLefXPosition = obWall.position.x - (xSub * 1000);
    var EsqLefYPosition = obWall.position.z + (obWall.scale.z * 1000);
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.x;
    var EsqLefHeigh = obWall.scale.y;
    AddWall_R000(/*1*/EsqLefXPosition,/*2*/EsqLefYPosition,/*3*/EsqLefLong * 10,/*4*/EsqLefWidth * 10,/*5*/EsqLefHeigh * 10,
    /*6*/    "Esq_50_00",
    /*7*/    IdEsq_00_D,
    /*8*/    IdWall_0,
    /*9*/    Sub_Long_0,
    /*10*/   Sub_Long_180,
    /*11*/   IdWall_00_D,
    /*12*/   IdEsq_90_D,
    /*13*/   Sub_Long_90,
    /*14*/   IdWall_270,
    /*15*/   Sub_Long_270,
    /*16*/   IdUndoRedo,
    /*17*/   false
    );
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = obWall.position.z + ((obWall.scale.z * 1000) - (xSub * 1000));
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;
    AddWall_R900(/*1*/  EsqTopXPosition,/*2*/  EsqTopYPosition,/*3*/  EsqTopLong,/*4*/  EsqTopWidth,/*5*/  EsqTopHeigh,/*6*/
        "Esq_50_90",
    /*7*/  IdEsq_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdEsq_00_D,
    /*12*/ IdWall_90_D,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    //New Wall
    var WallXPosition = obWall.position.x - ((xSub * 1000) + 150);
    var WallYPosition = obWall.position.z + (obWall.scale.z * 1000);
    var WallLong = 0.15;
    var WallWidth = obWall.scale.x;
    var Wallheigh = obWall.scale.y;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(/*1*/  WallXPosition,/*2*/  WallYPosition,/*3*/  WallLong * 10,/*4*/  WallWidth * 10,/*5*/  Wallheigh * 10,
      /*6*/  "Wall_R000",
      /*7*/  IdWall_00_D,
      /*8*/  IdEsq_00_D,
      /*9*/  xSub,
      /*10*/ Sub_Long_180,
      /*11*/ IdWall_180,
      /*12*/ IdWall_90,
      /*13*/ Sub_Long_90,
      /*14*/ IdWall_270,
      /*15*/ Sub_Long_270,
      /*16*/ IdUndoRedo,
      /*17*/ false
    );
    var WallTopXPosition = obWall.position.x;
    var WallTopYPosition = obWall.position.z;
    var WallopLong = obWall.scale.z - xSub;
    var WallTopWidth = obWall.scale.x;
    var WallTopHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo; if (IsTemporal !== true) { idUndoRedoTemp = obWall.IdUndoRedo; }
    AddWall_R900(/*1*/   WallTopXPosition,/*2*/   WallTopYPosition,/*3*/   WallopLong,/*4*/   WallTopWidth,/*5*/   WallTopHeigh,
      /*6*/   "Wall_R900",
      /*7*/   IdWall_90_D,
      /*8*/   IdWall_0,
      /*9*/   Sub_Long_0,
      /*10*/  Sub_Long_180,
      /*11*/  IdWall_180,
      /*12*/  IdWall_90,
      /*13*/  Sub_Long_90,
      /*14*/  IdEsq_90_D,
      /*15*/  xSub,
      /*16*/  idUndoRedoTemp,
      /*17*/  obWall,
    );
    scene.remove(obWall);
    //WallTop_
    InsertWall = 102;
};
function AddCorner60(Value, IsTemporal) {
    IdUndoRedo = IdUndoRedo + 1;
    Value = parseFloat(Value);
    meshEsq60.visible = false;
    var IdpartName = new Date().valueOf();
    var IdEsq_00_D = "Esq_60_00" + IdpartName;
    var IdEsq_90_D = "Esq_60_90" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_180_D = "Wall_R000_180" + IdpartName;
    var IdWall_00_D = "Wall_R000_0" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y + 0.03);
    //WallEsqTLe_
    var WallXPosition = obWall.position.x;
    var WallLong = Value - 0.03;
    var WallYPosition = obWall.position.z;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
    /*1 */  WallXPosition,
    /*2 */  WallYPosition,
    /*3 */  WallLong * 10,
    /*4 */  WallWidth * 10,
    /*5 */  Wallheigh * 10,
    /*6 */  "Wall_R000",
    /*7 */  IdWall_180_D,
    /*8 */  IdEsq_00_D,
    /*9 */  xSub,
    /*10 */ obWall.Sub_Long_180,
    ///*11 */ obWall.IdWall_180,
    /*12 */ IdWall_90,
    /*13 */ Sub_Long_90,
    /*14 */ IdWall_270,
    /*15 */ Sub_Long_270,
    /*16 */ IdUndoRedo,
    /*17 */ false,
    /*18*/  obWall
    );
    var WallXEsq = WallXPosition + WallLong * 1000;
    var WallXLong = xSub;
    AddWall_R000(
     /*1*/ WallXEsq,
     /*1*/ WallYPosition,
     /*1*/ WallXLong * 10,
     /*1*/ WallWidth * 10,
     /*1*/ Wallheigh * 10,
     /*1*/ "Esq_60_00",
     /*1*/ IdEsq_00_D,
     /*1*/ IdWall_00_D,
     /*1*/ Sub_Long_0,
     /*1*/ Sub_Long_180,
     /*1*/ IdWall_180_D,
     /*1*/ IdWall_90,
     /*1*/ Sub_Long_90,
     /*1*/ IdWall_270,
     /*1*/ Sub_Long_270,
     /*1*/ IdUndoRedo,
     /*1*/ false
    );
    var Wall2XPosition = WallXEsq + (xSub * 1000);
    var res = (xSub + Value) - 0.03;
    var Wall2Long = obWall.scale.x - res;
    var Wall2YPosition = obWall.position.z;
    var Wall2Width = obWall.scale.y;
    var Wall2heigh = obWall.scale.z;
    AddWall_R000(
    /*1*/  Wall2XPosition,
    /*2*/  Wall2YPosition,
    /*3*/  Wall2Long * 10,
    /*4*/  Wall2Width * 10,
    /*5*/  Wall2heigh * 10,
    /*6*/  "Wall_R000",
    /*7*/  IdWall_00_D,
    /*8*/  obWall.IdWall_0,
    /*9*/  obWall.Sub_Long_0,
    /*10*/ xSub,
    /*11*/ IdEsq_00_D,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo,
    /*17*/ false
    );
    var restx = xSub / 2;
    restx = restx + obWall.scale.y / 2;
    var Wall90XPosition = WallXEsq + (restx * 1000);
    var Wall90YPosition = obWall.position.z - ((obWall.scale.y * 1000) + 30);
    var Wall90Long = 0.03 + obWall.scale.y;
    var Wall90Width = obWall.scale.y;
    var Wall90Heigh = obWall.scale.z;
    AddWall_R900(
    /*1*/  Wall90XPosition,
    /*2*/  Wall90YPosition,
    /*3*/  Wall90Long,
    /*4*/  Wall90Width,
    /*5*/  Wall90Heigh,
    /*6*/  "Esq_60_90",
    /*7*/  IdEsq_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_90_D,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    var Wall90_2_Long = 0.15;
    var Wall_2_90YPosition = Wall90YPosition - 150;
    AddWall_R900(
     /*1*/  Wall90XPosition,
     /*2*/  Wall_2_90YPosition,
     /*3*/  Wall90_2_Long,
     /*4*/  Wall90Width,
     /*5*/  Wall90Heigh,
     /*6*/  "Wall_R900",
     /*7*/  IdWall_90_D,
     /*8*/  IdWall_0,
     /*9*/  Sub_Long_0,
     /*10*/ Sub_Long_180,
     /*11*/ IdWall_180,
     /*12*/ IdWall_90,
     /*13*/ Sub_Long_90,
     /*14*/ IdEsq_90_D,
     /*15*/ xSub,
     /*16*/ IdUndoRedo
    );
    scene.remove(obWall);
    InsertWall = 102;
};
function AddCorner80(Value, IsTemporal) {
    Value = parseFloat(Value);
    meshEsq80.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_90_270_D = "Wall_R900_270" + IdpartName;
    var IdEsq_00_D = "Esq_80_00" + IdpartName;
    var IdEsq_90_D = "Esq_80_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.x);
    IdUndoRedo = IdUndoRedo + 1;
    var EsqLefXPosition = obWall.position.x - (obWall.scale.x * 1000);
    var EsqLefYPosition = (obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000);
    var EsqLefWidth = obWall.scale.x * 10;
    var EsqLefHeigh = obWall.scale.y * 10;
    var EsqLefLong = xSub * 10;
    AddWall_R000(
        /* 1*/    EsqLefXPosition,
        /* 2*/    EsqLefYPosition,
        /* 3*/    EsqLefLong,
        /* 4*/    EsqLefWidth,
        /* 5*/    EsqLefHeigh,
        /* 6*/   "Esq_80_00",
        /* 7*/    IdEsq_00_D,
        /* 8*/    IdWall_00_D,
        /* 9*/    Sub_Long_0,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdWall_270,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );
    //WallEsqTAbajo_
    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000)) - (((xSub * 1000) + (obWall.scale.x * 1000)) / 2);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;

    AddWall_R900(
 /*1*/   EsqTopXPosition,
 /*2*/   EsqTopYPosition,
 /*3*/   EsqTopLong,
 /*4*/   EsqTopWidth,
 /*5*/   EsqTopHeigh,
 /*6*/   "Esq_80_90",
 /*7*/   IdEsq_90_D,
 /*8*/   IdWall_0,
 /*9*/   Sub_Long_0,
 /*10*/  Sub_Long_180,
 /*11*/  IdWall_180,
 /*12*/  IdWall_90,
 /*13*/  Sub_Long_90,
 /*14*/  IdWall_90_D,
 /*15*/  Sub_Long_270,
 /*16*/  IdUndoRedo,
        /*17*/
    );
    xSub = GetXsub(obWall.scale.x);
    var WallTopXPosition = obWall.position.x + 30;
    var WallTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000));
    var WallopLong = 1.5;
    var WallTopWidth = obWall.scale.x * 10;
    var WallTopHeigh = obWall.scale.y * 10;
    AddWall_R000(
/*1*/  WallTopXPosition,
/*2*/  WallTopYPosition,
/*3*/  WallopLong,
/*4*/  WallTopWidth,
/*5*/  WallTopHeigh,
/*6*/  "Wall_R000",
/*7*/  IdWall_00_D,
/*8*/  IdWall_0,
/*9*/  Sub_Long_0,
/*10*/ xSub,
/*11*/ IdEsq_00_D,
/*12*/ IdWall_90,
/*13*/ Sub_Long_90,
/*14*/ IdWall_270,
/*15*/ Sub_Long_270,
/*16*/ IdUndoRedo,
/*17*/ false
    );
    //Old WallTop_
    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqDownXPosition = obWall.position.x;
    var EsqDownLong = parseFloat(Value - 0.030);
    var EsqDownYPosition = ((obWall.position.z + (obWall.scale.z * 1000))) - (EsqDownLong * 1000);

    var EsqDownWidth = obWall.scale.x;
    var EsqDownHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition,
/*3*/   EsqDownLong,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_270_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  IdEsq_90_D,
/*13*/  xSub,
/*14*/  obWall.IdWall_270,
/*15*/  obWall.Sub_Long_270,
/*16*/  idUndoRedoTemp,
        /*17*/
    );
    var valueLong = obWall.scale.z;
    var EsqDownLong90 = parseFloat(valueLong - (Value + 0.03 + EsqDownWidth));
    var EsqDownYPosition90 = obWall.position.z /*+ (EsqDownLong90 * 1000)*/;
    idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition90,
/*3*/   EsqDownLong90,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  "0",
/*13*/  "0",
/*14*/  IdEsq_90_D,
/*15*/  xSub,
/*16*/  idUndoRedoTemp,
        /*17*/
    );


    scene.remove(obWall);
    InsertWall = 102;
};
function AddCorner40(Value, IsTemporal) {
    Value = parseFloat(Value);
    meshEsq40.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_90_270_D = "Wall_R900_270" + IdpartName;
    var IdEsq_00_D = "Esq_40_00" + IdpartName;
    var IdEsq_90_D = "Esq_40_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.x);
    IdUndoRedo = IdUndoRedo + 1;
    var EsqLefLong = xSub * 10;
    var EsqLefXPosition = obWall.position.x - (EsqLefLong * 100);
    var EsqLefYPosition = (obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000);
    var EsqLefWidth = obWall.scale.x * 10;
    var EsqLefHeigh = obWall.scale.y * 10;
    //Esq_40_00
    AddWall_R000(/* 1*/    EsqLefXPosition,/* 2*/    EsqLefYPosition,/* 3*/    EsqLefLong,/* 4*/    EsqLefWidth,/* 5*/    EsqLefHeigh,/* 6*/   "Esq_40_00",
        /* 7*/    IdEsq_00_D,
        /* 8*/    IdWall_00_D,
        /* 9*/    Sub_Long_0,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdWall_270,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );

    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000)) - (((xSub * 1000) + (obWall.scale.x * 1000)) / 2);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;
    //WallEsqTAbajo_
    AddWall_R900(/*1*/   EsqTopXPosition,/*2*/   EsqTopYPosition,/*3*/   EsqTopLong,/*4*/   EsqTopWidth,/*5*/   EsqTopHeigh,
 /*6*/   "Esq_40_90",
 /*7*/   IdEsq_90_D,
 /*8*/   IdWall_0,
 /*9*/   Sub_Long_0,
 /*10*/  Sub_Long_180,
 /*11*/  IdWall_180,
 /*12*/  IdWall_90,
 /*13*/  Sub_Long_90,
 /*14*/  IdWall_90_D,
 /*15*/  Sub_Long_270,
 /*16*/  IdUndoRedo,
        /*17*/
    );
    xSub = GetXsub(obWall.scale.x);
    var WallopLong = 1.5;
    var WallTopXPosition = obWall.position.x - ((EsqLefLong + WallopLong) * 100);
    var WallTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000));

    var WallTopWidth = obWall.scale.x * 10;
    var WallTopHeigh = obWall.scale.y * 10;
    //Wall_R000
    AddWall_R000(/*1*/  WallTopXPosition,/*2*/  WallTopYPosition,/*3*/  WallopLong,/*4*/  WallTopWidth,/*5*/  WallTopHeigh,
    /*6*/  "Wall_R000",
    /*7*/  IdWall_00_D,
    /*8*/  IdEsq_90_D,
    /*9*/  Sub_Long_0,
    /*10*/ xSub,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo,
    /*17*/ false
    );
    //Old WallTop_
    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqDownXPosition = obWall.position.x;
    var EsqDownLong = parseFloat(Value - 0.030);
    var EsqDownYPosition = ((obWall.position.z + (obWall.scale.z * 1000))) - (EsqDownLong * 1000);

    var EsqDownWidth = obWall.scale.x;
    var EsqDownHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition,
/*3*/   EsqDownLong,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_270_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  IdEsq_90_D,
/*13*/  xSub,
/*14*/  obWall.IdWall_270,
/*15*/  obWall.Sub_Long_270,
/*16*/  idUndoRedoTemp,
        /*17*/
    );
    var valueLong = obWall.scale.z;
    var EsqDownLong90 = parseFloat(valueLong - (Value + 0.03 + EsqDownWidth));
    var EsqDownYPosition90 = obWall.position.z /*+ (EsqDownLong90 * 1000)*/;
    idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition90,
/*3*/   EsqDownLong90,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  "0",
/*13*/  "0",
/*14*/  IdEsq_90_D,
/*15*/  xSub,
/*16*/  idUndoRedoTemp,
        /*17*/
    );
    scene.remove(obWall);
    InsertWall = 102;
};
function CreateConection90x0(ob, obWall, ValueNewWall, IsMenuMenu) {
    var positionWallConexion = getEsqPositionY(obWall.IdWall_270);
    if (IsMenuMenu === false) {
        var ValueNewWall = ((positionWallConexion.z - ob.position.z) / 1000) - 0.03;
    }
    else {
        ValueNewWall = ValueNewWall / 1000;
    }
    var xob = ob.position.x;
    var xobwall = obWall.position.x;
    var value = (xob - xobwall) / 1000;
    if (value < 0) { value = value * -1 }

    scene.remove(obWall);
    obWall = null;
    value = value - ob.scale.y;
    AddCorner20(value, undefined, ob, ValueNewWall);
};
function AddCorner20(Value, IsTemporal, wallD, ValueNewWall) {
    if (wallD !== undefined) {
        obWall = wallD;
    }
    var IdpartName = new Date().valueOf();
    var IdEsq_00_D = "Esq_20_00" + IdpartName;
    var IdEsq_90_D = "Esq_20_90" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_180_D = "Wall_R000_180" + IdpartName;
    var IdWall_00_D = "Wall_R000_0" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "Esq_20_00" + IdpartName;
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y + 0.03);
    IdUndoRedo = IdUndoRedo + 1;
    Value = parseFloat(Value);
    meshEsq60.visible = false;
    //WallEsqTLe_ oldWall
    var WallXPosition = obWall.position.x;
    var WallLong = Value - 0.03;
    var WallYPosition = obWall.position.z;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
     /*1*/   WallXPosition,
     /*2 */  WallYPosition,
     /*3 */  WallLong * 10,
     /*4 */  WallWidth * 10,
     /*5 */  Wallheigh * 10,
     /*6 */  "Wall_R000_180",
     /*7 */  IdWall_180_D,
     /*8 */  IdEsq_00_D,
     /*9 */  /*Sub_Long_0,*/ xSub,
     /*10 */ obWall.Sub_Long_180,
     /*11 */ obWall.IdWall_180,
     /*12 */ IdWall_90,
     /*13 */ Sub_Long_90,
     /*14 */ obWall.IdWall_270,
     /*15 */ Sub_Long_270,
     /*16 */ IdUndoRedo,
     /*17*/  false,
     /*18*/  obWall,
    );
    var WallXEsq = WallXPosition + WallLong * 1000;
    var WallXLong = xSub;
    /* Wall Right*/
    //getIdEsq30(obWall);
    AddWall_R000(
/* 1*/  WallXEsq,
/* 2*/  WallYPosition,
/* 3*/  WallXLong * 10,
/* 4*/  WallWidth * 10,
/* 5*/  Wallheigh * 10,
/* 6*/  "Esq_20_00",
/* 7*/  IdEsq_00_D,
/* 8*/  IdWall_00_D,
/* 9*/  Sub_Long_0,
/* 10*/ Sub_Long_180,
/* 11*/  IdWall_180_D,
/* 12*/  IdWall_90,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_00_D,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/  false
    );
    var Wall2XPosition = WallXEsq + (xSub * 1000);
    var res = (xSub + Value) - 0.03;
    var Wall2Long = obWall.scale.x - res;
    var Wall2YPosition = obWall.position.z;
    var Wall2Width = obWall.scale.y;
    var Wall2heigh = obWall.scale.z;
    AddWall_R000(
/* 1*/   Wall2XPosition,
/* 2*/   Wall2YPosition,
/* 3*/   Wall2Long * 10,
/* 4*/   Wall2Width * 10,
/* 5*/   Wall2heigh * 10,
/* 6*/   "Wall_R000_0",
/* 7*/   IdWall_00_D,
/* 8*/   obWall.IdWall_0,
/* 9*/   obWall.Sub_Long_0,
/* 10*/  /*Sub_Long_180*/ xSub,
/* 11*/  IdEsq_00_D,
/* 12*/  obWall.IdWall_90,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_270,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/   false
    );
    var restx = xSub / 2;
    restx = restx + obWall.scale.y / 2;
    var Wall90XPosition = WallXEsq + (restx * 1000);
    var Wall90YPosition = obWall.position.z - (obWall.scale.y * 1000);
    var Wall90Long = 0.03 + obWall.scale.y;
    var Wall90Width = obWall.scale.y;
    var Wall90Heigh = obWall.scale.z;
    AddWall_R900(
    /*1*/  Wall90XPosition,
    /*2*/  Wall90YPosition,
    /*3*/  Wall90Long,
    /*4*/  Wall90Width,
    /*5*/  Wall90Heigh,
    /*6*/  "Esq_20_90",
    /*7*/  IdEsq_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ '0',
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_90_D,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    var Wall90_2_Long = 0.15;
    if (ValueNewWall !== undefined) {
        Wall90_2_Long = ValueNewWall;
    }
    var Wall_2_90YPosition = obWall.position.z + 30;
    AddWall_R900(
    /*1*/ Wall90XPosition,
    /*2*/ Wall_2_90YPosition,
    /*3*/ Wall90_2_Long,
    /*4*/ Wall90Width,
    /*5*/ Wall90Heigh,
    /*6*/ "Wall_R900",
    /*7*/ IdWall_90_D,
    /*8*/ IdWall_0,
    /*9*/ Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdEsq_90_D,
    /*13*/ Wall90Long,
    /*14*/ IdWall_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    scene.remove(obWall);
    ResetSetup();
    InsertWall = 102;
};
function AddCornerX(Value, IsTemporal, wallD, ValueNewWall) {
    Value = parseFloat(Value);
    meshEsqX.visible = false;
    var IdpartName = new Date().valueOf();
    var IdWall_00_D = "Wall_R000" + IdpartName;
    var IdWall_180_D = "Wall_R000_180" + IdpartName;
    var IdWall_90_D = "Wall_R900" + IdpartName;
    var IdWall_90_270_D = "Wall_R900_270" + IdpartName;
    var IdEsq_00_D = "Esq_X_00" + IdpartName;
    var IdEsq_90_D = "Esq_X_90" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.x + 0.03);
    IdUndoRedo = IdUndoRedo + 1;
    var EsqLefXPosition = (obWall.position.x - ((obWall.scale.x * 1000))) - 30;
    var EsqLefYPosition = (obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000);
    var EsqLefWidth = obWall.scale.x * 10;
    var EsqLefHeigh = obWall.scale.y * 10;
    var EsqLefLong = (xSub * 10);
    AddWall_R000(
        /* 1*/    EsqLefXPosition,
        /* 2*/    EsqLefYPosition,
        /* 3*/    EsqLefLong,
        /* 4*/    EsqLefWidth,
        /* 5*/    EsqLefHeigh,
        /* 6*/   "Esq_X_00",
        /* 7*/    IdEsq_00_D,
        /* 8*/    IdWall_00_D,
        /* 9*/    EsqLefLong,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180_D,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdWall_270,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );
    var WallopLong_180 = 1.5;
    var EsqLefXPosition_180 = EsqLefXPosition - 150;
    AddWall_R000(
        /* 1*/    EsqLefXPosition_180,
        /* 2*/    EsqLefYPosition,
        /* 3*/    WallopLong_180,
        /* 4*/    EsqLefWidth,
        /* 5*/    EsqLefHeigh,
        /* 6*/   "Wall_R000",
        /* 7*/    IdWall_180_D,
        /* 8*/    IdEsq_00_D,
        /* 9*/    EsqLefLong,
        /* 10*/   Sub_Long_180,
        /* 11*/   IdWall_180,
        /* 12*/   IdWall_90,
        /* 13*/   Sub_Long_90,
        /* 14*/   IdWall_270,
        /* 15*/   Sub_Long_270,
        /* 16*/   IdUndoRedo,
        /* 17*/   false
    );

    //WallEsqTAbajo_
    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqTopXPosition = obWall.position.x;
    var EsqTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000)) - (((xSub * 1000) + (obWall.scale.x * 1000)) / 2);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.x;
    var EsqTopHeigh = obWall.scale.y;

    AddWall_R900(
 /*1*/   EsqTopXPosition,
 /*2*/   EsqTopYPosition,
 /*3*/   EsqTopLong,
 /*4*/   EsqTopWidth,
 /*5*/   EsqTopHeigh,
 /*6*/   "Esq_X_90",
 /*7*/   IdEsq_90_D,
 /*8*/   IdWall_0,
 /*9*/   Sub_Long_0,
 /*10*/  Sub_Long_180,
 /*11*/  IdWall_180,
 /*12*/  IdWall_90,
 /*13*/  Sub_Long_90,
 /*14*/  IdWall_90_D,
 /*15*/  Sub_Long_270,
 /*16*/  IdUndoRedo,
        /*17*/
    );
    xSub = GetXsub(obWall.scale.x);
    var WallTopXPosition = obWall.position.x + 30;
    var WallTopYPosition = ((obWall.position.z + (obWall.scale.z * 1000)) - (Value * 1000));
    var WallopLong = 1.5;
    var WallTopWidth = obWall.scale.x * 10;
    var WallTopHeigh = obWall.scale.y * 10;
    AddWall_R000(
/*1*/  WallTopXPosition,
/*2*/  WallTopYPosition,
/*3*/  WallopLong,
/*4*/  WallTopWidth,
/*5*/  WallTopHeigh,
/*6*/  "Wall_R000",
/*7*/  IdWall_00_D,
/*8*/  IdWall_0,
/*9*/  Sub_Long_0,
/*10*/ xSub,
/*11*/ IdEsq_00_D,
/*12*/ IdWall_90,
/*13*/ Sub_Long_90,
/*14*/ IdWall_270,
/*15*/ Sub_Long_270,
/*16*/ IdUndoRedo,
/*17*/ false
    );
    //Old WallTop_
    xSub = GetXsub(obWall.scale.x + 0.03);
    var EsqDownXPosition = obWall.position.x;
    var EsqDownLong = parseFloat(Value - 0.030);
    var EsqDownYPosition = ((obWall.position.z + (obWall.scale.z * 1000))) - (EsqDownLong * 1000);

    var EsqDownWidth = obWall.scale.x;
    var EsqDownHeigh = obWall.scale.y;
    var idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition,
/*3*/   EsqDownLong,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_270_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  IdEsq_90_D,
/*13*/  xSub,
/*14*/  obWall.IdWall_270,
/*15*/  obWall.Sub_Long_270,
/*16*/  idUndoRedoTemp,
        /*17*/
    );
    var valueLong = obWall.scale.z;
    var EsqDownLong90 = parseFloat(valueLong - (Value + 0.03 + EsqDownWidth));
    var EsqDownYPosition90 = obWall.position.z /*+ (EsqDownLong90 * 1000)*/;
    idUndoRedoTemp = IdUndoRedo;
    if (IsTemporal !== true) {
        idUndoRedoTemp = obWall.IdUndoRedo;
    }
    AddWall_R900(
/*1*/   EsqDownXPosition,
/*2*/   EsqDownYPosition90,
/*3*/   EsqDownLong90,
/*4*/   EsqDownWidth,
/*5*/   EsqDownHeigh,
/*6*/   "Wall_R900",
/*7*/   IdWall_90_D,
/*8*/   IdWall_0,
/*9*/   Sub_Long_0,
/*10*/  Sub_Long_180,
/*11*/  IdWall_180,
/*12*/  "0",
/*13*/  "0",
/*14*/  IdEsq_90_D,
/*15*/  xSub,
/*16*/  idUndoRedoTemp,
        /*17*/
    );
    scene.remove(obWall);
    InsertWall = 102;
};
function CreateConection90x0X(ob, obWall, ValueNewWall) {
    var positionWallConexion = getEsqPositionY(ob.IdWall_270);
    var ValueNewWall = ((positionWallConexion.z - ob.position.z) / 1000) - 0.03;
    var xob = ob.position.x;
    var xobwall = obWall.position.x;
    var value = (xob - xobwall) / 1000;
    if (value < 0) { value = value * -1 }
    scene.remove(obWall);
    obWall = null;
    value = value - ob.scale.y;
    AddCornerX_0(value, undefined, obWall, ValueNewWall);
};
function AddCornerX_0(Value, IsTemporal, wallD, ValueNewWall) {
    if (wallD !== undefined) {
        obWall = wallD;
    }
    var IdpartName = new Date().valueOf();
    var IdEsq_00_D = "Esq_X_00" + IdpartName;
    var IdEsq_90_D = "Esq_X_90" + IdpartName;
    var IdWall_90_D_270 = "Wall_R900_270" + IdpartName;
    var IdWall_90_D_90 = "Wall_R900_90" + IdpartName;
    var IdWall_180_D = "Wall_R000_180" + IdpartName;
    var IdWall_00_D = "Wall_R000_0" + IdpartName;
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y + 0.03);
    IdUndoRedo = IdUndoRedo + 1;
    Value = parseFloat(Value);
    meshEsqX.visible = false;
    //WallEsqTLe_ oldWall
    var WallXPosition = obWall.position.x;
    var WallLong = Value - 0.03;
    var WallYPosition = obWall.position.z;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    WallLong = parseFloat(WallLong.toFixed(3));
    WallWidth = parseFloat(WallWidth.toFixed(3));
    Wallheigh = parseFloat(Wallheigh.toFixed(3));
    AddWall_R000(
     /*1*/   WallXPosition,
     /*2 */  WallYPosition,
     /*3 */  WallLong * 10,
     /*4 */  WallWidth * 10,
     /*5 */  Wallheigh * 10,
     /*6 */  "Wall_R000",
     /*7 */  IdWall_180_D,
     /*8 */  IdEsq_00_D,
     /*9 */  /*Sub_Long_0,*/ xSub,
     /*10 */ obWall.Sub_Long_180,
     /*11 */ obWall.IdWall_180,
     /*12 */ IdWall_90,
     /*13 */ Sub_Long_90,
     /*14 */ IdWall_270,
     /*15 */ Sub_Long_270,
     /*16 */ IdUndoRedo,
     /*17*/  false,
     /*18*/  obWall,
    );
    var WallXEsq = WallXPosition + WallLong * 1000;
    var WallXLong = xSub;
    /* Wall Right*/
    //getIdEsq30(obWall);
    AddWall_R000(
/* 1*/  WallXEsq,
/* 2*/  WallYPosition,
/* 3*/  WallXLong * 10,
/* 4*/  WallWidth * 10,
/* 5*/  Wallheigh * 10,
/* 6*/  "Esq_X_00",
/* 7*/  IdEsq_00_D,
/* 8*/  IdWall_00_D,
/* 9*/  Sub_Long_0,
/* 10*/ Sub_Long_180,
/* 11*/  IdWall_180_D,
/* 12*/  IdWall_90,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_00_D,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/  false
    );
    var Wall2XPosition = WallXEsq + (xSub * 1000);
    var res = (xSub + Value) - 0.03;
    var Wall2Long = obWall.scale.x - res;
    var Wall2YPosition = obWall.position.z;
    var Wall2Width = obWall.scale.y;
    var Wall2heigh = obWall.scale.z;
    AddWall_R000(
/* 1*/   Wall2XPosition,
/* 2*/   Wall2YPosition,
/* 3*/   Wall2Long * 10,
/* 4*/   Wall2Width * 10,
/* 5*/   Wall2heigh * 10,
/* 6*/   "Wall_R000",
/* 7*/   IdWall_00_D,
/* 8*/   obWall.IdWall_0,
/* 9*/   obWall.Sub_Long_0,
/* 10*/  /*Sub_Long_180*/ xSub,
/* 11*/  IdWall_180,
/* 12*/  IdEsq_00_D,
/* 13*/  Sub_Long_90,
/* 14*/  IdWall_270,
/* 15*/  Sub_Long_270,
/* 16*/  IdUndoRedo,
/* 17*/   false
    );
    var restx2 = xSub / 2;
    restx2 = restx2 + (obWall.scale.y / 2);
    var Wall90XPosition = WallXEsq + (restx2 * 1000);
    var Wall90YPosition = (obWall.position.z - (obWall.scale.y * 1000) - 30);
    var Wall90Long = 0.06 + obWall.scale.y;
    var Wall90Width = obWall.scale.y;
    var Wall90Heigh = obWall.scale.z;
    AddWall_R900(
    /*1*/  Wall90XPosition,
    /*2*/  Wall90YPosition,
    /*3*/  Wall90Long,
    /*4*/  Wall90Width,
    /*5*/  Wall90Heigh,
    /*6*/  "Esq_X_90",
    /*7*/  IdEsq_90_D,
    /*8*/  IdWall_0,
    /*9*/  Sub_Long_0,
    /*10*/ Sub_Long_180,
    /*11*/ IdWall_180,
    /*12*/ IdWall_90,
    /*13*/ Sub_Long_90,
    /*14*/ IdWall_90_D_270,
    /*15*/ Sub_Long_270,
    /*16*/ IdUndoRedo
    );
    var Wall270_2_Long = 0.15;
    if (ValueNewWall !== undefined) {
        Wall270_2_Long = ValueNewWall;
    }
    var Wall_2_270YPosition = obWall.position.z + 30;

    AddWall_R900(
    /*1*/ Wall90XPosition,
    /*1*/ Wall_2_270YPosition,
    /*1*/ Wall270_2_Long,
    /*1*/ Wall90Width,
    /*1*/ Wall90Heigh,
    /*1*/ "Wall_R900",
    /*1*/ IdWall_90_D_270,
    /*1*/ IdWall_0,
    /*1*/ Sub_Long_0,
    /*1*/ Sub_Long_180,
    /*1*/ IdWall_180,
    /*1*/ IdEsq_90_D,
    /*1*/ Wall90Long,
    /*1*/ IdWall_270,
    /*1*/ Sub_Long_270,
    /*1*/ IdUndoRedo
    );

    var Wall90_2_Long = 0.15;
    var Wall_2_90YPosition = obWall.position.z - (180 + (obWall.scale.y * 1000));
    AddWall_R900(
    /*1*/ Wall90XPosition,
    /*1*/ Wall_2_90YPosition,
    /*1*/ Wall90_2_Long,
    /*1*/ Wall90Width,
    /*1*/ Wall90Heigh,
    /*1*/ "Wall_R900",
    /*1*/ IdWall_90_D_90,
    /*1*/ obWall.IdWall_0,
    /*1*/ obWall.Sub_Long_0,
    /*1*/ Sub_Long_180,
    /*1*/ IdWall_180,
    /*1*/ obWall.IdWall_90,
    /*1*/ obWall.Sub_Long_90,
    /*1*/ IdEsq_90_D,
    /*1*/ xSub,
    /*1*/ IdUndoRedo
    );
    scene.remove(obWall);
    ResetSetup();
    InsertWall = 102;
};
function AddCornerParall(Value, IsTemporal) {
    Value = parseFloat(Value * 10);

    meshWall_0.visible = false;
    InsertWall = 0;
    var PositionY = meshParall.position.z;
    if (IsTemporal === true) {
        var PositionY = ObParalle.position.z - (Value * 100);
    }

    let longWall = ObParalle.scale.x * 10;
    var widthWall = ObParalle.scale.y * 10;
    var heightWall = ObParalle.scale.z * 10;
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
    AddWall_R000(meshParall.position.x, PositionY, longWall, widthWall, heightWall, "Wall_R000", IdWall,
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
    ResetSetup();
    InsertWall = 102;
};
function AddCornerParall_90(Value, IsTemporal) {
    Value = parseFloat(Value * 10);

    meshWall_90.visible = false;
    InsertWall = 0;
    var PositionY = meshParall.position.y - ObParalle.scale.z * 1000;
    var PositionX = meshParall.position.x + (Value * 100);
    if (IsTemporal === true) {
        var PositionX = ObParalle.position.x + (Value * 100);
    }
    var WallTopXPosition = PositionX;
    var WallTopYPosition = PositionY;
    let longWall = ObParalle.scale.z;
    var widthWall = ObParalle.scale.x;
    var heightWall = ObParalle.scale.y;
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
    AddWall_R900(
        WallTopXPosition,
        WallTopYPosition,
        longWall,
        widthWall,
        heightWall,
        "Wall_R900",
        IdWall,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_90,
        IdWall_270,
        Sub_Long_270,
        IdUndoRedo
    );
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ActionWizard = 0;
    ResetSetup();
    InsertWall = 102;
};

