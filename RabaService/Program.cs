using log4net.Config;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace RabaService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
            string configFilePath = assemblyDirPath + "\\App.config";
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));

            var servicesToRun = new ServiceBase[]
                                {
                                    new RabaService()
                                };
            ServiceBase.Run(servicesToRun);

            //RabaService main = new RabaService();
            //main.ProcessRaba();
        }
    }
}