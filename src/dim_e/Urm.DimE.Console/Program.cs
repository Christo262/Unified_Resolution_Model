
// ----------------- Example usage -----------------
// This demonstrates how the resolution lens L̂_ρ is applied across multiple values of ρ.
using System;
using System.Numerics;
using Urm.Models;
using Urm.Theory;

Console.WriteLine("Unified Resolution Model Simulation:\n");
Console.WriteLine("{0,-5} | {1,-8} | Size | Projected Edges", "ρ", "D(ρ)");
Console.WriteLine(new string('-', 60));

var fullState = new QuantumState(20);
var random = new Random();

for (int i = 0; i < fullState.Amplitudes.Length; i++)
{
    fullState.Amplitudes[i] = new Complex(NextGaussian(random), NextGaussian(random)) / Math.Sqrt(2);
    fullState.SpinNetworkEdges[i] = i;
}

for (double rho = 0; rho <= 12; rho += 0.5)
{
    double d = DimensionFunctions.D(rho);
    var projected = LensOperator.Apply(fullState, rho);

    string visiblePart = string.Join(",", projected.SpinNetworkEdges);
    Console.WriteLine("{0,4:0.0} | {1,6:0.00}   | {2,4} | {3}", rho, d, projected.Amplitudes.Length, visiblePart);
}

static double NextGaussian(Random rand)
{
    // Box-Muller
    double u1 = 1.0 - rand.NextDouble();
    double u2 = 1.0 - rand.NextDouble();
    return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
}