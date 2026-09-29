using System.Runtime.InteropServices;
using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel.Primitives;

/// <summary>
/// Represents the witness stack of a transaction input.
/// </summary>
public sealed class WitnessStack : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    internal WitnessStack(IntPtr handle, bool ownsHandle = true)
    {
        _handle = handle != IntPtr.Zero
            ? handle
            : throw new ArgumentException("Invalid witness stack handle", nameof(handle));
        _ownsHandle = ownsHandle;
    }

    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Gets the number of items on the witness stack.
    /// </summary>
    public int Count
    {
        get
        {
            ThrowIfDisposed();
            return (int)NativeMethods.WitnessStackCountItems(_handle);
        }
    }

    /// <summary>
    /// Gets the raw bytes of the witness stack item at the specified index.
    /// </summary>
    public byte[] GetItem(int index)
    {
        ThrowIfDisposed();
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var bytes = new List<byte>();
        NativeMethods.WriteBytes writer = (data, len, _) =>
        {
            // Empty items (e.g. the CHECKMULTISIG dummy) arrive as a null pointer with len 0.
            if (len == 0)
                return 0;

            var buffer = new byte[len];
            Marshal.Copy(data, buffer, 0, (int)len);
            bytes.AddRange(buffer);
            return 0;
        };

        int result = NativeMethods.WitnessStackGetItemAt(_handle, (nuint)index, writer, IntPtr.Zero);
        if (result != 0)
            throw new TransactionException($"Failed to serialize witness stack item at index {index}");

        return bytes.ToArray();
    }

    /// <summary>
    /// Enumerates the raw bytes of every item on the witness stack, bottom to top.
    /// </summary>
    public IEnumerable<byte[]> EnumerateItems()
    {
        ThrowIfDisposed();

        int count = Count;
        for (int i = 0; i < count; i++)
        {
            yield return GetItem(i);
        }
    }

    /// <summary>
    /// Creates an owned copy of this witness stack.
    /// </summary>
    public WitnessStack Copy()
    {
        ThrowIfDisposed();
        var copy = NativeMethods.WitnessStackCopy(_handle);
        if (copy == IntPtr.Zero)
            throw new TransactionException("Failed to copy witness stack");

        return new WitnessStack(copy);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WitnessStack));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero && _ownsHandle)
            {
                NativeMethods.WitnessStackDestroy(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~WitnessStack()
    {
        if (_ownsHandle)
            Dispose();
    }
}
