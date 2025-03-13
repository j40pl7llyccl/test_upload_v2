using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uIP.Lib.BlockAction
{
    public partial class UCBlockBase
    {
        /*
         * Pattern 1: 由此代為執行基礎的 state change, 使用者可以專注開發功能
         * - 使用時提供兩個 delegate 作為協同 callback
         *   - FirstStateInfo: 註冊執行 UCBLOCK_STATE_PREPARING 時的 callback, 沒有提供則會執行失敗
         *   - DesignedStateInfo: 註冊不是預設 state 時的 callback, 沒有提供會執行失敗
         */
        //
        // Pattern 1
        //
        // Config
        //  - _nProceedingBackState: switch to which state
        //  - FirstStateInfo()
        //  - DesignedStateInfo()
        protected Int32 _nProceedingBackState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;
        protected object _p1stStateHandlerContext = null; // child instance of this class
        protected fpUCBlockStateCallback _fp1stStateHandler = null;
        protected object _pDesignedStateHandlerContext = null; // child instance of this class
        protected fpUCBlockStateCallback _fpDesignedStateHandler = null;

        protected object _ContextOfHandleStopState = null;
        protected fpUCBlockHandlePrefDefState _fpHandleStopState = null;
        protected object _ContextOfHandlePauseState = null;
        protected fpUCBlockHandlePrefDefState _fpHandlePauseState = null;
        protected object _ContextOfHandleProceedState = null;
        protected fpUCBlockHandlePrefDefState _fpHandleProceedState = null;
        protected object _ContextOfHandleFinishState = null;
        protected fpUCBlockHandlePrefDefState _fpHandleFinishState = null;
        protected object _ContextOfHandleErrorState = null;
        protected fpUCBlockHandlePrefDefState _fpHandleErrorState = null;

        protected Int32 WellDefinedRunner_1( UDataCarrierSet pParams )
        {
            //bool bError = false;
            //tYUX_BOOL locStopDone = (tYUX_BOOL)false;
            bool locStopDoneErr = false;
            bool locPauseErr = false;
            //tYUX_BOOL locProceedDone = (tYUX_BOOL)false;
            bool locProceedDoneErr = false;
            Int32 locPrevState = _nPrevState;
            Int32 locCurrState = _nState;

            if ( _bPause ) {
                _bPause = false;
                if ( locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR &&
                    locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH &&
                    locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_PAUSING &&
                    locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_STOP ) {
                    _nProceedingState = locCurrState;
                    _nState = locCurrState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_PAUSING;
                }
            }

            if ( _bStop ) {
                _bStop = false;
                if ( locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR &&
                    locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH &&
                    locCurrState != ( int ) UCBlockStateReserved.UCBLOCK_STATE_STOP ) {
                    _nState = locCurrState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_STOP;
                }
            }

            bool locFirstChanged = locCurrState != _nPrevState; // check 1st changing state
            _nLastPrevState = locFirstChanged ? _nPrevState : _nLastPrevState;
            //_nPrevState = locCurrState; // config previous to current => switch to do before returning
            if ( locFirstChanged )
            {
                _fpStateChangedCall?.Invoke( this, locCurrState );
                AddBlockState( locCurrState );
            }

            switch ( locCurrState ) {
            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_PREPARING:
                if ( locFirstChanged ) {
                    _bPause = false;
                    _bProceed = false;
                    _bStop = false;

                    _nProceedingBackState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA;

                    ClearBlockKeeper();
                    AddBlockState( locCurrState );
                    AssistantsRestToRun();
                }

                if ( _fp1stStateHandler == null )
                    _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                else
                    _fp1stStateHandler( _p1stStateHandlerContext, pParams, locFirstChanged, locCurrState, locPrevState, _BlockStateStorage, ref _nState );

                break;

            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH:
                if ( _fpHandleFinishState != null ) {
                    if ( !_fpHandleFinishState( _ContextOfHandleFinishState, locFirstChanged ) )
                        break;
                }
                _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_PREPARING; // back to 1st step of state machine
                _nPrevState = locCurrState; // config previous to current
                return ( int ) UCBlockBaseRet.RET_STATUS_OK;

            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_STOP:
                if ( locFirstChanged )
                    AssistantsFirstGotoStopState();

                if ( _fpHandleStopState != null ) {
                    if ( AssistantsHandlingStopState( out locStopDoneErr ) && _fpHandleStopState( _ContextOfHandleStopState, locFirstChanged ) )
                        _nState = locStopDoneErr ? ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR : ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH;
                } else {
                    if ( AssistantsHandlingStopState( out locStopDoneErr ) )
                        _nState = locStopDoneErr ? ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR : ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH;
                }
                break;

            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_PAUSING:
                if ( locFirstChanged )
                    AssistantsFirstGotoPauseState();

                if ( _fpHandlePauseState != null ) {
                    if ( !AssistantsHandlingPauseState( out locPauseErr ) || !_fpHandlePauseState( _ContextOfHandlePauseState, locFirstChanged ) )
                        break;
                } else {
                    if ( !AssistantsHandlingPauseState( out locPauseErr ) )
                        break;
                }

                if ( locPauseErr ) {
                    _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                    break;
                }

                if ( _bProceed ) {
                    //if ( _fpHandleProceedState != null ) {
                    //    if ( !AssistantsHandlingProceed( out locProceedDoneErr ) || !_fpHandleProceedState(_ContextOfHandleProceedState, locFirstChanged ) )
                    //        break;

                    //} else {
                    if ( !AssistantsHandlingProceed( out locProceedDoneErr ) ) // make all assistants recover from proceeding
                        break;
                    //}
                    if ( locProceedDoneErr ) {
                        _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                        break;
                    }


                    _bProceed = false;
                    if ( _fpHandleProceedState != null ) { // not done yet, change to exec block proceeding callback delegate
                        _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_PROCEEDING;
                    } else {
                        _nState = _nProceedingBackState == ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA ? _nProceedingState : _nProceedingBackState;
                    }
                    _nPrevState = locCurrState; // config previous to current
                    return ( int ) UCBlockBaseRet.RET_STATUS_PAUSE_RELEASED;
                }
                _nPrevState = locCurrState; // config previous to current
                return ( int ) UCBlockBaseRet.RET_STATUS_PAUSED;

            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR:
                if ( _fpHandleErrorState != null ) {
                    if ( !_fpHandleErrorState( _ContextOfHandleErrorState, locFirstChanged ) )
                        break;
                }
                _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_PREPARING;
                _nPrevState = locCurrState; // config previous to current
                return ( int ) UCBlockBaseRet.RET_STATUS_NG;

            case ( int ) UCBlockStateReserved.UCBLOCK_STATE_PROCEEDING:
                if ( _fpHandleProceedState == null ) {
                    if ( _fpLog != null ) _fpLog( eLogMessageType.WARNING, 50, String.Format( "[WellDefinedRunner_1] {0} cannot run without delegate", UCBlockStateReserved.UCBLOCK_STATE_PROCEEDING.ToString() ) );
                    _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                    break;
                }

                if ( _fpHandleProceedState( _ContextOfHandleProceedState, locFirstChanged ) ) { // done delegate, change to except state
                    _nState = _nProceedingBackState == ( int ) UCBlockStateReserved.UCBLOCK_STATE_NA ? _nProceedingState : _nProceedingBackState;
                }

                break;

            default:
                if ( _fpDesignedStateHandler == null )
                    _nState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_ERROR;
                else
                    _fpDesignedStateHandler( _pDesignedStateHandlerContext, pParams, locFirstChanged, locCurrState, locPrevState, _BlockStateStorage, ref _nState );

                break;
            }

            _nPrevState = locCurrState; // config previous to current
            return ( int ) UCBlockBaseRet.RET_STATUS_RUNNING;
        }

        public void FirstStateInfo( object pContext, fpUCBlockStateCallback fpHandler )
        {
            _p1stStateHandlerContext = pContext;
            _fp1stStateHandler = fpHandler;
        }
        public void DesignedStateInfo( object pContext, fpUCBlockStateCallback fpHandler )
        {
            _pDesignedStateHandlerContext = pContext;
            _fpDesignedStateHandler = fpHandler;
        }
        public void SetHandleDefStateStop( object context, fpUCBlockHandlePrefDefState fp )
        {
            _ContextOfHandleStopState = context;
            _fpHandleStopState = fp;
        }
        public void SetHandleDefStatePause( object context, fpUCBlockHandlePrefDefState fp )
        {
            _ContextOfHandlePauseState = context;
            _fpHandlePauseState = fp;
        }
        public void SetHandleDefStateProceed( object context, fpUCBlockHandlePrefDefState fp )
        {
            _ContextOfHandleProceedState = context;
            _fpHandleProceedState = fp;
        }
        public void SetHandleDefStateFinish( object context, fpUCBlockHandlePrefDefState fp )
        {
            _ContextOfHandleFinishState = context;
            _fpHandleFinishState = fp;
        }
        public void SetHandleDefStateError( object context, fpUCBlockHandlePrefDefState fp )
        {
            _ContextOfHandleErrorState = context;
            _fpHandleErrorState = fp;
        }

        protected void ConfigWellDefined1Callback( 
            object ctxOf1st = null, object ctxOfDesign = null,
            bool bHandStop = false, object contextOfStop = null, 
            bool bHandPause = false, object contextOfPause = null, 
            bool bHandProceed = false, object contextOfProceed = null,
            bool bHandFinish = false, object contextOfFinish = null,
            bool bHandError = false, object contextOfError = null )
        {
            _fp1stStateHandler = WellDefined1HandleFirstState;
            _p1stStateHandlerContext = ctxOf1st;

            _fpDesignedStateHandler = WellDefined1HandleDesignedState;
            _pDesignedStateHandlerContext = ctxOfDesign;

            if ( bHandStop ) {
                _ContextOfHandleStopState = contextOfStop;
                _fpHandleStopState = WellDefined1HandleStopState;
            }
            if ( bHandPause ) {
                _ContextOfHandlePauseState = contextOfPause;
                _fpHandlePauseState = WellDefined1HandlePauseState;
            }
            if ( bHandProceed ) {
                _ContextOfHandleProceedState = contextOfProceed;
                _fpHandleProceedState = WellDefined1HandleProceedState;
            }
            if ( bHandFinish ) {
                _ContextOfHandleFinishState = contextOfFinish;
                _fpHandleFinishState = WellDefined1HandleFinishState;
            }
            if ( bHandError ) {
                _ContextOfHandleErrorState = contextOfError;
                _fpHandleErrorState = WellDefined1HandleErrorState;
            }
        }

        // nNextState: must fill next state to make it work
        virtual protected void WellDefined1HandleFirstState( object pContext, UDataCarrierSet pParams, bool bFirstEnter, Int32 nCurrState, Int32 nCurrPrevState, List<Int32> pStateHistory, ref Int32 nNextState )
        {
            nNextState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH;
            //return ( int ) UCBlockBaseRet.RET_STATUS_OK;
        }
        virtual protected void WellDefined1HandleDesignedState( object pContext, UDataCarrierSet pParams, bool bFirstEnter, Int32 nCurrState, Int32 nCurrPrevState, List<Int32> pStateHistory, ref Int32 nNextState )
        {
            nNextState = ( int ) UCBlockStateReserved.UCBLOCK_STATE_FINISH;
            //return ( int ) UCBlockBaseRet.RET_STATUS_OK;
        }

        virtual protected bool WellDefined1HandleStopState( object context, bool isFirstTime )
        {
            return true; // true is done
        }
        virtual protected bool WellDefined1HandlePauseState( object context, bool isFirstTime )
        {
            return true; // true is done
        }
        virtual protected bool WellDefined1HandleProceedState( object context, bool isFirstTime )
        {
            _nProceedingBackState = _nProceedingState; // TODO: change to actual state
            return true; // true is done
        }
        virtual protected bool WellDefined1HandleFinishState( object context, bool isFirstTime )
        {
            return true; // true is done
        }
        virtual protected bool WellDefined1HandleErrorState( object context, bool isFirstTime )
        {
            return true; // true is done
        }
    }
}
