using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    public interface IClickable3D
    {
        //void MouseDownLeft(Vector3 location);
        //void MouseDownRight(Vector3 location);
        //void MouseDrag(Vector3 point);
        //void MouseUp(Vector2 location, Control parent, Point mouseLocation);
        //void MouseUp(Vector2 location, Point mouseLocation);

        float IsPointOnObject(Vector3 location, Vector3 direction);
    }
}
