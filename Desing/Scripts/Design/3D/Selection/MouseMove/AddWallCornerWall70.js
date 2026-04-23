function AddWallCornerWall70() {
    if (isYColision === false && ActiveAddCorner === "AddCorner70_exit") { ActiveAddCorner = "AddCorner70"; }
    if (ActiveAddCorner === "AddCorner70") {
        obWallMouseMove = null;
        if (isYColision !== true) {
            isYColision = false;
        }
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
                ActiveAddCorner = "AddCorner70_exit";
                xColision = 0;
                yColision = 0;
                if (TypeConetion === "Wall_R900") {
                    ActionDbl = "AddCorner70_90";
                    return;
                }
                if (TypeConetion === "Wall_R000") {
                    ActionDbl = "AddCorner70_00";
                    return;
                }
            }
            else {
                yColision = meshEsq70.position.y;
                xColision = meshEsq70.position.x
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
                    //wall 00

                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R000") {
                        obWall = intersectsSelection[i5].object;
                        if (obWall.IdWall_180 !== "0") {
                            continue;
                        }

                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x;
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R900";
                        return;
                    }
                    //Wall 90
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R900") {
                        obWall = intersectsSelection[i5].object;
                        if (obWall.IdWall_270 !== "0") {
                            continue;
                        }
                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x;
                        yColision = intersectsSelection[i5].object.position.z + (intersectsSelection[i5].object.scale.z * 1000);
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R000";
                        return;
                    }

                }
            }
        }
    }
};

