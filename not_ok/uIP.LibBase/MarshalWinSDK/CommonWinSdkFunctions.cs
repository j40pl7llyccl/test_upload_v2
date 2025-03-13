using System;
using System.Security;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    public static class CommonWinSdkFunctions
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool CloseHandle( IntPtr hObject );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        public static extern UInt32 GetTickCount();

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Kernel32.dll" )]
        public static extern Int32 QueryPerformanceCounter( ref long ticks );

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport( "Kernel32.dll" )]
        public static extern Int32 QueryPerformanceFrequency( ref long ticks );

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll" )]
        public static extern UInt32 GetLastError();

        private static long _nCurrFrequency = 0;
        public static long CurrFrequence { get { return _nCurrFrequency; } }

        static CommonWinSdkFunctions()
        {
            QueryPerformanceFrequency( ref _nCurrFrequency );
        }

        public static bool CalcDiffMillisecond( long tmBegFromCounter, long tmEndFromCounter, out double dfDiff )
        {
            return CalcDiffMillisecond( tmBegFromCounter, tmEndFromCounter, _nCurrFrequency, out dfDiff );
        }

        public static bool CalcDiffMillisecond( long tmBegFromCounter, long tmEndFromCounter, long freqFromFrequency, out double dfDiff )
        {
            dfDiff = 0.0;
            if ( freqFromFrequency <= ( long ) 0 ) return false;
            dfDiff = Convert.ToDouble( tmEndFromCounter - tmBegFromCounter ) / Convert.ToDouble( freqFromFrequency ) * 1000.0;
            return true;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool GetOverlappedResult( IntPtr hFile,
                                                       ref OVERLAPPED lpOverlapped,
                                                       ref UInt32 lpNumberOfBytesTransferred,
                                                       [MarshalAs( UnmanagedType.Bool )] bool bWait );

    }
}
