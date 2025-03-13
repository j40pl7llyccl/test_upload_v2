using System;
using System.Collections.Generic;

namespace uIP.Lib.Script
{
    /// <summary>
    /// 用來記錄之前所曾經執行過 script 的 3 種 carrier
    /// * propagation
    /// * result
    /// * drawing
    /// </summary>
    public class UScriptHistoryCarrier : IDisposable
    {
        private bool _bDisposed = false;

        private UScript _WhichScript = null;

        private bool _bHandleResult = false;
        private List<UMacroProduceCarrierResult> _ResultsCarriers = null;

        private bool _bHandlePropagation = false;
        private List<UMacroProduceCarrierPropagation> _PropagationCarriers = null;

        private bool _bHandleDrawing = false;
        private List<UMacroProduceCarrierDrawingResult> _DrawingCarriers = null;

        public UScript Script { get { return _bDisposed ? null : _WhichScript; } set { if ( _bDisposed ) return; _WhichScript = value; } }

        public List<UMacroProduceCarrierResult> ResultsCarriers
        {
            get
            {
                if ( _bDisposed ) return null;
                if ( _ResultsCarriers != null ) return _ResultsCarriers;
                return (_WhichScript == null ? null : _WhichScript.ResultCarriers);
            }
        }
        public List<UMacroProduceCarrierPropagation> PropagationCarriers
        {
            get
            {
                if ( _bDisposed ) return null;
                if ( _PropagationCarriers != null ) return _PropagationCarriers;
                return (_WhichScript == null ? null : _WhichScript.PropagationCarriers);
            }
        }
        public List<UMacroProduceCarrierDrawingResult> DrawingCarriers
        {
            get
            {
                if ( _bDisposed ) return null;
                if ( _DrawingCarriers != null ) return _DrawingCarriers;
                return (_WhichScript == null ? null : _WhichScript.DrawingResultCarriers);
            }
        }

        public UScriptHistoryCarrier( UScript whichScript, bool isStoreResult, bool isStorePropagation, bool isStoreDrawing )
        {
            _WhichScript = whichScript;
            if ( whichScript == null ) return;

            if ( isStoreResult )
            {
                _ResultsCarriers = new List<UMacroProduceCarrierResult>();
                if ( _WhichScript.ResultCarriers != null )
                {
                    List<UMacroProduceCarrierResult> src = _WhichScript.ResultCarriers;
                    for ( int i = 0 ; i < src.Count ; i++ )
                        _ResultsCarriers.Add( src[ i ] );
                    src.Clear();
                }
                _bHandleResult = true;
            }

            if ( isStorePropagation )
            {
                _PropagationCarriers = new List<UMacroProduceCarrierPropagation>();
                if ( _WhichScript.PropagationCarriers != null )
                {
                    List<UMacroProduceCarrierPropagation> src = _WhichScript.PropagationCarriers;
                    for ( int i = 0 ; i < src.Count ; i++ )
                        _PropagationCarriers.Add( src[ i ] );
                    src.Clear();
                }
                _bHandlePropagation = true;
            }

            if ( isStoreDrawing )
            {
                _DrawingCarriers = new List<UMacroProduceCarrierDrawingResult>();
                if ( _WhichScript.DrawingResultCarriers != null )
                {
                    List<UMacroProduceCarrierDrawingResult> src = _WhichScript.DrawingResultCarriers;
                    for ( int i = 0 ; i < src.Count ; i++ )
                        _DrawingCarriers.Add( src[ i ] );
                    src.Clear();
                }
                _bHandleDrawing = true;
            }
        }

        /// <summary>
        /// all list data will be handled by this class
        /// </summary>
        /// <param name="resultCarrier">input result carrier</param>
        /// <param name="propagationCarrier">input propagation carrier</param>
        /// <param name="drawingCarrier">input drawing carrier</param>
        public UScriptHistoryCarrier( bool bHandResult, List<UMacroProduceCarrierResult> resultCarrier,
                                      bool bHandPropagation, List<UMacroProduceCarrierPropagation> propagationCarrier,
                                      bool bHandleDraw, List<UMacroProduceCarrierDrawingResult> drawingCarrier )
        {
            _bHandleResult = bHandResult;
            _ResultsCarriers = resultCarrier;

            _bHandleDrawing = bHandPropagation;
            _PropagationCarriers = propagationCarrier;

            _bHandleDrawing = bHandleDraw;
            _DrawingCarriers = drawingCarrier;
        }

        public void Dispose()
        {
            if ( _bDisposed ) return;
            _bDisposed = true;

            _WhichScript = null;

            if ( _ResultsCarriers != null && _bHandleResult )
            {
                for ( int i = 0 ; i < _ResultsCarriers.Count ; i++ )
                    _ResultsCarriers[ i ].Dispose();
                _ResultsCarriers.Clear();
                _ResultsCarriers = null;
            }
            else
                _ResultsCarriers = null;

            if ( _PropagationCarriers != null && _bHandlePropagation )
            {
                for ( int i = 0 ; i < _PropagationCarriers.Count ; i++ )
                    _PropagationCarriers[ i ].Dispose();
                _PropagationCarriers.Clear();
                _PropagationCarriers = null;
            }
            else
                _PropagationCarriers = null;

            if ( _DrawingCarriers != null && _bHandleDrawing )
            {
                for ( int i = 0 ; i > _DrawingCarriers.Count ; i++ )
                    _DrawingCarriers[ i ].Dispose();
                _DrawingCarriers.Clear();
                _DrawingCarriers = null;
            }
            else
                _DrawingCarriers = null;
        }

        public static void Free( UScriptHistoryCarrier[] toFree )
        {
            if ( toFree == null ) return;
            foreach ( var i in toFree )
            {
                i.Dispose();
            }
        }

        public static void Free( List< UScriptHistoryCarrier > toFree )
        {
            Free( toFree.ToArray() );
        }
    }
}
