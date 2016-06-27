/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterService;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterService
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
				new ContactCenterService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
