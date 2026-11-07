using Desing.Models;
using System;
using System.Collections.Generic;

namespace Desing.Services
{
    /// <summary>
    /// Cálculo incremental de muro 2D para clientes CAD.
    /// Regla de arquitectura: la geometría se resuelve en C# (Desing), los clientes solo renderizan.
    /// </summary>
    public static class CadWall2dService
    {
        private const double Eps = 1e-8;

        public static List<PluginWall2dStepLineDTO> BuildStepAdditions(
            List<PuntoDTO> points,
            double thickness,
            bool reserveEndForNextStep,
            List<PluginWall2dAxisSegmentDTO> existingAxisSegments = null)
        {
            var additions = new List<PluginWall2dStepLineDTO>();
            if (points == null || points.Count < 2)
                return additions;

            var safeThickness = thickness > Eps ? thickness : 0.30;
            var half = safeThickness * 0.5;

            int lastIndex = points.Count - 1;
            var currentA = Clone(points[lastIndex - 1]);
            var currentB = Clone(points[lastIndex]);
            if (Distance2d(currentA, currentB) < Eps)
                return additions;

            var currentOffset = Perp2d(currentA, currentB, half);
            var plusA = Offset(currentA, currentOffset.X, currentOffset.Y);
            var plusB = Offset(currentB, currentOffset.X, currentOffset.Y);
            var minusA = Offset(currentA, -currentOffset.X, -currentOffset.Y);
            var minusB = Offset(currentB, -currentOffset.X, -currentOffset.Y);

            var endpointGuard = Math.Max(half * 0.2, 1e-4);
            var touchStartAxis = lastIndex == 1
                ? FindTouchingAxisSegment(currentA, existingAxisSegments, endpointGuard)
                : null;
            var touchEndAxis = FindTouchingAxisSegment(currentB, existingAxisSegments, endpointGuard);

            if (touchStartAxis != null)
            {
                ApplyTouchTrimAtStart(
                    touchStartAxis.Start,
                    touchStartAxis.End,
                    currentA,
                    currentB,
                    half,
                    ref plusA,
                    plusB,
                    ref minusA,
                    minusB);
            }

            if (lastIndex >= 2)
            {
                var prevA = Clone(points[lastIndex - 2]);
                var prevB = Clone(points[lastIndex - 1]);
                if (Distance2d(prevA, prevB) >= Eps)
                {
                    var prevDx = prevB.X - prevA.X;
                    var prevDy = prevB.Y - prevA.Y;
                    var prevLen = Math.Sqrt((prevDx * prevDx) + (prevDy * prevDy));
                    var prevUx = prevLen > Eps ? (prevDx / prevLen) : 0.0;
                    var prevUy = prevLen > Eps ? (prevDy / prevLen) : 0.0;

                    var prevOffset = Perp2d(prevA, prevB, half);
                    var prevPlusA = Offset(prevA, prevOffset.X, prevOffset.Y);
                    var prevPlusB = Offset(prevB, prevOffset.X, prevOffset.Y);
                    var prevMinusA = Offset(prevA, -prevOffset.X, -prevOffset.Y);
                    var prevMinusB = Offset(prevB, -prevOffset.X, -prevOffset.Y);

                    if (prevLen > Eps)
                    {
                        var prevReserve = Math.Min(half, prevLen * 0.5);
                        prevPlusB = new PuntoDTO
                        {
                            X = prevPlusB.X - (prevUx * prevReserve),
                            Y = prevPlusB.Y - (prevUy * prevReserve),
                            Z = prevPlusB.Z
                        };
                        prevMinusB = new PuntoDTO
                        {
                            X = prevMinusB.X - (prevUx * prevReserve),
                            Y = prevMinusB.Y - (prevUy * prevReserve),
                            Z = prevMinusB.Z
                        };
                    }

                    if (TryIntersect(prevPlusA, prevPlusB, plusA, plusB, out var miterPlus))
                    {
                        AddLineIfNotZero(additions, "face", prevPlusB, miterPlus);
                        plusA = miterPlus;
                    }

                    if (TryIntersect(prevMinusA, prevMinusB, minusA, minusB, out var miterMinus))
                    {
                        AddLineIfNotZero(additions, "face", prevMinusB, miterMinus);
                        minusA = miterMinus;
                    }
                }
            }

            if (touchEndAxis != null)
            {
                ApplyTouchTrimAtEnd(
                    touchEndAxis.Start,
                    touchEndAxis.End,
                    currentB,
                    currentA,
                    half,
                    plusA,
                    ref plusB,
                    minusA,
                    ref minusB);
            }

            var shouldReserveEnd = reserveEndForNextStep && touchEndAxis == null;
            if (shouldReserveEnd)
            {
                var dx = currentB.X - currentA.X;
                var dy = currentB.Y - currentA.Y;
                var len = Math.Sqrt((dx * dx) + (dy * dy));
                if (len > Eps)
                {
                    var reserve = Math.Min(half, len * 0.5);
                    var ux = dx / len;
                    var uy = dy / len;
                    plusB = new PuntoDTO
                    {
                        X = plusB.X - (ux * reserve),
                        Y = plusB.Y - (uy * reserve),
                        Z = plusB.Z
                    };
                    minusB = new PuntoDTO
                    {
                        X = minusB.X - (ux * reserve),
                        Y = minusB.Y - (uy * reserve),
                        Z = minusB.Z
                    };
                }
            }

            AddLineIfNotZero(additions, "axis", currentA, currentB);
            AddLineIfNotZero(additions, "face", plusA, plusB);
            AddLineIfNotZero(additions, "face", minusA, minusB);
            return additions;
        }

        private static void ApplyTouchTrimAtStart(
            PuntoDTO touchA,
            PuntoDTO touchB,
            PuntoDTO touchPoint,
            PuntoDTO probePoint,
            double half,
            ref PuntoDTO plusA,
            PuntoDTO plusB,
            ref PuntoDTO minusA,
            PuntoDTO minusB)
        {
            if (!TryChooseTouchFaceLine(touchA, touchB, half, touchPoint, probePoint, out var faceA, out var faceB))
                return;

            if (TryIntersect(plusA, plusB, faceA, faceB, out var hitPlus))
                plusA = hitPlus;
            if (TryIntersect(minusA, minusB, faceA, faceB, out var hitMinus))
                minusA = hitMinus;
        }

        private static void ApplyTouchTrimAtEnd(
            PuntoDTO touchA,
            PuntoDTO touchB,
            PuntoDTO touchPoint,
            PuntoDTO probePoint,
            double half,
            PuntoDTO plusA,
            ref PuntoDTO plusB,
            PuntoDTO minusA,
            ref PuntoDTO minusB)
        {
            if (!TryChooseTouchFaceLine(touchA, touchB, half, touchPoint, probePoint, out var faceA, out var faceB))
                return;

            if (TryIntersect(plusA, plusB, faceA, faceB, out var hitPlus))
                plusB = hitPlus;
            if (TryIntersect(minusA, minusB, faceA, faceB, out var hitMinus))
                minusB = hitMinus;
        }

        private static bool TryChooseTouchFaceLine(
            PuntoDTO touchA,
            PuntoDTO touchB,
            double half,
            PuntoDTO touchPoint,
            PuntoDTO probePoint,
            out PuntoDTO faceA,
            out PuntoDTO faceB)
        {
            faceA = null;
            faceB = null;

            var dx = touchB.X - touchA.X;
            var dy = touchB.Y - touchA.Y;
            var len = Math.Sqrt((dx * dx) + (dy * dy));
            if (len < Eps)
                return false;

            var side = (dx * (probePoint.Y - touchPoint.Y)) - (dy * (probePoint.X - touchPoint.X));
            var tol = 1e-6 * len;
            if (Math.Abs(side) <= tol)
                return false;

            var n = Perp2d(touchA, touchB, half);
            if (side > 0)
            {
                faceA = Offset(touchA, n.X, n.Y);
                faceB = Offset(touchB, n.X, n.Y);
            }
            else
            {
                faceA = Offset(touchA, -n.X, -n.Y);
                faceB = Offset(touchB, -n.X, -n.Y);
            }

            return true;
        }

        private static PluginWall2dAxisSegmentDTO FindTouchingAxisSegment(
            PuntoDTO point,
            List<PluginWall2dAxisSegmentDTO> segments,
            double endpointGuard)
        {
            if (point == null || segments == null || segments.Count == 0)
                return null;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment == null || segment.Start == null || segment.End == null)
                    continue;
                if (!PointOnSegmentInterior2d(point, segment.Start, segment.End, endpointGuard))
                    continue;
                return segment;
            }

            return null;
        }

        private static bool PointOnSegmentInterior2d(
            PuntoDTO point,
            PuntoDTO a,
            PuntoDTO b,
            double endpointGuard)
        {
            var len = Distance2d(a, b);
            if (len < Eps)
                return false;

            var guard = Math.Max(endpointGuard, 1e-6);
            guard = Math.Min(guard, len * 0.49);
            if (Distance2d(point, a) < guard || Distance2d(point, b) < guard)
                return false;

            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var cross = ((point.X - a.X) * dy) - ((point.Y - a.Y) * dx);
            if (Math.Abs(cross) > (1e-5 * len))
                return false;

            var dot = ((point.X - a.X) * dx) + ((point.Y - a.Y) * dy);
            var along = dot / len;
            if (along <= guard)
                return false;
            if (along >= len - guard)
                return false;

            return true;
        }

        private static void AddLineIfNotZero(
            List<PluginWall2dStepLineDTO> lines,
            string role,
            PuntoDTO start,
            PuntoDTO end)
        {
            if (lines == null || start == null || end == null)
                return;
            if (Distance2d(start, end) < Eps)
                return;

            lines.Add(new PluginWall2dStepLineDTO
            {
                Role = role,
                Start = Clone(start),
                End = Clone(end)
            });
        }

        private static PuntoDTO Clone(PuntoDTO p)
        {
            return new PuntoDTO
            {
                X = p?.X ?? 0,
                Y = p?.Y ?? 0,
                Z = p?.Z ?? 0
            };
        }

        private static PuntoDTO Offset(PuntoDTO p, double ox, double oy)
        {
            return new PuntoDTO
            {
                X = p.X + ox,
                Y = p.Y + oy,
                Z = p.Z
            };
        }

        private static Vector2 Perp2d(PuntoDTO a, PuntoDTO b, double half)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double len = Math.Sqrt((dx * dx) + (dy * dy));
            if (len < Eps)
                return new Vector2(0, half);

            double ux = dx / len;
            double uy = dy / len;
            return new Vector2(-uy * half, ux * half);
        }

        private static bool TryIntersect(
            PuntoDTO a1,
            PuntoDTO a2,
            PuntoDTO b1,
            PuntoDTO b2,
            out PuntoDTO hit)
        {
            hit = Clone(a2);
            double x1 = a1.X, y1 = a1.Y, x2 = a2.X, y2 = a2.Y;
            double x3 = b1.X, y3 = b1.Y, x4 = b2.X, y4 = b2.Y;
            double den = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            if (Math.Abs(den) < Eps)
                return false;

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            hit = new PuntoDTO
            {
                X = x1 + (t * (x2 - x1)),
                Y = y1 + (t * (y2 - y1)),
                Z = a1.Z
            };
            return true;
        }

        private static double Distance2d(PuntoDTO a, PuntoDTO b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt((dx * dx) + (dy * dy));
        }

        private sealed class Vector2
        {
            public Vector2(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X { get; private set; }
            public double Y { get; private set; }
        }
    }
}
