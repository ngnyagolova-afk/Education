
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public partial class Form1 : Form
{
    Bitmap source;
    int selected = 0;
    string[] names =
    {
        "Оригинал",
        "Ротация 45°",
        "Ротация 90°",
        "Мащабиране 2x",
        "Мащабиране 0.5x",
        "Срязване X",
        "Отразяване X",
        "Отразяване Y",
        "Комбинация: Ротация + Мащаб"
    };

    public Form1()
    {
        Text = "Линейни трансформации";
        Size = new Size(960, 640);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(35, 35, 55);
        source = CreateDemoBitmap(320, 320);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(25, 25, 45),
            Padding = new Padding(6), AutoScroll = true
        };
        foreach (var n in names)
        {
            var btn = new Button
            {
                Text = n, Width = 180, Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 60, 100),
                ForeColor = Color.White, Font = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 120, 180);
            string captured = n;
            btn.Click += (_, _) =>
            {
                selected = Array.IndexOf(names, captured);
                Invalidate();
            };
            panel.Controls.Add(btn);
        }
        Controls.Add(panel);
    }

    // Генерира тестово изображение с форми
    static Bitmap CreateDemoBitmap(int w, int h)
    {
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(40, 60, 120));
        g.FillEllipse(Brushes.OrangeRed, 30, 30, 100, 100);
        g.FillRectangle(Brushes.Gold, 160, 50, 80, 80);
        g.FillPolygon(Brushes.LimeGreen,
            new[] { new Point(160, 260), new Point(220, 160), new Point(280, 260) });
        g.FillEllipse(Brushes.DeepSkyBlue, 50, 200, 80, 80);
        using var pen = new Pen(Color.White, 2);
        g.DrawEllipse(pen, 1, 1, w - 3, h - 3);
        using var f = new Font("Arial", 11f, FontStyle.Bold);
        g.DrawString("Тест", f, Brushes.White, w / 2f - 18, h - 24);
        return bmp;
    }

    Matrix GetMatrix(int idx, int w, int h)
    {
        var m = new Matrix();
        switch (idx)
        {
            case 1: m.RotateAt(45,  new PointF(w / 2f, h / 2f)); break;
            case 2: m.RotateAt(90,  new PointF(w / 2f, h / 2f)); break;
            case 3:
                m.Translate(w / 2f, h / 2f);
                m.Scale(2, 2);
                m.Translate(-w / 2f, -h / 2f);
                break;
            case 4:
                m.Translate(w / 2f, h / 2f);
                m.Scale(0.5f, 0.5f);
                m.Translate(-w / 2f, -h / 2f);
                break;
            case 5: return new Matrix(1, 0.35f, 0, 1, 0, 0);
            case 6: return new Matrix(1, 0, 0, -1, 0, h);
            case 7: return new Matrix(-1, 0, 0, 1, w, 0);
            case 8:
                m.RotateAt(30, new PointF(w / 2f, h / 2f));
                m.Scale(1.4f, 1.4f);
                break;
        }
        return m;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.FromArgb(35, 35, 55));

        int ox = 220, oy = 20;
        int tw = source.Width, th = source.Height;

        // Оригинал (вляво, малък)
        g.DrawString("Оригинал", new Font("Segoe UI", 10f, FontStyle.Bold),
            Brushes.Gray, ox, oy);
        g.DrawImage(source, ox, oy + 22, tw / 2, th / 2);
        using var grayPen = new Pen(Color.Gray, 1);
        g.DrawRectangle(grayPen, ox, oy + 22, tw / 2, th / 2);

        // Резултат (вдясно, голям)
        int rx = ox + tw / 2 + 24, ry = oy;
        g.DrawString(names[selected], new Font("Segoe UI", 12f, FontStyle.Bold),
            Brushes.Orange, rx, ry);

        var state = g.Save();
        g.SetClip(new Rectangle(rx, ry + 22, tw, th));
        g.TranslateTransform(rx + tw / 2f, ry + 22 + th / 2f);
        g.MultiplyTransform(GetMatrix(selected, tw, th));
        g.TranslateTransform(-tw / 2f, -th / 2f);
        g.DrawImage(source, 0, 0, tw, th);
        g.Restore(state);
        g.DrawRectangle(Pens.Orange, rx, ry + 22, tw, th);
    }

    static void Main() => Application.Run(new Form1());
}
