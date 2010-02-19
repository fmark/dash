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
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(1024, 0);
			this.Size = new Size(800, 600);
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
		
		private void UI_Closed(object sender, EventArgs e)
        {
			if (alltraxThread.IsAlive) {
				finishAndWaitForThreads();	
			}
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
			//If there is an exception, signal that to the GUI
			//thread and then terminate this one.
			} catch (Exception e){
				lock (alltraxLock) alltraxExp = e;
			}
//			System.Console.WriteLine("Terminating worker thread.");
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
					if (MessageBox.Show("Cannot find alltrax device on COM port.  Switching to dummy data.\n" +
					                    "Press [F2] at a later time to retry.", 
					                "IO Error.", 
					                MessageBoxButtons.OKCancel, 
					                MessageBoxIcon.Error) == DialogResult.OK){
										//reset the exception status
						lock (alltraxLock) alltraxExp = null;
						startAlltrax(true);
						repaint = true;
					} else {
						System.Environment.Exit(0);
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
			panel2.Invalidate();
        }
		
		
		void panel2_Paint(object sender, PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			// now paint image
			Pen p = new Pen(System.Drawing.Color.Azure);
			Brush b = new SolidBrush(System.Drawing.Color.Red);
			
			int y = 1;
			int h = this.panel2.Height - y;
			int w = h;
			int x = (this.panel2.Width - w) / 2;
			
			g.FillEllipse(b, x, y, w, h);
			p.Dispose();	
			b.Dispose();
		}

		private void finishAndWaitForThreads(){
			lock(finishedLock) finished = true;
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
				repaint = false;
				finishAndWaitForThreads();
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
                (this.Size.Width - 480) / 2,
                (this.Size.Height - 480) / 2);
			panel1.Size = new Size(480, 480);
			/*System.Console.WriteLine(panel1.Size.Width);
			System.Console.WriteLine(this.Size.Height);
			System.Console.WriteLine(panel1.Size.Height);
			System.Console.WriteLine();*/
			panel1.BackColor = System.Drawing.Color.Bisque;
        }


    }
}
