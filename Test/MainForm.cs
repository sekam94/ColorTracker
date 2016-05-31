using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ColorTrackerLib;
using ColorTrackerLib.Device;

namespace Test
{
	public partial class MainForm : Form
	{
		private readonly Capture cap;
		private readonly SettingsControl settingsControl;
		private readonly Drawing drawing;

		private SettingsControl.Settings index;
		private SettingsControl.Settings thumb;

		private const int DISTANCE_TRASHOLD = 75;

        private Point? lastDrawPoint;

		private Stopwatch timer;

		public MainForm()
		{
			InitializeComponent();

			cap = new Capture(Camera.GetDevices()[0]);
			settingsControl = new SettingsControl(cap, false);
			drawing = new Drawing(640,480);

			settingsControl.Location = new Point(pictureBox1.Size.Width, menuStrip1.Height);
			settingsControl.Height = Height - menuStrip1.Height;
			Controls.Add(settingsControl);
			settingsControl.Visible = false;
			настройкиToolStripMenuItem.CheckedChanged += настройкиToolStripMenuItem_CheckedChanged;

			FormClosed += MainForm_FormClosed;

			index = new SettingsControl.Settings("Index");
			settingsControl.SettingsList.Add(index);
			thumb = new SettingsControl.Settings("Thumb");
			settingsControl.SettingsList.Add(thumb);

			timer = new Stopwatch();
			timer.Start();

			cap.NewFrameEvent += NewFrame;
			cap.Run();
		}

		void настройкиToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			if (настройкиToolStripMenuItem.Checked)
			{
				settingsControl.Visible = true;
			}
			else
			{
				settingsControl.Visible = false;
			}
		}

		private void очиститьToolStripMenuItem_Click(object sender, EventArgs e)
		{
			drawing.Clear();
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
				if (настройкиToolStripMenuItem.Checked)
					DrawMarkerSettingsImage(e, g);
				else
					Drawing(e, g);
			}
			pictureBox1.Image = e.Frame;
			timer.Restart();
		}

		private void Drawing(NewFrameEventArgs e, Graphics g)
		{
			g.DrawImage(drawing.Picture, 0, 0);

			var indexMarker = FindLargestMarker(e.Markers, index.Marker);
			var thumbMarker = FindLargestMarker(e.Markers, thumb.Marker);

            if (indexMarker != null && thumbMarker != null)
            {
	            var indexCluster = indexMarker.Cluster;
	            var thumbCluster = thumbMarker.Cluster;

                double distance =
                    Math.Sqrt((indexCluster.Center.X - thumbCluster.Center.X) * (indexCluster.Center.X - thumbCluster.Center.X) +
                    (indexCluster.Center.Y - thumbCluster.Center.Y) * (indexCluster.Center.Y - thumbCluster.Center.Y));

                Point center = new Point((indexCluster.Center.X + thumbCluster.Center.X) / 2, (indexCluster.Center.Y + thumbCluster.Center.Y) / 2);

                if (distance < DISTANCE_TRASHOLD)
                {
                    if (lastDrawPoint.HasValue)
                        drawing.DrawLine(center, lastDrawPoint.Value, 3, Color.DeepSkyBlue);
                    else
                        drawing.DrawCircle(center, 3, Color.DeepSkyBlue);
                    lastDrawPoint = center;
                }
                else
                    lastDrawPoint = null;
                DrawMarkersLine(indexMarker, thumbMarker, e, g);

                
            }
            else
                lastDrawPoint = null;
			
			if (index != null)
				DrawMarker(indexMarker, e, g);
			if (thumb != null)
				DrawMarker(thumbMarker, e, g);
		}

		private void DrawMarkersLine(Marker marker1, Marker marker2, NewFrameEventArgs e, Graphics g)
		{
			float averageH = Math.Max(marker1.Settings.AverageH, marker2.Settings.AverageH) -
			               Math.Min(marker1.Settings.AverageH, marker2.Settings.AverageH);
			Color color = Hsv.ToColor(new Hsv(averageH, 1, 1));
			g.DrawLine(new Pen(color,2), marker1.Cluster.Center, marker2.Cluster.Center);
		}

		private void DrawMarker(Marker marker, NewFrameEventArgs e, Graphics g)
		{
			if (marker != null)
				g.DrawEllipse(new Pen(Hsv.ToColor(new Hsv(marker.Settings.AverageH, 1, 1)), 2),
					marker.Cluster.Center.X - 5, marker.Cluster.Center.Y - 5, 10, 10);
		}

		private Marker FindLargestMarker(ICollection<Marker> list, MarkerSettings settings)
		{
			Marker largest = null;

			foreach (Marker marker in list)
			{
				if (marker.Settings == settings)
				{
					if (largest == null)
						largest = marker;
					if (marker.Cluster.Area > largest.Cluster.Area)
						largest = marker;
				}
			}
			return largest;
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
