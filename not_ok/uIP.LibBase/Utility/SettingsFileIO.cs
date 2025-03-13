using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Ionic.Zip;

namespace uIP.LibBase.Utility
{
    public class SettingsFileIO : IDisposable, ISettingIO
    {
        private string _strRwTmpFolderPath = null;
        private Dictionary<string, IDatIO> _Reg = new Dictionary<string, IDatIO>();
        private fpLogMessage _Log = null;

        public SettingsFileIO(string rwPath, fpLogMessage log)
        {
            if ( CommonUtilities.RCreateDir( rwPath ) )
                _strRwTmpFolderPath = String.Copy( rwPath );
            _Log = log;
        }
        public void Dispose()
        {
            if (Directory.Exists(_strRwTmpFolderPath)) {
                try { Directory.Delete( _strRwTmpFolderPath, true ); } catch { }
            }
            _strRwTmpFolderPath = null;
        }

        private static bool IsValid(string str)
        {
            if ( String.IsNullOrEmpty( str ) )
                return false;

            char[] invalidPathChars = Path.GetInvalidFileNameChars();
            if ( invalidPathChars == null || invalidPathChars.Length <= 0 )
                return true;
            for(int i = 0; i < invalidPathChars.Length; i++ ) {
                if ( str.IndexOf( invalidPathChars[ i ] ) >= 0 )
                    return false;
            }

            return true;
        }

        public bool Add(IDatIO inst)
        {
            if ( inst == null ) return false;
            if ( _Reg.ContainsKey( inst.UniqueName ) ) return false;
            if ( String.IsNullOrEmpty( inst.UniqueName ) ) return false;

            if ( !IsValid( inst.UniqueName ) ) return false;
            if ( !String.IsNullOrEmpty( inst.IOFileName ) && !IsValid( inst.IOFileName ) ) return false;

            _Reg.Add( inst.UniqueName, inst );
            return true;
        }
        public void Remove(IDatIO inst)
        {
            if ( inst == null ) return;
            if ( _Reg.ContainsKey( inst.UniqueName ) )
                _Reg.Remove( inst.UniqueName );
        }
        public void Remove(string name)
        {
            if ( String.IsNullOrEmpty( name ) ) return;
            if ( _Reg.ContainsKey( name ) )
                _Reg.Remove( name );
        }

        public bool Write(string filePath)
        {
            if (_Reg.Count <= 0 ) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: nothing to write!" ) );
                return false;
            }

            if ( String.IsNullOrEmpty( _strRwTmpFolderPath ) ) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format("[SettingFileIO] write error: invalid tmp folder path!") );
                return false;
            }

            // make tmp rw folder: _strRwTmpFolderPath + file name
            string tmpRootWPath = Path.Combine(_strRwTmpFolderPath, Path.GetFileNameWithoutExtension( filePath ));
            // make dir
            if ( !CommonUtilities.CreateDir( tmpRootWPath ) ) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: cannot create folder {0}!", tmpRootWPath ) );
                return false;
            }

            // create path for each
            foreach(KeyValuePair<string,IDatIO> kv in _Reg) {
                if ( kv.Value == null || String.IsNullOrEmpty( kv.Key ) )
                    continue;
                if ( !kv.Value.CanWrite )
                    continue;

                string wPath = Path.Combine( tmpRootWPath, kv.Key );
                if (!CommonUtilities.CreateDir(wPath)) {
                    if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: cannot create folder {0} for {1}!", wPath, kv.Key ) );
                    continue;
                }

                string fn = String.IsNullOrEmpty( kv.Value.IOFileName ) ? String.Format( "{0}.txt", kv.Key ) : kv.Value.IOFileName;
                try {
                    if ( !kv.Value.WriteDat( wPath, fn ) && _Log != null )
                        _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: call {0} write error!", kv.Key ) );
                }catch(Exception e) {
                    if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: call {0} write with exp\n{1}", kv.Key, e.ToString() ) );
                }
            }

            try {
                // write zip file
                using ( ZipFile zip = new ZipFile() ) {
                    zip.AddDirectory( tmpRootWPath );
                    zip.Save( filePath );
                }
            } catch(Exception e) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] write error: zip to file {0} error!\n{1}", filePath, e.ToString() ) );
                return false;
            } finally {
                if (Directory.Exists(tmpRootWPath)) {
                    try { Directory.Delete( tmpRootWPath, true ); } catch { }
                }
            }

            return true;
        }
        public bool Read(string filePath)
        {
            if ( _Reg.Count <= 0 ) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: nothing to write!" ) );
                return false;
            }

            if ( String.IsNullOrEmpty( _strRwTmpFolderPath ) ) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: invalid tmp folder path!" ) );
                return false;
            }

            if(!File.Exists(filePath)) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: invalid file path {0}!", filePath ) );
                return false;
            }

            string tmpRootRPath = Path.Combine( _strRwTmpFolderPath, Path.GetFileNameWithoutExtension( filePath ) );
            if (Directory.Exists(tmpRootRPath)) {
                try { Directory.Delete( tmpRootRPath, true ); } catch { }
                System.Threading.Thread.Sleep( 20 );
            }
            if (!CommonUtilities.CreateDir(tmpRootRPath)) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: cannot create folder {0}!", tmpRootRPath ) );
                return false;
            }

            // unzip
            using ( ZipFile zip = new ZipFile( filePath ) ) {
                zip.ZipErrorAction = ZipErrorAction.Skip;
                zip.ExtractAll( tmpRootRPath );
            }

            // read first layer
            string[] paths = Directory.GetDirectories( tmpRootRPath, "*", SearchOption.TopDirectoryOnly );
            for(int i =0;i < paths.Length; i++ ) {
                string mod = Path.GetFileName( paths[ i ] );
                if ( !_Reg.ContainsKey( mod ) || _Reg[ mod ] == null )
                    continue;
                if ( !_Reg[ mod ].CanRead ) continue;

                string fn = String.IsNullOrEmpty( _Reg[ mod ].IOFileName ) ? String.Format( "{0}.txt", mod ) : _Reg[ mod ].IOFileName;
                try {
                    if ( !_Reg[ mod ].ReadDat( paths[ i ], fn ) && _Log != null )
                        _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: call {0} read error!", mod ) );
                } catch(Exception e) {
                    if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: call {0} read with exp\n{1}", mod, e.ToString() ) );
                }
            }

            try { Directory.Delete( tmpRootRPath, true ); }
            catch(Exception e) {
                if ( _Log != null ) _Log( eLogMessageType.WARNING, 0, String.Format( "[SettingFileIO] read error: delete path {0} error!\n{1}", tmpRootRPath, e.ToString() ) );
            }

            return true;
        }

        public bool Config( IDatIO inst )
        {
            if ( inst == null ) return false;
            if ( !_Reg.ContainsKey( inst.UniqueName ) )
                return false;

            return _Reg[ inst.UniqueName ].PopupGUI();
        }
        public bool Config( string name )
        {
            if ( !_Reg.ContainsKey( name ) )
                return false;

            return _Reg[ name ].PopupGUI();
        }
        public Control GetConfig( IDatIO inst )
        {
            if ( inst == null ) return null;
            if ( !_Reg.ContainsKey( inst.UniqueName ) )
                return null;

            return _Reg[ inst.UniqueName ].GetGUI();
        }
        public Control GetConfig( string name )
        {
            if ( !_Reg.ContainsKey( name ) )
                return null;
            return _Reg[ name ].GetGUI();
        }

    }
}
