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