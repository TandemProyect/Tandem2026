using DAL;
using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloWallEsqTEsq_70_90 : BaseController
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
            var IsAngular90 = false;
            var IsAngular_00 = false;
            if (Tape_270 == "Universal_X")
            {
                IsAngular_00 = true;
            }
            if (Tape_180 == "Universal_Y")
            {
                IsAngular90 = true;
            }
            List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
            if (IsAngular90 == false)
            {
                var testLong = "450";
                if (datalong > 450) { testLong = "600"; }
                if (datalong > 600) { testLong = "750"; }
                if (datalong > 750) { testLong = "900"; }
                if (datalong > 900) { testLong = "1050"; }
                if (datalong > 1050) { testLong = "1200"; }
                switch (testLong)
                {
                    case "450":
                        ListRenderElementPanel270 = WallCorner.CornerPanel180_45.setdListElement(IsAngular_00, type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                    case "600":
                        ListRenderElementPanel270 = WallCorner.CornerPanel180_60.setdListElement(IsAngular_00, type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                    case "750":
                        ListRenderElementPanel270 = WallCorner.CornerPanel180_75.setdListElement(IsAngular_00, type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                    case "900":
                        ListRenderElementPanel270 = WallCorner.CornerPanel180_90.setdListElement(IsAngular_00, type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                    case "1050":
                        //ListRenderElementPanel270 = WallCorner.CornerPanel90_180_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                    case "1200":
                        //ListRenderElementPanel270 = WallCorner.CornerPanel90_180_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                        break;
                }
            }
            else
            {
                ListRenderElementPanel270 = WallTapeR.TapeRP_E70_180.setdListElement(dataCordenadX/* - (dataWith / 10)*/, 0, dataHeight, datalong, dataWith, (dataCordenadY + 75 + (dataWith / 10)) - (datalong / 10), 270, true, dataCordenadY, currentDefaultDisign.ExitingPanel2400);
            }



            if (ListRenderElementPanel270 != null)
            {
                foreach (var item in ListRenderElementPanel270)
                {
                    ModelRenderElement element = new ModelRenderElement();
                    element.IdElement = item.IdElement;
                    element.CodeName = item.CodeName;
                    element.Element = item.Element;
                    element.ElementF = item.ElementF;
                    element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                    element.LongDimTypeVertical = item.LongDimTypeVertical;
                    element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
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
