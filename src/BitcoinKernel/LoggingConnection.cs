using System.Runtime.InteropServices;
using BitcoinKernel.Interop;
using BitcoinKernel.Interop.Delegates;

public sealed class LoggingConnection : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly LoggingCallback _callback;
    private readonly Action<string, string, int> _managedCallback;
    private GCHandle _gcHandle;

    public LoggingConnection(Action<string, string, int> callback)
    {
        _managedCallback = callback ?? throw new ArgumentNullException(nameof(callback));

        // Keep callback alive for native code
        _callback = (userData, messagePtr, messageLen) =>
        {
            if (messagePtr != IntPtr.Zero && messageLen > 0)
            {
                var message = Marshal.PtrToStringUTF8(messagePtr, (int)messageLen);
                if (message != null)
                {
                    // TODO: Parse category and level from message if needed
                    _managedCallback("kernel", message, 2); // Default to INFO level
                }
            }
        };

        _gcHandle = GCHandle.Alloc(_callback);
        _handle = NativeMethods.LoggingConnectionCreate(_callback, IntPtr.Zero, null);

        if (_handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create logging connection");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.LoggingConnectionDestroy(_handle);
                _handle = IntPtr.Zero;
            }

            if (_gcHandle.IsAllocated)
                _gcHandle.Free();

            _disposed = true;
        }
    }

    ~LoggingConnection() => Dispose();
}
