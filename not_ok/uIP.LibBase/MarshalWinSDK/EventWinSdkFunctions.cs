using System;
using System.Runtime.InteropServices;
using System.Security;

namespace uIP.LibBase.MarshalWinSDK
{
    public enum EvtFlag : uint
    {
        CREATE_EVENT_INITIAL_SET = 0x00000002,
        CREATE_EVENT_MANUAL_RESET = 0x00000001,
    }

    /// <summary>
    /// the name of the event is ansi-string not wide char
    /// </summary>
    public static class EventWinSdkFunctions
    {
        #region Import DLL

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateEventA" )]
        private unsafe static extern IntPtr CreateEvent( SECURITY_ATTRIBUTES* lpEventAttributes, Int32 bManualReset, Int32 bInitialState, [MarshalAs( UnmanagedType.LPStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateEventExA" )]
        private unsafe static extern IntPtr CreateEventEx( SECURITY_ATTRIBUTES* lpEventAttributes, [MarshalAs( UnmanagedType.LPStr )] string lpName, UInt32 dfFlags, UInt32 dwDesiredAccess );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool ResetEvent( IntPtr hEvent );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool SetEvent( IntPtr hEvent );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenEventA" )]
        private static extern IntPtr OpenEvent( UInt32 dwDesiredAccess, Int32 bInheritHandle, [MarshalAs( UnmanagedType.LPStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool PulseEvent( IntPtr hHandle );

        #endregion

        public static IntPtr Create( bool bManualReset, bool bInitSignaled, string name )
        {
            IntPtr pEvt = IntPtr.Zero;
            unsafe
            {
                pEvt = CreateEvent( null, bManualReset ? 1 : 0, bInitSignaled ? 1 : 0, name );
            }

            return pEvt;
        }

        public static IntPtr Open( UInt32 dwDesiredAccess, bool bInheritHandle, string name )
        {
            return OpenEvent( dwDesiredAccess, bInheritHandle ? 1 : 0, name );
        }

        public static void Close( IntPtr h )
        {
            if ( h == IntPtr.Zero ) return;

            CommonWinSdkFunctions.CloseHandle( h );
        }

        public static void Reset( IntPtr h )
        {
            if ( h == IntPtr.Zero ) return;

            ResetEvent( h );
        }

        public static void Set( IntPtr h )
        {
            if ( h == IntPtr.Zero ) return;

            SetEvent( h );
        }

        public static void Pulse( IntPtr h )
        {
            if ( h == IntPtr.Zero ) return;

            PulseEvent( h );
        }

    }
}
