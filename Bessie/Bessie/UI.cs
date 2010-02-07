using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Bessie
{
    public partial class UI : Form
    {
		
		/* All calls to finished must be wrapped
		 * in a lock */
		private bool finished = false;
		private object finishedLock = new object();
		
		private bool repaint = false;

		
		/* Access to the following fields must
		 * be wrapped in a lock on alltraxLock! */
		private float batteryCurrent;
		private float batteryVoltage;
		private float diodeTemp;
		private DataSourceError errorStatus;
		private float outputCurrent;
		private float throttlePos;
		private Exception alltraxExp = null;
		
		private object alltraxLock = new object();
		/* End dataLock fields */
		private Thread alltraxThread;
        
        public UI()
        {
            InitializeComponent();
        }

		private void startAlltrax(bool dummyVersion){
			string com;
            if (Environment.GetCommandLineArgs().Length > 1)
            {
				com = Environment.GetCommandLineArgs()[1];
                
            } else {
				com = "";
            }
			alltraxThread = new Thread(unused => runAlltraxThread(com, dummyVersion));
			lock(finishedLock) finished = false;
			alltraxThread.Start();
			repaint = true;
			updateLabels();
		}
		
        private void UI_Load(object sender, EventArgs e)
        {
			startAlltrax(false);	
        }
		
		private void runAlltraxThread(object com, object dummy_data){
			string com_p = (string) com;
			bool dummy = (bool) dummy_data;
			bool finCp;
			float batteryCurrentCp;
			float batteryVoltageCp;
			float diodeTempCp;
			DataSourceError errorStatusCp;
			float outputCurrentCp;
			float throttlePosCp;
			DataSource alltrax;
						
			try {
				if (dummy){
					alltrax = new TestDataSource();
				} else {
					if (com_p.Equals("")){
						alltrax = new ControllerDataSource();
					} else {
						alltrax = new ControllerDataSource(com_p);
					}
				}
				alltrax.InitDataSource();
				lock(finishedLock) finCp = finished;
				while(!finCp){
					//read the COM port - slow!
					batteryCurrentCp = alltrax.GetBatteryCurrent();
					batteryVoltageCp = alltrax.GetBatteryVoltage();
					diodeTempCp = alltrax.GetDiodeTemp();
					errorStatusCp = alltrax.getErrorStatus();
					outputCurrentCp = alltrax.GetOutputCurrent();
					throttlePosCp = alltrax.GetThrottlePos();
					
					//copy the results into shared memory
					lock (alltraxLock){
						batteryCurrent = batteryCurrentCp;
						batteryVoltage = batteryVoltageCp;
						diodeTemp = diodeTempCp;
						errorStatus = errorStatusCp;
						outputCurrent = outputCurrentCp;
						throttlePos = throttlePosCp;
					}
					
					lock(finishedLock) finCp = finished;
				}
				alltrax.CloseDataSource();
			} catch (Exception e){
				lock (alltraxLock) alltraxExp = e;
			}
			System.Console.WriteLine("Terminating worker thread.");
		}

        private void updateLabels()
        {
			float batteryCurrentCp;
			float batteryVoltageCp;
			float diodeTempCp;
			DataSourceError errorStatusCp;
			float outputCurrentCp;
			float throttlePosCp;
			Exception exp;
			
			if (!repaint) return;

			//Get instrument data in lock, as it
			//can be updated by another thread.
			//Do as little as possible in lock so it
			//can be released ASAP
			lock (alltraxLock){
				batteryCurrentCp = batteryCurrent;
				batteryVoltageCp = batteryVoltage;
				diodeTempCp = diodeTemp;
				errorStatusCp = errorStatus;
				outputCurrentCp = outputCurrent;
				throttlePosCp = throttlePos;
				exp = alltraxExp;
			}
			
			if (exp == null){
				//Update the labels
				throttle.Text = throttlePosCp.ToString("N1") + "%";
	            temp.Text = diodeTempCp.ToString("N1") + " °C";
	            voltage.Text = batteryVoltageCp.ToString("N1") + " V";
	            outputcurrent.Text = outputCurrentCp.ToString("N1") + " A";
	            batterycurrent.Text = batteryCurrentCp.ToString("N1") + " A";
			} else {
				repaint = false;
				if (exp is System.IO.IOException){
					if (MessageBox.Show("Cannot read from the COM port.  Switching to dummy data.", 
					                "IO Error.", 
					                MessageBoxButtons.OKCancel, 
					                MessageBoxIcon.Error) == DialogResult.OK){
										//reset the exception status
						lock (alltraxLock) alltraxExp = null;
						startAlltrax(true);
						repaint = true;
					} else {
						System.Environment.Exit(1);
					}				

				} else {
					MessageBox.Show(exp.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
					System.Environment.Exit(1);
				}
			}
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // update
            updateLabels();
        }

		private void finishAndWaitForThreads(){
			lock(finishedLock) finished = true;
			System.Console.Out.Flush();
			alltraxThread.Join();
		}
		
        private void UI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                finishAndWaitForThreads();
				Application.Exit();
            }
           else if (e.KeyCode == Keys.F1)
            {
                System.Console.WriteLine("Starting dummy.");
				repaint = false;
				finishAndWaitForThreads();
				System.Console.WriteLine("Killed current, starting new.");
				startAlltrax(true);
				repaint = true;
                updateLabels();
            }
           else if (e.KeyCode == Keys.F2)
            {
                repaint = false;
				finishAndWaitForThreads();
				startAlltrax(false);
				repaint = true;
                updateLabels();
            }
        }


        private void UI_Resize(object sender, EventArgs e)
        {
            panel1.Location = new Point(
                (this.Size.Width - panel1.Size.Width) / 2,
                (this.Size.Height - panel1.Size.Height) / 2);
        }


    }
}
