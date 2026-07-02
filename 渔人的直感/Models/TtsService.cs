using System;
using System.Diagnostics;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace 渔人的直感.Models
{
    /// <summary>
    /// Windows 自带 TTS，咬钩时与杆型音频协调播放。
    /// </summary>
    public static class TtsService
    {
        private static SpeechSynthesizer _synthesizer;
        private static int _promptDurationMs = 800;
        private static int _biteSoundDurationMs = 600;
        private static readonly object Lock = new object();

        public static int PromptDurationMs => _promptDurationMs;
        public static int BiteSoundDurationMs => _biteSoundDurationMs;

        public static void Initialize()
        {
            lock (Lock)
            {
                _synthesizer?.Dispose();
                _synthesizer = new SpeechSynthesizer();
                _promptDurationMs = MeasurePromptDuration();
                _biteSoundDurationMs = MeasureWavDurationMs("中杆.wav");
            }
        }

        internal static void PlayBite(TugType tug, Action playBiteSound)
        {
            if (!Properties.Settings.Default.TtsEnabled)
            {
                playBiteSound?.Invoke();
                return;
            }

            var text = GetPrompt(tug);
            if (string.IsNullOrEmpty(text))
            {
                playBiteSound?.Invoke();
                return;
            }

            var offsetMs = (int)Properties.Settings.Default.TtsTimingOffsetMs;
            var minOffset = -_promptDurationMs;
            var maxOffset = _biteSoundDurationMs;
            if (offsetMs < minOffset) offsetMs = minOffset;
            if (offsetMs > maxOffset) offsetMs = maxOffset;

            Task.Run(() =>
            {
                try
                {
                    if (offsetMs <= 0)
                    {
                        Speak(text);
                        if (offsetMs < 0)
                            Thread.Sleep(-offsetMs);
                        playBiteSound?.Invoke();
                    }
                    else
                    {
                        playBiteSound?.Invoke();
                        Thread.Sleep(offsetMs);
                        Speak(text);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[TTS] 播放失败: {ex.Message}");
                    playBiteSound?.Invoke();
                }
            });
        }

        private static void Speak(string text)
        {
            lock (Lock)
            {
                _synthesizer?.Speak(text);
            }
        }

        private static string GetPrompt(TugType tug)
        {
            switch (tug)
            {
                case TugType.Light: return "轻竿";
                case TugType.Medium: return "中竿";
                case TugType.Heavy: return "鱼王竿";
                default: return null;
            }
        }

        private static int MeasurePromptDuration()
        {
            try
            {
                using (var synth = new SpeechSynthesizer())
                {
                    synth.SetOutputToNull();
                    var sw = Stopwatch.StartNew();
                    synth.Speak("中竿");
                    return Math.Max(300, (int)sw.ElapsedMilliseconds);
                }
            }
            catch
            {
                return 800;
            }
        }

        private static int MeasureWavDurationMs(string fileName)
        {
            try
            {
                var path = ResolveWavPath(fileName);
                if (!File.Exists(path))
                    return 600;

                using (var stream = File.OpenRead(path))
                using (var reader = new BinaryReader(stream))
                {
                    if (new string(reader.ReadChars(4)) != "RIFF")
                        return 600;

                    reader.ReadInt32();
                    if (new string(reader.ReadChars(4)) != "WAVE")
                        return 600;

                    var byteRate = 0;
                    var dataSize = 0;

                    while (stream.Position + 8 <= stream.Length)
                    {
                        var chunkId = new string(reader.ReadChars(4));
                        var chunkSize = reader.ReadInt32();

                        if (chunkId == "fmt ")
                        {
                            reader.ReadInt16();
                            reader.ReadInt16();
                            reader.ReadInt32();
                            byteRate = reader.ReadInt32();
                            if (chunkSize > 16)
                                reader.ReadBytes(chunkSize - 16);
                        }
                        else if (chunkId == "data")
                        {
                            dataSize = chunkSize;
                            break;
                        }
                        else
                        {
                            reader.ReadBytes(chunkSize);
                        }
                    }

                    if (byteRate <= 0 || dataSize <= 0)
                        return 600;

                    return Math.Max(100, (int)Math.Ceiling(dataSize * 1000.0 / byteRate));
                }
            }
            catch
            {
                return 600;
            }
        }

        private static string ResolveWavPath(string fileName)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var inWavFolder = Path.Combine(baseDir, "Wav", fileName);
            if (File.Exists(inWavFolder))
                return inWavFolder;

            return Path.Combine(baseDir, fileName);
        }
    }
}
