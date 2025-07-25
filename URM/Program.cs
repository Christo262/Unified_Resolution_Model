using System.Numerics;
using URM.Mathematics;
using URM.Models;

Console.WriteLine("Unified Resolution Model Simulation:\n");
Console.WriteLine("{0,-5} | {1,-8} | Projected State", "ρ", "D(ρ)");
Console.WriteLine(new string('-', 40));

QuantumState fullState = new QuantumState(20);
Random random = new Random();
for (int i = 0; i < 20; i++)
{
    fullState.Amplitudes[i] = new Complex(NextGaussian(random), NextGaussian(random)) / Math.Sqrt(2);
    fullState.SpinNetworkEdges[i] = i;
}

for (double rho = 0; rho <= 10; rho += 0.5)
{
    double d = DimensionFunctions.D(rho);
    QuantumState projected = LensOperator.Apply(fullState, rho);
    string visiblePart = string.Join("", projected.SpinNetworkEdges);
    Console.WriteLine("{0,4:0.0}  | {1,6:0.00}   | {2}", rho, d, visiblePart);
}

static double NextGaussian(Random rand)
{
    // Box-Muller transform
    double u1 = 1.0 - rand.NextDouble();
    double u2 = 1.0 - rand.NextDouble();
    return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
}