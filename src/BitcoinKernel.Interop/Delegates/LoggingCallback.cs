using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates;

/// <summary>
/// Function signature for the global logging callback. All bitcoin kernel
/// internal logs will pass through this callback.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void LoggingCallback(
    IntPtr user_data,
    IntPtr message,
    nuint message_len);