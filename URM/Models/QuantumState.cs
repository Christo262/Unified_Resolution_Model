using System.Numerics;

namespace URM.Models
{
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
