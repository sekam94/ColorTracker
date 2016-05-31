using System;
using System.Drawing;

namespace ColorTrackerLib
{
	public struct Hsv
	{
		private float _h;
		private float _s;
		private float _v;

		public float H
		{
			get
			{
				return _h;
			}
			set
			{
				if (value > 360 || value < 0)
					throw new ArgumentException();
				_h = value;
			}
		}

		public float S
		{
			get
			{
				return _s;
			}
			set
			{
				if (value < 0 || value > 1)
					throw new ArgumentException();
				_s = value;
			}
		}

		public float V
		{
			get
			{
				return _v;
			}
			set
			{
				if (value < 0 || value > 1)
					throw new ArgumentException();

				_v = value;
			}
		}

		public Hsv(float hue, float saturation, float value)
		{
			if (hue > 360 || hue < 0)
				throw new ArgumentException();
			_h = hue;

			if (saturation < 0 || saturation > 1)
				throw new ArgumentException();
			_s = saturation;

			if (value < 0 || value > 1)
				throw new ArgumentException();
			_v = value;
		}

		public static Hsv FromColor(Color color)
		{
			int max = Math.Max(color.R, Math.Max(color.G, color.B));
			int min = Math.Min(color.R, Math.Min(color.G, color.B));

			return new Hsv(color.GetHue(), 
							(max == 0) ? 0 : 1f - (1f * min / max), 
							max / 255f);
		}

		public static Color ToColor(Hsv hsv)
		{
			int hi = Convert.ToInt32(Math.Floor(hsv.H / 60)) % 6;
			double f = hsv.H / 60 - Math.Floor(hsv.H / 60);

			int v = Convert.ToInt32(hsv.V * 255);
			int p = Convert.ToInt32(v * (1 - hsv.S));
			int q = Convert.ToInt32(v * (1 - f * hsv.S));
			int t = Convert.ToInt32(v * (1 - (1 - f) * hsv.S));

			switch (hi)
			{
				case 0:
					return Color.FromArgb(255, v, t, p);
				case 1:
					return Color.FromArgb(255, q, v, p);
				case 2:
					return Color.FromArgb(255, p, v, t);
				case 3:
					return Color.FromArgb(255, p, q, v);
				case 4:
					return Color.FromArgb(255, t, p, v);
				default:
					return Color.FromArgb(255, v, p, q);
			}
		}

		public override string ToString() { return String.Format("Hsv ({0}, {1}, {2}) - {3}", H, S, V, ToColor(this)); }
	}
}
