using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Runtime.Serialization.Json;
using FaceAPIDemo.Model;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using AutoUpdaterDotNET;
using System.Net.Security;
namespace FaceAPIDemo
{
    public partial class Form1 : Form
    {
        SpeechSynthesizer syn = new SpeechSynthesizer();
        public Form1()
        {
            //TODO:Register App in Windows Registry
            if (Registry.GetValue(@"HKEY_CLASSES_ROOT\*\shell\Read Image Content\command", "", null) == null)
            {
                try
                {
                    var key = Registry.ClassesRoot.OpenSubKey("*").OpenSubKey("shell", true);
                    key = key.CreateSubKey("Read Image Content");
                    key = key.CreateSubKey("command");
                    key.SetValue("", Application.ExecutablePath + " \"%1\"");
                }
                catch
                {
                    MessageBox.Show("Open The App As Administrator\nWe just need it for the first run.", "Notification", MessageBoxButtons.OK);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "JPEG Image(*.jpg)|*.jpg|BMP Image(*.bmp)|*.bmp|PNG Image(*.png)|*.png|GIF Image(*.gif)|*.gif";
            //TODO: Check if the app was opened from Context Menu. If True We'll get the file path and process this file.
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Count() > 0)
                {
                    if (args[1].Contains(".jpg") || args[1].Contains(".bmp") || args[1].Contains(".png") || args[1].Contains(".gif"))
                    {
                        lblState.Text = "Processing...";
                        lblPath.Text = args[1];
                        button1.Enabled = false;
                        button2.Enabled = false;
                        ImageConverter im = new ImageConverter();
                        Bitmap bmp = new Bitmap(args[1]);
                        pictureBox1.Image = Bitmap.FromFile(args[1]);
                        byte[] img = (byte[])im.ConvertTo(bmp, typeof(byte[]));
                        detectFace(img);
                    }
                    else
                    {
                        MessageBox.Show("Unsupported File Type");
                        Environment.Exit(Environment.ExitCode);
                    }
                }
            }
            catch { }
        }
        #region Detection API Functions
        private void detectFace(string imgPath)
        {
            //Get Result From Face++ Detection API
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["api_key"] = "da24b5e759a050c278bfcc8a4a67a77d";
            queryString["api_secret"] = "zOS_jlBNdHdWyHrQDaksUdXf39f_J5xJ";
            queryString["url"] = imgPath;
            queryString["attribute"] = "gender,age,race,smiling";
            var uri = new Uri("https://apius.faceplusplus.com/v2/detection/detect?" + queryString);
            WebClient wb = new WebClient();
            try
            {
                new Thread(() =>
                {
                    var response = wb.DownloadString(uri);

                    DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(DetectedResult));
                    MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(response));
                    var result = (DetectedResult)contract.ReadObject(mstream);
                    //Get Result From Oxford Vision Detection API
                    queryString = HttpUtility.ParseQueryString(string.Empty);
                    queryString["visualFeatures"] = "Categories,Adult,Faces";
                    queryString["subscription-key"] = "c253f41b476747f99326b66d8ab87a35";
                    uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    var data = Encoding.ASCII.GetBytes("{\"Url\":\"" + imgPath + "\"}");
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = data.Length;
                    var responseString = "";


                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response1 = (HttpWebResponse)request.GetResponse();

                    responseString = new StreamReader(response1.GetResponseStream()).ReadToEnd();
                    contract = new DataContractJsonSerializer(typeof(vision));
                    mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
                    var Vision = (vision)contract.ReadObject(mstream);
                    showResult(result, Vision);
                }).Start();
            }
            catch { MessageBox.Show("Unsupported File Type"); }

        }
        public void detectFace(byte[] img)
        {
            //Get Result From Face++ Detection API
            Dictionary<object, object> param = new Dictionary<object, object>();
            string url = "https://apius.faceplusplus.com/v2/detection/detect";
            param.Add("api_key", "da24b5e759a050c278bfcc8a4a67a77d");
            param.Add("api_secret", "zOS_jlBNdHdWyHrQDaksUdXf39f_J5xJ");
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
            try
            {
                new Thread(() =>
                    {
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

                        //Get Result From OxFord Vision Detection API
                        var queryString = HttpUtility.ParseQueryString(string.Empty);
                        queryString["visualFeatures"] = "Categories,Adult,Faces";
                        queryString["subscription-key"] = "c253f41b476747f99326b66d8ab87a35";
                        var uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
                        request = (HttpWebRequest)WebRequest.Create(uri);
                        request.Method = "POST";
                        request.ContentType = "application/octet-stream";
                        request.ContentLength = img.Length;
                        var responseString = "";
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(img, 0, img.Length);
                        }
                        var response = (HttpWebResponse)request.GetResponse();
                        responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        contract = new DataContractJsonSerializer(typeof(vision));
                        mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
                        var Vision = (vision)contract.ReadObject(mstream);
                        showResult(result, Vision);
                    }).Start();
            }
            catch { MessageBox.Show("Unsupported File Type"); }
        }
        #endregion

        void showResult(DetectedResult result, vision Vision)
        {
            this.Invoke(new Action(() =>
            {
                button3.Show();
                richTextBox1.Clear();
                var res = result.face.OrderBy((o) => o.position.center.x).ToArray();
                syn.SetOutputToDefaultAudioDevice();
                bool hasFaces = false;
                //TODO:Check If The API Detected Adult Content In This Photo.
                if (Vision.categories != null)
                {
                    if (Vision.categories.Count() > 0)
                    {
                        syn.SpeakAsync("Image Category is:  ");
                        richTextBox1.Text += "Image Category is:  ";
                        foreach (var item in Vision.categories)
                        {
                            syn.SpeakAsync(item.name.Replace('_', ' ') + "\n");
                            richTextBox1.Text += item.name.Replace('_', ' ') + "\n";
                        }
                    }
                }
                //TODO:Check If The OxFord Vision API Detected Adult Content In This Photo.
                if (Vision.adult.isAdultContent)
                {
                    syn.SpeakAsync("Image Has Adult Content\n");
                    richTextBox1.Text += "Image Has Adult Content\n";
                }
                //Check If Face++ Returned Any Faces
                if (result.face != null)
                    if (result.face.Count() > 0)
                        hasFaces = true;
                if (hasFaces)
                {
                    syn.SpeakAsync(res.Count() + " Faces Detected\n");
                    richTextBox1.Text += res.Count() + " Faces Detected\n";
                    if (res.Count() > 0)
                    {
                        if (res.Count() > 1)
                        {
                            syn.SpeakAsync("Starting from the Left\n");
                            richTextBox1.Text += "Starting from the Left\n";
                        }
                        int count = 0;
                        foreach (var item in res)
                        {
                            string[] heshe = new string[2];
                            if(item.attribute.gender.value.ToLower()=="female")
                            {
                                heshe[0] = "she";
                                heshe[1] = "her";
                            }
                            else
                            {
                                heshe[0] = "he";
                                heshe[1] = "his";
                            }
                            richTextBox1.Text += "Age: " + item.attribute.age.value + " the Gender is: " + item.attribute.gender.value +  " "+heshe[1]+" Race is: " + item.attribute.race.value + " And "+heshe[0]+" Looks: " + faceState(Math.Round(item.attribute.smiling.value, 1)) + "\n";
                            syn.SpeakAsync("Face Number: " + (++count) + " : Age: " + item.attribute.age.value + " the Gender is: " + item.attribute.gender.value + " " + heshe[1] + " Race is: " + item.attribute.race.value + " And " + heshe[0] + " Looks: " + faceState(Math.Round(item.attribute.smiling.value, 1)) + "\n");
                        }
                    }
                }
                else
                {
                    //Face++ Didn't Return Any Faces So We'll See If OxFord Vision Returned any faces.
                    if (Vision.faces.Count > 0)
                    {
                        var ress = Vision.faces.OrderBy(o => o.faceRectangle.left).ToArray();
                        syn.SpeakAsync(ress.Count() + " Faces Detected\n");
                        richTextBox1.Text += ress.Count() + " Faces Detected\n";
                        if (ress.Count() > 0)
                        {
                            if (ress.Count() > 1)
                            {
                                syn.SpeakAsync("Starting from the Left\n");
                                richTextBox1.Text += "Starting from the Left\n";
                            }
                            int count = 0;
                            foreach (var item in ress)
                            {
                                richTextBox1.Text += "Age: " + item.age + " And the Gender is: " + item.gender + " " + "\n";
                                syn.SpeakAsync("Face Number: " + (++count) + " : Age: " + item.age + " And the Gender is: " + item.gender);
                            }
                        }
                    }
                    else
                    {
                        syn.SpeakAsync("No Faces Detected\n");
                        richTextBox1.Text += "No Faces Detected\n";
                    }

                }
                button1.Enabled = true;
                button2.Enabled = true;
                lblState.Text = "Done";

            }));

        }
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string filename = openFileDialog1.FileName;
            if (filename != "")
            {
                try { syn.SpeakAsyncCancelAll(); }
                catch { }
                richTextBox1.Clear();
                lblState.Text = "Processing...";
                lblPath.Text = filename;
                button1.Enabled = false;
                button2.Enabled = false;
                ImageConverter im = new ImageConverter();
                Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = Bitmap.FromFile(openFileDialog1.FileName);
                byte[] img = (byte[])im.ConvertTo(bmp, typeof(byte[]));
                detectFace(img);
            }

        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(textBox1.Text, UriKind.Absolute))
            {
                try { syn.SpeakAsyncCancelAll(); }
                catch { }
                richTextBox1.Clear();
                lblState.Text = "Processing...";
                button1.Enabled = false;
                button2.Enabled = false;
                WebClient client = new WebClient();
                try
                {
                    byte[] imgbyte = await client.DownloadDataTaskAsync(new Uri(textBox1.Text));
                    MemoryStream ms = new MemoryStream(imgbyte);
                    pictureBox1.Image = Bitmap.FromStream(ms);
                    detectFace(textBox1.Text);
                }
                catch
                {
                    lblState.Text = "Idle";
                    button1.Enabled = true;
                    button2.Enabled = true;
                    MessageBox.Show("Unsupported File Type");
                }
            }
            else
            {
                MessageBox.Show("Incorrect Url");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/ShawkyZ");
        }

        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ShawkyZ/EyePower");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AutoUpdater.Start("http://shawkyz.azurewebsites.net/apps/eyepower/Appcast.xml");
        }
        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoUpdater.Start("http://shawkyz.azurewebsites.net/apps/eyepower/Appcast.xml");
        }
        string faceState(double smileValue)
        {
            if (smileValue >= 0 && smileValue <= 20)
                return "Very Sad";
            else if (smileValue >= 21 && smileValue <= 40)
                return "Sad";
            else if (smileValue >= 41 && smileValue <= 60)
                return "Normal";
            else if (smileValue >= 61 && smileValue <= 80)
                return "Happy";
            else
                return "Very Happy";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try { syn.SpeakAsyncCancelAll(); button3.Hide(); }
            catch { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { syn.SpeakAsyncCancelAll();
            }
            catch { }
        }

    }

}