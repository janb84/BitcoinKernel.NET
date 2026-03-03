using System;
using System.Collections.Generic;

namespace BitcoinKernel.TestHandler.Registry;

/// <summary>
/// Maintains a map of reference names to IDisposable native objects across requests.
/// </summary>
public sealed class ObjectRegistry : IDisposable
{
    private readonly Dictionary<string, IDisposable> _store = new();
    private bool _disposed;

    /// <summary>
    /// Registers an object under the given reference name.
    /// Any previously registered object under that name is disposed first.
    /// </summary>
    public void Register(string refName, IDisposable obj)
    {
        if (_store.TryGetValue(refName, out var existing))
            existing.Dispose();

        _store[refName] = obj;
    }

    /// <summary>
    /// Retrieves an object from the registry by reference name, cast to T.
    /// </summary>
    /// <exception cref="KeyNotFoundException">When the name is not registered.</exception>
    /// <exception cref="InvalidCastException">When the object is not of type T.</exception>
    public T Get<T>(string refName)
    {
        if (!_store.TryGetValue(refName, out var obj))
            throw new KeyNotFoundException($"Registry: reference '{refName}' not found.");

        return (T)obj;
    }

    /// <summary>
    /// Removes and disposes the object registered under the given reference name.
    /// </summary>
    public void Destroy(string refName)
    {
        if (_store.Remove(refName, out var obj))
            obj.Dispose();
    }

    /// <summary>
    /// Returns true when the given reference name is registered.
    /// </summary>
    public bool Contains(string refName) => _store.ContainsKey(refName);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var obj in _store.Values)
        {
            try { obj.Dispose(); }
            catch { /* best-effort cleanup */ }
        }

        _store.Clear();
    }
}
