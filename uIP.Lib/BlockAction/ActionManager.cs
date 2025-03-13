using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace uIP.Lib.BlockAction
{
    public class ActionBlocksInfo
    {
        public UDataCarrier[] _InputParam = null;
        public string _strNameOfBlock = null;
        public UDataCarrier _BlockSettings = null;

        public ActionBlocksInfo() { }
        public ActionBlocksInfo(string nameOfBlock, UDataCarrier[] input)
        {
            _strNameOfBlock = nameOfBlock;
            _InputParam = input;
        }
        public ActionBlocksInfo( string nameOfBlock, UDataCarrier[] input, UDataCarrier settings )
        {
            _strNameOfBlock = nameOfBlock;
            _InputParam = input;
            _BlockSettings = settings;
        }
    }
    public class AnActionInfo
    {
        internal object _SyncDat = new object();
        public string _strNameOfAction = null;
        public List<ActionBlocksInfo> _Blocks = new List<ActionBlocksInfo>();

        public AnActionInfo() { }
        public AnActionInfo(string name)
        {
            _strNameOfAction = name;
        }

        public UDataCarrierSet[] BlocksInputs {
            get {
                if ( _Blocks == null || _Blocks.Count <= 0 ) return null;

                List<UDataCarrierSet> ret = new List<UDataCarrierSet>();
                for(int i = 0; i < _Blocks.Count; i++ ) {
                    ret.Add( new UDataCarrierSet( "", _Blocks[ i ]._InputParam, null ) );
                }
                return ret.ToArray();
            }
        }
        public string[] BlocksName {
            get {
                if (_Blocks == null || _Blocks.Count <= 0) return null;
                List<string> ret = new List<string>();
                for ( int i = 0; i < _Blocks.Count; i++ ) {
                    ret.Add( _Blocks[i]._strNameOfBlock );
                }
                return ret.ToArray();
            }
        }
        public void SetInputParam(UDataCarrier[] param, int index)
        {
            if (_Blocks == null || index < 0 || index >= _Blocks.Count)
                return;
            _Blocks[index]._InputParam = param;
        }
    }

    public class ActionManager : IDisposable, IDatIO
    {
        List<AnActionInfo> _Actions = new List<AnActionInfo>();
        ActionAgent _AA = null;

        public ActionAgent AA { get { return _AA; } set { _AA = value; } }
        public List<AnActionInfo> Actions {  get { return _Actions; } }

        public ActionManager() { }
        public ActionManager(ActionAgent aa) { _AA = aa; }

        public void Dispose()
        {
            if (_ControlAC != null) {
                _ControlAC.Dispose();
                _ControlAC = null;
            }
        }

        public void Clear()
        {
            _Actions.Clear();
        }
        public bool New(string nameOf )
        {
            if ( String.IsNullOrEmpty( nameOf ) )
                return false;
            for(int i = 0; i < _Actions.Count; i++ ) {
                if (_Actions[i]._strNameOfAction == nameOf) {
                    return true;
                }
            }

            _Actions.Add( new AnActionInfo( nameOf ) );
            return true;
        }
        public void Remove( string nameOf )
        {
            if ( String.IsNullOrEmpty( nameOf ) )
                return;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOf ) {
                    _Actions.RemoveAt( i );
                    return;
                }
            }
        }
        private UDataCarrier[] CloneInput(string nameOfAction, int index_0)
        {
            AnActionInfo a = Get( nameOfAction );
            if ( a == null ) return null;
            if ( a._Blocks == null || index_0 < 0 || index_0 >= a._Blocks.Count )
                return null;

            // set first
            if ( !_AA.CallBlockSet( a._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_CLONE_PARAMETER,
                UDataCarrier.MakeOne<UDataCarrierSet>( new UDataCarrierSet( "", a._Blocks[ index_0 ]._InputParam, null ) ) ) )
                return null;
            // get back
            UDataCarrier got = null;
            if ( !_AA.CallBlockGet( a._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_CLONE_PARAMETER,
                out got ) )
                return null;

            UDataCarrierSet s = got == null || got.Data == null ? null : got.Data as UDataCarrierSet;

            return s == null ? null : s._Array;
        }
        private UDataCarrier CloneSettings( string nameOfAction, int index_0 )
        {
            AnActionInfo a = Get( nameOfAction );
            if ( a == null ) return null;
            if ( a._Blocks == null || index_0 < 0 || index_0 >= a._Blocks.Count )
                return null;

            // set first
            if ( !_AA.CallBlockSet( a._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_CLONE_SETTINGS, a._Blocks[ index_0 ]._BlockSettings ) )
                return null;
            // get back
            UDataCarrier got = null;
            if ( !_AA.CallBlockGet( a._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_CLONE_SETTINGS,
                out got ) )
                return null;

            return got;
        }
        public bool Reproduce(string nameOfSrc, string nameOfNewOne)
        {
            if ( String.IsNullOrEmpty( nameOfSrc ) || String.IsNullOrEmpty( nameOfNewOne ) )
                return false;

            AnActionInfo a = Get( nameOfSrc );
            if ( a == null )
                return false;
            if ( Get( nameOfNewOne ) != null )
                return false;

            if ( !New( nameOfNewOne ) )
                return false;
            bool err = false;
            for ( int i = 0; i < a._Blocks.Count; i++ ) {
                UDataCarrier[] p = CloneInput( nameOfSrc, i );
                UDataCarrier settings = CloneSettings( nameOfSrc, i );
                if ( !AddBlock( nameOfNewOne, a._Blocks[ i ]._strNameOfBlock, p, settings ) ) {
                    err = true; break;
                }
            }
            if (err) {
                Remove( nameOfNewOne );
                return false;
            }
            return true;
        }
        public AnActionInfo Get(string nameOf)
        {
            if ( String.IsNullOrEmpty( nameOf ) )
                return null;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOf ) {
                    return _Actions[i];
                }
            }
            return null;
        }
        public bool AddBlock(string nameOfAction, string nameOfBlock, UDataCarrier[] input, UDataCarrier settings = null)
        {
            AnActionInfo got = null;
            for(int i = 0; i < _Actions.Count;i++ ) {
                if (_Actions[i]._strNameOfAction == nameOfAction) {
                    got = _Actions[ i ]; break;
                }
            }
            if ( got == null )
                return false;

            got._Blocks.Add( new ActionBlocksInfo( nameOfBlock, input, settings ) );
            return true;
        }
        public bool EditBlockInputVar( string nameOfAction, int index_0 )
        {
            AnActionInfo got = null;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOfAction ) {
                    got = _Actions[ i ]; break;
                }
            }
            if ( got == null )
                return false;

            if ( index_0 < 0 || index_0 >= got._Blocks.Count )
                return false;

            UDataCarrier dat;
            if ( _AA.CallBlockGet( got._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_POPUP_PARAMETERUI, out dat ) ) {
                if ( dat.Data != null && dat.Data.GetType() == typeof( UDataCarrier[] ) )
                    got._Blocks[ index_0 ]._InputParam = dat.Data as UDataCarrier[];
            }

            return true;
        }
        public bool EditBlockSettings( string nameOfAction, int index_0 )
        {
            AnActionInfo got = null;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOfAction ) {
                    got = _Actions[ i ]; break;
                }
            }
            if ( got == null )
                return false;

            if ( index_0 < 0 || index_0 >= got._Blocks.Count )
                return false;

            // popup UI
            if (_AA.CallBlockSet( got._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_POPUP_SETTING, null ) ) {
                // ok, get the settings
                UDataCarrier dat;
                if ( _AA.CallBlockGet( got._Blocks[ index_0 ]._strNameOfBlock, UCBlockBase.strUCB_SETTINGS, out dat ) ) {
                    got._Blocks[ index_0 ]._BlockSettings = dat;
                    return true;
                }
            }

            return false;
        }

        public bool RmvBlock(string nameOfAction, int index_0 )
        {
            AnActionInfo got = null;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOfAction ) {
                    got = _Actions[ i ]; break;
                }
            }
            if ( got == null )
                return false;

            if ( index_0 < 0 || index_0 >= got._Blocks.Count )
                return false;

            got._Blocks.RemoveAt( index_0 );
            return true;
        }
        public bool InsertBlock(string nameOfAction, string nameOfBlock, UDataCarrier[] input, int index_0)
        {
            AnActionInfo got = null;
            for ( int i = 0; i < _Actions.Count; i++ ) {
                if ( _Actions[ i ]._strNameOfAction == nameOfAction ) {
                    got = _Actions[ i ]; break;
                }
            }
            if ( got == null )
                return false;
            if ( index_0 < 0 || index_0 >= got._Blocks.Count )
                return false;

            got._Blocks.Insert( index_0, new ActionBlocksInfo( nameOfBlock, input ) );
            return true;
        }

        #region IDatIO
        string _strGivenName = null;
        string _strFileName = null;
        public string FileName {  get { return _strFileName; } set { _strFileName = value; } }
        public string GivenName { get { return _strGivenName; } set { _strGivenName = value; } }
        public string UniqueName { get { return _strGivenName; } }

        public string IOFileName { get { return String.IsNullOrEmpty( _strFileName) ? "ActionMgrSettings.xml" : _strFileName; } }
        public bool CanWrite { get { return _Actions.Count > 0; } }
        public bool WriteDat( string folderPath, string fileName )
        {
            try {
                using ( Stream ws = File.Open( Path.Combine( folderPath, fileName ), FileMode.Create ) ) {
                    XmlTextWriter w = new XmlTextWriter( ws, Encoding.UTF8 );
                    w.Formatting = Formatting.Indented;

                    w.WriteStartDocument();
                    w.WriteStartElement( "ActionsSettings" );

                    for(int i = 0; i <_Actions.Count; i++ ) {
                        w.WriteStartElement( "BlockAction" );
                        w.WriteAttributeString( "Name", _Actions[ i ]._strNameOfAction );
                        for(int j = 0; j < _Actions[i]._Blocks.Count; j++ ) {

                            string iparam = "";
                            if ( _Actions[ i ]._Blocks[ j ]._InputParam != null ) {
                                MemoryStream ms = new MemoryStream();
                                if ( UDataCarrier.WriteXml( _Actions[ i ]._Blocks[ j ]._InputParam, null, ms ) ) {
                                    iparam = Encoding.UTF8.GetString( ms.ToArray() );
                                } else {
                                    ms.Dispose();
                                    continue;
                                }
                                ms.Dispose();
                            }

                            string sparam = "";
                            if ( _Actions[ i ]._Blocks[ j ]._BlockSettings != null ) {
                                MemoryStream ms = new MemoryStream();
                                UDataCarrier[] arr = new UDataCarrier[] { _Actions[ i ]._Blocks[ j ]._BlockSettings };
                                if ( UDataCarrier.WriteXml( arr, null, ms ) ) {
                                    sparam = Encoding.UTF8.GetString( ms.ToArray() );
                                } else {
                                    ms.Dispose();
                                    continue;
                                }
                                ms.Dispose();
                            }


                            w.WriteStartElement( "BlockAction" );
                            w.WriteElementString( "Name", _Actions[ i ]._Blocks[ j ]._strNameOfBlock );
                            w.WriteElementString( "Input", iparam );
                            w.WriteElementString( "Settings", sparam );
                            w.WriteEndElement();
                        }

                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                    w.WriteEndDocument();

                    w.Flush();
                    w.Close();
                }
            } catch {
                return false;
            }

            return true;
        }
        public bool CanRead { get { return _AA != null; } }
        public bool ReadDat( string folderPath, string fileName )
        {
            // clear
            _Actions.Clear();

            try {
                using(Stream rs = File.Open(Path.Combine(folderPath, fileName), FileMode.Open)) {
                    XmlDocument doc = new XmlDocument();
                    doc.Load( rs );

                    XmlNodeList ns = doc.SelectNodes( "//ActionsSettings/BlockAction" );
                    if (ns != null && ns.Count > 0) {
                        for (int i = 0; i < ns.Count; i++ ) {
                            ReadAction( ns[ i ] );
                        }
                    }
                }
            }catch {
                return false;
            }
            return true;
        }
        private void ReadAction(XmlNode nod)
        {
            if ( nod.Attributes == null || nod.Attributes.Count <= 0 )
                return;

            string nameOfAction = nod.Attributes[ 0 ].InnerText;
            if ( String.IsNullOrEmpty( nameOfAction ) )
                return;

            XmlNodeList ns = nod.SelectNodes( "BlockAction" );
            if ( ns == null || ns.Count <= 0 )
                return;

            AnActionInfo one = new AnActionInfo( nameOfAction );
            for ( int i = 0; i < ns.Count; i++ ) {
                XmlNode name = ns[ i ].SelectSingleNode( "Name" );
                XmlNode input = ns[ i ].SelectSingleNode( "Input" );
                XmlNode settings = ns[ i ].SelectSingleNode( "Settings" );

                if ( name == null || input == null || settings == null )
                    continue;
                if ( String.IsNullOrEmpty( name.InnerText ) )
                    continue;
                if ( String.IsNullOrEmpty( input.InnerText ) ) {
                    one._Blocks.Add( new ActionBlocksInfo( String.Copy( name.InnerText ), null ) );
                    continue;
                }

                MemoryStream ms = null;
                bool status = false;
                UDataCarrier[] dat = null;
                UDataCarrier[] settingsDat = null;
                try {
                    ms = new MemoryStream( Encoding.UTF8.GetBytes( input.InnerText ) );
                    string[] addi = null;
                    status = UDataCarrier.ReadXml( ms, _AA.LoadedAssemblies.Assemblies, ref dat, ref addi );
                } catch {
                    if ( ms != null ) {
                        ms.Dispose();
                    }
                    continue;
                }
                ms.Dispose();
                ms = null;

                if (!String.IsNullOrEmpty(settings.InnerText)) {
                    try {
                        ms = new MemoryStream( Encoding.UTF8.GetBytes( settings.InnerText ) );
                        string[] addi = null;
                        status = UDataCarrier.ReadXml( ms, _AA.LoadedAssemblies.Assemblies, ref settingsDat, ref addi );
                    } catch {
                        if ( ms != null ) {
                            ms.Dispose();
                        }
                        continue;
                    }
                    ms.Dispose();
                    ms = null;
                }

                if ( !status )
                    continue;

                one._Blocks.Add( new ActionBlocksInfo( String.Copy( name.InnerText ), dat, settingsDat == null || settingsDat.Length <= 0 ? null : settingsDat[0] ) );
            }
            _Actions.Add( one );
        }

        public bool PopupGUI()
        {
            frmActionsConfig dlg = new frmActionsConfig();
            dlg.AM = this;

            dlg.ShowDialog();
            dlg.Dispose();
            dlg = null;
            return true;
        }
        UserControlActionsConfig _ControlAC = null;
        public Control GetGUI()
        {
            if (_ControlAC == null) {
                _ControlAC = new UserControlActionsConfig();
                _ControlAC.AM = this;
            }
            _ControlAC.UpdateActions();

            return _ControlAC;
        }

        #endregion
    }
}
