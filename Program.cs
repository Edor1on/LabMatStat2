using IntervalEstimationLab.Models;
using IntervalEstimationLab.Services;
using static IntervalEstimationLab.Services.ConsoleReport;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------- Вхідні дані: варіант 6 ----------
const double gamma = 0.95;
const double h = 0.2;
double start = 9.2;
int[] freq = { 4, 8, 15, 18, 36, 10, 6, 2 };

var rows = freq
    .Select((f, i) => new IntervalRow(start + i * h, start + (i + 1) * h, f))
    .ToList();

// ---------- Завдання 1 ----------
Header("Завдання 1. Інтервальний ряд і перехід до дискретного");
int n = rows.Sum(r => r.Frequency);
if (n != 99) throw new InvalidOperationException($"Σn_i = {n}, а має бути 99");
PrintSeries(rows);

// ---------- Завдання 2 ----------
Header("Завдання 2. Гістограма та полігон частот");
PrintHistogram(rows);
ExportCsv("series.csv", rows);
Console.WriteLine("\nДані збережено в series.csv");
Console.WriteLine("Гістограма: " + ChartExporter.SaveHistogram(rows));
Console.WriteLine("Полігон:    " + ChartExporter.SavePolygon(rows));

// ---------- Завдання 3 ----------
Header("Завдання 3. Точкові оцінки");
var p = StatisticsCalculator.ComputePointEstimates(rows);
PrintCalcTable(rows, p);

// ---------- Завдання 4 ----------
Header($"Завдання 4. Довірчий інтервал для m, γ = {F(gamma, 2)}");
var ci = StatisticsCalculator.ConfidenceIntervalForMean(p, gamma);
Console.WriteLine($"k = {p.N - 1},  t_γ = {F(ci.Quantile)}");
Console.WriteLine($"δ = t_γ·s/√n = {F(ci.Delta)}");
Console.WriteLine($"{F(ci.Lower)} < m < {F(ci.Upper)}");

// ---------- Завдання 5 ----------
Header($"Завдання 5. Довірчі інтервали для σ² та σ, γ = {F(gamma, 2)}");
var vi = StatisticsCalculator.ConfidenceIntervalForVariance(p, gamma);
Console.WriteLine($"χ1² (права) = {F(vi.Chi1)},  χ2² (ліва) = {F(vi.Chi2)}");
Console.WriteLine($"{F(vi.VarLower)} < σ² < {F(vi.VarUpper)}");
Console.WriteLine($"{F(vi.StdLower)} < σ  < {F(vi.StdUpper)}");

// ---------- Завдання 6 ----------
Header("Завдання 6. Відносна точність і ширина інтервалів");
double eps = ci.Delta / p.Mean * 100;
double widthM = ci.Upper - ci.Lower;
double widthVar = vi.VarUpper - vi.VarLower;
double widthStd = vi.StdUpper - vi.StdLower;
Console.WriteLine($"ε = δ/x̄·100% = {F(eps)} %");
Console.WriteLine($"Ширина інтервалу для m  : {F(widthM)}");
Console.WriteLine($"Ширина інтервалу для σ² : {F(widthVar)}");
Console.WriteLine($"Ширина інтервалу для σ  : {F(widthStd)}");
double leftArm = p.CorrectedVariance - vi.VarLower;
double rightArm = vi.VarUpper - p.CorrectedVariance;
Console.WriteLine($"\ns² - ліва межа = {F(leftArm)},  права межа - s² = {F(rightArm)}");
Console.WriteLine("Інтервал для σ² несиметричний, бо розподіл χ² асиметричний (правий «хвіст» довший),");
Console.WriteLine("тому ліве плече коротше за праве.");

// ---------- Завдання 7 ----------
Header("Завдання 7. Вплив надійності, t-розподіл vs нормальне наближення");
Console.WriteLine($"{"γ",6}{"t_γ",9}{"z_γ",9}{"δ (t)",10}{"δ (z)",10}{"Розбіжн.,%",12}   Інтервал для m");
double[] gammas = { 0.90, 0.95, 0.99 };
var tableRows = new List<(double g, MeanInterval ci)>();
foreach (var g in gammas)
{
    var c = StatisticsCalculator.ConfidenceIntervalForMean(p, g);
    double z = StatisticsCalculator.NormalQuantile(g);
    double dz = StatisticsCalculator.DeltaNormalApprox(p, g);
    double diff = Math.Abs(c.Delta - dz) / c.Delta * 100;
    tableRows.Add((g, c));
    Console.WriteLine($"{F(g, 2),6}{F(c.Quantile),9}{F(z),9}{F(c.Delta),10}{F(dz),10}{F(diff, 2),12}   ({F(c.Lower)}; {F(c.Upper)})");
}

// ---------- Завдання 8 ----------
Header("Завдання 8. Обсяг вибірки для покращення точності вдвічі");
int nNeeded = StatisticsCalculator.RequiredSampleSize(p, gamma, ci.Delta / 2);
Console.WriteLine($"За формулою (2.11): δ ~ 1/√n  =>  n' = 4·n = {4 * p.N}");
Console.WriteLine($"Точний перебір з урахуванням зміни t_γ: n' = {nNeeded}");

// ---------- Завдання 9 ----------
Header("Завдання 9. Висновки");
Console.WriteLine($"1) При γ = {F(gamma, 2)}: {F(ci.Lower)} < m < {F(ci.Upper)} мкФ;");
Console.WriteLine($"   {F(vi.VarLower)} < σ² < {F(vi.VarUpper)}.");
Console.WriteLine("   Тобто при багаторазовому повторенні вибірок близько 95 % побудованих інтервалів накриють");
Console.WriteLine("   справжні значення параметрів (випадкові межі інтервалу, а не сам параметр).");
Console.WriteLine($"2) Зі зростанням γ від 0,90 до 0,99 δ збільшується: {F(tableRows[0].ci.Delta)} → {F(tableRows[2].ci.Delta)}, інтервал розширюється.");
Console.WriteLine($"3) Зі збільшенням n у 4 рази δ зменшується вдвічі (n' ≈ {4 * p.N}).");
Console.WriteLine("4) Для n = 99 нормальне наближення дає майже той самий результат, що й розподіл Стьюдента.");
