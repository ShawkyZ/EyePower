using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Model
{
    public class vision
    {
        public List<category> categories { get; set; }
        public adult adult { get; set; }
        public List<face> faces { get; set; }
        public metadata metadata { get; set; }
    }
}
