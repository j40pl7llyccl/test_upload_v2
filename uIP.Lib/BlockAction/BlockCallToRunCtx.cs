using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uIP.Lib.BlockAction
{
    public class BlockCallToRunCtx : IDisposable
    {
        private bool _bDisposed = false;
        private bool _bDisposing = false;
        public UInt32 SN { get; private set; } = 0;
        public List<UCBlockBase> BlockInstances { get; set; } = new List<UCBlockBase>();
        public List<UDataCarrierSet> BlockInputs { get; set; } = new List<UDataCarrierSet>();
        private bool Terminate { get; set; } = false;
        private bool IsRunToEnd { get; set; } = false;

        public BlockCallToRunCtx(UInt32 sn)
        {
            SN = sn;
        }

        public void Dispose()
        {
            if ( _bDisposing || _bDisposed )
                return;
            _bDisposing = true;
            if (!IsRunToEnd)
            {
                Stop();
                DateTime prev = DateTime.Now;
                while(!IsRunToEnd)
                {
                    var diff = DateTime.Now - prev;
                    if ( diff.TotalSeconds > 10 )
                        throw new Exception($"[BlockCallToRunCtx] close work SN={SN} timeout" );
                    Thread.Sleep( 0 );
                }
            }
            if (BlockInputs != null)
            {
                foreach(var input in BlockInputs)
                    input?.Dispose();
            }

            if (BlockInstances != null)
            {
                foreach(var instance in BlockInstances)
                    instance?.Dispose();
            }

            _bDisposed = true;
            _bDisposing = false;
        }

        public bool Run()
        {
            UDataCarrierSet prev = null;
            bool isErr = false;
            for(int runIndex = 0; !Terminate && !_bDisposing && runIndex < BlockInstances.Count; runIndex++ )
            {
                UCBlockBase blk = BlockInstances [ runIndex ];
                UDataCarrierSet input = BlockInputs [ runIndex ];

                blk.PrevBlockRunDat = prev;
                
                bool stopLoop = false;

                for( ; !stopLoop && !_bDisposing ; )
                {
                    switch ( blk.Run( input ) )
                    {
                        case ( int ) UCBlockBaseRet.RET_STATUS_OK:
                            stopLoop = true;
                            break;

                        case ( int ) UCBlockBaseRet.RET_STATUS_NG:
                            stopLoop = true;
                            isErr = true;
                            break;
                    }
                }
                if ( isErr )
                    break;

                prev = blk.BlockRunDat;
            }
            IsRunToEnd = true;
            return !isErr;
        }

        public void Stop()
        {
            if ( BlockInstances == null )
                return;

            Terminate = true;
            foreach(var blk in BlockInstances )
                blk?.Stop();
        }
    }
}
