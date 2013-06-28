using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Windows.Forms;
using System.Drawing;

namespace Router
{
    public interface IClickable
    {
        void MouseDownLeft(Vector2 location);
        void MouseDownRight(Vector2 location);
        void MouseDrag(Vector2 point);
        void MouseUp(Vector2 location, Control parent, Point mouseLocation);
        //void MouseUp(Vector2 location, Point mouseLocation);

        float IsPointOnObject(Vector2 location);

        //void Hover(Vector2 location);

        //void UnHover();
    }
}
