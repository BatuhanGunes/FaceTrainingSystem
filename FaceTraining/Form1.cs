﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FaceTraining
{
    public partial class Form1 : Form
    {
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        private readonly HaarCascade faceDetected;
        private Image<Bgr, Byte> Frame;
        private Capture camera;
        private Image<Gray, byte> result;
        private Image<Gray, byte> TrainedFace = null;
        private Image<Gray, byte> grayFace = null;
        private readonly List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        private readonly List<string> labels = new List<string>();
        private readonly List<string> Users = new List<string>();
        private int Count;
        private readonly int NumLables;
        string name = null;

        public Form1()
        {
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');
                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;
                string FacesLoad;
                for(int i = 1; i < NumLables + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/face" + i + ".bmp"));
                    labels.Add(Labels[i]);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ButtonBaslat_Click(object sender, EventArgs e)
        {
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }
        private void ButtonKaydet_Click(object sender, EventArgs e)
        {
            Count += 1;
            grayFace = camera.QueryGrayFrame().Resize(480, 360, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] DetectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in DetectedFaces[0])
            {
                TrainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            trainingImages.Add(TrainedFace);
            labels.Add(textBoxIsim.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");
            for (int i = 1;i<trainingImages.ToArray().Length + 1; i++)
            {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray()[i - 1] + ",");
            }
            MessageBox.Show(textBoxIsim.Text + " eklendi.");
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            Users.Add("");
            Frame = camera.QueryFrame().Resize(480, 360, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if(trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias =new MCvTermCriteria(Count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));    
                }
                Users.Add("");
            }
            cameraBox.Image = Frame;
            Users.Clear();
        }
    }
}
