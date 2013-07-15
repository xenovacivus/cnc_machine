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
        }
        List<Face> faces;

        public ObjLoader()
        {
            vertices = new List<Vector3>();
            faces = new List<Face>();
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
                    vertices.Add(new Vector3 (x, y, z));
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
            GL.Color3(Color.DarkGreen);
            foreach (Face f in this.faces)
            {
                GL.Begin(BeginMode.TriangleFan);
                foreach (Vector3 v in f.vertices)
                {
                    GL.Vertex3(v);
                }
                GL.End();
            }
        }
    }
}
