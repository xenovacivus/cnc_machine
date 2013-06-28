using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot;
using OpenTK;

namespace Router
{
    public abstract class RouterCommand
    {
        public abstract void Execute(IHardware d);
        public abstract Vector3 FinalPosition();
    }
}
