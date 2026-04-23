using DAL;
using Desing.Controllers;
using System;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloWallEsqTLe : BaseController
    {
        internal static List<ModelRenderElement> setdListElement
        (
        string typeMesh,
        long yWith,
        long xWith,
        bool universalPanel,
        TSql_DefaultDesign currentDefaultDisign,
        long dataWith,
        long datalong,
        long dataHeight,
        long dataCordenadX,
        long dataCordenadY,
        long type,
        long? DataWithOtherCorner,
        string Tape_0,
        string Tape_180,
        string Tape_90,
        string Tape_270
            )
        {
            var PanelPerfil = 12;
            var IsAngular = true;
            if (typeMesh == "Esq_60_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq180 = WallCorner.CornerX_180_Esq.setdListElement(3, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX - (dataHeight / 10), dataCordenadY, DataWithOtherCorner);
                if (ListRenderElementEsq180 != null)
                {
                    foreach (var item in ListRenderElementEsq180)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;

                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                var ListRenderElementEsqX00 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX + (dataHeight / 10), dataCordenadY + 30, DataWithOtherCorner);
                if (ListRenderElementEsqX00 != null)
                {
                    foreach (var item in ListRenderElementEsqX00)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;

                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_60_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                var testLong = "450";
                if (dataHeight > 450) { testLong = "600"; }
                if (dataHeight > 600) { testLong = "750"; }
                if (dataHeight > 750) { testLong = "900"; }
                if (dataHeight > 900) { testLong = "1050"; }
                if (dataHeight > 1050) { testLong = "1200"; }
                switch (testLong)
                {
                    case "450":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_45.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "600":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_60.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "750":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_75.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "900":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_90.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1050":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_1050.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1200":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_1200.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                }
                if (ListRenderElementPanel270 != null)
                {
                    foreach (var item in ListRenderElementPanel270)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;

                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_50_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                //CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataHeight, 1, 0, DimType.Horizontal, typeMesh, "", 0);
                var ListRenderElementEsq50 = ModuloWallEsqTEsq_50_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataWith,
                    datalong,
                     dataHeight,
                    dataCordenadX,
                    dataCordenadY,
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );
                if (ListRenderElementEsq50 != null)
                {
                    foreach (var item in ListRenderElementEsq50)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.Type = item.Type;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_50_90")
            {
                var datalongTemporal = datalong;
                var dataWithTemporal = dataHeight;
                datalong = dataWith;
                dataHeight = datalongTemporal;
                dataWith = dataWithTemporal;
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                if (Tape_0 == "Universal_X")
                {
                    var Position_Y = (dataCordenadY + 30 + dataWith / 10) - (PanelPerfil / 2);
                    List<ModelRenderElement> ListRenderElementDataType0 = null;
                    ListRenderElementDataType0 = WallTapeR.TapeRP_E50_00.setdListElement(dataCordenadX, 0, dataHeight, dataWith, datalong, dataCordenadY, 270, true, Position_Y, currentDefaultDisign.ExitingPanel2400);
                    if (ListRenderElementDataType0 != null)
                    {
                        foreach (var item in ListRenderElementDataType0)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.Type = item.Type;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                    return ListRenderElement;
                }
                else
                {
                    var testLong = "450";
                    if (datalong > 450) { testLong = "600"; }
                    if (datalong > 600) { testLong = "750"; }
                    if (datalong > 750) { testLong = "900"; }
                    if (datalong > 900) { testLong = "1050"; }
                    if (datalong > 1050) { testLong = "1200"; }

                    CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, Int32.Parse(testLong), 2, 0, DimType.Horizontal, typeMesh, "", 0);
                    switch (testLong)
                    {
                        case "450":
                            ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_45.setdListElement(type, currentDefaultDisign, dataHeight, datalong, dataWith, dataCordenadX, dataCordenadY, Tape_0);
                            break;
                        case "600":
                            ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_60.setdListElement(type, currentDefaultDisign, dataHeight, datalong, dataWith, dataCordenadX, dataCordenadY, Tape_0);
                            break;
                        case "750":
                            ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_75.setdListElement(type, currentDefaultDisign, dataHeight, datalong, dataWith, dataCordenadX, dataCordenadY, Tape_0);
                            break;
                        case "900":
                            ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_90.setdListElement(type, currentDefaultDisign, dataHeight, datalong, dataWith, dataCordenadX, dataCordenadY, Tape_0);
                            break;
                        case "1050":
                            //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1050.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                            break;
                        case "1200":
                            //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1200.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight,dataCordenadX, dataCordenadY);
                            break;
                    }
                    if (ListRenderElementPanel270 != null)
                    {
                        foreach (var item in ListRenderElementPanel270)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;

                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                    return ListRenderElement;
                }
            }
            if (typeMesh == "Esq_X_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsqX = ModuloWallEsqTEsq_X_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataWith,
                    datalong,
                    dataHeight,
                    dataCordenadX,
                    dataCordenadY,
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );
                if (ListRenderElementEsqX != null)
                {
                    foreach (var item in ListRenderElementEsqX)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;

                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_70_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataWith, 2, 0, DimType.Horizontal, typeMesh, "", 0);
                //Panel 180
                if (Tape_270 == "Universal_Y")
                {
                    IsAngular = false;
                }
                if (Tape_180 == "Universal_X")
                {
                    return ListRenderElement;
                }
                if (Tape_180 == "Universal_Y")
                {
                    IsAngular = false;
                }
                var ListRenderElementEsq70 = ModuloWallEsqTEsq_70_90.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    datalong,
                    dataHeight,
                    dataWith,
                    dataCordenadX - (dataHeight / 10),
                    dataCordenadY - (dataWith / 10),
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );

                if (ListRenderElementEsq70 != null)
                {
                    foreach (var item in ListRenderElementEsq70)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;

                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_70_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var IsAngular180 = true;
                if (Tape_270 == "Universal_X")
                {
                    IsAngular = false;
                }
                if (Tape_180 == "Universal_Y")
                {
                    IsAngular180 = false;
                }
                //Internarl Corner
                var ListRenderElementEsq180 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (30 + (datalong / 10)), dataCordenadY - (datalong / 10), DataWithOtherCorner);
                if (ListRenderElementEsq180 != null)
                {
                    foreach (var item in ListRenderElementEsq180)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }

                if (IsAngular180 == true)
                {     //Angular
                    if (IsAngular == true)
                    {
                        var ListRenderElementAng = SedAng_180_Esq_270.setdListElement(dataCordenadX, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                        if (ListRenderElementAng != null)
                        {
                            foreach (var item in ListRenderElementAng)
                            {
                                ModelRenderElement element = new ModelRenderElement();
                                element.IdElement = item.IdElement;
                                element.CodeName = item.CodeName;
                                element.Element = item.Element;
                                element.ElementF = item.ElementF;
                                element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                                element.LongDimTypeVertical = item.LongDimTypeVertical;
                                element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                                element.ElementWood = item.ElementWood;
                                element.ElementUnion1 = item.ElementUnion1;
                                element.LongWood = item.LongWood;
                                element.heightWood = item.heightWood;
                                element.x = item.x;
                                element.y = item.y;
                                element.z = item.z;
                                element.XRotate = item.XRotate;
                                element.YRotate = item.YRotate;
                                element.ZRotate = item.ZRotate;
                                element.CodeName = item.CodeName;
                                element.Filter = item.Filter;
                                ListRenderElement.Add(element);
                            }
                        }
                    }
                }
                //Panel 270
                if (IsAngular == true)
                {
                    CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataHeight, 1, 0, DimType.Horizontal, typeMesh, "", 0);

                    var ListRenderElementEsq70 = ModuloWallEsqTEsq_70_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataWith,
                    datalong,
                    dataHeight,
                    dataCordenadX,
                    dataCordenadY,
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );
                    if (ListRenderElementEsq70 != null)
                    {
                        foreach (var item in ListRenderElementEsq70)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                }
                else
                {
                    var Position_Y = (dataCordenadY + 30 + dataWith / 10) - (PanelPerfil / 2);
                    List<ModelRenderElement> ListRenderElementDataType0 = null;
                    ListRenderElementDataType0 = WallTapeR.TapeRP_E70_270.setdListElement((dataCordenadX - 75) + (dataHeight / 10), 0, dataWith, datalong, dataHeight, dataCordenadY, 270, true, Position_Y, currentDefaultDisign.ExitingPanel2400);
                    if (ListRenderElementDataType0 != null)
                    {
                        foreach (var item in ListRenderElementDataType0)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.Type = item.Type;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_10_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataHeight, 1, 0, DimType.Horizontal, typeMesh, "", 0);
                var ListRenderElementEsqX27 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (datalong / 10), dataCordenadY + 30, DataWithOtherCorner);
                if (ListRenderElementEsqX27 != null)
                {
                    foreach (var item in ListRenderElementEsqX27)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                if (IsAngular == true)
                {
                    var ListRenderElementAng = SedAng90_180.setdListElement(dataCordenadX, 0, 0, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                    if (ListRenderElementAng != null)
                    {
                        foreach (var item in ListRenderElementAng)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                    var ListRenderElementEsq10_00 = ModuloWallEsqTEsq_10_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataWith,
                    datalong,
                    dataHeight,
                    dataCordenadX,
                    dataCordenadY,
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );
                    if (ListRenderElementEsq10_00 != null)
                    {
                        foreach (var item in ListRenderElementEsq10_00)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_10_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataWith, 2, 0, DimType.Horizontal, typeMesh, "", 0);
                if (IsAngular == true)
                {
                    //Panel 180
                    var ListRenderElementEsq70 = ModuloWallEsqTEsq_70_90.setdListElement(
                     typeMesh,
                     yWith,
                     xWith,
                     universalPanel,
                     currentDefaultDisign,
                     datalong,
                     dataHeight,
                     dataWith,
                     dataCordenadX - dataHeight / 10,
                     dataCordenadY - dataWith / 10,
                     type,
                     DataWithOtherCorner,
                     Tape_0,
                     Tape_180,
                     Tape_90,
                     Tape_270
                     );
                    if (ListRenderElementEsq70 != null)
                    {
                        foreach (var item in ListRenderElementEsq70)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_20_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataHeight, 1, 0, DimType.Horizontal, typeMesh, "", 0);
                //Right
                var ListRenderElementEsq90 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + ((datalong / 10) + 30), dataCordenadY + datalong / 10, 0);
                if (ListRenderElementEsq90 != null)
                {
                    foreach (var item in ListRenderElementEsq90)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                //Corner Lef
                var ListRenderElementEsqX270 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, 0);
                if (ListRenderElementEsqX270 != null)
                {
                    foreach (var item in ListRenderElementEsqX270)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                //Panel
                var ListRenderElementEsq20_00 = ModuloWallEsqTEsq_20_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataWith,
                    datalong,
                    dataHeight,
                    dataCordenadX + (dataHeight / 10),
                    dataCordenadY - (datalong / 10),
                    type,
                    DataWithOtherCorner,
                    Tape_0,
                    Tape_180,
                    Tape_90,
                    Tape_270
                    );
                if (ListRenderElementEsq20_00 != null)
                {
                    foreach (var item in ListRenderElementEsq20_00)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_30_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataHeight, 1, 0, DimType.Horizontal, typeMesh, "", 0);
                var CorrectionCornerX = ((dataHeight - datalong) / 10) - 30;
                var ListRenderElementEsqX270 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + CorrectionCornerX, dataCordenadY, DataWithOtherCorner);
                if (ListRenderElementEsqX270 != null)
                {
                    foreach (var item in ListRenderElementEsqX270)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                if (IsAngular == true)
                {
                    var ListRenderElementAng = SedAng90_0.setdListElement(dataCordenadX + dataHeight / 10, 0, 0, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                    if (ListRenderElementAng != null)
                    {
                        foreach (var item in ListRenderElementAng)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }

                    //Panel
                    var ListRenderElementEsq10_00 = ModuloWallEsqTEsq_10_00.setdListElement(
                        typeMesh,
                        yWith,
                        xWith,
                        universalPanel,
                        currentDefaultDisign,
                        dataWith,
                        datalong,
                        dataHeight,
                        dataCordenadX,
                        dataCordenadY,
                        type,
                        DataWithOtherCorner,
                        Tape_0,
                        Tape_180,
                        Tape_90,
                        Tape_270
                        );
                    if (ListRenderElementEsq10_00 != null)
                    {
                        foreach (var item in ListRenderElementEsq10_00)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.IdElement = item.IdElement;
                            element.CodeName = item.CodeName;
                            element.Element = item.Element;
                            element.ElementF = item.ElementF;
                            element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                            element.LongDimTypeVertical = item.LongDimTypeVertical;
                            element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                            element.ElementWood = item.ElementWood;
                            element.ElementUnion1 = item.ElementUnion1;
                            element.LongWood = item.LongWood;
                            element.heightWood = item.heightWood;
                            element.x = item.x;
                            element.y = item.y;
                            element.z = item.z;
                            element.XRotate = item.XRotate;
                            element.YRotate = item.YRotate;
                            element.ZRotate = item.ZRotate;
                            element.CodeName = item.CodeName;
                            element.Filter = item.Filter;
                            ListRenderElement.Add(element);
                        }
                    }

                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_30_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataWith, 2, 0, DimType.Horizontal, typeMesh, "", 0);
                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                var testLong = "450";
                if (dataWith > 450) { testLong = "600"; }
                if (dataWith > 600) { testLong = "750"; }
                if (dataWith > 750) { testLong = "900"; }
                if (dataWith > 900) { testLong = "1050"; }
                if (dataWith > 1050) { testLong = "1200"; }
                switch (testLong)
                {
                    case "450":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_45.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "600":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_60.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, "");
                        break;
                    case "750":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_75.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "900":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_90.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "1050":
                        //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1050.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                        break;
                    case "1200":
                        //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1200.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight,dataCordenadX, dataCordenadY);
                        break;
                }
                if (ListRenderElementPanel270 != null)
                {
                    foreach (var item in ListRenderElementPanel270)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_40_00")
            {
                long DataWithOtherCornerDef = 0;
                if (DataWithOtherCorner != null)
                {
                    DataWithOtherCornerDef = (long)DataWithOtherCorner;
                }
                if (DataWithOtherCornerDef == 0) { DataWithOtherCornerDef = datalong / 10; }
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq00 = WallCorner.CornerX_180_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + 30, dataCordenadY - (DataWithOtherCornerDef + 30), DataWithOtherCorner);


                if (ListRenderElementEsq00 != null)
                {
                    foreach (var item in ListRenderElementEsq00)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                var ListRenderElementEsq27 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY, DataWithOtherCorner);
                if (ListRenderElementEsq27 != null)
                {
                    foreach (var item in ListRenderElementEsq27)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_40_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                var testLong = "450";
                if (dataWith > 450) { testLong = "600"; }
                if (dataWith > 600) { testLong = "750"; }
                if (dataWith > 750) { testLong = "900"; }
                if (dataWith > 900) { testLong = "1050"; }
                if (dataWith > 1050) { testLong = "1200"; }
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataWith, 2, 0, DimType.Horizontal, typeMesh, "", 0);

                switch (testLong)
                {
                    case "450":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_45.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "600":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_60.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, Tape_0);
                        break;
                    case "750":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_75.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "900":
                        ListRenderElementPanel270 = Wall.WallCorner.CornerPanel000_90.setdListElement(type, currentDefaultDisign, datalong, dataHeight, dataWith, dataCordenadX, dataCordenadY, null);
                        break;
                    case "1050":
                        //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1050.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX, dataCordenadY);
                        break;
                    case "1200":
                        //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1200.setdListElement(type, currentDefaultDisign, dataWith, datalong, dataHeight,dataCordenadX, dataCordenadY);
                        break;
                }

                if (ListRenderElementPanel270 != null)
                {
                    foreach (var item in ListRenderElementPanel270)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_80_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq00 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (datalong / 10) + 30, dataCordenadY - 30, DataWithOtherCorner);
                if (ListRenderElementEsq00 != null)
                {
                    foreach (var item in ListRenderElementEsq00)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                var ListRenderElementEsq90 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (datalong / 10), dataCordenadY + 30, DataWithOtherCorner);
                if (ListRenderElementEsq90 != null)
                {
                    foreach (var item in ListRenderElementEsq90)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }
            if (typeMesh == "Esq_80_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)dataWith, 2, 0, DimType.Horizontal, typeMesh, "", 0);
                var ListRenderElementEsq70 = ModuloWallEsqTEsq_70_90.setdListElement(
                typeMesh,
                yWith,
                xWith,
                universalPanel,
                currentDefaultDisign,
                datalong,
                dataHeight,
                dataWith,
                dataCordenadX - (dataHeight / 10),
                dataCordenadY - (dataWith / 10),
                type,
                DataWithOtherCorner,
                Tape_0,
                Tape_180,
                Tape_90,
                Tape_270
                );
                if (ListRenderElementEsq70 != null)
                {
                    foreach (var item in ListRenderElementEsq70)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.ElementWood = item.ElementWood;
                        element.ElementUnion1 = item.ElementUnion1;
                        element.LongWood = item.LongWood;
                        element.heightWood = item.heightWood;
                        element.x = item.x;
                        element.y = item.y;
                        element.z = item.z;
                        element.XRotate = item.XRotate;
                        element.YRotate = item.YRotate;
                        element.ZRotate = item.ZRotate;
                        element.CodeName = item.CodeName;
                        element.Filter = item.Filter;
                        ListRenderElement.Add(element);
                    }
                }
                return ListRenderElement;
            }

            return null;
        }

        private static int parseInt(string testLong)
        {
            throw new NotImplementedException();
        }

        private static int getRestTypeHeight(int restHeight)
        {
            if (restHeight > 0 && restHeight <= 1200)
            {
                return 1200;
            }
            if (restHeight > 1200 && restHeight <= 2400)
            {
                return 2400;
            }
            return 2700;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}