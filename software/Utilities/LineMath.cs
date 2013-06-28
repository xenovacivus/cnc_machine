using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;

namespace Utilities
{
    public class LineMath
    {
        public static float DistanceTo(Vector2 line1, Vector2 line2, Vector2 a)
        {
            float r = Math.Min(Length(line1, a), Length(line2, a));

            float length = LineMath.Length(line1, line2);
            if (length == 0)
                return r;

            Vector2 n = LineMath.Scale(LineMath.Sub(line1, line2), 1.0f / length);
            float d1 = LineMath.Mult(line1, n) - LineMath.Mult(a, n);
            n = new Vector2(-n.Y, n.X);
            float d2 = LineMath.Mult(line1, n) - LineMath.Mult(a, n);
            
            if (d1 <= length && d1 >= 0)
            {
                return Math.Min(Math.Abs(d2), r);
            }
            return r;
        }
        public static Vector2 Scale(Vector2 a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }
        public static Vector2 Sub(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }
        public static float Mult(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static float Length(Vector2 a, Vector2 b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        internal static Vector2 Add(Vector2 actualPosition, Vector2 pointF)
        {
            return new Vector2(actualPosition.X + pointF.X, actualPosition.Y + pointF.Y);
        }
    }
}
