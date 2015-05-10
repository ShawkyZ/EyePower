using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAPIDemo.Detect
{
    public static class Decider
    {
        public static int decideCategoryAPI(string path)
            {
            FileStream fs = new FileStream(Path.GetDirectoryName(path)+"\\lastUsed.ep", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string[] output = (sr.ReadLine()).Split(' ');
            sr.Close();
            fs.Close();
            string input="";
            int useAPI = 0;
            if (output.Count() <= 1)
            {
                if (output[0] == "3")
                {
                    input = "1 0";
                    useAPI = 1;
                }
            }
            else
            {
                int times = int.Parse(output[1]);
                if (output[0] == "1" && times >= 9)
                {
                    input = "2 0";
                    useAPI = 2;
                }
                else if (output[0] == "2" && times >= 4)
                {
                    input = "3";
                    useAPI = 3;
                }
                else
                {
                    input = output[0] + " " + (++times).ToString();
                    if (output[0] == "1")
                        useAPI = 1;
                    else if (output[0] == "2")
                        useAPI = 2;
                }
            }
            FileStream fss = new FileStream("lastUsed.ep", FileMode.Create);
            StreamWriter sw = new StreamWriter(fss);
            sw.WriteLine(input);
            sw.Close();
            fss.Close();
            return useAPI;
        }
    }
}
