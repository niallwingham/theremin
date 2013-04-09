using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using System.IO;
using Theremin.Synthesizer.Voices;
using Theremin.Synthesizer;
using Theremin.Vision;
using System.Threading;
using Theremin.Synthesizer.Components;

namespace Theremin
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ThereminGame : Microsoft.Xna.Framework.Game
	{
		#region Constants

		public readonly int SampleRate = 44100;
		public readonly AudioChannels Channels = AudioChannels.Mono;
		public readonly int SamplesPerBuffer = 2048;
		public readonly int BytesPerSample = 2;
		public readonly int FrameWidth = 640;
		public readonly int FrameHeight = 480;
		public readonly int DepthFrameWidth = 320;
		public readonly int DepthFrameHeight = 240;
		public readonly int BytesPerPixel = 4;
		public readonly Rectangle TopLeft;
		public readonly Rectangle TopRight;
		public readonly Rectangle BottomRight;
		public readonly Rectangle BottomLeft;
		public readonly ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
		public readonly DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;
		public readonly int ConfidenceThreshold = 4;

		#endregion

		// Graphics
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private SpriteFont segoe;
		private Texture2D circleTexture;

		// Kinect
		private KinectSensor kinect;
		private CoordinateMapper coordinateMapper;

		// Color Image
		private Texture2D colorTexture;
		private byte[] colorFrameData;

		// Depth Image
		private DepthImagePixel[] depthFrameData;
		private SkeletonPoint[] skeletonPointData;

		// Skeleton Tracking
		private Skeleton[] skeletonFrameData = new Skeleton[6];
		private Joint hipCenter;
		private Joint handLeft;
		private Joint handRight;
		private Vector2 handLeftDelta = new Vector2();
		private Vector2 handRightDelta = new Vector2();

		// Hand Tracking
		private SkeletonPoint LeftHandCenter;
		private SkeletonPoint RightHandCenter;
		private DepthImage LeftHand;
		private DepthImage RightHand;

		// Gesture Recognition
		private HandTracker LeftHandTracker;
		private IList<DepthImagePoint> ConvexHull = new List<DepthImagePoint>();
		private IList<DepthImagePoint> Contour = new List<DepthImagePoint>();
		private IList<FingertipCandidate> Fingertips = new List<FingertipCandidate>();
		private int FingerCount = 0;
		private int FingerConfidence = 0;

		// Sound Effect
		private DynamicSoundEffectInstance sound;
		private double[] synthBuffer;
		private float[] workingBuffer;
		private byte[] xnaBuffer;
		private float time;
		private float deltaTime;

		// Background Sounds
		private SimpleVoice voice = new SimpleVoice();
		private Note[,] pattern = {
			{ Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.F3, Note.F3, Note.F3, Note.F3,     Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.F3, Note.F3, Note.F3, Note.F3,     Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.F3, Note.F3, Note.F3, Note.F3,     Note.Ab3, Note.Ab3, Note.Ab3, Note.Ab3, Note.G3, Note.G3, Note.G3, Note.G3 },
			{ Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.Ab3, Note.Ab3, Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.Ab3, Note.Ab3, Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.Ab3, Note.Ab3, Note.Ab3, Note.Ab3, Note.C4, Note.C4, Note.C4, Note.C4, Note.C4, Note.C4, Note.B3, Note.B3 },
			{ Note.G4, Note.G4, Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.C4, Note.C4,     Note.G4, Note.G4, Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.C4, Note.C4,     Note.G4, Note.G4, Note.Eb4, Note.Eb4, Note.C4, Note.C4, Note.C4, Note.C4,     Note.F4, Note.F4, Note.F4, Note.F4, Note.F4, Note.F4, Note.F4, Note.F4 },
		};
		private Voice[] backgroundVoices;
		private int patternIndex;

		// Hand Ranges
		private double minFrequencyLog2 = Math.Log(Note.G3.Frequency, 2);
		private double maxFrequencyLog2 = Math.Log(Note.C6.Frequency, 2);
		private double xRange = 0.5;
		private double yRange = 0.2;


		public ThereminGame()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferHeight = 2 * FrameHeight;
			graphics.PreferredBackBufferWidth = 2 * FrameWidth;
			Content.RootDirectory = "Content";

			TopLeft = new Rectangle(0, 0, FrameWidth, FrameHeight);
			TopRight = new Rectangle(FrameWidth, 0, FrameWidth, FrameHeight);
			BottomRight = new Rectangle(FrameWidth, FrameHeight, FrameWidth, FrameHeight);
			BottomLeft = new Rectangle(0, FrameHeight, FrameWidth, FrameHeight);

			colorFrameData = new byte[FrameWidth * FrameHeight * BytesPerPixel];
			depthFrameData = new DepthImagePixel[DepthFrameWidth * DepthFrameHeight];
			skeletonPointData = new SkeletonPoint[DepthFrameWidth * DepthFrameHeight];

			// Synth
			workingBuffer = new float[SamplesPerBuffer];
			synthBuffer = new double[SamplesPerBuffer];
			xnaBuffer = new byte[SamplesPerBuffer * BytesPerSample];
			deltaTime = 1.0f / SampleRate;

			backgroundVoices = new Voice[pattern.GetLength(0)];
			for (int i = 0; i < backgroundVoices.Length; i++)
			{
				backgroundVoices[i] = new SimpleVoice();
				backgroundVoices[i].Amplitude = 1;
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			this.IsMouseVisible = true;

			// Sound
			sound = new DynamicSoundEffectInstance(SampleRate, Channels);
			sound.Play();

			// Images
			colorTexture = new Texture2D(graphics.GraphicsDevice, FrameWidth, FrameHeight);
			LeftHand = new DepthImage(graphics.GraphicsDevice, DepthFrameWidth, DepthFrameHeight);
			RightHand = new DepthImage(graphics.GraphicsDevice, DepthFrameWidth, DepthFrameHeight);

			// Gesture Recognition
			LeftHandTracker = new HandTracker();
			//Thread gestureThread = new Thread(new ThreadStart(IdentifyGestures));
			//gestureThread.Start();

			//Thread kinectThread = new Thread(new ThreadStart(StartKinect));
			//kinectThread.Start();
			StartKinect();
		}

		private void StartKinect()
		{
			if (KinectSensor.KinectSensors.Count > 0)
			{
				kinect = KinectSensor.KinectSensors.First();
				kinect.ColorStream.Enable(ColorFormat);
				kinect.DepthStream.Enable(DepthFormat);

				var smoothing = new TransformSmoothParameters
				{
					Smoothing = 0.9f,
					JitterRadius = 0.2f
				};
				kinect.SkeletonStream.Enable();

				Thread kinectEventThread = new Thread(new ThreadStart(HandleKinectFrames));
				kinectEventThread.Start();
				//kinect.AllFramesReady += kinect_AllFramesReady;
				//kinect.AllFramesReady += LeftHandTracker.KinectFramesReady;

				try
				{
					kinect.Start();
					Console.WriteLine("Started Kinect sensor.");
				}
				catch (IOException e)
				{
					Console.Error.WriteLine("Error starting Kinect sensor: {0}", e.Message);
				}

				coordinateMapper = new CoordinateMapper(kinect);
			}
		}

		private void HandleKinectFrames()
		{
			kinect.AllFramesReady += kinect_AllFramesReady;
		}

		private void LocateHands()
		{
			var leftHandCenter = new SkeletonPoint();
			var rightHandCenter = new SkeletonPoint();
			var leftHandCount = 0;
			var rightHandCount = 0;
			var zLeftMin = float.MaxValue;
			var zLeftMax = float.MinValue;
			var zRightMin = float.MaxValue;
			var zRightMax = float.MinValue;
			for (var i = 0; i < skeletonPointData.Length; i++)
			{
				var point = skeletonPointData[i];
				DepthImagePoint depthPoint;
				if (point.Z > 0)
				{
					depthPoint = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(point, DepthFormat);
				} 
				else
				{
					// If we have an 'unknown distance' point it will have no X or Y coordinates, so make a dummy point
					depthPoint = new DepthImagePoint { X = i % DepthFrameWidth, Y = i / DepthFrameWidth, Depth = 0 };
				}

				if (Math.Abs(point.X - handRight.Position.X) < 0.15 && Math.Abs(point.Y - handRight.Position.Y) < 0.15 && Math.Abs(point.Z - handRight.Position.Z + 0.1) < 0.15)
				{
					RightHand.Points[i] = depthPoint;
					LeftHand.Points[i] = new DepthImagePoint { X = depthPoint.X, Y = depthPoint.Y, Depth = 0 };

					rightHandCenter.X += point.X;
					rightHandCenter.Y += point.Y;
					rightHandCenter.Z += point.Z;
					rightHandCount++;
					if (depthPoint.Depth < zRightMin) zRightMin = depthPoint.Depth;
					else if (depthPoint.Depth > zRightMax) zRightMax = depthPoint.Depth;
				}
				else if (Math.Abs(point.X - handLeft.Position.X) < 0.15 && Math.Abs(point.Y - handLeft.Position.Y) < 0.15 && Math.Abs(point.Z - handLeft.Position.Z + 0.1) < 0.15)
				{
					LeftHand.Points[i] = depthPoint;
					RightHand.Points[i] = new DepthImagePoint { X = depthPoint.X, Y = depthPoint.Y, Depth = 0 };

					leftHandCenter.X += point.X;
					leftHandCenter.Y += point.Y;
					leftHandCenter.Z += point.Z;
					leftHandCount++;
					if (depthPoint.Depth < zLeftMin) zLeftMin = depthPoint.Depth;
					else if (depthPoint.Depth > zLeftMax) zLeftMax = depthPoint.Depth;
				}
				else
				{
					RightHand.Points[i] = new DepthImagePoint { X = depthPoint.X, Y = depthPoint.Y, Depth = 0 };
					LeftHand.Points[i] = new DepthImagePoint { X = depthPoint.X, Y = depthPoint.Y, Depth = 0 };
				}
			}
			
			leftHandCenter.X /= leftHandCount;
			leftHandCenter.Y /= leftHandCount;
			leftHandCenter.Z /= leftHandCount;
			rightHandCenter.X /= rightHandCount;
			rightHandCenter.Y /= rightHandCount;
			rightHandCenter.Z /= rightHandCount;

			lock (GraphicsDevice)
			{
				handLeftDelta.X = leftHandCenter.X - hipCenter.Position.X;
				handLeftDelta.Y = leftHandCenter.Y - hipCenter.Position.Y;
				handRightDelta.X = rightHandCenter.X - hipCenter.Position.X;
				handRightDelta.Y = rightHandCenter.Y - hipCenter.Position.Y;
				LeftHandCenter = leftHandCenter;
				RightHandCenter = rightHandCenter;
			}

			var scaleLeft = 255 / (zLeftMax - zLeftMin) * 0.75f;
			var scaleRight = 255 / (zRightMax - zRightMin) * 0.75f;
			var leftHandDepthPoint = coordinateMapper.MapSkeletonPointToDepthPoint(leftHandCenter, DepthFormat);
			var rightHandDepthPoint = coordinateMapper.MapSkeletonPointToDepthPoint(rightHandCenter, DepthFormat);
			LeftHand.Update(zLeftMin, scaleLeft, leftHandDepthPoint.X, leftHandDepthPoint.Y);
			RightHand.Update(zRightMin, scaleRight, rightHandDepthPoint.X, rightHandDepthPoint.Y);
		}

		private void IdentifyGestures(DepthImage hand)
		{
			var hull = hand.CalculateConvexHull();
			var contour = hand.CalculateContour();
			var fingertips = new List<FingertipCandidate>();

			// Only search for fingertips if we have a reasonable outline
			if (contour.Length > 64)
			{
				// Filter out the bad candidates and order the rest by score
				IEnumerable<FingertipCandidate> candidates = hull.Intersect(contour)
																 .Select(p => new FingertipCandidate { Point = p, Score = ScoreCandidate(p, contour) })
																 .Where(c => c.Score < Math.PI / 3)
																 .OrderBy(c => c.Score);

				// Find the best five candidates, clearing out neighbours
				while (fingertips.Count <= 5 && candidates.Any())
				{
					var fingertip = candidates.First();
					fingertips.Add(fingertip);
					candidates = candidates.Where(c => new Vector2(c.Point.X - fingertip.Point.X, c.Point.Y - fingertip.Point.Y).LengthSquared() > 18);
				}
			}

			// Update our gesture confidence
			if (fingertips.Count == FingerCount)
			{
				FingerConfidence++;
			}
			else
			{
				FingerConfidence = 0;
			}
			FingerCount = fingertips.Count;

			lock (GraphicsDevice)
			{
				ConvexHull = hull;
				Contour = contour;
				Fingertips = fingertips;
			}
		}

		private double ScoreCandidate(DepthImagePoint candidate, DepthImagePoint[] contour)
		{
			var index = Array.IndexOf(contour, candidate);
			var left = contour[(index - 8 + contour.Length) % contour.Length];
			var right = contour[(index + 8) % contour.Length];

			var a = new Vector2(left.X - candidate.X, left.Y - candidate.Y);
			var b = new Vector2(right.X - candidate.X, right.Y - candidate.Y);
			var theta = Math.Acos(Vector2.Dot(a, b) / a.Length() / b.Length());

			return theta;
		}

		private void ProcessColorFrame(ColorImageFrame colorFrame)
		{
			colorFrame.CopyPixelDataTo(colorFrameData);
			for (int i = 0; i < colorFrameData.Length; i += 4)
			{
				byte red = colorFrameData[i];
				colorFrameData[i] = colorFrameData[i + 2];
				colorFrameData[i + 2] = red;
				colorFrameData[i + 3] = 255;
			}

			lock (GraphicsDevice)
			{
				colorTexture.SetData(colorFrameData);
			}
		}

		private void ProcessDepthAndSkeletonFrames(DepthImageFrame depthFrame, SkeletonFrame skeletonFrame)
		{
			depthFrame.CopyDepthImagePixelDataTo(depthFrameData);
			coordinateMapper.MapDepthFrameToSkeletonFrame(DepthFormat, depthFrameData, skeletonPointData);

			skeletonFrame.CopySkeletonDataTo(skeletonFrameData);
			Skeleton skeleton = skeletonFrameData.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
			if (skeleton != null)
			{
				hipCenter = skeleton.Joints[JointType.HipCenter];
				handLeft = skeleton.Joints[JointType.HandLeft];
				handRight = skeleton.Joints[JointType.HandRight];
			}

			if (handLeft.TrackingState != JointTrackingState.NotTracked && handRight.TrackingState != JointTrackingState.NotTracked)
			{
				LocateHands();
				IdentifyGestures(LeftHand);
			}
		}

		private void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			// Update the color image texture
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					ProcessColorFrame(colorFrame);
				}
			}

			// Update the depth & skeleton data
			using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (depthFrame != null && skeletonFrame != null)
				{
					ProcessDepthAndSkeletonFrames(depthFrame, skeletonFrame);
				}
			}
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// Load images & fonts
			circleTexture = Content.Load<Texture2D>("circle");
			segoe = Content.Load<SpriteFont>("segoe");
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		private void UpdateSoundBuffer()
		{
			Array.Clear(synthBuffer, 0, SamplesPerBuffer);

			double frequency, amplitude;
			lock (GraphicsDevice)
			{
				double dX = Math.Max(0, Math.Min(xRange, handRightDelta.X)) / xRange;
				double dY = Math.Max(0, Math.Min(yRange, handLeftDelta.Y)) / yRange;
				frequency = Math.Pow(2, minFrequencyLog2 + (maxFrequencyLog2 - minFrequencyLog2) * dX);
				amplitude = Math.Pow(2, dY) - 1;
			}

			// Update sound based on gesture
			if (FingerConfidence >= ConfidenceThreshold)
			{
				switch (FingerCount)
				{
					case 1:
						voice.Sample = Oscillators.Sine;
						break;
					case 2:
						voice.Sample = Oscillators.Triangle;
						break;
					case 3:
						voice.Sample = Oscillators.Sawtooth;
						break;
					case 4:
						voice.Sample = Oscillators.Square;
						break;
					case 5:
						voice.Sample = Oscillators.Overtone;
						break;
				}
			}

			voice.Frequency = Note.GetNearestNote(frequency).Frequency;
			voice.Frequency = frequency;
			voice.Amplitude = amplitude;
			voice.Sythesize(synthBuffer, 0, SamplesPerBuffer, SampleRate);

			// Background pattern
			//int newPatternIndex = ((int)Math.Floor(time)) % pattern.GetLength(1);
			//for (int i = 0; i < backgroundVoices.Length; i++)
			//{
			//	backgroundVoices[i].Amplitude = newPatternIndex == patternIndex ? 0.2 : 0;
			//	backgroundVoices[i].Frequency = pattern[i, newPatternIndex].Frequency;
			//	backgroundVoices[i].Sythesize(synthBuffer, 0, SamplesPerBuffer, SampleRate);
			//}
			//patternIndex = newPatternIndex;

			convertBuffer(synthBuffer, xnaBuffer);
			sound.SubmitBuffer(xnaBuffer);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			this.time = (float)gameTime.TotalGameTime.TotalSeconds;

			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				this.Exit();
			}

			// Update the sound buffer
			if (sound.PendingBufferCount <= 1)
			{
				UpdateSoundBuffer();
			}
		}

		private void DrawLabels(double volume, Note note, Rectangle targetArea)
		{
			// Tracked Joints
			DrawSkeletonPoint(handLeft.Position, targetArea, Color.White, null);
			DrawSkeletonPoint(handRight.Position, targetArea, Color.White, null);

			// Hand Centers
			var leftHandLabel = string.Format("{0}%", volume);
			var rightHandLabel = string.Format("{0}", note);
			DrawSkeletonPoint(LeftHandCenter, targetArea, Color.Red, leftHandLabel);
			DrawSkeletonPoint(RightHandCenter, targetArea, Color.Red, rightHandLabel);
		}

		private void DrawSkeletonPoint(SkeletonPoint point, Rectangle targetArea, Color color, string label)
		{
			// Draw the point
			var imagePoint = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(point, ColorFormat);
			var destinationRectangle = new Rectangle(targetArea.X + imagePoint.X - 4, targetArea.Y + imagePoint.Y - 4, 8, 8);
			spriteBatch.Draw(circleTexture, destinationRectangle, color);

			// Draw the (optional) label
			if (!string.IsNullOrWhiteSpace(label))
			{
				var labelSize = segoe.MeasureString(label);
				var position = new Vector2(destinationRectangle.X + (destinationRectangle.Width - labelSize.X) / 2, destinationRectangle.Y + 8);
				spriteBatch.DrawString(segoe, label, position, color);
			}
		}

		private void DrawHandImage(DepthImage handImage, Rectangle targetArea)
		{
			// Draw the hand
			var zoom = Math.Min(targetArea.Width / handImage.SourceRectangle.Width, targetArea.Height / handImage.SourceRectangle.Height);
			var position = new Vector2(targetArea.X + (targetArea.Width - zoom * handImage.SourceRectangle.Width) / 2, targetArea.Y + (targetArea.Height - zoom * handImage.SourceRectangle.Height) / 2);
			spriteBatch.Draw(handImage.Texture, position, handImage.SourceRectangle, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0);

			// Draw the gesture points
			if (handImage == LeftHand)
			{
				position.X -= zoom * handImage.SourceRectangle.X;
				position.Y -= zoom * handImage.SourceRectangle.Y;
				foreach (var point in Contour)
				{
					DrawHandPoint(point, position, zoom, Color.White);
				}
				foreach (var point in ConvexHull)
				{
					DrawHandPoint(point, position, zoom, Color.Green);
				}
				foreach (var fingertip in Fingertips)
				{
					DrawHandPoint(fingertip.Point, position, zoom, Color.Red/*, string.Format("[{0,4:F2}]", fingertip.Score)*/);
				}
				return;
			}
		}

		private void DrawHandPoint(DepthImagePoint point, Vector2 position, int zoom, Color color, string label = null)
		{
			var destinationRectangle = new Rectangle(0, 0, 8, 8);
			destinationRectangle.X = (int)(position.X + zoom * point.X);
			destinationRectangle.Y = (int)(position.Y + zoom * point.Y);
			spriteBatch.Draw(circleTexture, destinationRectangle, color);

			if (!string.IsNullOrWhiteSpace(label))
			{
				var labelSize = segoe.MeasureString(label);
				var labelPosition = new Vector2(destinationRectangle.X - labelSize.X / 2, destinationRectangle.Y + 8);
				spriteBatch.DrawString(segoe, label, labelPosition, color);
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			var volume = Math.Truncate(Math.Log(voice.Amplitude + 1, 2) * 100);
			var note = Note.GetNearestNote(voice.Frequency);

			lock (GraphicsDevice)
			{
				GraphicsDevice.Clear(Color.White);
				spriteBatch.Begin();

				// Draw zoomed-in hands
				if (handLeft.TrackingState != JointTrackingState.NotTracked)
				{
					DrawHandImage(LeftHand, BottomLeft);
				}
				if (handRight.TrackingState != JointTrackingState.NotTracked)
				{
					DrawHandImage(RightHand, BottomRight);
				}

				// Draw colour image
				spriteBatch.Draw(colorTexture, TopLeft, Color.White);
				DrawLabels(volume, note, TopLeft);

				// Draw raw hands
				spriteBatch.Draw(LeftHand.Texture, TopRight, Color.Red);
				spriteBatch.Draw(RightHand.Texture, TopRight, Color.Red);

				spriteBatch.End();
				GraphicsDevice.Textures[0] = null;
			}

			base.Draw(gameTime);
		}

		private void convertBuffer(double[] synthBuffer, byte[] xnaBuffer)
		{
			for (int i = 0; i < SamplesPerBuffer; i++)
			{
				double sample = Math.Min(1, Math.Max(-1, synthBuffer[i]));
				short shortSample = (short)(sample * short.MaxValue);
				int index = i * BytesPerSample;
				if (BitConverter.IsLittleEndian)
				{
					xnaBuffer[index] = (byte)shortSample;
					xnaBuffer[index + 1] = (byte)(shortSample >> 8);
				}
				else
				{
					xnaBuffer[index] = (byte)(shortSample >> 8);
					xnaBuffer[index + 1] = (byte)shortSample;
				}
			}
		}
	}

	public struct FingertipCandidate
	{
		public DepthImagePoint Point;
		public double Score;
	}
}
