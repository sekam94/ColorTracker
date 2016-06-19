using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ColorTrackerLib
{
	public sealed class Scanner
	{
		public List<MarkerSettings> MarkerSettings { get; } = new List<MarkerSettings>();

		private const int RES_MAGN = 4; // во сколько раз уменьшать разрешение
		private const float MIN_S = 0.2f;
		private const float MIN_V = 0.2f;
		private const int EPSILON = 20; //окрестность
		private const int NUM = 10; //количество точек в окрестности

		internal Dictionary<MarkerSettings, List<Cluster>> LocateMarkers(Frame frame)
		{
			if (MarkerSettings.Count == 0)
				return new Dictionary<MarkerSettings, List<Cluster>>();

			Bitmap copy = new Bitmap(frame.Bitmap, frame.Bitmap.Width / RES_MAGN, frame.Bitmap.Height / RES_MAGN);
			Hsv[,] hsvImage;

			using (var pixels = new ColorTrackerLibCpp.BitmapPixels(copy))
				hsvImage = ConvertToHsvParallel(pixels);

			Dictionary<MarkerSettings, List<Point>> points = FindPoints(hsvImage);
			//DrawDebug(frame, points);
			return FormMarkers(points);
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

		private Dictionary<MarkerSettings, List<Cluster>> FormMarkers(Dictionary<MarkerSettings, List<Point>> points)
		{
			Dictionary<MarkerSettings, List<Cluster>> markers = new Dictionary<MarkerSettings, List<Cluster>>();
			foreach (KeyValuePair<MarkerSettings, List<Point>> keyValuePair in points)
			{
				if (keyValuePair.Value.Count == 0)
					continue;

				var result = new List<Cluster>();
				foreach (Cluster cluster in Dbscan.Proceed(keyValuePair.Value, EPSILON, NUM))
					if (cluster.Area > 20)
						result.Add(cluster);

				if (result.Count > 0)
					markers[keyValuePair.Key] = result;
			}
			return markers;
		}
	}
}
