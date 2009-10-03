using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Bessie
{
    public partial class UI : Form
    {

        private DataSource alltrax;


        
        public UI()
        {
            InitializeComponent();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                alltrax = new ControllerDataSource(Environment.GetCommandLineArgs()[1]);
            }
            else
            {
                alltrax = new ControllerDataSource();
            }
            alltrax.InitDataSource();
            updateLabels();
        }

        private void UI_FormClosed(object sender, FormClosedEventArgs e)
        {
            alltrax.CloseDataSource();
        }

        private void updateLabels()
        {
            throttle.Text = alltrax.GetThrottlePos().ToString("N1") + "%";
            temp.Text = alltrax.GetDiodeTemp().ToString("N1") + " °C";
            voltage.Text = alltrax.GetBatteryVoltage().ToString("N1") + " V";
            outputcurrent.Text = alltrax.GetOutputCurrent().ToString("N1") + " A";
            batterycurrent.Text = alltrax.GetBatteryCurrent().ToString("N1") + " A";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // update
            updateLabels();
        }

        private void UI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
            else if (e.KeyCode == Keys.F1)
            {
                alltrax.CloseDataSource();
                alltrax = new TestDataSource();
                alltrax.InitDataSource();
                updateLabels();
            }
            else if (e.KeyCode == Keys.F2)
            {
                alltrax.CloseDataSource();
                alltrax = new ControllerDataSource();
                alltrax.InitDataSource();
                updateLabels();
            }
        }

        private void temp_Click(object sender, EventArgs e)
        {

        }

        private void UI_Resize(object sender, EventArgs e)
        {
            panel1.Location = new Point(
                (this.Size.Width - panel1.Size.Width) / 2,
                (this.Size.Height - panel1.Size.Height) / 2);
        }


    }
}
