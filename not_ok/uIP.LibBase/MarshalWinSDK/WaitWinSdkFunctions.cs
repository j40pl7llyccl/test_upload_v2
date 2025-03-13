using System;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    public static class WaitWinSdkFunctions
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern UInt32 WaitForSingleObject( IntPtr hHandle, UInt32 dwMilliseconds );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern UInt32 WaitForSingleObjectEx( IntPtr hHandle, UInt32 dwMilliseconds, Int32 bAlertable );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern UInt32 WaitForMultipleObjects( UInt32 nCount, IntPtr[] lpHandles, [MarshalAs( UnmanagedType.Bool )] bool bWaitAll, UInt32 dwMilliseconds );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern UInt32 WaitForMultipleObjectsEx( UInt32 nCount, IntPtr[] lpHandles, [MarshalAs( UnmanagedType.Bool )] bool bWaitAll, UInt32 dwMilliseconds, [MarshalAs( UnmanagedType.Bool )] bool bAlertable );
    }
}
