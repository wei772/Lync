/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpService
{
    using System.ServiceProcess;

    /// <summary>
    /// Starting point for the service
    /// </summary>
   public static class FastHelpStartUp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] 
            { 
                new FastHelpService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
