using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ColorTrackerLib;
using ColorTrackerLib.Device;

namespace Test
{
	public partial class MainForm : Form
	{
		private const int DISTANCE_TRASHOLD = 75;

		private readonly Capture _cap = new Capture(Camera.GetDevices()[0]);
		private readonly TouchEmulator _touchEmulator = new TouchEmulator();
		private readonly Stopwatch _timer = new Stopwatch();
		private readonly ResolutionAdapter _resolutionAdapter;

		private readonly PointerData _right = new PointerData
		{
			MarkerASettings = new MarkerSettings
			{
				AverageH = 350,
				MaxDifH = 10,
				MaxS = 1,
				MinS = 0.4f
			},
			MarkerBSettings = new MarkerSettings
			{
				AverageH = 220,
				MaxDifH = 10,
				MaxS = 1,
				MinS = 0.4f
			},
			Smoother = new Smoother()
		};

		private readonly PointerData _left = new PointerData
		{
			MarkerASettings = new MarkerSettings
			{
				AverageH = 20,
				MaxDifH = 15,
				MaxS = 1,
				MinS = 0.4f
			},
			MarkerBSettings = new MarkerSettings
			{
				AverageH = 170,
				MaxDifH = 15,
				MaxS = 1,
				MinS = 0.4f
			},
			Smoother = new Smoother()
		};

		private class PointerData
		{
			public MarkerSettings MarkerASettings { get; set; }
			public MarkerSettings MarkerBSettings { get; set; }
			public ISmoother Smoother { get; set; }
		}

		public MainForm()
		{
			InitializeComponent();

			var resultRectangle = Screen.PrimaryScreen.Bounds;
			resultRectangle.Inflate(400, 200);

			_resolutionAdapter = new ResolutionAdapter(new Rectangle(0, 0, 640, 480), resultRectangle);

			Disposed += (sender, args) =>
			{
				_cap.Dispose();
				_touchEmulator.Dispose();
			};

			_cap.NewFrameEvent += NewFrame;

			var settingsControl = new SettingsControl(_cap, false);
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("RightA", _right.MarkerASettings));
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("RightB", _right.MarkerBSettings));

			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("LeftA", _left.MarkerASettings));
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("LeftB", _left.MarkerBSettings));

			settingsControl.Location = new Point(pictureBox1.Size.Width, menuStrip1.Height);
			settingsControl.Height = Height;
			Controls.Add(settingsControl);

			_timer.Start();
			//_touchEmulator.Run();
			_cap.Run();
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
			}
		}

		private void UpdateImage(NewFrameEventArgs e)
		{
			//if (!_touchEmulator.IsRunning)
			//	throw new Exception("Emulator is not working");

			_timer.Stop();

			UpdateEmulator(e.Clusters);

			DrawMarkers(e);
			pictureBox1.Image = e.Frame;

			_timer.Restart();
		}

		private void UpdateEmulator(Dictionary<MarkerSettings, List<Cluster>> markers)
		{ 
			UpdatePointer(_touchEmulator.Right,
				_right.Smoother,
				GetMarkerCluster(_right.MarkerASettings, markers),
				GetMarkerCluster(_right.MarkerBSettings, markers),
				праваяРукаToolStripMenuItem.Checked);

			UpdatePointer(_touchEmulator.Left,
				_left.Smoother,
				GetMarkerCluster(_left.MarkerASettings, markers),
				GetMarkerCluster(_left.MarkerBSettings, markers),
				леваяРукаToolStripMenuItem.Checked);

			_touchEmulator.Update();
		}

		private Cluster GetMarkerCluster(MarkerSettings settings, Dictionary<MarkerSettings, List<Cluster>> markers)
		{
			List<Cluster> clusters;
			var found = markers.TryGetValue(settings, out clusters);

			if (found)
				return FindBiggestCuster(clusters);
			return null;
		}

		private const float RectangleScale = 0.1f;

		private void UpdatePointer(TouchEmulator.Pointer pointer, ISmoother smoother, Cluster clusterA, Cluster clusterB, bool enabled)
		{ 
			lock (pointer)
				if (enabled && clusterA != null && clusterB != null)
				{
					var rectA = clusterA.Borders;
					var rectB = clusterB.Borders;

					rectA.Inflate((int)(rectA.Size.Height * RectangleScale), (int)(rectA.Size.Width * RectangleScale));
					rectB.Inflate((int)(rectB.Size.Height * RectangleScale), (int)(rectB.Size.Width * RectangleScale));

					pointer.Point = smoother.SmoothPoint(_resolutionAdapter.AdaptPoint(MiddlePoint(clusterA.Center, clusterA.Center)));
					pointer.Contact = rectA.IntersectsWith(rectB);
					pointer.Enabled = true;
				}
				else
					pointer.Enabled = false;
		}

		private Cluster FindBiggestCuster(List<Cluster> markers)
		{
			if (markers == null || markers.Count == 0)
				return null;
			return markers.Aggregate((c1, c2) => c1.Area >= c2.Area ? c1 : c2);
		}

		private Point MiddlePoint(Point p1, Point p2)
		{
			return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
		}

		private void DrawMarkers(NewFrameEventArgs e)
		{
			using (Graphics g = Graphics.FromImage(e.Frame))
			{
				foreach (var pair in e.Clusters)
				{
					var pen = new Pen(Hsv.ToColor(new Hsv(pair.Key.AverageH, 1, 1)), 1);

					foreach (var cluster in pair.Value)
					{
						g.DrawRectangle(pen, cluster.Borders);
						g.DrawEllipse(pen, new Rectangle(cluster.Center.X - 5, cluster.Center.Y - 5, 10, 10));

						//foreach (Point p in cluster.Points)
						//	g.DrawRectangle(pen, p.X, p.Y, 1, 1);
					}
				}

				g.DrawString(_timer.ElapsedMilliseconds + " ms", new Font("Arial", 36), new SolidBrush(Color.Red), 50, 50);
			}
		}

		private void GreenButton(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripMenuItem;
			if (menuItem != null)
			{
				var item = menuItem;
				item.BackColor = item.Checked ? Color.LimeGreen : SystemColors.Control;
			}
		}
	}
}
