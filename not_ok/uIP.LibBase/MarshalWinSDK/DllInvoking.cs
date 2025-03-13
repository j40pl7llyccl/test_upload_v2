using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    public static class DllInvoking
    {
        [DllImport( "Kernel32.dll" )]
        public static extern IntPtr LoadLibraryA( IntPtr dllFileName );

        [DllImport( "Kernel32.dll" )]
        public static extern IntPtr LoadLibraryExA( IntPtr dllFileName, IntPtr hFile, UInt32 dwFlags );

        [DllImport( "Kernel32.dll" )]
        public static extern IntPtr LoadLibraryW( IntPtr dllFileName );

        [DllImport( "Kernel32.dll" )]
        public static extern IntPtr LoadLibraryExW( IntPtr dllFileName, IntPtr hFile, UInt32 dwFlags );

        [DllImport( "Kernel32.dll" )]
        public static extern IntPtr GetProcAddress( IntPtr hModule, IntPtr procName );

        [DllImport( "Kernel32.dll" )]
        public static extern Int32 FreeLibrary( IntPtr hModule );

        public static IntPtr LoadLibraryA( string dllFileName )
        {
            int len = 0;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<byte>( dllFileName, Encoding.ASCII, out len );

            if ( pStr == IntPtr.Zero )
                return IntPtr.Zero;

            IntPtr ret = LoadLibraryA( pStr );
            Marshal.FreeHGlobal( pStr );

            return ret;
        }
        /// <summary>
        /// Call win32 sdk LoadLibraryExA()
        /// </summary>
        /// <param name="dllFileName"></param>
        /// <param name="hFile"></param>
        /// <param name="dwFlags">using LOAD_LIB_FLAGS</param>
        /// <returns></returns>
        public static IntPtr LoadLibraryExA( string dllFileName, IntPtr hFile, UInt32 dwFlags )
        {
            int len = 0;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<byte>( dllFileName, Encoding.ASCII, out len );

            if ( pStr == IntPtr.Zero )
                return IntPtr.Zero;

            IntPtr ret = LoadLibraryExA( pStr, hFile, dwFlags );
            Marshal.FreeHGlobal( pStr );

            return ret;
        }

        public static IntPtr LoadLibraryW( string dllFileName )
        {
            int len = 0;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<Int16>( dllFileName, Encoding.Unicode, out len );

            if ( pStr == IntPtr.Zero )
                return IntPtr.Zero;

            IntPtr ret = LoadLibraryW( pStr );
            Marshal.FreeHGlobal( pStr );

            return ret;
        }

        /// <summary>
        /// Call win32 sdk LoadLibraryExW()
        /// </summary>
        /// <param name="dllFileName"></param>
        /// <param name="hFile"></param>
        /// <param name="dwFlags">using LOAD_LIB_FLAGS</param>
        /// <returns></returns>
        public static IntPtr LoadLibraryExW( string dllFileName, IntPtr hFile, UInt32 dwFlags )
        {
            int len = 0;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<Int16>( dllFileName, Encoding.Unicode, out len );

            if ( pStr == IntPtr.Zero )
                return IntPtr.Zero;

            IntPtr ret = LoadLibraryExW( pStr, hFile, dwFlags );
            Marshal.FreeHGlobal( pStr );

            return ret;
        }

        public static IntPtr GetProcAddress( IntPtr h, string funcName )
        {
            int len = 0;
            IntPtr pStr = InternalMethods.StringToIntptrHGlobal<byte>( funcName, Encoding.ASCII, out len );

            if ( pStr == IntPtr.Zero )
                return IntPtr.Zero;

            IntPtr ret = GetProcAddress( h, pStr );
            Marshal.FreeHGlobal( pStr );

            return ret;
        }

        public static Delegate ToDelegate<T>(IntPtr h, string funcName)
        {
            IntPtr fp = GetProcAddress( h, funcName );
            if ( fp == IntPtr.Zero )
                return null;

            return Marshal.GetDelegateForFunctionPointer( fp, typeof( T ) );
        }
    }
}
