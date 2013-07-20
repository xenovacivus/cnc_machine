using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class MathHelper
    {
        public static bool NearlyEqual(float a, float b)
        {
            // IEEE 754 floats have about 7 decimal digits, compare to about 4 (compensate for a few calculation errors)
            float compare_range = 0.0001f;
            float difference = (float)Math.Abs (a - b);
            return (difference < compare_range * a && difference < compare_range * b);
        }
    }
}
