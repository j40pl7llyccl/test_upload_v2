using System;
using System.Text;
using System.Runtime.InteropServices;

namespace uIP.LibBase.Utility
{
    public static class UDatManBuffArrConverting
    {
        public unsafe static Array ReverseByteBuffer<T>( byte[] buff, Int32[] dimLen )
        {
            if ( dimLen == null || buff == null ) return null;
            if ( !typeof( T ).IsValueType ) return null;

            int uu = typeof( T ) == typeof( char ) ? sizeof( char ) : Marshal.SizeOf( typeof( T ) );
            long eleSz = ( long ) 0;
            for ( int i = 0 ; i < dimLen.Length ; i++ )
            {
                if ( dimLen[ i ] <= 0 ) return null;

                eleSz = i == 0 ? ( long ) dimLen[ i ] : eleSz * ( long ) dimLen[ i ];
            }

            long byteCnt = eleSz * ( long ) uu;
            if ( byteCnt > ( long ) Int32.MaxValue ) return null;

            if ( buff.LongLength != (byteCnt * ( long ) 2) ) return null;

            bool berr = false;
            byte[] u8Dat = new byte[ byteCnt ];
            //int tmpVal = 0;
            //int mm = 0;
            int hi = 0, lo = 0;
            fixed ( byte* p8 = buff )
            {
                long idx = 0;
                //byte* p88 = p8;
                for ( long i = 0 ; i < buff.LongLength ; i += (( long ) 2) )
                {
                    hi = p8[ i ];
                    lo = p8[ i + 1 ];

                    if ( hi >= '0' && hi <= '9' )
                        hi -= '0';
                    else if ( hi >= 'a' && hi <= 'f' )
                        hi = hi - 'a' + 10;
                    else if ( hi >= 'A' && hi <= 'F' )
                        hi = hi - 'A' + 10;
                    else { berr = true; break; }

                    if ( lo >= '0' && lo <= '9' )
                        lo -= '0';
                    else if ( lo >= 'a' && lo <= 'f' )
                        lo = lo - 'a' + 10;
                    else if ( lo >= 'A' && lo <= 'F' )
                        lo = lo - 'A' + 10;
                    else { berr = true; break; }

                    u8Dat[ idx++ ] = ( byte ) (hi << 4 | lo);
                }
            }
            if ( berr ) return null;

            // be careful, I operate the managed buffer from GC
            Array ret = Array.CreateInstance( typeof( T ), dimLen );
            object tmp = ret;
            GCHandle gch = GCHandle.Alloc( tmp, GCHandleType.Pinned );
            try
            {
                Marshal.Copy( u8Dat, 0, gch.AddrOfPinnedObject(), u8Dat.Length );
            }
            finally { gch.Free(); }

            return ret;
        }
        public unsafe static string ConvertArrayT2String<T, UnitT>( T val, out Int32[] dimLen )
        {
            dimLen = null;

            Int32 unit = typeof( UnitT ) == typeof( char ) ? sizeof( char ) : Marshal.SizeOf( typeof( UnitT ) );
            object tmp = ( object ) val;
            if ( tmp == null ) return null;

            Array arr = val as Array;
            if ( arr == null || arr.Rank <= 0 ) return null;

            StringBuilder sb = new StringBuilder();
            long totalSz = ( long ) 0;

            dimLen = new Int32[ arr.Rank ];
            for ( int i = 0 ; i < dimLen.Length ; i++ )
            {
                dimLen[ i ] = arr.GetLength( i );
                totalSz = i == 0 ? ( long ) dimLen[ i ] : totalSz * ( long ) dimLen[ i ];
            }

            GCHandle gch = GCHandle.Alloc( tmp, GCHandleType.Pinned );
            try
            {
                void* addr = gch.AddrOfPinnedObject().ToPointer();
                byte* p8 = ( byte* ) addr;
                long cpSz = totalSz * ( long ) unit;
                for ( long i = 0 ; i < cpSz ; i++, p8++ )
                    sb.AppendFormat( "{0:x2}", *p8 );
            }
            finally { gch.Free(); }

            return sb.ToString();
        }
        public unsafe static string ConvertArrayT2String<UnitT>( object val, out Int32[] dimLen )
        {
            dimLen = null;

            Int32 unit = typeof( UnitT ) == typeof( char ) ? sizeof( char ) : Marshal.SizeOf( typeof( UnitT ) );
            Array arr = val as Array;
            if ( arr == null || arr.Rank <= 0 ) return null;

            StringBuilder sb = new StringBuilder();
            long totalSz = ( long ) 0;

            dimLen = new Int32[ arr.Rank ];
            for ( int i = 0 ; i < dimLen.Length ; i++ )
            {
                dimLen[ i ] = arr.GetLength( i );
                totalSz = i == 0 ? ( long ) dimLen[ i ] : totalSz * ( long ) dimLen[ i ];
            }

            GCHandle gch = GCHandle.Alloc( val, GCHandleType.Pinned );
            try
            {
                void* addr = gch.AddrOfPinnedObject().ToPointer();
                byte* p8 = ( byte* ) addr;
                long cpSz = totalSz * ( long ) unit;
                for ( long i = 0 ; i < cpSz ; i++, p8++ )
                    sb.AppendFormat( "{0:x2}", *p8 );
            }
            finally { gch.Free(); }

            return sb.ToString();
        }
    }

}

