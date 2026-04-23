
$('#btnWallOpacity').on('click', function () {
    var labelValue = document.getElementById('btnWallOpacity').innerHTML;
    if (labelValue === 'Ver Muro Transparente') {
        document.getElementById('btnWallOpacity').innerHTML = 'Ver Muro Opaco';
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].type == "Mesh") {
                if (scene.children[i].name == "") {
                    continue;
                }
                var obName = scene.children[i].name.substr(0, 4);
                var ob = scene.children[i];
                if (obName == "Wall") {
                    ob.material.transparent = true;
                    ob.material.opacity = 0.25;
                    ob.material.dispose();
                }
                if (obName == "Esq_") {
                    ob.material.transparent = true;
                    ob.material.opacity = 0.25;
                    ob.material.dispose();
                }
            }
        }
    }
    else
    {
        document.getElementById('btnWallOpacity').innerHTML = 'Ver Muro Transparente';
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].type == "Mesh") {
                if (scene.children[i].name == "") {
                    continue;
                }
                var obName = scene.children[i].name.substr(0, 4);
                var ob = scene.children[i];
                if (obName == "Wall") {
                    ob.material.transparent = false;
                    ob.material.opacity =1;
                    ob.material.dispose();
                }
                if (obName == "Esq_") {
                    ob.material.transparent = false;
                    ob.material.opacity =1;
                    ob.material.dispose();
                }
            }
        }

    }
});


$('#btnWallHide').on('click', function () {

    var labelValue = document.getElementById('btnWallHide').innerHTML;
    if (labelValue === 'Ocultar Muro') {
        document.getElementById('btnWallHide').innerHTML = 'Visualizar Muro';
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].type == "Mesh") {
                if (scene.children[i].name == "") {
                    continue;
                }
                var obName = scene.children[i].name.substr(0, 4);
                var ob = scene.children[i];
                if (obName == "Wall") {
                    ob.material.visible = false;
                }
                if (obName == "Esq_") {
                    ob.material.visible = false;
                }
            }
        }
    }
    else {
        document.getElementById('btnWallHide').innerHTML = 'Ocultar Muro';
        for (var i = 0; i < scene.children.length; i++) {
            if (scene.children[i].type == "Mesh") {
                if (scene.children[i].name == "") {
                    continue;
                }
                var obName = scene.children[i].name.substr(0, 4);
                var ob = scene.children[i];
                if (obName == "Wall") {
                    ob.material.visible = true;
                }
                if (obName == "Esq_") {
                    ob.material.visible = true;
                }
            }
        }

    }
});

function SelectArtiquel(id) {
    var obMenuV = "BtnV_" + id;
    document.getElementById(obMenuV).style.backgroundColor = "blue";
    if (_tempBtnV != "") {
        document.getElementById(_tempBtnV).style.backgroundColor = "#cd4237";
    }
    if (_tempBtn != "") {
        document.getElementById(_tempBtn).style.backgroundColor = "#ffc107";
    }
    _tempBtnV = obMenuV;
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type == "Mesh") {
            if (scene.children[i].name == "") {
                continue;
            }
            var ob = scene.children[i];
            var name = "Atk60_" + id;
            if (scene.children[i].name == name) {
                ob.material.visible = true;
            }
            else {
                if (ob.name.substr(0, 6) === "Atk60_") {
                    ob.material.visible = false;
                }
            }
        }
    }
};
function SelectOpacityArtiquel(id) {
    var obMenu = "Btn_" + id;
    document.getElementById(obMenu).style.backgroundColor = "blue";
    if (_tempBtn != "") {
        document.getElementById(_tempBtn).style.backgroundColor = "#ffc107";
    }
    if (_tempBtnV != "") {
        document.getElementById(_tempBtnV).style.backgroundColor = "#cd4237";
    }
    _tempBtn = obMenu;
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type == "Mesh") {
            if (scene.children[i].name == "") {
                continue;
            }
            var ob = scene.children[i];
            ob.material.visible = true;
            var name = "Atk60_" + id;

            if (scene.children[i].name == name) {
                /* document.getElementById(obMenu).style.backgroundColor = "blue";*/
                ob.material.transparent = false;
                ob.material.opacity = 1;
                ob.material.dispose();
            }
            else {
                if (ob.name.substr(0, 6) === "Atk60_") {
                    ob.material.transparent = true;
                    ob.material.opacity = 0.25;
                    ob.material.dispose();
                }


            }
        }
    }
};