namespace Urm.Theory
{
    /// <summary>
    /// Implements the dimensional emergence function D(ρ) = D₀ + Σ f_i(ρ),
    /// where f_i(ρ) is a sigmoid centered on each ρ_i.
    /// </summary>
    public static class DimensionFunctions
    {
        public static double D(double rho)
        {
            double sum = 0.0;
            foreach (var rho_i in Constants.RhoThresholds)
            {
                sum += Sigmoid(Constants.K * (rho - rho_i));
            }
            return Constants.D0 + sum;// Equation: D(ρ) = D₀ + Σ f_i(ρ)
        }

        // f_i(ρ) = 1 / (1 + e^{-k(ρ - ρ_i)}), a smooth sigmoid transition
        private static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + System.Math.Exp(-x));
        }

        /// <summary>
        /// Defines the core resolution parameter of URM as ρ = 1 / Δx.
        /// </summary>
        public static double RhoFromDeltaX(double deltaX) => 1.0 / deltaX;
    }
}
