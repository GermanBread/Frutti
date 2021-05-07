// System
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

// ManagedBass
using ManagedBass;

// Unsigned Framework
using UnsignedFramework;

// FFT
using FftSharp;

class Program
{
    /*
        Note: This code is a complete mess
    */
    public static List<string> errors = new List<string> { };
    private static CancellationTokenSource cts = new CancellationTokenSource();
    private static RPC rpc = new RPC();
    private static int refreshCounter = 0;
    static void Main(string[] args)
    {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleSIGINT);
        
        Console.CursorVisible = false;

        int _argOffset = 0;
        bool _clearConsole = true;
        if (args.Contains("--help") || args.Contains("-h")) {
            Console.WriteLine("Fruti made by GermanBRead#9077");
            Console.WriteLine("\t-h / --help = this help");
            Console.WriteLine("\t--noclear = prevent console from being cleared (fixes flicker but causes issues when console window is resized)");
            return;
        }
        if (args.Contains("--noclear")) {
            _argOffset++;
            _clearConsole = false;
        }
        
        bool isLoop = false;
        string MusicPath = args.Length > _argOffset ? args[_argOffset] : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        List<string> Files = new List<string> { };
        
        if (Directory.Exists(MusicPath)) {
            Files.AddRange(Directory.GetFiles(MusicPath, "*", SearchOption.AllDirectories).Where(a => Bass.SupportedFormats.Contains(Path.GetExtension(a))));
            Files.Shuffle();
        } else if (File.Exists(MusicPath)) {
            Files.Add(MusicPath);
            isLoop = true;
        }

        AudioClip AC = new AudioClip { };
        if (Files.Count == 0)
        {
            Console.WriteLine($"No music files were found in \"{MusicPath}\"");
            return;
        }
        new TaskFactory().StartNew(async ()
         => {
                // Connection was not successful, don't bother trying again
                if (!await rpc.Start()) return;
                while (!cts.IsCancellationRequested)
                {
                    await rpc.SetSong(AC, isLoop);
                    await Task.Delay(1000);
                }
                await rpc.Stop();
            }
        );
        
        for (var i = 0; i < Files.Count; i++)
        {
            AC.Open(Files[i]);
            AC.Play();

            while (AC.ClipStatus != PlaybackState.Stopped)
            {
                string playbackPrefix = "Now playing: ";
                string playbackName = new string(AC.FileName.Take(Console.WindowWidth - playbackPrefix.Length).ToArray());
                string playbackProgress = $" [{Math.Round(AC.ClipPosition * 10) / 10 + "s", -5}/{Math.Round(AC.ClipLength * 10) / 10 + "s", -5}]";

                refreshCounter = ++refreshCounter % 25;
                if (refreshCounter == 0 && _clearConsole) Console.Clear();
            
                Console.SetCursorPosition(0, 0);
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(BarGraph(AC.ClipPosition * 1000, AC.ClipLength * 1000, Console.WindowWidth));
                Console.ResetColor();
                Console.Write(playbackPrefix);
                Console.Write(playbackName);
                try {
                    Console.Write(new string(' ', Console.WindowWidth - playbackPrefix.Length - playbackName.Length - playbackProgress.Length));
                } catch { }
                Console.WriteLine(playbackProgress);
                
                if (isLoop) {
                    if (DateTime.Now.Second % 2 < 1) Console.ForegroundColor = ConsoleColor.White;
                    else Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("LOOP");
                    // If the token got cancelled, exit
                    if (cts.IsCancellationRequested) break;
                    continue;
                }
                
                if (Files.Count > 1 + i)
                    Console.WriteLine(new string(' ', Console.WindowWidth));
                    Console.WriteLine("Queue:" + new string(' ', Console.WindowWidth - 6));
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
                    string suffix = $"[{Path.GetExtension(Files[index]).ToUpper()}]";
                    string shortenedName = new string(name.Take(Console.WindowWidth - prefix.Length - suffix.Length).ToArray());
                    
                    Console.Write(prefix);
                    Console.ResetColor();
                    
                    Console.Write(shortenedName);
                    try {
                        Console.Write(new string(' ', Console.WindowWidth - prefix.Length - shortenedName.Length - suffix.Length));
                    } catch { }
                    Console.CursorLeft = Console.WindowWidth - suffix.Length;
                    Console.WriteLine(suffix);
                    Console.ResetColor();

                    /*// a 30ms window in bytes to be filled with sample data
                    int length = (int)Bass.ChannelSeconds2Bytes(AC.Handle, 0.03);

                    // first we need a mananged object where the sample data should be placed
                    // length is in bytes, so the number of floats to process is length/4 
                    float[] data = new float[length/4];

                    // get the sample data
                    length = Bass.ChannelGetData(AC.Handle, data, length);

                    FftSharp.Complex[] complex;
                    double[] fft = Array.ConvertAll<float, double>(data, x
                     => (double)x);
                    
                    complex = FftSharp.Complex.FromReal(fft);

                    FftSharp.Transform.IFFT(complex);

                    Array.ForEach(complex, x
                     => Console.WriteLine(x.Magnitude));

                    // It's something 
                    */
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
                
                // If the token got cancelled, exit
                if (cts.IsCancellationRequested) break;
                
                Thread.Sleep(50);
            }
            
            AC.Close();

            // If the token got cancelled, exit
            if (cts.IsCancellationRequested) break;

            // To make it loop
            if (isLoop) i--;
        }
        
        // Free up memory
        Bass.Stop();
        Bass.Free();
        
        Console.WriteLine("Exit");

        Console.CursorVisible = true;
    }
    static void HandleSIGINT(object source, ConsoleCancelEventArgs e) {
        e.Cancel = true;
        cts.Cancel();
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