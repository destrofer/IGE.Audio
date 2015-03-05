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
using System.Text;

namespace IGE.IO {
	[FileFormat("wav")]
	public class WavFile : AudioFile {
		public WavFile(Stream file) : base(file) {
			BinaryReader reader = new BinaryReader(file);
			
			Format = AudioFormat.Unknown;
			Frequency = 0;
			Channels = 0;
			BitsPerSample = 0;
			Samples = 0;
			m_AudioData = null;
			
			string fileHeaderId = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
			if( !fileHeaderId.Equals("RIFF") )
				throw new Exception("File format is not RIFF");

			uint fileSize = reader.ReadUInt32();
			uint fileOffset = 4; // file type id is also counted in fileSize

			string fileTypeId = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
			if( !fileTypeId.Equals("WAVE") )
				throw new Exception("RIFF file type is not WAVE");

			while( fileOffset < fileSize ) {
				string blockId = Encoding.ASCII.GetString(reader.ReadBytes(4), 0, 4);
				uint blockSize = reader.ReadUInt32();
				uint blockOffset = 0;
				fileOffset += 8 + blockSize;
				
				if( blockId.Equals("fmt ") ) {
					Format = (AudioFormat)reader.ReadUInt16();
					if( Format != AudioFormat.PCM )
						throw new Exception("Only PCM wave file formats are supported at the moment");
						
					Channels = (uint)reader.ReadUInt16();
					if( Channels != 1 && Channels != 2 )
						throw new Exception("Only mono (1 channel) and stereo (2 channel) wave formats are supported at the moment");
						
					Frequency = (uint)reader.ReadUInt32();
					
					reader.ReadUInt32(); // nAvgBytesPerSec
					reader.ReadUInt16(); // nBlockAlign
					
					BitsPerSample = (uint)reader.ReadUInt16();
					if( BitsPerSample != 8 && BitsPerSample != 16 && BitsPerSample != 32 )
						throw new Exception("Only 8, 16 and 32 bit per sample wave formats are supported at the moment");
						
					blockOffset = 16;
					if( m_AudioData != null ) {
						Samples = (uint)m_AudioData.Length / (BitsPerSample / 8) / Channels;
						break; // no need to parse anymore
					}
				}
				
				if( blockId.Equals("data") ) {
					m_AudioData = reader.ReadBytes((int)blockSize);
					blockOffset = blockSize;
					if( BitsPerSample != 0 && Channels != 0 ) {
						Samples = blockSize / (BitsPerSample / 8) / Channels;
						break; // no need to parse anymore
					}
				}

				if( blockSize > blockOffset )
					reader.ReadBytes((int)(blockSize - blockOffset));
			}
		}
	}
}
