using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robot
{
    public class Point3
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public Point3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public int Length(Point3 p)
        {
            Point3 d = new Point3(p.x - x, p.y - y, p.z - z);
            return (int)Math.Sqrt(d.x * d.x + d.y * d.y + d.z * d.z);
        }
        public bool Equals(Point3 p)
        {
            return ((p.x == x) && (p.y == y) && (p.z == z));
        }

        internal Point3 Add(Point3 p)
        {
            return new Point3(p.x + x, p.y + y, p.z + z);
        }
    }
    public class Point3F
    {
        public Point3F(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x;
        public float y;
        public float z;

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        /// <summary>
        /// Returns the length to another Point3F
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public float Length(Point3F p)
        {
            Point3F d = new Point3F(p.x - x, p.y - y, p.z - z);
            return (float)Math.Sqrt(d.x * d.x + d.y * d.y + d.z * d.z);
        }

        public bool Equals(Point3F p)
        {
            return ((p.x == x) && (p.y == y) && (p.z == z));
        }

        internal Point3F Normalize()
        {
            float length = this.Length(new Point3F (0, 0, 0));
            return new Point3F(x / length, y / length, z / length);
        }

        internal float Length()
        {
            return this.Length(new Point3F(0, 0, 0));
        }
    }

    public abstract class IHardware
    {
        public abstract Point3F GetPosition();

        public EventHandler onRobotReady;

        //public abstract void SetToGo(Point3F p, float tool_speed);
        public abstract void GoTo(Point3F p, float tool_speed);
        //public abstract float GetProgress();
        public abstract bool IsMoving();
        public abstract bool Ready();
        public abstract void SetSpeed(int speed);
        public abstract void SetOffset(Point3F p);
    }
}
