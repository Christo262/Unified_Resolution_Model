namespace Urm.Bec.Models
{
    public class ParticleSnapshot
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public double vx { get; set; }
        public double vy { get; set; }
        public double vz { get; set; }

        public double sx { get; set; }
        public double sy { get; set; }
        public double sz { get; set; }

        // Optional helper
        public System.Numerics.Vector3 Position => new((float)x, (float)y, (float)z);
        public System.Numerics.Vector3 Velocity => new((float)vx, (float)vy, (float)vz);
    }

}
