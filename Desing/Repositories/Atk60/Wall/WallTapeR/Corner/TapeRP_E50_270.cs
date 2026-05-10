using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_E70_270 : BaseController
    {
        private static int elementRightY;
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long nHeight, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH, bool IsCorner, long Position_Y, bool ExitingPanel2400)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            long WoodEternal = 0;
            var ConerPosition = dataCordenadY + (datalong / 10) + 75;
            elementRightY = (int)dataCordenadY;
            int SwitchDatawih = CommonElement.GetSwitchDatawih(dataWith);
            switch (SwitchDatawih)
            {
                case 100:
                    //elementRightY = (int)(ConerPosition - 40);
                    WoodEternal = 27;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 275, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);

                    break;
                case 150:
                    //elementRightY = (int)(ConerPosition - 45);
                    WoodEternal = 27;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 275, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);
                    break;
                case 200:
                    //elementRightY = (int)(ConerPosition - 50);
                    WoodEternal = 22;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 225, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);

                    break;
                case 250:
                    //elementRightY = (int)(ConerPosition - 55);
                    WoodEternal = 12;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 125, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);
                    break;
                case 300:
                    //elementRightY = (int)(ConerPosition - 60);
                    WoodEternal = 11;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 75, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);
                    break;
                case 350:
                    //elementRightY = (int)(ConerPosition - 65);
                    WoodEternal = 7;
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 75, 1, 0, DimType.Horizontal, "", " Tuerca ", 0);
                    CommonElement.AddDimHorizontal(0, ListRenderElement, EndWallX, dataCordenadY, 750, 1, 0, DimType.Horizontal, "Esq_70_00", "", 0);
                    break;
                default:
                    break;
            }

            if (nHeight != 0)
            {
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 7, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 47, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 82, DataHeight, ListRenderElement);
            }

            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    DataHeight = nHeight * 270;
                    SedPanels270Corner(EndWallX, nHeight, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion270(EndWallX + WoodEternal, DataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    nHeight = nHeight + 1;
                }
            }
            if (restHeight > 1201)
            {
                if (ExitingPanel2400 == true)
                {
                    DataHeight = nHeight * 240;
                    SedPanels240Corner((long)(EndWallX - WoodEternal), nHeight, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);

                    SedUnion240(EndWallX + WoodEternal, DataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 2400;
                    nHeight = nHeight + 1;
                }
                else
                {
                    DataHeight = nHeight * 120;
                    SedPanels120Corner(EndWallX, nHeight, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion120(EndWallX + WoodEternal, DataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 1200;
                    nHeight = nHeight + 1;
                    DataHeight = nHeight * 120;
                    SedPanels120Corner(EndWallX, nHeight, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    SedUnion120(EndWallX + WoodEternal, DataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                    restHeight = restHeight - 1200;
                    nHeight = nHeight + 1;
                }
            }

            if (restHeight > 0)
            {
                DataHeight = nHeight * 120;
                SedPanels120Corner(EndWallX, nHeight, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                SedUnion120(EndWallX + WoodEternal, DataHeight, datalong, Position_Y, dataWith, ListRenderElement, IsCorner);
                restHeight = restHeight - 1200;
                nHeight = nHeight + 1;
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
            elementRight.XRotate = 0;
            listRenderElement.Add(elementRight);
            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX, elementRightY - 35, dataHeight, listRenderElement, dataWith / 10);
            }
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 45, listRenderElement, "");
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 135, listRenderElement, "");
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 225, listRenderElement, "");
        }
        private static void SedPanels240Corner(long endWallX, long nHeight, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {

            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg240");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg240F");
            elementRight.CodeName = "24104224";
            elementRight.x = endWallX;
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = elementRightY;
            elementRight.XRotate = 0;
            listRenderElement.Add(elementRight);

            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 12, elementRightY - 35, dataHeight, listRenderElement, dataWith / 10);
            }
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 40, listRenderElement, "");
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 200, listRenderElement, "");


        }
        private static void SedPanels120Corner(long endWallX, long nHeight, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg120");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg120F");
            elementRight.CodeName = "12104120";
            elementRight.x = endWallX;
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = elementRightY;
            elementRight.XRotate = 0;
            listRenderElement.Add(elementRight);
            CommonElement.AddDimHorizontal(0, listRenderElement, endWallX, elementRightY - 75, 750, 2, 0, DimType.Horizontal, "", "", 0);
            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 12, elementRightY - 35, dataHeight, listRenderElement, dataWith / 10);
            }
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 20, listRenderElement, "");
            CommonElement.SedUnionVertical(1, 12, 75, endWallX, 0, dataCordenadY, nHeight + 80, listRenderElement, "");
        }
        private static void SedUnion270(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            //endWallX = endWallX + (datalong / 10);
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX;
            elementGancho.y = elementRightY;
            elementGancho.z = elementGancho.z + nHeight + 55;
            elementGancho.XRotate = 1;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX - 8;
            elementTuercaFija.y = elementRightY + 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 55;
            elementTuercaFija.XRotate = 271;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX;
            elementGancho2.y = elementRightY;
            elementGancho2.z = elementGancho2.z + nHeight + 135;
            elementGancho2.XRotate = 1;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX - 8;
            elementTuercaFija2.y = elementRightY + 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
            elementTuercaFija2.XRotate = 271;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX;
            elementGancho3.y = elementRightY;
            elementGancho3.z = elementGancho3.z + nHeight + 215;
            elementGancho3.XRotate = 1;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX - 8;
            elementTuercaFija3.y = elementRightY + 12;
            elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 215;
            elementTuercaFija3.XRotate = 271;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion240(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX;
            elementGancho.y = elementRightY;
            elementGancho.z = elementGancho.z + nHeight + 35;
            elementGancho.XRotate = 1;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX - 8;
            elementTuercaFija.y = elementRightY + 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 35;
            elementTuercaFija.XRotate = 271;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX;
            elementGancho2.y = elementRightY;
            elementGancho2.z = elementGancho2.z + nHeight + 135;
            elementGancho2.XRotate = 1;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX - 8;
            elementTuercaFija2.y = elementRightY + 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
            elementTuercaFija2.XRotate = 271;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX;
            elementGancho3.y = elementRightY;
            elementGancho3.z = elementGancho3.z + nHeight + 205;
            elementGancho3.XRotate = 1;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX - 8;
            elementTuercaFija3.y = elementRightY + 12;
            elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 205;
            elementTuercaFija3.XRotate = 271;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion120(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX;
            elementGancho.y = elementRightY;
            elementGancho.z = elementGancho.z + nHeight + 35;
            elementGancho.XRotate = 1;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX - 8;
            elementTuercaFija.y = elementRightY + 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 35;
            elementTuercaFija.XRotate = 271;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX;
            elementGancho2.y = elementRightY;
            elementGancho2.z = elementGancho2.z + nHeight + 115;
            elementGancho2.XRotate = 1;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX - 8;
            elementTuercaFija2.y = elementRightY + 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 115;
            elementTuercaFija2.XRotate = 271;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);
        }
    }
}
