using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ColorTrackerLib
{
	public class TouchEmulator : IDisposable
	{
		private static bool _initialized;
		private Thread _emulationThread;
		private bool _stop;

		private readonly PointerState _left = new PointerState(new Pointer(), 0);
		private readonly PointerState _right = new PointerState(new Pointer(), 1);

		public bool IsRunning { get; private set; }

		public Pointer Left => _left.Pointer;
		public Pointer Right => _right.Pointer;

		private void Initialize()
		{
			if (!_initialized)
			{
				TouchInjector.InitializeTouchInjection(256, TouchFeedback.INDIRECT);
				_initialized = true;
			}
		}

		public TouchEmulator()
		{
			Initialize();
		}

		public void Run()
		{
			if (IsRunning)
				throw new Exception("Already running");

			_emulationThread = new Thread(EmulationCycle);
			_emulationThread.Start();
		}

		public void Stop()
		{
			if (!IsRunning)
				throw new Exception("Not running");

			_stop = false;
		}

		private void EmulationCycle()
		{
			IsRunning = true;
			_stop = false;
			while (IsRunning)
			{
				try
				{
					if (_stop)
					{
						Left.Enabled = false;
						Right.Enabled = false;
						IsRunning = false;
					}

					UpdateContacts();
					Thread.Sleep(10);
				}
				catch (Exception e)
				{
					IsRunning = false;
				}
			}
		}

		private void UpdateContacts()
		{
			PointerTouchInfo[] pointers = new PointerTouchInfo[2];

			int num = 0;

			if (_left.Pointer.Enabled || _left.Injected)
			{
				pointers[num++] = GetPointerTouchInfo(_left);
				Trace.WriteLine($"Left: {pointers[num-1].PointerInfo.PointerFlags}");
			}

			if (_right.Pointer.Enabled || _right.Injected)
			{
				pointers[num++] = GetPointerTouchInfo(_right);
				Trace.WriteLine($"Right: {pointers[num - 1].PointerInfo.PointerFlags}");
			}

			if (num > 0)
				TouchInject(num, pointers);
		}

		private PointerTouchInfo GetPointerTouchInfo(PointerState state)
		{
			lock ((object) state.Pointer.Point)
			{
				PointerTouchInfo pti = new PointerTouchInfo
				{
					PointerInfo = 
					{
						PointerId = state.Id,
						PointerType = PointerInputType.TOUCH,
						PointerFlags = CountPointerFlags(state),
						PtPixelLocation =
						{
							X = state.Pointer.Point.X,
							Y = state.Pointer.Point.Y
						}
					}
				};

				if (pti.PointerInfo.PointerFlags == (PointerFlags.INRANGE | PointerFlags.UP))
					pti.PointerInfo.PtPixelLocation = new TouchPoint
					{
						X = state.LastPoint.X,
						Y = state.LastPoint.Y
					};
				else
					state.LastPoint = state.Pointer.Point;

				return pti;
			}
		}

		private PointerFlags CountPointerFlags(PointerState state)
		{
			if (state.Pointer.Enabled)
				state.Injected = true;
			else
			{
				if (state.IsInContact)
					state.Pointer.Contact = false;
				else
				{
					state.Injected = false;
					return PointerFlags.UPDATE;
				}
			}

			PointerFlags flags = PointerFlags.INRANGE;

			if (state.Pointer.Contact)
				flags |= PointerFlags.INCONTACT;

			if (state.Pointer.Contact != state.IsInContact)
			{
				flags |= state.Pointer.Contact ? PointerFlags.DOWN : PointerFlags.UP;
				state.IsInContact = state.Pointer.Contact;
			}
			else
				flags |= PointerFlags.UPDATE;
			
			return flags;
		}

		public class Pointer
		{
			private static Rectangle Bounds => Screen.PrimaryScreen.Bounds;

			private Point _point;

			public Point Point
			{
				get { return _point; }
				set
				{
					if (!Bounds.Contains(value))
					{
						int x = value.X;
						int y = value.Y;

						if (x < 0)
							x = 0;
						if (x >= Bounds.Width)
							x = Bounds.Width - 1;
						if (y < 0)
							y = 0;
						if (y >= Bounds.Height)
							y = Bounds.Height - 1;

						value = new Point(x,y);
					}
					lock ((object) _point)
						_point = value;
				}
			}

			public bool Contact { get; set; }
			public bool Enabled { get; set; }

			public Pointer()
			{
				Point = Screen.PrimaryScreen.Bounds.Location;
			}
		}
		
		public void Dispose()
		{
			_stop = true;
			while (IsRunning)
				Thread.Sleep(10);
		}

		private void TouchInject(int length, PointerTouchInfo[] pointers)
		{
			if (!TouchInjector.InjectTouchInput(length, pointers))
			{
				int error = Marshal.GetLastWin32Error();
				if (error != 0)
					throw new Win32Exception(error);
			}
		}

	private class PointerState
	{
		public Pointer Pointer { get; }
		public Point LastPoint { get; set; }
		public bool Injected { get; set; }
		public bool IsInContact { get; set; }
		public uint Id { get; }

		public PointerState(Pointer p, uint id)
		{
			Pointer = p;
			Id = id;
		}
	}
}
}