using OpenTK;
using OpenTK.Graphics.OpenGL;
using Router;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace Router
{
    public class InvoluteGear : SimpleClickable, IHasRouts, IClickable, IOpenGLDrawable //SimpleClickable, IHasRouts, IClickable, IOpenGLDrawable
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

        protected float radius; // computed
        float width = 45; // input param

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

        public InvoluteGear()
        {
            ComputePoints();
        }

        public InvoluteGear(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
            ComputePoints();
        }

        float addendum = 75;
        [DisplayNameAttribute("Addendum")]
        public float Addendum
        {
            get
            {
                return addendum;
            }
            set
            {
                addendum = value;
                this.ComputePoints();
            }
        }

        float dedendum = 75;
        [DisplayNameAttribute("Dedendum")]
        public float Dedendum
        {
            get
            {
                return dedendum;
            }
            set
            {
                dedendum = value;
                this.ComputePoints();
            }
        }

        float pressure_angle = 20;
        [DisplayNameAttribute("Pressure Angle")]
        public float PressureAngle
        {
            get
            {
                return pressure_angle;
            }
            set
            {
                pressure_angle = value;
                this.ComputePoints();
            }
        }

        float circular_pitch = 200;
        [DisplayNameAttribute("Circular Pitch")]
        public float CircularPitch
        {
            get
            {
                return circular_pitch;
            }
            set
            {
                circular_pitch = value;
                if (circular_pitch < 10)
                {
                    circular_pitch = 10;
                }
                this.ComputePoints();
            }
        }

        int teeth = 15;
        [DisplayNameAttribute("Number of Teeth")]
        public int Teeth
        {
            get
            {
                return teeth;
            }
            set
            {
                teeth = value;
                if (teeth < 2)
                {
                    teeth = 2;
                }
                this.ComputePoints();
            }
        }

        float base_rotation = 0;
        [DisplayNameAttribute("Rotation")]
        private float Rotation
        {
            get
            {
                return base_rotation;
            }
            set
            {
                base_rotation = value;
                this.ComputePoints();
            }
        }


        public double Sind(double angle_degrees)
        {
            return Math.Sin(Radians(angle_degrees));
        }

        public double Cosd(double angle_degrees)
        {
            return Math.Cos(Radians(angle_degrees));
        }

        public double Radians(double degrees)
        {
            return Math.PI * degrees / 180.0d;
        }

        public double Degrees(double radians)
        {
            return 180.0d * radians / Math.PI;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theta_degrees"></param>
        /// <param name="base_degrees"></param>
        /// <param name="base_radius"></param>
        /// <param name="distance">Perpendicular distance away from the involute curve</param>
        /// <returns></returns>
        private Vector2 ComputeInvolutePoint(double theta_degrees, double base_degrees, double base_radius, double distance)
        {
            Vector2 radial = new Vector2(
                (float)Cosd(theta_degrees + base_degrees),
                (float)Sind(theta_degrees + base_degrees));

            Vector2 tangential = radial.PerpendicularRight;
            double s = Radians(theta_degrees) * base_radius + distance;

            return Vector2.Multiply(radial, (float)base_radius) + Vector2.Multiply(tangential, (float)s);
        }

        public void Draw()
        {
            float contact_circumference = circular_pitch * teeth;

            double contact_radius = contact_circumference / (Math.PI * 2);
            double base_radius = contact_radius * Cosd(pressure_angle);

            double outer_radius = contact_radius + addendum;
            double inner_radius = contact_radius - dedendum;
            
            CoolDrawing.DrawCircle((float)base_radius, Center, Color.Black);
            CoolDrawing.DrawCircle((float)contact_radius, Center, Color.Orange);
            CoolDrawing.DrawCircle((float)outer_radius, Center, Color.DarkBlue);
            CoolDrawing.DrawCircle((float)inner_radius, Center, Color.DarkBlue);

            double theta_to_contact_radius = Degrees(Sind(pressure_angle) / Cosd(pressure_angle));

            List<Rout> Routs = new List<Rout>();

            float a_radians = (float)(Math.PI * (0 - (theta_to_contact_radius - pressure_angle)) / 180.0f);
            
            // Draw an involute curve off the surface of the gear
            GL.Begin(BeginMode.LineStrip);
            GL.Color3(Color.Black);

            // Create routs to match the drawn line
            Rout rt = new Rout();
            rt.Width = this.width;

            // Line from inner radius to start of base circle
            double sin_theta = (float)Math.Sin(a_radians);
            double cos_theta = (float)Math.Cos(a_radians);

            if (inner_radius < base_radius)
            {
                double x = inner_radius * cos_theta + this.X;
                double y = inner_radius * sin_theta + this.Y;
                GL.Vertex2(x, y);
                x = base_radius * cos_theta + this.X;
                y = base_radius * sin_theta + this.Y;
                GL.Vertex2(x, y);
            }

            // Degrees to rotate while drawing involute curve from base_radius to outer_radius
            float distance = (float)Degrees(Math.Sqrt(outer_radius * outer_radius - base_radius * base_radius) / base_radius);
                
            // Draw an involute line from base_radius to outer_radius
            for (double theta = 0; theta <= (distance + .0001); theta += (distance / 20.0f))
            {
                Vector2 point = ComputeInvolutePoint(theta, Degrees(a_radians), base_radius, 0);
                GL.Vertex2(point + Center);
            }
            GL.End();

            //outerRout.Draw();
            foreach (Rout a in this.GetRouts())
            {
                a.Draw();
            }
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
            float contact_circumference = circular_pitch * teeth;

            double contact_radius = contact_circumference / (Math.PI * 2);
            double base_radius = contact_radius * Cosd(pressure_angle);

            double outer_radius = contact_radius + addendum;
            double inner_radius = contact_radius - dedendum;

            double theta_to_contact_radius = Degrees(Sind(pressure_angle) / Cosd(pressure_angle));

            List<Rout> Routs = new List<Rout>();

            float a_radians = (float)(Math.PI * (0 - (theta_to_contact_radius - pressure_angle)) / 180.0f);

            // Create routs to match the drawn line
            Rout rt = new Rout();
            rt.Width = this.width;

            // Line from inner radius to start of base circle
            double sin_theta = (float)Math.Sin(a_radians);
            double cos_theta = (float)Math.Cos(a_radians);

            // Tails into gear for dedendum
            if (inner_radius < base_radius)
            {
                Vector2 radial = new Vector2((float)cos_theta, (float)sin_theta);
                Vector2 tangential = new Vector2((float)sin_theta, (float)(-cos_theta));
                tangential = radial.PerpendicularRight;
                rt.Points.Add(Vector2.Multiply(tangential, rt.Width / 2) + Center + Vector2.Multiply(radial, (float)(inner_radius + rt.Width / 2)));
                rt.Points.Add(Vector2.Multiply(tangential, rt.Width / 2) + Center + Vector2.Multiply(radial, (float)base_radius));
            }

            // Degrees to rotate while drawing involute curve from base_radius to outer_radius
            float distance = (float)Degrees(Math.Sqrt(outer_radius * outer_radius - base_radius * base_radius) / base_radius);

            for (double theta = 0; theta <= distance; theta += 3.0f/Sind(theta + 15))
            {
                Vector2 routPoint = ComputeInvolutePoint(theta, Degrees(a_radians), base_radius, rt.Width / 2);
                rt.Points.Add(Center + routPoint);
            }

            Vector2 lastDirection = ComputeInvolutePoint(distance, Degrees(a_radians), base_radius, rt.Width / 2);
            rt.Points.Add(Center + lastDirection);
            lastDirection = Vector2.Normalize(lastDirection);

            float spacing_degrees = 360 / teeth;
            float d2 = (float)(outer_radius + rt.Width / 2);
            Vector2 radial2 = new Vector2(
                (float)Cosd(spacing_degrees / 4),
                (float)Sind(spacing_degrees / 4));
            rt.Points.Add(Center + Vector2.Multiply(lastDirection, d2));

            // Flip the points in rt about the line radial2
            List<Vector2> back = new List<Vector2>();
            foreach (Vector2 v in rt.Points)
            {
                Vector2 original = v - Center;
                float parallel = Vector2.Dot(radial2, original);
                float perpendicular = Vector2.Dot(radial2.PerpendicularRight, original);
                Vector2 result = Vector2.Multiply(radial2, parallel) + Vector2.Multiply(radial2.PerpendicularRight, -perpendicular);
                back.Insert(0, result + Center);
            }

            foreach (Vector2 p in back)
            {
                rt.Points.Add(p);
            }

            Rout rout = new Rout();
            rout.Width = rt.Width;
            for (int tooth = 0; tooth < teeth; tooth++)
            {
                Quaternion q = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), tooth * OpenTK.MathHelper.TwoPi / (float)teeth + (float)Radians(base_rotation));
                foreach (Vector2 v in rt.Points)
                {
                    Vector2 original = v - Center;
                    Vector2 result = Vector2.Transform(original, q);
                    rout.Points.Add(result + Center);
                }
            }
            // And back to the first point!  TODO: Rotate this point too...
            rout.Points.Add(rt.Points[0]);

            
            this.outerRout = rout;
            outerRout.Depth = this.depth;


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

            float tr = width / 2;
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
