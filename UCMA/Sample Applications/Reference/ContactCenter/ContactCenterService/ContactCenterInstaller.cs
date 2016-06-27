/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
********************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterService
{
    [RunInstaller(true)]
    public partial class ContactCenterInstaller : Installer
    {
        public ContactCenterInstaller()
        {
            InitializeComponent();
        }

    }
}
