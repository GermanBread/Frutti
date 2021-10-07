// System
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FruttiReborn {
    public static class ListUtilities {
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = new Random().Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;
            }  
        }
    }
    public static partial class Entry {
        static async Task spinner(CancellationToken ct) {
            string _glyphs = "-\\|/";
            int _counter = 0;
            window.Move(window.CursorPosition.MoveRight().MoveRight());
            while (!ct.IsCancellationRequested) {
                _counter++;
                _counter %= _glyphs.Length;

                window.Move(window.CursorPosition.MoveLeft());
                window.AddCharacter(_glyphs[_counter]);
                
                updater.Update(window);
                
                try {
                    await Task.Delay(100, ct);
                } catch { }
            }
            window.Move(window.CursorPosition.MoveLeft());
            window.AddString("Done");
            updater.Update(window);
        }
        static void quit() {
            windowToken.Cancel();
            
            window.Move(new(0, 0));
            window.ClearLine();
            window.AddString("Stopping...");
            updater.Update(window);

            driver.Close();
            // Somehow causes an unhandled exception when Control+C
            //if (audio.Handle != 0)
            //    audio.Close();
            
            Environment.Exit(0);
            Console.WriteLine("Exit");
        }
    }
}