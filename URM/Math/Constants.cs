namespace URM.Mathematics
{
    /// <summary>
    /// Contains fundamental constants and resolution thresholds for URM simulation.
    /// </summary>
    public static class Constants
    {
        public const int D0 = 4;                 // Baseline 3+1
        public const double K = 4.0;             // Sigmoid steepness
        // 7 extra spatial dimensions to reach 11 (4 + 7)
        public static readonly double[] RhoThresholds = { 2.5, 5.5, 7.0, 8.0, 9.0, 9.5, 9.9 };
        public static int DMax => D0 + RhoThresholds.Length;
    }
}
