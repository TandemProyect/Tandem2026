using DAL;
using Desing.Controllers;
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
        long dataHeight,
        long dataWith,
        long datalong,
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
                var ListRenderElementEsq180 = WallCorner.CornerX_180_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX - (datalong / 10), dataCordenadY, DataWithOtherCorner);
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
                var ListRenderElementEsqX00 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (datalong / 10), dataCordenadY + (datalong / 10), DataWithOtherCorner);
                if (ListRenderElementEsqX00 != null)
                {
                    foreach (var item in ListRenderElementEsqX00)
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
            if (typeMesh == "Esq_60_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                var testLong = "450";
                if (datalong > 450) { testLong = "600"; }
                if (datalong > 600) { testLong = "750"; }
                if (datalong > 750) { testLong = "900"; }
                if (datalong > 900) { testLong = "1050"; }
                if (datalong > 1050) { testLong = "1200"; }
                switch (testLong)
                {
                    case "450":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_45.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "600":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_60.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "750":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_75.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "900":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_90.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1050":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1200":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
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
            if (typeMesh == "Esq_50_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq50 = ModuloWallEsqTEsq_50_00.setdListElement(
                    typeMesh,
                    yWith,
                    xWith,
                    universalPanel,
                    currentDefaultDisign,
                    dataHeight,
                    dataWith,
                    datalong,
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
                datalong = dataHeight;
                dataHeight = dataWith;
                dataWith = datalongTemporal;

                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
                if (Tape_0 == "Universal_X")
                {
                    var dimTypeVertical = DimType.No;
                    var DimTypeH = DimType.Horizontal;
                    var Elevation = 0;
                    var ElevationDiwydag = 0;
                    int RestTypeHeight = 300;
                    int n = (int)((dataHeight + 149) / 2700);
                    var restHeight = (int)((dataHeight) - (2700 * n));
                    var nHeight = 0;
                    var Position_Y = (dataCordenadY + 30 + dataWith / 10) - (PanelPerfil / 2);
                    if (n >= 1)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            List<ModelRenderElement> ListRenderElementDataType0 = null;
                            ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY + 30, 270, true);
                            if (ListRenderElementDataType0 != null)
                            {
                                foreach (var item in ListRenderElementDataType0)
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
                            List<ModelRenderElement> ListRenderElementDataTypeUnion_R_0 = null;
                            ListRenderElementDataTypeUnion_R_0 = WallTapeR.TapeRU_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, Position_Y, 270, true);
                            if (ListRenderElementDataTypeUnion_R_0 != null)
                            {
                                foreach (var item in ListRenderElementDataTypeUnion_R_0)
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
                            nHeight = nHeight + 270;
                        }
                    }
                    if (restHeight > 0)
                    {
                        RestTypeHeight = getRestTypeHeight(restHeight);
                        if (RestTypeHeight == 2400)
                        {
                            if (currentDefaultDisign.ExitingPanel2400 == true)
                            {
                                //Paneles
                                List<ModelRenderElement> ListRenderElementDataType0 = null;
                                ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY + 30, 240, true);
                                if (ListRenderElementDataType0 != null)
                                {
                                    foreach (var item in ListRenderElementDataType0)
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
                                //Uniones
                                List<ModelRenderElement> ListRenderElementDataTypeUnion_0_R = null;
                                ListRenderElementDataTypeUnion_0_R = WallTapeR.TapeRU_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, Position_Y, 240, false);
                                if (ListRenderElementDataTypeUnion_0_R != null)
                                {
                                    foreach (var item in ListRenderElementDataTypeUnion_0_R)
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
                                //Paneles
                                List<ModelRenderElement> ListRenderElementDataType0 = null;
                                ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY + 30, 120, true);
                                if (ListRenderElementDataType0 != null)
                                {
                                    foreach (var item in ListRenderElementDataType0)
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
                                //Uniones
                                List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                                ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, Position_Y, 120, false);
                                if (ListRenderElementDataTypeUnion_270_R != null)
                                {
                                    foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                                //Paneles 2
                                nHeight = nHeight + 120;
                                List<ModelRenderElement> ListRenderElementDataType02Level = null;
                                ListRenderElementDataType02Level = WallTapeR.TapeRP_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY + 30, 120, true);
                                if (ListRenderElementDataType02Level != null)
                                {
                                    foreach (var item in ListRenderElementDataType02Level)
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
                                //Uniones 2
                                List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R2 = null;
                                ListRenderElementDataTypeUnion_270_R2 = WallTapeR.TapeRU_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, Position_Y, 120, false);
                                if (ListRenderElementDataTypeUnion_270_R2 != null)
                                {
                                    foreach (var item in ListRenderElementDataTypeUnion_270_R2)
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
                        if (RestTypeHeight == 1200)
                        {
                            //Paneles
                            List<ModelRenderElement> ListRenderElementDataType0 = null;
                            ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY + 30, 120, true);
                            if (ListRenderElementDataType0 != null)
                            {
                                foreach (var item in ListRenderElementDataType0)
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
                            List<ModelRenderElement> ListRenderElementDataType270 = null;
                            ListRenderElementDataType270 = WallTapeR.TapeRP_270.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120, false);

                            //Uniones
                            List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                            ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_0.setdListElement(dataCordenadX, nHeight, dataWith, datalong, Position_Y, 120, false);
                            if (ListRenderElementDataTypeUnion_270_R != null)
                            {
                                foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                    switch (testLong)
                    {
                        case "450":
                            ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_45.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                            break;
                        case "600":
                            ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_60.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                            break;
                        case "750":
                            ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_75.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                            break;
                        case "900":
                            ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_90.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                            break;
                        case "1050":
                            //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                            break;
                        case "1200":
                            //ListRenderElementPanel270 = Wall.WallCorner.Corner50_90_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
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
                    dataHeight,
                    dataWith,
                    datalong,
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
            if (typeMesh == "Esq_70_90")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsqX90 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataWith, datalong, dataHeight, dataCordenadX + (datalong / 10), dataCordenadY + (datalong / 10), DataWithOtherCorner);
                if (ListRenderElementEsqX90 != null)
                {
                    foreach (var item in ListRenderElementEsqX90)
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
            if (typeMesh == "Esq_70_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                if (IsAngular == true)
                {
                    var ListRenderElementAng = SedAng_180_Esq_270.setdListElement(dataCordenadX, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
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
                return ListRenderElement;
            }
            if (typeMesh == "Esq_80_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq00 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + (dataWith / 10) + 30, dataCordenadY - (dataWith / 10), DataWithOtherCorner);
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
                var ListRenderElementEsq90 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + (dataWith / 10), dataCordenadY + 30, DataWithOtherCorner);
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
            if (typeMesh == "Esq_10_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsqX27 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + (dataWith / 10), dataCordenadY + 30, DataWithOtherCorner);
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
                    var ListRenderElementAng = SedAng90_180.setdListElement(dataCordenadX, 0, 0, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
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
                return ListRenderElement;
            }
            if (typeMesh == "Esq_20_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq90 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, (dataCordenadX + (datalong / 10)) - 30, dataCordenadY + (dataWith / 10), DataWithOtherCorner);
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

                var ListRenderElementEsqX270 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, DataWithOtherCorner);
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


                return ListRenderElement;
            }
            if (typeMesh == "Esq_30_00")
            {
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsqX270 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, DataWithOtherCorner);
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
                    var ListRenderElementAng = SedAng90_0.setdListElement(dataCordenadX + datalong / 10, 0, 0, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
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
                return ListRenderElement;
            }
            if (typeMesh == "Esq_40_00")
            {
                long DataWithOtherCornerDef = 0;
                if (DataWithOtherCorner != null)
                {
                    DataWithOtherCornerDef = (long)DataWithOtherCorner;
                }
                if (DataWithOtherCornerDef == 0) { DataWithOtherCornerDef = dataWith / 10; }
                List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
                var ListRenderElementEsq00 = WallCorner.CornerX_180_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + 30, dataCordenadY - (DataWithOtherCornerDef + 30), DataWithOtherCorner);
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
                var ListRenderElementEsq27 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, DataWithOtherCorner);
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
            return null;
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