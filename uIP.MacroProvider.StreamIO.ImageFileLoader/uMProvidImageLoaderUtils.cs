using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uIP.Lib;

namespace uIP.MacroProvider.StreamIO.ImageFileLoader
{
    public partial class uMProvidImageLoader
    {
        private static UDataCarrier CmpByStringInDesc( object ctx, UDataCarrier input )
        {
            if ( ctx == null || input == null || !( ctx is string want ) )
                return null;
            if ( string.IsNullOrEmpty( input.Desc ) )
                return null;
            return input.Desc == want ? input : null;
        }

        private static void ReloadFolder( Dictionary<string, UDataCarrier> dic, string folderpath, string searchpat, string toSaveFilepathsKey, string toSaveNextIndexKey )
        {
            try
            {
                // search files and sorting
                var got = Directory.GetFiles( folderpath, searchpat, SearchOption.TopDirectoryOnly );
                var byTimes = ( from f in got select new KeyValuePair<string, DateTime>( f, File.GetLastWriteTime(f) ) ).OrderBy( x=> x.Value).ToDictionary( pair => pair.Key, pair => pair.Value );
                var files = byTimes.Keys.ToArray();
                // reset info
                UDataCarrier.Set( dic, toSaveFilepathsKey, files );
                UDataCarrier.Set( dic, toSaveNextIndexKey, 0 );
                UDataCarrier.Set( dic, ImageFromMethodMutableDataKey.LoadedFileTimestamps.ToString(), byTimes );
            }
            catch { }
        }
    }
}
