using System.Drawing;

namespace ColorTrackerLib
{
	public class Smoother : ISmoother
	{
		private const float Alpha = 0.8f;
		private const float StayRadius = 0f;

		private readonly ExpSmoother _expSmoother = new ExpSmoother(Alpha);

		private Point _lastPoint;

		public Point SmoothPoint(Point p)
		{
			var smoothed = _expSmoother.SmoothPoint(p);

			if (_lastPoint.DistanceTo(smoothed) < StayRadius)
				return _lastPoint;

			return _lastPoint = smoothed;
		}
	}
}