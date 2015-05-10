using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FaceAPIDemo.Model.FacePlusPlus;
namespace FaceAPIDemo.Detect.Faces
{
    public static class FacePlusPlus
    {
        public static DetectedResult recongnizeWithURL(string url)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["api_key"] = "<API_KEY>";
            queryString["api_secret"] = "<API_SECRET>";
            queryString["url"] = url;
            queryString["attribute"] = "gender,age,race,smiling";
            var uri = new Uri("https://apius.faceplusplus.com/v2/detection/detect?" + queryString);
            WebClient wb = new WebClient();
            var response = wb.DownloadString(uri);
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(DetectedResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
            var result = (DetectedResult)contract.ReadObject(mstream);
            return result;
        }
        public static DetectedResult recognizewithImg(byte[] img)
        {
            Dictionary<object, object> param = new Dictionary<object, object>();
            string url = "https://apius.faceplusplus.com/v2/detection/detect";
            param.Add("api_key", "<API_KEY>");
            param.Add("api_secret", "<API_SECRET>");
            param.Add("attribute", "gender,age,race,smiling");
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Stream rs = request.GetRequestStream();
            string responseStr = null;
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in param.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, param[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "img", img, "text/plain");//image/jpeg
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);
            rs.Write(img, 0, img.Length);
            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();
            WebResponse wresp = null;
            wresp = request.GetResponse();
            Stream stream2 = wresp.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            responseStr = reader2.ReadToEnd();
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(DetectedResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseStr));
            var result = (DetectedResult)contract.ReadObject(mstream);
            return result;
        }
    }
}
