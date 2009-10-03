using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            alltrax = new ControllerDataSource();
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
        }


    }
}
