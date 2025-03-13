using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.Lib.UsrControl
{
    public delegate void fpUserRightsOutMessage( string str );

    //
    // Component description of access right
    //
    public class ComponentAccRight
    {
        public string _strOwner = null; // name of form, name of form full type name
        public string _strName = null;
        public int _nEnableLevel = 0;
        public int _nVisibleLevel = 0;
        public ComponentAccRight() { }
        public ComponentAccRight( string owner, string name, int enableLvl, int visibleLvl )
        {
            _strOwner = owner;
            _strName = name;
            _nEnableLevel = enableLvl;
            _nVisibleLevel = visibleLvl;
        }
    }

    // extra process for tab page
    internal class KeepingPageCtrl
    {
        internal string[] _strOwner = null;
        internal Control _OwnerForm = null;
        internal TabPage _tbCtrl = null;
        internal Control _Parent = null;
        internal int _tbIndex = -1;
        internal KeepingPageCtrl() { }
    }

    // if want to control, need to have the form instance.
    // So, make a class to store it and maintain it
    internal class KeepingFormCtrl
    {
        internal Form _Form;
        internal bool _bHandle;

        internal KeepingFormCtrl( Form frm, bool bHandle )
        {
            _Form = frm;
            _bHandle = bHandle;
        }
        internal void Close()
        {
            if ( _Form != null && _bHandle ) {
                if ( !_Form.Disposing && !_Form.IsDisposed ) {
                    _Form.Dispose();
                    _Form = null;
                }
            }
        }

        internal static KeepingFormCtrl New<T>( bool bHandle = true )
        {
            if ( !typeof( T ).IsSubclassOf( typeof( Form ) ) )
                return null;

            Form frm = Activator.CreateInstance<T>() as Form;
            return new KeepingFormCtrl( frm, bHandle );
        }
    }

    //
    // Define a user of group
    //
    public class UserGroup
    {
        public int _nGroup;
        public string _strName = null;
        public Dictionary<string, UserControlData> _Users = new Dictionary<string, UserControlData>();

        public UserGroup() { }
    }

    //
    // Define the group of access right
    //
    public class UserGroupAccessRight
    {
        public string _strNameOfGroup;
        public int _nEnabledLevel;
        public int _nVisibleLevel;
        public UserGroupAccessRight() { }
        public UserGroupAccessRight( string name, int enableLvl, int visibleLvl )
        {
            _strNameOfGroup = name;
            _nEnabledLevel = enableLvl;
            _nVisibleLevel = visibleLvl;
        }
    }

    //
    // Multi-language
    //
    public class RegControl
    {
        public Control _OwnerCtrl = null;
        public Control _RegCtrl = null;
        public RegControl() { }
        public RegControl( Control owner, Control reg )
        {
            _OwnerCtrl = owner;
            _RegCtrl = reg;
        }
    }

}
