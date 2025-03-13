using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using uIP.Lib;

namespace uIP.MacroProvider.Commons.CronJob
{
    public partial class FormCronJobTest : Form
    {
        internal CronJobProvider RefInstance { get; set; } = null;
        public FormCronJobTest()
        {
            InitializeComponent();
        }

        private void FormCronJobTest_FormClosing( object sender, FormClosingEventArgs e )
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void button_addInSeconds_Click( object sender, EventArgs e )
        {
            if ( RefInstance.AddInSecondsCall(
                 out var dummy,
                 UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                 UDataCarrier.MakeOne( "TestRun" ),
                 UDataCarrier.MakeOne( "Run in-seconds" ) ) )
            {
                MessageBox.Show( $"Add in-seconds success" );
            }
            else
                MessageBox.Show( $"Add in-seconds fail" );
        }

        private void button_rmvInSeconds_Click( object sender, EventArgs e )
        {
            RefInstance.RemoveInSecondsCall( out var dummy,
                UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                UDataCarrier.MakeOne( "TestRun" ) );
        }

        private void button_addInMinute_Click( object sender, EventArgs e )
        {
            if ( RefInstance.AddInMinuteCall(
                out var dummy,
                UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                UDataCarrier.MakeOne( "TestRun" ),
                UDataCarrier.MakeOne( "Run in-minute" ) ) )
            {
                MessageBox.Show( $"Add in-minute success" );
            }
            else
                MessageBox.Show( $"Add in-minute fail" );
        }

        private void button_rmvInMinute_Click( object sender, EventArgs e )
        {
            RefInstance.RemoveInMinuteCall(
                out var dummy,
                UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                UDataCarrier.MakeOne( "TestRun" ) );
        }

        private void button_addLongPeriod_Click( object sender, EventArgs e )
        {
            if ( RefInstance.AddLongPeriodCall(
                 out var dummy,
                 UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                 UDataCarrier.MakeOne( "TestRun" ),
                 UDataCarrier.MakeOne( 2 ),
                 UDataCarrier.MakeOne( "hour" ),
                 UDataCarrier.MakeOne( DateTime.Now ),
                 UDataCarrier.MakeOne( 1 ),
                 UDataCarrier.MakeOne( "Run long period in hour" )) )
            {
                MessageBox.Show( $"Add long period success" );
            }
            else
                MessageBox.Show( $"Add long period fail" );
        }

        private void button_rmvLongPeriod_Click( object sender, EventArgs e )
        {
            RefInstance.RemoveLongPeriodCall(
                out var dummy,
                UDataCarrier.MakeOne( RefInstance.GetType().FullName ),
                UDataCarrier.MakeOne( "TestRun" ) );
        }
    }
}
