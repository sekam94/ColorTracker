using System;
using System.Drawing;

namespace ColorTrackerLib
{
	public static class PointOperations
	{
		public static double DistanceTo(this Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2));
		}
	}
}