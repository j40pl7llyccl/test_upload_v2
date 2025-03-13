using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public interface IPollingRunner
    {
        string GivenName { get; }
        object AddPolling( UDataCarrier context, fpUCBlockHandleDatCarrier fpHandContext,
            fpBlockPollingTimeoutCallback fpCallback, int nInterval, int nTrg );
        void RemovePolling( object item );
    }
}
