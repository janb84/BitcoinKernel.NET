using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void NotifyFatalError(
    IntPtr user_data,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string error_message);