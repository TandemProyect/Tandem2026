using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Desing.Models
{
    public class ModelDesignMaterialList
    {
        public Array ListMaterialModel { get; set; }
    }
    public class ModelDesignCodeText
    {
        public string CodeText { get; set; }
    }

    public class ModelDesingList : TSql_Design
    {
        public bool OnlyUser { get; set; }
    }


    public class ModelDesing
    {
        public long IDDesaing { get; set; }
        public string PositionX { get; set; }
        public string PositionY { get; set; }
        public string PositionZ { get; set; }
        public string IdWall { get; set; }
        public string RotationX { get; set; }
        public string RotationY { get; set; }
        public string RotationZ { get; set; }
        public string Name { get; set; }
        public string ScaleX { get; set; }
        public string ScaleY { get; set; }
        public string ScaleZ { get; set; }
        public long Iniciall_Wall { get; set; }
        public long End_Wall { get; set; }
        public string TypeWall { get; set; }
        public string TypeWallLeft { get; set; }
        public string TypeWallRight { get; set; }

        public long IDCornerDown { get; set; }
        public long IDCornerLeft { get; set; }
        public long ScaleEsqy { get; set; }
        public bool CHeckDimWall { get; set; }
        public bool? CHeckBracketInside { get; set; }
        public bool CHeckBracketOutside { get; set; }
        public bool CHeckRijiInside { get; set; }
        public bool CHeckRijiOutside { get; set; }
        public bool CHeckPropInside { get; set; }
        public bool CHeckPropOutside { get; set; }
        public bool CHeckPropInsideInf { get; set; }
        public bool CHeckPropOutsideInf { get; set; }
        public bool CHeck750R { get; set; }
        public long LongLeft { get; set; }
        public long LongRight { get; set; }
        public bool IsSolutionCornerYUniversalPanelCorner { get; set; }
        public bool IsSolutionCornerXUniversalPanelCorner { get; set; }
        public string Tape_0 { get; set; }
        public string Tape_180 { get; set; }
        public string Tape_90 { get; set; }
        public string Tape_270 { get; set; }
        public string Grupo { get; set; }
        public string Sub_Long_0 { get; set; }
        public string Sub_Long_180 { get; set; }
        public string Sub_Long_90 { get; set; }
        public string Sub_Long_270 { get; set; }
        public string IdWall_0 { get; set; }
        public string IdWall_180 { get; set; }
        public string IdWall_90 { get; set; }
        public string IdWall_270 { get; set; }
        public string TypeWall_180 { get; set; }
        public string TypeWall_0 { get; set; }
        public string TypeWall_90 { get; set; }
        public string TypeWall_270 { get; set; }
        public bool IdTypeFormworkMode { get; set; }

        public static implicit operator List<object>(ModelDesing v)
        {
            throw new NotImplementedException();
        }
    }

    public class ModelStock
    {
        public bool ExitingPanel2400 { get; set; }
        public bool AddTapeWidthExactIfPossible { get; set; }
        public long IdEnvironmentValue { get; set; }
        public long IdEnvironmentOrbitValue { get; set; }
        public bool IsSolutionCornerWithUniversalPanel { get; set; }
    }
    public enum mnvironment
    {
        Real,
        Claro,
        Oscuro
    }
    public class ModelDesign3d
    {
        public long DesignId { get; set; }
        [Required(ErrorMessage = "El nombre del diseño es obligatorio")]
        [StringLength(500)]
        [DisplayName("Nombre del nuevo diseño")]
        public string DesignName { get; set; }


        [DisplayName("Observaciones")]
        public string AttDescription { get; set; }

        public string Grupo { get; set; }
        public long SelectedCamera { get; set; }
        public long? GroundSizeX { get; set; }
        public long? GroundSizeY { get; set; }
        public List<TSql_DesignCamera> ListOfCameras { get; set; }

        public List<TSql_DesignDetails> ElemtOfDesign { get; set; }

        public string Avatar { get; set; }
        public bool ExitingPanel2400 { get; set; }
        public decimal? NumberClosingStartEndWall { get; set; }
        public long LinkEnvironment { get; set; }

        public long? IdUndoRedo { get; set; }

        public long LinkEnvironmentOrbitValue { get; set; }
        public bool IsSolutionCornerWithUniversalPanel { get; set; }
    }
}

