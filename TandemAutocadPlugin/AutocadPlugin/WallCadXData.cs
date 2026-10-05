using System;
using System.Globalization;
using AutocadPlugin.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutocadPlugin
{
    internal static class WallCadXData
    {
        public const string AppName = "TANDEM";
        public const string LayerAxis = "TANDEM_MURO_EJE";
        public const string LayerFace = "TANDEM_MURO_CARA";

        public static void Write(
            Entity ent,
            string role,
            int groupId,
            double thickness,
            long wallDbId,
            bool isSpecial)
        {
            if (ent == null)
                return;
            ent.XData = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, role ?? "axis"),
                new TypedValue((int)DxfCode.ExtendedDataInteger32, groupId),
                new TypedValue((int)DxfCode.ExtendedDataReal, thickness),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "WID:" + wallDbId.ToString(CultureInfo.InvariantCulture)),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, isSpecial ? "SPC:1" : "SPC:0"));
        }

        public static void Read(
            Entity ent,
            out string role,
            out int groupId,
            out double thickness,
            out long wallDbId,
            out bool isSpecial)
        {
            role = "axis";
            groupId = 0;
            thickness = CadUnits.FromMillimeters(300);
            wallDbId = 0;
            isSpecial = false;
            if (ent == null || ent.XData == null)
                return;

            try
            {
                var inApp = false;
                var gotRole = false;
                foreach (var t in ent.XData.AsArray())
                {
                    if (t.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                    {
                        inApp = string.Equals(t.Value as string, AppName, StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    if (!inApp)
                        continue;

                    if (t.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
                    {
                        var text = (t.Value as string ?? "").Trim();
                        if (text.StartsWith("WID:", StringComparison.OrdinalIgnoreCase))
                        {
                            long id;
                            if (long.TryParse(text.Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                                wallDbId = id;
                        }
                        else if (text.StartsWith("SPC:", StringComparison.OrdinalIgnoreCase))
                        {
                            isSpecial = text.Length > 4 && text[4] == '1';
                        }
                        else if (!gotRole)
                        {
                            role = text;
                            gotRole = true;
                        }
                    }
                    else if (t.TypeCode == (int)DxfCode.ExtendedDataInteger32 && groupId == 0)
                    {
                        groupId = Convert.ToInt32(t.Value);
                    }
                    else if (t.TypeCode == (int)DxfCode.ExtendedDataReal)
                    {
                        thickness = Convert.ToDouble(t.Value);
                    }
                }
            }
            catch
            {
            }
        }

        public static XyzMmDto ToDesingMm(Point3d p)
        {
            return new XyzMmDto
            {
                X = CadUnits.ToMillimeters(p.X),
                Y = CadUnits.ToMillimeters(p.Z),
                Z = CadUnits.ToMillimeters(p.Y)
            };
        }

        public static Point3d ToAcad(XyzMmDto p)
        {
            if (p == null)
                return Point3d.Origin;
            var x = p.X ?? 0;
            var yPlan = p.Z ?? p.Y ?? 0;
            var z = p.Z.HasValue ? (p.Y ?? 0) : 0;
            return new Point3d(
                CadUnits.FromMillimeters(x),
                CadUnits.FromMillimeters(yPlan),
                CadUnits.FromMillimeters(z));
        }

        public static double DistToSegment(Point3d p, Point3d a, Point3d b)
        {
            var ab = b - a;
            var ap = p - a;
            var len2 = ab.DotProduct(ab);
            if (len2 < 1e-18)
                return p.DistanceTo(a);
            var t = Math.Max(0, Math.Min(1, ap.DotProduct(ab) / len2));
            return p.DistanceTo(a + ab * t);
        }
    }
}
