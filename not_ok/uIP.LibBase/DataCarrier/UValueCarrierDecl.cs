using System;
using System.Text;
using System.Runtime.InteropServices;

namespace uIP.LibBase.DataCarrier
{
    public enum eUValueCarrierDatType : int
    {
        Single = 0,
        Enumeration,
        Array,
    }

    public class UValueCarrierDescrption
    {
        protected string _strGivenName = null;
        public string GivenName { get { return _strGivenName; } set { _strGivenName = value; } }
        public UValueCarrierDescrption() { }
    }

    public class UValueCarrierSingleOne<T> : UValueCarrierDescrption
    {
        protected T _Value;
        protected T _ValueMin;
        protected T _ValueMax;
        protected T _ValueDef;
        protected T _ValueUnit;

        public T Value { get { return _Value; } set { _Value = value; } }
        public T RangeMin { get { return _ValueMin; } set { _ValueMin = value; } }
        public T RangeMax { get { return _ValueMax; } set { _ValueMax = value; } }
        public T Unit { get { return _ValueUnit; } set { _ValueUnit = value; } }

        public UValueCarrierSingleOne()
            : base()
        {
            if ( typeof( T ).IsArray )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept array type." );
            if ( typeof( T ).IsEnum )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept enum type." );
            if ( typeof( T ) == typeof( IntPtr ) )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept ptr." );
        }

        public UValueCarrierSingleOne( T val, T min, T max, T def, T unit )
            : base()
        {
            if ( typeof( T ).IsArray )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept array type." );
            if ( typeof( T ).IsEnum )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept enum type." );
            if ( typeof( T ) == typeof( IntPtr ) )
                throw new Exception( "[UValueCarrierSingleOne::UValueCarrierSingleOne] cannot accept ptr." );

            _Value = val;
            _ValueMin = min;
            _ValueMax = max;
            _ValueDef = def;
            _ValueUnit = unit;
        }
    }

    public class UValueCarrierArray<T, ArrT> : UValueCarrierDescrption
    {
        protected T _Value;
        protected T _ValueDef;
        protected Int32 _nDim = 0;
        protected Int32 _nArrItemUnit = 0;

        public T Value { get { return _Value; } set { _Value = value; } }

        public UValueCarrierArray()
            : base()
        {
            if ( !typeof( T ).IsArray )
                throw new Exception( "[UValueCarrierArray::UValueCarrierArray] cannot accept non-array type." );
            if ( !typeof( ArrT ).IsValueType )
                throw new Exception( "[UValueCarrierArray::UValueCarrierArray] cannot accept array item as non-ValueType." );

            _nArrItemUnit = Marshal.SizeOf( typeof( ArrT ) );
        }

        public UValueCarrierArray( T val, T def )
        {
            if ( !typeof( T ).IsArray )
                throw new Exception( "[UValueCarrierArray::UValueCarrierArray] cannot accept non-array type." );
            if ( !typeof( ArrT ).IsValueType )
                throw new Exception( "[UValueCarrierArray::UValueCarrierArray] cannot accept array item as non-ValueType." );

            _Value = val;
            _ValueDef = def;
            _nArrItemUnit = Marshal.SizeOf( typeof( ArrT ) );
        }

        public unsafe string Convert( out Int32[] dimLen )
        {
            dimLen = null;

            object tmp = ( object ) _Value;
            if ( tmp == null ) return null;

            Array arr = _Value as Array;
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
                long cpSz = totalSz * ( long ) _nArrItemUnit;
                for ( long i = 0 ; i < cpSz ; i++, p8++ )
                    sb.AppendFormat( "{0:x2}", *p8 );
            }
            finally { gch.Free(); }

            return sb.ToString();
        }
    }


    public class UValueCarrierEnum<T> : UValueCarrierDescrption
    {
        protected T _Value;
        protected T _ValueDef;
        protected string[] _EnumList = null;

        public UValueCarrierEnum()
            : base()
        {
            if ( !typeof( T ).IsEnum )
                throw new Exception( "[UValueCarrierEnum::UValueCarrierEnum] cannot accept non-enum type." );
        }

        public UValueCarrierEnum( T val, T def )
        {
            if ( !typeof( T ).IsEnum )
                throw new Exception( "[UValueCarrierEnum::UValueCarrierEnum] cannot accept non-enum type." );
            _Value = val;
            _ValueDef = def;
        }
    }
}