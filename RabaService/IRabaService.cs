namespace RabaService
{
    public interface IRabaService
    {
        bool UnZipFile(string ZipFileName, string DestinationFolder, bool MaintainSubFolder);

        bool ZipUpFile(string FileName, string ScanLocation, string zipFileTo, bool MaintainSubFolder);

        bool ProcessTaskMove(string SourceFileName,
                             string ScanLocation,
                             string TargetFolder,
                             bool MaintainSubFolders,
                             bool OverWrite);

        bool ProcessTaskRunSqlScript(string ScriptFile,
                                     string ServerName,
                                     string DataBaseName,
                                     bool IntegratedSecurity,
                                     string userId,
                                     string Password);

        bool ProcessTaskDelete(string SourceFileName);

        bool ProcessTaskCopy(string SourceFileName,
                             string ScanLocation,
                             string TargetFolder,
                             bool MaintainSubFolders,
                             bool OverWrite);

        bool ProcessRunBatchFile(string BatchFile, string TargetString, string FileName);
    }
}
