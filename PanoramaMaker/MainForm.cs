using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using OpenSURFcs;
using Accord.Imaging;
using AForge;
using Accord.Imaging.Filters;
using System.Drawing.Imaging;

namespace PanoramaMaker
{
    public partial class MainForm : Form
    {
        List<Image> input_images;       //input images
        List<List<IntPoint>> keypoints;   //each image has two lists of keypoints (left and right side) except the first and the last one (only right/left side)

        enum ImageSection {Left, Right};
        const int cropWidthPercent = 20; //the percentage of picture's area (next to the edge) to be searched for keypoints
        int cropWidth;

        public MainForm()
        {
            InitializeComponent();

            input_images = new List<Image>();
            keypoints = new List<List<IntPoint>>();
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
            }

            cropWidth = input_images[0].Width * cropWidthPercent / 100;
            toolStripStatusLabel1.Text = "Input images loaded. Next you should calculate image keypoints.";
        }

        private void calculateKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialog_result = MessageBox.Show("Detect keypoints using SURF algorithm? (Harris corner detector will be used otherwise)", "Preferred method", MessageBoxButtons.YesNo);
            if (dialog_result == DialogResult.Yes)
                detectKeypoints_SURF();
            else 
            if (dialog_result == DialogResult.No)
                detectKeypoints_Harris();
            else
                return;

            DrawKeypoints();
            toolStripStatusLabel1.Text = "Keypoints calculated. Next you should merge images.";
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
                    List<IPoint> surf_keypoints = FastHessian.getIpoints(0.001f, 5, 2, integralImage);

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

        private void DrawKeypoints()
        {
            Graphics graphics;
            Pen keypointPen = new Pen(Color.GreenYellow, 2f);
            int keypoints_cntr = 0;

            List<Control> pictureBox_thumbnails = flowLayoutPanel1.Controls.Cast<Control>().ToList();
            foreach (Control control in pictureBox_thumbnails) //first clear the old thumbnails
            {
                flowLayoutPanel1.Controls.Remove(control);
                control.Dispose();
            }

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

                    Point center;
                    if (i == 0) //if this is first image, keypoints are on the right side
                        center = new Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.X), Convert.ToInt32(keypoint.Y));
                    else //else keypoints are on the left side
                        center = new Point(Convert.ToInt32(cumulativeWidth + keypoint.X), Convert.ToInt32(keypoint.Y));

                    graphics.DrawEllipse(keypointPen, center.X - radius, center.Y - radius, diameter, diameter);

                    /* Find the matching keypoints (last image)
                     * Currently not used (Evgen's job)
                     */
                    //if (i > 0)
                    //{
                    //    float minDistance = float.MaxValue;
                    //    float tmpDistance;
                    //    IPoint matchingKeypoint = null;
                    //    foreach (IPoint keypoint2 in keypoints[keypoints_cntr-2])
                    //    {
                    //        tmpDistance = 0;
                    //        for (int k = 0; k < keypoint.descriptorLength; k++)
                    //            tmpDistance += (keypoint.descriptor[k] - keypoint2.descriptor[k]) * (keypoint.descriptor[k] - keypoint2.descriptor[k]);
                    //        tmpDistance = (float)Math.Sqrt(tmpDistance);
                    //        if (tmpDistance < minDistance)
                    //        {
                    //            minDistance = tmpDistance;
                    //            matchingKeypoint = keypoint2;
                    //        }
                    //    }

                    //    if (minDistance < 0.2f) //euclidean distance threshold
                    //    {
                    //        Point center2 = new Point(Convert.ToInt32(cumulativeWidth - cropWidth + matchingKeypoint.x), Convert.ToInt32(matchingKeypoint.y));
                    //        graphics.DrawLine(connectionPen, center, center2);
                    //    }
                    //}
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

                    Point center;
                    if (i == input_images.Count - 1) //if this is last image, the keypoints are on the left side
                        center = new Point(Convert.ToInt32(cumulativeWidth + keypoint.X), Convert.ToInt32(keypoint.Y));
                    else //else keypoints are on the right
                        center = new Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.X), Convert.ToInt32(keypoint.Y));

                    graphics.DrawEllipse(keypointPen, center.X - radius, center.Y - radius, diameter, diameter);
                }
                cumulativeWidth += input_images[0].Width;
            }
            ShowThumbnail(mergedImage, true);
        }
    }
}
