using System;
using System.Drawing;
using System.Threading;
using ColorTrackerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class TouchEmulatorTest
	{

		private TouchEmulator GetEmulatorInstance()
		{
			TouchEmulator emulator = new TouchEmulator();
			
			emulator.Right.Point = new Point(100, 100);
			emulator.Left.Point = new Point(100, 100);
			
			return emulator;
		} 


		[TestMethod]
		public void Running()
		{
			using (var emulator = GetEmulatorInstance())
			{
				emulator.Right.Enabled = true;
				emulator.Update();
				emulator.Right.Enabled = false;
				emulator.Update();
			}
		}

		[TestMethod]
		public void Tap()
		{
			using (var emulator = GetEmulatorInstance())
			{
				emulator.Right.Point = new Point(100, 100);

				emulator.Right.Enabled = true;
				emulator.Right.InContact = true;
				emulator.Update();

				Thread.Sleep(150);
				emulator.Update();
				Thread.Sleep(150);

				emulator.Right.InContact = false;
				emulator.Right.Enabled = false;
				emulator.Update();

				Thread.Sleep(150);
			}
		}

		[TestMethod]
		public void Moving()
		{
			using (var emulator = GetEmulatorInstance())
			{
				emulator.Right.Enabled = true;

				Point p = new Point(300, 300);

				for (int i = 0; i < 20; i++)
				{
					emulator.Right.Point = new Point
					{
						X = p.X + (int)(Math.Cos(i / 10d) * 100),
						Y = p.Y + (int)(Math.Sin(i / 10d) * 100)
					};
					emulator.Update();
					Thread.Sleep(10);
				}

				emulator.Right.Enabled = false;
				emulator.Update();
			}
		}
	}
}
