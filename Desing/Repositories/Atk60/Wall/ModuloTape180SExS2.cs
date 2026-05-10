using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTape180SExS2 : BaseController
    {
        private static bool HasPreviousModule = false;
        private static bool IsEndModule = false;
        private static bool IsFirstModule = false;
        private static long LastPanel = 0;
        private static long PanelPerfil = 12;
        private static long ElevationDiwydag = 0;
        private static bool Is2700 = false;
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedPanels30(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        //With 30
        private static void SedPanels30(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            var dimTypeVertical = DimType.No;
            var Elevation = 0;
            int RestTypeHeight = 300;
            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                Is2700 = true;
                LastPanel = 2700;
                for (int i = 0; i < n; i++)
                {
                    if (i == 0)
                    {
                        Elevation = 45;
                    }
                    else
                    {
                        Elevation = Elevation + 270;
                        CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, 10, dataCordenadY, nHeight, ListRenderElement, "90");
                        if (dataWith > 551 && dataWith <= 851)
                        {
                            CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, -40, dataCordenadY, nHeight, ListRenderElement, "90");
                        }
                    }
                    if ((dataWith) <= 352)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelReg270");
                        element.ElementF = Atk60Element.GetElement("PanelReg270F");
                        element.CodeName = "27104219";

                        element.x = endWallX;
                        element.z = element.z + nHeight;
                        element.y = dataCordenadY - ((dataWith / 10) / 2) + 35;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        ElevationDiwydag = ElevationDiwydag + 270;
                        nHeight = nHeight + 270;
                    }
                    else
                    {
                        if (dataWith > 350 && dataWith <= 551)
                        {
                            ModelRenderElement elementr = new ModelRenderElement();
                            elementr.Element = Atk60Element.GetElement("PanelReg270");
                            elementr.ElementF = Atk60Element.GetElement("PanelReg270F");
                            elementr.CodeName = "27104219";

                            elementr.x = endWallX;
                            elementr.z = elementr.z + nHeight;
                            elementr.y = dataCordenadY - (dataWith / 10) + 75;
                            elementr.XRotate = 270;
                            ListRenderElement.Add(elementr);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                        }
                        if (dataWith > 551 && dataWith <= 851)
                        {
                            ModelRenderElement elementr = new ModelRenderElement();
                            elementr.Element = Atk60Element.GetElement("Panel30270");
                            elementr.ElementF = Atk60Element.GetElement("Panel30270F");
                            elementr.CodeName = "27304205";

                            elementr.x = endWallX;
                            elementr.z = elementr.z + nHeight;
                            elementr.y = dataCordenadY - (dataWith / 10) + 30;
                            elementr.XRotate = 270;
                            ListRenderElement.Add(elementr);

                            ModelRenderElement elementr2 = new ModelRenderElement();
                            elementr2.Element = Atk60Element.GetElement("PanelReg270");
                            elementr2.ElementF = Atk60Element.GetElement("PanelReg270F");
                            elementr2.CodeName = "27104219";

                            elementr2.x = endWallX;
                            elementr2.z = elementr2.z + nHeight;
                            elementr2.y = dataCordenadY - (dataWith / 10) + 75 + 30;
                            elementr2.XRotate = 270;
                            ListRenderElement.Add(elementr2);
                            CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 45, ListRenderElement, "");
                            CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 135, ListRenderElement, "");
                            CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 225, ListRenderElement, "");
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                        }


                    }
                }
            }
            if (restHeight > 0)
            {
                RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    if (currentDefaultDisign.ExitingPanel2400 == true)
                    {
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert30_2400Element(endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                        LastPanel = 2400;
                    }
                    else
                    {
                        Insert30_1200Element(endWallX, 1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                        nHeight = nHeight + 120;
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert30_1200Element(endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                        nHeight = nHeight + 120;
                    }
                }
                if (RestTypeHeight == 1200)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert30_1200Element(endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                    LastPanel = 1200;
                }

            }
        }
        private static void Insert30_1200Element(long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight > 0)
            {
                var Elevation = nHeight + 120;
                CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, Elevation - 30, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, 10, dataCordenadY, nHeight, ListRenderElement, "90");

                //if (dataWith > 551 && dataWith <= 851)
                //{
                CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, -30, dataCordenadY, nHeight, ListRenderElement, "90");
                //}
                if (dataWith > 551 && dataWith <= 851)
                {
                    CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, -55, dataCordenadY, nHeight, ListRenderElement, "90");
                }


            }
            if ((dataWith) <= 350)
            {
                ModelRenderElement element = new ModelRenderElement();
                element.Element = Atk60Element.GetElement("PanelReg120");
                element.ElementF = Atk60Element.GetElement("PanelReg120F");
                element.CodeName = "12104120";
                element.x = endWallX;
                element.z = element.z + nHeight;
                element.y = dataCordenadY - ((dataWith / 10) / 2) + 35;
                element.XRotate = 270;
                ListRenderElement.Add(element);
                ElevationDiwydag = ElevationDiwydag + 120;
                nHeight = nHeight + 120;
            }
            else
            {
                if (dataWith > 350 && dataWith <= 551)
                {
                    ModelRenderElement elementr = new ModelRenderElement();
                    elementr.Element = Atk60Element.GetElement("PanelReg120");
                    elementr.ElementF = Atk60Element.GetElement("PanelReg120F");
                    elementr.CodeName = "12104120";
                    elementr.x = endWallX;
                    elementr.z = elementr.z + nHeight;
                    elementr.y = dataCordenadY - (dataWith / 10) + 75;
                    elementr.XRotate = 270;
                    ListRenderElement.Add(elementr);
                    ElevationDiwydag = ElevationDiwydag + 120;
                    nHeight = nHeight + 120;
                }
                if (dataWith > 551 && dataWith <= 851)
                {
                    ModelRenderElement elementr = new ModelRenderElement();
                    elementr.Element = Atk60Element.GetElement("Panel30120");
                    elementr.ElementF = Atk60Element.GetElement("Panel30120F");
                    elementr.CodeName = "12304211";
                    elementr.x = endWallX;
                    elementr.z = elementr.z + nHeight;
                    elementr.y = dataCordenadY - (dataWith / 10) + 30;
                    elementr.XRotate = 270;
                    ListRenderElement.Add(elementr);

                    ModelRenderElement elementr2 = new ModelRenderElement();
                    elementr2.Element = Atk60Element.GetElement("PanelReg120");
                    elementr2.ElementF = Atk60Element.GetElement("PanelReg120F");
                    elementr2.CodeName = "12104120";
                    elementr2.x = endWallX;
                    elementr2.z = elementr2.z + nHeight;
                    elementr2.y = dataCordenadY - (dataWith / 10) + 75 + 30;
                    elementr2.XRotate = 270;
                    ListRenderElement.Add(elementr2);
                    CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 35, ListRenderElement, "");
                    CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 115, ListRenderElement, "");
                    ElevationDiwydag = ElevationDiwydag + 120;
                    nHeight = nHeight + 120;
                }


            }
        }
        private static void Insert30_2400Element(long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight > 0)
            {
                var Elevation = nHeight + 240;
                CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, Elevation - 30, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, 10, dataCordenadY, nHeight, ListRenderElement, "90");

                //if (dataWith > 551 && dataWith <= 851)
                //{
                CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, -30, dataCordenadY, nHeight, ListRenderElement, "90");
                //}
                if (dataWith > 551 && dataWith <= 851)
                {
                    CommonElement.SedUnionHorizontal(2, PanelPerfil, 0, endWallX, -55, dataCordenadY, nHeight, ListRenderElement, "90");
                }
            }
            if ((dataWith) <= 350)
            {
                ModelRenderElement element = new ModelRenderElement();
                element.Element = Atk60Element.GetElement("PanelReg240");
                element.ElementF = Atk60Element.GetElement("PanelReg240F");
                element.CodeName = "24104224";
                element.x = endWallX;
                element.z = element.z + nHeight;
                element.y = dataCordenadY - ((dataWith / 10) / 2) + 35;
                element.XRotate = 270;
                ListRenderElement.Add(element);
                ElevationDiwydag = ElevationDiwydag + 120;
                nHeight = nHeight + 240;
            }
            else
            {
                if (dataWith > 350 && dataWith <= 551)
                {
                    ModelRenderElement elementr = new ModelRenderElement();
                    elementr.Element = Atk60Element.GetElement("PanelReg240");
                    elementr.ElementF = Atk60Element.GetElement("PanelReg240F");
                    elementr.CodeName = "24104224";
                    elementr.x = endWallX;
                    elementr.z = elementr.z + nHeight;
                    elementr.y = dataCordenadY - (dataWith / 10) + 75;
                    elementr.XRotate = 270;
                    ListRenderElement.Add(elementr);
                    ElevationDiwydag = ElevationDiwydag + 240;
                    nHeight = nHeight + 240;
                }
                if (dataWith > 551 && dataWith <= 851)
                {
                    ModelRenderElement elementr = new ModelRenderElement();
                    elementr.Element = Atk60Element.GetElement("Panel30240");
                    elementr.ElementF = Atk60Element.GetElement("Panel30240F");
                    elementr.CodeName = "24304244";
                    elementr.x = endWallX;
                    elementr.z = elementr.z + nHeight;
                    elementr.y = dataCordenadY - (dataWith / 10) + 30;
                    elementr.XRotate = 270;
                    ListRenderElement.Add(elementr);

                    ModelRenderElement elementr2 = new ModelRenderElement();
                    elementr2.Element = Atk60Element.GetElement("PanelReg240");
                    elementr2.ElementF = Atk60Element.GetElement("PanelReg240F");
                    elementr2.CodeName = "24104224";
                    elementr2.x = endWallX;
                    elementr2.z = elementr2.z + nHeight;
                    elementr2.y = dataCordenadY - (dataWith / 10) + 75 + 30;
                    elementr2.XRotate = 270;
                    ListRenderElement.Add(elementr2);
                    CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 40, ListRenderElement, "");
                    CommonElement.SedUnionVertical(0, PanelPerfil, 0, endWallX + PanelPerfil, 45, dataCordenadY - (dataWith / 10) + 30, nHeight + 200, ListRenderElement, "");
                    ElevationDiwydag = ElevationDiwydag + 240;
                    nHeight = nHeight + 240;
                }


            }
        }
        // End with 30
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