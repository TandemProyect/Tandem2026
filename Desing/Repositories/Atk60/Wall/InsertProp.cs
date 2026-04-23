using Desing.Controllers;
using System.Collections.Generic;
namespace Desing.Repositories.Atk60.Wall
{
    public class InsertProp : BaseController
    {
        public static void SedProp(long dataWith, long LongLeft, long DataHeight, long _Type, long dataCordenadX, long dataCordenadY, List<ModelRenderElement> ListRenderElement, long v1, long v2, long v3, long v4, bool IsSecontProp)
        {
            if (DataHeight < 1999)
            {
                return;
            }
            string ElementProp = Atk60Element.GetProp(DataHeight);
            if (_Type == 1)
            {
                var id = Atk60Element.GetID();
                ModelRenderElement Puntal = new ModelRenderElement();
                Puntal.IdElement = id;
                Puntal.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                Puntal.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                Puntal.z = 0;
                Puntal.x = dataCordenadX + v1;
                Puntal.y = dataCordenadY;
                Puntal.XRotate = 270;
                Puntal.Filter = "Puntal270";
                ListRenderElement.Add(Puntal);

                ModelRenderElement Puntal2 = new ModelRenderElement();
                Puntal2.IdElement = id;
                Puntal2.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                Puntal2.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                Puntal2.z = 0;
                Puntal2.x = dataCordenadX + v1;
                Puntal2.y = dataCordenadY;
                Puntal2.XRotate = 270;
                Puntal2.Filter = "Puntal270";
                ListRenderElement.Add(Puntal2);
                if (DataHeight > 1201)
                {
                    //Inferior
                    var idInf = Atk60Element.GetID();
                    ModelRenderElement PuntalInf = new ModelRenderElement();
                    PuntalInf.IdElement = idInf;
                    PuntalInf.ElementUnion1 = Atk60Element.GetPropFirstPartInf(ElementProp);
                    PuntalInf.CodeName = Atk60Element.GetCodePropFirstPartInf(ElementProp);
                    PuntalInf.z = 0;
                    PuntalInf.x = dataCordenadX + v1;
                    PuntalInf.y = dataCordenadY;
                    PuntalInf.XRotate = 270;
                    PuntalInf.Filter = "PuntalInf270";
                    ListRenderElement.Add(PuntalInf);
                    ModelRenderElement PuntalInf2 = new ModelRenderElement();
                    PuntalInf2.IdElement = idInf;
                    PuntalInf2.ElementUnion1 = Atk60Element.GetPropSecondPartInf(ElementProp);
                    PuntalInf2.CodeName = Atk60Element.GetCodePropSecondPartInf(ElementProp);
                    PuntalInf2.z = 0;
                    PuntalInf2.x = dataCordenadX + v1;
                    PuntalInf2.y = dataCordenadY;
                    PuntalInf2.XRotate = 270;
                    PuntalInf2.Filter = "PuntalInf270";
                    ListRenderElement.Add(PuntalInf2);
                }

                if (IsSecontProp == true)
                {

                    ////90
                    var id2 = Atk60Element.GetID();
                    ModelRenderElement Puntal180 = new ModelRenderElement();
                    Puntal180.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                    Puntal180.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                    Puntal180.IdElement = id2;
                    Puntal180.z = 0;
                    Puntal180.x = dataCordenadX + v2;
                    Puntal180.y = dataCordenadY - (dataWith / 10);
                    Puntal180.XRotate = 90;
                    Puntal180.Filter = "Puntal90";
                    ListRenderElement.Add(Puntal180);
                    ModelRenderElement Puntal2180 = new ModelRenderElement();
                    Puntal2180.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                    Puntal2180.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                    Puntal2180.IdElement = id2;
                    Puntal2180.z = 0;
                    Puntal2180.x = dataCordenadX + v2;
                    Puntal2180.y = dataCordenadY - (dataWith / 10);
                    Puntal2180.XRotate = 90;
                    Puntal2180.Filter = "Puntal90";
                    ListRenderElement.Add(Puntal2180);

                    var id180Inf = Atk60Element.GetID();
                    ModelRenderElement Puntal180Inf = new ModelRenderElement();
                    Puntal180Inf.IdElement = id180Inf;
                    Puntal180Inf.ElementUnion1 = Atk60Element.GetPropFirstPartInf(ElementProp);
                    Puntal180Inf.CodeName = Atk60Element.GetCodePropFirstPartInf(ElementProp);
                    Puntal180Inf.z = 0;
                    Puntal180Inf.x = dataCordenadX + v2;
                    Puntal180Inf.y = dataCordenadY - (dataWith / 10);
                    Puntal180Inf.XRotate = 90;
                    Puntal180Inf.Filter = "PuntalInf90";
                    ListRenderElement.Add(Puntal180Inf);

                    ModelRenderElement Puntal180Inf2 = new ModelRenderElement();
                    Puntal180Inf2.IdElement = id180Inf;
                    Puntal180Inf2.ElementUnion1 = Atk60Element.GetPropSecondPartInf(ElementProp);
                    Puntal180Inf2.CodeName = Atk60Element.GetCodePropSecondPartInf(ElementProp);
                    Puntal180Inf2.z = 0;
                    Puntal180Inf2.x = dataCordenadX + v2;
                    Puntal180Inf2.y = dataCordenadY - (dataWith / 10);
                    Puntal180Inf2.XRotate = 90;
                    Puntal180Inf2.Filter = "PuntalInf90";
                    ListRenderElement.Add(Puntal180Inf2);
                }
                if (IsSecontProp == true)
                {
                    //Secont Puntal
                    var id3 = Atk60Element.GetID();
                    ModelRenderElement Puntal22 = new ModelRenderElement();
                    Puntal22.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                    Puntal22.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                    Puntal22.IdElement = id3;
                    Puntal22.z = 0;
                    Puntal22.x = dataCordenadX + v1 + v3;
                    Puntal22.y = dataCordenadY;
                    Puntal22.XRotate = 270;
                    Puntal22.Filter = "Puntal270";
                    ListRenderElement.Add(Puntal22);

                    ModelRenderElement Puntal222 = new ModelRenderElement();
                    Puntal222.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                    Puntal222.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                    Puntal222.IdElement = id3;
                    Puntal222.z = 0;
                    Puntal222.x = dataCordenadX + v1 + v3;
                    Puntal222.y = dataCordenadY;
                    Puntal222.XRotate = 270;
                    Puntal222.Filter = "Puntal270";
                    ListRenderElement.Add(Puntal222);

                    //Inf
                    if (DataHeight > 1201)
                    {
                        var id23Inf = Atk60Element.GetID();
                        ModelRenderElement Puntal22Inf = new ModelRenderElement();
                        Puntal22Inf.ElementUnion1 = Atk60Element.GetPropFirstPartInf(ElementProp);
                        Puntal22Inf.CodeName = Atk60Element.GetCodePropFirstPartInf(ElementProp);
                        Puntal22Inf.IdElement = id23Inf;
                        Puntal22Inf.z = 0;
                        Puntal22Inf.x = dataCordenadX + v1 + v3;
                        Puntal22Inf.y = dataCordenadY;
                        Puntal22Inf.XRotate = 270;
                        Puntal22Inf.Filter = "PuntalInf270";
                        ListRenderElement.Add(Puntal22Inf);

                        ModelRenderElement Puntal222Inf = new ModelRenderElement();
                        Puntal222Inf.ElementUnion1 = Atk60Element.GetPropSecondPartInf(ElementProp);
                        Puntal222Inf.CodeName = Atk60Element.GetCodePropSecondPartInf(ElementProp);
                        Puntal222Inf.IdElement = id23Inf;
                        Puntal222Inf.z = 0;
                        Puntal222Inf.x = dataCordenadX + v1 + v3;
                        Puntal222Inf.y = dataCordenadY;
                        Puntal222Inf.XRotate = 270;
                        Puntal222Inf.Filter = "PuntalInf270";
                        ListRenderElement.Add(Puntal222Inf);
                    }
                    ////90
                    var id4 = Atk60Element.GetID();
                    ModelRenderElement Puntal1802 = new ModelRenderElement();
                    Puntal1802.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                    Puntal1802.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                    Puntal1802.IdElement = id4;
                    Puntal1802.z = 0;
                    Puntal1802.x = dataCordenadX + v2 + v3;
                    Puntal1802.y = dataCordenadY - (dataWith / 10);
                    Puntal1802.XRotate = 90;
                    Puntal1802.Filter = "Puntal90";
                    ListRenderElement.Add(Puntal1802);

                    ModelRenderElement Puntal21802 = new ModelRenderElement();
                    Puntal21802.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                    Puntal21802.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                    Puntal21802.IdElement = id4;
                    Puntal21802.z = 0;
                    Puntal21802.x = dataCordenadX + v2 + v3;
                    Puntal21802.y = dataCordenadY - (dataWith / 10);
                    Puntal21802.XRotate = 90;
                    Puntal21802.Filter = "Puntal90";
                    ListRenderElement.Add(Puntal21802);

                    //Inf
                    if (DataHeight > 1201)
                    {
                        var id4Inf = Atk60Element.GetID();
                        ModelRenderElement Puntal1802Inf = new ModelRenderElement();
                        Puntal1802Inf.ElementUnion1 = Atk60Element.GetPropFirstPartInf(ElementProp);
                        Puntal1802Inf.CodeName = Atk60Element.GetCodePropFirstPartInf(ElementProp);
                        Puntal1802Inf.IdElement = id4Inf;
                        Puntal1802Inf.z = 0;
                        Puntal1802Inf.x = dataCordenadX + v2 + v3;
                        Puntal1802Inf.y = dataCordenadY - (dataWith / 10);
                        Puntal1802Inf.XRotate = 90;
                        Puntal1802Inf.Filter = "PuntalInf90";
                        ListRenderElement.Add(Puntal1802Inf);

                        ModelRenderElement Puntal21802Inf = new ModelRenderElement();
                        Puntal21802Inf.ElementUnion1 = Atk60Element.GetPropSecondPartInf(ElementProp);
                        Puntal21802Inf.CodeName = Atk60Element.GetCodePropSecondPartInf(ElementProp);
                        Puntal21802Inf.IdElement = id4Inf;
                        Puntal21802Inf.z = 0;
                        Puntal21802Inf.x = dataCordenadX + v2 + v3;
                        Puntal21802Inf.y = dataCordenadY - (dataWith / 10);
                        Puntal21802Inf.XRotate = 90;
                        Puntal21802Inf.Filter = "PuntalInf90";
                        ListRenderElement.Add(Puntal21802Inf);
                    }
                }
            }
            if (_Type == 2)
            {
                var id5 = Atk60Element.GetID();
                ModelRenderElement Puntal = new ModelRenderElement();
                Puntal.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                Puntal.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                Puntal.IdElement = id5;
                Puntal.z = 0;
                Puntal.x = dataCordenadX - (dataWith / 10);
                Puntal.y = dataCordenadY + v1;
                Puntal.XRotate = 180;
                Puntal.Filter = "Puntal0";
                ListRenderElement.Add(Puntal);

                ModelRenderElement Puntal2 = new ModelRenderElement();
                Puntal2.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                Puntal2.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                Puntal2.IdElement = id5;
                Puntal2.z = 0;
                Puntal2.x = dataCordenadX - (dataWith / 10);
                Puntal2.y = dataCordenadY + v1;
                Puntal2.XRotate = 180;
                Puntal2.Filter = "Puntal0";
                ListRenderElement.Add(Puntal2);

                if (DataHeight > 1201)
                {
                    var idInf2_2 = Atk60Element.GetID();
                    ModelRenderElement PuntalInf_2 = new ModelRenderElement();
                    PuntalInf_2.IdElement = idInf2_2;
                    PuntalInf_2.ElementUnion1 = Atk60Element.GetPropFirstPartInf(ElementProp);
                    PuntalInf_2.CodeName = Atk60Element.GetCodePropFirstPartInf(ElementProp);
                    PuntalInf_2.IdElement = idInf2_2;
                    PuntalInf_2.z = 0;
                    PuntalInf_2.x = dataCordenadX - (dataWith / 10);
                    PuntalInf_2.y = dataCordenadY + v1;
                    PuntalInf_2.XRotate = 180;
                    PuntalInf_2.Filter = "PuntalInf180";
                    ListRenderElement.Add(PuntalInf_2);

                    ModelRenderElement PuntalInf2_2 = new ModelRenderElement();
                    PuntalInf2_2.IdElement = idInf2_2;
                    PuntalInf2_2.ElementUnion1 = Atk60Element.GetPropSecondPartInf(ElementProp);
                    PuntalInf2_2.CodeName = Atk60Element.GetCodePropSecondPartInf(ElementProp);
                    PuntalInf2_2.IdElement = idInf2_2;
                    PuntalInf2_2.z = 0;
                    PuntalInf2_2.x = dataCordenadX - (dataWith / 10);
                    PuntalInf2_2.y = dataCordenadY + v1;
                    PuntalInf2_2.XRotate = 180;
                    PuntalInf2_2.Filter = "PuntalInf180";
                    ListRenderElement.Add(PuntalInf2_2);
                }
                ////180
                if (IsSecontProp == true)
                {
                    var id7 = Atk60Element.GetID();
                    ModelRenderElement Puntal180 = new ModelRenderElement();
                    Puntal180.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                    Puntal180.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                    Puntal180.IdElement = id7;
                    Puntal180.x = dataCordenadX;
                    Puntal180.y = dataCordenadY + v2;
                    Puntal180.XRotate = 0;
                    Puntal180.Filter = "Puntal180";
                    ListRenderElement.Add(Puntal180);

                    ModelRenderElement Puntal2180 = new ModelRenderElement();
                    Puntal2180.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                    Puntal2180.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                    Puntal2180.IdElement = id7;
                    Puntal2180.z = 0;
                    Puntal2180.x = dataCordenadX;
                    Puntal2180.y = dataCordenadY + v2;
                    Puntal2180.XRotate = 0;
                    Puntal2180.Filter = "Puntal180";
                    ListRenderElement.Add(Puntal2180);

                    var id8 = Atk60Element.GetID();
                    ModelRenderElement Puntal1802 = new ModelRenderElement();
                    Puntal1802.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                    Puntal1802.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                    Puntal1802.IdElement = id8;
                    Puntal1802.x = dataCordenadX;
                    Puntal1802.y = dataCordenadY + v2 + v4;
                    Puntal1802.XRotate = 0;
                    Puntal1802.Filter = "Puntal180";
                    ListRenderElement.Add(Puntal1802);

                    ModelRenderElement Puntal21802 = new ModelRenderElement();
                    Puntal21802.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                    Puntal21802.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                    Puntal21802.IdElement = id8;
                    Puntal21802.z = 0;
                    Puntal21802.x = dataCordenadX;
                    Puntal21802.y = dataCordenadY + v2 + v4;
                    Puntal21802.XRotate = 0;
                    Puntal21802.Filter = "Puntal180";
                    ListRenderElement.Add(Puntal21802);
                    if (DataHeight > 1201)
                    {
                        var id6 = Atk60Element.GetID();
                        ModelRenderElement Puntal22 = new ModelRenderElement();
                        Puntal22.ElementUnion1 = Atk60Element.GetPropFirstPart(ElementProp);
                        Puntal22.CodeName = Atk60Element.GetCodePropFirstPart(ElementProp);
                        Puntal22.IdElement = id6;
                        Puntal22.z = 0;
                        Puntal22.x = dataCordenadX - (dataWith / 10);
                        Puntal22.y = dataCordenadY + v1 + v4;
                        Puntal22.XRotate = 180;
                        Puntal22.Filter = "PuntalInf0";
                        ListRenderElement.Add(Puntal22);
                        ModelRenderElement Puntal222 = new ModelRenderElement();
                        Puntal222.ElementUnion1 = Atk60Element.GetPropSecondPart(ElementProp);
                        Puntal222.CodeName = Atk60Element.GetCodePropSecondPart(ElementProp);
                        Puntal222.IdElement = id6;
                        Puntal222.z = 0;
                        Puntal222.x = dataCordenadX - (dataWith / 10);
                        Puntal222.y = dataCordenadY + v1 + v4;
                        Puntal222.XRotate = 180;
                        Puntal222.Filter = "PuntalInf0";
                        ListRenderElement.Add(Puntal222);
                    }
                }
            }
        }
    }
}
