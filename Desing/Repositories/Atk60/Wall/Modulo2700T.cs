using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class Modulo2700T : BaseController
    {
        private static bool HasPreviousModule = false;
        private static bool IsEndModule = false;
        private static bool IsFirstModule = false;
        private static long LastPanel = 0;
        private static long PanelPerfil = 12;
        private static bool Is2700 = false;
        private static bool _HasConer0 = false;
        private static string _codeName;
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
                if (Tape_0o270.Substring(0, 3) == "Esq")
                {
                    _HasConer0 = true;
                }
            }
            if (_DataSupEnd == 0)
            {
                if (_isEndModule == true)
                {
                    if (type == 1)
                    {
                        dataCordenadX = EndWallX - 270;
                    }
                    if (type == 2)
                    {
                        dataCordenadY = EndWallY - 270;
                    }
                }
            }
            IsEndModule = _isEndModule;
            HasPreviousModule = _HasPreviousModule;
            List<ModelRenderElement> ListRenderElement = new List<ModelRenderElement>();
            InsertProp.SedProp(dataWith, LongLeft, DataHeight, _Type, dataCordenadX, dataCordenadY, ListRenderElement, 59, 61, 150, 151, true);
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
            var DimTypeH = DimType.Horizontal;
            var Elevation = 0;
            var ElevationDiwydag = 0;
            int RestTypeHeight = 300;

            int n = (int)((DataHeight) / 900);

            var restHeight = (int)((DataHeight) - (900 * n));
            if (restHeight > 751)
            {
                n = n + 1;
            }
            var nHeight = 0;
            var IsDywidagT = true;
            for (int i = 0; i < n; i++)
            {
                var IsRiji = 0;
                if (i != n - 1)
                {
                    IsDywidagT = false;
                    IsRiji = 1;
                }
                else
                {
                    IsDywidagT = true;
                    IsRiji = 0;
                }
                if (n >= 1)
                {
                    Insert900ElementT(IsRiji, IsDywidagT, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 900;
                }
                nHeight = nHeight + 90;
            }
            if (restHeight > 0)
            {
                RestTypeHeight = getRestTypeHeight(restHeight);

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
                    Insert450ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
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
                    Insert450ElementT(0, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
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
                    Insert450ElementT(1, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    nHeight = nHeight + 45;
                    LastPanel = 450;
                    var IsRiji = 0;
                    if (Is2700 == true)
                    {
                        IsRiji = 1;
                    }
                    Insert300ElementT(IsRiji, false, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
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
                    Insert450ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 450;
                }
                if (RestTypeHeight == 300)
                {
                    var IsRiji = 0;
                    if (nHeight != 0)
                    {
                        IsRiji = 1;
                    }
                    Insert300ElementT(IsRiji, true, dimTypeVertical, currentDefaultDisign, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, datalong);
                    LastPanel = 300;
                }
            }
        }
        private static void Insert300ElementT(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
                CommonElement.SedDiwydagD(type, PanelPerfil, 215, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            }
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 215, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, false);
            var LongPanel = 300;
            var NexHeight = 22;
            if (IsEndModule != true)
            {
                if (LongPanel != 300)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
            }

            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            if (IsRiji == 1)
            {
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2700, type, 0, DimType.Horizontal, type.ToString(), "", 0);

            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel30270T");
            element.ElementF = Atk60Element.GetElement("Panel30270TF");
            element.CodeName = "27304205";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 270;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel30270T");
            element4.ElementF = Atk60Element.GetElement("Panel30270TF");
            element4.CodeName = "27304205";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 270;
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
        private static void Insert450ElementT(long IsRiji, bool IsDywidagT, DimType dimTypeVertical, DAL.TSql_DefaultDesign currentDefaultDisign, List<ModelRenderElement> ListRenderElement, long dataCordenadX, long dataCordenadY, int nHeight, long dataWith, long type, long datalong)
        {
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 41, false);
            CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 41, false);
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
                CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);

            }
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 55, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 215, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);


            var LongPanel = 450;
            var NexHeight = 38;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 450)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 450)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            //CommonElement.InsertUnionTElement270(IsDywidagT, ListRenderElement, dataCordenadX, dataCordenadY, nHeight, dataWith, type, 22, 300, IsEndModule, HasPreviousModule);
            var SupNHeight = 45;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2700, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel450270T");
            element.ElementF = Atk60Element.GetElement("Panel45270TF");
            element.CodeName = "27454206";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 270;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel450270T");
            element4.ElementF = Atk60Element.GetElement("Panel45270TF");
            element4.CodeName = "27454206";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 270;
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
            CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            if (IsDywidagT == true)
            {
                CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 53, false);
                CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 53, false);


            }
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 55, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);
            //SedDiwydagT27030(PanelPerfil, 0, dataCordenadX + 215, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 26, LastPanel);


            var LongPanel = 600;
            var NexHeight = 53;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 600)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 600)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {

                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 60;
            if (IsRiji == 1 || IsRiji == 2)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(IsRiji, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(IsRiji, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(IsRiji, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(IsRiji, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2700, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel60270T");
            element.ElementF = Atk60Element.GetElement("Panel60270TF");
            element.CodeName = "27604207";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 270;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel60270T");
            element4.ElementF = Atk60Element.GetElement("Panel60270TF");
            element4.CodeName = "27604207";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 270;
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
                CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 83, false);
            }
            CommonElement.SedDiwydagD(type, PanelPerfil, 55, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            CommonElement.SedDiwydagD(type, PanelPerfil, 210, dataCordenadX, dataCordenadY, nHeight, ListRenderElement, "", dataWith, 4, true);
            var LongPanel = 900;
            var NexHeight = 83;
            if (IsEndModule != true && HasPreviousModule == true)
            {
                if (LongPanel != 900)
                {
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                }
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
            }
            else
            {
                if (IsEndModule != true)
                {
                    if (LongPanel != 900)
                    {
                        CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, "");
                        CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 5, ListRenderElement, dataWith / 10, "");
                    }
                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + NexHeight, ListRenderElement, dataWith / 10, "");

                    CommonElement.SedUnionVertical(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, "");
                    CommonElement.SedUnionVerticalMirror(type, PanelPerfil, 0, dataCordenadX, 270, dataCordenadY, nHeight + 15, ListRenderElement, dataWith / 10, "");
                }
            }
            if (nHeight != 0)
            {
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, "90");
                CommonElement.SedUnionHorizontal(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, "90");
                //Mirror
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 20, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 135, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
                CommonElement.SedUnionHorizontalMirror(type, PanelPerfil, 0, dataCordenadX, 250, dataCordenadY, nHeight, ListRenderElement, dataWith / 10, "90");
            }
            var SupNHeight = 90;
            if (IsRiji == 1)
            {
                if (Is2700 == true)
                {
                    SupNHeight = 0;
                }
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRiji(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 30, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
                CommonElement.UnionRijiMirror(1, type, PanelPerfil, 0, dataCordenadX, 240, dataCordenadY, nHeight + SupNHeight, ListRenderElement, dataWith / 10, "");
            }
            CommonElement.AddDimHorizontal(nHeight, ListRenderElement, dataCordenadX, dataCordenadY, 2700, type, 0, DimType.Horizontal, type.ToString(), "", 0);
            ModelRenderElement element = new ModelRenderElement();
            element.Element = Atk60Element.GetElement("Panel900270T");
            element.ElementF = Atk60Element.GetElement("Panel900270TF");
            element.CodeName = "27904209";
            element.z = element.z + nHeight;
            element.x = dataCordenadX;
            element.y = dataCordenadY;
            element.XRotate = 0;
            if (type == 2)
            {
                element.XRotate = 270;
                element.y = dataCordenadY + 270;
            }
            ListRenderElement.Add(element);
            //Mirror
            ModelRenderElement element4 = new ModelRenderElement();
            element4.Element = Atk60Element.GetElement("Panel900270T");
            element4.ElementF = Atk60Element.GetElement("Panel900270TF");
            element4.CodeName = "27904209";
            element4.z = element4.z + nHeight;
            element4.x = dataCordenadX + 270;
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