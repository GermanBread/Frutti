using System.Linq;
using System.Threading;
using System.IO;
using System;
using ManagedBass;

namespace FruttiReborn {
    public class Audio {
        public int Handle { get; private set; }
        public string FilePath { get; private set; }
        public string FileName { get {
            return Path.GetFileName(FilePath);
        } }
        public double ClipPosition { get {
            long _bytePosition = Bass.ChannelGetPosition(Handle);
            if (_bytePosition == -1)
                return 0;
            double _secondsPosition = Bass.ChannelBytes2Seconds(Handle, _bytePosition);
            if (_secondsPosition < 0)
                return 0;
            return _secondsPosition;
        } set {
            long _bytePosition = Bass.ChannelSeconds2Bytes(Handle, value);
            if (_bytePosition == -1)
                throw new BassException { };
            if (!Bass.ChannelSetPosition(Handle, _bytePosition))
                throw new BassException { };
        } }
        public double ClipLength { get {
            long _bytePosition = Bass.ChannelGetLength(Handle);
            if (_bytePosition == -1)
                return 1;
            double _secondsPosition = Bass.ChannelBytes2Seconds(Handle, _bytePosition);
            if (_secondsPosition < 0)
                return 1;
            return _secondsPosition;
        } }
        public PlaybackState ClipStatus { get {
            return Bass.ChannelIsActive(Handle);
        } }
        public double[] Waveform { get {
            int[] _data = new int[256 * 2];
            _ = Bass.ChannelGetData(Handle, _data, _data.Length);

            //double _low = _data.Aggregate((val, agg) => val < agg ? val : agg);
            double _peak = _data.Aggregate((val, agg) => val > agg ? val : agg);

            return Array.ConvertAll<int, double>(_data[..256], x
             => {
                 double _output = Math.Clamp(Math.Abs((double)x / _peak), 0, 1);
                 return double.IsNaN(_output) ? 0 : _output;
             }
             );
        } }
        public Audio() {
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
            if (Handle != 0)
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