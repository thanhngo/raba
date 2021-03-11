using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Configuration;
using System.Collections;
using System.IO.Compression;

namespace RabaService
{
    public partial class RabaService : ServiceBase
    {
        private bool bInProcess = false;
        private bool bLoaded = false;
        private bool mbLog = false;
        private bool mbRestoreInProgress = false;
        private bool mbSettingsExist = false;
        private bool mbTransferFileInProgress = false;
        private bool mbWaitingForHeldFile = false;
        private bool mbZipInProgress = false;
        private int miTimerInterval = 60000;
        private DataSet moDSMapJob = null;
        private string mstrCFGFile;
        private string mszDBAccessPath = string.Empty;
        private string mszPathMapDayPart = string.Empty;
        private string mszPathMapJob = string.Empty;
        private string mszReportPath = string.Empty;
        private string mszSMTPServer = string.Empty;
        private System.Timers.Timer mtimer;
        private DataSet oDS = new DataSet();
        private string szCurrentFile = string.Empty;

        public RabaService()
        {
            this.InitializeComponent();
        }

        public bool ProcessRaba()
        {
            bool bReturn = false;

            if (this.mbLog)
            {
                this.WriteToLog("In Process, Log Enabled.", "Application", "Raba", EventLogEntryType.Information);
            }

            try
            {
                // Get All The files 
                // For all The Jobs
                ArrayList arrSettingsFiles = new ArrayList();

                if (this.mbLog)
                {
                    this.WriteToLog("In Process, Settings File Array Initialized.", "Application", "Raba", EventLogEntryType.Information);
                }

                arrSettingsFiles = this.GetSettingsFiles();

                if (this.mbLog)
                {
                    this.WriteToLog("In Process, Settings File Array Retrieved.", "Application", "Raba", EventLogEntryType.Information);
                }

                // Loop Through the Files 
                foreach (string sFileName in arrSettingsFiles)
                {
                    if (this.mbLog)
                    {
                        this.WriteToLog("In Process Looping through Settings Files, Currently Reading :" + sFileName, "Application", "Raba", EventLogEntryType.Error);
                    }

                    // Load and process the File
                    this.ProcessSettingsFile(sFileName);
                } // End Loop Through Files 

                // All is well 
                bReturn = true;
            }
            catch (Exception ex)
            {
                // Issue 
                // Log the issue 

                this.WriteToLog("Issue in ProcessRaba:" + ex.ToString(),
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                // And set the return value to a false 
                bReturn = false;
            }

            return bReturn;
        }

        protected override void OnStart(string[] args)
        {
            this.InitializeVariables();
            this.WriteToLog("On Start After Initialize Variables", "Application", "Raba", EventLogEntryType.Information);
            this.mtimer = new System.Timers.Timer(this.miTimerInterval);
            this.WriteToLog("On Start Timer Enabled", "Application", "Raba", EventLogEntryType.Information);
            this.mtimer.Elapsed += new System.Timers.ElapsedEventHandler(this.ServiceTimer_Tick);
            this.WriteToLog("On Start Timer Tick Handler Enabled", "Application", "Raba", EventLogEntryType.Information);
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
            bool bReturn = false;

            try
            {
                if (this.mbLog)
                {
                    string szMessage = " In FileMeetsConditions Checking File:  " + FileName
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

                    this.WriteToLog(szMessage, "Application", "Raba", EventLogEntryType.Information);
                }

                // Convert the File Size to KB 
                // so we can compare apples to apples 
                long FileSize = new FileInfo(FileName).Length / 1024;
                DateTime FileDate = new FileInfo(FileName).LastWriteTime;

                //MGM 
                // WriteToLog("In Process 81", "Application", "Raba", EventLogEntryType.Error);

                // First Check The File Name 
                if (this.mbLog)
                {
                    this.WriteToLog("In FileMeetsConditions Checking File Name Starts With  \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                }

                if (Path.GetFileName(FileName).ToUpper().StartsWith(ScanFilePrefix.ToUpper()))
                {
                    //MGM 
                    if (this.mbLog)
                    {
                        this.WriteToLog("In FileMeetsConditions File Meets Starts With  \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                    }

                    // Now We need to check out the 
                    // Extension 
                    if (FileName.ToUpper().EndsWith(ScanFileExtension.ToUpper()))
                    {
                        //MGM 
                        if (this.mbLog)
                        {
                            this.WriteToLog("In FileMeetsConditions File Meets Ends With  \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                        }

                        // Now Check Size
                        // first make sure that a size condition
                        if (ScanFileSizeGreaterThan == 0 && ScanFileSizeLessThan == 0) // No Criteria /// Go Through all files 
                        {
                            //MGM 
                            // WriteToLog("In Process 84:" , "Application", "Raba", EventLogEntryType.Error);
                            if (this.mbLog)
                            {
                                this.WriteToLog("In FileMeetsConditions File Meets Size Criteria  \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                            }

                            if (ScanFileUseRelativeAgeYounger || ScanFileUseRelativeAgeOlder)
                            {
                                if (this.mbLog)
                                {
                                    this.WriteToLog("In FileMeetsConditions Use Relative Age  Younger \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                                }

                                // Use The Relative Age of the File... 
                                if (OnlyCountWeekDays)
                                {
                                    // Calculate Relative Age based on WeekDays 
                                    ScanFileRelativeAgeYounger = this.GetTotalNumberOfDays(ScanFileRelativeAgeYounger);
                                }

                                TimeSpan oTSpan = new TimeSpan(ScanFileRelativeAgeYounger, 0, 0, 0);
                                if (ScanFileUseRelativeAgeYounger && FileDate >= DateTime.Now.Subtract(oTSpan))
                                {
                                    if (this.mbLog)
                                    {
                                        this.WriteToLog("In FileMeetsConditions File Meets Relative Age Younger Criteria  \r \n File Name : " + FileName, "Application", "Raba", EventLogEntryType.Information);
                                    }

                                    if (OnlyCountWeekDays)
                                    {
                                        // Calculate Relative Age based on WeekDays 
                                        ScanFileRelativeAgeOlder = this.GetTotalNumberOfDays(ScanFileRelativeAgeOlder);
                                    }

                                    oTSpan = new TimeSpan(ScanFileRelativeAgeOlder, 0, 0, 0);
                                    if (ScanFileUseRelativeAgeOlder && FileDate <= DateTime.Now.Subtract(oTSpan))
                                    {
                                        FileInfo oFileInfo = new FileInfo(FileName);
                                        // Check File Status Here.....
                                        if (this.FileNotHeld(oFileInfo))
                                        {
                                            bReturn = true;
                                        }
                                    }
                                    else
                                    {
                                        FileInfo oFileInfo = new FileInfo(FileName);
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

                                    if (ScanFileUseRelativeAgeOlder && FileDate <= DateTime.Now.Subtract(oTSpan))
                                    {
                                        FileInfo oFileInfo = new FileInfo(FileName);
                                        // Check File Status Here.....
                                        if (this.FileNotHeld(oFileInfo))
                                        {
                                            bReturn = true;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            else
                            {
                                // Meets Requirments of size...
                                // Now check the date 
                                if (FileDate >= ScanFileDateGreaterThan && FileDate <= ScanFileDateLessThan)
                                {
                                    //MGM 
                                    // WriteToLog("In Process 85:", "Application", "Raba", EventLogEntryType.Error);
                                    FileInfo oFileInfo = new FileInfo(FileName);
                                    // Check File Status Here.....
                                    if (this.FileNotHeld(oFileInfo))
                                    {
                                        //MGM 
                                        // WriteToLog("In Process 86:", "Application", "Raba", EventLogEntryType.Error);
                                        // Everything is met 
                                        // so we return a true 
                                        bReturn = true;
                                    }
                                }
                            }

                            ///
                        }
                        else
                        {
                            //MGM 
                            // WriteToLog("In Process 87:", "Application", "Raba", EventLogEntryType.Error);

                            if (FileSize >= ScanFileSizeGreaterThan && FileSize <= ScanFileSizeLessThan)
                            {
                                //MGM 
                                // WriteToLog("In Process 88:", "Application", "Raba", EventLogEntryType.Error);

                                // Meets Requirments of size...
                                // Now check the date 
                                if (FileDate >= ScanFileDateGreaterThan && FileDate <= ScanFileDateLessThan)
                                {
                                    //MGM 
                                    // WriteToLog("In Process 89:", "Application", "Raba", EventLogEntryType.Error);

                                    FileInfo oFileInfo = new FileInfo(FileName);
                                    // Check File Status Here.....
                                    if (this.FileNotHeld(oFileInfo))
                                    {
                                        //MGM 
                                        // WriteToLog("In Process 90:", "Application", "Raba", EventLogEntryType.Error);

                                        // Everything is met 
                                        // so we return a true 
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
                this.WriteToLog("In Process 888 Exception:" + ex.ToString(), "Application", "Raba", EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        private bool FileNotHeld(FileInfo fi)
        {
            FileStream fs;
            bool bReturn = false;

            try
            {
                fs = fi.Open(FileMode.Open, FileAccess.ReadWrite);

                fs.Close();

                // File is free to be used...
                bReturn = true;
            }

            catch (IOException)
            {
                // WriteToLog("File Is Held", "Application", "RABA", EventLogEntryType.Information);
                // All Is Not Well....
                bReturn = false;
            }

            return bReturn;
        }

        private ArrayList GetSettingsFiles()
        {
            ArrayList arrReturn = new ArrayList();

            if (this.mbLog)
            {
                this.WriteToLog("Get Settings Files, Started.", "Application", "Raba", EventLogEntryType.Information);
            }

            try
            {
                string szPathMapJob = string.Empty;
                string szSettingExtension = "RABA";
                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Reading Path Info.", "Application", "Raba", EventLogEntryType.Information);
                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MapJob"]))
                {
                    szPathMapJob = AppDomain.CurrentDomain.BaseDirectory + "\\" + ConfigurationManager.AppSettings["MapJob"];
                }
                else
                {
                    szPathMapJob = AppDomain.CurrentDomain.BaseDirectory + "\\Settings";
                }

                //szPathMapJob = "C:\\Program Files (x86)\\Meeda\\Raba\\Settings"; 

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Read Path Info: " + szPathMapJob, "Application", "Raba", EventLogEntryType.Information);
                }

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MapJobExtenstion"]))
                {
                    szSettingExtension = ConfigurationManager.AppSettings["MapJobExtenstion"];
                }

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Read Job Extension: " + szSettingExtension, "Application", "Raba", EventLogEntryType.Information);
                }

                string szSettingExtensionSearch = "*." + szSettingExtension;

                string[] sfiles = Directory.GetFiles(szPathMapJob, szSettingExtensionSearch, SearchOption.TopDirectoryOnly);

                if (this.mbLog)
                {
                    this.WriteToLog("Get Settings Files, Compiled Setings Files: " + szSettingExtension, "Application", "Raba", EventLogEntryType.Information);
                }

                foreach (string sFileName in sfiles)
                {
                    arrReturn.Add(sFileName);
                }
            }
            catch (Exception ex)
            {
                // Issue 
                // Log The Issue 

                this.WriteToLog("Exception Encountered In GetSettingsFiles" + ex.ToString(), "Application", "Raba", EventLogEntryType.Error);
                // Set the Return value to a 
                // false 
            }

            return arrReturn;
        }

        private int GetTotalNumberOfDays(int NumberOfBusinessDays)
        {
            // Calculate Number of days total based On Business Days 
            int iTotalNumberOfDays = 0;
            int iTotalNumberOfDaysExcludingWeekends = 0;
            int iDaysBack = 0;

            while (iTotalNumberOfDays < NumberOfBusinessDays)
            {
                TimeSpan tsTemp = new TimeSpan(iDaysBack, 0, 0, 0);

                if (DateTime.Now.Subtract(tsTemp).DayOfWeek == DayOfWeek.Saturday ||
                    DateTime.Now.Subtract(tsTemp).DayOfWeek == DayOfWeek.Sunday)
                {
                    // Don't Count 
                }
                else
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
            bool bReturn = false;

            try
            {
                this.WriteToLog("InitializeVariables Read Timer Interval", "Application", "Raba", EventLogEntryType.Information);

                if (Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]) != 0)
                {
                    this.miTimerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]);
                }

                this.WriteToLog("InitializeVariables Read Timer Interval Completed", "Application", "Raba", EventLogEntryType.Information);

                this.mbLog = Convert.ToBoolean(ConfigurationManager.AppSettings["TraceLog"]);

                if (this.mbLog) // If Logging Set To True 
                {
                    this.WriteToLog("Variable Initialized \n " +
                                    "\n mbLog  = " + Convert.ToString(this.mbLog) +
                                    "\n miTimerInterval  = " + Convert.ToString(this.miTimerInterval),
                                    "Application",
                                    "RabaService",
                                    EventLogEntryType.Information);
                }

                this.WriteToLog("InitializeVariables Completed", "Application", "Raba", EventLogEntryType.Information);

                // All is Well 
                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in Initializing Variables:InitializeVariables:" + ex.ToString(),
                                "Application",
                                "Raba",
                                EventLogEntryType.Error);
            }

            return bReturn;
        }

        private bool ProcessSettingsFile(string settingsFile)
        {
            bool bReturn = false;

            this.oDS = new DataSet();

            if (this.mbLog)
            {
                this.WriteToLog("In Process Settings File, Currently Processing :" + settingsFile, "Application", "Raba", EventLogEntryType.Error);
            }

            this.oDS.ReadXml(settingsFile);
            //MGM 

            if (this.mbLog)
            {
                this.WriteToLog("In Process Settings File, Completed Reading :" + settingsFile +
                                "\r \n Number of Rows In Settings File : " + this.oDS.Tables[0].Rows.Count, "Application", "Raba", EventLogEntryType.Error);
            }

            int CurrentRow = 0;

            foreach (DataRow oRow in this.oDS.Tables[0].Rows)
            {
                if (this.mbLog)
                {
                    CurrentRow += 1;
                    this.WriteToLog("In Process Settings File, Currently Reading Settings File :" + settingsFile +
                                    "\r \n Currently Processing Row Number : " + Convert.ToString(CurrentRow), "Application", "Raba", EventLogEntryType.Error);
                }

                var dateLesThan = DateTime.Parse(oRow["ScanFileDateLessThan"].ToString());
                var dateGreaterThan = DateTime.Parse(oRow["ScanFileDateGreaterThan"].ToString());
                var scanFileSizeLessThan = Convert.ToInt64(oRow["ScanFileSizeLessThan"]);
                var scanFileSizeGreaterThan = Convert.ToInt64(oRow["ScanFileSizeGreaterThan"]);
                var includeSubFolders = Convert.ToBoolean(oRow["IncludeSubFolders"]);
                var scanFileUseRelativeAgeYounger = Convert.ToBoolean(oRow["ScanFileUseRelativeAgeYounger"]);
                var scanFileUseRelativeAgeOlder = Convert.ToBoolean(oRow["ScanFileUseRelativeAgeOlder"]);
                var scanFileAgeYounger  = Convert.ToInt32(oRow["ScanFileAgeYounger"]);
                var scanFileAgeOlder = Convert.ToInt32(oRow["ScanFileAgeOlder"]);

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
                    includeSubFolders);
            }

            // All Is Well 
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
                                    string UserID,
                                    string Password,
                                    string DatabaseName,
                                    string DatabaseServer,
                                    string FileGroup,
                                    bool ActionCompleteRename,
                                    bool ActionCompleteTimeStamp,
                                    bool ActionCompleteDelete)
        {
            bool bReturn = false;

            //MGM 
            // WriteToLog("In Process 9", "Application", "Raba", EventLogEntryType.Error);

            switch (Task.ToUpper())
            {
                case "COPY":
                    break;

                case "DELETE":
                    break;

                case "MOVE":
                    break;

                case "BATCH":
                    break;

                case "ZIP":
                    string szRootName = Path.GetFileName(szFileName);
                    string szTargetFile = TargetLocation + "\\" + szRootName + ".ZIP";

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

                    break;

                case "SQLSCRIPT":

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
                                           string UserID,
                                           string Password,
                                           string DatabaseName,
                                           string DatabaseServer,
                                           bool RunSQLScript,
                                           string RunSQLScriptFilePath,
                                           string RestoreDatabaseFileGroups,
                                           bool IncludeSubfolders
        )
        {
            bool bReturn = false;

            //MGM 
            // WriteToLog("In Process 5", "Application", "Raba", EventLogEntryType.Error);

            if (this.mbLog)
            {
                string szMessage = "Entered ProcessTaskHandler : \r \n " +
                                   "TASK :                  " + Task + "\r \n " +
                                   "ScanLocation :          " + ScanLocation + "\r \n " +
                                   "ScanFileExtension :     " + ScanFileExtension + "\r \n " +
                                   "ScanFilePrefix :        " + ScanFilePrefix + "\r \n " +
                                   "ScanFileDateLessThan :  " + Convert.ToString(ScanFileDateLessThan) + "\r \n " +
                                   "ScanFileDateGreaterThan:" + Convert.ToString(ScanFileDateGreaterThan) + "\r \n " +
                                   "ScanFileUseRelativeAgeYounger: " + Convert.ToString(ScanFileUseRelativeAgeYounger) + "\r \n " +
                                   "ScanFileRelativeAgeYounger:    " + Convert.ToString(ScanFileRelativeAgeYounger) + "\r \n " +
                                   "ScanFileUseRelativeAgeOlder: " + Convert.ToString(ScanFileUseRelativeAgeOlder) + "\r \n " +
                                   "ScanFileRelativeAgeOlder:    " + Convert.ToString(ScanFileRelativeAgeOlder) + "\r \n " +
                                   "OnlyCountWeekDays:           " + Convert.ToString(OnlyCountWeekDays) + "\r \n " +
                                   "ScanFileSizeLessThan:   " + Convert.ToString(ScanFileSizeLessThan) + "\r \n " +
                                   "ScanFileSizeGreaterThan:" + Convert.ToString(ScanFileSizeGreaterThan) + "\r \n " +
                                   "TargetLocation:         " + TargetLocation + "\r \n " +
                                   "MaintainSubFolders:     " + Convert.ToString(MaintainSubFolders) + "\r \n " +
                                   "MaintainSubFolders:     " + Convert.ToString(Command) + "\r \n " +
                                   "ActionCompleteRename:   " + Convert.ToString(ActionCompleteRename) + "\r \n " +
                                   "ActionCompleteTimeStamp:" + Convert.ToString(ActionCompleteTimeStamp) + "\r \n " +
                                   "ActionCompleteDelete:   " + Convert.ToString(ActionCompleteDelete) + "\r \n " +
                                   "IntegratedSecurity:     " + Convert.ToString(IntegratedSecurity) + "\r \n " +
                                   "UserID:                 " + UserID + "\r \n " +
                                   "Password:               " + Password + "\r \n " +
                                   "DatabaseName:           " + DatabaseName + "\r \n " +
                                   "DatabaseServer:         " + DatabaseServer + "\r \n " +
                                   "RunSQLScript:           " + Convert.ToString(RunSQLScript) + "\r \n " +
                                   "RunSQLScriptFilePath:   " + RunSQLScriptFilePath + "\r \n " +
                                   "RestoreDatabaseFileGroups:" + RestoreDatabaseFileGroups + "\r \n " +
                                   "IncludeSubfolders:      " + Convert.ToString(IncludeSubfolders) + "\r \n ";

                this.WriteToLog(szMessage, "Application", "Raba", EventLogEntryType.Information);
            }

            if (Directory.Exists(ScanLocation))
            {
                //MGM 
                if (this.mbLog)
                {
                    this.WriteToLog("In ProcessTaskHandler Reading Scanlocation " + ScanLocation, "Application", "Raba", EventLogEntryType.Information);
                }

                // No Problem... all is well....
                string[] sfiles;
                if (IncludeSubfolders)
                {
                    sfiles = Directory.GetFiles(ScanLocation, "*.*", SearchOption.AllDirectories);
                }
                else
                {
                    sfiles = Directory.GetFiles(ScanLocation);
                }

                if (this.mbLog)
                {
                    this.WriteToLog("In ProcessTaskHandler Reading Scanlocation " + ScanLocation
                                                                                  + "\r \n  Total Number Of Files Returned " + Convert.ToString(sfiles.Length), "Application", "Raba", EventLogEntryType.Information);
                }

                foreach (string sFileName in sfiles)
                {
                    if (this.mbLog)
                    {
                        this.WriteToLog("In ProcessTaskHandler Reading Scanlocation " + ScanLocation
                                                                                      + "\r \n  Total Number Of Files Returned " + Convert.ToString(sfiles.Length)
                                                                                      + "\r \n  Currently Testing File : " + sFileName,
                                        "Application", "Raba", EventLogEntryType.Information);
                    }

                    if (this.FileMeetsConditions(sFileName,
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
                        //
                        // MGM 
                        // WriteToLog("In Process 8", "Application", "Raba", EventLogEntryType.Error);
                        if (this.mbLog)
                        {
                            this.WriteToLog("In ProcessTaskHandler Reading Scanlocation " + ScanLocation
                                                                                          + "\r \n  Total Number Of Files Returned " + Convert.ToString(sfiles.Length)
                                                                                          + "\r \n  File : " + sFileName + "     Meets Conditions",
                                            "Application", "Raba", EventLogEntryType.Information);
                        }

                        if (this.ProcessTask(sFileName,
                                             Task,
                                             ScanLocation,
                                             ScanFileExtension,
                                             ScanFilePrefix,
                                             TargetLocation,
                                             MaintainSubFolders,
                                             Command,
                                             IntegratedSecurity,
                                             UserID,
                                             Password,
                                             DatabaseName,
                                             DatabaseServer,
                                             RestoreDatabaseFileGroups,
                                             ActionCompleteRename,
                                             ActionCompleteTimeStamp,
                                             ActionCompleteDelete))
                        {
                            // First Make Sure it's not a Delete 
                            // action since we can't delete a file 
                            // once it's been deleted 
                            if (Task.ToUpper() == "DELETE")
                            {
                                // Do Nothing since it's 
                                // a delete anyway
                            }
                            else
                            {
                                // Now chaeck to see if there's a 
                                // delete after complete 
                                if (ActionCompleteDelete)
                                {
                                    //ProcessTaskDelete(sFileName);
                                }
                                else
                                {
                                }
                            }
                        }
                        else
                        {
                        }
                    } // End of File Meets Conditions Hit 
                } // end loop of Files In folder
            } // end checking of Scan Location 

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
                this.WriteToLog("Issue in Transferring File :ProcessRRDataTransfer:" + ex.ToString(),
                                "Application",
                                "Raba",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }

        private void ServiceTimer_Tick(object sender, EventArgs e)
        {
            if (this.mbLog) this.WriteToLog("Timer hit, Checking to See if the Service is in process", "Application", "Raba", EventLogEntryType.Information);

            if (!this.bInProcess)
            {
                if (this.mbLog) this.WriteToLog("Timer hit, Service Not In Process. Creating A New Process ", "Application", "Raba", EventLogEntryType.Information);

                this.bInProcess = true;

                if (this.mbLog) this.WriteToLog("Timer hit, About to enter process, a marker has been assinged. Creating A New Process ", "Application", "Raba", EventLogEntryType.Information);

                if (this.ProcessRaba())
                {
                    // All is Well now... 
                    // exit the process 
                    this.bInProcess = false;
                }
                else
                {
                    // We have a problem since the process 
                    // failed.... 
                    // notify admin... 

                    this.bInProcess = false;
                    // exit the process 
                    this.bInProcess = false;
                }
            }
            else
            {
                if (this.mbLog) this.WriteToLog("Timer hit, Service Still Running Process. Did not enter process.", "Application", "Raba", EventLogEntryType.Information);
            }
        }

        private bool UnZipFile(string ZipFileName, string DestinationFolder, bool MaintainSubFolder)
        {
            bool bReturn;
            try
            {
                using (FileStream inFile = File.OpenRead(ZipFileName))
                {
                    string curFile = Path.GetFileName(ZipFileName);
                    var fileExtension = Path.GetExtension(ZipFileName);
                    string origName = DestinationFolder + "\\" + curFile.Remove(curFile.Length - fileExtension.Length);
                    using (FileStream outFile = File.Create(origName))
                    {
                        using (GZipStream Decompress = new GZipStream(inFile, CompressionMode.Decompress))
                        {
                            Decompress.CopyTo(outFile);
                        }
                    }
                }

                // All is well 
                bReturn = true;
            }

            catch (Exception ex)
            {
                // Slight Issue.... 
                // Log Error ? Or Message To User ? 
                this.WriteToLog("Issue Unzip File :UnzipFile:" + ex.ToString(),
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                // in any case Return a false reading 
                bReturn = false;
            }

            return bReturn;
        }

        private bool WriteStatus()
        {
            var bReturn = false;
            string szSettingsFile = AppDomain.CurrentDomain.BaseDirectory + "\\CURRENT.RABA.STATUS";

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
                this.WriteToLog("Issue in WriteStatus:" + ex.ToString(),
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);
            }

            return bReturn;
        }

        private void WriteToLog(string logMessage, string logName, string LogSource, EventLogEntryType LogType)
        {
            EventLog ev = new EventLog(logName, Environment.MachineName, LogSource);

            ev.WriteEntry(logMessage, LogType);
            ev.Close();
        }

        private bool ZipUpFile(string FileName, string ScanLocation, string zipFileTo, bool MaintainSubFolder)
        {
            bool bReturn = false;

            try
            {
                using (FileStream sourceFile = File.OpenRead(FileName))
                    using (FileStream destinationFile = File.Create(zipFileTo))
                        using (GZipStream output = new GZipStream(destinationFile, CompressionMode.Compress))
                        {
                            sourceFile.CopyTo(output);
                        }

                bReturn = true;
            }
            catch (Exception ex)
            {
                this.WriteToLog("Issue in Zipping Up File :ProcessRRDataTransfer:" + ex.ToString(),
                                "Application",
                                "RabaService",
                                EventLogEntryType.Error);

                bReturn = false;
            }

            return bReturn;
        }
    }
}