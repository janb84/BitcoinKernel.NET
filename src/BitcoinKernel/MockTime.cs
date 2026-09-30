using BitcoinKernel.Exceptions;
using BitcoinKernel.Interop;

namespace BitcoinKernel;

/// <summary>
/// Overrides the clock the kernel reads, for tests. The override is process-wide
/// and affects every context, so gate its use (for example to regtest) yourself.
/// </summary>
public static class MockTime
{
    /// <summary>
    /// Fixes the kernel clock at the given Unix timestamp in seconds.
    /// </summary>
    /// <param name="unixSeconds">Between 1 and 4294967295, the largest block header timestamp.</param>
    public static void Set(long unixSeconds)
    {
        if (unixSeconds < 1 || unixSeconds > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(unixSeconds), $"Mock time must be between 1 and {uint.MaxValue}");

        Apply(unixSeconds);
    }

    /// <summary>
    /// Fixes the kernel clock at the given time, truncated to whole seconds.
    /// </summary>
    public static void Set(DateTimeOffset time) => Set(time.ToUnixTimeSeconds());

    /// <summary>
    /// Restores the system clock.
    /// </summary>
    public static void Reset() => Apply(0);

    private static void Apply(long unixSeconds)
    {
        int result = NativeMethods.SetMockTime(unixSeconds);
        if (result != 0)
            throw new KernelException($"Failed to set mock time to {unixSeconds} (error code: {result})");
    }
}
