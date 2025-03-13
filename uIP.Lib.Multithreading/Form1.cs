using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace uIP.Lib.Multithreading
{
    public partial class Form1 : Form
    {
		 private UMultithreading _MTProcessing;
		 private int numDispatcher;
		 private int countTestClick;
		 private int numTestJobs;
		 private int numTotalTestJobGrps;
		 private int countThreadsInDispatcher;
		 private int numJobsInJobGrps;
		 private int RowLimit = 1000;
		 private Dictionary<string, TestData> dictTest;

        public Form1()
        {
            InitializeComponent();
				_MTProcessing = new UMultithreading();
			  numDispatcher = 1;
			  countThreadsInDispatcher = 3;
				//_MTProcessing.NewDispatcher( "TestDispatcher2", 3, true );
				numTotalTestJobGrps = 1;
				countTestClick = 0;
				numTestJobs = 3;
				numJobsInJobGrps = 2;
				dictTest = new Dictionary<string, TestData>();
				//TxtFileLog.NewDir( @"D:\MTProcessingLog" );
				//TxtFileLog.NewFile();
        }

		  #region >>>Public Property<<<
		  //public int 
		  #endregion

		 private void btnTest_Click( object sender, EventArgs e )
		 {
			 countTestClick++;
			 appendUILog( "Test Start" );
			 TestData td = new TestData();
			 td.countTest = countTestClick;
			 td.countTestGrps = 0;
			 td.stamp = DateTime.Now.ToString();
			 string nameJG = string.Format( "JG{0}List{1}", countTestClick, td.countTestGrps );
			 dictTest.Add( nameJG, td );
			 ProcessingJobGroup jg = _MTProcessing.NewRunJobGroup( nameJG, SetNextTestJobGroup );

			 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job_head", jg.GivenName ), ExecJob, null, null, td.stamp );
			 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job_head2", jg.GivenName ), ExecJob, null, null, td.stamp );
			 //_MTProcessing.NewJob( jg, "TestDispatcher2", "jobhead_2", ExecJob, null, null, td.stamp );
			 _MTProcessing.AddRunJobGroup( jg );
		 }

		 //private void GenerateTestJobGroup( MTProcessingJobGroup jg, int numTestJob )
		 //{
		 //   for ( int i = 0; i < 2; i++ )
		 //   {
		 //      MTProcessingJobGroup jgAdd = new MTProcessingJobGroup();
		 //      string context = DateTime.Now.ToString();
		 //      _MTProcessing.NewJob( jgAdd, "TestDispatcher", string.Format( "job_added{0}", i + 1 ), ExecJob, null, null, context );

		 //   }
		 //}

		 private bool ExecJob( string threadNm, string jobNm, object context )
		 {
			 string timeDoJob = DateTime.Now.ToString();
			 string dataJob = string.Empty;
			 if ( context is string )
				 dataJob = ( string ) context;
				 
			 else
				 return false;

			 //appendUILog( string.Format( "{0}@Thread {1} do {2} created at {3}", timeDoJob, threadNm, jobNm, dataJob ) );
			 Console.WriteLine( string.Format( "{0}@Thread {1} do {2} created at {3}", timeDoJob, threadNm, jobNm, dataJob ) );
			 System.Threading.Thread.Sleep( 1000 );
			 return true;
		 }



		 private ProcessingJobGroup SetNextTestJobGroup( ProcessingJobGroup curr )
		 {
			 TestData td;
			 if ( dictTest.ContainsKey( curr.GivenName ) == true )
			 {
				 td = dictTest[curr.GivenName];
				 dictTest.Remove( curr.GivenName );

				 if ( td.countTestGrps < numTestJobs )
				 {
					 td.countTestGrps = td.countTestGrps + 1;
					 ProcessingJobGroup jg = _MTProcessing.NewJobGroup( string.Format( "JG{0}List{1}", td.countTest, td.countTestGrps ), SetNextTestJobGroup );
					 string context = DateTime.Now.ToString();
					 //MTProcessingThreadDispatcher disp = _MTProcessing.GetDispatcher( "TestDispatcher" );
					 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job{1}", jg.GivenName, td.countTestGrps ), ExecJob, null, null, context );
					 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job{1}_2", jg.GivenName, td.countTestGrps ), ExecJob, null, null, context );
					 //_MTProcessing.NewJob( jg, "TestDispatcher2", string.Format( "job{0}_2", td.countTestGrps ), ExecJob, null, null, context );
					 dictTest.Add( string.Format( "JG{0}List{1}", td.countTest, td.countTestGrps ), td );
					 //jg.NewJob( string.Format( "job{0}", countTestJobs ), ExecJob, null, null, context, disp, null );
					 //jg.NextHandler = SetNextTestJobGroup;
					 return jg;
					 //_MTProcessing.NewJob( jg, "TestDispatcher", string.Format( "job{0}", countTestJobs ), ExecJob, null, null, context );
				 }
				 else
				 {
					 td.countTestGrps = td.countTestGrps + 1;
					 ProcessingJobGroup jg = _MTProcessing.NewJobGroup( string.Format( "JG{0}List{1}", td.countTest, td.countTestGrps ), null );
					 string context = DateTime.Now.ToString();
					 //MTProcessingThreadDispatcher disp = _MTProcessing.GetDispatcher( "TestDispatcher" );
					 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job_tail", jg.GivenName ), ExecJob, null, null, context );
					 //jg.NewJob( string.Format( "job_tail", countTestJobs ), ExecJob, null, null, context, disp, null );
					 //jg.NextHandler = null;
					 return jg;
				 }
			 }
			 else
				 return null;
		 }

		 private void Form1_FormClosed( object sender, FormClosedEventArgs e )
		 {
			 _MTProcessing.Dispose();
			 dictTest.Clear();
		 }

		 private void btnInit_Click( object sender, EventArgs e )
		 {
			 numDispatcher = ( int ) numTotalDispatcher.Value;
			 numTotalTestJobGrps = ( int ) numTestJobGrps.Value;
			 numJobsInJobGrps = ( int ) numJobsInGrp.Value;
			 countThreadsInDispatcher = ( int ) numThreadsInDispatcher.Value;
			 for ( int i = 0; i < numDispatcher; i++ )
			 {
				 _MTProcessing.NewDispatcher( string.Format( "TestDispatcher{0}", i ), countThreadsInDispatcher, true );
			 }
		 }

		 private void Form1_Load( object sender, EventArgs e )
		 {
			 numTotalDispatcher.Value = numDispatcher;
			 numTestJobGrps.Value = numTotalTestJobGrps;
			 numJobsInGrp.Value = numJobsInJobGrps;
			 numThreadsInDispatcher.Value = countThreadsInDispatcher;
		 }

		 private void btnDoJobGrps_Click( object sender, EventArgs e )
		 {
			 for ( int i = 0; i < numTotalTestJobGrps; i++ )
			 {
				 //appendUILog( "Test Start" );
				 Console.WriteLine( "Test Start" );
				 TestData td = new TestData();
				 td.countTest = i;
				 td.countTestGrps = 0;
				 td.stamp = DateTime.Now.ToString();
				 string nameJG = string.Format( "JG{0}List{1}", i, td.countTestGrps );
				 dictTest.Add( nameJG, td );
				 ProcessingJobGroup jg = _MTProcessing.NewRunJobGroup( nameJG, SetNextTestJobGroup );

				 for ( int j = 0; j < numJobsInJobGrps; j++ )
				 {
					 _MTProcessing.NewJob( jg, "TestDispatcher0", string.Format( "{0}_job_head", jg.GivenName ), ExecJob, null, null, td.stamp );
				 }
			
				 _MTProcessing.AddRunJobGroup( jg );
				 System.Threading.Thread.Sleep( 100 );
			 }
		 }

		 private void appendUILog( object msg )
		 {
			 if ( this.InvokeRequired )
			 {
				 this.BeginInvoke( ( ParameterizedThreadStart ) appendUILog, msg );
				 return;
			 }

			 if ( lstAppStatus.Items.Count > RowLimit )
				 lstAppStatus.Items.RemoveAt( RowLimit );

			 lstAppStatus.Items.Insert( 0, msg.ToString() );
		 }
	}
	public struct TestData
	{
		public string stamp;
		public int countTest;
		public int countTestGrps;
	}
 }