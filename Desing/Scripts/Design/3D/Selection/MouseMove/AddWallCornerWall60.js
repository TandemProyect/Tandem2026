function AddWallCornerWall60() {
    if (ActiveAddCorner === "AddCorner60") {
        obWallMouseMove = null;
        if (isYColision !== true) { isYColision = false; }
        event.preventDefault();
        mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects);
        if (intersects.length > 0) {
            const intersect = intersects[0];
            meshEsq60.visible = true;
            meshEsq60.position.copy(intersect.point).add(intersect.face.normal);
            meshEsq60.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
            if (isYColision === true) {
                meshEsq60.position.z = yColision;
                isYColision = true;
                xColision = 0;
                ActionDbl = "AddCorner60";
                AddDim60(obWall);
                return;
            }
            else {
                yColision = meshEsq60.position.y;
                xColision = meshEsq60.position.x;
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
                        obWall = intersectsSelection[i5].object;
                        isYColision = true;
/*                        xColision = intersectsSelection[i5].object.position.x;*/
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        return;
                    }
                }
            }
        }
    }
};

