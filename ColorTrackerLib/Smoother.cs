using System.Drawing;

namespace ColorTrackerLib
{
	public class Smoother : ISmoother
	{
		private const float Alpha = 0.8f;

		private readonly ExpSmoother _expSmoother = new ExpSmoother(Alpha);

		private Point _lastPoint;

		public Point SmoothPoint(Point p)
		{
			var smoothed = _expSmoother.SmoothPoint(p);

			return _lastPoint = smoothed;
		}
	}
}