using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Vision
{
	public class DepthImage
	{
		public readonly int Width;
		public readonly int Height;
		public DepthImagePoint[] Points;
		public Rectangle SourceRectangle;
		public Texture2D Texture;
		public byte[] RgbaData;

		private GraphicsDevice GraphicsDevice;

		public DepthImage(GraphicsDevice graphics, int width, int height)
		{
			Width = width;
			Height = height;
			Points = new DepthImagePoint[width * height];
			RgbaData = new byte[width * height * 4];
			Texture = new Texture2D(graphics, width, height);
			SourceRectangle = new Rectangle(0, 0, width, height);
			GraphicsDevice = graphics;
		}

		public DepthImagePoint DepthPointAt(int x, int y)
		{
			return Points[Width * y  + x];
		}

		public void Update(float min, float scale, int cx, int cy)
		{
			var points = Points.Where(HasDepth);
			if (!points.Any())
			{
				return;
			}

			// Update the RGBA data
			Array.Clear(RgbaData, 0, RgbaData.Length);
			foreach (var point in points)
			{
				var rgbaIndex = (Width * point.Y + point.X) * 4;
				var intensity = (byte)(scale * (point.Depth - min));
				RgbaData[rgbaIndex] = intensity;
				RgbaData[rgbaIndex + 1] = intensity;
				RgbaData[rgbaIndex + 2] = intensity;
				RgbaData[rgbaIndex + 3] = 255;
			}

			// Update the texture
			lock (GraphicsDevice)
			{
				Texture.SetData(RgbaData);
			}

			// Calculate the clipping rectangle
			var padding = 10;
			var xMin = points.Min(p => p.X) - padding;
			var xMax = points.Max(p => p.X) + padding;
			var yMin = points.Min(p => p.Y) - padding;
			var yMax = points.Max(p => p.Y) + padding;

			// Center the clipping rectangle around the hand's center (and make sure we have a non-zero width)
			var dx = Math.Max(xMax - cx, cx - xMin);
			var dy = Math.Max(yMax - cy, cy - yMin);
			SourceRectangle.X = cx - dx;
			SourceRectangle.Y = cy - dy;
			SourceRectangle.Width = 2 * dx;
			SourceRectangle.Height = 2 * dy;
		}

		public DepthImagePoint[] CalculateConvexHull()
		{
			var hull = new DepthImagePoint[100];

			// Sort the points by x-coordinate
			var sortedPoints = Points.Where(HasDepth).OrderBy(p => p.X);

			// Calculate the lower hull
			var n = 0;
			foreach (var p in sortedPoints)
			{
				while (n >= 2 && !Util.IsLeftTurn(hull[n - 2], hull[n - 1], p)) n--;
				hull[n++] = p;
			}

			// Calculate the upper hull
			var min = n + 1;
			foreach (var p in sortedPoints.Reverse())
			{
				while (n >= min && !Util.IsLeftTurn(hull[n - 2], hull[n - 1], p)) n--;
				hull[n++] = p;
			}

			// Return the hull elements (the last point is the same as the first)
			return hull.Take(n - 1).ToArray();
		}

		public DepthImagePoint[] CalculateContour()
		{
			var contour = new List<DepthImagePoint>();
			var current = Points.FirstOrDefault(HasDepth);
			var previous = DepthPointAt(current.X > 0 ? current.X - 1 : current.X + 1, current.Y > 0 ? current.Y - 1 : current.Y + 1);
			do
			{
				var neighbourhood = MooreNeighbourhood(current, previous);
				contour.Add(current);
				previous = current;
				current = neighbourhood.FirstOrDefault(HasDepth);
			} while (current != contour[0] && HasDepth(current));
			return contour.ToArray();
		}

		private bool HasDepth(DepthImagePoint point)
		{
			return point.Depth > 0;
		}

		private IEnumerable<DepthImagePoint> MooreNeighbourhood(DepthImagePoint center, DepthImagePoint start)
		{
			var x = start.X;
			var y = start.Y;
			for (var i = 0; i < 8; i++)
			{
				// Circle the center point
				var dx = x - center.X;
				var dy = y - center.Y;
				if (dx + dy != 0) x -= dy;
				if (dx - dy != 0) y += dx;

				// Skip pixels outside the image boundaries
				if (0 <= x && x < Width && 0 <= y && y < Height)
				{
					yield return DepthPointAt(x, y);
				}
			}
		}
	}
}
