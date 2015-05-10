using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Model.Rekognize
{
    public class RekognizeResult
    {
        public string url { get; set; }
        public scene_understanding scene_understanding { get; set; }
        public usage usage { get; set; }
    }
    public class usage
    {
        public int quota { get; set; }
        public string status { get; set; }
        public string api_id { get; set; }
    }
    public class scene_understanding
    {
        public List<matches> matches { get; set; }
    }
    public class matches
    {
        public string tag { get; set; }
        public double score { get; set; }
    }
}
