namespace URM.Models
{
    public static class LensOperator
    {
        public static QuantumState Apply(QuantumState fullState, double rho)
        {
            int visibleSize = Math.Min(fullState.Amplitudes.Length, (int)(URM.Mathematics.DimensionFunctions.D(rho) * 2));
            var projectedState = new QuantumState(visibleSize);

            for (int i = 0; i < visibleSize; i++)
            {
                // Coarse-graining: average adjacent amplitudes
                if (2 * i + 1 < fullState.Amplitudes.Length)
                {
                    projectedState.Amplitudes[i] =
                        (fullState.Amplitudes[2 * i] + fullState.Amplitudes[2 * i + 1]) / Math.Sqrt(2);
                }
                else
                {
                    projectedState.Amplitudes[i] = fullState.Amplitudes[i];
                }

                projectedState.SpinNetworkEdges[i] = fullState.SpinNetworkEdges[i];
            }

            return projectedState;
        }
    }
}
