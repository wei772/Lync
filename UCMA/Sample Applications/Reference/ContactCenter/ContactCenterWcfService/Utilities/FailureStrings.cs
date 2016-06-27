/*=====================================================================
  File:      FailureStrings.cs
 
  Summary:   Represents failure string constants.
 

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{

    /// <summary>
    /// Represents a class with failure string constants.
    /// </summary>
    internal static class FailureStrings
    {
        #region generic failures

        internal static class GenericFailures
        {
            /// <summary>
            /// Given operation request is invalid.
            /// </summary>
            internal const string InvalidRequest = "Given request is invalid.";

            /// <summary>
            /// Unable to create callback channel.
            /// </summary>
            internal const string UnableToCreateCallbackChannel = "Unable to create a callback channel/";

            /// <summary>
            /// Given conversation in operation request is null.
            /// </summary>
            internal const string NullConversation = "No Conversation is provided in the request.";

            /// <summary>
            /// Given request does not have callback number.
            /// </summary>
            internal const string InvalidCallbackNumber = "Given callback number is invalid.";
            
            /// <summary>
            /// Given conversation in operation request is invalid.
            /// </summary>
            internal const string InvalidConversation = "Conversation provided in the request is invalid.";

            /// <summary>
            /// No instant messaging call is found in the given conversation.
            /// </summary>
            internal const string NoImCall = "No instant messaging call is found in the given conversation.";

            /// <summary>
            /// Im flow is not active.
            /// </summary>
            internal const string ImFlowNotActive = "Instant messaging call flow is not active.";


            /// <summary>
            /// Given async result is invalid.
            /// </summary>
            internal const string InvalidAsyncResult = "Given async result is invalid";


            /// <summary>
            /// Unexpected exception encountered.
            /// </summary>
            internal const string UnexpectedException = "Unexpected exception encountered.";
            
            /// <summary>
            /// Message delivery failed.
            /// </summary>
            internal const string MessageDeliveryFailed = "Message delivery failed.";

        }

        #endregion

    }
}