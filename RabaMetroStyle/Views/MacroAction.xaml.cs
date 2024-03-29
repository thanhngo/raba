﻿#region

using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using RabaMetroStyle.CustomControl;
using RabaMetroStyle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
#endregion

namespace RabaMetroStyle.Views
{
    /// <summary>
    ///     Interaction logic for MacroAction.xaml
    /// </summary>
    public partial class MacroAction : MetroWindow
    {
        public bool SavedAction;
        public Setting SavedActionDetails;
        public List<string> Errors;        

        public MacroAction()
        {
            this.InitializeComponent();
            this.PopulateDropDownListItem();
            this.SavedActionDetails = new Setting();
        }

        public MacroAction(Setting actionDetail)
        {
            this.InitializeComponent();
            this.PopulateDropDownListItem();
            this.SavedActionDetails = new Setting();

            foreach (var ddlActionItem in this.ddlAction.Items)
            {
                if (ddlActionItem.ToString() == actionDetail.Action)
                {
                    this.ddlAction.SelectedIndex = this.ddlAction.Items.IndexOf(ddlActionItem);
                    break;
                }
            }

            this.chkAfterActionDelete.IsChecked = actionDetail.ActionCompleteDelete.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.chkAfterActionRename.IsChecked = actionDetail.ActionCompleteRename.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.chkAfterActionTimeStamp.IsChecked = actionDetail.ActionCompleteTimeStamp.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.txtRestoreDatabase.Text = actionDetail.DatabaseName;
            this.txtRestoreServer.Text = actionDetail.DatabaseServer;
            this.chkIntegratedSecurity.IsChecked = actionDetail.IntegratedSecurity.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.txtPassword.Text = actionDetail.Password;
            this.txtRestoreFileTemplate.Text = actionDetail.RestoreDatabaseFileGroups;
            this.txtBatchFile.Text = actionDetail.RunSQLScriptFilePath;

            if (!string.IsNullOrEmpty(actionDetail.ScanFileDateGreaterThan))
            {
                this.dtDateFrom.SelectedDateTime = DateTime.Parse(actionDetail.ScanFileDateGreaterThan);
            }

            if (!string.IsNullOrEmpty(actionDetail.ScanFileDateLessThan))
            {
                this.dtDateTo.SelectedDateTime = DateTime.Parse(actionDetail.ScanFileDateLessThan);
            }

            this.chkOnlyCountWeekdays.IsChecked = actionDetail.OnlyCountWeekDays.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            if (!string.IsNullOrEmpty(actionDetail.ScanFileDateGreaterThan) && !string.IsNullOrEmpty(actionDetail.ScanFileDateLessThan))
            {
                this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.chkOnlyCountWeekdays.IsEnabled = false;
                this.chkOnlyCountWeekdays.IsChecked = false;
            }            

            if (!string.IsNullOrEmpty(actionDetail.ScanFileAgeYounger) && (actionDetail.ScanFileAgeYounger != "-1"))
            {
                this.nmrDaysOldAgeYouger.Value = double.Parse(actionDetail.ScanFileAgeYounger);

                this.chkRelativeAgeYougerThan.IsChecked = true;

                this.dtDateFrom.IsEnabled = false;
                this.dtDateTo.IsEnabled = false;
                this.dtDateFrom.SelectedDateTime = null;
                this.dtDateTo.SelectedDateTime = null;

                this.nmrDaysOldAgeYouger.IsEnabled = true;                
                //this.chkRelativeAgeOlderThan.IsEnabled = false;
                //this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.nmrDaysOldAgeYouger.Value = null;
                this.nmrDaysOldAgeYouger.IsEnabled = false;
                this.nmrDaysOldAgeOlder.IsEnabled = false;
                //this.chkOnlyCountWeekdays.IsEnabled = false;
                //this.chkRelativeAgeOlderThan.IsEnabled = true;                               
            }


            if (!string.IsNullOrEmpty(actionDetail.ScanFileAgeOlder) && (actionDetail.ScanFileAgeOlder != "-1"))
            {
                this.nmrDaysOldAgeOlder.IsEnabled = true;
                this.nmrDaysOldAgeOlder.Value = double.Parse(actionDetail.ScanFileAgeOlder);

                this.chkRelativeAgeOlderThan.IsChecked = true;

                this.dtDateFrom.IsEnabled = false;
                this.dtDateTo.IsEnabled = false;
                this.dtDateFrom.SelectedDateTime = null;
                this.dtDateTo.SelectedDateTime = null;

                this.nmrDaysOldAgeOlder.IsEnabled = true;
                //this.chkRelativeAgeYougerThan.IsEnabled = false;
                //this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.dtDateFrom.IsEnabled = true;
                this.dtDateTo.IsEnabled = true;

                //this.nmrDaysOldAgeOlder.Value = null;
                //this.nmrDaysOldAgeOlder.IsEnabled = false;
                //this.chkOnlyCountWeekdays.IsEnabled = false;
                //this.chkRelativeAgeYougerThan.IsEnabled = true;
            }

            this.txtScanExtension.Text = actionDetail.ScanFileExtension;
            this.txtScanPrefix.Text = actionDetail.ScanFilePrefix;
            this.txtSizeFrom.Text = actionDetail.ScanFileSizeGreaterThan == "0" ? string.Empty : actionDetail.ScanFileSizeGreaterThan;
            this.txtSizeTo.Text = actionDetail.ScanFileSizeLessThan == "0" ? string.Empty : actionDetail.ScanFileSizeLessThan;
            this.txtScanLocation.Text = actionDetail.ScanLocation;
            this.txtTargetLocation.Text = actionDetail.TargetLocation;
            this.txtTaskOrder.Text = actionDetail.TaskOrder;
            this.txtUserID.Text = actionDetail.UserID;
            this.txtRestoreFileTemplate.Text = actionDetail.RestoreDatabaseFileGroups;
            this.chkIncludeSubFolders.IsChecked = actionDetail.IncludeSubFolders.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.chkMaintainSubFolderStructure.IsChecked = actionDetail.MaintainSubFolders.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.txtBatchFile.Text = actionDetail.Command;
            this.chkConditonalDelete.IsChecked = actionDetail.ConditionalDelete.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            this.chkConditionalRun.IsChecked = !string.IsNullOrEmpty(actionDetail.ConditionalRun) && actionDetail.ConditionalRun.Equals("R");
            this.chkDependent.IsChecked = !string.IsNullOrEmpty(actionDetail.Dependent) && actionDetail.Dependent.Equals("D");
        }

        private void BtnBatchFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = this.lblBatchFile.Content.ToString().Contains("Batch") ? Properties.Resources.MacroAction_BtnBatchFile_BatchFiles : Properties.Resources.MacroAction_BtnRestoreFile_SQLScript;

            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtBatchFile.Text = dialog.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnRestoreFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = Properties.Resources.MacroAction_BtnRestoreFile_SQLScript;
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtRestoreFileTemplate.Text = dialog.FileName;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.EnteredInformationValid())
            {
                this.SavedActionDetails.Action = Convert.ToString(this.ddlAction.SelectedValue);
                this.SavedActionDetails.ActionCompleteDelete = this.chkAfterActionDelete.IsChecked.ToString();
                this.SavedActionDetails.ActionCompleteRename = this.chkAfterActionRename.IsChecked.ToString();
                this.SavedActionDetails.ActionCompleteTimeStamp = this.chkAfterActionTimeStamp.IsChecked.ToString();
                this.SavedActionDetails.DatabaseName = this.txtRestoreDatabase.Text;
                this.SavedActionDetails.DatabaseServer = this.txtRestoreServer.Text;
                this.SavedActionDetails.IntegratedSecurity = this.chkIntegratedSecurity.IsChecked.ToString();
                this.SavedActionDetails.Password = this.txtPassword.Text;
                this.SavedActionDetails.RestoreDatabaseFileGroups = this.txtRestoreFileTemplate.Text;
                this.SavedActionDetails.RunSQLScript = "False";
                this.SavedActionDetails.RunSQLScriptFilePath = this.txtBatchFile.Text;
                this.SavedActionDetails.ScanFileDateGreaterThan = this.dtDateFrom.SelectedDateTime == null ? string.Empty : this.dtDateFrom.SelectedDateTime.Value.ToString();
                this.SavedActionDetails.ScanFileDateLessThan = this.dtDateTo.SelectedDateTime == null ? string.Empty : this.dtDateTo.SelectedDateTime.Value.ToString();

                if (this.chkRelativeAgeYougerThan.IsChecked == true)
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeYounger = "True";
                    this.SavedActionDetails.ScanFileAgeYounger = this.nmrDaysOldAgeYouger.Value?.ToString();
                }
                else
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeYounger = "False";
                    this.SavedActionDetails.ScanFileAgeYounger = string.Empty;
                }

                if (this.chkRelativeAgeOlderThan.IsChecked == true)
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeOlder = "True";
                    this.SavedActionDetails.ScanFileAgeOlder = this.nmrDaysOldAgeOlder.Value?.ToString();
                }
                else
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeOlder = "False";
                    this.SavedActionDetails.ScanFileAgeOlder = string.Empty;
                }

                var isChecked = this.chkOnlyCountWeekdays.IsChecked;
                this.SavedActionDetails.OnlyCountWeekDays = ((isChecked != null) && isChecked.Value).ToString();
                this.SavedActionDetails.ScanFileExtension = this.txtScanExtension.Text;
                this.SavedActionDetails.ScanFilePrefix = this.txtScanPrefix.Text;
                if (this.txtSizeFrom.Text.Trim().Length == 0)
                {
                    this.txtSizeFrom.Text = "0";
                }

                this.SavedActionDetails.ScanFileSizeGreaterThan = this.txtSizeFrom.Text;

                if (this.txtSizeTo.Text.Trim().Length == 0)
                {
                    this.txtSizeTo.Text = "0";
                }

                this.SavedActionDetails.ScanFileSizeLessThan = this.txtSizeTo.Text;
                this.SavedActionDetails.ScanLocation = this.txtScanLocation.Text;
                this.SavedActionDetails.TargetLocation = this.txtTargetLocation.Text;

                if (this.txtTaskOrder.Text.Trim().Length == 0)
                {
                    this.txtTaskOrder.Text = "0";
                }

                this.SavedActionDetails.TaskOrder = this.txtTaskOrder.Text;
                this.SavedActionDetails.UserID = this.txtUserID.Text;

                this.SavedActionDetails.RestoreDatabaseFileGroups = this.txtRestoreFileTemplate.Text.Trim().Length == 0 ? "" : this.txtRestoreFileTemplate.Text;

                var @checked = this.chkIncludeSubFolders.IsChecked;
                this.SavedActionDetails.IncludeSubFolders = ((@checked != null) && @checked.Value).ToString();

                this.SavedActionDetails.MaintainSubFolders = this.chkMaintainSubFolderStructure.IsChecked.Value.ToString();

                this.SavedActionDetails.Command = this.txtBatchFile.Text;

                if (this.chkConditionalRun.IsChecked == true)
                {
                    this.SavedActionDetails.ConditionalRun = "R";
                }
                else
                {
                    this.SavedActionDetails.ConditionalRun = string.Empty;
                }

                if (this.chkDependent.IsChecked == true)
                {
                    this.SavedActionDetails.Dependent = "D";
                }
                else
                {
                    this.SavedActionDetails.Dependent = "C";
                }
                if (this.chkConditonalDelete.IsChecked == true)
                {
                    this.SavedActionDetails.ConditionalDelete = "true";
                }
                else
                {
                    this.SavedActionDetails.ConditionalDelete = "false";
                }


                this.SavedAction = true;

                this.Close();
            }
            else
            {
                var errorMessage = string.Join(",\n", Errors);
                MessageBox.Show("Cannot save action, please check the entered information.\n" + errorMessage,
                                "RABA: Information Center:Add Action");
            }
        }

        private void BtnScanFolder_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new VistaFolderBrowserDialog();
            //if (dialog.ShowDialog(this).GetValueOrDefault())
            //{
            //    this.txtScanLocation.Text = dialog.SelectedPath;
            //}

            var dialog = new FileFolderDialog();
            var result = dialog.ShowDialog();
            this.txtScanLocation.Text = dialog.SelectedPath;
        }

        private void BtnTargetLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FileFolderDialog();
            if(this.ddlAction.SelectedValue.ToString().ToUpper() == "UNZIP")
            {
                dialog.Dialog.Filter = "zip File (*.zip)|*.zip";
            }

            var result = dialog.ShowDialog();
            this.txtTargetLocation.Text = dialog.SelectedPath;
        }

        private void chkRelativeAgeYougerThan_Checked(object sender, EventArgs e)
        {
            if (this.chkRelativeAgeYougerThan.IsChecked == true)
            {
                this.dtDateFrom.IsEnabled = false;
                this.dtDateTo.IsEnabled = false;
                this.dtDateFrom.SelectedDateTime = null;
                this.dtDateTo.SelectedDateTime = null;


                this.nmrDaysOldAgeYouger.IsEnabled = true;
                this.nmrDaysOldAgeOlder.IsEnabled = false;
                //this.chkRelativeAgeOlderThan.IsEnabled = false;
                this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.dtDateFrom.IsEnabled = true;
                this.dtDateTo.IsEnabled = true;

                this.nmrDaysOldAgeYouger.Value = null;
                this.nmrDaysOldAgeYouger.IsEnabled = false;
                this.nmrDaysOldAgeOlder.IsEnabled = false;
                this.chkOnlyCountWeekdays.IsEnabled = false;
                //this.chkRelativeAgeOlderThan.IsEnabled = true;
            }
        }

        private void chkRelativeAgeOlderThan_Checked(object sender, EventArgs e)
        {
            if (this.chkRelativeAgeOlderThan.IsChecked == true)
            {
                this.dtDateFrom.IsEnabled = false;
                this.dtDateTo.IsEnabled = false;
                this.dtDateFrom.SelectedDateTime = null;
                this.dtDateTo.SelectedDateTime = null;

                this.nmrDaysOldAgeOlder.IsEnabled = true;                
                //this.chkRelativeAgeYougerThan.IsEnabled = false;
                this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.dtDateFrom.IsEnabled = true;
                this.dtDateTo.IsEnabled = true;

                this.nmrDaysOldAgeOlder.Value = null;
                this.nmrDaysOldAgeOlder.IsEnabled = false;                
                this.chkOnlyCountWeekdays.IsEnabled = false;
                //this.chkRelativeAgeYougerThan.IsEnabled = true;
            }
        }

        private void DdlAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.ddlAction.SelectedValue.ToString().ToUpper())
            {
                case "COPY":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Visible;
                    this.lblMainmainSub.Visibility = Visibility.Visible;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;

                    break;

                case "DELETE":
                    this.chkConditonalDelete.Visibility = Visibility.Visible;
                    this.lblConditonalDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.lblAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.chkAfterActionRename.Visibility = Visibility.Collapsed;
                    this.lblAfterActionRename.Visibility = Visibility.Collapsed;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Collapsed;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Collapsed;

                    if (this.chkConditonalDelete.IsChecked == true)
                    {
                        this.txtTargetLocation.Text = "";
                        this.txtTargetLocation.Visibility = Visibility.Visible;
                        this.btnTargetLocation.Visibility = Visibility.Visible;
                        this.lblTargetLocation.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.txtTargetLocation.Text = "";
                        this.txtTargetLocation.Visibility = Visibility.Collapsed;
                        this.btnTargetLocation.Visibility = Visibility.Collapsed;
                        this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    }
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;
                    this.lblMainmainSub.Visibility = Visibility.Collapsed;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;

                    break;

                case "MOVE":
                    this.chkAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.lblAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.chkAfterActionRename.Visibility = Visibility.Collapsed;
                    this.lblAfterActionRename.Visibility = Visibility.Collapsed;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Collapsed;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Collapsed;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Visible;
                    this.lblMainmainSub.Visibility = Visibility.Visible;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;

                    break;

                case "ZIP":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Visible;
                    this.lblMainmainSub.Visibility = Visibility.Visible;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;

                    break;

                case "UNZIP":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;
                    this.lblMainmainSub.Visibility = Visibility.Collapsed;
                    this.txtScanExtension.Text = ".zip";

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;

                    break;

                case "RESTORE":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;
                    this.lblMainmainSub.Visibility = Visibility.Collapsed;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Collapsed;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Collapsed;
                    this.btnBatchFile.Visibility = Visibility.Collapsed;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Collapsed;
                    break;

                case "BATCH":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;
                    this.lblMainmainSub.Visibility = Visibility.Collapsed;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.lblRestoreDatabase.Visibility = Visibility.Collapsed;
                    this.txtRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreServer.Visibility = Visibility.Collapsed;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.chkConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblConditonalDelete.Visibility = Visibility.Collapsed;
                    this.lblBatchFile.Visibility = Visibility.Visible;
                    this.lblBatchFile.Content = "Batch File";
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Visible;
                    this.btnBatchFile.Visibility = Visibility.Visible;

                    break;

                case "SQLSCRIPT":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;
                    this.lblMainmainSub.Visibility = Visibility.Collapsed;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.lblIntegratedSecurity.Visibility = Visibility.Collapsed;
                    this.txtRestoreDatabase.Visibility = Visibility.Visible;
                    this.lblRestoreDatabase.Visibility = Visibility.Visible;
                    this.txtRestoreServer.Visibility = Visibility.Visible;
                    this.lblRestoreServer.Visibility = Visibility.Visible;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Collapsed;
                    this.btnRestoreFile.Visibility = Visibility.Collapsed;

                    this.lblBatchFile.Visibility = Visibility.Visible;
                    this.lblBatchFile.Content = "SQL Script File";
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Visible;
                    this.btnBatchFile.Visibility = Visibility.Visible;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Visible;

                    break;

                case "RUN":
                    this.chkAfterActionDelete.Visibility = Visibility.Visible;
                    this.lblAfterActionDelete.Visibility = Visibility.Visible;
                    this.chkAfterActionRename.Visibility = Visibility.Visible;
                    this.lblAfterActionRename.Visibility = Visibility.Visible;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Visible;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Visible;
                    this.lblMainmainSub.Visibility = Visibility.Visible;

                    // Database Restore Section 
                    this.chkIntegratedSecurity.Visibility = Visibility.Visible;
                    this.lblIntegratedSecurity.Visibility = Visibility.Visible;
                    this.txtRestoreDatabase.Visibility = Visibility.Visible;
                    this.lblRestoreDatabase.Visibility = Visibility.Visible;
                    this.txtRestoreServer.Visibility = Visibility.Visible;
                    this.lblRestoreServer.Visibility = Visibility.Visible;
                    this.lblRestoreFileTemplate.Visibility = Visibility.Visible;
                    this.txtRestoreFileTemplate.Visibility = Visibility.Visible;
                    this.btnRestoreFile.Visibility = Visibility.Visible;

                    this.lblBatchFile.Visibility = Visibility.Visible;
                    this.txtBatchFile.Text = "";
                    this.txtBatchFile.Visibility = Visibility.Visible;
                    this.btnBatchFile.Visibility = Visibility.Visible;
                    this.grbSecurityAndDatabase.Visibility = Visibility.Visible;
                    break;
            }
        }

        private bool EnteredInformationValid()
        {
            var bReturn = true;
            this.Errors = new List<string>();

            if (this.txtScanLocation.Text.Trim().Length == 0)
            {
                bReturn = false;
                Errors.Add("Scan location: Invalid Folder");
            }
            else
            {
                // String... 
                // Now make sure that it's
                //  a valid folder 

                if (Directory.Exists(this.txtScanLocation.Text.Trim()))
                {
                    //epFileName.SetError(txtScanLocation, "");
                }
                else
                {
                    Errors.Add("Scan location: Invalid Folder");
                    bReturn = false;
                }
            }

            if ((this.txtSizeFrom.Text.Trim().Length == 0) || (this.txtSizeTo.Text.Trim().Length == 0))
            {
                //epFileName.SetError(txtSizeFrom, "");
                //epFileName.SetError(txtSizeTo, "");
            }
            else
            {
                if (Convert.ToInt64(this.txtSizeFrom.Text) > Convert.ToInt64(this.txtSizeTo.Text))
                {
                    Errors.Add("txtSizeFrom: From Size Cannot Be Larger Than The Size To!");
                    //epFileName.SetError(txtSizeFrom, "From Size Cannot Be Larger Than The Size To!");
                    //epFileName.SetError(txtSizeTo, "To Size Cannot Be Smaller Than The From Size!");
                    bReturn = false;
                }
            }

            if (this.ddlAction.SelectedValue.ToString().ToUpper() == "RESTORE")
            {
                if (this.txtRestoreDatabase.Text.Trim().Length == 0)
                {
                    // No Database Name 
                    Errors.Add("txtRestoreDatabase: Enter A Database Name!");
                    //epFileName.SetError(txtRestoreDatabase, "Enter A Database Name!");
                    bReturn = false;
                }

                if (this.txtRestoreServer.Text.Trim().Length == 0)
                {
                    // No Database Name 
                    Errors.Add("txtRestoreServer: Enter A Server Name!");
                    //epFileName.SetError(txtRestoreServer, "Enter A Server Name!");
                    bReturn = false;
                }
            }

            if (this.ddlAction.SelectedValue.ToString().ToUpper() == "DELETE")
            {
                if (this.chkConditonalDelete.IsChecked == true && string.IsNullOrEmpty(this.txtTargetLocation.Text))
                {
                    Errors.Add("Target location is required for Conditional Delete");
                    bReturn = false;
                }
            }

            return bReturn;
        }

        private void PopulateDropDownListItem()
        {
            this.ddlAction.Items.Add(new ListItem("COPY", "0"));
            this.ddlAction.Items.Add(new ListItem("DELETE", "1"));
            this.ddlAction.Items.Add(new ListItem("MOVE", "2"));
            this.ddlAction.Items.Add(new ListItem("ZIP", "3"));
            this.ddlAction.Items.Add(new ListItem("UNZIP", "4"));
            this.ddlAction.Items.Add(new ListItem("BATCH", "5"));
            this.ddlAction.Items.Add(new ListItem("SQLSCRIPT", "6"));
            //this.ddlAction.Items.Add(new ListItem("RUN", "7"));
            this.ddlAction.SelectedIndex = 0;
        }

        private void chkConditonalDelete_Checked(object sender, RoutedEventArgs e)
        {
            this.txtTargetLocation.Visibility = Visibility.Visible;
            this.btnTargetLocation.Visibility = Visibility.Visible;
            this.lblTargetLocation.Visibility = Visibility.Visible;
        }

        private void chkConditonalDelete_UnChecked(object sender, RoutedEventArgs e)
        {
            this.txtTargetLocation.Text = "";
            this.txtTargetLocation.Visibility = Visibility.Collapsed;
            this.btnTargetLocation.Visibility = Visibility.Collapsed;
            this.lblTargetLocation.Visibility = Visibility.Collapsed;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                return;
            }
            e.Handled = true;
        }

        private void dpick_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (this.dtDateFrom.SelectedDateTime != null && this.dtDateTo.SelectedDateTime != null)
            {
                this.chkOnlyCountWeekdays.IsEnabled = true;
            }
            else
            {
                this.chkOnlyCountWeekdays.IsEnabled = false;
                this.chkOnlyCountWeekdays.IsChecked = false;
            }
        }

        private void VerifyTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}