using System.Numerics;

namespace URM.Models
{
    /// <summary>
    /// Implements the lens operator L̂_ρ[Ψ_full] → Ψ(x, ρ), projecting the full state
    /// to the resolution-limited subspace H_ρ by downsampling based on D(ρ).
    /// </summary>
    public static class LensOperator
    {
        public static QuantumState Apply(QuantumState fullState, double rho)
        {
            double dRho = URM.Mathematics.DimensionFunctions.D(rho);
            double d0 = URM.Mathematics.Constants.D0;
            double dMax = URM.Mathematics.Constants.DMax;

            // Map effective dimension to a visible fraction of the state
            double fraction = (dRho - d0) / (dMax - d0);      // in [0, 1]
            fraction = Math.Max(0.0, Math.Min(1.0, fraction)); // clamp
            int visibleSize = Math.Max(1, (int)Math.Round(fullState.Amplitudes.Length * fraction));

            var projectedState = new QuantumState(visibleSize);

            // Simple coarse-graining: average adjacent amplitudes (downsampling by 2)
            for (int i = 0; i < visibleSize; i++)
            {
                int i0 = 2 * i;
                int i1 = 2 * i + 1;

                if (i1 < fullState.Amplitudes.Length)
                {
                    projectedState.Amplitudes[i] =
                        (fullState.Amplitudes[i0] + fullState.Amplitudes[i1]) / Math.Sqrt(2);
                }
                else if (i0 < fullState.Amplitudes.Length)
                {
                    projectedState.Amplitudes[i] = fullState.Amplitudes[i0];
                }
                else
                {
                    projectedState.Amplitudes[i] = Complex.Zero;
                }

                projectedState.SpinNetworkEdges[i] =
                    (i0 < fullState.SpinNetworkEdges.Length) ? fullState.SpinNetworkEdges[i0] : i;
            }

            return projectedState;
        }
    }
}

