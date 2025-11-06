using System.Runtime.InteropServices;
using BitcoinKernel.Interop.Delegates.Validation;

namespace BitcoinKernel.Interop.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ValidationInterfaceCallbacks
    {
        public IntPtr UserData;
        public ValidationBlockChecked BlockChecked;
        public ValidationNewPoWValidBlock NewPoWValidBlock;
        public ValidationBlockConnected BlockConnected;
        public ValidationBlockDisconnected BlockDisconnected;
    }
}