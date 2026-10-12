using Desing.Repositories.RepositoryCommun;
using System;
using System.Collections.Generic;

namespace Desing.Repositories.Repository_Desing_2.Atk60
{
    /// <summary>
    /// Puntal de aplome. Dos por módulo largo, en las dos caras.
    /// El origen del STL queda en la cara del panel, a ras de suelo:
    /// coordenada del muro, su rotación y 120 mm hacia el exterior.
    /// </summary>
    internal static class PushPullProp
    {
        private const double PanelFaceMm = 120d;
        private const double MinWallHeightMm = 2000d;
        private const double LowerPartFromMm = 1200d;

        internal static void Place(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            double moduleBaseAlongMm,
            double moduleLengthMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            AttributesList attrs)
        {
            if (outElements == null || anchor == null || wallHeightMm < MinWallHeightMm)
            {
                return;
            }

            var upperOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutside : null);
            var upperInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInside : null);
            var lowerOutside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropOutsideInf : null);
            var lowerInside = Atk60FaceInsertion.FlagOn(attrs != null ? attrs._CHeckPropInsideInf : null);
            if (!upperOutside && !upperInside && !lowerOutside && !lowerInside)
            {
                return;
            }

            var stations = Stations(moduleLengthMm);
            if (stations.Count == 0)
            {
                return;
            }

            var parts = PartsForHeight(wallHeightMm);

            if (parts.Count == 0)
            {
                return;
            }

            for (var si = 0; si < stations.Count; si++)
            {
                var alongMm = moduleBaseAlongMm + stations[si];
                if (alongMm < 30d || alongMm > wallLengthMm - 30d)
                {
                    continue;
                }

                for (var pi = 0; pi < parts.Count; pi++)
                {
                    var part = parts[pi];
                    var showOutside = part.Lower ? lowerOutside : upperOutside;
                    var showInside = part.Lower ? lowerInside : upperInside;
                    if (!showOutside && !showInside)
                    {
                        continue;
                    }

                    AddBothFaces(
                        outElements, anchor, idWall,
                        part.Path, part.Code, part.Color,
                        alongMm, yawRad, ux, uz, faceSign,
                        wallHeightMm, wallThicknessMm, wallLengthMm,
                        showOutside, showInside);
                }
            }
        }

        /// <summary>
        /// A 600 mm de cada extremo si el módulo llega a 2 m.
        /// Entre 1,20 m y 2 m, a 300 mm de cada extremo.
        /// </summary>
        private static List<double> Stations(double lengthMm)
        {
            var stations = new List<double>();
            var inset = 0d;
            if (lengthMm >= 2000d)
            {
                inset = 600d;
            }
            else if (lengthMm >= 1200d)
            {
                inset = 300d;
            }

            if (inset < 1d)
            {
                return stations;
            }

            stations.Add(inset);
            var far = lengthMm - inset;
            if (far - inset >= 400d)
            {
                stations.Add(far);
            }

            return stations;
        }

        private struct PropPart
        {
            public string Path;
            public string Code;
            public string Color;
            public bool Lower;
        }

        /// <summary>
        /// Misma tabla que el catálogo anterior. El cuerpo es la pieza exterior
        /// y el tubo es la interior. Por encima de 1,20 m se añade la de abajo.
        /// De 2,26 m a 2,55 m sigue siendo el puntal de 2,40 m. Por encima de 4,95 m
        /// el cuerpo es de una sola pieza, sin tubo interior.
        /// </summary>
        private static List<PropPart> PartsForHeight(double heightMm)
        {
            var key = "H03";
            if (heightMm > 300d && heightMm <= 455d) key = "H045";
            else if (heightMm > 455d && heightMm <= 605d) key = "H060";
            else if (heightMm > 605d && heightMm <= 755d) key = "H075";
            else if (heightMm > 755d && heightMm <= 905d) key = "H090";
            else if (heightMm > 905d && heightMm <= 1105d) key = "H0110";
            else if (heightMm > 1105d && heightMm <= 1205d) key = "H0120";
            else if (heightMm > 1205d && heightMm <= 1505d) key = "H0150";
            else if (heightMm > 1505d && heightMm <= 1805d) key = "H0180";
            else if (heightMm > 1805d && heightMm <= 1955d) key = "H0195";
            else if (heightMm > 1955d && heightMm <= 2105d) key = "H0210";
            else if (heightMm > 2105d && heightMm <= 2255d) key = "H0225";
            else if (heightMm > 2255d && heightMm <= 2555d) key = "H0240";
            else if (heightMm > 2555d && heightMm <= 2705d) key = "H0270";
            else if (heightMm > 2705d && heightMm <= 3605d) key = "H0300";
            else if (heightMm > 3605d && heightMm <= 4955d) key = "H0375";
            else if (heightMm > 4955d && heightMm <= 5405d) key = "HTipo3";
            else if (heightMm > 5405d && heightMm <= 5705d) key = "HTipo3-2";
            else if (heightMm > 5705d && heightMm <= 7350d) key = "HTipo4-41";

            string bodyFile;
            string bodyCode;
            string innerFile;
            string innerCode;
            string lowerFile;
            string lowerCode;
            string lowerInnerFile;
            string lowerInnerCode;
            switch (key)
            {
                case "H045":
                    bodyFile = "h045.stl"; bodyCode = "HTipo0"; innerFile = "h045_2.stl"; innerCode = "H0_2045";
                    lowerFile = "h270Inf.stl"; lowerCode = ""; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "H060":
                    bodyFile = "h060.stl"; bodyCode = "HTipo0"; innerFile = "h060_2.stl"; innerCode = "H0_2060";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H075":
                    bodyFile = "h075.stl"; bodyCode = "HTipo0"; innerFile = "h075_2.stl"; innerCode = "H0_2075";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H090":
                    bodyFile = "h090.stl"; bodyCode = "HTipo0"; innerFile = "h090_2.stl"; innerCode = "H0_2090";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0110":
                    bodyFile = "h110.stl"; bodyCode = "HTipo0"; innerFile = "h110_2.stl"; innerCode = "H0_2110";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0120":
                    bodyFile = "h120.stl"; bodyCode = "HTipo0"; innerFile = "h120_2.stl"; innerCode = "H0_2120";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0150":
                    bodyFile = "h150.stl"; bodyCode = "HTipo0"; innerFile = "h150_2.stl"; innerCode = "H0_2150";
                    lowerFile = "h270Inf.stl"; lowerCode = "H03"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0180":
                    bodyFile = "h180.stl"; bodyCode = "HTipo1"; innerFile = "h180_2.stl"; innerCode = "H0_2180";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0195":
                    bodyFile = "h195.stl"; bodyCode = "HTipo1"; innerFile = "h195_2.stl"; innerCode = "H0_2195";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0210":
                    bodyFile = "h210.stl"; bodyCode = "HTipo1"; innerFile = "h210_2.stl"; innerCode = "H0_2210";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0225":
                    bodyFile = "h225.stl"; bodyCode = "HTipo1"; innerFile = "h225_2.stl"; innerCode = "H0_2225";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0240":
                    bodyFile = "h240.stl"; bodyCode = "HTipo1"; innerFile = "h240_2.stl"; innerCode = "H0_2240";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0270":
                    bodyFile = "h270.stl"; bodyCode = "HTipo1"; innerFile = "h270_2.stl"; innerCode = "H0_2270";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0300":
                    bodyFile = "h300.stl"; bodyCode = "HTipo1"; innerFile = "h300_2.stl"; innerCode = "H0_2300";
                    lowerFile = "h270Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h270Inf_2.stl"; lowerInnerCode = "H0_203";
                    break;
                case "H0375":
                    bodyFile = "h375.stl"; bodyCode = "HTipo2"; innerFile = "h375_2.stl"; innerCode = "H0_2375";
                    lowerFile = "h375Inf.stl"; lowerCode = "H0_237"; lowerInnerFile = "h375Inf_2.stl"; lowerInnerCode = "H0_237";
                    break;
                case "HTipo3":
                    bodyFile = "Puntal_Tipo3.stl"; bodyCode = "HTipo3"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "HTipo3-2":
                    bodyFile = "Puntal_Tipo3-2.stl"; bodyCode = "HTipo3-2"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                case "HTipo4-41":
                    bodyFile = "Puntal_Tipo3-41.stl"; bodyCode = "HTipo4-41"; innerFile = ""; innerCode = "";
                    lowerFile = "Puntal_Tipo3Inf.stl"; lowerCode = "HTipo1"; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
                default:
                    bodyFile = "h03.stl"; bodyCode = "HTipo0"; innerFile = "h03_2.stl"; innerCode = "H0_203";
                    lowerFile = ""; lowerCode = ""; lowerInnerFile = ""; lowerInnerCode = "";
                    break;
            }

            var parts = new List<PropPart>();
            AddPart(parts, bodyFile, bodyCode, "prop", false);
            AddPart(parts, innerFile, innerCode, "prop-inner", false);
            if (heightMm > LowerPartFromMm)
            {
                AddPart(parts, lowerFile, lowerCode, "prop", true);
                AddPart(parts, lowerInnerFile, lowerInnerCode, "prop-inner", true);
            }

            return parts;
        }

        private static void AddPart(List<PropPart> parts, string file, string code, string color, bool lower)
        {
            if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            parts.Add(new PropPart
            {
                Path = "../../Content/DesignTools/Stl/ATK60/" + file,
                Code = code,
                Color = color,
                Lower = lower,
            });
        }

        private static void AddBothFaces(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double alongMm,
            double yawRad,
            double ux,
            double uz,
            double faceSign,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm,
            bool showOutside,
            bool showInside)
        {
            var jointX = anchor.X + (ux * alongMm);
            var jointZ = anchor.Z + (uz * alongMm);
            if (showOutside)
            {
                AddFace(
                    outElements, anchor, idWall, path, code, color,
                    jointX + (anchor.NormalX * PanelFaceMm),
                    anchor.Y,
                    jointZ + (anchor.NormalZ * PanelFaceMm),
                    -yawRad + (faceSign < 0d ? Math.PI : 0d),
                    anchor.NormalX,
                    anchor.NormalZ,
                    faceSign,
                    alongMm,
                    wallHeightMm,
                    wallThicknessMm,
                    wallLengthMm);
            }

            if (!showInside)
            {
                return;
            }

            var oppositeX = jointX - (anchor.NormalX * wallThicknessMm);
            var oppositeZ = jointZ - (anchor.NormalZ * wallThicknessMm);
            AddFace(
                outElements, anchor, idWall, path, code, color,
                oppositeX - (anchor.NormalX * PanelFaceMm),
                anchor.Y,
                oppositeZ - (anchor.NormalZ * PanelFaceMm),
                -yawRad + (faceSign < 0d ? Math.PI : 0d) + Math.PI,
                -anchor.NormalX,
                -anchor.NormalZ,
                -faceSign,
                alongMm,
                wallHeightMm,
                wallThicknessMm,
                wallLengthMm);
        }

        private static void AddFace(
            List<Atk60ElementPaintItem> outElements,
            Atk60WallPaintAnchor anchor,
            string idWall,
            string path,
            string code,
            string color,
            double x,
            double y,
            double z,
            double rotY,
            double normalX,
            double normalZ,
            double faceSign,
            double alongMm,
            double wallHeightMm,
            double wallThicknessMm,
            double wallLengthMm)
        {
            outElements.Add(new Atk60ElementPaintItem
            {
                IdWall = !string.IsNullOrWhiteSpace(idWall) ? idWall : anchor.IdWall,
                ElementType = "Union",
                ElementCode = code,
                Orientation = "Vertical",
                ImportPath = path,
                Color = color,
                X = x,
                Y = y,
                Z = z,
                RotX = 0,
                RotY = rotY,
                RotZ = 0,
                NormalX = normalX,
                NormalZ = normalZ,
                FaceSign = faceSign,
                IsMirrored = false,
                WallHeightMm = wallHeightMm,
                WallThicknessMm = wallThicknessMm,
                WallLengthMm = wallLengthMm,
                LocalAlongMm = alongMm,
                LocalUpMm = 0d,
                UseStrictPose = true,
            });
        }
    }
}
