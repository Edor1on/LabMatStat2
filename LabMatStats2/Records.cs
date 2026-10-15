namespace IntervalEstimationLab.Models;

/// <summary>Один рядок інтервального ряду: [Lower; Upper) та частота.</summary>
public record IntervalRow(double Lower, double Upper, int Frequency)
{
    public double Mid => (Lower + Upper) / 2.0;   // формула (2.2)
    public double Width => Upper - Lower;
}

/// <summary>Точкові оцінки (завдання 3).</summary>
public record PointEstimates(
    int N,
    double Mean,               // x̄
    double BiasedVariance,     // d̄
    double CorrectedVariance,  // s²
    double CorrectedStd);      // s

/// <summary>Довірчий інтервал для m (завдання 4).</summary>
public record MeanInterval(double Gamma, double Quantile, double Delta, double Lower, double Upper);

/// <summary>Довірчі інтервали для σ² та σ (завдання 5).</summary>
public record VarianceInterval(
    double Gamma,
    double Chi1,   // права критична точка χ²(α/2; k)
    double Chi2,   // ліва критична точка χ²(1-α/2; k)
    double VarLower, double VarUpper,
    double StdLower, double StdUpper);