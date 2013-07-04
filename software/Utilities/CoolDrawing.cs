using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Utilities
{
    public class CoolDrawing
    {
        public static void DrawFilledCircle(float radius, Vector2 position, Color color)
        {
            GL.PushMatrix();

            GL.Translate(position.X, position.Y, 0);

            GL.Color4(color);

            GL.Begin(BeginMode.TriangleFan);
            for (int x = 0; x < 20; x++)
            {
                GL.Vertex2 (Math.Sin(Math.PI * x / 10.0f) * radius, Math.Cos(Math.PI * x / 10.0f) * radius);
            }
            GL.End();

            GL.PopMatrix();
        }


        public static void DrawCircle(float radius, Vector2 position, Color color)
        {
            GL.PushMatrix();

            GL.Translate(position.X, position.Y, 0);

            GL.Color4(color);

            GL.Begin(BeginMode.LineLoop);
            for (int x = 0; x < 60; x++)
            {
                GL.Vertex2(Math.Sin(Math.PI * x * 6.0f / 180.0f) * radius, Math.Cos(Math.PI * x * 6.0f / 180.0f) * radius);
            }
            GL.End();

            GL.PopMatrix();
        }


        static float max_cut_depth = -10;
        static float max_rout_depth = -60;
        static float max_move_height = 30;

        public class Line
        {
            public Vector2 a;
            public Vector2 b;
            public Line(Vector2 a, Vector2 b)
            {
                this.a = a;
                this.b = b;
            }
        }
        public static void DrawDepthLine(float width, Line [] lines, float depth)
        {
            DrawLines(width, lines, Color.FromArgb(100, Color.Orange));

            if (depth < 0)
            {
                float w = width * (depth / max_cut_depth);
                if (w > width)
                {
                    w = width;
                }
                DrawLines(w, lines, Color.FromArgb(255, Color.OrangeRed));
            }
            if (depth < max_cut_depth)
            {
                float w = width * (depth / max_rout_depth);
                if (w > width)
                {
                    w = width;
                }
                DrawLines(w, lines, Color.FromArgb(200, Color.DarkRed));
            }
            if (depth > 0)
            {
                float w = width * (depth / max_move_height);
                if (w > width)
                {
                    w = width;
                }
                DrawLines(w, lines, Color.FromArgb(50, Color.Green));
            }
        }

        public static void DrawLines(float width, Line[] lines, Color c)
        {
            foreach (Line l in lines)
            {
                DrawLine(width, l.a, l.b, c);
            }
        }
        public static void DrawLine(double width, Vector2d a, Vector2d b, Color c)
        {
            DrawLine((float)width, new Vector2((float)a.X, (float)a.Y), new Vector2((float)b.X, (float)b.Y), c);
        }
        public static void DrawLine(float width, Vector2 a, Vector2 b, Color c)
        {
            float angle = (float)(180.0f / Math.PI * Math.Atan2(b.Y - a.Y, b.X - a.X));

            GL.PushMatrix();
            GL.Translate(a.X, a.Y, 0);
            GL.Rotate(angle, new Vector3(0, 0, 1));

            float length = (a - b).Length;
            GL.Color4(c);
            FillBox(new Box2(-width / 2, length, width / 2, 0));
            FillHalfCircle(width / 2, -180);
            GL.Translate(length, 0, 0);
            FillHalfCircle(width / 2, 0);

            GL.PopMatrix();
        }

        public static void FillHalfCircle(float radius, float startAngle)
        {
            float start_angle_radians = (float)(Math.PI * startAngle / 180.0f);
            GL.Begin(BeginMode.TriangleFan);
            for (int x = 0; x <= 10; x++)
            {
                GL.Vertex2(Math.Sin(Math.PI * x / 10.0f + start_angle_radians) * radius, Math.Cos(Math.PI * x / 10.0f + start_angle_radians) * radius);
            }
            GL.End();
        }

        public static void FillBox(Box2 b)
        {
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(b.Bottom, b.Left);
            GL.Vertex2(b.Bottom, b.Right);
            GL.Vertex2(b.Top, b.Right);
            GL.Vertex2(b.Top, b.Left);
            GL.End();
        }

        public static void DrawLine(Graphics g, float width, Vector2 a, Vector2 b, Color c)
        {
            System.Drawing.Drawing2D.GraphicsState s = g.Save();

            //g.DrawLine(Pens.DarkGreen, a, b);

            float angle = (float)(180.0f / Math.PI * Math.Atan2(b.Y - a.Y, b.X - a.X));
            Font f = new Font(FontFamily.GenericSansSerif, 80, FontStyle.Bold);

            g.TranslateTransform(a.X, a.Y);
            g.RotateTransform(angle);
            float length = LineMath.Length(a, b);

            Color transparent = Color.FromArgb(200, c.R, c.G, c.B);
            
            //Brush br = new System.Drawing.Drawing2D.HatchBrush (System.Drawing.Drawing2D.HatchStyle.Sphere, Color.FromArgb(128, 100, 50, 25), Color.FromArgb(0, 0, 0, 0));
            Brush br = new SolidBrush(transparent);
            g.FillRectangle(br, new RectangleF (0, -width/2, length, width));
            
            //g.DrawArc(Pens.Black, new RectangleF(-width / 2, -width / 2, width, width), 90, 180);
            g.FillEllipse(Brushes.White, new RectangleF(-width / 2, -width / 2, width, width));
            g.DrawEllipse(Pens.Black, new RectangleF(-width / 2, -width / 2, width, width));
            //g.DrawLine(Pens.Orange, new PointF(0, width / 2.0f), new PointF(length, width / 2.0f));
            //g.DrawLine(Pens.Orange, new PointF(0, -width / 2.0f), new PointF(length, -width / 2.0f));
            //g.TranslateTransform(p2.X - p1.X, p2.Y - p1.Y);

            g.TranslateTransform(length, 0);
            //g.DrawArc(Pens.Black, new RectangleF(-width / 2, -width / 2, width, width), 270, 180);
            g.FillEllipse(Brushes.White, new RectangleF(-width / 2, -width / 2, width, width));
            g.DrawEllipse(Pens.Black, new RectangleF(-width / 2, -width / 2, width, width));

            g.Restore(s);
        }

    }
}
