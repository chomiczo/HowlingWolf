using OpenTK.Audio.OpenAL;
using System;
using System.IO; // Dodane dyrektywy using
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpersNS
{
    public class Helpers
    {
        public static ALFormat GetFormat(int channels, int bits)
        {
            ALFormat format = ALFormat.Mono8;
            if (channels == 1)
            {
                if (bits == 8) format = ALFormat.Mono8;
                if (bits == 16) format = ALFormat.Mono16;
            }
            if (channels == 2)
            {
                if (bits == 8) format = ALFormat.Stereo8;
                if (bits == 16) format = ALFormat.Stereo16;
            }
            return format;
        }

        public static byte[] LoadWave(string filename, out int channels, out int bits, out int rate)
        {
            using (Stream sm = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(sm))
            {
                // Nagłówek RIFF
                string chunkID = new string(reader.ReadChars(4));
                if (chunkID != "RIFF") throw new NotSupportedException("To nie jest nawet plik RIFF");

                int chunkSize = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE") throw new NotSupportedException("To nie jest plik WAVE");

                // Format danych
                string subchunk1ID = new string(reader.ReadChars(4));
                if (subchunk1ID != "fmt ") throw new NotSupportedException("Nieznany format WAVE");

                int subchunk1IDSize = reader.ReadInt32();
                int audioFormat = reader.ReadInt16();
                int numChannels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                int byteRate = reader.ReadInt32();
                int blockAlign = reader.ReadInt16();
                int bitsPerSample = reader.ReadInt16();

                // Przesunięcie wskaźnika po nagłówkach do sekcji "data"
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    string subchunkID = new string(reader.ReadChars(4));
                    int subchunkSize = reader.ReadInt32();

                    if (subchunkID == "data")
                    {
                        byte[] data = reader.ReadBytes(subchunkSize);

                        channels = numChannels;
                        bits = bitsPerSample;
                        rate = sampleRate;

                        return data;
                    }
                    else
                    {
                        // Pomiń inne sekcje
                        reader.BaseStream.Seek(subchunkSize, SeekOrigin.Current);
                    }
                }

                throw new NotSupportedException("Nie znaleziono sekcji danych 'data' w pliku WAVE");
            }
        }

    }
}
