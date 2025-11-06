using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Delegates;

/// <summary>
/// Function signature for freeing user data.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void DestroyCallback(IntPtr user_data);
