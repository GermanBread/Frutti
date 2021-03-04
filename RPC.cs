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

        public RPC() { }
        
        public async Task<bool> Start() {
            client = new DiscordRpcClient("808109065930407958");

            // Supress everything
	        client.Logger = new ConsoleLogger() { Level = LogLevel.None };

            int retryCount = 0;
            while (!client.Initialize()) {
                await Task.Delay(500);
                if (++retryCount > 5)
                    return false;
            }
            
            return true;
        }
        public async Task Stop() {
            client.Deinitialize();
            client.Dispose();

            await Task.Delay(0);
        }
        public async Task SetSong(AudioClip clip) {
            // Call this as many times as you want and anywhere in your code.
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