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

		public class Pointer
		{
			public MarkerSettings MarkerASettings { get; }
			public MarkerSettings MarkerBSettings { get; }
			public bool Enabled { get; set; }

			internal TouchEmulator.Pointer EmulatorPointer { get; }
			internal bool IsMoving { get; set; }
			internal ISmoother Smoother { get; } = new Smoother();

			internal Pointer(MarkerSettings markerASettings, MarkerSettings markerBSettings, TouchEmulator.Pointer emulatorPointer)
			{
				MarkerASettings = markerASettings;
				MarkerBSettings = markerBSettings;
				EmulatorPointer = emulatorPointer;
			}
		}

		public Pointer RightPointer { get; }
		public Pointer LeftPointer { get; }

		public NewFrameEventHandler NewFrameEvent;
		public List<MarkerSettings> MarkerSettings => _cap.MarkerSettings;
		public bool Running => _cap.Running;

		public GestureTracker()
		{
			_cap.NewFrameEvent += NewFrame;

			RightPointer = new Pointer
			(
			new MarkerSettings
			{
				AverageH = 350,
				MaxDifH = 10,
				MaxS = 1,
				MinS = 0.4f
			},
			new MarkerSettings
			{
				AverageH = 220,
				MaxDifH = 10,
				MaxS = 1,
				MinS = 0.4f
			},
			_touchEmulator.Right
			);

			LeftPointer = new Pointer
			(
			new MarkerSettings
			{
				AverageH = 20,
				MaxDifH = 15,
				MaxS = 1,
				MinS = 0.4f
			},
			new MarkerSettings
			{
				AverageH = 170,
				MaxDifH = 15,
				MaxS = 1,
				MinS = 0.4f
			},
			_touchEmulator.Left
			);
		}

		private void NewFrame(object sender, NewFrameEventArgs e)
		{
			UpdatePointer(RightPointer, e.Clusters);
			UpdatePointer(LeftPointer, e.Clusters);

			_touchEmulator.Update();

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

		private const double MovementTreshold = 50d;
		private void UpdatePointer(Pointer pointer, Dictionary<MarkerSettings, List<Cluster>> markers)
		{
			var clusterA = GetMarkerCluster(pointer.MarkerASettings, markers);
			var clusterB = GetMarkerCluster(pointer.MarkerBSettings, markers);

			lock (pointer.EmulatorPointer)
				if (pointer.Enabled && clusterA != null && clusterB != null)
				{
					var newPoint = pointer.Smoother.SmoothPoint(_resolutionAdapter.AdaptPoint(clusterA.CenterOfMass.MiddlePoint(clusterB.CenterOfMass)));
					var newInContact = ClustersAreClose(clusterA, clusterB);
					var oldInContact = pointer.EmulatorPointer.InContact;

					if (!pointer.IsMoving &&
						(!oldInContact || pointer.EmulatorPointer.Point.DistanceTo(newPoint) > MovementTreshold))
						pointer.IsMoving = true;

					if (pointer.IsMoving)
						pointer.EmulatorPointer.Point = newPoint;

					if (!oldInContact && newInContact)
						pointer.IsMoving = false;

					pointer.EmulatorPointer.InContact = newInContact;
					pointer.EmulatorPointer.Enabled = true;
				}
				else
				{
					pointer.IsMoving = false;
					pointer.EmulatorPointer.Enabled = false;
				}
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

		//private const double RectangleScale = 0.1d;
		//private bool ClustersAreClose(Cluster clusterA, Cluster clusterB)
		//{
		//	var rectA = clusterA.Borders;
		//	var rectB = clusterB.Borders;

		//	rectA.Inflate((int)(rectA.Size.Height * RectangleScale), (int)(rectA.Size.Width * RectangleScale));
		//	rectB.Inflate((int)(rectB.Size.Height * RectangleScale), (int)(rectB.Size.Width * RectangleScale));

		//	return rectA.IntersectsWith(rectB);
		//}

		private bool ClustersAreClose(Cluster clusterA, Cluster clusterB)
		{
			var centerA = clusterA.Center;
			var centerB = clusterB.Center;

			var angle = Operations.AngleToOX(centerA, centerB);

			var pointA = clusterA.ApproximateBorderPoint(angle);
			var pointB = clusterA.ApproximateBorderPoint(angle);

			var minLen = Math.Min(pointA.DistanceTo(centerA), pointB.DistanceTo(centerB));

			return clusterA.DistanceTo(clusterB) < minLen;
		}

		//private bool ClustersAreClose(Cluster clusterA, Cluster clusterB)
		//{
		//	double distance = ((double)clusterA.Area + clusterB.Area) / 2 * 0.1;
		//	return clusterA.CenterOfMass.DistanceTo(clusterB.CenterOfMass) < distance;
		//}
	}
}