#region

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RabaMetroStyle.Models;
using RabaMetroStyle.Mvvm;
using RabaMetroStyle.Views;

#endregion

namespace RabaMetroStyle.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private ObservableCollection<string> activeMacroFiles;
        private ObservableCollection<Setting> currentSettings;
        private ObservableCollection<string> inactiveMacroFiles;

        private string selectedDisabledMacroFile;
        private Setting selectedItem;
        private string selectedMacroFile;

        public SettingsViewModel()
        {
            this.activeMacroFiles = new ObservableCollection<string>();
            this.inactiveMacroFiles = new ObservableCollection<string>();
            this.PopulateServiceInfo();
            if (Directory.Exists(this.SettingsFolderService))
            {
                PopulateSettingsFiles(this.activeMacroFiles, this.SettingsFolderService, "RABA");
                PopulateSettingsFiles(this.inactiveMacroFiles, this.SettingsFolderService, "RABA.DISABLED");
            }

            this.AddMacroDelegateCommand = new DelegateCommand(this.AddMacroFile);
            this.DisableMacroDelegateCommand = new DelegateCommand(this.DisableSelectedMacro);
            this.EnableMacroDelegateCommand = new DelegateCommand(this.EnableSelectedMacro);
            this.AddActionDelegateCommand = new DelegateCommand(this.AddMacroAction);
            this.DeleteActionDelegateCommand = new DelegateCommand(this.DeleteMacroAction);
            this.EditActionDelegateCommand = new DelegateCommand(this.EditMacroAction);
            this.PurgeMacroDelegateCommand = new DelegateCommand(this.PurgeMacroAction);
            this.CopyActionDelegateCommand = new DelegateCommand(this.CopyMacroAction);
        }

        public ICommand AddActionCommand => this.AddActionDelegateCommand;

        public ICommand AddMacroCommand => this.AddMacroDelegateCommand;

        public ICommand CopyActionCommand => this.CopyActionDelegateCommand;

        public ObservableCollection<Setting> CurrentSettingsTable
        {
            get => this.currentSettings;
            set
            {
                this.currentSettings = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand DeleteActionCommand => this.DeleteActionDelegateCommand;

        public ICommand DisableMacroCommand => this.DisableMacroDelegateCommand;

        public ICommand EditActionCommand => this.EditActionDelegateCommand;

        public ICommand EnableMacroCommand => this.EnableMacroDelegateCommand;

        public string ExecutablePath { get; set; }

        public bool IsSelectDisabledMacroFile => !string.IsNullOrEmpty(this.selectedDisabledMacroFile);

        public bool IsSelectMacroAction => this.selectedItem != null;

        public bool IsSelectMacroFile => !string.IsNullOrEmpty(this.selectedMacroFile);

        public string MachineName { get; private set; }

        public ObservableCollection<string> MacroFilesActive
        {
            get => this.activeMacroFiles;
            set
            {
                this.activeMacroFiles = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<string> MacroFilesInActive
        {
            get => this.inactiveMacroFiles;
            set
            {
                this.inactiveMacroFiles = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand PurgeMacroCommand => this.PurgeMacroDelegateCommand;

        public string SelectedDisabledMacroFile
        {
            get => this.selectedDisabledMacroFile;
            set
            {
                //this.selectedMacroFile = string.Empty;
                this.selectedDisabledMacroFile = value;
                this.OnPropertyChanged($"IsSelectMacroFile");
                this.OnPropertyChanged($"IsSelectDisabledMacroFile");
                this.OnPropertyChanged($"CanPurgeFile");
            }
        }

        public Setting SelectedItem
        {
            get => this.selectedItem;
            set
            {
                this.selectedItem = value;
                this.OnPropertyChanged($"IsSelectMacroAction");
            }
        }

        public string SelectedMacroFile
        {
            get => this.selectedMacroFile;
            set
            {
                //this.selectedDisabledMacroFile = string.Empty;
                this.selectedMacroFile = value;
                this.OnPropertyChanged($"IsSelectMacroFile");
                this.OnPropertyChanged($"IsSelectDisabledMacroFile");
                if (!string.IsNullOrEmpty(this.selectedMacroFile))
                {
                    this.HandleSelectedMacroFile();
                }
            }
        }

        public string SettingsFolderService { get; set; }

        private DelegateCommand AddActionDelegateCommand { get; }

        private DelegateCommand AddMacroDelegateCommand { get; }

        private DelegateCommand CopyActionDelegateCommand { get; }

        private DelegateCommand DeleteActionDelegateCommand { get; }

        private DelegateCommand DisableMacroDelegateCommand { get; }

        private DelegateCommand EditActionDelegateCommand { get; }

        private DelegateCommand EnableMacroDelegateCommand { get; }

        private DelegateCommand PurgeMacroDelegateCommand { get; }

        private static void PopulateSettingsFiles(ObservableCollection<string> macroFiles, string szFolder, string settingsExtension)
        {
            var szSettingExtension = settingsExtension;
            var sfiles = Directory.GetFiles(szFolder);

            foreach (var sFileName in sfiles)
            {
                if (sFileName.EndsWith(szSettingExtension))
                {
                    macroFiles.Add(Path.GetFileName(sFileName));
                }
            }
        }

        private static bool SaveSettings(string settingsFile, DataSet oDsSave)
        {
            oDsSave.WriteXml(settingsFile);
            return true;
        }

        private void AddMacroAction()
        {
            var addMacroActionForm = new MacroAction();
            addMacroActionForm.ShowDialog();

            if (addMacroActionForm.SavedAction)
            {
                var macroFileName = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                var tempSettingTable = new DataSet();
                tempSettingTable.ReadXml(macroFileName);

                if (tempSettingTable.Tables["tblTaskInfo"] == null)
                {
                    tempSettingTable.Tables.Add(this.DsFunctionCreateDataTableTasks());
                }

                var row = tempSettingTable.Tables["tblTaskInfo"].NewRow();

                row["Action"] = addMacroActionForm.SavedActionDetails.Action;
                row["ActionCompleteDelete"] = addMacroActionForm.SavedActionDetails.ActionCompleteDelete;
                row["ActionCompleteRename"] = addMacroActionForm.SavedActionDetails.ActionCompleteRename;
                row["ActionCompleteTimeStamp"] = addMacroActionForm.SavedActionDetails.ActionCompleteTimeStamp;
                row["DatabaseName"] = addMacroActionForm.SavedActionDetails.DatabaseName;
                row["DatabaseServer"] = addMacroActionForm.SavedActionDetails.DatabaseServer;
                row["IntegratedSecurity"] = addMacroActionForm.SavedActionDetails.IntegratedSecurity;
                row["Password"] = addMacroActionForm.SavedActionDetails.Password;
                row["RestoreDatabaseFileGroups"] = addMacroActionForm.SavedActionDetails.RestoreDatabaseFileGroups;
                row["RunSQLScript"] = addMacroActionForm.SavedActionDetails.RunSQLScript;
                row["RunSQLScriptFilePath"] = addMacroActionForm.SavedActionDetails.RunSQLScriptFilePath;
                row["ScanFileDateGreaterThan"] = addMacroActionForm.SavedActionDetails.ScanFileDateGreaterThan;
                row["ScanFileDateLessThan"] = addMacroActionForm.SavedActionDetails.ScanFileDateLessThan;
                row["ScanFileUseRelativeAgeYounger"] = addMacroActionForm.SavedActionDetails.ScanFileUseRelativeAgeYounger;
                row["ScanFileAgeYounger"] = addMacroActionForm.SavedActionDetails.ScanFileAgeYounger;
                row["ScanFileUseRelativeAgeOlder"] = addMacroActionForm.SavedActionDetails.ScanFileUseRelativeAgeOlder;
                row["ScanFileAgeOlder"] = addMacroActionForm.SavedActionDetails.ScanFileAgeOlder;
                row["OnlyCountWeekDays"] = addMacroActionForm.SavedActionDetails.OnlyCountWeekDays;
                row["ScanFileExtension"] = addMacroActionForm.SavedActionDetails.ScanFileExtension;
                row["ScanFilePrefix"] = addMacroActionForm.SavedActionDetails.ScanFilePrefix;
                row["ScanFileSizeGreaterThan"] = addMacroActionForm.SavedActionDetails.ScanFileSizeGreaterThan;
                row["ScanFileSizeLessThan"] = addMacroActionForm.SavedActionDetails.ScanFileSizeLessThan;
                row["ScanLocation"] = addMacroActionForm.SavedActionDetails.ScanLocation;
                row["IncludeSubFolders"] = addMacroActionForm.SavedActionDetails.IncludeSubFolders;
                row["MaintainSubFolders"] = addMacroActionForm.SavedActionDetails.MaintainSubFolders;
                row["Command"] = addMacroActionForm.SavedActionDetails.Command;
                row["TargetLocation"] = addMacroActionForm.SavedActionDetails.TargetLocation;
                row["TaskOrder"] = tempSettingTable.Tables["tblTaskInfo"].Rows.Count + 1;
                row["UserID"] = addMacroActionForm.SavedActionDetails.UserID;

                tempSettingTable.Tables["tblTaskInfo"].Rows.Add(row);

                this.OnPropertyChanged($"MacroFilesInActive");

                var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                SaveSettings(pathFile, tempSettingTable);
                this.HandleSelectedMacroFile();
            }
        }

        private void AddMacroFile()
        {
            var addMacroForm = new JobFileName();
            addMacroForm.ShowDialog();

            if (!addMacroForm.JobFileNameSaved)
            {
                return;
            }

            var szSettingsFileNew = this.SettingsFolderService + "\\" + addMacroForm.MacroFileName + ".RABA";

            var dataSet = new DataSet("Tasks");
            var dataTable = new DataTable("tblTaskInfo");
            dataSet.Tables.Add(dataTable);
            dataSet.WriteXml(szSettingsFileNew);

            this.activeMacroFiles.Add(addMacroForm.MacroFileName + ".RABA");
        }

        private ObservableCollection<Setting> ConvertDataTableToObservableCollection(DataSet settingTable)
        {
            var currentSetting = new ObservableCollection<Setting>();

            if (settingTable.Tables.Count <= 0)
            {
                return currentSetting;
            }

            if (settingTable.Tables["tblTaskInfo"].Rows.Count > 0)
            {
                currentSetting = new ObservableCollection<Setting>(from dRow in settingTable.Tables["tblTaskInfo"].AsEnumerable()
                                                                   select this.GetMacroActionDataTableRow(dRow));
            }

            return currentSetting;
        }

        private void CopyMacroAction()
        {
            if (this.selectedItem == null)
            {
                MessageBox.Show("Please select macro action", "Copy Macro Action", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
                return;
            }

            var newMacroAction = this.selectedItem;
            newMacroAction.TaskOrder = (this.currentSettings.Count + 1).ToString();

            this.currentSettings.Add(newMacroAction);

            using (var dataset = this.TransformSettingsToDataSet(this.currentSettings))
            {
                var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                SaveSettings(pathFile, dataset);
            }
        }

        private void DeleteMacroAction()
        {
            if (this.selectedItem == null)
            {
                return;
            }

            this.currentSettings.Remove(this.selectedItem);
            this.selectedItem = null;

            var dataSet = this.TransformSettingsToDataSet(this.currentSettings);
            var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
            SaveSettings(pathFile, dataSet);
        }

        private void DisableSelectedMacro()
        {
            if (string.IsNullOrEmpty(this.SelectedMacroFile))
            {
                MessageBox.Show("Please select file", "Disable File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var szFileName = this.SettingsFolderService + "\\" + this.SelectedMacroFile;
            File.Move(szFileName, szFileName + ".DISABLED");
            this.inactiveMacroFiles.Add(this.SelectedMacroFile + ".DISABLED");
            this.activeMacroFiles.Remove(this.SelectedMacroFile);
            this.CurrentSettingsTable = null;
        }

        private DataTable DsFunctionCreateDataTableTasks()
        {
            var oDtReturn = new DataTable("tblTaskInfo");

            try
            {
                oDtReturn.Columns.Add("ScanLocation");
                oDtReturn.Columns.Add("IncludeSubFolders");
                oDtReturn.Columns.Add("ScanFileExtension");
                oDtReturn.Columns.Add("ScanFilePrefix");
                oDtReturn.Columns.Add("ScanFileDateLessThan");
                oDtReturn.Columns.Add("ScanFileDateGreaterThan");
                oDtReturn.Columns.Add("ScanFileUseRelativeAgeYounger");
                oDtReturn.Columns.Add("ScanFileAgeYounger");
                oDtReturn.Columns.Add("ScanFileUseRelativeAgeOlder");
                oDtReturn.Columns.Add("OnlyCountWeekDays");
                oDtReturn.Columns.Add("ScanFileAgeOlder");
                oDtReturn.Columns.Add("ScanFileSizeLessThan");
                oDtReturn.Columns.Add("ScanFileSizeGreaterThan");
                oDtReturn.Columns.Add("Action");
                oDtReturn.Columns.Add("ActionCompleteRename");
                oDtReturn.Columns.Add("ActionCompleteTimeStamp");
                oDtReturn.Columns.Add("ActionCompleteDelete");
                oDtReturn.Columns.Add("TargetLocation");
                oDtReturn.Columns.Add("MaintainSubFolders");
                oDtReturn.Columns.Add("Command");
                oDtReturn.Columns.Add("IntegratedSecurity");
                oDtReturn.Columns.Add("UserID");
                oDtReturn.Columns.Add("Password");
                oDtReturn.Columns.Add("DatabaseName");
                oDtReturn.Columns.Add("DatabaseServer");
                oDtReturn.Columns.Add("TaskOrder");
                oDtReturn.Columns.Add("RunSQLScript");
                oDtReturn.Columns.Add("RunSQLScriptFilePath");
                oDtReturn.Columns.Add("RestoreDatabaseFileGroups");
            }
            catch (Exception ex)
            {
                // ignored
            }

            return oDtReturn;
        }

        private void EditMacroAction()
        {
            if (this.selectedItem == null)
            {
                MessageBox.Show("Please select macro action", "Edit Macro Action", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addMacroActionForm = new MacroAction(this.selectedItem);
            addMacroActionForm.ShowDialog();

            if (!addMacroActionForm.SavedAction)
            {
                return;
            }

            var selectedIndex = this.currentSettings.IndexOf(this.selectedItem);
            this.currentSettings[selectedIndex] = addMacroActionForm.SavedActionDetails;

            using (var dataSet = this.TransformSettingsToDataSet(this.currentSettings))
            {
                var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                SaveSettings(pathFile, dataSet);
            }
        }

        private void EnableSelectedMacro()
        {
            if (string.IsNullOrEmpty(this.selectedDisabledMacroFile))
            {
                MessageBox.Show("Please select file", "Enable File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var szFileName = string.Empty;
            szFileName = this.SettingsFolderService + "\\" + this.selectedDisabledMacroFile;
            this.SettingsFileEnable(szFileName);
            this.activeMacroFiles.Add(this.selectedDisabledMacroFile.Substring(0, this.selectedDisabledMacroFile.Length - 9));
            this.inactiveMacroFiles.Remove(this.selectedDisabledMacroFile);
        }

        private Setting GetMacroActionDataTableRow(DataRow row)
        {
            var settings = new Setting
                           {
                               Action = row["Action"].ToString(),
                               ActionCompleteDelete = row["ActionCompleteDelete"] == DBNull.Value ? string.Empty : (string)row["ActionCompleteDelete"],
                               ActionCompleteRename = row["ActionCompleteRename"] == DBNull.Value ? string.Empty : (string)row["ActionCompleteRename"],
                               ActionCompleteTimeStamp = row["ActionCompleteTimeStamp"] == DBNull.Value ? string.Empty : (string)row["ActionCompleteTimeStamp"],
                               DatabaseName = row["DatabaseName"] == DBNull.Value ? string.Empty : (string)row["DatabaseName"],
                               DatabaseServer = row["DatabaseServer"] == DBNull.Value ? string.Empty : (string)row["DatabaseServer"],
                               IntegratedSecurity = row["IntegratedSecurity"] == DBNull.Value ? string.Empty : (string)row["IntegratedSecurity"],
                               Password = row["Password"] == DBNull.Value ? string.Empty : (string)row["Password"],
                               RestoreDatabaseFileGroups = row["RestoreDatabaseFileGroups"] == DBNull.Value ? string.Empty : (string)row["RestoreDatabaseFileGroups"],
                               RunSQLScript = row["RunSQLScript"] == DBNull.Value ? string.Empty : (string)row["RunSQLScript"],
                               RunSQLScriptFilePath = row["RunSQLScriptFilePath"] == DBNull.Value ? string.Empty : (string)row["RunSQLScriptFilePath"],
                               ScanFileDateGreaterThan = row["ScanFileDateGreaterThan"] == DBNull.Value ? "1/1/1970" : (string)row["ScanFileDateGreaterThan"],
                               ScanFileDateLessThan = row["ScanFileDateLessThan"] == DBNull.Value ? "1/1/2070" : (string)row["ScanFileDateLessThan"],
                               ScanFileUseRelativeAgeYounger = row["ScanFileUseRelativeAgeYounger"] == DBNull.Value ? string.Empty : (string)row["ScanFileUseRelativeAgeYounger"],
                               ScanFileAgeYounger = row["ScanFileAgeYounger"] == DBNull.Value ? string.Empty : (string)row["ScanFileAgeYounger"],
                               ScanFileUseRelativeAgeOlder = row["ScanFileUseRelativeAgeOlder"] == DBNull.Value ? string.Empty : (string)row["ScanFileUseRelativeAgeOlder"],
                               ScanFileAgeOlder = row["ScanFileAgeOlder"] == DBNull.Value ? string.Empty : (string)row["ScanFileAgeOlder"],
                               OnlyCountWeekDays = row["OnlyCountWeekDays"] == DBNull.Value ? "False" : (string)row["OnlyCountWeekDays"],
                               ScanFileExtension = row["ScanFileExtension"] == DBNull.Value ? string.Empty : (string)row["ScanFileExtension"],
                               ScanFilePrefix = row["ScanFilePrefix"] == DBNull.Value ? string.Empty : (string)row["ScanFilePrefix"],
                               ScanFileSizeGreaterThan = row["ScanFileSizeGreaterThan"] == DBNull.Value ? string.Empty : (string)row["ScanFileSizeGreaterThan"],
                               ScanFileSizeLessThan = row["ScanFileSizeLessThan"] == DBNull.Value ? string.Empty : (string)row["ScanFileSizeLessThan"],
                               ScanLocation = row["ScanLocation"] == DBNull.Value ? string.Empty : (string)row["ScanLocation"],
                               IncludeSubFolders = row["IncludeSubFolders"] == DBNull.Value ? string.Empty : (string)row["IncludeSubFolders"],
                               MaintainSubFolders = row["MaintainSubFolders"] == DBNull.Value ? string.Empty : (string)row["MaintainSubFolders"],
                               Command = row["Command"] == DBNull.Value ? string.Empty : (string)row["Command"],
                               TargetLocation = row["TargetLocation"] == DBNull.Value ? string.Empty : (string)row["TargetLocation"],
                               UserID = row["UserID"] == DBNull.Value ? string.Empty : (string)row["UserID"]
                           };
            //settings.TaskOrder = row["TaskOrder"] == DBNull.Value ? string.Empty : (string)row["TaskOrder"];
            return settings;
        }

        private ServiceController GetServiceControllerInfo()
        {
            var oSc = new ServiceController("RabaService", this.MachineName);
            var oKey = Registry.LocalMachine;
            var serviceSubKey = oKey.OpenSubKey("SYSTEM")?.OpenSubKey("CurrentControlSet")?.OpenSubKey("Services")?.OpenSubKey("RabaService");
            var szServiceExePath = string.Empty;

            if (serviceSubKey != null)
            {
                szServiceExePath = Convert.ToString(serviceSubKey.GetValue("ImagePath").ToString());
            }

            var oWp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            szServiceExePath = szServiceExePath.Replace('\"', ' ');
            if (File.Exists(szServiceExePath))
            {
                var oFileInfo = new FileInfo(szServiceExePath);
                this.SettingsFolderService = oFileInfo.DirectoryName + "\\Settings";

                Directory.CreateDirectory(this.SettingsFolderService);
            }

            this.ExecutablePath = szServiceExePath;

            return oSc;
        }

        private void HandleSelectedMacroFile()
        {
            var macroFileName = this.SettingsFolderService + "\\" + this.selectedMacroFile;

            var tempSettingTable = new DataSet();
            tempSettingTable.ReadXml(macroFileName);

            this.CurrentSettingsTable = this.ConvertDataTableToObservableCollection(tempSettingTable);
        }

        private bool PopulateServiceInfo()
        {
            bool bReturn;
            try
            {
                this.MachineName = Environment.MachineName;

                var oSc = this.GetServiceControllerInfo();
                bReturn = true;
            }
            catch (NullReferenceException)
            {
                bReturn = false;
            }
            catch (Exception)
            {
                // ISSUE 
                bReturn = false;
            }

            return bReturn;
        }

        private void PurgeMacroAction()
        {
            var result = MessageBox.Show("Are You Sure You Wish To Permenantly Purge This Macro!", "caption", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.selectedDisabledMacroFile))
            {
                MessageBox.Show("Please select file", "Purge File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fileToDelete = this.selectedDisabledMacroFile;
            var filePath = this.SettingsFolderService + "\\" + fileToDelete;
            if (!File.Exists(filePath))
            {
                return;
            }

            this.MacroFilesInActive.Remove(this.selectedDisabledMacroFile);
            File.Delete(filePath);
        }

        private void SettingsFileEnable(string settingsFile)
        {
            File.Move(settingsFile, settingsFile.Substring(0, settingsFile.Length - 9));
        }

        private DataSet TransformSettingsToDataSet(ObservableCollection<Setting> macroActions)
        {
            var tempSettingTable = new DataSet();

            tempSettingTable.Tables.Add(this.DsFunctionCreateDataTableTasks());

            foreach (var setting in this.currentSettings)
            {
                var row = tempSettingTable.Tables["tblTaskInfo"].NewRow();

                row["Action"] = setting.Action;
                row["ActionCompleteDelete"] = setting.ActionCompleteDelete;
                row["ActionCompleteRename"] = setting.ActionCompleteRename;
                row["ActionCompleteTimeStamp"] = setting.ActionCompleteTimeStamp;
                row["DatabaseName"] = setting.DatabaseName;
                row["DatabaseServer"] = setting.DatabaseServer;
                row["IntegratedSecurity"] = setting.IntegratedSecurity;
                row["Password"] = setting.Password;
                row["RestoreDatabaseFileGroups"] = setting.RestoreDatabaseFileGroups;
                row["RunSQLScript"] = setting.RunSQLScript;
                row["RunSQLScriptFilePath"] = setting.RunSQLScriptFilePath;
                row["ScanFileDateGreaterThan"] = setting.ScanFileDateGreaterThan;
                row["ScanFileDateLessThan"] = setting.ScanFileDateLessThan;
                row["ScanFileUseRelativeAgeYounger"] = setting.ScanFileUseRelativeAgeYounger;
                row["ScanFileAgeYounger"] = setting.ScanFileAgeYounger;
                row["ScanFileUseRelativeAgeOlder"] = setting.ScanFileUseRelativeAgeOlder;
                row["ScanFileAgeOlder"] = setting.ScanFileAgeOlder;
                row["OnlyCountWeekDays"] = setting.OnlyCountWeekDays;
                row["ScanFileExtension"] = setting.ScanFileExtension;
                row["ScanFilePrefix"] = setting.ScanFilePrefix;
                row["ScanFileSizeGreaterThan"] = setting.ScanFileSizeGreaterThan;
                row["ScanFileSizeLessThan"] = setting.ScanFileSizeLessThan;
                row["ScanLocation"] = setting.ScanLocation;
                row["IncludeSubFolders"] = setting.IncludeSubFolders;
                row["MaintainSubFolders"] = setting.MaintainSubFolders;
                row["Command"] = setting.Command;
                row["TargetLocation"] = setting.TargetLocation;
                row["TaskOrder"] = tempSettingTable.Tables["tblTaskInfo"].Rows.Count + 1;
                row["UserID"] = setting.UserID;
                tempSettingTable.Tables["tblTaskInfo"].Rows.Add(row);
            }

            return tempSettingTable;
        }
    }
}