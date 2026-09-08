using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    internal static class Modulo270HeightPanelCatalog
    {
        private const string BaseGlbPath = "/Content/DesignTools/Atk-60NewGeneration/GLB/";

        internal static Modulo270Layout Resolve(double wallHeightMm)
        {
            return ResolveForModule(wallHeightMm, 2700);
        }

        internal static Modulo270Layout ResolveForModule(double wallHeightMm, int moduleLengthMm)
        {
            if (moduleLengthMm < 300)
            {
                moduleLengthMm = 2700;
            }

            var target = NormalizeTargetHeightMm(wallHeightMm);

            // Modulos estrechos (0,30..0,90): solo paneles verticales.
            // Ejemplo H=3,00 y modulo 0,45: 2,70x0,45 + 1,20x0,45. Sin tumbados.
            if (IsNarrowLengthModule(moduleLengthMm))
            {
                return BuildNarrowModuleVerticalStack((int)target, moduleLengthMm);
            }

            Modulo270Layout layout;
            if (!TryGetKnownLayout(target, out layout) || layout == null)
            {
                layout = target > 2700d
                    ? BuildMixedVerticalPlusTumbadoLayout(target)
                    : BuildGreedyTumbadoLayout(target);
            }

            return RemapLayoutToModuleLength(layout, moduleLengthMm);
        }

        private static bool IsNarrowLengthModule(int moduleLengthMm)
        {
            return moduleLengthMm == 300
                || moduleLengthMm == 450
                || moduleLengthMm == 600
                || moduleLengthMm == 750
                || moduleLengthMm == 900;
        }

        private static Modulo270Layout BuildNarrowModuleVerticalStack(int targetHeightMm, int widthMm)
        {
            var pieces = new List<Modulo270PieceLayout>();
            var remaining = targetHeightMm;
            var up = 0;

            while (remaining >= 299)
            {
                int familyH;
                if (remaining > 2400)
                {
                    familyH = 2700;
                }
                else if (remaining > 1200)
                {
                    familyH = 2400;
                }
                else
                {
                    familyH = 1200;
                }

                var glb = ResolveVerticalGlbCode(familyH, widthMm);
                pieces.Add(PieceVertical(glb, widthMm, familyH, 0, up));
                remaining -= familyH;
                up += familyH;
            }

            if (pieces.Count == 0)
            {
                var glb = ResolveVerticalGlbCode(1200, widthMm);
                pieces.Add(PieceVertical(glb, widthMm, 1200, 0, 0));
            }

            return Build(targetHeightMm, pieces.ToArray());
        }

        private static double NormalizeTargetHeightMm(double wallHeightMm)
        {
            var h = Math.Max(300d, Math.Min(6000d, wallHeightMm));
            return Math.Ceiling(h / 150d) * 150d;
        }

        private static bool TryGetKnownLayout(double targetHeightMm, out Modulo270Layout layout)
        {
            var p030 = PieceTumbado("27304205", 300, 2700, 0);
            var p045 = PieceTumbado("27454206", 450, 2700, 0);
            var p060 = PieceTumbado("27604207", 600, 2700, 0);
            var p075 = PieceTumbado("27754219", 750, 2700, 0);
            var p090 = PieceTumbado("27904209", 900, 2700, 0);

            switch ((int)targetHeightMm)
            {
                case 300:
                    layout = Build((int)targetHeightMm, p030);
                    return true;
                case 450:
                    layout = Build((int)targetHeightMm, p045);
                    return true;
                case 600:
                    layout = Build((int)targetHeightMm, p060);
                    return true;
                case 750:
                    layout = Build((int)targetHeightMm, p075);
                    return true;
                case 900:
                    layout = Build((int)targetHeightMm, p090);
                    return true;
                case 1050:
                    layout = Build((int)targetHeightMm,
                        p060,
                        PieceTumbado("27454206", 450, 2700, 600));
                    return true;
                case 1200:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800));
                    return true;
                case 1350:
                    layout = Build((int)targetHeightMm,
                        p090,
                        PieceTumbado("27454206", 450, 2700, 900));
                    return true;
                case 1500:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27304205", 300, 2700, 1200));
                    return true;
                case 1650:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27454206", 450, 2700, 1200));
                    return true;
                case 1800:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27604207", 600, 2700, 1200));
                    return true;
                case 1950:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27754219", 750, 2700, 1200));
                    return true;
                case 2100:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27904209", 900, 2700, 1200));
                    return true;
                case 2400:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("24904240", 900, 2400, 0),
                        PieceVertical("24904240", 900, 2400, 900),
                        PieceVertical("24904240", 900, 2400, 1800));
                    return true;
                case 2550:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("12904215", 900, 1200, 0),
                        PieceVertical("12904215", 900, 1200, 900),
                        PieceVertical("12904215", 900, 1200, 1800),
                        PieceTumbado("27904209", 900, 2700, 1200),
                        PieceTumbado("27454206", 450, 2700, 2100));
                    return true;
                case 2700:
                    layout = Build((int)targetHeightMm,
                        PieceVertical("27904209", 900, 2700, 0),
                        PieceVertical("27904209", 900, 2700, 900),
                        PieceVertical("27904209", 900, 2700, 1800));
                    return true;
            }

            layout = null;
            return false;
        }

        private static Modulo270Layout BuildGreedyTumbadoLayout(double targetHeightMm)
        {
            var options = new[]
            {
                new { H = 900, Glb = "27904209" },
                new { H = 750, Glb = "27754219" },
                new { H = 600, Glb = "27604207" },
                new { H = 450, Glb = "27454206" },
                new { H = 300, Glb = "27304205" },
            };

            var remaining = targetHeightMm;
            var up = 0;
            var pieces = new List<Modulo270PieceLayout>();

            for (var i = 0; i < options.Length && remaining >= 299.5; i++)
            {
                while (remaining >= options[i].H - 0.5)
                {
                    pieces.Add(PieceTumbado(options[i].Glb, options[i].H, 2700, up));
                    remaining -= options[i].H;
                    up += options[i].H;
                }
            }

            if (pieces.Count == 0)
            {
                pieces.Add(PieceTumbado("27304205", 300, 2700, 0));
            }

            return Build((int)targetHeightMm, pieces.ToArray());
        }

        private static Modulo270Layout BuildMixedVerticalPlusTumbadoLayout(double targetHeightMm)
        {
            var pieces = new List<Modulo270PieceLayout>();

            var baseOptions = new[]
            {
                new { BaseH = 2700, Glb = "27904209" },
                new { BaseH = 2400, Glb = "24904240" },
                new { BaseH = 1200, Glb = "12904215" },
            };

            var selectedBase = baseOptions[0];
            for (var i = 0; i < baseOptions.Length; i++)
            {
                var rem = targetHeightMm - baseOptions[i].BaseH;
                if (Math.Abs(rem) < 0.5 || rem >= 299.5)
                {
                    selectedBase = baseOptions[i];
                    break;
                }
            }

            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 0));
            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 900));
            pieces.Add(PieceVertical(selectedBase.Glb, 900, selectedBase.BaseH, 1800));

            var remaining = targetHeightMm - selectedBase.BaseH;
            var up = selectedBase.BaseH;
            var topOptions = new[]
            {
                new { H = 900, Glb = "27904209" },
                new { H = 750, Glb = "27754219" },
                new { H = 600, Glb = "27604207" },
                new { H = 450, Glb = "27454206" },
                new { H = 300, Glb = "27304205" },
            };

            for (var i = 0; i < topOptions.Length && remaining >= 299.5; i++)
            {
                while (remaining >= topOptions[i].H - 0.5)
                {
                    pieces.Add(PieceTumbado(topOptions[i].Glb, topOptions[i].H, 2700, up));
                    remaining -= topOptions[i].H;
                    up += topOptions[i].H;
                }
            }

            return Build((int)targetHeightMm, pieces.ToArray());
        }

        private static Modulo270Layout Build(int catalogHeightMm, params Modulo270PieceLayout[] pieces)
        {
            return new Modulo270Layout
            {
                CatalogHeightMm = catalogHeightMm,
                Pieces = pieces != null ? pieces.ToList() : new List<Modulo270PieceLayout>()
            };
        }

        private static Modulo270Layout RemapLayoutToModuleLength(Modulo270Layout source, int moduleLengthMm)
        {
            if (source == null || source.Pieces == null || source.Pieces.Count == 0)
            {
                return source;
            }

            var columns = DecomposeLengthToPanelWidths(moduleLengthMm);
            if (columns.Count == 0)
            {
                columns.Add(Math.Max(300, moduleLengthMm));
            }

            var pieces = new List<Modulo270PieceLayout>();
            var seenVerticalKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < source.Pieces.Count; i++)
            {
                var template = source.Pieces[i];
                if (template == null)
                {
                    continue;
                }

                var isTumbado = string.Equals(template.Orientation, "Tumbado", StringComparison.OrdinalIgnoreCase);
                if (isTumbado)
                {
                    var glb = ExtractGlbCode(template.ElementCode);
                    pieces.Add(PieceTumbado(glb, template.PieceHeightMm, moduleLengthMm, template.UpOffsetMm));
                    continue;
                }

                var key = template.UpOffsetMm.ToString() + ":" + template.PieceHeightMm.ToString();
                if (!seenVerticalKeys.Add(key))
                {
                    continue;
                }

                var along = 0;
                for (var c = 0; c < columns.Count; c++)
                {
                    var width = columns[c];
                    var glb = ResolveVerticalGlbCode(template.PieceHeightMm, width);
                    pieces.Add(PieceVertical(glb, width, template.PieceHeightMm, along, template.UpOffsetMm));
                    along += width;
                }
            }

            return Build(source.CatalogHeightMm, pieces.ToArray());
        }

        private static List<int> DecomposeLengthToPanelWidths(int lengthMm)
        {
            var options = new[] { 900, 750, 600, 450, 300 };
            var columns = new List<int>();
            var remaining = Math.Max(0, lengthMm);
            for (var i = 0; i < options.Length && remaining >= 299; i++)
            {
                while (remaining >= options[i])
                {
                    columns.Add(options[i]);
                    remaining -= options[i];
                }
            }

            return columns;
        }

        private static string ResolveVerticalGlbCode(int heightMm, int widthMm)
        {
            var family = 2700;
            if (heightMm <= 1200)
            {
                family = 1200;
            }
            else if (heightMm <= 2400)
            {
                family = 2400;
            }

            if (family == 1200)
            {
                switch (widthMm)
                {
                    case 900: return "12904215";
                    case 750: return "12754120";
                    case 600: return "12604213";
                    case 450: return "12454212";
                    case 300: return "12304211";
                }
            }
            else if (family == 2400)
            {
                switch (widthMm)
                {
                    case 900: return "24904240";
                    case 750: return "24754224";
                    case 600: return "24604242";
                    case 450: return "24454243";
                    case 300: return "24304244";
                }
            }

            switch (widthMm)
            {
                case 750: return "27754219";
                case 600: return "27604207";
                case 450: return "27454206";
                case 300: return "27304205";
                default: return "27904209";
            }
        }

        private static string ExtractGlbCode(string elementCode)
        {
            if (string.IsNullOrWhiteSpace(elementCode))
            {
                return "27904209";
            }

            const string prefix = "PANEL_";
            if (elementCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return elementCode.Substring(prefix.Length);
            }

            return elementCode;
        }

        private static Modulo270PieceLayout PieceVertical(string glbCode, int widthMm, int heightMm, int alongOffsetMm)
        {
            return PieceVertical(glbCode, widthMm, heightMm, alongOffsetMm, 0);
        }

        private static Modulo270PieceLayout PieceVertical(string glbCode, int widthMm, int heightMm, int alongOffsetMm, int upOffsetMm)
        {
            return new Modulo270PieceLayout
            {
                ElementCode = "PANEL_" + glbCode,
                Orientation = "Vertical",
                ImportPath = BaseGlbPath + glbCode + ".glb",
                PieceWidthMm = widthMm,
                PieceHeightMm = heightMm,
                AlongOffsetMm = alongOffsetMm,
                UpOffsetMm = upOffsetMm,
                InsertOffsetX = 0,
                InsertOffsetY = 0,
                InsertOffsetZ = 0,
                BaseRotX = 0,
                BaseRotY = 0,
                BaseRotZ = 0,
                UseStrictPose = true,
            };
        }

        private static Modulo270PieceLayout PieceTumbado(string glbCode, int heightMm, int widthMm, int upOffsetMm)
        {
            return new Modulo270PieceLayout
            {
                ElementCode = "PANEL_" + glbCode,
                Orientation = "Tumbado",
                ImportPath = BaseGlbPath + glbCode + ".glb",
                PieceWidthMm = widthMm,
                PieceHeightMm = heightMm,
                AlongOffsetMm = 0,
                UpOffsetMm = upOffsetMm,
                InsertOffsetX = 0,
                InsertOffsetY = 0,
                InsertOffsetZ = 0,
                BaseRotX = 0,
                BaseRotY = 0,
                BaseRotZ = -Math.PI * 0.5,
                UseStrictPose = true,
            };
        }
    }

    internal sealed class Modulo270Layout
    {
        public int CatalogHeightMm { get; set; }
        public List<Modulo270PieceLayout> Pieces { get; set; }
    }

    internal sealed class Modulo270PieceLayout
    {
        public string ElementCode { get; set; }
        public string Orientation { get; set; }
        public string ImportPath { get; set; }
        public int PieceWidthMm { get; set; }
        public int PieceHeightMm { get; set; }
        public int AlongOffsetMm { get; set; }
        public int UpOffsetMm { get; set; }
        public double InsertOffsetX { get; set; }
        public double InsertOffsetY { get; set; }
        public double InsertOffsetZ { get; set; }
        public double BaseRotX { get; set; }
        public double BaseRotY { get; set; }
        public double BaseRotZ { get; set; }
        public bool UseStrictPose { get; set; }
    }
}
