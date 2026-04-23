using DAL;
using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloWallEsqTEsq_10_00 : BaseController
    {
        private static long PanelPerfil = 12;
        internal static List<ModelRenderElement> setdListElement(
        string typeMesh,
        long yWith,
        long xWith,
        bool universalPanel,
        TSql_DefaultDesign currentDefaultDisign,
        long dataHeight,
        long dataWith,
        long datalong,
        long dataCordenadX,
        long dataCordenadY,
        long type,
        long? DataWithOtherCorner,
        string Tape_0,
        string Tape_180,
        string Tape_90,
        string Tape_270
        )
        {
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();

            List<ModelRenderElement> ListRenderElementPanel90 = new List<ModelRenderElement>();
            var testLong = "450";
            if (datalong > 450) { testLong = "600"; }
            if (datalong > 600) { testLong = "750"; }
            if (datalong > 750) { testLong = "900"; }
            if (datalong > 900) { testLong = "1050"; }
            if (datalong > 1050) { testLong = "1200"; }
            switch (testLong)
            {
                case "450":
                    ListRenderElementPanel90 = Wall.WallCorner.CornerPanel90_45.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                    break;
                case "600":
                    ListRenderElementPanel90 = Wall.WallCorner.CornerPanel90_60.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + 60, dataCordenadY - dataWith / 10);
                    break;
                case "750":
                    ListRenderElementPanel90 = Wall.WallCorner.CornerPanel90_75.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + 75, dataCordenadY - dataWith / 10);
                    break;
                case "900":
                    ListRenderElementPanel90 = Wall.WallCorner.CornerPanel90_90.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + 90, dataCordenadY - dataWith / 10);
                    break;
                case "1050":
                    //ListRenderElementPanel90 = Wall.WallCorner.Corner50_90_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                    break;
                case "1200":
                    //ListRenderElementPanel90 = Wall.WallCorner.Corner50_90_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                    break;
            }
            if (ListRenderElementPanel90 != null)
            {
                foreach (var item in ListRenderElementPanel90)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.IdElement = item.IdElement;
                    element.CodeName = item.CodeName;
                    element.Element = item.Element;
                    element.ElementF = item.ElementF;

                    element.ElementWood = item.ElementWood;
                    element.ElementUnion1 = item.ElementUnion1;
                    element.LongWood = item.LongWood;
                    element.heightWood = item.heightWood;
                    element.x = item.x;
                    element.y = item.y;
                    element.z = item.z;
                    element.XRotate = item.XRotate;
                    element.YRotate = item.YRotate;
                    element.ZRotate = item.ZRotate;
                    element.CodeName = item.CodeName;
                    element.Filter = item.Filter;
                    ListRenderElement.Add(element);
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
