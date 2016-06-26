using System;
using System.Drawing;
using ColorTrackerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class OperationsTest
	{
		[TestMethod]
		public void LengthTest()
		{
			var a = new PointF(0, 1);
			var b = new PointF(1, 0);

			Assert.AreEqual(a.VectorLen(), 1);
			Assert.AreEqual(b.VectorLen(), 1);
		}

		[TestMethod]
		public void MiddlePointTest()
		{
			var a = new PointF(0, 0);
			var b = new PointF(2, 2);

			Assert.AreEqual(a.MiddlePoint(b), new PointF(1, 1));
		}

		[TestMethod]
		public void AngleToOxTest()
		{
			var a = new PointF(0, 0);
			var b = new PointF(2, 0);

			var res1 = Operations.AngleToOX(a, b);
			var res2 = Operations.AngleToOX(b, a);

			Assert.AreEqual(res1, 0);
			Assert.AreEqual(res2, Math.PI);
		}
	}
}
