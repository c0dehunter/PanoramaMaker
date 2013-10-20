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

            //prepare containers
            input_images = new List<Image>();
        }

        private void openInputImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    input_images.Add(Image.FromFile(file));

                    Image thumb = input_images.Last().GetThumbnailImage(240,160, ()=>false, IntPtr.Zero);
                    PictureBox pb = new PictureBox();
                    pb.Height = thumb.Height;
                    pb.Width = thumb.Width;
                    pb.Image = thumb;
                    flowLayoutPanel1.Controls.Add(pb);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
