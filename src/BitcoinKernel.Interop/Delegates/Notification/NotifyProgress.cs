
using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Notification;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void NotifyProgress(
    IntPtr user_data,
    [MarshalAs(UnmanagedType.LPUTF8Str)] string title,
    int progress_percent,
    [MarshalAs(UnmanagedType.I1)] bool resume_possible);