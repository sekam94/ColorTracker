using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ColorTrackerLib
{
	public sealed class Scanner
	{
		public List<MarkerSettings> MarkerSettings { get; }

		private const int RES_MAGN = 4; // во сколько раз уменьшать разрешение
		private const float MIN_S = 0.2f;
		private const float MIN_V = 0.2f;
		private const int EPSILON = 10; //окрестность 10
		private const int NUM = 10; //количество точек в окрестности 10

		internal Scanner()
		{
			MarkerSettings = new List<MarkerSettings>();
		}

		private void DrawDebug(Frame frame, Dictionary<MarkerSettings, List<Point>> points)
		{
			frame.Bitmap = new Bitmap(frame.Bitmap);
			using (var pixels = new ColorTrackerLibCpp.BitmapPixels(frame.Bitmap))
				foreach (KeyValuePair<MarkerSettings, List<Point>> pair in points)
				{
					if (pair.Value.Count > 0)
					{
						foreach (Point point in pair.Value)
						{
							pixels.SetPixel(point.X, point.Y, Color.Red);
						}
					}
				}
		}

		internal ICollection<Marker> LocateMarkers(Frame frame)
		{
			if (MarkerSettings.Count == 0)
				return new List<Marker>();

			Bitmap copy = new Bitmap(frame.Bitmap, frame.Bitmap.Width / RES_MAGN, frame.Bitmap.Height / RES_MAGN);
			Hsv[,] hsvImage;

			using (var pixels = new ColorTrackerLibCpp.BitmapPixels(copy))
				hsvImage = ConvertToHsvParallel(pixels);

			Dictionary<MarkerSettings, List<Point>> points = FindPoints(hsvImage);
			DrawDebug(frame, points);
			List<Marker> markers = FormMarkers(points);
			return markers;
		}

		private Hsv[,] ConvertToHsvParallel(ColorTrackerLibCpp.BitmapPixels pixels)
		{
			Hsv[,] image = new Hsv[pixels.Width, pixels.Height];
			
			Parallel.ForEach(Partitioner.Create(0, pixels.Width, pixels.Width / Environment.ProcessorCount), range =>
			{
				for (int x = range.Item1; x < range.Item2; x++)
					for (int y = 0; y < pixels.Height; y++)
					{
						var pixel = pixels.GetPixel(x, y);
						image[x, y] = Hsv.FromColor(pixel);
					};
			});

			return image;
		}

		private Dictionary<MarkerSettings, List<Point>> FindPoints(Hsv[,] image)
		{
			Dictionary<MarkerSettings, List<Point>> points = new Dictionary<MarkerSettings, List<Point>>();
			foreach (MarkerSettings settings in MarkerSettings)
				points[settings] = new List<Point>();

			for (int x = 0; x < image.GetLength(0); x++)
				for (int y = 0; y < image.GetLength(1); y++)
				{
					Hsv hsv = image[x, y];
					if (hsv.S > MIN_S &&
						hsv.V > MIN_V)
						foreach (MarkerSettings settings in MarkerSettings)
						{
							float difH = Math.Abs(hsv.H - settings.AverageH);

							if (hsv.S > settings.MinS &&
								hsv.S < settings.MaxS &&
								(difH < settings.MaxDifH ||
								 difH > 360 - settings.MaxDifH))
								points[settings].Add(new Point(x * RES_MAGN, y * RES_MAGN));
						}
				}
			return points;
		}

		private List<Marker> FormMarkers(Dictionary<MarkerSettings, List<Point>> points)
		{
			List<Marker> markers = new List<Marker>();
			foreach (KeyValuePair<MarkerSettings, List<Point>> keyValuePair in points)
			{
				if (keyValuePair.Value.Count == 0)
					continue;

				List<Cluster> clusters = Dbscan.Proceed(keyValuePair.Value, EPSILON, NUM);
				foreach (Cluster cluster in clusters)
					if (cluster.Area > 5)
						markers.Add(new Marker(cluster, keyValuePair.Key));
			}
			return markers;
		}

	}

	public class Marker
	{
		public Cluster Cluster { get; }
		public MarkerSettings Settings { get; }

		public Marker(Cluster cluster, MarkerSettings settings)
		{
			Cluster = cluster;
			Settings = settings;
		}
	}
}
