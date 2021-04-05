using System.Collections;

namespace RabaService
{
    public interface ISqlBackUpManager
    {
        void BackUp(string FileName,
                    string Server,
                    string DataBaseName,
                    string userId,
                    string Password);

        bool DisconnectUsersFromDatabase(string DatabaseName, string Server);

        bool ExecuteSqlFromFile(string FileName,
                                string Server,
                                string DataBaseName);

        void Restore(string FileName,
                     string Server,
                     string DataBaseName,
                     string userId,
                     string Password);

        void Restore(string FileName,
                     string Server,
                     string DataBaseName,
                     string userId,
                     string Password,
                     ArrayList FileGroups);

        void Restore(string FileName,
                     string Server,
                     string DataBaseName,
                     ArrayList FileGroups);
    }
}