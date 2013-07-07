using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Robot
{
    public abstract class IHardware
    {
        public EventHandler onRobotReady;
        public abstract Vector3 GetPosition();
        public abstract void GoTo(Vector3 p, float tool_speed);
    }
}
