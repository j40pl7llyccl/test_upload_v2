using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    public partial class UScript
    {
        public static bool RunningControlFlow( bool enableLog, CancelExecScript cancel, List<UScript> scriptSet, UScript script,
                                               bool bSyncCall, int nWaitTimeout,
                                               bool isKeepingHistCarriers,
                                           out List<UScriptHistoryCarrier> retHistCarriers,
                                               UDataCarrier[] input = null,
                                               fpUDataCarrierSetResHandler inputHandler = null,
                                               bool bResetBeforeExec = true
            )
        {
            retHistCarriers = null;

            if ( !script.IsAvailable || script == null )
                return false;

            // binding the script to cancel
            if ( cancel != null ) cancel.RunningScript = script;

            retHistCarriers = new List<UScriptHistoryCarrier>();
            List<UScriptHistoryCarrier> tmpKeepForRunningScript = new List<UScriptHistoryCarrier>();
            int nIndexOfBegMacro = -1;
            UDataCarrier[] prevPropagationCarrierSet = input;
            fpUDataCarrierSetResHandler prevPropagationCarrierSetHandler = inputHandler;
            UScript nextScript = null;
            double dfTotalTimeSpan = 0.0;
            double dfCurrTimeSpan = 0.0;

            // auto-reset
            ScriptExecReturnCode retCode = ScriptExecReturnCode.NA;
            //script.StopLooping = false;

            bool bGotErr = false;
            for (;;)
            {
                nextScript = null;
                //dfCurrTimeSpan = 0.0;

                //if ( script.StopLooping )
                //{
                //    retCode = ScriptExecReturnCode.OK;
                //    break;
                //}

                bool bEnterCriticalSection = false;
                UMacroCapableOfCtrlFlow lastMacroOnes = null;
                if ( bSyncCall )
                {
                    if ( !script.SynchronizedObject.WaitOne( nWaitTimeout ) )
                    {
                        retCode = ScriptExecReturnCode.SyncTimeout;
                        script.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"[UScript::RunningControlFlow] Exec {script.NameOfId} cannot sync a semaphore in {nWaitTimeout}-ms" );
                        break;
                    }
                    bEnterCriticalSection = true;
                }

                // check cancel flag and current script
                if ( script.MacroSet != null && script.MacroSet.Count > 0 && cancel != null && cancel.Flag )
                {
                    if ( !script.MacroSet[ 0 ].CancelExec )
                    {
                        foreach ( var m in script.MacroSet )
                        {
                            m.CancelExec = true;
                        }
                    }
                }

                // run the script
                //bool tmpEnableGotoFunc = script.EnableGotoFunc;
                ScriptExecReturnCode rc = script.Running( false, false, nIndexOfBegMacro, -1, prevPropagationCarrierSet, prevPropagationCarrierSetHandler, null, true, false, bResetBeforeExec );
                if ( rc != ScriptExecReturnCode.OK )
                    bGotErr = true;
                else
                {
                    lastMacroOnes = script.LastExecMacro == null ? null : script.LastExecMacro as UMacroCapableOfCtrlFlow;
                    dfCurrTimeSpan = script.TotalExecTime;
                }
                dfTotalTimeSpan = dfTotalTimeSpan + dfCurrTimeSpan;
                //script.EnableGotoFunc = tmpEnableGotoFunc;

                // process information after exec
                if ( !bGotErr )
                {
                    // only keep last calling info
                    if ( !isKeepingHistCarriers )
                    {
                        // always keep last info after executing
                        if ( retHistCarriers != null )
                        {
                            // clear previous
                            for ( int i = 0; i < retHistCarriers.Count; i++ )
                            {
                                if ( retHistCarriers[ i ] == null ) continue;
                                retHistCarriers[ i ].Dispose();
                            }
                            retHistCarriers.Clear();
                        }
                    }

                    if ( retHistCarriers != null )
                    {
                        // create an item to store
                        UScriptHistoryCarrier keeping = new UScriptHistoryCarrier( true, script.ResultCarriers,         // take away results
                                                                                   true, script.PropagationCarriers,    // take away propagation
                                                                                   true, script.DrawingResultCarriers );// take away drawing
                        keeping.Script = script;
                        // init each list
                        script.ResultCarriers = new List<UMacroProduceCarrierResult>();         // init a new one
                        script.PropagationCarriers = new List<UMacroProduceCarrierPropagation>(); // init a new one
                        script.DrawingResultCarriers = new List<UMacroProduceCarrierDrawingResult>(); // init a new one
                        // add to list
                        retHistCarriers.Add( keeping );
                    }

                }
                else
                    script?.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"RunningControlFlow() exec {script?.NameOfId} with fail code={rc} and msg={script?.StatusMessage}" );

                // prevent data change
                if ( !bGotErr && lastMacroOnes != null )
                {
                    // config for next executing
                    switch ( lastMacroOnes.Iteration )
                    {
                        case MacroJumpingFunctions.JUMPING_TO_BEG_AGAIN:
                            if ( lastMacroOnes.IteratorData == null )
                            {
                                // no data meaning beginning from macro 0 and without carrying info.
                                nIndexOfBegMacro = -1;
                                prevPropagationCarrierSet = null;
                                prevPropagationCarrierSetHandler = null;
                            }
                            else
                            {
                                // discard script name and not change current script
                                nIndexOfBegMacro = lastMacroOnes.IteratorData._nBeginIndexOfTrgMacro;
                                prevPropagationCarrierSet = lastMacroOnes.IteratorData._FeedingPrevPropagationCarrierSet;
                                prevPropagationCarrierSetHandler = lastMacroOnes.IteratorData._PrevPropagationCarrierSetHandler;
                            }
                            break;

                        case MacroJumpingFunctions.JUMPING_TO_ANOTHER_SCRIPT_KEEPING_HISTORY_CARRIERS:
                        case MacroJumpingFunctions.JUMPING_TO_ANOTHER_SCRIPT:
                            if ( lastMacroOnes.Iteration == MacroJumpingFunctions.JUMPING_TO_ANOTHER_SCRIPT_KEEPING_HISTORY_CARRIERS )
                            {
                                // keep current
                                UScriptHistoryCarrier keeping = new UScriptHistoryCarrier( true, script.ResultCarriers,
                                                                                           true, script.PropagationCarriers,
                                                                                           true, script.DrawingResultCarriers );
                                keeping.Script = script;
                                // reset to empty list
                                script.ResultCarriers = new List<UMacroProduceCarrierResult>();
                                script.PropagationCarriers = new List<UMacroProduceCarrierPropagation>();
                                script.DrawingResultCarriers = new List<UMacroProduceCarrierDrawingResult>();
                                // store
                                tmpKeepForRunningScript.Add( keeping );
                            }
                            // search a script for next executing
                            if ( lastMacroOnes.IteratorData == null )
                            {
                                script?.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"RunningControlFlow() in {lastMacroOnes.Iteration} without data" );
                                bGotErr = true; // cannot jump to next without info
                                break;
                            }
                            if ( String.IsNullOrEmpty( lastMacroOnes.IteratorData._strNameOfScript ) )
                            {
                                script?.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"RunningControlFlow() in {lastMacroOnes.Iteration} without next script name" );
                                bGotErr = true; // cannot jump without script name
                                break;
                            }
                            if ( scriptSet == null )
                            {
                                script?.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"RunningControlFlow() in {lastMacroOnes.Iteration} without script set" );
                                bGotErr = true; // cannot jump without querying list info
                                break;
                            }
                            for ( int i = 0 ; i < scriptSet.Count ; i++ )
                            {
                                if ( scriptSet[ i ] == null ) continue;
                                if ( scriptSet[ i ].NameOfId == lastMacroOnes.IteratorData._strNameOfScript )
                                {
                                    nextScript = scriptSet[ i ]; break;
                                }
                            }
                            if ( nextScript == null )
                            {
                                script?.fpLog?.Invoke( eLogMessageType.WARNING, 0, $"RunningControlFlow() in {lastMacroOnes.Iteration} not find script {lastMacroOnes.IteratorData._strNameOfScript}" );
                                bGotErr = true; // cannot jump without script instance
                                break;
                            }

                            // config the calling info
                            nIndexOfBegMacro = lastMacroOnes.IteratorData._nBeginIndexOfTrgMacro;
                            prevPropagationCarrierSet = lastMacroOnes.IteratorData._FeedingPrevPropagationCarrierSet;
                            prevPropagationCarrierSetHandler = lastMacroOnes.IteratorData._PrevPropagationCarrierSetHandler;
                            break;

                        default:
                            // not iterate
                            lastMacroOnes = null; // reset to null to trigger break
                            break;
                    }
                }


                if ( bEnterCriticalSection )
                    script.SynchronizedObject.Release();

                // error handling
                if ( bGotErr )
                {
                    // clear previous keeping results
                    for ( int i = 0 ; i < retHistCarriers.Count ; i++ )
                    {
                        if ( retHistCarriers[ i ] == null ) continue;
                        retHistCarriers[ i ].Dispose();
                    }
                    retHistCarriers.Clear();

                    // clear macro keeping results
                    for ( int i = 0 ; i < tmpKeepForRunningScript.Count ; i++ )
                    {
                        if ( tmpKeepForRunningScript[ i ] == null ) continue;
                        tmpKeepForRunningScript[ i ].Dispose();
                    }
                    tmpKeepForRunningScript.Clear();

                    retCode = ScriptExecReturnCode.MacroExecErr;

                    break;
                }

                if ( script != null && enableLog && script.fpLog != null )
                {
                    if ( lastMacroOnes == null )
                        script.fpLog( eLogMessageType.NORMAL, 0, String.Format( "[UScript::RunningControlFlow] current exec time span = {0:F2} ms.", dfCurrTimeSpan ) );
                    else if ( nextScript != null )
                        script.fpLog( eLogMessageType.NORMAL, 0, String.Format( "[UScript::RunningControlFlow] current exec time span = {0:F2} ms, next script {1}, beg index = {2}", dfCurrTimeSpan, nextScript.NameOfId, nIndexOfBegMacro ) );
                }

                // break condition: last one was not jump macro
                if ( lastMacroOnes == null )
                    break;

                // set next script instance to executing one
                if ( nextScript != null )
                {
                    script = nextScript;
                    if (cancel != null) cancel.RunningScript = script;
                }

                // check if cancel
                if ( cancel != null && cancel.Flag )
                    break;
            } // end for

            if ( !bGotErr )
            {
                // merge tmp result
                for ( int i = 0 ; i < tmpKeepForRunningScript.Count ; i++ )
                {
                    retHistCarriers.Add( tmpKeepForRunningScript[ i ] );
                }
                tmpKeepForRunningScript.Clear();
            }

            if ( script != null && enableLog && script.fpLog != null )
                script.fpLog( eLogMessageType.NORMAL, 0, String.Format( "[UScript::RunningControlFlow] total time span = {0:F2} ms", dfTotalTimeSpan ) );

            //if ( script.fpLoopingDoneCall != null )
            //    script.fpLoopingDoneCall( script, retCode );
            cancel?.CancelCall();
            if ( retCode != ScriptExecReturnCode.NA ) { } // avoid warning

            return bGotErr ? false : true;
        }
    }
}
