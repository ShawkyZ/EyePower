using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Model
{
    public class adult
    {
        public bool isAdultContent { get; set; }
        public bool isRacyContent { get; set; }
        public double adultScore { get; set; }
        public double racyScore { get; set; }
        public string requestID { get; set; }
    }
}
