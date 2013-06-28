using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
//using Machine.Interfaces;
using System.Windows.Forms;

namespace Router
{
    public class SimpleClickable : IClickable
    {
        protected bool isMouseRDown = false;
        protected bool isMouseLDown = false;

        protected int closestPoint = -1;
        protected bool closestIsPoint = false;
        public bool just_moving = false;

        protected float gridScale = 5;
        protected Vector2 mouseClickPoint;
        protected Vector2 mousePoint;
        protected Vector2 MouseDragVector
        {
            get
            {
                return roundVec(mousePoint - mouseClickPoint);
            }
        }


        protected Vector2 roundVec (Vector2 v)
        {
            Vector2 v2 = new Vector2((int)((v.X) / gridScale + .5) * gridScale,
                    (int)((v.Y) / gridScale + .5) * gridScale);
            return v2;
        }

        public void MouseDownLeft(Vector2 location)
        {
            Console.WriteLine("MouseDownLeft" + location);
            mouseClickPoint = location;
            mousePoint = location;
            isMouseLDown = true;
        }

        public void MouseDownRight(Vector2 location)
        {
            mouseClickPoint = location;
            mousePoint = location;
            isMouseRDown = true;
        }

        public virtual void MouseDrag(Vector2 point)
        {
            mousePoint = point;


            //Vector2 v = new Vector2((int)((point.X) / gridScale + .5) * gridScale,
            //    (int)((point.Y) / gridScale + .5) * gridScale);

            //mouseClickPoint = v;
        }

        
        public virtual void MouseUp(Vector2 location, Control parent, Point mousePoint)
        {
            isMouseRDown = false;
            isMouseLDown = false;
            mouseClickPoint = location;
            this.mousePoint = location;
        }
        public float IsPointOnObject(Vector2 p)
        {
            return float.PositiveInfinity;
        }
    }
}
