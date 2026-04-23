
function Donload_File(File, NameFile) {
            const bytes = new Array(File);
            for (let i = 0; i < File.length; i++) {
                bytes[i] = File.charCodeAt(i);
            }
            const data = new Uint8Array(bytes);
            var blob = new Blob([data], {
                type: "text/plain; encoding=UTF-8"
            });
    saveAs(blob, NameFile);
 
}

function Design_3d_MaterialExportCad(name, x, y, z, RotationX, RotationY, RotationZ, ob) {
    var _x = x.toString();
    var _y = y.toString();
    var _z = z.toString();
    var _RotationX = "0";
    if (ob.CadRotation !== undefined) {
        _RotationX = ob.CadRotation.toString();
    }
    var _RotationY = RotationX.toString();
    var _RotationZ = RotationX.toString();
    var _listMaterialExport = [];
    if (name.substr(0, 14) === "Atk60_10443020") {
        _listMaterialExport.push({ NameBlock: "ATK_Union10443020", x: _x, y: _y, z: _z, Rotate_X: _RotationX, Rotate_Y: _RotationY, Rotate_Z: _RotationZ });
    }
    if (name.substr(0, 14) === "Atk60_27904209") {
        _listMaterialExport.push({ NameBlock: "ATK_Panel27x90", x: _x, y: _y, z: _z, Rotate_X: _RotationX, Rotate_Y: _RotationY, Rotate_Z: _RotationZ });
    }
    if (name.substr(0, 14) === "Atk60_27304205") {
        _listMaterialExport.push({ NameBlock: "ATK_Panel27x30", x: _x, y: _y, z: _z, Rotate_X: _RotationX, Rotate_Y: _RotationY, Rotate_Z: _RotationZ });
    }

    if (name.substr(0, 14) === "Atk60_27104219") {
        _listMaterialExport.push({ NameBlock: "ATK_Panel27x75R", x: _x, y: _y, z: _z, Rotate_X: _RotationX, Rotate_Y: _RotationY, Rotate_Z: _RotationZ });
    }

    if (name === "Atk60_4120000042") {
        _x = (x - 30).toString();
        _listMaterialExport.push({ NameBlock: "ATK_Braket", x: _x, y: _y, z: _z, Rotate_X: _RotationX, Rotate_Y: _RotationY, Rotate_Z: _RotationZ });
    }
    return _listMaterialExport;
};

function htmlEntities(str)
{
    return String(str).replace('&ntilde;', '')
        .replace('&#225;', 'á')
        .replace('&#233;', 'é')
        .replace('&#237;', 'í')
        .replace('&#243;', 'ó')
        .replace('&#250;', 'ú')
        .replace('&#193;', 'Á')
        .replace('&#201;', 'É')
        .replace('&#205;', 'Í')
        .replace('&#211;', 'Ó')
        .replace('&#218;', 'Ú')
        .replace('&Ntilde;', 'Ñ')
        .replace('&amp;', '&')
        .replace('&Ntilde;', 'Ñ')
        .replace('&ntilde;', 'ñ')
        .replace('&Ntilde;', 'Ñ')
        .replace('&Agrave;', 'À')
        .replace('&Aacute;', 'Á')
        .replace('&Acirc;', 'Â')
        .replace('&Atilde;', 'Ã')
        .replace('&Auml;', 'Ä')
        .replace('&Aring;', 'Å')
        .replace('&AElig;', 'Æ')
        .replace('&Ccedil;', 'Ç')
        .replace('&Egrave;', 'È')
        .replace('&Eacute;', 'É')
        .replace('&Ecirc;', 'Ê')
        .replace('&Euml;', 'Ë')
        .replace('&Igrave;', 'Ì')
        .replace('&Iacute;', 'Í')
        .replace('&Icirc;', 'Î')
        .replace('&Iuml;', 'Ï')
        .replace('&ETH;', 'Ð')
        .replace('&Ntilde;', 'Ñ')
        .replace('&Ograve;', 'Ò')
        .replace('&Oacute;', 'Ó')
        .replace('&Ocirc;', 'Ô')
        .replace('&Otilde;', 'Õ')
        .replace('&Ouml;', 'Ö')
        .replace('&Oslash;', 'Ø')
        .replace('&Ugrave;', 'Ù')
        .replace('&Uacute;', 'Ú')
        .replace('&Ucirc;', 'Û')
        .replace('&Uuml;', 'Ü')
        .replace('&Yacute;', 'Ý')
        .replace('&THORN;', 'Þ')
        .replace('&szlig;', 'ß')
        .replace('&agrave;', 'à')
        .replace('&aacute;', 'á')
        .replace('&acirc;', 'â')
        .replace('&atilde;', 'ã')
        .replace('&auml;', 'ä')
        .replace('&aring;', 'å')
        .replace('&aelig;', 'æ')
        .replace('&ccedil;', 'ç')
        .replace('&egrave;', 'è')
        .replace('&eacute;', 'é')
        .replace('&ecirc;', 'ê')
        .replace('&euml;', 'ë')
        .replace('&igrave;', 'ì')
        .replace('&iacute;', 'í')
        .replace('&icirc;', 'î')
        .replace('&iuml;', 'ï')
        .replace('&eth;', 'ð')
        .replace('&ntilde;', 'ñ')
        .replace('&ograve;', 'ò')
        .replace('&oacute;', 'ó')
        .replace('&ocirc;', 'ô')
        .replace('&otilde;', 'õ')
        .replace('&ouml;', 'ö')
        .replace('&oslash;', 'ø')
        .replace('&ugrave;', 'ù')
        .replace('&uacute;', 'ú')
        .replace('&ucirc;', 'û')
        .replace('&uuml;', 'ü')
        .replace('&yacute;', 'ý')
        .replace('&thorn;', 'þ')
        .replace('&yuml;', 'ÿ');
};
