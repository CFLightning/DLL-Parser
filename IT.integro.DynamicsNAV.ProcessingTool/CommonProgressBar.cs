using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public partial class CommonProgressBar : Form
    {
        public CommonProgressBar(string Caption ,int barSteps)
        {
            InitializeComponent();
            progressBar1.Maximum = barSteps;
            progressBar1.Step = 1;
            this.Text = Caption;
        }
        
        public void PerformStep(string currProcess)
        {
            labelCurrProcess.Text = currProcess + "...";
            this.Refresh();
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.PerformStep();
        }

        int minutes;
        int seconds;

        private void timer1_Tick(object sender, EventArgs e)
        {
            seconds++;
            if (seconds % 60 == 0)
            {
                seconds = 0;
                minutes++;
            }
            textBox1.Text = minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0');
        }
    }
}
