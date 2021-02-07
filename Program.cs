// System
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

// ManagedBass
using ManagedBass;

// Unsigned Framework
using UnsignedFramework;

class Program
{
    public static List<string> errors = new List<string> { };
    static void Main(string[] args)
    {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleSIGINT);
        
        Console.CursorVisible = false;
        
        bool isLoop = false;
        string MusicPath = args.Length > 0 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        List<string> Files = new List<string> { };
        
        if (Directory.Exists(MusicPath)) {
            Files.AddRange(Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories).Where(a => Bass.SupportedFormats.Contains(Path.GetExtension(a))));
            Files.Shuffle();
        } else {
            Files.Add(MusicPath);
            isLoop = true;
        }

        AudioClip AC = new AudioClip { };
        if (Files.Count == 0)
        {
            Console.WriteLine($"No music files were found in \"{MusicPath}\"");
            return;
        }
        for (var i = 0; i < Files.Count; i++)
        {
            AC.Open(Files[i]);
            AC.Play();

            while (AC.ClipStatus != PlaybackState.Stopped)
            {
                string playbackPrefix = "Now playing: ";
                string playbackName = new string(AC.FileName.Take(Console.WindowWidth - playbackPrefix.Length).ToArray());
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
                
                if (isLoop) {
                    if (DateTime.Now.Second % 2 < 1) Console.ForegroundColor = ConsoleColor.White;
                    else Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("LOOP");
                }
                
                if (Files.Count > 1 + i)
                    Console.WriteLine("\nQueue: ");
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
                
                if (errors.Count > 0)
                    Console.WriteLine("\nErrors: ");
                
                int minIndex = Math.Max(errors.Count - 5, 0);
                int maxIndex = Math.Min(minIndex + 6, errors.Count);

                for (int index = minIndex; index < maxIndex; index++)
                {
                    if (DateTime.Now.Millisecond / 100 == index - minIndex + 5)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    
                    string message = new string(errors[index].Take(Console.WindowWidth).ToArray());

                    Console.WriteLine(message);
                    Console.ResetColor();
                }
                
                Thread.Sleep(50);
            }
            
            AC.Close();

            // To make it loop
            if (isLoop) i--;
        }
        Console.WriteLine("Exit");

        Console.CursorVisible = true;
    }
    static void HandleSIGINT(object source, ConsoleCancelEventArgs e) {
        e.Cancel = false;
        Console.CursorVisible = true;
    }
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