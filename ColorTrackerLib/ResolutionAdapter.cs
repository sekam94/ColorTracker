using System.Drawing;

namespace ColorTrackerLib
{
	public class ResolutionAdapter
	{
		private readonly Rectangle _from;
		private readonly Rectangle _to;

		private readonly float _xRatio;
		private readonly float _yRatio;

		private readonly bool _fromZero;

		public ResolutionAdapter(Rectangle from, Rectangle to)
		{
			_from = from;
			_to = to;

			_xRatio = (float)to.Width / (float)from.Width;
			_yRatio = (float)to.Height / (float)from.Height;

			_fromZero = _from.Location == Point.Empty && _to.Location == Point.Empty;
		}

		public Point AdaptPoint(Point input)
		{
			if (_fromZero)
				return new Point
				{
					X = (int) (input.X*_xRatio),
					Y = (int) (input.Y*_yRatio)
				};

			return new Point
			{
				X = (int) (_to.X + (input.X - _from.X)*_xRatio),
				Y = (int) (_to.Y + (input.Y - _from.Y)*_yRatio)
			};
		}
	}
}