function AddWallCornerTopLeft()
{
    obWallMouseMove = null;
    if (isYColision !== true) { isYColision = false; }
    event.preventDefault();
    mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(objects);
    if (intersects.length > 0) {
        const intersect = intersects[0];
        meshEsq70.visible = true;
        meshEsq70.position.copy(intersect.point).add(intersect.face.normal);
        meshEsq70.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
        if (isYColision === true) {
            meshEsq70.position.x = xColision;
            meshEsq70.position.z = yColision;
            meshEsq70.visible = false;
            isYColision = false;
            xColision = 0;
            yColision = 0;
            TWTop0CDown1();
            return;
        }
        else {
            yColision = meshEsq70.position.y;
            xColision = meshEsq70.position.x;
        }
    }
    var intersectsSelection = raycaster.intersectObjects(scene.children);
    if (isYColision === false) {
        for (var i5 = 0; i5 < intersectsSelection.length; i5++) {
            if (isYColision === true) {
                continue;
            }
            if (intersectsSelection[i5].object.name === "") { continue; }
            if (intersectsSelection[i5].object.type === "Mesh") {
                if (intersectsSelection[i5].object.MeshTypeWall === "TWLef0CRight0") {
                    isYColision = true;
                    xColision = intersectsSelection[i5].object.position.x;
                    yColision = intersectsSelection[i5].object.position.z;
                    obWall = intersectsSelection[i5].object;
                    return;
                }
            }
        }
    } 
};