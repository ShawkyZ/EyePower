using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Model.Alchemy
{
    public class AlchemyResult
    {
        public string status { get; set; }
        public string usage { get; set; }
        public string url { get; set; }
        public List<imageKeywords> imageKeywords { get; set; }
    }
    public class imageKeywords
    {
        public string text { get; set; }
        public double score { get; set; }
                
    }
}
