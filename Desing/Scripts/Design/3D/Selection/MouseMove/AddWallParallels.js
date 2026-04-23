function AddWallParallels() {
    meshParall.visible = true;
    obWallMouseMove = null;
    if (isYColision !== true) { isYColision = false; }
    event.preventDefault();
    mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(objects);
    if (intersects.length > 0)
    {
        const intersect = intersects[0];
        if (ParallePositionY !== null) {
            meshParall90.position.copy(intersect.point).add(intersect.face.normal);
            meshParall90.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
        }
        else {
            meshParall.position.copy(intersect.point).add(intersect.face.normal);
            meshParall.position.divideScalar(10).floor().multiplyScalar(10).addScalar(10);
        }
        if (ParallePositionX !== null) 
        {
            meshParall.visible = true;
            meshParall90.visible = false;
            ActionDbl = "Control_Move_Parall";
            meshParall.position.x = ParallePositionX;
            AddDParalles_00(ObParalle);
            return;
        }
        if (ParallePositionY !== null) {
            meshParall.visible = false;
            meshParall90.visible = true;
            ActionDbl = "Control_Move_Parall_90";
            meshParall90.position.z = ParallePositionY + (ObParalle.scale.z * 1000);
            AddDParalles_90(ObParalle);
            return;
        }
    }
//    var intersectsSelection = raycaster.intersectObjects(scene.children);
};

