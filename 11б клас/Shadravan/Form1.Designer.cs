namespace Shadravan;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        pictureBox1 = new PictureBox();
        pictureBox2 = new PictureBox();
        pictureBox3 = new PictureBox();
        pictureBox4 = new PictureBox();
        pictureBox5 = new PictureBox();
        pictureBox6 = new PictureBox();
        pictureBox7 = new PictureBox();
        pictureBox8 = new PictureBox();
        pictureBox9 = new PictureBox();
        timer1      = new System.Windows.Forms.Timer(components);
        btnStart    = new Button();
        lblStatus   = new Label();

        // ── PictureBox-ове 3×3 ──────────────────────────────
        int size = 110, gap = 8, startX = 20, startY = 20;
        var pbs = new PictureBox[]
        {
            pictureBox1, pictureBox2, pictureBox3,
            pictureBox4, pictureBox5, pictureBox6,
            pictureBox7, pictureBox8, pictureBox9
        };
        for (int i = 0; i < 9; i++)
        {
            pbs[i].Size        = new Size(size, size);
            pbs[i].Location    = new Point(startX + (i % 3) * (size + gap),
                                           startY + (i / 3) * (size + gap));
            pbs[i].BorderStyle = BorderStyle.FixedSingle;
            pbs[i].Name        = $"pictureBox{i + 1}";
        }

        // ── Timer ────────────────────────────────────────────
        timer1.Interval = 1000;
        timer1.Tick    += new EventHandler(timer1_Tick);

        // ── Бутон Старт ──────────────────────────────────────
        btnStart.Text      = "▶  Старт";
        btnStart.Size      = new Size(120, 38);
        btnStart.Location  = new Point(startX + (size + gap), startY + 3 * (size + gap) + 12);
        btnStart.Font      = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnStart.BackColor = Color.FromArgb(0, 120, 212);
        btnStart.ForeColor = Color.White;
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.FlatAppearance.BorderSize = 0;
        btnStart.Click    += new EventHandler(btnStart_Click);

        // ── Label статус ─────────────────────────────────────
        lblStatus.Text      = "Готов. Натисни Старт.";
        lblStatus.Size      = new Size(370, 24);
        lblStatus.Location  = new Point(startX, btnStart.Bottom + 10);
        lblStatus.Font      = new Font("Segoe UI", 10F);
        lblStatus.ForeColor = Color.FromArgb(50, 50, 50);
        lblStatus.TextAlign = ContentAlignment.MiddleCenter;

        // ── Form ─────────────────────────────────────────────
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize    = new Size(
            startX * 2 + 3 * size + 2 * gap,
            lblStatus.Bottom + 16);
        Text          = "Шадраван — C(9,4) = 126 комбинации";
        BackColor     = Color.FromArgb(245, 245, 248);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox   = false;
        StartPosition = FormStartPosition.CenterScreen;
        Load         += new EventHandler(Form1_Load);

        // ── Добавяме контролите ──────────────────────────────
        foreach (var pb in pbs) Controls.Add(pb);
        Controls.Add(btnStart);
        Controls.Add(lblStatus);
    }

    #endregion

    private PictureBox pictureBox1, pictureBox2, pictureBox3;
    private PictureBox pictureBox4, pictureBox5, pictureBox6;
    private PictureBox pictureBox7, pictureBox8, pictureBox9;
    private System.Windows.Forms.Timer timer1;
    private Button btnStart;
    private Label  lblStatus;
}
