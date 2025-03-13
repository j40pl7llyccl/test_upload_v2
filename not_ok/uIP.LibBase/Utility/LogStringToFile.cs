using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace uIP.LibBase.Utility
{
    public class LogStringToFile
    {
        private uint _nNumofMaxCharsOfFile;
        private bool _bErrorOccur = false;
        private bool _bEnableLog = true;


        private string _strFilePathOfNormalLog = null;
        private UInt32 _nNormalLogFileCharCount;
        private object _SyncWrNormalLog = new object();

        private string _strFilePathOfWarningLog = null;
        private UInt32 _nWarningFileCharCount;
        private object _SyncWrWarningLog = new object();

        private string _strFilePathOfErrorLog = null;
        private UInt32 _nErrorFileCharCount;
        private object _SyncWrErrorLog = new object();

        private string _strWorkingDir = null;
        private string _strBakDir = null;
        private int _nMaxFileCountOfDir = 500;
        private int _nCurFileCountOfDir = 0;
        private object _SyncDirFileOp = new object();

        #region >>> Property <<<

        public string NormalLogFilePath { get { return _strFilePathOfNormalLog; } }
        public string WarningLogFilePath { get { return _strFilePathOfWarningLog; } }
        public string ErrorLogFilePath { get { return _strFilePathOfErrorLog; }}
        public string WorkingDir { get { return _strWorkingDir; } }

        public bool Enable { get { return _bEnableLog; } set { _bEnableLog = value; } }

        public int MaxFileCount { get { return _nMaxFileCountOfDir; } set { _nMaxFileCountOfDir = value < 10 ? 10 : value; } }

        #endregion

        public LogStringToFile(uint nMaxCharsOfFile, string dirPath, int maxFileCount, string strBakDir)
        {
            _nNormalLogFileCharCount = 0;
            _nWarningFileCharCount = 0;
            _nErrorFileCharCount = 0;

            _nNumofMaxCharsOfFile = nMaxCharsOfFile;
            _nMaxFileCountOfDir = maxFileCount < 10 ? 10 : maxFileCount;

            _strBakDir = Directory.Exists( strBakDir ) ? String.Copy( strBakDir ) : null;

            if ( (_bErrorOccur = !CommonUtilities.CreateDir( dirPath )) )
                Console.WriteLine( "LogStringToFile: cannot create path {0}", dirPath );
            else
                _strWorkingDir = String.Copy( dirPath );

            if(!_bErrorOccur)
            {
                try
                {
                    // check file count
                    CommonUtilities.MaintainDirFileCount( dirPath, out _nCurFileCountOfDir, _nMaxFileCountOfDir, _strBakDir );

                    // create file rw name
                    _strFilePathOfNormalLog = Path.Combine( _strWorkingDir, String.Format( "Message_{0}.txt", CommonUtilities.GetCurrentTimeStr() ) );
                    _strFilePathOfWarningLog = Path.Combine( _strWorkingDir, String.Format( "Warning_{0}.txt", CommonUtilities.GetCurrentTimeStr() ) );
                    _strFilePathOfErrorLog = Path.Combine( _strWorkingDir, String.Format( "Error_{0}.txt", CommonUtilities.GetCurrentTimeStr() ) );
                }
                catch { _bErrorOccur = true; }
            }
        }

        private static string TimeStamp()
        {
            return String.Format( "[{0:0000}-{1:00}-{2:00}-{3:00}:{4:00}:{5:00}'{6:000}\"]", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
        }

        private static void WriteFile( object sync, ref string filepath, string str, ref uint nCharCnt, UInt32 nMaxCharCnt, 
                                       int nMaxFileCnt, string bakPath, object syncDir )
        {
            if ( String.IsNullOrEmpty( str ) )
                return;

            bool bCriticalEnter = false;
            if ( sync != null )
            {
                Monitor.Enter( sync );
                bCriticalEnter = true;
            }

            StreamWriter sw = null;

            try
            {
                // write
                sw = new StreamWriter( filepath, nCharCnt == 0 ? false : true );
                string ts = TimeStamp();
                if ( sw != null ) { sw.Write( ts ); sw.Write( str ); sw.Write( '\n' ); }
                // count chars
                nCharCnt = nCharCnt + Convert.ToUInt32( ts.Length ) + Convert.ToUInt32( str.Length ) + 1;
                // check char count
                if ( nCharCnt > nMaxCharCnt )
                {
                    // new file name
                    if ( syncDir != null ) Monitor.Enter( syncDir ); // critical section enter
                    // check file count exceeding
                    int curFileCount = 0;
                    CommonUtilities.MaintainDirFileCount( Path.GetDirectoryName( filepath ), out curFileCount, nMaxFileCnt - 1, bakPath );
                    // change file name
                    string nm = Path.GetFileNameWithoutExtension( filepath );
                    string[] snm = nm.Split( new char[] { '_' } );
                    filepath = Path.Combine( Path.GetDirectoryName( filepath ), String.Format( "{0}_{1}.txt", snm[0], CommonUtilities.GetCurrentTimeStr() ) );
                    nCharCnt = 0;
                    if ( syncDir != null ) Monitor.Exit( syncDir ); // critical section leave
                }

            }
            catch (Exception exp)
            {
                Console.WriteLine("[LogStringToFile::WriteFile] exp = \n{0}", exp.ToString());
            }
            finally {
                if ( sw != null )
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }
                if ( bCriticalEnter )
                    Monitor.Exit( sync );
            }

        }

        public void MessageLog(string msg)
        {
            if ( !_bEnableLog || _bErrorOccur ) return;
            WriteFile( _SyncWrNormalLog, ref _strFilePathOfNormalLog, msg, ref _nNormalLogFileCharCount, _nNumofMaxCharsOfFile, _nMaxFileCountOfDir, _strBakDir, _SyncDirFileOp );
        }

        public void WarningLog(string msg)
        {
            if ( !_bEnableLog || _bErrorOccur ) return;
            WriteFile( _SyncWrWarningLog, ref _strFilePathOfWarningLog, msg, ref _nWarningFileCharCount, _nNumofMaxCharsOfFile, _nMaxFileCountOfDir, _strBakDir, _SyncDirFileOp );
        }

        public void ErrorLog(string msg)
        {
            if ( !_bEnableLog || _bErrorOccur ) return;
            WriteFile( _SyncWrErrorLog, ref _strFilePathOfErrorLog, msg, ref _nErrorFileCharCount, _nNumofMaxCharsOfFile, _nMaxFileCountOfDir, _strBakDir, _SyncDirFileOp );
        }
    }
}
