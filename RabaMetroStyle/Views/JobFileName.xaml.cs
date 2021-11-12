using MahApps.Metro.Controls;
using System;

namespace RabaMetroStyle.Views
{
    /// <summary>
    /// Interaction logic for JobFileName.xaml
    /// </summary>
    public partial class JobFileName : MetroWindow
    {
        public bool JobFileNameSaved = false;
        public string MacroFileName = string.Empty;
        private bool? jobFileNameLogged = false;
        private bool? jobFileSerial = false;        

        public JobFileName()
        {
            this.InitializeComponent();
        }

        public JobFileName(string MacroFile)
        {
            this.InitializeComponent();
            this.txtFileName.Text = MacroFile;
            this.chkLogActions.Visibility = System.Windows.Visibility.Hidden;
            this.chkRunSerials.Visibility = System.Windows.Visibility.Hidden;
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            this.JobFileNameSaved = false;
            this.Close();
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
            if (this.txtFileName.Text.Trim().Length <= 0)
            {                
                return;
            }

            this.MacroFileName = this.txtFileName.Text;
            this.jobFileNameLogged = this.chkLogActions.IsChecked;
            this.jobFileSerial = this.chkRunSerials.IsChecked;

            this.JobFileNameSaved = true;
            this.Close();
        }
    }
}