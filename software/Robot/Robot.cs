using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serial;
using System.Timers;
using OpenTK;

namespace Robot
{
    
    public class Robot : IHardware
    {
        private abstract class IRobotCommand
        {
            public abstract byte[] GenerateCommand();
            public abstract void ProcessResponse(byte[] data);
            public abstract bool IsDataValid();
        }

        private class ResetCommand : IRobotCommand
        {
            private bool data_valid;
            private static byte commandCode = 0x88;
            public ResetCommand ()
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

        private class MoveCommand : IRobotCommand
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
            public MoveCommand(Point3 location, UInt16 time_milliseconds)
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
    
        private class StatusCommand : IRobotCommand
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
                currentPosition = new Point3(x, y, z);

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

            private Point3 currentPosition;

            public Point3 CurrentPosition
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

        /// <summary>
        /// Gets the X, Y, Z Scale Factors for converting Inches to Stepper Ticks
        /// X Axis: 12.5 inches per tooth, 15 tooth pulley, 100 phases per revolution, 32 ticks per phase
        /// Y Axis: 12.5 inches per tooth, 15 tooth pulley, 100 phases per revolution, 32 ticks per phase
        /// Z Axis: 32 threads per inch * 32 ticks per phase * 100 phases per revolution
        /// </summary>
        /// <returns></returns>
        private static Vector3 ScaleFactors
        {
            get
            {
                return new Vector3(
                12.5f * (1.0f / 15f) * 100f * 32.0f,
                12.5f * (1.0f / 15f) * 100f * 32.0f,
                32 * 32 * 100 * 1.0f);
            }
        }

        IRobotCommand currentCommand = null;
        IRobotCommand pendingCommand = null;
        SerialPortWrapper serial;
        Timer t;

        int elapsedCounter = 0;

        public Robot(SerialPortWrapper serial)
        {
            this.serial = serial;
            serial.newDataAvailable += new SerialPortWrapper.newDataAvailableDelegate(NewDataAvailable);
            serial.receiveDataError += new SerialPortWrapper.receiveDataErrorDelegate(ReceiveDataError);
            t = new Timer();
            t.Interval = 20;
            t.Start();
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            lock (thisLock)
            {
                if (serial != null && serial.IsOpen)
                {
                    elapsedCounter++;
                    if ((elapsedCounter * 50) > (1000)) // More than 1 second to reply
                    {
                        Console.WriteLine("Device Timeout!");

                        // Send a status command
                        currentCommand = new StatusCommand();
                        
                        serial.Transmit(currentCommand.GenerateCommand(), 0x21);
                        elapsedCounter = 0;
                    }
                }
            }
            t.Start();
        }

        #region Serial Interface Callbacks
        
        private void ReceiveDataError(byte err)
        {
            Console.WriteLine("Data Error: " + err);
        }

        Object thisLock = new Object();
        private void NewDataAvailable(SerialPortWrapper.SimpleSerialPacket packet)
        {
            lock (thisLock)
            {
                if (currentCommand == null)
                {
                    Console.WriteLine("Error: Received data, but no command was sent!");
                    foreach (byte b in packet.Data)
                    {
                        Console.Write(b.ToString("x") + ", ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    currentCommand.ProcessResponse(packet.Data);
                    if (currentCommand.IsDataValid())
                    {
                        // See if there's any state information in the command used to 
                        // update location or other fields...
                        byte locations = MaxLocations;
                        if (currentCommand is StatusCommand)
                        {
                            StatusCommand c = currentCommand as StatusCommand;
                            currentPosition = c.CurrentPosition;
                            locations = c.Locations;
                        }
                        if (currentCommand is MoveCommand)
                        {
                            MoveCommand m = currentCommand as MoveCommand;
                            locations = m.Locations;
                        }
                        
                        currentCommand = GetNextCommand(locations);

                        elapsedCounter = 0;
                        serial.Transmit(currentCommand.GenerateCommand(), 0x21);
                    }
                    else
                    {
                        Console.WriteLine("Error: Did not process data correctly!");
                    }
                }
            }
        }

        static byte MaxLocations
        {
            get
            {
                return 4;
            }
        }

        #endregion

        private IRobotCommand GetNextCommand(byte locations)
        {
            IRobotCommand nextCommand = null;

            if (locations < MaxLocations)
            {
                // Ok to pass in another movement command

                // TODO: rework this to use a local buffer...
                if (onRobotReady != null)
                {
                    onRobotReady(this, EventArgs.Empty);
                }
                nextCommand = pendingCommand;
                if (nextCommand != null)
                {
                    pendingCommand = null;
                    locations = MaxLocations;
                }
            }

            if (nextCommand == null)
            {
                nextCommand = new StatusCommand();
            }
            
            return nextCommand;
        }

        Point3 currentPosition = new Point3 (0, 0, 0);

        public override Vector3 GetPosition()
        {
            Vector3 posInches = Vector3.Divide(currentPosition.ToVector3(), ScaleFactors) - Offset;
            return posInches;
        }

        Vector3 Offset = new Vector3(0, 0, 0);
        public override void SetOffset(Vector3 p)
        {
            Offset = p;
        }
        
        Vector3 lastPosition = new Vector3(0, 0, 0);

        /// <summary>
        /// Run the router from the current position to the given position
        /// </summary>
        /// <param name="p">Destination location in inches</param>
        /// <param name="inches_per_minute">Tool speed in inches per second</param>
        public override void GoTo(Vector3 p, float inches_per_minute)
        {
            lock (thisLock)
            {
                Vector3 delta = lastPosition - p;
                delta.Z = delta.Z * 10; // Z axis moves slower
                float inches = delta.Length;
                
                Point3 pointInt = new Point3(Vector3.Multiply(p, ScaleFactors));

                UInt16 time_milliseconds = (UInt16)(1000 * 60 * inches / inches_per_minute);

                if (time_milliseconds > 0)
                {

                    Console.WriteLine("Moving ({0}, {1}, {2}) to ({3}, {4}, {5}) in {6} milliseconds",
                        lastPosition.X,
                        lastPosition.Y,
                        lastPosition.Z,
                        pointInt.x,
                        pointInt.y,
                        pointInt.z,
                        time_milliseconds);

                    lastPosition = p;

                    pendingCommand = new MoveCommand(pointInt, time_milliseconds);
                }
                else
                {
                    Console.WriteLine("Ignoring command with time of 0");
                }
                //commands.Add
            }
        }

        List<IRobotCommand> commands = new List<IRobotCommand>();

        public void Reset()
        {
            throw new NotImplementedException("Hey this isn't implemented yet or is broken or something dont use it yet");
            if (pendingCommand == null)
            {
                pendingCommand = new ResetCommand();
            }
            else
            {
                Console.WriteLine("Could not send reset command because another command is pending!");
            }
        }
    }
}
