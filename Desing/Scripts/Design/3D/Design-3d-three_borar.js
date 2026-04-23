
function InitDesaint3d(_estimationId, _estimationName, _sizeX, _sizeY, _camera, _linkEnvironment, _LinkIdEnvironmentOrbitValue) {
    createMaterial()
    createEscena();
    createRender();
    createCamara()
    createAxes()
    createLight()
    //Maxi
    createEnvironment(_linkEnvironment, _LinkIdEnvironmentOrbitValue)
    createRay()
    renderer.setSize(pageWidth, pageHeight);
    //rendererMenu.setSize("500px", "500px");
    animate();
    DrawDesign();
}
function animate() {
    let objectSecction = new THREE.Group();
    scene.add(objectSecction);
    requestAnimationFrame(animate);
    //renderer.render(sceneMenu, camera);
    renderer.clearDepth();
    renderer.render(scene, camera);
    var dir = new THREE.Vector3();
    var sph = new THREE.Spherical();

    camera.getWorldDirection(dir);
    sph.setFromVector3(dir);
    compass.style.transform = `rotate(${THREE.Math.radToDeg(sph.theta) - 180}deg)`;


    viewCubeMatrix.extractRotation(camera.matrixWorldInverse);
    TWEEN.update();
    viewCube.style.transform = `translateZ(-300px) ${getCameraCSSMatrix(viewCubeMatrix)}`;
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();

    renderer.setSize(windowInnerWidth, windowInnerHeight);
    //rendererMenu.setSize(500, 500);

    activateViewCubeButtons();
    showViewCube();
    sedviewcube();
    viewCubeMatrix.extractRotation(camera.matrixWorldInverse);
    function showViewCube() {
        $(viewCube).show();
    }
    function sedviewcube() {
        showViewCube();
    }
    function activateViewCubeButtons() {
        var r = 255;
        var g = 195;
        var b = 0;
        const base = ".view-";

        $('#CameraHome').on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('camera3DView');
        });

        $('#viewCubeFront').on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('front');
        });

        $('#viewCubeBack').on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('back');
        });

        $("#viewCubeTop").on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('top');
        });

        $("#viewCubeBottom").on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('bottom');
        });

        $("#viewCubeLeft").on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('left');
        });

        $("#viewCubeRight").on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView('right');
        });

        $("[name='viewCubeView']").on('mouseenter', function (e) {
            let view = $(e.target).data('view');
            let object = base.concat(view);
            $(object).css("background-color", "rgba(" + r + "," + g + "," + b + ", 0.95)");
            $(object).css("cursor", "pointer");
        }).on('mouseleave', function (e) {
            let view = $(e.target).data('view');
            let object = base.concat(view);
            $(object).css("background-color", "");
        }).on('click', function (e) {
            e.stopPropagation();
            e.preventDefault();
            setView($(e.target).data('view'));
        });
    }
    function getCameraCSSMatrix(matrix) {
        var elements = matrix.elements;
        return 'matrix3d(' +
            epsilon(elements[0]) + ',' +
            epsilon(-elements[1]) + ',' +
            epsilon(elements[2]) + ',' +
            epsilon(elements[3]) + ',' +
            epsilon(elements[4]) + ',' +
            epsilon(-elements[5]) + ',' +
            epsilon(elements[6]) + ',' +
            epsilon(elements[7]) + ',' +
            epsilon(elements[8]) + ',' +
            epsilon(-elements[9]) + ',' +
            epsilon(elements[10]) + ',' +
            epsilon(elements[11]) + ',' +
            epsilon(elements[12]) + ',' +
            epsilon(-elements[13]) + ',' +
            epsilon(elements[14]) + ',' +
            epsilon(elements[15]) +
            ')';
    }
    function epsilon(value) {
        return Math.abs(value) < 1e-10 ? 0 : value;
    }
    function setView(view) {
        var camSettings = {
            near: 10,
            far: 30000,
            perspectiveFov: 60,
            orthographicScale: 3,
            orthographicZoom: 0.4,
            base2DPosition: 500,
            base3DPosition: 1100
        };
        var cameraTopPosition = new Position(x = 0, y = camSettings.base2DPosition * 10, z = 0);
        var cameraBottomPosition = new Position(x = 0, y = -camSettings.base2DPosition * 10, z = 0);
        var cameraLeftPosition = new Position(x = -camSettings.base2DPosition * 10, y = camSettings.base2DPosition, z = 0);
        var cameraRightPosition = new Position(x = camSettings.base2DPosition * 10, y = camSettings.base2DPosition, z = 0);
        var cameraFrontPosition = new Position(x = 0, y = camSettings.base2DPosition, z = camSettings.base2DPosition * 10);
        var cameraBackPosition = new Position(x = 0, y = camSettings.base2DPosition, z = -camSettings.base2DPosition * 10);
        var camera3DFrontTopLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DFrontTopRightPosition = new Position(x = camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DTopBackLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DTopBackRightPosition = new Position(x = camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DFrontBottomLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DFrontBottomRightPosition = new Position(x = camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DBottomBackLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DBottomBackRightPosition = new Position(x = camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DFrontLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = camSettings.base2DPosition, z = camSettings.base3DPosition * 2);
        var camera3DBackLeftPosition = new Position(x = -camSettings.base3DPosition * 2, y = camSettings.base2DPosition, z = -camSettings.base3DPosition * 2);
        var camera3DBackRightPosition = new Position(x = camSettings.base3DPosition * 2, y = camSettings.base2DPosition, z = -camSettings.base3DPosition * 2);
        var camera3DFrontRightPosition = new Position(x = camSettings.base3DPosition * 2, y = camSettings.base2DPosition, z = camSettings.base3DPosition * 2);
        var camera3DFrontTopPosition = new Position(x = 0, y = camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DFrontBottomPosition = new Position(x = 0, y = -camSettings.base2DPosition * 10, z = camSettings.base3DPosition * 2);
        var camera3DLeftTopPosition = new Position(x = -camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = 0);
        var camera3DRightTopPosition = new Position(x = camSettings.base3DPosition * 2, y = camSettings.base2DPosition * 10, z = 0);
        var camera3DTopBackPosition = new Position(x = 0, y = camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DLeftBottomPosition = new Position(x = -camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = 0);
        var camera3DBackBottomPosition = new Position(x = 0, y = -camSettings.base2DPosition * 10, z = -camSettings.base3DPosition * 2);
        var camera3DRightBottomPosition = new Position(x = camSettings.base3DPosition * 2, y = -camSettings.base2DPosition * 10, z = 0);
        var camera3DView = new Position(x = -camSettings.base3DPosition * 0.81, y = camSettings.base2DPosition, z = camSettings.base3DPosition * 0.81);
        switch (view) {
            case 'camera3DView':
                camera.position.x = camera3DView.x;
                camera.position.y = camera3DView.y;
                camera.position.z = camera3DView.z;
                break;
            case 'left':
                camera.position.x = cameraLeftPosition.x;
                camera.position.y = cameraLeftPosition.y;
                camera.position.z = cameraLeftPosition.z;
                break;
            case 'right':
                camera.position.x = cameraRightPosition.x;
                camera.position.y = cameraRightPosition.y;
                camera.position.z = cameraRightPosition.z;
                break;
            case 'top':
                camera.position.x = cameraTopPosition.x;
                camera.position.y = cameraTopPosition.y;
                camera.position.z = cameraTopPosition.z;
                break;
            case 'bottom':
                camera.position.x = cameraBottomPosition.x;
                camera.position.y = cameraBottomPosition.y;
                camera.position.z = cameraBottomPosition.z;
                break;
            case 'front':
                camera.position.x = cameraFrontPosition.x;
                camera.position.y = cameraFrontPosition.y;
                camera.position.z = cameraFrontPosition.z;
                break;
            case 'back':
                camera.position.x = cameraBackPosition.x;
                camera.position.y = cameraBackPosition.y;
                camera.position.z = cameraBackPosition.z;
                break;
            case 'front-top-left':
                camera.position.x = camera3DFrontTopLeftPosition.x;
                camera.position.y = camera3DFrontTopLeftPosition.y;
                camera.position.z = camera3DFrontTopLeftPosition.z;
                break;
            case 'front-top-right':
                camera.position.x = camera3DFrontTopRightPosition.x;
                camera.position.y = camera3DFrontTopRightPosition.y;
                camera.position.z = camera3DFrontTopRightPosition.z;
                break
            case 'top-back-left':
                camera.position.x = camera3DTopBackLeftPosition.x;
                camera.position.y = camera3DTopBackLeftPosition.y;
                camera.position.z = camera3DTopBackLeftPosition.z;
                break;
            case 'top-back-right':
                camera.position.x = camera3DTopBackRightPosition.x;
                camera.position.y = camera3DTopBackRightPosition.y;
                camera.position.z = camera3DTopBackRightPosition.z;
                break;
            case 'front-bottom-left':
                camera.position.x = camera3DFrontBottomLeftPosition.x;
                camera.position.y = camera3DFrontBottomLeftPosition.y;
                camera.position.z = camera3DFrontBottomLeftPosition.z;
                break;
            case 'front-bottom-right':
                camera.position.x = camera3DFrontBottomRightPosition.x;
                camera.position.y = camera3DFrontBottomRightPosition.y;
                camera.position.z = camera3DFrontBottomRightPosition.z;
                break
            case 'bottom-back-left':
                camera.position.x = camera3DBottomBackLeftPosition.x;
                camera.position.y = camera3DBottomBackLeftPosition.y;
                camera.position.z = camera3DBottomBackLeftPosition.z;
                break;
            case 'bottom-back-right':
                camera.position.x = camera3DBottomBackRightPosition.x;
                camera.position.y = camera3DBottomBackRightPosition.y;
                camera.position.z = camera3DBottomBackRightPosition.z;
                break
            case 'front-top':
                camera.position.x = camera3DFrontTopPosition.x;
                camera.position.y = camera3DFrontTopPosition.y;
                camera.position.z = camera3DFrontTopPosition.z;
                break;
            case 'front-bottom':
                camera.position.x = camera3DFrontBottomPosition.x;
                camera.position.y = camera3DFrontBottomPosition.y;
                camera.position.z = camera3DFrontBottomPosition.z;
                break;
            case 'front-left':
                camera.position.x = camera3DFrontLeftPosition.x;
                camera.position.y = camera3DFrontLeftPosition.y;
                camera.position.z = camera3DFrontLeftPosition.z;
                break;
            case 'front-right':
                camera.position.x = camera3DFrontRightPosition.x;
                camera.position.y = camera3DFrontRightPosition.y;
                camera.position.z = camera3DFrontRightPosition.z;
                break;
            case 'top-back':
                camera.position.x = camera3DTopBackPosition.x;
                camera.position.y = camera3DTopBackPosition.y;
                camera.position.z = camera3DTopBackPosition.z;
                break;
            case 'back-bottom':
                camera.position.x = camera3DBackBottomPosition.x;
                camera.position.y = camera3DBackBottomPosition.y;
                camera.position.z = camera3DBackBottomPosition.z;
                break;
            case 'back-right':
                camera.position.x = camera3DBackRightPosition.x;
                camera.position.y = camera3DBackRightPosition.y;
                camera.position.z = camera3DBackRightPosition.z;
                break;
            case 'back-left':
                camera.position.x = camera3DBackLeftPosition.x;
                camera.position.y = camera3DBackLeftPosition.y;
                camera.position.z = camera3DBackLeftPosition.z;
                break;
            case 'left-top':
                camera.position.x = camera3DLeftTopPosition.x;
                camera.position.y = camera3DLeftTopPosition.y;
                camera.position.z = camera3DLeftTopPosition.z;
                break;
            case 'right-top':
                camera.position.x = camera3DRightTopPosition.x;
                camera.position.y = camera3DRightTopPosition.y;
                camera.position.z = camera3DRightTopPosition.z;
                break;
            case 'left-bottom':
                camera.position.x = camera3DLeftBottomPosition.x;
                camera.position.y = camera3DLeftBottomPosition.y;
                camera.position.z = camera3DLeftBottomPosition.z;
                break;
            case 'right-bottom':
                camera.position.x = camera3DRightBottomPosition.x;
                camera.position.y = camera3DRightBottomPosition.y;
                camera.position.z = camera3DRightBottomPosition.z;
                break;
        }

        camera.updateProjectionMatrix();
        camera.controls.target.x = cameraDefaultTarget.x;
        camera.controls.target.y = cameraDefaultTarget.y;
        camera.controls.target.z = cameraDefaultTarget.z;
        camera.controls.target.x = 0;
        camera.controls.target.y = 0;
        camera.controls.target.z = 0;
        camera.controls.update();
        //tweenCamera(
        //    targetPosition,
        //    duration,
        //    cameraTypeId === 1 ? camera : orthographicCamera.camera,
        //    cameraTypeId === 1 ? controls : orthographicCamera.controls,
        //    cameraTypeId
        //);
    }

}
//Pasar a clase formato
function CountDecimals(value) {
    if (Math.floor(value) === value) return 0;
    return value.toString().split(".")[1].length || 0;
}
function FormatValueDesaint(value, precision) {
    return numeral(value).format(GetPrecisionFormatStringDesaint(precision));
}
//Pasar a clase formato
function GetPrecisionFormatStringDesaint(precision) {
    var formatString = '0,0';
    if (precision === 0) {
        return formatString;
    }
    var zero = "0";
    for (var i = 1; i < precision; i++) {
        zero = zero + "0";
    }
    return '0[.]' + zero;
}
function createMaterial() {
    materialDimWall = new THREE.MeshLambertMaterial({ color: 0x000000 });
    materialDim = new THREE.MeshLambertMaterial({ color: 0x000000 });
    if (LinkEnvironment === 9) {
        materialDim = new THREE.MeshLambertMaterial({ color: 0xFFFFFF });
        materialDimWall = new THREE.MeshLambertMaterial({ color: 0xFFFFFF });
    }
    var materialUnion1 = new THREE.MeshLambertMaterial({ color: 0x839192 });
    materialBase = new THREE.MeshLambertMaterial({ color: 0xefb608, opacity: 1, transparent: true });
    materialIcon = new THREE.MeshLambertMaterial({ color: 0xE1E1EB });
    materialBaseShow = new THREE.MeshBasicMaterial({ color: 0xefb608, opacity: 0.5, transparent: true });
    SelectMaterial = new THREE.MeshLambertMaterial({ color: 0x34DBDB });
    //Conexiones 
    SelectMaterialConexion_1 = new THREE.MeshLambertMaterial({ color: 0xFF5733 });
    SelectMaterialConexion_2 = new THREE.MeshLambertMaterial({ color: 0x5BFF33 });
    //materialEsq = new THREE.MeshLambertMaterial({ color: 0x3498DB });
    //materialSup = new THREE.MeshLambertMaterial({ color: 0x839192 });
    materialEsq = new THREE.MeshLambertMaterial({ color: 0x839192 });
    materialSup = new THREE.MeshLambertMaterial({ color: 0x839192 });
    materialWallAct = new THREE.MeshLambertMaterial({ color: 0xefb608 });
    materialWall = new THREE.MeshLambertMaterial({ color: 0x839192 });
    MaterialUnSelectIcon = new THREE.MeshBasicMaterial({ color: 0xffe000, opacity: 0.5, transparent: true });
    MaterialSelectIcon = new THREE.MeshBasicMaterial({ color: 0x1248EC, opacity: 1, transparent: true });

    materialGrill = new THREE.MeshLambertMaterial({ color: 0x0AA0F7 });

    materialGrillAct = new THREE.MeshLambertMaterial({ color: 0xF7D00A });


    //Materiales
}

function createEscena() {
    scene = new THREE.Scene();
    //sceneMenu = new THREE.Scene();
    viewCube = document.querySelector('.viewCube');
    viewCubeMatrix = new THREE.Matrix4();
}
function createRender() {
    renderer = new THREE.WebGLRenderer();
    //rendererMenu = new THREE.WebGLRenderer();
    renderer.shadowMap.enabled = true;
    //renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    document.getElementById('containerDesign').appendChild(renderer.domElement);
    //    document.getElementById('containerDesignMenu').appendChild(renderer.domElement);
}
function createCamara() {
    camera = new THREE.PerspectiveCamera(40, pageWidth / pageHeight, 1, 10000);
    camera.position.set(cameraDefaultPosition.x, cameraDefaultPosition.y, cameraDefaultPosition.z);
    controls = createControlsForCamera(camera, 1);
    function createControlsForCamera(camObject, camType) {
        control = new THREE.OrbitControls(camObject);
        control.target = new THREE.Vector3(
        cameraDefaultTarget.x,
        cameraDefaultTarget.y,
        cameraDefaultTarget.z);
        control.enabled = true;
        control.enableZoom = true;
        control.enablePan = true;
        control.enableRotate = true;
        control.enableDamping = false;
        control.autoRotate = false;
        control.screenSpacePanning = true;
        control.keyPanSpeed = currentPanSpeed;
        control.rotateSpeed = currentZoomSpeed;
        control.zoomSpeed = currentZoomSpeed;
        control.panSpeed = currentZoomSpeed;
        return control;
    }
    camera.position.set(cameraDefaultPosition.x, cameraDefaultPosition.y, cameraDefaultPosition.z);
    camera.controls = controls;
    camera.controls.update();
    camera.controls.saveState();
    perspectiveCamera = camera;
}
function createAxes() {
    // axesHelper
    var arrowPos = new THREE.Vector3(0, 0, 0);
    scene.add(new THREE.ArrowHelper(new THREE.Vector3(1, 0, 0), arrowPos, 160, 0xAEB6BF, 20, 10));
    scene.add(new THREE.ArrowHelper(new THREE.Vector3(0, 1, 0), arrowPos, 160, 0xAEB6BF, 20, 10));
    scene.add(new THREE.ArrowHelper(new THREE.Vector3(0, 0, 1), arrowPos, 160, 0xAEB6BF, 20, 10));
}
function createEnvironment(linkEnvironment, LinkIdEnvironmentOrbitValue) {
    document.getElementById("IdEnvironmentValue").value = linkEnvironment;
    document.getElementById("IdEnvironmentOrbitValue").value = LinkIdEnvironmentOrbitValue;
    ///infin
    let Max = 9;
    if (Max === 9) {
        sky = new THREE.Sky();
        sky.scale.setScalar(450000);
        var effectController = {
            turbidity: 15,
            rayleigh: 2,
            mieCoefficient: 0.005,
            mieDirectionalG: 2,
            luminance: 1.05,
            inclination: 0.49, // elevation / inclination
            azimuth: 0.25, // Facing front,
            sun: !true
        };
        var uniforms = sky.material.uniforms;
        uniforms["turbidity"].value = effectController.turbidity;
        uniforms["rayleigh"].value = effectController.rayleigh;
        uniforms["luminance"].value = effectController.luminance;
        uniforms["mieCoefficient"].value = effectController.mieCoefficient;
        uniforms["mieDirectionalG"].value = effectController.mieDirectionalG;
        uniforms["sunPosition"].value.set(10000, 10000, 10000);

        var grid = null;
        if (linkEnvironment === 2) {
            grid = new THREE.InfiniteGridHelper(25, 100, new THREE.Color(0xFFFFFF))
        }
        if (linkEnvironment === 7) {
            grid = new THREE.InfiniteGridHelper(25, 100, new THREE.Color(0x3498DB))
        }
        if (linkEnvironment === 8) {
            grid = new THREE.InfiniteGridHelper(25, 100, new THREE.Color(0xDDD4D4))
        }
        scene.add(grid);
        ground = new THREE.Mesh(
            new THREE.PlaneGeometry(9, 9, 1, 1),
            new THREE.ShadowMaterial({ color: 0x58D68D, opacity: 0.25, side: THREE.DoubleSide })
        );
        ground.rotation.x = - Math.PI / 2; // rotates X/Y to X/Z
        ground.position.y = - 1;
        ground.receiveShadow = true;
        if (linkEnvironment === 2) {
            scene.add(sky);
        }
        if (linkEnvironment === 7) {
            scene.background = new THREE.Color(0xF3F7F8);
        }
        if (linkEnvironment === 8) {
            scene.background = new THREE.Color(0xF3F7F8);
        }
        if (linkEnvironment === 9) {
            scene.background = new THREE.Color(0x434444);
        }
        ground.name = "AtenkoGround";
        scene.add(ground);
        var groundMenu = ground;

        //sceneMenu.add(ground2);
        //sceneMenu.add(sky);
    }
    else {
        let j = 1;
    }
}
function createLight() {
    // lights
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.5)
    scene.add(ambientLight)
    const light = new THREE.DirectionalLight()
    light.position.set(2.5, 2, 2)
    light.castShadow = true
    light.shadow.mapSize.width = 512
    light.shadow.mapSize.height = 512
    light.shadow.camera.near = 0.5
    light.shadow.camera.far = 100
    scene.add(light)
    const DirectionalLight = new THREE.DirectionalLight(0xffffff, 1);
    DirectionalLight.position.set(0, 1, 0); //default; light shining from top
    DirectionalLight.castShadow = true; // default false
    //scene.add(DirectionalLight);

    //Set up shadow properties for the light
    DirectionalLight.shadow.mapSize.width = 512; // default
    DirectionalLight.shadow.mapSize.height = 512; // default
    DirectionalLight.shadow.camera.near = 0.5; // default
    DirectionalLight.shadow.camera.far = 500; // default

    const helper = new THREE.DirectionalLightHelper(DirectionalLight)
    scene.add(helper)
    // light controls
}
function createRay() {
    //Ray
    mouse = new THREE.Vector2();
    raycaster = new THREE.Raycaster();
    const geometry = new THREE.PlaneBufferGeometry(10000, 10000);
    geometry.rotateX(- Math.PI / 2);
    plane = new THREE.Mesh(geometry, new THREE.MeshBasicMaterial({ visible: false }));
    scene.add(plane);
    objects.push(plane);


    const textDiv = document.createElement('div');
    textDiv.className = 'label';
    textDiv.textContent = "hehe";
    textDiv.style.marginTop = '-1em';
    const textLabel = new CSS2DObject(textDiv);
    textLabel.position.set(0, 2, 0);
    textLabel.scale.x = 10;
    textLabel.scale.y = 10;
    textLabel.scale.z = 10;
    scene.add(textLabel);

    //const material = new THREE.MeshFaceMaterial([
    //    new THREE.MeshPhongMaterial({
    //        color: 0x000000,
    //        flatShading: true,
    //    }), // front
    //    new THREE.MeshPhongMaterial({
    //        color: 0x000000
    //    }), // side
    //])

    //const loaderFonds = new THREE.FontLoader()
    //loaderFonds.load("../../Content/DesignTools/Fonts/optimer_regular.typeface.json", function (font) {
    //   geometryFond = new THREE.TextGeometry('Cota aquí', {
    //        font: font,
    //        size: 12,
    //        height: 0.2,
    //        curveSegments: 12,
    //        bevelEnabled: false,
    //        bevelThickness: 0.5,
    //        bevelSize: 0.3,
    //        bevelOffset: 0,
    //        bevelSegments: 5,
    //  })
    //    //geometryFond.geometry.parameters.text = "Hola Hola carruser deportivo";
    //    meshFonds = new THREE.Mesh(geometryFond, material)
    //    meshFonds.name = 'text'
    //    meshFonds.rotation.x = -0.5 * Math.PI;
    //    scene.add(meshFonds);
    //})

    //loaderFonds.load("../../Content/DesignTools/Fonts/optimer_regular.typeface.json", function (font)
    //{
    //    geometryFond.dispose();
    //    geometryFond = new THREE.TextGeometry('Hola Hola', {
    //        font: font,
    //        size: 12,
    //        height: 0.2,
    //        curveSegments: 12,
    //        bevelEnabled: false,
    //        bevelThickness: 0.5,
    //        bevelSize: 0.3,
    //        bevelOffset: 0,
    //        bevelSegments: 5,
    //    })
    //    meshFonds.geometry.dispose();
    //    scene.remove(meshFonds);

    //    meshFonds = new THREE.Mesh(geometryFond, material);
    //    scene.add(meshFonds);
    //})

    rollOverGeo = new THREE.BoxGeometry(25, 0, 25);
    rollOverMaterial = new THREE.MeshBasicMaterial({ color: 0xffe000, opacity: 0.5, transparent: true });
    rollOverMesh = new THREE.Mesh(rollOverGeo, rollOverMaterial);
    rollOverMesh.position.x = 0;
    rollOverMesh.position.z = 0;
    rollOverMesh.position.y = 0;
    rollOverMesh.visible = false;
    scene.add(rollOverMesh);


    var loaderWall_0 = new THREE.STLLoader();
    var ElementWall_0 = "../../Content/DesignTools/Control/Wall_0.stl";
    loaderWall_0.load(ElementWall_0, function (geometry) {
        meshWall_0 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshWall_0.position.set(0, 0, 0);
        meshWall_0.rotation.x = -0.5 * Math.PI;
        meshWall_0.name = "meshWall_0";
        meshWall_0.rotation.z = 0;
        meshWall_0.scale.set(1, 1, 1);
        //meshWall_0.scale.x = 0.03;
        //meshWall_0.scale.y = 0.03;
        //meshWall_0.scale.z = 0.027;
        meshWall_0.visible = false;
        scene.add(meshWall_0);
    });


    //90
    var loaderWall_90 = new THREE.STLLoader();
    var ElementWall_90 = "../../Content/DesignTools/Control/Wall_90.stl";
    loaderWall_90.load(ElementWall_90, function (geometry) {
        meshWall_90 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshWall_90.position.set(0, 0, 0);
        meshWall_90.rotation.x = -0.5 * Math.PI;
        meshWall_90.name = "meshWall_90";
        meshWall_90.rotation.z = 0;
        meshWall_90.scale.set(1, 1, 1);
        meshWall_90.visible = false;
        scene.add(meshWall_90);
    });

    var loaderEsq20Conexion = new THREE.STLLoader();
    var Element20Conexion = "../../Content/DesignTools/Control/Esq_20_Conexion.stl";
    loaderEsq20Conexion.load(Element20Conexion, function (geometry) {
        meshEsq20Conexion = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq20Conexion.position.set(0, 0, 0);
        meshEsq20Conexion.rotation.x = -0.5 * Math.PI;
        meshEsq20Conexion.name = "Esq20Conexion";
        meshEsq20Conexion.rotation.z = 0;
        meshEsq20Conexion.scale.set(1, 1, 1);
        meshEsq20Conexion.scale.x = 0.03;
        meshEsq20Conexion.scale.y = 0.03;
        meshEsq20Conexion.scale.z = 0.027;
        meshEsq20Conexion.visible = false;
        scene.add(meshEsq20Conexion);
    });




    var loaderEsqXConexion = new THREE.STLLoader();
    var ElementXConexion = "../../Content/DesignTools/Control/Esq_X_Conexion.stl";
    loaderEsqXConexion.load(ElementXConexion, function (geometry) {
        meshEsqXConexion = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsqXConexion.position.set(0, 0, 0);
        meshEsqXConexion.rotation.x = -0.5 * Math.PI;
        meshEsqXConexion.name = "EsqXConexion";
        meshEsqXConexion.rotation.z = 0;
        meshEsqXConexion.scale.set(1, 1, 1);
        meshEsqXConexion.scale.x = 0.03;
        meshEsqXConexion.scale.y = 0.03;
        meshEsqXConexion.scale.z = 0.027;
        meshEsqXConexion.visible = false;
        scene.add(meshEsqXConexion);
    });


    var loaderEsq20 = new THREE.STLLoader();
    var Element20 = "../../Content/DesignTools/Control/Esq20.stl";
    loaderEsq20.load(Element20, function (geometry) {
        meshEsq20 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq20.position.set(0, 0, 0);
        meshEsq20.rotation.x = -0.5 * Math.PI;
        meshEsq20.name = "Esq20";
        meshEsq20.rotation.z = 0;
        meshEsq20.scale.set(1, 1, 1);
        meshEsq20.scale.x = 0.03;
        meshEsq20.scale.y = 0.03;
        meshEsq20.scale.z = 0.027;
        meshEsq20.visible = false;
        scene.add(meshEsq20);
    });


    var loaderEsq40 = new THREE.STLLoader();
    var Element40 = "../../Content/DesignTools/Control/Esq40.stl";
    loaderEsq40.load(Element40, function (geometry) {
        meshEsq40 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq40.position.set(0, 0, 0);
        meshEsq40.rotation.x = -0.5 * Math.PI;
        meshEsq40.name = "Esq40";
        meshEsq40.rotation.z = 0;
        meshEsq40.scale.set(1, 1, 1);
        meshEsq40.scale.x = 0.03;
        meshEsq40.scale.y = 0.03;
        meshEsq40.scale.z = 0.027;
        meshEsq40.visible = false;
        scene.add(meshEsq40);
    });

    var loaderEsq60 = new THREE.STLLoader();
    var Element60 = "../../Content/DesignTools/Control/Esq60.stl";
    loaderEsq60.load(Element60, function (geometry) {
        meshEsq60 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq60.position.set(0, 0, 0);
        meshEsq60.rotation.x = -0.5 * Math.PI;
        meshEsq60.name = "Esq60";
        meshEsq60.rotation.z = 0;
        meshEsq60.scale.set(1, 1, 1);
        meshEsq60.scale.x = 0.03;
        meshEsq60.scale.y = 0.03;
        meshEsq60.scale.z = 0.027;
        meshEsq60.visible = false;
        scene.add(meshEsq60);
    });

    var loaderEsq80 = new THREE.STLLoader();
    var Element80 = "../../Content/DesignTools/Control/Esq80.stl";
    loaderEsq80.load(Element80, function (geometry) {
        meshEsq80 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq80.position.set(0, 0, 0);
        meshEsq80.rotation.x = -0.5 * Math.PI;
        meshEsq80.name = "Esq80";
        meshEsq80.rotation.z = 0;
        meshEsq80.scale.set(1, 1, 1);
        meshEsq80.scale.x = 0.03;
        meshEsq80.scale.y = 0.03;
        meshEsq80.scale.z = 0.027;
        meshEsq80.visible = false;
        scene.add(meshEsq80);
    });


    var loaderNucleo = new THREE.STLLoader();
    var ElementNucleo = "../../Content/DesignTools/Control/Nucleo.stl";
    loaderNucleo.load(ElementNucleo, function (geometry) {
        meshNucleo = new THREE.Mesh(geometry, rollOverMaterial);
        meshNucleo.position.set(0, 0, 0);
        meshNucleo.rotation.x = -0.5 * Math.PI;
        meshNucleo.name = "Nucleo";
        meshNucleo.rotation.z = 0;
        meshNucleo.scale.set(1, 1, 1);
        meshNucleo.scale.x = 15;
        meshNucleo.scale.y = 15;
        meshNucleo.scale.z = 5;
        meshNucleo.visible = false;
        scene.add(meshNucleo);
    });


    var loaderEsq10 = new THREE.STLLoader();
    var Element10 = "../../Content/DesignTools/Control/Esq10.stl";
    loaderEsq10.load(Element10, function (geometry) {
        meshEsq10 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq10.position.set(0, 0, 0);
        meshEsq10.rotation.x = -0.5 * Math.PI;
        meshEsq10.name = "Esq10";
        meshEsq10.rotation.z = 0;
        meshEsq10.scale.set(1, 1, 1);
        meshEsq10.scale.x = 0.03;
        meshEsq10.scale.y = 0.03;
        meshEsq10.scale.z = 0.027;
        meshEsq10.visible = false;
        scene.add(meshEsq10);
    });

    var loaderEsq30 = new THREE.STLLoader();
    var Element30 = "../../Content/DesignTools/Control/Esq30.stl";
    loaderEsq30.load(Element30, function (geometry) {
        meshEsq30 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq30.position.set(0, 0, 0);
        meshEsq30.rotation.x = -0.5 * Math.PI;
        meshEsq30.name = "Esq30";
        meshEsq30.rotation.z = 0;
        meshEsq30.scale.set(1, 1, 1);
        meshEsq30.scale.x = 0.03;
        meshEsq30.scale.y = 0.03;
        meshEsq30.scale.z = 0.027;
        meshEsq30.visible = false;
        scene.add(meshEsq30);
    });
    var loaderEsq50 = new THREE.STLLoader();
    var Element50 = "../../Content/DesignTools/Control/Esq50.stl";
    loaderEsq50.load(Element50, function (geometry) {
        meshEsq50 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq50.position.set(0, 0, 0);
        meshEsq50.rotation.x = -0.5 * Math.PI;
        meshEsq50.name = "Esq50";
        meshEsq50.rotation.z = 0;
        meshEsq50.scale.set(1, 1, 1);
        meshEsq50.scale.x = 0.03;
        meshEsq50.scale.y = 0.03;
        meshEsq50.scale.z = 0.027;
        meshEsq50.visible = false;
        scene.add(meshEsq50);
    });

    var loaderEsq70 = new THREE.STLLoader();
    var Element70 = "../../Content/DesignTools/Control/Esq70.stl";
    loaderEsq70.load(Element70, function (geometry) {
        meshEsq70 = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsq70.position.set(0, 0, 0);
        meshEsq70.rotation.x = -0.5 * Math.PI;
        meshEsq70.name = "Esq70";
        meshEsq70.rotation.z = 0;
        meshEsq70.scale.set(1, 1, 1);
        meshEsq70.scale.x = 0.03;
        meshEsq70.scale.y = 0.03;
        meshEsq70.scale.z = 0.027;
        meshEsq70.visible = false;
        scene.add(meshEsq70);
    });

    var loaderEsqX = new THREE.STLLoader();
    var ElementX = "../../Content/DesignTools/Control/EsqX.stl";
    loaderEsqX.load(ElementX, function (geometry) {
        meshEsqX = new THREE.Mesh(geometry, rollOverMaterial);
        meshEsqX.position.set(0, 0, 0);
        meshEsqX.rotation.x = -0.5 * Math.PI;
        meshEsqX.name = "EsqX";
        meshEsqX.rotation.z = 0;
        meshEsqX.scale.set(1, 1, 1);
        meshEsqX.scale.x = 0.03;
        meshEsqX.scale.y = 0.03;
        meshEsqX.scale.z = 0.027;
        meshEsqX.visible = false;
        scene.add(meshEsqX);
    });




    var loaderWallParall = new THREE.STLLoader();
    var ElementWallParall = "../../Content/DesignTools/Control/Wall_0.stl";
    loaderWallParall.load(ElementWallParall, function (geometry) {
        meshParall = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshParall.position.set(0, 0, 0);
        meshParall.rotation.x = -0.5 * Math.PI;
        meshParall.name = "meshWall_0";
        meshParall.rotation.z = 0;
        meshParall.scale.set(1, 1, 1);
        meshParall.visible = false;
        scene.add(meshParall);
    });

    var loaderWallParall90 = new THREE.STLLoader();
    var ElementWallParall90 = "../../Content/DesignTools/Control/Wall_90.stl";
    loaderWallParall90.load(ElementWallParall90, function (geometry) {
        meshParall90 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshParall90.position.set(0, 0, 0);
        meshParall90.rotation.x = -0.5 * Math.PI;
        meshParall90.name = "meshWall_90";
        meshParall90.rotation.z = 0;
        meshParall90.scale.set(1, 1, 1);
        meshParall90.visible = false;
        scene.add(meshParall90);
    });




    ///Add Dim Veertical
    geometryConoTop = new THREE.ConeGeometry(5, 20, 32);
    ConeTop = new THREE.Mesh(geometryConoTop, materialDimWall);
    ConeTop.rotation.x = - Math.PI * 1.5;
    ConeTop.rotation.z = - Math.PI;
    ConeTop.position.x = 0;
    ConeTop.position.y = 0;
    ConeTop.position.z = 10;
    ConeTop.name = "ConoTop";
    ConeTop.visible = false;
    scene.add(ConeTop);


    geometryConoDown = new THREE.ConeGeometry(5, 20, 32);
    ConeDown = new THREE.Mesh(geometryConoDown, materialDimWall);
    ConeDown.rotation.x = Math.PI * 1.5;
    ConeDown.rotation.z = Math.PI;
    ConeDown.position.x = 0;
    ConeDown.position.y = 0;
    ConeDown.position.z = -10;
    ConeDown.name = "ConoDown";
    ConeDown.visible = false;
    scene.add(ConeDown);


    pointsDimTop.push(new THREE.Vector3(0, 0, 0));
    pointsDimTop.push(new THREE.Vector3(-210, 0, 0));
    LineDimTop = new THREE.BufferGeometry().setFromPoints(pointsDimTop);
    LineDimTop = new THREE.Line(LineDimTop, materialDimWall);
    LineDimTop.name = "DimLine_Top";
    LineDimTop.visible = false;
    scene.add(LineDimTop);


    pointsDimDown.push(new THREE.Vector3(0, 0, 0));
    pointsDimDown.push(new THREE.Vector3(-210, 0, 0));
    LineDimDown = new THREE.BufferGeometry().setFromPoints(pointsDimDown);
    LineDimDown = new THREE.Line(LineDimDown, materialDimWall);
    LineDimDown.name = "DimLine_Down";
    LineDimDown.visible = false;
    scene.add(LineDimDown);


    var loaderLineDinV = new THREE.STLLoader();
    loaderLineDinV.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        LineDimTopToRDown = new THREE.Mesh(geometry, materialDimWall);
        LineDimTopToRDown.position.set(0, 0, 0);
        LineDimTopToRDown.rotation.x = -0.5 * Math.PI;
        LineDimTopToRDown.name = "";
        /*  LineDimTopToRDown.rotation.z = Math.PI;*/
        LineDimTopToRDown.scale.set(1, 1, 1);
        LineDimTopToRDown.scale.x = 0.0005;
        LineDimTopToRDown.scale.y = 1;
        LineDimTopToRDown.scale.z = 0.0005;
        LineDimDown.name = "LineDimTopToRDown";
        LineDimTopToRDown.visible = false;
        scene.add(LineDimTopToRDown);
    });





    ///Add Dim Horizontal
    geometryConoLeft = new THREE.ConeGeometry(5, 20, 32);
    ConeLeft = new THREE.Mesh(geometryConoLeft, materialDimWall);
    // 0
    /*Fistcone.rotation.x = Math.PI * 0.5;*/
    //180
    ConeLeft.rotation.x = Math.PI * 1.5;
    ConeLeft.rotation.z = Math.PI * -1.5;
    ConeLeft.position.x = 10;
    ConeLeft.position.y = 0;
    ConeLeft.position.z = 0;
    ConeLeft.name = "ConoLeft";
    ConeLeft.visible = false;
    scene.add(ConeLeft);

    geometryConoRight = new THREE.ConeGeometry(5, 20, 32);
    ConeRight = new THREE.Mesh(geometryConoRight, materialDimWall);
    ConeRight.rotation.x = Math.PI * 1.5;
    ConeRight.rotation.z = Math.PI * 1.5;
    ConeRight.position.x = -10;
    ConeRight.position.y = 0;
    ConeRight.position.z = 0;
    ConeRight.name = "ConoRight";
    ConeRight.visible = false;
    scene.add(ConeRight);


    pointsDimLef.push(new THREE.Vector3(0, 0, 0));
    pointsDimLef.push(new THREE.Vector3(0, 0, 210));
    LineLeft = new THREE.BufferGeometry().setFromPoints(pointsDimLef);
    LineDimLef = new THREE.Line(LineLeft, materialDimWall);
    LineDimLef.name = "DimLine_Left";
    LineDimLef.visible = false;
    scene.add(LineDimLef);

    pointsDimRight.push(new THREE.Vector3(0, 0, 0));
    pointsDimRight.push(new THREE.Vector3(0, 0, 210));
    LineRight = new THREE.BufferGeometry().setFromPoints(pointsDimRight);
    LineDimRight = new THREE.Line(LineRight, materialDimWall);
    LineDimRight.name = "DimLine_Right";
    LineDimRight.visible = false;
    scene.add(LineDimRight);

    var loaderLineDin = new THREE.STLLoader();
    loaderLineDin.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        LineRightToLeft = new THREE.Mesh(geometry, materialDimWall);
        LineRightToLeft.position.set(0, 0, 0);
        LineRightToLeft.rotation.x = -0.5 * Math.PI;
        LineRightToLeft.name = "";
        LineRightToLeft.rotation.z = Math.PI;
        LineRightToLeft.scale.set(1, 1, 1);
        LineRightToLeft.scale.x = 1;
        LineRightToLeft.scale.y = 0.0005;
        LineRightToLeft.scale.z = 0.0005;
        LineRightToLeft.visible = false;
        scene.add(LineRightToLeft);
    });

    //Control

    var loaderControl90 = new THREE.STLLoader();
    var Element90 = "../../Content/DesignTools/Control/IconMove.stl";
    loaderControl90.load(Element90, function (geometry) {
        meshControl_Move_90 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshControl_Move_90.position.set(0, 0, 0);
        meshControl_Move_90.rotation.x = -0.5 * Math.PI;
        meshControl_Move_90.rotation.z = 0;
        meshControl_Move_90.name = "Control_Move_90";
        meshControl_Move_90.scale.set(70, 40, 50);
        meshControl_Move_90.visible = false;
        scene.add(meshControl_Move_90);
    });

    var loaderControl270 = new THREE.STLLoader();
    var Element270 = "../../Content/DesignTools/Control/IconMove.stl";
    loaderControl270.load(Element270, function (geometry) {
        meshControl_Move_270 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshControl_Move_270.position.set(0, 0, 0);
        meshControl_Move_270.rotation.x = -0.5 * Math.PI;
        meshControl_Move_270.rotation.z = Math.PI;
        meshControl_Move_270.name = "Control_Move_270";
        meshControl_Move_270.scale.set(70, 40, 50);
        meshControl_Move_270.visible = false;
        scene.add(meshControl_Move_270);
    });


    var loaderControl180 = new THREE.STLLoader();
    var Element180 = "../../Content/DesignTools/Control/IconMove.stl";
    loaderControl180.load(Element180, function (geometry) {
        meshControl_Move_180 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshControl_Move_180.position.set(0, 0, 0);
        meshControl_Move_180.rotation.x = -0.5 * Math.PI;
        meshControl_Move_180.rotation.z = Math.PI * 0.5;
        meshControl_Move_180.name = "Control_Move_180";
        meshControl_Move_180.scale.set(70, 40, 50);
        meshControl_Move_180.visible = false;
        scene.add(meshControl_Move_180);
    });

    var loaderControl0 = new THREE.STLLoader();
    var Element0 = "../../Content/DesignTools/Control/IconMove.stl";
    loaderControl0.load(Element0, function (geometry) {
        meshControl_Move_0 = new THREE.Mesh(geometry, MaterialUnSelectIcon);
        meshControl_Move_0.position.set(0, 10, 0);
        meshControl_Move_0.rotation.x = 0.5 * Math.PI;
        meshControl_Move_0.rotation.z = Math.PI * 1.5;
        meshControl_Move_0.name = "Control_Move_0";
        meshControl_Move_0.scale.set(70, 40, 50);
        meshControl_Move_0.visible = false;
        scene.add(meshControl_Move_0);
    });


    //ControlDimText


    //const div = document.createElement('div');
    //div.id = "DivInputDim1"; 
    //div.style.width = '500px';
    //div.style.height = '500px';
    //div.style.backgroundColor = '#33FF3C';

    //const iframe = document.createElement('iframe');
    //iframe.style.width = '500px';
    //iframe.style.height = '500px';
    //iframe.style.border = '1px';
    //iframe.src = ['https://www.elpais.com'].join('');
    //div.appendChild(iframe);

    //const object = new THREE.CSS3DObject(div);
    //object.position.set(0, 0, 0);
    //object.name = "OBText";
    //object.scale.x = "300px";
    //object.scale.x = "300px";
    //scene.add(object);


    var radius = 1;
    var geom = new THREE.SphereGeometry(radius, 32, 16);
    geom.name = "DimAtos";
    var mat = new THREE.MeshBasicMaterial({ color: Math.random() * 0xFFFFFF, wireframe: true });
    _dim = new THREE.Mesh(geom, mat);
    NameTextDim = "Hola que tal";
    var canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    TextDim = canvas.getContext("2d");
    TextDim.font = "35pt Arial";
    TextDim.fillStyle = '#000000';
    if (LinkEnvironment === 9) { TextDim.fillStyle = '#FFFFFF'; }
    TextDim.textAlign = "center";
    TextDim.fillText(NameTextDim, size / 2, size / 3);
    var tex = new THREE.Texture(canvas);
    tex.needsUpdate = true;
    var spriteMat = new THREE.SpriteMaterial({ map: tex });
    var sprite = new THREE.Sprite(spriteMat);
    sprite.scale.set(100, 100, 1);
    sprite.position.x = 0;
    sprite.position.y = -10;
    sprite.position.z = 0;
    _dim.add(sprite);
    _dim.name = "Dim_DimAtos";
    _dim.visible = false;
    scene.add(_dim);

}