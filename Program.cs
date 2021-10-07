using System.Diagnostics;
// System
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

// NCurses
using Netcurses;

// BASS
using ManagedBass;

namespace FruttiReborn {
    public static partial class Entry {
        static Entry() {
            rpc = new();
            audio = new();
            queue = new();
            driver = new();
            windowToken = new();
            windowNeedsResize = false;
            consoleSize = new(Console.WindowWidth, Console.WindowHeight);
            window = new(consoleSize);
            updater = new(driver, consoleSize);
            foregroundColour = ConsoleColor.White;
            backgroundColour = ConsoleColor.Black;
            alternateColour = ConsoleColor.DarkGray;

            Console.CancelKeyPress += (_, e) => {
                quit();
            };

            Task.Run(() => {
                int _consoleWidth = Console.WindowWidth, _consoleHeight = Console.WindowHeight;

                while (!windowToken.IsCancellationRequested) {
                     if (_consoleWidth != Console.WindowWidth || _consoleHeight != Console.WindowHeight) {
                        _consoleWidth = Console.WindowWidth;
                        _consoleHeight = Console.WindowHeight;
                        windowNeedsResize = true;
                     }
                }
            });
        }

        public static void Main(string[] Args) {
            var _args = CmdlineParser.ParseCmdline(Args);

            window.Foreground = foregroundColour;
            window.Background = backgroundColour;
            window.Clear();

            //Console.Clear();

            window.AddString("Loading files...");
            updater.Update(window);

            {
                CancellationTokenSource _cts = new();
                _ = spinner(_cts.Token);

                _args.Directories.ForEach(x => {
                    Array.ForEach(Directory.GetFiles(x, "*.*", SearchOption.AllDirectories), y => {
                        if (Bass.SupportedFormats.Contains(Path.GetExtension(y)))
                            queue.Add(y);
                    });
                });
                
                _args.Files.ForEach(x => {
                    if (Bass.SupportedFormats.Contains(Path.GetExtension(x)))
                        queue.Add(x);
                });

                queue.Shuffle();

                _cts.Cancel();
            }

            window.Move(new(0, 0));
            window.ClearLine();
            window.AddString("Starting RPC...");
            updater.Update(window);

            {
                _ = rpc.Start();
            }

            window.Move(new(0, 0));
            window.ClearLine();
            window.AddString("Playing music...");
            updater.Update(window);
            
            if (queue.Count > 0) {
                int _counter = 0;
                double _deltaTime = 1;
                Stopwatch _watch = new();
                _watch.Start();

                while (!windowToken.IsCancellationRequested) {
                    if (windowNeedsResize) {
                        Size _newconsoleSize = new(Console.WindowWidth, Console.WindowHeight);
                        window = new(_newconsoleSize);
                        window.Foreground = foregroundColour;
                        window.Background = backgroundColour;
                        window.Clear();
                        updater = new(driver, _newconsoleSize);
                        consoleSize = _newconsoleSize;
                        
                        windowNeedsResize = false;
                    }
                    window.Move(new(0, 0));

                    if (audio.ClipStatus == PlaybackState.Stopped) {
                        _counter++;
                        // Modulo is too inconsistent for me
                        if (_counter >= queue.Count)
                            _counter = 0;
                        try {
                            audio.Replace(queue[_counter]);
                            audio.Play();
                        } catch {
                            _counter++;
                        }
                        window.Clear();
                    }

                    switch (driver.ReadKey().Key) {
                        case ConsoleKey.LeftArrow:
                            audio.ClipPosition = Math.Max(audio.ClipPosition - 5, 0);
                            break;
                        case ConsoleKey.RightArrow:
                            audio.ClipPosition = Math.Min(audio.ClipPosition + 5, audio.ClipLength - .1);
                            break;
                        case ConsoleKey.S:
                            audio.Stop();
                            break;
                        case ConsoleKey.R:
                            _counter -= 2;
                            if (_counter < 0)
                                _counter += queue.Count;
                            audio.Stop();
                            break;
                        case ConsoleKey.P:
                        case ConsoleKey.Spacebar:
                            if (audio.ClipStatus == PlaybackState.Paused)
                                audio.Play();
                            else
                                audio.Pause();
                            break;
                        case ConsoleKey.Q:
                        case ConsoleKey.Escape:
                            quit();
                            break;
                    }
                    driver.Refresh();

                    {
                        var _soundPosition = TimeSpan.FromSeconds(audio.ClipPosition);
                        var _soundDuration = TimeSpan.FromSeconds(audio.ClipLength);

                        string _textLeft, _textMiddle, _textRight, _sepLM, _sepMR;

                        _textLeft = $"{Path.GetFileNameWithoutExtension(audio.FileName)}";
                        {
                            static string bakeString(TimeSpan span) {
                                string _prebaked = "";
                                if (span.Days > 0)
                                    _prebaked += $"{(int)Math.Floor(span.TotalDays)}d ";
                                if (span.Hours > 0)
                                    _prebaked += $"{span.Hours}h ";
                                if (span.Minutes > 0)
                                    _prebaked += $"{span.Minutes}m ";
                                _prebaked += $"{span.Seconds}s";
                                return _prebaked;
                            }
                            _textMiddle = $"({bakeString(_soundPosition)} | {bakeString(_soundDuration)})";
                        }
                        _textRight = $"[{Path.GetExtension(audio.FileName).ToUpper()}]";
                        {
                            double _fraction = Math.Max((consoleSize.Width - _textLeft.Length - _textRight.Length - _textMiddle.Length) / 2d, 0);
                            _sepLM  = new(' ', (int)Math.Floor(_fraction));
                            _sepMR = new(' ', (int)Math.Ceiling(_fraction));
                        }
                        {
                            int _limit = Math.Max(consoleSize.Width - _textMiddle.Length - _textRight.Length, 0);
                            if (_textLeft.Length > _limit)
                                _textLeft = _textLeft[.._limit];
                        }
                        string _joined = _textLeft + _sepLM + _textMiddle + _sepMR + _textRight;
                        int _clipPosition = (int)Math.Ceiling(audio.ClipPosition / audio.ClipLength * _joined.Length);
                        
                        window.Foreground = backgroundColour;
                        window.Background = audio.ClipStatus == PlaybackState.Paused ? alternateColour : foregroundColour;
                        window.AddString(_joined[.._clipPosition]);

                        window.Foreground = foregroundColour;
                        window.Background = audio.ClipStatus == PlaybackState.Paused ? alternateColour : backgroundColour;
                        window.AddString(_joined[_clipPosition..]);
                        
                        window.Background = backgroundColour;
                    }

                    if (queue.Count > 1) {
                        window.Move(new(2, 2));
                        for (int i = 1; i < queue.Count; i++) {
                            window.Foreground = alternateColour;
                            window.AddString($"{i,3} - ");
                            window.Foreground = foregroundColour;
                            window.AddString($"{Path.GetFileName(queue[(i + _counter) % queue.Count])}");
                            window.Move(new(2, window.CursorPosition.MoveDown().Y));
                        }
                    }

                    updater.Update(window);

                    _ = rpc.SetSong(audio, _args.Flags.HasFlag(FruttiFlags.LoopFile));

                    _deltaTime = _watch.Elapsed.TotalSeconds;
                    _watch.Restart();
                }
            }

            if (audio.Handle != 0)
                audio.Close();
            driver.Close();
            rpc.Stop().Wait();
            if (queue.Count == 0)
                Console.WriteLine("No music files found. BASS supports the following files: {0}", Bass.SupportedFormats);
        }
        
        static Size consoleSize;
        static ConsoleArea window;
        static readonly Audio audio;
        static bool windowNeedsResize;
        static readonly RPCClient rpc;
        static ConsoleUpdater updater;
        static readonly List<string> queue;
        static readonly ConsoleDriver driver;
        static readonly ConsoleColor foregroundColour;
        static readonly ConsoleColor backgroundColour;
        static readonly ConsoleColor alternateColour;
        static readonly CancellationTokenSource windowToken;
    }
}