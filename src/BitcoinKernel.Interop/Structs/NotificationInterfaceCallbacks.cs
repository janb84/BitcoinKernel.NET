using System.Runtime.InteropServices;
using BitcoinKernel.Interop.Delegates.Notification;

namespace BitcoinKernel.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NotificationInterfaceCallbacks
    {
        public IntPtr UserData;
        public NotifyBlockTip BlockTip;
        public NotifyHeaderTip HeaderTip;
        public NotifyProgress Progress;
        public NotifyWarningSet WarningSet;
        public NotifyWarningUnset WarningUnset;
        public NotifyFlushError FlushError;
        public NotifyFatalError FatalError;
    }
}