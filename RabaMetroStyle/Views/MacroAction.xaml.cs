﻿using MahApps.Metro.Controls;
using RabaMetroStyle.Models;
using System;
using System.Web.UI.WebControls;
using System.Windows;

namespace RabaMetroStyle.Views
{
    /// <summary>
    /// Interaction logic for MacroAction.xaml
    /// </summary>
    public partial class MacroAction : MetroWindow
    {
        public bool SavedAction = false;
        public Setting SavedActionDetails;

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
                    ddlAction.SelectedIndex = this.ddlAction.Items.IndexOf(ddlActionItem);
                    break;
                }
            }

            this.chkAfterActionDelete.IsChecked = actionDetail.ActionCompleteDelete == "True" ? true : false;
            this.chkAfterActionRename.IsChecked = actionDetail.ActionCompleteRename == "True" ? true : false;
            this.chkAfterActionTimeStamp.IsChecked = actionDetail.ActionCompleteTimeStamp == "True" ? true : false;
            this.txtRestoreDatabase.Text = actionDetail.DatabaseName;
            this.txtRestoreServer.Text = actionDetail.DatabaseServer;
            this.chkIntegratedSecurity.IsChecked = actionDetail.IntegratedSecurity == "True" ? true : false;
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

            if (!string.IsNullOrEmpty(actionDetail.ScanFileAgeYounger))
            {
                if (actionDetail.ScanFileAgeYounger != "-1")
                {
                    this.nmrDaysOldAgeYouger.Value = Double.Parse(actionDetail.ScanFileAgeYounger);
                }
                this.chkRelativeAgeYougerThan.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(actionDetail.ScanFileAgeOlder))
            {
                if (actionDetail.ScanFileAgeOlder != "-1")
                {
                    this.nmrDaysOldAgeOlder.Value = Double.Parse(actionDetail.ScanFileAgeOlder);
                }
                this.chkRelativeAgeOlderThan.IsChecked = true;
            }

            this.chlOnlyCountWeekdays.IsChecked = actionDetail.OnlyCountWeekDays == "True" ? true : false;
            this.txtScanExtension.Text = actionDetail.ScanFileExtension;
            this.txtScanPrefix.Text = actionDetail.ScanFilePrefix;
            this.txtSizeFrom.Text = actionDetail.ScanFileSizeGreaterThan;
            this.txtSizeTo.Text = actionDetail.ScanFileSizeLessThan;
            this.txtScanLocation.Text = actionDetail.ScanLocation;
            this.txtTargetLocation.Text = actionDetail.TargetLocation;
            this.txtTaskOrder.Text = actionDetail.TaskOrder;
            this.txtUserID.Text = actionDetail.UserID;
            this.txtRestoreFileTemplate.Text = actionDetail.RestoreDatabaseFileGroups;
            this.chkIncludeSubFolders.IsChecked = actionDetail.IncludeSubFolders == "True" ? true : false;
            this.chkMaintainSubFolderStructure.IsChecked = actionDetail.MaintainSubFolders == "True" ? true : false;
            this.txtBatchFile.Text = actionDetail.Command;
        }

        private void BtnBatchFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtBatchFile.Text = dialog.SelectedPath;
            }
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnRestoreFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtRestoreFileTemplate.Text = dialog.SelectedPath;
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
                this.SavedActionDetails.ScanFileDateGreaterThan = this.dtDateFrom.SelectedDateTime == null ? "1/1/1970" : this.dtDateFrom.SelectedDateTime.Value.ToString();
                this.SavedActionDetails.ScanFileDateLessThan = this.dtDateTo.SelectedDateTime == null ? "1/1/2070" : this.dtDateTo.SelectedDateTime.Value.ToString();

                if (this.chkRelativeAgeYougerThan.IsChecked == true)
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeYounger = "True";
                    this.SavedActionDetails.ScanFileAgeYounger = this.nmrDaysOldAgeYouger.Value?.ToString();
                }
                else
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeYounger = "False";
                    this.SavedActionDetails.ScanFileAgeYounger = "-1";
                }

                if (this.chkRelativeAgeOlderThan.IsChecked == true)
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeOlder = "True";
                    this.SavedActionDetails.ScanFileAgeOlder = this.nmrDaysOldAgeOlder.Value?.ToString();
                }
                else
                {
                    this.SavedActionDetails.ScanFileUseRelativeAgeOlder = "False";
                    this.SavedActionDetails.ScanFileAgeOlder = "-1";
                }

                this.SavedActionDetails.OnlyCountWeekDays = this.chlOnlyCountWeekdays.IsChecked.Value.ToString();
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

                if (this.txtRestoreFileTemplate.Text.Trim().Length == 0)
                {
                    this.SavedActionDetails.RestoreDatabaseFileGroups = "";
                }
                else
                {
                    this.SavedActionDetails.RestoreDatabaseFileGroups = this.txtRestoreFileTemplate.Text;
                }

                this.SavedActionDetails.IncludeSubFolders = this.chkIncludeSubFolders.IsChecked.Value.ToString();

                this.SavedActionDetails.MaintainSubFolders = this.chkMaintainSubFolderStructure.IsChecked.Value.ToString();

                this.SavedActionDetails.Command = this.txtBatchFile.Text;

                this.SavedAction = true;

                this.Close();
            }
            else
            {
                // Notify User....
                MessageBox.Show("Cannot save action, please check the entered information.",
                                "RABA 2:Information Center:Add Action");
            }
        }

        private void BtnScanFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtScanLocation.Text = dialog.SelectedPath;
            }
        }

        private void BtnTargetLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.txtTargetLocation.Text = dialog.SelectedPath;
            }
        }

        private void DdlAction_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

                case "DELETE":
                    this.chkAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.lblAfterActionDelete.Visibility = Visibility.Collapsed;
                    this.chkAfterActionRename.Visibility = Visibility.Collapsed;
                    this.lblAfterActionRename.Visibility = Visibility.Collapsed;
                    this.chkAfterActionTimeStamp.Visibility = Visibility.Collapsed;
                    this.lblAfterActionTimeStamp.Visibility = Visibility.Collapsed;
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    this.chkMaintainSubFolderStructure.IsChecked = false;
                    this.chkMaintainSubFolderStructure.Visibility = Visibility.Collapsed;

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
            }
        }

        private Boolean EnteredInformationValid()
        {
            Boolean bReturn = true;

            if (this.txtScanLocation.Text.Trim().Length == 0)
            {
                bReturn = false;
                //epFileName.SetError(txtScanLocation, "Invalid Folder!");
            }
            else
            {
                // String... 
                // Now make sure that it's
                //  a valid folder 

                if (System.IO.Directory.Exists(this.txtScanLocation.Text.Trim()))
                {
                    //epFileName.SetError(txtScanLocation, "");
                }
                else
                {
                    //epFileName.SetError(txtScanLocation, "Invalid Folder!");
                    bReturn = false;
                }
            }

            if (this.txtSizeFrom.Text.Trim().Length == 0 || this.txtSizeTo.Text.Trim().Length == 0)
            {
                //epFileName.SetError(txtSizeFrom, "");
                //epFileName.SetError(txtSizeTo, "");
            }
            else
            {
                if (Convert.ToInt64(this.txtSizeFrom.Text) > Convert.ToInt64(this.txtSizeTo.Text))
                {
                    //epFileName.SetError(txtSizeFrom, "From Size Cannot Be Larger Than The Size To!");
                    //epFileName.SetError(txtSizeTo, "To Size Cannot Be Smaller Than The From Size!");
                    bReturn = false;
                }
                else
                {
                    //epFileName.SetError(txtSizeFrom, "");
                    //epFileName.SetError(txtSizeTo, "");
                }
            }

            if (this.ddlAction.SelectedValue.ToString().ToUpper() == "RESTORE")
            {
                if (this.txtRestoreDatabase.Text.Trim().Length == 0)
                {
                    // No Database Name 
                    //epFileName.SetError(txtRestoreDatabase, "Enter A Database Name!");
                    bReturn = false;
                }
                else
                {
                    //epFileName.SetError(txtRestoreDatabase, "");
                }

                if (this.txtRestoreServer.Text.Trim().Length == 0)
                {
                    // No Database Name 
                    //epFileName.SetError(txtRestoreServer, "Enter A Server Name!");
                    bReturn = false;
                }
                else
                {
                    //epFileName.SetError(txtRestoreServer, "");
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
            this.ddlAction.SelectedIndex = 0;
        }
    }
}