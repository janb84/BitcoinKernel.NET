using System.Runtime.InteropServices;
using BitcoinKernel.Interop.Enums;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void NotifyWarningUnset(
    IntPtr user_data,
    Warning warning);
