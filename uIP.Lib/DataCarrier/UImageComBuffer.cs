using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace uIP.Lib.DataCarrier
{
    public class UImageComBuffer : UImageBuffer
    {
        public UImageComBuffer() : base() { }
        public UImageComBuffer( int w, int h, int f ) : base( w, h, f ) { }

        protected override IntPtr AllocMem( long nSize )
        {
            return Marshal.AllocCoTaskMem( Convert.ToInt32( nSize ) );
        }
        protected override void FreeMem( IntPtr addr )
        {
            Marshal.FreeCoTaskMem( addr );
        }
    }
}
