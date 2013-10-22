using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace PanoramaMaker
{
    public partial class Form1 : Form
    {
        List<Image> input_images;

        public Form1()
        {
            InitializeComponent();

            input_images = new List<Image>();
        }

        #region MENU_CLICK_HANDLERS
        private void openInputImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                int imageIndex=0;
                foreach (String file in openFileDialog1.FileNames)
                {
                    input_images.Add(Image.FromFile(file));

                    Image thumb = input_images.Last().GetThumbnailImage(240,flowLayoutPanel1.Height-10, ()=>false, IntPtr.Zero);
                    PictureBox pb = new PictureBox();
                    pb.Height = thumb.Height;
                    pb.Width = thumb.Width;
                    pb.Image = thumb;
                    flowLayoutPanel1.Controls.Add(pb);

                    int tmpIndex = imageIndex;
                    pb.MouseClick += new MouseEventHandler((sender_new,e_new) => showLargeImage(sender_new,e_new,tmpIndex));
                    pb.MouseEnter += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Hand; });
                    pb.MouseLeave += new EventHandler((sender_new, e_new) => { Cursor = Cursors.Default; });
                    imageIndex++;
                }
            }

            toolStripStatusLabel1.Text = "Loaded input images. Next you should calculate image keypoints.";
        }

        private void calculateKeypointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Image image in input_images)
            {
                Image imageRef = image;
                calculateKeypoints(ref imageRef);
            }
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

        private void showLargeImage(object sender, MouseEventArgs e, int imageIndex)
        {
            Form imageWindow = new Form();
            imageWindow.Width = 800;
            imageWindow.Height = 600;

            PictureBox pb = new PictureBox();
            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Height = input_images[imageIndex].Height;
            pb.Width = input_images[imageIndex].Width;
            pb.Image = input_images[imageIndex];
            imageWindow.Controls.Add(pb);

            imageWindow.ShowDialog();
        }

        private void calculateKeypoints(ref Image image)
        {

        }
    }
}
