using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serial;
using System.Timers;

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
            private UInt16 speed_x, speed_y, speed_z;
            private static byte commandCode = 0x32;
            public MoveCommand(Point3 location, UInt16 speed_x, UInt16 speed_y, UInt16 speed_z)
            {
                this.x = (Int32)location.x;
                this.y = (Int32)location.y;
                this.z = (Int32)location.z;
                this.speed_x = speed_x;
                this.speed_y = speed_y;
                this.speed_z = speed_z;
                data_valid = false;
            }
            public override byte[] GenerateCommand()
            {
                return new byte[] { commandCode, 
                    (byte)(speed_x & 0xFF), (byte)((speed_x >> 8) & 0xFF),
                    (byte)(speed_y & 0xFF), (byte)((speed_y >> 8) & 0xFF),
                    (byte)(speed_z & 0xFF), (byte)((speed_z >> 8) & 0xFF),
                    (byte)(x & 0xFF), (byte)((x >> 8) & 0xFF), (byte)((x >> 16) & 0xFF), (byte)((x >> 24) & 0xFF),
                    (byte)(y & 0xFF), (byte)((y >> 8) & 0xFF), (byte)((y >> 16) & 0xFF), (byte)((y >> 24) & 0xFF),
                    (byte)(z & 0xFF), (byte)((z >> 8) & 0xFF), (byte)((z >> 16) & 0xFF), (byte)((z >> 24) & 0xFF)};
            }
            public override void ProcessResponse(byte[] data)
            {
                if (data.Length > 0 && data[0] == commandCode)
                {
                    data_valid = true;
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
                if (data.Length <= 13)
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

                currentPosition = new Point3(x, y, z);

                Console.WriteLine("Location = ({0}, {1}, {2}), Moving = ({3}, {4}, {5})", x, y, z, x_moving, y_moving, z_moving);
            }
            public override bool IsDataValid()
            {
                return data_valid;
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

        IRobotCommand currentCommand = null;
        IRobotCommand pendingCommand = null;
        SerialPortWrapper serial;
        Timer t;

        bool isMoving = true;

        enum SerialState
        {
            waiting_ping_reply,
            waiting_values_reply,
            waiting_speed_reply,
            not_busy,
            waiting_reply,
        }

        bool new_values = false;
        bool new_speeds = false;

        int elapsedCounter = 0;

        SerialState state = SerialState.not_busy;
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

        Int32 deltax = 0;
        Int32 deltay = 0;
        Int32 deltaz = 0;

        UInt16 speed = 1000;

        public override void SetSpeed(int speed)
        {
            if (speed >= 16)
            {
                this.speed = (UInt16)speed;
            }
            this.new_speeds = true;
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            lock (thisLock)
            {
                if (serial != null && serial.IsOpen)
                {
                    //if (state == SerialState.not_busy)
                    //{

                    //    if (pendingCommand != null)
                    //    {
                    //        currentCommand = pendingCommand;
                    //        pendingCommand = null;
                    //    }

                    //    // If there's no pending command, send a get status command
                    //    if (currentCommand == null)
                    //    {
                    //        currentCommand = new StatusCommand();
                    //    }

                    //    state = SerialState.waiting_reply;
                    //    elapsedCounter = 0;
                    //    serial.Transmit(currentCommand.GetSendBytes(), 0x21);



                    
                     
                        
                    //    //elapsedCounter = 0;
                    //    //if (new_values)
                    //    //{
                    //    //    state = SerialState.waiting_values_reply;
                    //    //    byte [] data = new byte [13];
                    //    //    data[0] = 0x42; // Move Delta Command
                    //    //    Console.WriteLine("Moving by = ({0}, {1}, {2})", deltax, deltay, deltaz);
                        
                    //    //    data[1] = (byte)(deltax & 0xFF);
                    //    //    data[2] = (byte)((deltax >> 8) & 0xFF);
                    //    //    data[3] = (byte)((deltax >> 16) & 0xFF);
                    //    //    data[4] = (byte)((deltax >> 24) & 0xFF);

                    //    //    data[5] = (byte)(deltay & 0xFF);
                    //    //    data[6] = (byte)((deltay >> 8) & 0xFF);
                    //    //    data[7] = (byte)((deltay >> 16) & 0xFF);
                    //    //    data[8] = (byte)((deltay >> 24) & 0xFF);

                    //    //    data[9]  = (byte)(deltaz & 0xFF);
                    //    //    data[10] = (byte)((deltaz >> 8) & 0xFF);
                    //    //    data[11] = (byte)((deltaz >> 16) & 0xFF);
                    //    //    data[12] = (byte)((deltaz >> 24) & 0xFF);

                    //    //    serial.Transmit(data, 0x21);

                    //    //}
                    //    //else if (new_speeds)
                    //    //{
                    //    //    Console.WriteLine("Speed = {0}, Add = {1}", speed, add);
                    //    //    state = SerialState.waiting_speed_reply;
                    //    //    byte [] data = new byte [4];
                    //    //    data[0] = 0x22; // Set Speed Command
                    //    //    data[1] = (byte)(speed & 0xFF);
                    //    //    data[2] = (byte)((speed >> 8) & 0xFF);
                    //    //    data[3] = add;
                    //    //    serial.Transmit(data, 0x21);
                    //    //}
                    //    //else
                    //    //{
                    //    //    state = SerialState.waiting_ping_reply;
                    //    //    serial.Transmit(new byte[] { 0x77 }, 0x21);
                    //    //}
                        

                    //}
                    //else
                    //{


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
                else
                {
                    state = SerialState.not_busy;
                }
            }
            t.Start();
        }

        // Serial Interface Callbacks
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
                        if (currentCommand is StatusCommand)
                        {
                            StatusCommand c = currentCommand as StatusCommand;
                            currentPosition = c.CurrentPosition;
                            isMoving = c.IsMoving();
                        }

                        currentCommand = GetNextCommand();

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

        private IRobotCommand GetNextCommand()
        {
            IRobotCommand nextCommand = null;

            if (!isMoving)
            {
                if (onRobotReady != null)
                {
                    onRobotReady(this, EventArgs.Empty);
                }
                nextCommand = pendingCommand;
                if (nextCommand != null)
                {
                    pendingCommand = null;
                    isMoving = true;
                }
            }

            if (nextCommand == null)
            {
                nextCommand = new StatusCommand();
            }
            
            return nextCommand;
        }

        //public EventHandler onRobotReady = null;
        
        Point3 lastPosition = new Point3 (0, 0, 0);
        Point3 currentPosition = new Point3 (0, 0, 0);
        Point3 destinationPosition = new Point3 (0, 0, 0);

        //public override float GetProgress()
        //{
        //    float l1 = lastPosition.Length(destinationPosition);
        //    float l2 = currentPosition.Length(destinationPosition);
        //    return l2 / l1;
        //}

        // 25 stepper revolutions per inch * 32 ticks per phase * 24 phases per revolution
        //private float xScaleFactor = 25 * 32 * 24 * 1.0f;

        // 12.5 inches per tooth, 15 tooth pulley, 100 phases per revolution
        private float xScaleFactor = 12.5f * (1.0f / 15f) * 100f * 32.0f;

        // 32 threads per inch * 32 ticks per phase * 24 phases per revolution
        //private float yScaleFactor = 32 * 32 * 24 * 1.0f;
        private float yScaleFactor = 12.5f * (1.0f / 15f) * 100f * 32.0f;
        

        // 32 threads per inch * 32 ticks per phase * 100 phases per revolution
        private float zScaleFactor = 32 * 32 * 100 * 1.0f;

        public override Point3F GetPosition()
        {
            Point3F currentPositionFloat = new Point3F(currentPosition.x / xScaleFactor - Offset.x, currentPosition.y / yScaleFactor - Offset.y, currentPosition.z / zScaleFactor - Offset.z);
            return currentPositionFloat;
        }

        Point3F Offset = new Point3F(0, 0, 0);
        public override void SetOffset(Point3F p)
        {
            Offset = p;
        }
        public override void GoTo(Point3F p, float tool_speed)
        {
            if (IsMoving())
            {
                throw new Exception("Cannot go to new position while already moving!");
            }
            Point3F cur = GetPosition();
            Point3F delta = new Point3F(p.x - cur.x, p.y - cur.y, p.z - cur.z);
            SetToGo(delta, tool_speed);
        }
        private void SetToGo(Point3F p, float tool_speed)
        {
            lock (thisLock)
            {
                //p = new Point3F(p.x + Offset.x, p.y + Offset.y, p.z + Offset.z);

                Point3 pointInt = new Point3((int)(p.x * xScaleFactor), (int)(p.y * yScaleFactor), (int)(p.z * zScaleFactor));
                if (IsMoving())
                {
                    throw new Exception("Cannot go to new position while already moving!");
                }
                if (pendingCommand != null)
                {
                    Console.WriteLine("Command is pending, cannot move to a new position!");
                    return;
                }
                lock (destinationPosition)
                {
                    isMoving = true;
                    float inches_per_minute = tool_speed;
                    float inches = p.Length();
                    // 16 uS per tick
                    // length of l inches
                    // inches/minute of inches_per_minute
                    float time_seconds = 60 * inches / inches_per_minute;
                    float num_ticks = time_seconds / 0.000016f;

                    lastPosition = currentPosition;
                    destinationPosition = currentPosition.Add(pointInt);

                    Point3F delta = new Point3F(Math.Abs(pointInt.x), Math.Abs(pointInt.y), Math.Abs(pointInt.z));
                    float max_delta = Math.Max(Math.Max(delta.x, delta.y), delta.z);
                    delta = new Point3F(delta.x / max_delta, delta.y / max_delta, delta.z / max_delta);
                    //delta = delta.Normalize();

                    // num_ticks = distance * period
                    float period_x = Math.Abs(num_ticks / pointInt.x);
                    float period_y = Math.Abs(num_ticks / pointInt.y);
                    float period_z = Math.Abs(num_ticks / pointInt.z);


                    float min_period = Math.Min(Math.Min(period_x, period_y), period_z);
                    Console.WriteLine("Min Period = {0}", min_period);
                    delta = new Point3F(min_period / delta.x, min_period / delta.y, min_period / delta.z);

                    float speed_scale = Math.Max(Math.Max(25.0f / delta.x, 15.0f / delta.y), 15.0f / delta.z);
                    if (speed_scale > 1)
                    {
                        Console.WriteLine("Had to scale the speed by {0}", 1.0f / speed_scale);
                        delta = new Point3F(delta.x * speed_scale, delta.y * speed_scale, delta.z * speed_scale);
                    }


                    UInt16 x_per = (UInt16)delta.x;
                    UInt16 y_per = (UInt16)delta.y;
                    UInt16 z_per = (UInt16)delta.z;
                    if (delta.x > UInt16.MaxValue)
                    {
                        x_per = UInt16.MaxValue;
                    }
                    if (delta.y > UInt16.MaxValue)
                    {
                        y_per = UInt16.MaxValue;
                    }
                    if (delta.z > UInt16.MaxValue)
                    {
                        z_per = UInt16.MaxValue;
                    }

                    Console.WriteLine("Moving ({0}, {1}, {2}) to ({3}, {4}, {5})", lastPosition.x, lastPosition.y, lastPosition.z, destinationPosition.x, destinationPosition.y, destinationPosition.z);
                    Console.WriteLine("Periods = ({0}, {1}, {2})", x_per, y_per, z_per);
                    pendingCommand = new MoveCommand(destinationPosition, x_per, y_per, z_per);
                    //commands.Add
                }
            }
        }

        List<IRobotCommand> commands = new List<IRobotCommand>();

        public override bool IsMoving()
        {
            return isMoving;
        }

        public override bool Ready()
        {
            if (serial.IsOpen)
            {
                if (pendingCommand == null)
                {
                    return !IsMoving();
                }
            }
            return false;
        }

        public void Reset()
        {
            if (pendingCommand == null)
            {
                pendingCommand = new ResetCommand();
                isMoving = true;
            }
            else
            {
                Console.WriteLine("Could not send reset command because another command is pending!");
            }
        }
    }
}
