//Print
$("#btnPrintDesing").on("click", function () {
    setTimeout(function () {
        //deactivateZoomAndPan();

        let fileName = "Hola" + '.png';
        let img = new Image();
        animate();
        img.src = renderer.domElement.toDataURL();

        if (window.navigator.msSaveBlob) {
            window.navigator.msSaveBlob(renderer.domElement.msToBlob(), fileName);
            e.preventDefault();
        } else {
            let imgForDownload = img.src.replace('data:image/png;base64,', '').replace('">', '');
            let data = base64ToArrayBuffer(imgForDownload);

            saveFileToDisk([data], fileName);
        }
        //    activateZoomAndPan();

        //var dataURL = renderer.domElement.toDataURL();
        //var link = document.createElement("a");
        //link.download = "demo.png";
        //link.href = dataURL;
        //link.target = "_blank";
        //link.click();
    }, 1000);
});
function screenshot() {
    var retunScreenshot = null;
    var fileName = "Hola" + '.png';
    let img = new Image();
    animate();
    img.src = renderer.domElement.toDataURL();
    if (window.navigator.msSaveBlob) {
        window.navigator.msSaveBlob(renderer.domElement.msToBlob(), fileName);
        e.preventDefault();
    } else {
        let imgForDownload = img.src.replace('data:image/png;base64,', '').replace('">', '');
        let data = base64ToArrayBuffer(imgForDownload);
        retunScreenshot = SaveImg([data], fileName);
    }
    return retunScreenshot;
};
function SaveImg(data, fileName) {
    var fileName = "Hola" + '.png';
    let img = new Image();
    animate();
    img.src = renderer.domElement.toDataURL();
    if (window.navigator.msSaveBlob) {
        window.navigator.msSaveBlob(renderer.domElement.msToBlob(), fileName);
        e.preventDefault();
    } else {
        let imgForDownload = img.src.replace('data:image/png;base64,', '').replace('">', '');
        let data = base64ToArrayBuffer(imgForDownload);
    }
    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style = "display: none";
    var blob = new Blob(data, { type: "octet/stream" }),
        url = window.URL.createObjectURL(blob);
    let strMime = "image/jpeg";
    let imgForDownload = renderer.domElement.toDataURL(strMime);
    return imgForDownload;   
}
function saveFileToDisk(data, fileName) {
 
    var a = document.createElement("a");
    document.body.appendChild(a);
    a.style = "display: none";
    var blob = new Blob(data, { type: "octet/stream" }),
        url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
}

function createScreenshot() {
    animate();
    let strMime = "image/jpeg";
    let imgForDownload = renderer.domElement.toDataURL(strMime);
    return imgForDownload;
}
function base64ToArrayBuffer(base64) {
    var binaryString = window.atob(base64);
    var binaryLen = binaryString.length;
    var bytes = new Uint8Array(binaryLen);
    for (var i = 0; i < binaryLen; i++) {
        var ascii = binaryString.charCodeAt(i);
        bytes[i] = ascii;
    }
    return bytes;
}