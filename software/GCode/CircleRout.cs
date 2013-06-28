using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.Windows.Forms;
using Utilities;

namespace Router
{
    public class CircleRout : Rout
    {
        protected Vector2 center = new Vector2 (0, 0);
        protected float radius = 50;
        protected int pointCount;

        public override float Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                this.ComputePoints();
            }
        }

        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
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
                return center.X;
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
                return center.Y;
            }
            set
            {
                center.Y = value;
                this.ComputePoints();
            }
        }

        public int PointCount
        {
            set
            {
                this.pointCount = value;
                this.ComputePoints();
            }
            get
            {
                return this.pointCount;
            }
        }

        public CircleRout()
        {
            ComputePoints();
        }

        public CircleRout(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
            ComputePoints();
        }

        public override void Draw()
        {
            for (int i = 0; i < (Points.Count - 1); i++)
            {
                Color c = Color.OrangeRed;
                CoolDrawing.DrawLine(width, new Vector2(Points[i].X, Points[i].Y), new Vector2(Points[i + 1].X, Points[i + 1].Y), c);
                CoolDrawing.DrawFilledCircle(width / 2, new Vector2(Points[i].X, Points[i].Y), c);
            }

            CoolDrawing.DrawFilledCircle(width / 2, center, Color.LightGreen);
            CoolDrawing.DrawCircle(width / 2, center, Color.Black);
        }

        public override float DistanceTo(Vector2 p)
        {
            float d = base.DistanceTo(p);
            Vector2 v = new Vector2(p.X, p.Y);
            float length = (center - v).Length;
            if (length < width / 2 && length < d)
            {
                d = length;
                base.closestIsPoint = false;
            }
            else
            {
                base.closestIsPoint = true;
            }
            return d;
        }

        private void ComputePoints()
        {
            this.Points = new List<Vector2>();

            float circumference = (float)Math.PI * 2 * radius;

            


            int _pointCount = pointCount;
            if (_pointCount <= 1)
            {
                pointCount = (int)(0.5f * circumference / width);
                if (_pointCount <= 12)
                {
                    _pointCount = 12;
                }
            }


            for (int i = 0; i <= _pointCount; i++)
            {
                Vector2 p = new Vector2((float)Math.Cos(Math.PI * 2 * i / _pointCount) * radius, (float)Math.Sin(Math.PI * 2 * i / _pointCount) * radius);
                p += center;
                Points.Add(new Vector2(p.X, p.Y));
            }

            base.HasChanged = true;
        }

        public override void MouseDrag(Vector2 point)
        {
            if (isMouseLDown)
            {
                Vector2 v = new Vector2((int)((point.X) / gridScale + .5) * gridScale,
                    (int)((point.Y) / gridScale + .5) * gridScale);
                Vector2 delta = v - mouseClickPoint;
                

                if (closestIsPoint)
                {
                    radius = (center - point).Length;
                    radius = gridScale * ((int)(radius / gridScale + .5));
                    ComputePoints();
                }
                else
                {
                    center = v;
                    ComputePoints();
                }
                
                base.mouseClickPoint = v;
            }
        }

        public override void MouseUp(OpenTK.Vector2 location, Control parent, System.Drawing.Point mousePoint)
        {
            if (isMouseRDown && null != parent)
            {
                ContextMenu c = new ContextMenu();
                c.MenuItems.Add(new MenuItem("Delete Circle", new EventHandler(DeleteRout)));
                c.Show(parent, mousePoint);
            }
            isMouseRDown = false;
            isMouseLDown = false;
        }
    }
}
