using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.Utility;

using System.Data.SQLite;
using Dapper;
using uIP.Lib.MarshalWinSDK;
using System.Dynamic;
using uIP.Lib;

namespace uIP.MacroProvider.DB.Sqlite
{
    public partial class FormTestDB : Form
    {
        /*
         * 1. Create Table: Fill query edit
CREATE TABLE if not exists test_tb_01 ( ID INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT,
  Age INTEGER,
  CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
         * 2. Add row: Fill query edit
vic, 50
alice, 32
joe, 7
         * 3. Query: fill query edit
SELECT * FROM test_tb_01;


ID
Name
Age
CreatedAt
         * 4. Drop table: fill query edit table name such as test_tb_01
         * 5. Truncate table
         *    - fill query edit table name such as test_tb_01
         *    - if there is no AUTOINCREMENT just call TruncateTable with table name otherwise giving 2nd arg with true to reset sequence
         */
        internal SqliteOp RefDBOp { get; set; } = null;
        public FormTestDB()
        {
            InitializeComponent();
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

        private void FormTestDB_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void button_exec_Click( object sender, EventArgs e )
        {
            if (RefDBOp == null)
            {
                LogMessage( "no instance to operate..." );
                return;
            }
            if (RefDBOp.Connection == null)
            {
                LogMessage( "DB connection not ready yet..." );
                return;
            }

            var qs = richTextBox_query.Text;
            var ret = RefDBOp.Exec( out var dummy, UDataCarrier.MakeOne( qs ) );
            LogMessage( $"Exec\n-----\n{qs}\n-----\nReturn={ret}\n" );

            /*
            using(var conn = RefDBOp.Connection.Clone() as SQLiteConnection)
            {
                var qs = richTextBox_query.Text;
                try
                {
                    var ret = conn.Execute( qs );
                    LogMessage( $"Exec\n-----\n{qs}\n-----\nReturn={ret}\n" );
                }
                catch(Exception ex)
                {
                    LogMessage( ex.ToString() );
                }
            }
            */
        }

        private void button_query_Click( object sender, EventArgs e )
        {
            if ( RefDBOp == null )
            {
                LogMessage( "no instance to operate..." );
                return;
            }
            if ( RefDBOp.Connection == null )
            {
                LogMessage( "DB connection not ready yet..." );
                return;
            }

            var qs = new List<string>();
            var prop = new List<string>();
            List<string> w = qs;
            var lines = richTextBox_query.Lines;
            var contEmpty = 0;
            foreach(var line in lines)
            {
                var l = line.Trim();
                if ( string.IsNullOrEmpty(l))
                {
                    if ( ++contEmpty >= 2 )
                        w = prop;
                    continue;
                }
                contEmpty = 0;
                w.Add( l );
            }

            List<UDataCarrier> pp = new List<UDataCarrier>()
            {
                UDataCarrier.MakeOne( string.Join( ";", qs.ToArray() ) ),
                null
            };
            foreach ( var p in prop ) pp.Add( UDataCarrier.MakeOne( p ) );


            if (RefDBOp.Query(out var backR, pp.ToArray() ))
            {
                if (backR?.Data is Dictionary<string, List<object>> rev)
                {

                }
            }

            using(var conn = RefDBOp.Connection.Clone() as SQLiteConnection)
            {

                var query = string.Join( "\n", qs.ToArray() );
                try
                {
                    var r = conn.Query( query );
                    var eachR = new List<string>();
                    foreach(var i in r)
                    {
                        var sb = new StringBuilder();
                        foreach(var p in prop)
                        {
                            try
                            {
                                if (i is ExpandoObject)
                                {
                                    if ( ( ( IDictionary<string, object> )i ).TryGetValue( p, out var v ) )
                                        sb.Append( $" {p} = {v}" );
                                }
                                else
                                {
                                    var dic = i as IDictionary<string, object>;
                                    if (dic.TryGetValue(p, out var vv))
                                        sb.Append( $" {p} = {vv}" );
                                    //var v = i.GetType().GetProperty( p ).GetValue( i, null );
                                    //sb.Append( $" {p} = {v}" );
                                }
                                //var toProp = i.GetType().GetProperty( p );
                                //var conv = toProp.GetValue( i );
                                //var conv = i[ p ];
                                //sb.Append( $" {p} = {i[ p ]}" );
                            }
                            catch { }
                        }
                        eachR.Add( sb.ToString() );
                    }
                    LogMessage( $"Got: {r.Count()}\n{string.Join("\n", eachR.ToArray())}" );
                }
                catch(Exception ex)
                {
                    LogMessage( ex.ToString() );
                }
            }
        }

        private void button_tableExist_Click( object sender, EventArgs e )
        {
            var name = richTextBox_query.Text;
            LogMessage( $"table {name}: is exist?={SqliteOp.TableExists( RefDBOp.Connection, name )}" );
        }

        private void button_insertTestTb_Click( object sender, EventArgs e )
        {
            var lines = richTextBox_query.Lines;
            using (var conn = RefDBOp.Connection.Clone() as SQLiteConnection)
            {
                var eachItem = new List<string>();
                foreach(var l in lines)
                {
                    var ss = l.Split( ',' );
                    if ( ss.Length < 2 )
                        continue;
                    eachItem.Add( $"INSERT INTO test_tb_01 (Name, Age, CreatedAt) VALUES ('{ss[0].Trim()}', {int.Parse( ss[1] )}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')" );
                }

                var qs = string.Join( ";", eachItem.ToArray() );
                var cmd = new SQLiteCommand( qs, conn );
                var trans = conn.BeginTransaction();
                try
                {
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
                catch(Exception ex)
                {
                    trans.Rollback();
                    LogMessage( ex.ToString() );
                }
            }
        }

        private void button_dropTable_Click( object sender, EventArgs e )
        {
            var tbNm = richTextBox_query.Text;
            var status = RefDBOp.DropTable( out var dummy, UDataCarrier.MakeOne(tbNm) );
            LogMessage( $"Drop table {tbNm} status?={status}" );
        }

        private void button_truncate_Click( object sender, EventArgs e )
        {
            var tbNm = richTextBox_query.Text;
            var status = RefDBOp.TruncateTable( out var dummy, UDataCarrier.MakeOne( tbNm ), UDataCarrier.MakeOne( true ) );
            LogMessage( $"Truncate table {tbNm} status?={status}" );
        }
    }
}
