function AddWallCornerWallX()
  {
    if (ActiveAddCorner === "AddCornerX") {
        obWallMouseMove = null;
        if (isYColision !== true) { isYColision = false; }
        event.preventDefault();
        mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(objects);
        if (intersects.length > 0) {
            const intersect = intersects[0];
            meshEsqX.visible = true;
            meshEsqX.position.copy(intersect.point).add(intersect.face.normal);
            meshEsqX.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
                if (isYColision === true) {
                    meshEsqX.position.z = yColision;
                    isYColision = true;
                    ActionDbl = "Control_Move_Esq_X";
                    AddDimX_00(obWall);
                    return;
                }
                else {
                    yColision = meshEsqX.position.y;
                //    xColision = meshEsq20.position.x;
                }
 
     
            if (isXColision === true)
            {
                    meshEsqX.position.x = xColision;
                    isXColision = true;
                    ActionDbl = "Control_Move_Esq_X";
                    AddDimX_90(obWall);
                    return;
                }
                else {
                    yColision = meshEsqX.position.y;
                    xColision = meshEsqX.position.x;
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
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R900") {
                        obWall = intersectsSelection[i5].object;
                        isXColision = true;
                        xColision = intersectsSelection[i5].object.position.x;
                        //yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        return;
                    }
                    if (intersectsSelection[i5].object.MeshTypeWall.substr(0, 9) === "Wall_R000") {
                        obWall = intersectsSelection[i5].object;
                        isYColision = true;
                        /*xColision = intersectsSelection[i5].object.position.x;*/
                        yColision = intersectsSelection[i5].object.position.z;
                        obWall = intersectsSelection[i5].object;
                        return;
                    }

                }
            }
        }
    }
};

