using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.ComponentModel;
using Utilities;
using System.Windows.Forms;

namespace Router
{
    public class PulleyRout : SimpleClickable, IHasRouts, IClickable, IOpenGLDrawable //SimpleClickable, IHasRouts, IClickable, IOpenGLDrawable
    {
        protected Vector2 center = new Vector2 (0, 0);
        double tooth_depth = 45;

        public double ToothDepth
        {
            get { return tooth_depth; }
            set { tooth_depth = value; this.ComputePoints(); }
        }

        private Vector2 Center
        {
            get
            {
                return base.roundVec (center + this.MouseDragVector);
            }
        }

        protected int teeth = 20; // input param
        protected float radius; // computed
        float width = 45; // input param
        double tooth_taper_angle = 5 / 180.0 * Math.PI; // angle of tooth edge with respect to tangent line on outside of pulley

        public int ToothTaperAngle
        {
            get
            {
                return (int)(tooth_taper_angle / Math.PI * 180 + .5);
            }
            set
            {
                if (value > -45 && value < 45)
                {
                    tooth_taper_angle = value / 180.0 * Math.PI; // angle of tooth edge with respect to tangent line on outside of pulley
                    this.ComputePoints ();
                }
            }
        }

        public int Teeth
        {
            get
            {
                return teeth;
            }
            set
            {
                if (value > 0)
                {
                    teeth = value;
                    this.ComputePoints();
                }
            }
        }

        [DisplayNameAttribute ("Tool Diameter")]
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                if (value > 0)
                {
                    width = value;
                    this.ComputePoints();
                }
            }
        }

        [DisplayNameAttribute ("Center Diameter")]
        public float Diameter
        {
            get
            {
                return radius * 2;
            }
            set
            {
                radius = value / 2;
                if (radius < 1)
                {
                    radius = 1;
                }
                this.ComputePoints();
            }
        }

        public float X
        {
            get
            {
                return Center.X;
            }
            set
            {
                center.X = value;
                this.ComputePoints();
            }
        }

        public float Y
        {
            get
            {
                return Center.Y;
            }
            set
            {
                center.Y = value;
                this.ComputePoints();
            }
        }

        public PulleyRout()
        {
            ComputePoints();
        }

        public PulleyRout(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
            ComputePoints();
        }

        public void Draw()
        {
            double pitch = 80;
            double tooth_width = 45; // outside tooth width (part that fits into the pulley)
            double c = this.teeth * pitch;

            double r = (c / (2 * Math.PI));

            for (int i = 0; i < this.teeth; i++)
            {
                double theta_total = ((double)1 / (double)this.teeth) * 2.0 * Math.PI;
                double theta_b = (tooth_width / pitch) * theta_total;
                double theta_a = (theta_total - theta_b) / 2.0;

                double theta_start = (double)i * theta_total;

                Vector2 a1 = new Vector2((float)(Math.Cos(theta_start) * r), (float)(Math.Sin(theta_start) * r));
                Vector2 a2 = new Vector2((float)(Math.Cos(theta_start + theta_a) * r), (float)(Math.Sin(theta_start + theta_a) * r));
                Vector2 a3 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b) * r), (float)(Math.Sin(theta_start + theta_a + theta_b) * r));
                Vector2 a4 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b + theta_a) * r), (float)(Math.Sin(theta_start + theta_a + theta_b + theta_a) * r));

                double w = (Math.Tan(tooth_taper_angle) * tooth_depth) / pitch * theta_total;
                Vector2 b1 = new Vector2((float)(Math.Cos(theta_start + theta_a + w) * (r - tooth_depth)), (float)(Math.Sin(theta_start + theta_a + w) * (r - tooth_depth)));
                Vector2 b2 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b - w) * (r - tooth_depth)), (float)(Math.Sin(theta_start + theta_a + theta_b - w) * (r - tooth_depth)));

                CoolDrawing.DrawLine(1, a2 + this.Center, b1 + this.Center, Color.DarkBlue);
                CoolDrawing.DrawLine(1, b1 + this.Center, b2 + this.Center, Color.Red);
                CoolDrawing.DrawLine(1, b2 + this.Center, a3 + this.Center, Color.DarkBlue);

                CoolDrawing.DrawLine(1, a1 + this.Center, a2 + this.Center, Color.Red);
                CoolDrawing.DrawLine(1, a3 + this.Center, a4 + this.Center, Color.DarkGreen);



                Vector2 p1 = new Vector2((float)(Math.Cos(theta_start) * radius), (float)(Math.Sin(theta_start) * radius));
                Vector2 p2 = new Vector2((float)(Math.Cos(theta_start) * r), (float)(Math.Sin(theta_start) * r));
                CoolDrawing.DrawLine(1, this.Center + p1, p2 + this.Center, Color.Black);
            }
            CoolDrawing.DrawCircle(radius, Center, Color.Black);

            foreach (Rout rout in this.GetRouts())
            {
                rout.Draw();
            }

            CoolDrawing.DrawFilledCircle(width / 2, Center, Color.LightGreen);
            CoolDrawing.DrawCircle(width / 2, Center, Color.Black);
        }

        public float DistanceTo(PointF p)
        {
            Vector2 v = new Vector2(p.X, p.Y);
            float length = (Center - v).Length;
            return length;
        }

        private Rout outerRout;
        private Rout innerRout;
        private float depth = -200;
        public float Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
                this.ComputePoints();
            }
        }

        float center_hole_depth = -200;
        public float CenterHoleDepth
        {
            get
            {
                return center_hole_depth;
            }
            set
            {
                center_hole_depth = value;
                this.ComputePoints();
            }
        }


        private void ComputePoints()
        {
            outerRout = new Rout();
            outerRout.Width = width;
            outerRout.Depth = depth;

            double pitch = 80;
            double tooth_width = 45; // outside tooth width (part that fits into the pulley)

            double c = this.teeth * pitch;

            double r = (c / (2 * Math.PI)); 
            double tr = this.width / 2; // tool radius

            for (int i = 0; i < this.teeth; i++)
            {
                //double pulley_tooth_center = (double)i;
                double theta_total = ((double)1 / (double)this.teeth) * 2.0 * Math.PI;
                double theta_b = (tooth_width / pitch) * theta_total;
                double theta_a = (theta_total - theta_b) / 2.0;

                double theta_start = (double)i * theta_total;

                Vector2 a1 = new Vector2((float)(Math.Cos(theta_start) * r), (float)(Math.Sin(theta_start) * r));
                Vector2 a2 = new Vector2((float)(Math.Cos(theta_start + theta_a) * r), (float)(Math.Sin(theta_start + theta_a) * r));
                Vector2 a3 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b) * r), (float)(Math.Sin(theta_start + theta_a + theta_b) * r));
                Vector2 a4 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b + theta_a) * r), (float)(Math.Sin(theta_start + theta_a + theta_b + theta_a) * r));

                double w = (Math.Tan(tooth_taper_angle) * tooth_depth) / pitch * theta_total;
                Vector2 b1 = new Vector2((float)(Math.Cos(theta_start + theta_a + w) * (r - tooth_depth)), (float)(Math.Sin(theta_start + theta_a + w) * (r - tooth_depth)));
                Vector2 b2 = new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b - w) * (r - tooth_depth)), (float)(Math.Sin(theta_start + theta_a + theta_b - w) * (r - tooth_depth)));

                CoolDrawing.DrawLine(1, a2 + this.Center, b1 + this.Center, Color.DarkBlue);
                CoolDrawing.DrawLine(1, b1 + this.Center, b2 + this.Center, Color.Red);
                CoolDrawing.DrawLine(1, b2 + this.Center, a3 + this.Center, Color.DarkBlue);

                CoolDrawing.DrawLine(1, a1 + this.Center, a2 + this.Center, Color.Red);
                CoolDrawing.DrawLine(1, a3 + this.Center, a4 + this.Center, Color.DarkGreen);

                Vector2 p = new Vector2((float)(Math.Cos(theta_start) * r), (float)(Math.Sin(theta_start) * r));
                CoolDrawing.DrawLine(1, this.Center, p + this.Center, Color.Black);

                // Tool Path
               
                double tw = tr * Math.Tan(0.25 * Math.PI - tooth_taper_angle / 2) / pitch * theta_total;

                Vector2 t1 = this.Center + new Vector2((float)(Math.Cos(theta_start) * (r + tr)), (float)(Math.Sin(theta_start) * (r + tr)));
                Vector2 t2 = this.Center + new Vector2((float)(Math.Cos(theta_start + theta_a + tw) * (r + tr)), (float)(Math.Sin(theta_start + theta_a + tw) * (r + tr)));
                Vector2 t3 = this.Center + new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b - tw) * (r + tr)), (float)(Math.Sin(theta_start + theta_a + theta_b - tw) * (r + tr)));
                Vector2 t4 = this.Center + new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b + theta_a) * (r + tr)), (float)(Math.Sin(theta_start + theta_a + theta_b + theta_a) * (r + tr)));

                Vector2 tb1 = this.Center + new Vector2((float)(Math.Cos(theta_start + theta_a + w + tw) * (r - tooth_depth + tr)), (float)(Math.Sin(theta_start + theta_a + w + tw) * (r - tooth_depth + tr)));
                Vector2 tb2 = this.Center + new Vector2((float)(Math.Cos(theta_start + theta_a + theta_b - w - tw) * (r - tooth_depth + tr)), (float)(Math.Sin(theta_start + theta_a + theta_b - w - tw) * (r - tooth_depth + tr)));

                //CoolDrawing.DrawLine((float)tr * 2, t1 + this.center, t2 + this.center, toolColor);
                //CoolDrawing.DrawLine((float)tr * 2, t2 + this.center, tb1 + this.center, toolColor);
                //CoolDrawing.DrawLine((float)tr * 2, tb1 + this.center, tb2 + this.center, toolColor);
                //CoolDrawing.DrawLine((float)tr * 2, tb2 + this.center, t3 + this.center, toolColor);
                //CoolDrawing.DrawLine((float)tr * 2, t3 + this.center, t4 + this.center, toolColor);

                outerRout.Points.Add(new Vector2(t1.X, t1.Y));
                outerRout.Points.Add(new Vector2(t2.X, t2.Y));
                outerRout.Points.Add(new Vector2(tb1.X, tb1.Y));
                outerRout.Points.Add(new Vector2(tb2.X, tb2.Y));
                outerRout.Points.Add(new Vector2(t3.X, t3.Y));
                outerRout.Points.Add(new Vector2(t4.X, t4.Y));
            }

            innerRout = new Rout();
            innerRout.Width = width;
            innerRout.Depth = center_hole_depth;

            float circumference = (float)Math.PI * 2 * radius;

            //int _pointCount = pointCount;
            //if (_pointCount <= 1)
            //{
                int _pointCount = (int)(0.5f * circumference / width);
                if (_pointCount <= 12)
                {
                    _pointCount = 12;
                }

            for (int i = 0; i <= _pointCount; i++)
            {
                Vector2 p = new Vector2((float)(Math.Cos(Math.PI * 2 * i / _pointCount) * (radius - tr)), (float)(Math.Sin(Math.PI * 2 * i / _pointCount) * (radius - tr)));
                p += Center;
                innerRout.Points.Add(new Vector2(p.X, p.Y));
            }

            //base.HasChanged = true;
        }

        public override void MouseDrag(Vector2 point)
        {
            base.MouseDrag(point);
            this.ComputePoints();
        }

//        public void MouseDrag(Vector2 point)
//        {
//            Console.WriteLine("Mouse MOved!" + point + ", mouse click point = " + mouseClickPoint);
//            if (isMouseLDown)
//            {
//                Vector2 v = base.roundVec(point);
////new Vector2((int)((point.X) / gridScale + .5) * gridScale,
////                    (int)((point.Y) / gridScale + .5) * gridScale);
//                Vector2 delta = v - mouseClickPoint;

//                center += delta; ;
//                ComputePoints();
                
//                base.mouseClickPoint = point;
//                //mouseClickPoint += delta;
//            }
//        }

        public override void MouseUp(OpenTK.Vector2 location, System.Windows.Forms.Control parent, System.Drawing.Point mousePoint)
        {
            if (isMouseLDown)
            {
                this.center = this.Center;
            }
            if (isMouseRDown)
            {
                ContextMenu c = new ContextMenu();
                c.MenuItems.Add(new MenuItem("Delete Pulley", new EventHandler (DeletePulley)));
                c.Show(parent, mousePoint);
            }
            base.MouseUp(location, parent, mousePoint);
            
            this.ComputePoints();
        }

        private void DeletePulley (Object o, EventArgs e)
        {
            Console.WriteLine("Delete Pulley not implemented apparently...");
        }


        // TODO: fix this interface!
        public float IsPointOnObject(Vector2 location)
        {
            PointF p = new PointF(location.X, location.Y);
            float distance = DistanceTo(p);
            if (distance < radius)
            {
                return distance;
            }
            return float.PositiveInfinity;
        }

        public List<Rout> GetRouts()
        {
            return new List<Rout> { innerRout, outerRout };
        }
    }
}
