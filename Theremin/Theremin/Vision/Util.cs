using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Vision
{
	class Util
	{
		public static bool IsLeftTurn(DepthImagePoint a, DepthImagePoint b, DepthImagePoint c)
		{
			var crossProduct = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
			return crossProduct > 0;
		}
	}
}
