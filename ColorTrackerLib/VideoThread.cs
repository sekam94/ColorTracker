using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ColorTrackerLib.Device;
using DirectShowLib;

namespace ColorTrackerLib
{
	internal sealed class VideoThread : IDisposable, ISampleGrabberCB
	{
		public Camera Device { get; private set; }
		public bool Running { get; private set; }
		public int ImageWidth { get; private set; }
		public int ImageHeight { get; private set; }

		public delegate void NewFrameCallBack(Frame frame);
		public NewFrameCallBack NewFrameCallback;

		private IFilterGraph2 _filterGraph;
		private IBaseFilter _capFilter;
		private int _hr;

		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

		public VideoThread(Camera camera)
		{
			Device = camera;
			SetupGraph();
		}

		public void Run()
		{
			if (_filterGraph != null)
			{
				IMediaControl mediaCtrl = _filterGraph as IMediaControl;
				_hr = mediaCtrl.Run();
				DsError.ThrowExceptionForHR(_hr);

				Running = true;
			}
		}

		public void Stop()
		{
			if (_filterGraph != null)
			{
				IMediaControl mediaCtrl = _filterGraph as IMediaControl;
				if (mediaCtrl == null)
					throw new NullReferenceException();
				_hr = mediaCtrl.Stop();
				DsError.ThrowExceptionForHR(_hr);

				Running = false;
			}
		}

		private void SetupGraph()
		{
			ISampleGrabber sampleGrabber = null;
			IPin capOut = null;
			IPin sampleIn = null;

			_filterGraph = new FilterGraph() as IFilterGraph2;

			try
			{
				// Create camera filter
				_hr = _filterGraph.AddSourceFilterForMoniker(Device.Moniker, null, Device.Name, out _capFilter);
				DsError.ThrowExceptionForHR(_hr);
				capOut = DsFindPin.ByDirection(_capFilter, PinDirection.Output, 0);

				// Create SampleGrabber
				sampleGrabber = new SampleGrabber() as ISampleGrabber;

				ConfigureSampleGrabber(sampleGrabber);
				sampleIn = DsFindPin.ByDirection(sampleGrabber as IBaseFilter, PinDirection.Input, 0);
				_hr = _filterGraph.AddFilter(sampleGrabber as IBaseFilter, "SampleGrabber");
				DsError.ThrowExceptionForHR(_hr);

				// Connect them
				_hr = _filterGraph.Connect(capOut, sampleIn);
				DsError.ThrowExceptionForHR(_hr);

				// Learn the video properties
				SaveSizeInfo(sampleGrabber);
			}
			finally
			{
				if (sampleGrabber != null)
					Marshal.ReleaseComObject(sampleGrabber);
				if (capOut != null)
					Marshal.ReleaseComObject(capOut);
				if (sampleIn != null)
					Marshal.ReleaseComObject(sampleIn);
			}
		}

		private void ConfigureSampleGrabber(ISampleGrabber sampleGrabber)
		{
			AMMediaType media = new AMMediaType
			{
				majorType = MediaType.Video,
				subType = MediaSubType.RGB24,
				formatType = FormatType.VideoInfo
			};

			// Set the media type to Video/RBG24
			_hr = sampleGrabber.SetMediaType(media);
			DsError.ThrowExceptionForHR(_hr);

			DsUtils.FreeAMMediaType(media);

			// Configure the samplegrabber
			_hr = sampleGrabber.SetCallback(this, 1);
			DsError.ThrowExceptionForHR(_hr);
		}

		private void SaveSizeInfo(ISampleGrabber sampGrabber)
		{
			// Get the media type from the SampleGrabber
			AMMediaType media = new AMMediaType();

			_hr = sampGrabber.GetConnectedMediaType(media);
			DsError.ThrowExceptionForHR(_hr);

			if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
			{
				throw new NotSupportedException("Unknown Grabber Media Format");
			}

			// Grab the size info
			VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
			ImageWidth = videoInfoHeader.BmiHeader.Width;
			ImageHeight = videoInfoHeader.BmiHeader.Height;
			//ImageStride = ImageWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

			DsUtils.FreeAMMediaType(media);
		}

		public void Dispose()
		{
			if (_filterGraph != null)
				try
				{
					Stop();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				finally
				{
					Marshal.ReleaseComObject(_filterGraph);
					_filterGraph = null;
				}
			if (_capFilter != null)
			{
				Marshal.ReleaseComObject(_capFilter);
				_capFilter = null;
			}
		}

		int ISampleGrabberCB.SampleCB(double sampleTime, IMediaSample sample)
		{
			Marshal.ReleaseComObject(sample);
			return 0;
		}

		int ISampleGrabberCB.BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
		{
			if (NewFrameCallback != null)
			{
				NewFrameCallBack buf = NewFrameCallback;

				Bitmap bitmap = new Bitmap(ImageWidth, ImageHeight, PixelFormat.Format24bppRgb);
				BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
				CopyMemory(bmData.Scan0, buffer, (uint)bufferLen);
				bitmap.UnlockBits(bmData);

				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipXY);

				buf(new Frame(bitmap, sampleTime));
			}

			return 0;
		}

		~VideoThread()
		{
			Dispose();
		}
    }
}
