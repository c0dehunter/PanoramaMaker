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
        /// <summary>
        /// Input images that user selects.
        /// </summary>
        List<Image> input_images;
        
        /// <summary>
        /// Detected keypoints that are later used for matching and blending.
        /// Each image has two lists of keypoints (left and right side) except the first and the last one (only right/left side).
        /// </summary>
        List<List<IntPoint>> keypoints;

        /// <summary>
        /// Homography matrix that defines tranformation - rotation and translation of two images.
        /// </summary>
        private MatrixH homography;

        /// <summary>
        /// Result image.
        /// </summary>
        Bitmap panorama;

        /// <summary>
        /// Defines section of the image when cropping.
        /// </summary>
        enum ImageSection { Left, Right };
        /// <summary>
        /// All phases for panorama.
        /// </summary>
        enum PanoramaPhase { InsertImages, DetectKeypoints, MatchKeypoints, Blend };

        /// <summary>
        /// Size of cropped image.
        /// </summary>
        int cropWidth;

        /// <summary>
        /// Percentage of cropped image width. User can change percent.
        /// </summary>
        int cropWidthPercent = 100;


        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            input_images = new List<Image>();
            keypoints = new List<List<IntPoint>>();
        }

        #region MENU_CLICK_HANDLERS
        /// <summary>
        /// Opens dialog for selecting input images.
        /// Pictures are then shown in the 'Input images' panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openInputImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            input_images = new List<Image>();
            keypoints = new List<List<IntPoint>>();

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                // clear all thumbnails from all panels
                ClearThumbnails();

                foreach (String file in openFileDialog1.FileNames)
                {
                    input_images.Add(Image.FromFile(file));

                    ShowThumbnail(input_images.Last(), false, PanoramaPhase.InsertImages);
                }

                cropWidth = input_images[0].Width * cropWidthPercent / 100;
                toolStripStatusLabel1.Text = "Input images loaded. Next you should calculate image keypoints.";
            }


        }

        /// <summary>
        /// Saves panorama image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePanoramaImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Images (*.jpg)|*.jpg|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                panorama.Save(saveFileDialog1.FileName);
                panorama.Dispose();
            }
        
        }

        /// <summary>
        /// Detected keypoints with method SURF.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calculateKeypointsSURFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detectKeypoints_SURF();
            DrawKeypoints();
            toolStripStatusLabel1.Text = "Keypoints calculated. Next you should merge images.";
        }

        /// <summary>
        /// Detected keypoints with method Harris.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calculateKeypointsHarrisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            detectKeypoints_Harris();
            DrawKeypoints();
            toolStripStatusLabel1.Text = "Keypoints calculated. Next you should merge keypoints.";
        }

        /// <summary>
        /// Match keypoints.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void matchKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MatchKeypoints();
            toolStripStatusLabel1.Text = "Keypoints are matched. Next you should blend images.";
        }

        /// <summary>
        /// Blend images.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void blendImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BlendImages();
            toolStripStatusLabel1.Text = "Images are blended. You now have panorama image.";
        }

        /// <summary>
        /// Creates panorama without showing intermediate steps.
        /// Possible to use with multiple images.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fastPanoramaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastPanorama();
        }

        /// <summary>
        /// Exits application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Shows about dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
        #endregion


        #region IMAGE_MANIPULATION
        /// <summary>
        /// Shows larger image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="image">Image to be enlarged.</param>
        private void ShowLargeImage(object sender, MouseEventArgs e, Image image)
        {
            // zoom current image
            ZoomPicBox pb = new ZoomPicBox();
            pb.Dock = DockStyle.Fill;
            pb.AutoScroll = true;
            pb.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            pb.Image = image;
            pb.Zoom = 0.8f;
            pb.MouseWheel += new MouseEventHandler((sender_new, e_new) => { pb.Zoom += e_new.Delta > 0 ? 0.1f : -0.1f; });

            // form to show large image
            Form imageWindow = new Form();
            imageWindow.Width = 1280;
            imageWindow.Height = 768;
            imageWindow.Controls.Add(pb);
            imageWindow.ShowDialog();
        }

        /// <summary>
        /// Shows image(s) in specific panel.
        /// </summary>
        /// <param name="image">Image to be shown.</param>
        /// <param name="isMerged">Whether image is already merged or not.</param>
        /// <param name="phase">In which phase are we showing image.</param>
        private void ShowThumbnail(Image image, bool isMerged, PanoramaPhase phase)
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

            switch (phase)
            {
                case PanoramaPhase.InsertImages:
                    flowLayoutPanel1.Controls.Add(pb);
                    break;
                case PanoramaPhase.DetectKeypoints:
                    flowLayoutPanel2.Controls.Add(pb);
                    break;
                case PanoramaPhase.MatchKeypoints:
                    flowLayoutPanel3.Controls.Add(pb);
                    break;
                case PanoramaPhase.Blend:
                    flowLayoutPanel4.Controls.Add(pb);
                    break;
                default:
                    break;
            }
            
            // create event handlers for image
            pb.MouseClick += new MouseEventHandler((sender_new, e_new) => ShowLargeImage(sender_new, e_new, image));
            pb.MouseEnter += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Hand; });
            pb.MouseLeave += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Default; });
        }

        /// <summary>
        /// Clears all image in all panels.
        /// </summary>
        private void ClearThumbnails()
        {
            List<Control> pictureBox_thumbnails = flowLayoutPanel1.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails)
            {
                flowLayoutPanel1.Controls.Remove(control);
                control.Dispose();
            }

            pictureBox_thumbnails = flowLayoutPanel2.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails)
            {
                flowLayoutPanel2.Controls.Remove(control);
                control.Dispose();
            }

            pictureBox_thumbnails = flowLayoutPanel3.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails)
            {
                flowLayoutPanel3.Controls.Remove(control);
                control.Dispose();
            }

            pictureBox_thumbnails = flowLayoutPanel4.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails)
            {
                flowLayoutPanel4.Controls.Remove(control);
                control.Dispose();
            }
        }

        /// <summary>
        /// Crops image on specific side.
        /// </summary>
        /// <param name="image">Image to be cropped.</param>
        /// <param name="side">Which side is going to be cropped.</param>
        /// <returns>Cropped bitmap.</returns>
        private Bitmap GetCroppedImage(Image image, ImageSection side)
        {
            Rectangle cropSection;
            if (side == ImageSection.Left)
                cropSection = new Rectangle(0, 0, cropWidth, image.Height);
            else
                cropSection = new Rectangle(image.Width - cropWidth, 0, cropWidth, image.Height);

            Bitmap sourceImage = new Bitmap(image);
            return sourceImage.Clone(cropSection, sourceImage.PixelFormat);
        }
        #endregion
        
        
        /// <summary>
        /// Detect keypoints in images using OpenSURF library 
        /// http://www.chrisevansdev.com/computer-vision-opensurf.html
        /// </summary>
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

        /// <summary>
        /// Detect keypoints in images using Harris corner detector from Accord.NET library
        /// http://accord-framework.net/docs/html/T_Accord_Imaging_HarrisCornersDetector.htm
        /// </summary>
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

        /// <summary>
        /// Draws detected keypoints on images in panel.
        /// </summary>
        private void DrawKeypoints()
        {
            Graphics graphics;
            Pen keypointPen = new Pen(Color.GreenYellow, 2f);
            int keypoints_cntr = 0;

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

                    graphics.DrawEllipse(keypointPen, center.X - radius, center.Y - radius, diameter, diameter);
                }
                cumulativeWidth += input_images[0].Width;
            }
            ShowThumbnail(mergedImage, true, PanoramaPhase.DetectKeypoints);
        }

        /// <summary>
        /// Matches detected keypoints.
        /// </summary>
        private void MatchKeypoints()
        {
            // matching keypoints step needs at least two images
            if (input_images.Count < 2) return;

            // using RANSAC homography estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            CorrelationMatching matcher = new CorrelationMatching(9);

            Bitmap mergedImage = new Bitmap(input_images.Count * input_images[0].Width, input_images[0].Height); //it is assumed the images are of same size!
            Graphics graphics = Graphics.FromImage(mergedImage);

            IntPoint[][] matches;

            int cumulativeWidth = 0;
            int keypoints_cntr = 0;

            // draw all images
            for (int i = 0; i < input_images.Count; i++) {
                graphics.DrawImage(input_images[i], cumulativeWidth, 0, input_images[i].Width, input_images[i].Height);
                cumulativeWidth += input_images[i].Width;
            }


            // iterate through all images
            for (int i = 0; i < input_images.Count; i++)
            {

                if (i != input_images.Count - 1)
                {
                    // match detected keypoints with maximum cross-correlation algorithm
                    matches = matcher.Match(new Bitmap(input_images[i]), new Bitmap(input_images[i + 1]), keypoints[keypoints_cntr].ToArray(), keypoints[keypoints_cntr + 1].ToArray());
              
                    IntPoint[] correlationPoints1 = matches[0];
                    IntPoint[] correlationPoints2 = matches[1];

                    // Plot RANSAC results against correlation results
                    homography = ransac.Estimate(correlationPoints1, correlationPoints2);

                    // take only inliers - good matches
                    correlationPoints1 = correlationPoints1.Submatrix(ransac.Inliers);
                    correlationPoints2 = correlationPoints2.Submatrix(ransac.Inliers);

                    PairsMarker pairs = new PairsMarker(correlationPoints1, correlationPoints2.Apply(p => new IntPoint(p.X + input_images[i].Width, p.Y)));
       
                    // draw matching pairs
                    for (int j = 0; j < pairs.Points1.Count(); j++) {
                        if (i % 2 == 0) graphics.DrawLine(new Pen(Color.GreenYellow, 1.5f), new System.Drawing.Point(correlationPoints1[j].X + i * input_images[i].Width, correlationPoints1[j].Y), new System.Drawing.Point(correlationPoints2[j].X + (i + 1) * input_images[i].Width, correlationPoints2[j].Y));
                        else            graphics.DrawLine(new Pen(Color.Red, 1.5f), new System.Drawing.Point(correlationPoints1[j].X + i * input_images[i].Width, correlationPoints1[j].Y), new System.Drawing.Point(correlationPoints2[j].X + (i + 1) * input_images[i].Width, correlationPoints2[j].Y));
                    }
                  
                    
                    keypoints_cntr += 2;
                }
            }

            ShowThumbnail(mergedImage, true, PanoramaPhase.MatchKeypoints);
            
        }

        /// <summary>
        /// Blends two images with homography matrix.
        /// </summary>
        private void BlendImages()
        {
            Blend blend = new Blend(homography, new Bitmap(input_images[0]));
            panorama = blend.Apply(new Bitmap(input_images[1]));
            ShowThumbnail(panorama, true, PanoramaPhase.Blend);
        }

        /// <summary>
        /// Creates panorama without showing intermediate steps.
        /// <para>1. Detect keypoints</para>
        /// <para>2. Match keypoints</para>
        /// <para>3. Blend images</para>
        /// Possible to use with multiple images.
        /// </summary>
        private void FastPanorama()
        {
            List<List<IntPoint>> _keypoints = new List<List<IntPoint>>();
            MatrixH _homography;

            Image _panoramaImage = input_images[0];
            int images_count = input_images.Count;

            for (int i = 1; i < images_count; i++)
            {
                _keypoints = Panorama.DetectKeypoints_SURF(_panoramaImage, input_images[i]);
                _homography = Panorama.MatchKeypoints(_panoramaImage, input_images[i], _keypoints);
                _panoramaImage = Panorama.BlendImages(_panoramaImage, input_images[i], _homography);
            }

            panorama = new Bitmap(_panoramaImage);
            ShowThumbnail(_panoramaImage, true, PanoramaPhase.Blend);
        }







    }
}
