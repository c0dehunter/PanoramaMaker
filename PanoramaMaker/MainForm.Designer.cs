namespace PanoramaMaker
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInputImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePanoramaAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panoramaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateKeypointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sURFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.harrisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.matchKeypointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blendImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.panoramaToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(946, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInputImagesToolStripMenuItem,
            this.savePanoramaAsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openInputImagesToolStripMenuItem
            // 
            this.openInputImagesToolStripMenuItem.Name = "openInputImagesToolStripMenuItem";
            this.openInputImagesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openInputImagesToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.openInputImagesToolStripMenuItem.Text = "Load input images";
            this.openInputImagesToolStripMenuItem.Click += new System.EventHandler(this.openInputImagesToolStripMenuItem_Click);
            // 
            // savePanoramaAsToolStripMenuItem
            // 
            this.savePanoramaAsToolStripMenuItem.Name = "savePanoramaAsToolStripMenuItem";
            this.savePanoramaAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.savePanoramaAsToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.savePanoramaAsToolStripMenuItem.Text = "Save panorama";
            this.savePanoramaAsToolStripMenuItem.Click += new System.EventHandler(this.savePanoramaImageToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // panoramaToolStripMenuItem
            // 
            this.panoramaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calculateKeypointsToolStripMenuItem,
            this.matchKeypointsToolStripMenuItem,
            this.blendImagesToolStripMenuItem});
            this.panoramaToolStripMenuItem.Name = "panoramaToolStripMenuItem";
            this.panoramaToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.panoramaToolStripMenuItem.Text = "Panorama";
            // 
            // calculateKeypointsToolStripMenuItem
            // 
            this.calculateKeypointsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sURFToolStripMenuItem,
            this.harrisToolStripMenuItem});
            this.calculateKeypointsToolStripMenuItem.Name = "calculateKeypointsToolStripMenuItem";
            this.calculateKeypointsToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.calculateKeypointsToolStripMenuItem.Text = "Detect keypoints";
            // 
            // sURFToolStripMenuItem
            // 
            this.sURFToolStripMenuItem.Name = "sURFToolStripMenuItem";
            this.sURFToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.sURFToolStripMenuItem.Text = "SURF";
            this.sURFToolStripMenuItem.Click += new System.EventHandler(this.calculateKeypointsSURFToolStripMenuItem_Click);
            // 
            // harrisToolStripMenuItem
            // 
            this.harrisToolStripMenuItem.Name = "harrisToolStripMenuItem";
            this.harrisToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.harrisToolStripMenuItem.Text = "Harris";
            this.harrisToolStripMenuItem.Click += new System.EventHandler(this.calculateKeypointsHarrisToolStripMenuItem_Click);
            // 
            // matchKeypointsToolStripMenuItem
            // 
            this.matchKeypointsToolStripMenuItem.Name = "matchKeypointsToolStripMenuItem";
            this.matchKeypointsToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.matchKeypointsToolStripMenuItem.Text = "Match keypoints";
            this.matchKeypointsToolStripMenuItem.Click += new System.EventHandler(this.matchKeypointsToolStripMenuItem_Click);
            // 
            // blendImagesToolStripMenuItem
            // 
            this.blendImagesToolStripMenuItem.Name = "blendImagesToolStripMenuItem";
            this.blendImagesToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.blendImagesToolStripMenuItem.Text = "Blend images";
            this.blendImagesToolStripMenuItem.Click += new System.EventHandler(this.blendImagesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Images (*.JPG)|*.JPG";
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Image loader";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.LavenderBlush;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 15);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(924, 153);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.flowLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(9, 38);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(928, 170);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Input images";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 566);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(946, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(946, 588);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(484, 397);
            this.Name = "MainForm";
            this.Text = "PanoramaMaker";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInputImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePanoramaAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem calculateKeypointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem matchKeypointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem panoramaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sURFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem harrisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blendImagesToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

