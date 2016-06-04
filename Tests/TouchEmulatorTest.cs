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
				emulator.Run();

				emulator.Right.Enabled = true;
				Thread.Sleep(500);
				emulator.Right.Enabled = false;
				Thread.Sleep(500);

				Assert.IsTrue(emulator.IsRunning);
			}
		}

		[TestMethod]
		public void Tap()
		{
			using (var emulator = GetEmulatorInstance())
			{
				emulator.Run();

				emulator.Right.Point = new Point(100, 100);

				emulator.Right.Enabled = true;
				emulator.Right.Contact = true;
				Thread.Sleep(150);
				emulator.Right.Contact = false;
				emulator.Right.Enabled = false;
				Thread.Sleep(150);

				Assert.IsTrue(emulator.IsRunning);
			}
		}

		[TestMethod]
		public void Moving()
		{
			using (var emulator = GetEmulatorInstance())
			{
				emulator.Run();

				emulator.Right.Enabled = true;

				Point p = new Point(300, 300);

				for (int i = 0; i < 20; i++)
				{
					emulator.Right.Point = new Point
					{
						X = p.X + (int)(Math.Cos(i) * 100),
						Y = p.Y + (int)(Math.Sin(i) * 100)
					};
					Thread.Sleep(100);
				}

				emulator.Right.Enabled = false;
				Assert.IsTrue(emulator.IsRunning);
			}
		}
	}
}
