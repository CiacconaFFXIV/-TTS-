using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace 渔人的直感.Models
{
    public static class DebugLog
    {
        private static readonly object Sync = new object();
        private static TextWriterTraceListener _fileListener;
        private static string _logFilePath;

        public static string LogFilePath => _logFilePath;

        public static void Initialize()
        {
#if DEBUG
            lock (Sync)
            {
                if (_fileListener != null)
                    return;

                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(logsDir);

                _logFilePath = Path.Combine(
                    logsDir,
                    $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                _fileListener = new TextWriterTraceListener(_logFilePath, "DebugLogFile")
                {
                    TraceOutputOptions = TraceOptions.DateTime
                };

                Debug.Listeners.Add(_fileListener);
                Debug.AutoFlush = true;

                WriteHeader();
            }
#endif
        }

        public static void Shutdown()
        {
#if DEBUG
            lock (Sync)
            {
                if (_fileListener == null)
                    return;

                Write("Application shutdown.");
                Debug.Flush();
                Debug.Listeners.Remove(_fileListener);
                _fileListener.Close();
                _fileListener.Dispose();
                _fileListener = null;
            }
#endif
        }

        public static void Write(string message)
        {
#if DEBUG
            if (string.IsNullOrEmpty(message))
                return;

            Debug.WriteLine(FormatLine("INFO", message));
#endif
        }

        public static void Exception(Exception exception, string context = null)
        {
#if DEBUG
            if (exception == null)
                return;

            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(context))
                builder.Append('[').Append(context).Append("] ");

            builder.Append(exception.GetType().Name).Append(": ").Append(exception.Message);
            Debug.WriteLine(FormatLine("ERROR", builder.ToString()));
            Debug.WriteLine(exception.ToString());
            Debug.Flush();
#endif
        }

        private static void WriteHeader()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var builder = new StringBuilder();
            builder.AppendLine("========== 渔人的直感 Debug Log ==========");
            builder.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            builder.AppendLine($"Version: {version}");
            builder.AppendLine($"OS: {Environment.OSVersion}");
            builder.AppendLine($".NET: {Environment.Version}");
            builder.AppendLine($"64-bit process: {Environment.Is64BitProcess}");
            builder.AppendLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            builder.AppendLine($"LogFile: {_logFilePath}");
            builder.AppendLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
            builder.AppendLine("==========================================");

            Debug.WriteLine(builder.ToString());
            Debug.Flush();
        }

        private static string FormatLine(string level, string message) =>
            $"[{level}] {message}";
    }
}
