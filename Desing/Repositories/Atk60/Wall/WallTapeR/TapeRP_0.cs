using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRP_0 : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH, bool IsCorner)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            if (DataHeight != 0)
            {
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 7, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 47, DataHeight, ListRenderElement);
                CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 82, DataHeight, ListRenderElement);
                //CommonElement.SedUnionHorizontal0(EndWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 122, DataHeight, ListRenderElement);
            }
            switch (TypeH)
            {
                case 270:
                    SedPanels270(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    break;
                case 240:
                    SedPanels240(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    break;
                case 120:
                    SedPanels120(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement, IsCorner);
                    break;
            }
            return ListRenderElement;
        }
        private static void SedPanels270(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            if (IsCorner == true)
            {
                //if ((dataWith) <= 489)
                //{
                //    ModelRenderElement elementRight2 = new ModelRenderElement();
                //    elementRight2.Element = Atk60Element.GetElement("Panel30270");
                //    elementRight2.ElementF = Atk60Element.GetElement("Panel30270F");
                //    elementRight2.CodeName = "27304205";
                //    elementRight2.x = endWallX;
                //    elementRight2.z = elementRight2.z + dataHeight;
                //    elementRight2.y = dataCordenadY;
                //    elementRight2.XRotate = 270;
                //    listRenderElement.Add(elementRight2);
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 45, listRenderElement, "");
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 135, listRenderElement, "");
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 225, listRenderElement, "");
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 45, listRenderElement, "");
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 135, listRenderElement, "");
                //    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 225, listRenderElement, "");


                //}
                var elementRightY = dataCordenadY + (75 - 30);
                ModelRenderElement elementRight = new ModelRenderElement();
                elementRight.Element = Atk60Element.GetElement("PanelReg270");
                elementRight.ElementF = Atk60Element.GetElement("PanelReg270F");
                elementRight.CodeName = "27104219";
                elementRight.x = endWallX;
                elementRight.z = elementRight.z + dataHeight;
                elementRight.y = elementRightY;
                elementRight.XRotate = 270;
                listRenderElement.Add(elementRight);
            }
            else
            {
                var elementRightY = dataCordenadY - ((dataWith / 10) - 75);
                ModelRenderElement elementRight2 = new ModelRenderElement();
                elementRight2.Element = Atk60Element.GetElement("PanelReg270");
                elementRight2.ElementF = Atk60Element.GetElement("PanelReg270F");
                elementRight2.CodeName = "27104219";
                elementRight2.x = endWallX;
                elementRight2.z = elementRight2.z + dataHeight;
                elementRight2.y = elementRightY;
                elementRight2.XRotate = 270;
                listRenderElement.Add(elementRight2);
            }
            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, dataHeight, listRenderElement, dataWith / 10);
            }
        }
        private static void SedPanels240(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            if (IsCorner == true)
            {
                if ((dataWith + 300) - 550 > 0)
                {
                    ModelRenderElement elementRight2 = new ModelRenderElement();
                    elementRight2.Element = Atk60Element.GetElement("Panel30240");
                    elementRight2.ElementF = Atk60Element.GetElement("Panel30240F");
                    elementRight2.CodeName = "24304244";
                    elementRight2.x = endWallX;
                    elementRight2.z = elementRight2.z + dataHeight;
                    elementRight2.y = dataCordenadY;
                    elementRight2.XRotate = 270;
                    listRenderElement.Add(elementRight2);
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 40, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 200, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 40, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 200, listRenderElement, "");
                }
            }
            else
            {
                var elementRightY = dataCordenadY - ((dataWith / 10) - 75);
                ModelRenderElement elementRight = new ModelRenderElement();
                elementRight.Element = Atk60Element.GetElement("PanelReg240");
                elementRight.ElementF = Atk60Element.GetElement("PanelReg240F");
                elementRight.CodeName = "24104224";
                elementRight.x = endWallX;
                elementRight.z = elementRight.z + dataHeight;
                elementRight.y = elementRightY;
                elementRight.XRotate = 270;
                listRenderElement.Add(elementRight);
            }
            if (dataHeight != 0)
            {
                CommonElement.SedUnionHorizontalTape(0, endWallX + 10, dataCordenadY - ((dataWith / 10) / 2) + 3, dataHeight, listRenderElement, dataWith / 10);
            }
        }
        private static void SedPanels120(long endWallX, long dataHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement, bool IsCorner)
        {
            if (IsCorner == true)
            {
                if ((dataWith + 300) - 550 > 0)
                {
                    ModelRenderElement elementRight2 = new ModelRenderElement();
                    elementRight2.Element = Atk60Element.GetElement("Panel30120");
                    elementRight2.ElementF = Atk60Element.GetElement("Panel30120F");
                    elementRight2.CodeName = "12304211";
                    elementRight2.x = endWallX;
                    elementRight2.z = elementRight2.z + dataHeight;
                    elementRight2.y = dataCordenadY;
                    elementRight2.XRotate = 270;
                    listRenderElement.Add(elementRight2);
                    var DistUnion = ((dataWith / 10) - 75) * -1;
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 20, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY, dataHeight + 80, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, 0, dataCordenadY - 30, dataHeight + 20, listRenderElement, "");
                }
                var elementRightY = dataCordenadY - ((dataWith / 10) - 75) + 30;
                ModelRenderElement elementRight = new ModelRenderElement();
                elementRight.Element = Atk60Element.GetElement("PanelReg120");
                elementRight.ElementF = Atk60Element.GetElement("PanelReg120F");
                elementRight.CodeName = "12104120";
                elementRight.x = endWallX;
                elementRight.z = elementRight.z + dataHeight;
                elementRight.y = elementRightY;
                elementRight.XRotate = 270;
                listRenderElement.Add(elementRight);
            }
            else
            {
                ModelRenderElement elementRight = new ModelRenderElement();
                elementRight.Element = Atk60Element.GetElement("PanelReg120");
                elementRight.ElementF = Atk60Element.GetElement("PanelReg120F");
                elementRight.CodeName = "12104120";
                elementRight.x = endWallX;
                elementRight.z = elementRight.z + dataHeight;
                elementRight.y = dataCordenadY - ((dataWith / 10) - 75);
                elementRight.XRotate = 270;
                listRenderElement.Add(elementRight);
                if (dataWith - 550 > 0)
                {
                    var DistUnion = ((dataWith / 10) - 75) * -1;
                    ModelRenderElement elementRight2 = new ModelRenderElement();
                    elementRight2.Element = Atk60Element.GetElement("PanelReg120");
                    elementRight2.ElementF = Atk60Element.GetElement("PanelReg120F");
                    elementRight2.CodeName = "12104120";
                    elementRight2.x = endWallX;
                    elementRight2.z = elementRight2.z + dataHeight;
                    elementRight2.y = (dataCordenadY + DistUnion) + 75;
                    elementRight2.XRotate = 270;
                    listRenderElement.Add(elementRight2);
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, DistUnion, dataCordenadY - 30, dataHeight + 20, listRenderElement, "");
                    CommonElement.SedUnionVertical(3, 12, 0, endWallX, DistUnion, dataCordenadY - 30, dataHeight + 80, listRenderElement, "");
                }
            }
        }
    }
}
