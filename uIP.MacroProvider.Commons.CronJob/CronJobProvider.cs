using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.Commons.CronJob
{
    public class CronJobProvider : UMacroMethodProviderPlugin
    {
        const int WakeUpPeriod = 5;
        private bool m_bClosing = false;
        private FormCronJobTest TestForm { get; set; } = null;
        private string WD { get; set; } = "";
        private string SavePath { get; set; } = "";
        private object SyncScheduling { get; set; } = new object();
        private bool IsDataDirty { get; set; } = false;

        /// <summary>
        /// scheduling info: key is SchedulingType
        /// </summary>
        private Dictionary<string, List<KeepingScheduleInfo>> Scheduling = new Dictionary<string, List<KeepingScheduleInfo>>();
        private Thread AutoSavingT { get; set; } = null;
        private Thread ScheduleInSecondsT { get; set; } = null;
        private Thread ScheduleInMinuteT { get; set; } = null;
        private Thread ScheduleInLongPeriodT { get; set; } = null;
        public CronJobProvider() { m_strInternalGivenName = "cron_job"; }
        public override bool Initialize( UDataCarrier[] param )
        {
            // check working dir exists
            if ( !UDataCarrier.GetByIndex( param, 1, "", out var workingDir ) || !Directory.Exists( workingDir ) )
                return false;

            if ( m_bOpened )
                return true;

            WD = workingDir;
            var iniPath = CommonUtilities.RCreateDir2( Path.Combine(WD, "inis") );
            if ( string.IsNullOrEmpty( iniPath ) )
                return false;
            SavePath = Path.Combine( iniPath, "scheduling.xml" );

            // create for each scheduling
            foreach(var st in Enum.GetNames( typeof( SchedulingType ) ))
            {
                Scheduling.Add( st, new List<KeepingScheduleInfo>() );
            }

            // reload settings
            ReloadSettings();

            // register call when system up
            ResourceManager.AddSystemUpCalls( new Action( SystemUpCall ) );

            // add class provide functions
            var givenName = "";

            givenName = "AddInSecondsCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Add in-seconds call",
                ArgsDescription = new string[] { "full type name: string", "function name: string", "args or more and cannot be manual handling resources", "..." },
                ReturnValueDescription = "NA",
                Call = AddInSecondsCall
            } );
            givenName = "RemoveInSecondsCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Remove in-seconds call",
                ArgsDescription = new string[] { "full type name: string", "function name: string" },
                ReturnValueDescription = "NA",
                Call = RemoveInSecondsCall
            } );

            givenName = "AddInMinuteCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Add in-minute call",
                ArgsDescription = new string[] { "full type name: string", "function name: string", "args or more and cannot be manual handling resources", "..." },
                ReturnValueDescription = "NA",
                Call = AddInMinuteCall
            } );
            givenName = "RemoveInMinuteCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Remove in-minute call",
                ArgsDescription = new string[] { "full type name: string", "function name: string" },
                ReturnValueDescription = "NA",
                Call = RemoveInMinuteCall
            } );

            givenName = "AddLongPeriodCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Add long period more than hours",
                ArgsDescription = new string[]
                {
                    "full type name: string", 
                    "function name: string", 
                    "Exec times(int): -1(always), > 1(others)", 
                    "Unit(string): hour/ day/ week/ month",
                    "Schedule time: DateTime",
                    "Scale(int): scale * uint(hour, day, week, month)"
                },
                ReturnValueDescription = "NA",
                Call = AddLongPeriodCall
            } );
            givenName = "RemoveLongPeriodCall";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Remove long period call",
                ArgsDescription = new string[] { "full type name: string", "function name: string" },
                ReturnValueDescription = "NA",
                Call = RemoveLongPeriodCall
            } );

            givenName = "TestRun";
            m_PluginClassProvideFuncs.Add( givenName, new PluginClassProvideFunc()
            {
                Description = "Test run display to console",
                ArgsDescription = new string[] { "display string" },
                ReturnValueDescription = "NA",
                Call = TestFunc
            } );

            // class ioctl
            givenName = "TestForm";
            m_PluginClassControls.Add( givenName, new UScriptControlCarrierPluginClass( givenName, false, true, false, null,
                null, OpenTestForm
                )
            );

            m_bOpened = true;
            return true;
        }

        public override void Close()
        {
            if ( m_bClosing )
                return;

            // set to closing
            m_bClosing = true;
            // call base to close
            base.Close();

            // do release resources
            ScheduleInSecondsT?.Join();
            ScheduleInMinuteT?.Join();
            ScheduleInLongPeriodT?.Join();
            AutoSavingT?.Join();

            ScheduleInSecondsT = null;
            ScheduleInMinuteT = null;
            ScheduleInLongPeriodT = null;
            AutoSavingT = null;

            // free resource
            TestForm?.Dispose();
            TestForm = null;

            // save current
            SaveSettings();
        }

        bool OpenTestForm( UScriptControlCarrier carrier, UDataCarrier[] data )
        {
            if ( TestForm == null )
                TestForm = new FormCronJobTest() { RefInstance = this };

            TestForm?.Show();

            return true;
        }

        internal void SystemUpCall()
        {
            // reload from settings
            ReloadSettings();

            // starting
            ScheduleInSecondsT = new Thread( ScheduleInSeconds );
            ScheduleInMinuteT = new Thread( ScheduleInMinute );
            ScheduleInLongPeriodT = new Thread( ScheduleInLogPeriod );
            AutoSavingT = new Thread( Check2Save );

            ScheduleInSecondsT.Start();
            ScheduleInMinuteT.Start();
            ScheduleInLongPeriodT.Start();
            AutoSavingT.Start();
        }

        internal void SaveSettings()
        {
            Monitor.Enter( SyncScheduling );
            try
            {
                IsDataDirty = false;

                List<string> k = new List<string>();
                List<UDataCarrier> v = new List<UDataCarrier>();
                foreach(var kv in Scheduling)
                {
                    k.Add( kv.Key );
                    v.Add( UDataCarrier.MakeOne( kv.Value.ToArray() ) );
                }

                var toStore = new List<UDataCarrier>()
                { UDataCarrier.MakeOne(k.ToArray())};
                foreach(var vv in v)
                    toStore.Add( vv );

                UDataCarrier.WriteXml( toStore.ToArray(), SavePath, null );
            }
            catch { }
            finally { Monitor.Exit( SyncScheduling ); }
        }
        internal void ReloadSettings()
        {
            if ( string.IsNullOrEmpty( SavePath ) || !File.Exists( SavePath ) )
                return;

            Monitor.Enter( SyncScheduling );
            try
            {
                UDataCarrier[] got = null;
                string[] dummy = null;
                if (UDataCarrier.ReadXml( SavePath, ref got, ref dummy ) && got != null && got.Length > 1)
                {
                    UDataCarrier.GetByIndex<string[]>( got, 0, null, out var keys );
                    if ( keys != null && (keys.Length + 1) == got.Length)
                    {
                        for(int i = 0; i < keys.Length; i++)
                        {
                            var k = keys[i];
                            if ( UDataCarrier.GetByIndex<KeepingScheduleInfo[]>(got, i + 1, null, out var v) && v != null)
                            {
                                if ( Scheduling.ContainsKey( k ) )
                                    Scheduling[ k ] = v.ToList();
                                else
                                    Scheduling.Add( k, v.ToList() );
                            }
                        }
                    }
                }
            }
            catch { }
            finally { Monitor.Exit( SyncScheduling ); }
        }

        private bool AddOrReplaceCall(SchedulingType type, params UDataCarrier[] args)
        {
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var typeFullName ) || !UDataCarrier.GetByIndex( args, 1, "", out var funcName ) ||
                string.IsNullOrEmpty( typeFullName ) || string.IsNullOrEmpty( funcName ) )
                return false;
            if ( !Scheduling.ContainsKey( type.ToString() ) )
                return false;

            int argBegin = 2;
            int numExec = -1;
            int scaleCount = 1;
            RenewType scaleType = RenewType.NA;
            DateTime scheduleAt = DateTime.Now;
            if( type == SchedulingType.LongPeriod )
            {
                if ( !UDataCarrier.GetByIndex( args, argBegin++, -1, out var execC ) ||
                     !UDataCarrier.GetByIndex( args, argBegin++, "", out var scaleT ) ||
                     !UDataCarrier.GetByIndex( args, argBegin++, DateTime.Now, out var schedule ) ||
                     !UDataCarrier.GetByIndex(args, argBegin++, 1, out var scaleC ) )
                    return false;
                numExec = execC;
                scaleT = scaleT.ToLower();
                if ( scaleT == "hour" ) scaleType = RenewType.ByHour;
                else if ( scaleT == "day" ) scaleType = RenewType.ByDay;
                else if ( scaleT == "week" ) scaleType = RenewType.ByWeek;
                else if ( scaleT == "month" ) scaleType = RenewType.ByMonth;
                else
                    return false;
                scheduleAt = schedule;
                scaleCount = scaleC;
            }

            // keep as args
            List<object> remainAsArgs = new List<object>();
            for ( int i = argBegin; i < args.Length; i++ )
                remainAsArgs.Add( args[ i ].Data );

            var status = false;
            Monitor.Enter( SyncScheduling );
            try
            {
                // get by schedule type
                var rec = Scheduling[ type.ToString() ];
                // check same one
                var lcTypeFullName = typeFullName.ToLower();
                var lcFuncName = funcName.ToLower();
                var sameOne = false;
                foreach(var r in rec)
                {
                    if (r.WhichPlugin.ToLower() == lcTypeFullName && r.WhichPluginClassFunc.ToLower() == lcFuncName)
                    {
                        r.RenewWay = scaleType;
                        r.ScheduleAt = scheduleAt;
                        r.ExceptCount = numExec;
                        r.NumberScales = scaleCount;
                        r.FuncArgs = remainAsArgs.ToArray();
                        sameOne = true; break;
                    }
                }
                // new one
                if ( !sameOne )
                {
                    rec.Add( new KeepingScheduleInfo()
                    {
                        WhichPlugin = typeFullName,
                        WhichPluginClassFunc = funcName,
                        ScheduleAt = scheduleAt,
                        ExceptCount = numExec,
                        NumberScales = scaleCount,
                        FuncArgs = remainAsArgs.ToArray(),
                        RenewWay = scaleType
                    } );
                }
                IsDataDirty = true;
                status = true;
            }
            catch { }
            finally { Monitor.Exit(SyncScheduling ); }

            return status;
        }

        private bool RemoveCall( SchedulingType type, params UDataCarrier[] args )
        {
            if ( !Scheduling.ContainsKey( type.ToString() ) )
                return false;
            if ( !UDataCarrier.GetByIndex( args, 0, "", out var fullClassName ) || !UDataCarrier.GetByIndex( args, 1, "", out var funcName ) )
                return false;
            Monitor.Enter( SyncScheduling );
            try
            {
                var lcFullClassName = fullClassName.ToLower();
                var lcFuncName = funcName.ToLower();
                var list = Scheduling[ type.ToString() ];
                foreach(var i in list)
                {
                    if (i.WhichPlugin.ToLower() == lcFullClassName && i.WhichPluginClassFunc.ToLower() == lcFuncName)
                    {
                        list.Remove( i );
                        IsDataDirty = true;
                        break;
                    }
                }
            }
            catch { }
            finally { Monitor.Exit( SyncScheduling ); }
            return true;
        }

        internal bool AddInSecondsCall(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            return AddOrReplaceCall( SchedulingType.InSeconds, args );
        }
        internal bool RemoveInSecondsCall( out UDataCarrier ret, params UDataCarrier[] args )
        {
            ret = null;
            return RemoveCall( SchedulingType.InSeconds, args );
        }


        internal bool AddInMinuteCall(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            return AddOrReplaceCall( SchedulingType.InMinute, args );
        }
        internal bool RemoveInMinuteCall(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            return RemoveCall(SchedulingType.InMinute, args );
        }

        internal bool AddLongPeriodCall(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            return AddOrReplaceCall(SchedulingType.LongPeriod, args );
        }
        internal bool RemoveLongPeriodCall(out UDataCarrier ret, params UDataCarrier[] args)
        {
            ret = null;
            return RemoveCall( SchedulingType.LongPeriod, args );
        }

        private List<RunData> GetRunData( string which )
        {
            if ( !Scheduling.ContainsKey( which ) )
                return null;
            var ret = new List<RunData>();
            Monitor.Enter( SyncScheduling );
            try
            {
                var toRun = Scheduling[ which ];
                // long period
                if ( which == SchedulingType.LongPeriod.ToString() )
                {
                    var cur = DateTime.Now;
                    var toRmv = new List<KeepingScheduleInfo>();
                    foreach ( var i in toRun )
                    {
                        if ( i.ExceptCount == 0 )
                        {
                            toRmv.Add( i );
                            continue;
                        }

                        // check if exists
                        var plugin = ULibAgent.Singleton.AssemblyPlugins.GetPluginInstanceByClassCSharpTypeName( i.WhichPlugin );
                        if ( plugin == null )
                        {
                            toRmv.Add( i );
                            continue;
                        }
                        if ( !plugin.PluginClassFunctions.ContainsKey( i.WhichPluginClassFunc ) )
                        {
                            toRmv.Add( i );
                            continue;
                        }

                        // on time to schedule
                        var diff = cur - i.ScheduleAt;
                        if ( diff.TotalSeconds >= 0 )
                        {
                            var args = new List<UDataCarrier>();
                            if ( i.FuncArgs != null )
                            {
                                foreach ( var a in i.FuncArgs )
                                {
                                    args.Add( UDataCarrier.MakeOne( a ) );
                                }
                            }

                            ret.Add( new RunData() { WhichPlugin = i.WhichPlugin, WhichPluginClassFunc = i.WhichPluginClassFunc, FuncArgs = args.ToArray() } );
                            if ( i.ExceptCount > 0 )
                                i.ExceptCount -= 1;

                            if ( i.ExceptCount == 0 )
                                toRmv.Add( i );
                            else
                            {
                                // prepare to next
                                switch ( i.RenewWay )
                                {
                                    case RenewType.ByHour: i.ScheduleAt = cur.AddHours( i.NumberScales ); break;
                                    case RenewType.ByDay: i.ScheduleAt = cur.AddDays( i.NumberScales ); break;
                                    case RenewType.ByWeek: i.ScheduleAt = cur.AddDays( i.NumberScales * 7 ); break;
                                    case RenewType.ByMonth: i.ScheduleAt = cur.AddMonths( i.NumberScales ); break;
                                    default: toRmv.Add( i ); break; // invalid to remove
                                }
                                // save
                                IsDataDirty = true;
                            }
                        }
                    }

                    // need to remove item
                    if ( toRmv.Count > 0 )
                    {
                        foreach ( var i in toRmv )
                            toRun.Remove( i );
                        // set dirty to save
                        IsDataDirty = true;
                    }
                }
                else
                {
                    foreach ( var i in toRun )
                    {
                        var args = new List<UDataCarrier>();
                        if ( i.FuncArgs != null )
                        {
                            foreach ( var a in i.FuncArgs )
                            {
                                args.Add( UDataCarrier.MakeOne( a ) );
                            }
                        }

                        ret.Add( new RunData() { WhichPlugin = i.WhichPlugin, WhichPluginClassFunc = i.WhichPluginClassFunc, FuncArgs = args.ToArray() } );
                    }
                }
            }
            catch { }
            finally { Monitor.Exit( SyncScheduling ); }
            return ret;
        }

        private void ScheduleInSeconds()
        {
            fpLog?.Invoke( eLogMessageType.NORMAL, 100, $"{GetType().Name}: schedule in seconds starting..." );

            var libA = ULibAgent.Singleton;
            var prev = DateTime.Now;
            while(!m_bClosing)
            {
                var diff = DateTime.Now - prev;
                var run = new List<RunData>();
                if ( diff.TotalSeconds > WakeUpPeriod )
                {
                    run = GetRunData( SchedulingType.InSeconds.ToString() );
                }
                else
                {
                    Thread.Sleep( 500 ); // sleep for 0.5-sec
                    continue;
                }
                // run?
                if ( run != null && run.Count > 0)
                {
                    // call to run
                    foreach(var i in run)
                    {
                        libA.AssemblyPlugins?.GetPluginInstanceByClassCSharpTypeName( i.WhichPlugin )?.CallClassProvideFunc( i.WhichPluginClassFunc, out var dummy, i.FuncArgs );
                    }
                }
                // reset prev
                prev = DateTime.Now;
            }
        }

        private void ScheduleInMinute()
        {
            fpLog?.Invoke( eLogMessageType.NORMAL, 100, $"{GetType().Name}: schedule in minutes starting..." );

            var libA = ULibAgent.Singleton;
            var prev = DateTime.Now;
            while ( !m_bClosing )
            {
                var diff = DateTime.Now - prev;
                var run = new List<RunData>();
                if ( diff.TotalMinutes > 1 )
                {
                    run = GetRunData( SchedulingType.InMinute.ToString() );
                }
                else
                {
                    Thread.Sleep( 500 ); // sleep for 0.5-sec
                    continue;
                }
                // run?
                if ( run != null && run.Count > 0 )
                {
                    // call to run
                    foreach ( var i in run )
                    {
                        libA.AssemblyPlugins?.GetPluginInstanceByClassCSharpTypeName( i.WhichPlugin )?.CallClassProvideFunc( i.WhichPluginClassFunc, out var dummy, i.FuncArgs );
                    }
                }
                // reset prev
                prev = DateTime.Now;
            }
        }

        private void ScheduleInLogPeriod()
        {
            fpLog?.Invoke( eLogMessageType.NORMAL, 100, $"{GetType().Name}: schedule in long period starting..." );

            var libA = ULibAgent.Singleton;
            while ( !m_bClosing )
            {
                var run = GetRunData( SchedulingType.LongPeriod.ToString() );
                // run?
                if ( run != null && run.Count > 0 )
                {
                    // call to run
                    foreach ( var i in run )
                    {
                        libA.AssemblyPlugins?.GetPluginInstanceByClassCSharpTypeName( i.WhichPlugin )?.CallClassProvideFunc( i.WhichPluginClassFunc, out var dummy, i.FuncArgs );
                    }
                }
                // sleep a while
                Thread.Sleep( 500 ); // sleep 0.5-sec
            }
        }

        private void Check2Save()
        {
            fpLog?.Invoke( eLogMessageType.NORMAL, 100, $"{GetType().Name}: check2save starting..." );
            while(!m_bClosing)
            {
                if (IsDataDirty)
                {
                    SaveSettings();
                }
                Thread.Sleep( 2000 ); // sleep 3-sec
            }
        }

        private static bool TestFunc( out UDataCarrier ret, params UDataCarrier[] args )
        {
            if ( UDataCarrier.GetByIndex( args, 0, "", out var dummy ) )
                Console.WriteLine( $"[{CommonUtilities.GetCurrentTimeStr()}] {typeof( CronJobProvider ).FullName}: exec display {dummy}" );
            else
                Console.WriteLine( $"[{CommonUtilities.GetCurrentTimeStr()}] {typeof( CronJobProvider ).FullName}: exec" );
            ret = null;
            return true;
        }
    }
}
