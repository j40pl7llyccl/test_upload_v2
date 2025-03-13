using System;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    [Flags]
    public enum CreatePipeOpenModeFlags : uint
    {
        PIPE_ACCESS_DUPLEX = 0x00000003,
        PIPE_ACCESS_INBOUND = 0x00000001,
        PIPE_ACCESS_OUTBOUND = 0x00000002,
        FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
        FILE_FLAG_WRITE_THROUGH = 0x80000000,
        FILE_FLAG_OVERLAPPED = 0x40000000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        ACCESS_SYSTEM_SECURITY = 0x01000000
    }

    [Flags]
    public enum CreatePipeModeFlags : uint
    {
        //One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
        PIPE_TYPE_BYTE = 0x00000000,
        PIPE_TYPE_MESSAGE = 0x00000004,
        //One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
        PIPE_READMODE_BYTE = 0x00000000,
        PIPE_READMODE_MESSAGE = 0x00000002,
        //One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
        PIPE_WAIT = 0x00000000,
        PIPE_NOWAIT = 0x00000001,
        //One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
        PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
        PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
    }

    [Flags]
    public enum SetPipeModeFlages : uint
    {
        PIPE_READMODE_BYTE = 0x00000000,
        PIPE_READMODE_MESSAGE = 0x00000002,
        PIPE_WAIT = 0x00000000,
        PIPE_NOWAIT = 0x00000001,
    }

    public static class PipeWinSdkFunctions
    {
        #region Dll Import

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public unsafe static extern bool CreatePipe( ref IntPtr hReadPipe,
                                                      ref IntPtr hWritePipe,
                                                      SECURITY_ATTRIBUTES* lpPipeAttributes,
                                                      UInt32 nSize );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CallNamedPipe( [MarshalAs( UnmanagedType.LPStr )]string lpNamedPipeName,
                                                         IntPtr lpInBuffer,
                                                         UInt32 nInBufferSize,
                                                         IntPtr lpOutBuffer,
                                                         UInt32 nOutBufferSize,
                                                         ref UInt32 lpBytesRead,
                                                         UInt32 nTimeOut );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public unsafe static extern bool ConnectNamedPipe( IntPtr hNamedPipe,
                                                           OVERLAPPED* lpOverlapped );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        public unsafe static extern IntPtr CreateNamedPipe( [MarshalAs( UnmanagedType.LPStr )] string lpName,
                                                             UInt32 dwOpenMode,
                                                             UInt32 dwPipeMode,
                                                             UInt32 nMaxInstances,
                                                             UInt32 nOutBufferSize,
                                                             UInt32 nInBufferSize,
                                                             UInt32 nDefaultTimeOut,
                                                             SECURITY_ATTRIBUTES* lpSecurityAttributes );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool DisconnectNamedPipe( IntPtr hNamedPipe );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool GetNamedPipeHandleState( IntPtr hNamedPipe,
                                                           IntPtr lpState, // uint32
                                                           IntPtr lpCurInstances, // uint32
                                                           IntPtr lpMaxCollectionCount, // uint32
                                                           IntPtr lpCollectDataTimeout, // uint32
                                                            [MarshalAs( UnmanagedType.LPStr )] string lpUserName,
                                                            UInt32 nMaxUserNameSize );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool GetNamedPipeInfo( IntPtr hNamedPipe,
                                                    IntPtr lpFlags, // uint32
                                                    IntPtr lpOutBufferSize, // uint32
                                                    IntPtr lpInBufferSize, // uint32
                                                    IntPtr lpMaxInstances ); // uint32

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool PeekNamedPipe( IntPtr hNamedPipe,
                                                 IntPtr lpBuffer,
                                                 UInt32 nBufferSize,
                                                 ref UInt32 lpBytesRead,
                                                 ref UInt32 lpTotalBytesAvail,
                                                 ref UInt32 lpBytesLeftThisMessage );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetNamedPipeHandleState( IntPtr hNamedPipe,
                                                           IntPtr lpMode, // uint32
                                                           IntPtr lpMaxCollectionCount, // uint32
                                                           IntPtr lpCollectDataTimeout ); // uint32

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public unsafe static extern bool TransactNamedPipe( IntPtr hNamedPipe,
                                                     IntPtr lpInBuffer,
                                                     UInt32 nInBufferSize,
                                                     IntPtr lpOutBuffer,
                                                     UInt32 nOutBufferSize,
                                                     ref UInt32 lpBytesRead,
                                                            OVERLAPPED* lpOverlapped );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool WaitNamedPipe( [MarshalAs( UnmanagedType.LPStr )] string lpNamedPipeName,
                                                 UInt32 nTimeOut );


        #endregion
    }
}
