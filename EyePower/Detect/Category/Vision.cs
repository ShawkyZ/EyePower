using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FaceAPIDemo.Model.Vision;

namespace FaceAPIDemo.Detect.Category
{
    public static class Vision
    {
        public static vision recongnizeWithURL(string url)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Categories,Adult,Faces";
            queryString["subscription-key"] = "<SUBSCRIPTION_KEY>";
            var uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            var data = Encoding.ASCII.GetBytes("{\"Url\":\"" + url + "\"}");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Proxy = null;
            request.ContentLength = data.Length;
            var responseString = "";
            using (var stream = request.GetRequestStream()) 
            {
                stream.Write(data, 0, data.Length);
            }
            var response1 = (HttpWebResponse)request.GetResponse();
            responseString = new StreamReader(response1.GetResponseStream()).ReadToEnd();
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(vision));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
            vision Vision = (vision)contract.ReadObject(mstream);
            return Vision;
        }
        public static vision recognizewithImg(byte[] img)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Categories,Adult,Faces";
            queryString["subscription-key"] = "<SUBSCRIPTION_KEY>";
            var uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.Proxy = null;
            request.ContentType = "application/octet-stream";
            request.ContentLength = img.Length;
            var responseString = "";
            using (var stream = request.GetRequestStream())
            {
                stream.Write(img, 0, img.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(vision));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
            vision Vision = (vision)contract.ReadObject(mstream);
            return Vision;
        }


    }
}
