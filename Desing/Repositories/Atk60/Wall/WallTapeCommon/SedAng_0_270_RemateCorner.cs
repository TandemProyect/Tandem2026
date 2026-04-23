using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class SedAng_0_270_RemateCorner : BaseController
    {
        internal static List<ModelRenderElement> setdListElement(long EndWallX, long LongLeft, long LongRight, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY)
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

            SedPanels30(EndWallX, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement);
            return ListRenderElement;
        }
        //With 30
        private static void SedPanels30(long endWallX, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            var realdataWith = dataWith;
            dataWith = 300;
            if (dataWith >= 450)
            { dataWith = 450; }
            if (dataWith >= 600)
            { dataWith = 600; }
            if (dataWith >= 750)
            { dataWith = 750; }
            if (dataWith >= 900)
            { dataWith = 900; }
            if (dataWith >= 1050)
            { dataWith = 1050; }
            if (dataWith >= 1200)
            { dataWith = 1200; }
            var Remate = (realdataWith - dataWith) / 10;
            {
                ModelRenderElement elementWood = new ModelRenderElement();
                elementWood.ElementWood = "../../Content/DesignTools/Control/Cube.stl";
                elementWood.LongWood = dataWith;
                elementWood.ParametFilter = Remate;
                elementWood.CodeName = "Wood";
                elementWood.Filter = "SEMA03";
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
                    CommonElement.SedUnionRigiHorizontal_0_Solape(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 65, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0_Solape(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 140, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0_Solape(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 220, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    nHeight = nHeight + 270;
                }
            }
            if (restHeight > 0)
            {
                var RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2400)
                {
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 55, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 140, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 225, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);

                    nHeight = nHeight + 240;
                }
                if (RestTypeHeight == 1200)
                {
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 25, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
                    CommonElement.SedUnionRigiHorizontal_0(DataHeight, Remate, endWallX + 7, currentDefaultDisign, nHeight + 115, datalong, dataCordenadX, dataCordenadY, realdataWith, ListRenderElement);
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