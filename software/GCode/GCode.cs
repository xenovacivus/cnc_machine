using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using OpenTK;

namespace Router
{
    public class GCode
    {
        public GCode ()
        {
            lastx = 0;
            lasty = 0;
            lastz = 0;
            linear_routs = new List<Rout>();
        }
        
        List<Rout> linear_routs;
        public Rout[] GetRouts()
        {
            Console.WriteLine("Returning " + linear_routs.Count() + " Linear Routs");
            return linear_routs.ToArray();
        }

        float lastx, lasty, lastz;
        public void Parse(string[] lines)
        {
            foreach (string s in lines)
            {
                Regex r = new Regex("^G(?<G_VALUE>\\d+)");
                if (r.IsMatch (s))
                {
                    Match m = r.Match(s);
                    Int32 g_value = Int32.Parse (m.Groups["G_VALUE"].Value);

                    if (g_value == 0)
                    {
                        // Rapid Positioning, go to (x, y, z) as fast as possible
                        Vector3 fromPoint = new Vector3(lastx, lasty, lastz);
                        GetFloat(s, "X", ref lastx);
                        GetFloat(s, "Y", ref lasty);
                        GetFloat(s, "Z", ref lastz);
                        Vector3 toPoint = new Vector3(lastx, lasty, lastz);

                        Rout rout = new Rout();
                        rout.just_moving = true;
                        rout.Width = 5;
                        rout.Points = new List<Vector2>(new Vector2[] { new Vector2(fromPoint.X * 1000, fromPoint.Y * 1000), new Vector2(toPoint.X * 1000, toPoint.Y * 1000) });
                        rout.Depth = toPoint.Z * 1000;
                        linear_routs.Add(rout);
                        
                    }
                    else if (g_value == 1)
                    {
                        // Linear Interpolation
                        Vector3 fromPoint = new Vector3(lastx, lasty, lastz);
                        GetFloat(s, "X", ref lastx);
                        GetFloat(s, "Y", ref lasty);
                        GetFloat(s, "Z", ref lastz);
                        Vector3 toPoint = new Vector3(lastx, lasty, lastz);
                        
                        Rout rout = new Rout();

                        rout.Width = 20;
                        rout.Points = new List<Vector2>(new Vector2[] { new Vector2(fromPoint.X * 1000, fromPoint.Y * 1000), new Vector2(toPoint.X * 1000, toPoint.Y * 1000) });
                        rout.Depth = toPoint.Z * 1000;
                        linear_routs.Add(rout);

                        //Console.WriteLine("From " + fromPoint + " To " + toPoint);
                    }
                    else if (g_value == 4)
                    {
                        // Dwell Time (X, U, or P): dwell time in milliseconds
                    }
                    else if (g_value == 20)
                    {
                        // Inch Mode
                        Console.WriteLine("Units are Inches");
                    }
                    else if (g_value == 21)
                    {
                        // Metric Mode
                        throw new NotSupportedException("Metric GCode is not supported!");
                    }
                    else if (g_value == 90)
                    {
                        // Absolute Programming
                        Console.WriteLine("Absolute Programming");
                    }
                    else
                    {
                        Console.WriteLine("G code is not understood: " + s);
                    }
                    //string groupName = r.GroupNumberFromName("G_VALUE").ToString();
                    //MatchCollection m = r.Matches;
                    //Console.WriteLine(s + ", Group Name = " + groupName);
                }
            }
        }

        /// <summary>
        /// Get a float in the format of "G01 Z-0.0100 F2.00", where string is Z, F, or other preceding character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool GetFloat(string input, string find, ref float f)
        {
            Regex r = new Regex(find + @"(?<VALUE>-?[\d\.]+)");
            if (r.IsMatch(input))
            {
                Match m = r.Match(input);
                string value_string = m.Groups["VALUE"].Value;
                
                f = float.Parse(value_string);
                //Console.WriteLine("Value for " + find + " is " + f);
                return true;
            }
            return false;
        }


    }
}
