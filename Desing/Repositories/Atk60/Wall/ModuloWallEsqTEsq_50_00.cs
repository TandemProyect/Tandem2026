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
            var IsAngular = true;
            if (Tape_0 == "Universal_X") { IsAngular = false; }
            if (Tape_270 == "Universal_Y") { IsAngular = false; }
            if (Tape_0 == "Other_Universal_X") { IsAngular = false; }
            if (Tape_270 == "Other_Universal_X") { IsAngular = false; }

            if (IsAngular == true)
            {
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
            }
            var ListRenderElementEsq = WallCorner.Corner50_00_Esq.setdListElement(3, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, DataWithOtherCorner);
            if (ListRenderElementEsq != null)
            {
                foreach (var item in ListRenderElementEsq)
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
            long rest = datalong - (dataWith + 300);
            if (rest > 5)
            {
            }
            else
            {
                List<ModelRenderElement> ListRenderElementUnion = new List<ModelRenderElement>();
                ListRenderElementUnion = WallCorner.Corner50_00_Union.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY);
                if (ListRenderElementUnion != null)
                {
                    foreach (var item in ListRenderElementUnion)
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
            }
            List<ModelRenderElement> ListRenderElementPanel270 = new List<ModelRenderElement>();
            if (Tape_270 == "Universal_Y")
            {
                var dimTypeVertical = DimType.No;
                var DimTypeH = DimType.Horizontal;
                var Elevation = 0;
                var ElevationDiwydag = 0;
                int RestTypeHeight = 300;
                int n = (int)((dataHeight + 149) / 2700);
                var restHeight = (int)((dataHeight) - (2700 * n));
                var nHeight = 0;
                var Position_Y = (dataCordenadY + 30 + dataWith / 10) - (PanelPerfil / 2);
                if (n >= 1)
                {
                    for (int i = 0; i < n; i++)
                    {
                        List<ModelRenderElement> ListRenderElementDataType0 = null;
                        ListRenderElementDataType0 = WallTapeR.TapeRP_270.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 270, true);
                        if (ListRenderElementDataType0 != null)
                        {
                            foreach (var item in ListRenderElementDataType0)
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
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_R_0 = null;
                        ListRenderElementDataTypeUnion_R_0 = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 270);
                        if (ListRenderElementDataTypeUnion_R_0 != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_R_0)
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
                            //Paneles
                            List<ModelRenderElement> ListRenderElementDataType0 = null;
                            ListRenderElementDataType0 = WallTapeR.TapeRP_270.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 240, true);
                            if (ListRenderElementDataType0 != null)
                            {
                                foreach (var item in ListRenderElementDataType0)
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
                            //Uniones
                            List<ModelRenderElement> ListRenderElementDataTypeUnion_0_R = null;
                            ListRenderElementDataTypeUnion_0_R = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, Position_Y, 240);
                            if (ListRenderElementDataTypeUnion_0_R != null)
                            {
                                foreach (var item in ListRenderElementDataTypeUnion_0_R)
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
                        else
                        {
                            //Paneles
                            List<ModelRenderElement> ListRenderElementDataType0 = null;
                            ListRenderElementDataType0 = WallTapeR.TapeRP_270.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 120, true);
                            if (ListRenderElementDataType0 != null)
                            {
                                foreach (var item in ListRenderElementDataType0)
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
                            //Uniones
                            List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R = null;
                            ListRenderElementDataTypeUnion_270_R = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, Position_Y, 120);
                            if (ListRenderElementDataTypeUnion_270_R != null)
                            {
                                foreach (var item in ListRenderElementDataTypeUnion_270_R)
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
                            //Paneles 2
                            nHeight = nHeight + 120;
                            List<ModelRenderElement> ListRenderElementDataType02Level = null;
                            ListRenderElementDataType02Level = WallTapeR.TapeRP_270.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 120, true);
                            if (ListRenderElementDataType02Level != null)
                            {
                                foreach (var item in ListRenderElementDataType02Level)
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
                            //Uniones 2
                            List<ModelRenderElement> ListRenderElementDataTypeUnion_270_R2 = null;
                            ListRenderElementDataTypeUnion_270_R2 = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 120);
                            if (ListRenderElementDataTypeUnion_270_R2 != null)
                            {
                                foreach (var item in ListRenderElementDataTypeUnion_270_R2)
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
                    }
                    if (RestTypeHeight == 1200)
                    {
                        //Paneles
                        List<ModelRenderElement> ListRenderElementDataType0 = null;
                        ListRenderElementDataType0 = WallTapeR.TapeRP_270.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 120, true);
                        if (ListRenderElementDataType0 != null)
                        {
                            foreach (var item in ListRenderElementDataType0)
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
                        //Uniones
                        List<ModelRenderElement> ListRenderElementDataTypeUnion_270_L = null;
                        ListRenderElementDataTypeUnion_270_L = WallTapeR.TapeRU_270_L.setdListElement(dataCordenadX + 75, nHeight, dataWith, datalong, dataCordenadY, 120);
                        if (ListRenderElementDataTypeUnion_270_L != null)
                        {
                            foreach (var item in ListRenderElementDataTypeUnion_270_L)
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
                    return ListRenderElement;
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
                        ListRenderElementPanel270 = WallCorner.Corner50_00_45.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "600":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_60.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "750":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_75.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "900":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_90.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1050":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_1050.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
                        break;
                    case "1200":
                        ListRenderElementPanel270 = WallCorner.Corner50_00_1200.setdListElement(type, currentDefaultDisign, dataHeight, dataWith, datalong, dataCordenadX, dataCordenadY, IsAngular);
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
