
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingMcuSessionNotificationProcessorFactory.cs
Description: ApplicationSharingMcuSessionNotificationProcessorFactory is a derivative of McuSessionNotificationProcessorFactory 
and is used to extend the platform to other modalities such as Application Sharing. Desktop sharing is typically used in Helpdesk
environment to assist information workers with computer issues for example.
**********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Internal.Collaboration;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration;

namespace Microsoft.Rtc.Collaboration.Samples.ApplicationSharing
{
    public class ApplicationSharingMcuSessionNotificationProcessorFactory : ConferenceMcuNotificationProcessorFactory
	{

        
        #region Constructors
        /// <summary>
        /// Initializes the instance.
        /// </summary>
        public ApplicationSharingMcuSessionNotificationProcessorFactory()
            : base()
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Returns the supported media type.
        /// </summary>
        public override Collection<string> MediaTypes
        {
            get
            {
                var collection = new Collection<string>();
                collection.Add(MediaType.ApplicationSharing);                
                return collection;
            }
        }

        /// <summary>
        /// Returns the Mcu type for the factory
        /// </summary>
        public override string McuType
        {
            get
            {
                return MediaType.ApplicationSharing;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Creates a processor instance to handle the mcu type and media types specified.
        /// </summary>
        /// <param name="mediaTypes">The media type for which to create the McuNotificationProcessor</param>
        /// <param name="context">The context for creating the object.</param>
        /// <returns>
        /// A processor that can handle notifications for the given mcu category. 
        /// Returns null if a processor is not available to handle the category.
        /// </returns>
        public override object Create(IEnumerable<string> mediaTypes, FactoryContext context)
        {
       
            ConferenceMcuNotificationProcessor mcuNotificationProcessor = null;

            if (mediaTypes.Contains<string>(MediaType.ApplicationSharing))
            {
                mcuNotificationProcessor = new ApplicationSharingMcuSessionNotificationProcessor(context.Conversation.ConferenceSession);
            }

            return mcuNotificationProcessor;
        }
        #endregion

	}
}
