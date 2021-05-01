using System.Linq;
using System.Threading;
using System.IO;
using System;
using ManagedBass;

namespace UnsignedFramework
{
    public class AudioClip {
        public int Handle { get; private set; }
        public string FilePath { get; private set; }
        public string FileName { get {
            return Path.GetFileName(FilePath);
        } }
        public double ClipPosition { get {
            long BytePosition = Bass.ChannelGetPosition(Handle);
            if (BytePosition == -1)
                throw new BassException { };
            double SecondsPosition = Bass.ChannelBytes2Seconds(Handle, BytePosition);
            if (SecondsPosition < 0)
                throw new BassException { };
            return SecondsPosition;
        } set {
            long BytePosition = Bass.ChannelSeconds2Bytes(Handle, value);
            if (BytePosition == -1)
                throw new BassException { };
            if (!Bass.ChannelSetPosition(Handle, BytePosition))
                throw new BassException { };
        } }
        public double ClipLength { get {
            long BytePosition = Bass.ChannelGetLength(Handle);
            if (BytePosition == -1)
                throw new BassException { };
            double SecondsPosition = Bass.ChannelBytes2Seconds(Handle, BytePosition);
            if (SecondsPosition < 0)
                throw new BassException { };
            return SecondsPosition;
        } }
        public PlaybackState ClipStatus { get {
            return Bass.ChannelIsActive(Handle);
        } }
        public AudioClip() {
            if (Bass.Init())
                return;
            else
                throw new BassException { };
        }
        public void Open(string File) {
            Handle = Bass.CreateStream(File);
            if (Handle == 0)
                throw new BassException { };
            FilePath = Path.GetFullPath(File);
        }
        public void Replace(string File) {
            Close();
            Open(File);
        }
        public void Play() {
            if (!Bass.ChannelPlay(Handle))
                throw new BassException { };
        }
        public void Pause() {
            if (!Bass.ChannelPause(Handle))
                throw new BassException { };
        }
        public void Stop() {
            if (!Bass.ChannelStop(Handle))
                throw new BassException { };
        }
        public void Close() {
            if (!Bass.StreamFree(Handle))
                throw new BassException { };
        }
    }
}