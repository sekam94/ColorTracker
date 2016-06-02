using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace ColorTrackerLib
{
	internal class Dbscan
	{
		class DbScanPoint
		{
			public Point Point { get; private set; }
			public bool Scanned { get; set; }
			public bool InCluster { get; set; }

			public int Id { get; private set; }
			public int[] Distances { get; set; }

			public DbScanPoint(Point point, int id)
			{
				Point = point;
				InCluster = false;
				Scanned = false;
			}
		}

		private static void CountDistances(List<DbScanPoint> list)
		{
			foreach (var point in list)
				point.Distances = new int[list.Count];

			Parallel.ForEach(list, point1 =>
			{
				foreach (var point2 in list)
				{
					if (point1.Distances[point2.Id] == 0)
						continue;

					int a = point2.Point.X - point1.Point.X;
					int b = point2.Point.Y - point1.Point.Y;

					int dist = a * a + b * b;

					point1.Distances[point2.Id] = dist;
					point2.Distances[point1.Id] = dist;
				}
			});
		}

		public static List<Cluster> Proceed(List<Point> points, int epsilon, int minPts)
		{
			if (points == null)
				throw new NullReferenceException();

			List<Cluster> clusters = new List<Cluster>();
			List<DbScanPoint> pointList = new List<DbScanPoint>();

			int id = 0;
			foreach (Point point in points)
				pointList.Add(new DbScanPoint(point, id++));

			CountDistances(pointList);

			foreach (DbScanPoint point in pointList)
			{
				if (point.Scanned == false)
				{
					List<DbScanPoint> neighborPts = RegionQuery(point, pointList, epsilon);
					point.Scanned = true;

					if (neighborPts.Count >= minPts)
						clusters.Add(new Cluster(ExpandCluster(neighborPts, pointList, minPts, epsilon)));
				}
			}

			return clusters;
		}

		private static List<Point> ExpandCluster(List<DbScanPoint> neighborPts, List<DbScanPoint> pointsList, int minPts, int epsilon)
		{
			Queue<DbScanPoint> queue = new Queue<DbScanPoint>(neighborPts);
			List<Point> cluster = new List<Point>();

			while (queue.Count > 0)
			{
				DbScanPoint point1 = queue.Dequeue();

				if (!point1.Scanned)
				{
					point1.Scanned = true;
					var neighborPts1 = RegionQuery(point1, pointsList, epsilon);
					if (neighborPts1.Count >= minPts)
						foreach (DbScanPoint pt in neighborPts1)
							queue.Enqueue(pt);
				}
				if (!point1.InCluster)
				{
					cluster.Add(point1.Point);
					point1.InCluster = true;
				}
			}
			return cluster;
		}

		private static List<DbScanPoint> RegionQuery(DbScanPoint point, List<DbScanPoint> pointsList, int epsilon)
		{
			List<DbScanPoint> ret = new List<DbScanPoint>();

			foreach (DbScanPoint otherPoint in pointsList)
				if (DistanceSquared(point, otherPoint) <= epsilon)
					ret.Add(otherPoint);
			return ret;
		}

		private static int DistanceSquared(DbScanPoint point1, DbScanPoint point2)
		{
			return point1.Distances[point2.Id];
		}
	}
}
