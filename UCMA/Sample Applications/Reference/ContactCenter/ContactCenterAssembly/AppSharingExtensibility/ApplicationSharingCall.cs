
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

/**********************************************************************************************
Module:  ApplicationSharingCall.cs
Description: ApplicationSharingCall is a derivative of B2BCall and is used to extend the platform 
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
    /// <summary>
    /// ApplicationSharingCall is a derivative of Call that supports application sharing
    /// </summary>
	public class ApplicationSharingCall : Call
	{

            private List<string> _supportedMediaTypes = new List<string>();

            private string _defaultMediaType = null;


            #region constructors
            /// <summary>
            /// ApplicationSharing constructor to extend the support of the platform to application sharing
            /// </summary>
            public ApplicationSharingCall(Conversation conversation)
                : base(conversation)
            {
                this.InitializeMediaTypes();
                //By default advertise for replaces support
                base.IsReplacesSupported = false;
                base.IsEarlyMediaSupported = true;
            }
            #endregion


            /// <summary>
            /// Initializes the supported and default media types
            /// </summary>
            private void InitializeMediaTypes()
            {
                this._defaultMediaType = MediaType.ApplicationSharing;

                this._supportedMediaTypes = new List<string>();
                this._supportedMediaTypes.Add(MediaType.ApplicationSharing);
            }

            #region overrides
            /// <summary>
            /// Gets the supported media types.
            /// </summary>
            public override ReadOnlyCollection<string> SupportedMediaTypes
            {
                get
                {
                    return new ReadOnlyCollection<string>(new List<string>(this._supportedMediaTypes));
                }
            }

            /// <summary>
            /// Gets the default media types supported by ApplicationSharingCall
            /// </summary>
            public override string DefaultMediaType
            {
                get { return this._defaultMediaType; }
                set { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Flag indicating whether call is handling incoming refer messages.
            /// </summary>
            protected override bool CanHandleTransferReceived
            {
                get { return false; }
            }


            protected override void HandleTransferReceived(CallTransferReceivedEventArgs e)
            {
                return;

            }


            /// <summary>
            /// A method that must be implemented by the derived class to handle the notifications
            /// received for a transfer operations sent.
            /// </summary>
            /// <param name="e"></param>
            protected override void HandleTransferNotificationReceived(TransferStateChangedEventArgs e)
            {
                return;
            }


 
            protected override bool HandleFlowConfigurationRequested(MediaFlow mediaFlow)
            {
                return false;
            }           
            #endregion

	}
}
