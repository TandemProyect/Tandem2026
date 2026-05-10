using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_90 : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            if (DataHeight != 0)
            {
                CommonElement.SedUnionHorizontal90(EndWallX - 23, dataCordenadY - (dataWith / 10) - 12, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal90(EndWallX - 60, dataCordenadY - (dataWith / 10) - 12, DataHeight, ListRenderElement);

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
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg270");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg270F");
            elementRight.CodeName = "27104219";

            elementRight.x = endWallX + (75 - (datalong / 10));
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = dataCordenadY - (dataWith / 10);
            elementRight.XRotate = 180;
            listRenderElement.Add(elementRight);
            if (dataWith - 550 > 0)
            {
                var DistUnion = (endWallX - (datalong / 10)) + 65/* + (75 - (datalong / 10)) + 75*/;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg270");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg270F");
                elementRight2.CodeName = "27104219";
                elementRight2.x = endWallX + (75 - (datalong / 10)) + 75;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - (dataWith / 10);
                elementRight2.XRotate = 180;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 45, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 135, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 225, listRenderElement, dataWith / 10, "90");
            }

        }
        private static void SedPanels240(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg240");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg240F");
            elementRight.CodeName = "24104224";
            elementRight.x = endWallX + (75 - (datalong / 10));
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = dataCordenadY - (dataWith / 10);
            elementRight.XRotate = 180;
            listRenderElement.Add(elementRight);

            if (dataWith - 550 > 0)
            {
                var DistUnion = (endWallX - (datalong / 10)) + 65/* + (75 - (datalong / 10)) + 75*/;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg240");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg240F");
                elementRight2.CodeName = "24104224";
                elementRight2.x = endWallX + (75 - (datalong / 10)) + 75;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - (dataWith / 10);
                elementRight2.XRotate = 180;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 40, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 200, listRenderElement, dataWith / 10, "90");
            }
        }
        private static void SedPanels120(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementRight = new ModelRenderElement();
            elementRight.Element = Atk60Element.GetElement("PanelReg120");
            elementRight.ElementF = Atk60Element.GetElement("PanelReg120F");
            elementRight.CodeName = "12104120";
            elementRight.x = endWallX + (75 - (datalong / 10));
            elementRight.z = elementRight.z + dataHeight;
            elementRight.y = dataCordenadY - (dataWith / 10);
            elementRight.XRotate = 180;
            listRenderElement.Add(elementRight);
            if (dataWith - 550 > 0)
            {
                var DistUnion = (endWallX - (datalong / 10)) + 65/* + (75 - (datalong / 10)) + 75*/;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg120");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg120F");
                elementRight2.CodeName = "12104120";
                elementRight2.x = endWallX + (75 - (datalong / 10)) + 75;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY - (dataWith / 10);
                elementRight2.XRotate = 180;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 20, listRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionVerticalMirror(4, 12, 0, DistUnion, 10, dataCordenadY, dataHeight + 80, listRenderElement, dataWith / 10, "90");
            }
        }
    }
}
