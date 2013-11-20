using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Imaging;
using OpenSURFcs;
using AForge;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;


namespace PanoramaMaker
{
    public partial class MainForm : Form
    {
        List<Image> input_images;       //input images
        List<List<IntPoint>> keypoints;   //each image has two lists of keypoints (left and right side) except the first and the last one (only right/left side)
        List<List<IntPoint>> correlationKeypoints; // centers calculated for merged image - added width
        List<MatrixH> homographyList;

        Bitmap panorama;

        enum ImageSection {Left, Right};
        int cropWidth;
        int cropWidthPercent = 100;

        private MatrixH homography;

        public MainForm()
        {
            InitializeComponent();

            input_images = new List<Image>();
            keypoints = new List<List<IntPoint>>();
            correlationKeypoints = new List<List<IntPoint>>();
            homographyList = new List<MatrixH>();
        }

        private static Bitmap Get24bppRgb(Image image)
        {
            var bitmap = new Bitmap(image);
            var bitmap24 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bitmap24))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, bitmap24.Width, bitmap24.Height));
            }
            return bitmap24;
        }

        #region MENU_CLICK_HANDLERS
        private void openInputImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            input_images = new List<Image>();
            keypoints = new List<List<IntPoint>>();

            foreach (Control control in flowLayoutPanel1.Controls.Cast<Control>().ToList())
            {
                flowLayoutPanel1.Controls.Remove(control);
                control.Dispose();
            }

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    input_images.Add(Image.FromFile(file));

                    ShowThumbnail(input_images.Last(), false);
                }

                cropWidth = input_images[0].Width * cropWidthPercent / 100;
                toolStripStatusLabel1.Text = "Input images loaded. Next you should calculate image keypoints.";
            }


        }

        private void savePanoramaImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Images (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                panorama.Save(saveFileDialog1.FileName);
                panorama.Dispose();
            }
        
        }

        private void calculateKeypointsSURFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detectKeypoints_SURF();
            DrawKeypoints();
            toolStripStatusLabel1.Text = "Keypoints calculated. Next you should merge images.";
        }

        private void calculateKeypointsHarrisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detectKeypoints_Harris();
            DrawKeypoints();
            toolStripStatusLabel1.Text = "Keypoints calculated. Next you should merge keypoints.";
        }


        private void matchKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MatchKeypoints();
            toolStripStatusLabel1.Text = "Keypoints are matched. Next you should blend images.";
        }


        private void blendImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BlendImages();
            toolStripStatusLabel1.Text = "Images are blended. You now have panorama image.";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
        #endregion

        private void ShowLargeImage(object sender, MouseEventArgs e, Image image)
        {
            ZoomPicBox pb = new ZoomPicBox();
            pb.Dock = DockStyle.Fill;
            pb.AutoScroll = true;
            pb.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            pb.Image = image;
            pb.Zoom = 0.3f;
            pb.MouseWheel += new MouseEventHandler((sender_new, e_new) => { pb.Zoom += e_new.Delta > 0 ? 0.1f : -0.1f; });

            Form imageWindow = new Form();
            imageWindow.Width = 1280;
            imageWindow.Height = 768;
            imageWindow.Controls.Add(pb);
            imageWindow.ShowDialog();
        }

        private void ShowThumbnail(Image image, bool isMerged)
        {
            Image thumb;
            if(isMerged)
                thumb = image.GetThumbnailImage(input_images.Count * 240, flowLayoutPanel1.Height - 10, () => false, IntPtr.Zero);
            else
                thumb = image.GetThumbnailImage(240, flowLayoutPanel1.Height - 10, () => false, IntPtr.Zero);

            PictureBox pb = new PictureBox();
            pb.Height = thumb.Height;
            pb.Width = thumb.Width;
            pb.Image = thumb;
            flowLayoutPanel1.Controls.Add(pb);

            pb.MouseClick += new MouseEventHandler((sender_new, e_new) => ShowLargeImage(sender_new, e_new, image));
            pb.MouseEnter += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Hand; });
            pb.MouseLeave += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Default; });
        }

        private void ClearThumbnails()
        {
            List<Control> pictureBox_thumbnails = flowLayoutPanel1.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails) //first clear the old thumbnails
            {
                flowLayoutPanel1.Controls.Remove(control);
                control.Dispose();
            }
        }

        private Bitmap GetCroppedImage(Image image, ImageSection side)
        {
            Rectangle cropSection;
            if (side == ImageSection.Left)
                cropSection = new Rectangle(0, 0, cropWidth, image.Height);
            else
                cropSection = new Rectangle(image.Width - cropWidth, 0, cropWidth, image.Height);

            Bitmap sourceImage = new Bitmap(image);
            // DEBUG
            //return sourceImage.Clone(cropSection, sourceImage.PixelFormat);
            return sourceImage;
        }

        /*
         * Detect keypoints in images using OpenSURF library
         * http://www.chrisevansdev.com/computer-vision-opensurf.html
         */
        private void detectKeypoints_SURF()
        {
            IntegralImage integralImage;
            for (int i = 0; i < input_images.Count; i++)
            {
                if (i != 0) //if not first image, calculate left side keypoints
                {
                    integralImage = IntegralImage.FromImage(GetCroppedImage(input_images[i], ImageSection.Left));
                    List<IPoint> surf_keypoints = FastHessian.getIpoints(0.001f, 5, 2, integralImage);

                    keypoints.Add(new List<IntPoint>());
                    foreach (IPoint p in surf_keypoints)
                        keypoints.Last().Add(new IntPoint((int)p.x, (int)p.y));
                }
                if (i != input_images.Count - 1) //if not last image, calculate right side keypoints
                {
                    integralImage = IntegralImage.FromImage(GetCroppedImage(input_images[i], ImageSection.Right));
                    List<IPoint> surf_keypoints = FastHessian.getIpoints(0.0002f, 5, 2, integralImage);

                    keypoints.Add(new List<IntPoint>());
                    foreach (IPoint p in surf_keypoints)
                        keypoints.Last().Add(new IntPoint((int)p.x, (int)p.y));
                }
            }
        }

        /*
         * Detect keypoints in images using Harris corner detector from Accord.NET library
         * http://accord-framework.net/docs/html/T_Accord_Imaging_HarrisCornersDetector.htm
         */
        private void detectKeypoints_Harris()
        {
            HarrisCornersDetector harris_detector = new HarrisCornersDetector(0.04f, 500f);
            for (int i = 0; i < input_images.Count; i++)
            {
                if (i != 0) //if not first image, calculate left side keypoints
                {
                    keypoints.Add(harris_detector.ProcessImage(GetCroppedImage(input_images[i], ImageSection.Left)));
                }
                if (i != input_images.Count - 1) //if not last image, calculate right side keypoints
                {
                    keypoints.Add(harris_detector.ProcessImage(GetCroppedImage(input_images[i], ImageSection.Right)));
                }
            }
        }


        private void DrawKeypoints()
        {
            Graphics graphics;
            Pen keypointPen = new Pen(Color.GreenYellow, 2f);
            int keypoints_cntr = 0;

            ClearThumbnails();

            for (int i = 0; i < keypoints.Count; i++) correlationKeypoints.Add(new List<IntPoint>());

            Bitmap mergedImage = new Bitmap(input_images.Count * input_images[0].Width, input_images[0].Height); //it is assumed the images are of same size!
            graphics = Graphics.FromImage(mergedImage);

            int cumulativeWidth = 0;
            for (int i = 0; i < input_images.Count; i++)
            {
                graphics.DrawImage(input_images[i], cumulativeWidth, 0, input_images[i].Width, input_images[i].Height);

                foreach (IntPoint keypoint in keypoints[keypoints_cntr++])
                {

                    int diameter = 10;
                    int radius = Convert.ToInt32(diameter / 2f);

                    System.Drawing.Point center;
                    if (i == 0) //if this is first image, keypoints are on the right side
                        center = new System.Drawing.Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.X), Convert.ToInt32(keypoint.Y));
                    else //else keypoints are on the left side
                        center = new System.Drawing.Point(Convert.ToInt32(cumulativeWidth + keypoint.X), Convert.ToInt32(keypoint.Y));

                    //correlationKeypoints[keypoints_cntr - 1].Add(new IntPoint(center.X, center.Y));

                    graphics.DrawEllipse(keypointPen, center.X - radius, center.Y - radius, diameter, diameter);

                }
                if (i == 0 || i == input_images.Count - 1) //if this is the first image or last image it has only one side of keypoints
                {
                    cumulativeWidth += input_images[0].Width;
                    continue;
                }

                foreach (IntPoint keypoint in keypoints[keypoints_cntr++])
                {
                    int diameter = 10;
                    int radius = Convert.ToInt32(diameter / 2f);

                    System.Drawing.Point center;
                    if (i == input_images.Count - 1) //if this is last image, the keypoints are on the left side
                        center = new System.Drawing.Point(Convert.ToInt32(cumulativeWidth + keypoint.X), Convert.ToInt32(keypoint.Y));
                    else //else keypoints are on the right
                        center = new System.Drawing.Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.X), Convert.ToInt32(keypoint.Y));

                    //correlationKeypoints[keypoints_cntr - 1].Add(new IntPoint(center.X, center.Y));

                    graphics.DrawEllipse(keypointPen, center.X - radius, center.Y - radius, diameter, diameter);
                }
                cumulativeWidth += input_images[0].Width;
            }
            ShowThumbnail(mergedImage, true);
        }


        private void MatchKeypoints()
        {
            // matching keypoints step needs at least two images
            if (input_images.Count < 2) return;


            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            CorrelationMatching matcher = new CorrelationMatching(9);
            
            IntPoint[][] matches;
            homographyList = new List<MatrixH>();


            Graphics graphics;
            Pen keypointPen = new Pen(Color.GreenYellow, 2f);
            int keypoints_cntr = 0;


            Bitmap mergedImage = new Bitmap(input_images.Count * input_images[0].Width, input_images[0].Height); //it is assumed the images are of same size!
            graphics = Graphics.FromImage(mergedImage);

            int cumulativeWidth = 0;

            for (int i = 0; i < input_images.Count; i++) {
                graphics.DrawImage(input_images[i], cumulativeWidth, 0, input_images[i].Width, input_images[i].Height);

                cumulativeWidth += input_images[i].Width;
            }

            for (int i = 0; i < input_images.Count; i++)
            {

                if (i != input_images.Count - 1)
                {
                    matches = matcher.Match(new Bitmap(input_images[i]), new Bitmap(input_images[i + 1]), keypoints[keypoints_cntr].ToArray(), keypoints[keypoints_cntr + 1].ToArray());
   
                    
                    IntPoint[] correlationPoints1 = matches[0];
                    IntPoint[] correlationPoints2 = matches[1];

                    

                    // Plot RANSAC results against correlation results
                    homography = ransac.Estimate(correlationPoints1, correlationPoints2);

                    homographyList.Add(homography);
    
                    correlationPoints1 = correlationPoints1.Submatrix(ransac.Inliers);
                    correlationPoints2 = correlationPoints2.Submatrix(ransac.Inliers);

                    // store correlations in list
                    correlationKeypoints[keypoints_cntr].AddRange(correlationPoints1);
                    correlationKeypoints[keypoints_cntr + 1].AddRange(correlationPoints2);

                    PairsMarker pairs = new PairsMarker(correlationPoints1, correlationPoints2.Apply(p => new IntPoint(p.X + input_images[i].Width, p.Y)));
       
                    for (int j = 0; j < pairs.Points1.Count(); j++) {
                        if (i % 2 == 0) graphics.DrawLine(new Pen(Color.GreenYellow, 1.5f), new System.Drawing.Point(correlationPoints1[j].X + i * input_images[i].Width, correlationPoints1[j].Y), new System.Drawing.Point(correlationPoints2[j].X + (i + 1) * input_images[i].Width, correlationPoints2[j].Y));
                        else            graphics.DrawLine(new Pen(Color.Red, 1.5f), new System.Drawing.Point(correlationPoints1[j].X + i * input_images[i].Width, correlationPoints1[j].Y), new System.Drawing.Point(correlationPoints2[j].X + (i + 1) * input_images[i].Width, correlationPoints2[j].Y));
                        //graphics.DrawLine(new Pen(Color.GreenYellow, 2f), new System.Drawing.Point(pairs.Points1[j].X, pairs.Points1[j].Y), new System.Drawing.Point(pairs.Points2[j].X, pairs.Points2[j].Y));
                    }
                  
                    
                    keypoints_cntr += 2;
                }

                cumulativeWidth += input_images[0].Width;

            }


            ClearThumbnails();
            ShowThumbnail(mergedImage, true);
            
        }
/*
        private void RANSAC()
        {

            // matching keypoints step needs at least two images
            if (input_images.Count < 2) return;


            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);


            Graphics graphics;
            Pen keypointPen = new Pen(Color.GreenYellow, 2f);
            int keypoints_cntr = 0;

            ClearThumbnails();

            Bitmap mergedImage = new Bitmap(input_images.Count * input_images[0].Width, input_images[0].Height); //it is assumed the images are of same size!
            graphics = Graphics.FromImage(mergedImage);

            int cumulativeWidth = 0;

            for (int i = 0; i < input_images.Count; i++)
            {
                graphics.DrawImage(input_images[i], cumulativeWidth, 0, input_images[i].Width, input_images[i].Height);

                cumulativeWidth += input_images[i].Width;
            }

            for (int i = 0; i < input_images.Count; i++)
            {

                if (i != input_images.Count - 1)
                {
                    matches = matcher.Match(new Bitmap(input_images[i]), new Bitmap(input_images[i + 1]), keypoints[keypoints_cntr].ToArray(), keypoints[keypoints_cntr + 1].ToArray());


                    IntPoint[] correlationPoints1 = matches[0];
                    IntPoint[] correlationPoints2 = matches[1];

                    homography = ransac.Estimate(correlationPoints1, correlationPoints2);

                    // Plot RANSAC results against correlation results
                    correlationPoints1 = correlationPoints1.Submatrix(ransac.Inliers);
                    correlationPoints2 = correlationPoints2.Submatrix(ransac.Inliers);



                    PairsMarker pairs = new PairsMarker(correlationPoints1, correlationPoints2);

                    for (int j = 0; j < pairs.Points1.Count(); j++)
                    {
                        graphics.DrawLine(new Pen(Color.GreenYellow, 2f), new System.Drawing.Point(correlationPoints1[j].X + i * input_images[i].Width, correlationPoints1[j].Y), new System.Drawing.Point(correlationPoints2[j].X + (i + 1) * input_images[i].Width, correlationPoints2[j].Y));
                    }


                    keypoints_cntr += 2;
                }

                cumulativeWidth += input_images[0].Width;

            }



            ShowThumbnail(mergedImage, true);
        }
*/
        private void BlendImages()
        {
            // 1 - 2 image
            Blend blend = new Blend(homographyList[0], new Bitmap(input_images[0]));
            panorama = blend.Apply(new Bitmap(input_images[1]));


            //Blend blend2 = new Blend(homographyList[1], new Bitmap(input_images[1]));
            //panorama = blend2.Apply(new Bitmap(input_images[2]));


            //Concatenate concat = new Concatenate(mergedImage);
            //Bitmap panorama = concat.Apply(mergedImage2);


           // mergedImage.Concatenate()

            ClearThumbnails();
            ShowThumbnail(panorama, true);

            // 2 - 3
            //Blend blend = new Blend(homographyList[1], new Bitmap(input_images[1]));
            //Bitmap mergedImage = blend.Apply(new Bitmap(input_images[2]));
            //mergedImage = blend.Apply(mergedImage);
            //ClearThumbnails();
            //ShowThumbnail(mergedImage, true);
        }

        private void Main()
        {
            int images_count = input_images.Count;
            for (int i = 0; i < images_count; i++)
            {

            }
        }





    }
}
