using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using FaceAPIDemo.Model.Rekognize;

namespace FaceAPIDemo.Detect.Category
{
    public static class Rekognize
    {
        public static RekognizeResult recongnizeWithURL(string url)
        {
            WebClient client = new WebClient();
            NameValueCollection collection = new NameValueCollection();
            collection["api_key"] = "<API_KEY>";
            collection["api_secret"] = "<API_SECRET>";
            collection["jobs"] = "scene_understanding_3";
            collection["num_return"] = "3";
            collection["urls"] = url;
            var response = client.UploadValues("http://rekognition.com/func/api/", collection);
            string json = System.Text.Encoding.UTF8.GetString(response);
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(RekognizeResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var res = (RekognizeResult)contract.ReadObject(mstream);
            return res;
        }
        public static RekognizeResult recongnizeWithImg(byte[] img)
        {
            WebClient client = new WebClient();
            NameValueCollection collection = new NameValueCollection();
            collection["api_key"]="<API_KEY>";
            collection["api_secret"]="<API_SECRET>";
            collection["jobs"]="scene_understanding_3";
            collection["num_return"]="3";
            collection["base64"] = Convert.ToBase64String(img);
            var response = client.UploadValues("http://rekognition.com/func/api/", collection);
            string json = System.Text.Encoding.UTF8.GetString(response);
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(RekognizeResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var res = (RekognizeResult)contract.ReadObject(mstream);
            return res;
        }
    }
}
