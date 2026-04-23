using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTapeRightRegular75 : BaseController
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

            SedPanels30(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            SedAngBottom(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            SedAngTop(EndWallX + 1, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        //With 30
        private static void SedPanels30(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
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
                Is2700 = true;
                LastPanel = 2700;
                for (int i = 0; i < n; i++)
                {
                    if (i == 0)
                    {
                        DimTypeH = DimType.Horizontal;
                        Elevation = 45;
                    }
                    else
                    {
                        DimTypeH = 0;
                        Elevation = Elevation + 270;
                        CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, Elevation - 30, ListRenderElement, dataWith / 10, "");
                    }

                    if ((dataWith) < 350)
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
                        //MAL
                        if ((dataWith) < 750)
                        {
                            ModelRenderElement element = new ModelRenderElement();
                            element.Element = Atk60Element.GetElement("PanelReg270");
                            element.ElementF = Atk60Element.GetElement("PanelReg270F");
                            element.CodeName = "27104219";

                            element.x = endWallX;
                            element.z = element.z + nHeight;
                            element.y = dataCordenadY - ((dataWith / 10) / 2) + 75;
                            element.XRotate = 270;
                            ListRenderElement.Add(element);

                            ModelRenderElement element2 = new ModelRenderElement();
                            element2.Element = Atk60Element.GetElement("PanelReg270");
                            element2.ElementF = Atk60Element.GetElement("PanelReg270F");
                            element2.CodeName = "27104219";

                            element2.x = endWallX;
                            element2.z = element2.z + nHeight;
                            element2.y = dataCordenadY - ((dataWith / 10) / 2);
                            element2.XRotate = 270;
                            ListRenderElement.Add(element2);

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
                if (RestTypeHeight == 2700)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert30_2700Element(endWallX, IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, datalong);
                    LastPanel = 1200;
                }
            }
        }
        private static void Insert30_2700Element(long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
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
        private static void Insert30_1200Element(long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -(dataWith / 10) / 2, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
            }

            ModelRenderElement element = new ModelRenderElement();
            switch (dataWith)
            {
                case 300:
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
                    element.Element = Atk60Element.GetElement("Panel45120");
                    element.ElementF = Atk60Element.GetElement("Panel45120F");
                    element.CodeName = "12454212";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);

                    ModelRenderElement element2 = new ModelRenderElement();
                    element2.Element = Atk60Element.GetElement("Panel30120");
                    element2.ElementF = Atk60Element.GetElement("Panel30120F");
                    element2.CodeName = "12304211";
                    element2.x = endWallX;
                    element2.z = element.z + nHeight;
                    element2.y = dataCordenadY - 45;
                    element2.XRotate = 270;
                    ListRenderElement.Add(element2);
                    break;
                case 900:
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
                    element.Element = Atk60Element.GetElement("Panel45120");
                    element.ElementF = Atk60Element.GetElement("Panel45120F");
                    element.CodeName = "12454212";
                    element.x = endWallX;
                    element.z = element.z + nHeight;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
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
        private static void Insert30_2400Element(long endWallX, long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long datalong)
        {
            if (nHeight != 0)
            {
                CommonElement.UnionRijiMirror(1, 2, 12, 0, endWallX, -15, dataCordenadY - 0, nHeight, ListRenderElement, dataWith / 10, "");
            }

            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel30240");
            element.ElementF = Atk60Element.GetElement("Panel30240F");
            element.CodeName = "24304244";

            element.x = endWallX;
            element.z = element.z + nHeight;
            element.y = dataCordenadY;
            element.XRotate = 270;
            ListRenderElement.Add(element);
        }
        // End with 30
        private static void SedAngBottom(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            int n = (int)((DataHeight + 299) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            var Elevation = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {

                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = dataCordenadY + 6;
                    elementGancho.z = elementGancho.z + nHeight + 55;
                    elementGancho.XRotate = 270;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija.CodeName = "10443020";
                    elementTuercaFija.x = endWallX + 12;
                    elementTuercaFija.y = dataCordenadY + 15;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 55;
                    elementTuercaFija.XRotate = 1;
                    elementTuercaFija.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija);

                    ModelRenderElement elementGancho2 = new ModelRenderElement();
                    elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho2.ElementF = "";
                    elementGancho2.CodeName = "1920811";
                    elementGancho2.Type = "";
                    elementGancho2.x = endWallX;
                    elementGancho2.y = dataCordenadY + 6;
                    elementGancho2.z = elementGancho2.z + nHeight + 135;
                    elementGancho2.XRotate = 270;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija2.CodeName = "10443020";
                    elementTuercaFija2.x = endWallX + 12;
                    elementTuercaFija2.y = dataCordenadY + 15;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);

                    ModelRenderElement elementGancho3 = new ModelRenderElement();
                    elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho3.ElementF = "";
                    elementGancho3.CodeName = "1920811";
                    elementGancho3.Type = "";
                    elementGancho3.x = endWallX;
                    elementGancho3.y = dataCordenadY + 6;
                    elementGancho3.z = elementGancho3.z + nHeight + 215;
                    elementGancho3.XRotate = 270;
                    ListRenderElement.Add(elementGancho3);

                    ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
                    elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija3.CodeName = "10443020";
                    elementTuercaFija3.x = endWallX + 12;
                    elementTuercaFija3.y = dataCordenadY + 15;
                    elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 215;
                    elementTuercaFija3.XRotate = 1;
                    elementTuercaFija3.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija3);


                    nHeight = nHeight + 270;
                    Elevation = Elevation + 270;
                }

            }

            if (restHeight > 0)
            {
                var RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    if (currentDefaultDisign.ExitingPanel2400 == true)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelExt240");
                        element.CodeName = "24000000";
                        element.Type = "";
                        element.x = endWallX;
                        element.y = dataCordenadY;
                        element.z = element.z + nHeight;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        Elevation = Elevation + 240;
                    }
                    else
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelExt120");
                        element.CodeName = "12000000";
                        element.Type = "";
                        element.x = endWallX;
                        element.y = dataCordenadY;
                        element.z = element.z + nHeight;
                        element.XRotate = 270;
                        ListRenderElement.Add(element);
                        Elevation = Elevation + 120;

                        ModelRenderElement element2 = new ModelRenderElement();
                        element2.Element = Atk60Element.GetElement("PanelExt120");
                        element2.CodeName = "12000000";
                        element2.Type = "";
                        element2.x = endWallX;
                        element2.y = dataCordenadY;
                        element2.XRotate = 270;
                        element2.z = element.z + nHeight;
                        ListRenderElement.Add(element2);
                    }

                }
                if (RestTypeHeight == 1200)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelExt120");
                    element.CodeName = "12000000";
                    element.Type = "";
                    element.x = endWallX;
                    element.y = dataCordenadY;
                    element.z = element.z + nHeight;
                    element.XRotate = 270;
                    ListRenderElement.Add(element);
                }
            }
        }
        private static void SedAngTop(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {

            int n = (int)((DataHeight + 299) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            var Elevation = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {

                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho.z = elementGancho.z + nHeight + 55;
                    elementGancho.XRotate = 90;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija.CodeName = "10443020";
                    elementTuercaFija.x = endWallX + 12;
                    elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 14;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 55;
                    elementTuercaFija.XRotate = 1;
                    elementTuercaFija.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija);

                    ModelRenderElement elementGancho2 = new ModelRenderElement();
                    elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho2.ElementF = "";
                    elementGancho2.CodeName = "1920811";
                    elementGancho2.Type = "";
                    elementGancho2.x = endWallX;
                    elementGancho2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho2.z = elementGancho2.z + nHeight + 135;
                    elementGancho2.XRotate = 90;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija2.CodeName = "10443020";
                    elementTuercaFija2.x = endWallX + 12;
                    elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 14;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);

                    ModelRenderElement elementGancho3 = new ModelRenderElement();
                    elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho3.ElementF = "";
                    elementGancho3.CodeName = "1920811";
                    elementGancho3.Type = "";
                    elementGancho3.x = endWallX;
                    elementGancho3.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho3.z = elementGancho3.z + nHeight + 215;
                    elementGancho3.XRotate = 90;
                    ListRenderElement.Add(elementGancho3);

                    ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
                    elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija3.CodeName = "10443020";
                    elementTuercaFija3.x = endWallX + 12;
                    elementTuercaFija3.y = (dataCordenadY - dataWith / 10) - 14;
                    elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 215;
                    elementTuercaFija3.XRotate = 1;
                    elementTuercaFija3.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija3);
                    nHeight = nHeight + 270;
                    Elevation = Elevation + 270;
                }
            }

            if (restHeight > 0)
            {
                var RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    if (currentDefaultDisign.ExitingPanel2400 == true)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelExt240");
                        element.CodeName = "24000000";
                        element.Type = "";
                        element.x = endWallX;
                        element.y = dataCordenadY - dataWith / 10;
                        element.z = element.z + nHeight;
                        element.XRotate = 180;
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 240;
                    }
                    else
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelExt120");
                        element.CodeName = "12000000";
                        element.Type = "";
                        element.x = endWallX;
                        element.y = dataCordenadY - dataWith / 10;
                        element.z = element.z + nHeight;
                        element.XRotate = 180;
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 120;

                        ModelRenderElement element2 = new ModelRenderElement();
                        element2.Element = Atk60Element.GetElement("PanelExt120");
                        element2.CodeName = "12000000";
                        element2.Type = "";
                        element2.x = endWallX;
                        element2.y = dataCordenadY - dataWith / 10;
                        element2.XRotate = 180;
                        element2.z = element.z + nHeight;
                        ListRenderElement.Add(element2);
                        nHeight = nHeight + 120;
                    }

                }
                if (RestTypeHeight == 1200)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelExt120");
                    element.CodeName = "12000000";
                    element.Type = "";
                    element.x = endWallX;
                    element.y = dataCordenadY - dataWith / 10;
                    element.z = element.z + nHeight;
                    element.XRotate = 180;
                    ListRenderElement.Add(element);
                    nHeight = nHeight + 120;
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