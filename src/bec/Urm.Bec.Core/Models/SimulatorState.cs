using System.Text.Json.Serialization;

namespace Urm.Bec.Models
{
    public enum TrapShape
    {
        Sphere,
        //Ellipsoid,
        //Box
    }

    public enum GasType
    {
        Rb87,
        Na23,
        Li7
    }
    public class SimulatorState
    {
        public double TargetCoherence { get; set; }
        public TrapShape TrapShape { get; set; }
        public string Gas { get; set; } = GasType.Rb87.ToString();
        public int ParticleCount { get; set; }

        [JsonPropertyName("rms")]
        public double RMS { get; set; }
        [JsonPropertyName("ke")]
        public double KineticEnergy { get; set; }
        [JsonPropertyName("pe")]
        public double PotentialEnergy { get; set; }
        [JsonPropertyName("frac")]
        public double CondensateFraction { get; set; }
        [JsonPropertyName("density")]
        public double CentralDensity { get; set; }
        [JsonPropertyName("rate")]
        public double ExpansionRate { get; set; }
        [JsonPropertyName("temp")]
        public double Temperature { get; set; }
        [JsonPropertyName("gasMass")]
        public double GasMass { get; set; }
    }

    public class SimulatorStateSettings
    {
        public TrapShape TrapShape { get; set; }
        public GasType Gas { get; set; }
        public int ParticleCount { get; set; } = 300;
    }
}
