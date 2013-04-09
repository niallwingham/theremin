using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Synthesizer.Components
{
	public delegate double Oscillator(double frequency, double time);

	public static class Oscillators
	{
		public static double Sine(double frequency, double time)
		{
			return Math.Sin(frequency * time * 2 * Math.PI);
		}

		public static double Triangle(double frequency, double time)
		{
			return Math.Abs(2 * (time * frequency - Math.Floor(time * frequency + 0.5))) * 2 - 1;
		}

		public static double Sawtooth(double frequency, double time)
		{
			var sample = 2 * (time * frequency - Math.Floor(time * frequency + 0.5));
			return sample / 4.0;
		}

		public static double Square(double frequency, double time)
		{
			var sample = Sine(frequency, time) >= 0 ? 1 : -1;
			return sample / 4.0;
		}

		public static double Overtone(double frequency, double time)
		{
			return Sine(frequency, time) * 0.7 + Sine(frequency * 2, time) * 0.1 + Sine(frequency * 3, time) * 0.1 + Sine(frequency * 4, time) * 0.05 + Sine(frequency * 5, time) * 0.05;
		}
	}
}
