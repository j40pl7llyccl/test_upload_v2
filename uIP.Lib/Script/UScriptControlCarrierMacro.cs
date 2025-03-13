using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    /// <summary>
    /// 指定用在設定 Macro 中所開放的參數設定
    /// </summary>
    public class UScriptControlCarrierMacro : UScriptControlCarrier
    {
        protected fpSetMacroScriptControlCarrier m_fpSet;
        protected fpGetMacroScriptControlCarrier m_fpGet;

        public fpSetMacroScriptControlCarrier SetParam
        {
            get { return m_fpSet; }
            set { m_fpSet = value; }
        }

        public fpGetMacroScriptControlCarrier GetParam
        {
            get { return m_fpGet; }
            set { m_fpGet = value; }
        }

        public UScriptControlCarrierMacro()
            : base()
        {
            m_fpSet = null;
            m_fpGet = null;
        }

        public UScriptControlCarrierMacro( string givenNameOfCarrier, bool canGet, bool canSet, bool canStore, UDataCarrierTypeDescription[] typeDescription, fpGetMacroScriptControlCarrier fpGet, fpSetMacroScriptControlCarrier fpSet )
            : base( givenNameOfCarrier, canGet, canSet, canStore, typeDescription )
        {
            m_fpSet = fpSet;
            m_fpGet = fpGet;
        }

        public static UScriptControlCarrierMacro Get( List<UScriptControlCarrierMacro> carriers, string queryGivenNameOfCarrier )
        {
            if ( carriers == null || carriers.Count <= 0 || String.IsNullOrEmpty( queryGivenNameOfCarrier ) ) return null;
            for ( int i = 0 ; i < carriers.Count ; i++ )
            {
                if ( carriers[ i ] == null || String.IsNullOrEmpty( carriers[ i ].Name ) ) continue;
                if ( carriers[ i ].Name == queryGivenNameOfCarrier ) return carriers[ i ];
            }

            return null;
        }

        public static UScriptControlCarrierMacro Get( UScriptControlCarrierMacro[] carriers, string queryGivenNameOfCarrier )
        {
            if ( carriers == null || carriers.Length <= 0 || String.IsNullOrEmpty( queryGivenNameOfCarrier ) )
                return null;

            for ( int i = 0 ; i < carriers.Length ; i++ )
            {
                if ( carriers[ i ] == null || String.IsNullOrEmpty( carriers[ i ].Name ) ) continue;
                if ( carriers[ i ].Name == queryGivenNameOfCarrier ) return carriers[ i ];
            }
            return null;
        }

        public static UScriptControlCarrier[] CloneDescs( Dictionary<string, UScriptControlCarrierMacro> src )
        {
            if ( src == null || src.Count <= 0 ) return null;
            List<UScriptControlCarrier> ret = new List<UScriptControlCarrier>();
            foreach ( var kv in src )
            {
                if ( kv.Value != null ) ret.Add( kv.Value );
            }
            return ret.ToArray();
        }
        public static UScriptControlCarrier[] CloneDescs( UScriptControlCarrierMacro[] src )
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Length ];
            for ( int i = 0 ; i < src.Length ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
        public static UScriptControlCarrier[] CloneDescs( List<UScriptControlCarrierMacro> src )
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Count ];
            for ( int i = 0 ; i < src.Count ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
    }

}
