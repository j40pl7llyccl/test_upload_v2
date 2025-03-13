using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Detection
{
    public partial class UserControlSopDetectParams : UserControl
    {
        internal enum AvailableTrgCond : int
        {
            deck_ex = 0,
            deck_mis,
            pen_ex,
            pen_mis,
            pen_on_deck,
            pen_on_wafer
        }
        internal enum TypeCond : int
        {
            Normal = 0,
            Warning,
            Error
        }
        private Dictionary<UDataCarrier, List<UDataCarrier>> TmpRegions { get; set; }
        private List<TriggerCondition> TmpTrgConds { get; set; }
        internal DetectionParameters Settings { get; private set; } = null;
        internal string ReferenceImageFilepath { get; set; }
        internal Bitmap ReferenceBackground { get; private set; }
        internal string DataRWDir { get; set; }
        public UserControlSopDetectParams()
        {
            InitializeComponent();

            comboBox_judgeObjRgn.Items.Add( $"0: touch work area" );
            comboBox_judgeObjRgn.Items.Add( $"1: obj center inside work area" );
            comboBox_judgeObjRgn.Items.Add( $"2: all obj inside work area" );

            comboBox_condAvailable.Items.Add( $"0: more than 1 deck" );
            comboBox_condAvailable.Items.Add( $"1: miss deck" );
            comboBox_condAvailable.Items.Add( $"2: more than 1 pen" );
            comboBox_condAvailable.Items.Add( $"3: pen miss" );
            comboBox_condAvailable.Items.Add( $"4: pen on deck" );
            comboBox_condAvailable.Items.Add( $"5: pen on wafer" );

            var ct = Enum.GetNames( typeof( TypeCond ) );
            foreach(var c in ct )
                comboBox_condType.Items.Add( c );
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            ReferenceBackground?.Dispose();
            ReferenceBackground = null;

            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        internal void ReloadSettings(DetectionParameters settings)
        {
            if ( settings == null )
                return;

            Settings = settings;

            comboBox_judgeObjRgn.SelectedIndex = settings.JudgeObjRegionWay;
            comboBox_whichWR.Items.Clear();
            foreach(var r in Settings.WorkRegions)
            {
                if ( r is Rectangle rect )
                    comboBox_whichWR.Items.Add( $"({rect.Left}, {rect.Top}, {rect.Right}, {rect.Bottom})" );
            }
            if ( comboBox_whichWR.Items.Count > 0 )
                comboBox_whichWR.SelectedIndex = 0;

            TmpRegions = ToConfRegions( Settings.WorkRegions, Settings.IgnoreRegionsInsideWorkRegions );
        }

        internal void UpdateSettings()
        {
            if ( Settings == null ) return;
            Settings.JudgeObjRegionWay = comboBox_judgeObjRgn.SelectedIndex;
            /*
            Settings.WaferMinArea = Convert.ToInt32( numericUpDown_waferMinArea.Value );
            Settings.Wafer2PenMaxDist = Convert.ToInt32( numericUpDown_waferPenMaxAcceptDist );
            Settings.TriggerConditions = TmpTrgConds;
            if ( TmpRegions == null || TmpRegions.Count == 0 )
            {
                Settings.WorkRegions = new List<object>();
                Settings.IgnoreRegionsInsideWorkRegions = new List<object[]>();
            }
            else
            {
                List<object> workR = new List<object>();
                List<object[]> ignoreR = new List<object[]>();
                foreach ( var kv in TmpRegions )
                {
                    var ignore = new object[ 0 ];
                    if (kv.Value != null && kv.Value.Count > 0)
                        ignore = ( from i in kv.Value select i.GetDicKeyStrOne() ).ToArray();

                    workR.Add( kv.Key.GetDicKeyStrOne() );
                    ignoreR.Add( ignore );
                }
                Settings.WorkRegions = workR;
                Settings.IgnoreRegionsInsideWorkRegions = ignoreR;
            }
            */
        }

        private void ResetWorkingRegionParam(int actIndex = 0)
        {
            // default
            numericUpDown_waferMinArea.Value = Convert.ToDecimal( 300 );
            numericUpDown_waferPenMaxAcceptDist.Value = Convert.ToDecimal( 200 );
            numericUpDown_waferPenIou.Value = Convert.ToDecimal( 11 );

            comboBox_condList.Items.Clear();
            numericUpDown_condTimeInterval.Value = Convert.ToDecimal( 0 );
            numericUpDown_condCountThreshold.Value = Convert.ToDecimal( 0 );
            comboBox_condType.SelectedIndex = 0;
            textBox_condGivenName.Text = "";

            comboBox_whichWR.Items.Clear();
            foreach(var rgn in Settings.WorkRegions)
            {
                var r = ( Rectangle )rgn;
                comboBox_whichWR.Items.Add( $"({r.X}, {r.Y}, {r.Width}, {r.Height})" );
            }

            if ( actIndex >= 0 && actIndex < Settings.WorkRegions.Count )
                comboBox_whichWR.SelectedIndex = actIndex;
        }

        private void ResetConditions( int whichWRIndex, int activeIndex = 0)
        {
            comboBox_condList.Items.Clear();
            numericUpDown_condTimeInterval.Value = Convert.ToDecimal( 0 );
            numericUpDown_condCountThreshold.Value = Convert.ToDecimal( 0 );
            comboBox_condType.SelectedIndex = 0;
            textBox_condGivenName.Text = "";

            if ( Settings == null || Settings.WorkRegionParams == null || whichWRIndex < 0 || whichWRIndex >= Settings.WorkRegionParams.Count )
                return;

            for ( int i = 0; i < Settings.WorkRegionParams[whichWRIndex].TriggerConditions.Count; i++ )
                comboBox_condList.Items.Add( $"{i + 1}" );

            if ( activeIndex >= 0 && activeIndex < comboBox_condList.Items.Count )
                comboBox_condList.SelectedIndex = activeIndex;
        }

        internal static Dictionary<UDataCarrier, List<UDataCarrier>> ToConfRegions( List<object> workRgn, List<object[]> ignoreRgnInsideWR )
        {
            if ( workRgn == null || workRgn.Count == 0 || ignoreRgnInsideWR == null || ignoreRgnInsideWR.Count != workRgn.Count )
                return new Dictionary<UDataCarrier, List<UDataCarrier>>();

            try
            {
                var keys = ( from i in workRgn select UDataCarrier.MakeOne( i ) ).ToList();
                var values = ( from i in ignoreRgnInsideWR select ( from ii in i select UDataCarrier.MakeOne( ii ) ).ToList() ).ToList();
                var ret = new Dictionary<UDataCarrier, List<UDataCarrier>>();
                for(int i = 0; i < keys.Count; i++)
                {
                    ret.Add( keys[i], values[i] );
                }
                return ret;
            }
            catch
            {
                return new Dictionary<UDataCarrier, List<UDataCarrier>>();
            }
        }

        private static UDataCarrier FromSpecific(UDataCarrier src)
        {
            if (UDataCarrier.Get<Rectangle>(src, new Rectangle(), out var rect))
            {
                return UDataCarrier.Make( new Rectangle( rect.X, rect.Y, rect.Width, rect.Height ) );
            }
            else if ( UDataCarrier.Get<Point[]>(src, null, out var pts))
            {
                return UDataCarrier.Make( ( from pt in pts select new Point( pt.X, pt.Y ) ).ToArray() );
            }
            return null;
        }

        private static Dictionary<UDataCarrier, List<UDataCarrier>> CloneRegions( Dictionary<UDataCarrier, List<UDataCarrier>> src)
        {
            var ret = new Dictionary<UDataCarrier, List<UDataCarrier>>();
            if ( src == null || src.Count == 0 ) return ret;
            foreach(var kv in src)
            {
                UDataCarrier nk = FromSpecific(kv.Key);
                if ( nk == null )
                    continue;
                var nv = ( from i in kv.Value select FromSpecific( i ) ).ToList();
                ret.Add( nk, nv );
            }
            return ret;
        }

        private void button_openRgnConfig_Click( object sender, EventArgs e )
        {
            if (File.Exists(ReferenceImageFilepath) && ReferenceBackground == null)
            {
                using ( var s = File.Open( ReferenceImageFilepath, FileMode.Open ) )
                {
                    ReferenceBackground = new Bitmap( s );
                }
            }

            FormWithOkCancel dlg = new FormWithOkCancel();
            dlg.Text = "Config regions";
            dlg.AddControl( new UsrCtrlRegionEditor( CloneRegions(TmpRegions), ReferenceBackground ) );
            if ( dlg.ShowDialog() == DialogResult.OK)
            {
                UsrCtrlRegionEditor re = dlg.AddedControl as UsrCtrlRegionEditor;
                TmpRegions = re.Regions;
                if ( re.BackgroundBmp != null )
                {
                    ReferenceBackground?.Dispose();
                    ReferenceBackground = new Bitmap( re.BackgroundBmp );
                }

                // using TmpRegions to check
                // check existing
                List<object> wrgnRmv = new List<object>();
                List<object[]> irgnRmv = new List<object[]>();
                List<WorkingRegionParameters> wrgnParamRmv = new List<WorkingRegionParameters>();
                for( int i = 0; i < Settings.WorkRegions.Count; i++ )
                {
                    var wr = Settings.WorkRegions[i];
                    var r = ( Rectangle )wr;
                    bool exist = false;
                    foreach(var kv in TmpRegions)
                    {
                        UDataCarrier.Get( kv.Key, new Rectangle(), out var wrgn );
                        if (r.Equals(wrgn))
                        {
                            exist = true; break;
                        }
                    }
                    // not existing
                    if ( !exist )
                    {
                        wrgnRmv.Add( wr );
                        irgnRmv.Add( Settings.IgnoreRegionsInsideWorkRegions[ i ] );
                        wrgnParamRmv.Add( Settings.WorkRegionParams[ i ] );
                    }
                }
                // remove
                foreach ( var i in wrgnRmv ) Settings.WorkRegions.Remove( i );
                foreach ( var i in irgnRmv ) Settings.IgnoreRegionsInsideWorkRegions.Remove( i );
                foreach ( var i in wrgnParamRmv ) Settings.WorkRegionParams.Remove( i );

                // check new
                List<object> newoneW = new List<object>();
                List<object[]> newoneI = new List<object[]>();
                foreach ( var kv in TmpRegions )
                {
                    // only rectange
                    if ( !UDataCarrier.Get( kv.Key, new Rectangle(), out var wr ) )
                        continue;

                    bool same = false;
                    foreach ( var i in Settings.WorkRegions )
                    {
                        Rectangle r = ( Rectangle )i;
                        if ( r.Equals( wr ) )
                        {
                            same = true; break;
                        }
                    }

                    if ( !same )
                    {
                        newoneW.Add( wr );
                        if ( kv.Value == null )
                            newoneI.Add( new object[ 0 ] );
                        else
                        {
                            List<object> tmp = new List<object>();
                            foreach(var i in kv.Value)
                            {
                                if ( i.Tp == typeof( Rectangle ) )
                                    tmp.Add( UDataCarrier.Get( i, new Rectangle() ) );
                                else if ( i.Tp == typeof( Point[] ) )
                                    tmp.Add( UDataCarrier.Get( i, new Point[ 0 ] ) );
                            }
                            newoneI.Add( tmp.ToArray() );
                        }
                    }
                }
                if ( newoneW.Count > 0 )
                {
                    Settings.WorkRegions.AddRange( newoneW );
                    Settings.IgnoreRegionsInsideWorkRegions.AddRange( newoneI );
                    for ( int i = 0; i < newoneW.Count; i++ )
                        Settings.WorkRegionParams.Add( new WorkingRegionParameters() );
                }

                // OK, now reset parameter settings
                ResetWorkingRegionParam();
            }
        }

        private void comboBox_whichWR_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !( sender is ComboBox cb ) || cb == null )
                return;

            if ( cb.SelectedIndex < 0 ) return;
            int index = cb.SelectedIndex;
            var selP = Settings.WorkRegionParams[ index ];
            numericUpDown_waferMinArea.Value = Convert.ToDecimal( selP.WaferMinArea );
            numericUpDown_waferPenMaxAcceptDist.Value = Convert.ToDecimal( selP.Wafer2PenMaxDist );
            numericUpDown_waferPenIou.Value = Convert.ToDecimal( selP.WaferPenIouPercentageThreshold );

            comboBox_condAvailable.SelectedIndex = 0;
            numericUpDown_condTimeInterval.Value = Convert.ToDecimal( 0 );
            numericUpDown_condCountThreshold.Value = Convert.ToDecimal( 0 );
            comboBox_condType.SelectedIndex = 0;
            textBox_condGivenName.Text = "";

            comboBox_condList.Items.Clear();
            if (selP.TriggerConditions != null)
            {
                for ( int i = 0; i < selP.TriggerConditions.Count; i++ )
                    comboBox_condList.Items.Add( $"{i + 1}" );
            }
        }

        private void button_removeCond_Click( object sender, EventArgs e )
        {
            if ( Settings == null || Settings.WorkRegionParams == null || 
                comboBox_whichWR.SelectedIndex < 0 || comboBox_whichWR.SelectedIndex >= Settings.WorkRegionParams.Count ||
                Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ] == null ||
                comboBox_condList.SelectedIndex < 0 || 
                comboBox_condList.SelectedIndex >= Settings.WorkRegionParams[comboBox_whichWR.SelectedIndex].TriggerConditions.Count )
                return;
            if ( MessageBox.Show( $"Sure to remove {comboBox_condList.SelectedIndex + 1}?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question )
                 != DialogResult.OK )
                return;

            Settings.WorkRegionParams[comboBox_whichWR.SelectedIndex].TriggerConditions.RemoveAt( comboBox_condList.SelectedIndex );
            ResetConditions( comboBox_whichWR.SelectedIndex );
        }

        private void button_condAdd_Click( object sender, EventArgs e )
        {
            if ( Settings == null || Settings.WorkRegionParams == null ||
                comboBox_whichWR.SelectedIndex < 0 || comboBox_whichWR.SelectedIndex >= Settings.WorkRegionParams.Count )
                return;
            if ( MessageBox.Show( "Sure to add new?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                return;

            Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].TriggerConditions.Add(
                new TriggerCondition()
                {
                    TriggerType = Enum.GetNames( typeof( AvailableTrgCond ))[ comboBox_condAvailable.SelectedIndex < 0 ? 0 : comboBox_condAvailable.SelectedIndex],
                    TimeIntervalSec = Convert.ToDouble( numericUpDown_condTimeInterval.Value ),
                    EventCountThreshold = Convert.ToInt32( numericUpDown_condCountThreshold.Value ),
                    MessageType = comboBox_condType.SelectedIndex < 0 ? "NA" : comboBox_condType.Items[ comboBox_condType.SelectedIndex ].ToString(),
                    Description = textBox_condGivenName.Text
                } );
            ResetConditions( comboBox_whichWR.SelectedIndex, Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].TriggerConditions.Count - 1 );
        }

        private void button_condReplace_Click( object sender, EventArgs e )
        {
            if ( Settings == null || Settings.WorkRegionParams == null ||
                comboBox_whichWR.SelectedIndex < 0 || comboBox_whichWR.SelectedIndex >= Settings.WorkRegionParams.Count ||
                comboBox_condList.SelectedIndex < 0 || comboBox_condList.SelectedIndex >= Settings.WorkRegionParams[comboBox_whichWR.SelectedIndex].TriggerConditions.Count )
                return;
            if ( MessageBox.Show( $"Sure to replace data in {comboBox_condList.SelectedIndex + 1}?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                return;

            var item = Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].TriggerConditions[ comboBox_condList.SelectedIndex ];
            item.TriggerType = Enum.GetNames( typeof( AvailableTrgCond ) )[ comboBox_condAvailable.SelectedIndex < 0 ? 0 : comboBox_condAvailable.SelectedIndex ];;
            item.TimeIntervalSec = Convert.ToDouble( numericUpDown_condTimeInterval.Value );
            item.EventCountThreshold = Convert.ToInt32( numericUpDown_condCountThreshold.Value );
            item.MessageType = comboBox_condType.SelectedIndex < 0 ? "NA" : comboBox_condType.Items[ comboBox_condType.SelectedIndex ].ToString();
            item.Description = textBox_condGivenName.Text;
        }

        private void button_apply_Click( object sender, EventArgs e )
        {
            if ( Settings == null || Settings.WorkRegions == null || 
                 comboBox_whichWR.SelectedIndex < 0 || comboBox_whichWR.SelectedIndex >= Settings.WorkRegions.Count )
                return;
            Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].WaferMinArea = Convert.ToInt32( numericUpDown_waferMinArea.Value );
            Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].Wafer2PenMaxDist = Convert.ToInt32( numericUpDown_waferPenMaxAcceptDist.Value );
            Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].WaferPenIouPercentageThreshold = Convert.ToInt32( numericUpDown_waferPenIou.Value );
        }

        private void comboBox_condList_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !( sender is ComboBox cb ) || cb == null )
                return;

            if ( cb.SelectedIndex < 0 ) return;
            if ( comboBox_whichWR.SelectedIndex < 0 ) return;

            var conf = Settings.WorkRegionParams[ comboBox_whichWR.SelectedIndex ].TriggerConditions[ cb.SelectedIndex ];
            if ( Enum.TryParse<AvailableTrgCond>( conf.TriggerType, out var tt ) )
                comboBox_condAvailable.SelectedIndex = ( int )tt;
            else
                comboBox_condAvailable.SelectedIndex = -1;
            numericUpDown_condTimeInterval.Value = Convert.ToDecimal( conf.TimeIntervalSec );
            numericUpDown_condCountThreshold.Value = Convert.ToDecimal( conf.EventCountThreshold );
            if ( Enum.TryParse<TypeCond>( conf.MessageType, out var mt ) )
                comboBox_condType.SelectedIndex = ( int )mt;
            else
                comboBox_condList.SelectedIndex = -1;
            textBox_condGivenName.Text = conf.Description;
        }
    }
}
