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
            var target = NormalizeTargetHeightMm(wallHeightMm);

            Modulo270Layout layout;
            if (TryGetKnownLayout(target, out layout))
            {
                return layout;
            }

            if (target > 2700d)
            {
                return BuildMixedVerticalPlusTumbadoLayout(target);
            }

            return BuildGreedyTumbadoLayout(target);
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

        private static Modulo270PieceLayout PieceVertical(string glbCode, int widthMm, int heightMm, int alongOffsetMm)
        {
            return new Modulo270PieceLayout
            {
                ElementCode = "PANEL_" + glbCode,
                Orientation = "Vertical",
                ImportPath = BaseGlbPath + glbCode + ".glb",
                PieceWidthMm = widthMm,
                PieceHeightMm = heightMm,
                AlongOffsetMm = alongOffsetMm,
                UpOffsetMm = 0,
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
    }
}
