namespace ColorTrackerGui
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.леваяРукаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.праваяРукаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(0, 28);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(853, 591);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.леваяРукаToolStripMenuItem,
            this.праваяРукаToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1152, 28);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// леваяРукаToolStripMenuItem
			// 
			this.леваяРукаToolStripMenuItem.CheckOnClick = true;
			this.леваяРукаToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.леваяРукаToolStripMenuItem.Name = "леваяРукаToolStripMenuItem";
			this.леваяРукаToolStripMenuItem.Size = new System.Drawing.Size(98, 24);
			this.леваяРукаToolStripMenuItem.Text = "Левая рука";
			this.леваяРукаToolStripMenuItem.Click += new System.EventHandler(this.ChangePointerState);
			// 
			// праваяРукаToolStripMenuItem
			// 
			this.праваяРукаToolStripMenuItem.CheckOnClick = true;
			this.праваяРукаToolStripMenuItem.Name = "праваяРукаToolStripMenuItem";
			this.праваяРукаToolStripMenuItem.Size = new System.Drawing.Size(108, 24);
			this.праваяРукаToolStripMenuItem.Text = "Правая рука";
			this.праваяРукаToolStripMenuItem.Click += new System.EventHandler(this.ChangePointerState);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(1152, 661);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(13327, 12297);
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ColorTracker";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem леваяРукаToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem праваяРукаToolStripMenuItem;
	}
}