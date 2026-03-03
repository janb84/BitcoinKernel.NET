using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates;

/// <summary>
/// Function signature for freeing user data.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void DestroyCallback(IntPtr user_data);
