using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.LibBase
{
    public delegate void fpUserLoginCall( UserBasicInfo usr, int nCurEnabledLevel, int nCurVisibleLevel );
    public interface IUserManagement
    {
        UserBasicInfo LoginedUser { get; }
        int CurrentEnabledLevel { get; }
        int CurrentVisibleLevel { get; }
        bool Login( string userName, string pwd );
        bool LoginWithUI();
        void Logout();
        void LogoutWithUI();
        bool ChangePwdWithUI();
        void InstallCallback( fpUserLoginCall func );
        void RemoveCallback( fpUserLoginCall func );
        void OpenEditor();
    }
}
