using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ColorTrackerLib.Device;

namespace ColorTrackerLib
{
	public sealed class Capture : IDisposable
	{
		public bool Running
		{
			get { return _videoThread.Running; }
		}

		public int ImageWidth
		{
			get { return _videoThread.ImageWidth; }
		}

		public int ImageHeight
		{
			get { return _videoThread.ImageHeight; }
		}

		public Scanner Scanner
		{
			get { return _scanner; }
		}

		private Scanner _scanner;

		public delegate void NewFrameEventHandler(object sender, NewFrameEventArgs e);

		public NewFrameEventHandler NewFrameEvent;

		private readonly VideoThread _videoThread;

		private double _lastTime;

		public Capture(Camera camera)
		{
			_scanner = new Scanner();
			_videoThread = new VideoThread(camera);
			_videoThread.NewFrameCallback = NewFrame;
		}

		private void NewFrame(Frame frame)
		{
#if DEBUG
			UpdateFrame(frame);
#else
			ThreadPool.QueueUserWorkItem(UpdateFrame, frame);
#endif
		}

		private void UpdateFrame(Object obj)
		{
			Frame frame = obj as Frame;

			var fingers = Scanner.LocateMarkers(frame);

			if (frame.Time > _lastTime)
			{
				_lastTime = frame.Time;

				if (NewFrameEvent != null && Running)
					NewFrameEvent(this, new NewFrameEventArgs(frame.Bitmap, frame.Time, fingers));
			}
		}

		public void Run()
		{
			_videoThread.Run();
		}

		public void Stop()
		{
			_videoThread.Stop();
		}

		public void Dispose()
		{
			_videoThread.Dispose();
		}
	}

	public sealed class NewFrameEventArgs : EventArgs
	{
		public Bitmap Frame { get; private set; }
		public Dictionary<MarkerSettings, List<Cluster>> Clusters { get; private set; }
		public double Time { get; private set; }

		internal NewFrameEventArgs(Bitmap frame, double time, Dictionary<MarkerSettings, List<Cluster>> clusters)
		{
			Frame = frame;
			Clusters = clusters;
			Time = time;
		}
	}
}
