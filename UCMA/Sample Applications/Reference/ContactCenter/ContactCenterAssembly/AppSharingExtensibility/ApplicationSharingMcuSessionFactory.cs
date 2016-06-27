
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingMcuSessionFactory.cs
Description: ApplicationSharingMcuSessionFactory is a derivative of ConferenceMcuSessionFactory and is used to extend the platform 
to other modalities such as Application Sharing. Desktop sharing is typically used in Helpdesk
environment to assist information workers with computer issues for example.
**********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Internal.Collaboration;
using Microsoft.Rtc.Collaboration;
using System.Collections.ObjectModel;

namespace Microsoft.Rtc.Collaboration.Samples.ApplicationSharing
{
    /// <summary>
    /// Use Microsoft.Rtc.Internal.Collaboration public APIs at your own risks. The APIs exposed in this 
    /// namespace are subject to change from release to release. Those APIs are not documented
    /// nor supported.
    /// </summary>
    class ApplicationSharingMcuSessionFactory : ConferenceMcuSessionFactory
	{

        public ApplicationSharingMcuSessionFactory(){}

        public override string McuType
        {
            get { return MediaType.ApplicationSharing; }
        }

        public override Collection<string> MediaTypes
        {
            get
            {
                var mediaTypes = new Collection<string>();

                mediaTypes.Add(MediaType.ApplicationSharing);

                return mediaTypes;
            }
        }

        public override object Create(IEnumerable<string> mediaTypes, FactoryContext context)
        {
            McuSession mcuSession = null;

            if (mediaTypes.Contains<string>(MediaType.ApplicationSharing))
            {
                mcuSession = new ApplicationSharingMcuSession();
            }

            return mcuSession;
        }

	}

}
