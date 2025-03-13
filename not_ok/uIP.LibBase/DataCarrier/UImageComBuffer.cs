using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace uIP.LibBase.DataCarrier
{
    public class UImageComBuffer : UImageBuffer
    {
        public UImageComBuffer() : base() { }
        public UImageComBuffer( UInt32 w, UInt32 h, UInt32 f ) : base( w, h, f ) { }

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
