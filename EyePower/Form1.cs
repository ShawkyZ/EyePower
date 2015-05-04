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
namespace FaceAPIDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            //TODO:Register App in Windows Registry
            if (Registry.GetValue(@"HKEY_CLASSES_ROOT\*\shell\Read Image Content\command","",null) == null)
            {
                try
                {
                    var key=Registry.ClassesRoot.OpenSubKey("*").OpenSubKey("shell",true);
                    key=key.CreateSubKey("Read Image Content");
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
                        detectVision(img);
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
        #region Vision API Functions
        private void detectVision(string imgPath)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Categories,Adult,Faces";
            queryString["subscription-key"] = "<Service_Subscrption_Key>";
            var uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            var data = Encoding.ASCII.GetBytes(imgPath);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;
            var responseString = "";
            try
            {
                new Thread(() =>
                    {
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }

                        var response = (HttpWebResponse)request.GetResponse();

                        responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(vision));
                        MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
                        var Vision = (vision)contract.ReadObject(mstream);
                        showResult(Vision);
                    }).Start();
            }
            catch { MessageBox.Show("Unsupported File Type"); }

        }
        private void detectVision(byte[] img)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["visualFeatures"] = "Categories,Adult,Faces";
            queryString["subscription-key"] = "<Service_Subscrption_Key>";
            var uri = new Uri("https://api.projectoxford.ai/vision/v1/analyses?" + queryString);
            var request = (HttpWebRequest)WebRequest.Create(uri);

           // var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = img.Length;
            var responseString = "";
            try
            {
                new Thread(() =>
                {
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(img, 0, img.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();

                    responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    DataContractJsonSerializer contract = new DataContractJsonSerializer(typeof(vision));
                    MemoryStream mstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseString));
                    var Vision = (vision)contract.ReadObject(mstream);
                    showResult(Vision);
                }).Start();
            }
            catch { MessageBox.Show("Unsupported File Type"); }

        }
        #endregion
        void showResult(vision Vision)
        {
            this.Invoke(new Action(()=>
            {
                richTextBox1.Clear();
                var res = Vision.faces.OrderBy(o => o.faceRectangle.left).ToArray();
                SpeechSynthesizer syn = new SpeechSynthesizer();
                syn.SetOutputToDefaultAudioDevice();
                bool hasFaces = false;
                //TODO:Check If the API Detected Categories For This Photo.
                if (Vision.categories!=null)
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
                //TODO:Check If The API Detected Adult Content In This Photo.
                if (Vision.adult.isAdultContent)
                {
                    syn.SpeakAsync("Image Has Adult Content\n");
                    richTextBox1.Text += "Image Has Adult Content\n";
                }
                //TODO:Check If The API Detected Human Faces In This Photo.
                if(Vision.faces!=null)
                    if (Vision.faces.Count() > 0)
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
                            richTextBox1.Text += "Age: " + item.age + " And the Gender is: " + item.gender + " " + "\n";
                            syn.SpeakAsync("Face Number: " + (++count) + " : Age: " + item.age + " And the Gender is: " + item.gender);
                        }
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
                lblState.Text = "Processing...";
                lblPath.Text = filename;
                button1.Enabled = false;
                button2.Enabled = false;
                ImageConverter im = new ImageConverter();
                Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = Bitmap.FromFile(openFileDialog1.FileName);
                byte[] img = (byte[])im.ConvertTo(bmp,typeof(byte[]));
                detectVision(img);
            }

        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(textBox1.Text, UriKind.Absolute))
            {
                lblState.Text = "Processing...";
                button1.Enabled = false;
                button2.Enabled = false;
                WebClient client = new WebClient();
                try
                {
                    byte[] imgbyte = await client.DownloadDataTaskAsync(new Uri(textBox1.Text));
                    MemoryStream ms = new MemoryStream(imgbyte);
                    pictureBox1.Image = Bitmap.FromStream(ms);
                    detectVision("{\"Url\":\"" + textBox1.Text + "\"}");
                }
                catch {
                    lblState.Text = "Idle";
                    button1.Enabled = true;
                    button2.Enabled = true;
                    MessageBox.Show("Unsupported File Type"); }
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

    }

}