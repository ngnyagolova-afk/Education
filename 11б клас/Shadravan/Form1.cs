namespace Shadravan;

public partial class Form1 : Form
{
    // ── Полета ───────────────────────────────────────────────
    PictureBox[] pictureBoxes = null!;

    Color[] colors =
    {
        Color.FromArgb(220,  60,  60),   // 0 червено
        Color.FromArgb( 50, 160,  50),   // 1 зелено
        Color.FromArgb( 50, 100, 220),   // 2 синьо
        Color.FromArgb(220, 200,  40),   // 3 жълто
        Color.FromArgb(220, 120,  40),   // 4 оранжево
        Color.FromArgb(140,  50, 200),   // 5 лилаво
        Color.FromArgb( 40, 190, 190),   // 6 циан
        Color.FromArgb(200,  50, 180),   // 7 магента
        Color.FromArgb(100, 100, 100),   // 8 сиво
    };

    int[]        seq             = new int[4];
    List<int[]>  allCombinations = new();
    int          current         = 0;

    // ── Конструктор ──────────────────────────────────────────
    public Form1()
    {
        InitializeComponent();
    }

    // ── Form_Load ─────────────────────────────────────────────
    private void Form1_Load(object sender, EventArgs e)
    {
        // Свързваме масива с контролите от Designer-а
        pictureBoxes = new PictureBox[]
        {
            pictureBox1, pictureBox2, pictureBox3,
            pictureBox4, pictureBox5, pictureBox6,
            pictureBox7, pictureBox8, pictureBox9
        };

        // Начални цветове — всяко квадратче с различен цвят
        for (int i = 0; i < 9; i++)
            pictureBoxes[i].BackColor = colors[i];

        // Генерираме всички C(9,4) = 126 комбинации веднъж
        Generate(0, 0);

        lblStatus.Text = $"Готов. Комбинации: {allCombinations.Count}. Натисни Старт.";
    }

    // ── Генериране на комбинации C(9,4) ─────────────────────
    void Generate(int index, int start)
    {
        if (index >= 4)
        {
            allCombinations.Add((int[])seq.Clone());   // Clone() е задължително!
            return;
        }
        for (int i = start; i < 9; i++)
        {
            seq[index] = i;
            Generate(index + 1, i + 1);
        }
    }

    // ── Timer_Tick — прилагаме следващата комбинация ─────────
    private void timer1_Tick(object sender, EventArgs e)
    {
        if (current >= allCombinations.Count)
        {
            timer1.Stop();
            btnStart.Text      = "▶  Старт";
            btnStart.BackColor = Color.FromArgb(0, 120, 212);
            lblStatus.Text     = "Готово! Всичките 126 комбинации са показани.";
            return;
        }

        int[] combo = allCombinations[current];

        // Ротираме цветовете на избраните 4 позиции циклично
        Color first = pictureBoxes[combo[0]].BackColor;
        for (int p = 0; p < 3; p++)
            pictureBoxes[combo[p]].BackColor = pictureBoxes[combo[p + 1]].BackColor;
        pictureBoxes[combo[3]].BackColor = first;

        current++;
        lblStatus.Text = $"Смяна {current} / {allCombinations.Count}  " +
                         $"  позиции: [{combo[0]}, {combo[1]}, {combo[2]}, {combo[3]}]";
    }

    // ── Бутон Старт ──────────────────────────────────────────
    private void btnStart_Click(object sender, EventArgs e)
    {
        if (timer1.Enabled)
        {
            // Пауза
            timer1.Stop();
            btnStart.Text      = "▶  Продължи";
            btnStart.BackColor = Color.FromArgb(0, 150, 80);
        }
        else
        {
            // Ако сме свършили — рестартираме от начало
            if (current >= allCombinations.Count)
            {
                current = 0;
                for (int i = 0; i < 9; i++)
                    pictureBoxes[i].BackColor = colors[i];
            }

            timer1.Start();
            btnStart.Text      = "⏸  Пауза";
            btnStart.BackColor = Color.FromArgb(180, 80, 0);
        }
    }
}
