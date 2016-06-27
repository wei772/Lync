/*=====================================================================
  File:      IContactCenterWcfService.cs
 
  Summary:   This is the service contract for the WCF endpoint.
 
******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using System.Collections.Generic;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService
{

    #region Service contracts

    /// <summary>
    /// Represents the service contract for Wcf presence services.
    /// </summary>
    [ServiceContract()]
    public interface IContactCenterWcfPresenceService
    {

        /// <summary>
        /// Method to get presence of a queue.
        /// </summary>
        /// <param name="queueName">name of the queue.</param>
        /// <returns>Presence of the queue</returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetPresence?queueName={queueName}")]
        ContactCenterEntityPresenceInformation GetQueuePresence(string queueName);

        /// <summary>
        /// Method to get presence of all available queues.
        /// </summary>
        /// <returns>Presence of the queue</returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetPresence")]
        List<ContactCenterEntityPresenceInformation> GetAvailableQueuePresence();

    }

    /// <summary>
    /// Represents the service contract Wcf Service endpoint to be used by silverlight clients.
    /// </summary>
    [ServiceContract(CallbackContract=typeof(IConversationCallback))]
    public interface IContactCenterWcfService
    {
        /// <summary>
        /// Method to report session termination by clients.
        /// </summary>
        [OperationContract(IsOneWay=true)]
        void SessionTerminated(SessionTerminationRequest request);

        /// <summary>
        /// Creates new conversation based on the given request.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(ArgumentFault))]
        [FaultContract(typeof(OperationFault))]
        CreateConversationResponse CreateConversation(CreateConversationRequest request);

        /// <summary>
        /// Starts establishing a im call based on given request.
        /// </summary>
        [OperationContract(AsyncPattern=true)]
        [FaultContract(typeof(ArgumentFault))]
        [FaultContract(typeof(OperationFault))]
        IAsyncResult BeginEstablishInstantMessagingCall(EstablishInstantMessagingCallRequest request, AsyncCallback asyncCallback, object state);
        EstablishInstantMessagingCallResponse EndEstablishInstantMessagingCall(IAsyncResult asyncResult);

        /// <summary>
        /// Send instant message on an instant messaging call.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(ArgumentFault))]
        [FaultContract(typeof(OperationFault))]
        IAsyncResult BeginSendInstantMessage(SendInstantMessageRequest request, AsyncCallback asyncCallback, object state);
        SendInstantMessageResponse EndSendInstantMessage(IAsyncResult asyncResult);

        /// <summary>
        /// Starts establishing a av call based on given request.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(ArgumentFault))]
        [FaultContract(typeof(OperationFault))]
        IAsyncResult BeginEstablishAudioVideoCall(EstablishAudioVideoCallRequest request, AsyncCallback asyncCallback, object state);
        EstablishAudioVideoCallResponse EndEstablishAudioVideoCall(IAsyncResult asyncResult);

        /// <summary>
        /// Terminates an existing conversation.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(ArgumentFault))]
        [FaultContract(typeof(OperationFault))]
        IAsyncResult BeginTerminateConversation(TerminateConversationRequest request, AsyncCallback asyncCallback, object state);
        TerminateConversationResponse EndTerminateConversation(IAsyncResult asyncResult);

        /// <summary>
        /// Method to report that client is composing a message.
        /// </summary>
        [FaultContract(typeof(ArgumentFault))]
        [OperationContract]
        void SetLocalComposingState(LocalComposingStateRequest request);
    }

    #endregion
}


