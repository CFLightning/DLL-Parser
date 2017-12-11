using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using IT.integro.DynamicsNAV.ProcessingTool.fileSplitter;
using IT.integro.DynamicsNAV.ProcessingTool.indentationChecker;
using IT.integro.DynamicsNAV.ProcessingTool.modificationSearchTool;
using IT.integro.DynamicsNAV.ProcessingTool.saveTool;
using IT.integro.DynamicsNAV.ProcessingTool.repositories;

namespace IT.integro.DynamicsNAV.ProcessingTool
{
    public partial class RunProcessingProgressBar : Form
    {
        string[] processes = { "Splitting file",
                "Checking indentations",
                "Searching for changes",
                "Cleaning code of changes",
                "Updating documentation trigger",
                "Saving objects to files",
                "Saving changes to files",
                "Saving documentation file",
                "Saving objects of modification",
                "Clearing repository"
            };
        bool[] processesMap;
        bool[] processesReturns;
        List<string> expModifications;
        string inputFilePath;
        string mappingFilePath;
        List<string> docModifications;
        string outputPath;

        public RunProcessingProgressBar(bool[] processesMap, string inputFilePath, string outputPath, string mappingFilePath, List<string> expModifications, List<string> docModifications)
        {
            InitializeComponent();
            progressBar1.Maximum = processes.Length;
            this.expModifications = expModifications;
            this.inputFilePath = inputFilePath;
            this.mappingFilePath = mappingFilePath;
            this.docModifications = docModifications;
            this.outputPath = outputPath;
            this.processesMap = processesMap;

            this.processesReturns = new bool[processesMap.Length-1];
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

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int processNo = 0;

            if (processesMap[processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = FileSplitter.SplitFile(inputFilePath);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = IndentationChecker.CheckIndentations();
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                ProcessFile.ReduceObjects(expModifications);
                processesReturns[processNo] = ModificationSearchTool.FindAndSaveChanges(expModifications);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = ModificationCleanerTool.CleanChangeCode();
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = DocumentationTrigger.UpdateDocumentationTrigger(docModifications);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = SaveTool.SaveObjectsToFiles(outputPath);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = SaveTool.SaveChangesToFiles(outputPath, expModifications);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = SaveTool.SaveDocumentationToFile(outputPath, DocumentationExport.GenerateDocumentationFile(outputPath, mappingFilePath, expModifications), expModifications, mappingFilePath);
            }

            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                processesReturns[processNo] = SaveTool.SaveObjectModificationFiles(outputPath, expModifications);
            }
            
            if (processesMap[++processNo])
            {
                backgroundWorker1.ReportProgress(processNo);
                ChangeClassRepository.changeRepository.Clear();
                ObjectClassRepository.objectRepository.Clear();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelCurrProcess.Text = processes[e.ProgressPercentage];
            labelHistory.Text += textBox1.Text + " - " + labelCurrProcess.Text + Environment.NewLine;
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            for (int i = 0; i < processesReturns.Length; i++)
            {
                if (processesReturns[i] != processesMap[i])
                {
                DialogResult = DialogResult.Abort;
                throw new System.ArgumentException("Background Worker Error", processes[i]);
                }
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void RunProcessingProgressBar_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
            timer1.Start();
            labelHistory.Text = "";
        }
    }
}
