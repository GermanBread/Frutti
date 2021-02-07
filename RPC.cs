// System
using System;
using System.Threading;
using System.Threading.Tasks;

// DiscordRPC
using DiscordRPC;
using DiscordRPC.Logging;

namespace UnsignedFramework
{
    public class RPC
    {
        private DiscordRpcClient client;
        private CancellationTokenSource tokenSource;

        public RPC() {
            tokenSource = new CancellationTokenSource();
        }
        
        public async Task Start() {
            client = new DiscordRpcClient("808109065930407958");

            //Set the logger
	        client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            client.Initialize();
            
            await Task.Delay(0);
        }
        public async Task Stop() {
            tokenSource.Cancel();
            client.Deinitialize();
            client.Dispose();

            await Task.Delay(0);
        }
        public async Task SetSong(AudioClip clip) {
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = $"Playing: {clip.FileName}",
                Timestamps = new Timestamps {
                    Start = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, (int)Math.Round(clip.ClipPosition))),
                    End = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, (int)Math.Round(clip.ClipPosition - clip.ClipLength)))
                },
                Secrets = null
            });
            await Task.Delay(0);
        }
    }
}