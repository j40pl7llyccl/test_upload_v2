using System;

namespace uIP.Lib.Utility
{
    public static class MsMethods
    {
        public static void WaitFromAppDoEvent( Int32 nSecond )
        {
            DateTime prevTm = DateTime.Now;
            while ( true )
            {
                System.Windows.Forms.Application.DoEvents();
                TimeSpan diff = DateTime.Now - prevTm;
                if ( diff.TotalSeconds > Convert.ToDouble( nSecond ) || !ResourceManager.SystemAvaliable )
                    break;
            }
        }
    }
}