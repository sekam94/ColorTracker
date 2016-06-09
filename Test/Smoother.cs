using System.CodeDom;
using System.Drawing;
using ColorTrackerLib;

namespace Test
{
	public class Smoother : ISmoother
	{
		private const float Alpha = 0.8f;
		private const float StayRadius = 20f;

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