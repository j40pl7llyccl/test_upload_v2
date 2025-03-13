using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.LibBase
{
    public interface IGuiAclManagement
    {
        Form CreateForm<T>( bool bHandle = true );
        bool AddForm( Form frm, bool bHandle = true );
        void RmvForm( Form frm, bool bBySettingHandleFormInst = true );
        Form CreateShowDialog<T>( int nEnabledLvl, int nVisibleLvl );
    }
}
