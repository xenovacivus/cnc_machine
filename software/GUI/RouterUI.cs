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

        public RouterUI()
        {
            InitializeComponent();
            serial = new SerialPortWrapper();
            robot = new Robot.Robot(serial);
            hw = robot as IHardware;
            router = new Router.Router(hw);

            router.OnRouterPositionUpdate = new Router.Router.NewRouterPositionDelegate(routerPositionUpdated);

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

        void routerPositionUpdated(PointF newPosition)
        {
            if (this.InvokeRequired && !this.Disposing)
            {
                try
                {
                    Router.Router.NewRouterPositionDelegate d = new Router.Router.NewRouterPositionDelegate(routerPositionUpdated);
                    this.Invoke(d, new object[] { newPosition });
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

        #region Chart Drawing

        public class Rect
        {
            public float x, y;
            public float w, h;
            public Rect()
            {
                x = 0; y = 0; w = 0; h = 0;
            }
            public Rect(float x, float y, float w, float h)
            {
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
            }
            public Rectangle ToRect()
            {
                return new Rectangle((int)x, (int)y, (int)w, (int)h);
            }
        }

        public abstract class Cheesburger
        {
            public abstract void MoveTo(PointF location);
            public abstract void RotateBy(float degrees);
        }

        public class Circle
        {
            public PointF location = new PointF (0, 0);
            public float radius = 1;
            public Circle()
            {
            }
        }

        public class Shape : Cheesburger
        {
            public override void MoveTo(PointF location)
            {
                this.location = location;
            }
            public override void RotateBy(float degrees)
            {
                this.rotation = degrees;
            }
            public List<Rect> rectangles = new List<Rect>();
            public List<Rect> filledRectangles = new List<Rect>();
            public List<Circle> circles = new List<Circle>();
            public List<Circle> filledCircles = new List<Circle>();
            public PointF location;
            public float rotation;
            public override string ToString()
            {
                return "Ima shape!";
            }
            public Shape()
            {
            }
            public void Draw(Graphics g)
            {
                GraphicsState s = g.Save();
                Pen p = new Pen(Brushes.DarkGray, 10);
                
                g.TranslateTransform(location.X, location.Y);
                g.RotateTransform(this.rotation);
                foreach (Rect r in rectangles)
                {
                    g.FillRectangle(Brushes.Red, r.ToRect());
                }
                foreach (Rect r in filledRectangles)
                {
                    
                    g.DrawRectangle(p, r.ToRect());
                }
                foreach (Circle c in circles)
                {
                    g.DrawEllipse(p, new RectangleF(c.location.X - c.radius, c.location.Y - c.radius, c.radius * 2, c.radius * 2));
                }
                foreach (Circle c in filledCircles)
                {
                    g.FillEllipse(Brushes.Red, new RectangleF(c.location.X - c.radius, c.location.Y - c.radius, c.radius * 2, c.radius * 2));
                }
                g.Restore(s);
            }
        }
        public List<Shape> shapes = new List<Shape>();

        Rout r = null;//new Rout();
        

        public class Cut : Cheesburger
        {
            public override void MoveTo(PointF location)
            {
                PointF difference = Sub(p2, p1);
                p1 = location;
                p2 = new PointF(p1.X + difference.X, p1.Y + difference.Y);
            }
            public override void RotateBy(float degrees)
            {
                throw new NotImplementedException();
            }
            public PointF p1;
            public PointF p2;
            public float width;

            public override string ToString()
            {
                return "I'm a cut!";
            }
            public Cut()
            {
            }
            public Cut(PointF pt1, PointF pt2)
            {
                p1 = pt1;// new PointF(2000.0f, 1000.0f);
                p2 = pt2;// new PointF(3000.0f, 2000.0f);
                width = 25;
            }
            public void SetPoint2(PointF p)
            {
                p2 = p;
            }
            public void Draw(Graphics g, PointF cursor)
            {
                System.Drawing.Drawing2D.GraphicsState s = g.Save();

                g.DrawLine(Pens.DarkGreen, p1, p2);


                //g.DrawLine(Pens.Black, new Point(0, 0), new Point(500, 500));
                float angle = (float)(180.0f / Math.PI * Math.Atan2(p2.Y - p1.Y, p2.X - p1.X));
                Font f = new Font(FontFamily.GenericSansSerif, 80, FontStyle.Bold);

                g.TranslateTransform(p1.X, p1.Y);
                g.RotateTransform(angle);
                float length = this.Length();

                /*
                PointF n = this.Scale(this.Sub(p1, p2), 1.0f / Length());
                float d1 = this.Mult(p1, n) - this.Mult(cursor, n);
                //Console.WriteLine("Distance = " + d1);

                n = new PointF(n.Y, n.X);// this.Scale(this.Sub(p1, p2), 1.0f / Length());
                float d2 = this.Mult(p1, n) - this.Mult(cursor, n);
                
                
                if ((d1 <= Length() && d1 >= 0) && (Math.Abs(d2) < width))
                {
                    g.FillRectangle(Brushes.Black, new RectangleF(0, - width / 2, length, width));
                }
                 */

                
                g.DrawArc(Pens.Orange, new RectangleF(-width/2, -width/2, width, width), 90, 180);
                g.DrawLine(Pens.Orange, new PointF(0, width / 2.0f), new PointF(length, width / 2.0f));
                g.DrawLine(Pens.Orange, new PointF(0, -width / 2.0f), new PointF(length, -width / 2.0f));
                //g.TranslateTransform(p2.X - p1.X, p2.Y - p1.Y);
                
                g.TranslateTransform(length, 0);
                g.DrawArc(Pens.Orange, new RectangleF(-width / 2, -width / 2, width, width), 270, 180);
                
                //g.DrawEllipse(Pens.Orange, new RectangleF(-width / 2.0f, width / 2.0f, width / 2.0f, width / 2.0f));
                
                g.Restore(s);
            }
            PointF Scale(PointF a, float b)
            {
                return new PointF(a.X * b, a.Y * b);
            }
            PointF Sub(PointF a, PointF b)
            {
                return new PointF(a.X - b.X, a.Y - b.Y);
            }
            float Mult(PointF a, PointF b)
            {
                return a.X * b.X + a.Y * b.Y;
            }
            float Length(PointF a, PointF b)
            {
                return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            }
            float Length()
            {
                return Length(p1, p2);
            }

        }

        private List<Cut> Cuts;

        #endregion

        #region Helper Methods

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    Int32 x = Int32.Parse(xBox.Text);
            //    Int32 y = Int32.Parse(yBox.Text);
            //    device.SetToGo(x, y);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Exception", ex.Message);
            //}
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private class DrawingData
        {
            public DrawingData()
            {
            }
            public int grid = 10;
            public float scale = 1;
            public float tx = 0;
            public float ty = 0;

            internal PointF SnapToGrid(PointF point)
            {
                point.X = (float)(Math.Floor(point.X / grid + .5) * grid);
                point.Y = (float)(Math.Floor(point.Y / grid + .5) * grid);
                return point;
            }
        }
        private DrawingData drawingData = new DrawingData();


        public class Stuff
        {
            public List<Cut> cuts = new List<Cut>();
            public List<Shape> shapes = new List<Shape>();
            public List<Rout> routs = new List<Rout>();
            public List<CircleRout> circles = new List<CircleRout>();
            public List<PulleyRout> pulleys = new List<PulleyRout>();
            public Stuff()
            {
            }
            public void Add(Object o)
            {
                // Start with derived types first
                if (o is CircleRout)
                {
                    circles.Add(o as CircleRout);
                }
                else if (o is PulleyRout)
                {
                    pulleys.Add(o as PulleyRout);
                }
                else if (o is Shape)
                {
                    shapes.Add(o as Shape);
                }
                else if (o is Cut)
                {
                    cuts.Add(o as Cut);
                }
                else if (o is Rout)
                {
                    routs.Add(o as Rout);
                }
                
            }

            internal List<object> GetObjects()
            {
                List<Object> objects = new List<object>();
                foreach (Object o in pulleys)
                {
                    objects.Add (o);
                }
                foreach (Object o in circles)
                {
                    objects.Add(o);
                }
                foreach (Object o in shapes)
                {
                    objects.Add(o);
                }
                foreach (Object o in cuts)
                {
                    objects.Add(o);
                }
                foreach (Object o in routs)
                {
                    objects.Add(o);
                }
                return objects;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();

            if (d.ShowDialog() == DialogResult.OK)
            {
                List<Object> rts = userControl11.GetObjects();
                List<Object> toSave = new List<Object>();
                Stuff s = new Stuff();
                foreach (Object o in rts)
                {
                    s.Add(o);
                }
                XmlSerializer ser = new XmlSerializer(typeof(Stuff));
                Stream writer = new FileStream (d.FileName, FileMode.OpenOrCreate);
                ser.Serialize(writer, s);
                writer.Close();
            }
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "XML Files (*.xml)|*.xml";
            if (DialogResult.OK == d.ShowDialog())
            {
                XmlSerializer s = new XmlSerializer(typeof(Stuff));
                Stream reader = new FileStream(d.FileName, FileMode.Open);
                Stuff st = s.Deserialize(reader) as Stuff;

                List<Object> objects = st.GetObjects();
                foreach (Object o in objects)
                {
                    userControl11.AddObject(o);
                }

                //foreach (Cut c in st.cuts)
                //{
                //    listBox1.Items.Add(c);
                //}
                //foreach (Shape sh in st.shapes)
                //{
                //    listBox1.Items.Add(sh);
                //}
                //foreach (Rout rt in st.routs)
                //{
                //    listBox1.Items.Add(rt);
                //}
                reader.Close();
            }
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

        private void tool_Click(object sender, EventArgs e)
        {
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
                Object [] objects = g_code.GetRouts();
                foreach (Object o in objects)
                {
                    userControl11.AddObject(o);
                }
                //listBox1.Items.AddRange(g_code.GetRouts());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (g_code != null)
            {
                RouterCommand [] router_commands = g_code.GetRouterCommands();

                router.AddRouterCommands(router_commands);

                //List<Rout> rts = routs;
                //foreach (Rout r in rts)
                //{
                //    for (int i = 0; i < (r.points.Count - 1); i++)
                //    {
                //        router.RoutPath(r.points[i], r.points[i + 1]);
                //    }
                //}
                //router.Complete();
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

    }
}
