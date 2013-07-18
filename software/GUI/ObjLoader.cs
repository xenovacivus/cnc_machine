using OpenTK;
using OpenTK.Graphics.OpenGL;
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
    class ObjLoader: IOpenGLDrawable
    {
        List<Vector3> vertices;
        private class Face
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
        }
        List<Face> faces;

        public class Plane
        {
            public Vector3 point = new Vector3 (0, 0, 0);
            public Vector3 normal = new Vector3(1, 0, 0);
            public float Distance(Vector3 p)
            {
                float scalar = Vector3.Dot(normal, point);
                return Vector3.Dot(normal, p) - scalar;
            }
        }

        private void Slice(Face f, Plane p)
        {
            if ((f.Normal - p.normal).Length < float.Epsilon)
            {
                // No intersection
            }
            else
            {
                float scalar = Vector3.Dot(p.normal, p.point);
                List<Vector3> intersectPoints = new List<Vector3>();
                for (int i = 0; i < f.vertices.Count(); i++)
                {
                    Vector3 v1 = f.vertices[i];
                    Vector3 v2 = f.vertices[(i + 1) % f.vertices.Count()];
                    float d1 = p.Distance(v1);
                    float d2 = p.Distance(v2);
                    if (d1 * d2 < float.Epsilon)
                    {
                        d1 = (float)(Math.Abs(d1));
                        d2 = (float)(Math.Abs(d2));
                        // One negative, one positive
                        float total = d1 + d2;
                        Vector3 result = (v1 * d2 + v2 * d1) / total;
                        intersectPoints.Add(result);
                    }
                    else
                    {
                        if (Math.Abs(d1) < float.Epsilon)
                        {
                            intersectPoints.Add(v1);
                        }
                        if (Math.Abs(d2) < float.Epsilon)
                        {
                            intersectPoints.Add(v2);
                        }
                    }
                }
                if (intersectPoints.Count() >= 2)
                {
                    GL.Begin(BeginMode.LineLoop);
                    foreach (Vector3 v in intersectPoints)
                    {
                        GL.Vertex3(v);

                    } GL.End();
                }
            }
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
                        faces.Add(face);
                    }
                }
            }
        }

        public void Draw()
        {
            //GL.Color3(Color.DarkGreen);
            foreach (Face f in this.faces)
            {
                Vector3 normal = f.Normal;

                //GL.Color3(Color.White);
                //GL.Begin(BeginMode.TriangleFan);
                //foreach (Vector3 v in f.vertices)
                //{
                //    GL.Normal3(normal);
                //    GL.Vertex3(v);
                //}
                //GL.End();

                GL.Color3(Color.LightGray);
                GL.Begin(BeginMode.LineLoop);
                foreach (Vector3 v in f.vertices)
                {
                    GL.Vertex3(v);
                }
                GL.End();

                
                for (int i = 0; i < 1000; i += 500)
                {
                //int i = (int)((DateTime.Now.Ticks &  Int32.MaxValue) / 50000) % 1000;
                    
                GL.Color3(Color.Blue);
                Plane p = new Plane();
                p.point = new Vector3(0, 0, i);
                p.normal = new Vector3(0, 0, 1);
                Slice(f, p);
                //GL.Color3(Color.Green);
                //p.normal = new Vector3(0, 1, 0);
                //p.point = new Vector3(0, -i, 0);
                //Slice(f, p);
                //GL.Color3(Color.Red);
                //p.normal = new Vector3(1, 0, 0);
                //p.point = new Vector3(i, 0, 0);
                //Slice(f, p);
                }
            }
        }
    }
}
