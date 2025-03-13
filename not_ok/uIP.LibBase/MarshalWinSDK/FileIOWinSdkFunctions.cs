using System;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    [Flags]
    public enum EFileAccess : uint
    {
        // winnt.h
        //
        // Standart Section
        //

        ACCESS_SYSTEM_SECURITY = 0x1000000,   // AccessSystemAcl access type
        MAXIMUM_ALLOWED = 0x2000000,     // MaximumAllowed access type


        // begin_ntddk begin_wdm begin_nthal begin_ntifs
        //
        //  The following are masks for the predefined standard access types
        //
        DELETE = 0x10000,
        READ_CONTROL = 0x20000,
        WRITE_DAC = 0x40000,
        WRITE_OWNER = 0x80000,
        SYNCHRONIZE = 0x100000,

        STANDARD_RIGHTS_REQUIRED = 0xF0000,
        STANDARD_RIGHTS_READ = READ_CONTROL,
        STANDARD_RIGHTS_WRITE = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE = READ_CONTROL,
        STANDARD_RIGHTS_ALL = 0x1F0000,
        SPECIFIC_RIGHTS_ALL = 0xFFFF,

        //
        // The FILE_READ_DATA and FILE_WRITE_DATA constants are also defined in
        // devioctl.h as FILE_READ_ACCESS and FILE_WRITE_ACCESS. The values for these
        // constants *MUST* always be in sync.
        // The values are redefined in devioctl.h because they must be available to
        // both DOS and NT.
        //
        FILE_READ_DATA = 0x0001, // file & pipe
        FILE_LIST_DIRECTORY = 0x0001, // directory
        FILE_WRITE_DATA = 0x0002, // file & pipe
        FILE_ADD_FILE = 0x0002, // directory
        FILE_APPEND_DATA = 0x0004, // file
        FILE_ADD_SUBDIRECTORY = 0x0004, // directory
        FILE_CREATE_PIPE_INSTANCE = 0x0004, // named pipe
        FILE_READ_EA = 0x0008, // file & directory
        FILE_WRITE_EA = 0x0010, // file & directory
        FILE_EXECUTE = 0x0020, // file
        FILE_TRAVERSE = 0x0020, // directory
        FILE_DELETE_CHILD = 0x0040, // directory
        FILE_READ_ATTRIBUTES = 0x0080, // all
        FILE_WRITE_ATTRIBUTES = 0x0100, // all

        //
        // Generic Section
        //

        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000,

        //SPECIFIC_RIGHTS_ALL  = 0x00FFFF,
        FILE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF),

        FILE_GENERIC_READ = (STANDARD_RIGHTS_READ |
                                 FILE_READ_DATA |
                                 FILE_READ_ATTRIBUTES |
                                 FILE_READ_EA |
                                 SYNCHRONIZE),

        FILE_GENERIC_WRITE = (STANDARD_RIGHTS_WRITE |
                                 FILE_WRITE_DATA |
                                 FILE_WRITE_ATTRIBUTES |
                                 FILE_WRITE_EA |
                                 FILE_APPEND_DATA |
                                 SYNCHRONIZE),

        FILE_GENERIC_EXECUTE = (STANDARD_RIGHTS_EXECUTE |
                                 FILE_READ_ATTRIBUTES |
                                 FILE_EXECUTE |
                                 SYNCHRONIZE),
    }

    [Flags]
    public enum EFileShare : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Enables subsequent open operations on an object to request read access. 
        /// Otherwise, other processes cannot open the object if they request read access. 
        /// If this flag is not specified, but the object has been opened for read access, the function fails.
        /// </summary>
        FILE_SHARE_READ = 0x00000001,
        /// <summary>
        /// Enables subsequent open operations on an object to request write access. 
        /// Otherwise, other processes cannot open the object if they request write access. 
        /// If this flag is not specified, but the object has been opened for write access, the function fails.
        /// </summary>
        FILE_SHARE_WRITE = 0x00000002,
        /// <summary>
        /// Enables subsequent open operations on an object to request delete access. 
        /// Otherwise, other processes cannot open the object if they request delete access.
        /// If this flag is not specified, but the object has been opened for delete access, the function fails.
        /// </summary>
        FILE_SHARE_DELETE = 0x00000004
    }

    public enum ECreationDisposition : uint
    {
        /// <summary>
        /// Creates a new file. The function fails if a specified file exists.
        /// </summary>
        CREATE_NEW = 1,
        /// <summary>
        /// Creates a new file, always. 
        /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes, 
        /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
        /// </summary>
        CREATE_ALWAYS = 2,
        /// <summary>
        /// Opens a file. The function fails if the file does not exist. 
        /// </summary>
        OPEN_EXISTING = 3,
        /// <summary>
        /// Opens a file, always. 
        /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OPEN_ALWAYS = 4,
        /// <summary>
        /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        /// The calling process must open the file with the GENERIC_WRITE access right. 
        /// </summary>
        TRUNCATE_EXISTING = 5
    }

    [Flags]
    public enum EFileAttributes : uint
    {
        FILE_ATTRIBUTE_READONLY = 0x00000001,
        FILE_ATTRIBUTE_HIDDEN = 0x00000002,
        FILE_ATTRIBUTE_SYSTEM = 0x00000004,
        FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
        FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
        FILE_ATTRIBUTE_DEVICE = 0x00000040,
        FILE_ATTRIBUTE_NORMAL = 0x00000080,
        FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
        FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
        FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
        FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
        FILE_ATTRIBUTE_OFFLINE = 0x00001000,
        FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
        FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
        FILE_FLAG_WRITE_THROUGH = 0x80000000,
        FILE_FLAG_OVERLAPPED = 0x40000000,
        FILE_FLAG_NO_BUFFERING = 0x20000000,
        FILE_FLAG_RANDOM_ACCESS = 0x10000000,
        FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
        FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
        FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
        FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
        FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
        FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
        FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000
    }

    public static class FileIoWinSdkFunctions
    {
        #region DLL Import

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public unsafe static extern IntPtr CreateFile( string lpFileName,
                                                       UInt32 dwDesiredAccess,
                                                       UInt32 dwShareMode,
                                                       SECURITY_ATTRIBUTES* lpSecurityAttributes,
                                                       UInt32 dwCreationDisposition,
                                                       UInt32 dwFlagsAndAttributes,
                                                       IntPtr hTemplateFile );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool ReadFile( IntPtr hFile,
                                            IntPtr lpBuffer,
                                            UInt32 nNumberOfBytesToRead,
                                            ref UInt32 lpNumberOfBytesRead,
                                            IntPtr lpOverlapped ); // refer to System.Threading.NativeOverlapped* if need

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool WriteFile( IntPtr hFile,
                                             IntPtr lpBuffer,
                                             UInt32 nNumberOfBytesToWrite,
                                             ref UInt32 lpNumberOfBytesWritten,
                                             IntPtr lpOverlapped ); // refer to System.Threading.NativeOverlapped* if need

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool FlushFileBuffers( IntPtr hFile );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CancelIo( IntPtr hFile );

        public enum EMoveMethod : uint
        {
            FILE_BEGIN = 0,
            FILE_CURRENT = 1,
            FILE_END = 2,
        }

        // test using "( UInt32 ) -1" for invalid
        // lpDistanceToMoveHigh input a "Int32 *" if need to a long jump
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        public unsafe static extern UInt32 SetFilePointer( IntPtr hFile,
                                                           Int32 lDistanceToMove,
                                                           Int32* lpDistanceToMoveHigh,
                                                           UInt32 dwMoveMethod );

        #endregion
    }
}
