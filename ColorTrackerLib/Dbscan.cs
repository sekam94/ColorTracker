using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ColorTrackerLib
{
	internal class Dbscan
	{
		class DbscanPoint
		{
			public Point Point { get; private set; }
			public bool Scanned { get; set; }
			public bool InCluster { get; set; }

			public int Id { get; private set; }
			public int[] Distances { get; set; }

			public DbscanPoint(Point point, int id)
			{
				Point = point;
				InCluster = false;
				Scanned = false;
				Id = id;
			}
		}

		private static void CountDistances(List<DbscanPoint> list)
		{
			foreach (var point in list)
				point.Distances = new int[list.Count];

			Parallel.ForEach(list, point1 =>
			{
				foreach (var point2 in list)
				{
					if (point1.Distances[point2.Id] != 0)
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

			epsilon *= epsilon;

			List<Cluster> clusters = new List<Cluster>();
			List<DbscanPoint> pointList = new List<DbscanPoint>();

			int id = 0;
			foreach (Point point in points)
				pointList.Add(new DbscanPoint(point, id++));

			CountDistances(pointList);

			foreach (DbscanPoint point in pointList)
			{
				if (point.Scanned == false)
				{
					List<DbscanPoint> neighborPts = RegionQuery(point, pointList, epsilon);
					point.Scanned = true;

					if (neighborPts.Count >= minPts)
						clusters.Add(new Cluster(ExpandCluster(neighborPts, pointList, minPts, epsilon)));
				}
			}

			return clusters;
		}

		private static List<Point> ExpandCluster(List<DbscanPoint> neighborPts, List<DbscanPoint> pointsList, int minPts, int epsilon)
		{
			Queue<DbscanPoint> queue = new Queue<DbscanPoint>(neighborPts);
			List<Point> cluster = new List<Point>();

			while (queue.Count > 0)
			{
				DbscanPoint point1 = queue.Dequeue();

				if (!point1.Scanned)
				{
					point1.Scanned = true;
					var neighborPts1 = RegionQuery(point1, pointsList, epsilon);
					if (neighborPts1.Count >= minPts)
						foreach (DbscanPoint pt in neighborPts1)
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

		private static List<DbscanPoint> RegionQuery(DbscanPoint point, List<DbscanPoint> pointsList, int epsilon)
		{
			List<DbscanPoint> ret = new List<DbscanPoint>();

			foreach (DbscanPoint otherPoint in pointsList)
				if (DistanceSquared(point, otherPoint) <= epsilon)
					ret.Add(otherPoint);
			return ret;
		}

		private static int DistanceSquared(DbscanPoint point1, DbscanPoint point2)
		{
			return point1.Distances[point2.Id];
		}
	}
}
