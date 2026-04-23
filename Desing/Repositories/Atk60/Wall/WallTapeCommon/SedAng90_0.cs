using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class SedAng90_0 : BaseController
    {
        private static string _codeName;
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
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelExt270");
                    element.ElementF = "";
                    element.CodeName = "27000000";
                    element.Type = "";
                    element.x = endWallX;
                    element.y = (dataCordenadY - dataWith / 10);
                    element.z = element.z + nHeight;
                    element.XRotate = 180;
                    ListRenderElement.Add(element);

                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho.z = elementGancho.z + nHeight + 65;
                    elementGancho.XRotate = 90;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija.CodeName = "7238001";
                    elementTuercaFija.x = endWallX - 8;
                    elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 65;
                    elementTuercaFija.XRotate = 1;
                    elementTuercaFija.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija);

                    ModelRenderElement elementGancho2 = new ModelRenderElement();
                    elementGancho2.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho2.ElementF = "";
                    elementGancho2.CodeName = "1920811";
                    elementGancho2.Type = "";
                    elementGancho2.x = endWallX;
                    elementGancho2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho2.z = elementGancho2.z + nHeight + 140;
                    elementGancho2.XRotate = 90;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija2.CodeName = "7238001";
                    elementTuercaFija2.x = endWallX - 8;
                    elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 140;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);

                    ModelRenderElement elementGancho3 = new ModelRenderElement();
                    elementGancho3.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho3.ElementF = "";
                    elementGancho3.CodeName = "1920811";
                    elementGancho3.Type = "";
                    elementGancho3.x = endWallX;
                    elementGancho3.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho3.z = elementGancho3.z + nHeight + 220;
                    elementGancho3.XRotate = 90;
                    ListRenderElement.Add(elementGancho3);

                    ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
                    elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija3.CodeName = "7238001";
                    elementTuercaFija3.x = endWallX - 8;
                    elementTuercaFija3.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 220;
                    elementTuercaFija3.XRotate = 1;
                    elementTuercaFija3.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija3);

                    //Lado0

                    ModelRenderElement elementGancho01 = new ModelRenderElement();
                    elementGancho01.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho01.ElementF = "";
                    elementGancho01.CodeName = "dywidag02";
                    elementGancho01.Type = "";
                    elementGancho01.x = endWallX + 6;
                    elementGancho01.y = (dataCordenadY - dataWith / 10);
                    elementGancho01.z = elementGancho01.z + nHeight + 55;
                    elementGancho01.XRotate = 270;
                    ListRenderElement.Add(elementGancho01);

                    ModelRenderElement elementTuercaFija01 = new ModelRenderElement();
                    elementTuercaFija01.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija01.CodeName = "7238001";
                    elementTuercaFija01.x = endWallX + 6;
                    elementTuercaFija01.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija01.z = elementTuercaFija01.z + nHeight + 55;
                    elementTuercaFija01.XRotate = 271;
                    elementTuercaFija01.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija01);

                    ModelRenderElement elementTuercaFija012 = new ModelRenderElement();
                    elementTuercaFija012.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija012.CodeName = "7238001";
                    elementTuercaFija012.x = endWallX + 6;
                    elementTuercaFija012.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija012.z = elementTuercaFija012.z + nHeight + 55;
                    elementTuercaFija012.XRotate = 271;
                    elementTuercaFija012.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija012);

                    ModelRenderElement elementGancho02 = new ModelRenderElement();
                    elementGancho02.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho02.ElementF = "";
                    elementGancho02.CodeName = "dywidag02";
                    elementGancho02.Type = "";
                    elementGancho02.x = endWallX + 6;
                    elementGancho02.y = (dataCordenadY - dataWith / 10);
                    elementGancho02.z = elementGancho02.z + nHeight + 140;
                    elementGancho02.XRotate = 270;
                    ListRenderElement.Add(elementGancho02);

                    ModelRenderElement elementTuercaFija02 = new ModelRenderElement();
                    elementTuercaFija02.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija02.CodeName = "7238001";
                    elementTuercaFija02.x = endWallX + 6;
                    elementTuercaFija02.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija02.z = elementTuercaFija02.z + nHeight + 140;
                    elementTuercaFija02.XRotate = 271;
                    elementTuercaFija02.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija02);

                    ModelRenderElement elementTuercaFija022 = new ModelRenderElement();
                    elementTuercaFija022.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija022.CodeName = "7238001";
                    elementTuercaFija022.x = endWallX + 6;
                    elementTuercaFija022.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija022.z = elementTuercaFija022.z + nHeight + 140;
                    elementTuercaFija022.XRotate = 271;
                    elementTuercaFija022.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija022);

                    ModelRenderElement elementGancho03 = new ModelRenderElement();
                    elementGancho03.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho03.ElementF = "";
                    elementGancho03.CodeName = "dywidag02";
                    elementGancho03.Type = "";
                    elementGancho03.x = endWallX + 6;
                    elementGancho03.y = (dataCordenadY - dataWith / 10);
                    elementGancho03.z = elementGancho03.z + nHeight + 215;
                    elementGancho03.XRotate = 270;
                    ListRenderElement.Add(elementGancho03);

                    ModelRenderElement elementTuercaFija04 = new ModelRenderElement();
                    elementTuercaFija04.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija04.CodeName = "7238001";
                    elementTuercaFija04.x = endWallX + 6;
                    elementTuercaFija04.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija04.z = elementTuercaFija04.z + nHeight + 215;
                    elementTuercaFija04.XRotate = 271;
                    elementTuercaFija04.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija04);

                    ModelRenderElement elementTuercaFija025 = new ModelRenderElement();
                    elementTuercaFija025.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija025.CodeName = "7238001";
                    elementTuercaFija025.x = endWallX + 6;
                    elementTuercaFija025.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija025.z = elementTuercaFija025.z + nHeight + 215;
                    elementTuercaFija025.XRotate = 271;
                    elementTuercaFija025.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija025);
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

                    }
                    else
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("PanelExt120");
                        element.CodeName = "12000000";
                        element.Type = "";
                        element.x = endWallX;
                        element.y = (dataCordenadY - dataWith / 10);
                        element.z = element.z + nHeight;
                        element.XRotate = 180;
                        ListRenderElement.Add(element);


                        ModelRenderElement element2 = new ModelRenderElement();
                        element2.Element = Atk60Element.GetElement("PanelExt120");
                        element2.CodeName = "12000000";
                        element2.Type = "";
                        element2.x = endWallX;
                        element2.y = dataCordenadY - dataWith / 10;
                        element2.XRotate = 180;
                        element2.z = element2.z + nHeight + 120;
                        ListRenderElement.Add(element2);

                    }


                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho.z = elementGancho.z + nHeight + 55;
                    elementGancho.XRotate = 90;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija.CodeName = "7238001";
                    elementTuercaFija.x = endWallX - 8;
                    elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 55;
                    elementTuercaFija.XRotate = 1;
                    elementTuercaFija.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija);

                    ModelRenderElement elementGancho2 = new ModelRenderElement();
                    elementGancho2.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho2.ElementF = "";
                    elementGancho2.CodeName = "1920811";
                    elementGancho2.Type = "";
                    elementGancho2.x = endWallX;
                    elementGancho2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho2.z = elementGancho2.z + nHeight + 140;
                    elementGancho2.XRotate = 90;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija2.CodeName = "7238001";
                    elementTuercaFija2.x = endWallX - 8;
                    elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 140;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);

                    ModelRenderElement elementGancho3 = new ModelRenderElement();
                    elementGancho3.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho3.ElementF = "";
                    elementGancho3.CodeName = "1920811";
                    elementGancho3.Type = "";
                    elementGancho3.x = endWallX;
                    elementGancho3.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho3.z = elementGancho3.z + nHeight + 225;
                    elementGancho3.XRotate = 90;
                    ListRenderElement.Add(elementGancho3);

                    ModelRenderElement elementTuercaFija3 = new ModelRenderElement();
                    elementTuercaFija3.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija3.CodeName = "7238001";
                    elementTuercaFija3.x = endWallX - 8;
                    elementTuercaFija3.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija3.z = elementTuercaFija3.z + nHeight + 225;
                    elementTuercaFija3.XRotate = 1;
                    elementTuercaFija3.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija3);
                    //Lado0

                    ModelRenderElement elementGancho01 = new ModelRenderElement();
                    elementGancho01.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho01.ElementF = "";
                    elementGancho01.CodeName = "dywidag02";
                    elementGancho01.Type = "";
                    elementGancho01.x = endWallX + 6;
                    elementGancho01.y = (dataCordenadY - dataWith / 10);
                    elementGancho01.z = elementGancho01.z + nHeight + 55;
                    elementGancho01.XRotate = 270;
                    ListRenderElement.Add(elementGancho01);

                    ModelRenderElement elementTuercaFija01 = new ModelRenderElement();
                    elementTuercaFija01.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija01.CodeName = "7238001";
                    elementTuercaFija01.x = endWallX + 6;
                    elementTuercaFija01.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija01.z = elementTuercaFija01.z + nHeight + 55;
                    elementTuercaFija01.XRotate = 271;
                    elementTuercaFija01.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija01);

                    ModelRenderElement elementTuercaFija012 = new ModelRenderElement();
                    elementTuercaFija012.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija012.CodeName = "7238001";
                    elementTuercaFija012.x = endWallX + 6;
                    elementTuercaFija012.y = (dataCordenadY - dataWith / 10) - 8;
                    elementTuercaFija012.z = elementTuercaFija012.z + nHeight + 55;
                    elementTuercaFija012.XRotate = 271;
                    elementTuercaFija012.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija012);

                    ModelRenderElement elementGancho02 = new ModelRenderElement();
                    elementGancho02.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho02.ElementF = "";
                    elementGancho02.CodeName = "dywidag02";
                    elementGancho02.Type = "";
                    elementGancho02.x = endWallX + 6;
                    elementGancho02.y = (dataCordenadY - dataWith / 10);
                    elementGancho02.z = elementGancho02.z + nHeight + 140;
                    elementGancho02.XRotate = 270;
                    ListRenderElement.Add(elementGancho02);

                    ModelRenderElement elementTuercaFija02 = new ModelRenderElement();
                    elementTuercaFija02.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija02.CodeName = "7238001";
                    elementTuercaFija02.x = endWallX + 6;
                    elementTuercaFija02.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija02.z = elementTuercaFija02.z + nHeight + 140;
                    elementTuercaFija02.XRotate = 271;
                    elementTuercaFija02.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija02);

                    ModelRenderElement elementTuercaFija022 = new ModelRenderElement();
                    elementTuercaFija022.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija022.CodeName = "7238001";
                    elementTuercaFija022.x = endWallX + 6;
                    elementTuercaFija022.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija022.z = elementTuercaFija022.z + nHeight + 140;
                    elementTuercaFija022.XRotate = 271;
                    elementTuercaFija022.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija022);

                    ModelRenderElement elementGancho03 = new ModelRenderElement();
                    elementGancho03.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho03.ElementF = "";
                    elementGancho03.CodeName = "dywidag02";
                    elementGancho03.Type = "";
                    elementGancho03.x = endWallX + 6;
                    elementGancho03.y = (dataCordenadY - dataWith / 10);
                    elementGancho03.z = elementGancho03.z + nHeight + 225;
                    elementGancho03.XRotate = 270;
                    ListRenderElement.Add(elementGancho03);

                    ModelRenderElement elementGancho04 = new ModelRenderElement();
                    elementGancho04.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementGancho04.CodeName = "7238001";
                    elementGancho04.x = endWallX + 6;
                    elementGancho04.y = (dataCordenadY - dataWith / 10);
                    elementGancho04.z = elementGancho04.z + nHeight + 225;
                    elementGancho04.XRotate = 271;
                    elementGancho04.ZRotate = "0";
                    ListRenderElement.Add(elementGancho04);

                    ModelRenderElement elementTuercaFija04 = new ModelRenderElement();
                    elementTuercaFija04.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija04.CodeName = "7238001";
                    elementTuercaFija04.x = endWallX + 6;
                    elementTuercaFija04.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija04.z = elementTuercaFija04.z + nHeight + 225;
                    elementTuercaFija04.XRotate = 271;
                    elementTuercaFija04.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija04);

                    nHeight = nHeight + 240;

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

                    ModelRenderElement elementGancho = new ModelRenderElement();
                    elementGancho.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho.ElementF = "";
                    elementGancho.CodeName = "1920811";
                    elementGancho.Type = "";
                    elementGancho.x = endWallX;
                    elementGancho.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho.z = elementGancho.z + nHeight + 25;
                    elementGancho.XRotate = 90;
                    ListRenderElement.Add(elementGancho);

                    ModelRenderElement elementTuercaFija = new ModelRenderElement();
                    elementTuercaFija.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija.CodeName = "7238001";
                    elementTuercaFija.x = endWallX - 8;
                    elementTuercaFija.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija.z = elementTuercaFija.z + nHeight + 25;
                    elementTuercaFija.XRotate = 1;
                    elementTuercaFija.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija);

                    ModelRenderElement elementGancho2 = new ModelRenderElement();
                    elementGancho2.Element = Atk60Element.GetUnion("GanchoRigidizador");
                    elementGancho2.ElementF = "";
                    elementGancho2.CodeName = "1920811";
                    elementGancho2.Type = "";
                    elementGancho2.x = endWallX;
                    elementGancho2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementGancho2.z = elementGancho2.z + nHeight + 115;
                    elementGancho2.XRotate = 90;
                    ListRenderElement.Add(elementGancho2);

                    ModelRenderElement elementTuercaFija2 = new ModelRenderElement();
                    elementTuercaFija2.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija2.CodeName = "7238001";
                    elementTuercaFija2.x = endWallX - 8;
                    elementTuercaFija2.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija2.z = elementTuercaFija2.z + nHeight + 115;
                    elementTuercaFija2.XRotate = 1;
                    elementTuercaFija2.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija2);
                    //Lado0

                    ModelRenderElement elementGancho01 = new ModelRenderElement();
                    elementGancho01.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho01.ElementF = "";
                    elementGancho01.CodeName = "dywidag02";
                    elementGancho01.Type = "";
                    elementGancho01.x = endWallX + 6;
                    elementGancho01.y = (dataCordenadY - dataWith / 10);
                    elementGancho01.z = elementGancho01.z + nHeight + 25;
                    elementGancho01.XRotate = 270;
                    ListRenderElement.Add(elementGancho01);

                    ModelRenderElement elementTuercaFija01 = new ModelRenderElement();
                    elementTuercaFija01.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija01.CodeName = "7238001";
                    elementTuercaFija01.x = endWallX + 6;
                    elementTuercaFija01.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija01.z = elementTuercaFija01.z + nHeight + 25;
                    elementTuercaFija01.XRotate = 271;
                    elementTuercaFija01.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija01);

                    ModelRenderElement elementTuercaFija012 = new ModelRenderElement();
                    elementTuercaFija012.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija012.CodeName = "7238001";
                    elementTuercaFija012.x = endWallX + 6;
                    elementTuercaFija012.y = (dataCordenadY - dataWith / 10) - 8;
                    elementTuercaFija012.z = elementTuercaFija012.z + nHeight + 25;
                    elementTuercaFija012.XRotate = 271;
                    elementTuercaFija012.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija012);

                    ModelRenderElement elementGancho02 = new ModelRenderElement();
                    elementGancho02.Element = Atk60Element.GetUnion("Dywigdag02");
                    elementGancho02.ElementF = "";
                    elementGancho02.CodeName = "dywidag02";
                    elementGancho02.Type = "";
                    elementGancho02.x = endWallX + 6;
                    elementGancho02.y = (dataCordenadY - dataWith / 10);
                    elementGancho02.z = elementGancho02.z + nHeight + 115;
                    elementGancho02.XRotate = 270;
                    ListRenderElement.Add(elementGancho02);

                    ModelRenderElement elementTuercaFija02 = new ModelRenderElement();
                    elementTuercaFija02.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija02.CodeName = "7238001";
                    elementTuercaFija02.x = endWallX + 6;
                    elementTuercaFija02.y = (dataCordenadY - dataWith / 10);
                    elementTuercaFija02.z = elementTuercaFija02.z + nHeight + 115;
                    elementTuercaFija02.XRotate = 271;
                    elementTuercaFija02.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija02);

                    ModelRenderElement elementTuercaFija022 = new ModelRenderElement();
                    elementTuercaFija022.ElementUnion1 = Atk60Element.GetUnion("TuercaExagonal");
                    elementTuercaFija022.CodeName = "7238001";
                    elementTuercaFija022.x = endWallX + 6;
                    elementTuercaFija022.y = (dataCordenadY - dataWith / 10) - 6;
                    elementTuercaFija022.z = elementTuercaFija022.z + nHeight + 115;
                    elementTuercaFija022.XRotate = 271;
                    elementTuercaFija022.ZRotate = "0";
                    ListRenderElement.Add(elementTuercaFija022);
                    nHeight = nHeight + 120;
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