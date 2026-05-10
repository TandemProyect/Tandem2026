using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_E50_00 : BaseController
    {
        private static int elementRightY;
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long nHeight, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH, bool IsCorner, long Position_Y, bool ExitingPanel2400)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            int WoodEternal = 0;
            var ConerPosition = dataCordenadY + (datalong / 10) + 75;
            elementRightY = 0;
            int SwitchDatawih = CommonElement.GetSwitchDatawih(dataWith);
            switch (SwitchDatawih)
            {
                case 100:
                    elementRightY = (int)(ConerPosition - 40);
                    WoodEternal = (int)(dataWith + 300) - 400;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 45, 325, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                case 150:
                    elementRightY = (int)(ConerPosition - 45);
                    WoodEternal = (int)(dataWith + 300) - 450;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 50, 275, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                case 200:
                    elementRightY = (int)(ConerPosition - 50);
                    WoodEternal = (int)(dataWith + 300) - 500;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 55, 225, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                case 250:
                    elementRightY = (int)(ConerPosition - 55);
                    WoodEternal = (int)(dataWith + 300) - 550;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 60, 175, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                case 300:
                    elementRightY = (int)(ConerPosition - 60);
                    WoodEternal = (int)(dataWith + 300) - 600;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 65, 125, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                case 350:
                    elementRightY = (int)(ConerPosition - 65);
                    WoodEternal = (int)(dataWith + 300) - 650;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY + 70, 75, 2, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    break;
                default:
                    break;
            }
            if (WoodEternal != 0)
            {
                CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, WoodEternal, 2, 0, DimType.Horizontal, "", "", WoodEternal);
                //Insertar aquí el Remate
                long CorrectionPosition = 75 - ((WoodEternal / 10 / 2)) + 2;
                var ListRenderElementRemate0 = Common.Remate0.setdListElement(ListRenderElement, WoodEternal - 1, DataHeight, dataWith, datalong, EndWallX, dataCordenadY);
            }
            if (nHeight != 0)
            {
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 7, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 47, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 82, DataHeight, ListRenderElement);
            }

            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            long RealDataHeight = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    SedPanels270Corner(EndWallX, nHeight, RealDataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion270(EndWallX, RealDataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    if (n == 1)
                    {
                        CommonElement.AddDimVertical(0, ListRenderElement, EndWallX, dataCordenadY, RealDataHeight, 2700, 2, 0, DimType.Vertical50, "", "", 2700);
                    }
                    RealDataHeight = RealDataHeight + 270;
                    nHeight = nHeight + 1;

                }
            }
            if (restHeight > 1201)
            {

                if (ExitingPanel2400 == true)
                {

                    SedPanels240Corner(EndWallX, nHeight, RealDataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion240(EndWallX, RealDataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 2400;
                    nHeight = nHeight + 1;
                    RealDataHeight = RealDataHeight + 240;
                }
                else
                {
                    SedPanels120Corner(EndWallX, nHeight, RealDataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion120(EndWallX, RealDataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 1200;
                    nHeight = nHeight + 1;
                    RealDataHeight = RealDataHeight + 120;
                    SedPanels120Corner(EndWallX, nHeight, RealDataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion120(EndWallX, RealDataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 1200;
                    nHeight = nHeight + 1;
                    RealDataHeight = RealDataHeight + 120;
                }
            }

            if (restHeight > 0)
            {

                SedPanels120Corner(EndWallX, nHeight, RealDataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                SedUnion120(EndWallX, RealDataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                restHeight = restHeight - 1200;
                nHeight = nHeight + 1;
                RealDataHeight = RealDataHeight * 120;
            }
            return ListRenderElement;
        }
        private static void SedPanels270Corner(long endWallX, long nHeight, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg270");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg270F");
            elementRight.CodeName = "27104219";
            elementRight.x = endWallX;
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = elementRightY;
            elementRight.XRotate = 270;
            listRenderElement.Add(elementRight);
            CommonElement.AddDimHorizontal(0, listRenderElement, endWallX, elementRightY - 75, 750, 2, 0, DimType.Horizontal, "", "", 0);
            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 12, elementRightY - 35, dataHeight, listRenderElement, dataWith / 10);
            }
        }
        private static void SedPanels240Corner(long endWallX, long nHeight, long RealDataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            CommonElement.AddDimVertical(0, listRenderElement, endWallX, dataCordenadY, RealDataHeight, 2400, 2, 0, DimType.Vertical50, "", "", 2400);
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg240");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg240F");
            elementRight.CodeName = "24104224";
            elementRight.x = endWallX;
            elementRight.z = elementRight.z + RealDataHeight;
            elementRight.y = elementRightY;
            elementRight.XRotate = 270;
            listRenderElement.Add(elementRight);
            CommonElement.AddDimHorizontal(0, listRenderElement, endWallX, elementRightY - 75, 750, 2, 0, DimType.Horizontal, "", "", 0);

            if (RealDataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 12, elementRightY - 35, RealDataHeight, listRenderElement, dataWith / 10);
            }
        }
        private static void SedPanels120Corner(long endWallX, long nHeight, long RealDataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            CommonElement.AddDimVertical(0, listRenderElement, endWallX, dataCordenadY, RealDataHeight, 1200, 2, 0, DimType.Vertical50, "", "", 1200);
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg120");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg120F");
            elementRight.CodeName = "12104120";
            elementRight.x = endWallX;
            elementRight.z = elementRight.z + RealDataHeight;
            elementRight.y = elementRightY;
            elementRight.XRotate = 270;
            listRenderElement.Add(elementRight);
            CommonElement.AddDimHorizontal(0, listRenderElement, endWallX, elementRightY - 75, 750, 2, 0, DimType.Horizontal, "", "", 0);
            if (RealDataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 12, elementRightY - 35, RealDataHeight, listRenderElement, dataWith / 10);
            }
        }
        private static void SedUnion270(long endWallX, long RealDataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {

            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 1;
            elementGancho.y = dataCordenadY + 6;
            elementGancho.z = elementGancho.z + RealDataHeight + 55;
            elementGancho.XRotate = 270;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 12;
            elementTuercaFija.y = dataCordenadY + 15;
            elementTuercaFija.z = elementTuercaFija.z + RealDataHeight + 55;
            elementTuercaFija.XRotate = 1;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 1;
            elementGancho2.y = dataCordenadY + 6;
            elementGancho2.z = elementGancho2.z + RealDataHeight + 135;
            elementGancho2.XRotate = 270;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 12;
            elementTuercaFija2.y = dataCordenadY + 15;
            elementTuercaFija2.z = elementTuercaFija2.z + RealDataHeight + 135;
            elementTuercaFija2.XRotate = 1;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX + 1;
            elementGancho3.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 6;
            elementGancho3.z = elementGancho3.z + RealDataHeight + 215;
            elementGancho3.XRotate = 270;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX + 12;
            elementTuercaFija3.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija3.z = elementTuercaFija3.z + RealDataHeight + 215;
            elementTuercaFija3.XRotate = 1;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion240(long endWallX, long RealDataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 1;
            elementGancho.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 6;
            elementGancho.z = elementGancho.z + RealDataHeight + 32;
            elementGancho.XRotate = 270;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 12;
            elementTuercaFija.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija.z = elementTuercaFija.z + RealDataHeight + 32;
            elementTuercaFija.XRotate = 1;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 1;
            elementGancho2.y = dataCordenadY /*- ((dataWith / 10) - 75) */+ 6;
            elementGancho2.z = elementGancho2.z + RealDataHeight + 135;
            elementGancho2.XRotate = 270;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 12;
            elementTuercaFija2.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija2.z = elementTuercaFija2.z + RealDataHeight + 135;
            elementTuercaFija2.XRotate = 1;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX + 1;
            elementGancho3.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 6;
            elementGancho3.z = elementGancho3.z + RealDataHeight + 205;
            elementGancho3.XRotate = 270;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX + 12;
            elementTuercaFija3.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija3.z = elementTuercaFija3.z + RealDataHeight + 205;
            elementTuercaFija3.XRotate = 1;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion120(long endWallX, long RealDataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 1;
            elementGancho.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 6;
            elementGancho.z = elementGancho.z + RealDataHeight + 35;
            elementGancho.XRotate = 270;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 12;
            elementTuercaFija.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija.z = elementTuercaFija.z + RealDataHeight + 35;
            elementTuercaFija.XRotate = 1;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 1;
            elementGancho2.y = dataCordenadY /*- ((dataWith / 10) - 75) */+ 6;
            elementGancho2.z = elementGancho2.z + RealDataHeight + 115;
            elementGancho2.XRotate = 270;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 12;
            elementTuercaFija2.y = dataCordenadY /*- ((dataWith / 10) - 75)*/ + 15;
            elementTuercaFija2.z = elementTuercaFija2.z + RealDataHeight + 115;
            elementTuercaFija2.XRotate = 1;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);
        }
    }
}
