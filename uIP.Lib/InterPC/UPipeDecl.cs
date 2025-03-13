using System;

namespace uIP.Lib.InterPC
{
    public enum eUPipeState : int
    {
        NA = 0,
        Connecting,
        Reading,
        Writing,
        DataReady,
        TxError,
        RxError,
        ConnectionFailure,
    }

    public delegate void fpUPipeDebug( string msg, int lvl );
    public delegate void fpUSrvProcPipeRxDat( byte[] buff, UInt32 nRd, ref byte[] rsp, ref UInt32 nBegRspIdx, ref UInt32 nRspCount );
    public delegate void fpUCltProcPipeRxDat( eUPipeState status, byte[] buff, UInt32 nRx, UInt32 statusCode );
    public delegate void fpUNamedPipeOpenStatus( string name, bool bStatus );

    public interface IPipeClientComm
    {
        bool Ready { get; }
        bool Add( byte[] txBuff, Int32 offset, Int32 len, fpUCltProcPipeRxDat fp );
    }
}
