
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingMcuSession.cs
Description: ApplicationSharingMcuSession is a derivative of McuSession and is used to extend the platform 
to other modalities such as Application Sharing. Desktop sharing is typically used in Helpdesk
environment to assist information workers with computer issues for example.
**********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.ComponentModel;

namespace Microsoft.Rtc.Collaboration.Samples.ApplicationSharing
{
	public class ApplicationSharingMcuSession:McuSession
	{
        public ApplicationSharingMcuSession()
            : base(MediaType.ApplicationSharing)
        {

        }

        public override IEnumerable<string> SupportedMediaTypes
        {
            get { return new string[] { MediaType.ApplicationSharing}; }
        }

        protected override void HandleParticipantEndpointAttendanceChanged(Collection<KeyValuePair<ParticipantEndpoint, McuParticipantEndpointProperties>> addedEndpoints, Collection<KeyValuePair<ParticipantEndpoint, McuParticipantEndpointProperties>> removedEndpoints)
        {
            return;
        }

        protected override void HandlePropertiesChanged(Microsoft.Rtc.Internal.Collaboration.PropertyMergeInformation<McuSessionProperties> pmi)
        {
           return;
        }

        protected override void HandleParticipantEndpointPropertiesChanged(ParticipantEndpoint endpoint, Microsoft.Rtc.Internal.Collaboration.PropertyMergeInformation<McuParticipantEndpointProperties> pmi)
        {
            return;
        }

        protected override void ResetCore()
        {
            return;
        }
      }
}
