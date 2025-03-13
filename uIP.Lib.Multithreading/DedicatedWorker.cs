using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uIP.Lib.Multithreading
{
    public enum DedicateWorkNotifyCode : int
    {
        EndByStop = 0,
        EndByTerminate,
        EndNeverExec,
        StartFail,
        BeginFail,
        ExecFail,
        NextFail,
        DelegateFail,
    }
    public delegate bool DedicatedWorkBeginHandler( object inpCtx, out object runCtx, out Action<object> runCtxResHandler );
    public delegate bool DedicatedWorkStopHandler( object inpCtx, object runCtx );
    public delegate bool DedicatedWorkNextHandler( object inpCtx, out object runCtx, out Action<object> runCtxResHandler );
    public delegate bool DedicatedWorkExecHandler( object inpCtx, object runCtx );
    public delegate void DedicateWorkEndNotifyHandler( object inpCtx, DedicateWorkNotifyCode code );

    public interface IManageDedicateWorker
    {
        void RemoveDW( object worker );
        bool AddDW( object worker );
    }

    public class DedicatedWorker : IDisposable
    {
        public bool IsRunning { get; private set; } = false;
        private IManageDedicateWorker Owner { get; set; }
        private object InputCtx { get; set; } = null;
        private Action<object> fInputCtxResHandler { get; set; } = null;
        private Thread Runner { get; set; } = null;
        internal bool Terminate { get; set; } = false;
        public bool Disposed { get; set; } = false;
        internal DedicatedWorkBeginHandler fBegin { get; set; } = null;
        internal DedicatedWorkStopHandler fStop { get; set; } = null;
        internal DedicatedWorkNextHandler fNext { get; set; } = null;
        internal DedicatedWorkExecHandler fExec { get; set; } = null;
        internal DedicateWorkEndNotifyHandler fNotify { get; set; } = null;
        public DedicatedWorker( IManageDedicateWorker owner, object inpCtx, Action<object> inpCtxResHandler = null )
        {
            Owner = owner;
            InputCtx = inpCtx;
            fInputCtxResHandler = inpCtxResHandler;
        }

        internal bool Start()
        {
            if ( fBegin == null || fNext == null || fExec == null)
            {
                //throw new InvalidOperationException( "delegate function invalid" );
                Notify2HandleInputCtx( DedicateWorkNotifyCode.DelegateFail );
                return false;
            }

            if ( Runner != null )
                return true;

            Runner = new Thread( Exec );
            if ( Owner != null )
            {
                // add fail
                if (!Owner.AddDW(this))
                {
                    Notify2HandleInputCtx( DedicateWorkNotifyCode.StartFail );
                    return false;
                }
            }
            Runner.Start();
            
            return true;
        }

        public bool Start( DedicatedWorkBeginHandler beg, DedicatedWorkNextHandler next, DedicatedWorkExecHandler exec, DedicateWorkEndNotifyHandler notify = null, DedicatedWorkStopHandler stop = null )
        {
            if ( IsRunning )
                return true;
            fBegin = beg;
            fNext = next;
            fExec = exec;
            fStop = stop;
            fNotify = notify;
            return Start();
        }

        public void Dispose()
        {
            if ( Disposed )
                return;
            Disposed = true;
            Terminate = true;
            if ( Runner != null )
            {
                if (IsRunning)
                    Runner.Join();
            }
            else
            {
                Notify2HandleInputCtx( DedicateWorkNotifyCode.EndNeverExec );
                Owner?.RemoveDW( this );
            }
        }

        private static void HandleCtxResource(object ctx, Action<object> handler)
        {
            if ( handler != null ) handler( ctx );
            else if (ctx != null)
            {
                if (ctx is IDisposable d) d?.Dispose();
                else if (ctx is IEnumerable collection && collection != null)
                {
                    foreach( var c in collection)
                    {
                        if (c is IDisposable dd) dd?.Dispose();
                    }
                }
            }
        }
        private void Notify2HandleInputCtx( DedicateWorkNotifyCode code )
        {
            fNotify?.Invoke( InputCtx, code );
            HandleCtxResource( InputCtx, fInputCtxResHandler );
            InputCtx = null;
            fInputCtxResHandler = null;
        }

        private void Exec()
        {
            IsRunning = true;

            object runCtx = null;
            Action<object> runCtxHandler = null;
            bool status = fBegin( InputCtx, out runCtx, out runCtxHandler );
            bool isStop = false;
            DedicateWorkNotifyCode code = DedicateWorkNotifyCode.EndByTerminate;
            if ( !status )
            {
                UMultithreading._LogWrn?.Invoke( 0, $"DedicatedWorker: begin from call {fBegin.Method} with {InputCtx?.GetType()}" );
                Notify2HandleInputCtx( DedicateWorkNotifyCode.BeginFail );
                Owner?.RemoveDW( this ); // call owner to handle resource
                IsRunning = false; // set not run
                return;
            }
            do
            {
                // check stop
                isStop = fStop?.Invoke( InputCtx, runCtx ) ?? false;
                if ( isStop )
                {
                    code = DedicateWorkNotifyCode.EndByStop;
                    break;
                }

                // exec
                status = fExec( InputCtx, runCtx );
                if ( !status )
                {
                    code = DedicateWorkNotifyCode.ExecFail;
                    break;
                }
                // handle ctx for run
                runCtxHandler?.Invoke( runCtx );
                runCtx = null;
                runCtxHandler = null;

                // check stop
                isStop = fStop?.Invoke( InputCtx, runCtx ) ?? false;
                if ( isStop )
                {
                    code = DedicateWorkNotifyCode.EndByStop;
                    break;
                }

                // next
                status = fNext( InputCtx, out runCtx, out runCtxHandler );
                if (!status)
                {
                    code = DedicateWorkNotifyCode.NextFail;
                    break;
                }
            } while ( !Disposed && !Terminate );
            IsRunning = false;

            // need to free run ctx
            if (runCtx != null) runCtxHandler?.Invoke( runCtx );
            // log
            UMultithreading._LogNor( $"DedicatedWorker: end with data {InputCtx?.ToString()}" );
            // notify and handle input ctx
            Notify2HandleInputCtx( code );
            // call owner to handle resource
            Owner?.RemoveDW( this );
        }
    }
}
