using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Robot
{
    internal class StatusCommand : IRobotCommand
    {
        private bool data_valid = false;
        private static byte commandCode = 0x77;
        public StatusCommand()
        {
        }
        public override byte[] GenerateCommand()
        {
            return new byte[] { commandCode };
        }
        public override void ProcessResponse(byte[] data)
        {
            data_valid = (data[0] == commandCode);
            if (data.Length <= 14)
            {
                data_valid = false;
                return;
            }

            Int32 x = 0;
            Int32 y = 0;
            Int32 z = 0;
            for (int i = 4; i >= 1; i--)
            {
                x = (x << 8) | data[i];
                y = (y << 8) | data[i + 4];
                z = (z << 8) | data[i + 8];
            }

            byte status_bits = data[13];
            bool x_moving = (status_bits & 0x01) > 0;
            bool y_moving = (status_bits & 0x02) > 0;
            bool z_moving = (status_bits & 0x04) > 0;

            is_moving = x_moving | y_moving | z_moving;
            locations = data[14];
            currentPosition = new Vector3i(x, y, z);

            //Console.WriteLine("Location = ({0}, {1}, {2}), Moving = ({3}, {4}, {5})", x, y, z, x_moving, y_moving, z_moving);
        }
        public override bool IsDataValid()
        {
            return data_valid;
        }
        private byte locations;
        public byte Locations
        {
            get
            {
                return locations;
            }
        }

        private Vector3i currentPosition;

        public Vector3i CurrentPosition
        {
            get
            {
                return currentPosition;
            }
        }

        bool is_moving = false;
        internal bool IsMoving()
        {
            return is_moving;
        }
    }
}
