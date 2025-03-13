using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib
{
    public class UserBasicInfo
    {
        protected string _strName = null;
        protected string _strPwd = null;
        protected int _nGroup = int.MaxValue;
        protected int _nInherGroupOfEnabled = -1;
        protected int _nInherGroupOfVisible = -1;

        public UserBasicInfo() { }
        public UserBasicInfo( string name, string pwd, int group )
        {
            _strName = String.IsNullOrEmpty( name ) ? "" : String.Copy( name );
            _strPwd = String.IsNullOrEmpty( pwd ) ? "" : String.Copy( pwd );
            _nGroup = group;
        }
        public UserBasicInfo( string name, string pwd, int group, int inherEnable, int inherVisible )
        {
            _strName = String.IsNullOrEmpty( name ) ? "" : String.Copy( name );
            _strPwd = String.IsNullOrEmpty( pwd ) ? "" : String.Copy( pwd );
            _nGroup = group;
            _nInherGroupOfEnabled = inherEnable;
            _nInherGroupOfVisible = inherVisible;
        }

        public string Name { get { return _strName; } set { _strName = String.IsNullOrEmpty( value ) ? "" : String.Copy( value ); } }
        public string Pwd { get { return _strPwd; } set { _strPwd = String.IsNullOrEmpty( value ) ? "" : String.Copy( value ); } }
        public int Group { get { return _nGroup; } set { _nGroup = value; } }
        public int InherEnabledGroup { get { return _nInherGroupOfEnabled; } set { _nInherGroupOfEnabled = value; } }
        public int InherVisibleGroup { get { return _nInherGroupOfVisible; } set { _nInherGroupOfVisible = value; } }
    }

    public delegate void fpUserLoginNotification( UserBasicInfo currUser, int nCurrEnabledLevel, int nCurrVisibleLevel );
}
