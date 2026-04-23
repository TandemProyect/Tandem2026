function AddWallCornerWall10() {
    if (isYColision === false && ActiveAddCorner === "AddCorner10_exit") { ActiveAddCorner = "AddCorner10"; }
    if (ActiveAddCorner === "AddCorner10") {
        obWallMouseMove = null;
        event.preventDefault();
        mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects);
        if (intersects.length > 0) {
            const intersect = intersects[0];
            meshEsq10.visible = true;
            meshEsq10.position.copy(intersect.point).add(intersect.face.normal);
            meshEsq10.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
            if (isYColision === true) {
                meshEsq10.position.x = xColision;
                meshEsq10.position.z = yColision;
                xColision = 0;
                yColision = 0;
                ActiveAddCorner = "AddCorner10_exit";
                if (TypeConetion === "Wall_R000") {
                    ActionDbl = "AddCorner10_00";
                    return;
                }
                if (TypeConetion === "Wall_R900") {
                    ActionDbl = "AddCorner10_90";
                    return;
                } 
                if (isXColision === true) {
                    meshEsq10.position.x = XColision;
                    isXColision = true;
                    xColision = 0;
                }
            }
            else {
                yColision = meshEsq10.position.y;
                xColision = meshEsq10.position.x;
                TypeConetion = "";
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
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Grill_900") {
                        isXColision = true;
                        XColision = intersectsSelection[i5].object.position.x;
                        obWall = intersectsSelection[i5].object;
                        obWall.material = materialGrillAct;
                        ChangeConection = true;
                        return;
                    }





                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R900")
                    {
                        obWall = intersectsSelection[i5].object;
                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x - (intersectsSelection[i5].object.scale.x * 1000);
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R900";
                        return;
                    }
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R000")
                    {
                        obWall = intersectsSelection[i5].object;
                        isYColision = true;
                        xColision = intersectsSelection[i5].object.position.x;
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        TypeConetion = "Wall_R000";
                        return;
                    }
                }
            }
        }
    }
};

