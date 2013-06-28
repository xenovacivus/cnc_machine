using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using Utilities;
using Robot;

namespace Router
{
    public class Router : IOpenGLDrawable
    {
        PointF currentPosition;
        PointF actualPosition;
        IHardware device = null;
        private List<RouterCommand> commands;
        System.Timers.Timer timer;

        Vector3 finalPosition = new Vector3(0, 0, 0);

        public delegate void NewRouterPositionDelegate (PointF newPoint);

        NewRouterPositionDelegate onRouterPositionUpdate;
        public NewRouterPositionDelegate OnRouterPositionUpdate
        {
            get
            {
                return onRouterPositionUpdate;
            }
            set
            {
                onRouterPositionUpdate = value;
            }
        }

        public Router(IHardware d)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 50;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            commands = new List<RouterCommand>();
            device = d;
            d.onRobotReady += new EventHandler(this.RobotReady);
            currentPosition = new PointF(0, 0);
            actualPosition = new PointF(0, 0);
            onRouterPositionUpdate = null;
        }

        void RobotReady(object o, EventArgs e)
        {
            lock (commands)
            {
                if (commands.Count > 0)
                {
                    RouterCommand c = commands[0];
                    commands.RemoveAt(0);
                    c.Execute(device);
                }
                else
                {
                    Point3F m = device.GetPosition();
                    finalPosition = new Vector3(m.x, m.y, m.z);
                }
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();

            if (onRouterPositionUpdate != null)
            {
                Point3F p = device.GetPosition();
                PointF pos = new PointF(p.x * 1000, p.y * 1000);
                onRouterPositionUpdate(pos);
            }
            timer.Start();
        }

        public void AddRouterCommands(RouterCommand[] router_commands)
        {
            lock (commands)
            {
                this.commands.AddRange(router_commands);
                Console.WriteLine("Now Has " + commands.Count + " Router Commands");
            }
        }

        public void Draw()
        {
            GL.PushMatrix();

            GL.Translate(0, 0, -.1);
            Point3F p = device.GetPosition();
            CoolDrawing.DrawCircle(12.5f, new OpenTK.Vector2(p.x * 1000, p.y * 1000), Color.DarkGreen);

            float w = 0;
            // Assume full bit penetrates at 20 mills depth
            if (p.z < 0)
            {
                w = -p.z / .020f * 12.5f;
                if (w > 12.5f)
                {
                    w = 12.5f;
                }
                CoolDrawing.DrawFilledCircle(w, new OpenTK.Vector2(p.x * 1000, p.y * 1000), Color.FromArgb(100, Color.OrangeRed));
                if (p.z < -.020)
                {
                    float t = (p.z + .020f) / .040f * 12.5f;
                    CoolDrawing.DrawFilledCircle(t, new OpenTK.Vector2(p.x * 1000, p.y * 1000), Color.FromArgb(100, Color.DarkRed));
                }
            }
            else
            {
                w = p.z / move_height * 12.5f;
                CoolDrawing.DrawFilledCircle(w, new OpenTK.Vector2(p.x * 1000, p.y * 1000), Color.FromArgb(100, Color.LightGreen));
            }

            GL.PopMatrix();
        }

        public void AddCommand(RouterCommand r)
        {
            lock (commands)
            {
                //bool timer_running = timer.Enabled;
                //timer.Enabled = false;
                commands.Add(r);
                finalPosition = r.FinalPosition();
                //timer.Enabled = timer_running;
            }
        }

        float move_height = 0.03f; // How high above the surface to move the router
        float move_speed = 4; // Moving speed (inches per minute)
        float rout_speed = 2; // Routing speed (inches per minute)
        float maxCutDepth = 10; // 10 mil max depth per cut

        [DescriptionAttribute ("Rout Speed (inches per minute")]
        [DisplayNameAttribute ("Routing Speed")]
        public float RoutSpeed
        {
            get
            {
                return rout_speed;
            }
            set
            {
                if (value > 0)
                {
                    rout_speed = value;
                }
            }
        }
        [DescriptionAttribute ("Moving speed (inches per minute")]
        [DisplayNameAttribute ("Moving Speed")]
        public float MoveSpeed
        {
            get { return move_speed; }
            set { if (value > 0) { move_speed = value; } }
        }
        [DescriptionAttribute ("Height above zero to while moving between cuts")]
        [DisplayNameAttribute ("Move Height")]
        public float MoveHeight
        {
            get { return move_height; }
            set { move_height = value; }
        }
        [DescriptionAttribute ("Maximum cut depth in mills")]
        [DisplayNameAttribute ("Max Cut Depth")]
        public float MaxCutDepth
        {
            get { return maxCutDepth; }
            set { maxCutDepth = value; }
        }


        public void RoutPath(Rout r, bool backwards)
        {

            List<Vector2> mine = new List<Vector2>();
            foreach (Vector2 p in (r.Points))
            {
                mine.Add(new Vector2(p.X, p.Y));
            }

            if (backwards)
            {
                mine.Reverse();
            }

            for (int i = 0; i < mine.Count; i++)
            {
                float x = mine[i].X / 1000.0f;
                float y = mine[i].Y / 1000.0f;
                float z = r.Depth / 1000.0f;

                MoveTool m = new MoveTool(new Vector3(x, y, z), rout_speed);
                if (i == 0)
                {
                    if ((finalPosition.Xy - m.Location.Xy).Length > .0001)
                    {
                        // Need to move the router up, over to new position, then down again.
                        MoveTool m1 = new MoveTool(new Vector3(finalPosition.X, finalPosition.Y, move_height), move_speed);
                        MoveTool m2 = new MoveTool(new Vector3(m.Location.X, m.Location.Y, move_height), move_speed);
                        AddCommand(m1);
                        AddCommand(m2);
                    }
                }
                AddCommand(m);
            }
        }

        // Move to (0, 0, move_height)
        public void Complete()
        {
            if (finalPosition.Z < move_height)
            {
                AddCommand(new MoveTool(new Vector3(finalPosition.X, finalPosition.Y, move_height), move_speed));
            }
            AddCommand(new MoveTool(new Vector3(0, 0, move_height), move_speed));
        }
    }
}
