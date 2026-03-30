
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public partial class Form1 : Form
{
    readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer() { Interval = 50 };
    int cx, cy, R;

    public Form1()
    {
        Text = "Часовник";
        Size = new Size(520, 560);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(20, 20, 50);
        cx = ClientSize.Width / 2;
        cy = ClientSize.Height / 2;
        R  = Math.Min(ClientSize.Width, ClientSize.Height) / 2 - 30;
        timer.Tick += (_, _) => Invalidate();
        timer.Start();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        cx = ClientSize.Width  / 2;
        cy = ClientSize.Height / 2;
        R  = Math.Min(ClientSize.Width, ClientSize.Height) / 2 - 30;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(20, 20, 50));

        // Циферблат
        using var rimPen = new Pen(Color.FromArgb(180, 200, 230), 4);
        g.DrawEllipse(rimPen, cx - R, cy - R, 2 * R, 2 * R);

        // Деления
        for (int i = 0; i < 60; i++)
        {
            double a  = Math.PI * 2 * i / 60 - Math.PI / 2;
            bool major = (i % 5 == 0);
            int  r1    = major ? R - 18 : R - 8;
            int  w     = major ? 3 : 1;
            float x1 = (float)(cx + r1 * Math.Cos(a));
            float y1 = (float)(cy + r1 * Math.Sin(a));
            float x2 = (float)(cx + R  * Math.Cos(a));
            float y2 = (float)(cy + R  * Math.Sin(a));
            using var tp = new Pen(major ? Color.White : Color.Gray, w);
            g.DrawLine(tp, x1, y1, x2, y2);
        }

        // Числа 3, 6, 9, 12
        using var font = new Font("Arial", R / 9f, FontStyle.Bold);
        int[] nums = { 3, 6, 9, 12 };
        foreach (int n in nums)
        {
            double a = Math.PI * 2 * n / 12 - Math.PI / 2;
            float tx = (float)(cx + (R - 36) * Math.Cos(a)) - 8;
            float ty = (float)(cy + (R - 36) * Math.Sin(a)) - 9;
            g.DrawString(n.ToString(), font, Brushes.LightGray, tx, ty);
        }

        // Времето
        var now  = DateTime.Now;
        double sec  = now.Second  + now.Millisecond / 1000.0;
        double min  = now.Minute  + sec  / 60.0;
        double hour = (now.Hour % 12) + min / 60.0;

        DrawHand(g, hour * 30,  (int)(R * 0.52), Color.Gold,       7);
        DrawHand(g, min  * 6,   (int)(R * 0.74), Color.White,      4);
        DrawHand(g, sec  * 6,   (int)(R * 0.87), Color.OrangeRed,  2);

        // Централна точка
        g.FillEllipse(Brushes.OrangeRed, cx - 7, cy - 7, 14, 14);
        g.FillEllipse(Brushes.White,     cx - 3, cy - 3,  6,  6);

        // Текущо време
        using var tf = new Font("Consolas", 13f, FontStyle.Bold);
        string ts = now.ToString("HH:mm:ss");
        g.DrawString(ts, tf, Brushes.White,
            cx - g.MeasureString(ts, tf).Width / 2,
            cy + R * 0.60f);
    }

    void DrawHand(Graphics g, double deg, int len, Color c, int w)
    {
        double a  = deg * Math.PI / 180 - Math.PI / 2;
        float  ex = (float)(cx + len * Math.Cos(a));
        float  ey = (float)(cy + len * Math.Sin(a));
        using var pen = new Pen(c, w)
        {
            StartCap = LineCap.Round,
            EndCap   = LineCap.Round
        };
        // Задна противотежест
        float bx = (float)(cx - len * 0.18 * Math.Cos(a));
        float by = (float)(cy - len * 0.18 * Math.Sin(a));
        g.DrawLine(pen, bx, by, ex, ey);
    }

    static void Main() => Application.Run(new Form1());
}
