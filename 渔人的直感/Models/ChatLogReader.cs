using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace 渔人的直感.Models
{
    public sealed class ChatLogReader
    {
        private const int LogMessageIndexOffset = 0x48;
        private const int LogMessageDataOffset = 0x60;
        private const int MaxIndexBufferBytes = 256 * 1024;

        private static readonly HashSet<byte> SelfGatheringLogKinds = new HashSet<byte>
        {
            0x3B, // 采集
            0x43  // 收藏品采集 / 钓鱼
        };

        private SigScanner _scanner;
        private IntPtr _raptureLogModule;
        private int _previousArrayIndex;
        private int _previousOffset;
        private bool _firstRun = true;
        private readonly List<int> _indexBuffer = new List<int>();

        public event Action<ChatLogEntry> MessageReceived;

        public bool IsInitialized => _raptureLogModule != IntPtr.Zero;

        public void Initialize(SigScanner scanner)
        {
            _scanner = scanner;
            _raptureLogModule = ChatLogData.ResolveRaptureLogModule(scanner);

            if (_raptureLogModule == IntPtr.Zero)
            {
                Debug.WriteLine("[ChatLog] 无法定位 RaptureLogModule，聊天抓取未启用。");
                return;
            }

            Debug.WriteLine($"[ChatLog] RaptureLogModule @ {_raptureLogModule.ToInt64():X}");
        }

        public void Poll()
        {
            if (!IsInitialized)
                return;

            try
            {
                var indexArrayStart = _scanner.ReadIntPtr(_raptureLogModule, LogMessageIndexOffset);
                var indexArrayPos = _scanner.ReadIntPtr(_raptureLogModule, LogMessageIndexOffset + 8);
                var dataBufferStart = _scanner.ReadIntPtr(_raptureLogModule, LogMessageDataOffset);

                if (indexArrayStart == IntPtr.Zero || indexArrayPos == IntPtr.Zero || dataBufferStart == IntPtr.Zero)
                    return;

                var currentArrayIndex = (int)((indexArrayPos.ToInt64() - indexArrayStart.ToInt64()) / 4);
                if (currentArrayIndex <= 0)
                    return;

                RefreshIndexBuffer(indexArrayStart, currentArrayIndex);

                if (_firstRun)
                {
                    _firstRun = false;
                    _previousArrayIndex = currentArrayIndex - 1;
                    _previousOffset = _indexBuffer[currentArrayIndex - 1];
                    return;
                }

                if (currentArrayIndex < _previousArrayIndex)
                {
                    ProcessRange(_previousArrayIndex, _indexBuffer.Count, dataBufferStart);
                    _previousOffset = 0;
                    _previousArrayIndex = 0;
                }

                if (_previousArrayIndex < currentArrayIndex)
                    ProcessRange(_previousArrayIndex, currentArrayIndex, dataBufferStart);

                _previousArrayIndex = currentArrayIndex;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ChatLog] 读取失败: {ex.Message}");
            }
        }

        private void RefreshIndexBuffer(IntPtr indexArrayStart, int requiredCount)
        {
            _indexBuffer.Clear();
            var bytesToRead = Math.Min(Math.Max(requiredCount * 4L, 4), MaxIndexBufferBytes);
            var buffer = _scanner.ReadBytes(indexArrayStart, (uint)bytesToRead);

            for (var i = 0; i + 4 <= buffer.Length; i += 4)
                _indexBuffer.Add(BitConverter.ToInt32(buffer, i));
        }

        private void ProcessRange(int fromIndex, int toIndex, IntPtr dataBufferStart)
        {
            for (var i = fromIndex; i < toIndex; i++)
            {
                if (i >= _indexBuffer.Count)
                    break;

                var currentOffset = _indexBuffer[i];
                var raw = ReadEntry(dataBufferStart, _previousOffset, currentOffset);
                _previousOffset = currentOffset;

                if (raw == null || raw.Length < 6 || !IsSelfGatheringMessage(raw))
                    continue;

                var entry = ChatLogEntry.Parse(raw);
                if (entry == null || string.IsNullOrWhiteSpace(entry.Message))
                    continue;

                RaiseMessage(entry);
            }
        }

        private static bool IsSelfGatheringMessage(byte[] raw)
        {
            var logInfo = (ushort)(raw[4] | (raw[5] << 8));
            var logKind = (byte)(logInfo & 0x7F);
            var sourceKind = (logInfo >> 11) & 0xF;
            return sourceKind == 1 && SelfGatheringLogKinds.Contains(logKind);
        }

        private byte[] ReadEntry(IntPtr dataBufferStart, int fromOffset, int toOffset)
        {
            var size = toOffset - fromOffset;
            if (size <= 0)
                return Array.Empty<byte>();

            return _scanner.ReadBytes(dataBufferStart + fromOffset, (uint)size);
        }

        private static void LogEntry(ChatLogEntry entry)
        {
            var timeText = entry.Timestamp == default ? "--:--:--" : entry.Timestamp.ToString("HH:mm:ss");
            Debug.WriteLine($"[ChatLog][{timeText}][{entry.Code}] {entry.Message}");
        }

        private void RaiseMessage(ChatLogEntry entry)
        {
            LogEntry(entry);
            MessageReceived?.Invoke(entry);
        }
    }
}
