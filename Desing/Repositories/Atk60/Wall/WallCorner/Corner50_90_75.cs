using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall.WallCorner
{
    public class Corner50_90_75 : BaseController
    {
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        private static long LastPanel = 0;
        internal static List<ModelRenderElement> setdListElement(long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedPanels(type, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        private static void SedPanels(long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            type = 2;
            Is2700 = false;
            var Elevation = 0;
            var ElevationDiwydag = 0;
            int RestTypeHeight = 300;
            int n = (int)((DataHeight + 149) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                Is2700 = true;
                for (int i = 0; i < n; i++)
                {
                    if (i == 0)
                    {
                        Elevation = 45;
                    }
                    else
                    {
                        Elevation = Elevation + 270;
                        CommonElement.UnionRiji(2, type, PanelPerfil, 0, dataCordenadX, 25, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 15, dataCordenadY, nHeight, ListRenderElement, "90");

                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 225, ListRenderElement, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 225, ListRenderElement, "");

                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("PanelReg270");
                    element.ElementF = Atk60Element.GetElement("PanelReg270F");
                    element.CodeName = "27104219";
                    element.LongDimTypeHorizontal = (long?)750;
                    element.LongDimTypeVertical = (long?)2700;
                    element.x = dataCordenadX;
                    element.z = element.z + nHeight;
                    element.XRotate = 270;
                    element.y = dataCordenadY + 75;
                    ListRenderElement.Add(element);
                    ElevationDiwydag = ElevationDiwydag + 270;
                    nHeight = nHeight + 270;
                }
            }
            if (restHeight > 0)
            {
                RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    if (currentDefaultDisign.ExitingPanel2400 == true)
                    {
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert2400Element(IsRiji, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                        LastPanel = 2400;
                    }
                    else
                    {
                        Insert1200Element(1, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                        nHeight = nHeight + 120;
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert1200Element(IsRiji, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                        nHeight = nHeight + 120;
                    }
                }
                if (RestTypeHeight == 1200)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert1200Element(IsRiji, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 1200;
                }
                //if (RestTypeHeight == 2700)
                //{
                //    var IsRiji = 0;
                //    if (nHeight != 0)
                //    {
                //        IsRiji = 1;
                //    }
                //    Insert2700Element(IsRiji, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                //    LastPanel = 1200;
                //}
            }
        }
        //private static void Insert2700Element(long IsRiji, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        //{
        //    var SupNHeight = 270;
        //    if (IsRiji == 1)
        //    {
        //        if (Is2700 == true)
        //        {
        //            SupNHeight = 0;
        //        }
        //        CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 25, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
        //        CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 25, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
        //        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 15, dataCordenadY, nHeight, ListRenderElement, "90");
        //        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 15, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
        //        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight, ListRenderElement, "90");
        //        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
        //    }
        //    ModelRenderElement element = new ModelRenderElement();
        //    element.Element = Atk60Element.GetElement("Panel60270");
        //    element.ElementF = Atk60Element.GetElement("Panel60270F");
        //    element.LongDimTypeHorizontal = (long?)600;
        //    element.LongDimTypeVertical = (long?)2700;
        //    element.CodeName = "27604207";
        //    element.z = element.z + nHeight;
        //    element.x = dataCordenadX;
        //    element.y = dataCordenadY;
        //    element.XRotate = 0;
        //    ListRenderElement.Add(element);
        //}
        private static void Insert1200Element(long IsRiji, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            type = 2;
            var SupNHeight = 120;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 25, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 15, dataCordenadY, nHeight + SupNHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, "90");

            }
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 20, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 80, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 20, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 80, ListRenderElement, "");
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("PanelReg120");
            element.ElementF = Atk60Element.GetElement("PanelReg120F");
            element.LongDimTypeHorizontal = (long?)750;
            element.LongDimTypeVertical = (long?)1200;
            element.CodeName = "12104120";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.XRotate = 270;
            element.y = dataCordenadY + 75;
            ListRenderElement.Add(element);
        }
        private static void Insert2400Element(long IsRiji, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            var SupNHeight = 240;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 25, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 60, dataCordenadY, nHeight, ListRenderElement, "90");
            }
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 40, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 200, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 40, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY + 75, nHeight + 200, ListRenderElement, "");


            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("PanelReg240");
            element.ElementF = Atk60Element.GetElement("PanelReg240F");
            element.LongDimTypeHorizontal = (long?)750;
            element.LongDimTypeVertical = (long?)2400;
            element.CodeName = "24104224";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.XRotate = 270;
            element.y = dataCordenadY + 75;
            ListRenderElement.Add(element);
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