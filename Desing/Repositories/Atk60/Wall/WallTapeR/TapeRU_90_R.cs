using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallTapeR
{
    public class TapeRU_90_R : BaseController
    {
        private static string _codeName;
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
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 5;
            elementGancho.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho.z = elementGancho.z + nHeight + 55;
            elementGancho.XRotate = 0;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 14;
            elementTuercaFija.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 55;
            elementTuercaFija.XRotate = 91;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 5;
            elementGancho2.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho2.z = elementGancho2.z + nHeight + 135;
            elementGancho2.XRotate = 0;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 14;
            elementTuercaFija2.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
            elementTuercaFija2.XRotate = 91;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX + 5;
            elementGancho3.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho3.z = elementGancho3.z + nHeight + 215;
            elementGancho3.XRotate = 0;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX + 14;
            elementTuercaFija3.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 215;
            elementTuercaFija3.XRotate = 91;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion240(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 5;
            elementGancho.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho.z = elementGancho.z + nHeight + 32;
            elementGancho.XRotate = 0;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 14;
            elementTuercaFija.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 32;
            elementTuercaFija.XRotate = 91;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 5;
            elementGancho2.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho2.z = elementGancho2.z + nHeight + 135;
            elementGancho2.XRotate = 0;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 14;
            elementTuercaFija2.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 135;
            elementTuercaFija2.XRotate = 91;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);

            ModelRenderElement elementGancho3 = new ModelRenderElement();
            elementGancho3.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho3.ElementF = "";
            elementGancho3.CodeName = "1920811";
            elementGancho3.Type = "";
            elementGancho3.x = endWallX + 5;
            elementGancho3.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho3.z = elementGancho3.z + nHeight + 205;
            elementGancho3.XRotate = 0;
            listRenderElement.Add(elementGancho3);

            ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
            elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija3.CodeName = "10443020";
            elementTuercaFija3.x = endWallX + 14;
            elementTuercaFija3.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 205;
            elementTuercaFija3.XRotate = 91;
            elementTuercaFija3.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija3);
        }
        private static void SedUnion120(long endWallX, long nHeight, long datalong, long dataCordenadY, long dataWith, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementGancho = new ModelRenderElement();
            elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho.ElementF = "";
            elementGancho.CodeName = "1920811";
            elementGancho.Type = "";
            elementGancho.x = endWallX + 5;
            elementGancho.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho.z = elementGancho.z + nHeight + 35;
            elementGancho.XRotate = 0;
            listRenderElement.Add(elementGancho);

            ModelRenderElement elementTuercaFija = new ModelRenderElement();
            elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija.CodeName = "10443020";
            elementTuercaFija.x = endWallX + 14;
            elementTuercaFija.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija.z = elementTuercaFija.z + nHeight + 35;
            elementTuercaFija.XRotate = 91;
            elementTuercaFija.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija);

            ModelRenderElement elementGancho2 = new ModelRenderElement();
            elementGancho2.Element = Atk60Element.GetUnion("GanchoCierre");
            elementGancho2.ElementF = "";
            elementGancho2.CodeName = "1920811";
            elementGancho2.Type = "";
            elementGancho2.x = endWallX + 5;
            elementGancho2.y = dataCordenadY - (dataWith / 10) - 1;
            elementGancho2.z = elementGancho2.z + nHeight + 115;
            elementGancho2.XRotate = 0;
            listRenderElement.Add(elementGancho2);

            ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
            elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementTuercaFija2.CodeName = "10443020";
            elementTuercaFija2.x = endWallX + 14;
            elementTuercaFija2.y = dataCordenadY - (dataWith / 10) - 12;
            elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 115;
            elementTuercaFija2.XRotate = 91;
            elementTuercaFija2.ZRotate = "0";
            listRenderElement.Add(elementTuercaFija2);
        }
    }
}
