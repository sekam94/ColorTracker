using System.Drawing;

namespace ColorTrackerLib
{
	public class ExpSmoother : ISmoother
	{
		private readonly float _alpha;

		private Point _lastSmoothedPoint;

		public ExpSmoother(float alpha)
		{
			_alpha = alpha;
		}

		public Point SmoothPoint(Point raw)
		{
			return _lastSmoothedPoint = new Point
			{
				X = (int)(_lastSmoothedPoint.X + _alpha * (raw.X - _lastSmoothedPoint.X)),
				Y = (int)(_lastSmoothedPoint.Y + _alpha * (raw.Y - _lastSmoothedPoint.Y))
			};
		}
	}
}