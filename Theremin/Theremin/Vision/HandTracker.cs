using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Vision
{
	public class HandFrameReadyEventArgs : EventArgs
	{
		private DepthImage HandImage;

		public HandFrameReadyEventArgs(DepthImage handImage)
		{
			HandImage = handImage;
		}

		public void CopyImageDataTo(byte[] imageData)
		{
			HandImage.RgbaData.CopyTo(imageData, 0);
		}
	}

	public class HandTracker
	{
		public void KinectFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
