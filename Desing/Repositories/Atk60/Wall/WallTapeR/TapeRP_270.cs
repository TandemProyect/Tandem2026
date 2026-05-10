using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_270 : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH, bool IsCorner)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            if (DataHeight != 0)
            {
                CommonElement.SedUnionHorizontal270(EndWallX - 55, dataCordenadY + 12, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal270(EndWallX - 17, dataCordenadY + 12, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal270(EndWallX - 130, dataCordenadY + 12, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal270(EndWallX - 92, dataCordenadY + 12, DataHeight, ListRenderElement);

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
            ModelRenderElement elementFrom = new ModelRenderElement();
            elementFrom.Element = Atk60Element.GetElement("PanelReg270");
            elementFrom.ElementF = Atk60Element.GetElement("PanelReg270F");
            elementFrom.CodeName = "27104219";
            elementFrom.LongDimTypeHorizontal = (long?)750;
            elementFrom.LongDimTypeVertical = (long?)2700;
            elementFrom.x = endWallX - 75;
            elementFrom.z = elementFrom.z + dataHeight;
            elementFrom.y = dataCordenadY;
            elementFrom.XRotate = 0;
            listRenderElement.Add(elementFrom);
            if (dataWith - 550 > 0)
            {
                var DistUnion = -150;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg270");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg270F");
                elementRight2.CodeName = "27104219";
                elementRight2.x = endWallX + DistUnion;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY;
                elementRight2.XRotate = 0;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 45, listRenderElement, "");
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 135, listRenderElement, "");
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 225, listRenderElement, "");
            }
        }
        private static void SedPanels240(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementFrom = new ModelRenderElement();
            elementFrom.Element = Atk60Element.GetElement("PanelReg240");
            elementFrom.ElementF = Atk60Element.GetElement("PanelReg240F");
            elementFrom.CodeName = "24104224";
            elementFrom.LongDimTypeHorizontal = (long?)750;
            elementFrom.LongDimTypeVertical = (long?)2400;
            elementFrom.x = endWallX - 75;
            elementFrom.z = elementFrom.z + dataHeight;
            elementFrom.y = dataCordenadY;
            elementFrom.XRotate = 0;
            listRenderElement.Add(elementFrom);

            if (dataWith - 550 > 0)
            {
                var DistUnion = -150;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg240");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg240F");
                elementRight2.CodeName = "24104224";
                elementRight2.x = endWallX + DistUnion;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY;
                elementRight2.XRotate = 0;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 40, listRenderElement, "");
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 200, listRenderElement, "");
            }
        }
        private static void SedPanels120(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementFrom = new ModelRenderElement();
            elementFrom.Element = Atk60Element.GetElement("PanelReg120");
            elementFrom.ElementF = Atk60Element.GetElement("PanelReg120F");
            elementFrom.CodeName = "12104120";
            elementFrom.LongDimTypeHorizontal = (long?)750;
            elementFrom.LongDimTypeVertical = (long?)1200;
            elementFrom.x = endWallX - 75;
            elementFrom.z = elementFrom.z + dataHeight;
            elementFrom.y = dataCordenadY;
            elementFrom.XRotate = 0;
            listRenderElement.Add(elementFrom);
            if (dataWith - 550 > 0)
            {
                var DistUnion = -150;
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg120");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg120F");
                elementRight2.CodeName = "12104120";
                elementRight2.x = endWallX + DistUnion;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = dataCordenadY;
                elementRight2.XRotate = 0;
                listRenderElement.Add(elementRight2);
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 20, listRenderElement, "");
                CommonElement.SedUnionVertical(1, 12, 0, endWallX, -75, dataCordenadY, dataHeight + 80, listRenderElement, "");
            }
        }
    }
}
