using GLFW;
using OpenTK;
using OpenTK.Audio.OpenAL;
using HelpersNS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PMLabs
{
    public class BC : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return Glfw.GetProcAddress(procName);
        }
    }

    class Program
    {
        static KeyCallback kc = KeyProcessor;

        static ALDevice device;
        static ALContext context;
        static int buf;
        static List<int> sources = new List<int>();
        static Random random = new Random();
        static bool isPlaying = false;

        public static void KeyProcessor(IntPtr window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (key == Keys.Space && state == InputState.Press)
            {
                isPlaying = !isPlaying;
                Console.WriteLine(isPlaying ? "ODTWARZANIE" : "PAUZA");

                if (isPlaying)
                {
                    foreach (var source in sources)
                    {
                        AL.SourcePlay(source);
                    }
                    StartWolfSounds();
                }
                else
                {
                    foreach (var source in sources)
                    {
                        AL.SourcePause(source);
                    }
                }
            }
        }

        public static void InitSound()
        {
            device = ALC.OpenDevice(null);
            context = ALC.CreateContext(device, new ALContextAttributes());
            ALC.MakeContextCurrent(context);

            buf = AL.GenBuffer();
            int channels, bits, sampleFreq;
            byte[] data = Helpers.LoadWave("howlwolf.wav", out channels, out bits, out sampleFreq);
            AL.BufferData<byte>(buf, Helpers.GetFormat(channels, bits), data, sampleFreq);

            for (int i = 0; i < 10; i++) // Tworzenie 10 źródeł dźwięku
            {
                int source = AL.GenSource();
                AL.BindBufferToSource(source, buf);
                AL.Source(source, ALSourceb.Looping, false);
                sources.Add(source);
            }
        }

        public static void FreeSound()
        {
            foreach (var source in sources)
            {
                AL.SourceStop(source);
                AL.DeleteSource(source);
            }
            sources.Clear();

            AL.DeleteBuffer(buf);
            if (context != ALContext.Null)
            {
                ALC.MakeContextCurrent(ALContext.Null);
                ALC.DestroyContext(context);
            }
            context = ALContext.Null;
            if (device != ALDevice.Null)
            {
                ALC.CloseDevice(device);
            }
            device = ALDevice.Null;
        }

        public static void StartWolfSounds()
        {
            Task.Run(() =>
            {
                while (isPlaying)
                {
                    int sourceIndex = random.Next(sources.Count);
                    int source = sources[sourceIndex];

                    float x = (float)(random.NextDouble() * 2.0 - 1.0) * 10.0f; // Losowe współrzędne w zakresie -10 do 10
                    float y = (float)(random.NextDouble() * 2.0 - 1.0) * 10.0f;
                    float z = (float)(random.NextDouble() * 2.0 - 1.0) * 10.0f;

                    AL.Source(source, ALSource3f.Position, x, y, z);

                    AL.SourcePlay(source);
                    Console.WriteLine($"Wycie wilka odtwarzane z pozycji: ({x}, {y}, {z})");

                    int sleepTime = random.Next(1000, 5000); // Losowe odstępy czasu między 1 a 5 sekund
                    Thread.Sleep(sleepTime);
                }
            });
        }

        public static void SoundEvents()
        {
            // Implementacja nie wymaga dodatkowych działań w tej metodzie
        }

        static void Main(string[] args)
        {
            Glfw.Init();

            Window window = Glfw.CreateWindow(500, 500, "OpenAL", GLFW.Monitor.None, Window.None);

            Glfw.MakeContextCurrent(window);
            Glfw.SetKeyCallback(window, kc);

            InitSound();

            while (!Glfw.WindowShouldClose(window))
            {
                SoundEvents();
                Glfw.PollEvents();
            }

            FreeSound();
            Glfw.Terminate();
        }
    }
}
