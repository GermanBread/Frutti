// System
using System;
using System.Threading.Tasks;

// DiscordRPC
using DiscordRPC;
using DiscordRPC.Logging;

namespace FruttiReborn {
    public class RPCClient {
        private DiscordRpcClient client;

        public async Task<bool> Start() {
            // If you abuse this I'm going to kill you :)
            client = new("808109065930407958");

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
        public async Task SetSong(Audio clip, bool isLoop) {
            // Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence() {
                Details = $"Playing: {clip.FileName}" + (isLoop ? "on repeat" : ""),
                Timestamps = !isLoop ? new Timestamps {
                    Start = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, (int)Math.Round(clip.ClipPosition))),
                    End = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, (int)Math.Round(clip.ClipPosition - clip.ClipLength)))
                } : null,
                Secrets = null
            });
            await Task.Delay(0);
        }
    }
}