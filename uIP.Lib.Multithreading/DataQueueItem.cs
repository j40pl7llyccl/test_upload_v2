using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace uIP.Lib.Multithreading
{
    /// <summary>
    /// Class type only
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DataQueueItem<T> : IDisposable
    {
        private T _Data;
        private DataQueuePool<T> _Owner;

        private bool _bDisposed;
        private bool _bDisposing;

        public DataQueuePool<T> Owner
        {
            get { return _Owner; }
            set { _Owner = value; }
        }

        public T Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        public DataQueueItem()
        {
            _Owner = null;

            _bDisposed = false;
            _bDisposing = false;
        }

        public DataQueueItem( T data )
        {
            _Data = data;
            _Owner = null;

            _bDisposed = false;
            _bDisposing = false;
        }

        public DataQueueItem( T data, DataQueuePool<T> owner )
        {
            _Data = data;
            _Owner = owner;

            _bDisposed = false;
            _bDisposing = false;
        }

        ~DataQueueItem()
        {
            Dispose( false );
        }

        #region >>> IDisposable <<<

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing )
        {
            if ( _bDisposing )
                return;
            _bDisposing = true;

            if ( !_bDisposed )
            {
                if ( _Data is IDisposable d )
                    d?.Dispose();
                else if ( _Data is IEnumerable i )
                {
                    foreach ( var ii in i )
                        ( ii as IDisposable )?.Dispose();
                }
            }

            _bDisposed = true;
            _bDisposing = false;
        }

        #endregion

        public void Back2Owner()
        {
            if ( _bDisposing || _bDisposed )
                return;
            if ( _Owner == null )
                return;

            if ( this.Data != null )
            {
                IDataPoolItemDataReset iReset = this.Data as IDataPoolItemDataReset;
                if ( iReset != null )
                    iReset.Reset();
            }

            _Owner.Put( this );
        }

    }
}
