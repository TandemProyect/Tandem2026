using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTape_0 : BaseController
    {

        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        internal static List<ModelRenderElement> setdListElement(bool CHeck750R, long EndWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

            SedPanels30(CHeck750R, EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        //With 30
        private static void SedPanels30(bool CHeck750R, long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            var realdataWith = dataWith;
            dataWith = 300;
            if (realdataWith >= 450)
            { dataWith = 450; }
            if (realdataWith >= 600)
            { dataWith = 600; }
            if (realdataWith >= 750)
            { dataWith = 750; }
            if (realdataWith >= 900)
            { dataWith = 900; }
            if (realdataWith >= 1050)
            { dataWith = 1050; }
            if (realdataWith >= 1200)
            { dataWith = 1200; }
            var suplement = (realdataWith - dataWith) / 10;
            dataCordenadY = dataCordenadY - suplement;
            var dimTypeVertical = DimType.No;
            var Elevation = 0;
            var ElevationDiwydag = 0;
            int RestTypeHeight = 300;
            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                Is2700 = true;
                for (int i = 0; i < n; i++)
                {
                    if (i == 0)
                    {
                        Elevation = 45;
                    }
                    else
                    {
                        Elevation = Elevation + 270;
                        if (suplement < 0.1)
                        {
                            CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        }
                    }
                    if (suplement < 0.1)
                    {
                        CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 225, ListRenderElement, "");

                        CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 45, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 135, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 225, ListRenderElement, dataWith / 10, "");

                        CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 225, ListRenderElement, "");

                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 225, ListRenderElement, "");
                    }
                    ModelRenderElement element = new ModelRenderElement();
                    switch (dataWith)
                    {
                        case 300:
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                            }
                            element.Element = Atk60Element.GetElement("Panel30270");
                            element.ElementF = Atk60Element.GetElement("Panel30270F");
                            element.CodeName = "27304205";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                            break;
                        case 450:
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                            }
                            element.Element = Atk60Element.GetElement("Panel45270");
                            element.ElementF = Atk60Element.GetElement("Panel45270F");
                            element.CodeName = "27454206";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                            break;
                        case 600:
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                            }
                            element.Element = Atk60Element.GetElement("Panel60270");
                            element.ElementF = Atk60Element.GetElement("Panel60270F");
                            element.CodeName = "27604207";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                            break;
                        case 750:

                            if (CHeck750R == true)
                            {
                                if (i != 0)
                                {
                                    CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                                }
                                element.Element = Atk60Element.GetElement("PanelReg270");
                                element.ElementF = Atk60Element.GetElement("PanelReg270F");
                                element.CodeName = "27104219";

                                element.x = endWallX;
                                element.z = element.z + nHeight;
                                element.y = dataCordenadY;
                                element.XRotate = 270;
                                ListRenderElement.Add(element);
                                nHeight = nHeight + 270;
                            }
                            else
                            {
                                if (i != 0)
                                {
                                    CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 15, nHeight, ListRenderElement, dataWith / 10);
                                }
                                element.Element = Atk60Element.GetElement("Panel45270");
                                element.ElementF = Atk60Element.GetElement("Panel45270F");
                                element.CodeName = "27454206";

                                element.x = endWallX;
                                element.z = element.z + nHeight;
                                element.y = dataCordenadY;
                                element.XRotate = 270;
                                ListRenderElement.Add(element);
                                if (i != 0)
                                {
                                    CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 65, nHeight, ListRenderElement, dataWith / 10);
                                }
                                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 45, ListRenderElement, "");
                                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 135, ListRenderElement, "");
                                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 225, ListRenderElement, "");
                                ModelRenderElement element2 = new ModelRenderElement();
                                element2.Element = Atk60Element.GetElement("Panel30270");
                                element2.ElementF = Atk60Element.GetElement("Panel30270F");
                                element2.CodeName = "27304205";

                                element2.x = endWallX;
                                element2.z = element2.z + nHeight;
                                element2.y = dataCordenadY - 45;
                                element2.XRotate = 270;
                                ListRenderElement.Add(element2);
                                ElevationDiwydag = ElevationDiwydag + 270;
                                nHeight = nHeight + 270;
                            }
                            break;

                        case 900:
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                            }
                            element.Element = Atk60Element.GetElement("Panel90270");
                            element.ElementF = Atk60Element.GetElement("Panel90270F");
                            element.CodeName = "27904209";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                            break;

                        case 1050:
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 25, nHeight, ListRenderElement, dataWith / 10);
                            }
                            element.Element = Atk60Element.GetElement("Panel45270");
                            element.ElementF = Atk60Element.GetElement("Panel45270F");
                            element.CodeName = "27454206";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            if (i != 0)
                            {
                                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 60, nHeight, ListRenderElement, dataWith / 10);
                            }
                            CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 45, ListRenderElement, "");
                            CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 135, ListRenderElement, "");
                            CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 225, ListRenderElement, "");

                            ModelRenderElement element3 = new ModelRenderElement();
                            element3.Element = Atk60Element.GetElement("Panel60270");
                            element3.ElementF = Atk60Element.GetElement("Panel60270F");
                            element3.CodeName = "27604207";

                            element3.x = endWallX;
                            element3.z = element3.z + nHeight;
                            element3.y = dataCordenadY - 45;
                            element3.XRotate = 270;
                            ListRenderElement.Add(element3);
                            ElevationDiwydag = ElevationDiwydag + 270;
                            nHeight = nHeight + 270;
                            break;
                        case 1200:

                            //Insertar grapa horizontal
                            //if (i != 0)
                            //{
                            //    CommonElement.SedUnionHorizontalTape(0, dataCordenadX + 17, dataCordenadY - 115, nHeight, ListRenderElement, dataWith / 10);
                            //}
                            element.Element = Atk60Element.GetElement("Panel90120T");
                            element.ElementF = Atk60Element.GetElement("Panel90120TF");

                            element.CodeName = "12904215";
                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);
                            ModelRenderElement element4 = new ModelRenderElement();
                            element4.Element = Atk60Element.GetElement("Panel90120T");
                            element4.ElementF = Atk60Element.GetElement("Panel90120TF");
                            element4.CodeName = "12904215";
                            element4.x = endWallX;
                            element4.z = element4.z + nHeight + 90;
                            element4.y = dataCordenadY;
                            element4.XRotate = 270;
                            ListRenderElement.Add(element4);
                            ModelRenderElement element5 = new ModelRenderElement();
                            element5.Element = Atk60Element.GetElement("Panel90120T");
                            element5.ElementF = Atk60Element.GetElement("Panel90120TF");
                            element5.CodeName = "12904215";
                            element5.x = endWallX;
                            element5.z = element5.z + nHeight + 180;
                            element5.y = dataCordenadY;
                            element5.XRotate = 270;
                            ListRenderElement.Add(element5);
                            nHeight = nHeight + 270;
                            break;
                        default:
                            break;
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
                        Insert30_2400Element(suplement, CHeck750R, endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                    }
                    else
                    {
                        Insert30_1200Element(suplement, CHeck750R, endWallX, 1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                        nHeight = nHeight + 120;
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert30_1200Element(suplement, CHeck750R, endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
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
                    Insert30_1200Element(suplement, CHeck750R, endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                }
                if (RestTypeHeight == 2700)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert30_2700Element(suplement, endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                }
            }
        }
        private static void Insert30_2700Element(long suplement, long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                if (suplement < 0.1)
                {
                    CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
                }
            }
            ModelRenderElement element = new ModelRenderElement();
            switch (dataWith)
            {
                case 300:
                    element.Element = Atk60Element.GetElement("Panel30270");
                    element.ElementF = Atk60Element.GetElement("Panel302270F");
                    element.CodeName = "27304205";
                    element.z = element.z + nHeight;
                    element.x = endWallX;
                    element.y = dataCordenadY;
                    ListRenderElement.Add(element);
                    break;
                case 450:
                    element.Element = Atk60Element.GetElement("Panel45270");
                    element.ElementF = Atk60Element.GetElement("Panel45270F");
                    element.CodeName = "27454206";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 600:
                    element.Element = Atk60Element.GetElement("Panel60270");
                    element.ElementF = Atk60Element.GetElement("Panel60270F");
                    element.CodeName = "27604207";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 750:
                    element.Element = Atk60Element.GetElement("Panel45270");
                    element.ElementF = Atk60Element.GetElement("Panel45270F");
                    element.CodeName = "27454206";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 225, ListRenderElement, "");

                    ModelRenderElement element2 = new ModelRenderElement();
                    element2.Element = Atk60Element.GetElement("Panel30270");
                    element2.ElementF = Atk60Element.GetElement("Panel30270F");
                    element2.CodeName = "27304205";
                    element2.x = endWallX;
                    element2.z = element.z + nHeight;
                    element2.y = dataCordenadY - 45;
                    element2.XRotate = 270;
                    ListRenderElement.Add(element2);
                    break;
                case 900:
                    element.Element = Atk60Element.GetElement("Panel90270");
                    element.ElementF = Atk60Element.GetElement("Panel90270F");
                    element.CodeName = "27904209";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 1050:
                    element.Element = Atk60Element.GetElement("Panel45270");
                    element.ElementF = Atk60Element.GetElement("Panel45270F");
                    element.CodeName = "27454206";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);

                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 225, ListRenderElement, "");
                    ModelRenderElement element3 = new ModelRenderElement();
                    element3.Element = Atk60Element.GetElement("Panel60270");
                    element3.ElementF = Atk60Element.GetElement("Panel60270F");
                    element3.CodeName = "27604207";
                    element3.x = endWallX;
                    element3.z = element3.z + nHeight;
                    element3.y = dataCordenadY - 45;
                    element3.XRotate = 270;
                    ListRenderElement.Add(element3);
                    break;
                case 1200:
                    element.Element = Atk60Element.GetElement("Panel90120T");
                    element.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element.CodeName = "12904215";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    ModelRenderElement element4 = new ModelRenderElement();
                    element4.Element = Atk60Element.GetElement("Panel90120T");
                    element4.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element4.CodeName = "12904215";
                    element4.x = endWallX;
                    element4.z = element4.z + nHeight + 90;
                    element4.y = dataCordenadY;
                    element4.XRotate = 270;
                    ListRenderElement.Add(element4);
                    ModelRenderElement element5 = new ModelRenderElement();
                    element5.Element = Atk60Element.GetElement("Panel90120T");
                    element5.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element5.CodeName = "12904215";
                    element5.x = endWallX;
                    element5.z = element5.z + nHeight + 180;
                    element5.y = dataCordenadY;
                    element5.XRotate = 270;
                    ListRenderElement.Add(element5);
                    break;
                default:
                    break;
            }
        }
        private static void Insert30_1200Element(long suplement, bool CHeck750R, long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                if (suplement < 0.1)
                {
                    CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
                }
            }
            if (suplement < 0.1)
            {
                CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 20, ListRenderElement, "");
                CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 80, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 20, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 80, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 20, ListRenderElement, "");
                CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 80, ListRenderElement, "");
                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 20, ListRenderElement, "");
                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 80, ListRenderElement, "");
            }
            ModelRenderElement element = new ModelRenderElement();
            switch (dataWith)
            {
                case 300:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel30120");
                    element.ElementF = Atk60Element.GetElement("Panel30120F");
                    element.CodeName = "12304211";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 450:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel45120");
                    element.ElementF = Atk60Element.GetElement("Panel45120F");
                    element.CodeName = "12454212";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 600:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel60120");
                    element.ElementF = Atk60Element.GetElement("Panel60120F");
                    element.CodeName = "12604213";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 750:
                    if (CHeck750R == true)
                    {
                        if (nHeight != 0)
                        {
                            CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                        }
                        element.Element = Atk60Element.GetElement("PanelReg120");
                        element.ElementF = Atk60Element.GetElement("PanelReg120F");
                        element.CodeName = "12104120";

                        element.x = endWallX;
                        element.z = element.z + nHeight;
                        element.y = dataCordenadY;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 270;
                    }
                    else
                    {
                        if (nHeight != 0)
                        {
                            CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 15, nHeight, ListRenderElement, dataWith / 10);
                        }
                        element.Element = Atk60Element.GetElement("Panel45120");
                        element.ElementF = Atk60Element.GetElement("Panel45120F");
                        element.CodeName = "12454212";
                        element.x = endWallX;
                        element.z = element.z + nHeight;
                        element.y = dataCordenadY;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        ModelRenderElement element2 = new ModelRenderElement();
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 20, ListRenderElement, "");
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 80, ListRenderElement, "");
                        element2.Element = Atk60Element.GetElement("Panel30120");
                        element2.ElementF = Atk60Element.GetElement("Panel30120F");
                        element2.CodeName = "12304211";
                        element2.x = endWallX;
                        element2.z = element2.z + nHeight;
                        element2.y = dataCordenadY - 45;
                        element2.XRotate = 270;
                        ListRenderElement.Add(element2);
                    }
                    break;
                case 900:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel90120");
                    element.ElementF = Atk60Element.GetElement("Panel90120F");
                    element.CodeName = "12904215";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 1050:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 30, nHeight, ListRenderElement, dataWith / 10);
                    }
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 100, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel45120");
                    element.ElementF = Atk60Element.GetElement("Panel45120F");
                    element.CodeName = "12454212";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);

                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 20, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 80, ListRenderElement, "");

                    ModelRenderElement element3 = new ModelRenderElement();
                    element3.Element = Atk60Element.GetElement("Panel60120");
                    element3.ElementF = Atk60Element.GetElement("Panel60120F");
                    element3.CodeName = "12604213";
                    element3.x = endWallX;
                    element3.z = element3.z + nHeight;
                    element3.y = dataCordenadY - 45;
                    element3.XRotate = 270;
                    ListRenderElement.Add(element3);
                    break;
                // aqui bug
                case 1200:

                    //Inserat piezas de union horizontal
                    element.Element = Atk60Element.GetElement("Panel90120T");
                    element.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element.CodeName = "12904215";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    ModelRenderElement element4 = new ModelRenderElement();
                    element4.Element = Atk60Element.GetElement("Panel90120T");
                    element4.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element4.CodeName = "12904215";
                    element4.x = endWallX;
                    element4.z = element4.z + nHeight + 90;
                    element4.y = dataCordenadY;
                    element4.XRotate = 270;
                    ListRenderElement.Add(element4);
                    ModelRenderElement element5 = new ModelRenderElement();
                    element5.Element = Atk60Element.GetElement("Panel90120T");
                    element5.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element5.CodeName = "12904215";
                    element5.x = endWallX;
                    element5.z = element5.z + nHeight + 180;
                    element5.y = dataCordenadY;
                    element5.XRotate = 270;
                    ListRenderElement.Add(element5);
                    break;
                default:
                    break;
            }
        }
        private static void Insert30_2400Element(long suplement, bool CHeck750R, long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                if (suplement < 0.1)
                {
                    CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -15, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
                }
            }

            if (suplement < 0.1)
            {
                CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 40, ListRenderElement, "");
                CommonElement.SedUnionVertical(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 200, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 40, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVerticalMirror(1, PanelPerfil, 0, endWallX - 2, 0, dataCordenadY, nHeight + 200, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 40, ListRenderElement, "");
                CommonElement.SedUnionVertical(2, PanelPerfil, 0, endWallX, 0, dataCordenadY - (dataWith / 10) + 4, nHeight + 200, ListRenderElement, "");
                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 40, ListRenderElement, "");
                CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, 0, dataCordenadY, nHeight + 200, ListRenderElement, "");
            }
            ModelRenderElement element = new ModelRenderElement();
            switch (dataWith)
            {
                case 300:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel30240");
                    element.ElementF = Atk60Element.GetElement("Panel30240F");
                    element.CodeName = "24304244";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 450:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel45240");
                    element.ElementF = Atk60Element.GetElement("Panel45240F");
                    element.CodeName = "24454243";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 600:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel60240");
                    element.ElementF = Atk60Element.GetElement("Panel60240F");
                    element.CodeName = "24604242";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 750:
                    if (CHeck750R == true)
                    {
                        if (nHeight != 0)
                        {
                            CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                        }
                        element.Element = Atk60Element.GetElement("PanelReg240");
                        element.ElementF = Atk60Element.GetElement("PanelReg240F");
                        element.CodeName = "24104224";

                        element.x = endWallX;
                        element.z = element.z + nHeight;
                        element.y = dataCordenadY;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 270;
                    }
                    else
                    {
                        if (nHeight != 0)
                        {
                            CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 15, nHeight, ListRenderElement, dataWith / 10);
                        }
                        element.Element = Atk60Element.GetElement("Panel45240");
                        element.ElementF = Atk60Element.GetElement("Panel45240F");
                        element.CodeName = "24454243";
                        element.x = endWallX;
                        element.z = element.z + nHeight;
                        element.y = dataCordenadY;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        ModelRenderElement element2 = new ModelRenderElement();
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 20, ListRenderElement, "");
                        CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 80, ListRenderElement, "");
                        element2.Element = Atk60Element.GetElement("Panel30240");
                        element2.ElementF = Atk60Element.GetElement("Panel30240F");
                        element2.CodeName = "24304244";
                        element2.x = endWallX;
                        element2.z = element2.z + nHeight;
                        element2.y = dataCordenadY - 45;
                        element2.XRotate = 270;
                        ListRenderElement.Add(element2);
                    }
                    break;
                case 900:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel90240");
                    element.ElementF = Atk60Element.GetElement("Panel90240F");
                    element.CodeName = "24904240";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    break;
                case 1050:
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 30, nHeight, ListRenderElement, dataWith / 10);
                    }
                    if (nHeight != 0)
                    {
                        CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - 100, nHeight, ListRenderElement, dataWith / 10);
                    }
                    element.Element = Atk60Element.GetElement("Panel45240");
                    element.ElementF = Atk60Element.GetElement("Panel45240F");
                    element.CodeName = "24454243";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);

                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 20, ListRenderElement, "");
                    CommonElement.SedUnionVertical(3, PanelPerfil, 0, endWallX, -45, dataCordenadY, nHeight + 80, ListRenderElement, "");

                    ModelRenderElement element3 = new ModelRenderElement();
                    element3.Element = Atk60Element.GetElement("Panel60240");
                    element3.ElementF = Atk60Element.GetElement("Panel60240F");
                    element3.CodeName = "24604242";
                    element3.x = endWallX;
                    element3.z = element3.z + nHeight;
                    element3.y = dataCordenadY - 45;
                    element3.XRotate = 270;
                    ListRenderElement.Add(element3);
                    break;
                // aqui bug
                case 1200:

                    //Insertar elemento de union horizontal
                    element.Element = Atk60Element.GetElement("Panel90120T");
                    element.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element.CodeName = "12904215";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                    ModelRenderElement element4 = new ModelRenderElement();
                    element4.Element = Atk60Element.GetElement("Panel90120T");
                    element4.ElementF = Atk60Element.GetElement("Panel90120TF");
                    element4.CodeName = "12904215";
                    element4.x = endWallX;
                    element4.z = element4.z + nHeight + 90;
                    element4.y = dataCordenadY;
                    element4.XRotate = 270;
                    ListRenderElement.Add(element4);
                    ModelRenderElement element5 = new ModelRenderElement();
                    element5.Element = Atk60Element.GetElement("Panel60120T");
                    element5.ElementF = Atk60Element.GetElement("Panel60120TF");
                    element5.CodeName = "12604213";
                    element5.x = endWallX;
                    element5.z = element5.z + nHeight + 180;
                    element5.y = dataCordenadY;
                    element5.XRotate = 270;
                    ListRenderElement.Add(element5);



                    break;
                default:
                    break;
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