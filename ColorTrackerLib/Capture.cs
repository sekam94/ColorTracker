using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ColorTrackerLib
{
	public delegate void NewFrameEventHandler(object sender, NewFrameEventArgs e);

	public sealed class NewFrameEventArgs : EventArgs
	{
		public Bitmap Frame { get; }
		public Dictionary<MarkerSettings, List<Cluster>> Clusters { get; }
		public double Time { get; }

		internal NewFrameEventArgs(Bitmap frame, double time, Dictionary<MarkerSettings, List<Cluster>> clusters)
		{
			Frame = frame;
			Clusters = clusters;
			Time = time;
		}
	}

	public sealed class Capture : IDisposable
	{
		public bool Running => _videoThread.Running;
		public int ImageWidth => _videoThread.ImageWidth;
		public int ImageHeight => _videoThread.ImageHeight;

		public List<MarkerSettings> MarkerSettings => _scanner.MarkerSettings;
		public NewFrameEventHandler NewFrameEvent;

		private readonly Scanner _scanner = new Scanner();
		private readonly VideoThread _videoThread;
		private double _lastTime;

		public Capture(Camera camera)
		{
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

			var fingers = _scanner.LocateMarkers(frame);

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
}
