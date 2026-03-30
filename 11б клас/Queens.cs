using System;

class Queens
{
    static int n;
    static int[]  col       = new int[100];
    static bool[] colUsed   = new bool[100];
    static bool[] diagUsed1 = new bool[200]; // row - col = const
    static bool[] diagUsed2 = new bool[200]; // row + col = const
    static int count = 0;

    static void Print()
    {
        Console.WriteLine($"Решение {count}:");
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
                Console.Write(col[r] == c ? "Q " : ". ");
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    static void Place(int row)
    {
        if (row >= n) { count++; Print(); return; }

        for (int t = 0; t < n; t++)
        {
            if (!colUsed[t] && !diagUsed1[row - t + n] && !diagUsed2[row + t])
            {
                col[row] = t;
                colUsed[t]         = true;
                diagUsed1[row-t+n] = true;
                diagUsed2[row+t]   = true;

                Place(row + 1);

                colUsed[t]         = false;
                diagUsed1[row-t+n] = false;
                diagUsed2[row+t]   = false;
            }
        }
    }

    static void Main(string[] args)
    {
        Console.Write("Въведи N: ");
        n = int.Parse(Console.ReadLine());
        Place(0);
        Console.WriteLine($"Общо решения за N={n}: {count}");
    }
}
