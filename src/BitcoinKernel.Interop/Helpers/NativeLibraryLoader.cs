
using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Helpers;

/// <summary>
/// Helper class for loading the native Bitcoin Kernel library.
/// Ensures the library is loaded only once in a thread-safe manner.
/// First attempt is made to load from known paths (e.g. LD_LIBRARY_PATH)
/// then falls back to package-supplied library's.
/// </summary>
static class NativeLibraryLoader
{
    private static bool _loaded = false;
    private static readonly ReaderWriterLockSlim _lock = new();
    public static void EnsureLoaded()
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_loaded) return;

            _lock.EnterWriteLock();
            try
            {
                if (_loaded) return;

                var libraryPaths = GetLibraryPaths();
                Exception? lastException = null;

                foreach (var libraryPath in libraryPaths)
                {
                    try
                    {
                        NativeLibrary.Load(libraryPath);
                        _loaded = true;
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                    }
                }
                throw new InvalidOperationException(
                    $"Failed to load Bitcoin Kernel native library. Tried: {string.Join(", ", libraryPaths.Where(p => p != null).Concat(new[] { "system LD_LIBRARY_PATH" }))}",
                    lastException);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to load Bitcoin Kernel native library. Ensure the library is in the application directory or system path. Supported platforms: Windows, Linux, OSX.",
                ex);
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    private static string[] GetLibraryPaths()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return
            [
                "bitcoinkernel.dll",
                Path.Combine(basePath, "runtimes", "win-x64", "native", "bitcoinkernel.dll")
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return
            [
                "libbitcoinkernel.so",
                Path.Combine(basePath, "runtimes", "linux-x64", "native", "libbitcoinkernel.so")
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return
            [
                "libbitcoinkernel.dylib",
                Path.Combine(basePath, "runtimes", "osx-x64", "native", "libbitcoinkernel.dylib")
            ];
        }

        throw new PlatformNotSupportedException(
            $"Unsupported platform: {RuntimeInformation.OSDescription}. Supported platforms: Windows, Linux, OSX.");
    }
}