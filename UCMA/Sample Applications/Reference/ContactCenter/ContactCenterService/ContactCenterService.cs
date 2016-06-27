/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterService
{
    public partial class ContactCenterService : ServiceBase
    {
        AcdPlatform platform;
        public ContactCenterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                this.platform = new AcdPlatform();
                string configXMLDoc = "";

                //Get Executing Path
                String path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                using (TextReader reader = new StreamReader(path+ @"\Config.xml"))
                {
                    configXMLDoc = reader.ReadToEnd();
                }
                this.platform.BeginStartUp(configXMLDoc,
                                           delegate(IAsyncResult ar)
                                           {
                                               try
                                               {
                                                   this.platform.EndStartUp(ar);
                                               }
                                               catch (Exception ex)
                                               {
                                                   AcdLogger logger = new AcdLogger();
                                                   logger.Log(ex);
                                               }
                                           
                                           },
                                           null);

            }
            catch (Exception ex)
            {
                AcdLogger logger = new AcdLogger();
                logger.Log(ex);
            }
        }

        protected override void OnStop()
        {
            //Request that the plaftorm gracefully shuts down .  Stop receiving calls and allow current calls to terinate.
            platform.BeginShutdown(delegate(IAsyncResult ar)
            { platform.EndShutdown(ar); },
                       platform);

        }
    }
}
