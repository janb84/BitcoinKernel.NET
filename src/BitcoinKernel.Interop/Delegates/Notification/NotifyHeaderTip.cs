using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NotifyHeaderTip(
    IntPtr user_data,
    IntPtr block_index,
    long timestamp);