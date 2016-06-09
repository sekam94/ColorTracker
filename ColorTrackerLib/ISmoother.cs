using System.Drawing;

namespace ColorTrackerLib
{
	public interface ISmoother
	{
		Point SmoothPoint(Point p);
	}
}