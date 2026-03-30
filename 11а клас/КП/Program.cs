using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class PizzaShop
{
    static async Task Main()
    {
        int chefs = 2;
        int couriers = 3;
        int capacity = 5;
        int totalPizzas = 20;

        var buffer = new BlockingCollection<int>(capacity);
        var cts = new CancellationTokenSource();

        // Готвачи (producers)
        Task[] chefTasks = new Task[chefs];
        for (int i = 0; i < chefs; i++)
        {
            int chefId = i + 1;
            chefTasks[i] = Task.Run(() => Chef(chefId, buffer, totalPizzas, cts.Token));
        }

        // Куриери (consumers)
        Task[] courierTasks = new Task[couriers];
        for (int i = 0; i < couriers; i++)
        {
            int courierId = i + 1;
            courierTasks[i] = Task.Run(() => Courier(courierId, buffer, cts.Token));
        }

        // Изчакваме готвачите да завършат (всички пици произведени)
        await Task.WhenAll(chefTasks);

        // Сигнализираме, че няма да се добавят повече елементи
        buffer.CompleteAdding();

        // Изчакваме куриерите да вземат останалите пици
        await Task.WhenAll(courierTasks);

        Console.WriteLine("Всички пици са доставени. Край.");
    }

    static void Chef(int id, BlockingCollection<int> buffer, int totalPizzas, CancellationToken token)
    {
        // Разпределяме номерата на пиците между готвачите
        for (int pizza = id; pizza <= totalPizzas; pizza += 1) // прост вариант - може да се прави по-добре
        {
            if (token.IsCancellationRequested) break;

            // Симулираме време за приготвяне
            Thread.Sleep(new Random().Next(200, 800));
            try
            {
                buffer.Add(pizza, token); // ако буферът е пълен, добавянето ще блокира
                Console.WriteLine($"[Готвач {id}] Приготви пица #{pizza} (буфер: {buffer.Count}/{buffer.BoundedCapacity})");
            }
            catch (InvalidOperationException)
            {
                // buffer е CompleteAdding
                break;
            }
        }
    }

    static void Courier(int id, BlockingCollection<int> buffer, CancellationToken token)
    {
        foreach (var pizza in buffer.GetConsumingEnumerable(token))
        {
            // Взема пица
            Console.WriteLine($"\t[Куриер {id}] Взе пица #{pizza} (буфер: {buffer.Count}/{buffer.BoundedCapacity})");
            // Симулираме доставка
            Thread.Sleep(new Random().Next(300, 1000));
            Console.WriteLine($"\t[Куриер {id}] Достави пица #{pizza}");
        }
    }
}

