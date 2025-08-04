using System.Text.Json.Serialization;

namespace Urm.Bec.Models
{
    public class SimulatorSnapshot
    {
        public SimulatorSnapshot(int tick, SimulatorState state, ParticleSnapshot[]? particles = null)
        {
            Tick = tick;
            State = state;
            Particles = particles;
        }
        [JsonPropertyName("second")]
        public int Tick { get; set; }
        public SimulatorState State { get; set; }
        public ParticleSnapshot[]? Particles { get; set; }
    }
}
