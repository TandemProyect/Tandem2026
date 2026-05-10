using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloTape_0_02 : BaseController
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
                elementWood.Filter = "TapeS2";
                elementWood.x = endWallX;
                elementWood.y = dataCordenadY;
                elementWood.z = 30;
                elementWood.heightWood = DataHeight;
                elementWood.XRotate = 0;
                ListRenderElement.Add(elementWood);
            }
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