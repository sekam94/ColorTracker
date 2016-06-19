using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ColorTrackerLib
{
	public class TouchEmulator : IDisposable
	{
		private readonly PointerState _left = new PointerState(new Pointer(), 0);
		private readonly PointerState _right = new PointerState(new Pointer(), 1);
		private readonly object _lock = new object();

		private bool _needUpdate;

		public Pointer Left => _left.Pointer;
		public Pointer Right => _right.Pointer;

		public bool Disposed { get; private set; }
		public bool InUpdate { get; private set; }

		static TouchEmulator()
		{
			TouchInjector.InitializeTouchInjection(256, TouchFeedback.INDIRECT);
		}

		public void Update()
		{
			if (Disposed)
				throw new ObjectDisposedException("TouchEmulator");

			try
			{
				UpdateContacts();
			}
			catch (Win32Exception e)
			{
				throw new Exception($"Coldn't update contacts: {e.Message}");
			}
		}

		private void UpdateContacts()
		{
			lock (_lock)
			{
				InUpdate = true;
				_needUpdate = true;
				while (_needUpdate)
				{
					_needUpdate = false;

					PointerTouchInfo[] pointers = new PointerTouchInfo[2];

					int num = 0;

					if (_left.Pointer.Enabled || _left.Injected)
					{
						pointers[num++] = GetPointerTouchInfo(_left);
						Trace.WriteLine($"Left: {pointers[num - 1].PointerInfo.PointerFlags}");
					}

					if (_right.Pointer.Enabled || _right.Injected)
					{
						pointers[num++] = GetPointerTouchInfo(_right);
						Trace.WriteLine($"Right: {pointers[num - 1].PointerInfo.PointerFlags}");
					}

					if (num > 0)
						TouchInject(num, pointers);
				}

				InUpdate = false;
			}
		}

		private PointerTouchInfo GetPointerTouchInfo(PointerState state)
		{
			if (state.Pointer.Enabled)
				state.Injected = true;
			else if (state.IsInContact)
				state.Pointer.InContact = false;
			else
				state.Injected = false;

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

			var flags = pti.PointerInfo.PointerFlags;

			if (state.Pointer.InContact != state.IsInContact)
				state.IsInContact = state.Pointer.InContact;

			if (flags == (PointerFlags.INRANGE | PointerFlags.UP))
			{
				pti.PointerInfo.PtPixelLocation = new TouchPoint
				{
					X = state.LastPoint.X,
					Y = state.LastPoint.Y
				};
				_needUpdate = true;
			}
			else
				state.LastPoint = state.Pointer.Point;

			return pti;
		}

		private PointerFlags CountPointerFlags(PointerState state)
		{
			PointerFlags flags = PointerFlags.INRANGE;

			if (state.Pointer.InContact != state.IsInContact)
				flags |= state.Pointer.InContact ? PointerFlags.DOWN : PointerFlags.UP;
			else
				flags |= PointerFlags.UPDATE;

			if (state.Pointer.InContact)
				flags |= PointerFlags.INCONTACT;

			if (!state.Injected)
				flags = PointerFlags.UPDATE;

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

						value = new Point(x, y);
					}
					_point = value;
				}
			}

			public bool InContact { get; set; }
			public bool Enabled { get; set; }

			public Pointer()
			{
				Point = Screen.PrimaryScreen.Bounds.Location;
			}
		}

		public void Dispose()
		{
			Disposed = true;

			Left.Enabled = false;
			Right.Enabled = false;

			UpdateContacts();
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