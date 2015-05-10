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
using FaceAPIDemo.Model.Vision;
using FaceAPIDemo.Model.FacePlusPlus;
using FaceAPIDemo.Detect;
using FaceAPIDemo.Detect.Category;
using FaceAPIDemo.Detect.Faces;
using FaceAPIDemo.Model.Alchemy;
using FaceAPIDemo.Model.Rekognize;
using System.Security.AccessControl;
using System.Media;
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
            toolTip1.SetToolTip(textBox1, "URL");            
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "JPEG Image(*.jpg)|*.jpg|BMP Image(*.bmp)|*.bmp|PNG Image(*.png)|*.png|GIF Image(*.gif)|*.gif";
            //TODO: Check if the app was opened from Context Menu. If True We'll get the file path and process this file.            
            string[] args = Environment.GetCommandLineArgs();
            GrantAccess(Path.GetDirectoryName(args[0]) + "\\lastUsed.ep");
            if (args.Count() > 1)
            {
                if (args[1].Contains(".jpg") || args[1].Contains(".bmp") || args[1].Contains(".png") || args[1].Contains(".gif"))
                {
                    FileInfo info = new FileInfo(args[1]);
                    double sizeinMB = info.Length / (1024 * 1024);
                    if (sizeinMB <= 4)
                    {
                        richTextBox1.Text = "Status: Processing...\n\n";
                        lblPath.Text = args[1];
                        button1.Enabled = false;
                        button2.Enabled = false;
                        ImageConverter im = new ImageConverter();
                        Bitmap bmp = new Bitmap(args[1]);
                        pictureBox1.Image = Bitmap.FromFile(args[1]);
                        byte[] img = (byte[])im.ConvertTo(bmp, typeof(byte[]));
                        new Thread(() =>
                        {
                            try
                            {
                                var r = FacePlusPlus.recognizewithImg(img);
                                int apiNum = Decider.decideCategoryAPI(args[0]);
                                if (sizeinMB >= 1 && apiNum == 1)
                                    apiNum = 2;
                                switch (apiNum)
                                {
                                    case 1:
                                        var a = Alchemy.recongnizeWithImg(img);
                                        showResult(r, a);
                                        break;
                                    case 2:
                                        var re = Rekognize.recongnizeWithImg(img);
                                        showResult(r, re);
                                        break;
                                    case 3:
                                        var v = Vision.recognizewithImg(img);
                                        showResult(r, v);
                                        break;

                                    default:
                                        MessageBox.Show("Please Try Again");
                                        break;
                                }
                                this.Invoke(new Action(() =>
                                {
                                    button1.Enabled = true;
                                    button2.Enabled = true;
                                }));
                            }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }).Start();
                    }
                    else
                    {
                        MessageBox.Show("File Size Is Larger Than 4 MB");
                    }
                }
                else
                {
                    MessageBox.Show("Unsupported File Type");
                    Environment.Exit(Environment.ExitCode);
                }
            }
        }

        void showResult(DetectedResult result, RekognizeResult rekognize)
        {
            new Thread(() =>
            {
                new SoundPlayer(Path.GetDirectoryName(Application.ExecutablePath) + "\\done.wav").PlaySync();
            }).Start();
            this.Invoke(new Action(() =>
            {
                richTextBox1.Text = "Status: Done.\n\n";
                richTextBox1.Clear();
                var res = result.face.OrderBy((o) => o.position.center.x).ToArray();
                syn.SetOutputToDefaultAudioDevice();
                bool hasFaces = false;
                //TODO:Check If The API Detected Categories In This Photo.
                if (rekognize.scene_understanding.matches != null)
                {
                    if (rekognize.scene_understanding.matches.Count() > 0)
                    {
                        richTextBox1.Text += "Image Category is: ";
                        int count = 0;
                        foreach (var item in rekognize.scene_understanding.matches)
                        {
                            if(count==rekognize.scene_understanding.matches.Count-1)
                            richTextBox1.Text += item.tag.Replace('_', ' ') + ".\n";
                            else
                            richTextBox1.Text += item.tag.Replace('_', ' ') + ", ";
                            count++;
                        }
                    }
                }
                //Check If Face++ Returned Any Faces
                if (result.face != null)
                    if (result.face.Count() > 0)
                        hasFaces = true;
                if (hasFaces)
                {
                    richTextBox1.Text += res.Count() + " Faces Detected.\n";
                    if (res.Count() > 0)
                    {
                        if (res.Count() > 1)
                        {
                            richTextBox1.Text += "Starting from the Left.\n";
                        }
                        int count = 0;
                        foreach (var item in res)
                        {
                            string[] heshe = new string[2];
                            if (item.attribute.gender.value.ToLower() == "female")
                            {
                                heshe[0] = "she";
                                heshe[1] = "her";
                            }
                            else
                            {
                                heshe[0] = "he";
                                heshe[1] = "his";
                            }
                            richTextBox1.Text +="Face Number " + (++count)+ ": Age " + item.attribute.age.value + ", the Gender is " + item.attribute.gender.value + ", " + heshe[1] + " Race is " + item.attribute.race.value + ", And " + heshe[0] + " Looks " + faceState(Math.Round(item.attribute.smiling.value, 1)) + " (" + Math.Round(item.attribute.smiling.value, 0) + "% Smiling)." + "\n";
                        }
                    }
                }
                else
                {
                    richTextBox1.Text += "No Faces Detected.\n";
                    button1.Enabled = true;
                    button2.Enabled = true;
                }
            }));

        }
        void showResult(DetectedResult result, AlchemyResult alchemy)
        {
            new Thread(() =>
            {
                new SoundPlayer(Path.GetDirectoryName(Application.ExecutablePath) + "\\done.wav").PlaySync();
            }).Start();
            this.Invoke(new Action(() =>
            {
                richTextBox1.Text = "State: Done.\n\n";
                richTextBox1.Clear();
                var res = result.face.OrderBy((o) => o.position.center.x).ToArray();
                syn.SetOutputToDefaultAudioDevice();
                bool hasFaces = false;
                //TODO:Check If The API Detected Any Categories In This Photo.
                if (alchemy.imageKeywords != null)
                {
                    if (alchemy.imageKeywords.Count() > 0)
                    {
                        richTextBox1.Text += "Image Category is: ";
                        int count = 0;
                        foreach (var item in alchemy.imageKeywords)
                        {
                            if (count == alchemy.imageKeywords.Count - 1)
                                richTextBox1.Text += item.text.Replace('_', ' ') + ".\n";
                            else
                                richTextBox1.Text += item.text.Replace('_', ' ') + ", ";
                            count++;
                        }
                    }
                }

                //Check If Face++ Returned Any Faces
                if (result.face != null)
                    if (result.face.Count() > 0)
                        hasFaces = true;
                if (hasFaces)
                {
                    richTextBox1.Text += res.Count() + " Faces Detected.\n";
                    if (res.Count() > 0)
                    {
                        if (res.Count() > 1)
                        {
                            richTextBox1.Text += "Starting from the Left.\n";
                        }
                        int count = 0;
                        foreach (var item in res)
                        {
                            string[] heshe = new string[2];
                            if (item.attribute.gender.value.ToLower() == "female")
                            {
                                heshe[0] = "she";
                                heshe[1] = "her";
                            }
                            else
                            {
                                heshe[0] = "he";
                                heshe[1] = "his";
                            }
                            richTextBox1.Text += "Face Number " + (++count) + ": Age " + item.attribute.age.value + ", the Gender is " + item.attribute.gender.value + ", " + heshe[1] + " Race is " + item.attribute.race.value + ", And " + heshe[0] + " Looks " + faceState(Math.Round(item.attribute.smiling.value, 1)) + " (" + Math.Round(item.attribute.smiling.value, 0) + "% Smiling)." + "\n";
                        }
                    }
                }
                else
                {
                    richTextBox1.Text += "No Faces Detected.\n";
                    button1.Enabled = true;
                    button2.Enabled = true;
                }
            }));

        }
        void showResult(DetectedResult result, vision Vision)
        {
            new Thread(() =>
            {
                new SoundPlayer(Path.GetDirectoryName(Application.ExecutablePath) + "\\done.wav").PlaySync();
            }).Start();
            this.Invoke(new Action(() =>
            {
                richTextBox1.Text = "State: Done.\n\n";
                richTextBox1.Clear();
                var res = result.face.OrderBy((o) => o.position.center.x).ToArray();
                syn.SetOutputToDefaultAudioDevice();
                bool hasFaces = false;
                //TODO:Check If The API Detected Adult Content In This Photo.
                if (Vision.categories != null)
                {
                    if (Vision.categories.Count() > 0)
                    {
                        richTextBox1.Text += "Image Category is: ";
                        int count = 0;
                        foreach (var item in Vision.categories)
                        {
                            if (count == Vision.categories.Count - 1)
                                richTextBox1.Text += item.name.Replace('_', ' ') + ".\n";
                            else
                                richTextBox1.Text += item.name.Replace('_', ' ') + ", ";
                            count++;
                        }
                    }
                }
                ////TODO:Check If The OxFord Vision API Detected Adult Content In This Photo.
                if (Vision.adult.isAdultContent)
                {
                    richTextBox1.Text += "Image Has Adult Content.\n";
                }
                //Check If Face++ Returned Any Faces
                if (result.face != null)
                    if (result.face.Count() > 0)
                        hasFaces = true;
                if (hasFaces)
                {
                    richTextBox1.Text += res.Count() + " Faces Detected.\n";
                    if (res.Count() > 0)
                    {
                        if (res.Count() > 1)
                        {
                            richTextBox1.Text += "Starting from the Left.\n";
                        }
                        int count = 0;
                        foreach (var item in res)
                        {
                            string[] heshe = new string[2];
                            if (item.attribute.gender.value.ToLower() == "female")
                            {
                                heshe[0] = "she";
                                heshe[1] = "her";
                            }
                            else
                            {
                                heshe[0] = "he";
                                heshe[1] = "his";
                            }
                            richTextBox1.Text += "Face Number " + (++count) +": Age " + item.attribute.age.value + ", the Gender is " + item.attribute.gender.value + ", " + heshe[1] + " Race is " + item.attribute.race.value + ", And " + heshe[0] + " Looks " + faceState(Math.Round(item.attribute.smiling.value, 1)) + " (" + Math.Round(item.attribute.smiling.value, 0) + "% Smiling)." + "\n";
                        }
                    }
                }
                else
                {
                    //Face++ Didn't Return Any Faces So We'll See If OxFord Vision Returned any faces.
                    if (Vision.faces.Count > 0)
                    {
                        var ress = Vision.faces.OrderBy(o => o.faceRectangle.left).ToArray();
                        richTextBox1.Text += ress.Count() + " Faces Detected.\n";
                        if (ress.Count() > 0)
                        {
                            if (ress.Count() > 1)
                            {
                                richTextBox1.Text += "Starting from the Left.\n";
                            }
                            int count = 0;
                            foreach (var item in ress)
                            {
                                richTextBox1.Text += "Face Number " + (++count) + ": Age " + item.age + ", And the Gender is " + item.gender + " " + ".\n";
                            }
                        }
                    }
                    else
                    {
                        richTextBox1.Text += "No Faces Detected/\n";
                    }

                }
                button1.Enabled = true;
                button2.Enabled = true;
            }));

        }
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string filename = openFileDialog1.FileName;
            if (filename != "")
            {
                FileInfo info = new FileInfo(openFileDialog1.FileName);
                double imgsizeMB = info.Length / (1024 * 1024);
                if (imgsizeMB <= 4)
                {
                    richTextBox1.Clear();
                    richTextBox1.Text = "Status: Processing...";
                    lblPath.Text = filename;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    ImageConverter im = new ImageConverter();
                    Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                    pictureBox1.Image = Bitmap.FromFile(openFileDialog1.FileName);
                    byte[] img = (byte[])im.ConvertTo(bmp, typeof(byte[]));
                    new Thread(() =>
                    {
                        var r = FacePlusPlus.recognizewithImg(img);
                        int apiNum = Decider.decideCategoryAPI(Application.ExecutablePath);
                        if (imgsizeMB >= 1 && apiNum == 1)
                            apiNum = 2;
                        switch (apiNum)
                        {
                            case 1:
                                var a = Alchemy.recongnizeWithImg(img);
                                showResult(r, a);
                                break;
                            case 2:
                                var re = Rekognize.recongnizeWithImg(img);
                                showResult(r, re);
                                break;
                            case 3:
                                var v = Vision.recognizewithImg(img);
                                showResult(r, v);
                                break;

                            default:
                                MessageBox.Show("Please Try Again");
                                break;
                        }
                        this.Invoke(new Action(()=>
                            {
                        button1.Enabled = true;
                        button2.Enabled = true;
                            }));
                    }).Start();
                   
                }
                else
                {
                    MessageBox.Show("Image Size Is Larger Than 4 MB.");
                }
            }

        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(textBox1.Text, UriKind.Absolute))
            {
                try { syn.SpeakAsyncCancelAll(); }
                catch { }
                richTextBox1.Clear();
                richTextBox1.Text = "Status: Processing...";
                button1.Enabled = false;
                button2.Enabled = false;
                WebClient client = new WebClient();
                try
                {
                    byte[] imgbyte = await client.DownloadDataTaskAsync(new Uri(textBox1.Text));
                    MemoryStream ms = new MemoryStream(imgbyte);
                    pictureBox1.Image = Bitmap.FromStream(ms);
                    Alchemy.recongnizeWithURL(textBox1.Text);
                    new Thread(() =>
                    {
                        var r = FacePlusPlus.recongnizeWithURL(textBox1.Text);
                        int apiNum = Decider.decideCategoryAPI(Application.ExecutablePath);
                        switch (apiNum)
                        {
                            case 1:
                                var a = Alchemy.recongnizeWithURL(textBox1.Text);
                                showResult(r, a);
                                break;
                            case 2:
                                var re = Rekognize.recongnizeWithURL(textBox1.Text);
                                showResult(r, re);
                                break;
                            case 3:
                                var v = Vision.recongnizeWithURL(textBox1.Text);
                                showResult(r, v);
                                break;

                            default:
                                MessageBox.Show("Please Try Again");
                                break;
                        }
                        this.Invoke(new Action(() =>
                        {
                            button1.Enabled = true;
                            button2.Enabled = true;
                        }));
                    }).Start();
                }
                catch
                {
                    richTextBox1.Text = "Choose an image from your local PC By Clicking Browse Button, Or Paste Image URL in the URL EditBox and click Start.";
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
            AutoUpdater.Start("http://shawkyz.github.io/EyePower/app/Appcast.xml");
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
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        private bool GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl,
                                                             InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                                                             PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }

        private void webSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://shawkyz.github.io/EyePower/");
        }

    }

}