using Ookii.Dialogs.Wpf;
using RabaMetroStyle.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;

namespace RabaMetroStyle.Views
{
    /// <summary>
    /// Interaction logic for ActionDetail.xaml
    /// </summary>
    public partial class ActionDetail : UserControl
    {
        public bool SavedAction;
        public Setting SavedActionDetails;
        public List<string> Errors;
        public ActionDetail()
        {
            InitializeComponent();
            this.PopulateDropDownListItem();
            this.ActionDetailGrid.Visibility = Visibility.Visible;
            this.NoActionSelected.Visibility = Visibility.Hidden;
        }

        private void BtnBatchFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = this.lblBatchFile.Content.ToString().Contains("Batch") ? Properties.Resources.MacroAction_BtnBatchFile_BatchFiles : Properties.Resources.MacroAction_BtnRestoreFile_SQLScript;

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.txtBatchFile.Text = dialog.FileName;
            }
        }

        private void BtnRestoreFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = Properties.Resources.MacroAction_BtnRestoreFile_SQLScript;
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.txtRestoreFileTemplate.Text = dialog.FileName;
            }
        }

        private void BtnScanFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.txtScanLocation.Text = dialog.SelectedPath;
            }
        }

        private void BtnTargetLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.txtTargetLocation.Text = dialog.SelectedPath;
            }
        }
        private void DdlAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.ddlAction.SelectedValue.ToString().ToUpper())
            {
                case "COPY":
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;

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

                    if (this.chkConditonalDelete.IsChecked == true)
                    {
                        this.txtTargetLocation.Visibility = Visibility.Visible;
                        this.btnTargetLocation.Visibility = Visibility.Visible;
                        this.lblTargetLocation.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.txtTargetLocation.Visibility = Visibility.Collapsed;
                        this.btnTargetLocation.Visibility = Visibility.Collapsed;
                        this.lblTargetLocation.Visibility = Visibility.Collapsed;
                    }

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Collapsed;
                    this.btnTargetLocation.Visibility = Visibility.Collapsed;
                    this.lblTargetLocation.Visibility = Visibility.Collapsed;

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
                    this.txtTargetLocation.Text = "";
                    this.txtTargetLocation.Visibility = Visibility.Visible;
                    this.btnTargetLocation.Visibility = Visibility.Visible;
                    this.lblTargetLocation.Visibility = Visibility.Visible;

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
    }
}
