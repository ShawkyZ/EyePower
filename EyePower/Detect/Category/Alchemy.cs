using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FaceAPIDemo.Model.Alchemy;

namespace FaceAPIDemo.Detect.Category
{
    public class Alchemy
    {
        public static AlchemyResult recongnizeWithURL(string url)
        {
            var queryString = HttpUtility.ParseQueryString(String.Empty);
            queryString["url"] = (new Uri(url)).ToString();
            queryString["apikey"] = "<API_KEY>";
            queryString["outputMode"] = "json";
            queryString["forceShowAll"] = "0";
            queryString["knowledgeGraph"] = "0";
            WebClient client = new WebClient();
            client.Proxy = null;
            var response =client.DownloadString(new Uri("http://access.alchemyapi.com/calls/url/URLGetRankedImageKeywords?"+queryString));
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(AlchemyResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
            AlchemyResult res =(AlchemyResult) contract.ReadObject(mstream);
            return res;
        }
        public static AlchemyResult recongnizeWithImg(byte[] img)
        {
            WebClient clients = new WebClient();
            var queryString = HttpUtility.ParseQueryString(String.Empty);
            clients.Headers.Add("Content-Type", "application/x-www-form-urlencode");
            queryString["apikey"] = "<API_KEY>";
            queryString["imagePostMode"] = "raw";
            queryString["outputMode"] = "json";
            queryString["forceShowAll"] = "0";
            queryString["knowledgeGraph"] = "0";
            WebClient client = new WebClient();
            client.Proxy = null;
            var uri = new Uri("http://access.alchemyapi.com/calls/image/ImageGetRankedImageKeywords?"+queryString);
            var response = client.UploadData(uri, img);
            string json = System.Text.Encoding.UTF8.GetString(response);
            DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(AlchemyResult));
            MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            AlchemyResult res = (AlchemyResult)contract.ReadObject(mstream);
            return res;
        }
    }
}
