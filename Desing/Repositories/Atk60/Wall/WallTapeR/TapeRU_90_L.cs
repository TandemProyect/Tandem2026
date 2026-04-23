using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRU_90_L : BaseController
    {

        internal static List<ModelRenderElement> setdListElement(long EndWallX, long DataHeight, long dataWith, long datalong, long dataCordenadY, long TypeH)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            switch (TypeH)
            {
                case 270:
                    SedUnion270(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
                case 240:
                    SedUnion240(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
                case 120:
                    SedUnion120(EndWallX, DataHeight, datalong, dataCordenadY, dataWith, ListRenderElement);
                    break;
            }
            return ListRenderElement;
        }
        private static void SedUnion270(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementGancho180 = new ModelRenderElement();
            elementGancho180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho180.ElementF = "";
            elementGancho180.CodeName = "1920811";
            elementGancho180.Type = "";
            elementGancho180.x = endWallX - 2;
            elementGancho180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho180.z = elementGancho180.z + nHeight + 55;
            elementGancho180.XRotate = 90;
            listRenderElement.Add(elementGancho180);

            ModelRenderElement elementTuercaFija180 = new ModelRenderElement();
            elementTuercaFija180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija180.CodeName = "10443020";
            elementTuercaFija180.x = endWallX - 12;
            elementTuercaFija180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija180.z = elementTuercaFija180.z + nHeight + 55;
            elementTuercaFija180.XRotate = 181;
            elementTuercaFija180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija180);

            ModelRenderElement elementGancho2180 = new ModelRenderElement();
            elementGancho2180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2180.ElementF = "";
            elementGancho2180.CodeName = "1920811";
            elementGancho2180.Type = "";
            elementGancho2180.x = endWallX - 2;
            elementGancho2180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho2180.z = elementGancho2180.z + nHeight + 135;
            elementGancho2180.XRotate = 90;
            listRenderElement.Add(elementGancho2180);

            ModelRenderElement elementTuercaFija2180 = new ModelRenderElement();
            elementTuercaFija2180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2180.CodeName = "10443020";
            elementTuercaFija2180.x = endWallX - 12;
            elementTuercaFija2180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija2180.z = elementTuercaFija2180.z + nHeight + 135;
            elementTuercaFija2180.XRotate = 181;
            elementTuercaFija2180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2180);

            ModelRenderElement elementTuercaFija4180 = new ModelRenderElement();
            elementTuercaFija4180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementTuercaFija4180.ElementF = "";
            elementTuercaFija4180.CodeName = "1920811";
            elementTuercaFija4180.Type = "";
            elementTuercaFija4180.x = endWallX - 2;
            elementTuercaFija4180.y = dataCordenadY - (dataWith / 10) - 5;
            elementTuercaFija4180.z = elementTuercaFija4180.z + nHeight + 215;
            elementTuercaFija4180.XRotate = 90;
            listRenderElement.Add(elementTuercaFija4180);

            ModelRenderElement elementTuercaFija3180 = new ModelRenderElement();
            elementTuercaFija3180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3180.CodeName = "10443020";
            elementTuercaFija3180.x = endWallX - 12;
            elementTuercaFija3180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija3180.z = elementTuercaFija3180.z + nHeight + 215;
            elementTuercaFija3180.XRotate = 181;
            elementTuercaFija3180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3180);
        }
        private static void SedUnion240(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementGancho180 = new ModelRenderElement();
            elementGancho180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho180.ElementF = "";
            elementGancho180.CodeName = "1920811";
            elementGancho180.Type = "";
            elementGancho180.x = endWallX - 2;
            elementGancho180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho180.z = elementGancho180.z + nHeight + 32;
            elementGancho180.XRotate = 90;
            listRenderElement.Add(elementGancho180);

            ModelRenderElement elementTuercaFija180 = new ModelRenderElement();
            elementTuercaFija180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija180.CodeName = "10443020";
            elementTuercaFija180.x = endWallX - 12;
            elementTuercaFija180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija180.z = elementTuercaFija180.z + nHeight + 32;
            elementTuercaFija180.XRotate = 181;
            elementTuercaFija180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija180);

            ModelRenderElement elementGancho2180 = new ModelRenderElement();
            elementGancho2180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2180.ElementF = "";
            elementGancho2180.CodeName = "1920811";
            elementGancho2180.Type = "";
            elementGancho2180.x = endWallX - 2;
            elementGancho2180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho2180.z = elementGancho2180.z + nHeight + 135;
            elementGancho2180.XRotate = 90;
            listRenderElement.Add(elementGancho2180);

            ModelRenderElement elementTuercaFija2180 = new ModelRenderElement();
            elementTuercaFija2180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2180.CodeName = "10443020";
            elementTuercaFija2180.x = endWallX - 12;
            elementTuercaFija2180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija2180.z = elementTuercaFija2180.z + nHeight + 135;
            elementTuercaFija2180.XRotate = 181;
            elementTuercaFija2180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2180);

            ModelRenderElement elementTuercaFija4180 = new ModelRenderElement();
            elementTuercaFija4180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementTuercaFija4180.ElementF = "";
            elementTuercaFija4180.CodeName = "1920811";
            elementTuercaFija4180.Type = "";
            elementTuercaFija4180.x = endWallX - 2;
            elementTuercaFija4180.y = dataCordenadY - (dataWith / 10) - 5;
            elementTuercaFija4180.z = elementTuercaFija4180.z + nHeight + 205;
            elementTuercaFija4180.XRotate = 90;
            listRenderElement.Add(elementTuercaFija4180);

            ModelRenderElement elementTuercaFija3180 = new ModelRenderElement();
            elementTuercaFija3180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3180.CodeName = "10443020";
            elementTuercaFija3180.x = endWallX - 12;
            elementTuercaFija3180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija3180.z = elementTuercaFija3180.z + nHeight + 205;
            elementTuercaFija3180.XRotate = 181;
            elementTuercaFija3180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3180);
        }
        private static void SedUnion120(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementGancho180 = new ModelRenderElement();
            elementGancho180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho180.ElementF = "";
            elementGancho180.CodeName = "1920811";
            elementGancho180.Type = "";
            elementGancho180.x = endWallX - 2;
            elementGancho180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho180.z = elementGancho180.z + nHeight + 35;
            elementGancho180.XRotate = 90;
            listRenderElement.Add(elementGancho180);

            ModelRenderElement elementTuercaFija180 = new ModelRenderElement();
            elementTuercaFija180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija180.CodeName = "10443020";
            elementTuercaFija180.x = endWallX - 12;
            elementTuercaFija180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija180.z = elementTuercaFija180.z + nHeight + 35;
            elementTuercaFija180.XRotate = 181;
            elementTuercaFija180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija180);

            ModelRenderElement elementGancho2180 = new ModelRenderElement();
            elementGancho2180.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2180.ElementF = "";
            elementGancho2180.CodeName = "1920811";
            elementGancho2180.Type = "";
            elementGancho2180.x = endWallX - 2;
            elementGancho2180.y = dataCordenadY - (dataWith / 10) - 5;
            elementGancho2180.z = elementGancho2180.z + nHeight + 115;
            elementGancho2180.XRotate = 90;
            listRenderElement.Add(elementGancho2180);

            ModelRenderElement elementTuercaFija2180 = new ModelRenderElement();
            elementTuercaFija2180.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2180.CodeName = "10443020";
            elementTuercaFija2180.x = endWallX - 12;
            elementTuercaFija2180.y = dataCordenadY - (dataWith / 10) - 14;
            elementTuercaFija2180.z = elementTuercaFija2180.z + nHeight + 115;
            elementTuercaFija2180.XRotate = 181;
            elementTuercaFija2180.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2180);

        }
    }
}
