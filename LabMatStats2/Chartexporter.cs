using IntervalEstimationLab.Models;
using ScottPlot;

namespace IntervalEstimationLab.Services;

/// <summary>Побудова гістограми та полігона частот і збереження у PNG (ScottPlot 5).</summary>
public static class ChartExporter
{
    private static readonly Color BarColor = Color.FromHex("#4C72B0");
    private static readonly Color LineColor = Color.FromHex("#C44E52");

    /// <summary>Завдання 2: гістограма частот.</summary>
    public static string SaveHistogram(IReadOnlyList<IntervalRow> rows, string path = "histogram.png")
    {
        double h = rows[0].Width;

        var bars = rows.Select(r => new Bar
        {
            Position = r.Mid,
            Value = r.Frequency,
            Size = h * 0.98,          // майже без зазорів між стовпцями
            FillColor = BarColor
        }).ToList();

        var plt = new Plot();
        plt.Add.Bars(bars);
        ApplyCommon(plt, rows, "Гістограма частот", h);
        plt.SavePng(path, 900, 600);
        return Path.GetFullPath(path);
    }

    /// <summary>Завдання 2: полігон частот (ламана через точки (x_i; n_i), замкнена нулями).</summary>
    public static string SavePolygon(IReadOnlyList<IntervalRow> rows, string path = "polygon.png")
    {
        double h = rows[0].Width;

        var xs = new List<double> { rows[0].Mid - h };
        var ys = new List<double> { 0 };
        xs.AddRange(rows.Select(r => r.Mid));
        ys.AddRange(rows.Select(r => (double)r.Frequency));
        xs.Add(rows[^1].Mid + h);
        ys.Add(0);

        var plt = new Plot();
        var sc = plt.Add.Scatter(xs.ToArray(), ys.ToArray());
        sc.Color = LineColor;
        sc.LineWidth = 2;
        sc.MarkerSize = 8;

        ApplyCommon(plt, rows, "Полігон частот", h);
        plt.SavePng(path, 900, 600);
        return Path.GetFullPath(path);
    }

    private static void ApplyCommon(Plot plt, IReadOnlyList<IntervalRow> rows, string title, double h)
    {
        plt.Title(title);
        plt.XLabel("Ємність конденсаторів, мкФ");
        plt.YLabel("Частота n_i");

        // підписи осі X — середини інтервалів
        plt.Axes.Bottom.SetTicks(
            rows.Select(r => r.Mid).ToArray(),
            rows.Select(r => r.Mid.ToString("F1")).ToArray());

        double maxFreq = rows.Max(r => r.Frequency);
        plt.Axes.SetLimitsY(0, maxFreq * 1.15);
        plt.Axes.SetLimitsX(rows[0].Mid - 1.5 * h, rows[^1].Mid + 1.5 * h);
    }
}