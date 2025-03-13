using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    /// <summary>
    /// 基礎描述，用於設定
    /// - 名稱
    /// - 型別: Type
    /// - 如果要進行參數儲存 m_bParam 要設定成 true, 無論是 macro 或是 plugin class
    /// </summary>
    public class UScriptControlCarrier
    {
        protected string m_strName;
        protected bool m_bAbleGet;
        protected bool m_bAbleSet;
        protected bool m_bParam;
        protected UDataCarrierTypeDescription[] m_TypesOfData;
        protected eUScriptControlCarrierSpreading m_HowToSpreading = eUScriptControlCarrierSpreading.Undefine;

        public string Name
        {
            get { return m_strName; }
            set { m_strName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); }
        }

        public bool CanGet
        {
            get { return m_bAbleGet; }
            set { m_bAbleGet = value; }
        }

        public bool CanSet
        {
            get { return m_bAbleSet; }
            set { m_bAbleSet = value; }
        }

        public bool CanStore
        {
            get { return m_bParam; }
            set { m_bParam = value; }
        }

        public eUScriptControlCarrierSpreading HowToSpread
        {
            get { return m_HowToSpreading; }
            set { m_HowToSpreading = value; }
        }

        public UDataCarrierTypeDescription[] DataTypes
        {
            get { return m_TypesOfData; }
            set { m_TypesOfData = value; }
        }

        public UScriptControlCarrier()
        {
            m_strName = null;
            m_bAbleGet = false;
            m_bAbleSet = false;
            m_bParam = false;
            m_TypesOfData = null;
        }

        public UScriptControlCarrier( string givenNameOfCarrier, bool canGet, bool canSet, bool canStore, UDataCarrierTypeDescription[] typeDescription )
        {
            m_strName = String.IsNullOrEmpty( givenNameOfCarrier ) ? null : String.Copy( givenNameOfCarrier );
            m_bAbleGet = canGet;
            m_bAbleSet = canSet;
            m_bParam = canStore;
            m_TypesOfData = typeDescription;
        }

        public bool CheckType( UDataCarrier[] data )
        {
            if ( m_TypesOfData == null )
                return true;

            if ( data == null || data.Length < m_TypesOfData.Length )
                return false;

            bool bRet = true;
            for ( int i = 0 ; i < m_TypesOfData.Length ; i++ )
            {
                if ( m_TypesOfData[ i ] == null )
                    continue;
                if ( data[ i ] == null )
                {
                    bRet = false; break;
                }
                if ( data[ i ].Tp != m_TypesOfData[ i ].Tp )
                {
                    bRet = false; break;
                }
            }

            return bRet;
        }

        public static UScriptControlCarrier Clone( UScriptControlCarrier src)
        {
            if ( src == null ) return null;
            UScriptControlCarrier ret = new UScriptControlCarrier();
            ret.Name = String.IsNullOrEmpty( src.Name ) ? null : String.Copy( src.Name );
            ret.CanGet = src.CanGet;
            ret.CanSet = src.CanSet;
            ret.CanStore = src.CanStore;
            ret.HowToSpread = src.HowToSpread;
            ret.DataTypes = src.DataTypes == null ? null : new UDataCarrierTypeDescription[ src.DataTypes.Length ];
            for ( int i = 0 ; i < src.DataTypes.Length ; i++ )
                ret.DataTypes[ i ] = new UDataCarrierTypeDescription( src.DataTypes[ i ].Tp, String.IsNullOrEmpty( src.DataTypes[ i ].Desc ) ? null : String.Copy( src.DataTypes[ i ].Desc ) );
            return ret;
        }
        public static UScriptControlCarrier[] Clone( UScriptControlCarrier[] src)
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Length ];
            for ( int i = 0 ; i < src.Length ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
        public static UScriptControlCarrier[] Clone( List<UScriptControlCarrier> src)
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Count ];
            for ( int i = 0 ; i < src.Count ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
    }

    /// <summary>
    /// 指名用在 MacroMethodProviderPlug Class 中，用來設定 class instance 中的參數
    /// </summary>
    public class UScriptControlCarrierPluginClass : UScriptControlCarrier
    {
        protected fpSetPluginClassScriptControlCarrier m_fpSet;
        protected fpGetPluginClassScriptControlCarrier m_fpGet;

        public fpSetPluginClassScriptControlCarrier SetParam
        {
            get { return m_fpSet; }
            set { m_fpSet = value; }
        }

        public fpGetPluginClassScriptControlCarrier GetParam
        {
            get { return m_fpGet; }
            set { m_fpGet = value; }
        }

        public UScriptControlCarrierPluginClass()
            : base()
        {
            m_fpSet = null;
            m_fpGet = null;
        }

        public UScriptControlCarrierPluginClass( string givenNameOfCarrier, bool canGet, bool canSet, bool canStore, UDataCarrierTypeDescription[] typeDescription, fpGetPluginClassScriptControlCarrier fpGet, fpSetPluginClassScriptControlCarrier fpSet )
            : base( givenNameOfCarrier, canGet, canSet, canStore, typeDescription )
        {
            m_fpSet = fpSet;
            m_fpGet = fpGet;
        }

        public static UScriptControlCarrier[] CloneDescs( Dictionary<string, UScriptControlCarrierPluginClass> src )
        {
            if ( src == null || src.Count <= 0 ) return null;
            List<UScriptControlCarrier> ret = new List<UScriptControlCarrier>();
            foreach(var kv in src)
            {
                if ( kv.Value != null ) ret.Add( kv.Value );
            }
            return ret.ToArray();
        }
        public static UScriptControlCarrier[] CloneDescs( UScriptControlCarrierPluginClass[] src )
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Length ];
            for ( int i = 0 ; i < src.Length ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
        public static UScriptControlCarrier[] CloneDescs( List<UScriptControlCarrierPluginClass> src )
        {
            if ( src == null ) return null;
            UScriptControlCarrier[] ret = new UScriptControlCarrier[ src.Count ];
            for ( int i = 0 ; i < src.Count ; i++ )
                ret[ i ] = UScriptControlCarrier.Clone( src[ i ] );

            return ret;
        }
    }
}
