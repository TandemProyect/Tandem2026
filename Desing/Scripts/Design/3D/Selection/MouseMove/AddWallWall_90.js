function AddWallWall_90() {
    event.preventDefault();
    mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(objects);
    if (intersects.length > 0) {
        const intersect = intersects[0];
        meshWall_90.visible = true;
        meshWall_90.position.copy(intersect.point).add(intersect.face.normal);
        meshWall_90.position.divideScalar(25).floor().multiplyScalar(25).addScalar(12.5);
        if (isYColision === true) {
            meshWall_90.position.z = yColision;
            xColision = 0;
            //ActionDbl = "AddCorner20";
            //AddDim60(obWall);
        }
        else {
            yColision = meshWall_90.position.y;
        }
        if (isXColision === true) {
            meshWall_90.position.x = XColision + ChangeConectionPosition;
            xColision = 0;
            //ActionDbl = "AddCorner20";
            //AddDim60(obWall);
        }
        else {
            xColision = meshWall_90.position.x + ChangeConectionPosition;
        }
    }
    var intersectsSelection = raycaster.intersectObjects(scene.children);
    for (var i5 = 0; i5 < intersectsSelection.length; i5++) {
        if (intersectsSelection[i5].object.name === "") { continue; }
        if (intersectsSelection[i5].object.type === "Mesh") {
            if (isYColision !== true) {
                if (intersectsSelection[i5].object.MeshTypeWall === "Grill_000") {
                    isYColision = true;
                    yColision = intersectsSelection[i5].object.position.z;
                    obWall = intersectsSelection[i5].object;
                    obWall.material = materialGrillAct;
                    ChangeConection = true;
                    return;
                }
            }
            if (isXColision !== true) {
                if (intersectsSelection[i5].object.MeshTypeWall === "Grill_900") {
                    isXColision = true;
                    XColision = intersectsSelection[i5].object.position.x + ChangeConectionPosition;
                    obWall = intersectsSelection[i5].object;
                    obWall.material = materialGrillAct;
                    ChangeConection = true;
                    return;
                }
            }
        }
    }
};