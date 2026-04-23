function AddWallCornerWall20() {
    if (ActiveAddCorner === "AddCorner20") {
        obWallMouseMove = null;
        if (isYColision !== true) { isYColision = false; }
        event.preventDefault();
        mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects);
        if (intersects.length > 0) {
            const intersect = intersects[0];
            meshEsq20.visible = true;
            meshEsq20.position.copy(intersect.point).add(intersect.face.normal);
            meshEsq20.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
            if (isYColision === true) {
                meshEsq20.position.z = yColision;
                isYColision = true;
                xColision = 0;
                ActionDbl = "AddCorner20";
                AddDim20(obWall);
                return;
            }
            else {
                yColision = meshEsq20.position.y;
                xColision = meshEsq20.position.x;
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
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R000")
                    {
                        isYColision = true;
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        return;
                    }
                    //if (intersectsSelection[i5].object.MeshTypeWall === "Grill_000") {
                    //    isYColision = true;
                    //    yColision = intersectsSelection[i5].object.position.z;
                    //    obWall = intersectsSelection[i5].object;
                    //    return;
                    //}
                }
            }
        }
    }
};

