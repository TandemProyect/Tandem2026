function SaveDesing(Scenechildren, id)
{
    //meshWall.IdWall_180 = IdWall_180;
    //meshWall.IdWall_0 = IdWall_0;
    //meshWall.Sub_Long_180 = Sub_Long_180;
    //meshWall.Sub_Long_0 = Sub_Long_0;
    var ListSaveScene = [];
    for (var i = 0; i < Scenechildren.length; i++)
    {
        if (Scenechildren[i].MeshTypeWall === undefined)
        {
            continue;
        }
        if (Scenechildren[i].type === "Mesh")
        {
            //if (Scenechildren[i].MeshTypeWall === "Wall_R000" || Scenechildren[i].MeshTypeWall === "Wall_R900" || Scenechildren[i].MeshTypeWall.substring(0, 4) === "Esq_")
            //{
                var objectToSave = Scenechildren[i];
                var Tape_0 = "";
                var Tape_90 = "";
                var Tape_180 = "";
                var Tape_270 = "";
                if (objectToSave.Tape_0 !== null) { Tape_0 = objectToSave.Tape_0 };
                if (objectToSave.Tape_90 !== null) { Tape_90 = objectToSave.Tape_90 };
                if (objectToSave.Tape_180 !== null) { Tape_180 = objectToSave.Tape_180 };
                if (objectToSave.Tape_270 !== null) { Tape_270 = objectToSave.Tape_270 };
                ListSaveScene.push
                    ({
                        IDDesaing: id,
                        PositionX: objectToSave.position.x,
                        PositionY: objectToSave.position.y,
                        PositionZ: objectToSave.position.z,
                        IdWall: objectToSave.idWall,
                        RotationX: objectToSave.rotation.x,
                        RotationY: objectToSave.rotation.y,
                        RotationZ: objectToSave.rotation.z,
                        Name: objectToSave.name,
                        ScaleX: objectToSave.scale.x,
                        ScaleY: objectToSave.scale.y,
                        ScaleZ: objectToSave.scale.z,
                        Iniciall_Wall: objectToSave.Iniciall_Wall,
                        End_Wall: objectToSave.End_Wall,
                        TypeWall: objectToSave.MeshTypeWall,
                        TypeWallLeft: objectToSave.MeshTypeWallLeft,
                        TypeWallRight: objectToSave.MeshTypeWallRight,
                        TypeWall_180: objectToSave.MeshTypeWall_180,
                        TypeWall_0: objectToSave.MeshTypeWall_0,
                        IDCornerDown: objectToSave.IdCornerDown,
                        IDCornerLeft: objectToSave.IdCornerLeft,
                        ScaleEsqy: objectToSave.ScaleEsqy,
                        CHeckDimWall: objectToSave.CHeckDimWall,
                        CHeckBracketInside: objectToSave.CHeckBracketInside,
                        CHeckBracketOutside: objectToSave.CHeckBracketOutside,
                        CHeckRijiInside: objectToSave.CHeckRijiInside,
                        CHeckRijiOutside: objectToSave.CHeckRijiOutside,
                        CHeckPropInside: objectToSave.CHeckPropInside,
                        CHeckPropOutside: objectToSave.CHeckPropOutside,
                        CHeckPropInsideInf: objectToSave.CHeckPropInsideInf,
                        CHeckPropOutsideInf: objectToSave.CHeckPropOutsideInf,
                        CHeck750R: objectToSave.CHeck750R,
                        LongLeft: objectToSave.LongLeft,
                        LongRight: objectToSave.LongRight,
                        IsSolutionCornerYUniversalPanelCorner: objectToSave.IsSolutionCornerYUniversalPanelCorner,
                        IsSolutionCornerXUniversalPanelCorner: objectToSave.IsSolutionCornerXUniversalPanelCorner,
                        Tape_0: Tape_0,
                        Tape_180: Tape_180,
                        Tape_90: Tape_90,
                        Tape_270: Tape_270,
                        Grupo: objectToSave.Grupo,
                        IdWall_90  : objectToSave.IdWall_90 ,
                        IdWall_270: objectToSave.IdWall_270,
                        IdWall_0: objectToSave.IdWall_0,
                        IdWall_180: objectToSave.IdWall_180,
                        Sub_Long_0: objectToSave.Sub_Long_0,
                        Sub_Long_180: objectToSave.Sub_Long_180,
                        Sub_Long_90: objectToSave.Sub_Long_90,
                        Sub_Long_270: objectToSave.Sub_Long_270,
                        IdTypeFormworkMode: objectToSave.IdTypeFormworkMode,
                    });
        }
    }
    return ListSaveScene
};