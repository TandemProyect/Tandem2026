function AddWallCornerWall50() {
    if (isYColision === false && ActiveAddCorner === "AddCorner50_exit")
    { ActiveAddCorner = "AddCorner50"; }

    if (ActiveAddCorner === "AddCorner50") {
        obWallMouseMove = null;
        if (isYColision !== true) { isYColision = false; }
        event.preventDefault();
        mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects);
        if (intersects.length > 0) {
            const intersect = intersects[0];
            meshEsq50.visible = true;
            meshEsq50.position.copy(intersect.point).add(intersect.face.normal);
            meshEsq50.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
            if (isYColision === true) {
                meshEsq50.position.x = xColision;
                meshEsq50.position.z = yColision;
                xColision = 0;
                yColision = 0;
                ActiveAddCorner = "AddCorner50_exit";
                if (TypeConetion === "Wall_R000") {
                    ActionDbl = "AddCorner50_00";
                    return;
                }
                if (TypeConetion === "Wall_R900") {
                    ActionDbl = "AddCorner50_90";
                    return;
                }
                return;
            }
            else {
                yColision = meshEsq50.position.y;
                xColision = meshEsq50.position.x;
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
                    if (intersectsSelection[i5].object.MeshTypeWall === undefined) { continue; }
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R000") {
                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x + intersectsSelection[i5].object.scale.x * 1000;
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R000";
                        return;
                    }
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R900") {
                        obWall = intersectsSelection[i5].object;
                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x;
                        yColision = intersectsSelection[i5].object.position.z + (intersectsSelection[i5].object.scale.z * 1000);
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R900";
                        return;
                    }
                }
            }
        }
    }
};