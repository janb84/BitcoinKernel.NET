using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates.Validation;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ValidationBlockChecked(
    IntPtr user_data,
    IntPtr block,
    IntPtr validation_state);