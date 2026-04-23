$(document).mouseup(function (event)
{
    if (InsertWall === 1033) {
        ActionDbl = "";
        //WallAddControl();
        //InsertDimWallH(obWall.scale.x * 1000, "Wall", obWall.position.x, obWall.position.y, obWall.position.z);
        controls = ReturnControlsForCamera(camera, 1);
        InsertWall = 102;
    }
    if (InsertWall === 103) 
    {
        ActionDbl = "";
        //WallAddControl();
        //InsertDimWallH(obWall.scale.x * 1000, "Wall", obWall.position.x, obWall.position.y, obWall.position.z);
        controls = ReturnControlsForCamera(camera, 1);
        InsertWall = 102;
    }
    if (InsertWall === 104) {
        //ActionDbl = "";
        //AddWallControlTop();
        //controls = ReturnControlsForCamera(camera, 1);
        InsertWall = 102;
    }
});
$(document).mousedown(function (event) {
    if (InsertWall === 103)
    {
        //document.getElementById("DivWallDimension").style.visibility = "visible";
        //document.getElementById("DivWallDimension").style.left = event.clientX +20;
        //document.getElementById("DivWallDimension").style.top = event.clientY + 20;
        InsertWall = 102;
    }
});
function UpdateControl() { 
}
function ReturnControlsForCamera(camObject, ActionType) {
    if (ActionType === 1) {
        camera.controls.enabled = true;
        perspectiveCamera.controls.enabled = true;
     }
    else {
        camera.controls.enabled = false;
        perspectiveCamera.controls.enabled = false;
    }
}



 

