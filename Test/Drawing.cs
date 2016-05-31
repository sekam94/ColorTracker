using System;
using System.Drawing;

namespace Test
{
	class Drawing : IDisposable
	{
		public Bitmap Picture { get { return _picture; } }

		private Bitmap _picture;
		private Graphics _graphics;

		public Drawing(int width, int height)
		{
			_picture = new Bitmap(width, height);
			_graphics = Graphics.FromImage(Picture);
			Clear();
		}

		public void Clear()
		{
			_graphics.FillRegion(new SolidBrush(Color.White), new Region(new Rectangle(0, 0, _picture.Width, _picture.Height)));
		}

		public void DrawCircle(Point point, float radius, Color color)
		{
			_graphics.FillEllipse(new SolidBrush(color), point.X - radius, point.Y - radius, radius * 2, radius * 2);
		}

        public void DrawLine(Point point1, Point point2, float thickness, Color color)
        {
            _graphics.DrawLine(new Pen(color, thickness), point1, point2);
        }

		public void Dispose()
		{
			_graphics.Dispose();
		}
	}
}
