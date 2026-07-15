using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryCommun
{
    public sealed class Desing2FormworkRequest
    {
        public string System { get; set; }
        public List<Desing2FormworkWallDto> Walls { get; set; }
        public List<Desing2FormworkWallDto> List { get; set; }
        public List<Desing2FormworkWallGeomDto> WallGeom { get; set; }
        public JObject WallConnections { get; set; }
        public JObject Meta { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }

    public sealed class Desing2WallIdsRequest
    {
        public string IdsJson { get; set; }
    }

    public sealed class Desing2FormworkWallDto
    {
        public string Id { get; set; }
        public string LineId { get; set; }
        public string WallGroupId { get; set; }
        public string WallRole { get; set; }
        public string PolylineId { get; set; }
        public string WallId { get; set; }
        public AttributesList Attributes { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }

    public sealed class ModulosAtk60Wall
    {
        public string IdWall { get; set; }
        public long M_270 { get; set; } = 0;
        public long M_255 { get; set; } = 0;
        public long M_240 { get; set; } = 0;
        public long M_225 { get; set; } = 0;
        public long M_210 { get; set; } = 0;
        public long M_195 { get; set; } = 0;
        public long M_180 { get; set; } = 0;
        public long M_165 { get; set; } = 0;
        public long M_150 { get; set; } = 0;
        public long M_135 { get; set; } = 0;
        public long M_120 { get; set; } = 0;
        public long M_105 { get; set; } = 0;
        public long M_090 { get; set; } = 0;
        public long M_075 { get; set; } = 0;
        public long M_060 { get; set; } = 0;
        public long M_045 { get; set; } = 0;
        public long M_0430 { get; set; } = 0;

        public double M_Remate { get; set; } = 0;


    }

    public sealed class Atk60ThreeJsPaintPayload
    {
        public List<Atk60WallPaintAnchor> Walls { get; set; } = new List<Atk60WallPaintAnchor>();
        public List<Atk60ElementPaintItem> Elements { get; set; } = new List<Atk60ElementPaintItem>();
    }

    public sealed class Atk60WallPaintAnchor
    {
        public string IdWall { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RotX { get; set; }
        public double RotY { get; set; }
        public double RotZ { get; set; }
        public Atk60WallPaintAnchorDebug Debug { get; set; }
    }

    public sealed class Atk60WallPaintAnchorDebug
    {
        public double StartX { get; set; }
        public double StartZ { get; set; }
        public double InsertX { get; set; }
        public double InsertZ { get; set; }
        public double FaceSign { get; set; }
        public double WidthMm { get; set; }
    }

    public sealed class Atk60ElementPaintItem
    {
        public string IdWall { get; set; }
        public string ElementCode { get; set; }
        public string ImportPath { get; set; }
        public string Color { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RotX { get; set; }
        public double RotY { get; set; }
        public double RotZ { get; set; }
    }



    public sealed class AttributesList
    {
        public string _idObject { get; set; }
        public string _TypeMesh { get; set; }
        public double? _Datalong { get; set; }
        public double? _DataWith { get; set; }
        public double? _DataHeight { get; set; }
        public double? _XRotation { get; set; }
        public double? _YrRtation { get; set; }
        public double? _ZRotation { get; set; }
        public bool? _IsFormwork { get; set; }
        public bool? _IsUniversalPanel { get; set; }
        public double? _XCoordinate { get; set; }
        public double? _YCoordinate { get; set; }
        public double? _ZCoordinate { get; set; }
        public string _Tape_1 { get; set; }
        public string _Tape_2 { get; set; }
        public string _Idconnection_1 { get; set; }
        public string _Idconnection_2 { get; set; }
        public bool? _CHeckBracketInside { get; set; }
        public bool? _CHeckBracketOutside { get; set; }
        public bool? _CHeckRijiInside { get; set; }
        public bool? _CHeckRijiOutside { get; set; }
        public bool? _CHeckPropInside { get; set; }
        public bool? _CHeckPropOutside { get; set; }
        public bool? _CHeckPropInsideInf { get; set; }
        public bool? _CHeckPropOutsideInf { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> Extra { get; set; }
    }

    public sealed class Desing2FormworkWallGeomDto
    {
        public string Id { get; set; }
        public string LineId { get; set; }
        public double? StartXmm { get; set; }
        public double? StartYmm { get; set; }
        public double? StartZmm { get; set; }
        public double? EndXmm { get; set; }
        public double? EndYmm { get; set; }
        public double? EndZmm { get; set; }
        public double? LengthMm { get; set; }
        public double? WidthMm { get; set; }
        public double? HeightMm { get; set; }
    }
}
