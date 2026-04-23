using System;

namespace Desing.Repositories.Atk60
{



    public class Atk60Element
    {
        public static string GetID()
        {
            var ticks = Guid.NewGuid().ToString("N");
            return ticks;
        }
        internal static string GetElement(string Element)
        {
            switch (Element)
            {

                //PanelRegulable
                case "PanelReg270":
                    return "../../Content/DesignTools/Stl/ATK60/27104219.stl";
                case "PanelReg270F":
                    return "../../Content/DesignTools/Stl/ATK60/27104219_F.stl";
                case "PanelReg240":
                    return "../../Content/DesignTools/Stl/ATK60/24104224.stl";
                case "PanelReg240F":
                    return "../../Content/DesignTools/Stl/ATK60/24104224_F.stl";
                case "PanelReg120":
                    return "../../Content/DesignTools/Stl/ATK60/12104120.stl";
                case "PanelReg120F":
                    return "../../Content/DesignTools/Stl/ATK60/12104120_F.stl";

                //Angular
                case "PanelExt270":
                    return "../../Content/DesignTools/Stl/ATK60/EEx27000000.stl";
                case "PanelExt240":
                    return "../../Content/DesignTools/Stl/ATK60/EEx24000000.stl";
                case "PanelExt120":
                    return "../../Content/DesignTools/Stl/ATK60/EEx12000000.stl";
                //270 Tubado
                //
                case "Panel900270T":
                    return "../../Content/DesignTools/Stl/ATK60/27904209T.stl";
                case "Panel900270TF":
                    return "../../Content/DesignTools/Stl/ATK60/27904209T_F.stl";
                case "Panel450270T":
                    return "../../Content/DesignTools/Stl/ATK60/27454206T.stl";
                case "Panel45270TF":
                    return "../../Content/DesignTools/Stl/ATK60/27454206T_F.stl";
                case "Panel30270T":
                    return "../../Content/DesignTools/Stl/ATK60/27304205T.stl";
                case "Panel30270TF":
                    return "../../Content/DesignTools/Stl/ATK60/27304205T_F.stl";

                case "Panel60270T":
                    return "../../Content/DesignTools/Stl/ATK60/27604207T.stl";
                case "Panel60270TF":
                    return "../../Content/DesignTools/Stl/ATK60/27604207T_F.stl";
                //120 Tubado
                case "Panel90120T":
                    return "../../Content/DesignTools/Stl/ATK60/12904215T.stl";
                case "Panel90120TF":
                    return "../../Content/DesignTools/Stl/ATK60/12904215T_F.stl";

                case "Panel30120T":
                    return "../../Content/DesignTools/Stl/ATK60/12304211T.stl";
                case "Panel30120TF":
                    return "../../Content/DesignTools/Stl/ATK60/12304211T_F.stl";

                case "Panel60120T":
                    return "../../Content/DesignTools/Stl/ATK60/12604213T.stl";
                case "Panel60120TF":
                    return "../../Content/DesignTools/Stl/ATK60/12604213T_F.stl";

                case "Panel45120T":
                    return "../../Content/DesignTools/Stl/ATK60/12454212T.stl";
                case "Panel45120TF":
                    return "../../Content/DesignTools/Stl/ATK60/12454212T_F.stl";

                //240 Tubado
                case "Panel90240T":
                    return "../../Content/DesignTools/Stl/ATK60/24904240T.stl";
                case "Panel90240TF":
                    return "../../Content/DesignTools/Stl/ATK60/24904240T_F.stl";
                case "Panel60240T":
                    return "../../Content/DesignTools/Stl/ATK60/24604242T.stl";
                case "Panel60240TF":
                    return "../../Content/DesignTools/Stl/ATK60/24604242T_F.stl";
                case "Panel45240T":
                    return "../../Content/DesignTools/Stl/ATK60/24454243T.stl";
                case "Panel45240TF":
                    return "../../Content/DesignTools/Stl/ATK60/24454243T_F.stl";
                case "Panel30240T":
                    return "../../Content/DesignTools/Stl/ATK60/24304244T.stl";
                case "Panel30240TF":
                    return "../../Content/DesignTools/Stl/ATK60/24304244T_F.stl";

                //270
                case "Panel90270":
                    return "../../Content/DesignTools/Stl/ATK60/27904209.stl";
                case "Panel90270F":
                    return "../../Content/DesignTools/Stl/ATK60/27904209_F.stl";
                case "Panel60270":
                    return "../../Content/DesignTools/Stl/ATK60/27604207.stl";
                case "Panel60270F":
                    return "../../Content/DesignTools/Stl/ATK60/27604207_F.stl";
                case "Panel45270":
                    return "../../Content/DesignTools/Stl/ATK60/27454206.stl";
                case "Panel45270F":
                    return "../../Content/DesignTools/Stl/ATK60/27454206_F.stl";
                case "Panel30270":
                    return "../../Content/DesignTools/Stl/ATK60/27304205.stl";
                case "Panel30270F":
                    return "../../Content/DesignTools/Stl/ATK60/27304205_F.stl";



                //240
                case "Panel90240":
                    return "../../Content/DesignTools/Stl/ATK60/24904240.stl";
                case "Panel90240F":
                    return "../../Content/DesignTools/Stl/ATK60/24904240_F.stl";
                case "Panel60240":
                    return "../../Content/DesignTools/Stl/ATK60/24604242.stl";
                case "Panel60240F":
                    return "../../Content/DesignTools/Stl/ATK60/24604242_F.stl";
                case "Panel45240":
                    return "../../Content/DesignTools/Stl/ATK60/24454243.stl";
                case "Panel45240F":
                    return "../../Content/DesignTools/Stl/ATK60/24454243_F.stl";
                case "Panel30240":
                    return "../../Content/DesignTools/Stl/ATK60/24304244.stl";
                case "Panel30240F":
                    return "../../Content/DesignTools/Stl/ATK60/24304244_F.stl";
                //120
                case "Panel90120":
                    return "../../Content/DesignTools/Stl/ATK60/12904215.stl";
                case "Panel90120F":
                    return "../../Content/DesignTools/Stl/ATK60/12904215_F.stl";
                case "Panel60120":
                    return "../../Content/DesignTools/Stl/ATK60/12604213.stl";
                case "Panel60120F":
                    return "../../Content/DesignTools/Stl/ATK60/12604213_F.stl";
                case "Panel45120":
                    return "../../Content/DesignTools/Stl/ATK60/12454212.stl";
                case "Panel45120F":
                    return "../../Content/DesignTools/Stl/ATK60/12454212_F.stl";
                case "Panel30120":
                    return "../../Content/DesignTools/Stl/ATK60/12304211.stl";
                case "Panel30120F":
                    return "../../Content/DesignTools/Stl/ATK60/12304211_F.stl";

                //Esquina
                case "PanelE1200":
                    return "../../Content/DesignTools/Stl/ATK60/E12004216.stl";
                case "PanelE1200F":
                    return "../../Content/DesignTools/Stl/ATK60/E12004216_F.stl";

                case "PanelE2400":
                    return "../../Content/DesignTools/Stl/ATK60/E24004217.stl";
                case "PanelE2400F":
                    return "../../Content/DesignTools/Stl/ATK60/E24004217_F.stl";

                case "PanelE2700":
                    return "../../Content/DesignTools/Stl/ATK60/E27004210.stl";
                case "PanelE2700F":
                    return "../../Content/DesignTools/Stl/ATK60/E27004210_F.stl";
            }
            return "";
        }
        public static string GetUnion(string Element)
        {
            switch (Element)
            {
                case "Dywigdag02":
                    return "../../Content/DesignTools/Stl/ATK60/dywidag02.stl";

                case "GanchoCierre":
                    return "../../Content/DesignTools/Stl/ATK60/1920811.stl";
                case "GanchoRigidizador":
                    return "../../Content/DesignTools/Stl/ATK60/10004225.stl";

                case "TuercaExagonal":
                    return "../../Content/DesignTools/Stl/ATK60/7238001.stl";

                case "H03":
                    return "../../Content/DesignTools/Stl/ATK60/h03.stl";
                case "H03_2":
                    return "../../Content/DesignTools/Stl/ATK60/h03_2.stl";


                case "Puntal270":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal270.stl";
                case "Puntal270_2":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal270_2.stl";
                case "Diwydag":
                    return "../../Content/DesignTools/Stl/ATK60/dywidag.stl";
                case "TuercaFija":
                    return "../../Content/DesignTools/Stl/ATK60/10443020.stl";
                case "Unionvertical_1":
                    return "../../Content/DesignTools/Stl/ATK60/1850162.stl";
                case "Unionvertical_2":
                    return "../../Content/DesignTools/Stl/ATK60/1850163.stl";

                case "UnionHorizonal":
                    return "../../Content/DesignTools/Stl/ATK60/10004220.stl";


                case "UnionHorizonalRegulable_1":
                    return "../../Content/DesignTools/Stl/ATK60/10000221.stl";
                case "UnionHorizonalRegulable_2":
                    return "../../Content/DesignTools/Stl/ATK60/10000221-2.stl";
                case "Fijador":
                    return "../../Content/DesignTools/Stl/ATK60/1850164.stl";
            }
            return "";
        }
        public static string GetDywidag(long WWall)
        {
            WWall = WWall + 100;
            var TypeDywidag = 2500;
            if (WWall <= 2400)
            {
                TypeDywidag = 2400;
            }
            if (WWall <= 2300)
            {
                TypeDywidag = 2300;
            }
            if (WWall <= 2200)
            {
                TypeDywidag = 2200;
            }
            if (WWall <= 2100)
            {
                TypeDywidag = 2100;
            }
            if (WWall <= 2000)
            {
                TypeDywidag = 2000;
            }
            if (WWall <= 1900)
            {
                TypeDywidag = 1900;
            }
            if (WWall <= 1800)
            {
                TypeDywidag = 1800;
            }
            if (WWall <= 1700)
            {
                TypeDywidag = 1700;
            }
            if (WWall <= 1600)
            {
                TypeDywidag = 1600;
            }
            if (WWall <= 1500)
            {
                TypeDywidag = 1500;
            }
            if (WWall <= 1400)
            {
                TypeDywidag = 1400;
            }
            if (WWall <= 1300)
            {
                TypeDywidag = 1300;
            }
            if (WWall <= 1200)
            {
                TypeDywidag = 1200;
            }
            if (WWall <= 1100)
            {
                TypeDywidag = 1100;
            }
            if (WWall <= 1000)
            {
                TypeDywidag = 1000;
            }
            if (WWall <= 900)
            {
                TypeDywidag = 900;
            }
            if (WWall <= 800)
            {
                TypeDywidag = 800;
            }
            if (WWall <= 700)
            {
                TypeDywidag = 700;
            }
            if (WWall <= 600)
            {
                TypeDywidag = 600;
            }
            if (WWall <= 500)
            {
                TypeDywidag = 500;
            }
            switch (TypeDywidag)
            {
                case 2900:
                    return "230290";
                case 2800:
                    return "230280";
                case 2700:
                    return "230270";
                case 2600:
                    return "230260";
                case 2500:
                    return "230250";
                case 2400:
                    return "230240";
                case 2300:
                    return "230230";
                case 2200:
                    return "230220";
                case 2100:
                    return "230210";
                case 2000:
                    return "230200";
                case 1900:
                    return "230190";
                case 1800:
                    return "230180";
                case 1700:
                    return "230170";
                case 1600:
                    return "230160";
                case 1500:
                    return "230150";
                case 1400:
                    return "230140";
                case 1300:
                    return "230130";
                case 1200:
                    return "230120";
                case 1100:
                    return "230110";
                case 1000:
                    return "230100";
                case 900:
                    return "230090";
                case 800:
                    return "230080";
                case 700:
                    return "230070";
                case 600:
                    return "230060";
                case 500:
                    return "230050";
            }
            return "";
        }
        public static string GetPropFirstPart(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "../../Content/DesignTools/Stl/ATK60/h03.stl";
                case "H045":
                    return "../../Content/DesignTools/Stl/ATK60/h045.stl";
                case "H060":
                    return "../../Content/DesignTools/Stl/ATK60/h060.stl";
                case "H075":
                    return "../../Content/DesignTools/Stl/ATK60/h075.stl";
                case "H090":
                    return "../../Content/DesignTools/Stl/ATK60/h090.stl";
                case "H0110":
                    return "../../Content/DesignTools/Stl/ATK60/h110.stl";
                case "H0120":
                    return "../../Content/DesignTools/Stl/ATK60/h120.stl";
                case "H0150":
                    return "../../Content/DesignTools/Stl/ATK60/h150.stl";
                case "H0180":
                    return "../../Content/DesignTools/Stl/ATK60/h180.stl";
                case "H0195":
                    return "../../Content/DesignTools/Stl/ATK60/h195.stl";
                case "H0210":
                    return "../../Content/DesignTools/Stl/ATK60/h210.stl";
                case "H0225":
                    return "../../Content/DesignTools/Stl/ATK60/h225.stl";
                case "H0240":
                    return "../../Content/DesignTools/Stl/ATK60/h240.stl";
                case "H0270":
                    return "../../Content/DesignTools/Stl/ATK60/h270.stl";
                case "H0300":
                    return "../../Content/DesignTools/Stl/ATK60/h300.stl";
                case "H0375":
                    return "../../Content/DesignTools/Stl/ATK60/h375.stl";
                case "HTipo3":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3.stl";
                case "HTipo3-2":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3-2.stl";
                case "HTipo4-41":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3-41.stl";

            }
            return "";
        }
        public static string GetPropSecondPart(string Element)
        {
            switch (Element)
            {
                case "HTipo4-41":
                    return "";
                case "HTipo3-2":
                    return "";
                case "HTipo3":
                    return "";
                case "H03":
                    return "../../Content/DesignTools/Stl/ATK60/h03_2.stl";
                case "H045":
                    return "../../Content/DesignTools/Stl/ATK60/h045_2.stl";
                case "H060":
                    return "../../Content/DesignTools/Stl/ATK60/h060_2.stl";
                case "H075":
                    return "../../Content/DesignTools/Stl/ATK60/h075_2.stl";
                case "H090":
                    return "../../Content/DesignTools/Stl/ATK60/h090_2.stl";
                case "H0110":
                    return "../../Content/DesignTools/Stl/ATK60/h110_2.stl";
                case "H0120":
                    return "../../Content/DesignTools/Stl/ATK60/h120_2.stl";
                case "H0150":
                    return "../../Content/DesignTools/Stl/ATK60/h150_2.stl";
                case "H0180":
                    return "../../Content/DesignTools/Stl/ATK60/h180_2.stl";
                case "H0195":
                    return "../../Content/DesignTools/Stl/ATK60/h195_2.stl";
                case "H0210":
                    return "../../Content/DesignTools/Stl/ATK60/h210_2.stl";
                case "H0225":
                    return "../../Content/DesignTools/Stl/ATK60/h225_2.stl";
                case "H0240":
                    return "../../Content/DesignTools/Stl/ATK60/h240_2.stl";
                case "H0270":
                    return "../../Content/DesignTools/Stl/ATK60/h270_2.stl";
                case "H0300":
                    return "../../Content/DesignTools/Stl/ATK60/h300_2.stl";
                case "H0375":
                    return "../../Content/DesignTools/Stl/ATK60/h375_2.stl";
            }
            return "";
        }
        public static string GetCodePropFirstPart(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "HTipo0";
                case "H045":
                    return "HTipo0";
                case "H060":
                    return "HTipo0";
                case "H075":
                    return "HTipo0";
                case "H090":
                    return "HTipo0";
                case "H0110":
                    return "HTipo0";
                case "H0120":
                    return "HTipo0";
                case "H0150":
                    return "HTipo0";
                case "H0180":
                    return "HTipo1";
                case "H0195":
                    return "HTipo1";
                case "H0210":
                    return "HTipo1";
                case "H0225":
                    return "HTipo1";
                case "H0240":
                    return "HTipo1";
                case "H0270":
                    return "HTipo1";
                case "H0300":
                    return "HTipo1";
                case "H0375":
                    return "HTipo2";
                case "HTipo3":
                    return "HTipo3";
                case "HTipo3-2":
                    return "HTipo3-2";
                case "HTipo4-41":
                    return "HTipo4-41";
            }
            return "";
        }
        public static string GetCodePropSecondPart(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "H0_203";
                case "H045":
                    return "H0_2045";
                case "H060":
                    return "H0_2060";
                case "H075":
                    return "H0_2075";
                case "H090":
                    return "H0_2090";
                case "H0110":
                    return "H0_2110";
                case "H0120":
                    return "H0_2120";
                case "H0150":
                    return "H0_2150";
                case "H0180":
                    return "H0_2180";
                case "H0195":
                    return "H0_2195";
                case "H0210":
                    return "H0_2210";
                case "H0225":
                    return "H0_2225";
                case "H0240":
                    return "H0_2240";
                case "H0270":
                    return "H0_2270";
                case "H0300":
                    return "H0_2300";
                case "H0375":
                    return "H0_2375";
                case "HTipo3":
                    return "";
                case "HTipo3-2":
                    return "";
            }
            return "";
        }


        public static string GetPropFirstPartInf(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "../../Content/DesignTools/Stl/ATK60/h03.stl";
                case "H045":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H060":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H075":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H090":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0110":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0120":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0150":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0180":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0195":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0210":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0225":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0240":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0270":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0300":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf.stl";
                case "H0375":
                    return "../../Content/DesignTools/Stl/ATK60/h375Inf.stl";
                case "HTipo3":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3Inf.stl";
                case "HTipo3-2":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3Inf.stl";
                case "HTipo4-41":
                    return "../../Content/DesignTools/Stl/ATK60/Puntal_Tipo3Inf.stl";

            }
            return "";
        }
        public static string GetPropSecondPartInf(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "";
                case "H045":
                    return "";
                case "H060":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H075":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H090":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0110":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0120":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0150":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0180":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0195":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0210":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0225":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0240":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0270":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0300":
                    return "../../Content/DesignTools/Stl/ATK60/h270Inf_2.stl";
                case "H0375":
                    return "../../Content/DesignTools/Stl/ATK60/h375Inf_2.stl";
                case "HTipo3":
                    return "";
                case "HTipo3-2":
                    return "";
                case "HTipo4-41":
                    return "";

            }
            return "";
        }
        public static string GetCodePropFirstPartInf(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "";
                case "H045":
                    return "";
                case "H060":
                    return "H03";
                case "H075":
                    return "H03";
                case "H090":
                    return "H03";
                case "H0110":
                    return "H03";
                case "H0120":
                    return "H03";
                case "H0150":
                    return "H03";
                case "H0180":
                    return "H0_237";
                case "H0195":
                    return "H0_237";
                case "H0210":
                    return "H0_237";
                case "H0225":
                    return "H0_237";
                case "H0240":
                    return "H0_237";
                case "H0270":
                    return "H0_237";
                case "H0300":
                    return "H0_237";
                case "H0375":
                    return "H0_237";
                case "HTipo3":
                    return "HTipo1";
                case "HTipo3-2":
                    return "HTipo1";
                case "HTipo4-41":
                    return "HTipo1";
            }
            return "";
        }
        public static string GetCodePropSecondPartInf(string Element)
        {
            switch (Element)
            {
                //0,30
                case "H03":
                    return "";
                case "H045":
                    return "";
                case "H060":
                    return "H0_203";
                case "H075":
                    return "H0_203";
                case "H090":
                    return "H0_203";
                case "H0110":
                    return "H0_203";
                case "H0120":
                    return "H0_203";
                case "H0150":
                    return "H0_203";
                case "H0180":
                    return "H0_203";
                case "H0195":
                    return "H0_203";
                case "H0210":
                    return "H0_203";
                case "H0225":
                    return "H0_203";
                case "H0240":
                    return "H0_203";
                case "H0270":
                    return "H0_203";
                case "H0300":
                    return "H0_203";
                case "H0375":
                    return "H0_237";
                case "HTipo3":
                    return "H0_237";
                case "HTipo3-2":
                    return "";
                case "H0_Tipo3":
                    return "";
                case "HTipo4-41":
                    return "";
            }
            return "";
        }
        public static string GetProp(long dataHeight)
        {
            string p = "H03";
            if (dataHeight > 300 && dataHeight <= 455)
            { p = "H045"; }
            if (dataHeight > 455 && dataHeight <= 605)
            { p = "H060"; }
            if (dataHeight > 605 && dataHeight <= 755)
            { p = "H075"; }
            if (dataHeight > 755 && dataHeight <= 905)
            { p = "H090"; }
            if (dataHeight > 905 && dataHeight <= 1105)
            { p = "H0110"; }
            if (dataHeight > 1105 && dataHeight <= 1205)
            { p = "H0120"; }
            if (dataHeight > 1205 && dataHeight <= 1505)
            { p = "H0150"; }
            if (dataHeight > 1505 && dataHeight <= 1805)
            { p = "H0180"; }
            if (dataHeight > 1805 && dataHeight <= 1955)
            { p = "H0195"; }
            if (dataHeight > 1955 && dataHeight <= 2105)
            { p = "H0210"; }
            if (dataHeight > 2105 && dataHeight <= 2255)
            { p = "H0225"; }
            if (dataHeight > 2105 && dataHeight <= 2255)
            { p = "H0225"; }
            if (dataHeight > 2255 && dataHeight <= 2405)
            { p = "H0240"; }
            if (dataHeight > 2405 && dataHeight <= 2555)
            { p = "H0240"; }
            if (dataHeight > 2555 && dataHeight <= 2705)
            { p = "H0270"; }
            if (dataHeight > 2705 && dataHeight <= 3605)
            { p = "H0300"; }
            if (dataHeight > 3605 && dataHeight <= 4955)
            { p = "H0375"; }
            if (dataHeight > 4955 && dataHeight <= 5405)
            { p = "HTipo3"; }
            if (dataHeight > 5405 && dataHeight <= 5705)
            { p = "HTipo3-2"; }
            if (dataHeight > 5705 && dataHeight <= 7350)
            { p = "HTipo4-41"; }
            return p;
        }
    }
}