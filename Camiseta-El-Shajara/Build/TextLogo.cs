using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Text;

static class TextLogo
{
    public static void Build(string outDir)
    {
        var fontName = FirstAvailable("Cooper Black", "Segoe UI Black", "Arial Black", "Impact");
        const int W = 2000;
        const int H = 900;

        using var letters = new GraphicsPath { FillMode = FillMode.Winding };
        using var font = new Font(fontName, 280, FontStyle.Regular, GraphicsUnit.Pixel);
        using var font2 = new Font(fontName, 250, FontStyle.Regular, GraphicsUnit.Pixel);
        using var bmp = new Bitmap(8, 8);
        using var g = Graphics.FromImage(bmp);
        var em = g.DpiY * font.SizeInPoints / 72f;
        var em2 = g.DpiY * font2.SizeInPoints / 72f;

        var w4 = g.MeasureString("4", font, PointF.Empty, StringFormat.GenericTypographic).Width;
        var gap = 36f;
        var xW = 200f;
        var row1 = w4 + gap + xW + gap + w4;
        var x0 = (W - row1) / 2f;
        letters.AddString("4", font.FontFamily, (int)font.Style, em, new PointF(x0, 170), StringFormat.GenericTypographic);
        letters.AddString("4", font.FontFamily, (int)font.Style, em, new PointF(x0 + w4 + gap + xW + gap, 170), StringFormat.GenericTypographic);
        var s2006 = g.MeasureString("2006", font2, PointF.Empty, StringFormat.GenericTypographic);
        letters.AddString("2006", font2.FontFamily, (int)font2.Style, em2,
            new PointF((W - s2006.Width) / 2f, 500), StringFormat.GenericTypographic);

        using var letterPen = new Pen(Color.Black, 22f) { LineJoin = LineJoin.Round, EndCap = LineCap.Round, StartCap = LineCap.Round };
        letters.Widen(letterPen);
        letters.Flatten(new Matrix(), 0.35f);

        using var cross = new GraphicsPath();
        var xMid = x0 + w4 + gap + xW / 2f;
        using var bar1 = new GraphicsPath();
        using var bar2 = new GraphicsPath();
        bar1.AddLine(xMid - 82, 222, xMid + 82, 378);
        bar2.AddLine(xMid + 82, 222, xMid - 82, 378);
        cross.AddPath(bar1, false);
        cross.AddPath(bar2, false);
        using var xPen = new Pen(Color.Black, 58f) { LineJoin = LineJoin.Round, EndCap = LineCap.Round, StartCap = LineCap.Round };
        cross.Widen(xPen);
        using var xHollow = (GraphicsPath)cross.Clone();
        using var xEdge = new Pen(Color.Black, 22f) { LineJoin = LineJoin.Round, EndCap = LineCap.Round, StartCap = LineCap.Round };
        xHollow.Widen(xEdge);
        xHollow.Flatten(new Matrix(), 0.35f);

        var loops = Figures(letters);
        loops.AddRange(Figures(xHollow));
        WriteSvg(Path.Combine(outDir, "4x4-2006.svg"), loops, W, H);
        WriteDxf(Path.Combine(outDir, "4x4-2006.dxf"), loops, W, H, 280);
        Preview(Path.Combine(outDir, "4x4-2006.png"), loops, W, H);
        Console.WriteLine($"  4x4 2006 -> {loops.Count} contornos  ({fontName})");
    }

    static string FirstAvailable(params string[] names)
    {
        foreach (var n in names)
        {
            try
            {
                using var f = new Font(n, 16);
                if (string.Equals(f.FontFamily.Name, n, StringComparison.OrdinalIgnoreCase))
                    return n;
            }
            catch { /* siguiente */ }
        }
        return FontFamily.GenericSansSerif.Name;
    }

    static List<List<(double x, double y)>> Figures(GraphicsPath path)
    {
        var loops = new List<List<(double x, double y)>>();
        if (path.PointCount < 3) return loops;
        var pts = path.PathPoints;
        var types = path.PathTypes;
        var cur = new List<(double x, double y)>();
        var start = 0;
        for (var i = 0; i < pts.Length; i++)
        {
            var isStart = (types[i] & 0x07) == 0;
            var isClose = (types[i] & 0x80) != 0;
            if (isStart && cur.Count > 2)
            {
                loops.Add(cur);
                cur = new List<(double x, double y)>();
            }
            if (isStart) start = i;
            cur.Add((pts[i].X, pts[i].Y));
            if (isClose)
            {
                var s = pts[start];
                if (Math.Abs(cur[^1].x - s.X) > 0.2 || Math.Abs(cur[^1].y - s.Y) > 0.2)
                    cur.Add((s.X, s.Y));
                if (cur.Count >= 4) loops.Add(cur);
                cur = new List<(double x, double y)>();
            }
        }
        if (cur.Count >= 4) loops.Add(cur);
        return loops;
    }

    static void Preview(string path, List<List<(double x, double y)>> loops, int w, int h)
    {
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var gp = new GraphicsPath { FillMode = FillMode.Alternate };
        foreach (var loop in loops)
        {
            if (loop.Count < 3) continue;
            gp.AddPolygon(loop.Select(p => new PointF((float)p.x, (float)p.y)).ToArray());
        }
        g.FillPath(Brushes.Black, gp);
        bmp.Save(path, ImageFormat.Png);
    }

    static void WriteSvg(string path, List<List<(double x, double y)>> loops, int w, int h)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {w} {h}\" fill=\"#111\" fill-rule=\"evenodd\" stroke=\"none\">");
        foreach (var loop in loops)
            sb.AppendLine("  <path d=\"" + string.Join(" ", loop.Select((p, i) =>
                (i == 0 ? "M" : "L") + Inv(p.x) + "," + Inv(p.y))) + " Z\"/>");
        sb.AppendLine("</svg>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    static void WriteDxf(string path, List<List<(double x, double y)>> loops, int w, int h, double mmWidth)
    {
        var s = mmWidth / w;
        var sb = new StringBuilder();
        void Pair(int c, string v) { sb.Append(c); sb.Append('\n'); sb.Append(v); sb.Append('\n'); }
        Pair(0, "SECTION"); Pair(2, "HEADER");
        Pair(9, "$ACADVER"); Pair(1, "AC1009");
        Pair(0, "ENDSEC");
        Pair(0, "SECTION"); Pair(2, "ENTITIES");
        foreach (var loop in loops)
        {
            for (var i = 0; i < loop.Count; i++)
            {
                var a = loop[i];
                var b = loop[(i + 1) % loop.Count];
                Pair(0, "LINE"); Pair(8, "TEXTO");
                Pair(10, Inv(a.x * s)); Pair(20, Inv((h - a.y) * s)); Pair(30, "0");
                Pair(11, Inv(b.x * s)); Pair(21, Inv((h - b.y) * s)); Pair(31, "0");
            }
        }
        Pair(0, "ENDSEC"); Pair(0, "EOF");
        File.WriteAllText(path, sb.ToString(), Encoding.ASCII);
    }

    static string Inv(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);
}
