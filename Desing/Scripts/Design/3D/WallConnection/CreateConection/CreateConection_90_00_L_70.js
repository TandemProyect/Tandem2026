function CreateConection_90_00_L_70(FirstWall, SecondWall) {
    Wall_Conexion_1.material = materialWall;
    Wall_Conexion_2.material = materialWall;
    const widthWall = 30;
    // Calculate the necessary Z distance between walls
    let referentPoint = Math.abs(FirstWall.position.z - SecondWall.position.z) - widthWall;

    // Align SecondWall to the corner in x-axis based on FirstWall's scale and position
    let e = FirstWall.scale.x * 1000; // Scale factor of FirstWall
    SecondWall.position.x = FirstWall.position.x - e;

    // Adjust the scale and position of SecondWall in x-axis
    let diffScaleX = FirstWall.position.x - SecondWall.position.x;
    SecondWall.scale.x -= (diffScaleX / 1000);

    obWall = SecondWall
    AddCorner_90_00_L_70(FirstWall, SecondWall);
    // Cleanup

    ResetSetup();
}
function AddCorner_90_00_L_70(FirstWall, SecondWall) {
    //Help Develop
    DrawPointHelp(FirstWall.position.x, 0, FirstWall.position.z);
    // Id new Element
    var t = "X = " + FirstWall.position.x;
    AddTexHelp(t, FirstWall.position.x - 20, FirstWall.position.z + 20);
    t = "Y = " + FirstWall.position.z;
    AddTexHelp(t, FirstWall.position.x + 20, FirstWall.position.z - 20);
    //End Help develop

    var IdpartName = new Date().valueOf();
    //conexiones Muro 0
    var IdWallMuro_00 = SecondWall.IdWall;
    var IdWallMuro_00Conexion0 = SecondWall.IdWall_0;
    var IdWallMuro_00Conexion90 = SecondWall.IdWall_90;
    var IdWallMuro_00Conexion90180 = "Esq_70_00" + IdpartName;
    var IdWallMuro_00Conexion90270 = SecondWall.IdWall_270;

    //conexiones Muro 90
    var IdWallMuro_90 = FirstWall.idWall;
    var IdWallMuro_90Conexion0 = FirstWall.IdWall_0;
    var IdWallMuro_90Conexion90 = FirstWall.IdWall_90;
    var IdWallMuro_90Conexion90180 = FirstWall.IdWall_180;
    var IdWallMuro_90Conexion90270 = "Esq_70_90" + IdpartName;

    //conexiones Esquina70 00
    var IdWallEsq_00 = "Esq_70_00" + IdpartName;
    var IdWallEsq_00Conexion0 = IdWallMuro_00;
    var IdWallEsq_00Conexion90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_00Conexion90180 = "0";
    var IdWallEsq_00Conexion90270 = "0";

    //conexiones Esquina 90
    var IdWallEsq_90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_90Conexion0 = IdWallMuro_00;
    var IdWallEsq_90Conexion90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_90Conexion90180 = "0";
    var IdWallEsq_90Conexion90270 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq70.visible = false;
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = obWall.position.x;
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
    // Esq_70_00
    AddWall_R000(
      /*1*/  EsqLefXPosition,
      /*2*/  EsqLefYPosition,
      /*3*/  EsqLefLong * 10,
      /*4*/  EsqLefWidth * 10,
      /*5*/  EsqLefHeigh * 10,
      /*6*/  "Esq_70_00",
      /*7*/  IdWallEsq_00,
      /*8*/  IdWallEsq_00Conexion0,
      /*9*/   0,
      /*10*/  0,
      /*11*/  IdWallEsq_00Conexion90180,
      /*12*/  IdWallEsq_00Conexion90,
      /*13*/  0,
      /*14*/  IdWallEsq_00Conexion90270,
      /*15*/  0,
      /*16*/  IdUndoRedo,
      /*17*/ false,
    );
    var EsqTopXPosition = obWall.position.x + obWall.scale.y * 1000;
    var EsqTopYPosition = obWall.position.z - xSub * 1000;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    //Esq_70_90
    AddWall_R900(
    /*1*/    EsqTopXPosition,
    /*2*/    EsqTopYPosition,
    /*3*/    EsqTopLong,
    /*4*/    EsqTopWidth,
    /*5*/    EsqTopHeigh,
    /*6*/   "Esq_70_90",
    /*7*/    IdWallEsq_90,
    /*8*/    IdWallEsq_90Conexion0,
    /*9*/    0,
    /*10*/   0,
    /*11*/   IdWallEsq_90Conexion90180,
    /*12*/   IdWallEsq_90Conexion90,
    /*13 */  0,
    /*14*/   IdWallEsq_90Conexion90270,
    /*15*/   0,
    /*16*/   IdUndoRedo,
    );
    //AddWall_R900
    var WallTopXPosition = FirstWall.position.x;
    var WallTopYPosition = FirstWall.position.z;
    var value = FirstWall.position.z;
    var value2 = EsqTopYPosition;
    var WallTopLong = (value - value2) / 1000;
    if (WallTopLong < 0.0001) { WallTopLong = WallTopLong * -1 };
    var WallTopWidth = obWall.scale.y;
    var WallTopHeigh = obWall.scale.z;
    AddWall_R900(
     /*1*/   WallTopXPosition,
     /*2*/   WallTopYPosition,
     /*3*/   WallTopLong,
     /*4*/   WallTopWidth,
     /*5*/   WallTopHeigh,
     /*6*/   "Wall_R900",
     /*7*/   IdWallMuro_90,
     /*8*/   IdWallMuro_90Conexion0,
     /*9*/   0,
     /*10*/  0,
     /*11*/  IdWallMuro_90Conexion90180,
     /*12*/  IdWallMuro_90Conexion90,
     /*13*/  0,
     /*14*/  IdWallMuro_90Conexion90270,
     /*15*/  0,
     /*16*/  IdUndoRedo,
    );
    //AddWall_R000
    var WallXPosition = obWall.position.x + (xSub * 1000);
    var WallYPosition = obWall.position.z;
    var WallLong = obWall.scale.x - xSub;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    AddWall_R000(
    /*1*/   WallXPosition,
    /*2*/   WallYPosition,
    /*3*/   WallLong * 10,
    /*4*/   WallWidth * 10,
    /*5*/   Wallheigh * 10,
    /*6*/   "Wall_R000",
    /*7*/   IdWallMuro_00,
    /*8*/   IdWallMuro_00Conexion0,
    /*9*/   0,
    /*10*/   0,
    /*11*/  IdWallMuro_00Conexion90180,
    /*12*/  IdWallMuro_00Conexion90,
    /*13*/  0,
    /*14*/  IdWallMuro_00Conexion90270,
    /*15*/  0,
    /*16*/  null,
    /*17*/  false,
    /*18*/  null,
    );
    scene.remove(FirstWall);
    InsertWall = 102;
};

function CreateConection_90_00_L_180(FirstWall, SecondWall) {
    Wall_Conexion_1.material = materialWall;
    Wall_Conexion_2.material = materialWall;
    const widthWall = 30;
    const distanceBetweenWalls = FirstWall.position.x - (SecondWall.position.x + SecondWall.scale * 1000)

    // Adjust the scale and position of SecondWall in x-axis
    const secondWallEndPosition = SecondWall.position.x + SecondWall.scale.x * 1000
    let diffScaleX = secondWallEndPosition + widthWall - FirstWall.position.x;
    SecondWall.scale.x -= (diffScaleX / 1000);

    obWall = SecondWall
    AddCorner_90_00_L_180(FirstWall, SecondWall);
    //// Cleanup

    ResetSetup();
}

function AddCorner_90_00_L_180(FirstWall, SecondWall) {
    //Help Develop
    DrawPointHelp(FirstWall.position.x, 0, FirstWall.position.z);
    // Id new Element
    var t = "X = " + FirstWall.position.x;
    AddTexHelp(t, FirstWall.position.x - 20, FirstWall.position.z + 20);
    t = "Y = " + FirstWall.position.z;
    AddTexHelp(t, FirstWall.position.x + 20, FirstWall.position.z - 20);
    //End Help develop

    var IdpartName = new Date().valueOf();
    //conexiones Muro 0
    var IdWallMuro_00 = SecondWall.IdWall;
    var IdWallMuro_00Conexion0 = SecondWall.IdWall_0;
    var IdWallMuro_00Conexion90 = SecondWall.IdWall_90;
    var IdWallMuro_00Conexion90180 = "Esq_70_00" + IdpartName;
    var IdWallMuro_00Conexion90270 = SecondWall.IdWall_270;

    //conexiones Muro 90
    var IdWallMuro_90 = FirstWall.idWall;
    var IdWallMuro_90Conexion0 = FirstWall.IdWall_0;
    var IdWallMuro_90Conexion90 = FirstWall.IdWall_90;
    var IdWallMuro_90Conexion90180 = FirstWall.IdWall_180;
    var IdWallMuro_90Conexion90270 = "Esq_70_90" + IdpartName;

    //conexiones Esquina70 00
    var IdWallEsq_00 = "Esq_70_00" + IdpartName;
    var IdWallEsq_00Conexion0 = IdWallMuro_00;
    var IdWallEsq_00Conexion90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_00Conexion90180 = "0";
    var IdWallEsq_00Conexion90270 = "0";

    //conexiones Esquina 90
    var IdWallEsq_90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_90Conexion0 = IdWallMuro_00;
    var IdWallEsq_90Conexion90 = "Esq_70_90" + IdpartName;
    var IdWallEsq_90Conexion90180 = "0";
    var IdWallEsq_90Conexion90270 = "0";
    IdUndoRedo = IdUndoRedo + 1;
    meshEsq70.visible = false;
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = (obWall.position.x + obWall.scale.x * 1000 - 30);
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
    // Esq_70_00
    AddWall_R000(
      /*1*/  EsqLefXPosition,
      /*2*/  EsqLefYPosition,
      /*3*/  EsqLefLong * 10,
      /*4*/  EsqLefWidth * 10,
      /*5*/  EsqLefHeigh * 10,
      /*6*/  "Esq_70_00",
      /*7*/  IdWallEsq_00,
      /*8*/  IdWallEsq_00Conexion0,
      /*9*/   0,
      /*10*/  0,
      /*11*/  IdWallEsq_00Conexion90180,
      /*12*/  IdWallEsq_00Conexion90,
      /*13*/  0,
      /*14*/  IdWallEsq_00Conexion90270,
      /*15*/  0,
      /*16*/  IdUndoRedo,
      /*17*/ false,
    );
    var EsqTopXPosition = (obWall.position.x + obWall.scale.x * 1000) + obWall.scale.y * 1000;
    var EsqTopYPosition = (obWall.position.z) - xSub * 1000;
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    //Esq_70_90
    AddWall_R900(
    /*1*/    EsqTopXPosition,
    /*2*/    EsqTopYPosition,
    /*3*/    EsqTopLong,
    /*4*/    EsqTopWidth,
    /*5*/    EsqTopHeigh,
    /*6*/   "Esq_70_90",
    /*7*/    IdWallEsq_90,
    /*8*/    IdWallEsq_90Conexion0,
    /*9*/    0,
    /*10*/   0,
    /*11*/   IdWallEsq_90Conexion90180,
    /*12*/   IdWallEsq_90Conexion90,
    /*13 */  0,
    /*14*/   IdWallEsq_90Conexion90270,
    /*15*/   0,
    /*16*/   IdUndoRedo,
    );
    //AddWall_R900
    var WallTopXPosition = FirstWall.position.x;
    var WallTopYPosition = FirstWall.position.z;
    var value = FirstWall.position.z;
    var value2 = EsqTopYPosition;
    var WallTopLong = (value - value2) / 1000;
    if (WallTopLong < 0.0001) { WallTopLong = WallTopLong * -1 };
    var WallTopWidth = obWall.scale.y;
    var WallTopHeigh = obWall.scale.z;
    AddWall_R900(
     /*1*/   WallTopXPosition,
     /*2*/   WallTopYPosition,
     /*3*/   WallTopLong,
     /*4*/   WallTopWidth,
     /*5*/   WallTopHeigh,
     /*6*/   "Wall_R900",
     /*7*/   IdWallMuro_90,
     /*8*/   IdWallMuro_90Conexion0,
     /*9*/   0,
     /*10*/  0,
     /*11*/  IdWallMuro_90Conexion90180,
     /*12*/  IdWallMuro_90Conexion90,
     /*13*/  0,
     /*14*/  IdWallMuro_90Conexion90270,
     /*15*/  0,
     /*16*/  IdUndoRedo,
    );
    //AddWall_R000
    var WallXPosition = FirstWall.position.x + (xSub * 1000);
    var WallYPosition = obWall.position.z;
    var WallLong = obWall.scale.x - xSub;
    var WallWidth = obWall.scale.y;
    var Wallheigh = obWall.scale.z;
    AddWall_R000(
    /*1*/   WallXPosition,
    /*2*/   WallYPosition,
    /*3*/   WallLong * 10,
    /*4*/   WallWidth * 10,
    /*5*/   Wallheigh * 10,
    /*6*/   "Wall_R000",
    /*7*/   IdWallMuro_00,
    /*8*/   IdWallMuro_00Conexion0,
    /*9*/   0,
    /*10*/   0,
    /*11*/  IdWallMuro_00Conexion90180,
    /*12*/  IdWallMuro_00Conexion90,
    /*13*/  0,
    /*14*/  IdWallMuro_00Conexion90270,
    /*15*/  0,
    /*16*/  null,
    /*17*/  false,
    /*18*/  null,
    );
    scene.remove(FirstWall);
    InsertWall = 102;
}