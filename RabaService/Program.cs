﻿using System.ServiceProcess;

namespace RabaService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
                            {
                                new RabaService()
                            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}