// Environment
function createGril() {
    setCameraPerspective();
    setCameraOrthographic();
}
function InertAxis() {
    var loader = new THREE.STLLoader();
    var AxisMaterialX = new THREE.MeshPhongMaterial({
        color: 0x96A2A3,
        shininess: 0
    });
    loader.load("../Content/DesignTools/Control/Axis_X.stl", function (geometry) {
        Axis = new THREE.Mesh(geometry, AxisMaterialX);
        Axis.position.set(0, 0, 0);
        Axis.traverse(function (child) {
            if (child instanceof THREE.Mesh) {
                child.visible = true;
            }
        });
        Axis.scale.set(0.05, 0.05, 0.05);
        //Valdosta.alphaTest: -1;
        Axis.rotation.x = 1.5 * Math.PI;
        //Valdosta.opacity= 0,
        Axis.transparent = true,
            Axis.callback = function () { console.log(this.name); }
        scene.add(Axis);
        Axis.name = "Axis_X";
    });
    var AxisMaterialY = new THREE.MeshPhongMaterial({
        color: 0x96A2A3,
        shininess: 0
    });

    loader.load("../Content/DesignTools/Control/Axis_Y.stl", function (geometry) {
        Axis = new THREE.Mesh(geometry, AxisMaterialY);
        Axis.position.set(0, 0, 0);
        Axis.traverse(function (child) {
            if (child instanceof THREE.Mesh) {
                child.visible = true;
            }
        });
        Axis.scale.set(0.05, 0.05, 0.05);
        //Valdosta.alphaTest: -1;
        Axis.rotation.x = 1.5 * Math.PI;
        //Valdosta.opacity= 0,
        Axis.transparent = true,
            Axis.callback = function () { console.log(this.name); }
        scene.add(Axis);
        Axis.name = "Axis_Y";
    });
    var AxisMaterialZ = new THREE.MeshPhongMaterial({
        color: 0x96A2A3,
        shininess: 0
    });
    loader.load("../Content/DesignTools/Control/Axis_Z.stl", function (geometry) {
        Axis = new THREE.Mesh(geometry, AxisMaterialZ);
        Axis.position.set(0, 0, 0);
        Axis.traverse(function (child) {
            if (child instanceof THREE.Mesh) {
                child.visible = true;
            }
        });
        Axis.scale.set(0.05, 0.05, 0.05);
        //Valdosta.alphaTest: -1;
        Axis.rotation.x = 1.5 * Math.PI;
        //Valdosta.opacity= 0,
        Axis.transparent = true,
            Axis.callback = function () { console.log(this.name); }
        scene.add(Axis);
        Axis.name = "Axis_Z";
    });
}



