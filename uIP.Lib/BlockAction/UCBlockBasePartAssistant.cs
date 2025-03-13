using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        protected List<TBlockAssistant> _pAssistants = new List<TBlockAssistant>();
        protected bool _bAssistantsRunningPausing = false;

        private void DisposeAssistants()
        {
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants [ i ] == null ) continue;
                if ( _pAssistants [ i ]._pAssistant != null )
                    _pAssistants [ i ]._pAssistant.Dispose();
                _pAssistants [ i ] = null;
            }
            _pAssistants.Clear();
        }

        protected T GetAssistant<T>( int index ) where T : UCBlockBase
        {
            if (IsDispose || _pAssistants == null ) return null;
            try
            {
                if ( index < 0 )
                    index = _pAssistants.Count + index;
                if ( index < 0 || index >= _pAssistants.Count )
                    return null;
                return (T) _pAssistants [ index ]._pAssistant;
            } catch { return null; }
        }

        protected void ResetAssistantsFlags()
        {
            if ( IsDispose ) return;
            for(int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null ) continue;
                _pAssistants[ i ]._bDoneWithError = false;
                _pAssistants[ i ]._bRunningDone = false;
            }
        }

        protected void AssistantsRestToRun()
        {
            if (IsDispose) return;
            foreach(var a in _pAssistants)
            {
                if (a == null) continue;
                a._bDoneWithError |= false;
                a._bRunningDone = false;
                a._pAssistant.ResetToRun();
            }
        }

        protected void AssistantsClearFinalData()
        {
            foreach(var a in _pAssistants)
            {
                a?._pAssistant?.ClearFinalData();
            }
        }

        protected void SubassistantsSet( List<TBlockAssistant> sub, string name, UDataCarrier data )
        {
            if ( IsDispose || sub == null || sub.Count <= 0 )
                return;
            foreach ( var ss in sub )
            {
                if (ss == null || ss._pAssistant == null) continue;
                ss._pAssistant.Set( name, data );
                SubassistantsSet( ss._pAssistant._pAssistants, name, data );
            }
        }
        protected bool AssistantsSet( string pCtrl, UDataCarrier pData)
        {
            if ( IsDispose ) return false;
            for(int i = 0; i <_pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[i]._pAssistant == null ) continue;
                _pAssistants[ i ]._pAssistant.Set( pCtrl, pData );
                SubassistantsSet( _pAssistants [i]._pAssistant._pAssistants, pCtrl, pData );
            }

            return true;
        }
        protected bool AssistantsSet( object ctrlDesc, UDataCarrier pData )
        {
            return AssistantsSet( ctrlDesc?.ToString() ?? null, pData );
        }
        protected bool AssistantSet(int index, string pCtrl, UDataCarrier pData)
        {
            if (IsDispose ) return false;
            if ( index < 0 )
                index += _pAssistants.Count;
            if ( index < 0 || index >= _pAssistants.Count )
                return false;
            return _pAssistants [ index ]._pAssistant.Set( pCtrl, pData );
        }
        protected bool AssistantSet( int index, object ctrlDesc, UDataCarrier pData )
        {
            return AssistantSet(index, ctrlDesc?.ToString() ?? null, pData );
        }
        public bool AssistantSetT<T>(int index, object ctrlDesc, T v)
        {
            try
            {
                return AssistantSet(index, ctrlDesc, UDataCarrier.MakeOne(v));
            } catch { return false; }
        }
        protected bool AssistantsSet(string whichAssistant, string pCtrl, UDataCarrier pData)
        {
            if ( IsDispose ) return false;
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null ) continue;
                if ( _pAssistants[ i ]._pAssistant.ID == whichAssistant ) {
                   return  _pAssistants[ i ]._pAssistant.Set( pCtrl, pData );
                }
            }

            return false;
        }
        protected bool AssistantsSet( string whichAssistant, object ctrlDesc, UDataCarrier pData )
        {
            return AssistantsSet(whichAssistant, ctrlDesc?.ToString() ?? null, pData );
        }
        protected bool AssistantGet( string whichAssistant, string pCtrl, out UDataCarrier pData )
        {
            pData = null;
            if ( IsDispose ) return false;
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null ) continue;
                if ( _pAssistants[ i ]._pAssistant.ID == whichAssistant ) {
                    return _pAssistants[ i ]._pAssistant.Get( pCtrl, out pData );
                }
            }

            return false;
        }
        protected bool AssistantGet( string whichAssistant, object ctrlDesc, out UDataCarrier pData )
        {
            return AssistantGet( whichAssistant, ctrlDesc?.ToString() ?? null, out pData );
        }
        protected bool AssistantGet(int index, string pCtrl, out UDataCarrier pData )
        {
            pData=null;
            if (IsDispose) return false;
            if ( index < 0 )
                index += _pAssistants.Count;
            if ( index < 0 || index > _pAssistants.Count ) return false;
            return _pAssistants [index]._pAssistant.Get(pCtrl, out pData );
        }
        protected bool AssistantGet( int index, object ctrlDesc, out UDataCarrier pData )
        {
            return AssistantGet(index, ctrlDesc?.ToString()??null, out pData );
        }
        protected T AssistantGetT<T>(string whichAssistant, string ctrlName, T def)
        {
            try
            {
                if (!AssistantGet(whichAssistant, ctrlName, out var data))
                    return def;

                T r = ( T ) data.Data;
                data.Data = null;
                return r;
            } catch { return def; }
        }
        protected T AssistantGetT<T>(string whichAssistant, object ctrlDesc, T def)
        {
            return AssistantGetT( whichAssistant, ctrlDesc?.ToString() ?? null, def );
        }
        protected T AssistantGetT<T>( int index, string ctrlName, T def )
        {
            try
            {
                if ( !AssistantGet( index, ctrlName, out var data ) )
                    return def;
                T r = ( T ) data.Data;
                data.Data = null;
                return r;
            } catch { return def; }
        }
        public T AssistantGetT<T>(int index, object ctrlDesc, T def)
        {
            return AssistantGetT(index, ctrlDesc?.ToString() ?? null, def );
        }

        protected bool SetAssistantParam( int index_0, UDataCarrierSet param )
        {
            if ( IsDispose || _pAssistants.Count <= 0 ) {
                return false;
            }
            if ( index_0 < 0 || index_0 >= _pAssistants.Count || _pAssistants[ index_0 ] == null ) {
                return false;
            }
            _pAssistants [ index_0 ]._pParam?.Dispose();
            _pAssistants[ index_0 ]._pParam = param;
            return true;
        }
        protected bool SetAssistantParam(UCBlockBase blk, UDataCarrierSet param)
        {
            if ( IsDispose || _pAssistants.Count <= 0 ) {
                return false;
            }
            for (int i =0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null ) continue;
                if (_pAssistants[i]._pAssistant == blk) {
                    _pAssistants [ i ]._pParam?.Dispose();
                    _pAssistants[ i ]._pParam = param;
                    return true;
                }
            }
            return false;
        }

        // return:
        //  - RET_STATUS_OK: Done and success
        //  - RET_STATUS_NG: Done with error
        private Int32 RunWithAssistant( TBlockAssistant pWant, Int32 nBlockStateWhenFinish, bool bChangeBlock2ErrWhenAssistantFail )
        {
            if ( pWant._pAssistant == null ) {
                if ( _fpLog != null ) {
                    _fpLog( eLogMessageType.NORMAL, 0, String.Format( "[{0}]-ERROR- WorkWithAssistant() null assistant!", _strID ) );
                }
                return ( int ) UCBlockBaseRet.RET_STATUS_NG;

            }
            Int32 locRetCode = RunBlock( pWant._pAssistant, pWant._pParam );
            if ( locRetCode == ( int ) UCBlockBaseRet.RET_STATUS_OK )
                _nState = nBlockStateWhenFinish;
            else if ( locRetCode == ( int ) UCBlockBaseRet.RET_STATUS_NG ) {
                if ( bChangeBlock2ErrWhenAssistantFail )
                    _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
            }

            return locRetCode;
        }
        protected Int32 WorkWithAssistant( Int32 assistantIndex, Int32 nBlockStateWhenFinish, bool bChangeBlock2ErrWhenAssistantFail )
        {
            if ( IsDispose || _pAssistants.Count <= 0 ) {
                _nState = nBlockStateWhenFinish;
                return (int)UCBlockBaseRet.RET_STATUS_OK;
            }
            if ( assistantIndex < 0 || assistantIndex >= _pAssistants.Count || _pAssistants[assistantIndex] == null ) {
                if ( _fpLog != null) {
                    _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}]-ERROR- WorkWithAssistant() assistantIndex {1} out of range!", _strID, assistantIndex ));
                }
                return ( int ) UCBlockBaseRet.RET_STATUS_NG;
            }

            TBlockAssistant pWant = _pAssistants[ assistantIndex ];
            return RunWithAssistant( pWant, nBlockStateWhenFinish, bChangeBlock2ErrWhenAssistantFail );
        }
        protected Int32 WorkWithAssistant(UCBlockBase blk, Int32 nBlockStateWhenFinish, bool bChangeBlock2ErrWhenAssistantFail )
        {
            if ( IsDispose || _pAssistants.Count <= 0 ) {
                _nState = nBlockStateWhenFinish;
                return ( int ) UCBlockBaseRet.RET_STATUS_OK;
            }
            TBlockAssistant pWant = null;
            for (int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null ) continue;
                if (_pAssistants[i]._pAssistant == blk) {
                    pWant = _pAssistants[ i ];
                    break;
                }
            }
            if ( pWant == null )
                return ( int ) UCBlockBaseRet.RET_STATUS_NG;
            return RunWithAssistant( pWant, nBlockStateWhenFinish, bChangeBlock2ErrWhenAssistantFail );
        }

        #region Predefined state: pause
        protected void AssistantsFirstGotoPauseState()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return;

            _bAssistantsRunningPausing =  true;
            // reset all assistants flags
            ResetAssistantsFlags();
            // all assistants' state change to pause
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[i] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._pAssistant.IsRunning )
                    _pAssistants[ i ]._pAssistant.Pause();
            }
        }
        protected bool AssistantsRunToPauseState()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            bool locDone = true;
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._bRunningDone )
                    continue;
                if ( !_pAssistants[ i ]._pAssistant.IsRunning ) {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = false;
                    continue;
                }
                // run assistant block
                Int32 locRet = RunBlock( _pAssistants[ i ]._pAssistant, _pAssistants[ i ]._pParam );
                if ( locRet == (int)UCBlockBaseRet.RET_STATUS_PAUSED ) // expect state -> done
                {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = false;
                } else if ( locRet == ( int ) UCBlockBaseRet.RET_STATUS_NG ) // done with error
                  {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = true;
                } else // unexpect state -> not done yet
                    locDone = false;
            }

            return locDone;
        }
        protected bool AssistantsHandlingPauseState( out bool bErr)
        {
            bErr = false;
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            // need to run assistants pausing
            if ( _bAssistantsRunningPausing ) {
                if ( AssistantsRunToPauseState() ) {
                    // all done: reset flag
                    _bAssistantsRunningPausing = false;
                    bool gotErr = false;

                    if ( _fpLog != null ) {
                        _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}] exec pause done, proceed state = {1}.", _strID, _nProceedingState) );
                    }
                    for ( int i = 0; i < _pAssistants.Count; i++ ) {
                        if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                            continue;

                        if ( _pAssistants[ i ]._bDoneWithError ) // check any error
                        {
                            gotErr =  true;
                            if ( _fpLog != null ) {
                                _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}] exec {1} pause got error.", _strID, _pAssistants[i]._pAssistant.ID) );
                            }
                        }
                    }
                    if ( gotErr ) // auto change to error state
                    {
                        _nState = (int)UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                        bErr = gotErr;
                    }
                    return true;
                }
            } else
                return true;
            return false;
        }
        #endregion

        #region Predefined action in pause state: proceed
        protected void AssistantsFirstProceed()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return;

            // reset all assistants flags, prepare to check
            ResetAssistantsFlags();
            // call to pause
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._pAssistant.IsRunning )
                    _pAssistants[ i ]._pAssistant.Proceed();
            }
        }
        protected bool AssistantsGoProceeding()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            bool locDone = true;
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._bRunningDone )
                    continue;
                if ( !_pAssistants[ i ]._pAssistant.IsRunning ) {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError =  false;
                    continue;
                }
                // run assistant block
                Int32 locRet = RunBlock( _pAssistants[ i ]._pAssistant, _pAssistants[ i ]._pParam );
                if ( locRet == (int)UCBlockBaseRet.RET_STATUS_PAUSE_RELEASED ) // expect state -> done
                {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = false;
                } else if ( locRet == (int)UCBlockBaseRet.RET_STATUS_NG ) // done with error
                  {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = true;
                } else // still running
                    locDone = false;
            }

            return locDone;
        }
        protected bool AssistantsHandlingProceed( out bool bErr)
        {
            bErr = false;
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            bool bDone = AssistantsGoProceeding(); // run all assistants' blocks
            if ( bDone ) {
                bool gotErr = false;
                for ( int i = 0; i < _pAssistants.Count; i++ ) {
                    // discard null assistant
                    if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                        continue;

                    if ( _pAssistants[ i ]._bDoneWithError ) // check any error when run assistant block
                    {
                        gotErr = true;
                        if ( _fpLog != null ) {
                            _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}] exec {1} proceeding got error.", _strID, _pAssistants[i]._pAssistant.ID ) );
                        }
                    }
                }
                if ( gotErr ) {
                    _nState = (int)UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                    bErr = gotErr;
                }
            }

            return bDone;
        }
        #endregion

        #region Predefined state: stop
        protected void AssistantsFirstGotoStopState()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return;

            // reset all assistants flags, prepare to check
            ResetAssistantsFlags();
            // call to stop
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._pAssistant.IsRunning )
                    _pAssistants[ i ]._pAssistant.Stop();
            }
        }
        protected bool AssistantsRunToStopState()
        {
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            bool isAllDone =  false;
            bool locDone =  true;
            for ( int i = 0; i < _pAssistants.Count; i++ ) {
                if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                    continue;
                if ( _pAssistants[ i ]._bRunningDone )
                    continue;
                if ( !_pAssistants[ i ]._pAssistant.IsRunning ) {
                    _pAssistants[ i ]._bRunningDone =  true;
                    _pAssistants[ i ]._bDoneWithError =  false;
                    continue;
                }
                // run assistant block
                Int32 locRet = RunBlock( _pAssistants[ i ]._pAssistant, _pAssistants[ i ]._pParam );
                if ( locRet == (int)UCBlockBaseRet.RET_STATUS_OK ) // expect state -> done
                {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = false;
                } else if ( locRet == ( int ) UCBlockBaseRet.RET_STATUS_NG ) // done with error
                  {
                    _pAssistants[ i ]._bRunningDone = true;
                    _pAssistants[ i ]._bDoneWithError = true;
                } else // still running
                    locDone = false;
            }

            isAllDone = locDone;
            return isAllDone;
        }
        protected bool AssistantsHandlingStopState( out bool bErr)
        {
            bErr = false;
            if ( IsDispose || _pAssistants.Count <= 0 )
                return true;

            bool bDone = AssistantsRunToStopState();
            if ( bDone ) {
                for ( int i = 0; i < _pAssistants.Count; i++ ) {
                    if ( _pAssistants[ i ] == null || _pAssistants[ i ]._pAssistant == null )
                        continue;
                    if ( _pAssistants[ i ]._bDoneWithError ) {
                        bErr = true;
                        if ( _fpLog != null) {
                            _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}] exec {1} stopping got error.", _strID, _pAssistants[i]._pAssistant.ID ) );
                        }
                    }
                }
                if ( bErr ) {
                    if ( _fpLog != null ) {
                        _fpLog(eLogMessageType.NORMAL, 0, String.Format( "[{0}] stopping got error change state to STATE_ERROR.", _strID));
                    }
                    _nState = (int)UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                }
            }

            return bDone;
        }
        #endregion
    }
}
