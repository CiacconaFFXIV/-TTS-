using System;
using System.Diagnostics;

namespace 渔人的直感.Models
{
    /// <summary>
    /// 聊天日志模块地址解析。
    /// 偏移参考 FFXIVClientStructs：Framework.UIModule @ +0x2B68，UIModule.RaptureLogModule @ +0x1AC0。
    /// </summary>
    internal static class ChatLogData
    {
        private const int FrameworkUiModuleOffset = 0x2B68;
        private const int UIModuleRaptureLogModuleOffset = 0x1AC0;

        public static IntPtr ResolveRaptureLogModule(SigScanner scanner)
        {
            var frameworkPtrAddress = scanner.GetStaticAddressFromSig("48 8B 1D ?? ?? ?? ?? 8B 7C 24", 3);
            if (frameworkPtrAddress == IntPtr.Zero)
            {
                Debug.WriteLine("[ChatLog] Framework 特征码未找到。");
                return IntPtr.Zero;
            }

            var framework = scanner.ReadIntPtr(frameworkPtrAddress);
            if (framework == IntPtr.Zero)
            {
                Debug.WriteLine("[ChatLog] Framework 指针无效。");
                return IntPtr.Zero;
            }

            var uiModule = scanner.ReadIntPtr(framework + FrameworkUiModuleOffset);
            if (uiModule == IntPtr.Zero)
            {
                Debug.WriteLine("[ChatLog] UIModule 指针无效。");
                return IntPtr.Zero;
            }

            return uiModule + UIModuleRaptureLogModuleOffset;
        }
    }
}
