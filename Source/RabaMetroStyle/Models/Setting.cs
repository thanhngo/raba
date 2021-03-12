using System.ComponentModel;

namespace RabaMetroStyle.Models
{
    public class Setting
    {
        [DisplayName("Action")]
        public string Action { get; set; }

        [DisplayName("On Completion Delete")]
        public string ActionCompleteDelete { get; set; }

        [DisplayName("On Completion Rename")]
        public string ActionCompleteRename { get; set; }

        [DisplayName("On Completion Time Stamp")]
        public string ActionCompleteTimeStamp { get; set; }

        [DisplayName("Command")]
        public string Command { get; set; }

        [DisplayName("Database")]
        public string DatabaseName { get; set; }

        [DisplayName("Server")]
        public string DatabaseServer { get; set; }

        [DisplayName("Include Sub Folder")]
        public string IncludeSubFolders { get; set; }

        [DisplayName("Integrated Security")]
        public string IntegratedSecurity { get; set; }

        [DisplayName("Maintain Sub Folders")]
        public string MaintainSubFolders { get; set; }

        [DisplayName("Only Count Weekdays")]
        public string OnlyCountWeekDays { get; set; }

        public string Password { get; set; }

        [DisplayName("Restore File Groups")]
        public string RestoreDatabaseFileGroups { get; set; }

        [DisplayName("Run SQL Script")]
        public string RunSQLScript { get; set; }

        [DisplayName("Script Path")]
        public string RunSQLScriptFilePath { get; set; }

        [DisplayName("File Age Older (Days)")]
        public string ScanFileAgeOlder { get; set; }

        [DisplayName("File Age Younger (Days)")]
        public string ScanFileAgeYounger { get; set; }

        [DisplayName("Date Greater Than")]
        public string ScanFileDateGreaterThan { get; set; }

        [DisplayName("Date Less Than")]
        public string ScanFileDateLessThan { get; set; }

        [DisplayName("File Extension")]
        public string ScanFileExtension { get; set; }

        [DisplayName("Prefix")]
        public string ScanFilePrefix { get; set; }

        [DisplayName("Size Greater Than")]
        public string ScanFileSizeGreaterThan { get; set; }

        [DisplayName("Size Less Than")]
        public string ScanFileSizeLessThan { get; set; }

        [DisplayName("Use Relative Age Older")]
        public string ScanFileUseRelativeAgeOlder { get; set; }

        [DisplayName("Use Relative Age Younger")]
        public string ScanFileUseRelativeAgeYounger { get; set; }

        [DisplayName("Scan Location")]
        public string ScanLocation { get; set; }

        [DisplayName("Target Location")]
        public string TargetLocation { get; set; }

        [DisplayName("Task Order")]
        public string TaskOrder { get; set; }

        public string UserID { get; set; }
    }
}