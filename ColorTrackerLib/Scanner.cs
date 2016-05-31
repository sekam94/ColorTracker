using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ColorTrackerLib
{
	public sealed class Scanner
	{
		public List<MarkerSettings> MarkerSettings { get; }

		private const int RES_MAGN = 2; // во сколько раз уменьшать разрешение
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
			{
				//hsvImage = ConvertToHsvParallel(pixels);

				for (int x = 0; x < copy.Width/2; x++)
					for (int y = 0; y < copy.Height; y++)
					{
						//Hsv hsv = hsvImage[x, y];
						//hsv.V = 1;
						//pixels.SetPixel(x, y, Hsv.ToColor(hsv));

						Color c = pixels.GetPixel(x, y);
						pixels.SetPixel(x,y, pixels.GetPixel(pixels.Width - x, y));
						pixels.SetPixel(pixels.Width - x, y, c);
					}
			}


			frame.Bitmap = copy;

			//Dictionary <MarkerSettings, List<Point>> points = FindPoints(hsvImage);
			//DrawDebug(frame, points);
			//List<Marker> markers = FormMarkers(points);
			//return markers;

			return new List<Marker>();
		}

		private Hsv[,] ConvertToHsvParallel(ColorTrackerLibCpp.BitmapPixels pixels)
		{
			Hsv[,] image = new Hsv[pixels.Width, pixels.Height];

			var cpus = Environment.ProcessorCount;
			var len = pixels.Width / cpus;

			Parallel.For(0, cpus, delegate (int i)
			{
				for (int x = len * i; x < len * (i + 1) && x < pixels.Width; x++)
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
			//TODO: сделать через массив
			Dictionary<MarkerSettings, List<Point>> pointsC = new Dictionary<MarkerSettings, List<Point>>();
			foreach (MarkerSettings settings in MarkerSettings)
				pointsC[settings] = new List<Point>();

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
								pointsC[settings].Add(new Point(x * RES_MAGN, y * RES_MAGN));
						}
				}
			return pointsC;
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
