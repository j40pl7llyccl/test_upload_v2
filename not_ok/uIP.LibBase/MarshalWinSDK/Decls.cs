using System;
using System.Runtime.InteropServices;

namespace uIP.LibBase.MarshalWinSDK
{
    [StructLayout( LayoutKind.Sequential )]
    public struct OVERLAPPED
    {
        public UIntPtr Internal;
        public UIntPtr InternalHigh;
        public UInt32 Offset;
        public UInt32 OffsetHigh;
        public IntPtr hEvent;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct SECURITY_ATTRIBUTES
    {
        public UInt32 nLength; // DWORD
        public IntPtr lpSecurityDescriptor; // LPVOID
        public Int32 bInheritHandle;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;//PVOID
        public IntPtr AllocationBase;//PVOID
        public UInt32 AllocationProtect;//DWORD
        public UIntPtr RegionSize; //SIZE_T
        public UInt32 State;//DWORD
        public UInt32 Protect;//DWORD
        public UInt32 Type;//DWORD
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct POINT
    {
        public Int32 x;
        public Int32 y;

        public POINT(int px, int py)
        {
            x = px;
            y = py;
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int l, int t, int r, int b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }
    }

}
