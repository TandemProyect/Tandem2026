using Desing.Controllers;
using System.Collections.Generic;

namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloCruceT : BaseController
    {
        private static string MeshTdwidag;
        private static string _codeName;

        internal static List<ModelRenderElement> setdListElement(long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY, RotateMesh meshRotateX, RotateMesh meshRotateMirrowX, string dataRotateZ, long _Type)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            GetTdwidag(dataWith);

            SedPanels2700(type, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement, _Type);
            //SendRemate();
            //SedUnionVertical(data, currentDefaultDisign, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        private static void SedPanels2700(long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement, long _Type)
        {
            int n = (int)(DataHeight / 2700);
            int nt = (int)(DataHeight / 900);
            var restHeight = (int)(DataHeight - (2700 * n));
            var restHeightT = (int)(DataHeight - (900 * nt));
            var nHeight = 0;
            //Insert Panel
            string LongSolution = getLongSpaces(datalong);
            if (LongSolution == "1,20")
            {
                if (nt >= 1)
                {
                    for (int i = 0; i < nt; i++)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("Panel90120T");
                        element.ElementF = Atk60Element.GetElement("Panel90120TF"); ;
                        element.CodeName = "12904215";
                        element.z = element.z + nHeight;
                        switch (type)
                        {
                            case 4:
                                element.x = dataCordenadX + 120;
                                element.y = (dataCordenadY - dataWith / 10);
                                element.XRotate = 180;
                                break;
                        }
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 90;
                    }
                }
                //Hacer Resto
                string restHeightTGrup = getrestHeightTGrup(restHeightT);
                if (restHeightTGrup == "0,30")
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel30120T");
                    element.ElementF = Atk60Element.GetElement("Panel30120TF"); ;
                    element.CodeName = "12304211";
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.x = dataCordenadX + 120;
                            element.y = (dataCordenadY - dataWith / 10);
                            element.XRotate = 180;
                            break;
                    }
                    ListRenderElement.Add(element);

                }
                if (restHeightTGrup == "0,45")
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel45120T");
                    element.ElementF = Atk60Element.GetElement("Panel45120TF"); ;
                    element.CodeName = "12454212";
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.x = dataCordenadX + 120;
                            element.y = (dataCordenadY - dataWith / 10);
                            element.XRotate = 180;
                            break;
                    }
                    ListRenderElement.Add(element);

                }
                if (restHeightTGrup == "0,60")
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel90120T");
                    element.ElementF = Atk60Element.GetElement("Panel90120TF"); ;
                    element.CodeName = "12904215";
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.x = dataCordenadX + 120;
                            element.y = (dataCordenadY - dataWith / 10);
                            element.XRotate = 180;
                            break;
                    }
                    ListRenderElement.Add(element);

                }
                if (restHeightTGrup == "0,90")
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel90120T");
                    element.ElementF = Atk60Element.GetElement("Panel90120TF"); ;
                    element.CodeName = "12904215";
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.x = dataCordenadX + 120;
                            element.y = (dataCordenadY - dataWith / 10);
                            element.XRotate = 180;
                            break;
                    }
                    ListRenderElement.Add(element);

                }



                //if (restHeightT > 1200)
                //{
                //    ModelRenderElement element = new ModelRenderElement();
                //    element.Element = Atk60Element.GetElement("Panel90240");
                //    element.ElementF = Atk60Element.GetElement("Panel90240F"); ;
                //    element.CodeName = "24904240";
                //    element.z = element.z + nHeight;
                //    switch (type)
                //    {
                //        case 4:
                //            element.x = dataCordenadX + 90;
                //            element.y = (dataCordenadY - dataWith / 10);
                //            element.XRotate = 180;
                //            break;
                //    }
                //    ListRenderElement.Add(element);
                //}
                //else
                //{
                //    if (restHeight > 0.01)
                //    {
                //        ModelRenderElement element = new ModelRenderElement();
                //        element.Element = Atk60Element.GetElement("Panel90120");
                //        element.ElementF = Atk60Element.GetElement("Panel90120F"); ;
                //        element.CodeName = "12904215";
                //        element.z = element.z + nHeight;
                //        switch (type)
                //        {
                //            case 4:
                //                element.x = dataCordenadX + 90;
                //                element.y = (dataCordenadY - dataWith / 10);
                //                element.XRotate = 180;
                //                break;
                //        }
                //        ListRenderElement.Add(element);
                //    }
                //}
            }


            if (LongSolution == "0,90")
            {
                if (n >= 1)
                {
                    for (int i = 0; i < n; i++)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("Panel90270");
                        element.ElementF = Atk60Element.GetElement("Panel90270F"); ;
                        element.CodeName = "27904209";
                        element.z = element.z + nHeight;
                        switch (type)
                        {
                            case 4:
                                element.x = dataCordenadX + 90;
                                element.y = (dataCordenadY - dataWith / 10);
                                element.XRotate = 180;
                                break;
                        }
                        ListRenderElement.Add(element);
                        nHeight = nHeight + 270;
                    }
                }
                if (restHeight > 1200)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel90240");
                    element.ElementF = Atk60Element.GetElement("Panel90240F"); ;
                    element.CodeName = "24904240";
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.x = dataCordenadX + 90;
                            element.y = (dataCordenadY - dataWith / 10);
                            element.XRotate = 180;
                            break;
                    }
                    ListRenderElement.Add(element);
                }
                else
                {
                    if (restHeight > 0.01)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.Element = Atk60Element.GetElement("Panel90120");
                        element.ElementF = Atk60Element.GetElement("Panel90120F"); ;
                        element.CodeName = "12904215";
                        element.z = element.z + nHeight;
                        switch (type)
                        {
                            case 4:
                                element.x = dataCordenadX + 90;
                                element.y = (dataCordenadY - dataWith / 10);
                                element.XRotate = 180;
                                break;
                        }
                        ListRenderElement.Add(element);
                    }
                }
            }
            //Panel de Esquina
            nHeight = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelE2700");
                    element.ElementF = Atk60Element.GetElement("PanelE2700F"); ;
                    element.CodeName = "E27004210";
                    element.x = dataCordenadX;
                    element.y = dataCordenadY;
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.XRotate = 90;
                            break;
                    }
                    ListRenderElement.Add(element);
                    ModelRenderElement element2 = new ModelRenderElement();
                    element2.Element = Atk60Element.GetElement("PanelE2700");
                    element2.ElementF = Atk60Element.GetElement("PanelE2700F"); ;
                    element2.CodeName = "E27004210";
                    element2.x = dataCordenadX + 30 + (dataWith / 10);
                    element2.y = dataCordenadY + 30;
                    element2.z = element2.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element2.XRotate = 0;
                            break;
                    }
                    ListRenderElement.Add(element2);
                    nHeight = nHeight + 270;
                }

            }
            if (restHeight > 1200)
            {
                ModelRenderElement element = new ModelRenderElement();
                element.Element = Atk60Element.GetElement("PanelE2400");
                element.ElementF = Atk60Element.GetElement("PanelE2400F"); ;
                element.CodeName = "E24004217";
                element.x = dataCordenadX;
                element.y = dataCordenadY;
                element.z = element.z + nHeight;
                switch (type)
                {
                    case 4:
                        element.XRotate = 90;
                        break;
                }
                ListRenderElement.Add(element);
                ModelRenderElement element2 = new ModelRenderElement();
                element2.Element = Atk60Element.GetElement("PanelE2400");
                element2.ElementF = Atk60Element.GetElement("PanelE2400F"); ;
                element2.CodeName = "E24004217";
                element2.x = dataCordenadX + 30 + (dataWith / 10);
                element2.y = dataCordenadY + 30;
                element2.z = element2.z + nHeight;
                switch (type)
                {
                    case 4:
                        element2.XRotate = 0;
                        break;
                }
                ListRenderElement.Add(element2);
            }
            else
            {
                if (restHeight > 0.01)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelE1200");
                    element.ElementF = Atk60Element.GetElement("PanelE1200F"); ;
                    element.CodeName = "E12004216";
                    element.x = dataCordenadX;
                    element.y = dataCordenadY;
                    element.z = element.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element.XRotate = 90;
                            break;
                    }
                    ListRenderElement.Add(element);
                    ModelRenderElement element2 = new ModelRenderElement();
                    element2.Element = Atk60Element.GetElement("PanelE1200");
                    element2.ElementF = Atk60Element.GetElement("PanelE1200F"); ;
                    element2.CodeName = "E12004216";
                    element2.x = dataCordenadX + 30 + (dataWith / 10);
                    element2.y = dataCordenadY + 30;
                    element2.z = element2.z + nHeight;
                    switch (type)
                    {
                        case 4:
                            element2.XRotate = 0;
                            break;
                    }
                    ListRenderElement.Add(element2);
                }
            }
        }

        private static string getrestHeightTGrup(int restHeightT)
        {
            var dataRetun = "0,30";
            if (restHeightT > 300) { dataRetun = "0,45"; }
            if (restHeightT > 450) { dataRetun = "0,60"; }
            if (restHeightT > 600) { dataRetun = "0,90"; }
            return dataRetun;
        }

        private static string getLongSpaces(long datalong)
        {
            var dataRetun = "0,90";
            if (datalong > 900) { dataRetun = "1,20"; }
            if (datalong > 1200) { dataRetun = "1,50"; }
            if (datalong > 1500) { dataRetun = "1,80"; }
            if (datalong > 1800) { dataRetun = "2,10"; }
            return dataRetun;
        }

        private static void Insert1200Element(List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel90120");
            element.ElementF = Atk60Element.GetElement("Panel90120F");
            element.CodeName = "12904215";
            element.z = element.z + nHeight;
            switch (type)
            {
                case 1:

                    element.x = dataCordenadX;
                    element.y = dataCordenadY;
                    element.XRotate = 0;
                    break;
                case 3:
                    element.x = dataCordenadX;
                    element.y = dataCordenadY;
                    element.XRotate = 270;
                    break;
            }
            ListRenderElement.Add(element);
            ModelRenderElement element2 = new ModelRenderElement();
            element2.Element = Atk60Element.GetElement("Panel90120");
            element2.ElementF = Atk60Element.GetElement("Panel90120F");
            element2.CodeName = "12904215";
            element2.z = element2.z + nHeight;
            switch (type)
            {
                case 1:
                    element2.x = dataCordenadX + 90;
                    element2.y = dataCordenadY;
                    element2.XRotate = 0;
                    break;
                case 3:
                    element2.x = dataCordenadX;
                    element2.y = dataCordenadY + 90;
                    element2.XRotate = 270;
                    break;
            }
            ListRenderElement.Add(element2);
            ModelRenderElement element3 = new ModelRenderElement();
            element3.Element = Atk60Element.GetElement("Panel90120");
            element3.ElementF = Atk60Element.GetElement("Panel90120F");
            element3.CodeName = "12904215";
            element3.z = element3.z + nHeight;
            switch (type)
            {
                case 1:
                    element3.x = dataCordenadX + 180;
                    element3.y = dataCordenadY;
                    element3.XRotate = 0;
                    break;
                case 3:
                    element3.x = dataCordenadX;
                    element3.y = dataCordenadY + 180;
                    element3.XRotate = 270;
                    break;
            }
            ListRenderElement.Add(element3);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel90120");
            element4.ElementF = Atk60Element.GetElement("Panel90120F");
            element4.CodeName = "12904215";
            element4.z = element4.z + nHeight;
            switch (type)
            {
                case 1:
                    element4.x = dataCordenadX + 90;
                    element4.y = (dataCordenadY - dataWith / 10);
                    element4.XRotate = 180;
                    break;
                case 3:
                    element4.x = (dataCordenadX - datalong / 10);
                    element4.y = dataCordenadY - 90;
                    element4.XRotate = 90;
                    break;
            }
            ListRenderElement.Add(element4);
            ModelRenderElement element5 = new ModelRenderElement();
            element5.Element = Atk60Element.GetElement("Panel90120");
            element5.ElementF = Atk60Element.GetElement("Panel90120F");
            element5.CodeName = "12904215";
            element5.z = element5.z + nHeight;
            switch (type)
            {
                case 1:
                    element5.x = dataCordenadX + 180;
                    element5.y = (dataCordenadY - dataWith / 10);
                    element5.XRotate = 180;
                    break;
                case 3:
                    element5.x = (dataCordenadX - datalong / 10);
                    element5.y = dataCordenadY;
                    element5.XRotate = 90;
                    break;
            }
            ListRenderElement.Add(element5);
            ModelRenderElement element6 = new ModelRenderElement();
            element6.Element = Atk60Element.GetElement("Panel90120");
            element6.ElementF = Atk60Element.GetElement("Panel90120F");
            element6.CodeName = "12904215";
            element6.z = element6.z + nHeight;
            switch (type)
            {
                case 1:
                    element6.x = dataCordenadX + 270;
                    element6.y = (dataCordenadY - dataWith / 10);
                    element6.XRotate = 180;
                    break;
                case 3:
                    element6.x = (dataCordenadX - datalong / 10);
                    element6.y = dataCordenadY + 90;
                    element6.XRotate = 90;
                    break;
            }
            ListRenderElement.Add(element6);
        }

        private static int getRestTypeHeight(int restHeight)
        {
            if (restHeight > 300 && restHeight <= 450)
            {
                return 450;
            }
            if (restHeight > 450 && restHeight <= 600)
            {
                return 600;
            }
            if (restHeight > 600 && restHeight <= 900)
            {
                return 900;
            }
            if (restHeight > 900 && restHeight <= 1200)
            {
                return 1200;
            }
            if (restHeight > 1200 && restHeight <= 1500)
            {
                return 1500;
            }
            if (restHeight > 1500 && restHeight <= 1800)
            {
                return 1800;
            }
            if (restHeight > 1800 && restHeight <= 2100)
            {
                return 2100;
            }
            if (restHeight > 2100 && restHeight <= 2400)
            {
                return 2400;
            }
            if (restHeight > 2400 && restHeight <= 2700)
            {
                return 2700;
            }
            return 0;
        }

        private static void SendRemate()
        {
            throw new System.NotImplementedException();
        }

        private static void SedPanelsRest()
        {
            //switch (lastPanel)
            //{
            //    case 60:
            //        ModelRenderElement element60 = new ModelRenderElement();
            //        element60.Element = "../../Content/DesignTools/Stl/ATK60/27604207.stl";
            //        element60.ElementF = "../../Content/DesignTools/Stl/ATK60/27604207_F.stl";
            //        element60.x = dataCordenadX + (datalong / 10) - 60;
            //        element60.CodeName = "27604207";
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 60);
            //        element60.y = dataCordenadY;
            //        element60.z = data.ZCoordinate;
            //        _listRenderElement.Add(element60);
            //        element60.XRotate = 0 + RotateWall;
            //        break;
            //    case 45:
            //        ModelRenderElement element45 = new ModelRenderElement();
            //        element45.Element = "../../Content/DesignTools/Stl/ATK60/27904209.stl";
            //        element45.ElementF = "../../Content/DesignTools/Stl/ATK60/27904209_F.stl";
            //        element45.CodeName = "27904209";
            //        element45.x = dataCordenadX + datalong - 45;
            //        element45.y = dataCordenadY;
            //        element45.z = data.ZCoordinate;
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 45);
            //        _listRenderElement.Add(element45);
            //        element45.XRotate = 0 + RotateWall;
            //        break;
            //    case 30:
            //        ModelRenderElement element30 = new ModelRenderElement();
            //        element30.Element = "../../Content/DesignTools/Stl/ATK60/27304205.stl";
            //        element30.ElementF = "../../Content/DesignTools/Stl/ATK60/27304205_F.stl";
            //        element30.x = dataCordenadX + datalong - 30;
            //        element30.CodeName = "27304205";
            //        element30.y = dataCordenadY;
            //        element30.z = data.ZCoordinate;
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 30);
            //        _listRenderElement.Add(element30);
            //        element30.XRotate = 0 + RotateWall;
            //        break;
            //}
            //if (Wood > 0.01)
            //{
            //    ModelRenderElement elementWood = new ModelRenderElement();
            //    elementWood.ElementWood = "../../Content/DesignTools/Control/Cube.stl";
            //    elementWood.LongWood = Wood;
            //    elementWood.CodeName = "Wood";
            //    elementWood.x = RestPoint - Wood / 10;
            //    elementWood.y = dataCordenadY;
            //    elementWood.z = data.ZCoordinate;
            //    elementWood.heightWood = 270;
            //    _listRenderElement.Add(elementWood);
            //    elementWood.XRotate = 180 + RotateWall;
            //}
            //// Mirror Panels
            //AddModulo = 0;

            //switch (lastPanel)
            //{
            //    case 60:
            //        ModelRenderElement element60 = new ModelRenderElement();
            //        element60.Element = "../../Content/DesignTools/Stl/ATK60/27604207.stl";
            //        element60.ElementF = "../../Content/DesignTools/Stl/ATK60/27604207_F.stl";
            //        element60.CodeName = "27604207";
            //        element60.x = dataCordenadXMirrow + (datalong / 10);
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 60);
            //        element60.y = (dataCordenadY - dataWith / 10);
            //        element60.z = data.ZCoordinate;
            //        _listRenderElement.Add(element60);
            //        element60.XRotate = 180 + RotateWall;
            //        break;
            //    case 45:
            //        ModelRenderElement element45 = new ModelRenderElement();
            //        element45.Element = "../../Content/DesignTools/Stl/ATK60/27904209.stl";
            //        element45.ElementF = "../../Content/DesignTools/Stl/ATK60/27904209_F.stl";
            //        element45.CodeName = "27904209";
            //        element45.x = dataCordenadXMirrow + datalong;
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 45);
            //        element45.y = (dataCordenadY - dataWith / 10);
            //        element45.z = data.ZCoordinate;
            //        _listRenderElement.Add(element45);
            //        element45.XRotate = 180 + RotateWall;
            //        break;
            //    case 30:
            //        ModelRenderElement element30 = new ModelRenderElement();
            //        element30.Element = "../../Content/DesignTools/Stl/ATK60/27304205.stl";
            //        element30.ElementF = "../../Content/DesignTools/Stl/ATK60/27304205_F.stl";
            //        element30.CodeName = "27304205";
            //        element30.x = dataCordenadXMirrow + datalong;
            //        element30.y = (dataCordenadY - dataWith / 10);
            //        element30.z = data.ZCoordinate;
            //        RestPoint = (int)((int)dataCordenadX + (datalong / 10) - 30);
            //        _listRenderElement.Add(element30);
            //        element30.XRotate = 180 + RotateWall;
            //        break;
            //}
            //if (Wood > 0.01)
            //{
            //    ModelRenderElement elementWood = new ModelRenderElement();
            //    elementWood.ElementWood = "../../Content/DesignTools/Control/Cube.stl";
            //    elementWood.CodeName = "Wood";
            //    elementWood.LongWood = Wood;
            //    elementWood.x = RestPoint - Wood / 10;
            //    elementWood.y = (dataCordenadY - dataWith / 10 - 12);
            //    elementWood.z = data.ZCoordinate;
            //    elementWood.heightWood = 270;
            //    _listRenderElement.Add(elementWood);
            //    elementWood.XRotate = 180 + RotateWall;
            //}
        }

        private static void SedUnionVertical(ModelLeven data, DAL.TSql_DefaultDesign currentDefaultDisign, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> _listRenderElement)
        {
            //for (int i = 0; i < n; i++)
            //{
            //    if (i != 0)
            //    {
            //        ModelRenderElement elementu = new ModelRenderElement();
            //        elementu.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //        elementu.x = dataCordenadXMirrow + AddModulo - 90;
            //        elementu.y = (dataCordenadY - dataWith / 10) - 12;
            //        elementu.z = data.ZCoordinate + 45;
            //        elementu.XRotate = 180 + RotateWall;
            //        _listRenderElement.Add(elementu);
            //        ModelRenderElement elementu2 = new ModelRenderElement();
            //        elementu2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //        elementu2.x = dataCordenadXMirrow + AddModulo - 90;
            //        elementu2.y = (dataCordenadY - dataWith / 10) - 12;
            //        elementu2.z = data.ZCoordinate + 135;
            //        elementu2.XRotate = 180 + RotateWall;
            //        _listRenderElement.Add(elementu2);
            //        ModelRenderElement elementu3 = new ModelRenderElement();
            //        elementu3.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //        elementu3.x = dataCordenadXMirrow + AddModulo - 90;
            //        elementu3.y = (dataCordenadY - dataWith / 10) - 12;
            //        elementu3.z = data.ZCoordinate + 225;
            //        elementu3.XRotate = 180 + RotateWall;
            //        _listRenderElement.Add(elementu3);
            //    }
            //}
        }
        private static void SedMensulas()
        {
            //    var InsertMensula = 3;
            //    for (int i = 0; i < n; i++)
            //    {
            //        if (InsertMensula == 0) { InsertMensula = 3; }
            //        if (InsertMensula == 3)
            //        {
            //            //../../Content/DesignTools/Control/Cube.stl
            //            ModelRenderElement elementMensula = new ModelRenderElement();
            //            elementMensula.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/4120000042.stl";
            //            elementMensula.CodeName = "4120000042";
            //            elementMensula.x = dataCordenadX + AddModulo + 14;
            //            elementMensula.y = dataCordenadY + 12;
            //            elementMensula.z = data.ZCoordinate + 240;
            //            elementMensula.XRotate = 0 + RotateWall;
            //            ListRenderElement.Add(elementMensula);
            //        }
            //        InsertMensula -= 1;
            //        ModelRenderElement element = new ModelRenderElement();
            //        element.Element = "../../Content/DesignTools/Stl/ATK60/27904209.stl";
            //        element.ElementF = "../../Content/DesignTools/Stl/ATK60/27904209_F.stl";
            //        element.CodeName = "27904209";
            //        element.x = dataCordenadX + AddModulo;
            //        RestPoint = (int)dataCordenadX + AddModulo;
            //        element.y = dataCordenadY;
            //        element.z = data.ZCoordinate;
            //        element.XRotate = 0 + RotateWall;
            //        ListRenderElement.Add(element);
            //        if (i != 0)
            //        {
            //            //Cerrojo 1
            //            ModelRenderElement elementu = new ModelRenderElement();
            //            elementu.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //            elementu.CodeName = "10004220";
            //            elementu.x = dataCordenadX + AddModulo;
            //            elementu.y = dataCordenadY + 12;
            //            elementu.z = data.ZCoordinate + 45;
            //            elementu.XRotate = 0 + RotateWall;
            //            ListRenderElement.Add(elementu);
            //            ModelRenderElement elementu2 = new ModelRenderElement();
            //            elementu2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //            elementu2.CodeName = "10004220";
            //            elementu2.x = dataCordenadX + AddModulo;
            //            elementu2.y = dataCordenadY + 12;
            //            elementu2.z = data.ZCoordinate + 135;
            //            elementu2.XRotate = 0 + RotateWall;
            //            ListRenderElement.Add(elementu2);
            //            ModelRenderElement elementu3 = new ModelRenderElement();
            //            elementu3.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10004220.stl";
            //            elementu3.CodeName = "10004220";
            //            elementu3.x = dataCordenadX + AddModulo;
            //            elementu3.y = dataCordenadY + 12;
            //            elementu3.z = data.ZCoordinate + 225;
            //            elementu3.XRotate = 0 + RotateWall;
            //            ListRenderElement.Add(elementu3);
            //            // Tirante

            //        }
            //        ModelRenderElement elementT = new ModelRenderElement();
            //        elementT.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            //        elementT.CodeName = "10443020";
            //        elementT.x = dataCordenadX + AddModulo + 4;
            //        elementT.y = dataCordenadY + 13;
            //        elementT.z = data.ZCoordinate + 55;
            //        elementT.XRotate = 0 + RotateWall;
            //        ListRenderElement.Add(elementT);

            //        ModelRenderElement elementT2 = new ModelRenderElement();
            //        elementT2.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            //        elementT2.CodeName = "10443020";
            //        elementT2.x = dataCordenadX + AddModulo + 4;
            //        elementT2.y = dataCordenadY + 13;
            //        elementT2.z = data.ZCoordinate + 215;
            //        elementT2.XRotate = 0 + RotateWall;
            //        ListRenderElement.Add(elementT2);

            //        ModelRenderElement elementTdwidag0 = new ModelRenderElement();
            //        elementTdwidag0.ElementUnion1 = MeshTdwidag;
            //        elementT2.CodeName = "10443020";
            //        elementTdwidag0.x = dataCordenadX + AddModulo + 4;
            //        elementTdwidag0.y = dataCordenadY - ((dataWith / 10) / 2);
            //        elementTdwidag0.z = data.ZCoordinate + 55;
            //        elementTdwidag0.XRotate = 0 + RotateWall;
            //        ListRenderElement.Add(elementTdwidag0);

            //        ModelRenderElement elementTdwidag = new ModelRenderElement();
            //        elementTdwidag.ElementUnion1 = MeshTdwidag;
            //        elementTdwidag.CodeName = _codeName;
            //        elementTdwidag.x = dataCordenadX + AddModulo + 4;
            //        elementTdwidag.y = dataCordenadY - ((dataWith / 10) / 2);
            //        elementTdwidag.z = data.ZCoordinate + 215;
            //        elementTdwidag.XRotate = 0 + RotateWall;
            //        ListRenderElement.Add(elementTdwidag);
            //        AddModulo += 90;
            //    }
        }


        private static void GetTdwidag(long dataWith)
        {
            if (dataWith > 500)
            {
                MeshTdwidag = "../../Content/DesignTools/Stl/ATK60/0230120.stl";
                _codeName = "0230120";
            }
            if (dataWith > 700)
            {
                MeshTdwidag = "../../Content/DesignTools/Stl/ATK60/0230150.stl";
                _codeName = "0230150";
            }
            if (dataWith > 1000)
            {
                MeshTdwidag = "../../Content/DesignTools/Stl/ATK60/0230200.stl";
                _codeName = "0230200";
            }
        }

        private static int getRest(long rest)
        {
            var value = 30;

            if (rest >= 450)
            {
                value = 450;
            }
            if (rest >= 600)
            {
                value = 60;
            }
            if (rest >= 900)
            {
                value = 90;
            }
            return value;
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