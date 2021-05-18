using System.Collections.Generic;

namespace RabaService
{
    public class ActionResult
    {
        public bool IsSussess { get; set; }
        public string ActionName { get; set; }
        public string ScanLocation { get; set; }
        public string ScanFileExtension { get; set; }
        public string ScanFilePrefix { get; set; }
        public string TargetLocation { get; set; }
        public string Command { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseServer { get; set; }
        public string RestoreDatabaseFileGroup { get; set; }
        public bool ActionCompleteRename { get; set; }
        public bool ActionCompleteTimeStamp { get; set; }
        public bool ActionCompleteDelete { get; set; }
        public string RestoreDatabaseFileGroups { get; set; }
        public bool MaintainSubFolders { get; set; }
        public List<string> FileMeetConditions { get; set; }
    }
}
