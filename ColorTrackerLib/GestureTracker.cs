using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ColorTrackerLib
{
	public class GestureTracker : IDisposable
	{
		private readonly Capture _cap = new Capture(Camera.GetDevices()[0]);
		private readonly TouchEmulator _touchEmulator = new TouchEmulator();
		private readonly ResolutionAdapter _resolutionAdapter = new ResolutionAdapter(new Rectangle(0, 0, 640, 480), Rectangle.Inflate(Screen.PrimaryScreen.Bounds, 400, 200));

		private readonly ISmoother _rightSmoother = new Smoother();
		private readonly ISmoother _leftSmoother = new Smoother();

		public class PointerData
		{
			public MarkerSettings MarkerASettings { get; set; }
			public MarkerSettings MarkerBSettings { get; set; }
			public bool Enabled { get; set; }
		}

		public PointerData RightPointer { get; } = new PointerData
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
			}
		};

		public PointerData LeftPointer { get; } = new PointerData
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
			}
		};

		public NewFrameEventHandler NewFrameEvent;
		public List<MarkerSettings> MarkerSettings => _cap.MarkerSettings;
		public bool Running => _cap.Running;

		public GestureTracker()
		{
			_cap.NewFrameEvent += NewFrame;
		}

		private void NewFrame(object sender, NewFrameEventArgs e)
		{
			UpdateEmulator(e.Clusters);

			if (NewFrameEvent != null && Running)
				NewFrameEvent(this, e);
		}

		public void Run()
		{
			_cap.Run();
		}

		public void Stop()
		{
			_cap.Stop();
		}

		public void Dispose()
		{
			_cap.Dispose();
			_touchEmulator.Dispose();
		}

		private void UpdateEmulator(Dictionary<MarkerSettings, List<Cluster>> markers)
		{
			UpdatePointer(_touchEmulator.Right,
				_rightSmoother,
				GetMarkerCluster(RightPointer.MarkerASettings, markers),
				GetMarkerCluster(RightPointer.MarkerBSettings, markers),
				RightPointer.Enabled);

			UpdatePointer(_touchEmulator.Left,
				_leftSmoother,
				GetMarkerCluster(LeftPointer.MarkerASettings, markers),
				GetMarkerCluster(LeftPointer.MarkerBSettings, markers),
				LeftPointer.Enabled);

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

		private Cluster FindBiggestCuster(List<Cluster> markers)
		{
			if (markers == null || markers.Count == 0)
				return null;
			return markers.Aggregate((c1, c2) => c1.Area >= c2.Area ? c1 : c2);
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

					//pointer.Point = smoother.SmoothPoint(_resolutionAdapter.AdaptPoint(MiddlePoint(clusterA.Center, clusterA.Center)));
					pointer.Point = smoother.SmoothPoint(_resolutionAdapter.AdaptPoint(clusterA.Center));
					pointer.Contact = rectA.IntersectsWith(rectB);
					pointer.Enabled = true;
				}
				else
					pointer.Enabled = false;
		}
	}
}