using System.Runtime.InteropServices;

namespace BitcoinKernel.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LoggingOptions
    {
        public int LogTimestamps;
        public int LogTimeMicros;
        public int LogThreadNames;
        public int LogSourceLocations;
        public int AlwaysPrintCategoryLevels;
    }
}