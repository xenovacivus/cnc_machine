using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot
{
    internal class ResetCommand : IRobotCommand
    {
        private bool data_valid;
        private static byte commandCode = 0x88;
        public ResetCommand()
        {
            data_valid = false;
        }
        public override byte[] GenerateCommand()
        {
            return new byte[] { commandCode };
        }
        public override void ProcessResponse(byte[] data)
        {
            if (data.Length > 0 && data[0] == commandCode)
            {
                data_valid = true;
            }
            else
            {
                Console.WriteLine("Error processing reset command data!");
                foreach (byte b in data)
                {
                    Console.WriteLine(b);
                }
            }
        }
        public override bool IsDataValid()
        {
            return data_valid;
        }
    }
}
