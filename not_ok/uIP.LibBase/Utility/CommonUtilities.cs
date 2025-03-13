using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace uIP.LibBase.Utility
{
    public static class CommonUtilities
    {
        public static string GetCurrentTimeStr(string splitStr = "_")
        {
            return String.Format( "{0:0000}{7}{1:00}{7}{2:00}{7}{3:00}{7}{4:00}{7}{5:00}{7}{6:000}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, splitStr );
        }

        public static string RemoveControlNonPrintableChars( string s, bool bIncSpace = false )
        {
            if ( string.IsNullOrEmpty( s ) ) return "";
            string repo = Regex.Replace( s, @"\p{C}+", string.Empty );
            return bIncSpace ? Regex.Replace( repo, @"\s+", string.Empty ) : repo;
        }

        public static string RemoveEndDirSymbol(string path)
        {
            if ( String.IsNullOrEmpty( path ) ) return path;
            char[] tmpCharArr = path.ToCharArray();
            return (tmpCharArr[ tmpCharArr.Length - 1 ] == '\\' || tmpCharArr[ tmpCharArr.Length - 1 ] == '/') ?
                   new string( tmpCharArr, 0, tmpCharArr.Length - 1 ) : path;
        }

        private static int CompareDirFilesDiffVals( DirFilesCmpInfo x1, DirFilesCmpInfo x2 )
        {
            if ( x1 == null && x2 == null )
                return 0;

            if ( x1 == null || x2 == null )
            {
                if ( x1 == null && x2 != null )
                    return -1;
                return 1;
            }

            if ( x1.Diff > x2.Diff )
                return 1;
            else if ( x1.Diff < x2.Diff )
                return -1;
            return 0;
        }

        internal class DirFilesCmpInfo
        {
            private long _n64Diff = 0;
            private int _n32Index = -1;

            internal long Diff
            {
                get { return _n64Diff; }
                set { _n64Diff = value; }
            }

            internal int Index
            {
                get { return _n32Index; }
                set { _n32Index = value; }
            }

            internal DirFilesCmpInfo() { }
            internal DirFilesCmpInfo( long diff, int index ) { _n64Diff = diff; _n32Index = index; }
        }

        public static void MaintainDirFileCount( string dirPath, out int nNumOfFiles, int nMaxFiles, string bakDirPath = null )
        {
            nNumOfFiles = 0;
            if ( String.IsNullOrEmpty( dirPath ) || !Directory.Exists( dirPath ) )
                return;

            bool bBackupDirExist = String.IsNullOrEmpty( bakDirPath ) ? false : Directory.Exists( bakDirPath );

            string[] fileNms = Directory.GetFiles( dirPath, "*.*", SearchOption.TopDirectoryOnly );
            // No file in dir, reset
            if ( fileNms == null || fileNms.Length <= 0 )
                nNumOfFiles = 0;
            else if ( fileNms.Length > nMaxFiles )
            {
                List<DirFilesCmpInfo> tmpList = new List<DirFilesCmpInfo>();
                long currTicks = DateTime.UtcNow.Ticks;
                int fileCnt = 0;

                for ( int i = 0 ; i < fileNms.Length ; i++ )
                {
                    if ( String.IsNullOrEmpty( fileNms[ i ] ) || !File.Exists( fileNms[ i ] ) )
                        continue;

                    // Get file last access time and find out a file not accessed for a long time.
                    long fileTicks = File.GetLastWriteTimeUtc( fileNms[ i ] ).Ticks;
                    tmpList.Add( new DirFilesCmpInfo( fileTicks - currTicks, i ) );

                    fileCnt++;
                }

                tmpList.Sort( CompareDirFilesDiffVals );

                if ( fileCnt > 0 )
                {
                    if ( fileCnt > nMaxFiles )
                    {
                        for ( int i = 0 ; i < tmpList.Count ; i++ )
                        {
                            if ( tmpList[ i ] == null )
                                continue;
                            if ( tmpList[ i ].Index < 0 || tmpList[ i ].Index >= fileNms.Length || String.IsNullOrEmpty( fileNms[ tmpList[ i ].Index ] ) )
                            {
                                tmpList[ i ] = null;
                                continue;
                            }

                            if ( File.Exists( fileNms[ tmpList[ i ].Index ] ) )
                            {
                                if ( bBackupDirExist )
                                {
                                    string trgPath = Path.Combine( bakDirPath, Path.GetFileName( fileNms[ tmpList[ i ].Index ] ) );
                                    File.Copy( fileNms[ tmpList[ i ].Index ], trgPath, true );
                                }

                                File.Delete( fileNms[ tmpList[ i ].Index ] );
                                tmpList[ i ] = null;

                                if ( --fileCnt <= nMaxFiles )
                                    break;
                            }
                        } // end for-i

                        nNumOfFiles = 0;
                        for ( int i = 0 ; i < tmpList.Count ; i++ )
                        {
                            if ( tmpList[ i ] != null )
                                nNumOfFiles++;
                        }
                    } // if- fileCnt > nMaxFiles
                } // if-fileCnt
            }

            return;
        }

        public static bool CreateDir(string dir, int nWaitAWhile = 100)
        {
            if ( String.IsNullOrEmpty( dir ) )
                return false;

            bool bRet = true;

            if ( !Directory.Exists( dir ) )
            {
                try
                {
                    Directory.CreateDirectory( dir );
                    Thread.Sleep( nWaitAWhile );
                }
                catch { bRet = false; }
            }

            return bRet;
        }
        public static bool RCreateDir(string path, int sleep = 100)
        {
            string curPath = null;
            //path = Path.GetFullPath( path );
            int semPos = path.IndexOf(':');
            if (semPos == 0)
                return false;
            else if (semPos > 0)
            {
                curPath = path.Substring(0, semPos + 1);
                curPath += "\\";
                path = path.Substring(semPos + 1, path.Length - (semPos + 1));
            }


            string[] pathes = path.Split('\\', '/');
            if (pathes == null)
                return false;
            if (pathes.Length == 1)
            {
                return CreateDir(pathes[0], sleep);
            }

            int i = String.IsNullOrEmpty(curPath) ? 1 : 0;
            if (String.IsNullOrEmpty(curPath)) curPath = pathes[0];
            for (; i < pathes.Length; i++)
            {
                if (String.IsNullOrEmpty(pathes[i]))
                    continue;
                curPath = Path.Combine(curPath, pathes[i]);
                if (!CreateDir(curPath))
                    return false;
            }

            return true;
        }

        public static string RCreateDir2( string path, int sleep = 100 )
        {
            if ( !RCreateDir( path, sleep ) ) return "";
            return path;
        }

        public static bool NewFile( string filepath )
        {
            bool ret = true;
            try
            {
                using ( Stream rw = File.Open( filepath, FileMode.Create ) )
                { }
            }
            catch { ret = false; }
            return ret;
        }

        private delegate IntPtr fpMemAlloc( int cb );
        private static IntPtr StringToIntptr<T>(string str, Encoding enc, fpMemAlloc fpAllocMem, out int lenInByte )
        {
            lenInByte = 0;
            if ( typeof( T ) != typeof( byte ) && typeof( T ) != typeof( Int16 ) && typeof(T) != typeof(char) )
                return IntPtr.Zero;
            if ( String.IsNullOrEmpty( str ) || enc == null )
                return IntPtr.Zero;

            int unit = typeof(T) == typeof(char) ? sizeof(char) : Marshal.SizeOf( typeof( T ) );

            byte[] bytes = enc.GetBytes( str );
            lenInByte = bytes.Length + 1 * unit;
            IntPtr pMem = fpAllocMem( lenInByte );
            Marshal.Copy( bytes, 0, pMem, bytes.Length );

            unsafe
            {
                if ( unit == 1 ) {
                    byte* p8 = ( byte* ) pMem.ToPointer();
                    p8[ bytes.Length ] = 0;
                } else {
                    Int16* p16 = ( Int16* ) pMem.ToPointer();
                    p16[ bytes.Length / unit ] = 0;
                }
            }
            return pMem;
        }

        public static IntPtr StringToIntptrHGlobal<T>(string str, Encoding enc, out int lenInByte )
        {
            return StringToIntptr<T>( str, enc, Marshal.AllocHGlobal, out lenInByte );
        }
        public static IntPtr StringToIntptrCoTask<T>( string str, Encoding enc, out int lenInByte )
        {
            return StringToIntptr<T>( str, enc, Marshal.AllocCoTaskMem, out lenInByte );
        }
        public static string IntptrToString( IntPtr pStr, int nStr, Encoding enc )
        {
            if ( nStr <= 0 || pStr == IntPtr.Zero )
                return null;

            byte[] bytes = new byte[ nStr ];
            Marshal.Copy( pStr, bytes, 0, nStr );
            string got = null;

            try {
                got = enc.GetString( bytes );
                got = got.Replace( "\0", "" );
            } catch { return null; }

            return got;
        }
        public static string IntptrToAsciiString(IntPtr pStr)
        {
            if ( pStr == IntPtr.Zero ) return null;

            string ret = null;
            unsafe
            {
                byte* p8 = (byte*) pStr.ToPointer();
                int cnt = 0;
                while(true) {
                    if ( p8[ cnt ] == 0 || p8[cnt] == '\0' )
                        break;
                    cnt++;
                }

                if ( cnt == 0 )
                    return null;

                byte[] tmp = new byte[ cnt ];
                for ( int i = 0; i < cnt; i++ ) tmp[ i ] = p8[ i ];

                ret = Encoding.ASCII.GetString( tmp );
            }

            return ret;
        }
        public static void StringByteArrToIntptr(byte[] str, IntPtr p, int nSz)
        {
            if ( str == null || str.Length <= 0 || p == IntPtr.Zero || nSz <= 0 )
                return;
            unsafe
            {
                byte* p8 = ( byte* ) p.ToPointer();
                int i = 0;
                for ( ; i < str.Length && i < nSz; i++ ) p8[ i ] = str[ i ];
                if ( i < nSz ) p8[ i ] = 0; // append the EOS
            }
        }

        public static byte[] Utf8StringToByteArr(string str)
        {
            if ( String.IsNullOrEmpty( str ) ) return null;

            byte[] tmp = Encoding.UTF8.GetBytes( str );
            byte[] ret = new byte[ tmp.Length + 1 ];
            unsafe
            {
                fixed(byte *pDst = ret)
                {
                    Marshal.Copy( tmp, 0, new IntPtr( ( void* ) pDst ), tmp.Length );
                }
            }
            ret[ tmp.Length ] = 0;
            return ret;
        }

        public static string Utf8IntptrToString(IntPtr pBuff)
        {
            if (pBuff == IntPtr.Zero)
                return null;
            string ret = "";
            byte[] b = null;
            unsafe
            {
                byte* p8 = (byte*)pBuff.ToPointer();
                int cnt = 0;
                while (p8[cnt] != 0 && p8[cnt] != '\0')
                {
                    cnt++;
                }
                if (cnt > 0)
                {
                    b = new byte[cnt];
                    Marshal.Copy(pBuff, b, 0, cnt);
                }
            }
            if (b != null)
                ret = Encoding.UTF8.GetString(b);
            return ret;
        }

        public static string ToAnsiString(string str, int nMax)
        {
            if ( String.IsNullOrEmpty( str ) ) return str;
            byte[] bts = Encoding.ASCII.GetBytes( str );
            return Encoding.ASCII.GetString( bts, 0, bts.Length > nMax ? nMax : bts.Length );
        }

        public static string ToUtf8String( string s )
        {
            if ( string.IsNullOrEmpty( s ) ) return "";
            byte[] ba = Encoding.UTF8.GetBytes( s );
            return Encoding.UTF8.GetString( ba );
        }

        public static List<T> PureShallowCopyList<T>(List<T> src)
        {
            if ( src == null ) return null;
            List<T> dst = new List<T>();
            foreach ( T item in src )
                dst.Add( item );
            return dst;
        }

        public static string[] MySplit( string str, string sp )
        {
            if ( String.IsNullOrEmpty( sp ) || String.IsNullOrEmpty( str ) ) return new string[] { str };

            List<string> ret = new List<string>();
            int index = str.IndexOf( sp );
            if ( index < 0 ) {
                return new string[] { str };
            }

            ret.Add( str.Substring( 0, index ) );
            try { str = str.Substring( index + sp.Length ); } catch {
                return ret.ToArray();
            }
            if ( String.IsNullOrEmpty( str ) )
                return ret.ToArray();

            while ( true ) {
                index = str.IndexOf( sp );
                if ( index < 0 ) {
                    ret.Add( str );
                    break;
                }

                ret.Add( str.Substring( 0, index ) );
                try { str = str.Substring( index + sp.Length ); } catch {
                    break;
                }
                if ( String.IsNullOrEmpty( str ) ) break;
            }
            return ret.ToArray();
        }

        public static void AddResolveCurrDomainAssembly()
        {
            if ( _bEverPlugResolve ) return;
            _bEverPlugResolve = true;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveCurrDomainAssembly;
        }
        private static bool _bEverPlugResolve = false;
        private static Assembly ResolveCurrDomainAssembly( Object sender, ResolveEventArgs args )
        {
            Assembly[] curr = AppDomain.CurrentDomain.GetAssemblies();
            if ( curr == null || curr.Length <= 0 )
                return null;

            string[] fields = args.Name.Split( ',' );
            string assemNm = fields[ 0 ];

            for ( int i = 0; i < curr.Length; i++ ) {
                if ( curr[ i ] == null || String.IsNullOrEmpty( curr[ i ].FullName ) )
                    continue;
                string[] tmp = curr[ i ].FullName.Split( ',' );
                if ( assemNm == tmp[ 0 ] )
                    return curr[ i ];
            }

            return null;
        }

        public static T[] VariableMakeArr<T>(params T[] input)
        {
            return input;
        }

        public static string StringConvert( string src, Encoding srcEnc, Encoding dstEnc )
        {
            if ( String.IsNullOrEmpty( src ) || srcEnc == null || dstEnc == null ) return "";
            byte[] srcBA = srcEnc.GetBytes( src );
            byte[] dstBA = Encoding.Convert( srcEnc, dstEnc, srcBA );
            return dstEnc.GetString( dstBA );
        }

        public static bool IsMethodCompatibleWithDelegate<T>(MethodInfo method) where T : class
        {
            Type delegateType = typeof(T);
            MethodInfo delegateSignature = delegateType.GetMethod("Invoke");
            if ( delegateSignature == null ) return false;

            bool parametersEqual = delegateSignature
                .GetParameters()
                .Select(x => x.ParameterType)
                .SequenceEqual(method.GetParameters()
                    .Select(x => x.ParameterType));

            return delegateSignature.ReturnType == method.ReturnType &&
                   parametersEqual;
        }
    }
}
