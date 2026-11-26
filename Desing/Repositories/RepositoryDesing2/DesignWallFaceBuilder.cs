using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Repositories.RepositoryDesing2
{
    /// <summary>Eje de muro en planta (mm de escena: X, Z = Y de CAD).</summary>
    public sealed class DesignWallFaceAxis
    {
        public long Id { get; set; }
        public long WallDbId { get; set; }
        public string WallId { get; set; }
        public double P1X { get; set; }
        public double P1Z { get; set; }
        public double P2X { get; set; }
        public double P2Z { get; set; }
        public double ThicknessMm { get; set; }
        public double HeightMm { get; set; }
        public double BaseY { get; set; }
    }

    public sealed class DesignWallFace
    {
        public int Id { get; set; }
        public string IdText { get; set; }
        public string WallId { get; set; }
        public DesignWallFaceAxis Axis { get; set; }
        public bool IsInterior { get; set; }
        public string Orientation { get; set; }
        public double P1X { get; set; }
        public double P1Z { get; set; }
        public double P2X { get; set; }
        public double P2Z { get; set; }
        public double LengthMm { get; set; }
        /// <summary>Normal hacia fuera de la cara (hacia donde mira).</summary>
        public double NormalX { get; set; }
        public double NormalZ { get; set; }

        public string SideName
        {
            get { return IsInterior ? "Interior" : "Exterior"; }
        }
    }

    /// <summary>
    /// Copia en servidor de <c>buildDesignWallFaces</c> (Scripts/Desing2/desing2-wall-faces.js):
    /// mismos F-n que Develop › ID Faces. Por grupo conectado: primero exteriores en sentido
    /// horario desde la de abajo, luego interiores. Interior = lado del centroide del grupo.
    /// Si cambias uno, cambia el otro.
    /// </summary>
    public static class DesignWallFaceBuilder
    {
        public const double DefaultEpsMm = 50d;

        public static List<DesignWallFace> Build(IList<DesignWallFaceAxis> axes, double epsMm = DefaultEpsMm)
        {
            var eps = epsMm > 0d ? epsMm : DefaultEpsMm;
            var clean = new List<DesignWallFaceAxis>();
            if (axes != null)
            {
                for (var i = 0; i < axes.Count; i++)
                {
                    double ux, uz, len;
                    if (axes[i] != null && Dir(axes[i], out ux, out uz, out len))
                    {
                        clean.Add(axes[i]);
                    }
                }
            }

            var faces = new List<DesignWallFace>();
            var nextId = 1;
            var groups = ConnectedComponents(clean, eps);
            for (var g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                double cx, cz;
                Centroid(group, out cx, out cz);
                var ordered = OrderClockwiseFromBottom(group, cx, cz);
                for (var pass = 0; pass < 2; pass++)
                {
                    var interior = pass == 1;
                    for (var i = 0; i < ordered.Count; i++)
                    {
                        var face = BuildOne(ordered[i], interior, nextId, group, cx, cz, eps);
                        if (face != null)
                        {
                            faces.Add(face);
                            nextId++;
                        }
                    }
                }
            }

            return faces;
        }

        private static bool Dir(DesignWallFaceAxis axis, out double ux, out double uz, out double len)
        {
            var dx = axis.P2X - axis.P1X;
            var dz = axis.P2Z - axis.P1Z;
            len = Math.Sqrt(dx * dx + dz * dz);
            if (!(len > 1e-6))
            {
                ux = 0d;
                uz = 0d;
                return false;
            }

            ux = dx / len;
            uz = dz / len;
            return true;
        }

        private static double Thickness(DesignWallFaceAxis axis)
        {
            return axis.ThicknessMm > 1d ? axis.ThicknessMm : 300d;
        }

        private static bool Near(double ax, double az, double bx, double bz, double eps)
        {
            var dx = ax - bx;
            var dz = az - bz;
            return Math.Sqrt(dx * dx + dz * dz) <= eps;
        }

        private static bool PointOnSegment(double px, double pz, double ax, double az, double bx, double bz, double eps)
        {
            var dx = bx - ax;
            var dz = bz - az;
            var len = Math.Sqrt(dx * dx + dz * dz);
            if (!(len > 1e-9))
            {
                return Near(px, pz, ax, az, eps);
            }

            var t = ((px - ax) * dx + (pz - az) * dz) / (len * len);
            if (t < -1e-6 || t > 1d + 1e-6)
            {
                return false;
            }

            return Near(px, pz, ax + dx * t, az + dz * t, eps);
        }

        private static double JoinedHalfMm(
            DesignWallFaceAxis self,
            double endX,
            double endZ,
            double alongUx,
            double alongUz,
            IList<DesignWallFaceAxis> axes,
            double eps)
        {
            var best = 0d;
            for (var i = 0; i < axes.Count; i++)
            {
                var other = axes[i];
                if (other == null || ReferenceEquals(other, self) || (self != null && other.Id == self.Id))
                {
                    continue;
                }

                double ux, uz, len;
                if (!Dir(other, out ux, out uz, out len))
                {
                    continue;
                }

                if (Math.Abs(ux * alongUx + uz * alongUz) > 0.35d)
                {
                    continue;
                }

                var touches = Near(endX, endZ, other.P1X, other.P1Z, eps)
                    || Near(endX, endZ, other.P2X, other.P2Z, eps)
                    || PointOnSegment(endX, endZ, other.P1X, other.P1Z, other.P2X, other.P2Z, eps);
                if (!touches)
                {
                    continue;
                }

                var half = Thickness(other) * 0.5d;
                if (half > best)
                {
                    best = half;
                }
            }

            return best;
        }

        private static int Root(int[] parent, int i)
        {
            var x = i;
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }

            return x;
        }

        private static List<List<DesignWallFaceAxis>> ConnectedComponents(IList<DesignWallFaceAxis> axes, double eps)
        {
            var n = axes.Count;
            var parent = new int[n];
            for (var i = 0; i < n; i++)
            {
                parent[i] = i;
            }

            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var a = axes[i];
                    var b = axes[j];
                    if (Near(a.P1X, a.P1Z, b.P1X, b.P1Z, eps)
                        || Near(a.P1X, a.P1Z, b.P2X, b.P2Z, eps)
                        || Near(a.P2X, a.P2Z, b.P1X, b.P1Z, eps)
                        || Near(a.P2X, a.P2Z, b.P2X, b.P2Z, eps))
                    {
                        var pa = Root(parent, i);
                        var pb = Root(parent, j);
                        if (pa != pb)
                        {
                            parent[pa] = pb;
                        }
                    }
                }
            }

            // Mismo orden que el Map del JS: por la primera aparición de cada raíz.
            var order = new List<int>();
            var byRoot = new Dictionary<int, List<DesignWallFaceAxis>>();
            for (var i = 0; i < n; i++)
            {
                var p = Root(parent, i);
                List<DesignWallFaceAxis> list;
                if (!byRoot.TryGetValue(p, out list))
                {
                    list = new List<DesignWallFaceAxis>();
                    byRoot[p] = list;
                    order.Add(p);
                }

                list.Add(axes[i]);
            }

            return order.Select(p => byRoot[p]).ToList();
        }

        private static void Mid(DesignWallFaceAxis axis, out double x, out double z)
        {
            x = (axis.P1X + axis.P2X) * 0.5d;
            z = (axis.P1Z + axis.P2Z) * 0.5d;
        }

        private static void Centroid(IList<DesignWallFaceAxis> axes, out double cx, out double cz)
        {
            var sx = 0d;
            var sz = 0d;
            for (var i = 0; i < axes.Count; i++)
            {
                double mx, mz;
                Mid(axes[i], out mx, out mz);
                sx += mx;
                sz += mz;
            }

            cx = axes.Count > 0 ? sx / axes.Count : 0d;
            cz = axes.Count > 0 ? sz / axes.Count : 0d;
        }

        private static List<DesignWallFaceAxis> OrderClockwiseFromBottom(IList<DesignWallFaceAxis> axes, double cx, double cz)
        {
            // OrderByDescending es estable, como Array.prototype.sort.
            var ordered = axes
                .OrderByDescending(a =>
                {
                    double mx, mz;
                    Mid(a, out mx, out mz);
                    return Math.Atan2(mz - cz, mx - cx);
                })
                .ToList();
            if (ordered.Count < 2)
            {
                return ordered;
            }

            var best = 0;
            for (var i = 1; i < ordered.Count; i++)
            {
                double mx, mz, bx, bz;
                Mid(ordered[i], out mx, out mz);
                Mid(ordered[best], out bx, out bz);
                if (mz < bz - 1e-6 || (Math.Abs(mz - bz) <= 1e-6 && mx < bx))
                {
                    best = i;
                }
            }

            return ordered.Skip(best).Concat(ordered.Take(best)).ToList();
        }

        private static DesignWallFace BuildOne(
            DesignWallFaceAxis axis,
            bool interior,
            int id,
            IList<DesignWallFaceAxis> group,
            double cx,
            double cz,
            double eps)
        {
            double ux, uz, len;
            if (!Dir(axis, out ux, out uz, out len))
            {
                return null;
            }

            var nLeftX = -uz;
            var nLeftZ = ux;
            double mx, mz;
            Mid(axis, out mx, out mz);
            var towardCenter = nLeftX * (cx - mx) + nLeftZ * (cz - mz);
            var leftIsInterior = towardCenter > 1e-6;
            var extSign = leftIsInterior ? -1d : 1d;
            var sideSign = interior ? -extSign : extSign;
            var halfT = Thickness(axis) * 0.5d;
            var ox = nLeftX * halfT * sideSign;
            var oz = nLeftZ * halfT * sideSign;
            var half1 = JoinedHalfMm(axis, axis.P1X, axis.P1Z, ux, uz, group, eps);
            var half2 = JoinedHalfMm(axis, axis.P2X, axis.P2Z, -ux, -uz, group, eps);
            var along = interior ? -1d : 1d;
            var p1x = axis.P1X + ox - ux * half1 * along;
            var p1z = axis.P1Z + oz - uz * half1 * along;
            var p2x = axis.P2X + ox + ux * half2 * along;
            var p2z = axis.P2Z + oz + uz * half2 * along;
            var faceLen = Math.Sqrt((p2x - p1x) * (p2x - p1x) + (p2z - p1z) * (p2z - p1z));
            if (interior && faceLen < 20d)
            {
                p1x = axis.P1X + ox;
                p1z = axis.P1Z + oz;
                p2x = axis.P2X + ox;
                p2z = axis.P2Z + oz;
                faceLen = Math.Sqrt((p2x - p1x) * (p2x - p1x) + (p2z - p1z) * (p2z - p1z));
            }

            return new DesignWallFace
            {
                Id = id,
                IdText = "F-" + id,
                WallId = string.IsNullOrEmpty(axis.WallId) ? axis.Id.ToString(System.Globalization.CultureInfo.InvariantCulture) : axis.WallId,
                Axis = axis,
                IsInterior = interior,
                Orientation = Math.Abs(ux) >= Math.Abs(uz) ? "horizontal" : "vertical",
                P1X = p1x,
                P1Z = p1z,
                P2X = p2x,
                P2Z = p2z,
                LengthMm = faceLen,
                NormalX = nLeftX * sideSign,
                NormalZ = nLeftZ * sideSign
            };
        }
    }

    /// <summary>
    /// Vista de una cara para el plano de montaje, en la escena del visor (Y arriba; Z de escena = −Y de CAD,
    /// como la importación DWG): mirando la cara de frente, P1 queda a la izquierda y P2 a la derecha.
    /// Los grados son los de la planta de CAD (antihorario desde +X).
    /// </summary>
    public sealed class DesignWallFaceFrame
    {
        public const double ThroughToleranceMm = 1d;

        public double P1X { get; private set; }
        public double P1Z { get; private set; }
        public double P2X { get; private set; }
        public double P2Z { get; private set; }
        public double LengthMm { get; private set; }
        public double UX { get; private set; }
        public double UZ { get; private set; }
        public double NX { get; private set; }
        public double NZ { get; private set; }
        public double WallAngleDeg { get; private set; }
        public double NormalDeg { get; private set; }

        public static DesignWallFaceFrame From(DesignWallFace face)
        {
            var nx = face.NormalX;
            var nz = face.NormalZ;
            // Quien mira la cara está delante (lado +N) mirando hacia −N; con Y arriba su derecha es (nz, −nx).
            var rx = nz;
            var rz = -nx;
            var p1x = face.P1X;
            var p1z = face.P1Z;
            var p2x = face.P2X;
            var p2z = face.P2Z;
            if ((p2x - p1x) * rx + (p2z - p1z) * rz < 0d)
            {
                p1x = face.P2X;
                p1z = face.P2Z;
                p2x = face.P1X;
                p2z = face.P1Z;
            }

            return new DesignWallFaceFrame
            {
                P1X = p1x,
                P1Z = p1z,
                P2X = p2x,
                P2Z = p2z,
                LengthMm = face.LengthMm,
                UX = rx,
                UZ = rz,
                NX = nx,
                NZ = nz,
                WallAngleDeg = AxisAngleDeg(face.Axis.P2X - face.Axis.P1X, face.Axis.P2Z - face.Axis.P1Z),
                NormalDeg = Round3(Normalize360(Math.Atan2(-nz, nx) * 180d / Math.PI))
            };
        }

        /// <summary>Ángulo del eje en la planta de CAD (Y = −Z de escena), antihorario desde +X, en [0, 180) y con 3 decimales.</summary>
        public static double AxisAngleDeg(double dx, double dz)
        {
            var deg = Normalize360(Math.Atan2(-dz, dx) * 180d / Math.PI);
            if (deg >= 180d)
            {
                deg -= 180d;
            }

            deg = Round3(deg);
            return deg >= 180d ? 0d : deg;
        }

        public void Local(double x, double z, out double localX, out double depth)
        {
            localX = (x - P1X) * UX + (z - P1Z) * UZ;
            depth = (x - P1X) * NX + (z - P1Z) * NZ;
        }

        public const double AlongToleranceMm = 400d;
        public const double MaxFrontMm = 4000d;

        /// <summary>
        /// La pieza está sobre esta cara: a lo largo dentro de la cara (± 400 mm) y en profundidad entre
        /// la otra cara del muro y 4 m por delante (puntales). Fuera = encofrado de muros que ya no existen.
        /// </summary>
        public bool Holds(double x, double z, double thicknessMm, out double depth)
        {
            double localX;
            Local(x, z, out localX, out depth);
            var behind = (thicknessMm > 1d ? thicknessMm : 300d) + 60d;
            return localX >= -AlongToleranceMm
                && localX <= LengthMm + AlongToleranceMm
                && depth >= -behind
                && depth <= MaxFrontMm;
        }

        private static double Normalize360(double deg)
        {
            var d = deg % 360d;
            if (d < 0d)
            {
                d += 360d;
            }

            return Math.Abs(d - 360d) < 1e-9 ? 0d : d;
        }

        private static double Round3(double value)
        {
            var r = Math.Round(value, 3, MidpointRounding.AwayFromZero);
            return r == 0d ? 0d : r;
        }
    }

    /// <summary>Elige la cara de una pieza a partir del eje al que está enlazada.</summary>
    public static class DesignWallFaceAssigner
    {
        /// <param name="exterior">Cara exterior del eje.</param>
        /// <param name="interior">Cara interior del eje.</param>
        /// <param name="isThrough">true si la pieza está sobre el eje (tirante): va a la exterior y sale en las dos vistas.</param>
        public static DesignWallFace Pick(DesignWallFace exterior, DesignWallFace interior, double x, double z, out bool isThrough)
        {
            isThrough = false;
            var axis = exterior != null ? exterior.Axis : (interior != null ? interior.Axis : null);
            if (axis == null)
            {
                return null;
            }

            var dx = axis.P2X - axis.P1X;
            var dz = axis.P2Z - axis.P1Z;
            var len = Math.Sqrt(dx * dx + dz * dz);
            if (!(len > 1e-6))
            {
                return exterior ?? interior;
            }

            // Distancia con signo al eje, en el sentido de la normal de cada cara.
            var nLeftX = -dz / len;
            var nLeftZ = dx / len;
            var s = (x - axis.P1X) * nLeftX + (z - axis.P1Z) * nLeftZ;
            if (Math.Abs(s) <= DesignWallFaceFrame.ThroughToleranceMm)
            {
                isThrough = exterior != null && interior != null;
                return exterior ?? interior;
            }

            if (exterior != null && s * (exterior.NormalX * nLeftX + exterior.NormalZ * nLeftZ) > 0d)
            {
                return exterior;
            }

            if (interior != null && s * (interior.NormalX * nLeftX + interior.NormalZ * nLeftZ) > 0d)
            {
                return interior;
            }

            return exterior ?? interior;
        }
    }
}
