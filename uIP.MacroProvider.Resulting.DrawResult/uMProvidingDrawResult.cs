using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Resulting.DrawResult
{
    public class uMProvidingDrawResult : UMacroMethodProviderPlugin
    {
        const string Drawing2FormMethodName = "Drawing2Form";

        public uMProvidingDrawResult() : base()
        {
            m_strInternalGivenName = "Drawing2Form Result";
        }

        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            m_UserQueryOpenedMethods.Add(
                new UMacro( null, m_strCSharpDefClassName, Drawing2FormMethodName, Drawing2Form,
                            null, null,
                            null, null)
            );

            /*
            m_MacroControls.Add( "FormTitle", new UScriptControlCarrierMacro( "FormTitle", true, true, true,
                    new UDataCarrierTypeDescription[]
                    {
                        new UDataCarrierTypeDescription(typeof(string), "Title name")
                    },
                    IoctrlGet_FormTitle, IoctrlSet_FormTitle
                )
            );
            */
            m_createMacroDoneFromMethod.Add( Drawing2FormMethodName, MacroShellDoneCall_All );
            m_macroMethodConfigPopup.Add( Drawing2FormMethodName, PopupMacroConfigDialog_All );
            // parameters file GET/SET
            m_macroMethodSettingsGet.Add( Drawing2FormMethodName, GetMacroMethodSettings );
            m_macroMethodSettingsSet.Add( Drawing2FormMethodName, SetMacroMethodSettings );


            m_bOpened = true;
            return true;
        }

        private bool MacroShellDoneCall_All( string callMethodName, UMacro instance )
        {
            // after create an instance of Macro, call to create Mutable init data
            if ( callMethodName == Drawing2FormMethodName )
            {
                ResourceManager.InvokeMainThread( new Action( () =>
                {
                    var form = new FormDisplay() { CoorScale = 0.5 };
                    form.Location = new Point( Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height );
                    form.Show();

                    var formC = UDataCarrier.MakeOne( form, true );
                    formC.fpDataHandler = ResourceManager.InvokeMainThreadFreeResource;

                    instance.MutableInitialData = UDataCarrier.MakeOne( new Dictionary<string, UDataCarrier>()
                    {
                        { MutableDataKey.Form.ToString(), formC },
                        { MutableDataKey.Param_EnableDraw.ToString(), UDataCarrier.MakeOne(true) },
                        { MutableDataKey.Param_ShowResult.ToString(), UDataCarrier.MakeOne(true) }
                    }, true );
                } ) );
            }
            return true;
        }

        private Form PopupMacroConfigDialog_All( string callMethodName, UMacro macroToConf )
        {
            if (callMethodName == Drawing2FormMethodName)
            {
                return new FormEditDisplayFormTitle() { WorkWith = macroToConf }.UpdateToUI();
            }
            return null;
        }

        bool GetMacroMethodSettings( UMacro m, out object settings, out Type t )
        {
            settings = null;
            t = null;
            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null, out var dic ) )
                return true;

            var keep = (from kv in dic where kv.Key.IndexOf("Param_") == 0 select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            if ( keep != null && keep.Count > 0 )
            {
                if ( UDataCarrier.SerializeDicKeyString( keep, out var conv ) && conv != null )
                {
                    settings = conv;
                    t = conv.GetType();
                }
            }
            return true;
        }
        bool SetMacroMethodSettings( UMacro m, object settings )
        {
            if ( !UDataCarrier.DeserializeDicKeyStringValueOne( settings as string[], out var param ) )
                return false;

            if ( !UDataCarrier.Get<Dictionary<string, UDataCarrier>>( m.MutableInitialData, null, out var dic ) )
                return false;

            foreach(var kv in param)
            {
                UDataCarrier.Set( dic, kv.Key, kv.Value );
            }

            // reset title
            ResourceManager.InvokeMainThread( () =>
            {
                if ( UDataCarrier.Get<Form>( dic, MutableDataKey.Form.ToString(), null, out var frm ) )
                {
                    frm.Text = UDataCarrier.Get( dic, MutableDataKey.Param_FormTitle.ToString(), "Display" );
                    if ( !UDataCarrier.GetDicKeyStrOne( m.MutableInitialData, MutableDataKey.Param_ShowResult.ToString(), false ) )
                        frm.Hide();
                }

            } );

            return true;
        }


        /*
        private UDataCarrier[] IoctrlGet_FormTitle( UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus )
        {
            bRetStatus = false;
            if ( !UDataCarrier.Get<Form>( whichMacro?.MutableInitialData ?? null, null, out var form ) )
                return null;

            bRetStatus = true;
            return UDataCarrier.MakeOneItemArray( string.IsNullOrEmpty( form.Text ) ? "" : string.Copy( form.Text ) );
        }
        private bool IoctrlSet_FormTitle( UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data )
        {
            if ( !UDataCarrier.Get<Form>( whichMacro?.MutableInitialData ?? null, null, out var form ) )
                return false;
            if ( !UDataCarrier.GetByIndex( data, 0, "", out var title ) )
                return false;

            form.Invoke( new System.Action( () => form.Text = title ) );
            return true;
        }
        */

        private UDataCarrier[] Drawing2Form( UMacro MacroInstance,
                                             UDataCarrier[] PrevPropagationCarrier,
                                             List<UMacroProduceCarrierResult> historyResultCarriers,
                                             List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                             List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                             List<UScriptHistoryCarrier> historyCarrier,
                                         ref bool bStatusCode, ref string strStatusMessage,
                                         ref UDataCarrier[] CurrPropagationCarrier,
                                         ref UDrawingCarriers CurrDrawingCarriers,
                                         ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                         ref fpUDataCarrierSetResHandler ResultCarrierHandler )
        {
            UDataCarrier.GetDicKeyStrOne( MacroInstance.MutableInitialData, MutableDataKey.Param_EnableDraw.ToString(), true, out var enableDrawing );
            if (!enableDrawing)
            {
                bStatusCode = true;
                return null;
            }

            if (UDataCarrier.Get<Dictionary<string, UDataCarrier>>(MacroInstance.MutableInitialData, null, out var dic))
            {
                var formVisible = UDataCarrier.Get( dic, MutableDataKey.Param_ShowResult.ToString(), true );
                var frm = UDataCarrier.Get<FormDisplay>( dic, MutableDataKey.Form.ToString(), null );
                if ( frm != null)
                {
                    if ( frm.Visible != formVisible )
                    {
                        if ( formVisible )
                        {
                            if ( frm.WindowState != FormWindowState.Normal )
                            {
                                if ( frm.InvokeRequired )
                                    frm.Invoke( new Action( () => frm.WindowState = FormWindowState.Normal ) );
                                else
                                    frm.WindowState = FormWindowState.Normal;
                            }
                            frm.Show();
                        }
                        else frm.Hide();
                    }
                    if (formVisible && !frm.EverAdj && ( frm.Location.X >= Screen.PrimaryScreen.WorkingArea.Width || frm.Location.Y >= Screen.PrimaryScreen.WorkingArea.Height ) )
                    {
                        int x = Screen.PrimaryScreen.WorkingArea.Width / 2 - frm.Width / 2;
                        int y = Screen.PrimaryScreen.WorkingArea.Height / 2 - frm.Height / 2;
                        if ( frm.InvokeRequired )
                            frm.Invoke( new Action<Point>( pt => frm.Location = pt ), new Point( x, y ) );
                        else
                            frm.Location = new Point( x, y );
                        frm.EverAdj = true;
                    }
                }

                var drawRs = MacroInstance.OwnerOfScript.DrawingResultCarriers ?? null;
                if ( drawRs != null )
                {
                    int resW = 0, resH = 0;
                    UMacroProduceCarrierDrawingResult which = null;
                    foreach ( var c in drawRs )
                    {
                        if ( c == null ) continue;
                        if ( c.BackgroundW > 0 && c.BackgroundH > 0 )
                        {
                            resW = c.BackgroundW;
                            resH = c.BackgroundH;
                            which = c;
                            break;
                        }
                    }
                    // got image resolution
                    if ( resW > 0 && resH > 0 )
                    {
                        double zoom = formVisible ? frm.CoorScale : 1.0;
                        int iw = Convert.ToInt32( zoom * resW );
                        int ih = Convert.ToInt32( zoom * resH );
                        Bitmap b = null;
                        bool canHandle = false;//new Bitmap( iw, ih, System.Drawing.Imaging.PixelFormat.Format24bppRgb );

                        if ( formVisible )
                        {
                            b = new Bitmap( iw, ih, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
                            canHandle = true;
                        }
                        else
                        {
                            var got = UDataCarrier.Get<Bitmap>( dic, MutableDataKey.Bitmap.ToString(), null );
                            if ( got == null || got.Width != iw || got.Height != ih )
                            {
                                b = new Bitmap( iw, ih, System.Drawing.Imaging.PixelFormat.Format24bppRgb );
                                UDataCarrier.Set( dic, MutableDataKey.Bitmap.ToString(), b, true );
                            }
                            else
                            {
                                b = got;
                            }
                        }

                        try
                        {
                            foreach ( var drw in drawRs )
                            {
                                if ( drw == null ) continue;
                                UDrawingCarriers.Drawing( b, drw.Data, zoom, zoom );
                            }
                        }
                        catch { if(canHandle) b?.Dispose(); b = null; }

                        if ( formVisible )
                            if ( b != null ) frm.DrawBitmap( b );

                        bStatusCode = true;
                        return new UDataCarrier[] { UDataCarrier.MakeOne( b, "drawing result" ) };
                    }
                }
            }

            bStatusCode = true;
            return null;
        }

    }
}
