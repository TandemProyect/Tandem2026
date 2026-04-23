//Camera

//var Camera

var cameraTopPosition = new Position(x = 0, y = 3500, z = 0);
var cameraLeftPosition = new Position(x = -1000, y = 300, z = 0);
var cameraFrontPosition = new Position(x = 0, y = 300, z = 1000);
var cameraPerspective3dPosition = new Position(x = -600, y = 550, z = 700);
var camSettings = {
    near: 10,
    far: 30000,
    perspectiveFov: 60,
    orthographicScale: 3,
    orthographicZoom: 0.4,
    Factor: 200
};
 
var cameraTypeId = 1;
var oZoom = camSettings.orthographicZoom;
var cameraDefaultPosition = new Position(x = 0, y = 300, z = 1000);
var cameraDefaultTarget = new Position(x = 0, y = 300, z = 0);

var currentPanSpeed = 10;
var currentZoomSpeed = 1;
 
var camSettings = {
    near: 10,
    far: 30000,
    perspectiveFov: 60,
    orthographicScale: 3,
    orthographicZoom: 0.4,
    Factor: 200
};
 
 
//Creates both cameras
$("#areaTop").on('click', function (e) {
    e.stopPropagation();
    e.preventDefault();
    setView('top');
});
$("#areaLeft").on('click', function (e) {
    e.stopPropagation();
    e.preventDefault();
    setView('left');
});
$("#areaFront").on('click', function (e) {
    e.stopPropagation();
    e.preventDefault();
    setView('front');
});
$("#area3d").on('click', function (e) {
    e.stopPropagation();
    e.preventDefault();
    setView('3d');
});
 
function setCameraValuesFromDatabase(_type, _positionX, _positionY, _positionZ, _targetX, _targetY, _targetZ, _zoom) {
    cameraDefaultPosition = new Position(0, 0, 0);
    cameraDefaultTarget = new Position(0, 0, 0);

    cameraDefaultPosition.x = _positionX;
    cameraDefaultPosition.y = _positionY;
    cameraDefaultPosition.z = _positionZ;

    cameraDefaultTarget.x = _targetX;
    cameraDefaultTarget.y = _targetY;
    cameraDefaultTarget.z = _targetZ;
};
 