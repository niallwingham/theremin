using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Theremin.Synthesizer.Components;

namespace Theremin.Synthesizer.Voices
{
	public interface Voice
	{
		// Properties
		double Frequency { get; set; }
		double Amplitude { get; set; }
		Oscillator Sample { get; set; }

		// Methods
		void Sythesize(double[] buffer, int start, int samples, int sampleRate);
	}
}
