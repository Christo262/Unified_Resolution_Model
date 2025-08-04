using Urm.Bec.Contracts;
using Urm.Bec.Models;

namespace Urm.Bec.Web.Services
{
    internal class Simulator : ISimulator
    {
        private readonly SimulatorState _state;
        private readonly List<SimulatorSnapshot> _history;
        public Simulator()
        {
            _state = new SimulatorState
            {
                TargetCoherence = 0.0,
                RMS = 0.0,
                TrapShape = TrapShape.Sphere
            };
            _history = new List<SimulatorSnapshot>();
        }

        public SimulatorState State => _state;

        public event EventHandler<SimulatorState>? OnStateSnapshot;

        public void UpdateSnaphsot(SimulatorState state)
        {
            state.TrapShape = _state.TrapShape;
            state.Gas = _state.Gas;
            state.TargetCoherence = _state.TargetCoherence;
            state.ParticleCount = _state.ParticleCount;
            var snapshot = new SimulatorSnapshot(_history.Count, state);
            _history.Add(snapshot);
            OnStateSnapshot?.Invoke(this, state);
        }

        public void AddParticleSnapshot(ParticleSnapshot[] particles)
        {
            var h = _history.LastOrDefault();
            if(h != null)
            {
                h.Particles = particles;
            }
        }

        public void Reset()
        {
            _history.Clear();
            _state.TargetCoherence = 0.0;
        }

        public IReadOnlyList<SimulatorSnapshot> History => _history;
    }
}
