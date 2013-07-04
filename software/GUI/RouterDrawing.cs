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
        Viewport viewport;

        public event EventHandler DrawScene2D = null;
        public event EventHandler SelectedItemChanged = null;
        float gridScale = 10;

        Timer drawTimer;

        public RouterDrawing()
        {
            this.InitializeComponent();

            this.viewport = new Viewport(this);
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
            Vector2 location = viewport.TransformToViewport(ClientRectangle, e.Location);
            viewport.Zoom(location, -e.Delta / 120);
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
                clickedObject.MouseDownRight(location);
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

            selectedItemNeedsUpdate = true;
            if (e.Button == MouseButtons.None)
            {
                // Nothing pressed, enable hover highlighting?
            }

            if (clickedObject != null)
            {
                clickedObject.MouseDrag(mouseLocation);
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
                GL.Ortho(viewport.Box.Left, viewport.Box.Right, viewport.Box.Bottom, viewport.Box.Top, 1, -1);

                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);
                

                GL.MatrixMode(MatrixMode.Modelview);

                GL.Enable(EnableCap.Blend); // glEnable(GL_BLEND);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha); // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

                GL.ClearColor(Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.LoadIdentity();



                GL.Color3(Color.Black);

                //GL.Color3(Color.Black);
                //DrawCrosshairs(mouseLocation, gridScale);
                GL.Color3(Color.LightGray);
                DrawGrid(viewport.Box, gridScale);
                
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
