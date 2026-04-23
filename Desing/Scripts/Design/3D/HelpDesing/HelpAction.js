//Desing
$("#BtnCloseHelp").on("click", function () 
{
    videoElem.src = "";
    $("#Loading").hide();
    ReturnControlsForCamera(camera, 1);
    $("#HelpDesing").hide("slide", { direction: "left" }, 400);

    });
$("#CallHelp").on("click", function () {
    $("#Loading").show();
    ReturnControlsForCamera(camera, 2);
    //1400px    
    var pageWidth = document.documentElement.scrollWidth;
    var LeftM = pageWidth - ((pageWidth / 2) + 700);
    document.getElementById("HelpDesing").style.left = LeftM;
    $("#HelpDesing").show("slide", { direction: "left" }, 400);
    videoElem = document.getElementById("Video_Principal");
    videoElem.src = "../../Files/Help/Video/Presentacion2025.mp4";
    videoElem.play();
});