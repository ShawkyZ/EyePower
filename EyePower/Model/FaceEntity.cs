using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceAPIDemo.Model
{
    public class Point
    {
        public double x{ get; set; }
        public double y{ get; set; }
    }
    public class Gender
    {
        public double confidence { get; set; }
        public string value { get; set; }
    }
    public class Age 
    {
        public int range { get; set; }
        public int value { get; set; }
    }
    public class Smiling
    {
        public double value { get; set; }
    }
    public class Race
    {
        public double confidence { get; set; }
        public string value { get; set; }
    }
    public class Attribute
    {
        public Gender gender { get; set; }
        public Age age { get; set; }
        public Race race { get; set; }
        public Smiling smiling { get; set; }
    }
    public class Face
    {
        public Attribute attribute { get; set; }
        public string face_id { get; set; }
        public Position position { get; set; }
        public string tag { get; set; }
    }
    public class DetectedResult
    {
        public List<Face> face { get; set; }
        public int img_height { get; set; }
        public string img_id { get; set; }
        public int img_width { get; set; }
        public string session_id { get; set; }
        public string url{ get; set; }
    }
    public class Position
    {
        public Point eye_left { get; set; }
        public Point center { get; set; }
        public double width { get; set; }
        public Point mouth_left { get; set; }
        public double height { get; set; }
        public Point mouth_right { get; set; }
        public Point eye_right { get; set; }
        public Point nose { get; set; }
    }
}
