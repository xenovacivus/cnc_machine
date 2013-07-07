using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Utilities
{
    /// <summary>
    /// Minimal class mimicking the behavior of a Vector3 for integer types
    /// Currently only instrumented for container purposes
    /// </summary>
    public class Vector3i
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i(Vector3 v)
        {
            this.x = (int)Math.Round (v.X);
            this.y = (int)Math.Round (v.Y);
            this.z = (int)Math.Round (v.Z);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
