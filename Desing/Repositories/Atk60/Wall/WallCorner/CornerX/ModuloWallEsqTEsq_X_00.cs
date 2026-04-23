using DAL;
using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class ModuloWallEsqTEsq_X_00 : BaseController
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
            var ListRenderElementEsq180 = WallCorner.CornerX_180_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + 30, dataCordenadY - ((dataWith / 10) + 30), DataWithOtherCorner);
            if (ListRenderElementEsq180 != null)
            {
                foreach (var item in ListRenderElementEsq180)
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
            var ListRenderElementEsq270 = WallCorner.CornerX_270_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, DataWithOtherCorner);
            if (ListRenderElementEsq270 != null)
            {
                foreach (var item in ListRenderElementEsq270)
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
            var ListRenderElementEsqX00 = WallCorner.CornerX_00_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX + (datalong / 10), dataCordenadY - (dataWith / 10), DataWithOtherCorner);
            if (ListRenderElementEsqX00 != null)
            {
                foreach (var item in ListRenderElementEsqX00)
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
            var ListRenderElementEsqX90 = WallCorner.CornerX_90_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, (dataCordenadX + (datalong / 10) - 30), dataCordenadY + (dataWith / 10), DataWithOtherCorner);
            if (ListRenderElementEsqX90 != null)
            {
                foreach (var item in ListRenderElementEsqX90)
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
