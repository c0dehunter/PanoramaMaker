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

namespace PanoramaMaker
{
    public partial class MainForm : Form
    {
        List<Image> input_images;       //input images
        List<List<IPoint>> keypoints;   //each image has two list of keypoints (left and right side) except the first and the last one (only right/left side)
        enum ImageSection {Left, Right};
        const int cropWidth = 500;

        public MainForm()
        {
            InitializeComponent();

            input_images = new List<Image>();
            keypoints = new List<List<IPoint>>();
        }

        #region MENU_CLICK_HANDLERS
        private void openInputImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            input_images = new List<Image>();
            keypoints = new List<List<IPoint>>();

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

            toolStripStatusLabel1.Text = "Input images loaded. Next you should calculate image keypoints.";
        }

        private void calculateKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            calculateKeypoints();
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
         * Calculate keypoints in images Using OpenSURF library
         * http://www.chrisevansdev.com/computer-vision-opensurf.html
         */
        private void calculateKeypoints()
        {
            IntegralImage integralImage;
            for(int i=0; i<input_images.Count; i++)
            {
                if(i != 0) //if not first image, calculate left side keypoints
                {
                    integralImage = IntegralImage.FromImage(GetCroppedImage(input_images[i], ImageSection.Left));
                    keypoints.Add(FastHessian.getIpoints(0.0002f, 5, 2, integralImage));
                    SurfDescriptor.DecribeInterestPoints(keypoints.Last(), false, false, integralImage);
                }
                if (i != input_images.Count - 1) //if not last image, calculate right side keypoints
                {
                    integralImage = IntegralImage.FromImage(GetCroppedImage(input_images[i], ImageSection.Right));
                    keypoints.Add(FastHessian.getIpoints(0.0002f, 5, 2, integralImage));
                    SurfDescriptor.DecribeInterestPoints(keypoints.Last(), false, false, integralImage);
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
            Pen redPen = new Pen(Color.Red, 2f);
            Pen bluePen = new Pen(Color.Blue, 2f);
            Pen orientationPen = new Pen(Color.GreenYellow, 2f);
            Pen connectionPen = new Pen(Color.Silver, 2f);
            Pen pen;
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

                foreach (IPoint keypoint in keypoints[keypoints_cntr++])
                {
                    int diameter = 2 * Convert.ToInt32(2.5f * keypoint.scale);
                    int radius = Convert.ToInt32(diameter / 2f);

                    Point center;
                    if(i == 0) //if this is first image, keypoints are on the right side
                        center = new Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.x), Convert.ToInt32(keypoint.y));
                    else //else keypoints are on the left side
                        center = new Point(Convert.ToInt32(cumulativeWidth + keypoint.x), Convert.ToInt32(keypoint.y));

                    Point orientation = new Point(Convert.ToInt32(radius * Math.Cos(keypoint.orientation)), Convert.ToInt32(radius * Math.Sin(keypoint.orientation)));

                    pen = keypoint.laplacian > 0 ? bluePen : redPen;

                    graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, diameter, diameter);
                    graphics.DrawLine(orientationPen, center.X, center.Y, center.X + orientation.X, center.Y + orientation.Y);

                    //find the matching keypoints (last image)
                    if (i > 0)
                    {
                        float minDistance = float.MaxValue;
                        float tmpDistance;
                        IPoint matchingKeypoint = null;
                        foreach (IPoint keypoint2 in keypoints[keypoints_cntr-2])
                        {
                            tmpDistance = 0;
                            for (int k = 0; k < keypoint.descriptorLength; k++)
                                tmpDistance += (keypoint.descriptor[k] - keypoint2.descriptor[k]) * (keypoint.descriptor[k] - keypoint2.descriptor[k]);
                            tmpDistance = (float)Math.Sqrt(tmpDistance);
                            if (tmpDistance < minDistance)
                            {
                                minDistance = tmpDistance;
                                matchingKeypoint = keypoint2;
                            }
                        }

                        if (minDistance < 0.2f) //euclidean distance threshold
                        {
                            Point center2 = new Point(Convert.ToInt32(cumulativeWidth - cropWidth + matchingKeypoint.x), Convert.ToInt32(matchingKeypoint.y));
                            graphics.DrawLine(connectionPen, center, center2);
                        }
                    }
                }
                if (i == 0 || i == input_images.Count - 1) //if this is the first image or last image it has only one side of keypoints
                {
                    cumulativeWidth += input_images[0].Width;
                    continue;
                }

                foreach (IPoint keypoint in keypoints[keypoints_cntr++])
                {
                    int diameter = 2 * Convert.ToInt32(2.5f * keypoint.scale);
                    int radius = Convert.ToInt32(diameter / 2f);

                    Point center;
                    if (i == input_images.Count -1) //if this is last image, the keypoints are on the left side
                        center = new Point(Convert.ToInt32(cumulativeWidth + keypoint.x), Convert.ToInt32(keypoint.y));
                    else //else keypoints are on the right
                        center = new Point(Convert.ToInt32(cumulativeWidth + input_images[0].Width - cropWidth + keypoint.x), Convert.ToInt32(keypoint.y));
                        
                    Point orientation = new Point(Convert.ToInt32(radius * Math.Cos(keypoint.orientation)), Convert.ToInt32(radius * Math.Sin(keypoint.orientation)));

                    pen = keypoint.laplacian > 0 ? bluePen : redPen;

                    graphics.DrawEllipse(pen, center.X - radius, center.Y - radius, diameter, diameter);
                    graphics.DrawLine(orientationPen, center.X, center.Y, center.X + orientation.X, center.Y + orientation.Y);
                }

                cumulativeWidth += input_images[0].Width;
            }

            ShowThumbnail(mergedImage, true);
        }
    }
}
