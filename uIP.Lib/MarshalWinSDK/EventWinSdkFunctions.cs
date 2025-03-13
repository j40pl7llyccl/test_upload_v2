using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace uIP.Lib.MarshalWinSDK
{
    /// <summary>
    /// the name of the event is ansi-string not wide char
    /// </summary>
    public static class EventWinSdkFunctions
    {
        #region Import DLL

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateEventA" )]
        public unsafe static extern IntPtr CreateEvent( SECURITY_ATTRIBUTES* lpEventAttributes, Int32 bManualReset, Int32 bInitialState, [MarshalAs( UnmanagedType.LPStr )] string lpName );
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateEventA" )]
        private unsafe static extern IntPtr CreateEvent02( SECURITY_ATTRIBUTES* lpEventAttributes, Int32 bManualReset, Int32 bInitialState, IntPtr lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateEventExA" )]
        private unsafe static extern IntPtr CreateEventEx( SECURITY_ATTRIBUTES* lpEventAttributes, [MarshalAs( UnmanagedType.LPStr )] string lpName, UInt32 dfFlags, UInt32 dwDesiredAccess );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ResetEvent( IntPtr hEvent );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetEvent( IntPtr hEvent );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenEventA" )]
        public static extern IntPtr OpenEvent( UInt32 dwDesiredAccess, Int32 bInheritHandle, [MarshalAs( UnmanagedType.LPStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool PulseEvent( IntPtr hHandle );

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool InitializeSecurityDescriptor( out SECURITY_DESCRIPTOR securityDescriptor, uint dwRevision );

        [DllImport( "advapi32.dll", SetLastError = true )]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetSecurityDescriptorDacl( ref SECURITY_DESCRIPTOR securityDescriptor, [MarshalAs(UnmanagedType.Bool)]bool daclPresent, IntPtr dacl, [MarshalAs(UnmanagedType.Bool)]bool daclDefaulted );

        public const uint SECURITY_DESCRIPTOR_REVISION = 1;

        #endregion

        /// <summary>
        /// create a event
        /// </summary>
        /// <param name="bManualReset">manual reset event</param>
        /// <param name="bInitSignaled">init signaled</param>
        /// <param name="name">null may be admin right be careful => 0x80004005; use "" is better</param>
        /// <returns></returns>
        public static IntPtr Create( bool bManualReset, bool bInitSignaled, string name )
        {
            IntPtr pEvt = IntPtr.Zero;
            IntPtr mem = IntPtr.Zero;
            try
            {
                unsafe
                {
                    name = name ?? "";
                    byte[] enc = Encoding.ASCII.GetBytes( name );
                    mem = Marshal.AllocCoTaskMem( enc.Length + 1 );
                    byte* cpn = ( byte* )mem.ToPointer();
                    if ( mem != IntPtr.Zero )
                    {
                        Marshal.Copy( enc, 0, mem, enc.Length );
                        cpn[ enc.Length ] = 0;
                        pEvt = CreateEvent02( null, bManualReset ? 1 : 0, bInitSignaled ? 1 : 0, mem );
                    }
                }

            }
            catch(Exception e)
            {
                Console.WriteLine( $"Create event with exception:\n{InternalMethods.Dump01( e )}" );
            }
            finally
            {
                if ( mem != IntPtr.Zero )
                    Marshal.FreeCoTaskMem( mem );
            }

            return pEvt;
        }

        public static unsafe IntPtr CreateEventWithConfSecurity( bool bManualReset, bool bInitSignaled, string name )
        {
            IntPtr memSD = IntPtr.Zero;
            IntPtr memSA = IntPtr.Zero;

            try
            {
                int szSD = Marshal.SizeOf( typeof( SECURITY_DESCRIPTOR ) );
                memSD = Marshal.AllocCoTaskMem( szSD );
                int szSA = Marshal.SizeOf( typeof( SECURITY_ATTRIBUTES ) );
                memSA = Marshal.AllocCoTaskMem( szSA );
                SECURITY_DESCRIPTOR sd;
                var status = InitializeSecurityDescriptor( out sd, SECURITY_DESCRIPTOR_REVISION );
                if ( !status )
                {
                    Console.WriteLine( $"[CreateEvent]InitializeSecurityDescriptor: last error code={CommonWinSdkFunctions.GetLastError()}" );
                    return IntPtr.Zero;
                }
                status = SetSecurityDescriptorDacl( ref sd, true, IntPtr.Zero, false );
                if ( !status )
                {
                    Console.WriteLine( $"[CreateEvent]SetSecurityDescriptorDacl: last error code={CommonWinSdkFunctions.GetLastError()}" );
                    return IntPtr.Zero;
                }
                Marshal.StructureToPtr( sd, memSD, false );

                SECURITY_ATTRIBUTES sa;
                sa.nLength = ( uint )szSD;
                sa.bInheritHandle = Convert.ToInt32( false );
                sa.lpSecurityDescriptor = memSD;
                Marshal.StructureToPtr( sa, memSA, false );

                return CreateEvent( ( SECURITY_ATTRIBUTES* )memSA.ToPointer(), Convert.ToInt32( bManualReset ), Convert.ToInt32( bInitSignaled ), name );
            }
            catch ( Exception e )
            {
                ULibAgent.Singleton.LogError?.Invoke( $"uProviderSopCommunication::CreateEvent ({name}) with error\n{e}" );
                return IntPtr.Zero;
            }
            finally
            {
                if ( memSD != IntPtr.Zero ) Marshal.FreeCoTaskMem( memSD );
                if ( memSA != IntPtr.Zero ) Marshal.FreeCoTaskMem( memSA );
            }
        }


        public static IntPtr Open( UInt32 dwDesiredAccess, bool bInheritHandle, string name )
        {
            return OpenEvent( dwDesiredAccess, bInheritHandle ? 1 : 0, name );
        }

        public static void Close( IntPtr h )
        {
            if ( h == IntPtr.Zero ) return;

            try
            {
                CommonWinSdkFunctions.CloseHandle( h );
            }
            catch(Exception e)
            {
                Console.WriteLine( $"Close event with exception:\n{InternalMethods.Dump01( e )}" );
            }
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
