using System;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

namespace ZwcadPlugin
{
    /// <summary>
    /// El detector MVC (LCornerDetector / Desing_2) trabaja en milímetros.
    /// </summary>
    internal static class CadUnits
    {
        public static double MillimetersPerDrawingUnit()
        {
            try
            {
                int units = Convert.ToInt32(AcadApp.GetSystemVariable("INSUNITS"));
                if (units == 4) return 1.0;
                if (units == 5) return 10.0;
                if (units == 6) return 1000.0;
            }
            catch
            {
            }
            return 1000.0;
        }

        public static double ToMillimeters(double drawingValue)
        {
            return drawingValue * MillimetersPerDrawingUnit();
        }

        public static double FromMillimeters(double millimeters)
        {
            double scale = MillimetersPerDrawingUnit();
            return scale <= 0 ? millimeters : millimeters / scale;
        }
    }
}

