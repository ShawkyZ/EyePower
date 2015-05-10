using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Model.Vision
{
    public class vision
    {
        public List<category> categories { get; set; }
        public adult adult { get; set; }
        public List<face> faces { get; set; }
        public metadata metadata { get; set; }
    }
    public class category
    {
        public string name { get; set; }
        public string score { get; set; }
    }
    public class metadata
    {
        public double width { get; set; }
        public double height { get; set; }
        public string format { get; set; }
    }
    public class faceRectangle
    {
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
    public class face
    {
        public int age { get; set; }
        public string gender { get; set; }
        public faceRectangle faceRectangle { get; set; }
    }
    public class adult
    {
        public bool isAdultContent { get; set; }
        public bool isRacyContent { get; set; }
        public double adultScore { get; set; }
        public double racyScore { get; set; }
        public string requestID { get; set; }
    }
}
