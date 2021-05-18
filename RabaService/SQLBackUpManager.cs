using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace RabaService
{
    public class SqlBackUpManager : ISqlBackUpManager
    {
        public void BackUp(string FileName,
                           string Server,
                           string DataBaseName,
                           string userId,
                           string Password)
        {
            try
            {
                //create an instance of a server class
                var sqlServer = new Server(new ServerConnection(Server, userId, Password));
                ////create a backup class instance               
                var backup = new Backup
                {
                    Action = BackupActionType.Database,
                    Database = "your database name that needs to be backed up "
                };
                backup.Devices.AddDevice(FileName, DeviceType.File);
                backup.SqlBackupAsync(sqlServer);
            }
            catch (Exception err)
            {
                // 
            }
        }

        public bool DisconnectUsersFromDatabase(string DatabaseName, string Server)
        {
            var bReturn = false;

            var oDt = new DataTable();

            var sSql = string.Empty;
            var sConn = string.Empty;
            var spid = string.Empty;

            var oStringBuilder = new SqlConnectionStringBuilder();

            oStringBuilder.IntegratedSecurity = true;
            oStringBuilder.DataSource = Server;
            oStringBuilder.InitialCatalog = "master";
            oStringBuilder.AsynchronousProcessing = false;

            sConn = oStringBuilder.ConnectionString;

            // First Build the SQL String 

            sSql = $"SELECT * FROM sysprocesses WHERE dbid = db_id('{DatabaseName}');";

            var oConn = new SqlConnection(sConn);
            oConn.Open();

            var oCmd = new SqlCommand(sSql, oConn);

            oCmd.CommandTimeout = 0;

            var oDa = new SqlDataAdapter(oCmd);

            oDa.Fill(oDt);

            oCmd.ExecuteNonQuery();

            var rowCount = oDt.Rows.Count;
            if (rowCount > 0)
            {
                // Connections Exist 
                foreach (DataRow dr in oDt.Rows)
                {
                    spid = dr["spid"].ToString();
                    sSql = $"KILL {spid}";
                    oCmd.CommandText = sSql;
                    oCmd.ExecuteNonQuery();
                }
            }

            // End of Work Area 

            oCmd.Dispose();
            oConn.Dispose();
            // All is well 
            bReturn = true;

            return bReturn;
        }

        /// <summary>
        ///     Using Integrated Authentication
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Server"></param>
        /// <param name="DataBaseName"></param>
        /// <returns></returns>
        public bool ExecuteSqlFromFile(string FileName,
                                       string Server,
                                       string DataBaseName)
        {
            var bReturn = false;

            try
            {
                StreamReader myStreamReader = null;
                myStreamReader = File.OpenText(FileName);
                var sSql = myStreamReader.ReadToEnd();

                var oStringBuilder = new SqlConnectionStringBuilder { IntegratedSecurity = true, DataSource = Server, InitialCatalog = DataBaseName };
                var sConn = oStringBuilder.ConnectionString;

                var oConn = new SqlConnection(sConn);
                oConn.Open();
                var oCmd = new SqlCommand(sSql, oConn) { CommandTimeout = 0 };
                oCmd.ExecuteNonQuery();

                bReturn = true;
            }
            catch (Exception ex)
            {
                // Issue...
                this.WriteToLog("Issue In ExecuteSQLFromFile:" + ex, "Application", "RABA", EventLogEntryType.Information);
                bReturn = false;
            }

            return bReturn;
        }

        public void Restore(string FileName,
                            string Server,
                            string DataBaseName,
                            string userId,
                            string Password
        )
        {
            try
            {
                var sqlServer = new Server(new ServerConnection(Server, userId, Password));
                var restore = new Restore
                {
                    Database = "Name of Database to be restored",
                    Action = RestoreActionType.Database,
                    ReplaceDatabase = true,
                    NoRecovery = false
                };
                restore.Devices.AddDevice(FileName, DeviceType.File);
                restore.SqlRestoreAsync(sqlServer);
            }
            catch (Exception err)
            {
                var sMessage = err.ToString();
            }
        }

        /// <summary>
        ///     UserID and Password supplied
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Server"></param>
        /// <param name="DataBaseName"></param>
        /// <param name="userId"></param>
        /// <param name="Password"></param>
        /// <param name="FileGroups"></param>
        public void Restore(string FileName,
                            string Server,
                            string DataBaseName,
                            string userId,
                            string Password,
                            ArrayList FileGroups)
        {
            var sSqlRestore = string.Empty;
            var sConn = string.Empty;

            var oStringBuilder = new SqlConnectionStringBuilder();

            oStringBuilder.UserID = userId;
            oStringBuilder.Password = Password;
            oStringBuilder.DataSource = Server;
            oStringBuilder.InitialCatalog = "master";

            sConn = oStringBuilder.ConnectionString;

            // First Build the Restore String 

            sSqlRestore = " RESTORE DATABASE ";
            sSqlRestore += "[" + DataBaseName + "] ";
            sSqlRestore += "FROM  DISK = N'" + FileName + "'  ";
            sSqlRestore += " WITH  FILE = 1,  ";
            //sSQLRestore += "MOVE N'TESTER' TO N'C:\TESTER_data.mdf',   "; 
            //sSQLRestore += "MOVE N'TESTER_log  ' TO N'C:\TESTER_log  _data.mdf',  ";  

            foreach (RestoreFileGroup oRestoreFile in FileGroups)
            {
                sSqlRestore += "MOVE N'" + oRestoreFile.FileLogicalName + "' TO N'" + oRestoreFile.FilePath + " ',   ";
            }

            sSqlRestore += " RECOVERY,  ";
            sSqlRestore += " NOUNLOAD,  ";
            sSqlRestore += " REPLACE,   ";
            sSqlRestore += " STATS = 10 ";

            var oConn = new SqlConnection(sConn);
            oConn.Open();

            var oCmd = new SqlCommand(sSqlRestore, oConn);

            oCmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Using Integrated Authentication
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Server"></param>
        /// <param name="DataBaseName"></param>
        /// <param name="FileGroups"></param>
        public void Restore(string FileName,
                            string Server,
                            string DataBaseName,
                            ArrayList FileGroups)
        {
            var sSqlRestore = string.Empty;
            var sConn = string.Empty;
            //WriteToLog("In Restore:20", "Application", "RABA", EventLogEntryType.Information); 

            var oStringBuilder = new SqlConnectionStringBuilder();

            oStringBuilder.IntegratedSecurity = true;
            oStringBuilder.DataSource = Server;
            oStringBuilder.InitialCatalog = "master";
            // oStringBuilder.AsynchronousProcessing = true  ; 

            sConn = oStringBuilder.ConnectionString;

            //WriteToLog("In Restore:21", "Application", "RABA", EventLogEntryType.Information); 

            // First Build the Restore String 

            sSqlRestore = " RESTORE DATABASE ";
            sSqlRestore += "[" + DataBaseName + "] ";
            sSqlRestore += "FROM  DISK = N'" + FileName + "'  ";
            sSqlRestore += " WITH  FILE = 1,  ";
            //sSQLRestore += "MOVE N'TESTER' TO N'C:\TESTER_data.mdf',   "; 
            //sSQLRestore += "MOVE N'TESTER_log  ' TO N'C:\TESTER_log  _data.mdf',  ";  

            foreach (RestoreFileGroup oRestoreFile in FileGroups)
            {
                sSqlRestore += "MOVE N'" + oRestoreFile.FileLogicalName + "' TO N'" + oRestoreFile.FilePath + " ',   ";
            }

            sSqlRestore += " RECOVERY,  ";
            sSqlRestore += " NOUNLOAD,  ";
            sSqlRestore += " REPLACE,   ";
            sSqlRestore += " STATS = 10 ";

            var oConn = new SqlConnection(sConn);
            oConn.Open();

            //WriteToLog("In Restore:22", "Application", "RABA", EventLogEntryType.Information);
            //WriteToLog("SQL Restore " + sSQLRestore, "Application" ,  "RABA", EventLogEntryType.Information); 

            var oCmd = new SqlCommand(sSqlRestore, oConn);

            oCmd.CommandTimeout = 0;

            oCmd.ExecuteNonQuery();

            //oCMD.BeginExecuteNonQuery(CallbackExecuteNonquery , oCMD    );
            //WriteToLog("In Restore:23", "Application", "RABA", EventLogEntryType.Information); 
        }

        private void CallbackExecuteNonquery(IAsyncResult result)
        {
            try
            {
            }
            catch (Exception ex)
            {
                // We are running the code in a seperate thread so we need to catch the exception. 
                // Else we are unable to catch the exception anywhere.
                //this.Invoke(new DisplayStatusDelegate(DisplayStatus), "Error: " + ex.Message);
            }
        }

        private void WriteToLog(string LogMessage, string LogName, string LogSource, EventLogEntryType LogType)
        {
            var ev = new EventLog(LogName, Environment.MachineName, LogSource);

            ev.WriteEntry(LogMessage, LogType);
            ev.Close();
        }

        public struct RestoreFileGroup
        {
            // Property
            public string FileLogicalName { get; set; }

            public string FilePath { get; set; }
        }
    }
}