using ClipperLib;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Router;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace GUI
{
    class ObjLoader: IOpenGLDrawable, IClickable3D
    {
        public class Intersect
        {
            public bool intersects;
            public Vector3 intersectPoint;
            public float distance;
        }

        List<Vector3> vertices;
        public class Face
        {
            public List<Vector3> vertices = new List<Vector3>();
            public Face()
            {
            }
            public Vector3 Normal
            {
                get
                {
                    if (vertices.Count() >= 3)
                    {
                        Vector3 v = Vector3.Cross(vertices[1] - vertices[2], vertices[0] - vertices[1]);
                        v.Normalize();
                        return v;
                    }
                    return new Vector3(0, 1, 0);
                }
            }
            public Intersect CalculateIntersect (Vector3 location, Vector3 direction)
            {
                Intersect i = new Intersect();

                i.intersectPoint = new Vector3(0, 0, 0);
                i.intersects = false;

                Plane plane = new Plane(this);

                float distance = plane.Distance(location, direction);
                //Console.WriteLine("Distance = {0}", distance);

                i.distance = distance;
                i.intersectPoint = distance * direction + location;
                if (distance < 0)
                {
                    i.intersects = false;
                }
                else
                {
                    i.intersects = ObjLoader.IsPointInPolygon(this, i.intersectPoint);
                }

                return i;
            }
        }
        public List<Face> faces;

        public class PolyLine
        {
            public List<Vector3> points = new List<Vector3>();
            public override string ToString()
            {
                string s = ""
                    ;
                foreach (Vector3 v in points)
                {
                    s = s + " " + v / 125;
                }
                return s;
            }
        }

        public class Plane
        {
            public Plane()
            {
            }

            public Plane(Face f)
            {
                point = f.vertices[0];
                normal = f.Normal;
            }

            public Vector3 point = new Vector3 (0, 0, 0);
            public Vector3 normal = new Vector3(1, 0, 0);
            public float Distance(Vector3 location)
            {
                float scalar = Vector3.Dot(normal, point);
                return Vector3.Dot(normal, location) - scalar;
            }
            /// <summary>
            /// Get the distance from location along the vector direction until the plane.
            /// The result is positive if the point is forward along the vector, negative otherwise.
            /// </summary>
            /// <param name="location"></param>
            /// <param name="direction"></param>
            /// <returns></returns>
            public float Distance(Vector3 location, Vector3 direction)
            {
                float distance = Distance(location);

                distance = Math.Abs(distance / Vector3.Dot(direction, normal));
                if (Math.Abs(Distance(location + direction * distance)) > Math.Abs(Distance(location - direction * distance)))
                {
                    distance = -distance;
                }

                return distance;
            }
        }

        static float DistanceToCylinder(Vector3 cy1, Vector3 cy2, Vector3 point)
        {
            float distance = 0;

            if ((cy1 - cy2).Length < 0.0001)
            {
                return (cy1 - point).Length;
            }

            Plane p = new Plane();
            p.normal = cy1 - cy2;
            p.normal.Normalize();
            p.point = cy2;

            float d1 = p.Distance(point);

            p.normal = -p.normal;
            p.point = cy1;
            float d2 = p.Distance(point);

            if (float.IsNaN(d2) || float.IsNaN(d1))
            {
            }

            if (Math.Sign(d1) == Math.Sign(d2))
            {
                // Inside Endpoints
                Vector3 pt = point - p.normal * d2;
                distance = (pt - p.point).Length;
            }
            else
            {
                distance = (float)Math.Min((point - cy1).Length, (point - cy2).Length);
            }

            return distance;
        }

        private void DrawCone1(Vector3 p1, Vector3 p2)
        {
            Vector3 normal = p2 - p1;
            normal.Normalize();
            DrawCone(p1, normal);
        }

        private void DrawCone(Vector3 point, Vector3 direction)
        {
            float size = 100;
            Vector3 c = new Vector3(0, 1, 0);
            if (Math.Abs(direction.Y) > 0.5)
            {
                c = new Vector3(1, 0, 0);
            }
            Vector3 perp1 = Vector3.Cross(direction, c);
            perp1.Normalize();
            Vector3 perp2 = Vector3.Cross(perp1, direction);
            GL.Begin(BeginMode.TriangleFan);
            
            GL.Normal3(direction);
            GL.Vertex3(point + direction * size);
            for (float theta = 0; theta < OpenTK.MathHelper.TwoPi; theta += OpenTK.MathHelper.TwoPi/36)
            {
                float x = (float)Math.Sin(theta);
                float y = (float)Math.Cos(theta);
                Vector3 tail = (perp1 * x + perp2 * y) * size/3 + point;
                GL.Normal3(perp1 * x + perp2 * y);
                GL.Vertex3(tail);
            }
            GL.End();
        }

        private void Slice(Plane p)
        {
            float epsilon = 0.01f; // TODO: compute proper epsilon value
            
            List<PolyLine> polyLines = new List<PolyLine>();
            List<Vector3> all_points = new List<Vector3>();
            foreach (Face f in this.faces)
            {
                PolyLine newLine = TrianglePlaneIntersect(f, p);

                // Only add lines with exactly 2 points - others are a no match or error
                if (newLine.points.Count() == 2 && (newLine.points[0] - newLine.points[1]).Length> epsilon)
                {
                    polyLines.Add(newLine);

                    // Add the vertices to the all_points list - only need to add the first one, the tail will be the head of another point
                    bool matched = false;
                    foreach (Vector3 point in all_points)
                    {
                        if ((point - newLine.points[0]).Length < epsilon)
                        {
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        all_points.Add(newLine.points[0]);
                    }
                }
            }

            // polyLines is a unordered list of line segments.
            // If a line segment is oriented with point[0] on (0, 0, 0) and point[1] 
            // somewhere on the positive Y axis, the solid object is in the direction of the positive x axis.
            //
            //              p[1]xxxxxxxxxxxxxxxxxxxxxxxx
            //               xx                       xx
            //               xx  <object over here>   xx
            //               xx                       xx
            //              p[0]xxxxxxxxxxxxxxxxxxxxxxxx
            //
            


            List<PolyLine> newPolyLines = new List<PolyLine>();
            for (int i = 0; i < polyLines.Count(); i++)
            {
                int points = polyLines[i].points.Count();
                
                Vector3 v1 = polyLines[i].points[0];
                Vector3 v2 = polyLines[i].points[1];

                //DrawCone1(v1, v2);
            
                List<Vector3> points_on_line = new List<Vector3>();
                foreach (Vector3 v in all_points)
                {
                    if ((v1 - v).Length >= epsilon && (v2 - v).Length >= epsilon && DistanceToCylinder(v1, v2, v) < epsilon)
                    {
                        points_on_line.Add(v);
                    }
                }
                    
                points_on_line.Insert(0, v1);
                points_on_line.Add(v2);
            
                // Order from v1 to v2
                var sorted = points_on_line.OrderBy(order_vec => (order_vec - v1).Length);
            
                PolyLine newPolyLine = new PolyLine();
                foreach (Vector3 v in sorted)
                {
                    if (newPolyLine.points.Count() == 0 || (newPolyLine.points[newPolyLine.points.Count() - 1] - v).Length > epsilon)
                    {
                        newPolyLine.points.Add(v);
                    }
                }
                if (newPolyLine.points.Count() >= 2)
                {
                    newPolyLines.Add(newPolyLine);
                }
                if (newPolyLine.points.Count() >= 3)
                {
                    // Shouldn't get here!
                }
            }
            
  
            List<LinePointIndices> lpis = new List<LinePointIndices>();
            List<Vector3> vertices = new List<Vector3>();
            List<List<int>> v_lookup = new List<List<int>>();
            
            foreach (PolyLine l in newPolyLines)
            {
                int lastIndex = -1;
                foreach (Vector3 pointVec in l.points)
                {
                    int currentIndex = -1;
                    for (int i = 0; i < vertices.Count(); i++)
                    {
                        float length = (vertices[i] - pointVec).Length;
                        if (length < epsilon)
                        {
                            currentIndex = i;
                            continue;
                        }
                    }
                    if (currentIndex == -1)
                    {
                        vertices.Add(pointVec);
                        v_lookup.Add(new List<int>());
                        currentIndex = vertices.Count() - 1;
                    }
            
                    if (lastIndex != -1 && lastIndex != currentIndex)
                    {
                        LinePointIndices line = new LinePointIndices();
            
                        bool already_matched = false;
                        foreach (int line_index in v_lookup[lastIndex])
                        {
                            LinePointIndices l2 = lpis[line_index];
                            if (l2.indices[1] == currentIndex)
                            {
                                already_matched = true;
                            }
                        }
                        if (!already_matched)
                        {
            
                            line.indices.Add(lastIndex);
                            line.indices.Add(currentIndex);
                            lpis.Add(line);
            
                            v_lookup[lastIndex].Add(lpis.Count() - 1);
                            v_lookup[currentIndex].Add(lpis.Count() - 1);
                        }
                    }
                    lastIndex = currentIndex;
                }
            }

            //GL.PushMatrix();
            //GL.Translate(new Vector3(0, 0, 100));
            //GL.Begin(BeginMode.Lines);
            //foreach (LinePointIndices l in lpis)
            //{
            //    GL.Color3(Color.Red);
            //    GL.Vertex3(vertices[l.indices[0]]);
            //    GL.Color3(Color.Green);
            //    GL.Vertex3(vertices[l.indices[1]]);
            //}
            //GL.End();
            //GL.PopMatrix();

            //List<Vector3> scaled = new List<Vector3>();
            List<int> vector_indices_to_see = new List<int>();
            foreach (Vector3 v in vertices)
            {
                //scaled.Add(v / 125);
                vector_indices_to_see.Add(vector_indices_to_see.Count());
            }

            List<LinePointIndices> slices = new List<LinePointIndices>();

            GL.PushMatrix();
            GL.PointSize(10);
            List<int> seenVertices = new List<int>();
            while(vector_indices_to_see.Count() > 0)
            {
                List<int> line_indices = v_lookup [vector_indices_to_see[0]];
                vector_indices_to_see.RemoveAt(0);
                if (line_indices.Count() == 0)
                {
                    continue;
                }
                LinePointIndices line = lpis[line_indices[0]]; // Only need to look at one line with this vertex
                LinePointIndices start_line = new LinePointIndices();
                start_line.indices.Add(line.indices[0]);
                start_line.indices.Add(line.indices[1]);
                GL.Color3(Color.Green);
                DrawCone1(vertices[start_line.indices[0]], vertices[start_line.indices[1]]);
                LinePointIndices loop = FindLoop(seenVertices, p.normal, vertices, v_lookup, lpis, start_line);

                if (loop != null)
                {
                    slices.Add(loop);
                    GL.Color3(Color.LightBlue);
                    GL.Begin(BeginMode.LineLoop);
                    Vector3 add = new Vector3(0, 0, 0);
                    foreach (int i in loop.indices)
                    {
                        vector_indices_to_see.RemoveAll(value => value == i);
                        GL.Vertex3(vertices[i] + add);
                        seenVertices.Add(i);
                        //add += new Vector3(0, 0, 25);
                    }
                    GL.End();
                    //GL.Translate(new Vector3(0, 0, +100));
                }
                //break;
            }
            GL.PointSize(1);
            GL.PopMatrix();


            Vector3 normal = new Vector3(0, 0, 1);
            float toolRadius = 1000;
            GL.LineWidth(1);

            List<LineLoop> loops = new List<LineLoop>();

            GL.Disable(EnableCap.Lighting);
            foreach (LinePointIndices l in slices)
            {
                LineStrip line = new LineStrip();
                for (int i = 0; i < l.indices.Count (); i++)
                {
                    int i0 = (i - 1 + l.indices.Count()) % l.indices.Count();
                    int i1 = (i + 0 + l.indices.Count()) % l.indices.Count();
                    int i2 = (i + 1 + l.indices.Count()) % l.indices.Count();
                    int i3 = (i + 2 + l.indices.Count()) % l.indices.Count();

                    Vector3 v1 = vertices[l.indices[i1]];
                    Vector3 v2 = vertices[l.indices[i2]];
                    Vector3 v3 = vertices[l.indices[i3]];

                    Vector3 direction = v2 - v1;
                    direction.Normalize();
                    Vector3 perpendicular = Vector3.Cross(normal, direction);
                    //perpendicular.Normalize();



                    Vector3 dirNext = v3 - v2; // vertices[l.indices[i3]] - vertices[l.indices[i2]];
                    dirNext.Normalize();
                    Vector3 perp2 = Vector3.Cross(normal, dirNext);
                    float a = Angle(direction, dirNext, normal) / 2;// -OpenTK.MathHelper.Pi;

                    //if (Math.Abs(a) < 0.01 || Math.Abs(a - OpenTK.MathHelper.PiOver2) < 0.01)
                    //{
                    //    continue;
                    //}

                    float extend = (float)Math.Tan(a);
                    
                    if (extend > 0)
                    {
                        extend = 0;
                    }
                    
                    Vector3 p1 = (v1 + v2) / 2 + perpendicular * toolRadius;
                    Vector3 p2 = (v2 - direction * extend * toolRadius) + perpendicular * toolRadius;
                    Vector3 P3 = (v2 + v3) / 2 + perp2 * toolRadius;

                    
                    //GL.Begin(BeginMode.Lines);
                    
                    if (a <= OpenTK.MathHelper.PiOver2)
                    {
                        //GL.Color3(Color.Blue);
                        //GL.Vertex3(p1);
                        //GL.Vertex3(p2);
                        //GL.Color3(Color.Orange);
                        //GL.Vertex3(p2);
                        //GL.Vertex3((v2 - direction * extend * toolRadius) + perp2 * toolRadius);
                        //GL.Color3(Color.Purple);
                        //GL.Vertex3((v2 - direction * extend * toolRadius) + perp2 * toolRadius);
                        //GL.Vertex3(P3);

                        line.Append(p1);
                        line.Append(p2);
                        line.Append((v2 - direction * extend * toolRadius) + perp2 * toolRadius);
                        line.Append(P3);
                    }
                    else
                    {
                        line.Append(p1);
                        line.Append(p2);
                        line.Append(P3);
                        //GL.Color3(Color.Green);
                        //GL.Vertex3(p1);
                        //GL.Vertex3(p2);
                        //GL.Color3(Color.Red);
                        //GL.Vertex3(p2);
                        //GL.Vertex3(P3);
                    }

                    
                    

                    
                    
                    //GL.Vertex3(v1 + perpendicular * toolRadius);
                    //GL.Vertex3(v2 + perpendicular * toolRadius);
                    //GL.Color3(Color.Red);
                    //GL.Vertex3(v2 + perpendicular * (toolRadius + 1));
                    //GL.Vertex3(v2 + perpendicular * (toolRadius + 1) - direction * extend * toolRadius);
                    //    
                    //GL.Color3(Color.Blue);
                    //GL.Vertex3(v2 + perp2 * (toolRadius + 1));
                    //GL.Vertex3(v2 + perp2 * (toolRadius + 1) + dirNext * extend * toolRadius);
                    
                    //GL.End();
                }
                GL.Color3(Color.LightPink);
                LineLoop loop = new LineLoop(line);
                
                try
                {
                    loop.Clean(normal);
                }
                catch (Exception e)
                {
                }
                loops.Add(loop);
                loop.Draw();
            }
            GL.Enable(EnableCap.Lighting);


            //Clipper c = new Clipper();
            //c.Clear();
            //List<IntPoint> polygon = new List<IntPoint>();
            
            //c.AddPolygon(
            //bool succeeded = c.Execute(GetClipType(), solution, GetPolyFillType(), GetPolyFillType());

            if (loops.Count() == 2)
            {
                Vector3 up = new Vector3(0, 0, 1);
                if (Math.Abs (normal.Z) > 0.8)
                {
                    up = new Vector3(1, 0, 0);
                }

                Matrix4 transform = Matrix4.LookAt(normal, new Vector3 (0, 0, 0), up);
                Matrix4 inverseTransform = Matrix4.Invert(transform);
                Clipper c = new Clipper();
                c.Clear();

                try
                {
                    foreach (LineLoop loop in loops)
                    {
                        List<IntPoint> polygon = new List<IntPoint>();
                        foreach (Vector3 vertex in loop.Vertices)
                        {
                            // Convert the float points into integers

                            Vector3 result = Vector3.Transform(vertex, transform);
                            IntPoint point = new IntPoint((long)result.X, (long)result.Y);

                            polygon.Add(point);

                            //Vector3 original = Vector3.Transform(result, inverseTransform);

                            //Console.Write("Vertex {0} => IntPoint {1}", vertex, point);
                        }
                        polygon.RemoveAt(0);
                        //polygon.Reverse();
                        c.AddPolygon(polygon, PolyType.ptSubject);
                    }
                    List<List<IntPoint>> union = new List<List<IntPoint>>();
                    bool r = c.Execute(ClipType.ctUnion, union, PolyFillType.pftNegative, PolyFillType.pftNegative);

                    List<List<IntPoint>> with_offset = Clipper.OffsetPolygons(union, 500, JoinType.jtSquare);
                    GL.PushMatrix();
                    
                    List<LineStrip> strips = new List<LineStrip>();
                    foreach (List<IntPoint> polygon in with_offset)
                    {
                        GL.Translate(new Vector3(0, 0, 1000));
                        polygon.Reverse();
                        LineStrip strip = new LineStrip();
                        foreach (IntPoint point in polygon)
                        {
                            strip.Append(Vector3.Transform(new Vector3(point.X, point.Y, 0.0f), inverseTransform));
                        }
                        strip.Append(Vector3.Transform(new Vector3(polygon[0].X, polygon[0].Y, 0.0f), inverseTransform));
                        
                        strips.Add(strip);
                        GL.LineWidth(10);
                        strip.Draw();
                        GL.LineWidth(1);
                    }
                    GL.PopMatrix();

                    //LineLoop l = LineLoop.Intersect(loops[0], loops[1], normal);
                    //if (l != null)
                    //{
                    //    GL.PushMatrix();
                    //    GL.LineWidth(5);
                    //    GL.Translate(0, 0, 100);
                    //    l.Draw();
                    //    GL.LineWidth(1);
                    //    GL.PopMatrix();
                    //}
                }
                catch (Exception e)
                {
                }
            }
        }

        public class LineStrip
        {
            protected float epsilon = 0.01f;
            protected List<Vector3> vertices = new List<Vector3>();
            protected List<int> indices = new List<int>();
            public LineStrip()
            {
            }

            public bool ContainsVertex(Vector3 vertex)
            {
                bool result = vertices.Exists(v => (v - vertex).Length < 0.001f);
                return result;
            }

            public void Append(Vector3 v)
            {
                int index = vertices.FindIndex(vertex => (vertex - v).Length < epsilon);
                if (index < 0)
                {
                    vertices.Add(v);
                    index = vertices.Count() - 1;
                }

                int count = indices.Count();
                if (count > 0 && indices[count - 1] == index)
                {
                    // Same as last point
                    return;
                }
                else if (count > 1)
                {
                    // See if they're all on a line
                    Vector3 v1 = vertices[indices[count - 2]];
                    Vector3 v2 = vertices[indices[count - 1]];
                    Vector3 v3 = v;

                    if (OnSameLine(v1, v2, v3))
                    {
                        // Points on the same line in the same direction, don't need v2
                        indices[count - 1] = index;
                        return;
                    }
                }
                indices.Add(index);
                
            }

            protected bool OnSameLine(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                Vector3 n1 = v2 - v1;
                Vector3 n2 = v3 - v1;

                n1.Normalize();
                n2.Normalize();

                return ((n1 - n2).Length < 0.0001f);
            }

            virtual public System.Collections.Generic.IEnumerable<Vector3> Vertices
            {
                get
                {
                    foreach (int index in indices)
                    {
                        yield return vertices[index];
                    }
                }
            }

            public Vector3 GetVertex(int vertexNumber)
            {
                return vertices[indices[vertexNumber]];
            }

            public void Draw()
            {
                GL.Color3(Color.Yellow);
                GL.Begin(BeginMode.LineStrip);
                foreach (Vector3 v in Vertices)
                {
                    GL.Vertex3(v);
                    GL.Color3(Color.Blue);
                }
                GL.End();

                GL.Color3(Color.Yellow);
                GL.PointSize(5);
                GL.Begin(BeginMode.Points);
                foreach (Vector3 v in Vertices)
                {
                    GL.Vertex3(v);
                    GL.Color3(Color.Blue);
                }
                GL.End();
                GL.PointSize(1);
            }
        }

        public class LineLoop : LineStrip
        {
            public LineLoop()
            {
            }
            public LineLoop(LineStrip l)
            {
                foreach (Vector3 v in l.Vertices)
                {
                    this.Append(v);
                }
                if (indices.Count() < 3)
                {
                    throw new Exception("Cannot make a loop from a line with fewer than 3 vertices");
                }
                if (indices[0] != indices[indices.Count() - 1])
                {
                    throw new Exception("Line is not a loop - the first and last vertex must be the same");
                }

                // Remove the last index - it's redundant.
                indices.RemoveAt(indices.Count() - 1);

                // Test if the connection back to the first vertex makes the last vertex unnecessary
                Vector3 v1 = vertices[indices[indices.Count() - 1]];
                Vector3 v2 = vertices[indices[0]];
                Vector3 v3 = vertices[indices[1]];
                if (OnSameLine(v1, v2, v3))
                {
                    indices.RemoveAt(0);
                }
            }


            /// <summary>
            /// Combine one line loop with another
            /// </summary>
            /// <param name="other"></param>
            /// <returns>Array of LineLoops which don't intersect (will be one or two loops)</returns>
            public static LineLoop Intersect(LineLoop one, LineLoop another, Vector3 normal)
            {

                LineStrip one2 = new LineStrip();
                List<Vector3> vv = new List<Vector3>(one.Vertices);
                for (int i = 0; i < vv.Count; i++)
                {
                    int j = (i + 2) % vv.Count;
                    one2.Append(vv[j]);
                }
                one2.Append(one2.GetVertex(0));
                one = new LineLoop(one2);

                GL.PointSize(19);
                GL.Color3(Color.GreenYellow);
                GL.Begin(BeginMode.Points);
                GL.Vertex3(one.GetVertex(0));
                GL.End();
                GL.PointSize(1);

                

                LineLoop following = one;
                LineLoop searching = another;

                Vector3 startPoint = following.GetVertex(0);
                LineStrip ls = new LineStrip();
                List<LineLoop> loops = new List<LineLoop>();
                for (int i = 0; i < following.indices.Count(); i++)
                {
                    Vector3 v1 = following.GetVertex(i);
                    Vector3 v2 = following.GetVertex((i + 1) % following.indices.Count());

                    //if (ls.ContainsVertex(v1))
                   // {
                        bool matched = false;
                        LineStrip ls2 = new LineStrip();
                        foreach (Vector3 v in ls.Vertices)
                        {
                            if (!matched && (v - v1).Length < 0.001)
                            {
                                matched = true;
                            }
                            if (matched)
                            {
                                ls2.Append(v);
                            }
                        }
                        if (matched)
                        {

                            ls2.Append(v1);
                            GL.PushMatrix();
                            GL.Translate(0, 0, 50);
                            ls2.Draw();
                            GL.PopMatrix();
                            // Done!  Got a loop
                            LineLoop l = new LineLoop(ls2);
                            return l;
                        }
                    //}
                    ls.Append(v1);

                    int searchingCount = searching.indices.Count();
                    for (int j = 0; j < searchingCount; j++)
                    {
                        Vector3 v3 = searching.GetVertex(j);
                        Vector3 v4 = searching.GetVertex((j + 1) % searchingCount);

                        Vector3 n1 = (v2 - v1);
                        Vector3 n2 = (v4 - v3);

                        n1.Normalize();
                        n2.Normalize();

                        float d1 = DistanceToCylinder(v1, v2, v3);
                        float d2 = DistanceToCylinder(v3, v4, v1);

                        if (d2 < 10)
                        {
                        }

                        // Point v3 is on Line v1-v2:
                        if (DistanceToCylinder(v1, v2, v3) < 1)
                        {
                            // Follow the line most to the outside 
                            float angle = Angle(n1, n2, normal);
                            if (angle < OpenTK.MathHelper.Pi)
                            {
                                GL.PointSize(10);
                                GL.Color3(Color.Orange);
                                GL.Begin(BeginMode.Points);
                                GL.Vertex3(v3);
                                GL.End();
                                GL.PointSize(1);
                                ls.Append(v3);
                                // Follow the line v3-v4
                                LineLoop temp = searching;
                                searching = following;
                                following = temp;
                                i = j;
                                break;
                            }
                            else
                            {
                                // Continue following the line v1-v2
                            }
                        }
                        // Point v2 is on the line v3-v4
                        else if (DistanceToCylinder(v3, v4, v1) < 1)
                        {
                            // Follow the line most to the outside
                            float angle = Angle(n1, n2, normal);
                            if (angle < OpenTK.MathHelper.Pi)
                            {
                                GL.PointSize(10);
                                GL.Color3(Color.Orange);
                                GL.Begin(BeginMode.Points);
                                GL.Vertex3(v1);
                                GL.End();
                                GL.PointSize(1);
                                //ls.Append(v1);
                                // Follow the line v3-v4
                                LineLoop temp = searching;
                                searching = following;
                                following = temp;
                                i = j;
                                break;
                            }
                            else
                            {
                                // Continue following the line v1-v2
                            }
                        }
                        // TODO: Add case for intersecting lines!
                        //else if (0)
                        //{
                        //}
                    }
                        
                }
                //return loops.ToArray();

                ls.Append(following.GetVertex(following.indices[0]));
                //if (ls.ContainsVertex(v1))
                //{
                    //ls.Append(v1);
                    GL.PushMatrix();
                    GL.Translate(0, 0, 50);
                    ls.Draw();
                    GL.PopMatrix();
                    // Done!  Got a loop
                    LineLoop l2 = new LineLoop(ls);
                    return l2;
                //}
            }
            /// <summary>
            /// Trim interior loops from the line
            /// </summary>
            public void Clean(Vector3 normal)
            {
                List<int> remove = new List<int>();
                for (int i = 0; i < indices.Count(); i++)
                {
                    Vector3 v1 = GetVertex(i);
                    Vector3 v2 = GetVertex((i + 1) % indices.Count());
                    for (int j = 2; j < indices.Count() - 1; j++)
                    {
                        int j1 = (j + i) % indices.Count();
                        int j2 = (j + i + 1) % indices.Count();
                        Vector3 v3 = GetVertex(j1);
                        Vector3 v4 = GetVertex(j2);

                        Vector3 n1 = v2 - v1;
                        Vector3 n2 = v4 - v3;
                        n1.Normalize();
                        n2.Normalize();

                        if ((n1 - n2).Length < 0.0001f)
                        {
                            // No crossing, same direction
                            continue;
                        }

                        Vector3 perp = Vector3.Cross(normal, n2);
                        Plane p = new Plane ();

                        //GL.Begin(BeginMode.Lines);
                        //GL.Vertex3(v4);
                        //GL.Vertex3(v4 + perp * 100);
                        //GL.End();
                        p.normal = perp;
                        p.point = v3;

                        float distance = p.Distance(v1, n1);
                        if (distance > 0 && distance < (v2 - v1).Length && (Vector3.Dot(perp, n1) < 0))//!float.IsInfinity(distance))
                        {
                            Vector3 point = n1 * distance + v1;
                            if (DistanceToCylinder (v3, v4, point) < 0.01f)
                            {
                                for (int k = 0; k < j; k++)
                                {
                                    int removeIndex = (i + 1) % indices.Count();
                                    indices.RemoveAt(removeIndex);
                                }
                                vertices.Add(point);
                                indices.Insert(i + 1, vertices.Count() - 1);
                                //GL.PointSize(15);
                                //GL.Color3(Color.Turquoise);
                                //GL.Begin(BeginMode.Points);
                                //GL.Vertex3(point);
                                //
                                //GL.End();
                                //GL.PointSize(1);
                                i = i - 1;
                                continue;
                            }
                        }
                    }
                }
                LineLoop n = new LineLoop(this);
                this.indices = n.indices;
                this.vertices = n.vertices;
            }

            override public IEnumerable<Vector3> Vertices
            {
                get
                {
                    foreach (Vector3 v in base.Vertices)
                    {
                        yield return v;
                    }
                    yield return vertices[indices[0]];
                }
            }
        }

        private LinePointIndices FindLoop(List<int> seenVertices, Vector3 normal, List<Vector3> vertices, List<List<int>> v_lookup, List<LinePointIndices> lpis, LinePointIndices start_line)
        {
            List<LinePointIndices> return_list = new List<LinePointIndices>();

            if (seenVertices.Contains(start_line.indices[0]) || seenVertices.Contains(start_line.indices[1]))
            {
                return null;
            }

            while (start_line.indices.Count <= 100)
            {
                var point_count = start_line.indices.Count();    
                var headIndex = start_line.indices[point_count - 1];
                var lastIndex = start_line.indices[point_count - 2];
                var headPoint = vertices[headIndex];
                var lastPoint = vertices[lastIndex];

                var direction = headPoint - lastPoint;
                direction.Normalize();

                // Find all the other segments starting at the head point
                List<int> line_indexes = v_lookup[headIndex];
                // To start at the head point, the first element must match the head index
                line_indexes.RemoveAll(i => lpis[i].indices[0] != headIndex);

                float largestAngle = 0;
                int bestIndex = -1;
                // Always choose the leftmost vertex - maximize the loop size!
                // Find the leftmost vertex below, if there are any vertices.
                foreach (int index in line_indexes)
                {
                    LinePointIndices l = lpis[index];
                    Vector3 newPoint = vertices[l.indices[1]];

                    Vector3 newDirection = newPoint - headPoint;
                    newDirection.Normalize();
                    float angle = Angle(direction, -newDirection, normal);
                    if (largestAngle < angle || bestIndex == -1)
                    {
                        bestIndex = l.indices[1];
                        largestAngle = angle;
                    }
                }

                if (bestIndex == -1)
                {
                    // Bad - line terminated, can't make a loop
                    Console.WriteLine("Not a looped line!");
                    return null;
                }

                if (seenVertices.Contains(bestIndex))
                {
                    return null;
                }

                // Add the new point to the line - but first check this causes a loop!
                var firstIndex = start_line.indices.IndexOf(headIndex);
                if (firstIndex != point_count - 1)
                {
                    // Got a loop!  See if the point after head is the best index
                    if (start_line.indices[firstIndex + 1] == bestIndex)
                    {
                        // Keeper!  Remove the starting interior vertices, inclusive of the
                        // matched index to head (don't need two of the same point floating around).
                        start_line.indices.RemoveRange(0, firstIndex + 1);
                        return start_line;
                    }
                }
                start_line.indices.Add(bestIndex);
            }

            throw new Exception("Too Many Vertices!");
        }

        /// <summary>
        /// Measure the counter-clockwise angle around the normal in radians from one vector to another.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="another"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        private static float Angle(Vector3 one, Vector3 another, Vector3 normal)
        {
            float inv_cos = Vector3.Dot(one, another);
            float inv_sin = Vector3.Dot(Vector3.Cross(one, another), normal);

            float angle = 0;
            if (Math.Abs(inv_cos) > Math.Abs(inv_sin))
            {
                if (inv_cos > 0)
                {
                    // Between -Pi/2 and Pi/2
                    angle = (float)Math.Asin(inv_sin);
                }
                else
                {
                    angle = OpenTK.MathHelper.Pi - (float)Math.Asin(inv_sin);
                }
            }
            else
            {
                // Determine the quadrant
                if (inv_sin > 0)
                {
                    // Angle is between 0 and Pi
                    angle = (float)Math.Acos(inv_cos);
                }
                else
                {
                    // Angle is between Pi and 2*Pi
                    angle = (float)Math.Acos(-inv_cos) + OpenTK.MathHelper.Pi;
                }
            }
            if (angle < 0)
            {
                angle += OpenTK.MathHelper.TwoPi;
            }
            return angle;
        }

        private class LinePointIndices
        {
            public List<int> indices = new List<int>();
            public string ToString()
            {
                string s = "";
                foreach (int i in indices)
                {
                    s += " " + i;
                }
                return s;
            }
        }

        private PolyLine TrianglePlaneIntersect(Face f, Plane p)
        {
            PolyLine polyLine = new PolyLine();

            float epsilon = 0.01f; // TODO: Auto compute based on scale
            float epsilon_unit = 0.00001f; // Unit size epsilon value
            Vector3 f_normal = f.Normal;
            f_normal.Normalize();
            p.normal.Normalize();

            

            if ((f_normal - p.normal).Length < epsilon_unit || (f_normal + p.normal).Length < epsilon_unit)
            {
                // No intersection
            }
            else
            {
                Vector3 expected_direction = Vector3.Cross(f.Normal, p.normal);
                
                // Assume we're dealing with triangles only
                int verts = f.vertices.Count();
                if (verts != 3)
                {
                    throw new Exception("The number of vertices is not 3!");
                }

                
                float[] d = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    d[i] = p.Distance(f.vertices[i]);
                }

                for (int i = 0; i < 3; i++)
                {
                    // Is the line on the plane?
                    if (Math.Abs(d[i]) < epsilon && Math.Abs(d[(i + 1) % 3]) < epsilon)
                    {
                        polyLine.points.Add(f.vertices[i]);
                        polyLine.points.Add(f.vertices[(i + 1) % 3]);
                        break;
                    }
                }

                if (polyLine.points.Count() == 0)
                {
                    // Line not on a plain: might have an intersection with a point and the opposite line
                    for (int i = 0; i < 3; i++)
                    {
                        float d1 = d[i];
                        float d2 = d[(i + 1) % 3];
                        float d3 = d[(i + 2) % 3];
                        if (Math.Abs(d[i]) < epsilon && Math.Sign(d2) != Math.Sign(d3))
                        {
                            d2 = Math.Abs(d2);
                            d3 = Math.Abs(d3);
                            
                            // One negative, one positive
                            float total = d2 + d3;
                            Vector3 result = (f.vertices[(i + 1) % 3] * d3 + f.vertices[(i + 2) % 3] * d2) / total;
                            polyLine.points.Add(f.vertices[i]);
                            polyLine.points.Add(result);
                            break;
                        }
                    }
                    if (polyLine.points.Count() == 0)
                    {
                        // No edge in plane and no point + line intersect: maybe two lines intersect?
                        for (int i = 0; i < 3; i++)
                        {
                            // Intersection with an edge
                            if (Math.Sign(d[i]) != Math.Sign(d[(i + 1) % 3]))
                            {
                                float d1 = Math.Abs(d[i]);
                                float d2 = Math.Abs(d[(i + 1) % 3]);
                                float total = d1 + d2;
                                Vector3  result = (f.vertices[i] * d2 + f.vertices[(i + 1) % 3] * d1) / total;
                                polyLine.points.Add(result);
                                if (polyLine.points.Count() == 2)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                if (polyLine.points.Count() >= 2)
                {
                    //DrawCone1(polyLine.points[0], polyLine.points[1]);
                    Vector3 direction = polyLine.points[1] - polyLine.points[0];
                    if (Vector3.Dot(direction, expected_direction) < 0)
                    {
                        PolyLine reversed = new PolyLine();
                        reversed.points.Add(polyLine.points[1]);
                        reversed.points.Add(polyLine.points[0]);
                        polyLine = reversed;
                    }
                //
                //
                //    Color[] colors = new Color[] { Color.DarkRed, Color.LightGreen, Color.DarkBlue };
                //    int i = 0;
                //    GL.Begin(BeginMode.LineLoop);
                //    foreach (Vector3 v in polyLine.points)
                //    {
                //        GL.Color3(colors[i++]);
                //        GL.Vertex3(v);
                //
                //    }
                //    GL.End();
                //
                //    GL.PointSize(10);
                //    GL.Color3(Color.Orange);
                //    GL.Begin(BeginMode.Points);
                //    foreach (Vector3 v in polyLine.points)
                //    {
                //        GL.Vertex3(v);
                //    }
                //    GL.End();
                //    GL.PointSize(1);
                }
            }
            return polyLine;
        }

        public ObjLoader()
        {
            vertices = new List<Vector3>();
            faces = new List<Face>();
            //Face f = new Face();
            //f.vertices.Add(new Vector3(0, 0, 0));
            //f.vertices.Add(new Vector3(0, 1, 0));
            //f.vertices.Add(new Vector3(0, 1, -1));
            //f.vertices.Add(new Vector3(0, 0, -1));
        }

        public void LoadObj(string filepath)
        {
            string[] strings = System.IO.File.ReadAllLines(filepath);
            Regex vertexRegex = new Regex (@"^v\s+(?<x>\S+)\s+(?<y>\S+)\s+(?<z>\S+)", RegexOptions.IgnoreCase);
            //Regex faceRegex = new Regex (@"^f(?<face_data>\s+\w+)+", RegexOptions.IgnoreCase);
            //Regex f = new Regex(@"^f(?<face_data>[^/]*/(?<itemNumber>\w+))+", RegexOptions.IgnoreCase);
            Regex faceRegex = new Regex(@"^f(?<face_data>\s+(?<vertex>\d+)/?(?<texture_coordinate>\d+)?/?(?<vertex_normal>\d+)?)+", RegexOptions.IgnoreCase);
            //Regex f = new Regex(@"(?<container>\d{5})(?<serial>[^/]*/(?<itemNumber>\w{5})(?<quantity>\d{3}))+", RegexOptions.IgnoreCase);

            // "f 1/1/1 2/2/1 3/3/1 4/4/1 5/5/1"
            foreach (string s in strings)
            {
                // Lines starting with v are a vertex:
                // "v 10.2426 4.5e-013 -31.7638"
                if (vertexRegex.IsMatch(s))
                {
                    Match m = vertexRegex.Match(s);
                    float x = float.Parse(m.Groups["x"].Value);
                    float y = float.Parse(m.Groups["y"].Value);
                    float z = float.Parse(m.Groups["z"].Value);
                    vertices.Add(new Vector3 (x, z, y) * 1000);
                    //Console.WriteLine("Vertex Found: {0}", v);
                }
                else if (faceRegex.IsMatch(s))
                {
                    Match m = faceRegex.Match(s);

                    //Console.WriteLine(m.Groups["face_data"].Captures.Count);
                    //Console.WriteLine(m.Groups["vertex"].Captures.Count);
                    //Console.WriteLine(m.Groups["texture_coordinate"].Captures.Count);
                    //Console.WriteLine(m.Groups["vertex_normal"].Captures.Count);

                    Face face = new Face();

                    CaptureCollection vert_captures = m.Groups["vertex"].Captures;
                    CaptureCollection texcoord_captures = m.Groups["texture_coordinate"].Captures;
                    CaptureCollection norm_captures = m.Groups["vertex_normal"].Captures;
                    for (int i = 0; i < vert_captures.Count; i++)
                    {
                        int vert_index = int.Parse(vert_captures[i].Value) - 1;
                        if (vert_index < 0 || vert_index > vertices.Count)
                        {
                            Console.WriteLine("Bad vertex index {0}, only {1} vertices loaded", vert_index, vertices.Count);
                        }
                        else
                        {
                            face.vertices.Add(this.vertices[vert_index]);
                        }
                    }
                    if (texcoord_captures.Count == vert_captures.Count)
                    {
                        // TODO: Add texture coordinates to the face
                    }
                    if (norm_captures.Count == vert_captures.Count)
                    {
                        // TODO: Add vertex normals to the face
                    }

                    if (face.vertices.Count < 3)
                    {
                        Console.WriteLine("Bad face defined, less than 3 vertices");
                    }
                    else
                    {
                        AddFace(face);
                    }
                }
            }
        }

        public void AddFace(Face f)
        {
            //this.faces.Add(f);
            this.faces.AddRange(Tessellate(f));
        }

        private List<Face> Tessellate(Face f)
        {
            // Base Case: Face is already a triangle
            if (f.vertices.Count() == 3)
            {
                return new List<Face>(new Face[] { f });
            }
            else
            {
                int verts = f.vertices.Count();
                // Find an ear on the face, remove it
                for (int i = 0; i < verts; i++)
                {
                    Vector3 v1 = f.vertices[i];
                    Vector3 v2 = f.vertices[(i + 1) % verts];
                    Vector3 v3 = f.vertices[(i + 2) % verts];
                    Face tri = new Face();
                    tri.vertices.Add(v1);
                    tri.vertices.Add(v2);
                    tri.vertices.Add(v3);

                    bool anyPointInPolygon = false;
                    foreach (Vector3 p in f.vertices)
                    {
                        if (p != v1 && p != v2 && p != v3 && IsPointInPolygon(tri, p))
                        {
                            anyPointInPolygon = true;
                            break;
                        }
                    }

                    // First check: see if any point in the original polygon is inside the new triangle
                    if (anyPointInPolygon) //AnyPointInPolygon(tri, f.vertices))
                    {
                        // Can't use this triangle, move onto the next one
                    }
                    else
                    {
                        // Second Check: see if the midpoint on the new line is within the original polygon
                        Vector3 midpoint = (v1 + v3) / 2;
                        if (!IsPointInPolygon(f, midpoint))
                        {
                            // Can't use this triangle, move onto the next one
                        }
                        else
                        {
                            // Triangle satisfies criteria for an ear! Add it to the face list.
                            List<Face> faces = new List<Face>();
                            faces.Add(tri);
                            f.vertices.RemoveAt((i + 1) % verts);
                            faces.AddRange(Tessellate(f));
                            return faces;
                        }
                    }
                }
            }

            // If we get here, the face has no ear and more than 3 vertices... Bad situation!
            //throw new Exception("Failed to tessellate face!");
            badfaces.Add(f);
            return new List<Face>();
        }

        List<Face> badfaces = new List<Face>();

        static bool IsPointInPolygon(Face f, Vector3 point)
        {
            // TODO: Implement a ray tracing approach instead - angle summation can be unreliable and slow.
            float angle = 0;
            for (int i = 0; i < f.vertices.Count(); i++)
            {
                Vector3 v1 = f.vertices[i] - point;
                Vector3 v2 = f.vertices[(i + 1) % f.vertices.Count()] - point;
                float direction = Vector3.Dot (Vector3.Cross(v1, v2), f.Normal) / (v1.Length * v2.Length);
                if (direction > -.0001) // Biased towards within polygon
                {
                    angle += (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length * v2.Length));
                }
                else
                {
                    angle -= (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length * v2.Length));
                }
            }
            angle = (float)Math.Abs(angle);
            //Console.WriteLine("In Face = {0}, Angle = {1} | {2}", Utilities.MathHelper.NearlyEqual (angle, OpenTK.MathHelper.TwoPi), angle, OpenTK.MathHelper.TwoPi);
            return Utilities.MathHelper.NearlyEqual(angle, OpenTK.MathHelper.TwoPi) || angle > OpenTK.MathHelper.TwoPi;
        }



        public void Draw()
        {
            GL.Color3(Color.DarkGreen);
            GL.PointSize(5);
            GL.Begin(BeginMode.Points);
            GL.Vertex3(mousePoint);
            GL.End();
            GL.PointSize(1);

            foreach (Face bad in this.badfaces)
            {
                GL.Color3(Color.Red);
                GL.Begin(BeginMode.TriangleFan);
                foreach (Vector3 v in bad.vertices)
                {
                    GL.Normal3(bad.Normal);
                    GL.Vertex3(v);
                }
                GL.End();
            }
            
            for (int i = 0; i < faces.Count(); i++)
            {
                Face f = faces[i];
                
                Vector3 normal = f.Normal;

                if (withinFace)
                {
                    GL.Color3(Color.Orange);
                }
                else
                {
                    GL.Color3(Color.White);
                }
                GL.Begin(BeginMode.TriangleFan);
                foreach (Vector3 v in f.vertices)
                {
                    GL.Normal3(normal);
                    GL.Vertex3(v);
                }
                GL.End();

                //GL.Color3(Color.LightGray);
                //GL.Begin(BeginMode.LineLoop);
                //foreach (Vector3 v in f.vertices)
                //{
                //    GL.Vertex3(v);
                //}
                //GL.End();

                if (i == closestFace)
                {
                    GL.PointSize(10);
                    GL.Begin(BeginMode.Points);
                    GL.Color3(Color.Red);
                    GL.Vertex3(f.vertices[0]);
                    GL.Color3(Color.Green);
                    GL.Vertex3(f.vertices[1]);
                    GL.Color3(Color.Blue);
                    GL.Vertex3(f.vertices[2]);
                    GL.End();
                    GL.PointSize(1);
                }

                
                
                //for (int i = 0; i < 1000; i += 500)
                //{
                //int i = (int)((DateTime.Now.Ticks &  Int32.MaxValue) / 50000) % 1000;
                //int i = (int)mousePoint.Z;
                
                

                //GL.Color3(Color.Green);
                //p.normal = new Vector3(0, 1, 0);
                //p.point = new Vector3(0, mousePoint.Y, 0);
                //Slice(f, p);
                //GL.Color3(Color.Red);
                //p.normal = new Vector3(1, 0, 0);
                //p.point = new Vector3(mousePoint.X, 0, 0);
                //Slice(f, p);
                
                //}
            }

            GL.LineWidth(4);
            GL.Color3(Color.Blue);
            Plane p = new Plane();
            p.point = new Vector3(0, 0, mousePoint.Z);
            p.normal = new Vector3(0, 0, 1);
            Slice(p);
            GL.LineWidth(1);
        }


        Vector3 mousePoint = new Vector3(0, 0, 0);
        bool withinFace = false;

        int closestFace = -1;

        public float IsPointOnObject(Vector3 location, Vector3 direction)
        {
            Intersect closest = new Intersect();
            closest.distance = float.PositiveInfinity;
            for (int a = 0; a < faces.Count(); a++)
            {
                Face f = faces[a];
                Intersect i = f.CalculateIntersect(location, direction);
                if (i.intersects)
                {
                    if (i.distance < closest.distance)
                    {
                        closest = i;
                        closestFace = a;
                    }
                }
            }
            withinFace = true;
            mousePoint = new Vector3(0, 0, 0);
            if (float.IsInfinity(closest.distance))
            {
                withinFace = false;
                
            }
            else
            {
                mousePoint = closest.intersectPoint;
                //float d = DistanceToCylinder(new Vector3(1000, 0, 0), new Vector3(0, 0, 0), mousePoint);
                //Console.WriteLine("Distance to Cylinder = {0}, Point = {1}", d, mousePoint);
                //Console.WriteLine("Mouse Point = {0}", mousePoint);
                
                //Vector3 original = new Vector3(1, 0, 0);
                //Vector3 another = mousePoint - new Vector3 (500, 500, 0);
                //another.Normalize();
                //float d = OpenTK.MathHelper.RadiansToDegrees(Angle(original, another, new Vector3(0, 0, 1)));
                //Console.WriteLine("Angle = " + d);
            }
            return closest.distance;
            //Console.WriteLine("Outside Face");
        }
    }
}
