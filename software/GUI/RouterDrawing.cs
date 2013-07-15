using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Robot;
using Serial;
using Utilities;
using Router;

namespace GUI
{

    public partial class RouterDrawing : GLControl
    {
        Color clearColor;

        OpenTK.Vector2 mouseLocation = new Vector2();
        //OpenTK.Box2 viewBox = new Box2(-10, 90, 90, -10);
        //Viewport viewport;
        Viewport3d viewport;

        public event EventHandler DrawScene2D = null;
        public event EventHandler SelectedItemChanged = null;
        float gridScale = 10;

        Timer drawTimer;

        public RouterDrawing()
        {
            this.InitializeComponent();

            this.viewport = new Viewport3d(this);
            this.Resize += new EventHandler(UserControl1_Resize);
            this.MouseMove += new MouseEventHandler(UserControl1_MouseMove);
            this.MouseEnter += new EventHandler(UserControl1_MouseEnter);
            this.MouseLeave += new EventHandler(UserControl1_MouseLeave);
            this.MouseUp += new MouseEventHandler(UserControl1_MouseUp);
            this.MouseDown += new MouseEventHandler(UserControl1_MouseDown);
            this.MouseWheel += new MouseEventHandler(UserControl1_MouseWheel);

            drawTimer = new Timer();
            drawTimer.Tick += new EventHandler(drawTimer_Tick);
            drawTimer.Interval = 100;
            drawTimer.Start();
        }

        bool selectedItemNeedsUpdate = false;
        void drawTimer_Tick(object sender, EventArgs e)
        {
            if (selectedItemNeedsUpdate && SelectedItemChanged != null && clickedObject != null)
            {
                SelectedItemChanged(clickedObject, EventArgs.Empty);
                selectedItemNeedsUpdate = false;
            }
            this.Invalidate();
        }

        class Viewport3d : IClickable
        {
            RouterDrawing parent;
            //Box2 viewPort = new Box2(0, 100, 100, 0);
            Vector2 mouseDownLocation = new Vector2(0, 0);
            bool isMouseLDown = false;
            bool isMouseRDown = false;

            public Matrix4 cameraMatrix = Matrix4.CreateTranslation(0, 0, -5);

            //Rectangle oldParentRectangle;

            public Viewport3d(RouterDrawing parent)
            {
                this.parent = parent;
                parent.Resize += new EventHandler(parent_Resize);
                cameraMatrix = Matrix4.Mult(Matrix4.CreateRotationX(-OpenTK.MathHelper.PiOver4), cameraMatrix);
                //oldParentRectangle = parent.ClientRectangle;
            }

            void parent_Resize(object sender, EventArgs e)
            {
                Console.WriteLine("Parent Resized! " + e.ToString());
                Rectangle newRect = parent.ClientRectangle;
                //float widthChange = ((float)newRect.Width) / ((float)oldParentRectangle.Width);
                //float aspectRatio = ((float)newRect.Width) / ((float)newRect.Height);
                //this.viewPort.Width = this.viewPort.Width * widthChange;
                //this.viewPort.Right = this.viewPort.Left + this.viewPort.Width * widthChange;
                //this.viewPort.Bottom = this.viewPort.Top - this.viewPort.Width / aspectRatio;
                //oldParentRectangle = newRect;
            }
            Matrix4 mouseDownMatrix = Matrix4.Identity;
            public void MouseDownLeft(Vector2 location)
            {
                mouseDownLocation = location / 1000.0f;
                mouseDownMatrix = cameraMatrix;
                isMouseLDown = true;
            }
            public void MouseDownRight(Vector2 location)
            {
                mouseDownLocation = location / 1000.0f;
                mouseDownMatrix = cameraMatrix;
                isMouseRDown = true;
            }

            Vector2 newLocation = new Vector2(0, 0);
            public void MouseDrag(Vector2 location)
            {
                if (isMouseLDown)
                {
                    Vector2 l2 = location / 1000.0f;

                    newLocation = l2;
                    Vector2 delta = l2 - mouseDownLocation;
                    cameraMatrix = Matrix4.Mult(Matrix4.CreateTranslation(delta.X, delta.Y, 0), mouseDownMatrix);
                }
                else if (isMouseRDown)
                {
                }
            }

            public void MouseUp(Vector2 location, Control parent, Point mouseLocation)
            {
                if (isMouseRDown)
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
                    c.Show(parent, mouseLocation);
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
                //PointF p = new PointF(mouseDownLocation.X, mouseDownLocation.Y);
                //r.points.Add(p);
                //r.points.Add(p);
                parent.AddObject(r);
            }

            void AddPulleyClick(object sender, EventArgs e)
            {
                PulleyRout r = new PulleyRout(mouseDownLocation, 100);
                //PointF p = new PointF(mouseDownLocation.X, mouseDownLocation.Y);
                //r.points.Add(p);
                //r.points.Add(p);
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

            internal void Zoom(Vector2 location, int p)
            {
                Vector3 distance = this.ComputeMouseTarget(location);

                float originalZ = cameraMatrix.Row3.Z;

                // Goal is to keep distance the same as before, even after the zoom

                cameraMatrix = Matrix4.Mult(cameraMatrix, Matrix4.CreateTranslation(0, 0, -0.25f * p));

                Vector3 d2 = this.ComputeMouseTarget(location);

                Vector3 difference = distance - d2;

                cameraMatrix = Matrix4.Mult(cameraMatrix, Matrix4.CreateTranslation(-difference.X, -difference.Y, 0));
            }

            public void MouseMove(Vector2 screen_location)
            {
                p = ComputeMouseTarget(screen_location);
                if (isMouseRDown)
                {
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

                    cameraMatrix = mouseDownMatrix;
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

                    cameraMatrix = Matrix4.Mult(Matrix4.CreateTranslation(point), mouseDownMatrix);
                    cameraMatrix = Matrix4.Mult(Matrix4.CreateRotationZ(x), cameraMatrix);
                    Matrix4 m = cameraMatrix;
                    m.Invert();
                    Vector3 left = new Vector3(m.Row0.X, m.Row0.Y, m.Row0.Z);
                    cameraMatrix = Matrix4.Mult(Matrix4.CreateFromAxisAngle(left, y), cameraMatrix);
                    cameraMatrix = Matrix4.Mult(Matrix4.CreateTranslation(-point), cameraMatrix);
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
                Matrix4 m = cameraMatrix;
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
                GL.Vertex3(0, 0, -10);
                GL.Vertex3(0, 0, 10);
                for (float i = -10; i <= 10; i+= 1.0f)
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
                Vector3 left = new Vector3(cameraMatrix.Row0.X, cameraMatrix.Row0.Y, cameraMatrix.Row0.Z);
                //left.Normalize();
                GL.LineWidth(2);
                Vector3 pos = Vector3.TransformPosition(new Vector3(0, 0, 0), cameraMatrix);
                GL.PushMatrix();
                GL.LoadIdentity();
                GL.Begin(BeginMode.Lines);
                GL.Vertex3(pos);
                GL.Vertex3(pos + left);
                GL.End();
                GL.PopMatrix();
                GL.LineWidth(1);
                
                

                

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

        class Viewport : IClickable
        {
            RouterDrawing parent;
            Box2 viewPort = new Box2(0, 100, 100, 0);
            Vector2 mouseDownLocation = new Vector2(0, 0);
            bool isMouseLDown = false;
            bool isMouseRDown = false;
            Rectangle oldParentRectangle;

            public Box2 Box
            {
                get
                {
                    return viewPort;
                }
            }

            public Viewport(RouterDrawing parent)
            {
                this.parent = parent;
                parent.Resize += new EventHandler(parent_Resize);
                oldParentRectangle = parent.ClientRectangle;
            }

            void parent_Resize(object sender, EventArgs e)
            {
                Console.WriteLine("Parent Resized! " + e.ToString());
                Rectangle newRect = parent.ClientRectangle;
                float widthChange = ((float)newRect.Width) / ((float)oldParentRectangle.Width);
                float aspectRatio = ((float)newRect.Width) / ((float)newRect.Height);
                //this.viewPort.Width = this.viewPort.Width * widthChange;
                this.viewPort.Right = this.viewPort.Left + this.viewPort.Width * widthChange;
                this.viewPort.Bottom = this.viewPort.Top - this.viewPort.Width / aspectRatio;
                oldParentRectangle = newRect;
                
            }

            public void MouseDownLeft(Vector2 location)
            {
                mouseDownLocation = location;
                isMouseLDown = true;
            }
            public void MouseDownRight(Vector2 location)
            {
                mouseDownLocation = location;
                isMouseRDown = true;
            }
            public void MouseDrag(Vector2 location)
            {
                if (isMouseLDown)
                {
                    Vector2 delta = location - mouseDownLocation;
                    viewPort.Bottom -= delta.Y;
                    viewPort.Top -= delta.Y;
                    viewPort.Left -= delta.X;
                    viewPort.Right -= delta.X;
                }
            }
            public void MouseUp(Vector2 location, Control parent, Point mouseLocation)
            {
                if (isMouseRDown)
                {
                    ContextMenu c = new ContextMenu();
                    c.MenuItems.Add (new MenuItem ("Add Rout", new EventHandler (AddRoutClick)));
                    c.MenuItems.Add(new MenuItem("Add Circle", new EventHandler(AddCircleClick)));
                    c.MenuItems.Add(new MenuItem("Add Pulley", new EventHandler(AddPulleyClick)));
                    c.MenuItems.Add(new MenuItem("Add Involute Gear", new EventHandler(AddInvoluteGearClick)));
                    c.MenuItems.Add(new MenuItem("Reset View", new EventHandler(ResetViewClick)));
                    c.MenuItems.Add("Item 1");
                    c.MenuItems.Add("Item 2");
                    c.MenuItems.Add("Item 3");
                    c.Show(parent, mouseLocation);
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
                CircleRout r = new CircleRout (mouseDownLocation, 100);
                //PointF p = new PointF(mouseDownLocation.X, mouseDownLocation.Y);
                //r.points.Add(p);
                //r.points.Add(p);
                parent.AddObject(r);
            }

            void AddPulleyClick(object sender, EventArgs e)
            {
                PulleyRout r = new PulleyRout(mouseDownLocation, 100);
                //PointF p = new PointF(mouseDownLocation.X, mouseDownLocation.Y);
                //r.points.Add(p);
                //r.points.Add(p);
                parent.AddObject(r);
            }

            void ResetViewClick(object sender, EventArgs e)
            {
                viewPort = new Box2(0, 100, 100, 0);
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
                float xScale = viewPort.Width / (float)original.Width;
                float yScale = viewPort.Height / (float)original.Height;
                Vector2 location = new Vector2(point.X * xScale + viewPort.Left, viewPort.Height - point.Y * yScale + viewPort.Bottom);
                return location;
            }

            public Point TransformFromViewport(Rectangle original, Vector2 location)
            {
                float xScale = original.Width / (float)viewPort.Width;
                float yScale = original.Height / (float)viewPort.Height;
                location.X -= viewPort.Left;
                location.Y = viewPort.Height - location.Y + viewPort.Bottom;
                Point p = new Point((int)(location.X * xScale), (int)(location.Y * yScale));
                return p;
            }

            public void Zoom(Vector2 location, int p)
            {
                float scale = (float)Math.Pow(1.2, p);

                viewPort.Bottom -= location.Y;
                viewPort.Top -= location.Y;
                viewPort.Right -= location.X;
                viewPort.Left -= location.X;
                float width = viewPort.Width;
                float height = viewPort.Height;

                viewPort.Top *= scale;
                viewPort.Right *= scale;
                viewPort.Left *= scale;
                viewPort.Bottom *= scale;


                //location = location * scale;
                viewPort.Bottom += location.Y;
                viewPort.Top += location.Y;
                viewPort.Right += location.X;
                viewPort.Left += location.X;



            }
        }

        #region Mouse Control


        IClickable clickedObject = null;
        List<Object> objects = new List<Object>();

        void UserControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            //viewport.zoom(e.Delta / 120);
            Vector2 location = viewport.TransformToViewport(ClientRectangle, e.Location);
            viewport.Zoom(new Vector2 (e.Location.X, e.Location.Y), -e.Delta / 120);
            this.Invalidate();
        }
        
        void UserControl1_MouseDown(object sender, MouseEventArgs e)
        {
            // See where the mouse is clicked
            // If on an object, activate that object
            // If on the background, activate scrolling
            Vector2 location = viewport.TransformToViewport(ClientRectangle, e.Location);

            //IClickable last = clickedObject;

            DateTime t = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                float closest = float.PositiveInfinity;
                foreach (Object o in this.objects)
                {
                    IClickable c = o as IClickable;
                    if (c != null)
                    {
                        float distance = c.IsPointOnObject(location);
                        //Console.WriteLine("Distance = " + distance);
                        if (distance < closest)
                        {

                            closest = distance;
                            clickedObject = c;
                        }
                    }
                }
            }
            double time = (DateTime.Now - t).TotalMilliseconds;
            Console.WriteLine ("Took " + time + " Milliseconds");

            //if (clickedObject != null)
            //{
            //    if (last != clickedObject)
            //    {
            //        last.UnHover();
            //    }
            //    clickedObject.Hover(location);
            //}

            if (clickedObject == null)
            {
                clickedObject = viewport as IClickable;
            }

            if (this.SelectedItemChanged != null)
            {
                SelectedItemChanged.Invoke(clickedObject, EventArgs.Empty);
            }
                
            if (e.Button == MouseButtons.Left)
            {
                clickedObject.MouseDownLeft(location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                clickedObject.MouseDownRight(new Vector2 (e.Location.X, e.Location.Y));
            }
            this.Invalidate();
        }

        void UserControl1_MouseUp(object sender, MouseEventArgs e)
        {
            Vector2 location = viewport.TransformToViewport(this.ClientRectangle, e.Location);
            if (clickedObject != null)
            {
                clickedObject.MouseUp(location, this, e.Location);
                //if (this.SelectedItemChanged != null)
                //{
                //    //SelectedItemChanged.Invoke(clickedObject, EventArgs.Empty);
                //}
                clickedObject = null;
            }
        }

        void UserControl1_MouseLeave(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void UserControl1_MouseEnter(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        Vector2 lastMouseLocation = new Vector2(0, 0);
        void UserControl1_MouseMove(object sender, MouseEventArgs e)
        {
            
            mouseLocation = viewport.TransformToViewport(this.ClientRectangle, e.Location);
            viewport.MouseMove(new Vector2 (e.Location.X, e.Location.Y));

            selectedItemNeedsUpdate = true;
            if (e.Button == MouseButtons.None)
            {
                // Nothing pressed, enable hover highlighting?
            }

            if (clickedObject != null)
            {
                //if (e.Button == MouseButtons.Left)
                //{
                    clickedObject.MouseDrag(mouseLocation);
                //}
                //else
                //{
                //    clickedObject.MouseDrag(new Vector2 (e.loc
            }
            this.Invalidate();
            
        }


        #endregion
        
        void UserControl1_Resize(object sender, EventArgs e)
        {
            //GL.Viewport(0, 0, Width, Height);

            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
            //if (this.ClientSize.Height == 0)
            //    this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, 1);

            //GL.Viewport(0, 0, this.ClientSize.Width, this.ClientSize.Height);
        }

        public Color ClearColor
        {
            get { return clearColor; }
            set
            {
                clearColor = value;

                if (!this.DesignMode)
                {
                    MakeCurrent();
                    GL.ClearColor(clearColor);
                }
            }
        }


        #region Drawing

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!this.DesignMode)
            {
                MakeCurrent();


                GL.Viewport(ClientRectangle);

                float aspect = this.ClientSize.Width / (float)this.ClientSize.Height;

                //Matrix4 projection_matrix;
                //Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspect, 1, 64, out projection_matrix);

                //GL.MatrixMode(MatrixMode.Projection);
                //GL.LoadMatrix(ref projection_matrix);

                //GL.Viewport(0, 0, Width, Height);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                
                // 2D Setup
                //GL.Ortho(viewport.Box.Left, viewport.Box.Right, viewport.Box.Bottom, viewport.Box.Top, 1, -1);

                // 3D Setup
                Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 0.1f, 10000.0f);
                GL.LoadMatrix(ref perspective);

                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);

                
                GL.MatrixMode(MatrixMode.Modelview);

                GL.Enable(EnableCap.Blend); // glEnable(GL_BLEND);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha); // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

                GL.ClearColor(Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.LoadIdentity();

                GL.LoadMatrix(ref viewport.cameraMatrix);

                viewport.DrawAxis();
                GL.Scale(.001, .001, .001);
                //GL.Translate(0, 0, -1000);



                GL.Color3(Color.Black);

                //GL.Color3(Color.Black);
                //DrawCrosshairs(mouseLocation, gridScale);
                GL.Color3(Color.LightGray);
                //DrawGrid(viewport.Box, gridScale);
                
                if (DrawScene2D != null)
                {
                    DrawScene2D(this, EventArgs.Empty);
                }



                //DateTime t = DateTime.Now;

                //for (int f = 0; f < 10; f++)
                //{
                    for (int i = objects.Count()-1; i >= 0; i--)
                    {
                        IIsUseful useful = objects[i] as IIsUseful;
                        if (useful != null && !useful.IsUseful())
                        {
                            objects.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            IOpenGLDrawable d = objects[i] as IOpenGLDrawable;
                            if (d != null)
                            {
                                d.Draw();
                            }
                        }
                    }
                //}
                //double time = (DateTime.Now - t).TotalMilliseconds;
                //Console.WriteLine("Drawing Took " + time + " Milliseconds");

                



                GL.Begin(BeginMode.Quads);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 1, 0);
                GL.Vertex3(1, 1, 0);
                GL.Vertex3(1, 0, 0);
                GL.End();
                //Console.WriteLine("OnPaint Called!");
                SwapBuffers();
            }
        }

        private void DrawCrosshairs(Vector2 location, float size)
        {
            // Draw Crosshairs
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(location + new Vector2(0, size/2));
            GL.Vertex2(location + new Vector2(0, -size / 2));
            GL.Vertex2(location + new Vector2(size / 2, 0));
            GL.Vertex2(location + new Vector2(-size / 2, 0));
            GL.End();
        }

        /// <summary>
        /// Draw a grid centered on zero with the current gridScale in all directions within the visible area in viewBox.
        /// </summary>
        private void DrawGrid (Box2 box, float grid)
        {
            GL.Color3(Color.Black);

            GL.Begin(BeginMode.Lines);
            GL.Vertex2(0, box.Top);
            GL.Vertex2(0, box.Bottom);
            GL.Vertex2(box.Left, 0);
            GL.Vertex2(box.Right, 0);
            GL.End();

            GL.Color3(Color.LightGray);

            for (float x = grid; x < box.Right; x += grid)
            {
                GL.Begin(BeginMode.Lines);
                GL.Vertex2(x, box.Top);
                GL.Vertex2(x, box.Bottom);
                GL.End();
            }

            for (float x = -grid; x > box.Left; x -= grid)
            {
                GL.Begin(BeginMode.Lines);
                GL.Vertex2(x, box.Top);
                GL.Vertex2(x, box.Bottom);
                GL.End();
            }

            for (float y = grid; y < box.Top; y += grid)
            {
                GL.Begin(BeginMode.Lines);
                GL.Vertex2(box.Left, y);
                GL.Vertex2(box.Right, y);
                GL.End();
            }

            for (float y = -grid; y > box.Bottom; y -= grid)
            {
                GL.Begin(BeginMode.Lines);
                GL.Vertex2(box.Left, y);
                GL.Vertex2(box.Right, y);
                GL.End();
            }
        }

        #endregion


        internal void AddObject(Object o)
        {
            if (o != null)
            {
                this.objects.Add(o);
            }
        }

        internal List<object> GetObjects()
        {
            return this.objects;
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {

        }
    }
}
