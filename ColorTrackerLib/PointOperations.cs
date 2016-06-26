﻿using System;
using System.Drawing;

namespace ColorTrackerLib
{
	public static class Operations
	{
		public static double DistanceTo(this Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2));
		}

		public static Point MiddlePoint(this Point p1, Point p2)
		{
			return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
		}

		public static double VectorLen(this PointF p1)
		{
			return Math.Abs(p1.X * p1.X + p1.Y * p1.Y);
		}

		public static double DistanceTo(this PointF p1, PointF p2)
		{
			return Math.Sqrt(Math.Pow(Math.Abs(p1.X - p2.X), 2) + Math.Pow(Math.Abs(p1.Y - p2.Y), 2));
		}

		public static double DistanceTo(this Cluster clusterA, Cluster clusterB)
		{
			var rectA = clusterA.Borders;
			var rectB = clusterB.Borders;

			var centerA = clusterA.Center;
			var centerB = clusterB.Center;


			double angleA = AngleToOX(centerA, centerB);
			double angleB = angleA - Math.PI;

			var pointA = clusterA.ApproximateBorderPoint(angleA);
			var pointB = clusterA.ApproximateBorderPoint(angleB);

			return pointA.DistanceTo(pointB);
		}

		public static double AngleToOX(PointF p1, PointF p2)
		{
			var a = new PointF(p1.Y - p2.Y, p2.X - p1.X);
			var ox = new PointF(1, 0);

			return Math.Acos(	a.X * ox.X + a.Y * ox.Y /
								a.VectorLen() * ox.VectorLen()	);
		}

		public static PointF ApproximateBorderPoint(this Cluster cluster, double angle)
		{
			return new PointF(cluster.Center.X + (float)Math.Cos(angle) * (float)cluster.Borders.Width / 2,
								   cluster.Center.Y + (float)Math.Sin(angle) * (float)cluster.Borders.Height / 2);
		}
	}
}