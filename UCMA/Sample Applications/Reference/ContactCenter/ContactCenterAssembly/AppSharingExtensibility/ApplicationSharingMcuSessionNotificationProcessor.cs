
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingMcuSessionNotificationProcessor.cs
Description: ApplicationSharingMcuSessionNotificationProcessor is a derivative of ConferenceMcuNotificationProcessor and is used to extend the platform 
to other modalities such as Application Sharing. Desktop sharing is typically used in Helpdesk
environment to assist information workers with computer issues for example.
**********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Internal.Collaboration;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.ComponentModel;

namespace Microsoft.Rtc.Collaboration.Samples.ApplicationSharing
{
    /// <summary>
    /// The use of public APIs in Microsoft.Rtc.Internal.Collaboration is subject to change from
    /// release to release. Use those APIs at your own risks. Microsoft does not support support or
    /// documentation for these APIs.
    /// </summary>
    class ApplicationSharingMcuSessionNotificationProcessor : ConferenceMcuNotificationProcessor
    {
        #region Constructors
        /// <summary>
        /// Initializes the object.
        /// </summary>
        public ApplicationSharingMcuSessionNotificationProcessor(ConferenceSession conferenceSession)
            : base(conferenceSession)
        {
        }

        #endregion

        #region Public properties
        /// <summary>
        /// Gets the type of MCU the processor is compatible with.
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
        /// Populates the  MCU Session settings based on the XML
        /// </summary>
        /// <param name="body">The raw XML to be parsed to MCU properties.</param>
        /// <returns>Information about property values and which (possibly) have changed from their default values.</returns>
        protected override PropertyMergeInformation<McuSessionProperties> HandleGetMcuProperties(byte[] body)
        {
            //we simply need to return the default implementation in our case. No need for
            // further extensibility
            var pmi = new PropertyMergeInformation<McuSessionProperties>();

            return pmi;
        }

        /// <summary>
        /// Merge MCU properties with new XML data.
        /// </summary>
        /// <param name="properties">Existing values for previously parsed properties.</param>
        /// <param name="body">Raw XML containing updated property values</param>
        /// <returns>The new property values and names of the properties that changed.</returns>
        protected override PropertyMergeInformation<McuSessionProperties> HandleMergeMcuProperties(
            McuSessionProperties properties,
            byte[] body)
        {
            //we simply need to return the default implementation in our case. No need for
            // further extensibility
            return new PropertyMergeInformation<McuSessionProperties>();
        }

        /// <summary>
        /// Populates the participant properties based on XML.
        /// </summary>
        /// <param name="body">The raw XML to be parsed to participant properties.</param>
        /// <returns>Information about property values and which (possibly) have changed from their default values.</returns>
        protected override PropertyMergeInformation<McuParticipantEndpointProperties> HandleGetParticipantEndpointProperties(byte[] body)
        {
            //we simply need to return the default implementation in our case. No need for
            // further extensibility
            PropertyMergeInformation<McuParticipantEndpointProperties> pmi = new PropertyMergeInformation<McuParticipantEndpointProperties>();

            return pmi;
        }

        /// <summary>
        /// Merge participant properties with new XML data.
        /// </summary>
        /// <param name="properties">Existing values for previously parsed properties.</param>
        /// <param name="body">Raw XML containing updated property values</param>
        /// <returns>The new property values and names of the properties that changed.</returns>
        protected override PropertyMergeInformation<McuParticipantEndpointProperties> HandleMergeParticipantEndpointProperties(
            McuParticipantEndpointProperties properties,
            byte[] body)
        {
            //we simply need to return the default implementation in our case. No need for
            // further extensibility
            var pmi = new PropertyMergeInformation<McuParticipantEndpointProperties>();

            return pmi;
        }
        #endregion protected
    }
}