using System;
using System.Collections.Generic;
using System.Xml;
using uIP.LibBase.DataCarrier;

namespace uIP.LibBase.Macro
{
    /// <summary>
    /// Macro produce basic carrier inform storage
    /// </summary>
    public abstract class UMacroProduce : IDisposable
    {
        protected bool _bDispoing = false;
        protected bool _bDisposed = false;

        protected string _strScriptName;
        protected string _strPluginClassName;
        protected string _strMacroMethod;
        protected Int32 _nMacroIndexOfScript;
        protected string _strAppend;

        public bool Invalid { get { return _bDispoing || _bDisposed; } }
        public string ScriptName { get { return _strScriptName; } set { _strScriptName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); } }
        public string PluginClassName { get { return _strPluginClassName; } set { _strPluginClassName = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); } }
        public string MacroMethod { get { return _strMacroMethod; } set { _strMacroMethod = String.IsNullOrEmpty( value ) ? null : String.Copy( value ); } }
        public Int32 IndexOfScript { get { return _nMacroIndexOfScript; } set { _nMacroIndexOfScript = value; } }
        public string Append { get { return _strAppend; } set { _strAppend = value; } }

        public UMacroProduce( string scriptName, string pluginClassName, string macroMethodOfPlugin, Int32 indexOfScript )
        {
            _strScriptName = scriptName;
            _strPluginClassName = pluginClassName;
            _strMacroMethod = macroMethodOfPlugin;
            _nMacroIndexOfScript = indexOfScript;
        }

        public void Dispose()
        {
            if ( _bDispoing || _bDisposed ) return;
            _bDispoing = true;

            Dispose( true );

            _bDisposed = true;
            _bDispoing = false;
        }

        protected abstract void Dispose( bool bDisposing );

        protected static object Check<T>( List<T> list, Int32 index_0 )
        {
            if ( list == null ) return null;
            if ( !typeof( T ).IsSubclassOf( typeof( UMacroProduce ) ) )
                return null;

            for ( int i = 0 ; i < list.Count ; i++ )
            {
                UMacroProduce b = list[ i ] as UMacroProduce;
                if ( b == null || b.Invalid ) continue;
                if ( b.IndexOfScript == index_0 )
                {
                    return (( object ) list[ i ]);
                }
            }
            return null;
        }

        protected static object Check<T>( List<T> list, string nameOfPluginClass, string macroMethod )
        {
            if ( list == null || String.IsNullOrEmpty( nameOfPluginClass ) || String.IsNullOrEmpty( macroMethod ) ) return null;
            if ( !typeof( T ).IsSubclassOf( typeof( UMacroProduce ) ) ) return null;

            for ( int i = 0 ; i < list.Count ; i++ )
            {
                UMacroProduce b = list[ i ] as UMacroProduce;
                if ( b == null || b.Invalid ) continue;
                if ( b.PluginClassName == nameOfPluginClass && b.MacroMethod == macroMethod )
                {
                    return (( object ) list[ i ]);
                }
            }

            return null;
        }

        protected static object Check<T>( List<T> list, string nameOfScript, Int32 index_0 )
        {
            if ( list == null || String.IsNullOrEmpty( nameOfScript ) ) return null;

            for ( int i = 0 ; i < list.Count ; i++ )
            {
                UMacroProduce b = list[ i ] as UMacroProduce;
                if ( b == null || b.Invalid ) continue;
                if ( b.IndexOfScript == index_0 && b._strScriptName == nameOfScript )
                {
                    return (( object ) list[ i ]);
                }
            }

            return null;
        }

        protected static object Check<T>( List<T> list, string nameOfScript, string nameOfPluginClass, string macroMethod )
        {
            if ( list == null || String.IsNullOrEmpty( nameOfPluginClass ) || String.IsNullOrEmpty( macroMethod ) || String.IsNullOrEmpty( nameOfScript ) ) return null;

            for ( int i = 0 ; i < list.Count ; i++ )
            {
                UMacroProduce b = list[ i ] as UMacroProduce;
                if ( b == null || b.Invalid ) continue;
                if ( b._strScriptName == nameOfScript && b.PluginClassName == nameOfPluginClass && b.MacroMethod == macroMethod )
                {
                    return (( object ) list[ i ]);
                }
            }

            return null;
        }

        public static void Free<T>( List<T> list )
        {
            if ( list == null ) return;

            for ( int i = 0 ; i < list.Count ; i++ )
            {
                if ( list[ i ] == null ) continue;
                IDisposable disp = list[ i ] as IDisposable;
                if ( disp != null )
                    disp.Dispose();
            }

            list.Clear();
        }
    }


    /// <summary>
    /// Store results of a macro reported
    /// </summary>
    public class UMacroProduceCarrierResult : UMacroProduce
    {
        private UDataCarrier[] _ResultSet;
        private fpUDataCarrierSetResHandler _fpHandler;

        public UDataCarrier[] ResultSet { get { return ((_bDispoing || _bDisposed) ? null : _ResultSet); } }

        public UMacroProduceCarrierResult( string nameOfScript, string nameOfPluginClass, string macroMethod, Int32 indexOfScript, UDataCarrier[] resultCarrier, fpUDataCarrierSetResHandler resultHandler )
            : base( nameOfScript, nameOfPluginClass, macroMethod, indexOfScript )
        {
            _ResultSet = resultCarrier;
            _fpHandler = resultHandler;
        }

        protected override void Dispose( bool bDisposing )
        {
            if ( _fpHandler != null && _ResultSet != null )
                _fpHandler( _ResultSet );
            _ResultSet = null;
        }

        public void ResetResult( UDataCarrier[] resultCarrier, fpUDataCarrierSetResHandler resultHandler )
        {
            if ( _fpHandler != null && _ResultSet != null )
                _fpHandler( _ResultSet );

            _ResultSet = resultCarrier;
            _fpHandler = resultHandler;
        }

        public static UMacroProduceCarrierResult GetItem( List<UMacroProduceCarrierResult> carriers, Int32 index_0 )
        {
            return ( UMacroProduceCarrierResult ) Check<UMacroProduceCarrierResult>( carriers, index_0 );
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierResult> carriers, Int32 index_0 )
        {
            UMacroProduceCarrierResult info = ( UMacroProduceCarrierResult ) Check<UMacroProduceCarrierResult>( carriers, index_0 );
            return (info == null ? null : info._ResultSet);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierResult> carriers, string nameOfPluginClass, string macroMathod )
        {
            UMacroProduceCarrierResult info = ( UMacroProduceCarrierResult ) Check<UMacroProduceCarrierResult>( carriers, nameOfPluginClass, macroMathod );
            return (info == null ? null : info.ResultSet);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierResult> carriers, string nameOfScript, Int32 index_0 )
        {
            UMacroProduceCarrierResult info = ( UMacroProduceCarrierResult ) Check<UMacroProduceCarrierResult>( carriers, nameOfScript, index_0 );
            return (info == null ? null : info.ResultSet);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierResult> carriers, string nameOfScript, string nameOfPluginClass, string macroMethod )
        {
            UMacroProduceCarrierResult info = ( UMacroProduceCarrierResult ) Check<UMacroProduceCarrierResult>( carriers, nameOfScript, nameOfPluginClass, macroMethod );
            return (info == null ? null : info.ResultSet);
        }
    }

    /// <summary>
    /// Store data of a macro and propagate to other macros
    /// </summary>
    public class UMacroProduceCarrierPropagation : UMacroProduce
    {
        private UDataCarrier[] _PropagationCarrier;
        private fpUDataCarrierSetResHandler _fpHandler;

        public UDataCarrier[] PropagationCarrier { get { return ((_bDispoing || _bDisposed) ? null : _PropagationCarrier); } }

        /// <summary>
        /// create class instance
        /// </summary>
        /// <param name="nameOfScript">script name</param>
        /// <param name="nameeOfPluginClass">plugin class name</param>
        /// <param name="macroMathod">macro method name</param>
        /// <param name="indexOfScript">index of script</param>
        /// <param name="propagation">provide data to propagate</param>
        /// <param name="propagationHandler">handle data of propagation</param>
        public UMacroProduceCarrierPropagation( string nameOfScript, string nameeOfPluginClass, string macroMathod, Int32 indexOfScript, UDataCarrier[] propagation, fpUDataCarrierSetResHandler propagationHandler )
            : base( nameOfScript, nameeOfPluginClass, macroMathod, indexOfScript )
        {
            _PropagationCarrier = propagation;
            _fpHandler = propagationHandler;
        }

        protected override void Dispose( bool bDisposing )
        {
            if ( _fpHandler != null && _PropagationCarrier != null )
                _fpHandler( _PropagationCarrier );
            _PropagationCarrier = null;
        }

        public void Reset( UDataCarrier[] propagation, fpUDataCarrierSetResHandler propagationHandler )
        {
            if ( _fpHandler != null && _PropagationCarrier != null )
                _fpHandler( _PropagationCarrier );

            _PropagationCarrier = propagation;
            _fpHandler = propagationHandler;
        }

        public static UMacroProduceCarrierPropagation GetItem( List<UMacroProduceCarrierPropagation> carriers, Int32 index_0 )
        {
            return ( UMacroProduceCarrierPropagation ) Check<UMacroProduceCarrierPropagation>( carriers, index_0 );
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierPropagation> carriers, Int32 index_0 )
        {
            UMacroProduceCarrierPropagation info = ( UMacroProduceCarrierPropagation ) Check<UMacroProduceCarrierPropagation>( carriers, index_0 );
            return (info == null ? null : info._PropagationCarrier);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierPropagation> carriers, string nameOfPluginClass, string macroMethod )
        {
            UMacroProduceCarrierPropagation info = ( UMacroProduceCarrierPropagation ) Check<UMacroProduceCarrierPropagation>( carriers, nameOfPluginClass, macroMethod );
            return (info == null ? null : info._PropagationCarrier);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierPropagation> carriers, string nameOfScript, Int32 index_0 )
        {
            UMacroProduceCarrierPropagation info = ( UMacroProduceCarrierPropagation ) Check<UMacroProduceCarrierPropagation>( carriers, nameOfScript, index_0 );
            return (info == null ? null : info._PropagationCarrier);
        }

        public static UDataCarrier[] Get( List<UMacroProduceCarrierPropagation> carriers, string nameOfScript, string nameOfPluginClass, string macroMethod )
        {
            UMacroProduceCarrierPropagation info = ( UMacroProduceCarrierPropagation ) Check<UMacroProduceCarrierPropagation>( carriers, nameOfScript, nameOfPluginClass, macroMethod );
            return (info == null ? null : info._PropagationCarrier);
        }
    }

    /// <summary>
    /// Store drawing results
    /// </summary>
    public class UMacroProduceCarrierDrawingResult : UMacroProduce
    {
        private static string _strRootEleName = "ProduceCarrierDrawDat";
        private static string _strScriptEleName = "ScriptName";
        private static string _strPluginClassEleName = "PluginClassName";
        private static string _strMacroMethodEleName = "MacroMethod";
        private static string _strMacroIndexEleName = "MacroIndexOfScript";

        private UDrawingCarriers _DrawingData = null;

        public Int32 BackgroundW { get { return (_DrawingData == null ? 0 : _DrawingData.BackgroundW); } set { if ( _DrawingData != null ) _DrawingData.BackgroundW = value; } }
        public Int32 BackgroundH { get { return (_DrawingData == null ? 0 : _DrawingData.BackgroundH); } set { if ( _DrawingData != null ) _DrawingData.BackgroundH = value; } }

        public List<UDrawingCarrier> Data { get { return ((_DrawingData == null || _DrawingData.DrawingData == null) ? null : _DrawingData.DrawingData); } }
        /// <summary>
        /// create class instance
        /// </summary>
        /// <param name="nameOfScript">script name</param>
        /// <param name="nameOfPluginClass">plug class name</param>
        /// <param name="macroMethod">macro method name</param>
        /// <param name="indexOfScript">index of script</param>
        public UMacroProduceCarrierDrawingResult( string nameOfScript, string nameOfPluginClass, string macroMethod, Int32 indexOfScript )
            : base( nameOfScript, nameOfPluginClass, macroMethod, indexOfScript )
        {
            _DrawingData = new UDrawingCarriers();
        }

        public UMacroProduceCarrierDrawingResult( string nameOfScript, string nameOfPluginClass, string macroMethod, Int32 indexOfScript, UDrawingCarriers carrier )
            : base( nameOfScript, nameOfPluginClass, macroMethod, indexOfScript )
        {
            _DrawingData = carrier;
        }

        protected override void Dispose( bool bDisposing )
        {
            if ( _DrawingData != null ) _DrawingData.Dispose();
            _DrawingData = null;
        }

        public void ResetDrawing( UDrawingCarriers carrier )
        {
            if ( _DrawingData != null ) _DrawingData.Dispose();
            _DrawingData = carrier;
        }

        public bool Add<T>(T dat)
        {
            if ( _bDispoing || _bDisposed || _DrawingData == null ) return false;

            return _DrawingData.Add<T>( dat );
        }

        public void ReadXml( XmlNode nodP )
        {
            if ( _bDispoing || _bDisposed ) return;
            if ( _DrawingData != null ) _DrawingData.Read( nodP );
        }

        public void WriteXml( XmlTextWriter tw )
        {
            if ( _bDispoing || _bDisposed ) return;
            if ( _DrawingData != null ) _DrawingData.Write( tw );
        }

        public static UMacroProduceCarrierDrawingResult GetItem( List<UMacroProduceCarrierDrawingResult> carriers, Int32 index_0 )
        {
            return ( UMacroProduceCarrierDrawingResult ) Check<UMacroProduceCarrierDrawingResult>( carriers, index_0 );
        }

        public static UDrawingCarriers Get( List<UMacroProduceCarrierDrawingResult> carriers, Int32 index_0 )
        {
            UMacroProduceCarrierDrawingResult info = ( UMacroProduceCarrierDrawingResult ) Check<UMacroProduceCarrierDrawingResult>( carriers, index_0 );
            return (info == null ? null : info._DrawingData);
        }

        public static UDrawingCarriers Get( List<UMacroProduceCarrierDrawingResult> carriers, string nameOfPluginClass, string macroMethod )
        {
            UMacroProduceCarrierDrawingResult info = ( UMacroProduceCarrierDrawingResult ) Check<UMacroProduceCarrierDrawingResult>( carriers, nameOfPluginClass, macroMethod );
            return (info == null ? null : info._DrawingData);
        }

        public static UDrawingCarriers Get( List<UMacroProduceCarrierDrawingResult> carriers, string nameOfScript, Int32 index_0 )
        {
            UMacroProduceCarrierDrawingResult info = ( UMacroProduceCarrierDrawingResult ) Check<UMacroProduceCarrierDrawingResult>( carriers, nameOfScript, index_0 );
            return (info == null ? null : info._DrawingData);
        }

        public static UDrawingCarriers Get( List<UMacroProduceCarrierDrawingResult> carriers, string nameOfScript, string nameOfPluginClass, string macroMethod )
        {
            UMacroProduceCarrierDrawingResult info = ( UMacroProduceCarrierDrawingResult ) Check<UMacroProduceCarrierDrawingResult>( carriers, nameOfScript, nameOfPluginClass, macroMethod );
            return (info == null ? null : info._DrawingData);
        }

        public static void WriteXml( List<UMacroProduceCarrierDrawingResult> carriers, XmlTextWriter tw )
        {
            if ( carriers == null || carriers.Count <= 0 || tw == null ) return;

            for ( int i = 0 ; i < carriers.Count ; i++ )
            {
                if ( carriers[ i ] == null ) continue;

                tw.WriteStartElement( _strRootEleName );

                tw.WriteElementString( _strScriptEleName, carriers[ i ].ScriptName );
                tw.WriteElementString( _strPluginClassEleName, carriers[ i ].PluginClassName );
                tw.WriteElementString( _strMacroMethodEleName, carriers[ i ].MacroMethod );
                tw.WriteElementString( _strMacroIndexEleName, carriers[ i ].IndexOfScript.ToString() );

                carriers[ i ].WriteXml( tw );

                tw.WriteEndElement();
            }
        }

        public static void ReadXml( XmlNode nodp, out List<UMacroProduceCarrierDrawingResult> carriers )
        {
            carriers = null;
            if ( nodp == null ) return;
            XmlNodeList nodl = nodp.SelectNodes( _strRootEleName );
            if ( nodl == null || nodl.Count <= 0 ) return;

            carriers = new List<UMacroProduceCarrierDrawingResult>();

            XmlNode nod = null;

            for ( int i = 0 ; i < nodl.Count ; i++ )
            {
                if ( nodl == null ) continue;

                string nameOfScript = null;
                string nameOfSharpPluginClass = null;
                string nameOfMethodPlugin = null;
                Int32 idx = 0;

                nod = nodl[ i ].SelectSingleNode( _strScriptEleName );
                nameOfScript = String.IsNullOrEmpty( nod.InnerText ) ? null : String.Copy( nod.InnerText );

                nod = nodl[ i ].SelectSingleNode( _strPluginClassEleName );
                nameOfSharpPluginClass = String.IsNullOrEmpty( nod.InnerText ) ? null : String.Copy( nod.InnerText );

                nod = nodl[ i ].SelectSingleNode( _strMacroMethodEleName );
                nameOfMethodPlugin = String.IsNullOrEmpty( nod.InnerText ) ? null : String.Copy( nod.InnerText );

                nod = nodl[ i ].SelectSingleNode( _strMacroIndexEleName );
                try { idx = Convert.ToInt32( nod.InnerText ); }
                catch { idx = 0; }

                UMacroProduceCarrierDrawingResult info = new UMacroProduceCarrierDrawingResult( nameOfScript, nameOfSharpPluginClass, nameOfMethodPlugin, idx );
                info.ReadXml( nodl[ i ] );

                carriers.Add( info );
            }

        }
    }

}

