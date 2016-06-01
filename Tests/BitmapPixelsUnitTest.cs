using System.Drawing;
using System.Drawing.Imaging;
using ColorTrackerLibCpp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class BitmapPixelsUnitTest
	{
		private Bitmap bitmap;
		private Bitmap clone;

		public BitmapPixelsUnitTest()
		{
			bitmap = new Bitmap(3, 2, PixelFormat.Format24bppRgb);

			bitmap.SetPixel(0, 0, Color.White);
			bitmap.SetPixel(1, 0, Color.Gray);
			bitmap.SetPixel(2, 0, Color.Black);
			bitmap.SetPixel(0, 1, Color.Red);
			bitmap.SetPixel(1, 1, Color.Green);
			bitmap.SetPixel(2, 0, Color.Blue);


			clone = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
		}

		[TestMethod]
		public void Reading()
		{
			using (var bitmapPixels = new BitmapPixels(clone))
				for (int i = 0; i < bitmap.Width; i++)
					for (int j = 0; j < bitmap.Height; j++)
						Assert.AreEqual(bitmap.GetPixel(i, j), bitmapPixels.GetPixel(i, j));
		}

		[TestMethod]
		public void Writing()
		{
			using (var bitmapPixels = new BitmapPixels(clone))
				for (int i = 0; i < bitmap.Width; i++)
					for (int j = 0; j < bitmap.Height; j++)
					{
						bitmapPixels.SetPixel(i, j, bitmap.GetPixel(i, j));
						Assert.AreEqual(bitmap.GetPixel(i, j), bitmapPixels.GetPixel(i, j));
					}
		}
	}
}
