using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Synthesizer.Components
{
	public class PortamentoVoice
	{
		// Fields
		private IDictionary<float, FrequencyState> frequencies;
		private float targetFrequency;
		private float time;

		// Properties
		public Oscillator Sample { get; set; }
		public TimeSpan CrossFadeDuration { get; set; }
		public float Frequency {
			get {
				return targetFrequency;
			}
			set {
				if (targetFrequency != value)
				{
					var previous = frequencies[targetFrequency];
					previous.State = State.Release;

					targetFrequency = value;
					if (frequencies.ContainsKey(targetFrequency))
					{
						var current = frequencies[targetFrequency];
						current.State = State.Attack;
					}
					else
					{
						float fade = (float) CrossFadeDuration.TotalSeconds;
						frequencies.Add(targetFrequency, new FrequencyState(fade, fade));
					}
				}

				targetFrequency = value;
				if (!frequencies.ContainsKey(targetFrequency))
				{
					frequencies.Add(targetFrequency, new FrequencyState());
				}
			}
		}

		// Constructors
		public PortamentoVoice()
		{
			frequencies = new Dictionary<float, FrequencyState>();
			time = 0;

			Sample = Oscillators.Sine;
			CrossFadeDuration = TimeSpan.FromSeconds(0.5);
		}

		// Methods
		public void Sythesize(double[] buffer, int start, int samples, int sampleRate)
		{
			var deltaTime = 1 / sampleRate;
			foreach (var entry in frequencies.ToList())
			{
				float time = this.time;
				float frequency = entry.Key;
				FrequencyState state = entry.Value;
				for (var i = start; i < samples; i++)
				{
					buffer[i] += (double) state.Amplitude * Sample(entry.Key, time);
					time += deltaTime;
					state.Tick(deltaTime);
				}
				if (state.State == State.Dead)
				{
					frequencies.Remove(frequency);
				}
			}
			this.time += (samples - start) * deltaTime;
		}
	}

	enum State
	{
		Attack,
		Sustain,
		Release,
		Dead
	};

	struct FrequencyState
	{
		private float attack;
		private float release;
		private float duration;
		private State state;

		public FrequencyState(float attack, float release)
		{
			this.attack = attack;
			this.release = release;
			duration = 0;
			state = State.Attack;
		}

		public float Amplitude
		{
			get
			{
				switch (state)
				{
					case State.Sustain:
						return 1;
					case State.Attack:
						return 0.5f;
					case State.Release:
						return 0.5f;
					default:
						return 0;
				}
			}
		}

		public State State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
				duration = 0;
			}
		}

		public void Tick(float time)
		{
			duration += time;
			if (state == State.Attack && duration > attack)
			{
				state = State.Sustain;
				duration = duration - attack;
			}
			else if (state == State.Release && duration > release)
			{
				state = State.Dead;
				duration = duration - release;
			}
		}
	}
}
