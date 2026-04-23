using System.Collections.Generic;

namespace Desing.Controllers
{

    public class Vector3dPoint
    {
        public long x { get; set; }
        public long y { get; set; }
        public long z { get; set; }
    }


    public class TemporalList
    {
        public string AtkCode { get; set; }
        public string AtkGrup { get; set; }
    }



    public class ModelWalls
    {
        public string TypeMesh { get; set; }
        public long DataCordenadX { get; set; }
        public long DataCordenadY { get; set; }
        public long DataHeight { get; set; }
        public long DataWith { get; set; }
        public long? DataWithOtherCorner { get; set; }
        public long Datalong { get; set; }
        public long DesignId { get; set; }
        public long Type { get; set; }
        public string DataRotateX { get; set; }
        public string DataRotateY { get; set; }
        public string DataRotateZ { get; set; }
        public long DataSupInicial { get; set; }
        public long DataSupEnd { get; set; }
        public bool UniversalPanel { get; set; }
        public long XWith { get; set; }
        public long YWith { get; set; }
        public string IdWall { get; set; }
        public long LongLeft { get; set; }
        public long LongRight { get; set; }
        public bool CHeck750R { get; set; }
        public string Tape_0 { get; set; }
        public string Tape_180 { get; set; }
        public string Tape_90 { get; set; }
        public string Tape_270 { get; set; }
        public bool IdTypeFormworkMode { get; set; }

    }


    public class ModelRenderElement
    {
        public string Type { get; set; }
        public bool ElementMirrow { get; set; }
        public string Element { get; set; }
        public string ElementF { get; set; }
        public string ElementWood { get; set; }
        public string ElementUnion1 { get; set; }
        public long? LongDim { get; set; }
        public long? LongDimTypeHorizontal { get; set; }
        public long? LongDimTypeHorizontalT { get; set; }
        public long? LongDimTypeVertical { get; set; }
        public long? LongWood { get; set; }
        public long? heightWood { get; set; }
        public long x { get; set; }
        public long y { get; set; }
        public long z { get; set; }
        public long XRotate { get; set; }
        public string ZRotate { get; set; }
        public long YRotate { get; set; }
        public string CodeName { get; set; }
        public long? XWith { get; set; }
        public string IdWall { get; set; }
        public string IdElement { get; set; }
        public string Filter { get; set; }
        public long? ParametFilter { get; set; }

    }

    public enum TypeHeight
    {
        Vertical,
        Horizontal
    }
    public enum DimType
    {
        Vertical = 1,
        Horizontal = 2,
        HorizontalT = 3,
        Vertical50 = 50,
        No = 4
    }

    public enum RotateMesh
    {
        rotate_0,
        rotate_90,
        rotate_180,
        rotate_270,
    }


    public enum TypeDimension
    {
        d030 = 300,
        d045 = 450,
        d060 = 600,
        d090 = 900,
        d120 = 1200,
        d150 = 1500,
        d180 = 1800,
        d210 = 2100,
        d240 = 2400,
        d270 = 2700,
    }

    public class ModelHLevel
    {
        public List<ModelLeven> ListLevel { get; set; }
    }
    public class ModelLeven
    {
        public TypeDimension Level { get; set; }
        public TypeHeight TypeLevel { get; set; }
        public long ZCoordinate { get; set; }
    }
}
