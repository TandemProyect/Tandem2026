using System;

namespace OpenCadStudioPluging
{
    internal static class BridgeActionMap
    {
        public static string[] MapToBridgeArguments(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return null;

            switch (action.Trim().ToLowerInvariant())
            {
                case "wall-2d":
                case "muro-2d":
                case "wall2d":
                case "muro2d":
                case "muro":
                case "lines":
                case "lineas":
                    return new[] { "ui-run", "TANDEM_MURO2D" };
                case "wall-3d":
                case "muro-3d":
                case "generar3d":
                    return new[] { "ocs-smoke" };
                case "formwork":
                case "encofrar":
                    return new[] { "ocs-smoke" };
                case "polyline":
                    return new[] { "ocs-run", "PLINE 0,0 500,0 500,500" };
                case "copy":
                    return new[] { "ocs-run", "COPY" };
                case "move":
                    return new[] { "ocs-run", "MOVE" };
                case "trim":
                    return new[] { "ocs-run", "TRIM" };
                case "extend":
                    return new[] { "ocs-run", "EXTEND" };
                case "stretch":
                    return new[] { "ocs-run", "STRETCH" };
                case "offset":
                    return new[] { "ocs-run", "OFFSET" };
                case "delete":
                    return new[] { "ocs-run", "ERASE" };
                case "server":
                case "show-server":
                    return new[] { "show-server" };
                case "local":
                case "set-local":
                    return new[] { "set-local" };
                case "produccion":
                case "set-production":
                    return new[] { "set-production" };
                default:
                    return null;
            }
        }
    }
}
