
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingCallFactory.cs
Description: ApplicationSharingCallFactory is an implementation of CallFactory. this factory is
used by the CollaborationPlatform to automatically instantiates an ApplicationSharingCall upon
receiving an SDP invite with media token "applicationsharing"
**********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.ComponentModel;
using Microsoft.Rtc.Collaboration;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Internal.Collaboration;

namespace Microsoft.Rtc.Collaboration.Samples.ApplicationSharing
{
    /// <summary>
    /// Use Microsoft.Rtc.Internal.Collaboration at your own risks. The APIs exposed
    /// in this namespace are subject to changes from release to release and are not
    /// documented nor supported in any shape or form.
    /// </summary>
	public class ApplicationSharingCallFactory:CallFactory
	{
            /// <summary>
            /// ApplicationSharingCallFactory Constructor (using the base constructor).
            /// </summary>
            public ApplicationSharingCallFactory()
            {
            }

            #region Public properties

            /// <summary>
            /// Returns the supported media types.
            /// </summary>
            public override Collection<string> MediaTypes
            {
                get
                {
                    Collection<string> collection = new Collection<string>();
                    collection.Add(MediaType.ApplicationSharing);
                    return collection;
                }
            }

            #endregion

            /// <summary>
            /// Overrides the default creation to create an ApplicationSharingCall
            /// </summary>
            public override object Create(IEnumerable<string> mediaTypes, FactoryContext context)
            {
               Conversation conversation = context.Conversation;

                Call call = null;

                if (mediaTypes.Contains<string>(MediaType.ApplicationSharing))
                {
                    call = new ApplicationSharingCall(conversation);               
                }

               return call;
            }
        }


}

