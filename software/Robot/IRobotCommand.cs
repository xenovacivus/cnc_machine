using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    internal abstract class IRobotCommand
    {
        public abstract byte[] GenerateCommand();
        public abstract void ProcessResponse(byte[] data);
        public abstract bool IsDataValid();
    }
}
