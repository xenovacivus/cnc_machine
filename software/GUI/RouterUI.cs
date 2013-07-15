using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;
using RoverEmulator;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Robot;
using Serial;
using Utilities;
using Router;
using OpenTK;

namespace GUI
{
    public partial class RouterUI : Form
    {
        Robot.Robot robot;
        IHardware hw;
        Router.Router router;
        SerialPortWrapper serial;
        Timer t;

        void PosUpdate(object o, EventArgs e)
        {
            if (this.InvokeRequired && !this.Disposing)
            {
                try
                {
                    this.Invoke(new EventHandler(PosUpdate), new object[] { o, e });
                }
                catch (ObjectDisposedException ex)
                {
                }
            }
            else
            {
                userControl11.Invalidate();
            }
        }

        public RouterUI()
        {
            InitializeComponent();
            serial = new SerialPortWrapper();
            robot = new Robot.Robot(serial);
            hw = robot as IHardware;
            router = new Router.Router(hw);
            hw.onPositionUpdate += new EventHandler(PosUpdate);

            // Setup for doing our own form painting
            //this.SetStyle(
            //  ControlStyles.AllPaintingInWmPaint |
            //  ControlStyles.UserPaint |
            //  ControlStyles.DoubleBuffer, true);
            t = new Timer();
            t.Interval = 25;
            t.Tick += new EventHandler(t_Tick);
            t.Enabled = false;
            //t.Start();

            this.propertyGrid.SelectedGridItemChanged += new SelectedGridItemChangedEventHandler(propertyGrid_SelectedGridItemChanged);
            
            userControl11.SelectedItemChanged += new EventHandler(userControl11_SelectedItemChanged);
            userControl11.AddObject(router);
            //g1 = new InvoluteGear();
            //g2 = new InvoluteGear();
            //userControl11.AddObject(g1);
            //g2.X = 122.5493f * 2;
            //g2.Rotation = 180.0f / 11.0f + 0.425f;
            //userControl11.AddObject(g2);
        }
        //InvoluteGear g1;
        //InvoluteGear g2;

        void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            userControl11.Invalidate();
        }

        void userControl11_SelectedItemChanged(object sender, EventArgs e)
        {
            this.radioButton1.Checked = true;
            this.propertyGrid.SelectedObject = sender;
        }

        private void RoutAllClick(object sender, EventArgs e)
        {
            //if (g_code != null)
            //{
            //    RouterCommand[] router_commands = g_code.GetRouterCommands();
            //    router.AddRouterCommands(router_commands);
            //}

            List<Object> rts = userControl11.GetObjects();
            foreach (Object o in rts)
            {
                IHasRouts ihs = o as IHasRouts;
                if (ihs != null)
                {
                    foreach (Rout r in ihs.GetRouts())
                    {
                        bool backwards = false;
                        float originalDepth = r.Depth;
                        float currentDepth = 0; // Surface depth
                        
                        float maxCutDepth = router.MaxCutDepth;
                        while (originalDepth < (currentDepth - maxCutDepth))
                        {
                            r.Depth = currentDepth - maxCutDepth;
                            router.RoutPath(r, backwards);
                            backwards = !backwards;
                            currentDepth = currentDepth - maxCutDepth;
                        }

                        // Final Cut
                        r.Depth = originalDepth;
                        router.RoutPath(r, backwards);
                    }
                }
            }
            router.Complete();
        }

        private void CompleteClick(object sender, EventArgs e)
        {
            router.Complete();
        }

        COMPortForm comPortForm = null;
        private void button4_Click(object sender, EventArgs e)
        {
            if (comPortForm == null || comPortForm.IsDisposed)
            {
                comPortForm = new COMPortForm(serial);
            }

            if (!comPortForm.Visible)
            {
                comPortForm.Show(null);
            }
            else
            {
                comPortForm.Focus();
            }
        }

        float last_x = 0;
        float last_y = 0;
        float last_z = 0;
        void t_Tick(object sender, EventArgs e)
        {
            //float z = (float)up_down_z.Value; // float.Parse(box_z.Text);

            //g1.Rotation = -z * 100;
            //g2.Rotation = z * 100 + 180.0f / 11.0f + 0.425f;

            float toolSpeed = (float)this.toolSpeedUpDown.Value;
            //lock (hw)
            //{
            //    if (hw.IsMoving())
            //    {
            //    }
            //    else
            //    {
            //        float x = (float)up_down_x.Value; // float.Parse(box_x.Text);
            //        float y = (float)up_down_y.Value; // float.Parse(box_y.Text);
            //        float z = (float)up_down_z.Value; // float.Parse(box_z.Text);
            //        if (last_x != x || last_y != y || last_z != z)
            //        {
            //            hw.SetOffset(new Vector3(-x, -y, -z));
            //            hw.GoTo(new Vector3(0, 0, 0), toolSpeed);
            //            last_x = x;
            //            last_y = y;
            //            last_z = z;
            //        }
            //    }
            //}
        }

        private void robot_button_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                t_Tick(sender, e);
            }
            //if (hw.IsMoving())
            //{
            //}
            //else
            //{
            //    float x = (float)up_down_x.Value; // float.Parse(box_x.Text);
            //    float y = (float)up_down_y.Value; // float.Parse(box_y.Text);
            //    float z = (float)up_down_z.Value; // float.Parse(box_z.Text);
            //    if (last_x != x || last_y != y || last_z != z)
            //    {
            //        hw.SetOffset(new Point3F(-x, -y, -z));
            //        hw.GoTo(new Point3F(0, 0, 0), 5);
            //        last_x = x;
            //        last_y = y;
            //        last_z = z;
            //    }
            //}
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            robot.Reset();
        }

        private void box_x_TextChanged(object sender, EventArgs e)
        {

        }

        GCode g_code;
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Drill Files (*.nc)|*.nc";
            if (DialogResult.OK == d.ShowDialog())
            {
                string[] lines = System.IO.File.ReadAllLines(d.FileName);
                g_code = new GCode();
                g_code.Parse(lines);
                Rout [] routs = g_code.GetRouts();
                foreach (Rout r in routs)
                {
                    userControl11.AddObject(r);
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            t.Enabled = checkBox1.Checked;
            robot_button.Enabled = !checkBox1.Checked;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                this.propertyGrid.SelectedObject = this.router;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Drill Files (*.obj)|*.obj";
            if (DialogResult.OK == d.ShowDialog())
            {
                ObjLoader o = new ObjLoader();
                o.LoadObj(d.FileName);
                userControl11.AddObject(o);
                //string[] lines = System.IO.File.ReadAllLines(d.FileName);
                //g_code = new GCode();
                //g_code.Parse(lines);
                //Rout[] routs = g_code.GetRouts();
                //foreach (Rout r in routs)
                //{
                //    userControl11.AddObject(r);
                //}
            }
        }

    }
}
