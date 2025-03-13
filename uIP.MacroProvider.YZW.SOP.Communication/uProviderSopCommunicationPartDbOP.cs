using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.YZW.SOP.Communication
{
    public partial class uProviderSopCommunication
    {
        enum RegularClrClassCtrl
        {
            ScheduleDays,
            NumOfTimes,
            Applied
        }

        const string DBProvider = "sqlite_db";
        const string BufferResultTable = "buffer_results";
        const string VideoResultTable = "video_results";
        const string FolderResultTable = "folder_results";

        const string RegularlyClearDbFuncName = "RegularlyClearDb";
        const int PerLoadingMaxCount = 20;
        const string RegularlyClearCtrlName = "DbTableRegularClean";

        enum DBFuncs
        {
            Query,
            Exec,
            AddExec,
            ExecMany,
            CreateDefaultPrimaryKey,
            CreateDefaultTimestamp,
            CreateIntegerField,
            CreateFloatingField,
            CreateBlobField,
            CreateTextField,
            CreateTimeField,
            CreateTable,
            TableExists,
            DropTable,
            TruncateTable,
        }

        bool IsDbReady { get; set; } = false;
        string DbProviderClassFullname { get; set; } = "";
        string CronJobProviderClassFullname { get; set; } = "";
        bool BufferResultTableReady { get; set; } = false;
        bool VideoResultTableReady { get; set; } = false;
        bool FolderResultTableReady { get; set; } = false;

        // Make as a class control op variable
        // - store cron job
        // - fields describe in RegularClrClassCtrl
        Dictionary<string, UDataCarrier> InstallCronJob = new Dictionary<string, UDataCarrier>();

        private void InitDbOP()
        {
            m_PluginClassProvideFuncs.Add( RegularlyClearDbFuncName, new PluginClassProvideFunc()
            {
                Description = "clear db regularly",
                ArgsDescription = null,
                ReturnValueDescription = null,
                Call = RegularlyClearDb
            } );

            m_PluginClassControls.Add( RegularlyClearCtrlName, new UScriptControlCarrierPluginClass( RegularlyClearCtrlName,
                true, true, true,
                new UDataCarrierTypeDescription[]
                {
                    new UDataCarrierTypeDescription( typeof(string[]), "convert dictionary<string, UDataCarrier>")
                },
                ClassCtrlGet_RegularClrSchedule, ClassCtrlSet_RegularClrSchedule ) );

            ResourceManager.AddSystemUpCalls( DbOpSysUpCall );
        }

        void DbOpSysUpCall()
        {
            CronJobProviderClassFullname = ULibAgent.Singleton.AssemblyPlugins.GetPluginInstanceFromGivenName( "cron_job" )?.NameOfCSharpDefClass ?? "";
        }


        UDataCarrier[] ClassCtrlGet_RegularClrSchedule( UScriptControlCarrier carrier, ref bool bRetStatus )
        {
            if (carrier.Name == RegularlyClearCtrlName )
            {
                if ( InstallCronJob.Count == 0 )
                {
                    InstallCronJob.Add( RegularClrClassCtrl.ScheduleDays.ToString(), UDataCarrier.MakeOne( 7 ) );
                    InstallCronJob.Add( RegularClrClassCtrl.NumOfTimes.ToString(), UDataCarrier.MakeOne( -1 ) );
                    InstallCronJob.Add( RegularClrClassCtrl.Applied.ToString(), UDataCarrier.MakeOne( false ) );
                    if (UDataCarrier.SerializeDicKeyString( InstallCronJob, out var toStore01 ))
                    {
                        bRetStatus = true;
                        return UDataCarrier.MakeOneItemArray( toStore01 );
                    }
                }
                else if (UDataCarrier.SerializeDicKeyString(InstallCronJob, out var toStore02))
                {
                    bRetStatus = true;
                    return UDataCarrier.MakeOneItemArray( toStore02 );
                }
            }

            bRetStatus = false;
            return null;
        }
        bool ClassCtrlSet_RegularClrSchedule( UScriptControlCarrier carrier, UDataCarrier[] data )
        {
            if (carrier.Name == RegularlyClearCtrlName && UDataCarrier.GetByIndex(data, 0, new string[0], out var content ))
            {
                if ( UDataCarrier.DeserializeDicKeyStringValueOne(content, out var rev ))
                {
                    InstallCronJob = rev;
                    // not apply yet, set it
                    if (InstallCronJob.TryGetValue( RegularClrClassCtrl.Applied.ToString(), out var appliedCarr) && 
                        UDataCarrier.Get(appliedCarr, true, out var applied) && 
                        !applied)
                    {
                        int numTimes = UDataCarrier.Get( InstallCronJob, RegularClrClassCtrl.NumOfTimes.ToString(), -1 );
                        int numDays = UDataCarrier.Get( InstallCronJob, RegularClrClassCtrl.ScheduleDays.ToString(), 7 );

                        // reg cron job
                        if ( ULibAgent.CallPluginClassOpenedFuncRetStatus( CronJobProviderClassFullname, "AddLongPeriodCall", out _,
                             UDataCarrier.MakeOne( NameOfCSharpDefClass ),
                             UDataCarrier.MakeOne( RegularlyClearDbFuncName ),
                             UDataCarrier.MakeOne(numTimes),
                             UDataCarrier.MakeOne( "day" ),
                             UDataCarrier.MakeOne( DateTime.Now.AddDays( numDays) ),
                             UDataCarrier.MakeOne( numDays ) ) )
                        {
                            UDataCarrier.Set( InstallCronJob, RegularClrClassCtrl.Applied.ToString(), true );
                        }
                    }
                }
            }

            return true;
        }

        internal bool IsTableExist(string tbName)
        {
            return ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.TableExists.ToString(), out _, UDataCarrier.MakeOne( tbName ) );
        }

        internal Dictionary<string, List<object>> Query(string q)
        {
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.Query.ToString(), out var found, UDataCarrier.MakeOne( q ) ))
                return new Dictionary<string, List<object>>();

            return UDataCarrier.Get( found, new Dictionary<string, List<object>>() );
        }

        void ClearRowData(DateTime now, string tbName, string idFieldName = "id", string imagePathFieldName = "image_path" )
        {
            while ( true )
            {
                var q = $"SELECT {idFieldName}, {imagePathFieldName} {tbName} WHERE created_at < '{now.Year:0000}-{now.Month:00}-{now.Day:00}' LIMIT {PerLoadingMaxCount};";
                var found = Query( q );
                if ( found == null || found.Count == 0 || found.First().Value == null || found.First().Value.Count <= 0 )
                    break;

                var dirToDel = new List<string>();
                var idToRmv = new List<string>();
                if ( found.TryGetValue( imagePathFieldName, out var imagePaths ) )
                {
                    foreach ( var f in imagePaths )
                    {
                        var filepath = f as string;
                        if ( !string.IsNullOrEmpty( filepath ) && File.Exists( filepath ) )
                        {
                            try
                            {
                                File.Delete( filepath );
                            }
                            catch { }
                        }
                        var dir = Path.GetDirectoryName( filepath );
                        if ( !dirToDel.Contains( dir ) )
                            dirToDel.Add( dir );
                    }
                }
                if ( found.TryGetValue( idFieldName, out var ids ) )
                {
                    foreach ( var i in ids )
                    {
                        long id = 0;
                        try
                        {
                            id = Convert.ToInt64( i );
                        }
                        catch ( Exception ex )
                        {
                            continue;
                        }

                        idToRmv.Add( id.ToString() );
                    }
                }

                // remove rows
                var ss = string.Join( ",", idToRmv.ToArray() );
                q = $"DELETE FROM {tbName} WHERE {idFieldName} IN ({ss});";
                if ( ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname , DBFuncs.Exec.ToString(), out _, UDataCarrier.MakeOne(q)) )
                {
                    // check if empty
                    foreach(var d in dirToDel)
                    {
                        var files = Directory.GetFiles( d, "*", SearchOption.AllDirectories );
                        if ( files != null && files.Length > 0 )
                            continue;
                        try
                        {
                            Directory.Delete( d, true );
                        }
                        catch { }
                    }
                }
            }

        }

        void ClearBufferResultTable(DateTime now)
        {
            // check table exist?
            if ( !IsTableExist( BufferResultTable ) )
                return;

            ClearRowData( now, BufferResultTable );
        }

        void ClearVideoResultTable( DateTime now )
        {
            // check table exist?
            if ( !IsTableExist( VideoResultTable ) )
                return;

            ClearRowData( now, VideoResultTable );
        }

        void ClearFolderResultTable( DateTime now )
        {
            // check table exist?
            if ( !IsTableExist( FolderResultTable ) )
                return;

            ClearRowData( now, FolderResultTable );
        }

        bool RegularlyClearDb( out UDataCarrier ret, params UDataCarrier[] ctx )
        {
            // on-time call

            var now = DateTime.Now;
            ClearBufferResultTable(now);
            ClearVideoResultTable(now);
            ClearFolderResultTable(now);

            ret = null;
            return true;
        }

        static string GenInsert( string tbname, Dictionary<string, object> dic )
        {
            var ks = new List<string>();
            var vs = new List<string>();
            foreach(var kv in dic )
            {
                bool bStr = false;
                if ( kv.Value is string s )
                {
                    if ( string.IsNullOrEmpty( s ) )
                        continue;
                    bStr = true;
                }

                ks.Add( kv.Key );
                vs.Add( bStr ? $"'{kv.Value}'" : $"{kv.Value}" );
            }

            return $"INSERT INTO {tbname} ({string.Join(",", ks.ToArray())}) VALUES({string.Join(",", vs.ToArray())})";
        }

        bool AddExecString(string str)
        {
            if ( string.IsNullOrEmpty( str ) )
                return false;
            if ( !IsDbReady )
                return false;
            return ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.AddExec.ToString(), out _, UDataCarrier.MakeOne( str ) );
        }

        UDataCarrier[] GenBufferResultTableField( string tablename )
        {
            if ( !IsDbReady || string.IsNullOrEmpty( tablename ) )
                return null;

            var fields = new List<UDataCarrier>()
            {
                UDataCarrier.MakeOne( tablename )
            };
            UDataCarrier got = null;

            // field: Id
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultPrimaryKey.ToString(), out got, UDataCarrier.MakeOne( "id" ) ) )
                return null;
            fields.Add( got );

            // field: data_source
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "data_source" ) ) )
                return null;
            fields.Add( got );

            // field: status int(8) bitmap => 0 ok
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateIntegerField.ToString(), out got,
                    UDataCarrier.MakeOne( "status" ), UDataCarrier.MakeOne( 8 ) ) )
                return null;
            fields.Add( got );

            /*
            // field: error text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "error" ) ) )
                return null;
            fields.Add( got );

            // field: description text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "description" ) ) )
                return null;
            fields.Add( got );
            */

            // field: image_path text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "image_path" ) ) )
                return null;
            fields.Add( got );

            // field: results
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "results" ) ) )
                return null;
            fields.Add( got );

            // field: CreatedAt
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultTimestamp.ToString(), out got, UDataCarrier.MakeOne( "created_at" ) ) )
                return null;
            fields.Add( got );


            return fields.ToArray();
        }

        UDataCarrier[] GenVideoResultTableField( string tablename )
        {
            if ( !IsDbReady || string.IsNullOrEmpty( tablename ) )
                return null;

            var fields = new List<UDataCarrier>()
            {
                UDataCarrier.MakeOne(tablename)
            };
            UDataCarrier got = null;

            // field: Id
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultPrimaryKey.ToString(), out got, UDataCarrier.MakeOne( "id" ) ) )
                return null;
            fields.Add( got );

            // field: data_source
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "data_source" ) ) )
                return null;
            fields.Add( got );

            // field: status int(8) bitmap => 0 ok
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateIntegerField.ToString(), out got,
                    UDataCarrier.MakeOne( "status" ), UDataCarrier.MakeOne( 8 ) ) )
                return null;
            fields.Add( got );

            // field: error text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "error" ) ) )
                return null;
            fields.Add( got );

            // field: description text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "description" ) ) )
                return null;
            fields.Add( got );

            // field: image_path text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "image_path" ) ) )
                return null;
            fields.Add( got );

            // field: results
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "results" ) ) )
                return null;
            fields.Add( got );

            // field: CreatedAt
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultTimestamp.ToString(), out got, UDataCarrier.MakeOne( "created_at" ) ) )
                return null;
            fields.Add( got );

            return fields.ToArray();
        }

        private UDataCarrier[] GenFolderResultTableField( string tablename )
        {
            if ( !IsDbReady || string.IsNullOrEmpty( tablename ) )
                return null;

            var fields = new List<UDataCarrier>()
            {
                UDataCarrier.MakeOne(tablename)
            };
            UDataCarrier got = null;

            // field: Id
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultPrimaryKey.ToString(), out got, UDataCarrier.MakeOne( "id" ) ) )
                return null;
            fields.Add( got );

            // field: data_source
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "data_source" ) ) )
                return null;
            fields.Add( got );

            // field: status int(8) bitmap => 0 ok
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateIntegerField.ToString(), out got,
                    UDataCarrier.MakeOne( "status" ), UDataCarrier.MakeOne( 8 ) ) )
                return null;
            fields.Add( got );

            // field: error text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "error" ) ) )
                return null;
            fields.Add( got );

            // field: description text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "description" ) ) )
                return null;
            fields.Add( got );

            // field: image_path text
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "image_path" ) ) )
                return null;
            fields.Add( got );

            // field: results
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateTextField.ToString(), out got, UDataCarrier.MakeOne( "results" ) ) )
                return null;
            fields.Add( got );

            // field: CreatedAt
            if ( !ULibAgent.CallPluginClassOpenedFuncRetStatus( DbProviderClassFullname, DBFuncs.CreateDefaultTimestamp.ToString(), out got, UDataCarrier.MakeOne( "created_at" ) ) )
                return null;
            fields.Add( got );

            return fields.ToArray();
        }
    }
}
