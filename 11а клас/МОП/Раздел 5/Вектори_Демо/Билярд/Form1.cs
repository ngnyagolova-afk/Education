
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// ── Ball ──────────────────────────────────────────────────────────────────────
public class Ball
{
    public PointF Pos, Vel;
    public Color  Color;
    public int    Number;
    public const float R        = 16f;
    public const float FRICTION = 0.991f;

    public Ball(float x, float y, float vx, float vy, Color c, int n = 0)
    { Pos = new(x,y); Vel = new(vx,vy); Color = c; Number = n; }

    public bool Moving => MathF.Abs(Vel.X) > 0.05f || MathF.Abs(Vel.Y) > 0.05f;

    public void Update(int W, int H)
    {
        Pos = new(Pos.X + Vel.X, Pos.Y + Vel.Y);
        Vel = new(Vel.X * FRICTION, Vel.Y * FRICTION);
        if (!Moving) Vel = new(0, 0);
        if (Pos.X - R < 22)    { Pos = new(22 + R,    Pos.Y); Vel = new(-Vel.X, Vel.Y); }
        if (Pos.X + R > W-22)  { Pos = new(W-22-R,    Pos.Y); Vel = new(-Vel.X, Vel.Y); }
        if (Pos.Y - R < 22)    { Pos = new(Pos.X,  22 + R);   Vel = new(Vel.X, -Vel.Y); }
        if (Pos.Y + R > H-22)  { Pos = new(Pos.X,  H-22-R);   Vel = new(Vel.X, -Vel.Y); }
    }

    public void Collide(Ball o)
    {
        float dx = o.Pos.X - Pos.X, dy = o.Pos.Y - Pos.Y;
        float dist = MathF.Sqrt(dx*dx + dy*dy);
        if (dist < 2*R && dist > 0.01f)
        {
            float nx = dx/dist, ny = dy/dist;
            float overlap = 2*R - dist;
            Pos   = new(Pos.X   - nx*overlap/2, Pos.Y   - ny*overlap/2);
            o.Pos = new(o.Pos.X + nx*overlap/2, o.Pos.Y + ny*overlap/2);
            float dvx = Vel.X - o.Vel.X, dvy = Vel.Y - o.Vel.Y;
            float imp = dvx*nx + dvy*ny;
            if (imp > 0)
            {
                Vel   = new(Vel.X   - imp*nx, Vel.Y   - imp*ny);
                o.Vel = new(o.Vel.X + imp*nx, o.Vel.Y + imp*ny);
            }
        }
    }

    public void Draw(Graphics g)
    {
        using var br = new SolidBrush(Color);
        g.FillEllipse(br, Pos.X - R, Pos.Y - R, 2*R, 2*R);
        // Блясък
        using var shine = new SolidBrush(Color.FromArgb(80, 255, 255, 255));
        g.FillEllipse(shine, Pos.X - R*0.5f, Pos.Y - R*0.6f, R*0.7f, R*0.5f);
        // Контур
        g.DrawEllipse(Pens.White, Pos.X - R, Pos.Y - R, 2*R, 2*R);
        // Номер
        if (Number > 0)
        {
            using var f = new Font("Arial", R * 0.65f, FontStyle.Bold);
            string s = Number.ToString();
            var sz = g.MeasureString(s, f);
            g.DrawString(s, f, Brushes.White, Pos.X - sz.Width/2, Pos.Y - sz.Height/2);
        }
    }
}

// ── Form ──────────────────────────────────────────────────────────────────────
public partial class Form1 : Form
{
    readonly List<Ball> balls = new();
    Ball cue;
    readonly System.Windows.Forms.Timer timer  = new System.Windows.Forms.Timer() { Interval = 16 };
    bool   dragging;
    PointF dragStart;
    int    score = 0;
    static readonly Random rnd = new(7);

    static readonly Color[] COLORS =
    {
        Color.Gold, Color.DodgerBlue, Color.Crimson, Color.LimeGreen,
        Color.Orchid, Color.Orange, Color.Cyan, Color.HotPink, Color.YellowGreen
    };

    // Джобове
    static PointF[] Pockets(int W, int H) => new[]
    {
        new PointF(28f, 28f), new PointF(W/2f, 18f), new PointF(W-28f, 28f),
        new PointF(28f, H-28f), new PointF(W/2f, H-18f), new PointF(W-28f, H-28f)
    };
    const float POCKET_R = 20f;

    public Form1()
    {
        Text = "Билярд — Вектори";
        Size = new Size(900, 580);
        DoubleBuffered = true;

        Reset();

        timer.Tick += (_, _) =>
        {
            foreach (var b in balls) b.Update(ClientSize.Width, ClientSize.Height);
            for (int i = 0; i < balls.Count; i++)
                for (int j = i + 1; j < balls.Count; j++)
                    balls[i].Collide(balls[j]);
            CheckPockets();
            Invalidate();
        };
        timer.Start();

        var lblHelp = new Label
        {
            Text = "Влачи от бялата топка за да стреляш  |  R = нова игра",
            Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(20, 60, 20), ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f)
        };
        Controls.Add(lblHelp);
    }

    void Reset()
    {
        balls.Clear(); score = 0;
        // Стандартна пирамида
        float sx = ClientSize.Width * 0.65f, sy = ClientSize.Height / 2f;
        int n = 1, row = 0;
        for (int i = 0; i < 9; i++)
        {
            if (i == n) { row++; n += row + 1; }
            float bx = sx + row * Ball.R * 2.1f;
            float by = sy + (i - n + row + 1 - row/2f) * Ball.R * 2.2f;
            balls.Add(new Ball(bx, by, 0, 0, COLORS[i % COLORS.Length], i + 1));
        }
        cue = new Ball(ClientSize.Width * 0.28f, ClientSize.Height / 2f, 0, 0, Color.WhiteSmoke, 0);
        balls.Insert(0, cue);
    }

    void CheckPockets()
    {
        var pockets = Pockets(ClientSize.Width, ClientSize.Height);
        for (int i = balls.Count - 1; i >= 1; i--)
        {
            foreach (var p in pockets)
            {
                float dx = balls[i].Pos.X - p.X;
                float dy = balls[i].Pos.Y - p.Y;
                if (dx*dx + dy*dy < POCKET_R * POCKET_R)
                {
                    score++;
                    balls.RemoveAt(i);
                    break;
                }
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.R) { Reset(); Invalidate(); }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            dragging  = true;
            dragStart = e.Location;
            this.Focus();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!dragging) return;
        float power = 0.14f;
        cue.Vel = new PointF(
            (dragStart.X - e.X) * power,
            (dragStart.Y - e.Y) * power);
        dragging = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int W = ClientSize.Width, H = ClientSize.Height - 26;

        // Маса
        using var tableBrush = new LinearGradientBrush(
            new Rectangle(0, 0, W, H),
            Color.FromArgb(34, 110, 34),
            Color.FromArgb(20, 80, 20),
            LinearGradientMode.ForwardDiagonal);
        g.FillRectangle(tableBrush, 0, 0, W, H);

        // Рамка
        using var rimBrush = new LinearGradientBrush(new Rectangle(0,0,W,H),
            Color.FromArgb(120,70,20), Color.FromArgb(80,40,10),
            LinearGradientMode.ForwardDiagonal);
        using var rimPen = new Pen(rimBrush, 22);
        g.DrawRectangle(rimPen, 11, 11, W - 22, H - 22);

        // Линия на позиция
        using var linePen = new Pen(Color.FromArgb(80, 255, 255, 255), 1);
        g.DrawLine(linePen, (int)(W * 0.28f), 22, (int)(W * 0.28f), H - 22);

        // Джобове
        foreach (var p in Pockets(W, H))
        {
            g.FillEllipse(Brushes.Black, p.X - POCKET_R, p.Y - POCKET_R, POCKET_R*2, POCKET_R*2);
            using var pp = new Pen(Color.FromArgb(60, 40, 10), 3);
            g.DrawEllipse(pp, p.X - POCKET_R, p.Y - POCKET_R, POCKET_R*2, POCKET_R*2);
        }

        foreach (var b in balls) b.Draw(g);

        // Прицел
        if (dragging)
        {
            var mouse = PointToClient(Cursor.Position);
            float dx = (dragStart.X - mouse.X) * 2.5f;
            float dy = (dragStart.Y - mouse.Y) * 2.5f;
            using var aimPen = new Pen(Color.FromArgb(200, 255, 255, 100), 2)
            {
                DashStyle = DashStyle.Dash
            };
            g.DrawLine(aimPen,
                cue.Pos.X, cue.Pos.Y,
                cue.Pos.X + dx, cue.Pos.Y + dy);
            // Стрелка
            using var arrowPen = new Pen(Color.Yellow, 2.5f)
                { CustomEndCap = new AdjustableArrowCap(5, 5) };
            g.DrawLine(arrowPen,
                cue.Pos.X, cue.Pos.Y,
                cue.Pos.X + dx * 0.9f, cue.Pos.Y + dy * 0.9f);
        }

        // HUD
        using var hf = new Font("Segoe UI", 11f, FontStyle.Bold);
        g.DrawString($"Вкарани: {score}   Останали: {balls.Count - 1}",
            hf, Brushes.White, 10, 6);

        if (balls.Count == 1)
        {
            using var wf = new Font("Segoe UI", 28f, FontStyle.Bold);
            string msg = "БРАВО! Всички вкарани!";
            var sz = g.MeasureString(msg, wf);
            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)),
                W/2f - sz.Width/2 - 10, H/2f - sz.Height/2 - 6,
                sz.Width + 20, sz.Height + 12);
            g.DrawString(msg, wf, Brushes.Gold,
                W/2f - sz.Width/2, H/2f - sz.Height/2);
        }
    }

    static void Main() => Application.Run(new Form1());
}
