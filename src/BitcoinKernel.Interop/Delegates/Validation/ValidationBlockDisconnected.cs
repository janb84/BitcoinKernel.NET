using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Validation;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void ValidationBlockDisconnected(
    IntPtr user_data,
    IntPtr block);