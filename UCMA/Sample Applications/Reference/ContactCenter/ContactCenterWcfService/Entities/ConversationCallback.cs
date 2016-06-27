/*=====================================================================
  File:      IConversationCallback.cs
 
  Summary:   This is the callback contract for conversation callback.
 
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{

    #region callback contracts
    /// <summary>
    /// Represents the callback contract to be used by silverlight clients to receive all callbacks from the server.
    /// </summary>
    [ServiceContract()]
    public interface IConversationCallback
    {
        [OperationContract(IsOneWay = true)]
        void RemoteComposingStatus(RemoteComposingStatusNotification remoteComposingStatusNotification);

        [OperationContract(IsOneWay = true)]
        void InstantMessageReceived(InstantMessageReceivedNotification imReceivedNotification);

        [OperationContract(IsOneWay = true)]
        void InstantMessageCallTerminated(InstantMessageCallTerminationNotification imCallTerminationNotification);

        [OperationContract(IsOneWay = true)]
        void AudioVideoCallTerminated(AudioVideoCallTerminationNotification audioVideoCallTerminationNotification);

        [OperationContract(IsOneWay = true)]
        void ConversationTerminated(ConversationTerminationNotification conversationTerminationNotification);
    }

    #endregion

}
