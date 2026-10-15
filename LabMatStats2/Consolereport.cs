using System.Globalization;
using System.Text;
using IntervalEstimationLab.Models;

namespace IntervalEstimationLab.Services;

/// <summary>Форматований вивід у консоль та експорт у CSV.</summary>
public static class ConsoleReport
{
    private static readonly CultureInfo Ua = CultureInfo.GetCultureInfo("uk-UA");

    public static string F(double v, int digits = 4) => v.ToString("F" + digits, Ua);

    public static void Header(string text)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine(text);
        Console.WriteLine(new string('=', 70));
    }

    /// <summary>Завдання 1: вихідний ряд і перехід до дискретного.</summary>
    public static void PrintSeries(IReadOnlyList<IntervalRow> rows)
    {
        Console.WriteLine($"{"Інтервал",-14}{"n_i",5}{"x_i",9}");
        foreach (var r in rows)
            Console.WriteLine($"{F(r.Lower, 1) + "–" + F(r.Upper, 1),-14}{r.Frequency,5}{F(r.Mid, 1),9}");
        Console.WriteLine($"{"Σ",-14}{rows.Sum(r => r.Frequency),5}");
    }

    /// <summary>Завдання 2: гістограма (текстова) і точки полігона.</summary>
    public static void PrintHistogram(IReadOnlyList<IntervalRow> rows)
    {
        int max = rows.Max(r => r.Frequency);
        const int width = 40;

        Console.WriteLine("Гістограма частот:");
        foreach (var r in rows)
        {
            int len = (int)Math.Round((double)r.Frequency / max * width);
            Console.WriteLine($"{F(r.Lower, 1)}–{F(r.Upper, 1)} | {new string('█', len)} {r.Frequency}");
        }

        Console.WriteLine();
        Console.WriteLine("Точки полігона частот (x_i; n_i):");
        Console.WriteLine(string.Join("  ", rows.Select(r => $"({F(r.Mid, 1)}; {r.Frequency})")));
    }

    /// <summary>Завдання 3: розрахункова таблиця.</summary>
    public static void PrintCalcTable(IReadOnlyList<IntervalRow> rows, PointEstimates p)
    {
        Console.WriteLine($"{"x_i",7}{"n_i",5}{"x_i·n_i",11}{"(x_i-x̄)²",12}{"n_i(x_i-x̄)²",15}");
        foreach (var r in rows)
        {
            double dev2 = Math.Pow(r.Mid - p.Mean, 2);
            Console.WriteLine($"{F(r.Mid, 1),7}{r.Frequency,5}{F(r.Mid * r.Frequency, 2),11}{F(dev2, 6),12}{F(dev2 * r.Frequency, 6),15}");
        }
        Console.WriteLine();
        Console.WriteLine($"x̄  = {F(p.Mean)}");
        Console.WriteLine($"d̄  = {F(p.BiasedVariance)}   (зміщена)");
        Console.WriteLine($"s² = {F(p.CorrectedVariance)}   (виправлена)");
        Console.WriteLine($"s  = {F(p.CorrectedStd)}");
    }

    public static void ExportCsv(string path, IReadOnlyList<IntervalRow> rows)
    {
        var sb = new StringBuilder("lower;upper;mid;frequency\n");
        foreach (var r in rows)
            sb.AppendLine(string.Join(';',
                r.Lower.ToString(Ua), r.Upper.ToString(Ua), r.Mid.ToString(Ua), r.Frequency));
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}