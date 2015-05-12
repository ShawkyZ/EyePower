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
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

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
                if (args[1].ToLower().Contains(".jpg") || args[1].ToLower().Contains(".bmp") || args[1].ToLower().Contains(".png") || args[1].ToLower().Contains(".gif"))
                {
                        richTextBox1.Clear();
                        richTextBox1.Text = "Status: Uploading...";
                        lblPath.Text = args[1];
                        button1.Enabled = false;
                        button2.Enabled = false;
                        Bitmap bmp = new Bitmap(args[1]);
                        bool imageFlipped = false;
                        foreach (var p in bmp.PropertyItems)
                        {
                            if (p.Id == 274)
                            {
                                var Orientation = (int)p.Value[0];
                                if (Orientation == 6)
                                {
                                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    imageFlipped = true;
                                }
                                if (Orientation == 7)
                                {
                                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    imageFlipped = true;
                                }
                                if (Orientation == 8)
                                {
                                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                    imageFlipped = true;
                                }
                                break;
                            }
                        }
                        try
                        {
                            if (imageFlipped)
                            {
                                if (Path.GetFileName(args[1]).ToLower().EndsWith(".jpg"))
                                    bmp.Save(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1]), System.Drawing.Imaging.ImageFormat.Jpeg);
                                else if (Path.GetFileName(args[1]).ToLower().EndsWith(".png"))
                                    bmp.Save(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1]), System.Drawing.Imaging.ImageFormat.Png);
                                else if (Path.GetFileName(args[1]).ToLower().EndsWith(".gif"))
                                    bmp.Save(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1]), System.Drawing.Imaging.ImageFormat.Gif);
                                else if (Path.GetFileName(args[1]).ToLower().EndsWith(".bmp"))
                                    bmp.Save(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1]), System.Drawing.Imaging.ImageFormat.Bmp);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        Account acc = new Account("eyepower", "<API_KEY>", "<API_SECRET>");
                        Cloudinary cloud = new Cloudinary(acc);
                        var uploadResult = new ImageUploadResult();
                        string imgURI = "";
                        new Thread(() =>
                            {
                                try
                                {
                                    if (imageFlipped)
                                        uploadResult = cloud.Upload(new ImageUploadParams { File = new FileDescription(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1])) });
                                    else
                                        uploadResult = cloud.Upload(new ImageUploadParams { File = new FileDescription(args[1]) });
                                    imgURI = uploadResult.Uri.AbsoluteUri;
                                    if (imageFlipped)
                                        File.Delete(Path.GetDirectoryName(args[1]) + "\\Temp" + Path.GetFileName(args[1]));
                                    imageFlipped = false;
                                }
                                catch
                                {
                                    imageFlipped = false;
                                    MessageBox.Show("Error in Uploading The Image.");
                                    lblPath.Text = "";
                                    button1.Enabled = true;
                                   // button2.Enabled = true;
                                    richTextBox1.Text = "Image Content Here.";
                                    return;
                                }

                                pictureBox1.Image = bmp;
                                richTextBox1.Text = "Status: Processing...";

                                try
                                {
                                    var r = FacePlusPlus.recongnizeWithURL(imgURI);
                                    int apiNum = Decider.decideCategoryAPI(Application.ExecutablePath);
                                    switch (apiNum)
                                    {
                                        case 1:
                                            var a = Alchemy.recongnizeWithURL(imgURI);
                                            showResult(r, a);
                                            break;
                                        case 2:
                                            var re = Rekognize.recongnizeWithURL(imgURI);
                                            showResult(r, re);
                                            break;
                                        case 3:
                                            var v = Vision.recongnizeWithURL(imgURI);
                                            showResult(r, v);
                                            break;

                                        default:
                                            MessageBox.Show("Please Try Again");
                                            break;
                                    }
                                    DelResParams delPar = new DelResParams();
                                    delPar.PublicIds.Add(uploadResult.PublicId);
                                    cloud.DeleteResources(delPar);
                                    this.Invoke(new Action(() =>
                                    {
                                        button1.Enabled = true;
                                       // button2.Enabled = true;
                                    }));
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                            }).Start();
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
                            if (count == rekognize.scene_understanding.matches.Count - 1)
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
                            richTextBox1.Text += "Face Number " + (++count) + ": Age " + item.attribute.age.value + ", the Gender is " + item.attribute.gender.value + ", " + heshe[1] + " Race is " + item.attribute.race.value + ", And " + heshe[0] + " Looks " + faceState(Math.Round(item.attribute.smiling.value, 1)) + " (" + Math.Round(item.attribute.smiling.value, 0) + "% Smiling)." + "\n";
                        }
                    }
                }
                else
                {
                    richTextBox1.Text += "No Faces Detected.\n";
                    button1.Enabled = true;
                   // button2.Enabled = true;
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
                   // button2.Enabled = true;
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
                            richTextBox1.Text += "Face Number " + (++count) + ": Age " + item.attribute.age.value + ", the Gender is " + item.attribute.gender.value + ", " + heshe[1] + " Race is " + item.attribute.race.value + ", And " + heshe[0] + " Looks " + faceState(Math.Round(item.attribute.smiling.value, 1)) + " (" + Math.Round(item.attribute.smiling.value, 0) + "% Smiling)." + "\n";
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
                        richTextBox1.Text += "No Faces Detected.\n";
                    }

                }
                button1.Enabled = true;
              //  button2.Enabled = true;
            }));

        }
        private async void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string filename = openFileDialog1.FileName;
            if (filename != "")
            {
                richTextBox1.Clear();
                richTextBox1.Focus();
                richTextBox1.Text = "Status: Uploading...";
                lblPath.Text = filename;
                button1.Enabled = false;
                // button2.Enabled = false;
                Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                bool imageFlipped=false;
                foreach (var p in bmp.PropertyItems)
                {
                    if (p.Id == 274)
                    {
                        var Orientation = (int)p.Value[0];
                        if (Orientation == 6)
                        {
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            imageFlipped=true;
                        }
                        if (Orientation == 7)
                        {
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            imageFlipped=true;
                        }
                        if (Orientation == 8)
                        {
                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            imageFlipped=true;
                        }
                           break;
                    }
                }
                if(imageFlipped)
                {
                if(Path.GetFileName(openFileDialog1.FileName).ToLower().EndsWith(".jpg"))
                    bmp.Save(Path.GetDirectoryName(openFileDialog1.FileName) + "\\Temp" + Path.GetFileName(openFileDialog1.FileName),System.Drawing.Imaging.ImageFormat.Jpeg);
                else if (Path.GetFileName(openFileDialog1.FileName).ToLower().EndsWith(".png"))
                    bmp.Save(Path.GetDirectoryName(openFileDialog1.FileName) + "\\Temp" + Path.GetFileName(openFileDialog1.FileName),System.Drawing.Imaging.ImageFormat.Png);
                else if (Path.GetFileName(openFileDialog1.FileName).ToLower().EndsWith(".gif"))
                    bmp.Save(Path.GetDirectoryName(openFileDialog1.FileName) + "\\Temp" + Path.GetFileName(openFileDialog1.FileName), System.Drawing.Imaging.ImageFormat.Gif);
                else if (Path.GetFileName(openFileDialog1.FileName).ToLower().EndsWith(".bmp"))
                    bmp.Save(Path.GetDirectoryName(openFileDialog1.FileName) + "\\Temp" + Path.GetFileName(openFileDialog1.FileName), System.Drawing.Imaging.ImageFormat.Bmp);
                }
                Account acc = new Account("eyepower", "<API_KEY>", "<API_SECRET>");
                Cloudinary cloud = new Cloudinary(acc);
                var uploadResult = new ImageUploadResult();
                string imgURI = "";
                try
                {
                    if(imageFlipped)
                    uploadResult = await cloud.UploadAsync(new ImageUploadParams {File=new FileDescription(Path.GetDirectoryName(openFileDialog1.FileName)+"\\Temp"+Path.GetFileName(openFileDialog1.FileName)) });
                    else
                     uploadResult=await cloud.UploadAsync(new ImageUploadParams {File=new FileDescription (openFileDialog1.FileName)});
                    imgURI = uploadResult.Uri.AbsoluteUri;
                    if(imageFlipped)
                    File.Delete(Path.GetDirectoryName(openFileDialog1.FileName)+"\\Temp"+Path.GetFileName(openFileDialog1.FileName));
                    imageFlipped = false;
                }
                catch
                {
                    imageFlipped = false;
                    MessageBox.Show("Error in Uploading The Image.");
                    lblPath.Text = "";
                    button1.Enabled = true;
                    // button2.Enabled = true;
                    richTextBox1.Text = "Image Content Here.";
                    return;
                }
                
                pictureBox1.Image = bmp;
                richTextBox1.Text = "Status: Processing...";
                new Thread(() =>
                {
                    var r = FacePlusPlus.recongnizeWithURL(imgURI);
                    int apiNum = Decider.decideCategoryAPI(Application.ExecutablePath);
                    switch (apiNum)
                    {
                        case 1:
                            var a = Alchemy.recongnizeWithURL(imgURI);
                            showResult(r, a);
                            break;
                        case 2:
                            var re = Rekognize.recongnizeWithURL(imgURI);
                            showResult(r, re);
                            break;
                        case 3:
                            var v = Vision.recongnizeWithURL(imgURI);
                            showResult(r, v);
                            break;

                        default:
                            MessageBox.Show("Please Try Again");
                            break;
                    }
                    DelResParams delPar = new DelResParams();
                    delPar.PublicIds.Add(uploadResult.PublicId);
                    cloud.DeleteResources(delPar);
                    this.Invoke(new Action(() =>
                        {
                            button1.Enabled = true;
                           // button2.Enabled = true;
                        }));
                }).Start();

            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(textBox1.Text, UriKind.Absolute))
            {
                richTextBox1.Clear();
                richTextBox1.Focus();
                richTextBox1.Text = "Status: Processing...";
                button1.Enabled = false;
                button2.Enabled = false;
                WebClient client = new WebClient();
                try
                {
                    new Thread(() =>
                    {
                        byte[] imgbyte = client.DownloadData(new Uri(textBox1.Text));
                        MemoryStream ms = new MemoryStream(imgbyte);
                        pictureBox1.Image = Bitmap.FromStream(ms);
                        Alchemy.recongnizeWithURL(textBox1.Text);
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
                          //  button2.Enabled = true;
                        }));
                        textBox1.Text = "Paste Image URL Here";
                    }).Start();
                }
                catch
                {
                    textBox1.Text = "Paste Image URL Here";
                    richTextBox1.Text = "Image Content Here.";
                    button1.Enabled = true;
                   // button2.Enabled = true;
                    MessageBox.Show("Unsupported File Type");
                }
            }
            else
            {
                MessageBox.Show("Incorrect Url");
                button2.Enabled = false;
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
            AutoUpdater.Start("http://shawkyz.github.io/EyePower/app/Appcast.xml");
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
            Environment.Exit(Environment.ExitCode);
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

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (textBox1.Text == "Paste Image URL Here")
                textBox1.Text = "";
            else if (textBox1.Text == " " || textBox1.Text == "")
                textBox1.Text = "Paste Image URL Here";
        }
        protected override bool ProcessCmdKey(ref Message message, Keys keys)
        {
            switch (keys)
            {
                case Keys.Control | Keys.D1:
                    textBox1.Focus();
                    return true;
                case Keys.Control | Keys.D2:
                    button1.Focus();
                    return true;
                case Keys.Control | Keys.D3:
                    richTextBox1.Focus();
                    return true;
                case Keys.Control | Keys.H:
                    MessageBox.Show("To start using Eye Power Please do the following:\n1- If you want to add an Image URL press Control + 1. Then type or paste the URL and Press Enter.\n2- If you want to choose an Image from your PC press Control + 2 and press Enter to show The Open File Dialog and select the Image.\n3- If you want to show this text again press Control + H in the application.");
                    return true; 
            }
            return base.ProcessCmdKey(ref message, keys);
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Text != "" && textBox1.Text != " " && textBox1.Text != "Paste Image URL Here")
                button2.Enabled = true;
            if (e.KeyData == Keys.Back && textBox1.Text.Count() <= 1)
            {
                button2.Enabled = false;
                textBox1.Clear();
            }
            if (e.KeyData == Keys.Enter&&button2.Enabled)
                button2_Click(sender, e);
            if (e.KeyData == (Keys.Control | Keys.V))
                button2.Enabled = true;
        }
        private void readMeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To start using Eye Power Please do the following:\n1- If you want to add an Image URL press Control + 1. Then type or paste the URL and Press Enter.\n2- If you want to choose an Image from your PC press Control + 2 and press Enter to show The Open File Dialog and select the Image.\n3- If you want to show this text again press Control + H in the application.");
        }
    }
}
