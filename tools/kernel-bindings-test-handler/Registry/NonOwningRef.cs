namespace BitcoinKernel.TestHandler.Registry;

/// <summary>
/// Wraps any object in a no-op IDisposable so it can live in the ObjectRegistry.
/// Use this for objects whose lifetime is managed elsewhere (e.g. Chain, BlockIndex).
/// </summary>
public sealed class NonOwningRef<T> : IDisposable
{
    public T Value { get; }

    public NonOwningRef(T value)
    {
        Value = value;
    }

    public void Dispose()
    {
        // Intentionally empty – we do not own the underlying object.
    }
}
