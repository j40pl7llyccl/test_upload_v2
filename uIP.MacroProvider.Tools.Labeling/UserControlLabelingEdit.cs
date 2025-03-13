using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using uIP.Lib;
using uIP.Lib.MarshalWinSDK;
using uIP.Lib.Utility;
using static System.Net.Mime.MediaTypeNames;

namespace uIP.MacroProvider.Tools.Labeling
{
    public partial class UserControlLabelingEdit : UserControl
    {
        internal float[] ZoomValue { get; private set; } = new float[] { ( float )0.2, ( float )0.5, 1, 2, 3 };

        string SourceImageFilepath { get; set; } = "";
        Bitmap DisplayBmp { get; set; } = null; // loaded bitmap
        List<int> Labels { get; set; } = new List<int>(); // Store: label list
        List<string> LabelDescs { get; set; } = new List<string>(); // Store: label description
        Dictionary<int, List<object>> LabelingObjects { get; set; } = new Dictionary<int, List<object>>(); // Store: labeled regions
        List<object> ActiveLabeling { get; set; } = null; // Reference: current active regions of selected label
        public float CurrZoom { get; private set; } = 1; // Store: curren zoom factor

        URectTracker Tracker { get; set; } = null; // UI: tracker
        PolyonSelection PolySelect { get; set; } = null; // UI: polygon select

        public float ActiveLineWidth { get; set; } = 3; // UI: display line width
        public Color ActiveColor { get; set; } = Color.LimeGreen; // UI: display color
        public Color SelectedColor { get; set; } = Color.HotPink; // UI: selected region color

        internal bool DirtyOfSaving { get; set; } = false; // dirty flag
        internal bool HaveSettings { get; set; } = false; // have settings flag

        public UserControlLabelingEdit()
        {
            InitializeComponent();

            var rect = new TRectangle(
                panel_container.Width / 2 - 50,
                panel_container.Height / 2 - 50,
                panel_container.Width / 2 + 50,
                panel_container.Height / 2 + 50 );

            Tracker = new URectTracker( panel_drawing ) { Visible = false, Enabled = false, SenseSize = 7, Positions = rect };
            PolySelect = new PolyonSelection( panel_drawing ) { Visible = false, Enabled = false };

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            Tracker?.Dispose();
            PolySelect?.Dispose();


            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        /*
        public void ConfigDrawingRegion(int w, int h)
        {
            if ( w <= 0 || h <= 0 )
                return;

            if (w != panel_drawing.Width || h != panel_drawing.Height )
            {
                if (InvokeRequired)
                {
                    Invoke( new Action( () => {
                        panel_drawing.Width = w;
                        panel_drawing.Height = h;
                    } ) );
                }
            }
        }
        */

        #region Config drawing bitmap
        /// <summary>
        /// set bitmap to DisplayBmp.
        /// Config display area with 
        /// </summary>
        /// <param name="bmp"></param>
        private void SetBmp(Bitmap bmp)
        {
            if ( bmp == null )
                return;
            DisplayBmp?.Dispose();
            DisplayBmp = bmp;
            var szW = Convert.ToInt32( CurrZoom * bmp.Width );
            var szH = Convert.ToInt32(CurrZoom * bmp.Height );
            if (szW != panel_drawing.Width || szH != panel_drawing.Height)
            {
                panel_drawing.Width = szW;
                panel_drawing.Height = szH;
            }
            panel_drawing.Invalidate();
        }

        /// <summary>
        /// Load image from file and config drawing panel
        /// </summary>
        /// <param name="path">image file path</param>
        /// <returns>this object</returns>
        public UserControlLabelingEdit LoadImageFile( string path )
        {
            Bitmap bmp = null;
            try
            {
                using ( var fs = File.Open( path, FileMode.Open ) )
                {
                    bmp = new Bitmap( fs );
                }

                if ( InvokeRequired)
                {
                    Invoke( new Action<Bitmap>( SetBmp ), bmp );
                    return this;
                }

                SetBmp( bmp );
                SourceImageFilepath = path;
            }
            catch { bmp?.Dispose(); }

            return this;
        }
        #endregion

        #region Parameters GET/ SET
        /// <summary>
        /// get current parameters
        /// </summary>
        /// <returns>
        /// parameters
        /// labels: int[]
        /// label_desc: string[]
        /// zoom: float
        /// </returns>
        public Dictionary<string, UDataCarrier> GetParameters()
        {
            return new Dictionary<string, UDataCarrier>()
            {
                { "labels", UDataCarrier.MakeOne( Labels.ToArray()) },
                { "label_desc", UDataCarrier.MakeOne( LabelDescs.ToArray()) },
                { "zoom", UDataCarrier.MakeOne(trackBar_zoom.Value) }
            };
        }

        /// <summary>
        /// config parameters by UDataCarrier defined dictionay OP
        /// </summary>
        /// <param name="p">parameters</param>
        private void ConfigParametersToUI(Dictionary<string, UDataCarrier> p)
        {
            if ( p.TryGetValue("labels", out var labelsC) && p.TryGetValue( "label_desc", out var descC ) )
            {
                if ( UDataCarrier.Get(labelsC, new int[0], out var labels ) && UDataCarrier.Get(descC, new string[0], out var desc) && labels.Length == desc.Length)
                {
                    Labels = labels.ToList();
                    LabelDescs = desc.ToList();

                    comboBox_labelList.Items.Clear();
                    for(int i = 0; i < Labels.Count; i++)
                    {
                        comboBox_labelList.Items.Add( $"{Labels[i]}: {LabelDescs[i]}" );
                    }
                }
            }
            if (p.TryGetValue("zoom", out var zoomC) && UDataCarrier.Get(zoomC, 2, out var zoomIndex))
            {
                CurrZoom = ZoomValue[ zoomIndex ];
                trackBar_zoom.Value = zoomIndex;
            }

            if ( DisplayBmp != null )
                panel_drawing.Invalidate();
        }
        /// <summary>
        /// set parameters to config UI
        /// </summary>
        /// <param name="input">parameters</param>
        /// <returns>this instance</returns>
        public UserControlLabelingEdit SetParameters(Dictionary<string, UDataCarrier> input)
        {
            if ( input == null ) return this;

            if (InvokeRequired)
            {
                Invoke( new Action<Dictionary<string, UDataCarrier>>( ConfigParametersToUI ), input );
                return this;
            }

            ConfigParametersToUI( input );
            return this;
        }

        /// <summary>
        /// reload parameters from given dir and fix label_conf.xml file name
        /// </summary>
        /// <param name="dir">dir path contain label_conf.xml</param>
        /// <returns>status</returns>
        public bool ReloadSettings( string dir = "" )
        {
            if (string.IsNullOrEmpty(dir) )
            {
                if ( !File.Exists( SourceImageFilepath ) )
                    return false;
                dir = Path.GetDirectoryName( SourceImageFilepath );
            }
            var confP = Path.Combine( dir, "label_conf.xml" );
            if ( File.Exists( confP ) )
            {
                UDataCarrier[] info = null;
                string[] dummy = null;
                if ( UDataCarrier.ReadXml( confP, ref info, ref dummy ) )
                {
                    if ( UDataCarrier.DeserializeDicKeyStringValueOne( UDataCarrier.GetItem<string[]>( info, 0, null, out var d02 ), out var conf ) )
                    {
                        SetParameters( conf );
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Labeling region OP
        /// <summary>
        /// load labeled regions from file and update to UI
        /// </summary>
        /// <param name="path">path to store labeled objects</param>
        /// <returns>this instance</returns>
        public UserControlLabelingEdit ReloadLabeledObjects(string path)
        {
            if ( DisplayBmp == null )
                return this;

            LabelingObjects = LoadLabelingObjFromFile( path, DisplayBmp.Width, DisplayBmp.Height );
            //SourceImageFilepath = path;
            return this;
        }

        /// <summary>
        /// save labeled regions to file
        /// </summary>
        /// <param name="path">path to save; null or empty using same name of image file</param>
        /// <returns>status</returns>
        public bool SaveLabeledObjects(string path = "")
        {
            if ( string.IsNullOrEmpty( path ) )
                path = Path.Combine( Path.GetDirectoryName( SourceImageFilepath ), $"{Path.GetFileNameWithoutExtension( SourceImageFilepath )}.txt" );

            if ( DisplayBmp == null )
                return false;

            SaveLabelingObjToFile(path, LabelingObjects, DisplayBmp.Width, DisplayBmp.Height );
            return true;
        }

        /// <summary>
        /// reset current active regions and releate to UI
        /// </summary>
        private void ResetRegions()
        {
            comboBox_labelList.SelectedIndex = -1; // force user to pick one
            ActiveLabeling = null;
            LabelingObjects = new Dictionary<int, List<object>>();

            comboBox_regionList.Items.Clear();
        }

        /// <summary>
        /// clear all region data
        /// </summary>
        public void ClearRegionData()
        {
            if (InvokeRequired)
            {
                Invoke( new Action( ResetRegions ) );
                return;
            }
            ResetRegions();
        }

        /// <summary>
        /// load labeled region data from file
        /// - line format
        /// [label#] [rect left / w] [rect top / h] [rect width / w] [rect height / h]
        /// [label#] [pt x / w] [pt y / h] ...
        /// </summary>
        /// <param name="path">path to file</param>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        /// <returns>loaded regions</returns>
        public static Dictionary<int, List<object>> LoadLabelingObjFromFile(string path, int width, int height)
        {
            if ( !File.Exists( path ) )
                return new Dictionary<int, List<object>>();

            var ret = new Dictionary<int, List<object>>();
            using ( var fs = new StreamReader( path ) )
            {
                while( !fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    if ( string.IsNullOrEmpty(line) ||  line.Trim() == "" )
                        continue;

                    var fields = line.Split( ' ' );
                    if ( fields.Length < 5 || ( ( fields.Length - 1 ) % 2 ) != 0 )
                        continue;

                    try
                    {
                        var label = int.Parse( fields[ 0 ] );
                        if ( !ret.ContainsKey(label))
                            ret.Add(label, new List<object>());

                        var curr = ret[label];
                        if ( (fields.Length - 1 ) == 4)
                        {
                            int x = Convert.ToInt32( ( float ) width * float.Parse( fields[ 1 ] ) );
                            int y = Convert.ToInt32( ( float ) height * float.Parse( fields[ 2 ] ) );
                            int w = Convert.ToInt32( ( float ) width * float.Parse( fields[ 3 ] ) );
                            int h = Convert.ToInt32( (float ) height * float.Parse( fields[ 4 ] ) );
                            curr.Add( new Rectangle( x - w / 2, y - h / 2, w, h ) );
                        }
                        else
                        {
                            List<Point> pts = new List<Point>();
                            for( int i = 1; i < fields.Length; i+=2 )
                            {
                                int x = Convert.ToInt32( ( float ) width * float.Parse( fields[ i ] ) );
                                int y = Convert.ToInt32((float)height * float.Parse( fields[ i + 1 ] ) );
                                pts.Add( new Point( x, y ) );
                            }
                            curr.Add( pts.ToArray() );
                        }
                    } catch { continue; }
                }
            }

            return ret;
        }

        /// <summary>
        /// save labeled regions to file
        /// </summary>
        /// <param name="filepath">path to save</param>
        /// <param name="data">region data</param>
        /// <param name="w">image width</param>
        /// <param name="h">image height</param>
        public static void SaveLabelingObjToFile(string filepath, Dictionary<int, List<object>> data, int w, int h)
        {
            if ( data == null || data.Count == 0 || w <= 0 || h <= 0 )
                return;

            using ( var ws = new StreamWriter( filepath, false ) )
            {
                foreach(var kv in data)
                {
                    foreach(var v in kv.Value )
                    {
                        var sb = new StringBuilder();
                        sb.Append( $"{kv.Key}" );
                        if ( v is Rectangle r)
                        {
                            // center(x, y ), w, h
                            sb.Append( $" {( float ) (r.X + r.Width / 2) / ( float ) w} {( float ) ( r.Y + r.Height / 2) / ( float ) h} {( float ) r.Width / ( float ) w} {( float ) r.Height / ( float ) h}" );
                        }
                        else if ( v is Point[] pts )
                        {
                            foreach(var pt in pts)
                            {
                                sb.Append( $" {( float ) pt.X / ( float ) w} {( float ) pt.Y / ( float ) h}" );
                            }
                        }
                        ws.WriteLine( sb.ToString() );
                    }
                }
            }
        }
        #endregion

        #region Process key command
        /*
        protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
        {
            // UI: conntrol itself to get key down and handle want key(s)

            if ( keyData ==(Keys.Control | Keys.N)) // to add
            {
                AddLabelingRegion();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.D)) // to delete
            {
                RemoveLabelingRegion();
                return true;
            }
            return base.ProcessCmdKey( ref msg, keyData );
        }
        */

        internal void RxKey(Keys keyData )
        {
            if ( keyData == ( Keys.Control | Keys.N ) ) // to add
                AddLabelingRegion();
            else if ( keyData == ( Keys.Control | Keys.D ) ) // to delete
                RemoveLabelingRegion();
        }

        #endregion

        #region Process zoom changed
        private void trackBar_zoom_ValueChanged( object sender, EventArgs e )
        {
            // UI: zoom factor change

            TrackBar tb = sender as TrackBar;

            CurrZoom = ZoomValue[ tb.Value ];
            label_zoomV.Text = $"{CurrZoom}";

            // reset size
            if ( DisplayBmp != null)
            {
                panel_drawing.Width = Convert.ToInt32( DisplayBmp.Width * CurrZoom );
                panel_drawing.Height = Convert.ToInt32( DisplayBmp.Height * CurrZoom );
            }

            PolySelect?.ResetZoom( CurrZoom );
            panel_drawing.Invalidate();
            // set dirty
            //ConfigDirty();
        }
        #endregion

        #region drawing info in panel
        /// <summary>
        /// trigger drawing panel to repaint
        /// </summary>
        internal void InvalidDrawing()
        {
            panel_drawing.Invalidate();
        }

        /// <summary>
        /// draw object by its type(Rectangle/ Point[])
        /// </summary>
        /// <param name="g">graphics instance</param>
        /// <param name="obj">object to draw</param>
        /// <param name="c">color</param>
        /// <param name="lineW">line width</param>
        /// <param name="zoom">zoom factor of coordinate</param>
        private static void DrawObject( Graphics g, object obj, Color c, float lineW, float zoom)
        {
            using ( var p = new Pen( c, lineW ) )
            {
                if ( obj is Rectangle rect)
                {
                    if (zoom == 1)
                        g.DrawRectangle( p, rect );
                    else
                    {
                        int l = rect.Left; l = Convert.ToInt32( l * zoom );
                        int t = rect.Top; t = Convert.ToInt32( t * zoom );
                        int r = rect.Right; r = Convert.ToInt32( r * zoom );
                        int b = rect.Bottom; b = Convert.ToInt32( b * zoom );
                        g.DrawRectangle( p, new Rectangle( l, t, r - l, b - t ) );
                    }
                }
                else if ( obj is Point[] pts)
                {
                    if (zoom == 1)
                        g.DrawPolygon( p, pts );
                    else
                    {
                        var zpts = ( from pt in pts select new Point( Convert.ToInt32( pt.X * zoom ), Convert.ToInt32( pt.Y * zoom ) ) ).ToArray();
                        g.DrawPolygon( p, zpts );
                    }
                }
            }
        }

        private void panel_drawing_Paint( object sender, PaintEventArgs e )
        {
            // UI: panel drawing info

            if ( DisplayBmp != null )
                e.Graphics.DrawImage( DisplayBmp, new Rectangle( 0, 0, Convert.ToInt32( CurrZoom * DisplayBmp.Width ), Convert.ToInt32( CurrZoom * DisplayBmp.Height ) ) );

            if ( checkBox_showAll.Checked )
            {
                foreach ( var kv in LabelingObjects )
                {
                    var i = kv.Key % RegionColors.Length;
                    if ( i < 0 )
                        i += RegionColors.Length;
                    if ( i >= 0 && i < RegionColors.Length )
                    {
                        foreach ( var obj in kv.Value )
                        {
                            DrawObject( e.Graphics, obj, RegionColors[ i ], ActiveLineWidth, CurrZoom );
                        }
                    }
                }
            }
            else
            {
                var al = ActiveLabeling;
                if ( al != null )
                {
                    foreach ( var obj in al )
                        DrawObject( e.Graphics, obj, ActiveColor, ActiveLineWidth, CurrZoom );
                }

                if ( ActiveLabeling != null && comboBox_regionList.SelectedIndex >= 0 && comboBox_regionList.SelectedIndex < ActiveLabeling.Count )
                    DrawObject( e.Graphics, ActiveLabeling[ comboBox_regionList.SelectedIndex ], SelectedColor, ActiveLineWidth, CurrZoom );
            }
        }
        #endregion

        #region label info process
        private void button_labelAdd_Click( object sender, EventArgs e )
        {
            // UI: add/ replace a label
            // - label no.
            // - label description

            var labelV = Convert.ToInt32( numericUpDown_labelV.Value );
            var labelD = string.IsNullOrEmpty( textBox_lableDesc.Text ) ? "" : string.Copy( textBox_lableDesc.Text );

            if ( Labels.Contains(labelV) ) // replace
            {
                if ( MessageBox.Show( $"Existing label {labelV}, replace by current?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                    return;
                for(int i = 0; i < Labels.Count; i++ )
                {
                    if ( Labels[i] == labelV )
                    {
                        // update desc
                        LabelDescs[ i ] = labelD;
                        // update ui
                        comboBox_labelList.Items[ i ] = $"{labelV}: {labelD}";
                        // set dirty
                        ConfigDirty();
                        break;
                    }
                }
            }
            else // add
            {
                Labels.Add( labelV );
                LabelDescs.Add( labelD );
                comboBox_labelList.Items.Add( $"{labelV}: {labelD}" );
                // set dirty
                ConfigDirty();
            }
        }

        private void button_lablelDelete_Click( object sender, EventArgs e )
        {
            // UI: delete a label

            if ( comboBox_labelList.SelectedIndex < 0 )
                return;

            var s = comboBox_labelList.Items[ comboBox_labelList.SelectedIndex ].ToString();
            var ss = s.Split( ':' );
            if ( !int.TryParse( ss[ 0 ], out var index ) )
                return;

            var pos = -1;
            for(int i = 0; i < Labels.Count; i++)
            {
                if ( Labels[i] == index )
                {
                    pos = i;
                    break;
                }
            }

            if (pos >= 0)
            {
                if (MessageBox.Show($"Remove label {index}?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK ) return;

                Labels.RemoveAt( pos );
                LabelDescs.RemoveAt( pos );
                comboBox_labelList.Items.RemoveAt( pos );
                comboBox_labelList.SelectedIndex = -1;
                // set dirty
                ConfigDirty();
            }
        }

        private void button_lableReplace_Click( object sender, EventArgs e )
        {
            // UI: replace a label data

            if ( comboBox_labelList.SelectedIndex < 0 )
                return;

            var s = comboBox_labelList.Items[ comboBox_labelList.SelectedIndex ].ToString();
            var ss = s.Split( ':' );
            if ( !int.TryParse( ss[ 0 ], out var index ) )
                return;

            var pos = -1;
            for ( int i = 0; i < Labels.Count; i++ )
            {
                if ( Labels[ i ] == index )
                {
                    pos = i;
                    break;
                }
            }

            if ( pos >= 0 )
            {
                if ( MessageBox.Show( $"Replace label {index} with current info?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK ) return;
                LabelDescs[pos] = string.IsNullOrEmpty(textBox_lableDesc.Text) ? "" : string.Copy(textBox_lableDesc.Text);
                comboBox_labelList.Items[ pos ] = $"{Labels[ pos ]}: {LabelDescs[ pos ]}";
                // set dirty
                ConfigDirty();
            }
        }

        /// <summary>
        /// reset UI regions by given a set of label
        /// </summary>
        /// <param name="list"></param>
        private void ResetRegionList(List<object> list)
        {
            comboBox_regionList.Items.Clear();
            for ( int i = 0; i < list.Count; i++ )
            {
                var append = "";
                if ( list[ i ].GetType() == typeof( Rectangle ) )
                    append = "rectangle";
                else if ( list[ i ].GetType() == typeof( Point[] ) )
                    append = "polygon";

                comboBox_regionList.Items.Add( string.IsNullOrEmpty( append ) ? $"{i + 1}" : $"{i + 1}: {append}" );
            }
        }

        private void comboBox_labelList_SelectedIndexChanged( object sender, EventArgs e )
        {
            // UI: select a label
            // - reset region data in a combobox
            // - trigger repaint to update info

            if ( !( sender is ComboBox cb ) )
                return;

            if (cb.SelectedIndex < 0)
            {
                ActiveLabeling = null;
                comboBox_regionList.Items.Clear();
                return;
            }

            var s = cb.Items[ cb.SelectedIndex ].ToString();
            var ss = s.Split(':');
            if ( !int.TryParse( ss[ 0 ], out var index ) )
                return;

            if ( LabelingObjects.TryGetValue( index, out var rgns ) )
                ResetRegionList( rgns );
            else
            {
                LabelingObjects.Add( index, new List<object>() );
                comboBox_regionList.Items.Clear();
            }

            // set active labeling
            ActiveLabeling = LabelingObjects[ index ];
            // draw current
            panel_drawing.Invalidate();
        }
        #endregion

        #region Labeling region OP

        private void radioButton_rect_Click( object sender, EventArgs e )
        {
            // UI: select a way to surround an object
            Tracker.Enabled = radioButton_rect.Checked;
            Tracker.Visible = radioButton_rect.Checked;

            PolySelect.Enabled = radioButton_polygon.Checked;
            PolySelect.Visible = radioButton_polygon.Checked;
            button_resetPolygon.Enabled = radioButton_polygon.Checked;

            panel_drawing.Invalidate();
        }

        static int AdjRange(int v, int min, int max)
        {
            if ( v < min ) v = min;
            if ( v >= max ) v = max - 1;
            return v;
        }

        /// <summary>
        /// get coordinate from rectangle tracker by zoom factor
        /// </summary>
        /// <param name="ret">current coordinate</param>
        /// <param name="chk">previous rects to check same one</param>
        /// <returns>true: ready; false: same</returns>
        private bool GetTrackerCoor( out Rectangle ret, List<object> chk = null)
        {
            var trect = Tracker.Positions;
            int l = trect.left; l = Convert.ToInt32( l / CurrZoom );
            int t = trect.top; t = Convert.ToInt32( t / CurrZoom );
            int r = trect.right; r = Convert.ToInt32( r / CurrZoom );
            int b = trect.bottom; b = Convert.ToInt32( b / CurrZoom );

            if ( DisplayBmp != null)
            {
                l = AdjRange( l, 0, DisplayBmp.Width );
                r = AdjRange( r, 0, DisplayBmp.Width );
                t = AdjRange( t, 0, DisplayBmp.Height );
                b = AdjRange( b, 0, DisplayBmp.Height );
            }

            var rect = new Rectangle( l , t, r - l, b - t );
            if (chk == null)
            {
                ret = rect;
                return true;
            }

            foreach(var i in chk)
            {
                if (i is Rectangle r2c)
                {
                    if ( r2c.Equals(rect))
                    {
                        ret = new Rectangle();
                        return false;
                    }
                }
            }

            ret = rect;
            return true;
        }

        /// <summary>
        /// add a region to a specific label.
        /// ActiveLabeling must set first.
        /// </summary>
        private void AddLabelingRegion()
        {
            if ( comboBox_labelList.SelectedIndex < 0 )
            {
                MessageBox.Show( $"Please pick a label to select object!" );
                return;
            }

            if ( radioButton_rect.Checked )
            {
                if ( GetTrackerCoor( out var got, ActiveLabeling ) )
                {
                    ActiveLabeling.Add( got );
                    comboBox_regionList.Items.Add( $"{ActiveLabeling.Count}: rectangle" );
                    panel_drawing.Invalidate();
                    // set dirty
                    ConfigDirty();
                }
                return;
            }
            else if ( radioButton_polygon.Checked )
            {
                if ( !PolySelect.IsFinished )
                {
                    MessageBox.Show( $"Please pick more than 3 points and double click to finish!" );
                    return;
                }

                ActiveLabeling.Add( PolySelect.Coordinates );
                comboBox_regionList.Items.Add( $"{ActiveLabeling.Count}: polygon" );
                PolySelect.ResetPoints( true );
                // set dirty
                ConfigDirty();
                return;
            }

            MessageBox.Show( "Please select a type of selection!" );
        }

        /// <summary>
        /// remove a selected labeling region
        /// </summary>
        private void RemoveLabelingRegion()
        {
            if ( comboBox_labelList.SelectedIndex < 0 )
            {
                MessageBox.Show( $"Please pick a label to select object!" );
                return;
            }

            if (comboBox_regionList.SelectedIndex < 0)
            {
                MessageBox.Show( $"Please pick a region to remove!" );
                return;
            }


            if ( MessageBox.Show( "Remove selecting region?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                return;

            ActiveLabeling.RemoveAt( comboBox_regionList.SelectedIndex );
            comboBox_regionList.Items.RemoveAt( comboBox_regionList.SelectedIndex );
            comboBox_regionList.SelectedIndex = -1;
            // set dirty
            ConfigDirty();

            panel_drawing.Invalidate();
        }

        private void button_addRgn_Click( object sender, EventArgs e )
        {
            // UI: add a labeling region
            AddLabelingRegion();
        }

        private void button_rmvRegion_Click( object sender, EventArgs e )
        {
            // UI: remove a selected labeling region
            RemoveLabelingRegion();
        }

        private void button_resetPolygon_Click( object sender, EventArgs e )
        {
            // UI: reset polygon region to empty
            if (!PolySelect.IsEmpty && MessageBox.Show("Reset polygon points?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                PolySelect.ResetPoints( true );
            }
        }

        private void comboBox_regionList_SelectedIndexChanged( object sender, EventArgs e )
        {
            // UI region combobox: pick a region

            if ( ActiveLabeling == null )
                return;
            if ( !( sender is ComboBox cb ) || cb.SelectedIndex < 0 )
                return;

            // trigger drawing to display info
            panel_drawing.Invalidate();
        }
        #endregion

        static string BrowserDir { get; set; } = "";

        private void button_changeLabel_Click( object sender, EventArgs e )
        {
            var from = Convert.ToInt32( numericUpDown_changeLabelFrom.Value );
            var to = Convert.ToInt32( numericUpDown_changeLabelTo.Value );
            if ( from == to )
                return;

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            var pickDir = "";
            if (!string.IsNullOrEmpty(BrowserDir))
                dlg.SelectedPath = BrowserDir;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                pickDir = !string.IsNullOrEmpty( dlg.SelectedPath ) ? dlg.SelectedPath : "";
            }
            dlg.Dispose();
            if ( string.IsNullOrEmpty( pickDir ) || !Directory.Exists(pickDir ))
                return;

            BrowserDir = pickDir;
            List<string> filepaths = new List<string>();
            var txts = Directory.GetFiles( pickDir, "*.txt", SearchOption.TopDirectoryOnly );
            var csvs = Directory.GetFiles( pickDir, "*.csv", SearchOption.TopDirectoryOnly );

            if (txts != null && txts.Length > 0)
                filepaths.AddRange( txts.ToList() );
            if (csvs != null && csvs.Length > 0)
                filepaths.AddRange(csvs.ToList() );

            filepaths = ( from f in filepaths select f ).OrderBy( x => Path.GetFileName( x ) ).ToList();

            foreach(var f in filepaths)
            {
                var toSave = new List<string>();
                try
                {
                    using ( var fs = File.Open( f, FileMode.Open ) )
                    {
                        using ( var sr = new StreamReader( fs ) )
                        {
                            while ( !sr.EndOfStream )
                            {
                                var line = sr.ReadLine();
                                var fields = line.Split( ' ', ',' );
                                var toProc = new List<string>();
                                foreach(var s in fields)
                                {
                                    var ts = s ?? "";
                                    ts = ts.Trim();
                                    if ( string.IsNullOrEmpty( ts ) )
                                        continue;
                                    toProc.Add( ts );
                                }

                                if ( int.TryParse( toProc[ 0 ], out var i ) && i == from )
                                {
                                    toProc.RemoveAt( 0 );
                                    toSave.Add( $"{to} {string.Join( " ", toProc.ToArray() )}" );
                                }
                                else
                                    toSave.Add( line ); // cannot parse write it back
                            }
                        }
                    }
                }
                catch { toSave = null; }

                if (toSave != null && toSave.Count > 0)
                {
                    using ( var fs = File.Open( f, FileMode.Create, FileAccess.ReadWrite ) )
                    {
                        using ( var ws = new StreamWriter( fs ) )
                        {
                            foreach ( var s in toSave )
                                ws.WriteLine( s );
                        }
                    }
                }
            }
        }

        #region Save changed

        /// <summary>
        /// config dirty
        /// </summary>
        private void ConfigDirty()
        {
            if ( DirtyOfSaving )
                return;
            DirtyOfSaving = true;
            button_saveChange.BackColor = Color.HotPink;
        }

        /// <summary>
        /// save changed
        /// </summary>
        internal void SaveChanged()
        {
            // reset dirty info
            DirtyOfSaving = false;
            button_saveChange.BackColor = SystemColors.Control;

            if ( string.IsNullOrEmpty( SourceImageFilepath ) )
                return;

            // save settings
            var dirPath = Path.GetDirectoryName( SourceImageFilepath );
            if ( UDataCarrier.SerializeDicKeyString( GetParameters(), out var toSave ) )
            {
                UDataCarrier.WriteXml( UDataCarrier.MakeOneItemArray( toSave ), Path.Combine( dirPath, "label_conf.xml" ), null );
            }

            // save config
            var labelingPath = Path.Combine( dirPath, $"{Path.GetFileNameWithoutExtension( SourceImageFilepath )}.txt" );
            SaveLabeledObjects( labelingPath );
        }

        /// <summary>
        /// reset dirty info
        /// </summary>
        internal void ResetSaveChanged()
        {
            DirtyOfSaving = false;
            button_saveChange.BackColor = SystemColors.Control;
        }

        private void button_saveChange_Click( object sender, EventArgs e )
        {
            // UI: button click action
            SaveChanged();
        }
        #endregion

        #region Test UI, not use right now
        private void button_saveTest_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( SourceImageFilepath ) )
                return;

            var dirPath = Path.GetDirectoryName( SourceImageFilepath );
            if (UDataCarrier.SerializeDicKeyString( GetParameters(), out var toSave ))
            {
                UDataCarrier.WriteXml( UDataCarrier.MakeOneItemArray( toSave ), Path.Combine( dirPath, "label_conf.xml" ), null );
            }

            var labelingPath = Path.Combine( dirPath, $"{Path.GetFileNameWithoutExtension( SourceImageFilepath )}.txt" );
            SaveLabeledObjects( labelingPath );
        }

        private void button_testLoad_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter = "png|*.png|jpg|*.jpg|bmp|*.bmp|any|*.*" };
            var status = true;
            var imageP = "";
            if ( dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using ( var fs = File.Open( dlg.FileName, FileMode.Open ) )
                    {
                        var bmp = new Bitmap( fs );
                        if ( bmp == null ) status = false;
                        bmp?.Dispose();
                    }

                    imageP = string.Copy( dlg.FileName );
                }
                catch { status = false; }
            }
            else
                status = false;
            dlg.Dispose();

            if ( !status )
                return;

            LoadImageFile(imageP);
            var confP = Path.Combine( Path.GetDirectoryName( imageP ), "label_conf.xml" );
            if (File.Exists(confP))
            {
                UDataCarrier[] info = null;
                string[] dummy = null;
                if (UDataCarrier.ReadXml(confP, ref info, ref dummy ))
                {
                    if ( UDataCarrier.DeserializeDicKeyStringValueOne( UDataCarrier.GetItem<string[]>( info, 0, null, out var d02 ), out var conf ) )
                        SetParameters( conf );
                }
            }
            var labelingObjP = Path.Combine( Path.GetDirectoryName( imageP ), $"{Path.GetFileNameWithoutExtension( imageP )}.txt" );
            if (File.Exists(labelingObjP))
            {
                ReloadLabeledObjects( labelingObjP );
            }
        }
        #endregion

        static Color[] RegionColors = new Color[] {
            Color.Pink,
            Color.Orange,
            Color.YellowGreen,
            Color.Lime,
            Color.Aqua,
            Color.Indigo,
            Color.Purple,
            Color.BlueViolet,
            Color.Chartreuse,
            Color.Coral,
            Color.DarkOrange,
            Color.DarkTurquoise,
            Color.DeepSkyBlue,
            Color.Fuchsia,
            Color.LightBlue,
            Color.LightGreen,
            Color.LightSeaGreen,
            Color.MediumAquamarine,
            Color.MediumOrchid,
            Color.MediumPurple,
            Color.MediumSlateBlue,
            Color.MediumSpringGreen,
            Color.MediumTurquoise,
            Color.MediumVioletRed,
            Color.OrangeRed,
            Color.Pink,
            Color.Plum,
            Color.PowderBlue,
            Color.Tomato,
            Color.Yellow,
        };

        private void checkBox_showAll_Click( object sender, EventArgs e )
        {
            panel_drawing.Invalidate();

            if ( sender is CheckBox cb)
            {
                if ( !cb.Checked )
                    panel_drawing.Invalidate();
            }
        }
    }
}
