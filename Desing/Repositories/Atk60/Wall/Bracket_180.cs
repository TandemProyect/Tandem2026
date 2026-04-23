using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class Bracket_180 : BaseController
    {

        internal static object SedBraket(long longLeft, long longRight, long type, long dataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            var Modulo = 0;
            var l = datalong + longLeft;
            var n = (l / 2400);
            var r = l - (n * 2400);
            var hMensula = dataHeight / 10 - 30;
            var hMensulaTablon = dataHeight / 10 + 26;
            var hMensulaTablon2 = dataHeight / 10 - 24;
            var hMensulaTablon3 = dataHeight / 10 + 97;
            var hMensulaTablon4 = dataHeight / 10 - 25;
            for (int i = 0; i < n; i++)
            {
                {
                    ModelRenderElement elementMensula = new ModelRenderElement();
                    elementMensula.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
                    elementMensula.CodeName = "4120000042";
                    elementMensula.x = dataCordenadX - 12 - (dataWith / 10);
                    elementMensula.y = dataCordenadY + 60 + Modulo;
                    elementMensula.z = hMensula;
                    elementMensula.XRotate = 180;
                    ListRenderElement.Add(elementMensula);
                    if (i == 0)
                    {
                        //ModelRenderElement elementTablon_1QM = new ModelRenderElement();
                        //elementTablon_1QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5-2.stl";
                        //elementTablon_1QM.CodeName = "Tablon24x15x5-2";
                        //elementTablon_1QM.x = dataCordenadX + -109 - (dataWith / 10);
                        //elementTablon_1QM.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_1QM.z = hMensulaTablon;
                        //elementTablon_1QM.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_1QM);

                        //ModelRenderElement elementTablon_2QM = new ModelRenderElement();
                        //elementTablon_2QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5-2.stl";
                        //elementTablon_2QM.CodeName = "Tablon24x15x5-2";
                        //elementTablon_2QM.x = dataCordenadX - 109 - (dataWith / 10);
                        //elementTablon_2QM.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_2QM.z = hMensulaTablon2;
                        //elementTablon_2QM.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_2QM);

                        //ModelRenderElement elementTablon_3QM = new ModelRenderElement();
                        //elementTablon_3QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5-2.stl";
                        //elementTablon_3QM.CodeName = "Tablon24x15x5-2";
                        //elementTablon_3QM.x = dataCordenadX - 109 - (dataWith / 10);
                        //elementTablon_3QM.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_3QM.z = hMensulaTablon3;
                        //elementTablon_3QM.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_3QM);


                        //ModelRenderElement elementTablon_1 = new ModelRenderElement();
                        //elementTablon_1.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
                        //elementTablon_1.CodeName = "Tablon24x15x5";
                        //elementTablon_1.x = dataCordenadX - 100 - (dataWith / 10);
                        //elementTablon_1.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_1.z = hMensulaTablon4;
                        //elementTablon_1.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_1);

                        //ModelRenderElement elementTablon_2 = new ModelRenderElement();
                        //elementTablon_2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
                        //elementTablon_2.CodeName = "Tablon24x15x5";
                        //elementTablon_2.x = dataCordenadX - 82 - (dataWith / 10);
                        //elementTablon_2.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_2.z = hMensulaTablon4;
                        //elementTablon_2.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_2);

                        //ModelRenderElement elementTablon_3 = new ModelRenderElement();
                        //elementTablon_3.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
                        //elementTablon_3.CodeName = "Tablon24x15x5";
                        //elementTablon_3.x = dataCordenadX - 64 - (dataWith / 10);
                        //elementTablon_3.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_3.z = hMensulaTablon4;
                        //elementTablon_3.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_3);

                        //ModelRenderElement elementTablon_4 = new ModelRenderElement();
                        //elementTablon_4.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
                        //elementTablon_4.CodeName = "Tablon24x15x5";
                        //elementTablon_4.x = dataCordenadX - 46 - (dataWith / 10);
                        //elementTablon_4.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_4.z = hMensulaTablon4;
                        //elementTablon_4.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_4);

                        //ModelRenderElement elementTablon_5 = new ModelRenderElement();
                        //elementTablon_5.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon24x15x5.stl";
                        //elementTablon_5.CodeName = "Tablon24x15x5";
                        //elementTablon_5.x = dataCordenadX - 28 - (dataWith / 10);
                        //elementTablon_5.y = dataCordenadY + 60 + Modulo;
                        //elementTablon_5.z = hMensulaTablon4;
                        //elementTablon_5.XRotate = 180;
                        //ListRenderElement.Add(elementTablon_5);

                        //ModelRenderElement elementMensula2 = new ModelRenderElement();
                        //elementMensula2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
                        //elementMensula2.CodeName = "4120000042";
                        //elementMensula2.x = dataCordenadX - 12 - (dataWith / 10);
                        //elementMensula2.y = dataCordenadY + 180 + Modulo;
                        //elementMensula2.z = hMensula;
                        //elementMensula2.XRotate = 180;
                        //ListRenderElement.Add(elementMensula2);
                    }
                }
                Modulo = Modulo + 240;
            }
            if (r > 1190)
            {
                {
                    ModelRenderElement elementMensula = new ModelRenderElement();
                    elementMensula.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
                    elementMensula.CodeName = "4120000042";
                    elementMensula.x = dataCordenadX - 12 - (dataWith / 10);
                    elementMensula.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    elementMensula.z = hMensula;
                    elementMensula.XRotate = 180;
                    ListRenderElement.Add(elementMensula);

                    //ModelRenderElement elementTablon_20QM = new ModelRenderElement();
                    //elementTablon_20QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5-2.stl";
                    //elementTablon_20QM.CodeName = "Tablon10x15x5-2";
                    //elementTablon_20QM.x = dataCordenadX + -109 - (dataWith / 10);
                    //elementTablon_20QM.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_20QM.z = hMensulaTablon;
                    //elementTablon_20QM.XRotate = 180;
                    //elementTablon_20QM.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_20QM);

                    //ModelRenderElement elementTablon_2QM = new ModelRenderElement();
                    //elementTablon_2QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5-2.stl";
                    //elementTablon_2QM.CodeName = "Tablon10x15x5-2";
                    //elementTablon_2QM.x = dataCordenadX - 109 - (dataWith / 10);
                    //elementTablon_2QM.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_2QM.z = hMensulaTablon2;
                    //elementTablon_2QM.XRotate = 180;
                    //elementTablon_2QM.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_2QM);

                    //ModelRenderElement elementTablon_3QM = new ModelRenderElement();
                    //elementTablon_3QM.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5-2.stl";
                    //elementTablon_3QM.CodeName = "Tablon10x15x5-2";
                    //elementTablon_3QM.x = dataCordenadX - 109 - (dataWith / 10);
                    //elementTablon_3QM.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_3QM.z = hMensulaTablon3;
                    //elementTablon_3QM.XRotate = 180;
                    //elementTablon_3QM.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_3QM);


                    //ModelRenderElement elementTablon_1 = new ModelRenderElement();
                    //elementTablon_1.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5.stl";
                    //elementTablon_1.CodeName = "Tablon10x15x5";
                    //elementTablon_1.x = dataCordenadX - 100 - (dataWith / 10);
                    //elementTablon_1.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_1.z = hMensulaTablon4;
                    //elementTablon_1.XRotate = 180;
                    //elementTablon_1.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_1);

                    //ModelRenderElement elementTablon_2 = new ModelRenderElement();
                    //elementTablon_2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5.stl";
                    //elementTablon_2.CodeName = "Tablon10x15x5";
                    //elementTablon_2.x = dataCordenadX - 82 - (dataWith / 10);
                    //elementTablon_2.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_2.z = hMensulaTablon4;
                    //elementTablon_2.XRotate = 180;
                    //elementTablon_2.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_2);

                    //ModelRenderElement elementTablon_3 = new ModelRenderElement();
                    //elementTablon_3.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5.stl";
                    //elementTablon_3.CodeName = "Tablon10x15x5";
                    //elementTablon_3.x = dataCordenadX - 64 - (dataWith / 10);
                    //elementTablon_3.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_3.z = hMensulaTablon4;
                    //elementTablon_3.XRotate = 180;
                    //elementTablon_3.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_3);

                    //ModelRenderElement elementTablon_4 = new ModelRenderElement();
                    //elementTablon_4.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5.stl";
                    //elementTablon_4.CodeName = "Tablon10x15x5";
                    //elementTablon_4.x = dataCordenadX - 46 - (dataWith / 10);
                    //elementTablon_4.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_4.z = hMensulaTablon4;
                    //elementTablon_4.XRotate = 180;
                    //elementTablon_4.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_4);

                    //ModelRenderElement elementTablon_5 = new ModelRenderElement();
                    //elementTablon_5.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/Tablon10x15x5.stl";
                    //elementTablon_5.CodeName = "Tablon10x15x5";
                    //elementTablon_5.x = dataCordenadX - 28 - (dataWith / 10);
                    //elementTablon_5.y = dataCordenadY + ((r / 10) / 3) + Modulo;
                    //elementTablon_5.z = hMensulaTablon4;
                    //elementTablon_5.XRotate = 180;
                    //elementTablon_5.Filter = r.ToString();
                    //ListRenderElement.Add(elementTablon_5);

                    //ModelRenderElement elementMensula2 = new ModelRenderElement();
                    //elementMensula2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
                    //elementMensula2.CodeName = "4120000042";
                    //elementMensula2.x = dataCordenadX - 12 - (dataWith / 10);
                    //elementMensula2.y = dataCordenadY + (2 * (r / 10) / 3) + Modulo;
                    //elementMensula2.z = hMensula;
                    //elementMensula2.XRotate = 180;
                    //ListRenderElement.Add(elementMensula2);
                }
            }
            return ListRenderElement;
        }
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