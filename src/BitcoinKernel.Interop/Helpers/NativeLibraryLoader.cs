namespace BitcoinKernel.Interop.Helpers
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Helper class for loading
    static class NativeLibraryLoader
    {
        private static bool _loaded = false;
        private static readonly System.Threading.ReaderWriterLockSlim _lock = new System.Threading.ReaderWriterLockSlim();

        public static void EnsureLoaded()
        {
            if (_loaded) return;

            try
            {
                string libraryPath = GetLibraryPath();
                NativeLibrary.Load(libraryPath);
                _loaded = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to load Bitcoin Kernel native library. " +
                    "Ensure the library is in the application directory or system path.", ex);
            }

        }

        private static string GetLibraryPath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(basePath, "runtimes", "win-x64", "native", "bitcoinkernel.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(basePath, "runtimes", "linux-x64", "native", "libbitcoinkernel.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                 return Path.Combine(basePath, "runtimes", "osx-x64", "native", "libbitcoinkernel.dylib");
            }

            throw new PlatformNotSupportedException(
                $"Unsupported platform: {RuntimeInformation.OSDescription}");
        }
    }
}