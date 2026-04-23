using DAL;
using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloWallEsqTEsq_50_00 : BaseController
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

            var ListRenderElementEsq = WallCorner.Corner50_00_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + (((datalong / 10) - 30)) - (dataWith / 10), dataCordenadY, DataWithOtherCorner);
            if (ListRenderElementEsq != null)
            {
                foreach (var item in ListRenderElementEsq)
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
            var IsAngular = true;
            if (Tape_0 == "Universal_X") { IsAngular = false; }
            if (Tape_270 == "Universal_Y") { IsAngular = false; }
            if (Tape_0 == "Other_Universal_X") { IsAngular = false; }
            if (Tape_270 == "Other_Universal_X") { IsAngular = false; }
            if (IsAngular == true)
            {
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, (int)datalong, 1, 0, DimType.Horizontal, typeMesh, "", 0);
                var ListRenderElementAng = SedAng270_0.setdListElement(dataCordenadX + (datalong / 10), currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                if (ListRenderElementAng != null)
                {
                    foreach (var item in ListRenderElementAng)
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
            }

            List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
            if (Tape_270 == "Universal_Y")
            {
                var Position_Y = (dataCordenadY + 30 + dataWith / 10) - (PanelPerfil / 2);
                List<ModelRenderElement> ListRenderElementDataType0 = null;
                CommonElement.AddDimHorizontal(0, ListRenderElement, dataCordenadX, dataCordenadY, 750, 1, 0, DimType.Horizontal, typeMesh, "", 0);

                ListRenderElementDataType0 = WallTapeR.TapeRP_E50_270.setdListElement(dataCordenadX, 0, dataHeight, dataWith, datalong, dataCordenadY, 270, true, Position_Y, currentDefaultDisign.ExitingPanel2400);
                if (ListRenderElementDataType0 != null)
                {
                    foreach (var item in ListRenderElementDataType0)
                    {
                        ModelRenderElement element = new ModelRenderElement();
                        element.IdElement = item.IdElement;
                        element.CodeName = item.CodeName;
                        element.Element = item.Element;
                        element.ElementF = item.ElementF;
                        element.Type = item.Type;
                        element.LongDimTypeHorizontal = item.LongDimTypeHorizontal;
                        element.LongDimTypeHorizontalT = item.LongDimTypeHorizontalT;
                        element.LongDimTypeVertical = item.LongDimTypeVertical;
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

            }
            else
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
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_45.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "600":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_60.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "750":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_75.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "900":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_90.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1050":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1200":
                        ListRenderElementPanel270 = WallCorner.CornerPanel270_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
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
