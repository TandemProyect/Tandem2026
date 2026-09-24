namespace TandemRevit
{
    /// <summary>
    /// Revit trabaja en pies internos. LCornerDetector usa milímetros.
    /// </summary>
    internal static class CadUnits
    {
        private const double MillimetersPerFoot = 304.8;

        public static double ToMillimeters(double feet)
        {
            return feet * MillimetersPerFoot;
        }

        public static double FromMillimeters(double millimeters)
        {
            return millimeters / MillimetersPerFoot;
        }
    }
}
