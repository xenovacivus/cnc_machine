using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Robot
{
    internal class MoveCommand : IRobotCommand
    {
        private bool data_valid;
        private Int32 x, y, z;
        private UInt16 time_milliseconds;
        private static byte commandCode = 0x32;
        private byte locations;
        public byte Locations
        {
            get
            {
                return locations;
            }
        }
        public MoveCommand(Vector3i location, UInt16 time_milliseconds)
        {
            this.x = (Int32)location.x;
            this.y = (Int32)location.y;
            this.z = (Int32)location.z;
            this.time_milliseconds = time_milliseconds;
            data_valid = false;
        }
        public override byte[] GenerateCommand()
        {
            return new byte[] { commandCode, 
                    (byte)(time_milliseconds & 0xFF), (byte)((time_milliseconds >> 8) & 0xFF),
                    (byte)(x & 0xFF), (byte)((x >> 8) & 0xFF), (byte)((x >> 16) & 0xFF), (byte)((x >> 24) & 0xFF),
                    (byte)(y & 0xFF), (byte)((y >> 8) & 0xFF), (byte)((y >> 16) & 0xFF), (byte)((y >> 24) & 0xFF),
                    (byte)(z & 0xFF), (byte)((z >> 8) & 0xFF), (byte)((z >> 16) & 0xFF), (byte)((z >> 24) & 0xFF)};
        }
        public override void ProcessResponse(byte[] data)
        {
            if (data.Length > 0 && data[0] == commandCode)
            {
                data_valid = true;
                locations = data[1];
                //Console.WriteLine("Response from MoveCommand: locations = {0}", locations);
            }
            else
            {
                Console.WriteLine("Error processing move command data!");
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
