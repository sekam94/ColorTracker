using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ColorTrackerLib;
using ColorTrackerLib.Device;

namespace Test
{
	public partial class ColorTracker : Form
	{
		private readonly Capture cap;
		private readonly SettingsControl settingsControl;

		private SettingsControl.Settings testMarker;

		private const int DISTANCE_TRASHOLD = 75;
		
		private Stopwatch timer;

		public ColorTracker()
		{
			InitializeComponent();

			cap = new Capture(Camera.GetDevices()[0]);
			settingsControl = new SettingsControl(cap, false);

			settingsControl.Location = new Point(pictureBox1.Size.Width, 0);
			settingsControl.Height = Height;
			Controls.Add(settingsControl);
			//settingsControl.Visible = true;

			FormClosed += MainForm_FormClosed;

			testMarker = new SettingsControl.Settings("Test marker");
			settingsControl.SettingsList.Add(testMarker);

			timer = new Stopwatch();
			timer.Start();

			cap.NewFrameEvent += NewFrame;
			cap.Run();
		}


		void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			cap.Stop();
		}

		delegate void NewFrameCallback(NewFrameEventArgs e);
		private void NewFrame(object sender, NewFrameEventArgs e)
		{
			try
			{
				if (InvokeRequired)
					Invoke(new NewFrameCallback(UpdateImage), e);
				else
					UpdateImage(e);
			}
			catch (ObjectDisposedException)
			{
				cap.Stop();
			}
		}

		private void UpdateImage(NewFrameEventArgs e)
		{
			timer.Stop();
			using (Graphics g = Graphics.FromImage(e.Frame))
			{
				DrawMarkerSettingsImage(e, g);
			}
			pictureBox1.Image = e.Frame;
			timer.Restart();
		}


		private void DrawMarkerSettingsImage(NewFrameEventArgs e, Graphics g)
		{
			foreach (Marker marker in e.Markers)
			{
				Rectangle rec = marker.Cluster.Borders;
				g.DrawRectangle(new Pen(Hsv.ToColor(new Hsv(marker.Settings.AverageH, 1, 1)), 2), rec);
				foreach (Point p in marker.Cluster.Points)
					g.DrawRectangle(new Pen(Hsv.ToColor(new Hsv(marker.Settings.AverageH, 1, 1)), 1),
						p.X, p.Y, 1, 1);
			}

			g.DrawString(timer.ElapsedMilliseconds + " ms", new Font("Arial", 36), new SolidBrush(Color.Red), 50, 50);
		}
	}
}
