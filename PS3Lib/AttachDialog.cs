using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;

namespace PS3Lib
{
    public partial class AttachDialog : Form
    {
        public AttachDialog()
        {
            InitializeComponent();
            refreshprocess();
        }

        List<int> procid = new List<int>();
        public void refreshprocess()
        {
            comboBox1.Items.Clear();
            procid.Clear();
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process p in ps)
            {
                try
                {
                    comboBox1.Items.Add(p.Id + "_" + p.MachineName + "_" + p.MainModule.FileName);
                    procid.Add(p.Id);
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
                if (procid[comboBox1.SelectedIndex] > 0)
                {
                    CCAPI.mem.OpenProcess(procid[comboBox1.SelectedIndex]);
                }
            }
            Close();
        }
    }
}
