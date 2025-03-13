using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uIP.Lib;

namespace uIP.Lib.UsrControl
{
    public class UserControlData : UserBasicInfo
    {
        protected Dictionary<int, UserGroupAccessRight> _refGroupRights = null;
        protected DateTime m_tLatestLogin = DateTime.MinValue;
        protected DateTime m_tLatestLogout = DateTime.MinValue;

        public UserControlData( Dictionary<int, UserGroupAccessRight> groupRights ) : base() { _refGroupRights = groupRights; }
        public UserControlData( Dictionary<int, UserGroupAccessRight> groupRights, string name, string pwd, int group )
            : base( name, pwd, group )
        {
            _refGroupRights = groupRights;
        }
        public UserControlData( Dictionary<int, UserGroupAccessRight> groupRights, string name, string pwd, int group, int inherGroupEnable, int inherGroupVisible )
            : base( name, pwd, group, inherGroupEnable, inherGroupVisible )
        {
            _refGroupRights = groupRights;
        }
        public int EnabledLevel {
            get {
                int curEnabledLvl = 0;
                if ( _refGroupRights == null ) return curEnabledLvl;
                if ( _refGroupRights.ContainsKey( _nGroup ) )
                    curEnabledLvl = _refGroupRights[ _nGroup ]._nEnabledLevel;
                if ( _nInherGroupOfEnabled >= 0 && _refGroupRights.ContainsKey( _nInherGroupOfEnabled ) )
                    curEnabledLvl = _refGroupRights[ _nInherGroupOfEnabled ]._nEnabledLevel; // over-write
                return curEnabledLvl;
            }
        }
        public int VisibleLevel {
            get {
                int curVisibleLvl = 0;
                if ( _refGroupRights == null ) return curVisibleLvl;
                if ( _refGroupRights.ContainsKey( _nGroup ) )
                    curVisibleLvl = _refGroupRights[ _nGroup ]._nVisibleLevel;
                if ( _nInherGroupOfVisible >= 0 && _refGroupRights.ContainsKey( _nInherGroupOfVisible ) )
                    curVisibleLvl = _refGroupRights[ _nInherGroupOfVisible ]._nVisibleLevel;
                return curVisibleLvl;
            }
        }
        public bool GetGroup( int g, out UserGroupAccessRight right )
        {
            right = null;
            if ( _refGroupRights == null ) return false;
            if ( !_refGroupRights.ContainsKey( g ) ) return false;

            right = new UserGroupAccessRight( _refGroupRights[ g ]._strNameOfGroup, _refGroupRights[ g ]._nEnabledLevel, _refGroupRights[ g ]._nVisibleLevel );
            return true;
        }
        public string GetGroup()
        {
            if ( _refGroupRights == null ) return "";
            if ( !_refGroupRights.ContainsKey( _nGroup ) ) return "NA";
            return _refGroupRights[ _nGroup ]._strNameOfGroup;
        }
        public virtual string GetWriteStr()
        {
            return String.Format( "\"{0}\"= \"{1}\", {2}, {3}, {4}\n", _strName, _strPwd, _nGroup, _nInherGroupOfEnabled, _nInherGroupOfVisible );
        }
        public virtual bool SetReadStr( string str )
        {
            if ( String.IsNullOrEmpty( str ) ) return false;

            string[] ss = str.Split( ',', '=' );
            if ( ss == null || ss.Length < 5 )
                return false;

            for ( int i = 0; i < ss.Length; i++ ) {
                string s = ss[ i ].Trim();
                switch ( i ) {
                case 0:
                    if ( String.IsNullOrEmpty( s ) )
                        return false;
                    _strName = s.Replace( "\"", "" );
                    break;

                case 1:
                    if ( String.IsNullOrEmpty( s ) )
                        return false;
                    _strPwd = s.Replace( "\"", "" );
                    break;

                case 2:
                    try { _nGroup = Convert.ToInt32( s ); } catch { return false; }
                    break;

                case 3:
                    try { _nInherGroupOfEnabled = Convert.ToInt32( s ); } catch { return false; }
                    break;

                case 4:
                    try { _nInherGroupOfVisible = Convert.ToInt32( s ); } catch { return false; }
                    break;
                }
            }
            return true;
        }

        public DateTime LatestLoginTimestamp { get { return m_tLatestLogin; } }
        public DateTime LatestLogoutTimestamp {  get { return m_tLatestLogout; } }
        public virtual void Login()
        {
            m_tLatestLogin = DateTime.Now;
        }
        public virtual void Logout()
        {
            m_tLatestLogout = DateTime.Now;
        }
    }
}
