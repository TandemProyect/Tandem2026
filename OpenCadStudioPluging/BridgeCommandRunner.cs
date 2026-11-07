using System;
using System.Globalization;
using System.Threading;

namespace OpenCadStudioPluging
{
    internal static class BridgeCommandRunner
    {
        public static int Run(string[] args)
        {
            if (args == null || args.Length == 0)
                throw new ArgumentException("No se recibieron argumentos para ejecutar.", nameof(args));

            var command = (args[0] ?? "").Trim().ToLowerInvariant();
            switch (command)
            {
                case "ping":
                case "connect":
                    return MvcPingClient.PingAsync().GetAwaiter().GetResult();
                case "set-local":
                    MvcServerSettings.SetTarget("local");
                    Console.WriteLine("Servidor cambiado a local (localhost:44384).");
                    return 0;
                case "set-production":
                    MvcServerSettings.SetTarget("production");
                    Console.WriteLine("Servidor cambiado a produccion (tdesing.net).");
                    return 0;
                case "show-server":
                    Console.WriteLine("Servidor MVC: " + MvcServerSettings.CurrentLabel() + " - " + MvcServerSettings.CurrentUrl());
                    return 0;
                case "ocs-smoke":
                    return ExecuteCadCommandAsync(OpenCadServeProtocol.BuildLineCommand(0, 0, 1000, 0))
                        .GetAwaiter().GetResult();
                case "ocs-run":
                    if (args.Length < 2)
                        throw new InvalidOperationException("Uso: ocs-run <comando-cad>");
                    return ExecuteCadCommandAsync(args[1]).GetAwaiter().GetResult();
                case "ui-run":
                    if (args.Length < 2)
                        throw new InvalidOperationException("Uso: ui-run <comando-cad>");
                    OpenCadUiCommandLauncher.RunInActiveWindow(args[1]);
                    Console.WriteLine("OpenCADStudio UI command launched: " + args[1]);
                    return 0;
                case "wall-step":
                    if (args.Length < 3)
                        throw new InvalidOperationException("Uso: wall-step <thickness> [reserve-end 0|1] <x1,y1,z1;x2,y2,z2;...> [xa,ya,za,xb,yb,zb;...]");
                    if (!TryParseDouble(args[1], out var thickness))
                        throw new InvalidOperationException("Thickness inválido para wall-step.");
                    var reserveEndForNextStep = false;
                    var pointsArgIndex = 2;
                    if (args.Length >= 4 && TryParseBool01(args[2], out var parsedReserve))
                    {
                        reserveEndForNextStep = parsedReserve;
                        pointsArgIndex = 3;
                    }
                    if (args.Length <= pointsArgIndex)
                        throw new InvalidOperationException("wall-step requiere argumento de puntos.");
                    var pointsArg = args[pointsArgIndex];
                    var segmentsArg = args.Length > pointsArgIndex + 1 ? args[pointsArgIndex + 1] : "";
                    var points = MvcWall2dStepClient.ParsePointsArg(pointsArg);
                    if (points.Count < 2)
                        throw new InvalidOperationException("wall-step requiere al menos 2 puntos válidos.");
                    var existingAxisSegments = MvcWall2dStepClient.ParseAxisSegmentsArg(segmentsArg);
                    var lines = MvcWall2dStepClient
                        .CalculateStepAsync(points, thickness, reserveEndForNextStep, existingAxisSegments)
                        .GetAwaiter()
                        .GetResult();
                    foreach (var line in lines)
                    {
                        if (line == null || line.Start == null || line.End == null)
                            continue;

                        var role = string.IsNullOrWhiteSpace(line.Role) ? "face" : line.Role.Trim().ToLowerInvariant();
                        Console.WriteLine(
                            "WALL2D|{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                            role,
                            FormatCoord(line.Start.X),
                            FormatCoord(line.Start.Y),
                            FormatCoord(line.Start.Z),
                            FormatCoord(line.End.X),
                            FormatCoord(line.End.Y),
                            FormatCoord(line.End.Z));
                    }
                    return 0;
                case "ocs-insert-line":
                    if (args.Length != 5)
                        throw new InvalidOperationException("Uso: ocs-insert-line <x1> <y1> <x2> <y2>");
                    if (!TryParseDouble(args[1], out var x1)
                        || !TryParseDouble(args[2], out var y1)
                        || !TryParseDouble(args[3], out var x2)
                        || !TryParseDouble(args[4], out var y2))
                        throw new InvalidOperationException("Coordenadas invalidas.");
                    return ExecuteCadCommandAsync(OpenCadServeProtocol.BuildLineCommand(x1, y1, x2, y2))
                        .GetAwaiter().GetResult();
                default:
                    throw new InvalidOperationException("Comando no reconocido: " + command);
            }
        }

        private static bool TryParseDouble(string raw, out double value)
        {
            return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseBool01(string raw, out bool value)
        {
            var text = (raw ?? "").Trim();
            if (string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }
            if (string.Equals(text, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
            {
                value = false;
                return true;
            }
            value = false;
            return false;
        }

        private static string FormatCoord(double value)
        {
            return value.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        private static System.Threading.Tasks.Task<int> ExecuteCadCommandAsync(string cadCommand)
        {
            return ExecuteCadCommandCoreAsync(cadCommand);
        }

        private static async System.Threading.Tasks.Task<int> ExecuteCadCommandCoreAsync(string cadCommand)
        {
            var client = OpenCadStudioServeClient.CreateDefault();
            var response = await client.RunCadCommandAsync(cadCommand, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine(
                "OpenCADStudio OK | status={0} | added={1} | entities={2} | blocked_by={3}",
                response.Status,
                response.Added,
                response.Entities,
                response.BlockedBy ?? "null");
            return 0;
        }
    }
}
