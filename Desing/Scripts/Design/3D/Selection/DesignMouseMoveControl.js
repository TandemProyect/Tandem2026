//function MouseMove_Control_0(intersectsObject) {
//    var d = 0;
//    AddDivDim = true;
//    ImDraw = true;
//    ActionDbl = "Control_Move_0";
//    document.getElementById("DivInputDim").style.display = "inline";
//    controls = ReturnControlsForCamera(camera, 2);
//    renderer.domElement.style.cursor = 'pointer';
//    var Difx = 0;
//    intersectsObject.material = MaterialSelectIcon;
//    var dragControls = null;
//    dragControls = new THREE.DragControls(objectsMoveX, camera, renderer.domElement);
//    var currentZ = 0;
//    var currentX = 0;
//    dragControls.addEventListener('dragstart', function () {
//        currentZ = objectsMoveX[0].position.z;
//    });
//    dragControls.addEventListener('drag', function (e) {
//        objectsMoveX[0].position.y = 0;
//        objectsMoveX[0].position.z = currentZ;
//        var a = parseInt((obWall.position.x + obWall.scale.x * 1000).toFixed(0));
//        var b = parseInt((objectsMoveX[0].position.x).toFixed(0));
//        d = (b - a) / 1000;
//        obWall.scale.x = obWall.scale.x + d;
//        $("#Datalong").val((obWall.scale.x * 10).toFixed(2));
//        $("#DataCordenadX").val((obWall.position.x * 10).toFixed(2));
//        ConeRight.position.x = objectsMoveX[0].position.x;
//        LineDimRight.position.x = objectsMoveX[0].position.x;
//        LineRightToLeft.position.x = obWall.position.x + Difx;
//        LineRightToLeft.scale.x = obWall.scale.x;
//        NewPosition = obWall.position.x;
//        _dim.visible = false;
//        event.preventDefault();
//        NameTextDim = (obWall.scale.x * 10).toFixed(3);
//        var xDim = obWall.position.x + ((obWall.scale.x * 1000) / 2);
//        var yDim = obWall.position.z + 190;
//        //AddDimTemporal(NameTextDim, xDim, yDim);
//        if (obWall.scale.x < 0.029) {
//            alert("El muro no puede medir menos de 0,30 mts");
//            obWall.scale.x = xOwall;
//            ResetSetup();
//            return;
//        }
//    });
//    dragControls.addEventListener('dragend', function () {
//        controls = ReturnControlsForCamera(camera, 1);
//        meshControl_Move_180.material = MaterialUnSelectIcon;
//        NameTextDim = (obWall.scale.x * 10).toFixed(3);
//        var x = obWall.position.x + ((obWall.scale.x * 1000) / 2);
//        var y = obWall.position.z + 190;
//        AddDimText(NameTextDim, x, y);
//    });
//};