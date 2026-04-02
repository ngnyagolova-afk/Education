using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuzikalnaKutia
{
    public partial class Form1 : Form
    {
        // ── Масив с 16-те бутона (попълва се в Form_Load) ──────────────
        Button[] buttons;

        // ── 16 начални цвята ────────────────────────────────────────────
        Color[] baseColors = new Color[]
        {
            Color.Red,     Color.Green,   Color.Blue,    Color.Yellow,
            Color.Orange,  Color.Purple,  Color.Cyan,    Color.Magenta,
            Color.Lime,    Color.Pink,    Color.Brown,   Color.Gold,
            Color.Teal,    Color.Indigo,  Color.Salmon,  Color.Gray
        };

        // ── 16 ноти в Hz (До–Си, две октави) ───────────────────────────
        int[] notes = new int[]
        {
            262, 294, 330, 349, 392, 440, 494, 523,
            554, 587, 622, 659, 698, 740, 784, 831
        };

        // ── Рекурсия и комбинации ───────────────────────────────────────
        int[] seq = new int[3];
        List<int[]> allCombinations = new List<int[]>();
        int current = 0;

        // ── Конструктор ─────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
        }

        // ── Form_Load ───────────────────────────────────────────────────
        private void Form1_Load(object sender, EventArgs e)
        {
            // Попълваме масива с бутоните по ред (горе-ляво → долу-дясно)
            buttons = new Button[]
            {
                button1,  button2,  button3,  button4,
                button5,  button6,  button7,  button8,
                button9,  button10, button11, button12,
                button13, button14, button15, button16
            };

            // Задаваме начален цвят и номер на всеки бутон
            for (int i = 0; i < 16; i++)
            {
                buttons[i].BackColor = baseColors[i];
                buttons[i].Text      = (i + 1).ToString();
                buttons[i].ForeColor = Color.White;
                buttons[i].Font      = new Font("Arial", 14, FontStyle.Bold);
            }

            // Генерираме всички C(16,3) = 560 комбинации
            Generate(0, 0);

            lblInfo.Text = $"Общо комбинации: {allCombinations.Count}";
        }

        // ── Generate() — рекурсия C(16,3) ──────────────────────────────
        void Generate(int index, int start)
        {
            // Базов случай: избрали сме 3 елемента → запазваме копие
            if (index >= 3)
            {
                allCombinations.Add((int[])seq.Clone()); // Clone е задължително!
                return;
            }

            for (int i = start; i < 16; i++)
            {
                seq[index] = i;
                Generate(index + 1, i + 1);
            }
        }

        // ── Timer_Tick — анимация + звук ────────────────────────────────
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Спираме след изчерпване на всички 560 комбинации
            if (current >= allCombinations.Count)
            {
                timer1.Stop();
                lblInfo.Text = "Край — изсвирени всички 560 комбинации!";
                return;
            }

            // Връщаме предишните 3 бутона на базовите им цветове
            if (current > 0)
            {
                int[] prev = allCombinations[current - 1];
                for (int p = 0; p < 3; p++)
                    buttons[prev[p]].BackColor = baseColors[prev[p]];
            }

            // Вземаме текущата комбинация
            int[] combo = allCombinations[current];

            // Активираме 3-те бутона (бял цвят)
            for (int p = 0; p < 3; p++)
                buttons[combo[p]].BackColor = Color.White;

            // Обновяваме Label
            lblInfo.Text = $"Комбинация {current + 1} от {allCombinations.Count}";

            // Свирим нотите в отделна нишка (Console.Beep блокира UI!)
            int[] comboSnapshot = (int[])combo.Clone();
            Task.Run(() =>
            {
                foreach (int idx in comboSnapshot)
                    Console.Beep(notes[idx], 250);
            });

            current++;
        }

        // ── Бутон Старт ─────────────────────────────────────────────────
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Нулираме брояча и възстановяваме цветовете
            current = 0;
            for (int i = 0; i < 16; i++)
                buttons[i].BackColor = baseColors[i];

            lblInfo.Text = $"Комбинация 1 от {allCombinations.Count}";
            timer1.Start();
        }

        // ── Бутон Стоп ──────────────────────────────────────────────────
        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            lblInfo.Text = $"Спряно на комбинация {current} от {allCombinations.Count}";
        }
    }
}
