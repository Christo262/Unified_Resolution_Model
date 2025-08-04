using Urm.Bec.Models;

namespace Urm.Bec.Contracts
{
    public interface ISimulator
    {
        SimulatorState State { get; }
        IReadOnlyList<SimulatorSnapshot> History{ get; }

        event EventHandler<SimulatorState>? OnStateSnapshot;
        void UpdateSnaphsot(SimulatorState state);
        void AddParticleSnapshot(ParticleSnapshot[] particles);
        void Reset();
    }
}
