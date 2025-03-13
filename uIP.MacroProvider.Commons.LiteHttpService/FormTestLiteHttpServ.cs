using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.Commons.LiteHttpService
{
    public partial class FormTestLiteHttpServ : Form
    {
        internal LiteHttpServer RefInstance { get; set; } = null;
        public FormTestLiteHttpServ()
        {
            InitializeComponent();
        }

        public void InstallTest()
        {
            if ( RefInstance == null ) return;

            RefInstance.InstallPostReturnJson( out var dummy, UDataCarrier.MakeVariableItemsArray( "/test/json_01", new Func<Uri, string, List<string>>( TestJsonPost ) ) );
            RefInstance.InstallGetFile( out dummy, UDataCarrier.MakeVariableItemsArray( "/test/get_file", new Func<Uri, List<object>>( TestGetFile ) ) );
        }

        private List<string> TestJsonPost(Uri uri, string postData)
        {
            var sb = new StringBuilder();
            sb.Append( $"POST from {uri.AbsoluteUri}\n" );
            sb.Append( $" AbsPath={uri.AbsolutePath}\n" );
            sb.Append( $" PostData=\n{postData}\n" );

            LogMessage( sb.ToString() );

            return new List<string> { "200", JsonSerializer.Serialize(new { Key01="aaa", K02="bbb" } ) };
        }

        private List<object> TestGetFile(Uri uri)
        {
            var path = @"D:\project\02_test\Cap_2025-01-10 172728.png";
            var sb = new StringBuilder();
            sb.Append( $"GET from {uri.AbsoluteUri}\n" );
            sb.Append( $" AbsPath={uri.AbsolutePath}\n" );
            sb.Append( $" filename={Path.GetFileName( uri.AbsolutePath )}\n" );
            sb.Append( $" open file path={path}\n" );
            LogMessage( sb.ToString() );


            return new List<object> { 200, File.Open( path, FileMode.Open ), ( int )( new FileInfo( path ).Length ) };
        }

        void LogMessage(string s)
        {
            if (richTextBox_message.InvokeRequired)
            {
                BeginInvoke( new Action<string>( LogMessage ), s );
                return;
            }

            richTextBox_message.AppendText( $"[{CommonUtilities.GetCurrentTimeStr()}] {s}\n" );
        }

        private void FormTestLiteHttpServ_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
