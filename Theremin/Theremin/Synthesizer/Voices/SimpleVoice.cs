using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Theremin.Synthesizer.Components;

namespace Theremin.Synthesizer.Voices
{
	public class SimpleVoice : Voice
	{
		// Fields
		private double time;
		private double frequency;
		private double amplitude;
		private double targetAmplitude;

		// Constructors
		public SimpleVoice()
		{
			time = 0;
			Sample = Oscillators.Sine;
		}

		// Properties
		public double Amplitude {
			get
			{
				return targetAmplitude;
			}
			set
			{
				targetAmplitude = value;
				amplitude = targetAmplitude;
			}
		}
		public double Frequency
		{
			get
			{
				return frequency;
			}
			set
			{
				// Preserve the phase of the wave when changing its frequency
				var phase = time % (1 / frequency);
				time = phase * frequency / value;
				frequency = value;
			}
		}
		public Oscillator Sample { get; set; }

		// Methods
		public void Sythesize(double[] buffer, int start, int samples, int sampleRate)
		{
			double deltaTime = 1.0 / sampleRate;
			for (var i = start; i < samples; i++)
			{
				var sample = Sample(frequency, time);
				buffer[i] += amplitude * sample;
				time += deltaTime;
			}
		}
	}
}
