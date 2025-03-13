using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

using System.Data.SQLite;
using Dapper;
using System.Threading;

namespace uIP.MacroProvider.DB.Sqlite
{
    public partial class SqliteOp : UMacroMethodProviderPlugin
    {
        internal string Path2DB { get; set; } = "";

        internal SQLiteConnection Connection { get; set; } = null;
        int PoolingSize { get; set; } = 100;

        FormTestDB TestForm { get; set; } = null;

        public SqliteOp() { m_strInternalGivenName = "sqlite_db"; }
        public override bool Initialize( UDataCarrier[] param )
        {
            if ( !ResourceManager.Get( "OutputDataPath", "", out var outputDataPath ) )
                return false;

            if ( m_bOpened )
                return true;


            var dir = CommonUtilities.RCreateDir2( Path.Combine( outputDataPath, "db" ));
            if ( string.IsNullOrEmpty( dir ) )
                return false;
            Path2DB = Path.Combine(dir, "sqlite.db");

            //
            // create a db and connect
            //
            var connS = $"Data Source={Path2DB};Pooling=True;Max Pool Size={PoolingSize}";
            var connI = new SQLiteConnection( connS );
            try
            {
                connI.Open();
            }
            catch(Exception e)
            {
                if ( ResourceManager.Get<ULibAgent>( ResourceManager.LibAgent, null, out var libA ) )
                    libA.LogError?.Invoke( $"{GetType().FullName}: call initialize fail with message\n{e}" );
                connI?.Dispose();
                connI = null;
                return false;
            }
            Connection = connI;

            var givenName = "";

            // provide class func
            givenName = "Query";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create table by full query string",
                ArgsDescription = new string[] { "query string", "if need append param; dynamic new { }" },
                Call = Query
            } );

            givenName = "Exec";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Exec string",
                ArgsDescription = new string[] { "query string", "if need append param" },
                Call = Exec
            } );

            givenName = "AddExec";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Add exec query(s)",
                ArgsDescription = new string[] { "query string", "if need append param" },
                Call = AddExec
            } );

            givenName = "ExecMany";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Exec many strings",
                ArgsDescription = new string[] { "query string", "..." },
                Call = ExecMany
            } );

            givenName = "CreateDefaultPrimaryKey";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create default auto-increment primary key",
                ArgsDescription = new string[] { "field name or null to default given name, 'Id'" },
                Call = CreateDefaultPrimaryKeyField
            } );

            givenName = "CreateDefaultTimestamp";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create with assigning current date time to this field when created and is UTC not local time",
                ArgsDescription = new string[] { "field name or null to default given name, 'CreatedAt'" },
                Call = CreateDefaultTimestampField
            } );

            givenName = "CreateIntegerField";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create an integer field",
                ArgsDescription = new string[] { "field name not empty", "size: 1 ~ 8" },
                Call = CreateIntegerField
            } );

            givenName = "CreateFloatingField";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create floating point field",
                ArgsDescription = new string[] { "field name not empty", "size: can be empty" },
                Call = CreateFloatingField
            } );

            givenName = "CreateBlobField";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create blob field",
                ArgsDescription = new string[] { "field name not empty", "size: can be empty" },
                Call = CreateBlobField
            } );

            givenName = "CreateTextField";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create string field",
                ArgsDescription = new string[] { "field name not empty", "size: can be empty" },
                Call = CreateTextField
            } );

            givenName = "CreateTimeField";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create time field",
                ArgsDescription = new string[] { "field name not empty" },
                Call = CreateTimeField
            } );

            givenName = "CreateTable";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Create table by defining each field without comma",
                ArgsDescription = new string[] { "Name of table", "field and type description", "..."},
                Call = CreateTable
            } );

            givenName = "TableExists";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Check table existing",
                ArgsDescription = new string[] { "table name to check" },
                Call = CheckTableExist
            } );

            givenName = "DropTable";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Drop table",
                ArgsDescription = new string[] { "table name" },
                Call = DropTable
            } );

            givenName = "TruncateTable";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Clear table items and reset sequence",
                ArgsDescription = new string[] { "table name", "reset sequence or not and can be null as false" },
                Call = TruncateTable
            } );

            // class get set
            givenName = "TestFrom";
            m_PluginClassControls.Add( givenName, new UScriptControlCarrierPluginClass( givenName, false, true, false, null,
                null, OpenTestForm
                )
            );

            InitAsyncExec();

            m_bOpened = true;
            return true;
        }

        public override void Close()
        {
            EndAsyncExec();

            base.Close();
            // close db connection
            Connection?.Close();
            Connection?.Dispose();
            Connection = null;
            // close test form if need
            TestForm?.Dispose();
            TestForm = null;
        }

        internal static bool TableExists(SQLiteConnection conn, string name)
        {
            if ( conn == null || string.IsNullOrEmpty( name ) )
                return false;
            var qstr = @"
SELECT EXISTS (
 SELECT 
  name
 FROM 
  sqlite_schema 
 WHERE 
  type='table' AND 
  name=@TableName
);
";
            try
            {
                var r = conn.Query<int>( qstr, new { TableName = name } );
                return r == null || r.Count() <= 0 ? false : r.First() != 0;
            }
            catch(Exception ex)
            {
                if ( ex != null ) { }
                return false;
            }
        }

        bool OpenTestForm( UScriptControlCarrier carrier, UDataCarrier[] data )
        {
            if ( TestForm == null)
                TestForm = new FormTestDB() { RefDBOp = this };

            TestForm?.Show();

            return true;
        }

        internal bool Query( out UDataCarrier ret, params UDataCarrier[] ctx )
        {
            ret = null;
            if ( Connection == null || !m_bOpened || !UDataCarrier.GetByIndex( ctx, 0, "", out var qs ) || string.IsNullOrEmpty( qs ) )
                return false;

            var status = false;
            IEnumerable<dynamic> results = null; 
            using ( var conn = Connection.Clone() as SQLiteConnection )
            {
                try
                {
                    if (ctx.Length == 1)
                    {
                        results = conn.Query( qs );
                        ret = results == null || results.Count() <= 0 ? null : UDataCarrier.MakeOne( results );
                        status = true;
                    }
                    else
                    {
                        results = ctx[ 1 ] == null || ctx[ 1 ].Data == null ? conn.Query( qs ) : conn.Query( qs, ctx[ 1 ].Data );
                        ret = results == null || results.Count() <= 0 ? null : UDataCarrier.MakeOne( results );
                        status = true;
                    }
                }
                catch { }
            }

            if (ctx.Length > 2 && results != null && results.Count() > 0)
            {
                var fin = new Dictionary<string, List<object>>();
                for(int i = 2; i < ctx.Length; i++)
                {
                    if ( !UDataCarrier.GetByIndex( ctx, i, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                        continue;
                    List<object> conv = new List<object>();
                    foreach(var item in results)
                    {
                        if ( item is IDictionary<string, object> r)
                        {
                            if ( r.TryGetValue( fieldName, out var fieldValue ) )
                                conv.Add( fieldValue );
                        }
                    }
                    fin.Add( fieldName, conv );
                }
                ret = UDataCarrier.MakeOne( fin );
                status = true;
            }

            return status;
        }

        internal bool Exec( out UDataCarrier ret, params UDataCarrier[] ctx )
        {
            ret = null;
            if ( Connection == null || !m_bOpened || !UDataCarrier.GetByIndex( ctx, 0, "", out var qs ) || string.IsNullOrEmpty( qs ) )
                return false;

            var status = false;
            using(var conn = Connection.Clone() as SQLiteConnection )
            {
                try
                {
                    if ( ctx.Length == 1 )
                        status = conn.Execute( qs ) >= 0;
                    else if ( ctx.Length == 2 )
                        status = conn.Execute(qs, ctx[ 1 ].Data ) >= 0;
                }
                catch { }
            }
            return status;
        }

        internal bool AddExec(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( Connection == null || !m_bOpened || tExec == null )
                return false;

            return AddQuery( arg );
        }

        internal bool ExecMany(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( Connection == null || !m_bOpened || ctx == null )
                return false;

            List<string> qss = new List<string>();
            for(int i = 0; i < ctx.Length; i++)
            {
                if ( !UDataCarrier.GetByIndex( ctx, i, "", out var q ) || string.IsNullOrEmpty( q ) )
                    continue;
                qss.Add( q );
            }
            if ( qss.Count <= 0 )
                return false;

            var qs = string.Join( ";\n", qss.ToArray() );
            var status = false;
            using ( var conn = Connection.Clone() as SQLiteConnection )
            {
                var cmd = new SQLiteCommand( qs, conn );
                var trans = conn.BeginTransaction();
                try
                {
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    status = true;
                }
                catch
                {
                    trans.Rollback();
                }
            }

            return status;
        }

        internal bool CreateDefaultPrimaryKeyField(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            var fieldName = !UDataCarrier.GetByIndex( ctx, 0, "", out var name ) || string.IsNullOrEmpty( name ) ? "Id" : name;
            ret = UDataCarrier.MakeOne( $"{fieldName} INTEGER PRIMARY KEY AUTOINCREMENT" );
            return true;
        }

        internal bool CreateIntegerField(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( arg, 0, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                return false;
            var placeholder = UDataCarrier.GetItem( arg, 1, 0, out _ );

            ret = UDataCarrier.MakeOne( $"{fieldName} {( placeholder <= 0 || placeholder > 8 ? "INTEGER" : $"int({placeholder})" )}" );
            return true;
        }

        internal bool CreateFloatingField(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( arg, 0, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                return false;

            ret = UDataCarrier.MakeOne( $"{fieldName} REAL" );
            return true;
        }

        internal bool CreateBlobField(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( arg, 0, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                return false;

            ret = UDataCarrier.MakeOne( $"{fieldName} BLOB" );
            return true;
        }

        internal bool CreateTextField(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( arg, 0, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                return false;
            var count = UDataCarrier.GetItem( arg, 1, 0, out _ );

            ret = UDataCarrier.MakeOne( $"{fieldName} {( count <= 0 ? "TEXT" : $"VARCHAR({count})" )}" );
            return true;
        }

        internal bool CreateTimeField(out UDataCarrier ret, params UDataCarrier[] arg)
        {
            ret = null;
            if ( !UDataCarrier.GetByIndex( arg, 0, "", out var fieldName ) || string.IsNullOrEmpty( fieldName ) )
                return false;

            ret = UDataCarrier.MakeOne( $"{fieldName} DATETIME" );
            return true;
        }

        internal bool CreateDefaultTimestampField(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            var fieldName = !UDataCarrier.GetByIndex( ctx, 0, "", out var name ) || string.IsNullOrEmpty( name ) ? "CreatedAt" : name;
            ret = UDataCarrier.MakeOne( $"{fieldName} DATETIME DEFAULT CURRENT_TIMESTAMP" );
            return true;
        }

        internal bool CreateTable(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( Connection == null || !m_bOpened )
                return false;

            if ( !UDataCarrier.GetByIndex( ctx, 0, "", out var tbName ) || string.IsNullOrEmpty( tbName ) )
                return false;

            var fields = new List<string>();
            for ( int i = 1; i < ctx.Length; i++ )
            {
                if ( !UDataCarrier.GetByIndex( ctx, i, "", out var field ) || string.IsNullOrEmpty( field ) )
                    return false;
                fields.Add( field );
            }
            if ( fields.Count <= 0 )
                return false;

            var qs = new StringBuilder();
            qs.Append( $"CREATE TABLE IF NOT EXISTS {tbName} (\n" );
            qs.Append( $"{string.Join( ",\n", fields.ToArray() )}" );
            qs.Append( ");" );

            var status = true;
            using(var conn = Connection.Clone() as SQLiteConnection)
            {
                try
                {
                    status = conn.Execute( qs.ToString() ) >= 0;
                }
                catch { status = false; }
            }

            return status;
        }

        internal bool CheckTableExist( out UDataCarrier ret, params UDataCarrier[] ctx )
        {
            ret = null;
            if ( Connection == null || !m_bOpened || !UDataCarrier.GetByIndex<string>( ctx, 0, "", out var tbName ) || string.IsNullOrEmpty( tbName ) )
                return false;

            var status = true;
            using(var conn = Connection.Clone() as SQLiteConnection)
            {
                try
                {
                    status = TableExists( conn, tbName );
                }
                catch { status = false; }
            }

            ret = UDataCarrier.MakeOne( status );
            return true;
        }

        internal bool DropTable(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( Connection == null || !m_bOpened || !UDataCarrier.GetByIndex( ctx, 0, "", out var tbName ) || string.IsNullOrEmpty( tbName ) )
                return false;
            var status = false;
            using ( var conn = Connection.Clone() as SQLiteConnection )
            {
                try
                {
                    status = conn.Execute( $"DROP TABLE {tbName}" ) >= 0;
                }
                catch {}
            }

            return status;
        }

        internal bool TruncateTable(out UDataCarrier ret, params UDataCarrier[] ctx)
        {
            ret = null;
            if ( Connection == null || !m_bOpened || !UDataCarrier.GetByIndex( ctx, 0, "", out var tbName ) || string.IsNullOrEmpty( tbName ) )
                return false;
            UDataCarrier.GetByIndex( ctx, 1, false, out var resetSeq );
            var status = false;
            using ( var conn = Connection.Clone() as SQLiteConnection )
            {
                try
                {
                    var qs = resetSeq ? $"DELETE FROM {tbName};DELETE FROM SQLITE_SEQUENCE WHERE name='{tbName}';" : $"DELETE FROM {tbName}";
                    var cmd = new SQLiteCommand( qs, conn );
                    var trans = conn.BeginTransaction();
                    try
                    {
                        cmd.Transaction = trans;
                        cmd.ExecuteNonQuery();
                        trans.Commit();
                        status = true;
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                    }
                }
                catch { }
            }

            return status;
        }

    }
}
