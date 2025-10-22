using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS3Lib
{
    public partial class AttachDialog : Form
    {
        public AttachDialog()
        {
            InitializeComponent();
        }
        bool pidsort = false;
        List<Process> procid = new List<Process>();
        public void refreshprocess()
        {
            comboBox1.Items.Clear();
            procid.Clear();
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
            IEnumerable<System.Diagnostics.Process> sortedProcesses;

            if (pidsort)
            {
                // PIDでソート
                sortedProcesses = ps.OrderBy(p => p.Id);
            }
            else
            {
                // プロセス名でソート
                sortedProcesses = ps.OrderBy(p => p.ProcessName);
            }
            foreach (System.Diagnostics.Process p in sortedProcesses)
            {
                try
                {
                    if (pidsort)
                    {
                        comboBox1.Items.Add(p.Id + "_" + p.ProcessName);
                    }
                    else
                    {
                        comboBox1.Items.Add(p.ProcessName + "_" + p.Id);
                    }
                    procid.Add(p);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refreshprocess();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                if (procid[comboBox1.SelectedIndex].Id > 0)
                {
                    CCAPI.proc = procid[comboBox1.SelectedIndex];
                }
            }
            Close();
        }

        private void AttachDialog_Shown(object sender, EventArgs e)
        {
            refreshprocess();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pidsort = !pidsort;
            refreshprocess();
        }
    }
}
