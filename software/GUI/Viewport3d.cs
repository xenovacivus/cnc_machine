using OpenTK;
using OpenTK.Graphics.OpenGL;
using Router;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public class Viewport3d : IClickable
    {
        RouterDrawing parent;
        //Box2 viewPort = new Box2(0, 100, 100, 0);
        Vector2 mouseDownLocation = new Vector2(0, 0);
        bool isMouseLDown = false;
        bool isMouseRDown = false;
        bool wasMouseDragged = false;

        public Matrix4 viewMatrix = Matrix4.CreateTranslation(0, 0, -5);

        public Viewport3d(RouterDrawing parent)
        {
            this.parent = parent;
            parent.Resize += new EventHandler(parent_Resize);
            viewMatrix = Matrix4.Mult(Matrix4.CreateRotationX(-OpenTK.MathHelper.PiOver4), viewMatrix);
        }

        void parent_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("Parent Resized! " + e.ToString());
            Rectangle newRect = parent.ClientRectangle;
        }
        Matrix4 mouseDownMatrix = Matrix4.Identity;
        public void MouseDownLeft(Vector2 location)
        {
            mouseDownLocation = location / 1000.0f;
            mouseDownMatrix = viewMatrix;
            isMouseLDown = true;
            wasMouseDragged = false;
        }
        public void MouseDownRight(Vector2 location)
        {
            mouseDownLocation = location / 1000.0f;
            mouseDownMatrix = viewMatrix;
            isMouseRDown = true;
            wasMouseDragged = false;
        }

        Vector2 newLocation = new Vector2(0, 0);
        public void MouseDrag(Vector2 location)
        {
            // TODO: Fix this behavior
            //if ((location / 1000 - mouseDownLocation).Length > float.Epsilon)
            //{
            //    wasMouseDragged = true;
            //}
            if (isMouseLDown)
            {
                Vector2 l2 = location / 1000.0f;

                newLocation = l2;
                Vector2 delta = l2 - mouseDownLocation;
                viewMatrix = Matrix4.Mult(Matrix4.CreateTranslation(delta.X, delta.Y, 0), mouseDownMatrix);
            }
            else if (isMouseRDown)
            {
            }
        }

        public void MouseUp(Vector2 location, Control parent, Point mouseLocation)
        {
            if (isMouseRDown && !wasMouseDragged)
            {
                ContextMenu c = new ContextMenu();
                c.MenuItems.Add(new MenuItem("Add Rout", new EventHandler(AddRoutClick)));
                c.MenuItems.Add(new MenuItem("Add Circle", new EventHandler(AddCircleClick)));
                c.MenuItems.Add(new MenuItem("Add Pulley", new EventHandler(AddPulleyClick)));
                c.MenuItems.Add(new MenuItem("Add Involute Gear", new EventHandler(AddInvoluteGearClick)));
                c.MenuItems.Add(new MenuItem("Reset View", new EventHandler(ResetViewClick)));
                c.MenuItems.Add("Item 1");
                c.MenuItems.Add("Item 2");
                c.MenuItems.Add("Item 3");
                //c.Show(parent, mouseLocation);
            }
            isMouseLDown = false;
            isMouseRDown = false;
        }

        private void AddInvoluteGearClick(object sender, EventArgs e)
        {
            InvoluteGear r = new InvoluteGear(mouseDownLocation, 100);
            parent.AddObject(r);
        }

        void AddRoutClick(object sender, EventArgs e)
        {
            Rout r = new Rout();
            Vector2 p = new Vector2(mouseDownLocation.X, mouseDownLocation.Y);
            r.Points.Add(p);
            r.Points.Add(p);
            parent.AddObject(r);
        }

        void AddCircleClick(object sender, EventArgs e)
        {
            CircleRout r = new CircleRout(mouseDownLocation, 100);
            parent.AddObject(r);
        }

        void AddPulleyClick(object sender, EventArgs e)
        {
            PulleyRout r = new PulleyRout(mouseDownLocation, 100);
            parent.AddObject(r);
        }

        void ResetViewClick(object sender, EventArgs e)
        {
            //viewPort = new Box2(0, 100, 100, 0);
        }

        public float IsPointOnObject(Vector2 location)
        {
            return 0;
        }

        /// <summary>
        /// Translate a screen point to a point in the viewport
        /// </summary>
        /// <param name="mousePoint"></param>
        public Vector2 TransformToViewport(Rectangle original, Point point)
        {
            Vector3 location = this.ComputeMouseTarget(new Vector2(point.X, point.Y));
            location = location * 1000;
            return new Vector2(location.X, location.Y);
        }

        private Vector3 CameraPosition
        {
            get
            {
                Matrix4 m = viewMatrix;
                m.Invert();
                return new Vector3(m.Row3.X, m.Row3.Y, m.Row3.Z);
            }
        }

        Vector3 point = new Vector3 (0, 0, 0);
        internal void Zoom(Vector2 location, int p)
        {
            //Vector3 distance = this.ComputeMouseTarget(location);
            Vector3 target = this.ComputeMouseTarget(location);
            point = target;
            // Vector pointing toward the target
            Vector3 direction = target - CameraPosition;

            viewMatrix = Matrix4.Mult(Matrix4.CreateTranslation(direction * .05f * p), viewMatrix);

            //float originalZ = cameraMatrix.Row3.Z;
            //
            //// Goal is to keep distance the same as before, even after the zoom
            //
            //cameraMatrix = Matrix4.Mult(cameraMatrix, Matrix4.CreateTranslation(0, 0, -0.25f * p));
            //
            //Vector3 d2 = this.ComputeMouseTarget(location);
            //
            //Vector3 difference = distance - d2;
            //
            //cameraMatrix = Matrix4.Mult(cameraMatrix, Matrix4.CreateTranslation(-difference.X, -difference.Y, 0));
        }

        public void MouseMove(Vector2 screen_location)
        {
            p = ComputeMouseTarget(screen_location);
            if (isMouseRDown)
            {
                //wasMouseDragged = true;
                // Rotate Scene

                // Rotate about the point in the center of the screen



                // location is mouse position on the screen
                // Screen is 45 degrees up/down
                Rectangle r = parent.ClientRectangle;
                if (r.Height == 0)
                {
                    r.Height = 1;
                }

                Vector2 center = new Vector2(r.Width / 2, r.Height / 2);

                viewMatrix = mouseDownMatrix;
                Vector3 point = this.ComputeMouseTarget(center);

                float y = -screen_location.Y / (float)r.Height + 0.5f;
                float x = screen_location.X / (float)r.Width - 0.5f;


                Vector2 l2 = mouseDownLocation * 1000;
                float y2 = -l2.Y / (float)r.Height + 0.5f;
                float x2 = l2.X / (float)r.Width - 0.5f;

                float aspect = AspectRatio;

                //Console.WriteLine("pos = {0}, {1}", (x2 - x) * aspect, y2 - y);

                x = (x2 - x) * aspect;
                y = (y2 - y);
                x = x * 4;
                y = y * 4;

                viewMatrix = Matrix4.Mult(Matrix4.CreateTranslation(point), mouseDownMatrix);
                viewMatrix = Matrix4.Mult(Matrix4.CreateRotationZ(x), viewMatrix);
                Matrix4 m = viewMatrix;
                m.Invert();
                Vector3 left = new Vector3(m.Row0.X, m.Row0.Y, m.Row0.Z);
                viewMatrix = Matrix4.Mult(Matrix4.CreateFromAxisAngle(left, y), viewMatrix);
                viewMatrix = Matrix4.Mult(Matrix4.CreateTranslation(-point), viewMatrix);
            }
            //Console.WriteLine("Mouse Position = {0}", p);
        }

        private float AspectRatio
        {
            get
            {
                Rectangle r = parent.ClientRectangle;
                if (r.Height == 0)
                {
                    r.Height = 1;
                }
                return r.Width / (float)r.Height;
            }
        }


        Vector3 test = new Vector3(0, 0, 0);
        public Vector3 ComputeMouseTarget(Vector2 screen_location)
        {
            Matrix4 m = viewMatrix;
            if (this.isMouseLDown)
            {
                m = mouseDownMatrix;
            }

            Rectangle r = parent.ClientRectangle;

            if (r.Height == 0)
            {
                r.Height = 1;
            }

            // Scale the x & y positions such that the point (0, 0) is in the center and (0.5, 0.5) is in the upper right
            float y = -screen_location.Y / (float)r.Height + 0.5f;
            float x = screen_location.X / (float)r.Width - 0.5f;

            float a = (float)(0.5f / Math.Tan(Math.PI / 8.0d));
            float aspect = r.Width / (float)r.Height;

            Vector3 v = new Vector3(x * aspect / a, y / a, -1);// *5;// m.Row3.Z;
            //Vector3 v = new Vector3(-x * aspect / a, -y * aspect / a, 1);// *5;// m.Row3.Z;

            test = v;

            // Make the vector intersect with the plane at (0, 0, 0) and normal (0, 0, 1)

            Vector3 planeNormal = new Vector3(0, 0, 1);
            Vector3 pointOnPlane = new Vector3(0, 0, 0);

            pointOnPlane = Vector3.Transform(pointOnPlane, m);
            planeNormal = Vector3.Transform(planeNormal, m) - Vector3.Transform(new Vector3(0, 0, 0), m);

            Vector3 cameraPosition = new Vector3(m.Row3.X, m.Row3.Y, m.Row3.Z);
            float distanceToPlane = Vector3.Dot(pointOnPlane, planeNormal); // -Vector3.Dot(cameraPosition, planeNormal);
            v = Vector3.Normalize(v);
            //v.Normalize();
            float distanceAlongLine = distanceToPlane / (Vector3.Dot(planeNormal, v));

            v = v * distanceAlongLine;

            v = Vector3.Transform(v, Matrix4.Invert(m));

            //Console.WriteLine("DistanceAlongLine = " + distanceAlongLine + ", " + v);



            //   Vector3 matrixForward = new Vector3(m.Column0.X, m.Column0.Y, m.Column0.Z);
            //   float distance = 1.0f / (Vector3.Dot(matrixForward, new Vector3(0, 1, 0)));

            //v = v - new Vector3(m.Row3.X, m.Row3.Y, 0);
            return v;
        }

        Vector3 p;

        public void DrawAxis()
        {

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(-10, 0, 0);
            GL.Vertex3(10, 0, 0);
            GL.Color3(Color.Green);
            GL.Vertex3(0, -10, 0);
            GL.Vertex3(0, 10, 0);
            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 10);
            for (float i = -10; i <= 10; i += 1.0f)
            {
                GL.Color3(Color.LightPink);
                GL.Vertex3(-10, i, 0);
                GL.Vertex3(10, i, 0);
                GL.Color3(Color.LightGreen);
                GL.Vertex3(i, -10, 0);
                GL.Vertex3(i, 10, 0);
                //GL.Color3(Color.Red);
                //GL.Vertex3(0, 0, -10);
                //GL.Vertex3(0, 0, 10);
            }


            GL.End();

            GL.Color3(Color.BlueViolet);
            Vector3 left = new Vector3(viewMatrix.Row0.X, viewMatrix.Row0.Y, viewMatrix.Row0.Z);
            //left.Normalize();
            GL.LineWidth(2);
            Vector3 pos = Vector3.TransformPosition(new Vector3(0, 0, 0), viewMatrix);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(pos);
            GL.Vertex3(pos + left);
            GL.End();
            GL.PopMatrix();
            GL.LineWidth(1);

            GL.PointSize(4);
            GL.Begin(BeginMode.Points);
            GL.Vertex3(point);
            GL.End();
            GL.PointSize(1);





            //GL.PushMatrix();
            //GL.LoadIdentity();
            GL.Begin(BeginMode.Lines);
            //GL.Vertex3(p + new Vector3(10, 0, 0));
            //GL.Vertex3(p + new Vector3(-10, 0, 0));

            // if (!isMouseLDown)
            // {
            //     GL.Color3(Color.Black);
            //     GL.Vertex3(new Vector3(10, p.Y, 0));
            //     GL.Vertex3(new Vector3(-10, p.Y, 0));
            // 
            //     GL.Vertex3(new Vector3(p.X, -10, 0));
            //     GL.Vertex3(new Vector3(p.X, 10, 0));
            // }

            GL.End();


            //GL.PointSize(5);
            //GL.Color3(Color.DarkOrange);
            //GL.Begin(BeginMode.Points);
            //GL.Vertex3(p);
            //GL.End();
            //GL.PointSize(1);
            //
            //
            //GL.PushMatrix();
            //GL.LoadIdentity();
            //GL.PointSize(5);
            //GL.Begin(BeginMode.Points);
            //GL.Vertex3(test);
            //GL.End();
            //GL.PointSize(1);
            ////GL.PopMatrix();
            //GL.PopMatrix();
        }
    }

}
