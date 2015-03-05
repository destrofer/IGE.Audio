/*
 * Author: Viacheslav Soroka
 * 
 * This file is part of IGE <https://github.com/destrofer/IGE>.
 * 
 * IGE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * IGE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with IGE.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;

namespace IGE.IO {
	public abstract class AudioFile : GameFile {
		public uint Frequency;
		public uint Channels;
		public uint BitsPerSample;
		public uint Samples;
		public bool IsMono { get { return Channels == 1; } }
		public AudioFormat Format;
		
		public byte[] m_AudioData;
	
		public AudioFile(Stream file) : base(file) {
		}

		// TODO: optimize the audio sample transformation algorithms
		
		protected byte Get8bitValueAt(int index) {
			return m_AudioData[index];
		}
		
		protected short Get16bitValueAt(int index) {
			// unchecked { return (short)((int)m_AudioData[index * 2 + 1] << 8 | (int)m_AudioData[index * 2]); }
			unsafe { fixed(byte *ptr = &m_AudioData[index * 2]) { return *((short *)ptr); } }
		}
		
		protected int Get32bitValueAt(int index) {
			unsafe { fixed(byte *ptr = &m_AudioData[index * 4]) { return *((int *)ptr); } }
		}
		
		protected float GetFloatValueAt(int index) {
			switch( BitsPerSample ) {
					case 32: return (float)(((double)Get32bitValueAt(index) + 2147483648.0) / 4294967296.0);
				case 16: return ((float)Get16bitValueAt(index) + 32768.0f) / 65536.0f;
			}
			return (float)Get8bitValueAt(index) / 256.0f;
		}
		
		protected float GetMonoAt(int index) {
			return (Channels == 1) ? GetFloatValueAt(index) : (0.5f * (GetFloatValueAt(index * 2) + GetFloatValueAt(index * 2 + 1)));
		}
		
		protected float GetLeftAt(int index) {
			return (Channels == 1) ? GetFloatValueAt(index) : GetFloatValueAt(index * 2);
		}
		
		protected float GetRightAt(int index) {
			return (Channels == 1) ? GetFloatValueAt(index) : GetFloatValueAt(index * 2 + 1);
		}
		
		public byte[] Get8BitData(bool stereo) {
			int i, j, bps = (stereo ? 2 : 1);
			byte[] arr = new byte[Samples * bps];
			if( stereo ) {
				for( i = (int)Samples - 1, j = bps * ((int)Samples - 1); i >= 0; i--, j -= bps ) {
					arr[j] = (byte)(GetLeftAt(i) * 256.0);
					arr[j+1] = (byte)(GetRightAt(i) * 256.0);
				}
			}
			else {
				for( i = (int)Samples - 1; i >= 0; i-- ) {
					arr[i] = (byte)(GetMonoAt(i) * 256.0);
				}
			}
			return arr;
		}
		
		public short[] Get16BitData(bool stereo) {
			int i, j, bps = (stereo ? 2 : 1);
			short[] arr = new short[Samples * bps];
			if( stereo ) {
				for( i = (int)Samples - 1, j = bps * ((int)Samples - 1); i >= 0; i--, j -= bps ) {
					arr[j] = (short)(GetLeftAt(i) * 65536.0 - 32768.0);
					arr[j+1] = (short)(GetRightAt(i) * 65536.0 - 32768.0);
				}
			}
			else {
				for( i = (int)Samples - 1; i >= 0; i-- ) {
					arr[i] = (short)(GetMonoAt(i) * 65536.0 - 32768.0);
				}
			}
			return arr;
		}
	}
	
	// find wave formats at http://www.videolan.org/developers/vlc/doc/doxygen/html/vlc__codecs_8h-source.html
	public enum AudioFormat : ushort {
		Unknown = 0x0000,
		PCM,
		ADPCM,
		IEEE_Float,
		ALAW = 0x0006,
		MULAW,
		DTS_MS,
		WMAS = 0x000a,
		IMA_ADPCM = 0x0011,
		TrueSpeech = 0x0022,
		GSM610 = 0x0031,
		MSNAUDIO = 0x0032,
		G726 = 0x0045,
		Mpeg = 0x0050,
		MpegLayer3 = 0x0055,
		Dolby_AC3_SPDIF = 0x0092,
	}
}
