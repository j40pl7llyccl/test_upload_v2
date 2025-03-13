using System;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    /// <summary>
    /// the name of the sync obj is ansi string not wide char
    /// </summary>
    public static class SyncWinSdkFunctions
    {
        #region Dll Import

        #region >>> Mutex <<<
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateMutexA" )]
        private unsafe static extern IntPtr CreateMutex( SECURITY_ATTRIBUTES* lpMutexAttributes,
                                                         [MarshalAs( UnmanagedType.Bool )] bool bInitialOwner,
                                                         [MarshalAs( UnmanagedType.LPTStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateMutexA" )]
        private static extern IntPtr CreateMutex_1( IntPtr lpMutexAttributes,
                                                    [MarshalAs( UnmanagedType.Bool )] bool bInitialOwner,
                                                    IntPtr lpName );


        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenMutexA" )]
        private static extern IntPtr OpenMutex( UInt32 dwDesiredAccess,
                                                [MarshalAs( UnmanagedType.Bool )] bool bInheritHandle,
                                                [MarshalAs( UnmanagedType.LPTStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenMutexA" )]
        private static extern IntPtr OpenMutex_1( UInt32 dwDesiredAccess,
                                                [MarshalAs( UnmanagedType.Bool )] bool bInheritHandle,
                                                IntPtr lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ReleaseMutex( IntPtr hMutex );
        #endregion

        #endregion

        public static IntPtr CreateMutex( string name )
        {
            int dummy;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<byte>( name, System.Text.Encoding.ASCII, out dummy );
            IntPtr ret = IntPtr.Zero;
            unsafe
            {
                ret = CreateMutex_1( IntPtr.Zero, false, pStr );
            }

            Marshal.FreeHGlobal( pStr );

            return ret;
        }

        public static IntPtr OpenMutex( string name )
        {
            int dummy;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<byte>( name, System.Text.Encoding.ASCII, out dummy );

            IntPtr ret = OpenMutex_1( ( UInt32 ) MUTEX_ACC_RIGHT.ALL_ACCESS, false, pStr );

            Marshal.FreeHGlobal( pStr );

            return ret;
        }

    }
}
