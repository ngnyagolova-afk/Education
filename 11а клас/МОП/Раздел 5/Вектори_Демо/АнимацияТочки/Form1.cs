
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public partial class Form1 : Form
{
    const int N      = 14;
    const int FRAMES = 150;

    readonly Random    rnd    = new(42);
    readonly PointF[]  orig   = new PointF[N];
    readonly PointF[]  cur    = new PointF[N];
    readonly Color[]   colors = new Color[N];
    readonly System.Windows.Forms.Timer     timer  = new System.Windows.Forms.Timer() { Interval = 25 };
    int frame = 0;

    // режим: 0=ротация, 1=мащаб, 2=срязване, 3=комбо
    int mode = 0;
    readonly string[] modeNames =
        { "Ротация", "Пулсиращ мащаб", "Срязване", "Комбинация" };

    public Form1()
    {
        Text = "Анимация на точки с линейни трансформации";
        Size = new Size(680, 720);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(18, 18, 40);

        var hsvColors = new[]
        {
            Color.OrangeRed, Color.Orange, Color.Gold, Color.YellowGreen,
            Color.LimeGreen, Color.Cyan, Color.DeepSkyBlue, Color.DodgerBlue,
            Color.BlueViolet, Color.Violet, Color.HotPink, Color.Crimson,
            Color.Aquamarine, Color.Coral
        };
        for (int i = 0; i < N; i++)
        {
            orig[i]   = new PointF((float)(rnd.NextDouble() * 1.6 - 0.8),
                                   (float)(rnd.NextDouble() * 1.6 - 0.8));
            colors[i] = hsvColors[i % hsvColors.Length];
        }

        // Бутони за режим
        int bx = 10;
        for (int i = 0; i < modeNames.Length; i++)
        {
            int captured = i;
            var btn = new Button
            {
                Text = modeNames[i], Left = bx, Top = 10, Width = 155, Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 60, 110),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 140, 220);
            btn.Click += (_, _) => { mode = captured; frame = 0; };
            Controls.Add(btn);
            bx += 160;
        }

        timer.Tick += (_, _) => { frame = (frame + 1) % FRAMES; Invalidate(); };
        timer.Start();
    }

    PointF Transform(PointF p, double t)
    {
        double a  = 2 * Math.PI * t;
        double s  = 1 + 0.7 * Math.Sin(2 * Math.PI * t);
        double sh = 0.8 * Math.Sin(2 * Math.PI * t);

        return mode switch
        {
            0 => Rotate(p, a),
            1 => new PointF((float)(p.X * s), (float)(p.Y * s)),
            2 => new PointF((float)(p.X + sh * p.Y), p.Y),
            _ => Rotate(new PointF((float)(p.X * (0.7 + 0.5 * Math.Abs(Math.Sin(a)))),
                                   (float)(p.Y * (0.7 + 0.5 * Math.Abs(Math.Sin(a))))), a)
        };
    }

    static PointF Rotate(PointF p, double a)
    {
        double c = Math.Cos(a), s = Math.Sin(a);
        return new PointF((float)(c * p.X - s * p.Y),
                          (float)(s * p.X + c * p.Y));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(18, 18, 40));

        float cx = ClientSize.Width  / 2f;
        float cy = (ClientSize.Height + 50) / 2f;
        float sc = Math.Min(ClientSize.Width, ClientSize.Height - 60) / 2.6f;

        // Мрежа
        using var gp = new Pen(Color.FromArgb(35, 255, 255, 255));
        for (int i = -3; i <= 3; i++)
        {
            g.DrawLine(gp, cx + i * sc / 2, 55, cx + i * sc / 2, ClientSize.Height - 10);
            g.DrawLine(gp, 10, cy + i * sc / 2, ClientSize.Width - 10, cy + i * sc / 2);
        }
        // Оси
        using var axPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f);
        g.DrawLine(axPen, cx, 55, cx, ClientSize.Height - 10);
        g.DrawLine(axPen, 10, cy, ClientSize.Width - 10, cy);

        double t = (double)frame / FRAMES;

        // Траектории (опашки)
        for (int i = 0; i < N; i++)
        {
            int trail = 18;
            for (int tf2 = trail; tf2 >= 1; tf2--)
            {
                int pf = ((frame - tf2) % FRAMES + FRAMES) % FRAMES;
                double pt = (double)pf / FRAMES;
                var pp = Transform(orig[i], pt);
                float px = cx + pp.X * sc, py = cy + pp.Y * sc;
                int alpha = (int)(255.0 * (trail - tf2) / trail * 0.4);
                using var tb = new SolidBrush(Color.FromArgb(alpha, colors[i]));
                float r2 = 4f * (trail - tf2) / trail;
                g.FillEllipse(tb, px - r2, py - r2, r2 * 2, r2 * 2);
            }
        }

        // Оригинални точки
        for (int i = 0; i < N; i++)
        {
            float ox2 = cx + orig[i].X * sc, oy2 = cy + orig[i].Y * sc;
            g.FillEllipse(Brushes.DimGray, ox2 - 4, oy2 - 4, 8, 8);
        }

        // Анимирани точки
        for (int i = 0; i < N; i++)
        {
            cur[i] = Transform(orig[i], t);
            float px = cx + cur[i].X * sc, py = cy + cur[i].Y * sc;
            using var br = new SolidBrush(colors[i]);
            g.FillEllipse(br, px - 9, py - 9, 18, 18);
            using var ep = new Pen(Color.White, 1.5f);
            g.DrawEllipse(ep, px - 9, py - 9, 18, 18);
        }

        // Статус
        double angleDeg = t * 360;
        using var sf = new Font("Consolas", 10f);
        g.DrawString(
            $"Режим: {modeNames[mode]}   frame: {frame}/{FRAMES}   angle: {angleDeg:F0}°",
            sf, Brushes.LightGray, 10, ClientSize.Height - 24);

        // Легенда
        using var lf = new Font("Segoe UI", 9f);
        g.FillEllipse(Brushes.DimGray, 14, ClientSize.Height - 48, 10, 10);
        g.DrawString("оригинал", lf, Brushes.Gray, 26, ClientSize.Height - 50);
        g.FillEllipse(Brushes.OrangeRed, 110, ClientSize.Height - 48, 10, 10);
        g.DrawString("анимирани точки", lf, Brushes.LightGray, 122, ClientSize.Height - 50);
    }

    static void Main() => Application.Run(new Form1());
}
