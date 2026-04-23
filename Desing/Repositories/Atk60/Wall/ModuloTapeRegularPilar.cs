using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTapeRegularPilar : BaseController
    {
        private static bool HasPreviousModule = false;
        private static bool IsEndModule = false;
        private static bool IsFirstModule = false;
        private static long LastPanel = 0;
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        private static string _codeName;
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedElement(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }

        private static void SedElement(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            InsertProp.SedProp(dataWith, 0, DataHeight, 1, dataCordenadX - 16, dataCordenadY, ListRenderElement, 0, 0, 0, 0, false);
            InsertProp.SedProp(dataWith, 0, DataHeight, 2, (dataCordenadX + (dataWith / 10)), dataCordenadY - 47, ListRenderElement, 0, 0, 0, 0, false);

            var dimTypeVertical = DimType.No;
            var DimTypeH = DimType.Horizontal;
            var Elevation = 0;
            var ElevationDiwydag = 0;
            int RestTypeHeight = 300;
            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    //Paneles
                    //Lado 0
                    List<ModelRenderElement> ListRenderElementDataType0 = null;
                    ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270, false);
                    if (ListRenderElementDataType0 != null)
                    {
                        foreach (var item in ListRenderElementDataType0)
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

                    List<ModelRenderElement> ListRenderElementDataTypeUnion_R_0 = null;
                    ListRenderElementDataTypeUnion_R_0 = WallTapeR.TapeRU_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270, false);
                    if (ListRenderElementDataTypeUnion_R_0 != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_R_0)
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

                    //Lado 270
                    List<ModelRenderElement> ListRenderElementDataType270 = null;
                    ListRenderElementDataType270 = WallTapeR.TapeRP_270.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270, false);
                    if (ListRenderElementDataType270 != null)
                    {
                        foreach (var item in ListRenderElementDataType270)
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
                    List<ModelRenderElement> ListRenderElementDataType180 = null;
                    //Lado 180
                    ListRenderElementDataType180 = WallTapeR.TapeRP_180.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270);
                    if (ListRenderElementDataType180 != null)
                    {
                        foreach (var item in ListRenderElementDataType180)
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
                    List<ModelRenderElement> ListRenderElementDataType90 = null;
                    //Lado 90
                    ListRenderElementDataType90 = WallTapeR.TapeRP_90.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270);
                    if (ListRenderElementDataType90 != null)
                    {
                        foreach (var item in ListRenderElementDataType90)
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
                    //Uniones
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L = null;
                    ListRenderElementDataTypeUnion_270_L = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 270);
                    if (ListRenderElementDataTypeUnion_270_L != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_270_L)
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
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_90_L = null;
                    ListRenderElementDataTypeUnion_90_L = WallTapeR.TapeRU_90_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 270);
                    if (ListRenderElementDataTypeUnion_90_L != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_90_L)
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
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_90_R = null;
                    ListRenderElementDataTypeUnion_90_R = WallTapeR.TapeRU_90_R.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 270);
                    if (ListRenderElementDataTypeUnion_90_R != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_90_R)
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
                        ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240, false);
                        if (ListRenderElementDataType0 != null)
                        {
                            foreach (var item in ListRenderElementDataType0)
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
                        List<ModelRenderElement> ListRenderElementDataType270 = null;
                        ListRenderElementDataType270 = WallTapeR.TapeRP_270.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240, false);
                        if (ListRenderElementDataType270 != null)
                        {
                            foreach (var item in ListRenderElementDataType270)
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
                        List<ModelRenderElement> ListRenderElementDataType180 = null;
                        ListRenderElementDataType180 = WallTapeR.TapeRP_180.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240);
                        if (ListRenderElementDataType180 != null)
                        {
                            foreach (var item in ListRenderElementDataType180)
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
                        List<ModelRenderElement> ListRenderElementDataType90 = null;
                        ListRenderElementDataType90 = WallTapeR.TapeRP_90.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240);
                        if (ListRenderElementDataType90 != null)
                        {
                            foreach (var item in ListRenderElementDataType90)
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
                        //Uniones
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                        ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240, false);
                        if (ListRenderElementDataTypeUnion_270_R != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L = null;
                        ListRenderElementDataTypeUnion_270_L = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 240);
                        if (ListRenderElementDataTypeUnion_270_L != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_L)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_L = null;
                        ListRenderElementDataTypeUnion_90_L = WallTapeR.TapeRU_90_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 240);
                        if (ListRenderElementDataTypeUnion_90_L != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_L)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_R = null;
                        ListRenderElementDataTypeUnion_90_R = WallTapeR.TapeRU_90_R.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 240);
                        if (ListRenderElementDataTypeUnion_90_R != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_R)
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
                    }
                    else
                    {
                        //Paneles
                        List<ModelRenderElement> ListRenderElementDataType0 = null;
                        ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataType0 != null)
                        {
                            foreach (var item in ListRenderElementDataType0)
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
                        List<ModelRenderElement> ListRenderElementDataType270 = null;
                        ListRenderElementDataType270 = WallTapeR.TapeRP_270.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataType270 != null)
                        {
                            foreach (var item in ListRenderElementDataType270)
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
                        List<ModelRenderElement> ListRenderElementDataType180 = null;
                        ListRenderElementDataType180 = WallTapeR.TapeRP_180.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataType180 != null)
                        {
                            foreach (var item in ListRenderElementDataType180)
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
                        List<ModelRenderElement> ListRenderElementDataType90 = null;
                        ListRenderElementDataType90 = WallTapeR.TapeRP_90.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataType90 != null)
                        {
                            foreach (var item in ListRenderElementDataType90)
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
                        //Uniones
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                        ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataTypeUnion_270_R != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L = null;
                        ListRenderElementDataTypeUnion_270_L = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_270_L != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_L)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_L = null;
                        ListRenderElementDataTypeUnion_90_L = WallTapeR.TapeRU_90_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_90_L != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_L)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_R = null;
                        ListRenderElementDataTypeUnion_90_R = WallTapeR.TapeRU_90_R.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_90_R != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_R)
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

                        nHeight = nHeight + 120;

                        List<ModelRenderElement> ListRenderElementDataType02Level = null;
                        ListRenderElementDataType02Level = WallTapeR.TapeRP_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataType02Level != null)
                        {
                            foreach (var item in ListRenderElementDataType02Level)
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
                        List<ModelRenderElement> ListRenderElementDataType27002Level = null;
                        ListRenderElementDataType27002Level = WallTapeR.TapeRP_270.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataType27002Level != null)
                        {
                            foreach (var item in ListRenderElementDataType27002Level)
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
                        List<ModelRenderElement> ListRenderElementDataType18002Level = null;
                        ListRenderElementDataType18002Level = WallTapeR.TapeRP_180.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataType18002Level != null)
                        {
                            foreach (var item in ListRenderElementDataType18002Level)
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
                        List<ModelRenderElement> ListRenderElementDataType9002Level = null;
                        ListRenderElementDataType9002Level = WallTapeR.TapeRP_90.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataType9002Level != null)
                        {
                            foreach (var item in ListRenderElementDataType9002Level)
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
                        //Uniones
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R02Level = null;
                        ListRenderElementDataTypeUnion_270_R02Level = WallTapeR.TapeRU_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                        if (ListRenderElementDataTypeUnion_270_R02Level != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_R02Level)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L02Level = null;
                        ListRenderElementDataTypeUnion_270_L02Level = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_270_L02Level != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_L02Level)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_L02Level = null;
                        ListRenderElementDataTypeUnion_90_L02Level = WallTapeR.TapeRU_90_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_90_L02Level != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_L02Level)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_90_R02Level = null;
                        ListRenderElementDataTypeUnion_90_R02Level = WallTapeR.TapeRU_90_R.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_90_R02Level != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_90_R02Level)
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
                    }
                }
                if (RestTypeHeight == 1200)
                {
                    //Paneles
                    List<ModelRenderElement> ListRenderElementDataType0 = null;
                    ListRenderElementDataType0 = WallTapeR.TapeRP_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                    if (ListRenderElementDataType0 != null)
                    {
                        foreach (var item in ListRenderElementDataType0)
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
                    List<ModelRenderElement> ListRenderElementDataType270 = null;
                    ListRenderElementDataType270 = WallTapeR.TapeRP_270.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                    if (ListRenderElementDataType270 != null)
                    {
                        foreach (var item in ListRenderElementDataType270)
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
                    List<ModelRenderElement> ListRenderElementDataType180 = null;
                    ListRenderElementDataType180 = WallTapeR.TapeRP_180.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                    if (ListRenderElementDataType180 != null)
                    {
                        foreach (var item in ListRenderElementDataType180)
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
                    List<ModelRenderElement> ListRenderElementDataType90 = null;
                    ListRenderElementDataType90 = WallTapeR.TapeRP_90.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                    if (ListRenderElementDataType90 != null)
                    {
                        foreach (var item in ListRenderElementDataType90)
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
                    //Uniones
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                    ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_0.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120, false);
                    if (ListRenderElementDataTypeUnion_270_R != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L = null;
                    ListRenderElementDataTypeUnion_270_L = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                    if (ListRenderElementDataTypeUnion_270_L != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_270_L)
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
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_90_L = null;
                    ListRenderElementDataTypeUnion_90_L = WallTapeR.TapeRU_90_L.setdListElement(dataCordenadX, nHeight, dataWith, datalong, dataCordenadY, 120);
                    if (ListRenderElementDataTypeUnion_90_L != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_90_L)
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
                    List<ModelRenderElement> ListRenderElementDataTypeUnion_90_R = null;
                    ListRenderElementDataTypeUnion_90_R = WallTapeR.TapeRU_90_R.setdListElement(endWallX, nHeight, dataWith, datalong, dataCordenadY, 120);
                    if (ListRenderElementDataTypeUnion_90_R != null)
                    {
                        foreach (var item in ListRenderElementDataTypeUnion_90_R)
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
                }
            }
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
        //Realizar Comun Dywidag
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