using System.Numerics;

namespace URM.Models
{
    /// <summary>
    /// Represents a simplified coarse-grainable quantum state with amplitudes and spin network edges.
    /// </summary>
    public class QuantumState
    {
        public Complex[] Amplitudes { get; set; }
        public int[] SpinNetworkEdges { get; set; }

        public QuantumState(int size)
        {
            Amplitudes = new Complex[size];
            SpinNetworkEdges = new int[size];
        }
    }
}
