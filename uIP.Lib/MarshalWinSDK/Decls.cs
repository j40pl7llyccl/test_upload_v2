using System;
using System.Runtime.InteropServices;

namespace uIP.Lib.MarshalWinSDK
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

    [StructLayoutAttribute( LayoutKind.Sequential )]
    public struct SECURITY_DESCRIPTOR
    {
        public byte revision;
        public byte size;
        public short control;
        public IntPtr owner;
        public IntPtr group;
        public IntPtr sacl;
        public IntPtr dacl;
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
        public Int32 left;
        public Int32 top;
        public Int32 right;
        public Int32 bottom;

        public RECT(int l, int t, int r, int b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct RGNDATAHEADER
    {
        public Int32 dwSize;
        public Int32 iType;
        public Int32 nCount;
        public Int32 nRgnSize;
        public RECT rcBound;
    }
}
