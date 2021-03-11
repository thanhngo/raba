using RabaMetroStyle.Mvvm;
using System;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;

namespace RabaMetroStyle.ViewModels
{
    public class ServiceViewModel : BindableBase
    {
        private bool disableInterval = false;
        private string executablePath;
        private string machineName;
        private bool serviceInstalledOnMachine = false;

        private string serviceState;
        private string settingsFolderService;

        public ServiceViewModel()
        {
            this.PopulateServiceInfo();
            this.StartServiceDelegateCommand = new DelegateCommand(this.StartService);
            this.StopServiceDelegateCommand = new DelegateCommand(this.StopService);
        }

        public bool DisableInterval
        {
            get { return this.disableInterval; }
            set
            {
                this.disableInterval = value;
                this.OnPropertyChanged("DisableInterval");
                this.OnPropertyChanged("DisableStopButton");
            }
        }

        public bool DisableStopButton
        {
            get { return !this.disableInterval; }
            set { this.disableInterval = value; }
        }

        public string ExecutablePath
        {
            get { return this.executablePath; }
            set { this.executablePath = value; }
        }

        public string MachineName
        {
            get { return this.machineName; }
        }

        public string ServiceState
        {
            get => this.serviceState;
            set => this.serviceState = value;
        }

        public string ServiceStateText
        {
            get { return this.ServiceState; }
            set
            {
                this.ServiceState = value;
                this.OnPropertyChanged("ServiceStateText");
            }
        }

        public string SettingsFolderService
        {
            get { return this.settingsFolderService; }
            set { this.settingsFolderService = value; }
        }

        public ICommand StartServiceCommand
        {
            get { return this.StartServiceDelegateCommand; }
        }

        public ICommand StopServiceCommand
        {
            get { return this.StopServiceDelegateCommand; }
        }

        private DelegateCommand StartServiceDelegateCommand { get; set; }

        private DelegateCommand StopServiceDelegateCommand { get; set; }

        public bool IsCurrentProcessAdmin()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private ServiceController GetServiceControllerInfo()
        {
            var oSC = new ServiceController("RabaService", this.machineName);
            Microsoft.Win32.RegistryKey oKey = Microsoft.Win32.Registry.LocalMachine;
            var serviceSubKey = oKey.OpenSubKey("SYSTEM")?.OpenSubKey("CurrentControlSet")?.OpenSubKey("Services")?.OpenSubKey("RabaService");
            var szServiceExePath = string.Empty;

            if (serviceSubKey != null)
            {
                szServiceExePath = Convert.ToString(serviceSubKey.GetValue("ImagePath").ToString());
            }

            System.Security.Principal.WindowsPrincipal oWP = new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent());
            szServiceExePath = szServiceExePath.Replace('\"', ' ');
            if (System.IO.File.Exists(szServiceExePath))
            {
                var oFileInfo = new System.IO.FileInfo(szServiceExePath);
                this.settingsFolderService = oFileInfo.DirectoryName + "\\Settings";
            }

            this.executablePath = szServiceExePath;

            return oSC;
        }

        private bool PopulateServiceInfo()
        {
            bool bReturn;
            try
            {
                this.machineName = Environment.MachineName.ToString();

                var oSC = this.GetServiceControllerInfo();
                this.ServiceStateText = Convert.ToString(oSC.Status);

                switch (this.ServiceStateText.ToUpper())
                {
                    case "RUNNING":
                        this.DisableInterval = false;
                        break;

                    case "STOPPED":
                        this.DisableInterval = true;
                        break;
                }

                this.serviceInstalledOnMachine = true;
                bReturn = true;
            }
            catch (NullReferenceException exNull)
            {
                bReturn = false;
            }
            catch (Exception ex)
            {
                // ISSUE 
                bReturn = false;
            }

            return bReturn;
        }

        private void StartService()
        {
            try
            {
                if (!this.IsCurrentProcessAdmin())
                {
                    MessageBox.Show("Please running program with administrator to use this function");
                    return;
                }

                var oSC = this.GetServiceControllerInfo();

                if (oSC.Status == ServiceControllerStatus.Stopped)
                {
                    oSC.Start();
                }
                else
                {
                    MessageBox.Show("There Was an Issue In Starting the Service \n The Service Might Be In The Process Of Changing Status");
                }

                this.ServiceStateText = "Running";
                this.DisableInterval = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There Was an Issue In Starting the Service \n The Service Might Be In The Process Of Changing Status");
            }
        }

        private void StopService()
        {
            if (!this.IsCurrentProcessAdmin())
            {
                MessageBox.Show("Please running program with administrator to use this function");
                return;
            }

            var oSC = this.GetServiceControllerInfo();
            if (oSC.Status == ServiceControllerStatus.Running)
            {
                oSC.Stop();
                this.ServiceStateText = "Stopped";
                this.DisableInterval = true;
            }
        }
    }
}