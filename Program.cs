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
            AC.Open(Files[0]);
            for (var i = 0; i < Files.Count; i++)
            {
                AC.Replace(Files[i]);
                AC.Play();

                while (AC.ClipStatus == PlaybackState.Playing)
                {
                    string playbackPrefix = "Now playing: ";
                    string playbackName = new string(Path.GetFileNameWithoutExtension(AC.FileName).Take(Console.WindowWidth - playbackPrefix.Length).ToArray());
                    string playbackProgress = $" [{Math.Round(AC.ClipPosition * 10) / 10 + "s", -5}/{Math.Round(AC.ClipLength * 10) / 10 + "s", -5}]";
                
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(BarGraph(AC.ClipPosition * 1000, AC.ClipLength * 1000, Console.WindowWidth));
                    Console.ResetColor();
                    Console.Write(playbackPrefix);
                    Console.Write(playbackName);
                    try {
                        Console.CursorLeft = Console.WindowWidth - playbackProgress.Length;
                    } catch { }
                    Console.WriteLine(playbackProgress);
                    if (Files.Count > i + 1)
                    {
                        Console.WriteLine("Queue: ");
                        for (var index = i; index < Math.Clamp(Files.Count, 0, 6 + i); index++)
                        {
                            if (DateTime.Now.Millisecond / 100 == index - 1)
                                Console.ForegroundColor = ConsoleColor.White;
                            else if (DateTime.Now.Millisecond / 100 == index)
                                Console.ForegroundColor = ConsoleColor.Gray;
                            else
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                            if (i == index) continue;
                            
                            string prefix = $"{index - i}: ";
                            string name = Path.GetFileNameWithoutExtension(Files[index]);
                            string extension = Path.GetExtension(Files[index]).ToUpper();
                            
                            Console.Write(prefix);
                            Console.ResetColor();
                            
                            Console.Write(new string(name.Take(Console.WindowWidth - extension.Length - 2 - prefix.Length).ToArray()));
                            try {
                                Console.CursorLeft = Console.WindowWidth - extension.Length - 2;
                            } catch { }
                            Console.WriteLine($"[{extension}]");
                            Console.ResetColor();
                        }
                    Thread.Sleep(50);
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
