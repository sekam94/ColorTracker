using System.Drawing;

namespace ColorTrackerLib
{
	class Frame
	{
		public Bitmap Bitmap { get; set; }
		public double Time { get; private set; }

		public Frame(Bitmap bitmap, double time)
		{
			Bitmap = bitmap;
			Time = time;
		}
	}
}