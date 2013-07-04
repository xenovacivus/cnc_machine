using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using Utilities;

namespace Router
{
    public class Rout : IHasRouts, IClickable, IOpenGLDrawable, IIsUseful
    {
        protected List<Vector2> points;
        protected float width = 25;
        protected float depth = -5;

        public List<Vector2> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }
        public float Depth
        {
            get
            {
                return depth;
            }
            set
            {
                depth = value;
                HasChanged = true;
            }
        }
        public virtual float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                if (width < 1f)
                {
                    width = 1f;
                }
                HasChanged = true;
            }
        }

        protected bool isMouseRDown = false;
        protected bool isMouseLDown = false;

        protected int closestPoint = -1;
        protected bool closestIsPoint = false;
        public bool just_moving = false;

        protected float gridScale = 5;
        protected Vector2 mouseClickPoint;


        protected bool is_display_list_good = false;
        protected int displayList = -1;

        private bool bounding_box_is_good = false;
        Box2 boundingBox = new Box2();

        public Rout()
        {
            Points = new List<Vector2>();
        }

        protected bool HasChanged
        {
            set
            {
                bounding_box_is_good = false;
                is_display_list_good = false;
            }
        }

        #region IDrawable Interface

        public virtual void Draw()
        {
            if (is_display_list_good)
            {
                GL.CallList(displayList);
            }
            else
            {
                if (displayList < 0)
                {
                    displayList = GL.GenLists(1);
                }
        
                GL.NewList(displayList, ListMode.CompileAndExecute);
                List<CoolDrawing.Line> lines = new List<CoolDrawing.Line>();
                if (Points.Count >= 2)
                {
                    
                    for (int i = 0; i < (Points.Count - 1); i++)
                    {
                        lines.Add(new CoolDrawing.Line (new Vector2(Points[i].X, Points[i].Y), new Vector2(Points[i + 1].X, Points[i + 1].Y)));
                        
                    }
                    CoolDrawing.DrawDepthLine(width, lines.ToArray(), this.Depth);
                }
                for (int i = 0; i < Points.Count; i++)
                {
                    Vector2 p = Points[i];
        
        
                    if (closestIsPoint && (i == closestPoint) && isMouseLDown)
                    {
                        CoolDrawing.DrawFilledCircle(width / 2, new Vector2(p.X, p.Y), Color.FromArgb(100, Color.Purple));
                    }
                    else
                    {
                        //CoolDrawing.DrawFilledCircle(width / 2, new Vector2(p.X, p.Y), Color.White);
                    }
                    CoolDrawing.DrawCircle(width / 2, new Vector2(p.X, p.Y), Color.Black);
                }
                GL.EndList();
                is_display_list_good = true;
            }
        }

        #endregion


        
        private Box2 BoundingBox()
        {
            if (!bounding_box_is_good)
            {
                boundingBox = new Box2(float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity);
                foreach (Vector2 p in Points)
                {
                    if (p.X > boundingBox.Right)
                    {
                        boundingBox.Right = p.X;
                    }
                    if (p.X < boundingBox.Left)
                    {
                        boundingBox.Left = p.X;
                    }
                    if (p.Y > boundingBox.Top)
                    {
                        boundingBox.Top = p.Y;
                    }
                    if (p.Y < boundingBox.Bottom)
                    {
                        boundingBox.Bottom = p.Y;
                    }
                }
                boundingBox.Bottom -= width / 2;
                boundingBox.Top += width / 2;
                boundingBox.Left -= width / 2;
                boundingBox.Right += width / 2;
                bounding_box_is_good = true;
            }
            return boundingBox;
        }


        /// <summary>
        /// Returns the distance to the closest line on the rout
        /// </summary>
        /// <param name="p"></param>
        public virtual float DistanceTo(Vector2 p)
        {

            Box2 b = BoundingBox();
            if ((p.X > b.Right || p.X < b.Left) && (p.Y > b.Top || p.Y < b.Bottom))
            {
                return float.PositiveInfinity;
            }

            float closest = float.PositiveInfinity;
            for (int i = 0; i < Points.Count; i++)
            {
                float l = LineMath.Length(Points[i], p);
                if (closest > l)
                {
                    closest = l;
                    closestPoint = i;
                }
            }
            if (closest < width/2)
            {
                closestIsPoint = true;
                return closest;
            }
            closestIsPoint = false;

            closest = float.PositiveInfinity;
            for (int i = 0; i < (Points.Count - 1); i++)
            {
                float newValue = LineMath.DistanceTo(Points[i], Points[i + 1], p);
                if (newValue < closest)
                {
                    closest = newValue;
                    closestPoint = i;
                }
            }
            return closest;
        }

        internal Rout Delete()
        {
            if (closestIsPoint && closestPoint >= 0 && closestPoint < Points.Count)
            {
                Points.RemoveAt(closestPoint);
            }
            else if (closestPoint >=0 && closestPoint < (Points.Count-1))
            {
                Rout r = new Rout();
                for (int i = closestPoint + 1; i < Points.Count; i++)
                {
                    r.Points.Add(Points[i]);
                }
                Points.RemoveRange(closestPoint + 1, r.Points.Count) ;
                if (r.Points.Count > 1)
                    return r;
            }
            HasChanged = true;
            return null;
        }

        public bool IsUseful()
        {
            return Points.Count >= 2;
        }

        public void MouseDownLeft(Vector2 location)
        {
            mouseClickPoint = location;
        
            // Update what's selected, only consider it clicked if there's something to click on
            isMouseLDown = (IsPointOnObject(mouseClickPoint) < width / 2);
        }

        public void MouseDownRight(Vector2 location)
        {
            mouseClickPoint = location;
            isMouseRDown = (IsPointOnObject(mouseClickPoint) < width / 2);
        }
        
        public virtual void MouseDrag(Vector2 point)
        {
            Vector2 v = new Vector2((int)((point.X) / gridScale + .5) * gridScale,
                (int)((point.Y) / gridScale + .5) * gridScale);
        
            mouseClickPoint = v;
        
            if (isMouseLDown)
            {
                if (closestIsPoint)
                {
                    Points[closestPoint] = new Vector2(v.X, v.Y); //(points[closestPoint].X + rounded.X, points[closestPoint].Y + rounded.Y);
                }
                else
                {
                    this.Points.Insert(closestPoint + 1, new Vector2(v.X, v.Y)); //mouseClickPoint.X, mouseClickPoint.Y));
                    this.closestIsPoint = true;
                    this.closestPoint++;
                }
                HasChanged = true;
            }
        }

        
        public virtual void MouseUp(Vector2 location, Control parent, System.Drawing.Point mousePoint)
        {
            if (isMouseRDown)
            {
                ContextMenu c = new ContextMenu();
                c.MenuItems.Add(new MenuItem("Delete Rout", new EventHandler(DeleteRout)));
                c.Show(parent, mousePoint);
            }
            isMouseRDown = false;
            isMouseLDown = false;
            HasChanged = true;
        }

        protected void DeleteRout (object sender, EventArgs e)
        {
            Points = new List<Vector2>(); // Remove all points... any list containing this item should check if there are no points, and remove if necessary.
            Console.WriteLine("Delete Rout Clicked!");
            HasChanged = true;
        }

        public float IsPointOnObject(Vector2 location)
        {
            Vector2 p = new Vector2(location.X, location.Y);
            float distance = DistanceTo(p);
            if (distance < width / 2)
            {
                return distance;
            }
            return float.PositiveInfinity;
        }

        public List<Rout> GetRouts()
        {
            return new List<Rout> { this };
        }
    }
}
