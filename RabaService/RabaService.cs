#region

using ICSharpCode.SharpZipLib.Zip;
using log4net.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;

#endregion

namespace RabaService
{
    public partial class RabaService : ServiceBase, IRabaService
    {
        private bool bInProcess;
        private bool bLoaded = false;
        private bool mbLog = true;
        private bool mbRestoreInProgress = false;
        private bool mbSettingsExist = false;
        private bool mbTransferFileInProgress;
        private bool mbWaitingForHeldFile = false;
        private bool mbZipInProgress = false;
        private int miTimerInterval = 60000;
        private DataSet moDsMapJob = null;
        private string mszDbAccessPath = string.Empty;
        private string mszPathMapJob = string.Empty;
        private Timer mtimer;
        private DataSet oDs = new DataSet();
        private string szCurrentFile = string.Empty;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ActionResult PreviousRun;

        public RabaService()
        {
            this.InitializeComponent();
        }

        public bool ProcessRaba()
        {
            var bReturn = false;

            if (this.mbLog)
            {
                this.WriteToLog("In Process, Log Enabled.", "Application", "RabaService", EventLogEntryType.Information);
            }

            try
            {
                if (this.mbLog)
                {
                    this.WriteToLog("In Process, Settings File Array Initialized.", "Application", "RabaService", EventLogEntryType.Information);
                }

                var arrSettingsFiles = this.GetSettingsFiles();

                if (this.mbLog)
                {
                    this.WriteToLog("In Process, Settings File Array Retrieved.", "Application", "RabaService", EventLogEntryType.Information);
                }

                foreach (string sFileName in arrSettingsFiles)
                {
                    if (this.mbLog)
                    {
                        this.WriteToLog("In Process Looping through Settings Files, Currently Reading :" + sFileName, "Application", "RabaService", EventLogEntryType.Error);
                    }

                    this.ProcessSettingsFile(sFileName);
                }

                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in ProcessRaba:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        public bool ProcessRunBatchFile(string BatchFile, string TargetString, string FileName)
        {
            var bReturn = false;

            try
            {
                TextReader oTextReader = new StreamReader(BatchFile);
                var sBatchFileBody = oTextReader.ReadToEnd();
                oTextReader.Close();
                var batchFileRun = BatchFile + ".RAN." + DateTime.Now.Year.ToString("0000") + "."
                                   + DateTime.Now.Month.ToString("00") + "."
                                   + DateTime.Now.Day.ToString("00") + "."
                                   + DateTime.Now.Hour.ToString("00") + "."
                                   + DateTime.Now.Minute.ToString("00") + "."
                                   + DateTime.Now.Second.ToString("00") + ".BAT";

                var sw = new StreamWriter(batchFileRun);
                sw.Write(sBatchFileBody.Replace(TargetString, FileName));
                sw.Close();

                var proc = Process.Start(batchFileRun);
                proc.WaitForExit(); // Waits for the process to end. 

                bReturn = true;
            }

            catch (Exception ex)
            {
                this.WriteToLog("Issue in Running Batch File File :RunBatchFile:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        public bool ProcessTaskCopy(string SourceFileName,
                                    string ScanLocation,
                                    string TargetFolder,
                                    bool MaintainSubFolders,
                                    bool OverWrite)
        {
            var bReturn = false;
            var szRootName = Path.GetFileName(SourceFileName);

            var szSubFolderName = Path.GetDirectoryName(SourceFileName)?.Replace(ScanLocation, "");

            if (MaintainSubFolders)
            {
                if (!string.IsNullOrEmpty(szSubFolderName))
                {
                    var sTemp = TargetFolder + szSubFolderName;
                    if (!Directory.Exists(sTemp))
                    {
                        Directory.CreateDirectory(sTemp);
                    }

                    szSubFolderName += "\\";
                }
                else
                {
                    szSubFolderName = "\\";
                }
            }
            else
            {
                szSubFolderName = "\\";
            }

            var szTargetFile = TargetFolder + szSubFolderName + szRootName;

            if (File.Exists(szTargetFile))
            {
                var fileInfo = new FileInfo(SourceFileName);
                var sSource = fileInfo.Length;

                fileInfo = new FileInfo(szTargetFile);
                var sTarget = fileInfo.Length;
                if (sSource != sTarget)
                {
                    szTargetFile = TargetFolder + szSubFolderName + szRootName.Replace(Path.GetExtension(szTargetFile), "") + "."
                                   + DateTime.Now.Year.ToString("0000") + "."
                                   + DateTime.Now.Month.ToString("00") + "."
                                   + DateTime.Now.Day.ToString("00") + "."
                                   + DateTime.Now.Hour.ToString("00") + "."
                                   + DateTime.Now.Minute.ToString("00") + "."
                                   + DateTime.Now.Second.ToString("00")
                                   + Path.GetExtension(SourceFileName);

                    File.Copy(SourceFileName, szTargetFile);
                }
            }
            else
            {
                File.Copy(SourceFileName, szTargetFile);
            }

            bReturn = true;
            return bReturn;
        }

        public bool ProcessTaskDelete(string SourceFileName, string ScanLocation, string TargetFolder, bool ConditonalDelete = false)
        {
            bool bReturn = false;
            try
            {
                if (!ConditonalDelete)
                {
                    File.Delete(SourceFileName);
                }
                else
                {
                    var szRootName = Path.GetFileName(SourceFileName);

                    var szSubFolderName = Path.GetDirectoryName(SourceFileName)?.Replace(ScanLocation, "");

                    if (!string.IsNullOrEmpty(szSubFolderName))
                    {
                        var sTemp = TargetFolder + szSubFolderName;
                        if (!Directory.Exists(sTemp))
                        {
                            Directory.CreateDirectory(sTemp);
                        }

                        szSubFolderName += "\\";
                    }
                    else
                    {
                        szSubFolderName = "\\";
                    }

                    var szTargetFile = TargetFolder + szSubFolderName + szRootName;

                    if (File.Exists(szTargetFile))
                    {
                        File.Delete(szTargetFile);
                    }
                }
                bReturn = true;
            }
            catch (Exception ex)
            {
                bReturn = false;
            }

            return bReturn;
        }

        public bool ProcessTaskMove(string SourceFileName,
                                    string ScanLocation,
                                    string TargetFolder,
                                    bool MaintainSubFolders,
                                    bool OverWrite)
        {
            var bReturn = false;
            var szRootName = Path.GetFileName(SourceFileName);

            var szSubFolderName = Path.GetDirectoryName(SourceFileName)?.Replace(ScanLocation, "");

            if (MaintainSubFolders)
            {
                if (!string.IsNullOrEmpty(szSubFolderName))
                {
                    var sTemp = TargetFolder + szSubFolderName;
                    if (!Directory.Exists(sTemp))
                    {
                        Directory.CreateDirectory(sTemp);
                    }

                    szSubFolderName += "\\";
                }
                else
                {
                    szSubFolderName = "\\";
                }
            }
            else
            {
                szSubFolderName = "\\";
            }

            var szTargetFile = TargetFolder + szSubFolderName + szRootName;

            File.Move(SourceFileName, szTargetFile);

            bReturn = true;

            return bReturn;
        }

        public bool ProcessTaskRestoreDatabase(string BackUpFile,
                                               string ServerName,
                                               string DataBaseName,
                                               bool IntegratedSecurity,
                                               string userId,
                                               string Password,
                                               string FileGroupPath)
        {
            var bReturn = false;
            var sMessage = " BackUpFile = " + BackUpFile
                                            + "\n ServerName = " + ServerName
                                            + "\n DataBaseName = " + DataBaseName
                                            + "\n UserID = " + userId
                                            + "\n Password = " + Password
                                            + "\n FileGroupPath = " + FileGroupPath;

            try
            {
                this.WriteToLog("In RestoreDatabase :" + sMessage, "Application", "RabaService", EventLogEntryType.Information);

                var oDsRestoreFile = new DataSet();

                if (FileGroupPath.Length > 0)
                {
                    oDsRestoreFile.ReadXml(FileGroupPath);
                }

                var passedArrays = new ArrayList();

                foreach (DataRow oRow in oDsRestoreFile.Tables[0].Rows)
                {
                    var oSqlBackUpManager = new SqlBackUpManager.RestoreFileGroup { FileLogicalName = Convert.ToString(oRow["FileLogicalName"]), FilePath = Convert.ToString(oRow["FilePath"]) };

                    passedArrays.Add(oSqlBackUpManager);
                }

                var oRestore = new SqlBackUpManager();

                this.WriteToLog("Restore 1:", "Application", "RABA IN RESTORE", EventLogEntryType.Information);

                if (IntegratedSecurity)
                {
                    this.WriteToLog("Restore 2:", "Application", "RABA IN RESTORE", EventLogEntryType.Information);

                    oRestore.DisconnectUsersFromDatabase(DataBaseName,
                                                         ServerName);

                    oRestore.Restore(BackUpFile,
                                     ServerName,
                                     DataBaseName,
                                     passedArrays);
                }
                else
                {
                    oRestore.Restore(BackUpFile,
                                     ServerName,
                                     DataBaseName,
                                     userId,
                                     Password,
                                     passedArrays);
                }
                bReturn = true;
            }
            catch (Exception ex)
            {
                var szExceptionString = "Exception Generated In Restore \n";

                szExceptionString += "Source          : " + ex.Source + "\n";
                szExceptionString += "Message         : " + ex.Message + "\n";
                szExceptionString += "Inner Exception : " + ex.InnerException + "\n";
                szExceptionString += "To String       : " + ex + "\n";
                szExceptionString += "Stack Trace     : " + ex.StackTrace + "\n";

                this.WriteToLog(szExceptionString, "Application", "RABA IN RESTORE", EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        public bool ProcessTaskRunSqlScript(string ScriptFile,
                                            string ServerName,
                                            string DataBaseName,
                                            bool IntegratedSecurity,
                                            string userId,
                                            string Password)
        {
            var bReturn = false;
            var sMessage = " ScriptFile = " + ScriptFile
                                            + "\n ServerName = " + ServerName
                                            + "\n DataBaseName = " + DataBaseName
                                            + "\n UserID = " + userId
                                            + "\n Password = " + Password;
            var oSql = new SqlBackUpManager();
            try
            {
                oSql.ExecuteSqlFromFile(ScriptFile, ServerName, DataBaseName);
                bReturn = true;
            }
            catch (Exception ex)
            {
                var szExceptionString = "Exception Generated In Restore \n";

                szExceptionString += "Source          : " + ex.Source + "\n";
                szExceptionString += "Message         : " + ex.Message + "\n";
                szExceptionString += "Inner Exception : " + ex.InnerException + "\n";
                szExceptionString += "To String       : " + ex + "\n";
                szExceptionString += "Stack Trace     : " + ex.StackTrace + "\n";

                this.WriteToLog(szExceptionString, "Application", "RABA IN Run SQL Script", EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        public bool UnZipFile(string ZipFileName, string DestinationFolder, bool MaintainSubFolder)
        {
            bool bReturn;
            try
            {
                var fz = new FastZip();
                fz.ExtractZip(ZipFileName, DestinationFolder, null);

                bReturn = true;
            }

            catch (Exception ex)
            {
                this.WriteToLog("Issue Unzip File :UnzipFile:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        public bool ZipUpFile(string FileName, string ScanLocation, string zipFileTo, bool MaintainSubFolder)
        {
            var bReturn = false;

            try
            {
                using (var s = new ZipOutputStream(File.Create(zipFileTo)))
                {
                    s.SetLevel(9); // 0-9, 9 being the highest compression
                    var buffer = new byte[4096];

                    var entry = new ZipEntry(Path.GetFileName(FileName)) { DateTime = DateTime.Now };
                    s.PutNextEntry(entry);
                    using (var fs = File.OpenRead(FileName))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }

                    s.Finish();
                    s.Close();
                }

                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in Zipping Up File :ProcessRRDataTransfer:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        protected override void OnStart(string[] args)
        {
            this.InitializeVariables();
            this.WriteToLog("On Start After Initialize Variables", "Application", "RabaService", EventLogEntryType.Information);
            this.mtimer = new Timer(this.miTimerInterval);
            this.WriteToLog("On Start Timer Enabled", "Application", "RabaService", EventLogEntryType.Information);
            this.mtimer.Elapsed += this.ServiceTimer_Tick;
            this.WriteToLog("On Start Timer Tick Handler Enabled", "Application", "RabaService", EventLogEntryType.Information);
            this.mtimer.Enabled = true;
        }

        protected override void OnStop()
        {
        }

        private bool FileMeetsConditions(string FileName,
                                         string ScanFileExtension,
                                         string ScanFilePrefix,
                                         DateTime ScanFileDateLessThan,
                                         DateTime ScanFileDateGreaterThan,
                                         bool ScanFileUseRelativeAgeYounger,
                                         int ScanFileRelativeAgeYounger,
                                         bool ScanFileUseRelativeAgeOlder,
                                         int ScanFileRelativeAgeOlder,
                                         bool OnlyCountWeekDays,
                                         long ScanFileSizeLessThan,
                                         long ScanFileSizeGreaterThan)
        {
            var bReturn = false;

            try
            {
                if (this.mbLog)
                {
                    var szMessage = " In FileMeetsConditions Checking File:  " + FileName
                                                                               + "\r \n  ScanFileExtension                   :  " + ScanFileExtension
                                                                               + "\r \n  ScanFilePrefix                      :  " + ScanFilePrefix
                                                                               + "\r \n  ScanFileDateLessThan                :  " + ScanFileDateLessThan.ToShortDateString()
                                                                               + "\r \n  ScanFileDateGreaterThan             :  " + ScanFileDateGreaterThan.ToShortDateString()
                                                                               + "\r \n  ScanFileUseRelativeAgeYounger       :  " + Convert.ToString(ScanFileUseRelativeAgeYounger)
                                                                               + "\r \n  ScanFileRelativeAgeYounger          :  " + Convert.ToString(ScanFileRelativeAgeYounger)
                                                                               + "\r \n  ScanFileUseRelativeAgeOlder         :  " + Convert.ToString(ScanFileUseRelativeAgeOlder)
                                                                               + "\r \n  ScanFileRelativeAgeOlder            :  " + Convert.ToString(ScanFileRelativeAgeOlder)
                                                                               + "\r \n  ScanFileSizeLessThan                :  " + Convert.ToString(ScanFileSizeLessThan)
                                                                               + "\r \n  ScanFileSizeGreaterThan             :  " + Convert.ToString(ScanFileSizeGreaterThan);

                    this.WriteToLog(szMessage, "Application", "RabaService", EventLogEntryType.Information);
                }

                var fileSize = new FileInfo(FileName).Length / 1024;
                var fileDate = new FileInfo(FileName).LastWriteTime;

                if (this.mbLog)
                {
                    this.WriteToLog("In FileMeetsConditions Checking File Name Starts With  \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                }

                if (Path.GetFileName(FileName).ToUpper().StartsWith(ScanFilePrefix.ToUpper()))
                {
                    //MGM 
                    if (this.mbLog)
                    {
                        this.WriteToLog("In FileMeetsConditions File Meets Starts With  \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                    }

                    // Now We need to check out the 
                    // Extension 
                    ScanFileExtension = ScanFileExtension.Replace("*.",string.Empty);
                    if (FileName.ToUpper().EndsWith(ScanFileExtension.ToUpper()))
                    {
                        //MGM 
                        if (this.mbLog)
                        {
                            this.WriteToLog("In FileMeetsConditions File Meets Ends With  \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                        }

                        // Now Check Size
                        // first make sure that a size condition
                        if ((ScanFileSizeGreaterThan == 0) && (ScanFileSizeLessThan == 0)) // No Criteria /// Go Through all files 
                        {
                            //MGM 
                            // WriteToLog("In Process 84:" , "Application", "RabaService", EventLogEntryType.Error);
                            if (this.mbLog)
                            {
                                this.WriteToLog("In FileMeetsConditions File Meets Size Criteria  \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                            }

                            if (ScanFileUseRelativeAgeYounger || ScanFileUseRelativeAgeOlder)
                            {
                                if (this.mbLog)
                                {
                                    this.WriteToLog("In FileMeetsConditions Use Relative Age  Younger \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                                }

                                // Use The Relative Age of the File... 
                                if (OnlyCountWeekDays)
                                {
                                    // Calculate Relative Age based on WeekDays 
                                    ScanFileRelativeAgeYounger = this.GetTotalNumberOfDays(ScanFileRelativeAgeYounger);
                                }

                                var oTSpan = new TimeSpan(ScanFileRelativeAgeYounger, 0, 0, 0);
                                if (ScanFileUseRelativeAgeYounger && (fileDate >= DateTime.Now.Subtract(oTSpan)))
                                {
                                    if (this.mbLog)
                                    {
                                        this.WriteToLog("In FileMeetsConditions File Meets Relative Age Younger Criteria  \r \n File Name : " + FileName, "Application", "RabaService", EventLogEntryType.Information);
                                    }

                                    if (OnlyCountWeekDays)
                                    {
                                        // Calculate Relative Age based on WeekDays 
                                        ScanFileRelativeAgeOlder = this.GetTotalNumberOfDays(ScanFileRelativeAgeOlder);
                                    }

                                    oTSpan = new TimeSpan(ScanFileRelativeAgeOlder, 0, 0, 0);
                                    if (ScanFileUseRelativeAgeOlder && (fileDate <= DateTime.Now.Subtract(oTSpan)))
                                    {
                                        var oFileInfo = new FileInfo(FileName);
                                        // Check File Status Here.....
                                        if (this.FileNotHeld(oFileInfo))
                                        {
                                            bReturn = true;
                                        }
                                    }
                                    else
                                    {
                                        var oFileInfo = new FileInfo(FileName);
                                        // Check File Status Here.....
                                        if (this.FileNotHeld(oFileInfo))
                                        {
                                            bReturn = true;
                                        }
                                    }
                                }
                                else
                                {
                                    // var allDates = GetDates(); // method which returns a list of dates  // filter dates by working day's   var countOfWorkDays = allDates      .Where(day => day.IsWorkingDay())      .Count() ; 
                                    if (OnlyCountWeekDays)
                                    {
                                        // Calculate Relative Age based on WeekDays 
                                        ScanFileRelativeAgeOlder = this.GetTotalNumberOfDays(ScanFileRelativeAgeOlder);
                                    }

                                    oTSpan = new TimeSpan(ScanFileRelativeAgeOlder, 0, 0, 0);

                                    if (ScanFileUseRelativeAgeOlder && (fileDate <= DateTime.Now.Subtract(oTSpan)))
                                    {
                                        var oFileInfo = new FileInfo(FileName);
                                        // Check File Status Here.....
                                        if (this.FileNotHeld(oFileInfo))
                                        {
                                            bReturn = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((fileDate >= ScanFileDateGreaterThan) && (fileDate <= ScanFileDateLessThan))
                                {
                                    var oFileInfo = new FileInfo(FileName);
                                    if (this.FileNotHeld(oFileInfo))
                                    {
                                        bReturn = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((fileSize >= ScanFileSizeGreaterThan) && (fileSize <= ScanFileSizeLessThan))
                            {
                                if ((fileDate >= ScanFileDateGreaterThan) && (fileDate <= ScanFileDateLessThan))
                                {
                                    var oFileInfo = new FileInfo(FileName);
                                    if (this.FileNotHeld(oFileInfo))
                                    {
                                        bReturn = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MGM 
                this.WriteToLog("In Process 888 Exception:" + ex, "Application", "RabaService", EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        private bool FileNotHeld(FileInfo fi)
        {
            var bReturn = false;
            try
            {
                var fs = fi.Open(FileMode.Open, FileAccess.ReadWrite);

                fs.Close();
                bReturn = true;
            }

            catch (IOException)
            {
                bReturn = false;
            }

            return bReturn;
        }

        private ArrayList GetSettingsFiles()
        {
            var arrReturn = new ArrayList();

            if (this.mbLog)
            {
                this.WriteToLog("Get Settings Files, Started.", "Application", "RabaService", EventLogEntryType.Information);
            }

            try
            {
                string szPathMapJob;
                var szSettingExtension = "RABA";
                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Reading Path Info.", "Application", "RabaService", EventLogEntryType.Information);
                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MapJob"]))
                {
                    szPathMapJob = AppDomain.CurrentDomain.BaseDirectory + "\\" + ConfigurationManager.AppSettings["MapJob"];
                }
                else
                {
                    szPathMapJob = AppDomain.CurrentDomain.BaseDirectory + "\\Settings";
                }

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Read Path Info: " + szPathMapJob, "Application", "RabaService", EventLogEntryType.Information);
                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MapJobExtenstion"]))
                {
                    szSettingExtension = ConfigurationManager.AppSettings["MapJobExtenstion"];
                }

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Read Job Extension: " + szSettingExtension, "Application", "RabaService", EventLogEntryType.Information);
                }

                var szSettingExtensionSearch = "*." + szSettingExtension;

                var settingFiles = Directory.GetFiles(szPathMapJob, szSettingExtensionSearch, SearchOption.TopDirectoryOnly);

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Compiled Setings Files: " + szSettingExtension, "Application", "RabaService", EventLogEntryType.Information);
                }

                foreach (var sFileName in settingFiles)
                {                    
                    arrReturn.Add(sFileName);
                }
            }
            catch (Exception ex)
            {
                this.WriteToLog("Exception Encountered In GetSettingsFiles" + ex, "Application", "RabaService", EventLogEntryType.Error);
            }

            return arrReturn;
        }

        private int GetTotalNumberOfDays(int NumberOfBusinessDays)
        {
            // Calculate Number of days total based On Business Days 
            var iTotalNumberOfDays = 0;
            var iTotalNumberOfDaysExcludingWeekends = 0;
            var iDaysBack = 0;

            while (iTotalNumberOfDays < NumberOfBusinessDays)
            {
                var tsTemp = new TimeSpan(iDaysBack, 0, 0, 0);

                if ((DateTime.Now.Subtract(tsTemp).DayOfWeek != DayOfWeek.Saturday) && (DateTime.Now.Subtract(tsTemp).DayOfWeek != DayOfWeek.Sunday))
                {
                    iTotalNumberOfDays++;
                }

                iTotalNumberOfDaysExcludingWeekends++;
            }

            var iReturn = iTotalNumberOfDaysExcludingWeekends;

            return iReturn;
        }

        private bool InitializeVariables()
        {
            var bReturn = false;

            try
            {                
                this.WriteToLog("InitializeVariables Read Timer Interval", "Application", "RabaService", EventLogEntryType.Information);

                if (Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]) != 0)
                {
                    this.miTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]);
                }

                this.WriteToLog("InitializeVariables Read Timer Interval Completed", "Application", "RabaService", EventLogEntryType.Information);

                this.mbLog = Convert.ToBoolean(ConfigurationManager.AppSettings["TraceLog"]);

                if (this.mbLog)
                {
                    this.WriteToLog("Variable Initialized \n " +
                                    "\n mbLog  = " + Convert.ToString(this.mbLog) +
                                    "\n miTimerInterval  = " + Convert.ToString(this.miTimerInterval),
                                    "Application",
                                    "RabaService",
                                    EventLogEntryType.Information);
                }

                this.WriteToLog("InitializeVariables Completed", "Application", "RabaService", EventLogEntryType.Information);
                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in Initializing Variables:InitializeVariables:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);
            }

            return bReturn;
        }

        private bool ProcessSettingsFile(string settingsFile)
        {
            var bReturn = false;

            this.oDs = new DataSet();

            if (this.mbLog)
            {
                this.WriteToLog("In Process Settings File, Currently Processing :" + settingsFile, "Application", "RabaService", EventLogEntryType.Information);
            }

            this.oDs.ReadXml(settingsFile);

            if (this.oDs.Tables.Count == 0)
            {
                this.WriteToLog("File " + settingsFile + " does not in correct format ", "Application", "RabaService", EventLogEntryType.Information);
                return false;
            }

            if (this.mbLog)
            {
                this.WriteToLog("In Process Settings File, Completed Reading :" + settingsFile +
                                "\r \n Number of Rows In Settings File : " + this.oDs.Tables[0].Rows.Count, "Application", "RabaService", EventLogEntryType.Information);
            }

            var currentRow = 0;

            var hasDependentColumn = this.oDs.Tables[0].Columns.Contains("Dependent");
            var hasConditionalRunColumn = this.oDs.Tables[0].Columns.Contains("ConditionalRun");
            var hasConditionalDeleteColumn = this.oDs.Tables[0].Columns.Contains("ConditionalDelete");

            foreach (DataRow oRow in this.oDs.Tables[0].Rows)
            {
                if (this.mbLog)
                {
                    currentRow += 1;
                    this.WriteToLog("In Process Settings File, Currently Reading Settings File :" + settingsFile +
                                    "\r \n Currently Processing Row Number : " + Convert.ToString(currentRow), "Application", "RabaService", EventLogEntryType.Information);
                }

                var dateLesThan = DateTime.Parse(oRow["ScanFileDateLessThan"].ToString());
                var dateGreaterThan = DateTime.Parse(oRow["ScanFileDateGreaterThan"].ToString());
                var scanFileSizeLessThan = Convert.ToInt64(oRow["ScanFileSizeLessThan"]);
                var scanFileSizeGreaterThan = Convert.ToInt64(oRow["ScanFileSizeGreaterThan"]);
                var includeSubFolders = Convert.ToBoolean(oRow["IncludeSubFolders"]);
                var scanFileUseRelativeAgeYounger = Convert.ToBoolean(oRow["ScanFileUseRelativeAgeYounger"]);
                var scanFileUseRelativeAgeOlder = Convert.ToBoolean(oRow["ScanFileUseRelativeAgeOlder"]);
                var scanFileAgeYounger = Convert.ToInt32(oRow["ScanFileAgeYounger"]);
                var scanFileAgeOlder = Convert.ToInt32(oRow["ScanFileAgeOlder"]);
                var dependent = hasDependentColumn ? Convert.ToString(oRow["Dependent"]) : string.Empty;
                var conditionalRun = hasConditionalRunColumn ? Convert.ToString(oRow["ConditionalRun"]) : string.Empty;
                var conditionalDelete = hasConditionalDeleteColumn ? Convert.ToBoolean(oRow["ConditionalDelete"]) : false;

                this.ProcessTaskHandler(
                    Convert.ToString(oRow["Action"]),
                    Convert.ToString(oRow["ScanLocation"]),
                    Convert.ToString(oRow["ScanFileExtension"]),
                    Convert.ToString(oRow["ScanFilePrefix"]),
                    dateLesThan,
                    dateGreaterThan,
                    scanFileUseRelativeAgeYounger,
                    scanFileAgeYounger,
                    scanFileUseRelativeAgeOlder,
                    scanFileAgeOlder,
                    Convert.ToBoolean(oRow["OnlyCountWeekDays"]),
                    scanFileSizeLessThan,
                    scanFileSizeGreaterThan,
                    Convert.ToString(oRow["TargetLocation"]),
                    Convert.ToBoolean(oRow["MaintainSubFolders"]),
                    Convert.ToString(oRow["Command"]),
                    Convert.ToBoolean(oRow["ActionCompleteRename"]),
                    Convert.ToBoolean(oRow["ActionCompleteTimeStamp"]),
                    Convert.ToBoolean(oRow["ActionCompleteDelete"]),
                    Convert.ToBoolean(oRow["IntegratedSecurity"]),
                    Convert.ToString(oRow["UserID"]),
                    Convert.ToString(oRow["Password"]),
                    Convert.ToString(oRow["DatabaseName"]),
                    Convert.ToString(oRow["DatabaseServer"]),
                    Convert.ToBoolean(oRow["RunSQLScript"]),
                    Convert.ToString(oRow["RunSQLScriptFilePath"]),
                    Convert.ToString(oRow["RestoreDatabaseFileGroups"]),
                    includeSubFolders,
                    dependent,
                    conditionalRun,
                    conditionalDelete);
            }
            bReturn = true;

            return bReturn;
        }

        private bool ProcessTask(string szFileName,
                                 string Task,
                                 string ScanLocation,
                                 string ScanFileExtension,
                                 string ScanFilePrefix,
                                 string TargetLocation,
                                 bool MaintainSubFolders,
                                 string Command,
                                 bool IntegratedSecurity,
                                 string userId,
                                 string Password,
                                 string DatabaseName,
                                 string DatabaseServer,
                                 string FileGroup,
                                 bool ActionCompleteRename,
                                 bool ActionCompleteTimeStamp,
                                 bool ActionCompleteDelete,
                                 bool ConditionalDelete)
        {
            var bReturn = false;

            switch (Task.ToUpper())
            {
                case "COPY":
                    // Log the Start of the Activity                     
                    this.ProcessTaskCopy(szFileName, ScanLocation, TargetLocation, MaintainSubFolders, true);
                    if (ActionCompleteRename)
                    {
                        if (ActionCompleteTimeStamp)
                        {
                            this.RenameFile(szFileName, szFileName + DateTime.Now.Year.ToString("0000") + "."
                                                        + DateTime.Now.Month.ToString("00") + "."
                                                        + DateTime.Now.Day.ToString("00") + "."
                                                        + DateTime.Now.Hour.ToString("00") + "."
                                                        + DateTime.Now.Minute.ToString("00") + "."
                                                        + DateTime.Now.Second.ToString("00") + ".COPIED");
                        }
                        else
                        {
                            this.RenameFile(szFileName, szFileName + ".COPIED");
                        }
                    }
                    else
                    {
                        if (ActionCompleteDelete)
                        {
                            File.Delete(szFileName);
                        }
                    }

                    bReturn = true;
                    break;

                case "DELETE":
                    this.ProcessTaskDelete(szFileName, ScanLocation, TargetLocation, ConditionalDelete);
                    bReturn = true;
                    break;

                case "MOVE":
                    this.ProcessTaskMove(szFileName, ScanLocation, TargetLocation, MaintainSubFolders, true);
                    bReturn = true;
                    break;

                case "BATCH":
                    if(!Path.GetExtension(szFileName).Equals(".BATCH"))
                    {
                        this.ProcessRunBatchFile(Command, "[FILENAME]", szFileName);

                        if (ActionCompleteTimeStamp)
                        {
                            this.RenameFile(szFileName, szFileName + DateTime.Now.Year.ToString("0000") + "."
                                                        + DateTime.Now.Month.ToString("00") + "."
                                                        + DateTime.Now.Day.ToString("00") + "."
                                                        + DateTime.Now.Hour.ToString("00") + "."
                                                        + DateTime.Now.Minute.ToString("00") + "."
                                                        + DateTime.Now.Second.ToString("00") + ".BATCH");
                        }
                        else
                        {
                            this.RenameFile(szFileName, szFileName + ".BATCH");
                        }
                    }                    

                    bReturn = true;
                    break;

                case "ZIP":
                    var szRootName = Path.GetFileName(szFileName);
                    var szTargetFile = TargetLocation + "\\" + szRootName + ".zip";

                    this.ZipUpFile(szFileName, ScanLocation, szTargetFile, MaintainSubFolders);
                    if (ActionCompleteRename)
                    {
                        if (ActionCompleteTimeStamp)
                        {
                            this.RenameFile(szFileName, szFileName + "." + DateTime.Now.Year.ToString("0000") + "."
                                                        + DateTime.Now.Month.ToString("00") + "."
                                                        + DateTime.Now.Day.ToString("00") + "."
                                                        + DateTime.Now.Hour.ToString("00") + "."
                                                        + DateTime.Now.Minute.ToString("00") + "."
                                                        + DateTime.Now.Second.ToString("00") + ".ZIPPED");
                        }
                        else
                        {
                            this.RenameFile(szFileName, szFileName + ".ZIPPED");
                        }
                    }
                    else
                    {
                        if (ActionCompleteDelete)
                        {
                            File.Delete(szFileName);
                        }
                    }

                    bReturn = true;
                    break;

                case "UNZIP":
                    this.UnZipFile(szFileName, TargetLocation, MaintainSubFolders);
                    if (ActionCompleteRename)
                    {
                        if (ActionCompleteTimeStamp)
                        {
                            this.RenameFile(szFileName, szFileName + "." + DateTime.Now.Year.ToString("0000") + "."
                                                        + DateTime.Now.Month.ToString("00") + "."
                                                        + DateTime.Now.Day.ToString("00") + "."
                                                        + DateTime.Now.Hour.ToString("00") + "."
                                                        + DateTime.Now.Minute.ToString("00") + "."
                                                        + DateTime.Now.Second.ToString("00") + ".UNZIPPED");
                        }
                        else
                        {
                            this.RenameFile(szFileName, szFileName + ".UNZIPPED");
                        }
                    }
                    else
                    {
                        if (ActionCompleteDelete)
                        {
                            File.Delete(szFileName);
                        }
                    }

                    bReturn = true;
                    break;

                case "RESTORE":

                    this.ProcessTaskRestoreDatabase(szFileName, DatabaseServer, DatabaseName, IntegratedSecurity, userId, Password, FileGroup);

                    if (ActionCompleteRename)
                    {
                        if (ActionCompleteTimeStamp)
                        {
                            this.RenameFile(szFileName, szFileName + "." + DateTime.Now.Year.ToString("0000") + "."
                                                        + DateTime.Now.Month.ToString("00") + "."
                                                        + DateTime.Now.Day.ToString("00") + "."
                                                        + DateTime.Now.Hour.ToString("00") + "."
                                                        + DateTime.Now.Minute.ToString("00") + "."
                                                        + DateTime.Now.Second.ToString("00") + ".RESTORED");
                        }
                        else
                        {
                            this.RenameFile(szFileName, szFileName + ".RESTORED");
                        }
                    }
                    else
                    {
                        if (ActionCompleteDelete)
                        {
                            File.Delete(szFileName);
                        }
                    }

                    bReturn = true;
                    break;

                case "SQLSCRIPT":
                    this.ProcessTaskRunSqlScript(Command, DatabaseServer, DatabaseName, true, userId, Password);
                    if (ActionCompleteTimeStamp)
                    {
                        this.RenameFile(szFileName, szFileName + DateTime.Now.Year.ToString("0000") + "."
                                                    + DateTime.Now.Month.ToString("00") + "."
                                                    + DateTime.Now.Day.ToString("00") + "."
                                                    + DateTime.Now.Hour.ToString("00") + "."
                                                    + DateTime.Now.Minute.ToString("00") + "."
                                                    + DateTime.Now.Second.ToString("00") + ".SCRIPTRUN");
                    }
                    else
                    {
                        this.RenameFile(szFileName, szFileName + ".SCRIPTRUN");
                    }

                    bReturn = true;
                    break;
            }

            return bReturn;
        }

        private bool ProcessTaskHandler(string Task,
                                        string ScanLocation,
                                        string ScanFileExtension,
                                        string ScanFilePrefix,
                                        DateTime ScanFileDateLessThan,
                                        DateTime ScanFileDateGreaterThan,
                                        bool ScanFileUseRelativeAgeYounger,
                                        int ScanFileRelativeAgeYounger,
                                        bool ScanFileUseRelativeAgeOlder,
                                        int ScanFileRelativeAgeOlder,
                                        bool OnlyCountWeekDays,
                                        long ScanFileSizeLessThan,
                                        long ScanFileSizeGreaterThan,
                                        string TargetLocation,
                                        bool MaintainSubFolders,
                                        string Command,
                                        bool ActionCompleteRename,
                                        bool ActionCompleteTimeStamp,
                                        bool ActionCompleteDelete,
                                        bool IntegratedSecurity,
                                        string userId,
                                        string Password,
                                        string DatabaseName,
                                        string DatabaseServer,
                                        bool runSqlScript,
                                        string runSqlScriptFilePath,
                                        string RestoreDatabaseFileGroups,
                                        bool IncludeSubfolders,
                                        string Dependent,
                                        string ConditionalRun,
                                        bool ConditonalDelete
        )
        {
            var bReturn = false;
            var useCurrentTaskCondition = false;
            if (this.mbLog)
            {
                var szMessage = $"Entered ProcessTaskHandler : \r \n TASK :                  {Task}\r \n ScanLocation :          {ScanLocation}\r \n ScanFileExtension :     {ScanFileExtension}\r \n ScanFilePrefix :        {ScanFilePrefix}\r \n ScanFileDateLessThan :  {Convert.ToString(ScanFileDateLessThan)}\r \n ScanFileDateGreaterThan:{Convert.ToString(ScanFileDateGreaterThan)}\r \n ScanFileUseRelativeAgeYounger: {Convert.ToString(ScanFileUseRelativeAgeYounger)}\r \n ScanFileRelativeAgeYounger:    {Convert.ToString(ScanFileRelativeAgeYounger)}\r \n ScanFileUseRelativeAgeOlder: {Convert.ToString(ScanFileUseRelativeAgeOlder)}\r \n ScanFileRelativeAgeOlder:    {Convert.ToString(ScanFileRelativeAgeOlder)}\r \n OnlyCountWeekDays:           {Convert.ToString(OnlyCountWeekDays)}\r \n ScanFileSizeLessThan:   {Convert.ToString(ScanFileSizeLessThan)}\r \n ScanFileSizeGreaterThan:{Convert.ToString(ScanFileSizeGreaterThan)}\r \n TargetLocation:         {TargetLocation}\r \n MaintainSubFolders:     {Convert.ToString(MaintainSubFolders)}\r \n MaintainSubFolders:     {Convert.ToString(Command)}\r \n ActionCompleteRename:   {Convert.ToString(ActionCompleteRename)}\r \n ActionCompleteTimeStamp:{Convert.ToString(ActionCompleteTimeStamp)}\r \n ActionCompleteDelete:   {Convert.ToString(ActionCompleteDelete)}\r \n IntegratedSecurity:     {Convert.ToString(IntegratedSecurity)}\r \n UserID:                 {userId}\r \n Password:               {Password}\r \n DatabaseName:           {DatabaseName}\r \n DatabaseServer:         {DatabaseServer}\r \n RunSQLScript:           {Convert.ToString(runSqlScript)}\r \n RunSQLScriptFilePath:   {runSqlScriptFilePath}\r \n RestoreDatabaseFileGroups:{RestoreDatabaseFileGroups}\r \n IncludeSubfolders:      {Convert.ToString(IncludeSubfolders)}\r \n ";

                this.WriteToLog(szMessage, "Application", "RabaService", EventLogEntryType.Information);
            }

            if (!Directory.Exists(ScanLocation))
            {
                return false;
            }

            if (this.mbLog)
            {
                this.WriteToLog($"In ProcessTaskHandler Reading Scanlocation {ScanLocation}", "Application", "RabaService", EventLogEntryType.Information);
            }

            var fileInScanLocation = new List<string>();

            if(ConditionalRun.Equals("R"))
            {
                if(PreviousRun != null)
                {
                    if(Dependent.Equals("D"))
                    { 
                        if(PreviousRun.FileMeetConditions.Count == 0)
                        {
                            PreviousRun.IsSussess = false;
                            return false;
                        }
                        else
                        {
                            fileInScanLocation = PreviousRun.FileMeetConditions;
                            ScanLocation = PreviousRun.ScanLocation;
                            ScanFileExtension = PreviousRun.ScanFileExtension;
                            ScanFilePrefix = PreviousRun.ScanFilePrefix;
                            TargetLocation = PreviousRun.TargetLocation;
                            MaintainSubFolders = PreviousRun.MaintainSubFolders;
                            Command = PreviousRun.Command;
                            IntegratedSecurity = PreviousRun.IntegratedSecurity;
                            userId = PreviousRun.UserId;
                            Password = PreviousRun.Password;
                            DatabaseName = PreviousRun.DatabaseName;
                            DatabaseServer = PreviousRun.DatabaseServer;
                            RestoreDatabaseFileGroups = PreviousRun.RestoreDatabaseFileGroups;
                            ActionCompleteRename = PreviousRun.ActionCompleteRename;
                            ActionCompleteTimeStamp = PreviousRun.ActionCompleteTimeStamp;
                            ActionCompleteDelete = PreviousRun.ActionCompleteDelete;
                        }
                    }
                    else
                    {
                        if (PreviousRun.FileMeetConditions.Count == 0)
                        {
                            useCurrentTaskCondition = true;
                        }
                        else
                        {
                            fileInScanLocation = PreviousRun.FileMeetConditions;
                            ScanLocation = PreviousRun.ScanLocation;
                            ScanFileExtension = PreviousRun.ScanFileExtension;
                            ScanFilePrefix = PreviousRun.ScanFilePrefix;
                            TargetLocation = PreviousRun.TargetLocation;
                            MaintainSubFolders = PreviousRun.MaintainSubFolders;
                            Command = PreviousRun.Command;
                            IntegratedSecurity = PreviousRun.IntegratedSecurity;
                            userId = PreviousRun.UserId;
                            Password = PreviousRun.Password;
                            DatabaseName = PreviousRun.DatabaseName;
                            DatabaseServer = PreviousRun.DatabaseServer;
                            RestoreDatabaseFileGroups = PreviousRun.RestoreDatabaseFileGroups;
                            ActionCompleteRename = PreviousRun.ActionCompleteRename;
                            ActionCompleteTimeStamp = PreviousRun.ActionCompleteTimeStamp;
                            ActionCompleteDelete = PreviousRun.ActionCompleteDelete;
                        }
                    }
                }
            }
            else
            {
                var sfiles = IncludeSubfolders ? Directory.GetFiles(ScanLocation, "*.*", SearchOption.AllDirectories) : Directory.GetFiles(ScanLocation);
                fileInScanLocation = new List<string>(sfiles);
                useCurrentTaskCondition = true;
            }            

            if (this.mbLog)
            {
                this.WriteToLog($"In ProcessTaskHandler Reading Scanlocation {ScanLocation}\r \n  Total Number Of Files Returned {Convert.ToString(fileInScanLocation.Count)}", "Application", "RabaService", EventLogEntryType.Information);
            }

            if (Task.Equals("RUN"))
            {
                PreviousRun = new ActionResult();
                PreviousRun.IsSussess = true;
                PreviousRun.ScanFileExtension = ScanFileExtension;
                PreviousRun.IntegratedSecurity = IntegratedSecurity;
                PreviousRun.Password = Password;
                PreviousRun.RestoreDatabaseFileGroup = RestoreDatabaseFileGroups;
                PreviousRun.ScanFileExtension = ScanFileExtension;
                PreviousRun.ScanFilePrefix = ScanFilePrefix;
                PreviousRun.ScanLocation = ScanLocation;
                PreviousRun.TargetLocation = TargetLocation;
                PreviousRun.UserId = userId;
                PreviousRun.Command = Command;
                PreviousRun.ActionCompleteDelete = ActionCompleteDelete;
                PreviousRun.ActionCompleteRename = ActionCompleteRename;
                PreviousRun.ActionCompleteTimeStamp = ActionCompleteTimeStamp;
                PreviousRun.DatabaseName = DatabaseName;
                PreviousRun.DatabaseServer = DatabaseServer;
                PreviousRun.MaintainSubFolders = MaintainSubFolders;
                PreviousRun.RestoreDatabaseFileGroups = RestoreDatabaseFileGroups;
                PreviousRun.FileMeetConditions = new List<string>();
            }
                
            foreach (var sFileName in fileInScanLocation)
            {
                if (this.mbLog)
                {
                    this.WriteToLog($"In ProcessTaskHandler Reading Scanlocation {ScanLocation}\r \n  Total Number Of Files Returned {Convert.ToString(fileInScanLocation.Count)}\r \n  Currently Testing File : {sFileName}",
                                    "Application", "RabaService", EventLogEntryType.Information);
                }

                if(!ConditionalRun.Equals("R") || useCurrentTaskCondition)
                {
                    if (!this.FileMeetsConditions(sFileName,
                                              ScanFileExtension,
                                              ScanFilePrefix,
                                              ScanFileDateLessThan,
                                              ScanFileDateGreaterThan,
                                              ScanFileUseRelativeAgeYounger,
                                              ScanFileRelativeAgeYounger,
                                              ScanFileUseRelativeAgeOlder,
                                              ScanFileRelativeAgeOlder,
                                              OnlyCountWeekDays,
                                              ScanFileSizeLessThan,
                                              ScanFileSizeGreaterThan))
                    {
                        continue;
                    }
                }

                if (this.mbLog)
                {
                    this.WriteToLog($"In ProcessTaskHandler Reading Scanlocation {ScanLocation}\r \n  Total Number Of Files Returned {Convert.ToString(fileInScanLocation.Count)}\r \n  File : {sFileName}     Meets Conditions",
                                    "Application", "RabaService", EventLogEntryType.Information);
                }

                if(Task.Equals("RUN"))
                {
                    PreviousRun.FileMeetConditions.Add(sFileName);
                    continue;
                }

                if (!this.ProcessTask(sFileName,
                                      Task,
                                      ScanLocation,
                                      ScanFileExtension,
                                      ScanFilePrefix,
                                      TargetLocation,
                                      MaintainSubFolders,
                                      Command,
                                      IntegratedSecurity,
                                      userId,
                                      Password,
                                      DatabaseName,
                                      DatabaseServer,
                                      RestoreDatabaseFileGroups,
                                      ActionCompleteRename,
                                      ActionCompleteTimeStamp,
                                      ActionCompleteDelete,
                                      ConditonalDelete))
                {
                    continue;
                }

                if (Task.ToUpper() == "DELETE")
                {
                    continue;
                }

                if (ActionCompleteDelete)
                {
                    this.ProcessTaskDelete(sFileName, ScanLocation, TargetLocation);
                }
            } // end loop of Files In folder

            return bReturn;
        }

        private bool RenameFile(string SourceFile, string TargetFileName)
        {
            var bReturn = false;
            try
            {
                File.Move(SourceFile, TargetFileName);
                this.mbTransferFileInProgress = false;
                bReturn = true;
            }

            catch (Exception ex)
            {
                this.WriteToLog("Issue in Transferring File :ProcessRRDataTransfer:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        private void ServiceTimer_Tick(object sender, EventArgs e)
        {
            if (this.mbLog)
            {
                this.WriteToLog("Timer hit, Checking to See if the Service is in process", "Application", "RabaService", EventLogEntryType.Information);
            }

            if (!this.bInProcess)
            {
                if (this.mbLog)
                {
                    this.WriteToLog("Timer hit, Service Not In Process. Creating A New Process ", "Application", "RabaService", EventLogEntryType.Information);
                }

                this.bInProcess = true;

                if (this.mbLog)
                {
                    this.WriteToLog("Timer hit, About to enter process, a marker has been assinged. Creating A New Process ", "Application", "RabaService", EventLogEntryType.Information);
                }

                this.ProcessRaba();
                this.bInProcess = false;
            }
            else
            {
                if (this.mbLog)
                {
                    this.WriteToLog("Timer hit, Service Still Running Process. Did not enter process.", "Application", "RabaService", EventLogEntryType.Information);
                }
            }
        }

        private bool WriteStatus()
        {
            var bReturn = false;
            var szSettingsFile = AppDomain.CurrentDomain.BaseDirectory + "\\CURRENT.RABA.STATUS";

            try
            {
                if (File.Exists(szSettingsFile))
                {
                    File.Delete(szSettingsFile);
                }

                TextWriter otw = new StreamWriter(szSettingsFile);

                var szCurrentStatus = " The Current Time  " + DateTime.Now.ToLongTimeString();

                otw.Write(szCurrentStatus);
                otw.Close();
                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in WriteStatus:" + ex,
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);
            }

            return bReturn;
        }

        private void WriteToLog(string logMessage, string logName, string LogSource, EventLogEntryType LogType)
        {
            //var ev = new EventLog(logName, Environment.MachineName, LogSource);
            //ev.WriteEntry(logMessage, LogType);
            //ev.Close();

            switch (LogType)
            {
                case EventLogEntryType.Information:
                    log.Info(logMessage);
                    break;
                case EventLogEntryType.Error:
                    log.Error(logMessage);
                    break;
                case EventLogEntryType.Warning:
                    log.Warn(logMessage);
                    break;
            }
        }
    }
}