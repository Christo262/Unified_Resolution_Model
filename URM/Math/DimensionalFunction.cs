namespace URM.Mathematics
{
    public static class DimensionFunctions
    {
        public static double D(double rho)
        {
            return 4 + Sigmoid(rho - 2.5) + Sigmoid(rho - 5.5);
        }

        private static double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-4 * x));
        }

        public static double RhoFromEnergyScale(double energyGeV)
        {
            const double PlanckEnergyGeV = 1.22e19;
            return Math.Log10(energyGeV / PlanckEnergyGeV);
        }
    }
}
