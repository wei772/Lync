/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpService
{
    using System.ServiceProcess;
    using FastHelpServer;
    using System;
    using System.Threading;
    using FastHelp.Logging;

    /// <summary>
    ///   Represents Windows Service for UCMA BOT
    /// </summary>
    public partial class FastHelpService : ServiceBase
    {
        /// <summary>
        ///  Instance of UCMA BOT
        /// </summary>
        private FastHelpServerApp ucmaBot;

        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastHelpService"/> class.
        /// </summary>
        public FastHelpService()
        {
            this.InitializeComponent();
            this.ServiceName = "FastHelpService";
            this.logger = new EventLogger(this.ServiceName);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this.logger.Log("Starting bot service");

            if (!ThreadPool.QueueUserWorkItem(new WaitCallback(StartService)))
            {
                 throw new Exception("Fatal internal error");
            }            
        }


        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this.logger.Log("Stopping bot service");
            StopService();
        }

        private void StartService(object obj)
        {
            this.logger.Log("Started bot service");
            this.ucmaBot = new FastHelpServerApp();
            this.ucmaBot.Start();
        }

        private void StopService()
        {
            this.logger.Log("Stopped bot service");
            if (this.ucmaBot != null)
            {
                this.ucmaBot.Stop();
            }
        }
    }
}