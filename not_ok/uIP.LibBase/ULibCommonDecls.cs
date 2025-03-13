using System;

namespace uIP.LibBase
{
    public enum eLogMessageType
    {
        NORMAL,
        WARNING,
        ERROR,
    }
    public delegate void fpLogMessage( eLogMessageType type, Int32 id, String message );

}
