function SedAndGetVariableList(children, _listWalls)
{
    var j = children.name.substr(0, 9);
    var _dataRotateX = (children.rotation.x * 100).toFixed(2).toString();
    var _dataRotateY = (children.rotation.y * 100).toFixed(2).toString();
    var _dataRotateZ = (children.rotation.z * 100).toFixed(2).toString();
    var _XWith = children.XWith;
    var _YWith = children.YWith;
    var _Type = 0;
    var _Datalong = 0;
    var _DataWith = 0;
    var _DataHeight = 0;
    var _LongLeft = 0;
    var _LongRight = 0;
    var _idWall = 0;
    var _UniversalPanel = false;
    var _CHeck750R = false;
    var _Tape_0 = "";
    var _Tape_180 = "";
    var _Tape_90 = "";
    var _Tape_270 = "";
    var _DataSupEnd = 0;
    var _Sub_Long_180 = "";
    var _Sub_Long_0 = "";
    var _IdWall_90  = "";
    var _IdWall_270 = "";
    var _Sub_Long_90 = "";
    var _Sub_Long_270 = "";
    var _TypeWall_0 = "";
    var _TypeWall_180 = "";
    var _TypeWall_90 = "";
    var _TypeWall_270 = "";
    var _IdTypeFormworkMode = 0;
    var _DataWithOtherCorner = "";
    if (children.name.substr(0, 4) === "Esq_")
    {
        _TypeMesh = children.MeshTypeWall;
        _idWall = children.idWall;
        _Type = 4;
        _Datalong = Math.ceil((children.scale.x) * 10000).toFixed();
        _DataWith = Math.ceil((children.scale.y) * 10000).toFixed();
        _DataWithOtherCorner = Math.ceil(children.Sub_Long_0 * 10000).toFixed(); 
        _DataHeight = Math.ceil((children.scale.z) * 10000).toFixed();
        _UniversalPanel = _UniversalPanel;
        _XWith = 0;
        _YWith = 0;
        _YWith = 0;
        _LongLeft = Math.ceil((children.LongLeft) * 10000).toFixed();
        _LongRight = Math.ceil((children.LongRight) * 10000).toFixed();
        _CHeck750R = children.CHeck750R;
        _Tape_0 = children.Tape_0;
        _Tape_180 = children.Tape_180;
        _Tape_90 = children.Tape_90;
        _Tape_270 = children.Tape_270;
        _Sub_Long_180 = children._Sub_Long_180;
        _Sub_Long_0 = children._Sub_Long_0;
        _IdWall_90  = children._IdWall_90 ;
        _IdWall_270 = children._IdWall_270;
        _Sub_Long_90 = children._Sub_Long_90;
        _Sub_Long_270 = children._Sub_Long_270;
        _TypeWall_0 = children._TypeWall_0;
        _TypeWall_180 = children._TypeWall_180;
        _TypeWall_90 = children._TypeWall_90;
        _TypeWall_270 = children._TypeWall_270;
    }
    if (children.name.substr(0, 5) === "Pilar") {
        _TypeMesh = children.MeshTypeWall;
        _idWall = children.idWall;
        _Type = 7;
        _Datalong = Math.ceil((children.scale.x) * 10000).toFixed();
        _DataWith = Math.ceil((children.scale.y) * 10000).toFixed();
        _DataWithOtherCorner = "";
        _DataHeight = Math.ceil((children.scale.z) * 10000).toFixed();
        _UniversalPanel = _UniversalPanel;
        _XWith = 0;
        _YWith = 0;
        _YWith = 0;
        _LongLeft = Math.ceil((children.LongLeft) * 10000).toFixed();
        _LongRight = Math.ceil((children.LongRight) * 10000).toFixed();
        _CHeck750R = children.CHeck750R;
        _Tape_0 = children.Tape_0;
        _Tape_180 = children.Tape_180;
        _Tape_90 = children.Tape_90;
        _Tape_270 = children.Tape_270;
        _Sub_Long_180 = children._Sub_Long_180;
        _Sub_Long_0 = children._Sub_Long_0;
        _IdWall_90  = children._IdWall_90 ;
        _IdWall_270 = children._IdWall_270;
        _Sub_Long_90 = children._Sub_Long_90;
        _Sub_Long_270 = children._Sub_Long_270;
    }
    switch (children.name.substr(0, 9))
    {
        case "Wall_R000":
            // Sedobject(children);
            _TypeMesh = children.MeshTypeWall;
            _idWall = children.idWall; 
            _Type = 1;
            _Datalong = Math.ceil((children.scale.x) * 10000).toFixed();
            _DataWith = Math.ceil((children.scale.y) * 10000).toFixed();
            _DataWithOtherCorner = "";
            _DataHeight = Math.ceil((children.scale.z) * 10000).toFixed();
            _UniversalPanel = _UniversalPanel;
            _XWith = 0;
            _YWith = 0;
            _YWith = 0;
            _LongLeft = Math.ceil((children.LongLeft) * 10000).toFixed();
            _LongRight = Math.ceil((children.LongRight) * 10000).toFixed();
            _CHeck750R = children.CHeck750R;
            _IdTypeFormworkMode = children.IdTypeFormworkMode;
            if (parseInt(children.IdWall_0) !== 0) {
                _Tape_0 = children.IdWall_0;
            }
            //if (parseInt(children.IdWall_0) !== '0') {
            //    _Tape_0 = 0;
            //}
            else {
                _Tape_0 = children.Tape_0;
            }
            if (parseInt(children.IdWall_180) !== 0) {
                _Tape_180 = children.IdWall_180;
            }
            else {
                _Tape_180 = "";
            }
            if (parseInt(children.IdWall_90) !== 0) {
                _Tape_90 = children.IdWall_90;
            }
            else {
                _Tape_90 = "";
            }
            if (parseInt(children.IdWall_270) !== 0) {
                _Tape_270 = children.IdWall_270;
            }
            else {
                _Tape_270 = "";
            }
            _DataSupEnd = children.End_Wall;
            _Sub_Long_180 = children._Sub_Long_180;
            _Sub_Long_0 = children._Sub_Long_0;
            _IdWall_90  = children._IdWall_90 ;
            _IdWall_270 = children._IdWall_270;
            _Sub_Long_90 = children._Sub_Long_90;
            _Sub_Long_270 = children._Sub_Long_270;
            _TypeWall_0 = children._TypeWall_0;
            _TypeWall_180 = children._TypeWall_180;
            _TypeWall_90 = children._TypeWall_90;
            _TypeWall_270 = children._TypeWall_270;
            break;
        case "Wall_R900":
            _IdTypeFormworkMode = children.IdTypeFormworkMode;
            _TypeMesh = children.MeshTypeWall;
            _idWall = children.idWall;
            _DataHeight = Math.ceil((children.scale.y) * 10000).toFixed();
            _DataWith = Math.ceil((children.scale.x * 10000)).toFixed();
            _DataWithOtherCorner = "";
            _Datalong = Math.ceil((children.scale.z) * 10000).toFixed();
            _Type = 2;
            _UniversalPanel = _UniversalPanel;
            _XWith = 0;
            _YWith = 0;
            _LongLeft = Math.ceil((children.LongLeft) * 10000).toFixed();
            _LongRight = Math.ceil((children.LongRight) * 10000).toFixed();
            _CHeck750R = children.CHeck750R;
            _Sub_Long_180 = children._Sub_Long_180;
            _Sub_Long_0 = children._Sub_Long_0;
            _IdWall_90  = children._IdWall_90 ;
            _IdWall_270 = children._IdWall_270;
            _Sub_Long_90 = children._Sub_Long_90;
            _Sub_Long_270 = children._Sub_Long_270;
            _TypeWall_0 = children._TypeWall_0;
            _TypeWall_180 = children._TypeWall_180;
            _TypeWall_90 = children._TypeWall_90;
            _TypeWall_270 = children._TypeWall_270;
            if (parseInt(children.IdWall_0) !== 0) {
                _Tape_0 = children.Tape_0;
            }
            else {
                _Tape_0 = "";
            }

            if (parseInt(children.IdWall_180) !== 0) {
                _Tape_180 = children.Tape_180;
            }
            else {
                _Tape_180 = "";
            }
            if (parseInt(children.IdWall_90) !== 0) {
                _Tape_90 = children.Tape_90;
            }
            else {
                _Tape_90 = "";
            }
            if (parseInt(children.IdWall_270) !== 0) {
                _Tape_270 = children.Tape_270;
            }
            else {
                _Tape_270 = "";
            }
            break;
        case "WallEsqTL":
            _TypeMesh = children.MeshTypeWall;
            _idWall = 0;
            //Esquina Top Lef
            _DataHeight = Math.ceil((children.scale.y) * 10000).toFixed();
            _DataWith = Math.ceil((children.scale.x) * 10000).toFixed();
            _DataWithOtherCorner = "";
            _Datalong = Math.ceil((children.scale.z) * 10000).toFixed();
            _UniversalPanel = children.UniversalPanel;
            _Type = 4;
            _XWith = Math.ceil((_XWith) * 10000).toFixed();
            _YWith = Math.ceil((_YWith) * 10000).toFixed();
            _LongLeft = 0;
            _LongRight = 0;
            _CHeck750R = children.CHeck750R;
            _Sub_Long_180 = children._Sub_Long_180;
            _Sub_Long_0 = children._Sub_Long_0;
            _IdWall_90  = children._IdWall_90 ;
            _IdWall_270 = children._IdWall_270;
            _Sub_Long_90 = children._Sub_Long_90;
            _Sub_Long_270 = children._Sub_Long_270;
            _TypeWall_0 = children._TypeWall_0;
            _TypeWall_180 = children._TypeWall_180;
            _TypeWall_90 = children._TypeWall_90;
            _TypeWall_270 = children._TypeWall_270;
            break;
        case "WallEPanel":
            //Panel Universal
            _TypeMesh = children.MeshTypeWall;
            _idWall = 0;
            _DataHeight = Math.ceil((children.scale.y) * 10000).toFixed();
            _DataWith = Math.ceil((children.scale.x) * 10000).toFixed();
            _DataWithOtherCorner ="";
            _Datalong = Math.ceil((children.scale.z) * 10000).toFixed();
            _Type = 5;
            _UniversalPanel = _UniversalPanel;
            _XWith = 0;
            _YWith = 0;
            _LongLeft = 0;
            _LongRight = 0;
            _CHeck750R = children.CHeck750R;
            _Sub_Long_180 = children._Sub_Long_180;
            _Sub_Long_0 = children._Sub_Long_0;
            _IdWall_90  = children._IdWall_90 ;
            _IdWall_270 = children._IdWall_270;
            _Sub_Long_90 = children._Sub_Long_90;
            _Sub_Long_270 = children._Sub_Long_270;
            break;
    }
    _listWalls.push
        ({
            TypeMesh: _TypeMesh,
            DesignId: id,
            Datalong: _Datalong,
            DataWith: _DataWith,
            DataWithOtherCorner: _DataWithOtherCorner,
            DataHeight: _DataHeight,
            DataRotateX: _dataRotateX,
            DataRotateY: _dataRotateY,
            DataRotateZ: _dataRotateZ,
            DataCordenadX: Math.ceil(children.position.x),
            DataCordenadY: Math.ceil(children.position.z),
            Type: _Type,
            DataSupInicial: children.Iniciall_Wall,
            DataSupEnd: _DataSupEnd,
            UniversalPanel: _UniversalPanel,
            XWith: _XWith,
            YWith: _YWith,
            IdWall: _idWall,
            LongLeft: _LongLeft,
            LongRight: _LongRight,
            CHeck750R: _CHeck750R,
            Tape_0: _Tape_0,
            Tape_180: _Tape_180,
            Tape_90: _Tape_90,
            Tape_270: _Tape_270,
            Sub_Long_180: _Sub_Long_180,
            Sub_Long_0: _Sub_Long_0,
            IdWall_90 : _IdWall_90 ,
            IdWall_270: _IdWall_270,
            Sub_Long_90: _Sub_Long_90,
            Sub_Long_270: _Sub_Long_270,
            TypeWall_0: _TypeWall_0,
            TypeWall_180: _TypeWall_180,
            TypeWall_90: _TypeWall_90,
            TypeWall_270: _TypeWall_270,
            IdTypeFormworkMode: _IdTypeFormworkMode
        });
};
