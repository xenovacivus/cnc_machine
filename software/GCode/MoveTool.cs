using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Robot;

namespace Router
{
    public class MoveTool : RouterCommand
    {
        private Vector3 toPoint;
        private float tool_speed;

        public Vector3 Location
        {
            get
            {
                return toPoint;
            }
        }

        public override Vector3 FinalPosition()
        {
            return toPoint;
        }

        public MoveTool(Vector3 toPoint, float tool_speed)
        {
            // TODO: Complete member initialization
            this.toPoint = toPoint;
            this.tool_speed = tool_speed;
        }
        public override void Execute(IHardware d)
        {
            Console.WriteLine("Going to " + toPoint);
            Vector3 p = new Vector3(toPoint.X, toPoint.Y, toPoint.Z);
            d.GoTo(p, tool_speed);
        }
    }
}
