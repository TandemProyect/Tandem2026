using DAL;
using Desing.Controllers;
using System.Collections.Generic;

namespace Desing.Repositories.Atk60.Wall
{
    public class CommonElement : BaseController
    {
        internal static long PanelPerfil = 12;


        internal static void SedUnionHorizontal90(long _cordenadX, long _cordenadY, long nHeight, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = nHeight;
            elementu.ZRotate = "90M";
            listRenderElement.Add(elementu);
        }
        internal static void SedUnionHorizontal180(long _cordenadX, long _cordenadY, long nHeight, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = nHeight;
            elementu.ZRotate = "180M";
            listRenderElement.Add(elementu);
        }
        internal static void SedUnionHorizontal0(long _cordenadX, long _cordenadY, long nHeight, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = nHeight;
            elementu.ZRotate = "0M";
            listRenderElement.Add(elementu);
        }
        internal static void SedUnionHorizontal270(long _cordenadX, long _cordenadY, long nHeight, List<ModelRenderElement> listRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = nHeight;
            elementu.ZRotate = "270M";
            listRenderElement.Add(elementu);
        }
        internal static void SedUnionHorizontalTape(int _type, long _cordenadX, long _cordenadY, long nHeight, List<ModelRenderElement> listRenderElement, long dataWith)
        {
            var _ZRotate = "";
            if (_type == 0)
            {
                _ZRotate = "0M";
            }

            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = nHeight;
            elementu.ZRotate = _ZRotate;
            listRenderElement.Add(elementu);
        }

        internal static void SedUnionRigiHorizontal_0_Solape(long DataHeight, long Remate, long endWallX, TSql_DefaultDesign currentDefaultDisign, long nHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            if (DataHeight / 10 < nHeight)
            {
                return;
            }
            ModelRenderElement elementRigi = new ModelRenderElement();
            elementRigi.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_1");
            elementRigi.CodeName = "1850162";
            elementRigi.LongWood = dataWith;
            elementRigi.ParametFilter = Remate;
            elementRigi.Filter = "SEMA03";
            elementRigi.x = endWallX - 2;
            elementRigi.y = dataCordenadY;
            elementRigi.z = nHeight;
            elementRigi.heightWood = nHeight;
            elementRigi.XRotate = 0;
            ListRenderElement.Add(elementRigi);

            //Lado 270


            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2.CodeName = "1850164";
            elementu2.Filter = "SEMA03";
            elementu2.x = (endWallX - 5) + Remate;
            elementu2.y = dataCordenadY + 5;
            elementu2.z = nHeight;
            elementu2.XRotate = 2;
            elementu2.ZRotate = "0";
            ListRenderElement.Add(elementu2);

            ModelRenderElement elementT = new ModelRenderElement();
            elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT.CodeName = "10443020-2";
            elementT.Filter = "SExS01_Placa";
            elementT.x = (endWallX + 6) + Remate;
            elementT.y = dataCordenadY + 5;
            elementT.z = nHeight;
            elementT.XRotate = 1;
            elementT.ZRotate = "0";
            ListRenderElement.Add(elementT);

            //Lado 90
            ModelRenderElement elementu2270 = new ModelRenderElement();
            elementu2270.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2270.CodeName = "1850164";
            elementu2270.Filter = "SEMA03";
            elementu2270.x = (endWallX - 5) + Remate;
            elementu2270.y = dataCordenadY - ((dataWith / 10) - 5);
            elementu2270.z = nHeight;
            elementu2270.XRotate = 2;
            elementu2270.ZRotate = "0";
            ListRenderElement.Add(elementu2270);

            ModelRenderElement elementT90 = new ModelRenderElement();
            elementT90.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT90.CodeName = "10443020-2";
            elementT90.Filter = "SExS01_Placa";
            elementT90.x = (endWallX + 6) + Remate;
            elementT90.y = dataCordenadY - ((dataWith / 10) - 5);
            elementT90.z = nHeight;
            elementT90.XRotate = 1;
            elementT90.ZRotate = "0";
            ListRenderElement.Add(elementT90);
        }
        internal static void SedUnionRigiHorizontal_0(long DataHeight, long Remate, long endWallX, TSql_DefaultDesign currentDefaultDisign, long nHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            if (DataHeight / 10 < nHeight)
            {
                return;
            }
            ModelRenderElement elementRigi = new ModelRenderElement();
            elementRigi.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_1");
            elementRigi.CodeName = "1850162";
            elementRigi.LongWood = dataWith;
            elementRigi.ParametFilter = Remate;
            elementRigi.Filter = "SExS01";
            elementRigi.x = endWallX - 2;
            elementRigi.y = dataCordenadY - ((dataWith / 10) / 2);
            elementRigi.z = nHeight;
            elementRigi.heightWood = nHeight;
            elementRigi.XRotate = 0;
            ListRenderElement.Add(elementRigi);

            //Lado 270


            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2.CodeName = "1850164";
            elementu2.Filter = "SExS01";
            elementu2.x = (endWallX - 5) + Remate;
            elementu2.y = dataCordenadY + 5;
            elementu2.z = nHeight;
            elementu2.XRotate = 0;
            elementu2.ZRotate = "0";
            ListRenderElement.Add(elementu2);

            ModelRenderElement elementT = new ModelRenderElement();
            elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT.CodeName = "10443020-2";
            elementT.Filter = "SExS01_Placa";
            elementT.x = (endWallX + 6) + Remate;
            elementT.y = dataCordenadY + 5;
            elementT.z = nHeight;
            elementT.XRotate = 1;
            elementT.ZRotate = "0";
            ListRenderElement.Add(elementT);

            //Lado 90
            ModelRenderElement elementu2270 = new ModelRenderElement();
            elementu2270.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2270.CodeName = "1850164";
            elementu2270.Filter = "SExS01";
            elementu2270.x = (endWallX - 5) + Remate;
            elementu2270.y = dataCordenadY - ((dataWith / 10) + 5);
            elementu2270.z = nHeight;
            elementu2270.XRotate = 0;
            elementu2270.ZRotate = "0";
            ListRenderElement.Add(elementu2270);

            ModelRenderElement elementT90 = new ModelRenderElement();
            elementT90.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT90.CodeName = "10443020-2";
            elementT90.Filter = "SExS01_Placa";
            elementT90.x = (endWallX + 6) + Remate;
            elementT90.y = dataCordenadY - ((dataWith / 10) + 5);
            elementT90.z = nHeight;
            elementT90.XRotate = 1;
            elementT90.ZRotate = "0";
            ListRenderElement.Add(elementT90);
        }

        internal static void SedUnionHorizontal_0_01(long DataHeight, long Remate, long endWallX, TSql_DefaultDesign currentDefaultDisign, long nHeight, long datalong, long dataCordenadX, long dataCordenadY, long dataWith, List<ModelRenderElement> ListRenderElement)
        {
            if (DataHeight / 10 < nHeight)
            {
                return;
            }
            //Lado 270
            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2.CodeName = "1850164";
            elementu2.Filter = "SExS01";
            elementu2.x = dataCordenadX - 2;
            elementu2.y = dataCordenadY + 5;
            elementu2.z = nHeight;
            elementu2.XRotate = 0;
            elementu2.ZRotate = "0";
            ListRenderElement.Add(elementu2);

            ModelRenderElement elementT = new ModelRenderElement();
            elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT.CodeName = "10443020-2";
            elementT.Filter = "SExS01_Placa";
            elementT.x = dataCordenadX + 10;
            elementT.y = dataCordenadY + 5;
            elementT.z = nHeight;
            elementT.XRotate = 1;
            elementT.ZRotate = "0";
            ListRenderElement.Add(elementT);

            //Lado 90
            ModelRenderElement elementu2270 = new ModelRenderElement();
            elementu2270.ElementUnion1 = Atk60Element.GetUnion("Fijador");
            elementu2270.CodeName = "1850164";
            elementu2270.Filter = "SExS01";
            elementu2270.x = dataCordenadX - 2;
            elementu2270.y = dataCordenadY - ((dataWith / 10) + 5);
            elementu2270.z = nHeight;
            elementu2270.XRotate = 0;
            elementu2270.ZRotate = "0";
            ListRenderElement.Add(elementu2270);

            ModelRenderElement elementT90 = new ModelRenderElement();
            elementT90.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
            elementT90.CodeName = "10443020-2";
            elementT90.Filter = "SExS01_Placa";
            elementT90.x = dataCordenadX + 10;
            elementT90.y = dataCordenadY - ((dataWith / 10) + 5);
            elementT90.z = nHeight;
            elementT90.XRotate = 1;
            elementT90.ZRotate = "0";
            ListRenderElement.Add(elementT90);
        }

        internal static void SedUnionVerticalRegulable180(long _h, long _remate, long dataCordenadX, long dataCordenadY, List<ModelRenderElement> ListRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_1");
            elementu.CodeName = "10000221";
            elementu.x = dataCordenadX + 5;
            elementu.y = dataCordenadY - 8;
            elementu.z = _h;
            elementu.heightWood = _h;
            elementu.XRotate = 180;
            ListRenderElement.Add(elementu);

            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_2");
            elementu2.CodeName = "10000221B";
            elementu2.x = dataCordenadX + 5;
            elementu2.y = dataCordenadY - 8;
            elementu2.z = _h;
            elementu2.heightWood = _h;
            elementu2.XRotate = 180;
            ListRenderElement.Add(elementu2);
        }

        internal static void SedUnionVerticalRegulable90(long _h, long _remate, long dataCordenadX, long dataCordenadY, List<ModelRenderElement> ListRenderElement)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_1");
            elementu.CodeName = "10000221";
            elementu.x = dataCordenadX - 14;
            elementu.y = dataCordenadY - 5;
            elementu.z = _h;
            elementu.heightWood = _h;
            elementu.XRotate = 90;
            ListRenderElement.Add(elementu);

            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_2");
            elementu2.CodeName = "10000221B";
            elementu2.x = dataCordenadX - 14;
            elementu2.y = dataCordenadY - _remate;
            elementu2.z = _h;
            elementu2.heightWood = _h;
            elementu2.XRotate = 90;
            ListRenderElement.Add(elementu2);
        }

        internal static void SedUnionVerticalRegulable0(long _h, long _remate, long dataCordenadX, long dataCordenadY, List<ModelRenderElement> ListRenderElement, long hTotest)
        {
            if (_h * 10 > hTotest)
            {
                return;
            }
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_1");
            elementu.CodeName = "10000221";
            elementu.x = dataCordenadX + 12;
            elementu.y = dataCordenadY + _remate;
            elementu.z = _h;
            elementu.heightWood = _h;
            elementu.XRotate = 0;
            ListRenderElement.Add(elementu);

            ModelRenderElement elementu2 = new ModelRenderElement();
            elementu2.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonalRegulable_2");
            elementu2.CodeName = "10000221B";
            elementu2.x = dataCordenadX + 12;
            elementu2.y = dataCordenadY + 6;
            elementu2.z = _h;
            elementu2.heightWood = _h;
            elementu2.XRotate = 0;
            ListRenderElement.Add(elementu2);
        }

        internal static void UnionRijiMirror(long _typeLong, long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {
            var _cordenadYPlaca = 0;
            var _cordenadXPlaca = 0;
            var _cordenadYFijador = 0;
            var _cordenadXFijador = 0;
            var XRotate = 180;
            if (_type == 1)
            {
                _cordenadX = _cordenadX + _addModulo + Suplement;
                _cordenadY = _cordenadY - (_dataWith + _PanelPerfil);
                _cordenadYPlaca = (int)(_cordenadY - (_PanelPerfil - 3));
                _cordenadXPlaca = (int)(_cordenadX + (_PanelPerfil - 6));
                _cordenadYFijador = (int)(_cordenadY + (_PanelPerfil - 6));
                _cordenadXFijador = (int)(_cordenadX);
                XRotate = 90;
            }
            if (_type == 2)
            {
                _cordenadX = _cordenadX + (_PanelPerfil + 8);
                _cordenadY = (long)(_cordenadY + _addModulo + Suplement + 4);
                _cordenadYPlaca = (int)(_cordenadY);
                _cordenadXPlaca = (int)(_cordenadX + (_PanelPerfil + 6));
                _cordenadYFijador = (int)(_cordenadY);
                _cordenadXFijador = (int)(_cordenadX - (_PanelPerfil + 6));
                XRotate = 0;
            }

            if (_typeLong == 1)
            {
                ModelRenderElement elementu = new ModelRenderElement();
                elementu.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_1");
                elementu.CodeName = "1850162";
                elementu.x = _cordenadX + _addModulo;
                elementu.y = _cordenadY;
                elementu.z = _cordenadZ;
                elementu.XRotate = XRotate;
                elementu.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu);

                ModelRenderElement elementu2 = new ModelRenderElement();
                elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu2.CodeName = "1850164";
                elementu2.x = _cordenadXFijador;
                elementu2.y = _cordenadYFijador;
                elementu2.XRotate = XRotate;
                elementu2.z = _cordenadZ - 20;
                elementu2.ZRotate = _ZRotate;
                elementu2.Filter = "Tape";
                _listRenderElement.Add(elementu2);

                ModelRenderElement elementT = new ModelRenderElement();
                elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT.CodeName = "10443020-2";
                elementT.x = _cordenadX;
                elementT.y = _cordenadYPlaca;
                elementT.z = _cordenadZ - 20;
                elementT.XRotate = XRotate + 1;
                elementT.ZRotate = _ZRotate;
                elementT.Filter = "Tape";
                _listRenderElement.Add(elementT);

                ModelRenderElement elementu3 = new ModelRenderElement();
                elementu3.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu3.CodeName = "1850164";
                elementu3.x = _cordenadXFijador;
                elementu3.y = _cordenadYFijador;
                elementu3.z = _cordenadZ + 20;
                elementu3.XRotate = XRotate;
                elementu3.ZRotate = _ZRotate;
                elementu3.Filter = "Tape";
                _listRenderElement.Add(elementu3);

                ModelRenderElement elementT2 = new ModelRenderElement();
                elementT2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT2.CodeName = "10443020-2";
                elementT2.x = _cordenadX;
                elementT2.y = _cordenadYPlaca;
                elementT2.z = _cordenadZ + 20;
                elementT2.XRotate = XRotate + 1;
                elementT2.ZRotate = _ZRotate;
                elementT2.Filter = "Tape";
                _listRenderElement.Add(elementT2);

            }
            if (_typeLong == 2)
            {
                ModelRenderElement elementu = new ModelRenderElement();
                elementu.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_2");
                elementu.CodeName = "1850163";
                elementu.x = _cordenadX + _addModulo;
                elementu.y = _cordenadY;
                elementu.z = _cordenadZ;
                elementu.XRotate = XRotate;
                elementu.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu);

                ModelRenderElement elementu2 = new ModelRenderElement();
                elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu2.CodeName = "1850164";
                elementu2.x = _cordenadXFijador;
                elementu2.y = _cordenadYFijador;
                elementu2.XRotate = XRotate;
                elementu2.z = _cordenadZ - 20;
                elementu2.ZRotate = _ZRotate;
                elementu2.Filter = "Tape";
                _listRenderElement.Add(elementu2);

                ModelRenderElement elementT = new ModelRenderElement();
                elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT.CodeName = "10443020-2";
                elementT.x = _cordenadX;
                elementT.y = _cordenadYPlaca;
                elementT.z = _cordenadZ - 20;
                elementT.XRotate = XRotate + 1;
                elementT.ZRotate = _ZRotate;
                elementT.Filter = "Tape";
                _listRenderElement.Add(elementT);

                ModelRenderElement elementu3 = new ModelRenderElement();
                elementu3.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu3.CodeName = "1850164";
                elementu3.x = _cordenadXFijador;
                elementu3.y = _cordenadYFijador;
                elementu3.z = _cordenadZ + 20;
                elementu3.XRotate = XRotate;
                elementu3.ZRotate = _ZRotate;
                elementu3.Filter = "Tape";
                _listRenderElement.Add(elementu3);

                ModelRenderElement elementT2 = new ModelRenderElement();
                elementT2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT2.CodeName = "10443020-2";
                elementT2.x = _cordenadX;
                elementT2.y = _cordenadYPlaca;
                elementT2.z = _cordenadZ + 20;
                elementT2.XRotate = XRotate + 1;
                elementT2.ZRotate = _ZRotate;
                elementT2.Filter = "Tape";
                _listRenderElement.Add(elementT2);
            }
        }
        internal static void UnionRiji(long _typeLong, long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {
            var XRotate = 0;
            var _cordenadYPlaca = 0;
            var _cordenadXPlaca = 0;
            var _cordenadYFijador = 0;
            var _cordenadXFijador = 0;
            if (_type == 1)
            {
                _cordenadX = _cordenadX + Suplement + _addModulo;
                _cordenadY = (long)(_cordenadY + _PanelPerfil - 1);
                _cordenadYPlaca = (int)(_cordenadY + (_PanelPerfil - 4));
                _cordenadXPlaca = (int)(_cordenadX);
                _cordenadYFijador = (int)(_cordenadY - (_PanelPerfil - 8));
                _cordenadXFijador = (int)(_cordenadX);
                XRotate = 270;
            }
            if (_type == 2)
            {
                _cordenadX = _cordenadX - (_dataWith + _PanelPerfil + 8);
                _cordenadY = (long)(_cordenadY + _addModulo + Suplement + 4);
                _cordenadYPlaca = (int)(_cordenadY);
                _cordenadXPlaca = (int)(_cordenadX + 2);
                _cordenadYFijador = (int)(_cordenadY);
                _cordenadXFijador = (int)(_cordenadX + (_PanelPerfil + 6));
                XRotate = 180;
            }
            if (_typeLong == 1)
            {
                ModelRenderElement elementu = new ModelRenderElement();
                elementu.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_1");
                elementu.CodeName = "1850162";
                elementu.x = _cordenadX;
                elementu.y = _cordenadY;
                elementu.z = _cordenadZ;
                elementu.XRotate = XRotate;
                elementu.ZRotate = "0";
                _listRenderElement.Add(elementu);

                ModelRenderElement elementu2 = new ModelRenderElement();
                elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu2.CodeName = "1850164";
                elementu2.x = _cordenadXFijador;
                elementu2.y = _cordenadYFijador;
                elementu2.z = _cordenadZ - 20;
                elementu2.XRotate = XRotate;
                elementu2.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu2);
                ModelRenderElement elementT = new ModelRenderElement();
                elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT.CodeName = "10443020-2";
                elementT.x = _cordenadXPlaca;
                elementT.y = _cordenadYPlaca;
                elementT.z = _cordenadZ - 20;
                elementT.XRotate = XRotate + 1;
                elementT.ZRotate = _ZRotate;
                _listRenderElement.Add(elementT);

                ModelRenderElement elementu3 = new ModelRenderElement();
                elementu3.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu3.CodeName = "1850164";
                elementu3.x = _cordenadXFijador;
                elementu3.y = _cordenadYFijador;
                elementu3.z = _cordenadZ + 20;
                elementu3.XRotate = XRotate;
                elementu3.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu3);

                ModelRenderElement elementT2 = new ModelRenderElement();
                elementT2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT2.CodeName = "10443020-2";
                elementT2.x = _cordenadXPlaca;
                elementT2.y = _cordenadYPlaca;
                elementT2.z = _cordenadZ + 20;
                elementT2.XRotate = XRotate + 1;
                elementT2.ZRotate = _ZRotate;
                _listRenderElement.Add(elementT2);
            }
            if (_typeLong == 2)
            {
                ModelRenderElement elementu = new ModelRenderElement();
                elementu.ElementUnion1 = Atk60Element.GetUnion("Unionvertical_2");
                elementu.CodeName = "1850163";
                elementu.x = _cordenadX;
                elementu.y = _cordenadY;
                elementu.z = _cordenadZ + 10;
                elementu.XRotate = XRotate;
                elementu.ZRotate = "0";
                _listRenderElement.Add(elementu);

                ModelRenderElement elementu2 = new ModelRenderElement();
                elementu2.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu2.CodeName = "1850164";
                elementu2.x = _cordenadXFijador;
                elementu2.y = _cordenadYFijador;
                elementu2.z = _cordenadZ - 30;
                elementu2.XRotate = XRotate;
                elementu2.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu2);

                ModelRenderElement elementT = new ModelRenderElement();
                elementT.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT.CodeName = "10443020-2";
                elementT.x = _cordenadXPlaca;
                elementT.y = _cordenadYPlaca;
                elementT.z = _cordenadZ - 30;
                elementT.XRotate = XRotate + 1;
                elementT.ZRotate = _ZRotate;
                _listRenderElement.Add(elementT);

                ModelRenderElement elementu3 = new ModelRenderElement();
                elementu3.ElementUnion1 = Atk60Element.GetUnion("Fijador");
                elementu3.CodeName = "1850164";
                elementu3.x = _cordenadXFijador;
                elementu3.y = _cordenadYFijador;
                elementu3.z = _cordenadZ + 30;
                elementu3.XRotate = XRotate;
                elementu3.ZRotate = _ZRotate;
                _listRenderElement.Add(elementu3);
                ModelRenderElement elementT2 = new ModelRenderElement();
                elementT2.ElementUnion1 = Atk60Element.GetUnion("TuercaFija");
                elementT2.CodeName = "10443020-2";
                elementT2.x = _cordenadXPlaca;
                elementT2.y = _cordenadYPlaca;
                elementT2.z = _cordenadZ + 30;
                elementT2.XRotate = XRotate + 1;
                elementT2.ZRotate = _ZRotate;
                _listRenderElement.Add(elementT2);
            }
        }

        internal static void UnionRijiHorizontal_0(long _typeLong, long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {

        }


        internal static void SedDiwydagD(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, string _ZRotate, long _dataWith, long _elevation, bool IsPanelHorizontal)
        {
            var placa1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            var codePlaca = "10443020";

            if (_cordenadZ == 0 && IsPanelHorizontal == true)
            {
                placa1 = "../../Content/DesignTools/Stl/ATK60/1920894.stl";
                codePlaca = "1920894";
                ModelRenderElement TuercaExagonal = new ModelRenderElement();
                TuercaExagonal.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/7238001.stl";
                TuercaExagonal.CodeName = "7238001";
                TuercaExagonal.x = _cordenadX + 4 + _addModulo;
                TuercaExagonal.y = _cordenadY + 13;
                TuercaExagonal.z = _cordenadZ + 4;
                TuercaExagonal.XRotate = 0;
                if (_type == 2)
                {
                    TuercaExagonal.XRotate = 90;
                    TuercaExagonal.x = _cordenadX + 13;
                    TuercaExagonal.y = _cordenadY + 4 + _addModulo;
                }
                _listRenderElement.Add(TuercaExagonal);
                ModelRenderElement TuercaExagonalMirror = new ModelRenderElement();
                TuercaExagonalMirror.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/7238001.stl";
                TuercaExagonalMirror.CodeName = "7238001";
                TuercaExagonalMirror.x = _cordenadX + 4 + _addModulo;
                TuercaExagonalMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
                TuercaExagonalMirror.z = _cordenadZ + _elevation;
                TuercaExagonalMirror.XRotate = 180;
                TuercaExagonalMirror.ZRotate = _ZRotate;
                if (_type == 2)
                {
                    TuercaExagonalMirror.XRotate = 270;
                    TuercaExagonalMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
                    TuercaExagonalMirror.y = _cordenadY + 4 + _addModulo;
                }
                _listRenderElement.Add(TuercaExagonalMirror);
            }

            ModelRenderElement elementPlacaSecontLevel = new ModelRenderElement();
            elementPlacaSecontLevel.ElementUnion1 = placa1;
            elementPlacaSecontLevel.CodeName = codePlaca;
            elementPlacaSecontLevel.x = _cordenadX + 4 + _addModulo;
            elementPlacaSecontLevel.y = _cordenadY + 13;
            elementPlacaSecontLevel.z = _cordenadZ + _elevation;
            elementPlacaSecontLevel.XRotate = 0;
            if (_type == 2)
            {
                elementPlacaSecontLevel.XRotate = 90;
                elementPlacaSecontLevel.x = _cordenadX + 13;
                elementPlacaSecontLevel.y = _cordenadY + 4 + _addModulo;
            }
            _listRenderElement.Add(elementPlacaSecontLevel);


            ModelRenderElement elementTSecontLevelMirror = new ModelRenderElement();
            elementTSecontLevelMirror.ElementUnion1 = placa1;
            elementTSecontLevelMirror.CodeName = codePlaca;
            elementTSecontLevelMirror.x = _cordenadX + 4 + _addModulo;
            elementTSecontLevelMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
            elementTSecontLevelMirror.z = _cordenadZ + _elevation;
            elementTSecontLevelMirror.XRotate = 180;
            elementTSecontLevelMirror.ZRotate = _ZRotate;
            if (_type == 2)
            {
                elementTSecontLevelMirror.XRotate = 270;
                elementTSecontLevelMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
                elementTSecontLevelMirror.y = _cordenadY + 4 + _addModulo;
            }
            _listRenderElement.Add(elementTSecontLevelMirror);


            var LongDywidag = _dataWith + 240 + 150;
            ModelRenderElement elementTdwidag = new ModelRenderElement();
            elementTdwidag.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
            elementTdwidag.CodeName = Atk60Element.GetDywidag(LongDywidag);
            elementTdwidag.x = _cordenadX + 4 + _addModulo;
            elementTdwidag.y = _cordenadY - ((_dataWith / 10) / 2);
            elementTdwidag.z = _cordenadZ + _elevation;
            elementTdwidag.XRotate = 0;
            if (_type == 2)
            {
                elementTdwidag.XRotate = 90;
                elementTdwidag.x = _cordenadX - ((_dataWith / 10) / 2);
                elementTdwidag.y = _cordenadY + 4 + _addModulo;
            }
            _listRenderElement.Add(elementTdwidag);

        }
        internal static void SedDiwydag(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, string _ZRotate, long _dataWith)
        {
            ModelRenderElement elementTdwidagFistLeven = new ModelRenderElement();
            elementTdwidagFistLeven.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
            elementTdwidagFistLeven.CodeName = Atk60Element.GetDywidag(_dataWith + 240 + 150);
            elementTdwidagFistLeven.x = _cordenadX + 4;
            elementTdwidagFistLeven.y = _cordenadY - ((_dataWith / 10) / 2);
            elementTdwidagFistLeven.z = _cordenadZ + 215;
            elementTdwidagFistLeven.XRotate = 0;
            if (_type == 2)
            {
                elementTdwidagFistLeven.XRotate = 90;
                elementTdwidagFistLeven.x = _cordenadX - ((_dataWith / 10) / 2);
                elementTdwidagFistLeven.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementTdwidagFistLeven);
            ModelRenderElement elementTdwidagSecontLeven = new ModelRenderElement();
            elementTdwidagSecontLeven.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
            elementTdwidagSecontLeven.CodeName = Atk60Element.GetDywidag(_dataWith + 240 + 150);
            elementTdwidagSecontLeven.x = _cordenadX + 4;
            elementTdwidagSecontLeven.y = _cordenadY - ((_dataWith / 10) / 2);
            elementTdwidagSecontLeven.z = _cordenadZ + 55;
            elementTdwidagSecontLeven.XRotate = 0;
            if (_type == 2)
            {
                elementTdwidagSecontLeven.XRotate = 90;
                elementTdwidagSecontLeven.x = _cordenadX - ((_dataWith / 10) / 2);
                elementTdwidagSecontLeven.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementTdwidagSecontLeven);

            ModelRenderElement elementPlacaFistLevel = new ModelRenderElement();
            elementPlacaFistLevel.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            elementPlacaFistLevel.CodeName = "10443020";
            elementPlacaFistLevel.x = _cordenadX + 4;
            elementPlacaFistLevel.y = _cordenadY + 13;
            elementPlacaFistLevel.z = _cordenadZ + 55;
            elementPlacaFistLevel.XRotate = 0;
            if (_type == 2)
            {
                elementPlacaFistLevel.XRotate = 90;
                elementPlacaFistLevel.x = _cordenadX + 13;
                elementPlacaFistLevel.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementPlacaFistLevel);

            ModelRenderElement elementPlacaSecontLevel = new ModelRenderElement();
            elementPlacaSecontLevel.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            elementPlacaSecontLevel.CodeName = "10443020";
            elementPlacaSecontLevel.x = _cordenadX + 4;
            elementPlacaSecontLevel.y = _cordenadY + 13;
            elementPlacaSecontLevel.z = _cordenadZ + 215;
            elementPlacaSecontLevel.XRotate = 0;
            if (_type == 2)
            {
                elementPlacaSecontLevel.XRotate = 90;
                elementPlacaSecontLevel.x = _cordenadX + 13;
                elementPlacaSecontLevel.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementPlacaSecontLevel);
            //Mirror
            ModelRenderElement elementTFistLevelMirror = new ModelRenderElement();
            elementTFistLevelMirror.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            elementTFistLevelMirror.CodeName = "10443020";
            elementTFistLevelMirror.x = _cordenadX + 4 + _addModulo;
            elementTFistLevelMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
            elementTFistLevelMirror.z = _cordenadZ + 55;
            elementTFistLevelMirror.XRotate = 180;
            elementTFistLevelMirror.ZRotate = _ZRotate;
            if (_type == 2)
            {
                elementTFistLevelMirror.XRotate = 270;
                elementTFistLevelMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
                elementTFistLevelMirror.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementTFistLevelMirror);

            ModelRenderElement elementTSecontLevelMirror = new ModelRenderElement();
            elementTSecontLevelMirror.ElementUnion1 = "../../Content/DesignTools/Stl/ATK60/10443020.stl";
            elementTSecontLevelMirror.CodeName = "10443020";
            elementTSecontLevelMirror.x = _cordenadX + 4 + _addModulo;
            elementTSecontLevelMirror.y = _cordenadY - _PanelPerfil - (_dataWith / 10) - 1;
            elementTSecontLevelMirror.z = _cordenadZ + 215;
            elementTSecontLevelMirror.XRotate = 180;
            elementTSecontLevelMirror.ZRotate = _ZRotate;
            if (_type == 2)
            {
                elementTSecontLevelMirror.XRotate = 270;
                elementTSecontLevelMirror.x = _cordenadX - _PanelPerfil - (_dataWith / 10) - 1;
                elementTSecontLevelMirror.y = _cordenadY + 4;
            }
            _listRenderElement.Add(elementTSecontLevelMirror);

        }
        internal static void SedUnionHorizontal(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, string _ZRotate)
        {
            if (_type == 1 || _type == 4)
            {
                _cordenadX = _cordenadX + Suplement + _addModulo;
                _cordenadY = _cordenadY + _PanelPerfil;
                _ZRotate = "270M";
            }
            if (_type == 2)
            {
                _cordenadY = _cordenadY + Suplement + _addModulo;
                _cordenadX = _cordenadX + _PanelPerfil;
                _ZRotate = "0M";
            }
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = _cordenadZ;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void SedUnionHorizontalMirror(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {

            if (_type == 1 || _type == 4)
            {
                _cordenadX = _cordenadX + Suplement + +_addModulo;
                _cordenadY = _cordenadY - _PanelPerfil - _dataWith;
                _ZRotate = "90M";
            }
            if (_type == 2)
            {
                _cordenadY = _cordenadY + Suplement + _addModulo;
                _cordenadX = _cordenadX - _PanelPerfil - _dataWith;
                _ZRotate = "180M";
            }

            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = _cordenadZ;
            elementu.XRotate = 180;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void SedUnionVertical(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, string _ZRotate)
        {
            if (_type == 0) { _ZRotate = "0"; }
            if (_type == 270) { _ZRotate = "270"; }
            if (_type == 90) { _ZRotate = "90"; }
            if (_type == 180) { _ZRotate = "180"; }
            if (_type == 1801) { _ZRotate = "180S"; }
            if (_type == 10) { _ZRotate = "180S"; }

            if (_type == 1 || _type == 4)
            {
                _cordenadX = _cordenadX + Suplement + _addModulo;
                _cordenadY = _cordenadY + _PanelPerfil;
                _ZRotate = "270";
            }
            if (_type == 2)
            {
                _cordenadY = _cordenadY + Suplement + _addModulo;
                _cordenadX = _cordenadX + _PanelPerfil;
                _ZRotate = "0";
            }
            if (_type == 3)
            {
                _cordenadY = _cordenadY + Suplement + _addModulo;
                _cordenadX = _cordenadX + _PanelPerfil;
                _ZRotate = "0Coodinate";
            }
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = _cordenadZ;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void SedUnionVerticalMirror(long _type, long _PanelPerfil, long _addModulo, long _cordenadX, long Suplement, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {
            if (_type == 1 || _type == 4)
            {
                _cordenadX = _cordenadX + Suplement + +_addModulo;
                _cordenadY = _cordenadY - _PanelPerfil - _dataWith;
                _ZRotate = "90";
            }
            if (_type == 2)
            {
                _cordenadY = _cordenadY + Suplement + _addModulo;
                _cordenadX = _cordenadX - _PanelPerfil - _dataWith;
                _ZRotate = "180";
            }

            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX;
            elementu.y = _cordenadY;
            elementu.z = _cordenadZ;
            elementu.XRotate = 180;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void SedUnionVerticalMirrorEsqDirec90(long _PanelPerfil, long _addModulo, long _cordenadX, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX + _addModulo;
            elementu.y = _cordenadY - (_PanelPerfil - 3);
            elementu.z = _cordenadZ;
            elementu.XRotate = 90;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void SedUnionVerticalMirrorEsqDirec180(long _PanelPerfil, long _addModulo, long _cordenadX, long _cordenadY, long _cordenadZ, List<ModelRenderElement> _listRenderElement, long _dataWith, string _ZRotate)
        {
            ModelRenderElement elementu = new ModelRenderElement();
            elementu.ElementUnion1 = Atk60Element.GetUnion("UnionHorizonal");
            elementu.CodeName = "10004220";
            elementu.x = _cordenadX + _addModulo;
            elementu.y = _cordenadY - (_PanelPerfil - 3);
            elementu.z = _cordenadZ;
            elementu.XRotate = 180;
            elementu.ZRotate = _ZRotate;
            _listRenderElement.Add(elementu);
        }
        internal static void AddDimHorizontal(long nHeight,

            List<ModelRenderElement> listRenderElement,
            long dataCordenadX,
            long dataCordenadY,
            int DimText,
            long type,
            long typeLong,
            DimType TypeDim,
            string typeMesh,
            string Filter,
            long? LongWood
            )
        {
            if (nHeight != 0) { return; }

            ModelRenderElement DimElemente = new ModelRenderElement();
            DimElemente.Filter = Filter;
            DimElemente.LongWood = LongWood;
            if (TypeDim == DimType.Horizontal)
            {
                DimElemente.LongDimTypeHorizontal = DimText;
                DimElemente.CodeName = "Dim_Horizontal";
            }
            else
            {
                DimElemente.LongDimTypeHorizontalT = DimText;
                DimElemente.CodeName = "Dim_Horizontal_T";

            }
            DimElemente.Type = type.ToString();
            DimElemente.x = dataCordenadX;
            DimElemente.y = dataCordenadY;
            if (type == 1)
            {
                DimElemente.x = dataCordenadX + typeLong;
            }
            else
            {
                DimElemente.y = dataCordenadY + (typeLong + DimText / 10);
            }
            listRenderElement.Add(DimElemente);
        }

        internal static void AddDimVertical(long nHeight,
        List<ModelRenderElement> listRenderElement,
        long dataCordenadX,
        long dataCordenadY,
        long dataCordenadZ,
        int DimText,
        long type,
        long typeLong,
        DimType TypeDim,
        string typeMesh,
        string Filter,
        long? LongWood
    )
        {
            if (nHeight != 0) { return; }

            ModelRenderElement DimElemente = new ModelRenderElement();
            DimElemente.Filter = TypeDim.ToString();
            DimElemente.LongWood = LongWood;
            if (TypeDim == DimType.Vertical50)
            {
                DimElemente.LongDimTypeVertical = DimText;
                DimElemente.CodeName = "Dim_Vertical";
            }

            DimElemente.Type = type.ToString();
            DimElemente.x = dataCordenadX;
            DimElemente.y = dataCordenadY;
            DimElemente.z = dataCordenadZ;
            if (TypeDim == DimType.Vertical50)
            {
                DimElemente.y = dataCordenadY + typeLong;
            }
            listRenderElement.Add(DimElemente);
        }

        internal static int GetSwitchDatawih(long dataWith)
        {
            int value = 0;
            if (dataWith > 99) { value = 100; }
            if (dataWith > 149) { value = 150; }
            if (dataWith > 199) { value = 200; }
            if (dataWith > 249) { value = 250; }
            if (dataWith > 299) { value = 300; }
            if (dataWith > 349) { value = 350; }
            if (dataWith > 399) { value = 350; }
            if (dataWith > 449) { value = 350; }
            if (dataWith > 550) { value = 350; }
            return value;
        }
    }
}