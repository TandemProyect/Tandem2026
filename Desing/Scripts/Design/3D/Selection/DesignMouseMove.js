document.addEventListener('mousemove', onclick, false);
$(document).mousemove(function (event) {

    SelectMaterial = new THREE.MeshLambertMaterial({ color: 0x34DBDB });
    EraseMaterial = new THREE.MeshLambertMaterial({ color: 0xFE0905 });
    if (IsFormArtive !== true) {
        if (AddDivDim === true && KeyActive === false) {
            var mousex = event.clientX;
            var mousey = event.clientY;
            if (ActionDbl === "Control_Move_270") {
                document.getElementById("DivInputDim").style.left = (mousex - 150) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 30) + 'px';
            }
            if (ActionDbl === "Control_Move_90") {
                document.getElementById("DivInputDim").style.left = (mousex - 150) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey + 30) + 'px';
            }
            if (ActionDbl === "Control_Move_180") {
                document.getElementById("DivInputDim").style.left = (mousex - 150) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 30) + 'px';
            }
            if (ActionDbl === "Control_Move_0") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Esq_60") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Esq_80") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Esq_X") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Parall") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Parall_90") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Esq_40") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            if (ActionDbl === "Control_Move_Esq_20") {
                document.getElementById("DivInputDim").style.left = (mousex + 50) + 'px';
                document.getElementById("DivInputDim").style.top = (mousey - 10) + 'px';
            }
            document.getElementById("InputDim").value = NameTextDim;
        }
        if (Edit_Wall === 0) {
            SelectMaterial = new THREE.MeshLambertMaterial({ color: 0x839192 });
        }
        if (InsertWall === 0) {
            event.preventDefault();
            if (mouse === null) {
                return;
            }
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                if (intersects[i].object.name === "") { continue; }
                {
                    if (intersects[i].object.name.substr(0, 6) === "Worker") {
                        //GetSceneListMaterial(scene.children);
                        renderer.domElement.style.cursor = 'pointer';
                        obWallMouseMove = intersects[i].object;
                        ActionDbl = "Worker";
                    }
                }
            }
        }
        //SeleccionarPanel
        if (InsertWall === 200) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                if (intersects[i].object.name === "") { continue; }
                {
                    if (intersects[i].object.name.substr(0, 10) === "Atk60_H0_2") {
                        if (ActionDbl === "Prop") {
                            var materialold = new THREE.MeshLambertMaterial({ color: 0xDCDBDA });
                            obWallMouseMove.material = materialold;
                            renderer.domElement.style.cursor = '';
                            obWallMouseMove = null;
                            ActionDbl = "";
                            obWallMouseMoveOldMaterial = null;
                        }
                        renderer.domElement.style.cursor = 'pointer';
                        obWallMouseMove = intersects[i].object;
                        obWallMouseMoveOldMaterial = obWallMouseMove.material;
                        ActionDbl = "Prop";
                        obWallMouseMove.material = EraseMaterial;
                    }
                    else {
                        if (ActionDbl === "Prop") {
                            var materialold = new THREE.MeshLambertMaterial({ color: 0xDCDBDA });
                            obWallMouseMove.material = materialold;
                            renderer.domElement.style.cursor = '';
                            obWallMouseMove = null;
                            ActionDbl = "";
                            obWallMouseMoveOldMaterial = null;
                        }
                    }
                }
            }
        }
        if (InsertWall === 102) {
            var left = 0;
            var top = 0;
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                raycaster.setFromCamera(mouse, camera);
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.type === "Mesh") {
                    /*UpdateControl();*/
                    if (intersects[i].object.name === "NaN") {
                        continue;
                    }
                    if (intersects[i].object.name === "Control_Move_0") {
                        if (obWall !== null) {
                            MouseMove_Control_0(intersects[i].object);
                        }
                    }
                    MouseMove_Control_180();
                    MouseMove_Control_90();
                    MouseMove_Control_270();
                    if (intersects[i].object.name === null) { continue; }
                    if (intersects[i].object.name === undefined) { continue; }
                    if (intersects[i].object.name.substr(0, 5) === "Pilar") {
                        if (document.getElementById("IdShowMuros").checked === true) {
                            return;
                        }

                        if (obWall !== null) {
                            if (obWall.name === intersects[i].object.name) {
                                continue;
                            }
                        }
                        if (obWallMouseMove !== null) {
                            obWallMouseMove.material = materialWall;
                        }
                        if (obWallMouseMoveSecontObject !== null) {
                            obWallMouseMoveSecontObject.material = materialWall;
                        }

                        renderer.domElement.style.cursor = 'pointer';
                        ActionDbl = "Pilar";
                        obWallMouseMove = null;
                        obWallMouseMove = intersects[i].object;
                        intersects[i].object.material = SelectMaterial;
                        UpdateControl();
                    }
                    if (intersects[i].object.name === "ConeLeft") {
                        var j1 = 1;
                    }
                    if (intersects[i].object.name.substr(0, 9) === "Wall_R000") {
                        if (document.getElementById("IdShowMuros").checked === true)
                        {
                            return;
                        }
                        if (OtherCornerObject !== null) {
                            if (OtherCornerObject === undefined) { continue };
                            OtherCornerObject.material = materialWall;
                        }
                        if (ActionWizard === 500) {
                            ParallePositionX = intersects[i].object.position.x;
                            ObParalle = intersects[i].object;
                        }
                        meshEsq20Conexion.visible = false;
                        meshEsqXConexion.visible = false;
                        IsMiddleOfConnecting = false;
                        SelectMaterial = new THREE.MeshLambertMaterial({ color: 0x34DBDB });
                        if (ActionDbl === "Control_Move_180") {
                            AddDivDim = false;
                            document.getElementById("DivInputDim").style.display = "inline";
                        }
                        if (ActionDbl === "Control_Move_0") {
                            AddDivDim = false;
                            document.getElementById("DivInputDim").style.display = "inline";
                        }

                        if (obWallMouseMove !== null) {
                            if (obWallMouseMove !== Wall_Conexion_1) {
                                obWallMouseMove.material = materialWall;
                            }
                        }
                        if (obWallMouseMoveSecontObject !== null) {
                            obWallMouseMoveSecontObject.material = materialWall;
                        }
                        renderer.domElement.style.cursor = 'pointer';
                        obWallMouseMove = null;

                        obWallMouseMove = intersects[i].object;
                        if (intersects[i] !== Wall_Conexion_1) {
                            intersects[i].object.material = SelectMaterial;
                        }
                        var ob = intersects[i].object;
                        if (ActionDbl === "Control_Move_90") {
                            FirstWallConexion = ob;
                            SecontWallConexion = obWall;
                            if (obWall.position.z > ob.position.z) {

                                meshEsqXConexion.visible = false;
                                meshEsq20Conexion.position.x = obWall.position.x;
                                meshEsq20Conexion.position.z = ob.position.z;
                                meshEsq20Conexion.position.y = obWall.position.y;
                                meshEsq20Conexion.visible = true;
                                var obwalld = obWall;
                                IsMiddleOfConnectingX = false;
                                IsMiddleOfConnecting = true;
                            }
                            else {
                                meshEsq20Conexion.visible = false;
                                meshEsqXConexion.position.x = obWall.position.x;
                                meshEsqXConexion.position.z = ob.position.z;
                                meshEsqXConexion.position.y = obWall.position.y;
                                meshEsqXConexion.visible = true;
                                var obwalld = obWall;
                                IsMiddleOfConnecting = false;
                                IsMiddleOfConnectingX = true;
                            }
                            obwalld.material = SelectMaterial;

                        }
                        else {
                            AddDimWall_0(ob);
                        }
                    }
                    if (intersects[i].object.name.substr(0, 9) === "Wall_R900") {
                        if (document.getElementById("IdShowMuros").checked === true) {
                            return;
                        }
                        if (OtherCornerObject === undefined) { continue;}
                        if (OtherCornerObject !== null) {
                            
                            OtherCornerObject.material = materialWall;
                        }
                        if (ActionWizard === 500) {
                            ParallePositionY = intersects[i].object.position.z;
                            ObParalle = intersects[i].object;

                        }
                        meshEsq20Conexion.visible = false;
                        meshEsqXConexion.visible = false;
                        IsMiddleOfConnecting = false;
                        IsMiddleOfConnectingX = false;
                        SelectMaterial = new THREE.MeshLambertMaterial({ color: 0x34DBDB });
                        if (ActionDbl === "Control_Move_90") {
                            AddDivDim = false;
                            document.getElementById("DivInputDim").style.display = "inline";
                        }
                        if (ActionDbl === "Control_Move_270") {
                            AddDivDim = false;
                            document.getElementById("DivInputDim").style.display = "inline";
                        }
                        if (obWallMouseMove !== null) {
                            if (obWallMouseMove !== Wall_Conexion_1) {
                                obWallMouseMove.material = materialWall;
                            }
                        }
                        if (obWallMouseMoveSecontObject !== null) {
                            obWallMouseMoveSecontObject.material = materialWall;
                        }
                        renderer.domElement.style.cursor = 'pointer';
                        obWallMouseMove = null;
                        obWallMouseMove = intersects[i].object;

                        if (intersects[i].object !== Wall_Conexion_1) {
                            intersects[i].object.material = SelectMaterial;
                        }
                        var ob90 = intersects[i].object;
                        AddDimWall_90(ob90);
                    }
                    if (intersects[i].object.name.substr(0, 4) === "Esq_") {
                        if (document.getElementById("IdShowMuros").checked === true) {
                            return;
                        }
                        if (obWallMouseMove !== null) {
                            obWallMouseMove.material = materialWall;
                        }
                        if (OtherCornerObject !== null) {
                            if (OtherCornerObject !== undefined) {
                                OtherCornerObject.material = materialWall;
                            }
                        }
                        obWallMouseMove = intersects[i].object;
                        intersects[i].object.material = SelectMaterial;
                        OtherCornerObject = GetOthetWallEsq(intersects[i].object);
                        if (OtherCornerObject !== undefined) {
                            if (OtherCornerObject !== null) {
                                OtherCornerObject.material = SelectMaterial;
                            }
                        }
                        renderer.domElement.style.cursor = 'pointer';
                        if (intersects[i].object.name.substr(0, 6) === "Esq_50") { ActionDbl = "Esq_50"; }
                        if (intersects[i].object.name.substr(0, 6) === "Esq_70") { ActionDbl = "Esq_70"; }
                        if (intersects[i].object.name.substr(0, 6) === "Esq_10") { ActionDbl = "Esq_10"; }
                        if (intersects[i].object.name.substr(0, 6) === "Esq_30") { ActionDbl = "Esq_30"; }
                    }
                    if (intersects[i].object.name.substr(0, 6) === "Worker") {
                        //GetSceneListMaterial(scene.children);
                        renderer.domElement.style.cursor = 'pointer';
                        obWallMouseMove = intersects[i].object;
                        ActionDbl = "Worker";
                    }
                }
            }
        }

        // Insertar Esquina
        if (InsertWall === 55) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
                raycaster.setFromCamera(mouse, camera);
                if (intersects[i].object.type === "Mesh") {
                    if (intersects[i].object.name === "") {
                        continue;
                    }
                    if (intersects[i].object.name.substr(0, 10) === "Face_Wall_") {

                        if (obOld !== null) {
                            obOld = obOld.material.color.setHex(0x839192);
                        }
                        intersects[i].object.material.color.setHex(0xF4C53D);
                        obOld = intersects[i].object;
                    }
                }
            }
        }
        if (InsertWall === 56) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
 
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                if (intersects[i].object.type === "Mesh") {
                    if (intersects[i].object.name === "") {
                        continue;
                    }
                    if (intersects[i].object.name.substr(0, 8) === "Face_Esq") {
                        //ReturnControlsForCamera(camera, 2);
                        camera.controls.update();
                        var xx = event.pageX;
                        var yy = event.pageY;
                        if (obOld != null) {
                            obOld = obOld.material.color.setHex(0x839192);
                        }
                        intersects[i].object.material.color.setHex(0xffe000);
                        obOld = intersects[i].object;
                    }
                }
            }
        }
        if (/*InsertWall === 1 || */InsertWall === 45 || InsertWall === 5 || InsertWall === 14) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            const intersects = raycaster.intersectObjects(objects);
            if (intersects.length > 0) {
                const intersect = intersects[0];
                rollOverMesh.position.copy(intersect.point).add(intersect.face.normal);
                rollOverMesh.position.divideScalar(25).floor().multiplyScalar(25).addScalar(12.5);
            }
        }
        if (InsertWall === 16) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            const intersects = raycaster.intersectObjects(objects);
            if (intersects.length > 0) {
                const intersect = intersects[0];
                meshNucleo.position.copy(intersect.point).add(intersect.face.normal);
                meshNucleo.position.divideScalar(25).floor().multiplyScalar(25).addScalar(12.5);
            }
        }
        if (Edit_Wall === 20) {
            event.preventDefault();
            mouse.set((event.clientX / window.innerWidth) * 2 - 1, - (event.clientY / window.innerHeight) * 2 + 1);
            raycaster.setFromCamera(mouse, camera);
            var intersects = raycaster.intersectObjects(scene.children);
            for (var i = 0; i < intersects.length; i++) {
                if (intersects[i].object.name === "") { continue; }
                if (intersects[i].object.name === undefined) { continue; }
                if (intersects[i].object.name === null) { continue; }
                if (intersects[i].object.type === "Mesh") {
                    
                    if (intersects[i].object.name.substr(0, 19) === "Din_Text_Wall_Long_") {
                        intersects[i].object.material.color.setHex(0xffe000);
                        obcontrolx = intersects[i].object;
                        ActualControl = intersects[i].object;
                        break;
                    }
                }
            }
        }
        //Insert Wall
        if (InsertWall === 1) {
            AddWallWall_0();
        }
        if (InsertWall === 111) {
            AddWallWall_0();
        }
        if (InsertWall === 2) {
            AddWallWall_90();
        }
        if (InsertWall === 222) {
            AddWallWall_90();
        }
        //Insert Tools
        if (ActionWizard === 500) {
            AddWallParallels();
        }
        //Insert Corner
        if (InsertWall === 15) {
            AddWallCornerWallX();
        }
        if (InsertWall === 10) {
            AddWallCornerWall10();
        }
        if (InsertWall === 30) {
            AddWallCornerWall30();
        }
        if (InsertWall === 70) {
            AddWallCornerWall70();
        }
        if (InsertWall === 50) {
            AddWallCornerWall50();
        }
        if (InsertWall === 60) {
            AddWallCornerWall60();
        }
        if (InsertWall === 20) {
            AddWallCornerWall20();
        }
        if (InsertWall === 80) {
            AddWallCornerWall80();
        }
        if (InsertWall === 40) {
            AddWallCornerWall40();
        }

        if (Wall_Conexion_1 !== null) {
            Wall_Conexion_1.material = SelectMaterialConexion_1;
        }
        if (Wall_Conexion_2 !== null) {
            Wall_Conexion_2.material = SelectMaterialConexion_2;
        }
        function MouseMove_Control_0(intersectsObject) {
            if (obWall == null) { return; }
            var xOwall = obWall.scale.x;
            var d = 0;
            AddDivDim = true;
            ImDraw = true;
            ActionDbl = "Control_Move_0";
            document.getElementById("DivInputDim").style.display = "inline";
            controls = ReturnControlsForCamera(camera, 2);
            renderer.domElement.style.cursor = 'pointer';
            var Difx = 0;
            intersectsObject.material = MaterialSelectIcon;
            var dragControls = null;
            dragControls = new THREE.DragControls(objectsMoveX, camera, renderer.domElement);
            var currentZ = 0;
            var currentX = 0;
            dragControls.addEventListener('dragstart', function () {
                currentZ = objectsMoveX[0].position.z;
            });
            dragControls.addEventListener('drag', function (e) {
                objectsMoveX[0].position.y = 0;
                objectsMoveX[0].position.z = currentZ;
                if (obWall !== null) {
                    var a = parseInt((obWall.position.x + obWall.scale.x * 1000).toFixed(0));
                    var b = parseInt((objectsMoveX[0].position.x).toFixed(0));
                    d = (b - a) / 1000;
                    obWall.scale.x = obWall.scale.x + d;
                    $("#Datalong").val((obWall.scale.x * 10).toFixed(2));
                    $("#DataCordenadX").val((obWall.position.x * 10).toFixed(2));
                    ConeRight.position.x = objectsMoveX[0].position.x;
                    LineDimRight.position.x = objectsMoveX[0].position.x;
                    LineRightToLeft.position.x = obWall.position.x + Difx;
                    LineRightToLeft.scale.x = obWall.scale.x;
                    NewPosition = obWall.position.x;
                    _dim.visible = false;
                    event.preventDefault();
                    NameTextDim = (obWall.scale.x * 10).toFixed(3);
                    var xDim = obWall.position.x + ((obWall.scale.x * 1000) / 2);
                    var yDim = obWall.position.z + 190;
                    //AddDimTemporal(NameTextDim, xDim, yDim);
                    if (obWall.scale.x < 0.029) {
                        /*dragControls.deactivate();*/
                        alert("El muro no puede medir menos de 0,30 mts");
                        obWall.scale.x = xOwall;
                        ResetSetup();
                        return;
                        ImDraw = false;
                    }
                }
            });
            dragControls.addEventListener('dragend', function () {
                controls = ReturnControlsForCamera(camera, 1);
                meshControl_Move_180.material = MaterialUnSelectIcon;

                if (obWall !== null) {
                    NameTextDim = (obWall.scale.x * 10).toFixed(3);
                    var x = obWall.position.x + ((obWall.scale.x * 1000) / 2);
                    var y = obWall.position.z + 190;
                    AddDimText(NameTextDim, x, y);
                }
            });
        };
        function MouseMove_Control_180() {
            if (obWall == null) { return; }
            if (intersects[i].object.name === "") { return; }
            if (intersects[i].object.name === undefined) { return; }
            if (intersects[i].object.name === null) { return; }
            if (intersects[i].object.name === "Control_Move_180") {
                var xOwall = obWall.scale.x;
                var xOwallPosition = obWall.position.x;
                ImDraw = true;
                AddDivDim = true;
                ActionDbl = "Control_Move_180";
                document.getElementById("DivInputDim").style.display = "inline";
                controls = ReturnControlsForCamera(camera, 2);
                renderer.domElement.style.cursor = 'pointer';
                var Difx = 0;
                intersects[i].object.material = MaterialSelectIcon;
                var dragControls = null;
                dragControls = new THREE.DragControls(objectsMoveXEnd, camera, renderer.domElement);
                var currentZ = 0;
                var currentX = 0;
                dragControls.addEventListener('dragstart', function () {
                    currentZ = objectsMoveXEnd[0].position.z;
                });
                dragControls.addEventListener('drag', function (e) {
                    if (obWall !== null) {
                        Difx = objectsMoveXEnd[0].position.x - obWall.position.x;
                        objectsMoveXEnd[0].position.y = 0;
                        objectsMoveXEnd[0].position.z = currentZ;
                        var d = (objectsMoveXEnd[0].position.x - obWall.position.x);
                        obWall.position.x = obWall.position.x + Difx;
                        obWall.scale.x = obWall.scale.x - d / 1000;
                        $("#Datalong").val((obWall.scale.x * 10).toFixed(2));
                        $("#DataCordenadX").val((obWall.position.x * 10).toFixed(2));
                        ConeLeft.position.x = obWall.position.x + Difx;
                        LineDimLef.position.x = obWall.position.x + Difx;
                        LineRightToLeft.position.x = obWall.position.x + Difx;
                        LineRightToLeft.scale.x = obWall.scale.x;
                        NewPosition = obWall.position.x;
                        _dim.visible = false;
                        event.preventDefault();
                        NameTextDim = (obWall.scale.x * 10).toFixed(3);
                        //var xDim = obWall.position.x + ((obWall.scale.x * 1000) / 2);
                        //var yDim = obWall.position.z + 190;
                        //AddDimTemporal(NameTextDim, xDim, yDim);
                        if (obWall.scale.x < 0.029) {
                            /*dragControls.deactivate();*/
                            alert("El muro no puede medir menos de 0,30 mts");
                            obWall.scale.x = xOwall;
                            obWall.position.x = xOwallPosition;
                            ResetSetup();
                            return;
                            ImDraw = false;
                        }
                    }
                });
                dragControls.addEventListener('dragend', function () {
                    if (obWall !== null) {
                        controls = ReturnControlsForCamera(camera, 1);
                        meshControl_Move_180.material = MaterialUnSelectIcon;
                        NameTextDim = (obWall.scale.x * 10).toFixed(3);
                        var x = obWall.position.x + ((obWall.scale.x * 1000) / 2);
                        var y = obWall.position.z + 190;
                        AddDimText(NameTextDim, x, y);

                    }
                });
            }
        };
        function MouseMove_Control_90() {
            if (obWall == null) { return; }
            if (intersects[i].object.name === "") { return; }
            if (intersects[i].object.name === undefined) { return; }
            if (intersects[i].object.name === null) { return; }
            if (intersects[i].object.name === "Control_Move_90") {
                var zOwall = obWall.scale.z;
                var zOwallPosition = obWall.position.z;
                ActionDbl = "Control_Move_90";
                IsMiddleOfConnecting = false;
                meshEsq20Conexion.visible = false;
                meshEsqXConexion.visible = false;
                ImDraw = true;
                AddDivDim = true;
                document.getElementById("DivInputDim").style.display = "inline";
                controls = ReturnControlsForCamera(camera, 2);
                renderer.domElement.style.cursor = 'pointer';
                var Difz = 0;
                intersects[i].object.material = MaterialSelectIcon;
                var dragControls = null;
                var objectsMoveZ = [];
                objectsMoveZ.pop();
                objectsMoveZ.push(meshControl_Move_90);
                dragControls = new THREE.DragControls(objectsMoveZ, camera, renderer.domElement);
                var currentZ = 0;
                var currentX = 0;
                dragControls.addEventListener('dragstart', function (event) {
                    currentX = objectsMoveZ[0].position.X;
                });
                dragControls.addEventListener('drag', function (event) {
                    if (obWall !== null) {
                        Difz = obWall.position.z - objectsMoveZ[0].position.z;
                        objectsMoveZ[0].position.y = 0;
                        objectsMoveZ[0].position.x = currentX;
                        var dz = (objectsMoveZ[0].position.z - obWall.position.z);
                        obWall.position.z = obWall.position.z - Difz;
                        obWall.scale.z = obWall.scale.z - dz / 1000;
                        $("#DataWith").val((obWall.scale.z * 10).toFixed(2));
                        $("#DataCordenadY").val((obWall.position.z * 10).toFixed(2));
                        $("#Datalong").val((obWall.scale.x * 10).toFixed(2));
                        NameTextDim = (obWall.scale.z * 10).toFixed(3);
                        ConeTop.position.z = obWall.position.z - Difz;
                        LineDimTop.position.z = obWall.position.z - Difz;
                        LineDimTopToRDown.position.z = (obWall.position.z - Difz) + (obWall.scale.z * 1000);
                        LineDimTopToRDown.scale.y = obWall.scale.z;
                        if (obWall.scale.z < 0.029) {
                            alert("El muro no puede medir menos de 0,30 mts");
                            obWall.scale.z = zOwall;
                            obWall.position.z = zOwallPosition;
                            ResetSetup();
                            return;
                            ImDraw = false;
                        }
                    }
                });
                dragControls.addEventListener('dragend', function (event) {
                    if (obWall !== null) {
                        controls = ReturnControlsForCamera(camera, 1);
                        meshControl_Move_90.material = MaterialUnSelectIcon;
                        NameTextDim = (obWall.scale.z * 10).toFixed(3);
                        var x = obWall.position.x - 190;
                        var y = obWall.position.z + ((obWall.scale.z * 1000) / 2);
                        AddDimText(NameTextDim, x, y);
                    }
                });
            }
        };
        function MouseMove_Control_270() {
            if (obWall == null) { return; }
            if (intersects[i].object.name === "") { return; }
            if (intersects[i].object.name === undefined) { return; }
            if (intersects[i].object.name === null) { return; }
            if (intersects[i].object.name === "Control_Move_270") {
                var zOwall = obWall.scale.z;
                var zOwallPosition = obWall.position.z;
                AddDivDim = true;
                ImDraw = true;
                ActionDbl = "Control_Move_270";
                document.getElementById("DivInputDim").style.display = "inline";
                controls = ReturnControlsForCamera(camera, 2);
                renderer.domElement.style.cursor = 'pointer';
                var Difz = 0;
                intersects[i].object.material = MaterialSelectIcon;
                var objectsMoveZ = [];
                objectsMoveZ.pop();
                objectsMoveZ.push(meshControl_Move_270);
                var dragControls = null;
                dragControls = new THREE.DragControls(objectsMoveZ, camera, renderer.domElement);
                var currentZ = 0;
                var currentX = 0;
                dragControls.addEventListener('dragstart', function (event) {
                    currentX = objectsMoveZ[0].position.X;
                });
                dragControls.addEventListener('drag', function (event) {
                    if (obWall !== null) {
                        Difz = obWall.position.z - objectsMoveZ[0].position.z;
                        objectsMoveZ[0].position.y = 0;
                        objectsMoveZ[0].position.x = currentX;
                        var dz = (objectsMoveZ[0].position.z - (obWall.position.z + obWall.scale.z * 1000));
                        obWall.scale.z = obWall.scale.z + (dz / 1000);
                        meshControl_Move_270.position.z = obWall.position.z + obWall.scale.z * 1000;
                        meshControl_Move_270.visible = true;
                        $("#DataWith").val((obWall.scale.z * 10).toFixed(2));
                        $("#DataCordenadY").val((obWall.position.z * 10).toFixed(2));
                        $("#Datalong").val((obWall.scale.x * 10).toFixed(2));
                        NameTextDim = (obWall.scale.z * 10).toFixed(3);
                        ConeDown.position.z = obWall.position.z + (obWall.scale.z * 1000);
                        LineDimDown.position.z = obWall.position.z + (obWall.scale.z * 1000);
                        LineDimTopToRDown.scale.y = obWall.scale.z;
                        if (obWall.scale.z < 0.029) {
                            alert("El muro no puede medir menos de 0,30 mts");
                            obWall.scale.z = zOwall;
                            ResetSetup();
                            return;
                            ImDraw = false;
                            zOwall = null;
                        }
                    }
                });
                dragControls.addEventListener('dragend', function (event) {
                    if (obWall !== null) {
                        controls = ReturnControlsForCamera(camera, 1);
                        meshControl_Move_270.material = MaterialUnSelectIcon;
                        NameTextDim = (obWall.scale.z * 10).toFixed(3);
                        var x = obWall.position.x - 190;
                        var y = obWall.position.z + ((obWall.scale.z * 1000) / 2);
                        AddDimText(NameTextDim, x, y);
                    }
                });
            }

        };
    }
    if (CurrentInsertWall != null) {
        ResetView();
    };
});
function GetOthetWallEsq(MasterObject) {
    //Esq_50
    if (MasterObject.idWall.substr(0, 6) === "Esq_50") {
        if (MasterObject.IdWall_180.substr(0, 4) === "Esq_") {
            var OtherCornerObject = GetObToChange(MasterObject.IdWall_180);
            return OtherCornerObject;
        }
        if (MasterObject.IdWall_90.substr(0, 4) === "Esq_") {
            var OtherCornerObject = GetObToChange(MasterObject.IdWall_90);
            return OtherCornerObject;
        }
    }
    //Aqyí 70
    if (MasterObject.idWall.substr(0, 9) === "Esq_70_90")
    {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_0);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    
    }
    if (MasterObject.idWall.substr(0, 9) === "Esq_70_00") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_90);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    }
    if (MasterObject.idWall.substr(0, 9) === "Esq_70_90") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_0);
        if (OtherCornerObject !== null) { return OtherCornerObject; }

    }
    if (MasterObject.idWall.substr(0, 9) === "Esq_10_00") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_270);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    }
    if (MasterObject.idWall.substr(0, 9) === "Esq_10_90") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_0);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    }


    if (MasterObject.idWall.substr(0, 9) === "Esq_30_00") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_270);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    }
    if (MasterObject.idWall.substr(0, 9) === "Esq_30_90") {
        var OtherCornerObject = GetObToChange(MasterObject.IdWall_180);
        if (OtherCornerObject !== null) { return OtherCornerObject; }
    }

    //EndEsq_50
    return null;
};
function HelpSelectMesh() {
    var _listMesh = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            _listMesh.push
                ({
                    name: scene.children[i].name
                });
        }
    }
    return _listMesh;
};
function HelpSelectMeshId() {
    var _listMesh = [];
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            _listMesh.push
                ({
                    idWall: scene.children[i].idWall
                });
        }
    }
    return _listMesh;
};
function getEsqPositionY(IdWall) {
    var listMesh = HelpSelectMeshId();
    for (var i = 0; i < scene.children.length; i++) {
        if (IdWall === scene.children[i].idWall) {
            return scene.children[i].position;
        }
    }
    return null;
};


