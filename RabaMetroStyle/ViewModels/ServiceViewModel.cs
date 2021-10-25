#region

using Microsoft.Win32;
using RabaMetroStyle.Mvvm;
using System;
using System.IO;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;

#endregion

namespace RabaMetroStyle.ViewModels
{
    public class ServiceViewModel : BindableBase
    {
        private bool disableInterval;
        //private bool serviceInstalledOnMachine;
        private bool isServiceStart;

        public ServiceViewModel()
        {
            this.PopulateServiceInfo();
            this.StartServiceDelegateCommand = new DelegateCommand(this.StartService);
            this.StopServiceDelegateCommand = new DelegateCommand(this.StopService);
        }

        public bool DisableInterval
        {
            get => this.disableInterval;
            set
            {
                this.disableInterval = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("DisableStopButton");
            }
        }

        public bool IsServiceStart
        {
            get => this.isServiceStart;
            set
            {
                this.isServiceStart = value;
                this.OnPropertyChanged();
            }
        }

        public bool DisableStopButton
        {
            get => !this.disableInterval;
            set => this.disableInterval = value;
        }

        public string ExecutablePath { get; set; }

        public string MachineName { get; private set; }

        public string ServiceState { get; set; }

        public string ServiceStateText
        {
            get => this.ServiceState;
            set
            {
                this.ServiceState = value;
                this.OnPropertyChanged();
            }
        }

        public string SettingsFolderService { get; set; }

        public ICommand StartServiceCommand => this.StartServiceDelegateCommand;

        public ICommand StopServiceCommand => this.StopServiceDelegateCommand;

        private DelegateCommand StartServiceDelegateCommand { get; }

        private DelegateCommand StopServiceDelegateCommand { get; }

        private ServiceController GetServiceControllerInfo()
        {
            var oSC = new ServiceController("RabaService", this.MachineName);
            var oKey = Registry.LocalMachine;
            var serviceSubKey = oKey.OpenSubKey("SYSTEM")?.OpenSubKey("CurrentControlSet")?.OpenSubKey("Services")?.OpenSubKey("RabaService");
            var szServiceExePath = string.Empty;

            if (serviceSubKey != null)
            {
                szServiceExePath = Convert.ToString(serviceSubKey.GetValue("ImagePath").ToString());
            }

            var oWP = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            szServiceExePath = szServiceExePath.Replace('\"', ' ');
            if (File.Exists(szServiceExePath))
            {
                var oFileInfo = new FileInfo(szServiceExePath);
                this.SettingsFolderService = oFileInfo.DirectoryName + "\\Settings";
            }

            this.ExecutablePath = szServiceExePath;

            return oSC;
        }

        private bool IsCurrentProcessAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool PopulateServiceInfo()
        {
            bool bReturn;
            try
            {
                this.MachineName = Environment.MachineName;

                var oSc = this.GetServiceControllerInfo();
                this.ServiceStateText = Convert.ToString(oSc.Status);

                switch (this.ServiceStateText.ToUpper())
                {
                    case "RUNNING":
                        IsServiceStart = true;
                        this.DisableInterval = true;
                        break;

                    case "STOPPED":
                        IsServiceStart = false;
                        this.DisableInterval = false;
                        break;
                }

                //this.serviceInstalledOnMachine = true;
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

                var oSc = this.GetServiceControllerInfo();

                if (oSc.Status == ServiceControllerStatus.Stopped)
                {
                    oSc.Start();
                    this.ServiceStateText = "Running";
                    this.DisableInterval = true;
                }
                else
                {
                    oSc.Stop();
                    this.ServiceStateText = "Stopped";
                    this.DisableInterval = false;
                }
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
            if (oSC.Status != ServiceControllerStatus.Running)
            {
                return;
            }

            oSC.Stop();
            this.ServiceStateText = "Stopped";
            this.DisableInterval = false;
        }

        private string scanLocation;

        public string ScanLocation { get => scanLocation; set => SetProperty(ref scanLocation, value); }
    }
}