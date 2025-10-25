
using System.Diagnostics;
using WadMaker.Tests.Performance;

Measure(() => new StressTestMap().Create(9000, 640));
//PromptTest();

void PromptTest()
{
    int pillars = 10;
    while (true)
    {
        int size = (int)((Math.Sqrt(pillars) + 2) * 256);
        
        Console.Write($"Generating room with {pillars} pillars ({size}x{size})....");
        Measure(() => new StressTestMap().Create(size, pillars));

        Console.WriteLine("Continue?");
        if (Console.ReadKey().Key != ConsoleKey.Y)
            break;

        Console.WriteLine();
        pillars *= 2;
    }
}

void Measure(Action a)
{
    var t = new Stopwatch();
    t.Start();
    a();
    t.Stop();

    Console.WriteLine($"Finished in #{t.Elapsed.TotalSeconds.ToString("0.00")} seconds");
}