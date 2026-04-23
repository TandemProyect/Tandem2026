
$("#Eliminar_Test").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
});
$("#Insert_Test2701").on('click', function (e)
{
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
    }
    var l = 2.7;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(100, -350 + top, 0, w, h, l);
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.60;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.75;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.90;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.05;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2702").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }

    h = 0.45;
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2703").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = -500;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2704").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2705").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2706").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test2707").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2708").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.7;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
///2,400
$("#Insert_Test2401").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(100, -350 + top, 0, w, h, l);
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.60;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.75;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.90;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.05;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2402").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }

    h = 0.45;
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2403").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = -500;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2404").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2405").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2406").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test2407").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2408").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 2.4;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});

///1,200
$("#Insert_Test1201").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 0, 0, 50, "Wall_Dim00Test");
    Testdireccion90(100, -350 + top, 0, w, h, l);
    var xs = 300;
    var xsd = 300;
    var xd = 400;
    var xdd = 500;

    h = 0.45;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 0.60;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.75;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.90;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.05;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1202").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }

    h = 0.45;
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1203").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = -500;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1204").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1205").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1206").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test1207").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1208").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
 
    }
    var l = 1.2;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
///900
$("#Insert_Test0901").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 0, 0, 50, "Wall_Dim00Test");
    Testdireccion90(100, -350 + top, 0, w, h, l);
    var xs = 300;
    var xsd = 300;
    var xd = 400;
    var xdd = 500;

    h = 0.45;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 0.60;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.75;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 0.90;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.05;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0902").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }

    h = 0.45;
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0903").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = -500;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0904").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0905").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0906").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test0907").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0908").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 0.75;
    var w = 0.25;
    var top = 0;
    var xs = 300;
    var xsd = 600;
    var xd = 400;
    var xdd = 500;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);

    xs = xs + 600;
    xsd = xsd + 600;
    xd = xd + 600;
    xdd = xdd + 600;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -350 + top, 0, w, h, l);
    InsertWall = 102
});

 
 
function EraseTest() {
    for (var i = 0; i < scene.children.length; i++) {
        if (scene.children[i].type === "Mesh") {
            if (scene.children[i].name === "") {
                continue;
            }
            if (scene.children[i].name.substr(0, 4) === "Wall") {
                switch (scene.children[i].name.substr(0, 14)) {
                    case "Wall_R000Test":
                        scene.remove(scene.children[i]);
                        break;
                    case "Wall_R900Test":
                        scene.remove(scene.children[i]);
                        break;
                    case "Wall_Dim00Test":
                        scene.remove(scene.children[i]);
                        break;
                }
            }
        }
    }
};

$("#Insert_Test2701c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
    }
    var l = 8.1;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(0, -1000 + top, 0, w, h, l);
    var xs = 2500;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.60;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.75;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.90;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.05;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2702c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    h = 0.45;
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2703c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = 0;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2704c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2705c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2706c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test2707c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2708c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 8.1;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2401c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
    }
    var l = 7.8;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(0, -1000 + top, 0, w, h, l);
    var xs = 2500;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.60;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.75;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.90;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.05;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2402c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    h = 0.45;
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2403c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = 0;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2404c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2405c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2406c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test2407c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test2408c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 7.8;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});

$("#Insert_Test1201c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
    }
    var l = 3.90;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(0, -1000 + top, 0, w, h, l);
    var xs = 2500;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.60;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.75;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.90;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.05;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1202c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    h = 0.45;
    var l = 3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1203c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.9;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = 0;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1204c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1205c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1206c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l =3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test1207c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test1208c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l =3.9;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0901c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
    }
    var l = 3.60;
    var w = 0.25;
    var h = 0.3;
    var top = 0;
    Testdireccion0(0, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, 300, 0, 50, "Wall_Dim00Test");
    Testdireccion90(0, -1000 + top, 0, w, h, l);
    var xs = 2500;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 500;
    h = 0.45;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.60;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.75;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 0.90;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.05;
    Testdireccion0(xd, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0902c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    h = 0.45;
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 1.20;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;

    h = 1.35;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.50;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.65;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.8;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 1.95;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.1;
    Testdireccion0(xs, 0, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0903c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;

    xs = 0;
    xsd = 0;
    xd = 0;
    xdd = 0;
    h = 2.25;
    top = 0;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 2.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0904c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 3.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 3.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 4.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0905c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 5.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 5.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0906c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 6.15;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.3;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.45;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.6;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.75;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 6.9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.05;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102;
});
$("#Insert_Test0907c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 7.2;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.35;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.5;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.65;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.8;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 7.95;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.1;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
$("#Insert_Test0908c").on('click', function (e) {
    $("#ViewTest").hide("slide", { direction: "left" }, 400);
    if (testActive === false) {
        testActive = true;
    }
    else {
        testActive = false;
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();
        EraseTest();

    }
    var l = 3.6;
    var w = 0.25;
    var top = 0;
    var xs = 1000;
    var xsd = 1000;
    var xd = 1000;
    var xdd = 1000;
    h = 8.25;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.4;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.55;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.7;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 8.85;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);

    xs = xs + 1000;
    xsd = xsd + 1000;
    xd = xd + 1000;
    xdd = xdd + 1000;
    h = 9;
    Testdireccion0(xs, top, 0, l, w, h);
    AddDimControl(l + " x " + h, xsd, 0, top + 50, "Wall_Dim00Test");
    Testdireccion90(xd, -1000 + top, 0, w, h, l);
    InsertWall = 102
});
 
function GetSceneListMaterial(Scene)
{
    var _listMaterialTest = [];
    for (var i = 0; i < Scene.length; i++)
    {
        if (Scene[i].type === "Mesh")
        {
            if (Scene[i].name === "") {
                continue;
            }
            if (Scene[i].name.substr(0, 6) === "Atk60_")
            {
                _listMaterialTest.push
                    ({
                        AtkCode: scene.children[i].name,
                    });
            }
        }
    }
    var j = _listMaterialTest;
};
function testUnins() {
    var l = 7;
    var w = 0.30;
    var top = 0;
    var xs = 0;
    var xsd = 0;
    var xd = 0;
    var h = 2.70;
    //Testdireccion0(xs, top, 0, l, w, h);
    Testdireccion90(0, 0 + top, 0, w, h, l);
    InsertWall = 102
};

function Testdireccion0(x, y, ZRotate, _longWall, _widthWall, _heightWall) {
    var partName = new Date().valueOf();
    var NameWall = "Wall_R000Test" + partName;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(x, 0, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = -0.5 * Math.PI;
        meshWall.name = NameWall;
        meshWall.rotation.z = Math.PI;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = _longWall / 10;
        meshWall.scale.y = _widthWall / 10;
        meshWall.scale.z = _heightWall / 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Wall_R000";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = true;
        meshWall.CHeckBracketInside = false;
        meshWall.CHeckBracketOutside = false;
        meshWall.CHeckRijiInside = true;
        meshWall.CHeckRijiOutside = true;
        meshWall.CHeckPropInside = true;
        meshWall.CHeckPropOutside = true;
        meshWall.CHeckPropInsideInf = true;
        meshWall.CHeckPropOutsideInf = true;
        meshWall.CHeck750R = true;
        meshWall.idWall = partName;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        scene.add(meshWall);
    });
};
function Testdireccion90(x, y, ZRotate, _longWall, _widthWall, _heightWall) {
    Edit_Wall = 20;
    var Wall = new THREE.Group();
    var partName = new Date().valueOf();
    var NameWall = "Wall_R900Test" + partName;
    var loader = new THREE.STLLoader();
    loader.load("../../Content/DesignTools/Control/Cube.stl", function (geometry) {
        var meshWall = new THREE.Mesh(geometry, materialWall);
        meshWall.position.set(x, _widthWall * 100, y);
        meshWall.idWall = partName;
        meshWall.rotation.x = 0;
        meshWall.CHeck750R = true;
        meshWall.name = NameWall;
        meshWall.rotation.z = 0;
        meshWall.scale.set(1, 1, 1);
        meshWall.scale.x = _longWall / 10;
        meshWall.scale.y = _widthWall / 10;
        meshWall.scale.z = _heightWall / 10;
        meshWall.Iniciall_Wall = 0;
        meshWall.End_Wall = 0;
        meshWall.MeshTypeWall = "Wall_R900AddCorner70Left0Right0";
        meshWall.MeshTypeWallLeft = "";
        meshWall.MeshTypeWallRight = "";
        meshWall.MeshTypeWall_180 = 0;
        meshWall.MeshTypeWall_0 = 0;
        meshWall.IdCornerDown = 0;
        meshWall.IdCornerLeft = 0;
        meshWall.ScaleEsqy = 0;
        meshWall.CHeckDimWall = true;
        meshWall.CHeckBracketInside = true;
        meshWall.CHeckBracketOutside = true;
        meshWall.CHeckRijiInside = true;
        meshWall.CHeckRijiOutside = true;
        meshWall.CHeckPropInside = true;
        meshWall.CHeckPropOutside = true;
        meshWall.CHeckPropInsideInf = true;
        meshWall.CHeckPropOutsideInf = true;
        meshWall.idWall = partName;
        meshWall.LongLeft = 0;
        meshWall.LongRight = 0;
        scene.add(meshWall);
    });
};