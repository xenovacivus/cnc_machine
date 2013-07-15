using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serial;
using System.Timers;
using OpenTK;
using Utilities;

namespace Robot
{
    public class Robot : IHardware
    {
        IRobotCommand currentCommand = null;
        SerialPortWrapper serial;
        Timer t;
        int elapsedCounter = 0;

        Vector3 currentPosition = new Vector3(0, 0, 0);

        Vector3 lastPosition = new Vector3(0, 0, 0);

        public Robot(SerialPortWrapper serial)
        {
            this.serial = serial;
            serial.newDataAvailable += new SerialPortWrapper.newDataAvailableDelegate(NewDataAvailable);
            serial.receiveDataError += new SerialPortWrapper.receiveDataErrorDelegate(ReceiveDataError);
            t = new Timer();
            t.Interval = 50;
            t.Start();
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
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
                            currentPosition = Vector3.Divide (c.CurrentPosition.ToVector3(), ScaleFactors);
                            locations = c.Locations;
                            if (onPositionUpdate != null)
                            {
                                onPositionUpdate(this, EventArgs.Empty);
                            }
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
            currentCommand = null;
            if (locations < MaxLocations)
            {
                // Ok to pass in another movement command

                // TODO: rework this to use a local buffer...
                if (onRobotReady != null)
                {
                    onRobotReady(this, EventArgs.Empty);
                }
            }

            if (currentCommand == null)
            {
                currentCommand = new StatusCommand();
            }
            
            return currentCommand;
        }

        public override Vector3 GetPosition()
        {
            return currentPosition;
        }
        
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
                
                // Z axis has a slower maximum speed.
                // TODO: implement clamping to a maximum speed for each axis.
                delta.Z = delta.Z * 10;

                float inches = delta.Length;

                Vector3i pointInt = new Vector3i(Vector3.Multiply(p, ScaleFactors));

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

                    currentCommand = new MoveCommand(pointInt, time_milliseconds);
                }
                else
                {
                    Console.WriteLine("Ignoring command with time of 0");
                }
            }
        }

        public void Reset()
        {
            throw new NotImplementedException("Hey this isn't implemented yet or is broken or something dont use it yet");
        }
    }
}
