using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTape_0_01 : BaseController
    {
        private static bool HasPreviousModule = false;
        private static bool IsEndModule = false;
        private static bool IsFirstModule = false;
        private static long LastPanel = 0;
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

            SedPanels30(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        //With 30
        private static void SedPanels30(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            var Remate = (dataCordenadX - endWallX) / 10;
            {
                ModelRenderElement elementWood = new ModelRenderElement();
                elementWood.ElementWood = "../../Content/DesignTools/Control/Cube.stl";
                elementWood.LongWood = dataWith;
                elementWood.ParametFilter = Remate;
                elementWood.CodeName = "Wood";
                elementWood.Filter = "SExS01";
                elementWood.x = endWallX;
                elementWood.y = dataCordenadY;
                elementWood.z = 30;
                elementWood.heightWood = DataHeight;
                elementWood.XRotate = 0;
                ListRenderElement.Add(elementWood);
            }
            int n = (int)((DataHeight + 299) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                for (int i = 0; i < n; i++)
                {
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 65, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 140, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 220, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    nHeight = nHeight + 270;
                }
            }
            if (restHeight > 0)
            {
                var RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    //if (currentDefaultDisign.ExitingPanel2400 == true)
                    //{
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 55, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 140, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 225, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    nHeight = nHeight + 240;
                    //}
                    //else
                    //{
                    //    CommonElement.SedUnionRigiHorizontal_0(Remate, endWallX, currentDefaultDisign, nHeight + 25, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    //    CommonElement.SedUnionRigiHorizontal_0(Remate, endWallX, currentDefaultDisign, nHeight + 115, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    //    nHeight = nHeight + 120;
                    //    CommonElement.SedUnionRigiHorizontal_0(Remate, endWallX, currentDefaultDisign, nHeight + 25, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    //    CommonElement.SedUnionRigiHorizontal_0(Remate, endWallX, currentDefaultDisign, nHeight + 115, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    //}
                }
                if (RestTypeHeight == 1200)
                {
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 25, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX, currentDefaultDisign, nHeight + 115, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
                    nHeight = nHeight + 120;
                }
            }
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