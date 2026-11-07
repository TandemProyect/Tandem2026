using System;
using System.Globalization;
using System.Text.Json;

namespace OpenCadStudioPluging
{
    internal static class OpenCadServeProtocol
    {
        public static string BuildLegacyNewRequest()
        {
            return "{\"op\":\"new\"}";
        }

        public static string BuildLegacyRunRequest(string cadCommand)
        {
            if (string.IsNullOrWhiteSpace(cadCommand))
                throw new ArgumentException("El comando CAD no puede estar vacio.", nameof(cadCommand));

            return JsonSerializer.Serialize(new LegacyRunRequest
            {
                op = "run",
                cmd = cadCommand
            });
        }

        public static string BuildLineCommand(double x1, double y1, double x2, double y2)
        {
            return "LINE "
                + FormatCoord(x1) + "," + FormatCoord(y1) + " "
                + FormatCoord(x2) + "," + FormatCoord(y2);
        }

        public static bool TryParseReadyBanner(string line, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                error = "Banner vacio.";
                return false;
            }

            try
            {
                using (var doc = JsonDocument.Parse(line))
                {
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("ok", out var okElement) || okElement.ValueKind != JsonValueKind.True)
                    {
                        error = "Banner sin ok:true.";
                        return false;
                    }

                    if (!root.TryGetProperty("ready", out var readyElement) || readyElement.ValueKind != JsonValueKind.True)
                    {
                        error = "Banner sin ready:true.";
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                error = "Banner invalido: " + ex.Message;
                return false;
            }
        }

        public static bool TryEnsureOkResponse(string line, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(line))
            {
                error = "Respuesta vacia.";
                return false;
            }

            try
            {
                using (var doc = JsonDocument.Parse(line))
                {
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("ok", out var okElement))
                    {
                        error = "La respuesta no contiene campo ok.";
                        return false;
                    }

                    if (okElement.ValueKind == JsonValueKind.True)
                        return true;

                    var detail = root.TryGetProperty("error", out var errorElement)
                        ? errorElement.ToString()
                        : "ok:false";
                    error = "OpenCADStudio devolvio error: " + detail;
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = "Respuesta invalida: " + ex.Message;
                return false;
            }
        }

        public static bool TryParseRunResponse(string line, out OpenCadRunResponse response, out string error)
        {
            response = null;
            if (!TryEnsureOkResponse(line, out error))
                return false;

            try
            {
                using (var doc = JsonDocument.Parse(line))
                {
                    var root = doc.RootElement;
                    response = new OpenCadRunResponse(
                        status: ReadString(root, "status") ?? "completed",
                        blockedBy: ReadNullableString(root, "blocked_by"),
                        added: ReadInt(root, "added"),
                        entities: ReadInt(root, "entities"),
                        rawJson: line);
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = "No se pudo interpretar la respuesta run: " + ex.Message;
                return false;
            }
        }

        private static string FormatCoord(double value)
        {
            return value.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        private static string ReadString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return null;
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString();
            return element.ToString();
        }

        private static string ReadNullableString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return null;
            if (element.ValueKind == JsonValueKind.Null)
                return null;
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString();
            return element.ToString();
        }

        private static int ReadInt(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return 0;
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var intValue))
                return intValue;
            return 0;
        }

        private sealed class LegacyRunRequest
        {
            public string op { get; set; }
            public string cmd { get; set; }
        }
    }

    internal sealed class OpenCadRunResponse
    {
        public OpenCadRunResponse(string status, string blockedBy, int added, int entities, string rawJson)
        {
            Status = status;
            BlockedBy = blockedBy;
            Added = added;
            Entities = entities;
            RawJson = rawJson;
        }

        public string Status { get; private set; }
        public string BlockedBy { get; private set; }
        public int Added { get; private set; }
        public int Entities { get; private set; }
        public string RawJson { get; private set; }
    }
}
