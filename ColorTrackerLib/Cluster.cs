using System;
using System.Collections.Generic;
using System.Drawing;

namespace ColorTrackerLib
{
	public class Cluster
	{
		private List<Point> _points;

		public Rectangle Borders { get; }
		public int Area { get; }
		public Point CenterOfMass { get; }
		public Point Center { get; }
		public IList<Point> Points { get { return _points.AsReadOnly(); } } 
		

		public Cluster(ICollection<Point> points)
		{
			if (points.Count < 1)
				throw new ArgumentException("Array doesn't contain any points");

			_points = new List<Point>();

			int right = 0;
			int left = Int32.MaxValue;
			int top = Int32.MaxValue;
			int bottom = 0;

			int x = 0;
			int y = 0;

			foreach (Point point in points)
			{
				x += point.X;
				y += point.Y;

				Area++;
				_points.Add(point);

				if (point.X < left)
					left = point.X;

				if (point.X > right)
					right = point.X;

				if (point.Y < top)
					top = point.Y;

				if (point.Y > bottom)
					bottom = point.Y;
			}

			Borders = Rectangle.FromLTRB(left, top, right, bottom);
			CenterOfMass = new Point(x / Area, y / Area);
			Center = new Point((left + right)/2, (top + bottom)/2);
		}
	};
}
