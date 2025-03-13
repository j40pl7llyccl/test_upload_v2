using System;
using System.Runtime.InteropServices;
using System.Security;

namespace uIP.LibBase.MarshalWinSDK
{

    public enum FileMapProtection : uint
    {
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        SEC_COMMIT = 0x8000000,
        SEC_IMAGE = 0x1000000,
        SEC_NOCACHE = 0x10000000,
        SEC_RESERVE = 0x4000000,
    }

    public enum FileMapAccess : uint
    {
        FILE_MAP_ALL_ACCESS = 0x001f,
        FILE_MAP_COPY = 0x0001,
        FILE_MAP_READ = 0x0004,
        FILE_MAP_WRITE = 0x0002,
        FILE_MAP_EXECUTE = 0x0020,
    }

    public enum AllocType : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_RESET = 0x80000,
        MEM_LARGE_PAGES = 0x20000000,
        MEM_PHYSICAL = 0x400000,
        MEM_TOP_DOWN = 0x100000,
        MEM_WRITE_WATCH = 0x200000
    }

    public enum FreeType : uint
    {
        MEM_DECOMMIT = 0x4000,
        MEM_RELEASE = 0x8000,
    }

    public enum MemoryProtection : uint
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    public static class MemWinSdkFunctions
    {
        #region DLL Import

        #region >>> Shared Memory Part <<<
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        private unsafe static extern IntPtr CreateFileMapping( IntPtr hFile,
                                                               SECURITY_ATTRIBUTES* lpAttributes,
                                                               UInt32 flProtect,
                                                               UInt32 dwMaximumSizeHigh,
                                                               UInt32 dwMaximumSizeLow,
                                                               [MarshalAs( UnmanagedType.LPStr )] string lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "CreateFileMappingA" )]
        private unsafe static extern IntPtr CreateFileMapping_1( IntPtr hFile,
                                                               IntPtr lpAttributes,
                                                               UInt32 flProtect,
                                                               UInt32 dwMaximumSizeHigh,
                                                               UInt32 dwMaximumSizeLow,
                                                               IntPtr lpName );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenFileMappingA" )]
        private static extern IntPtr OpenFileMapping( UInt32 dwDesiredAccess,
                                                      [MarshalAs( UnmanagedType.Bool )] bool bInheritHandle,
                                                      [MarshalAs( UnmanagedType.LPStr )] string lpName // ansi string name, not wide char
            );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, EntryPoint = "OpenFileMappingA" )]
        private static extern IntPtr OpenFileMapping_1( UInt32 dwDesiredAccess,
                                                      [MarshalAs( UnmanagedType.Bool )] bool bInheritHandle,
                                                      IntPtr lpName // ansi string name, not wide char
            );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern IntPtr MapViewOfFile( IntPtr hFileMappingObject,
                                                    UInt32 dwDesiredAccess,
                                                    UInt32 dwFileOffsetHigh,
                                                    UInt32 dwFileOffsetLow,
                                                    UIntPtr dwNumberOfBytesToMap );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern IntPtr MapViewOfFileEx( IntPtr hFileMappingObject,
                                                      UInt32 dwDesiredAccess,
                                                      UInt32 dwFileOffsetHigh,
                                                      UInt32 dwFileOffsetLow,
                                                      UIntPtr dwNumberOfBytesToMap,
                                                      IntPtr lpBaseAddress );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool UnmapViewOfFile( IntPtr lpBaseAddress );
        #endregion

        #region >>> Virtual <<<
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern IntPtr VirtualAlloc( IntPtr lpAddress,
                                                   UIntPtr dwSize,
                                                   UInt32 flAllocationType, // AllocType
                                                   UInt32 flProtect );      // MemoryProtection

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
        private static extern IntPtr VirtualAllocEx( IntPtr hProcess,
                                                     IntPtr lpAddress,
                                                     UIntPtr dwSize,
                                                     UInt32 flAllocationType, // AllocType
                                                     UInt32 flProtect );      // MemoryProtection

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualFree( IntPtr lpAddress,
                                                UIntPtr dwSize,
                                                UInt32 dwFreeType ); // FreeType

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true, ExactSpelling = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualFreeEx( IntPtr hProcess,
                                                  IntPtr lpAddress,
                                                  UIntPtr dwSize,
                                                  UInt32 dwFreeType ); // FreeType

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualLock( IntPtr lpAddress, UIntPtr dwSize );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualUnlock( IntPtr lpAddress, UIntPtr dwSize );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualProtect( IntPtr lpAddress,
                                                   UIntPtr dwSize,
                                                   UInt32 flNewProtect, // MemoryProtection
                                                   out UInt32 lpflOldProtect );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool VirtualProtectEx( IntPtr hProcess,
                                                     IntPtr lpAddress,
                                                     UIntPtr dwSize,
                                                     UInt32 flNewProtect, // MemoryProtection
                                                     out UInt32 lpflOldProtect );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        private unsafe static extern UIntPtr VirtualQuery( IntPtr lpAddress,
                                                    MEMORY_BASIC_INFORMATION* lpBuffer,
                                                    UIntPtr dwLength );


        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        private unsafe static extern UIntPtr VirtualQueryEx( IntPtr hProcess,
                                                      IntPtr lpAddress,
                                                      MEMORY_BASIC_INFORMATION* lpBuffer,
                                                      UIntPtr dwLength );

        #endregion

        #region >>> COM <<<

        [DllImport( "ole32.dll" )]
        private static extern IntPtr CoTaskMemAlloc( IntPtr cb );

        [DllImport( "ole32.dll" )]
        private static extern void CoTaskMemFree( IntPtr pv );

        #endregion

        #region >>> Memory Access <<<

        [DllImport( "msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy", SetLastError = false )]
        public static extern void NativeMemcpy( IntPtr dst, IntPtr src, UIntPtr length );

        [DllImport( "msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memset", SetLastError = false )]
        public static extern void NativeMemset( IntPtr dst, Int32 c, UIntPtr length );

        [DllImport( "msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcpy", SetLastError = false )]
        public static extern unsafe void NativeMemcpy( void* dst, void* src, UIntPtr length );

        #endregion

        #endregion

        /// <summary>
        /// Create the file mapping object in ASCII string
        /// </summary>
        /// <param name="buffsz">maximum object size</param>
        /// <param name="name">name of mapping object</param>
        /// <returns>mapping file object</returns>
        public static IntPtr CreateFileMapping( UInt32 buffsz, string name, UInt32 nBuffSzHighPart = 0 )
        {
            int dummy;
            IntPtr pName = InternalMethods.StringToIntptrHGlobal<byte>( name, System.Text.Encoding.ASCII, out dummy );
            IntPtr ret = IntPtr.Zero;
            unsafe
            {
                ret = CreateFileMapping_1( new IntPtr( -1 ), IntPtr.Zero, ( UInt32 ) FileMapProtection.PAGE_READWRITE, nBuffSzHighPart, buffsz, pName );
            }
            Marshal.FreeHGlobal( pName );
            return ret;
        }

        /// <summary>
        /// Open an existing file mapping in ACSII string
        /// </summary>
        /// <param name="name">name of mapping object</param>
        /// <returns>mapping file object</returns>
        public static IntPtr OpenFileMapping( string name )
        {
            int dummy;
            IntPtr pName = InternalMethods.StringToIntptrHGlobal<byte>( name, System.Text.Encoding.ASCII, out dummy );
            IntPtr ret = OpenFileMapping_1( ( UInt32 ) FileMapAccess.FILE_MAP_ALL_ACCESS, false, pName );
            Marshal.FreeHGlobal( pName );
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hMapObj">file mapping object</param>
        /// <param name="buffsz">number of bytes to map</param>
        /// <returns>starting address of the mapped view</returns>
        public static IntPtr MapViewOfFile( IntPtr hMapObj, UInt32 buffsz )
        {
            return MapViewOfFile( hMapObj, ( UInt32 ) FileMapAccess.FILE_MAP_ALL_ACCESS, ( UInt32 ) 0, ( UInt32 ) 0, new UIntPtr( buffsz ) );
        }

        /// <summary>
        /// Close the mapping file object and its buffer
        /// </summary>
        /// <param name="handle">mapping file object</param>
        /// <param name="bufferPtr">accessing buffer address</param>
        public static void CloseFileMapping( IntPtr handle, IntPtr bufferPtr )
        {
            if ( bufferPtr != IntPtr.Zero )
                UnmapViewOfFile( bufferPtr );
            if ( handle != IntPtr.Zero )
                CommonWinSdkFunctions.CloseHandle( handle );
        }

        /// <summary>
        /// Allocate memory from COM memory space to IPC
        /// </summary>
        /// <param name="size">need memory size</param>
        /// <returns>address of the memory</returns>
        public static IntPtr AllocComMem( long size )
        {
            if ( size == 0 ) return IntPtr.Zero;

            return CoTaskMemAlloc( new IntPtr( size ) );
        }

        /// <summary>
        /// Free memory of COM
        /// </summary>
        /// <param name="addr">allocated memory</param>
        public static void FreeComMem( IntPtr addr )
        {
            if ( addr == IntPtr.Zero )
                return;
            CoTaskMemFree( addr );
        }

        [DllImport( "kernel32.dll", CallingConvention = CallingConvention.Cdecl )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool GlobalMemoryStatusEx( IntPtr lpBuffer );

        public static bool GetSystemMemInfo( out UInt64 totalPhy, out UInt64 availablePhy,
                                             out UInt64 totalPagFile, out UInt64 availablePagFile,
                                             out UInt64 totalVirtual, out UInt64 availableVirtual )
        {
            totalPhy = availablePhy = totalPagFile = availablePagFile = totalVirtual = availableVirtual = ( UInt64 ) 0;

            MEMORYSTATUSEX info;
            info.dwLength = ( uint ) Marshal.SizeOf( typeof( MEMORYSTATUSEX ) );
            bool bret = false;

            unsafe
            {
                void* ptr = ( void* ) &info;
                bret = GlobalMemoryStatusEx( new IntPtr( ptr ) );
            }

            if ( bret )
            {
                totalPhy = info.ullTotalPhys;
                availablePhy = info.ullAvailPhys;

                totalPagFile = info.ullTotalPageFile;
                availablePagFile = info.ullAvailPageFile;

                totalVirtual = info.ullTotalVirtual;
                availableVirtual = info.ullAvailVirtual;
            }
            return bret;
        }
    }
}
