using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Roads
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
           DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
           DirectoryInfo di2 = new DirectoryInfo(di.Parent.Parent.FullName);
            if(!Directory.Exists(di2.FullName + "\\data"))
                Directory.CreateDirectory(di2.FullName + "\\data");
           
       textBox2.Text = di2.FullName + "\\data";
        }

        
        private void panel1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            panel1.BackColor = colorDialog1.Color;
        }

        private void panel2_Click(object sender, EventArgs e)
        {

            colorDialog2.ShowDialog();
            panel2.BackColor = colorDialog2.Color;
        }

        private void selectPathButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = textBox2.Text;
            folderBrowserDialog.ShowDialog();
            textBox2.Text = folderBrowserDialog.SelectedPath;
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            string windir = Environment.GetEnvironmentVariable("WINDIR");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = windir + @"\explorer.exe";
            prc.StartInfo.Arguments = textBox2.Text;
            prc.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.backColor = panel1.BackColor;
            Program.dir = textBox2.Text;
            Program.fileName = textBox1.Text+".png";
            Program.foreColor = panel2.BackColor;
            Program.height = (int)heigthNumericUpDown.Value;
            Program.width = (int)widthNumericUpDown.Value;
            Program.size = (int)sizeNumericUpDown.Value;
            progressBar1.Value = 0;
            progressBar1.Maximum = Program.LEN;
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            backgroundWorker.Dispose();
            MessageBox.Show("Image created succesfully!");
        }

        void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(progressBar1.Value < Program.LEN - e.ProgressPercentage)
            progressBar1.Value += e.ProgressPercentage;
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Program.CreateImage(backgroundWorker);  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("When I was a kid I used to play game based on drawing one of the three shapes in a single quare in a notebook. Games was called \"Roads\".\nOne day I was sitting on (boring) lecture and drawing those shapes in my notebook. Then I thought \"Hey!\" - I took out my laptop and implemented this ;]\nHave fun.\n\nanka.jot");
        }
    }
}
