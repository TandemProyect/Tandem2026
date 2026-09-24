using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

var outDir = @"C:\00_Tandem2026\Camiseta-El-Shajara";
Directory.CreateDirectory(outDir);

var src = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    @".cursor\projects\c-00-Tandem2026\assets\4x4-2006-caligrafia.png");
if (!File.Exists(src))
    throw new FileNotFoundException(src);

Exact.Build(src, outDir, "4x4-2006-luna", dropTiny: 120);
Console.WriteLine("OK -> " + outDir);

static class Exact
{
    public static void Build(string srcPng, string outDir, string name, int dropTiny)
    {
        using var bmp = (Bitmap)Image.FromFile(srcPng);
        using var hi = ScaleSmooth(bmp, 3);
        var letters = ToInk(hi, 0, 240);
        var moon = ToInk(hi, 241, 690);
        DropTiny(letters, dropTiny);
        DropTiny(moon, 200);
        Subtract(moon, letters);
        var w = letters.GetLength(1);
        var h = letters.GetLength(0);
        var loopsL = Contours(letters);
        var loopsM = Contours(moon);
        var all = loopsL.Concat(loopsM).ToList();
        WriteSvg(Path.Combine(outDir, name + ".svg"), all, w, h);
        WriteDxfLayers(Path.Combine(outDir, name + ".dxf"), loopsL, loopsM, w, h, 220);
        using var prev = (Bitmap)Image.FromFile(srcPng);
        prev.Save(Path.Combine(outDir, name + ".png"), ImageFormat.Png);
        Console.WriteLine($"  letras {loopsL.Count} + luna {loopsM.Count} -> {name}.dxf");
    }

    static Bitmap ScaleSmooth(Bitmap src, int scale)
    {
        var dst = new Bitmap(src.Width * scale, src.Height * scale, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(dst);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.DrawImage(src, 0, 0, dst.Width, dst.Height);
        return dst;
    }

    static bool[,] ToInk(Bitmap bmp, int minSum, int maxSum)
    {
        var w = bmp.Width;
        var h = bmp.Height;
        var ink = new bool[h, w];
        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var bytes = new byte[Math.Abs(data.Stride) * h];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            var stride = data.Stride;
            for (var y = 0; y < h; y++)
            {
                var row = y * stride;
                for (var x = 0; x < w; x++)
                {
                    var i = row + x * 4;
                    if (bytes[i + 3] <= 20) continue;
                    var sum = bytes[i] + bytes[i + 1] + bytes[i + 2];
                    ink[y, x] = sum >= minSum && sum <= maxSum;
                }
            }
        }
        finally { bmp.UnlockBits(data); }
        return ink;
    }

    static void Subtract(bool[,] a, bool[,] b)
    {
        var h = a.GetLength(0); var w = a.GetLength(1);
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                if (b[y, x]) a[y, x] = false;
    }

    static void DropTiny(bool[,] ink, int minArea)
    {
        var h = ink.GetLength(0); var w = ink.GetLength(1);
        var seen = new bool[h, w];
        var dirs = new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                if (!ink[y, x] || seen[y, x]) continue;
                var bag = new List<(int x, int y)>();
                var q = new Queue<(int x, int y)>();
                q.Enqueue((x, y));
                seen[y, x] = true;
                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    bag.Add(p);
                    foreach (var d in dirs)
                    {
                        var nx = p.x + d.x; var ny = p.y + d.y;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h || seen[ny, nx] || !ink[ny, nx]) continue;
                        seen[ny, nx] = true;
                        q.Enqueue((nx, ny));
                    }
                }
                if (bag.Count < minArea)
                    foreach (var c in bag) ink[c.y, c.x] = false;
            }
    }

    static List<List<(double x, double y)>> Contours(bool[,] ink)
    {
        var h = ink.GetLength(0); var w = ink.GetLength(1);
        bool Ink(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && ink[y, x];
        var segs = new List<((double x, double y) a, (double x, double y) b)>();
        for (var y = -1; y < h; y++)
            for (var x = -1; x < w; x++)
            {
                var code = (Ink(x, y) ? 8 : 0) | (Ink(x + 1, y) ? 4 : 0) | (Ink(x + 1, y + 1) ? 2 : 0) | (Ink(x, y + 1) ? 1 : 0);
                if (code is 0 or 15) continue;
                var top = (x + 1.0, y + 0.5);
                var right = (x + 1.5, y + 1.0);
                var bottom = (x + 1.0, y + 1.5);
                var left = (x + 0.5, y + 1.0);
                switch (code)
                {
                    case 1: case 14: segs.Add((left, bottom)); break;
                    case 2: case 13: segs.Add((bottom, right)); break;
                    case 3: case 12: segs.Add((left, right)); break;
                    case 4: case 11: segs.Add((top, right)); break;
                    case 6: case 9: segs.Add((top, bottom)); break;
                    case 7: case 8: segs.Add((left, top)); break;
                    case 5: segs.Add((left, top)); segs.Add((bottom, right)); break;
                    case 10: segs.Add((left, bottom)); segs.Add((top, right)); break;
                }
            }

        string Key((double x, double y) p) => $"{p.x:0.##}|{p.y:0.##}";
        var adj = new Dictionary<string, List<((double x, double y) p, int id)>>();
        for (var i = 0; i < segs.Count; i++)
        {
            void Add(string k, (double x, double y) p)
            {
                if (!adj.TryGetValue(k, out var list)) adj[k] = list = new();
                list.Add((p, i));
            }
            Add(Key(segs[i].a), segs[i].b);
            Add(Key(segs[i].b), segs[i].a);
        }
        var used = new bool[segs.Count];
        var loops = new List<List<(double x, double y)>>();
        for (var i = 0; i < segs.Count; i++)
        {
            if (used[i]) continue;
            var loop = new List<(double x, double y)>();
            var cur = segs[i].a;
            var guard = 0;
            while (guard++ < segs.Count + 2)
            {
                loop.Add(cur);
                if (!adj.TryGetValue(Key(cur), out var nexts)) break;
                var found = false;
                foreach (var n in nexts)
                {
                    if (used[n.id]) continue;
                    used[n.id] = true;
                    cur = n.p;
                    found = true;
                    break;
                }
                if (!found) break;
                if (Key(cur) == Key(loop[0]) && loop.Count > 2) break;
            }
            if (loop.Count < 5) continue;
            loops.Add(SmoothLoop(loop));
        }
        return loops;
    }

    static List<(double x, double y)> SmoothLoop(List<(double x, double y)> raw)
    {
        if (TryCircle(raw, out var circle))
            return circle;
        var avg = MovingAverage(raw, 7);
        var cha = Chaikin(avg, 3);
        var simple = Rdp(cha, 1.35);
        return simple.Count >= 4 ? simple : cha;
    }

    static bool TryCircle(List<(double x, double y)> pts, out List<(double x, double y)> circle)
    {
        circle = pts;
        var cx = pts.Average(p => p.x);
        var cy = pts.Average(p => p.y);
        var rs = pts.Select(p => Math.Sqrt((p.x - cx) * (p.x - cx) + (p.y - cy) * (p.y - cy))).ToList();
        var mean = rs.Average();
        if (mean < 8) return false;
        var var = rs.Average(r => (r - mean) * (r - mean));
        var cv = Math.Sqrt(var) / mean;
        if (cv > 0.085) return false;
        var n = 72;
        var outPts = new List<(double x, double y)>(n);
        for (var i = 0; i < n; i++)
        {
            var a = i * 2 * Math.PI / n;
            outPts.Add((cx + mean * Math.Cos(a), cy + mean * Math.Sin(a)));
        }
        circle = outPts;
        return true;
    }

    static List<(double x, double y)> MovingAverage(List<(double x, double y)> pts, int win)
    {
        var n = pts.Count;
        var half = win / 2;
        var r = new List<(double x, double y)>(n);
        for (var i = 0; i < n; i++)
        {
            double sx = 0, sy = 0;
            for (var k = -half; k <= half; k++)
            {
                var p = pts[(i + k + n * 4) % n];
                sx += p.x; sy += p.y;
            }
            var d = half * 2 + 1;
            r.Add((sx / d, sy / d));
        }
        return r;
    }

    static List<(double x, double y)> Chaikin(List<(double x, double y)> pts, int rounds)
    {
        var cur = pts;
        for (var r = 0; r < rounds; r++)
        {
            var next = new List<(double x, double y)>(cur.Count * 2);
            for (var i = 0; i < cur.Count; i++)
            {
                var a = cur[i];
                var b = cur[(i + 1) % cur.Count];
                next.Add((0.75 * a.x + 0.25 * b.x, 0.75 * a.y + 0.25 * b.y));
                next.Add((0.25 * a.x + 0.75 * b.x, 0.25 * a.y + 0.75 * b.y));
            }
            cur = next;
        }
        return cur;
    }

    static List<(double x, double y)> Rdp(List<(double x, double y)> pts, double eps)
    {
        if (pts.Count < 3) return pts;
        var keep = new bool[pts.Count];
        keep[0] = keep[^1] = true;
        void Rec(int a, int b)
        {
            var max = -1.0; var idx = -1;
            var ax = pts[a].x; var ay = pts[a].y; var bx = pts[b].x; var by = pts[b].y;
            var dx = bx - ax; var dy = by - ay; var den = dx * dx + dy * dy;
            for (var i = a + 1; i < b; i++)
            {
                double d2;
                if (den < 1e-9) { var ex = pts[i].x - ax; var ey = pts[i].y - ay; d2 = ex * ex + ey * ey; }
                else
                {
                    var t = Math.Clamp(((pts[i].x - ax) * dx + (pts[i].y - ay) * dy) / den, 0, 1);
                    var ex = pts[i].x - (ax + t * dx); var ey = pts[i].y - (ay + t * dy);
                    d2 = ex * ex + ey * ey;
                }
                if (d2 > max) { max = d2; idx = i; }
            }
            if (idx >= 0 && max > eps * eps) { keep[idx] = true; Rec(a, idx); Rec(idx, b); }
        }
        Rec(0, pts.Count - 1);
        return pts.Where((_, i) => keep[i]).ToList();
    }

    static void SavePreview(Bitmap src, bool[,] ink, string path)
    {
        using var bmp = new Bitmap(src);
        var h = ink.GetLength(0);
        var w = ink.GetLength(1);
        var yWipe = (int)(h * 0.88);
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                if (y >= yWipe && x > w * 0.2 && x < w * 0.8)
                {
                    bmp.SetPixel(x, y, Color.White);
                    continue;
                }
                if (!ink[y, x])
                {
                    var p = bmp.GetPixel(x, y);
                    if (p.R + p.G + p.B < 700) bmp.SetPixel(x, y, Color.White);
                }
            }
        bmp.Save(path, ImageFormat.Png);
    }

    static void SaveInkPng(bool[,] ink, string path)
    {
        var h = ink.GetLength(0);
        var w = ink.GetLength(1);
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            var stride = data.Stride;
            var bytes = new byte[Math.Abs(stride) * h];
            for (var y = 0; y < h; y++)
            {
                var row = y * stride;
                for (var x = 0; x < w; x++)
                {
                    var i = row + x * 4;
                    var v = ink[y, x] ? (byte)17 : (byte)255;
                    bytes[i] = v; bytes[i + 1] = v; bytes[i + 2] = v; bytes[i + 3] = 255;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
        }
        finally { bmp.UnlockBits(data); }
        bmp.Save(path, ImageFormat.Png);
    }

    static void WriteSvg(string path, List<List<(double x, double y)>> loops, int w, int h)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {w} {h}\" fill=\"#111\" fill-rule=\"evenodd\" stroke=\"none\">");
        foreach (var loop in loops)
            sb.AppendLine("  <path d=\"" + string.Join(" ", loop.Select((p, i) => (i == 0 ? "M" : "L") + Inv(p.x) + "," + Inv(p.y))) + " Z\"/>");
        sb.AppendLine("</svg>");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    static void WriteDxfLayers(string path, List<List<(double x, double y)>> letters, List<List<(double x, double y)>> moon, int w, int h, double mmWidth)
    {
        var s = mmWidth / w;
        var sb = new StringBuilder();
        void Pair(int c, string v) { sb.Append(c); sb.Append('\n'); sb.Append(v); sb.Append('\n'); }
        void Dump(List<List<(double x, double y)>> loops, string layer)
        {
            foreach (var loop in loops)
            {
                for (var i = 0; i < loop.Count; i++)
                {
                    var a = loop[i]; var b = loop[(i + 1) % loop.Count];
                    Pair(0, "LINE"); Pair(8, layer);
                    Pair(10, Inv(a.x * s)); Pair(20, Inv((h - a.y) * s)); Pair(30, "0");
                    Pair(11, Inv(b.x * s)); Pair(21, Inv((h - b.y) * s)); Pair(31, "0");
                }
            }
        }
        Pair(0, "SECTION"); Pair(2, "HEADER");
        Pair(9, "$ACADVER"); Pair(1, "AC1009");
        Pair(0, "ENDSEC");
        Pair(0, "SECTION"); Pair(2, "ENTITIES");
        Dump(letters, "LETRAS");
        Dump(moon, "LUNA");
        Pair(0, "ENDSEC"); Pair(0, "EOF");
        File.WriteAllText(path, sb.ToString(), Encoding.ASCII);
    }

    static string Inv(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);
}
