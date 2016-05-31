using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;

namespace ColorTrackerLib.Device
{
	public class Camera
	{
		public IMoniker Moniker { get; private set; }
		public string Name { get; private set; }

		private Camera(DsDevice device)
		{
			Moniker = device.Mon;
			Name = device.Name;
		}

		public static List<Camera> GetDevices()
		{
			var capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
			var cameras = new List<Camera>();

			foreach (DsDevice capDevice in capDevices)
				cameras.Add(new Camera(capDevice));

			return cameras;
		}
	}
}
