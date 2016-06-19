using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ColorTrackerLib;

namespace ColorTrackerGui
{
	public partial class MainForm : Form
	{
		private readonly Stopwatch _timer = new Stopwatch();
		private readonly GestureTracker _gestureTracker = new GestureTracker();

		public MainForm()
		{
			InitializeComponent();

			Disposed += (sender, args) =>
			{
				_gestureTracker.Dispose();
			};

			var settingsControl = new SettingsControl(_gestureTracker.MarkerSettings, false);
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("RightA", _gestureTracker.RightPointer.MarkerASettings));
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("RightB", _gestureTracker.RightPointer.MarkerBSettings));

			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("LeftA", _gestureTracker.LeftPointer.MarkerASettings));
			settingsControl.SettingsList.Add(new SettingsControl.SettingsWrapper("LeftB", _gestureTracker.LeftPointer.MarkerBSettings));

			settingsControl.Location = new Point(pictureBox1.Size.Width, menuStrip1.Height);
			settingsControl.Height = Height;
			Controls.Add(settingsControl);

			_gestureTracker.NewFrameEvent += NewFrame;

			_timer.Start();
			_gestureTracker.Run();
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
			_timer.Stop();

			DrawMarkers(e);
			pictureBox1.Image = e.Frame;

			_timer.Restart();
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

		private void ChangePointerState(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripMenuItem;
			if (menuItem != null)
			{
				var item = menuItem;
				var enabled = item.Checked;
				item.BackColor = enabled ? Color.LimeGreen : SystemColors.Control;

				if (menuItem == праваяРукаToolStripMenuItem)
					_gestureTracker.RightPointer.Enabled = enabled;
				else if (menuItem == праваяРукаToolStripMenuItem)
					_gestureTracker.LeftPointer.Enabled = enabled;
			}
		}
	}
}
