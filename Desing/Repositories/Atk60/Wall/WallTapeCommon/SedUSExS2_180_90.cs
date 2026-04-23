using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class SedUSExS2_180_90 : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long endWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
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
                    elementGancho.XRotate = 181;
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
                    elementGancho2.XRotate = 181;
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
                    elementGancho3.XRotate = 181;
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
                        ModelRenderElement elementGancho = new ModelRenderElement();
                        elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
                        elementGancho.ElementF = "";
                        elementGancho.CodeName = "1920811";
                        elementGancho.Type = "";
                        elementGancho.x = endWallX;
                        elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                        elementGancho.z = elementGancho.z + nHeight + 40;
                        elementGancho.XRotate = 181;
                        ListRenderElement.Add(elementGancho);

                        ModelRenderElement elementTuercaFija = new ModelRenderElement();
                        elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija.CodeName = "10443020";
                        elementTuercaFija.x = endWallX + 12;
                        elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija.z = elementTuercaFija.z + nHeight + 40;
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
                        elementGancho2.z = elementGancho2.z + nHeight + 200;
                        elementGancho2.XRotate = 181;
                        ListRenderElement.Add(elementGancho2);

                        ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                        elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija2.CodeName = "10443020";
                        elementTuercaFija2.x = endWallX + 12;
                        elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 200;
                        elementTuercaFija2.XRotate = 1;
                        elementTuercaFija2.ZRotate = "0";
                        ListRenderElement.Add(elementTuercaFija2);
                    }
                    else
                    {
                        ModelRenderElement elementGancho = new ModelRenderElement();
                        elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
                        elementGancho.ElementF = "";
                        elementGancho.CodeName = "1920811";
                        elementGancho.Type = "";
                        elementGancho.x = endWallX;
                        elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                        elementGancho.z = elementGancho.z + nHeight + 35;
                        elementGancho.XRotate = 181;
                        ListRenderElement.Add(elementGancho);

                        ModelRenderElement elementTuercaFija = new ModelRenderElement();
                        elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija.CodeName = "10443020";
                        elementTuercaFija.x = endWallX + 12;
                        elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija.z = elementTuercaFija.z + nHeight + 35;
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
                        elementGancho2.z = elementGancho2.z + nHeight + 115;
                        elementGancho2.XRotate = 181;
                        ListRenderElement.Add(elementGancho2);

                        ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                        elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija2.CodeName = "10443020";
                        elementTuercaFija2.x = endWallX + 12;
                        elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 115;
                        elementTuercaFija2.XRotate = 1;
                        elementTuercaFija2.ZRotate = "0";
                        ListRenderElement.Add(elementTuercaFija2);




                        ModelRenderElement elementGancho2n = new ModelRenderElement();
                        elementGancho2n.Element = Atk60Element.GetUnion("GanchoCierre");
                        elementGancho2n.ElementF = "";
                        elementGancho2n.CodeName = "1920811";
                        elementGancho2n.Type = "";
                        elementGancho2n.x = endWallX;
                        elementGancho2n.y = (dataCordenadY - dataWith / 10) - 6;
                        elementGancho2n.z = elementGancho2n.z + nHeight + 155;
                        elementGancho2n.XRotate = 181;
                        ListRenderElement.Add(elementGancho2n);

                        ModelRenderElement elementTuercaFija2n = new ModelRenderElement();
                        elementTuercaFija2n.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija2n.CodeName = "10443020";
                        elementTuercaFija2n.x = endWallX + 12;
                        elementTuercaFija2n.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija2n.z = elementTuercaFija2n.z + nHeight + 155;
                        elementTuercaFija2n.XRotate = 1;
                        elementTuercaFija2n.ZRotate = "0";
                        ListRenderElement.Add(elementTuercaFija2n);

                        ModelRenderElement elementGancho22n = new ModelRenderElement();
                        elementGancho22n.Element = Atk60Element.GetUnion("GanchoCierre");
                        elementGancho22n.ElementF = "";
                        elementGancho22n.CodeName = "1920811";
                        elementGancho22n.Type = "";
                        elementGancho22n.x = endWallX;
                        elementGancho22n.y = (dataCordenadY - dataWith / 10) - 6;
                        elementGancho22n.z = elementGancho22n.z + nHeight + 235;
                        elementGancho22n.XRotate = 181;
                        ListRenderElement.Add(elementGancho22n);

                        ModelRenderElement elementTuercaFija22n = new ModelRenderElement();
                        elementTuercaFija22n.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                        elementTuercaFija22n.CodeName = "10443020";
                        elementTuercaFija22n.x = endWallX + 12;
                        elementTuercaFija22n.y = (dataCordenadY - dataWith / 10) - 14;
                        elementTuercaFija22n.z = elementTuercaFija22n.z + nHeight + 235;
                        elementTuercaFija22n.XRotate = 1;
                        elementTuercaFija22n.ZRotate = "0";
                        ListRenderElement.Add(elementTuercaFija22n);

                    }

                }
                if (RestTypeHeight == 1200)
                {
                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoCierre");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho.z = elementGancho.z + nHeight + 35;
                    elementGancho.XRotate = 181;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija.CodeName = "10443020";
                    elementTuercaFija.x = endWallX + 12;
                    elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 14;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 35;
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
                    elementGancho2.z = elementGancho2.z + nHeight + 115;
                    elementGancho2.XRotate = 181;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                    elementTuercaFija2.CodeName = "10443020";
                    elementTuercaFija2.x = endWallX + 12;
                    elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 14;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 115;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);
                }
            }
            return ListRenderElement;
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