using System.IO.Packaging;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System;
using ManagedBass;
using UnsignedFramework;

namespace BassTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            string MusicPath = args.Length > 0 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            List<string> Files = new List<string> { };
            Files.AddRange(Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories).Where(a => a.EndsWith(".mp3") || a.EndsWith(".wav") || a.EndsWith(".m4a")));
            Files.Shuffle();

            AudioClip AC;
            try
            {
                AC = new AudioClip { };
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("Windows: Download the .zip at https://www.un4seen.com/ and extract Bass.dll to this app's folder");
                Console.WriteLine("Linux: Same procedure as Windows, instead of extracting Bass.dll, extact libbass.so and copy that to /lib/libbass.so");
                return;
            }
            if (Files.Count == 0)
            {
                Console.WriteLine($"No music files were found in \"{MusicPath}\"");
                return;
            }
            for (var i = 0; i < Files.Count; i++)
            {
                if (AC.ClipStatus == PlaybackState.Stopped)
                {
                    try
                    {
                        if (AC.FilePath != null)
                            AC.Replace(Files[i]);
                        else
                            AC.Open(Files[i]);
                        AC.Play();
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                while (AC.ClipStatus == PlaybackState.Playing)
                {
                    string PlaybackStatus = $"Now playing: {Path.GetFileNameWithoutExtension(AC.FileName)} [{Math.Round(AC.ClipPosition * 10) / 10}s/{Math.Round(AC.ClipLength * 10) / 10}s]";
                
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(BarGraph(AC.ClipPosition * 1000, AC.ClipLength * 1000, Console.WindowWidth));
                    Console.ResetColor();
                    Console.WriteLine(PlaybackStatus);
                    if (Files.Count > i + 1)
                    {
                        Console.WriteLine("Queue: ");
                        for (var index = i; index < Math.Clamp(Files.Count, 0, 6 + i); index++)
                        {
                            if (index > i)
                                Console.WriteLine($"{index - i}: {Path.GetFileNameWithoutExtension(Files[index])}");
                        }
                    Thread.Sleep(100);
                }
                Thread.Sleep(10);
            }
            AC.Close();

            Console.CursorVisible = true;
        }}
        static string BarGraph(double value, double maxValue, int width)
        {
            string output = "";
            string format = System.Environment.OSVersion.Platform == PlatformID.Win32NT ? "█" : "▏▎▍▌▋▊▉█";

            double ratio = value / maxValue;
            for (double i = 0; i < Math.Ceiling(width * ratio); i++)
            {
                output += format[(int)Math.Clamp(((ratio * format.Length) * width) - (i * format.Length), 0, format.Length - 1)];
            }
            output += new string(' ', (int)Math.Floor((1 - ratio) * width));
            return output;
        }
    }
}
