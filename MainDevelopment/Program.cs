using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MainDevelopment
{
    static class Program
    {
        static string AppGuid = "{5993E83A-BC73-4DC5-BA67-C25927CC85D2}";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [ STAThread]
        static void Main()
        {
            //ProgramExecOnce.Check( AppGuid, new Action<object>( ctx => MessageBox.Show( "Program already exec! End program..." ) ), null );
            using ( var mux = new Mutex( false, "Global\\" + AppGuid ) )
            {
                if ( !mux.WaitOne(0, false))
                {
                    //MessageBox.Show( "Program already exec! End program..." );
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new Form1( AppGuid ) );
            }
        }
    }
}
