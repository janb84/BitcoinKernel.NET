using System.Runtime.InteropServices;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void NotifyWarningSet(
    IntPtr user_data,
    Warning warning);