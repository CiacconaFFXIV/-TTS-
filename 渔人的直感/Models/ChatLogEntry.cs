using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 从游戏内存解析出的单条聊天日志。
    /// </summary>
    public sealed class ChatLogEntry
    {
        private static readonly Regex SpecialPurposeUnicodeRegex =
            new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        private static readonly Regex NoPrintingCharactersRegex =
            new Regex(@"[\x00-\x1F]", RegexOptions.Compiled);

        private static readonly Regex ReplacementCharRegex =
            new Regex(@"[\uFFFD]", RegexOptions.Compiled);

        public DateTime Timestamp { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// 解析 RaptureLogModule 中 LogMessageData 的原始条目。
        /// 格式：4 字节时间戳 + 2 字节频道码 + 2 字节保留 + SeString 正文。
        /// </summary>
        public static ChatLogEntry Parse(byte[] raw)
        {
            if (raw == null || raw.Length < 8)
                return null;

            var entry = new ChatLogEntry();

            var timestampBytes = new[] { raw[3], raw[2], raw[1], raw[0] };
            var timestampHex = BitConverter.ToString(timestampBytes).Replace("-", string.Empty);
            if (int.TryParse(timestampHex, System.Globalization.NumberStyles.HexNumber, null, out var unixTime))
                entry.Timestamp = UnixTimeStampToDateTime(unixTime);

            entry.Code = $"{raw[5]:X2}{raw[4]:X2}";

            var textLength = raw.Length - 8;
            if (textLength <= 0)
            {
                entry.Message = string.Empty;
                return entry;
            }

            var textBytes = new byte[textLength];
            Buffer.BlockCopy(raw, 8, textBytes, 0, textLength);
            entry.Message = CleanMessage(textBytes);
            return entry;
        }

        /// <summary>
        /// 剥离 FF14 SeString 控制字节后按 UTF-8 解码。
        /// </summary>
        private static string CleanMessage(byte[] bytes)
        {
            var cleaned = new List<byte>(bytes.Length);

            for (var i = 0; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case 2:
                        if (i + 2 >= bytes.Length)
                            break;

                        var length = bytes[i + 2];
                        if (length > 1)
                        {
                            // 道具图标/Sheet 链接等长控制块：整段跳过，避免二进制字节破坏 UTF-8
                            i += 3 + (length - 1);
                        }
                        else if (i + 4 < bytes.Length)
                        {
                            i += 4;
                            cleaned.Add(32);
                            cleaned.Add(bytes[i]);
                        }
                        break;

                    case 31:
                        cleaned.Add((byte)':');
                        break;

                    default:
                        cleaned.Add(bytes[i]);
                        break;
                }
            }

            var text = Encoding.UTF8.GetString(cleaned.ToArray());
            text = SpecialPurposeUnicodeRegex.Replace(text, string.Empty);
            text = ReplacementCharRegex.Replace(text, string.Empty);
            text = NoPrintingCharactersRegex.Replace(text, string.Empty);
            text = text.TrimStart(':', ' ');
            return text.Replace("\0", string.Empty).Trim();
        }

        private static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(unixTimeStamp)
                .ToLocalTime();
        }
    }
}
