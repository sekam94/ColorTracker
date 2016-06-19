using System.Drawing;

namespace ColorTrackerLib
{
	class Frame
	{
		public Bitmap Bitmap { get; }
		public double Time { get; }

		public Frame(Bitmap bitmap, double time)
		{
			Bitmap = bitmap;
			Time = time;
		}
	}
}