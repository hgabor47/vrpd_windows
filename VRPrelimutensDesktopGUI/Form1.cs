using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using FormSerialisation;
using System.IO;

namespace VRPrelimutensDesktopGUI
{
    public partial class Form1 : Form
    {
        static string ProgramFilesFolder;
       
        public Form1()
        {
            InitializeComponent();
            Icon icon = Icon.ExtractAssociatedIcon("vrpd.ico");
            this.Icon = icon;
            ProgramFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/vrpdGUI";
            if (!Directory.Exists(ProgramFilesFolder))
            {
                try
                {
                    Directory.CreateDirectory(ProgramFilesFolder);
                }
                catch (Exception)
                { }
            }

        }

        string lastip = "";
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox t = (sender as TextBox);
            int pos = t.SelectionStart;
            if (validip(t))
            {
                lastip = t.Text;
            }
            else
            {
                t.Text = lastip;
                t.SelectionStart = pos-1;
            }
            
        }

        private bool validip(TextBox ip)
        {
            char[] split = new char[] { '.' };
            String[] parts = ip.Text.Split(split);
            if (parts.Length > 4) return false;
            bool ok = true;
            foreach (String a in parts)
            {
                if ((a.Length < 4) && (a.Length > -1))
                {
                }
                else
                {
                    ok = false;
                }
            }

            return ok;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            maxwin.Text = (sender as TrackBar).Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            String s = "-ib "+ib.Text.Trim()+" -andro "+net.Text.Trim()+" -id c9146853-5b63-4e72-bd03-8234f53edbbf -lc "+lc.Value.ToString();
            //ExecuteCommand("call VRMainContentExporter.exe " + s);
            ExecuteCommand(s);
        }

        public Process process;
        void ExecuteCommand(string command)
        {
            //var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            var processInfo = new ProcessStartInfo("VRMainContentExporter.exe", command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;

            process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    this.BeginInvoke(new MethodInvoker(delegate
                    {                        
                        outp.AppendText(e.Data+ Environment.NewLine);
                        //outp.Refresh();
                    }));
                }
            };
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    this.BeginInvoke(new MethodInvoker(delegate
                    {
                        outp.AppendText("err:>" + e.Data+ Environment.NewLine);
                        //outp.Refresh();
                    }));
                }
            };
            process.BeginErrorReadLine();
           

            //process.WaitForExit();

            //outp.AppendText("ExitCode: "+process.ExitCode.ToString());
            //process.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                process.StandardInput.WriteLine("s\n");
                button2.Enabled = false;
                System.Threading.Thread.Sleep(2000);
                process.Close();
                button1.Enabled = true;
            }
            catch (Exception e22) { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            button2_Click(sender, null);
            try
            {
                FormSerialisor.Serialise(this, ProgramFilesFolder + @"\vrpd.xml");                
            }
            catch (Exception)
            {
            }
            finally
            {
               
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormSerialisor.Deserialise(this, ProgramFilesFolder + @"\vrpd.xml");
        }
    }
}
