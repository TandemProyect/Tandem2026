using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

internal static class TraceCamiseta
{
    private const double TargetWidthMm = 280.0;
    private const double RdpEpsilonPx = 0.95;
    private const int MinLoopPoints = 8;
    private const int InkThreshold = 140;

    private struct Pt
    {
        public double X;
        public double Y;
        public Pt(double x, double y) { X = x; Y = y; }
        public string Key { get { return X.ToString("0") + "," + Y.ToString("0"); } }
    }

    private class Edge
    {
        public int X1, Y1, X2, Y2;
        public bool Used;
    }

    private class Loop
    {
        public bool Closed;
        public List<Pt> Points;
    }

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static int Main(string[] args)
    {
        string src = args.Length > 0 ? args[0] : @"C:\00_Tandem2026\exports\camiseta-4x4-shajara\caravana-jeep-camello-jeep.png";
        string outDir = args.Length > 1 ? args[1] : Path.GetDirectoryName(src);
        string stem = Path.GetFileNameWithoutExtension(src);
        string title = args.Length > 2 ? args[2] : "EL SHAJARA";
        string subtitle = args.Length > 3 ? args[3] : "4x4  ·  RUTA SOLIDARIA";

        Directory.CreateDirectory(outDir);

        Bitmap bmp = (Bitmap)Image.FromFile(src);
        int w = bmp.Width;
        int h = bmp.Height;
        bool[,] mask = BuildMask(bmp, w, h);
        bmp.Dispose();

        List<Loop> loops = TraceLoops(mask, w, h);
        loops = SimplifyLoops(loops);

        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (Loop loop in loops)
        {
            foreach (Pt p in loop.Points)
            {
                if (p.X < minX) minX = (int)Math.Floor(p.X);
                if (p.Y < minY) minY = (int)Math.Floor(p.Y);
                if (p.X > maxX) maxX = (int)Math.Ceiling(p.X);
                if (p.Y > maxY) maxY = (int)Math.Ceiling(p.Y);
            }
        }

        double inkW = Math.Max(1, maxX - minX);
        double inkH = Math.Max(1, maxY - minY);
        double scale = TargetWidthMm / inkW;
        double artH = inkH * scale;
        double gapMm = 8.0;

        double textH;
        List<Loop> textLoops = BuildTextLoops(title, subtitle, TargetWidthMm, out textH);
        double totalH = artH + (textLoops.Count > 0 ? gapMm + textH : 0);

        List<Loop> artMm = Transform(loops, minX, maxY, scale, 0, 0, true);
        List<Loop> textMm = Transform(textLoops, 0, 0, 1, 0, -(artH + gapMm), false);
        List<Loop> all = new List<Loop>();
        all.AddRange(artMm);
        all.AddRange(textMm);

        string svgArt = Path.Combine(outDir, stem + ".svg");
        string svgText = Path.Combine(outDir, stem + "-con-texto.svg");
        string dxfArt = Path.Combine(outDir, stem + "_DXF-R12.dxf");
        string dxfText = Path.Combine(outDir, stem + "-con-texto_DXF-R12.dxf");
        string pngText = Path.Combine(outDir, stem + "-con-texto.png");

        WriteSvg(svgArt, artMm, TargetWidthMm, artH);
        WriteSvg(svgText, all, TargetWidthMm, totalH);
        WriteDxfR12(dxfArt, artMm);
        WriteDxfR12(dxfText, all);
        RenderPreview(pngText, all, TargetWidthMm, totalH);

        Console.WriteLine("src=" + src);
        Console.WriteLine("loopsArt=" + artMm.Count + " loopsText=" + textMm.Count + " ink=" + inkW + "x" + inkH);
        Console.WriteLine(svgArt);
        Console.WriteLine(svgText);
        Console.WriteLine(dxfArt);
        Console.WriteLine(dxfText);
        Console.WriteLine(pngText);
        return 0;
    }

    private static bool[,] BuildMask(Bitmap bmp, int w, int h)
    {
        bool[,] mask = new bool[w, h];
        BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        int stride = data.Stride;
        byte[] bytes = new byte[stride * h];
        Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
        bmp.UnlockBits(data);
        for (int y = 0; y < h; y++)
        {
            int row = y * stride;
            for (int x = 0; x < w; x++)
            {
                int i = row + x * 3;
                byte b = bytes[i];
                byte g = bytes[i + 1];
                byte r = bytes[i + 2];
                int lum = (r * 30 + g * 59 + b * 11) / 100;
                mask[x, y] = lum < InkThreshold;
            }
        }
        return mask;
    }

    private static bool Solid(bool[,] mask, int w, int h, int x, int y)
    {
        if (x < 0 || y < 0 || x >= w || y >= h) return false;
        return mask[x, y];
    }

    private static List<Loop> TraceLoops(bool[,] mask, int w, int h)
    {
        List<Edge> edges = new List<Edge>(w * 8);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[x, y]) continue;
                if (!Solid(mask, w, h, x, y - 1)) edges.Add(new Edge { X1 = x, Y1 = y, X2 = x + 1, Y2 = y });
                if (!Solid(mask, w, h, x + 1, y)) edges.Add(new Edge { X1 = x + 1, Y1 = y, X2 = x + 1, Y2 = y + 1 });
                if (!Solid(mask, w, h, x, y + 1)) edges.Add(new Edge { X1 = x + 1, Y1 = y + 1, X2 = x, Y2 = y + 1 });
                if (!Solid(mask, w, h, x - 1, y)) edges.Add(new Edge { X1 = x, Y1 = y + 1, X2 = x, Y2 = y });
            }
        }

        Dictionary<string, List<int>> byStart = new Dictionary<string, List<int>>(edges.Count);
        for (int i = 0; i < edges.Count; i++)
        {
            string k = edges[i].X1 + "," + edges[i].Y1;
            List<int> list;
            if (!byStart.TryGetValue(k, out list))
            {
                list = new List<int>(2);
                byStart[k] = list;
            }
            list.Add(i);
        }

        List<Loop> loops = new List<Loop>();
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].Used) continue;
            int startX = edges[i].X1;
            int startY = edges[i].Y1;
            List<Pt> pts = new List<Pt>();
            pts.Add(new Pt(edges[i].X1, edges[i].Y1));
            int cx = edges[i].X2;
            int cy = edges[i].Y2;
            edges[i].Used = true;
            pts.Add(new Pt(cx, cy));
            int guard = 0;
            while (!(cx == startX && cy == startY) && guard < 400000)
            {
                guard++;
                string key = cx + "," + cy;
                int next = -1;
                List<int> cands;
                if (byStart.TryGetValue(key, out cands))
                {
                    for (int c = 0; c < cands.Count; c++)
                    {
                        if (!edges[cands[c]].Used)
                        {
                            next = cands[c];
                            break;
                        }
                    }
                }
                if (next < 0) break;
                edges[next].Used = true;
                cx = edges[next].X2;
                cy = edges[next].Y2;
                pts.Add(new Pt(cx, cy));
            }
            if (pts.Count >= MinLoopPoints)
            {
                loops.Add(new Loop { Closed = (cx == startX && cy == startY), Points = pts });
            }
        }
        return loops;
    }

    private static List<Loop> SimplifyLoops(List<Loop> loops)
    {
        List<Loop> result = new List<Loop>();
        foreach (Loop loop in loops)
        {
            List<Pt> col = RemoveColinear(loop.Points, loop.Closed);
            List<Pt> simp = Rdp(col, RdpEpsilonPx, loop.Closed);
            if (simp.Count >= 4)
            {
                result.Add(new Loop { Closed = loop.Closed, Points = simp });
            }
        }
        return result;
    }

    private static List<Pt> RemoveColinear(List<Pt> pts, bool closed)
    {
        if (pts.Count < 3) return pts;
        List<Pt> outPts = new List<Pt>();
        int n = pts.Count;
        int last = closed && Almost(pts[0], pts[n - 1]) ? n - 1 : n;
        for (int i = 0; i < last; i++)
        {
            Pt prev = pts[(i - 1 + last) % last];
            Pt p = pts[i];
            Pt next = pts[(i + 1) % last];
            double cross = (p.X - prev.X) * (next.Y - p.Y) - (p.Y - prev.Y) * (next.X - p.X);
            if (Math.Abs(cross) > 0.01) outPts.Add(p);
        }
        if (!closed && pts.Count > 0)
        {
            if (outPts.Count == 0 || !Almost(outPts[0], pts[0])) outPts.Insert(0, pts[0]);
            if (!Almost(outPts[outPts.Count - 1], pts[last - 1])) outPts.Add(pts[last - 1]);
        }
        return outPts;
    }

    private static List<Pt> Rdp(List<Pt> pts, double eps, bool closed)
    {
        if (pts.Count < 3) return new List<Pt>(pts);
        List<Pt> work = new List<Pt>(pts);
        if (closed && work.Count > 1 && Almost(work[0], work[work.Count - 1]))
        {
            work.RemoveAt(work.Count - 1);
        }
        bool[] keep = new bool[work.Count];
        keep[0] = true;
        keep[work.Count - 1] = true;
        RdpRec(work, 0, work.Count - 1, eps * eps, keep);
        List<Pt> result = new List<Pt>();
        for (int i = 0; i < work.Count; i++)
        {
            if (keep[i]) result.Add(work[i]);
        }
        return result;
    }

    private static void RdpRec(List<Pt> pts, int a, int b, double eps2, bool[] keep)
    {
        double max = -1;
        int idx = -1;
        Pt pa = pts[a];
        Pt pb = pts[b];
        for (int i = a + 1; i < b; i++)
        {
            double d = Dist2Segment(pts[i], pa, pb);
            if (d > max)
            {
                max = d;
                idx = i;
            }
        }
        if (idx >= 0 && max > eps2)
        {
            keep[idx] = true;
            RdpRec(pts, a, idx, eps2, keep);
            RdpRec(pts, idx, b, eps2, keep);
        }
    }

    private static double Dist2Segment(Pt p, Pt a, Pt b)
    {
        double vx = b.X - a.X;
        double vy = b.Y - a.Y;
        double wx = p.X - a.X;
        double wy = p.Y - a.Y;
        double c1 = vx * wx + vy * wy;
        if (c1 <= 0) return wx * wx + wy * wy;
        double c2 = vx * vx + vy * vy;
        if (c2 <= c1)
        {
            double dx = p.X - b.X;
            double dy = p.Y - b.Y;
            return dx * dx + dy * dy;
        }
        double t = c1 / c2;
        double px = a.X + t * vx;
        double py = a.Y + t * vy;
        double ex = p.X - px;
        double ey = p.Y - py;
        return ex * ex + ey * ey;
    }

    private static bool Almost(Pt a, Pt b)
    {
        return Math.Abs(a.X - b.X) < 1e-6 && Math.Abs(a.Y - b.Y) < 1e-6;
    }

    private static List<Loop> BuildTextLoops(string title, string subtitle, double widthMm, out double heightMm)
    {
        heightMm = 0;
        List<Loop> loops = new List<Loop>();
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(subtitle)) return loops;

            string fontName = FirstAvailableFont(new[] { "Arial Black", "Impact", "Cooper Black", "Arial" });
        using (FontFamily family = new FontFamily(fontName))
        using (GraphicsPath path = new GraphicsPath())
        using (StringFormat fmt = (StringFormat)StringFormat.GenericTypographic.Clone())
        {
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Near;
            fmt.FormatFlags |= StringFormatFlags.NoClip;

            float em = 72f;
            RectangleF layout = new RectangleF(0, 0, 1000, 400);
            if (!string.IsNullOrWhiteSpace(title))
            {
                path.AddString(title, family, (int)FontStyle.Regular, em, layout, fmt);
            }
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                RectangleF sub = new RectangleF(0, em * 1.15f, 1000, 200);
                path.AddString(subtitle, family, (int)FontStyle.Regular, em * 0.42f, sub, fmt);
            }
            path.Flatten(null, 0.4f);
            RectangleF bounds = path.GetBounds();
            if (bounds.Width < 1) return loops;

            double scale = (widthMm * 0.62) / bounds.Width;
            double offsetX = (widthMm - bounds.Width * scale) / 2.0 - bounds.Left * scale;
            heightMm = bounds.Height * scale;

            loops.AddRange(PathToLoopsYUp(path, scale, offsetX, bounds));
        }
        return loops;
    }

    private static string FirstAvailableFont(string[] names)
    {
        using (InstalledFontCollection installed = new InstalledFontCollection())
        {
            HashSet<string> have = new HashSet<string>(installed.Families.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
            foreach (string n in names)
            {
                if (have.Contains(n)) return n;
            }
        }
        return "Arial";
    }

    private static List<Loop> PathToLoopsYUp(GraphicsPath path, double scale, double ox, RectangleF bounds)
    {
        List<Loop> loops = new List<Loop>();
        PointF[] pts = path.PathPoints;
        byte[] types = path.PathTypes;
        List<Pt> current = new List<Pt>();
        bool closed = false;
        for (int i = 0; i < pts.Length; i++)
        {
            int baseType = types[i] & 7;
            bool isClose = (types[i] & 128) != 0;
            if (baseType == 0)
            {
                if (current.Count > 2)
                {
                    loops.Add(new Loop { Closed = closed, Points = new List<Pt>(current) });
                }
                current.Clear();
                closed = false;
            }
            double x = pts[i].X * scale + ox;
            double y = (bounds.Bottom - pts[i].Y) * scale;
            current.Add(new Pt(x, y));
            if (isClose) closed = true;
        }
        if (current.Count > 2)
        {
            loops.Add(new Loop { Closed = closed, Points = current });
        }
        return loops;
    }

    private static List<Loop> Transform(List<Loop> loops, double minX, double maxY, double scale, double ox, double oy, bool fromPixel)
    {
        List<Loop> result = new List<Loop>(loops.Count);
        foreach (Loop loop in loops)
        {
            List<Pt> pts = new List<Pt>(loop.Points.Count);
            foreach (Pt p in loop.Points)
            {
                double x, y;
                if (fromPixel)
                {
                    x = (p.X - minX) * scale + ox;
                    y = (maxY - p.Y) * scale + oy;
                }
                else
                {
                    x = p.X + ox;
                    y = p.Y + oy;
                }
                pts.Add(new Pt(x, y));
            }
            result.Add(new Loop { Closed = loop.Closed, Points = pts });
        }
        return result;
    }

    private static void WriteSvg(string path, List<Loop> loops, double width, double height)
    {
        StringBuilder sb = new StringBuilder();
        double minY, maxY;
        BoundsY(loops, out minY, out maxY);
        double spanY = Math.Max(height, maxY - minY);
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine(string.Format(Inv,
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0:0.###}mm\" height=\"{1:0.###}mm\" viewBox=\"0 0 {0:0.###} {1:0.###}\">",
            width, spanY));
        sb.AppendLine("<g fill=\"#000\" fill-rule=\"evenodd\" stroke=\"none\">");
        foreach (Loop loop in loops)
        {
            if (loop.Points.Count < 2) continue;
            sb.Append("<path d=\"");
            sb.Append("M ");
            sb.Append(loop.Points[0].X.ToString("0.###", Inv));
            sb.Append(" ");
            sb.Append((maxY - loop.Points[0].Y).ToString("0.###", Inv));
            for (int i = 1; i < loop.Points.Count; i++)
            {
                sb.Append(" L ");
                sb.Append(loop.Points[i].X.ToString("0.###", Inv));
                sb.Append(" ");
                sb.Append((maxY - loop.Points[i].Y).ToString("0.###", Inv));
            }
            if (loop.Closed) sb.Append(" Z");
            sb.AppendLine("\"/>");
        }
        sb.AppendLine("</g></svg>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    private static void WriteDxfR12(string path, List<Loop> loops)
    {
        List<string> lines = new List<string>();
        Action<string> A = s => lines.Add(s);
        A("0"); A("SECTION"); A("2"); A("HEADER");
        A("9"); A("$ACADVER"); A("1"); A("AC1009");
        A("9"); A("$INSUNITS"); A("70"); A("4");
        A("0"); A("ENDSEC");
        A("0"); A("SECTION"); A("2"); A("TABLES");
        A("0"); A("TABLE"); A("2"); A("LAYER"); A("70"); A("1");
        A("0"); A("LAYER"); A("2"); A("CAMISETA_4X4"); A("70"); A("0"); A("62"); A("7"); A("6"); A("CONTINUOUS");
        A("0"); A("ENDTAB"); A("0"); A("ENDSEC");
        A("0"); A("SECTION"); A("2"); A("ENTITIES");
        foreach (Loop loop in loops)
        {
            List<Pt> pts = loop.Points;
            if (pts.Count < 2) continue;
            A("0"); A("POLYLINE"); A("8"); A("CAMISETA_4X4"); A("62"); A("7"); A("66"); A("1"); A("70"); A(loop.Closed ? "1" : "0");
            foreach (Pt p in pts)
            {
                A("0"); A("VERTEX"); A("8"); A("CAMISETA_4X4");
                A("10"); A(p.X.ToString("0.###", Inv));
                A("20"); A(p.Y.ToString("0.###", Inv));
                A("30"); A("0");
            }
            A("0"); A("SEQEND");
        }
        A("0"); A("ENDSEC"); A("0"); A("EOF");
        File.WriteAllLines(path, lines.ToArray(), Encoding.ASCII);
    }

    private static void RenderPreview(string path, List<Loop> loops, double widthMm, double heightMm)
    {
        double minY, maxY;
        BoundsY(loops, out minY, out maxY);
        double spanY = Math.Max(1e-6, maxY - minY);
        int pxW = 1600;
        int pxH = Math.Max(1, (int)Math.Round(pxW * (spanY / widthMm)));
        double sx = pxW / widthMm;
        double sy = pxH / spanY;
        using (Bitmap bmp = new Bitmap(pxW, pxH, PixelFormat.Format24bppRgb))
        using (Graphics g = Graphics.FromImage(bmp))
        using (GraphicsPath gp = new GraphicsPath(FillMode.Alternate))
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Loop loop in loops)
            {
                if (loop.Points.Count < 2) continue;
                PointF[] pts = new PointF[loop.Points.Count];
                for (int i = 0; i < loop.Points.Count; i++)
                {
                    pts[i] = new PointF((float)(loop.Points[i].X * sx), (float)((maxY - loop.Points[i].Y) * sy));
                }
                if (loop.Closed) gp.AddPolygon(pts);
                else gp.AddLines(pts);
            }
            g.FillPath(Brushes.Black, gp);
            bmp.Save(path, ImageFormat.Png);
        }
    }

    private static void BoundsY(List<Loop> loops, out double minY, out double maxY)
    {
        minY = double.MaxValue;
        maxY = double.MinValue;
        foreach (Loop loop in loops)
        {
            foreach (Pt p in loop.Points)
            {
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }
        }
        if (minY == double.MaxValue)
        {
            minY = 0;
            maxY = 1;
        }
    }
}
