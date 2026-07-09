using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace 渔人的直感.Models
{
    public sealed class BuffWatcher
    {
        public const int SlotCount = Data.StatusSlotCount;
        public const int SlotStride = Data.StatusSlotStride;

        private readonly PlayerBuff[] _previous = new PlayerBuff[SlotCount];
        private bool _hasSnapshot;

        public void Poll(SigScanner scanner, IntPtr statusArrayPtr)
        {
            if (scanner == null || statusArrayPtr == IntPtr.Zero)
                return;

            var current = ReadAll(scanner, statusArrayPtr);

            if (!_hasSnapshot)
            {
                LogSnapshot("初始 Buff 列表", current);
                CopyToPrevious(current);
                _hasSnapshot = true;
                return;
            }

            for (var i = 0; i < SlotCount; i++)
            {
                var prev = _previous[i];
                var now = current[i];

                if (prev.IsEmpty && now.IsEmpty)
                    continue;

                if (prev.IsEmpty && !now.IsEmpty)
                {
                    Debug.WriteLine($"[BuffWatch] + 新增 {now}");
                    continue;
                }

                if (!prev.IsEmpty && now.IsEmpty)
                {
                    Debug.WriteLine($"[BuffWatch] - 移除 {prev}");
                    continue;
                }

                if (prev.Id != now.Id)
                {
                    Debug.WriteLine($"[BuffWatch] ~ 槽位替换 {prev.Slot}: {prev} -> {now}");
                    continue;
                }

                if (prev.Stacks != now.Stacks ||
                    Math.Abs(prev.Duration - now.Duration) >= 1.0f ||
                    prev.Owner != now.Owner)
                {
                    Debug.WriteLine($"[BuffWatch] ~ 更新 {now} (原 Duration={prev.Duration:F1}s Stacks={prev.Stacks})");
                }
            }

            CopyToPrevious(current);
        }

        public void Reset()
        {
            _hasSnapshot = false;
            for (var i = 0; i < SlotCount; i++)
                _previous[i] = new PlayerBuff { Slot = i };
        }

        public static IList<PlayerBuff> ReadAll(SigScanner scanner, IntPtr statusArrayPtr)
        {
            var result = new List<PlayerBuff>(SlotCount);
            for (var i = 0; i < SlotCount; i++)
            {
                var offset = i * SlotStride;
                var buff = new PlayerBuff
                {
                    Slot = i,
                    Id = scanner.ReadInt16(statusArrayPtr + offset),
                    Stacks = scanner.ReadInt16(statusArrayPtr + offset + 2),
                    Duration = scanner.ReadFloat(statusArrayPtr + offset + 4),
                    Owner = scanner.ReadInt32(statusArrayPtr + offset + 8)
                };
                result.Add(buff);
            }

            return result;
        }

        public static bool HasBuff(SigScanner scanner, IntPtr statusArrayPtr, short buffId)
        {
            if (scanner == null || statusArrayPtr == IntPtr.Zero)
                return false;

            for (var i = 0; i < SlotCount; i++)
            {
                if (scanner.ReadInt16(statusArrayPtr + i * SlotStride) == buffId)
                    return true;
            }

            return false;
        }

        public static bool TryGetBuffRemaining(SigScanner scanner, IntPtr statusArrayPtr, short buffId, out float remaining)
        {
            remaining = 0f;
            if (scanner == null || statusArrayPtr == IntPtr.Zero)
                return false;

            for (var i = 0; i < SlotCount; i++)
            {
                var offset = i * SlotStride;
                if (scanner.ReadInt16(statusArrayPtr + offset) != buffId)
                    continue;

                remaining = scanner.ReadFloat(statusArrayPtr + offset + 4);
                return true;
            }

            return false;
        }

        private void CopyToPrevious(IList<PlayerBuff> current)
        {
            for (var i = 0; i < SlotCount; i++)
            {
                var buff = current[i];
                _previous[i] = new PlayerBuff
                {
                    Slot = buff.Slot,
                    Id = buff.Id,
                    Stacks = buff.Stacks,
                    Duration = buff.Duration,
                    Owner = buff.Owner
                };
            }
        }

        private static void LogSnapshot(string title, IList<PlayerBuff> buffs)
        {
            var builder = new StringBuilder();
            builder.Append("[BuffWatch] ").Append(title).Append(':');

            var count = 0;
            foreach (var buff in buffs)
            {
                if (buff.IsEmpty)
                    continue;

                builder.AppendLine();
                builder.Append("  ").Append(buff);
                count++;
            }

            if (count == 0)
                builder.Append(" (无 Buff)");

            Debug.WriteLine(builder.ToString());
        }
    }
}
