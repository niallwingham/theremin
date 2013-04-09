using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Theremin.Synthesizer
{
	public class Note
	{
		public double Frequency { get; set; }
		public string Name { get; set; }
		public override string ToString()
		{
			return Name;
		}

		public static Note C1 = new Note { Frequency = 32.70, Name = "C1" };
		public static Note Db1 = new Note { Frequency = 34.65, Name = "Db1" };
		public static Note D1 = new Note { Frequency = 36.71, Name = "D1" };
		public static Note Eb1 = new Note { Frequency = 38.89, Name = "Eb1" };
		public static Note E1 = new Note { Frequency = 41.20, Name = "E1" };
		public static Note F1 = new Note { Frequency = 43.65, Name = "F1" };
		public static Note Gb1 = new Note { Frequency = 46.25, Name = "Gb1" };
		public static Note G1 = new Note { Frequency = 49.00, Name = "G1" };
		public static Note Ab1 = new Note { Frequency = 51.91, Name = "Ab1" };
		public static Note A1 = new Note { Frequency = 55.00, Name = "A1" };
		public static Note Bb1 = new Note { Frequency = 58.27, Name = "Bb1" };
		public static Note B1 = new Note { Frequency = 61.74, Name = "B1" };
		public static Note C2 = new Note { Frequency = 65.41, Name = "C2" };
		public static Note Db2 = new Note { Frequency = 69.30, Name = "Db2" };
		public static Note D2 = new Note { Frequency = 73.42, Name = "D2" };
		public static Note Eb2 = new Note { Frequency = 77.78, Name = "Eb2" };
		public static Note E2 = new Note { Frequency = 82.41, Name = "E2" };
		public static Note F2 = new Note { Frequency = 87.31, Name = "F2" };
		public static Note Gb2 = new Note { Frequency = 92.50, Name = "Gb2" };
		public static Note G2 = new Note { Frequency = 98.00, Name = "G2" };
		public static Note Ab2 = new Note { Frequency = 103.83, Name = "Ab2" };
		public static Note A2 = new Note { Frequency = 110.00, Name = "A2" };
		public static Note Bb2 = new Note { Frequency = 116.54, Name = "Bb2" };
		public static Note B2 = new Note { Frequency = 123.47, Name = "B2" };
		public static Note C3 = new Note { Frequency = 130.81, Name = "C3" };
		public static Note Db3 = new Note { Frequency = 138.59, Name = "Db3" };
		public static Note D3 = new Note { Frequency = 146.83, Name = "D3" };
		public static Note Eb3 = new Note { Frequency = 155.56, Name = "Eb3" };
		public static Note E3 = new Note { Frequency = 164.81, Name = "E3" };
		public static Note F3 = new Note { Frequency = 174.61, Name = "F3" };
		public static Note Gb3 = new Note { Frequency = 185.00, Name = "Gb3" };
		public static Note G3 = new Note { Frequency = 196.00, Name = "G3" };
		public static Note Ab3 = new Note { Frequency = 207.65, Name = "Ab3" };
		public static Note A3 = new Note { Frequency = 220.00, Name = "A3" };
		public static Note Bb3 = new Note { Frequency = 233.08, Name = "Bb3" };
		public static Note B3 = new Note { Frequency = 246.94, Name = "B3" };
		public static Note C4 = new Note { Frequency = 261.63, Name = "C4" };
		public static Note Db4 = new Note { Frequency = 277.18, Name = "Db4" };
		public static Note D4 = new Note { Frequency = 293.67, Name = "D4" };
		public static Note Eb4 = new Note { Frequency = 311.13, Name = "Eb4" };
		public static Note E4 = new Note { Frequency = 329.63, Name = "E4" };
		public static Note F4 = new Note { Frequency = 349.23, Name = "F4" };
		public static Note Gb4 = new Note { Frequency = 369.99, Name = "Gb4" };
		public static Note G4 = new Note { Frequency = 392.00, Name = "G4" };
		public static Note Ab4 = new Note { Frequency = 415.31, Name = "Ab4" };
		public static Note A4 = new Note { Frequency = 440.00, Name = "A4" };
		public static Note Bb4 = new Note { Frequency = 466.16, Name = "Bb4" };
		public static Note B4 = new Note { Frequency = 493.88, Name = "B4" };
		public static Note C5 = new Note { Frequency = 523.25, Name = "C5" };
		public static Note Db5 = new Note { Frequency = 554.37, Name = "Db5" };
		public static Note D5 = new Note { Frequency = 587.33, Name = "D5" };
		public static Note Eb5 = new Note { Frequency = 622.25, Name = "Eb5" };
		public static Note E5 = new Note { Frequency = 659.26, Name = "E5" };
		public static Note F5 = new Note { Frequency = 698.46, Name = "F5" };
		public static Note Gb5 = new Note { Frequency = 739.99, Name = "Gb5" };
		public static Note G5 = new Note { Frequency = 783.99, Name = "G5" };
		public static Note Ab5 = new Note { Frequency = 830.61, Name = "Ab5" };
		public static Note A5 = new Note { Frequency = 880.00, Name = "A5" };
		public static Note Bb5 = new Note { Frequency = 932.33, Name = "Bb5" };
		public static Note B5 = new Note { Frequency = 987.77, Name = "B5" };
		public static Note C6 = new Note { Frequency = 1046.50, Name = "C6" };
		public static Note Db6 = new Note { Frequency = 1108.73, Name = "Db6" };
		public static Note D6 = new Note { Frequency = 1174.66, Name = "D6" };
		public static Note Eb6 = new Note { Frequency = 1244.51, Name = "Eb6" };
		public static Note E6 = new Note { Frequency = 1318.51, Name = "E6" };
		public static Note F6 = new Note { Frequency = 1396.91, Name = "F6" };
		public static Note Gb6 = new Note { Frequency = 1479.98, Name = "Gb6" };
		public static Note G6 = new Note { Frequency = 1567.98, Name = "G6" };
		public static Note Ab6 = new Note { Frequency = 1661.22, Name = "Ab6" };
		public static Note A6 = new Note { Frequency = 1760.00, Name = "A6" };
		public static Note Bb6 = new Note { Frequency = 1864.66, Name = "Bb6" };
		public static Note B6 = new Note { Frequency = 1975.54, Name = "B6" };
		public static Note C7 = new Note { Frequency = 2093.01, Name = "C7" };
		public static Note Db7 = new Note { Frequency = 2217.46, Name = "Db7" };
		public static Note D7 = new Note { Frequency = 2349.32, Name = "D7" };
		public static Note Eb7 = new Note { Frequency = 2489.02, Name = "Eb7" };
		public static Note E7 = new Note { Frequency = 2637.02, Name = "E7" };
		public static Note F7 = new Note { Frequency = 2793.83, Name = "F7" };
		public static Note Gb7 = new Note { Frequency = 2959.96, Name = "Gb7" };
		public static Note G7 = new Note { Frequency = 3135.97, Name = "G7" };
		public static Note Ab7 = new Note { Frequency = 3322.44, Name = "Ab7" };
		public static Note A7 = new Note { Frequency = 3520.00, Name = "A7" };
		public static Note Bb7 = new Note { Frequency = 3729.31, Name = "Bb7" };
		public static Note B7 = new Note { Frequency = 3951.07, Name = "B7" };

		private static Note[] notes = { C1, Db1, D1, Eb1, E1, F1, Gb1, G1, Ab1, A1, Bb1, B1, C2, Db2, D2, Eb2, E2, F2, Gb2, G2, Ab2, A2, Bb2, B2, C3, Db3, D3, Eb3, E3, F3, Gb3, G3, Ab3, A3, Bb3, B3, C4, Db4, D4, Eb4, E4, F4, Gb4, G4, Ab4, A4, Bb4, B4, C5, Db5, D5, Eb5, E5, F5, Gb5, G5, Ab5, A5, Bb5, B5, C6, Db6, D6, Eb6, E6, F6, Gb6, G6, Ab6, A6, Bb6, B6, C7, Db7, D7, Eb7, E7, F7, Gb7, G7, Ab7, A7, Bb7, B7 };

		public static Note GetNearestNote(double frequency)
		{
			return notes.LastOrDefault(note => note.Frequency <= frequency) ?? notes.First();
		}
	}
}
