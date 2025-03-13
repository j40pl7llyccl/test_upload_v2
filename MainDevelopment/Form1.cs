using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Utility;
using uIP.Lib.UsrControl;
using uIP.Lib.MarshalWinSDK;
using System.Xml.Linq;

namespace MainDevelopment
{
    public partial class Form1 : Form
    {
        internal class AnyC
        {
            internal int A { get; set; } = 0;
            internal string S { get; set; } = "";
        }

        //private LogStringToFile m_FileLog = null;
        private string _strInitPath;
        public Form1()
        {
            InitializeComponent();
            _strInitPath = Directory.GetCurrentDirectory();
            ULibAgent.Singleton.InitResources( _strInitPath, this );
            ResourceManager.SystemUpCall();
        }

        public Form1(string guid)
        {
            InitializeComponent();
            _strInitPath = Directory.GetCurrentDirectory();
            ResourceManager.Reg( ResourceManager.ProgGuid, guid );

            ULibAgent.Start(_strInitPath, this);

            //ULibAgent.Singleton.InitResources( _strInitPath, this );
            //ResourceManager.SystemUpCall();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            //ResourceManager.SystemDownCall();
            //ULibAgent.Singleton.Dispose();
            ULibAgent.Close();

            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        private void btn_openFolder_Click( object sender, EventArgs e )
        {
            string fullPath = null;
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = _strInitPath;
            if( dlg.ShowDialog() == DialogResult.OK )
            {
                fullPath = Directory.Exists( dlg.SelectedPath ) ? String.Copy( dlg.SelectedPath ) : null;
            }
            dlg.Dispose(); dlg = null;
            if ( String.IsNullOrEmpty( fullPath ) )
                return;

            //m_FileLog = new LogStringToFile( 50, fullPath, 6, Path.Combine(Directory.GetCurrentDirectory(), "bak") );
        }

        private void btn_addText_Click( object sender, EventArgs e )
        {
            //if ( m_FileLog == null ) return;
            //m_FileLog.MessageLog( tbox_Input.Text );
        }

        private void button_openImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK )
            {
                if (!string.IsNullOrEmpty(dlg.FileName))
                {
                    using(var fs = File.Open(dlg.FileName, FileMode.Open))
                    {
                        var bmp = new Bitmap(fs);
                        UImageBuffer buf = new UImageBuffer();
                        if (buf.FromBitmap(bmp))
                        {
                            var convB = buf.ToBitmap();
                            pictureBox_loadImage.Image?.Dispose();
                            pictureBox_loadImage.Image = convB;
                            pictureBox_loadImage.Width = convB.Width;
                            pictureBox_loadImage.Height = convB.Height;
                        }
                        buf.Dispose();
                        bmp?.Dispose();
                    }
                }
            }
            dlg.Dispose();
        }

        private void button_btnTrim_Click(object sender, EventArgs e)
        {
            if (pictureBox_loadImage.Image == null)
                return;
            if (pictureBox_loadImage.Image is Bitmap b)
            {
                UImageBuffer buf = new UImageBuffer();
                if (buf.FromBitmap(b))
                {
                    UImageBuffer roiBuff = new UImageBuffer();
                    Rectangle r = new Rectangle();
                    if (UImageBuffer.Trim(roiBuff, buf, 
                          new Rectangle(Convert.ToInt32(numericUpDown_roiL.Value), Convert.ToInt32(numericUpDown_roiT.Value), 
                                        Convert.ToInt32(numericUpDown_roiW.Value), Convert.ToInt32(numericUpDown_roiH.Value)),
                          ref r,
                          checkBox_bufPack.Checked))
                    {
                        SaveFileDialog dlg = new SaveFileDialog() { Filter = "bmp|*.bmp|png|*.png" };
                        if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
                        {
                            var ext = Path.GetExtension(dlg.FileName).ToLower();
                            ImageFormat format = ImageFormat.Bmp;
                            if (ext == ".png") format = ImageFormat.Png;
                            roiBuff.SaveBmp(dlg.FileName, format);
                        }
                    }
                    roiBuff.Dispose();
                }
                buf.Dispose();
            }
        }

        private void button_convert24_Click(object sender, EventArgs e)
        {
            if (pictureBox_loadImage.Image == null) return;
            if (pictureBox_loadImage.Image is Bitmap b)
            {
                var sb = UImageBuffer.ConvertBitmap(b, PixelFormat.Format24bppRgb, false);
                if(sb != null)
                {
                    SaveFileDialog dlg = new SaveFileDialog() { Filter = "bmp|*.bmp|png|*.png" };
                    if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
                    {
                        var ext = Path.GetExtension(dlg.FileName).ToLower();
                        ImageFormat format = ImageFormat.Bmp;
                        if (ext == ".png") format = ImageFormat.Png;
                        sb.Save(dlg.FileName, format);
                    }
                    sb.Dispose();
                }
            }
        }

        private void button_showScriptEditor_Click( object sender, EventArgs e )
        {
            if (ResourceManager.Get(ResourceManager.ScriptEditor) is Form f)
            {
                f?.Show();
            }
        }

        private void button_encrypt_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string filepath = null;
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
            {
                filepath = string.Copy( dlg.FileName );
            }
            dlg.Dispose();
            if ( string.IsNullOrEmpty( filepath ) )
                return;

            string dstPath = Path.Combine( Path.GetDirectoryName( filepath ), $"{Path.GetFileNameWithoutExtension( filepath )}.bin" );
            FileEncryptUtility.ENC( filepath, dstPath );
        }

        private void button_decrypt_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileEncryptUtility.DEC( dlg.FileName, Path.Combine( Path.GetDirectoryName(dlg.FileName), $"{Path.GetFileNameWithoutExtension(dlg.FileName)}_{CommonUtilities.GetCurrentTimeStr("")}" ));
            }
            dlg.Dispose();
        }

        private void button_aclEditor_Click( object sender, EventArgs e )
        {
            FormAclEditor editor = new FormAclEditor();
            editor.ShowDialog();
            editor.Dispose();
        }

        private void button_editUser_Click( object sender, EventArgs e )
        {
            ULibAgent.Singleton.UserManager.OpenEditor();            
        }

        private void button_login_Click( object sender, EventArgs e )
        {
            ULibAgent.Singleton.UserManager.LoginWithUI();
        }

        private void button_test_Click( object sender, EventArgs e )
        {
            Dictionary<string, UDataCarrier> test = new Dictionary<string, UDataCarrier>()
            {
                { "a", UDataCarrier.MakeOne("string 01") },
                { "b", UDataCarrier.MakeOne(1) }
            };

            if (UDataCarrier.SerializeDicKeyString(test, out var serialized, true))
            {
                SaveFileDialog dlg = new SaveFileDialog();
                if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
                {
                    UDataCarrier.WriteXml( new UDataCarrier[] { UDataCarrier.MakeOne( serialized ) }, dlg.FileName, null );
                }
                dlg.Dispose();
            }

        }

        private void button_testRead_Click( object sender, EventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dlg.FileName))
            {
                UDataCarrier[] got = null;
                string[] dummy = null;
                var status = UDataCarrier.ReadXml( dlg.FileName, ref got, ref dummy );
                if (status)
                {
                    status = UDataCarrier.DeserializeDicKeyStringValueOne( got[ 0 ].Data as string[], out var deserialized, true );
                    Console.WriteLine( status );
                }
                Console.WriteLine(status);
            }
        }

        private void button_ControlEditor_Click( object sender, EventArgs e )
        {
            FormListControl dlg = new FormListControl();
            dlg.ShowDialog();
            dlg.Dispose();
        }

        private void button_sqliteTest_Click( object sender, EventArgs e )
        {
            ULibAgent.Singleton.SetPluginClassControlByCSharpClassName( "uIP.MacroProvider.DB.Sqlite.SqliteOp", "TestFrom", null );
        }

        private void button_cronJobTest_Click( object sender, EventArgs e )
        {
            ULibAgent.Singleton.SetPluginClassControlByCSharpClassName( "uIP.MacroProvider.Commons.CronJob.CronJobProvider", "TestForm", null );
        }

        private void button_httpServTest_Click( object sender, EventArgs e )
        {
            ULibAgent.Singleton.SetPluginClassControlByCSharpClassName( "uIP.MacroProvider.Commons.LiteHttpService.LiteHttpServer", "TestForm", null );
        }

        IntPtr hEvt = IntPtr.Zero;

        private void button_createEvt_Click( object sender, EventArgs e )
        {
            hEvt = EventWinSdkFunctions.Create( true, false, "" );
        }

        private void button_closeEvent_Click( object sender, EventArgs e )
        {
            EventWinSdkFunctions.Close( hEvt );
        }

        private void Form1_Resize( object sender, EventArgs e )
        {
            if ( WindowState == FormWindowState.Minimized )
            {
                notifyIcon1.Visible = true;
                Hide();
            }
            else
                notifyIcon1.Visible = false;
        }

        private void notifyIcon1_MouseMove( object sender, MouseEventArgs e )
        {
            //notifyIcon1.ShowBalloonTip( 2000 );
        }

        private void notifyIcon1_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Shown( object sender, EventArgs e )
        {
            if (ResourceManager.Get<Dictionary<string, string[]>>("startup", null, out var got) && got != null)
            {
                if (got.TryGetValue("auto_min", out var vv) && vv != null && vv.Length > 0 && bool.TryParse( vv[0], out var isMin) && isMin)
                    WindowState = FormWindowState.Minimized;
            }
        }

        private void Form1_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!ULibAgent.CheckAbleToEnd(true))
                {
                    e.Cancel = true;
                }
            }
        }

        static string TmpFolderInsp = @"D:\project\02_test\tmp_output\src_img_r2";
        private void button_openFolderToRun_Click( object sender, EventArgs e )
        {
            var folderpath = "";
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if ( !string.IsNullOrEmpty( TmpFolderInsp ) )
                dlg.SelectedPath = TmpFolderInsp;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                folderpath = dlg.SelectedPath;
            }
            dlg.Dispose();
            if ( string.IsNullOrEmpty( folderpath ) )
                return;

            var propagation = new UDataCarrier[] {
                UDataCarrier.MakeOne( folderpath, "input_source_from_folder" ),
                UDataCarrier.MakeOne( DateTime.Now, "input timestamp" ),
                UDataCarrier.MakeOne( "input_source_from_folder", "input_source_from" )
            };

            var execScript = "20250311-001";
            var script = ULibAgent.Singleton.Scripts.GetScript( execScript );
            if ( script == null ) return;
            var retC = script.Running( true, prevPropagation: propagation );
            richTextBox_msg.AppendText( $"Call {execScript} with result code={retC}" );
        }

        internal const string InputCarrierBuffPtrDesc = "input buffer pointer";
        internal const string InputCarrierBuffWidthDesc = "input buffer width";
        internal const string InputCarrierBuffHeightDesc = "input buffer height";
        internal const string InputCarrierBuffPixelBitsDesc = "input buffer pixel bits";
        internal const string InputCarrirBuffStrideDesc = "input buffer stride";
        internal const string InputCarrierTimestamp = "input timestamp";

        private void button_openImageToRun_Click( object sender, EventArgs e )
        {
            var filepath = "";
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                filepath = dlg.FileName;
            }
            dlg.Dispose();
            if ( string.IsNullOrEmpty( filepath ) ) return;

            UImageBuffer buff = new UImageBuffer();
            if (!buff.LoadBmp( filepath ))
            {
                buff.Dispose();
                return;
            }

            var propagation = new UDataCarrier[]
            {
                    UDataCarrier.MakeOne(buff.Buffer, InputCarrierBuffPtrDesc),
                    UDataCarrier.MakeOne(buff.Width, InputCarrierBuffWidthDesc),
                    UDataCarrier.MakeOne(buff.Height, InputCarrierBuffHeightDesc),
                    UDataCarrier.MakeOne(buff.Bits, InputCarrierBuffPixelBitsDesc),
                    UDataCarrier.MakeOne(buff.Stride, InputCarrirBuffStrideDesc),
                    UDataCarrier.MakeOne(DateTime.Now, InputCarrierTimestamp),
                    UDataCarrier.MakeOne("input_source_from_buffer", "input_source_from")
            };

            var execScript = "20250311-002";
            var script = ULibAgent.Singleton.Scripts.GetScript( execScript );
            if ( script == null ) return;
            var retC = script.Running( true, prevPropagation: propagation, bResetBeforeExec: false );
            richTextBox_msg.AppendText( $"Call {execScript} with result code={retC}" );

            buff.Dispose();
        }
    }
}
