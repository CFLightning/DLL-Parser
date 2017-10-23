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
        public CommonProgressBar(int barSteps)
        {
            InitializeComponent();
            progressBar1.Maximum = barSteps;
            progressBar1.Step = 1;
        }
        
        public void PerformStep(string currProcess)
        {
            labelCurrProcess.Text = currProcess + "...";
            this.Refresh();
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.PerformStep();
        }
    }
}
