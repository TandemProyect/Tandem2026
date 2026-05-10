using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_180 : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

            if (DataHeight != 0)
            {
                CommonElement.SedUnionHorizontal180(EndWallX - ((datalong / 10) + 12), dataCordenadY - ((dataWith / 10) + 27), DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal180(EndWallX - ((datalong / 10) + 12), dataCordenadY - ((dataWith / 10) - 7), DataHeight, ListRenderElement);

            }

            switch (TypeH)
            {
                case 270:
                    SedPanels270(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
                case 240:
                    SedPanels240(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
                case 120:
                    SedPanels120(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
            }
            return ListRenderElement;
        }
        private static void SedPanels270(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementLeft = new ModelRenderElement();
            elementLeft.Element = Atk60Element.GetElement("PanelReg270");
            elementLeft.ElementF = Atk60Element.GetElement("PanelReg270F");
            elementLeft.CodeName = "27104219";
            elementLeft.x = endWallX - (datalong / 10);
            elementLeft.z = elementLeft.z + dataHeight;
            elementLeft.y = dataCordenadY - 75;
            elementLeft.XRotate = 90;
            listRenderElement.Add(elementLeft);
            if (dataWith - 550 > 0)
            {
                var DistUnion = ((dataWith / 10) - 75) * -1;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg270");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg270F");
                elementRight2.CodeName = "27104219";
                elementRight2.x = endWallX - (datalong / 10);
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - 150;
                elementRight2.XRotate = 90;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 45, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 135, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 225, listRenderElement, dataWith / 10, "90");
            }
        }
        private static void SedPanels240(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementLeft = new ModelRenderElement();
            elementLeft.Element = Atk60Element.GetElement("PanelReg240");
            elementLeft.ElementF = Atk60Element.GetElement("PanelReg240F");
            elementLeft.CodeName = "24104224";
            elementLeft.x = endWallX - (datalong / 10);
            elementLeft.z = elementLeft.z + dataHeight;
            elementLeft.y = dataCordenadY - 75;
            elementLeft.XRotate = 90;
            listRenderElement.Add(elementLeft);
            if (dataWith - 550 > 0)
            {
                var DistUnion = ((dataWith / 10) - 75) * -1;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg240");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg240F");
                elementRight2.CodeName = "24104224";
                elementRight2.x = endWallX - (datalong / 10);
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - 150;
                elementRight2.XRotate = 90;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 40, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 200, listRenderElement, dataWith / 10, "90");
            }
        }
        private static void SedPanels120(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementLeft = new ModelRenderElement();
            elementLeft.Element = Atk60Element.GetElement("PanelReg120");
            elementLeft.ElementF = Atk60Element.GetElement("PanelReg120F");
            elementLeft.CodeName = "12104120";
            elementLeft.x = endWallX - (datalong / 10);
            elementLeft.z = elementLeft.z + dataHeight;
            elementLeft.y = dataCordenadY - 75;
            elementLeft.XRotate = 90;
            listRenderElement.Add(elementLeft);
            if (dataWith - 550 > 0)
            {
                var DistUnion = ((dataWith / 10) - 75) * -1;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg120");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg120F");
                elementRight2.CodeName = "12104120";
                elementRight2.x = endWallX - (datalong / 10);
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - 150;
                elementRight2.XRotate = 90;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 20, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(2, 12, 0, endWallX, 10, dataCordenadY - (75 + 10), dataHeight + 80, listRenderElement, dataWith / 10, "90");
            }


        }
    }
}

