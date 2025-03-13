using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace uIP.LibBase.Utility
{
    public class KeyValues
    {
        private string _Key = null;
        private string[] _Values = null;

        public string Key { get { return _Key; } set { _Key = value; } }
        public string[] Values { get { return _Values; } set { _Values = value; } }

        public KeyValues() { }
        public KeyValues(string key, string[] vals)
        {
            _Key = key;
            _Values = vals;
        }
    }

    public enum eIniContentEncoding
    {
        Ansi,
        Unicode,
        Utf7,
        Utf8,
        Utf32
    }


    public class SectionDataOfIni
    {
        private string m_strSecName = null;
        private List<KeyValues> m_listData = new List<KeyValues>();

        public SectionDataOfIni() { }
        public string SectionName {
            get { return m_strSecName; }
            set { m_strSecName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); }
        }
        public List<KeyValues> Data {
            get { return m_listData; }
            set { m_listData = value; }
        }
        private static string[] TrimReplace(string[] arr, string[] rmvStr)
        {
            if ( rmvStr == null || rmvStr.Length <= 0 ) return arr;

            List< string > tmpL = arr == null || arr.Length <= 0 ? new List< string >() : arr.ToList();
            List<string> repo = new List< string >();
            foreach ( var item in tmpL )
            {
                string tmp = item;
                tmp = tmp.Trim();
                foreach ( var rmv in rmvStr )
                {
                    if (string.IsNullOrEmpty( rmv )) continue;
                    tmp = tmp.Replace( rmv, "" );
                }
                repo.Add( tmp );
            }

            return repo.ToArray();
        }

        public string[] GetValue( string key, bool bIgnoreCase = false, string[] trimToRmvSimbols = null )
        {
            if ( m_listData == null || m_listData.Count <= 0 )
                return null;

            key = bIgnoreCase ? key.ToLower() : key;
            for ( int i = 0; i < m_listData.Count; i++ ) {
                string cur = bIgnoreCase ? m_listData[ i ].Key.ToLower() : m_listData[ i ].Key;
                if ( cur == key ) {
                    return TrimReplace(m_listData[ i ].Values, trimToRmvSimbols);
                }
            }

            return null;
        }
        public List<string[]> GetValues( string key, bool bIgnoreCase = false, string[] trimToRmvSimbols = null )
        {
            if ( m_listData == null || m_listData.Count <= 0 )
                return null;

            key = bIgnoreCase ? key.ToLower() : key;
            List<string[]> ret = new List<string[]>();
            for ( int i = 0; i < m_listData.Count; i++ ) {
                string cur = bIgnoreCase ? m_listData[ i ].Key.ToLower() : m_listData[ i ].Key;
                if ( cur == key )
                {
                    ret.Add( TrimReplace( m_listData[ i ].Values, trimToRmvSimbols ) );
                }
            }

            return ret;
        }
    }

    /// <summary>
    /// Parsing following
    /// - Format
    /// // comment here
    /// [section name]
    /// key=val_1, val_2; val_3
    /// 
    /// - Paramerter
    ///   * change the split symbol using ValSplitSymbals, and default are ',' and ';'
    ///   * if default parsing buffer may not enought and can be changed using constructor
    ///     - nMaxSectionNm: default 10k
    ///     - nMaxSecData: default 10k
    ///     - nPerLineMax: 1k
    ///
    /// - Call
    ///   * new a instance
    ///   * Parsing()
    ///   * Get()
    /// </summary>
    public class IniReaderUtility
    {
        protected static char[] _strLineSplitToken = new char[] { '\n', '\r', '\0', ( char ) 0 };
        protected static byte[] _byteLineSplitToken = new byte[] { ( Byte ) '\n', ( Byte ) '\r', ( Byte ) '\0', ( Byte ) '\t', ( Byte ) 0 };

        [DllImport( "Kernel32.dll", EntryPoint = "GetPrivateProfileSectionA" )]
        internal static extern Int32 GetProfileSectionAnsi( string sectionStr, ref byte lpReturnedString, Int32 nSize, string lpFileName );

        [DllImport( "Kernel32.dll", EntryPoint = "GetPrivateProfileSectionNamesA" )]
        internal static extern Int32 GetProfileSectionNamesAnsi( ref byte lpszReturnBuffer, Int32 nSize, string lpFileName );

        //[DllImport( "Kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPrivateProfileSectionW" )]
        //internal static extern Int32 GetProfileSectionUnicode( string sectionStr, ref byte lpReturnedString, Int32 nSize, string lpFileName );

        //[DllImport( "Kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPrivateProfileSectionNamesW" )]
        //internal static extern Int32 GetProfileSectionNamesUnicode( ref byte lpszReturnBuffer, Int32 nSize, string lpFileName );

        protected char[] m_ValSplitSymbols = new char[] { ',', ';' };
        protected eIniContentEncoding m_EncodeContentType = eIniContentEncoding.Ansi;

        protected Int32 m_nMaxSectionsNmBuffer = 1024 * 100;
        protected Int32 m_nMaxSectionDataBuffer = 1024 * 100;
        protected Int32 m_nMaxTmpBuffer = 1024;
        protected List<SectionDataOfIni> m_listData = new List<SectionDataOfIni>();

        public char[] ValSplitSymbals { get { return m_ValSplitSymbols; } set { m_ValSplitSymbols = value; } }
        public eIniContentEncoding EncodeContentType { get { return m_EncodeContentType; } set{ m_EncodeContentType = value; } }

        public List<SectionDataOfIni> Data
        {
            get { return m_listData; }
        }

        public IniReaderUtility() { }
        public IniReaderUtility( int nMaxSectionNm, int nMaxSecData, int nPerLineMax )
        {
            m_nMaxSectionsNmBuffer = nMaxSectionNm;
            m_nMaxSectionDataBuffer = nMaxSecData;
            m_nMaxTmpBuffer = nPerLineMax;
        }

        public bool Parsing( string strFilePath )
        {
            // Clear data first
            m_listData.Clear();

            // Check file
            if ( String.IsNullOrEmpty( strFilePath ) || !File.Exists( strFilePath ) ) return false;

            // Get sections
            List<string> sectionList = GetSections( strFilePath );

            // Get each data in section
            GetSectionData( sectionList, strFilePath );

            return true;
        }

        public SectionDataOfIni Get( string sectionNm )
        {
            if ( String.IsNullOrEmpty( sectionNm ) || m_listData == null || m_listData.Count <= 0 ) return null;

            SectionDataOfIni ret = null;
            for ( int i = 0 ; i < m_listData.Count ; i++ )
            {
                if ( m_listData[ i ] == null || String.IsNullOrEmpty( m_listData[ i ].SectionName ) )
                    continue;
                if ( m_listData[ i ].SectionName == sectionNm )
                {
                    ret = m_listData[ i ]; break;
                }
            }

            return ret;
        }

        private static string[] GetVFromK( SectionDataOfIni dat, string k )
        {
            if ( dat == null || string.IsNullOrEmpty( k ) )
                return null;
            if ( dat.Data == null || dat.Data.Count <= 0 )
                return null;

            for(int i = 0; i < dat.Data.Count; i++ ) {
                if (dat.Data[i].Key == k) {
                    return dat.Data[ i ].Values;
                }
            }
            return null;
        }
        public bool Get(string sectionNm, List<string> keys, ref List<string[]> values)
        {
            if ( values != null ) values.Clear();
            else values = new List<string[]>();

            SectionDataOfIni dat = Get( sectionNm );
            if ( dat == null )
                return false;

            for ( int i = 0; i < keys.Count; i++ )
                values.Add( GetVFromK( dat, keys[ i ] ) );
            return true;
        }

        public bool Get( string sectionNm, out Dictionary< string, string[] > kvs, params string[] keys )
        {
            kvs = new Dictionary< string, string[] >();
            SectionDataOfIni dat = Get( sectionNm );
            if ( dat == null )
                return false;

            for ( int i = 0; i < keys.Length; i++ )
            {
                kvs.Add( keys[i], GetVFromK( dat, keys[ i ] ) );
            }
            return true;
        }
        public bool Get(string sectionNm, ref List<string[]> values, params string[] keys)
        {
            if ( values != null ) values.Clear();
            else values = new List<string[]>();

            if ( keys == null || keys.Length <= 0 )
                return false;

            List<string> lk = new List<string>();
            for ( int i = 0; i < keys.Length; i++ ) lk.Add( keys[ i ] );

            return Get( sectionNm, lk, ref values );
        }

        public string[] GetSections()
        {
            List<string> ret = new List<string>();
            if (m_listData == null || m_listData.Count <= 0)
                return null;
            for (int i = 0; i < m_listData.Count; i++)
            {
                ret.Add(m_listData[i].SectionName);
            }

            return ret.ToArray();
        }

        protected List<string> GetSections( string strFilePath )
        {
            byte[] report = new byte[ m_nMaxSectionsNmBuffer ];
            byte[] asection = new byte[ m_nMaxTmpBuffer ];
            if ( report == null || asection == null ) return null;

            Int32 nReport = GetProfileSectionNamesAnsi( ref report[ 0 ], report.Length, strFilePath );
            if ( nReport <= 0 ) return null;

            List<string> retList = new List<string>();
            int nSectionByteCnt = 0;
            asection[ 0 ] = 0;
            for ( int i = 0 ; i < nReport ; i++ )
            {
                if ( report[ i ] == 0 || report[ i ] == '\0' )
                {
                    // Fill EOS
                    asection[ nSectionByteCnt ] = 0;
                    // Convert
                    string sectionNm = ByteArrToString( asection );
                    if ( !String.IsNullOrEmpty( sectionNm ) )
                        retList.Add( sectionNm );
                    // Reset
                    asection[ 0 ] = 0;
                    nSectionByteCnt = 0;

                    continue;
                }

                if ( nSectionByteCnt < (asection.Length - 1) )
                    asection[ nSectionByteCnt++ ] = report[ i ];
            }

            return retList;
        }

        protected void GetSectionData( List<string> sections, string strFilePath )
        {
            m_listData.Clear();

            if ( sections == null || sections.Count <= 0 ) return;
            for ( int i = 0 ; i < sections.Count ; i++ )
            {
                if ( String.IsNullOrEmpty( sections[ i ] ) ) continue;

                SectionDataOfIni secD = new SectionDataOfIni();
                secD.SectionName = sections[ i ];

                m_listData.Add( secD );
            }

            byte[] buff = new byte[ m_nMaxSectionDataBuffer ];
            if ( buff == null ) return;

            for ( int i = 0 ; i < sections.Count ; i++ )
            {
                if ( String.IsNullOrEmpty( sections[ i ] ) ) continue;

                // variable init
                Int32 nReport = 0;
                // get section data & convert to string
                nReport = GetProfileSectionAnsi( sections[ i ], ref buff[ 0 ], buff.Length, strFilePath );
                if ( nReport <= 0 ) continue;

                // Spilt to line
                string[] lines = ByteArrToStringArr( buff, nReport, _byteLineSplitToken, m_EncodeContentType );
                if ( lines == null || lines.Length <= 0 ) continue;

                // Tmp list
                List<KeyValues> tmpStrList = new List<KeyValues>();

                for ( int j = 0 ; j < lines.Length ; j++ )
                {
                    if ( String.IsNullOrEmpty( lines[ j ] ) ) continue;

                    // convert curr line to char[]
                    string tmpStr = lines[ j ].Trim(); // remove the space of header and tail
                    if ( String.IsNullOrEmpty( tmpStr ) ) continue;

                    char[] lineData = tmpStr.ToCharArray();
                    if ( lineData == null || lineData.Length <= 0 ) continue;
                    // comment check
                    if ( IsComment( lineData ) ) continue;
                    // split by token
                    string[] kvs = tmpStr.Split( '=' );
                    if ( kvs == null || kvs.Length != 2 ) continue;

                    KeyValues kvsR = new KeyValues();
                    List<string> procVs = new List<string>();
                    kvsR.Key = String.IsNullOrEmpty( kvs[ 0 ] ) ? "" : kvs[ 0 ].Trim();

                    string vals = kvs[ 1 ].Trim();
                    if ( String.IsNullOrEmpty( vals ) ) continue;
                    string[] svals = m_ValSplitSymbols == null || m_ValSplitSymbols.Length <= 0 ?
                                     new string[] { vals } : vals.Split( m_ValSplitSymbols );
                    if ( svals == null ) continue;
                    for ( int k = 0 ; k < svals.Length ; k++ )
                    {
                        procVs.Add( String.IsNullOrEmpty( svals[ k ] ) ? "" : svals[ k ].Trim() );
                    }
                    kvsR.Values = procVs.ToArray();

                    tmpStrList.Add( kvsR ); 
                }

                // Need add
                if ( m_listData[ i ] != null )
                    m_listData[ i ].Data = tmpStrList;
            }
        }

        // Check if begin with "//"
        protected bool IsComment( char[] line )
        {
            if ( line == null || line.Length <= 0 )
                return false;

            if ( line[ 0 ] == ';' ) return true;
            if ( line.Length >= 2 && line[ 0 ] == '/' && line[ 1 ] == '/' ) return true;
            return false;
        }

        public static string[] ByteArrToStringArr( byte[] val, int nUsed, byte[] splitTok, eIniContentEncoding encType = eIniContentEncoding.Ansi )
        {
            if ( val == null || val.Length == 0 ) return null;
            Encoding enc = null;
            switch(encType)
            {
                case eIniContentEncoding.Ansi: enc = new ASCIIEncoding(); break;
                case eIniContentEncoding.Unicode: enc = new UnicodeEncoding(); break;
                case eIniContentEncoding.Utf7: enc = new UTF7Encoding(); break;
                case eIniContentEncoding.Utf8: enc = new UTF8Encoding(); break;
                case eIniContentEncoding.Utf32: enc = new UTF32Encoding(); break;
                default: enc = new ASCIIEncoding(); break;
            }
            if ( enc == null ) return null;

            if ( splitTok == null || splitTok.Length <= 0 )
            {
                return (new string[ 1 ] { enc.GetString( val ) });
            }

            List<string> tmpList = new List<string>();
            if ( tmpList == null ) return null;
            int indexBeg = -1, byteCnt = 0;

            for ( int i = 0 ; i < val.Length && i < nUsed ; i++ )
            {
                // Token check
                bool isTok = false;
                if ( splitTok.Length == 1 )
                {
                    isTok = val[ i ] == splitTok[ 0 ];
                }
                else
                {
                    for ( int j = 0 ; j < splitTok.Length ; j++ )
                    {
                        if ( val[ i ] == splitTok[ j ] )
                        {
                            isTok = true; break;
                        }
                    }
                }

                // Get token
                if ( isTok )
                {
                    if ( byteCnt > 0 && indexBeg >= 0 )
                        tmpList.Add( enc.GetString( val, indexBeg, byteCnt ) );

                    indexBeg = -1;
                    byteCnt = 0;
                    continue;
                }

                if ( indexBeg < 0 )
                    indexBeg = i;
                if ( indexBeg >= 0 )
                    byteCnt++;
            }

            if ( byteCnt > 0 && indexBeg >= 0 )
                tmpList.Add( enc.GetString( val, indexBeg, byteCnt ) );

            return (tmpList.Count <= 0 ? null : tmpList.ToArray());
        }

        public static string ByteArrToString( byte[] val )
        {
            if ( val == null || val.Length <= 0 ) return null;

            int nLen = 0;
            for ( int i = 0 ; i < val.Length ; i++ )
            {
                if ( val[ i ] == '\0' || val[ i ] == 0 ) break;
                nLen++;
            }
            if ( nLen <= 0 ) return null;

            byte[] str = new byte[ nLen ];
            for ( int i = 0 ; i < nLen ; i++ )
                str[ i ] = val[ i ];

            ASCIIEncoding enc = new ASCIIEncoding();
            if ( enc == null ) return null;

            return (enc.GetString( str ));
        }

        public static string GetItem( string[] ss, int index, bool bRmvQuote = true, bool bDoTrim = true )
        {
            if ( ss == null || index < 0 || index >= ss.Length )
                return "";

            string s = ss[ index ];
            if ( bDoTrim ) s = s.Trim();
            if ( bRmvQuote ) s = s.Replace( "\"", "" ).Replace( "'", "" );

            return s;
        }
    }
}
