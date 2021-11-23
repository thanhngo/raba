#region

using Microsoft.Win32;
using RabaMetroStyle.Models;
using RabaMetroStyle.Mvvm;
using RabaMetroStyle.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

#endregion

namespace RabaMetroStyle.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private ObservableCollection<string> activeMacroFiles;
        private ObservableCollection<string> selectedActiveMacroFiles;
        private ObservableCollection<string> selectedDisabledMacroFiles;
        private ObservableCollection<Setting> currentSettings;
        private ObservableCollection<string> inactiveMacroFiles;                

        private string selectedDisabledMacroFile;
        private Setting selectedItem;
        private string selectedMacroFile;
        private string quickSaveVisibility = "Hidden";
        private string quickEditActionSectionVisibility = "Hidden";
        private string showHideRabaFile = "Visible";
        private int totalPreviousSelectedActiveFile = 0;

        private int action { get; set; }
        private string command { get; set; }
        private string databaseName { get; set; }
        private string databaseServer { get; set; }
        private bool includeSubFolders { get; set; }
        private string integratedSecurity { get; set; }
        private string runSQLScript { get; set; }
        private string runSQLScriptFilePath { get; set; }
        private string scanFileExtension { get; set; }
        private string scanFilePrefix { get; set; }
        private string scanFileSizeGreaterThan { get; set; }
        private string scanFileSizeLessThan { get; set; }
        private string scanLocation { get; set; }
        private string targetLocation { get; set; }
        private bool conditionalDelete { get; set; }
        private DataGridLength colWidth { get; set; }

        public SettingsViewModel()
        {
            this.activeMacroFiles = new ObservableCollection<string>();
            this.inactiveMacroFiles = new ObservableCollection<string>();
            this.selectedActiveMacroFiles = new ObservableCollection<string>();
            this.selectedDisabledMacroFiles = new ObservableCollection<string>();
            this.PopulateServiceInfo();
            if (Directory.Exists(this.SettingsFolderService))
            {
                PopulateSettingsFiles(this.activeMacroFiles, this.SettingsFolderService, "RABA");
                PopulateSettingsFiles(this.inactiveMacroFiles, this.SettingsFolderService, "RABA.DISABLED");
            }

            this.AddMacroDelegateCommand = new DelegateCommand(this.AddMacroFile);
            this.DisableMacroDelegateCommand = new DelegateCommand(this.DisableSelectedMacro);
            this.RenameMacroDelegateCommand = new DelegateCommand(this.RenameMacroFile);
            this.EnableMacroDelegateCommand = new DelegateCommand(this.EnableSelectedMacro);
            this.AddActionDelegateCommand = new DelegateCommand(this.AddMacroAction);
            this.DeleteActionDelegateCommand = new DelegateCommand(this.DeleteMacroAction);
            this.EditActionDelegateCommand = new DelegateCommand(this.EditMacroAction);
            this.QuickEditActionDelegateCommand = new DelegateCommand(this.QuickEditMacroAction);
            this.PurgeMacroDelegateCommand = new DelegateCommand(this.PurgeMacroAction);
            this.CopyActionDelegateCommand = new DelegateCommand(this.CopyMacroAction);
            this.DoubleClickDelegateCommand = new DelegateCommand(this.EditMacroAction);
            this.QuickSaveMacroDelegateCommand = new DelegateCommand(this.QuickSaveAction);
            this.CancelActionDelegateCommand = new DelegateCommand(this.CancelAction);
            this.ToggerMenuDelegateCommand = new DelegateCommand(this.ToggerMenu);
        }        

        public ICommand AddActionCommand => this.AddActionDelegateCommand;

        public ICommand AddMacroCommand => this.AddMacroDelegateCommand;

        public ICommand CopyActionCommand => this.CopyActionDelegateCommand;
        public ICommand DoubleClickCommand => this.DoubleClickDelegateCommand;

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

        public ICommand RenameMacroCommand => this.RenameMacroDelegateCommand;

        public ICommand EditActionCommand => this.EditActionDelegateCommand;
        public ICommand QuickEditActionCommand => this.QuickEditActionDelegateCommand;

        public ICommand EnableMacroCommand => this.EnableMacroDelegateCommand;
        public ICommand QuickSaveActionCommand => this.QuickSaveMacroDelegateCommand;
        public ICommand CancelActionCommand => this.CancelActionDelegateCommand;
        public ICommand ToggerMenuCommand => this.ToggerMenuDelegateCommand;

        public DataGridLength ColWidth
        {
            get => this.colWidth;
            set
            {
                this.colWidth = value;
                this.OnPropertyChanged();
            }
        }

        public int TotalPreviousSelectedActiveFile
        {
            get => this.totalPreviousSelectedActiveFile;
            set
            {
                this.totalPreviousSelectedActiveFile = value;
            }
        }

        public string ExecutablePath { get; set; }

        public bool IsSelectDisabledMacroFile => !string.IsNullOrEmpty(this.selectedDisabledMacroFile);

        public bool IsSelectMacroAction => this.selectedItem != null;

        public bool IsSelectMacroFile => !string.IsNullOrEmpty(this.selectedMacroFile);

        public string MachineName { get; private set; }
        public string QuickSaveVisibility
        {
            get => this.quickSaveVisibility;
            set
            {
                this.quickSaveVisibility = value;
                this.OnPropertyChanged();
            }
        }
        public string QuickEditVisibility
        {
            get => this.quickSaveVisibility.Equals("Hidden") ? "Visible" : "Hidden";
        }

        public string ShowHideManageRabaFile
        {
            get => this.showHideRabaFile;
            set
            {
                showHideRabaFile = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged("ShowHideCurrentFilePanel");
            }
        }

        public string ShowHideCurrentFilePanel
        {
            get => showHideRabaFile.Equals("Collapsed") ? "Visible" : "Collapsed";
        }

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

        public ObservableCollection<string> SelectedActiveMacroFiles
        {
            get => this.selectedActiveMacroFiles;
            set
            {
                this.selectedActiveMacroFiles.Clear();
                foreach (var file in value)
                {
                    selectedActiveMacroFiles.Add(file);
                }

            }
        }

        public ObservableCollection<string> SelectedDisabledMacroFiles
        {
            get => this.selectedDisabledMacroFiles;
            set
            {
                selectedDisabledMacroFiles.Clear();
                foreach (var file in value)
                {
                    selectedDisabledMacroFiles.Add(file);
                }
            }
        }

        public ICommand PurgeMacroCommand => this.PurgeMacroDelegateCommand;

        public string SelectedDisabledMacroFile
        {
            get => this.selectedDisabledMacroFile ;
            set
            {
                this.SelectedMacroFile = null;
                if(value != null)
                {
                    this.selectedActiveMacroFiles.Clear();
                    this.totalPreviousSelectedActiveFile = 0;
                    this.OnPropertyChanged($"SelectedMacroFile");
                }
                
                this.selectedDisabledMacroFile = value == null ? value : value + ".RABA.DISABLED";                
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
                if (selectedItem != null)
                {
                    this.PopulateSelectedData();
                }

                this.IsSelectedAction = selectedItem == null ? "Hidden" : "Visible";
                this.IsNotSlectedAction = selectedItem == null ? "Visible" : "Hidden";
                this.OnPropertyChanged($"IsSelectedAction");
                this.OnPropertyChanged($"IsNotSlectedAction");
                this.OnPropertyChanged($"IsSelectMacroAction");
            }
        }

        public string IsNotSlectedAction { get; set; }
        public string IsSelectedAction
        {
            get => this.quickEditActionSectionVisibility;
            set
            {
                this.quickEditActionSectionVisibility = value;
            }
        }

        private void PopulateSelectedData()
        {
            this.ScanLocation = selectedItem.ScanLocation;
            this.Action = GetActionIndex(selectedItem.Action);
            this.IncludeSubFolders = selectedItem.IncludeSubFolders.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.ScanFileExtension = selectedItem.ScanFileExtension;
            this.ScanFilePrefix = selectedItem.ScanFilePrefix;
            this.DatabaseName = selectedItem.DatabaseName;
            this.DatabaseServer = selectedItem.DatabaseServer;
            this.IntegratedSecurity = selectedItem.IntegratedSecurity;
            this.RunSQLScript = selectedItem.RunSQLScript;
            this.RunSQLScriptFilePath = selectedItem.RunSQLScriptFilePath;
            this.ScanFileSizeGreaterThan = selectedItem.ScanFileSizeGreaterThan;
            this.ScanFileSizeLessThan = selectedItem.ScanFileSizeLessThan;
            this.ConditionalDelete = selectedItem.ConditionalDelete.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.TargetLocation = selectedItem.TargetLocation;
            this.OnPropertyChanged($"ScanLocation");
            this.OnPropertyChanged($"Action");
            this.OnPropertyChanged($"IncludeSubFolders");
            this.OnPropertyChanged($"ScanFileExtension");
            this.OnPropertyChanged($"ScanFilePrefix");
            this.OnPropertyChanged($"DatabaseName");
            this.OnPropertyChanged($"DatabaseServer");
            this.OnPropertyChanged($"IntegratedSecurity");
            this.OnPropertyChanged($"RunSQLScript");
            this.OnPropertyChanged($"RunSQLScriptFilePath");
            this.OnPropertyChanged($"ScanFileSizeGreaterThan");
            this.OnPropertyChanged($"ScanFileSizeLessThan");
            this.OnPropertyChanged($"ConditionalDelete");
            this.OnPropertyChanged($"TargetLocation");
        }

        private int GetActionIndex(string Action)
        {
            var actionIndex = 0;
            switch (Action)
            {
                case "COPY":
                    actionIndex = 0;
                    break;
                case "DELETE":
                    actionIndex = 1;
                    break;
                case "MOVE":
                    actionIndex = 2;
                    break;
                case "ZIP":
                    actionIndex = 3;
                    break;
                case "UNZIP":
                    actionIndex = 4;
                    break;
                case "BATCH":
                    actionIndex = 5;
                    break;
                case "SQLSCRIPT":
                    actionIndex = 6;
                    break;
            }
            return actionIndex;
        }

        private string GetAction(int ActionIndex)
        {
            string action = "COPY";
            switch (ActionIndex)
            {
                case 0:
                    action = "COPY";
                    break;
                case 1:
                    action = "DELETE";
                    break;
                case 2:
                    action = "MOVE";
                    break;
                case 3:
                    action = "ZIP";
                    break;
                case 4:
                    action = "UNZIP";
                    break;
                case 5:
                    action = "BATCH";
                    break;
                case 6:
                    action = "SQLSCRIPT";
                    break;
            }
            return action;
        }

        public string SelectedMacroFile
        {
            get => this.selectedMacroFile;
            set
            {
                var removeSettingFile = false;
                if(value != null)
                {
                    this.selectedDisabledMacroFile = null;
                    this.OnPropertyChanged($"SelectedDisabledMacroFile");
                }                

                if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    if (this.SelectedActiveMacroFiles.Count - this.TotalPreviousSelectedActiveFile == 1 && this.SelectedActiveMacroFiles.Count != 2)
                    {
                        removeSettingFile = true;
                    }
                    else
                    {
                        if (this.TotalPreviousSelectedActiveFile - this.SelectedActiveMacroFiles.Count == 1 && this.TotalPreviousSelectedActiveFile != 3)
                        {
                            removeSettingFile = true;
                        }
                    }
                }
                else
                {
                    this.TotalPreviousSelectedActiveFile = 1;
                    this.SelectedActiveMacroFiles.Clear();
                }
                

                if (removeSettingFile)
                {
                    this.CurrentSettingsTable = null;
                    this.TotalPreviousSelectedActiveFile = this.SelectedActiveMacroFiles.Count;                    
                }
                else
                {
                    this.selectedMacroFile = value == null ? value : value + ".RABA";
                    this.TotalPreviousSelectedActiveFile = this.SelectedActiveMacroFiles.Count;
                    this.OnPropertyChanged($"IsSelectMacroFile");
                    this.OnPropertyChanged($"IsSelectDisabledMacroFile");
                    this.OnPropertyChanged();
                    if (!string.IsNullOrEmpty(this.selectedMacroFile))
                    {
                        this.HandleSelectedMacroFile();
                    }
                }                
            }
        }

        public int Action
        {
            get => this.action;
            set
            {
                QuickSaveVisibility = "Visible";
                action = value;
            }
        }
        public string Command
        {
            get => this.command;
            set
            {
                QuickSaveVisibility = "Visible";
                command = value;
            }
        }
        public string DatabaseName
        {
            get => this.databaseName;
            set
            {
                QuickSaveVisibility = "Visible";
                databaseName = value;
            }
        }
        public string DatabaseServer
        {
            get => this.databaseServer;
            set
            {
                QuickSaveVisibility = "Visible";
                databaseServer = value;
            }
        }
        public bool IncludeSubFolders
        {
            get => this.includeSubFolders;
            set
            {
                QuickSaveVisibility = "Visible";
                includeSubFolders = value;
            }
        }
        public string IntegratedSecurity
        {
            get => this.integratedSecurity;
            set
            {
                QuickSaveVisibility = "Visible";
                integratedSecurity = value;
            }
        }
        public string RunSQLScript
        {
            get => this.runSQLScript;
            set
            {
                QuickSaveVisibility = "Visible";
                runSQLScript = value;
            }
        }
        public string RunSQLScriptFilePath
        {
            get => this.runSQLScriptFilePath;
            set
            {
                QuickSaveVisibility = "Visible";
                runSQLScriptFilePath = value;
            }
        }
        public string ScanFileExtension
        {
            get => this.scanFileExtension;
            set
            {
                QuickSaveVisibility = "Visible";
                scanFileExtension = value;
            }
        }
        public string ScanFilePrefix
        {
            get => this.scanFilePrefix;
            set
            {
                QuickSaveVisibility = "Visible";
                scanFilePrefix = value;
            }
        }
        public string ScanFileSizeGreaterThan
        {
            get => this.scanFileSizeGreaterThan;
            set
            {
                QuickSaveVisibility = "Visible";
                scanFileSizeGreaterThan = value;
            }
        }
        public string ScanFileSizeLessThan
        {
            get => this.scanFileSizeLessThan;
            set
            {
                QuickSaveVisibility = "Visible";
                scanFileSizeLessThan = value;
            }
        }
        public string ScanLocation
        {
            get => this.scanLocation;
            set
            {
                QuickSaveVisibility = "Visible";
                scanLocation = value;
            }
        }
        public string TargetLocation
        {
            get => this.targetLocation;
            set
            {
                QuickSaveVisibility = "Visible";
                targetLocation = value;
            }
        }
        public bool ConditionalDelete
        {
            get => this.conditionalDelete;
            set
            {
                QuickSaveVisibility = "Visible";
                conditionalDelete = value;
            }
        }

        public string SettingsFolderService { get; set; }
        private DelegateCommand AddActionDelegateCommand { get; }

        private DelegateCommand AddMacroDelegateCommand { get; }

        private DelegateCommand CopyActionDelegateCommand { get; }

        private DelegateCommand DeleteActionDelegateCommand { get; }

        private DelegateCommand DisableMacroDelegateCommand { get; }
        private DelegateCommand RenameMacroDelegateCommand { get; }

        private DelegateCommand EditActionDelegateCommand { get; }
        private DelegateCommand QuickEditActionDelegateCommand { get; }

        private DelegateCommand EnableMacroDelegateCommand { get; }
        private DelegateCommand QuickSaveMacroDelegateCommand { get; }
        private DelegateCommand CancelActionDelegateCommand { get; }
        private DelegateCommand ToggerMenuDelegateCommand { get; }

        private DelegateCommand PurgeMacroDelegateCommand { get; }
        private DelegateCommand DoubleClickDelegateCommand { get; }

        private static void PopulateSettingsFiles(ObservableCollection<string> macroFiles, string szFolder, string settingsExtension)
        {
            var szSettingExtension = settingsExtension;
            var sfiles = Directory.GetFiles(szFolder);

            foreach (var sFileName in sfiles)
            {
                if (sFileName.EndsWith(szSettingExtension))
                {
                    macroFiles.Add(Path.GetFileNameWithoutExtension(sFileName).Replace(".RABA", string.Empty));
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

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("Dependent"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("Dependent");
                }

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("ConditionalRun"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("ConditionalRun");
                }

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("ConditionalDelete"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("ConditionalDelete");
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
                row["Dependent"] = addMacroActionForm.SavedActionDetails.Dependent;
                row["ConditionalRun"] = addMacroActionForm.SavedActionDetails.ConditionalRun;
                row["ConditionalDelete"] = addMacroActionForm.SavedActionDetails.ConditionalDelete;

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

            if(szSettingsFileNew.Length > 255)
            {
                MessageBox.Show("File name is too long. Should be less than 255 chracters");
                return;
            }            

            var dataSet = new DataSet("Tasks");
            var dataTable = new DataTable("tblTaskInfo");
            dataSet.Tables.Add(dataTable);
            dataSet.WriteXml(szSettingsFileNew);

            this.activeMacroFiles.Add(addMacroForm.MacroFileName);
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

            var messageBoxResult = MessageBox.Show("Are you sure to delete this item", "Confirmation", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.No)
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
            var count = SelectedActiveMacroFiles.Count;
            if (count > 1)
            {
                var cloneSelectedActiveMacroFiles = new List<String>();
                foreach (var file in SelectedActiveMacroFiles)
                {
                    cloneSelectedActiveMacroFiles.Add(file);
                }

                foreach (var selectedMacroFile in cloneSelectedActiveMacroFiles)
                {
                    var szFileName = this.SettingsFolderService + "\\" + selectedMacroFile + ".RABA";
                    File.Move(szFileName, szFileName + ".DISABLED");
                    this.inactiveMacroFiles.Add(selectedMacroFile);
                    this.activeMacroFiles.Remove(selectedMacroFile);
                    this.CurrentSettingsTable = null;
                }

                this.SelectedMacroFile = null;
                this.totalPreviousSelectedActiveFile = 0;
            }
            else
            {
                if (string.IsNullOrEmpty(this.SelectedMacroFile))
                {
                    MessageBox.Show("Please select file", "Disable File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var szFileName = this.SettingsFolderService + "\\" + this.SelectedMacroFile;
                File.Move(szFileName, szFileName + ".DISABLED");
                this.inactiveMacroFiles.Add(this.SelectedMacroFile.Replace(".RABA", string.Empty));
                this.activeMacroFiles.Remove(this.SelectedMacroFile.Replace(".RABA", string.Empty));
                this.CurrentSettingsTable = null;
            }            
        }

        private void RenameMacroFile()
        {
            var addMacroForm = new JobFileName(this.selectedMacroFile.Replace(".RABA", string.Empty));
            addMacroForm.ShowDialog();

            if (!addMacroForm.JobFileNameSaved)
            {
                return;
            }           

            // if the file name is changed 
            if (!String.Equals(this.selectedMacroFile, addMacroForm.MacroFileName, StringComparison.CurrentCultureIgnoreCase))
            {
                var oldPath = Path.Combine(this.SettingsFolderService, selectedMacroFile);
                var newPath = Path.Combine(this.SettingsFolderService, addMacroForm.MacroFileName + ".RABA");

                if (newPath.Length > 255)
                {
                    MessageBox.Show("File name is too long. Should be less than 255 chracters");
                    return;
                }

                // if renamed file already exists and not just changing case 
                if (File.Exists(newPath))
                {
                    MessageBox.Show(String.Format("File already exists:\n{0}", newPath));
                    return;
                }
                else
                {
                    this.activeMacroFiles.Remove(SelectedMacroFile.Replace(".RABA", string.Empty));
                    this.activeMacroFiles.Add(addMacroForm.MacroFileName.Replace(".RABA", string.Empty));                    
                    Directory.Move(oldPath, newPath);
                    SelectedMacroFile = addMacroForm.MacroFileName;
                }
            }
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
                oDtReturn.Columns.Add("Dependent");
                oDtReturn.Columns.Add("ConditionalRun");
                oDtReturn.Columns.Add("ConditionalDelete");
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

            this.colWidth = new DataGridLength(5);            

            using (var dataSet = this.TransformSettingsToDataSet(this.currentSettings))
            {
                var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                SaveSettings(pathFile, dataSet);
            }
        }

        private void QuickEditMacroAction()
        {
            if (this.selectedItem == null)
            {
                MessageBox.Show("Please select macro action", "Edit Macro Action", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.QuickSaveVisibility = "Visible";
        }

        private void EnableSelectedMacro()
        {
            var count = SelectedDisabledMacroFiles.Count;
            if (count > 1)
            {
                var cloneSelectedDisabledMacroFiles = new List<String>();
                foreach (var file in SelectedDisabledMacroFiles)
                {
                    cloneSelectedDisabledMacroFiles.Add(file);
                }

                foreach (var selectedMacroFile in cloneSelectedDisabledMacroFiles)
                {
                    var szFileName = string.Empty;
                    szFileName = this.SettingsFolderService + "\\" + selectedMacroFile + ".RABA.DISABLED";
                    this.SettingsFileEnable(szFileName);
                    this.activeMacroFiles.Add(selectedMacroFile.Replace(".RABA.DISABLED", string.Empty));
                    this.inactiveMacroFiles.Remove(selectedMacroFile.Replace(".RABA.DISABLED", string.Empty));
                }                
            }
            else
            {
                if (string.IsNullOrEmpty(this.selectedDisabledMacroFile))
                {
                    MessageBox.Show("Please select file", "Enable File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var szFileName = string.Empty;
                szFileName = this.SettingsFolderService + "\\" + this.selectedDisabledMacroFile;
                this.SettingsFileEnable(szFileName);
                this.activeMacroFiles.Add(this.selectedDisabledMacroFile.Replace(".RABA.DISABLED", string.Empty));
                this.inactiveMacroFiles.Remove(this.selectedDisabledMacroFile.Replace(".RABA.DISABLED", string.Empty));
            }            
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
                ScanFileDateGreaterThan = row["ScanFileDateGreaterThan"] == DBNull.Value ? string.Empty : (string)row["ScanFileDateGreaterThan"],
                ScanFileDateLessThan = row["ScanFileDateLessThan"] == DBNull.Value ? string.Empty : (string)row["ScanFileDateLessThan"],
                ScanFileUseRelativeAgeYounger = row["ScanFileUseRelativeAgeYounger"] == DBNull.Value ? string.Empty : (string)row["ScanFileUseRelativeAgeYounger"],
                ScanFileAgeYounger = row["ScanFileAgeYounger"] == DBNull.Value ? string.Empty : (string)row["ScanFileAgeYounger"] == "-1" ? string.Empty : (string)row["ScanFileAgeYounger"],
                ScanFileUseRelativeAgeOlder = row["ScanFileUseRelativeAgeOlder"] == DBNull.Value ? string.Empty : (string)row["ScanFileUseRelativeAgeOlder"],
                ScanFileAgeOlder = row["ScanFileAgeOlder"] == DBNull.Value ? string.Empty : (string)row["ScanFileAgeOlder"] == "-1" ? string.Empty : (string)row["ScanFileAgeOlder"],
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
            if (row.Table.Columns.Contains("Dependent"))
            {
                settings.Dependent = row["Dependent"] == DBNull.Value ? string.Empty : (string)row["Dependent"];
            }
            if (row.Table.Columns.Contains("ConditionalRun"))
            {
                settings.ConditionalRun = row["ConditionalRun"] == DBNull.Value ? string.Empty : (string)row["ConditionalRun"];
            }
            if (row.Table.Columns.Contains("ConditionalDelete"))
            {
                settings.ConditionalDelete = row["ConditionalDelete"] == DBNull.Value ? string.Empty : (string)row["ConditionalDelete"];
            }
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
            var warningMessage = "Are You Sure You Wish To Permenantly Purge This Macro!";
            if (SelectedDisabledMacroFiles.Count > 1)
            {
                warningMessage = "Are You Sure You Wish To Permenantly Purge Selected Macros!";
            }

            var result = MessageBox.Show(warningMessage, "caption", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if(SelectedDisabledMacroFiles.Count > 1)
            {
                var cloneSelectedDisabledMacroFiles = new List<String>();
                foreach (var file in SelectedDisabledMacroFiles)
                {
                    cloneSelectedDisabledMacroFiles.Add(file);
                }

                foreach (var selectedMacroFile in cloneSelectedDisabledMacroFiles)
                {
                    var filePath = this.SettingsFolderService + "\\" + selectedMacroFile + ".RABA.DISABLED";
                    if (!File.Exists(filePath))
                    {
                        return;
                    }

                    this.MacroFilesInActive.Remove(selectedMacroFile);
                    File.Delete(filePath);
                }
            }
            else
            {
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

                this.MacroFilesInActive.Remove(this.selectedDisabledMacroFile.Replace(".RABA.DISABLED", String.Empty));
                File.Delete(filePath);
            }            
        }

        private void SettingsFileEnable(string settingsFile)
        {
            File.Move(settingsFile, settingsFile.Substring(0, settingsFile.Length - 9));
        }

        private DataSet TransformSettingsToDataSet(ObservableCollection<Setting> macroActions)
        {
            var tempSettingTable = new DataSet("Tasks");

            tempSettingTable.Tables.Add(this.DsFunctionCreateDataTableTasks());

            foreach (var setting in this.currentSettings)
            {
                var row = tempSettingTable.Tables["tblTaskInfo"].NewRow();

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("Dependent"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("Dependent");
                }

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("ConditionalRun"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("ConditionalRun");
                }

                if (!tempSettingTable.Tables["tblTaskInfo"].Columns.Contains("ConditionalDelete"))
                {
                    tempSettingTable.Tables["tblTaskInfo"].Columns.Add("ConditionalDelete");
                }

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
                row["Dependent"] = setting.Dependent;
                row["ConditionalRun"] = setting.ConditionalRun;
                row["ConditionalDelete"] = setting.ConditionalDelete;
                tempSettingTable.Tables["tblTaskInfo"].Rows.Add(row);
            }

            return tempSettingTable;
        }

        private void QuickSaveAction()
        {
            var selectedIndex = this.currentSettings.IndexOf(this.selectedItem);
            var cloneSelectItem = selectedItem;

            cloneSelectItem.Action = GetAction(Action);
            cloneSelectItem.ScanLocation = this.ScanLocation;
            cloneSelectItem.IncludeSubFolders = this.IncludeSubFolders == true ? "True" : "False";
            cloneSelectItem.ScanFileExtension = this.ScanFileExtension;
            cloneSelectItem.ScanFilePrefix = this.ScanFilePrefix;
            cloneSelectItem.DatabaseName = this.DatabaseName;
            cloneSelectItem.DatabaseServer = this.DatabaseServer;
            cloneSelectItem.IntegratedSecurity = this.IntegratedSecurity;
            cloneSelectItem.RunSQLScript = this.RunSQLScript;
            cloneSelectItem.RunSQLScriptFilePath = this.RunSQLScriptFilePath;
            cloneSelectItem.ScanFileSizeGreaterThan = this.ScanFileSizeGreaterThan;
            cloneSelectItem.ScanFileSizeLessThan = this.ScanFileSizeLessThan;
            cloneSelectItem.ConditionalDelete = this.ConditionalDelete == true ? "True" : "False";
            cloneSelectItem.TargetLocation = this.TargetLocation;

            this.currentSettings[selectedIndex] = cloneSelectItem;

            using (var dataSet = this.TransformSettingsToDataSet(this.currentSettings))
            {
                var pathFile = this.SettingsFolderService + "\\" + this.selectedMacroFile;
                SaveSettings(pathFile, dataSet);
            }

            CollectionViewSource.GetDefaultView(currentSettings).Refresh();
            selectedItem = cloneSelectItem;
            this.QuickSaveVisibility = "Hidden";
        }

        private void CancelAction()
        {
            this.QuickSaveVisibility = "Hidden";
        }

        private void ToggerMenu()
        {
            ShowHideManageRabaFile = showHideRabaFile.Equals("Collapsed") ? "Visible" : "Collapsed";
        }        
    }
}