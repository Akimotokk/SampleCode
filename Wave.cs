using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace dsp2
{
	public class Wave
	{
		string filename; //FileName(.wav)
		private byte[] riffID; // "riff"
		private uint size;  // ファイルサイズ-8
		private byte[] wavID;  // "WAVE"
		private byte[] fmtID;  // "fmt "
		private uint fmtSize; // fmtchunkbite
		private ushort format; // Format
		private ushort channels; // channels
		private uint sampleRate; //samplingRate
		private uint bytePerSec; // datapersec
		private ushort blockSize; // blocksize
		private ushort bit;  // bit
		private byte[] dataID; // "data"
		private uint dataSize; // sampledatasize
		List<short> dataSampleL;
		List<short> dataSampleR;
		public Wave (string fn)
		{
			filename = fn;
		}
		public void readWave()
		{
			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			using (BinaryReader br = new BinaryReader(fs))
			{
				try
				{
					riffID = br.ReadBytes(4);
					size = br.ReadUInt32();
					wavID = br.ReadBytes(4);
					fmtID = br.ReadBytes(4);
					fmtSize = br.ReadUInt32();
					format = br.ReadUInt16();
					channels = br.ReadUInt16();
					sampleRate = br.ReadUInt32();
					bytePerSec = br.ReadUInt32();
					blockSize = br.ReadUInt16();
					bit = br.ReadUInt16();
					dataID = br.ReadBytes(4);
					dataSize = br.ReadUInt32();
					dataSampleL = new List<short>();
					dataSampleR = new List<short>();
					if(channels == 1)
					{
						for(int i = 0;i < dataSize / blockSize;i++)
						{
							dataSampleL.Add((short)br.ReadUInt16());
						}
					}
					else if(channels == 2){

						for(int i = 0;i < dataSize / blockSize;i++)
						{
							dataSampleL.Add((short)br.ReadUInt16());
							dataSampleR.Add((short)br.ReadUInt16());
						}
					}
				}
				finally
				{
					if (br != null)
					{
						br.Close();
					}
					if (fs != null)
					{
						fs.Close();
					}
				}
			}

		}
		public void writeWave(string fn)
		{
			if (channels == 2) {
				dataSize = (uint)Math.Max (dataSampleL.Count, dataSampleR.Count) * blockSize;
			}
			else if(channels == 1){
				dataSize = (uint)dataSampleL.Count * blockSize;
			}
			using (FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write))
				using (BinaryWriter bw = new BinaryWriter(fs))
			{
				try
				{
					bw.Write(riffID);
					bw.Write(size);
					bw.Write(wavID);
					bw.Write(fmtID);
					bw.Write(fmtSize);
					bw.Write(format);
					bw.Write(channels);
					bw.Write(sampleRate);
					bw.Write(bytePerSec);
					bw.Write(blockSize);
					bw.Write(bit);
					bw.Write(dataID);
					bw.Write(dataSize);
					if(channels == 2){

						for (int i = 0; i < dataSize / blockSize; i++)
						{
							if (i < dataSampleL.Count)
							{
								bw.Write((ushort)dataSampleL[i]);
							}
							else
							{
								bw.Write(0);
							}

							if (i < dataSampleR.Count)
							{
								bw.Write((ushort)dataSampleR[i]);
							}
							else
							{
								bw.Write(0);
							}
						}
					}
					else if(channels == 1){
						for (int i = 0; i < dataSize / blockSize; i++)
						{
							if (i < dataSampleL.Count)
							{
								bw.Write((ushort)dataSampleL[i]);
							}
							else
							{
								bw.Write(0);
							}
						}
					}
				}
				finally
				{
					if (bw != null)
					{
						bw.Close();
					}
					if (fs != null)
					{
						fs.Close();
					}
				}
			}

			return;
		}
 		public List<short> getSample(int channel){
			if (channel == 1) {
				return dataSampleL;
			} else if (channel == 2) {
				return dataSampleR;
			} else {
				return null;			
			}
		}
		public void showWaveHeader()
		{
			Console.WriteLine ("FileName = " + filename);
			Console.WriteLine ("riffID = " + riffID);
			Console.WriteLine ("size = " + size);
			Console.WriteLine ("wavID= " + wavID);
			Console.WriteLine ("fmtID = " + fmtID);
			Console.WriteLine ("fmtSize = " + fmtSize);
			Console.WriteLine ("format = " + format);
			Console.WriteLine ("channels = " + channels);
			Console.WriteLine ("bytePerSec = " + bytePerSec);
			Console.WriteLine ("blockSize = " + blockSize);
			Console.WriteLine ("bit = " + bit);
			Console.WriteLine ("dataID " + dataID);
			Console.WriteLine ("dataSize = " + dataSize);

		}
		public void add_Sinewave(double frequency,int samplesize,short amplitude)
		{
			if (samplesize > this.dataSampleL.Count) {
				Console.WriteLine (dataSampleL.Count + " : " + dataSampleR.Count);
				Console.WriteLine ("SampleSizeError");
				return;
			}
			for (int i = 0; i < samplesize; i++) {
					double t = (double)i / sampleRate;
					this.dataSampleL [i] += (short) (Math.Sin (2 * Math.PI * frequency * t) * amplitude );
					if (channels == 2) {
						this.dataSampleR[i] +=  (short) (Math.Sin (2 * Math.PI * frequency * t) * amplitude );
					}
			}
		}
		public void FIR(List<Complex> filter) {
			List<Complex> resL = null;
			List<Complex> resR = null;
			this.FIR_calculation (filter, ref resL, ref resR);
			this.dataSampleL = Fft.ToShort (resL);
			this.dataSampleR = Fft.ToShort (resR);
		}
		public void FIR_delay(int delaytime){
			List<Complex> delay = new List<Complex> ();
			for (int i = 0; i < delaytime; i++) {
				delay.Add (new Complex ());
			}
			delay.Add(new Complex(1,0));
			FIR (delay);
		}
		private void FIR_calculation(List<Complex> filter,ref List<Complex> resultL,ref List<Complex> resultR)
		{
			int n = this.dataSampleL.Count - 1 + filter.Count;
			String bin = Convert.ToString(n, 2);
			int ftpoint = (int)Math.Pow(2.0, bin.Length);
			List<short> xL = appendArray (this.dataSampleL, ftpoint - this.dataSampleL.Count);
			List<short> xR = appendArray (this.dataSampleR, ftpoint - this.dataSampleR.Count);
			List<Complex> y1 = appendArray(filter, ftpoint - filter.Count);
			List <Complex> XL = Fft.Fft_Ifft(xL, ftpoint, true);
			List <Complex> XR = Fft.Fft_Ifft(xR, ftpoint, true);
			y1 = Fft.Fft_Ifft(y1, ftpoint, true);

			List<Complex> power_spectrum1 = new List<Complex>();
			List<Complex> power_spectrum2 = new List<Complex>();
			for (int i = 0; i < ftpoint; i++)
			{
				power_spectrum1.Add(y1[i] * XL[i]);
				power_spectrum2.Add(y1[i] * XR[i]);
			}
			power_spectrum1 = Fft.Fft_Ifft(power_spectrum1, ftpoint, false);
			power_spectrum2 = Fft.Fft_Ifft(power_spectrum2, ftpoint, false);
			resultL = power_spectrum1;
			resultR = power_spectrum2;
		}
		private static List<Complex> appendArray(List<Complex> x,int size)
		{
			List<Complex> result = new List<Complex>(x);
			for (int i = 0;i < size ; i++)
			{
				result.Add(new Complex());
			}
			return result;

		}
		private static List<short> appendArray(List<short> x,int size)
		{
			List<short> result = new List<short>(x);
			for (int i = 0;i < size ; i++)
			{
				result.Add(0);
			}
			return result;
		}
	}
}

