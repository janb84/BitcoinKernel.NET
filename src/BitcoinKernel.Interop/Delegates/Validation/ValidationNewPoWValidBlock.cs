using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Validation;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ValidationNewPoWValidBlock(
    IntPtr user_data,
    IntPtr block_index,
    IntPtr block);