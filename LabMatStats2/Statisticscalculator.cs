using IntervalEstimationLab.Models;
using MathNet.Numerics.Distributions;

namespace IntervalEstimationLab.Services;

/// <summary>Уся математика лабораторної. Без виводу в консоль — лише обчислення.</summary>
public static class StatisticsCalculator
{
    public static PointEstimates ComputePointEstimates(IReadOnlyList<IntervalRow> rows)
    {
        int n = rows.Sum(r => r.Frequency);

        double mean = rows.Sum(r => r.Mid * r.Frequency) / n;                           // (2.3)
        double biased = rows.Sum(r => r.Frequency * Math.Pow(r.Mid - mean, 2)) / n;     // (2.4)
        double corrected = biased * n / (n - 1);                                        // (2.5)
        double std = Math.Sqrt(corrected);                                              // (2.6)

        return new PointEstimates(n, mean, biased, corrected, std);
    }

    /// <summary>Двобічна критична точка Стьюдента t(α/2; k).</summary>
    public static double StudentQuantile(double gamma, int k)
        => StudentT.InvCDF(0, 1, k, 1 - (1 - gamma) / 2);

    /// <summary>Двобічна квантиль нормального розподілу (для порівняння, формула 2.15).</summary>
    public static double NormalQuantile(double gamma)
        => Normal.InvCDF(0, 1, 1 - (1 - gamma) / 2);

    /// <summary>Формули (2.10), (2.11).</summary>
    public static MeanInterval ConfidenceIntervalForMean(PointEstimates p, double gamma)
    {
        double t = StudentQuantile(gamma, p.N - 1);
        double delta = t * p.CorrectedStd / Math.Sqrt(p.N);
        return new MeanInterval(gamma, t, delta, p.Mean - delta, p.Mean + delta);
    }

    /// <summary>Точність за нормальним наближенням (2.15).</summary>
    public static double DeltaNormalApprox(PointEstimates p, double gamma)
        => NormalQuantile(gamma) * p.CorrectedStd / Math.Sqrt(p.N);

    /// <summary>Формули (2.13), (2.14).</summary>
    public static VarianceInterval ConfidenceIntervalForVariance(PointEstimates p, double gamma)
    {
        int k = p.N - 1;
        double alpha = 1 - gamma;

        // P(χ² > χ1²) = α/2     -> права (більша) точка
        double chi1 = ChiSquared.InvCDF(k, 1 - alpha / 2);
        // P(χ² > χ2²) = 1-α/2   -> ліва (менша) точка
        double chi2 = ChiSquared.InvCDF(k, alpha / 2);

        double varLow = k * p.CorrectedVariance / chi1;   // більше χ² у знаменнику -> ліва межа
        double varUp = k * p.CorrectedVariance / chi2;

        return new VarianceInterval(gamma, chi1, chi2, varLow, varUp,
                                    Math.Sqrt(varLow), Math.Sqrt(varUp));
    }

    /// <summary>
    /// Мінімальний n, при якому δ(n) ≤ targetDelta (s вважаємо незмінним).
    /// </summary>
    public static int RequiredSampleSize(PointEstimates p, double gamma, double targetDelta)
    {
        int n = 2;
        while (StudentQuantile(gamma, n - 1) * p.CorrectedStd / Math.Sqrt(n) > targetDelta)
            n++;
        return n;
    }
}