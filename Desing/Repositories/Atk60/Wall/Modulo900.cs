using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class Modulo900 : BaseController
    {
        private static bool HasPreviousModule = false;
        private static bool IsEndModule = false;
        private static bool IsFirstModule = false;
        private static long LastPanel = 0;
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        internal static List<ModelRenderElement> setdListElement(string Tape_180o90, string Tape_0o270, long EndWallX, long EndWallY, long LongLeft, long LongRight, bool IsFirstModule, bool IsDimActive, long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long dataWith, long datalong, long dataCordenadX, long dataCordenadY, RotateMesh meshRotateX, RotateMesh meshRotateMirrowX, string dataRotateZ, long _Type, bool _isEndModule, bool _HasPreviousModule, long _DataSupEnd)
        {
            if (Tape_0o270 != null)
            {
                if (Tape_0o270.Substring(0, 3) == "Esq")
                {
                    _isEndModule = false;
                }
                if (Tape_0o270 == "TapeS2")
                {
                    _isEndModule = false;
                }
            }
            if (_DataSupEnd == 0)
            {
                //if (_isEndModule == true)
                //{
                //    if (type == 1)
                //    {
                //        dataCordenadX = EndWallX - 90;
                //    }
                //    if (type == 2)
                //    {
                //        dataCordenadY = EndWallY - 90;
                //    }
                //}
            }
            IsEndModule = _isEndModule;
            HasPreviousModule = _HasPreviousModule;
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedPanels(IsFirstModule, IsDimActive, type, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement, _Type, _isEndModule, _HasPreviousModule);
            return ListRenderElement;
        }
        private static void SedPanels(bool IsFirstModule, bool _isDimActive, long type, DAL.TSql_DefaultDesign currentDefaultDisign, long DataHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement, long _Type, bool _isEndModule, bool _HasPreviousModule)
        {
            Is2700 = false;
            var dimTypeVertical = DimType.No;
            if (_isDimActive == true)
            {
                dimTypeVertical = DimType.Vertical;
            }

            if (IsFirstModule == false)
            {
                dimTypeVertical = DimType.No;
            }
            var Elevation = 0;
            var ElevationDiwydag = 0;
            int RestTypeHeight = 300;
            int n = (int)((DataHeight + 249) / 2700);
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n >= 1)
            {
                Is2700 = true;
                LastPanel = 2700;
                for (int i = 0; i < n; i++)
                {
                    if (_isEndModule != true)
                    {
                        var Suplement = 0;
                        if (type != 2)
                        {
                            Suplement = 90;
                        }
                        CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX + Suplement, 0, dataCordenadY, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX + Suplement, 0, dataCordenadY, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX + Suplement, 0, dataCordenadY, nHeight + 225, ListRenderElement, "");
                    }
                    else
                    {
                        //CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        //CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                    }
                    if (i == 0)
                    {
                        Elevation = 45;
                    }
                    else
                    {
                        Elevation = Elevation + 270;
                        CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 60, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 60, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                    }
                    if (_HasPreviousModule != true)
                    {
                        CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                    }
                    else
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 225, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 45, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 135, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 225, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(),"",0);

                    ModelRenderElement element = new ModelRenderElement();
                    element.Element = Atk60Element.GetElement("Panel90270");
                    element.ElementF = Atk60Element.GetElement("Panel90270F");
                    element.CodeName = "27904209";
                    element.x = dataCordenadX;
                    element.y = dataCordenadY;
                    element.z = element.z + nHeight;
                    element.XRotate = 0;
                    if (type == 2)
                    {
                        element.XRotate = 270;
                        element.y = dataCordenadY + 90;
                    }
                    ListRenderElement.Add(element);
                    ElevationDiwydag = ElevationDiwydag + 270;
                    nHeight = nHeight + 270;
                }

                //Mirror
                nHeight = 0;
                for (int i = 0; i < n; i++)
                {
                    ModelRenderElement elements11 = new ModelRenderElement();
                    elements11.Element = Atk60Element.GetElement("Panel90270");
                    elements11.ElementF = Atk60Element.GetElement("Panel90270F");
                    elements11.CodeName = "27904209";
                    elements11.z = elements11.z + nHeight;
                    elements11.x = dataCordenadX + 90;
                    elements11.y = (dataCordenadY - dataWith / 10);
                    elements11.XRotate = 180;
                    if (type == 2)
                    {
                        elements11.y = dataCordenadY;
                        elements11.x = (dataCordenadX - dataWith / 10);
                        elements11.XRotate = 90;
                    }
                    ListRenderElement.Add(elements11);


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
                        Insert2400Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule, _HasPreviousModule);
                        LastPanel = 2400;
                    }
                    else
                    {
                        Insert1200Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule, _HasPreviousModule);
                        nHeight = nHeight + 120;
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }

                        Insert1200Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule, _HasPreviousModule);
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
                    Insert1200Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule, _HasPreviousModule);
                    LastPanel = 1200;
                }
                if (RestTypeHeight == 2700)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert2700Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule, _HasPreviousModule);
                    LastPanel = 1200;
                }
            }
        }

        private static void Insert2700Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong, bool _isEndModule, bool _HasPreviousModule)
        {
            if (_isEndModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            else
            {
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            if (_HasPreviousModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            var SupNHeight = 270;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 60, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 60, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(),"",0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel90270");
            element.ElementF = Atk60Element.GetElement("Panel90270F");
            element.CodeName = "27904209";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 90;
            }
            ListRenderElement.Add(element);

            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel90270");
            element4.ElementF = Atk60Element.GetElement("Panel90270F");
            element4.CodeName = "27904209";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 90;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert1200Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong, bool _isEndModule, bool _HasPreviousModule)
        {
            if (_isEndModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            }
            else
            {
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            }
            if (_HasPreviousModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            }
            var SupNHeight = 120;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight + SupNHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 70, dataCordenadY, nHeight + SupNHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 70, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "90");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(),"",0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel90120");
            element.ElementF = Atk60Element.GetElement("Panel90120F");
            element.CodeName = "12904215";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 90;
            }
            ListRenderElement.Add(element);

            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel90120");
            element4.ElementF = Atk60Element.GetElement("Panel90120F");
            element4.CodeName = "12904215";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 90;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert2400Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong, bool _isEndModule, bool _HasPreviousModule)
        {
            if (_isEndModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 185, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            else
            {
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 185, false);
                //CommonElement.SedDiwydagD(type, PanelPerfil, 80, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            if (_HasPreviousModule != true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 185, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
            }
            var SupNHeight = 240;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 40, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 70, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 70, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(),"",0);

            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel90240");
            element.ElementF = Atk60Element.GetElement("Panel90240F");
            element.CodeName = "24904240";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 90;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel90240");
            element4.ElementF = Atk60Element.GetElement("Panel90240F");
            element4.CodeName = "24904240";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 90;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
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