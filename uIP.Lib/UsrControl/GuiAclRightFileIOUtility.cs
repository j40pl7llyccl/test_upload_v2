using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using uIP.Lib.Utility;

namespace uIP.Lib.UsrControl
{
    public class ComponentAccLevel
    {
        public int _nEnableLevel = 0;
        public int _nVisibleLevel = 0;
        public ComponentAccLevel() { }
        public ComponentAccLevel(int enLvl, int vsLvl)
        {
            _nEnableLevel = enLvl;
            _nVisibleLevel = vsLvl;
        }
    }
    public class GuiAclRightFileIOUtility
    {
        string _strFullTypeName = null;
        Dictionary<string, ComponentAccLevel> _CompoRights = new Dictionary<string, ComponentAccLevel>();

        public string FullTypeName { get { return _strFullTypeName; } set { _strFullTypeName = value; } }
        public Dictionary<string, ComponentAccLevel> ComponentsRights { get { return _CompoRights; } }

        public GuiAclRightFileIOUtility() { }
        public GuiAclRightFileIOUtility(string tpFullName)
        {
            _strFullTypeName = tpFullName;
        }

        public void Clear()
        {
            _CompoRights.Clear();
        }
        public void Add( string nameOfCompo, int nEnableLevel, int nVisibleLevel )
        {
            if ( String.IsNullOrEmpty( nameOfCompo ) )
                return;
            if ( _CompoRights.ContainsKey( nameOfCompo ) ) {
                _CompoRights[ nameOfCompo ] = new ComponentAccLevel( nEnableLevel, nVisibleLevel );
                return;
            }

            _CompoRights.Add( nameOfCompo, new ComponentAccLevel( nEnableLevel, nVisibleLevel ) );
        }
        public void Rmv(string nameOfCompo)
        {
            if ( _CompoRights.ContainsKey( nameOfCompo ) )
                _CompoRights.Remove( nameOfCompo );
        }

        public bool Write(string filePath)
        {
            if ( String.IsNullOrEmpty( filePath ) )
                return false;
            if ( String.IsNullOrEmpty( _strFullTypeName ) )
                return false;

            try {
                using(Stream ws = File.Open(filePath, FileMode.Create)) {
                    StreamWriter w = new StreamWriter( ws );

                    w.WriteLine( "[{0}]", _strFullTypeName );
                    foreach(KeyValuePair<string, ComponentAccLevel> kv in _CompoRights) {
                        w.WriteLine( "{0}={1},{2}", kv.Key, kv.Value._nEnableLevel, kv.Value._nVisibleLevel );
                    }
                }
            } catch { return false; }

            return true;
        }
        public bool Read(string filePath, string whichSection)
        {
            if ( !File.Exists( filePath ) )
                return false;

            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( filePath ) )
                return false;

            SectionDataOfIni dat = ini.Get( whichSection );
            if ( dat == null || dat.Data == null || dat.Data.Count <= 0 )
                return false;

            _strFullTypeName = string.Copy( whichSection );
            _CompoRights.Clear();

            for(int i = 0; i < dat.Data.Count;  i++) {
                int e, v;
                try {
                    e = Convert.ToInt32( dat.Data[ i ].Values[ 0 ] );
                    v = Convert.ToInt32( dat.Data[ i ].Values[ 1 ] );
                } catch { continue; }

                if ( String.IsNullOrEmpty( dat.Data[ i ].Key ) )
                    continue;

                _CompoRights.Add( String.Copy( dat.Data[ i ].Key ), new ComponentAccLevel( e, v ) );
            }

            return true;
        }

        public static List<GuiAclRightFileIOUtility> Read(string path)
        {
            IniReaderUtility ini = new IniReaderUtility();
            if ( !ini.Parsing( path ) )
                return null;

            List<GuiAclRightFileIOUtility> ret = new List<GuiAclRightFileIOUtility>();
            string[] sections = ini.GetSections();
            if (sections == null || sections.Length <= 0)
                return null;

            for(int i = 0; i < sections.Length; i++ ) {
                GuiAclRightFileIOUtility g = new GuiAclRightFileIOUtility();
                if (g.Read(path, sections[i])) {
                    ret.Add( g );
                }
            }

            return ret;
        }
        public static bool Write(string filePath, List<GuiAclRightFileIOUtility> acls)
        {
            if (acls == null || acls.Count <= 0) {
                return false;
            }

            try {
                using(Stream ws = File.Open(filePath, FileMode.Create)) {
                    StreamWriter w = new StreamWriter( ws );
                    for(int i = 0; i < acls.Count; i++ ) {
                        if ( acls[ i ] == null || acls[ i ].ComponentsRights == null || acls[ i ].ComponentsRights.Count <= 0 || String.IsNullOrEmpty( acls[ i ].FullTypeName ) )
                            continue;
                        w.WriteLine( "[{0}]", acls[ i ].FullTypeName );
                        foreach(KeyValuePair<string, ComponentAccLevel> kv in acls[i].ComponentsRights) {
                            w.WriteLine( "{0}={1},{2}", kv.Key, kv.Value._nEnableLevel, kv.Value._nVisibleLevel );
                        }
                        w.WriteLine();
                    }
                    w.Flush();
                    w.Close();
                }
            } catch {
                return false;
            }
            return true;
        }

    }
}
