using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.Geometry;
using Newtonsoft.Json.Linq;

namespace AutocadPlugin
{
    internal sealed class Atk60Snap
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public Point3d LocalMeters { get; set; }
    }

    internal static class Atk60SnapCatalog
    {
        public static IList<Atk60Snap> Load(string codeName, string role)
        {
            var list = new List<Atk60Snap>();
            var jsonPath = Atk60DwgResolver.FindSnapJson(codeName);
            if (!string.IsNullOrWhiteSpace(jsonPath) && File.Exists(jsonPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(jsonPath));
                    var snaps = root["snaps"] as JArray;
                    if (snaps != null)
                    {
                        foreach (var token in snaps)
                        {
                            var snapRole = ((string)token["role"] ?? "").Trim();
                            if (!string.IsNullOrWhiteSpace(role)
                                && !string.Equals(snapRole, role, StringComparison.OrdinalIgnoreCase))
                                continue;
                            list.Add(new Atk60Snap
                            {
                                Id = ((string)token["id"] ?? "").Trim(),
                                Role = snapRole,
                                LocalMeters = new Point3d(
                                    ReadD(token, "x"),
                                    ReadD(token, "y"),
                                    ReadD(token, "z"))
                            });
                        }
                    }
                }
                catch
                {
                }
            }

            if (list.Count == 0 && string.Equals(role, "PANEL", StringComparison.OrdinalIgnoreCase))
                list.AddRange(DefaultPanelVertices(codeName));
            return list;
        }

        public static IList<Atk60Snap> DefaultPanelVertices(string codeName)
        {
            double width = 0.9;
            double height = 2.7;
            if (string.Equals(codeName, "27904209", StringComparison.OrdinalIgnoreCase))
            {
                width = 0.9;
                height = 2.7;
            }
            return new[]
            {
                new Atk60Snap { Id = "V_BL", Role = "PANEL", LocalMeters = new Point3d(0, 0, 0) },
                new Atk60Snap { Id = "V_BR", Role = "PANEL", LocalMeters = new Point3d(width, 0, 0) },
                new Atk60Snap { Id = "V_TL", Role = "PANEL", LocalMeters = new Point3d(0, 0, height) },
                new Atk60Snap { Id = "V_TR", Role = "PANEL", LocalMeters = new Point3d(width, 0, height) }
            };
        }

        private static double ReadD(JToken token, string name)
        {
            if (token == null || token[name] == null)
                return 0;
            return (double)token[name];
        }
    }
}
