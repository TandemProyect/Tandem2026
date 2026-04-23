const FactorScale = 1000;
const PositionOffset = 60;

 
function ResolverConnection_0(FirstWall, SecondWall)
{
    var FirstWallDef = FirstWall;
    var SecondWallDef = SecondWall;
    if (FirstWall.position.x > SecondWall.position.x)
    {
        FirstWallDef = SecondWall;
        SecondWallDef = FirstWall;
    }
    //atributos SecondWallDef
    var SecontPositionX = SecondWallDef.position.x + SecondWallDef.scale.x * 1000;
    //DrawPoint(SecontPositionX, 275, SecondWallDef.position.z);
    //DrawPoint(FirstWallDef.position.x, 275, FirstWallDef.position.z);
    var LongFirstWallDef = SecontPositionX - FirstWallDef.position.x;
    FirstWallDef.scale.x = LongFirstWallDef / 1000;
    FirstWallDef.IdWall_0 = SecondWallDef.IdWall_0;
    scene.remove(SecondWallDef);
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ActionWizard = 0;
    ResetSetup();
}
function ResolverConnection_90(FirstWall, SecondWall)
{
    var FirstWallDef = FirstWall;
    var SecondWallDef = SecondWall;
    if (FirstWall.position.z < SecondWall.position.z) {
        FirstWallDef = SecondWall;
        SecondWallDef = FirstWall;
    }

    var SecontPositionY = SecondWallDef.position.z;
    var FirstPositionY = FirstWall.position.z + FirstWall.scale.z * 1000;
    DrawPoint(SecondWallDef.position.x, 275, SecontPositionY);
    DrawPoint(FirstWallDef.position.x, 275, FirstPositionY);

    var LongSecondWallDef = SecontPositionY - FirstPositionY;
    if (LongSecondWallDef < 0) { LongSecondWallDef = LongSecondWallDef  * -1}
    FirstWallDef.scale.x = LongSecondWallDef / 1000;
    FirstWallDef.IdWall_0 = SecondWallDef.IdWall_0;
    scene.remove(FirstWallDef);
    SecondWallDef.scale.z = LongSecondWallDef / 1000;
    SecondWallDef.IdWall_270 = FirstWallDef.IdWall_270;
}
function CreateConectionT_90_00(FirstWall, SecondWall) {
    if (FirstWall.position.z > SecondWall.position.z) {
        const FirstWallX = FirstWall.position.x;
        const SecondWallScaledWidth = SecondWall.scale.x * FactorScale;
        const SecondWallEndX = SecondWall.position.x + SecondWallScaledWidth;

        if ((FirstWallX - PositionOffset) < SecondWall.position.x) {
            CreateConectionT_90_00_Top_L_10(FirstWall, SecondWall);
        } else {
            if (SecondWallEndX > FirstWallX) {
                CreateConectionT_90_00_Top_T(FirstWall, SecondWall);
            }
            else {
                CreateConectionT_90_00_Top_L_30(FirstWall, SecondWall);
            }
        }

    }
    else {
        CreateConectionT_90_00_Down(FirstWall, SecondWall);
    }
}
function CreateConectionT_90_00_Down(FirstWall, SecondWall) {
    const FirstWallX = FirstWall.position.x;
    const SecondWallScaledWidth = SecondWall.scale.x * FactorScale;
    if ((FirstWallX - PositionOffset) < SecondWall.position.x) {
        CreateConection_90_00_L_70(FirstWall, SecondWall);
    } else {
        CreateConection_90_00_L_180(FirstWall, SecondWall);
    }
};
function CreateConectionT_90_00_Top_T(FishConetionWall, SecontConexionWall) {
    Wall_Conexion_1.material = materialWall;
    Wall_Conexion_2.material = materialWall;
    var ReferentPoint = SecontConexionWall.position.z - FishConetionWall.position.z;
    if (ReferentPoint < 0) { ReferentPoint = ReferentPoint * -1 }
    var W_Wall = 30;
    ReferentPoint = ReferentPoint - W_Wall;
    //Aqui
    AddCorner20_00Conexion(null, ReferentPoint, FishConetionWall, SecontConexionWall);
    //scene.remove(FishConetionWall);
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ActionWizard = 0;
};
function CreateConectionT_90_00_Top_L_10(FirstWall, SecondWall) {
    Wall_Conexion_1.material = materialWall;
    Wall_Conexion_2.material = materialWall;
    //DrawPoint(FirstWall.position.x, 275, FirstWall.position.z + (FirstWall.scale.z * 1000));
    var ReferentPoint = SecondWall.position.z - FirstWall.position.z;
    if (ReferentPoint < 0) { ReferentPoint = ReferentPoint * -1 }
    var W_Wall = 30;
    ReferentPoint = ReferentPoint - W_Wall;

    //DrawPoint(FirstWall.position.x, 275, FirstWall.position.z);
    var diffScaleX = FirstWall.position.x - SecondWall.position.x;
    SecondWall.scale.x = SecondWall.scale.x - (diffScaleX / 1000);
    var e = FirstWall.scale.x * 1000;
    SecondWall.position.x = FirstWall.position.x - e;
    /*AddCorner10_00(IsTemporal)*/
    obWall = SecondWall;
    //Aqui
    FirstWall.scale.z = FirstWall.scale.z + (ReferentPoint / 1000);
    FirstWall.position.z = FirstWall.position.z - ReferentPoint;
    AddCorner10_00(null, ReferentPoint, FirstWall);
    //scene.remove(FishConetionWall);
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ActionWizard = 0;
};
function CreateConectionT_90_00_Top_L_30(FishConetionWall, SecontConexionWall) {
    Wall_Conexion_1.material = materialWall;
    Wall_Conexion_2.material = materialWall;
    var ReferentPointFish = SecontConexionWall.position.z - FishConetionWall.position.z;
    if (ReferentPointFish < 0) { ReferentPointFish = ReferentPointFish * -1 }
    var W_Wall = 30;
    ReferentPointFish = ReferentPointFish - W_Wall;
    //DrawPoint((FishConetionWall.position.x - (FishConetionWall.scale.x * 1000)), 275, FishConetionWall.position.z);
    var ReferentPoint = FishConetionWall.position.x - (FishConetionWall.scale.x * 1000);
    if (ReferentPoint < 0) { ReferentPoint = ReferentPoint * -1 }

    ReferentPoint = ReferentPoint;
    var diffScaleX = (ReferentPoint - SecontConexionWall.position.x) / 1000;
    SecontConexionWall.scale.x = diffScaleX - (W_Wall / 1000);
    var e = FishConetionWall.scale.x * 1000;
    //SecontConexionWall.position.x = FishConetionWall.position.x - e;
    /*AddCorner10_00(IsTemporal)*/
    obWall = SecontConexionWall;
    //Aqui
    FishConetionWall.scale.z = FishConetionWall.scale.z + (ReferentPointFish / 1000);
    FishConetionWall.position.z = FishConetionWall.position.z - ReferentPointFish;

    //Esquinas
    var IdpartName = new Date().valueOf();
    var IdEsq_00_D = "Esq_10_00" + IdpartName;
    var IdEsq_90_D = "Esq_10_90" + IdpartName;
    var IdWall_90_D = ""
    var IdWall_00_D = "";
    var Sub_Long_0 = "0";
    var Sub_Long_90 = "0";
    var Sub_Long_270 = "0";
    var Sub_Long_180 = "0";
    var IdWall_180 = "0";
    var IdWall_270 = "0";
    var IdWall_0 = "0";
    var IdWall_90 = "0";
    var xSub = GetXsub(obWall.scale.y);
    var EsqLefXPosition = (obWall.position.x + obWall.scale.x * 1000);
    var EsqLefYPosition = obWall.position.z;
    var EsqLefLong = xSub;
    var EsqLefWidth = obWall.scale.y;
    var EsqLefHeigh = obWall.scale.z;
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
     /*14*/   IdWall_270,
     /*15*/   Sub_Long_270,
     /*16*/   IdUndoRedo,
     /*17*/   false
    );
    var EsqTopXPosition = FishConetionWall.position.x;
    var EsqTopYPosition = obWall.position.z - (obWall.scale.y * 1000);
    var EsqTopLong = xSub;
    var EsqTopWidth = obWall.scale.y;
    var EsqTopHeigh = obWall.scale.z;
    AddWall_R900(
        EsqTopXPosition,
        EsqTopYPosition,
        EsqTopLong,
        EsqTopWidth,
        EsqTopHeigh,
        "Esq_30_90",
        IdEsq_90_D,
        IdWall_0,
        Sub_Long_0,
        Sub_Long_180,
        IdWall_180,
        IdWall_90,
        Sub_Long_90,
        IdWall_90_D,
        Sub_Long_270,
        IdUndoRedo
    );
    Wall_Conexion_1 = null;
    Wall_Conexion_2 = null;
    ActionWizard = 0;
};

function AddCorner20_00Conexion(IsTemporal, ReferentPoint, FishConetionWall, SecontConexionWall) {
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
    var xSub = GetXsub(FishConetionWall.scale.x + 0.03);
    IdUndoRedo = IdUndoRedo + 1;
    var Value = (FishConetionWall.position.x - SecontConexionWall.position.x);
    var Value2 = (FishConetionWall.scale.x + 0.03) * 1000;
    Value = (Value - Value2) / 1000;
    Value = parseFloat(Value);
    var WallXPosition = SecontConexionWall.position.x;
    var WallLong = Value;
    var WallYPosition = SecontConexionWall.position.z;
    var WallWidth = SecontConexionWall.scale.y;
    var Wallheigh = SecontConexionWall.scale.z;
    //Wall_R000_180
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
     /*10 */ SecontConexionWall.Sub_Long_180,
     /*11 */ SecontConexionWall.IdWall_180,
     /*12 */ IdWall_90,
     /*13 */ Sub_Long_90,
     /*14 */ IdWall_270,
     /*15 */ Sub_Long_270,
     /*16 */ IdUndoRedo,
     /*17*/  false,
     /*18*/  SecontConexionWall,
    );
    FishConetionWall.scale.z = FishConetionWall.scale.z + (ReferentPoint / 1000);
    FishConetionWall.position.z = FishConetionWall.position.z - ReferentPoint;
    var distWall00 = (SecontConexionWall.position.x - FishConetionWall.position.x);
    if (distWall00 < 0) { distWall00 = distWall00 * -1 }
    distWall00 = ((distWall00 + (SecontConexionWall.scale.y * 1000))) / 1000;
    SecontConexionWall.scale.x = SecontConexionWall.scale.x - distWall00;
    SecontConexionWall.position.x = SecontConexionWall.position.x + (distWall00 * 1000);


    var WallXEsq = WallXPosition + WallLong * 1000;
    var WallXLong = xSub;
    // Wall Right
    /*    getIdEsq30(obWall);*/
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

    var restx = xSub / 2;
    restx = restx + SecontConexionWall.scale.y / 2;
    var Wall90XPosition = WallXEsq + (restx * 1000);
    var Wall90YPosition = SecontConexionWall.position.z - (SecontConexionWall.scale.y * 1000);
    var Wall90Long = 0.03 + SecontConexionWall.scale.y;
    var Wall90Width = SecontConexionWall.scale.y;
    var Wall90Heigh = SecontConexionWall.scale.z;
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

    ResetSetup();
    InsertWall = 102;
};