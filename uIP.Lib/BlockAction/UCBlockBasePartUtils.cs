using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        protected static string DefWorkDir = Directory.GetCurrentDirectory();
        // just use the pParam and not handle the parameter
        public static Int32 RunBlock( UCBlockBase pBlock, UDataCarrierSet pParam )
        {
            if ( pBlock == null )
                return ( int ) UCBlockBaseRet.RET_STATUS_NG;

            return pBlock.Run( pParam );
        }

        //
        // Check
        //
        public bool IsRunning {
            get {
                return ( 
                    ( _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_FINISH || _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_NA || _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_ERROR ) &&
                    ( _nState == (int) UCBlockStateReserved.UCBLOCK_STATE_PREPARING ) ) ? false : true;
            }
        }
        public bool IsInitState {
            get {
                return ( _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_NA && _nState == (int) UCBlockStateReserved.UCBLOCK_STATE_PREPARING );
            }
        }
        public bool IsOkDone {
            get {
                return ( _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_FINISH && _nState == (int) UCBlockStateReserved.UCBLOCK_STATE_PREPARING );
            }
        }
        public bool IsErrorDone {
            get {
                return ( _nPrevState == (int) UCBlockStateReserved.UCBLOCK_STATE_ERROR && _nState == (int) UCBlockStateReserved.UCBLOCK_STATE_PREPARING );
            }
        }

        protected static string GetState<T>(int val)
        {
            if (Enum.IsDefined(typeof(T), val)) {
                return Enum.GetName( typeof( T ), val );
            }
            return "";
        }

        protected List<object> OwnerObjectList {
            get {
                List<object> ret = new List<object>();
                object next = Owner;
                while(next != null)
                {
                    ret.Insert(0, next);
                    if ( next is UCBlockBase blk )
                        next = blk.Owner;
                    else
                        next = null;
                }
                return ret;
            }
        }
        protected List<string> OwnerNameList {
            get {
                List<string> ret = new List<string>();
                object next = Owner;
                while(next != null)
                {
                    ret.Insert(0, next.GetType().Name);
                    if (next is UCBlockBase blk )
                        next = blk.Owner;
                    else next = null;
                }
                return ret;
            }
        }
        protected string PackOwnerString(string prepend = "", string append = "", string joinStr = "->", string emptyDefault = "")
        {
            var lst = OwnerNameList;
            if ( lst.Count <= 0 )
                return emptyDefault;
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty( prepend ) ) sb.Append( prepend );
            sb.Append( string.Join( joinStr, lst.ToArray() ) );
            if (!string.IsNullOrEmpty(append) ) sb.Append( append );
            return sb.ToString();
        }
        protected string PrependOwnerString(string s, string prefix="->")
        {
            return PackOwnerString( append: $"{prefix}{s}", emptyDefault: s );
        }

        protected string GetRwDir(string defRoot = "", string defFolder = "", bool createIfNotExist = true)
        {
            if (!string.IsNullOrEmpty(_strRwDir))
            {
                if (createIfNotExist)
                {
                    if (!Directory.Exists(_strRwDir))
                    {
                        try
                        { Directory.CreateDirectory( _strRwDir ); } catch { return null; }
                    }
                    return _strRwDir;
                }
            }
            if ( string.IsNullOrEmpty( defFolder ) )
                return null;
            var dir = Path.Combine(string.IsNullOrEmpty(defRoot) ? DefWorkDir : defRoot, defFolder);
            if ( createIfNotExist )
            {
                if ( !Directory.Exists( dir ) )
                {
                    try
                    { Directory.CreateDirectory( dir ); } catch { return null; }
                }
            }
            return dir;
        }

        public static Dictionary<int, T> GetEnumDict<T>()
        {
            try
            {
                var ev = Enum.GetValues( typeof( T ) );
                var r = new Dictionary<int, T>();
                for(int i = 0; i < ev.Length; i++ )
                {
                    r.Add( ( int ) ev.GetValue( i ), ( T ) ev.GetValue( i ) );
                }
                return r;
            } catch { return null; }
        }
        public static Dictionary<int, object> GetEnumDic( Dictionary<int, object> input, params Type [] types )
        {
            try
            {
                if ( types == null )
                    return input;
                if (input == null ) input = new Dictionary<int, object>();
                foreach(var t in types )
                {
                    var ev = Enum.GetValues ( t );
                    for(int i = 0; i < ev.Length;i++ )
                    {
                        input.Add((int)ev.GetValue( i ), ev.GetValue( i ) );
                    }
                }
                return input;
            } catch { return input; }
        }
    }
}
