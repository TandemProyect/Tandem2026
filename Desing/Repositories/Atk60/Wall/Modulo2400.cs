using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class Modulo2400 : BaseController
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
                if (type == 1)
                {
                    dataCordenadX = EndWallX - 240;
                }
                if (type == 2)
                {
                    dataCordenadY = EndWallY - 240;
                }
            }

            IsEndModule = _isEndModule;
            HasPreviousModule = _HasPreviousModule;
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            SedPanels(IsFirstModule, IsDimActive, type, currentDefaultDisign, DataHeight, datalong, dataCordenadX, dataCordenadY, dataWith, ListRenderElement, _Type, _isEndModule, _HasPreviousModule);
            InsertProp.SedProp(dataWith, LongLeft, DataHeight, _Type, dataCordenadX, dataCordenadY, ListRenderElement, 59, 61, 120, 121, true);
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

            int n = (int)((DataHeight) / 2700);
            if (DataHeight > 2705 && DataHeight < 2855)
            {
                n = 0;
            }
            var restHeight = (int)((DataHeight) - (2700 * n));
            var nHeight = 0;
            if (n == 0)
            {
                if (restHeight > 2551)
                {
                    n = n + 1;
                    restHeight = restHeight - 2700;
                }
            }
            if (n >= 1)
            {
                Is2700 = true;
                LastPanel = 2700;
                for (int i = 0; i < n; i++)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 225, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 45, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 135, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 225, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 45, ListRenderElement, dataWith / 10, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 135, ListRenderElement, dataWith / 10, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 225, ListRenderElement, dataWith / 10, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 45, ListRenderElement, dataWith / 10, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 80, ListRenderElement, dataWith / 10, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 225, ListRenderElement, dataWith / 10, "");
                    if (_HasPreviousModule == false)
                    {
                        CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                    }
                    else
                    {
                        CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    }
                    if (_isEndModule == true)
                    {
                        //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                        //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                    }
                    else
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 45, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 135, ListRenderElement, "");
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 225, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 45, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 135, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 225, ListRenderElement, dataWith / 10, "");
                    }
                    //Horisontal
                    if (i == 0)
                    {
                        Elevation = 45;
                    }
                    else
                    {
                        Elevation = Elevation + 270;
                        CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, Elevation - 30, ListRenderElement, dataWith / 10, "");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 220, dataCordenadY, nHeight, ListRenderElement, "90");
                        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                        CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 220, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                    }
                    CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(), "", 0);
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
                    CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 90, DimType.Horizontal, type.ToString(), "", 0);
                    ModelRenderElement element2 = new ModelRenderElement();
                    element2.Element = Atk60Element.GetElement("Panel90270");
                    element2.ElementF = Atk60Element.GetElement("Panel90270F");
                    element2.CodeName = "27904209";
                    element2.z = element2.z + nHeight;
                    element2.x = dataCordenadX + 90;
                    element2.y = dataCordenadY;
                    element2.XRotate = 0;
                    if (type == 2)
                    {
                        element2.XRotate = 270;
                        element2.x = dataCordenadX;
                        element2.y = dataCordenadY + 180;
                    }
                    ListRenderElement.Add(element2);
                    CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 600, type, 180, DimType.Horizontal, type.ToString(), "", 0);
                    ModelRenderElement element3 = new ModelRenderElement();
                    element3.Element = Atk60Element.GetElement("Panel60270");
                    element3.ElementF = Atk60Element.GetElement("Panel60270F");
                    element3.CodeName = "27604207";
                    element3.z = element3.z + nHeight;
                    element3.x = dataCordenadX + 180;
                    element3.y = dataCordenadY;
                    element3.XRotate = 0;
                    if (type == 2)
                    {
                        element3.XRotate = 270;
                        element3.x = dataCordenadX;
                        element3.y = dataCordenadY + 240;
                    }
                    ListRenderElement.Add(element3);
                    nHeight = nHeight + 270;
                    ElevationDiwydag = ElevationDiwydag + 270;
                }
                //Mirror
                nHeight = 0;
                for (int i = 0; i < n; i++)
                {
                    ModelRenderElement elements = new ModelRenderElement();
                    elements.Element = Atk60Element.GetElement("Panel90270");
                    elements.ElementF = Atk60Element.GetElement("Panel90270F");
                    elements.CodeName = "27904209";
                    elements.z = elements.z + nHeight;
                    elements.x = dataCordenadX + 90;
                    elements.y = (dataCordenadY - dataWith / 10);
                    elements.XRotate = 180;
                    if (type == 2)
                    {
                        elements.y = dataCordenadY;
                        elements.x = (dataCordenadX - dataWith / 10);
                        elements.XRotate = 90;
                    }
                    ListRenderElement.Add(elements);
                    ModelRenderElement elements2 = new ModelRenderElement();
                    elements2.Element = Atk60Element.GetElement("Panel90270");
                    elements2.ElementF = Atk60Element.GetElement("Panel90270F");
                    elements2.CodeName = "27904209";
                    elements2.z = elements2.z + nHeight;
                    elements2.x = dataCordenadX + 180;
                    elements2.y = (dataCordenadY - dataWith / 10);
                    elements2.XRotate = 180;
                    if (type == 2)
                    {
                        elements2.y = dataCordenadY + 90;
                        elements2.x = (dataCordenadX - dataWith / 10);
                        elements2.XRotate = 90;
                    }
                    ListRenderElement.Add(elements2);

                    ModelRenderElement elements3 = new ModelRenderElement();
                    elements3.Element = Atk60Element.GetElement("Panel60270");
                    elements3.ElementF = Atk60Element.GetElement("Panel60270F");
                    elements3.CodeName = "27604207";
                    elements3.z = elements3.z + nHeight;
                    elements3.x = dataCordenadX + 240;
                    elements3.y = (dataCordenadY - dataWith / 10);
                    elements3.XRotate = 180;
                    if (type == 2)
                    {
                        elements3.y = dataCordenadY + 180;
                        elements3.x = (dataCordenadX - dataWith / 10);
                        elements3.XRotate = 90;
                    }
                    ListRenderElement.Add(elements3);
                    nHeight = nHeight + 270;
                }
            }





            if (restHeight > 0)
            {
                RestTypeHeight = getRestTypeHeight(restHeight);
                if (RestTypeHeight == 2850)
                {
                    Insert2400Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    LastPanel = 2400;
                    nHeight = nHeight + 240;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }

                if (RestTypeHeight == 2550)
                {
                    Insert1200Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    LastPanel = 1200;
                    nHeight = nHeight + 120;
                    Insert900ElementT(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 900;
                    nHeight = nHeight + 90;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 2400)
                {
                    if (currentDefaultDisign.ExitingPanel2400 == true)
                    {
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert2400Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                        LastPanel = 2400;
                    }
                    else
                    {
                        Insert1200Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                        nHeight = nHeight + 120;
                        var IsRiji = 0;
                        if (Is2700 == true)
                        {
                            IsRiji = 1;
                        }
                        Insert1200Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                        nHeight = nHeight + 120;
                    }
                }
                if (RestTypeHeight == 2250)
                {
                    Insert1200Element(2, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    nHeight = nHeight + 120;
                    LastPanel = 1200;
                    Insert600ElementT(0, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    nHeight = nHeight + 60;
                    LastPanel = 600;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 2100)
                {
                    Insert1200Element(1, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    LastPanel = 1200;
                    nHeight = nHeight + 120;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert900ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 900;
                }
                if (RestTypeHeight == 1950)
                {
                    Insert1200Element(2, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    nHeight = nHeight + 120;
                    LastPanel = 1200;
                    Insert450Element(0, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                    nHeight = nHeight + 45;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert300Element(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 300;
                }
                if (RestTypeHeight == 1800)
                {
                    Insert1200Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    nHeight = nHeight + 120;
                    LastPanel = 1200;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert600ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 600;
                }
                if (RestTypeHeight == 1650)
                {
                    Insert1200Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    nHeight = nHeight + 120;
                    LastPanel = 1200;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 1500)
                {
                    Insert1200Element(1, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    nHeight = nHeight + 120;
                    LastPanel = 1200;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert300Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 300;
                }
                if (RestTypeHeight == 1350)
                {
                    Insert900ElementT(1, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    nHeight = nHeight + 90;
                    LastPanel = 900;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 1200)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert1200Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong, _isEndModule);
                    LastPanel = 1200;
                }
                if (RestTypeHeight == 1050)
                {
                    var IsRiji = 1;
                    if (Is2700 == true)
                    {
                        IsRiji = 2;
                    }
                    Insert600ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    nHeight = nHeight + 60;
                    LastPanel = 600;
                    Insert450Element(0, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 900)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert900ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 900;
                }
                if (RestTypeHeight == 750)
                {
                    Insert450Element(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    nHeight = nHeight + 45;
                    LastPanel = 450;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert300Element(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 300;
                }
                if (RestTypeHeight == 600)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert600ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 600;
                }
                if (RestTypeHeight == 450)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert450Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 300)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert300Element(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 300;
                }
            }
        }
        private static void Insert300Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
                CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            }
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, false);
            var LongPanel = 300;
            var NexHeight = 22;
            if (IsEndModule != true)
            {
                if (LongPanel != 300)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
            }

            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            if (IsRiji == 1)
            {
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2400, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel30240T");
            element.ElementF = Atk60Element.GetElement("Panel30240TF");
            element.CodeName = "24304244";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 240;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel30240T");
            element4.ElementF = Atk60Element.GetElement("Panel30240TF");
            element4.CodeName = "24304244";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 240;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            element4.ZRotate = "90";
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert450Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 41, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 41, false);
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
                CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);

            }
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 55, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 215, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);


            var LongPanel = 450;
            var NexHeight = 38;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 450)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 450)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 45;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2400, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel45240T");
            element.ElementF = Atk60Element.GetElement("Panel45240TF");
            element.CodeName = "24454243";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 240;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel45240T");
            element4.ElementF = Atk60Element.GetElement("Panel45240TF");
            element4.CodeName = "27454206";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 240;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            element4.ZRotate = "90";
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert600ElementT(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 53, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 53, false);


            }


            var LongPanel = 600;
            var NexHeight = 53;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 600)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 600)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 60;
            if (IsRiji == 1 || IsRiji == 2)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(IsRiji, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(IsRiji, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(IsRiji, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(IsRiji, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2400, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel60240T");
            element.ElementF = Atk60Element.GetElement("Panel60240TF");
            element.CodeName = "24604242";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 240;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel60240T");
            element4.ElementF = Atk60Element.GetElement("Panel60240TF");
            element4.CodeName = "24604242";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 240;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            element4.ZRotate = "90";
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert900ElementT(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 83, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 83, false);
            }
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            CommonElement.SedDiwydagD(type, PanelPerfil, 185, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            var LongPanel = 900;
            var NexHeight = 83;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 900)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 900)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 80, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 160, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 90;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2400, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel90240T");
            element.ElementF = Atk60Element.GetElement("Panel90240TF");
            element.CodeName = "24904240";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 240;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel90240T");
            element4.ElementF = Atk60Element.GetElement("Panel90240TF");
            element4.CodeName = "24904240";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 240;
            element4.y = (dataCordenadY - dataWith / 10);
            element4.XRotate = 180;
            element4.ZRotate = "90";
            if (type == 2)
            {
                element4.y = dataCordenadY;
                element4.x = (dataCordenadX - dataWith / 10);
                element4.XRotate = 90;
            }
            ListRenderElement.Add(element4);
        }
        private static void Insert1200Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong, bool _isEndModule)
        {
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 20, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 80, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 20, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 80, ListRenderElement, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 20, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 80, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 20, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 80, ListRenderElement, dataWith / 10, "");

            CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 170, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 170, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);

            if (IsEndModule != true)
            {
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 20, ListRenderElement, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 80, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 20, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 80, ListRenderElement, dataWith / 10, "");
                CommonElement.SedDiwydagD(type, PanelPerfil, 240, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 240, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
            }
            else
            {
                //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 35, false);
                //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 115, false);
            }
            if (nHeight != 0)
            {
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 230, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 120;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            if (IsRiji == 2)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(2, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(2, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(2, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(2, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(), "", 0);
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
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 90, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element2 = new ModelRenderElement();
            element2.Element = Atk60Element.GetElement("Panel90120");
            element2.ElementF = Atk60Element.GetElement("Panel90120F");

            element2.CodeName = "12904215";
            element2.z = element2.z + nHeight;
            element2.x = dataCordenadX + 90;
            element2.y = dataCordenadY;
            element2.XRotate = 0;
            if (type == 2)
            {
                element2.XRotate = 270;
                element2.y = dataCordenadY + 180;
                element2.x = dataCordenadX;
            }
            ListRenderElement.Add(element2);
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 600, type, 180, DimType.Horizontal, type.ToString(), "", 0);

            ModelRenderElement element3 = new ModelRenderElement();
            element3.Element = Atk60Element.GetElement("Panel60120");
            element3.ElementF = Atk60Element.GetElement("Panel60120F");
            element3.CodeName = "12604213";
            element3.z = element3.z + nHeight;
            element3.x = dataCordenadX + 180;
            element3.y = dataCordenadY;
            element3.XRotate = 0;
            if (type == 2)
            {
                element3.XRotate = 270;
                element3.y = dataCordenadY + 240;
                element3.x = dataCordenadX;
            }
            ListRenderElement.Add(element3);
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
            ModelRenderElement element5 = new ModelRenderElement();
            element5.Element = Atk60Element.GetElement("Panel90120");
            element5.ElementF = Atk60Element.GetElement("Panel90120F");
            element5.CodeName = "12904215";
            element5.z = element5.z + nHeight;
            element5.x = dataCordenadX + 180;
            element5.y = (dataCordenadY - dataWith / 10);
            element5.XRotate = 180;
            if (type == 2)
            {
                element5.y = dataCordenadY + 90;
                element5.x = (dataCordenadX - dataWith / 10);
                element5.XRotate = 90;
            }
            ListRenderElement.Add(element5);
            ModelRenderElement element6 = new ModelRenderElement();
            element6.Element = Atk60Element.GetElement("Panel60120");
            element6.ElementF = Atk60Element.GetElement("Panel60120F");
            element6.CodeName = "12604213";
            element6.z = element6.z + nHeight;
            element6.x = dataCordenadX + 240;
            element6.y = (dataCordenadY - dataWith / 10);
            element6.XRotate = 180;
            if (type == 2)
            {
                element6.y = dataCordenadY + 180;
                element6.x = (dataCordenadX - dataWith / 10);
                element6.XRotate = 90;
            }
            ListRenderElement.Add(element6);
        }
        private static void Insert2400Element(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong, bool _isEndModule)
        {
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 40, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 200, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 40, ListRenderElement, "");
            CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 200, ListRenderElement, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 40, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 90, dataCordenadY, nHeight + 200, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 40, ListRenderElement, dataWith / 10, "");
            CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 180, dataCordenadY, nHeight + 200, ListRenderElement, dataWith / 10, "");


            if (HasPreviousModule == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 40, ListRenderElement, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 200, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 40, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 0, dataCordenadY, nHeight + 200, ListRenderElement, dataWith / 10, "");
                if (IsEndModule == true)
                {
                    //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    //CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                }
            }
            else
            {
                if (IsEndModule != true)
                {
                    CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 240, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 240, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 40, ListRenderElement, "");
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + 120, ListRenderElement, "");
                }
                else
                {
                    CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 0, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 90, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 180, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);

                    CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 215, false);
                    CommonElement.SedDiwydagD(type, PanelPerfil, 230, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 55, false);
                }
            }
            if (nHeight != 0)
            {
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 230, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 230, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 240;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 210, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 0, DimType.Horizontal, type.ToString(), "", 0);
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
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 900, type, 90, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element2 = new ModelRenderElement();
            element2.Element = Atk60Element.GetElement("Panel90240");
            element2.ElementF = Atk60Element.GetElement("Panel90240F");
            element2.CodeName = "24904240";
            element2.z = element2.z + nHeight;
            element2.x = dataCordenadX + 90;
            element2.y = dataCordenadY;
            element2.XRotate = 0;
            if (type == 2)
            {
                element2.XRotate = 270;
                element2.y = dataCordenadY + 180;
                element2.x = dataCordenadX;
            }
            ListRenderElement.Add(element2);
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 600, type, 180, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element3 = new ModelRenderElement();
            element3.Element = Atk60Element.GetElement("Panel60240");
            element3.ElementF = Atk60Element.GetElement("Panel60240F");
            element3.CodeName = "24604242";
            element3.z = element3.z + nHeight;
            element3.x = dataCordenadX + 180;
            element3.y = dataCordenadY;
            element3.XRotate = 0;
            if (type == 2)
            {
                element3.XRotate = 270;
                element3.y = dataCordenadY + 240;
                element3.x = dataCordenadX;
            }
            ListRenderElement.Add(element3);
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
            ModelRenderElement element5 = new ModelRenderElement();
            element5.Element = Atk60Element.GetElement("Panel90240");
            element5.ElementF = Atk60Element.GetElement("Panel90240F");
            element5.CodeName = "24904240";
            element5.z = element5.z + nHeight;
            element5.x = dataCordenadX + 180;
            element5.y = (dataCordenadY - dataWith / 10);
            element5.XRotate = 180;
            if (type == 2)
            {
                element5.y = dataCordenadY + 90;
                element5.x = (dataCordenadX - dataWith / 10);
                element5.XRotate = 90;
            }
            ListRenderElement.Add(element5);
            ModelRenderElement element6 = new ModelRenderElement();
            element6.Element = Atk60Element.GetElement("Panel60240");
            element6.ElementF = Atk60Element.GetElement("Panel60240F");
            element6.CodeName = "24604242";
            element6.z = element6.z + nHeight;
            element6.x = dataCordenadX + 240;
            element6.y = (dataCordenadY - dataWith / 10);
            element6.XRotate = 180;
            if (type == 2)
            {
                element6.y = dataCordenadY + 180;
                element6.x = (dataCordenadX - dataWith / 10);
                element6.XRotate = 90;
            }
            ListRenderElement.Add(element6);
        }

        private static int getRestTypeHeight(int restHeight)
        {
            if (restHeight > 300 && restHeight <= 450)
            {
                return 450;
            }
            if (restHeight > 450 && restHeight <= 600)
            {
                return 600;
            }
            if (restHeight > 600 && restHeight <= 750)
            {
                return 750;
            }
            if (restHeight > 750 && restHeight <= 900)
            {
                return 900;
            }
            if (restHeight > 900 && restHeight <= 1050)
            {
                return 1050;
            }
            if (restHeight > 1050 && restHeight <= 1200)
            {
                return 1200;
            }

            if (restHeight > 900 && restHeight <= 1350)
            {
                return 1350;
            }
            if (restHeight > 1350 && restHeight <= 1500)
            {
                return 1500;
            }
            if (restHeight > 1500 && restHeight <= 1650)
            {
                return 1650;
            }

            if (restHeight > 1500 && restHeight <= 1800)
            {
                return 1800;
            }
            if (restHeight > 1800 && restHeight <= 1950)
            {
                return 1950;
            }
            if (restHeight > 1950 && restHeight <= 2100)
            {
                return 2100;
            }

            if (restHeight > 2100 && restHeight <= 2250)
            {
                return 2250;
            }
            if (restHeight > 2250 && restHeight <= 2400)
            {
                return 2400;
            }

            if (restHeight > 2400 && restHeight <= 2550)
            {
                return 2550;
            }
            if (restHeight > 2550 && restHeight <= 2700)
            {
                return 2700;
            }
            if (restHeight > 2700 && restHeight <= 2852)
            {
                return 2850;
            }
            return 300;
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