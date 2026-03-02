using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NotifyBlockTip(
    IntPtr user_data,
    IntPtr block_index);

