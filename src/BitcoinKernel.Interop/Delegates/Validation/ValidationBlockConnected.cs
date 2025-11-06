using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Validation;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ValidationBlockConnected(
    IntPtr user_data,
    IntPtr block_index,
    IntPtr block);