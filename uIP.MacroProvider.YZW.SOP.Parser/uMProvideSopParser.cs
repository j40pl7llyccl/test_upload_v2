using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uIP.Lib;
using uIP.Lib.Script;
using uIP.Lib.Utility;

namespace uIP.MacroProvider.YZW.SOP.Parser
{
    public class uMProvideSopParser : UMacroMethodProviderPlugin
    {
        const string ParseVideoSplitOpenFuncName = "ParseVideoSplit";
        public uMProvideSopParser() { }
        public override bool Initialize( UDataCarrier[] param )
        {
            if ( m_bOpened )
                return true;

            m_PluginClassProvideFuncs.Add(
                ParseVideoSplitOpenFuncName,
                new PluginClassProvideFunc()
                {
                    Description = "parse video splitted file info",
                    Call = ParseVideoSplit,
                    ArgsDescription = new string[]
                    {
                        "Mutable data of macro in dictionary<string, UDataCarrier>",
                        "Folder path contain images",
                        "File to parsing full path",
                        "key of dictionary to fill file paths",
                        "key of dictionary to fill file time stamp in form Dictionary<string, DateTime>"
                    },
                    ReturnValueDescription = ""
                } );

            m_bOpened = true;
            return true;
        }

        private bool ParseVideoSplit( out UDataCarrier ret, params UDataCarrier[] ctx )
        {
            if ( !UDataCarrier.GetByIndex(ctx, 2, "", out var iniFullPath) || !File.Exists(iniFullPath) )
            {
                ret = UDataCarrier.MakeOne( "no ini file info" );
                return false;
            }
            if ( !UDataCarrier.GetByIndex<Dictionary<string, UDataCarrier>>(ctx, 0, null, out var dic) || dic == null)
            {
                ret = UDataCarrier.MakeOne( "cannot get dic in arg index 0" );
                return false;
            }
            if (!UDataCarrier.GetByIndex(ctx, 3, "", out var filePathKey) || string.IsNullOrEmpty(filePathKey))
            {
                ret = UDataCarrier.MakeOne( "cannot get file path key in arg index 3" );
                return false;
            }
            if (!UDataCarrier.GetByIndex(ctx, 4, "", out var fileTimestampKey) || string.IsNullOrEmpty(fileTimestampKey))
            {
                ret = UDataCarrier.MakeOne( "cannot get file timestamp key in arg index 4" );
                return false;
            }

            var ini = new IniReaderUtility();
            if (!ini.Parsing(iniFullPath))
            {
                ret = UDataCarrier.MakeOne( $"parse ini file {iniFullPath} fail" );
                return false;
            }

            var sections = ini.GetSections() ?? new string[ 0 ];
            var ss = ( from s in sections where s != "System Section" select s ).ToArray();
            if ( ss.Length != 1)
            {
                ret = UDataCarrier.MakeOne( $"Section more than one: {string.Join("/ ", ss)}" );
                return false;
            }

            var sd = ini.Get( ss[ 0 ] );
            var currT = DateTime.Now;
            var got = new Dictionary<string, DateTime>();
            var dir = Path.GetDirectoryName( iniFullPath );
            foreach(var kv in sd.Data)
            {
                var offsetSec = float.Parse( kv.Key.Replace( "s", "" ) );
                got.Add( Path.Combine( dir, kv.Values[ 0 ].Trim() ), currT.AddSeconds( offsetSec ) );
            }

            UDataCarrier.Set( dic, filePathKey, got.Keys );
            UDataCarrier.Set( dic, fileTimestampKey, got );
            ret = null;
            return true;
        }
    }
}
