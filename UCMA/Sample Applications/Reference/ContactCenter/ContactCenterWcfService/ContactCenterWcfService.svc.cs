/*=====================================================================
  File:      ContactCenterWcfServiceImplementation.cs
 
  Summary:   Main service implementation of all service contracts.
 

******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;

using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;

using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Configuration;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.ContactCenterWcfService.ContextInformation;


[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: CLSCompliant(true)]

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]

    public class ContactCenterWcfServiceImplementation : IContactCenterWcfService, IContactCenterWcfPresenceService
    {

        #region private consts
        /// <summary>
        /// Content type.
        /// </summary>
        private const string ConversationContextContentType = "ConversationContext";

        #endregion

        #region private variables

        /// <summary>
        /// Application endpoint to use.
        /// </summary>
        private readonly ApplicationEndpoint m_applicationEndpoint;

        /// <summary>
        /// Web conversation manager.
        /// </summary>
        private readonly WebConversationManager m_webConversationManager;

        /// <summary>
        /// Contact center service poller.
        /// </summary>
        private readonly ContactCenterServicePoller m_servicePoller;

        /// <summary>
        /// Context information provider.
        /// </summary>
        private readonly IContextInformationProvider m_contextInformationProvider;


        /// <summary>
        /// Pending list of click to call async results.
        /// </summary>
        private readonly List<EstablishClickToCallAsyncResult> m_pendingClickToCallAsyncResults = new List<EstablishClickToCallAsyncResult>();

        /// <summary>
        /// Presence cache.
        /// </summary>
        private readonly UcPresenceCache m_presenceCache;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor to create a new contact center wcf service.
        /// </summary>
        public ContactCenterWcfServiceImplementation()
        {
            AppDomain.CurrentDomain.DomainUnload += this.CurrentDomain_DomainUnload;
            UcmaHelper ucmaHelper = UcmaHelper.GetInstance();
            ucmaHelper.Start();
            ApplicationEndpoint applicationEndpoint = ucmaHelper.ApplicationEndpoint;
            string contactCenterTrustedGruu = ucmaHelper.GetContactCenterTrustedGruu();
            m_contextInformationProvider = ContextInformationProviderFactory.GetDefaultContextInformationProvider();
            if (applicationEndpoint != null)
            {
                if(!String.IsNullOrEmpty(contactCenterTrustedGruu)) 
                {
                    LocalEndpointState currentEndpointState = applicationEndpoint.State;
                    if (currentEndpointState == LocalEndpointState.Established || currentEndpointState == LocalEndpointState.Reestablishing)
                    {
                        m_applicationEndpoint = applicationEndpoint;
                        m_applicationEndpoint.RegisterForIncomingCall<AudioVideoCall>(this.HandleIncomingAudioVideoCall);
                        m_webConversationManager = new WebConversationManager(applicationEndpoint, ucmaHelper.TimerWheel);
                        m_servicePoller = new ContactCenterServicePoller(applicationEndpoint, contactCenterTrustedGruu, ucmaHelper.TimerWheel);
                        m_servicePoller.Start();

                        //initialize the presence cache
                        m_presenceCache = new UcPresenceCache(m_applicationEndpoint);
                    }
                    else
                    {
                        throw new InvalidOperationException("Endpoint state is not valid " + currentEndpointState.ToString());
                    }
                }
                else 
                {
                    throw new InvalidOperationException("Contact center trusted gruu not available.");
                }
            }
            else
            {
                throw new InvalidOperationException("Endpoint not initialized");
            }
        }

        /// <summary>
        /// App domain unload.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            ApplicationEndpoint applicationEndpoint = m_applicationEndpoint;
            if (applicationEndpoint != null)
            {
                applicationEndpoint.UnregisterForIncomingCall<AudioVideoCall>(this.HandleIncomingAudioVideoCall);
            }
            ContactCenterServicePoller servicePoller = m_servicePoller;
            if (servicePoller != null)
            {
                servicePoller.Stop();
            }
            UcPresenceCache presenceCache = m_presenceCache;
            if (presenceCache != null)
            {
                presenceCache.CleanupCache();
            }
            WebConversationManager webConversationManager = this.WebConversationManager;
            if (webConversationManager != null)
            {
                webConversationManager.Stop();
            }

            UcmaHelper.GetInstance().Stop();
        }

        #endregion

        #region private properties

        /// <summary>
        /// Gets the application endpoint to use.
        /// </summary>
        private ApplicationEndpoint ApplicationEndpoint
        {
            get
            {
                return m_applicationEndpoint;
            }
        }

        /// <summary>
        /// Gets the context information provider. Can be null.
        /// </summary>
        private IContextInformationProvider ContextInformationProvider
        {
            get { return m_contextInformationProvider; }
        }

        /// <summary>
        /// Gets the web conversation manager associated with this service.
        /// </summary>
        private WebConversationManager WebConversationManager
        {
            get { return m_webConversationManager; }
        }
        #endregion

        #region service contract implementation

        /// <summary>
        /// Session termination reported by client.
        /// </summary>
        /// <param name="request">Session termination request.</param>
        public void SessionTerminated(SessionTerminationRequest request)
        {
            if (request != null)
            {
                List<WebConversation> clientConversations = new List<WebConversation>(request.Conversations);
                if (clientConversations != null && clientConversations.Count > 0)
                {
                    foreach (WebConversation clientConversation in clientConversations)
                    {
                        WebConversation localWebConversation = this.WebConversationManager.GetWebConversationFromId(clientConversation.Id);
                        if (localWebConversation != null)
                        {
                            BackToBackCall b2bCall = localWebConversation.BackToBackCall;
                            Conversation ucmaConversation = localWebConversation.Conversation;
                            if (b2bCall != null)
                            {
                                b2bCall.BeginTerminate(
                                                    delegate(IAsyncResult asyncResult)
                                                    {
                                                        BackToBackCall asyncStateBackToBackCall = asyncResult.AsyncState as BackToBackCall;
                                                        asyncStateBackToBackCall.EndTerminate(asyncResult);
                                                    },
                                                    b2bCall);
                            }
                            if (ucmaConversation != null)
                            {
                                ucmaConversation.BeginTerminate(
                                                        delegate(IAsyncResult asyncResult)
                                                        {
                                                            Conversation asyncStateConversation = asyncResult.AsyncState as Conversation;
                                                            asyncStateConversation.EndTerminate(asyncResult);
                                                        },
                                                        ucmaConversation);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new conversation based on given input request.
        /// </summary>
        /// <param name="request">Input request.</param>
        /// <returns>Create Conversation response.</returns>
        public CreateConversationResponse CreateConversation(CreateConversationRequest request)
        {
            CreateConversationResponse response = null;
            if (request == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidRequest, "request"));
            }
            else
            {
                IConversationCallback conversationCallback = OperationContext.Current.GetCallbackChannel<IConversationCallback>();
                if (conversationCallback == null)
                {
                    //Failed to create a callback channel.
                    throw new FaultException<OperationFault>(FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnableToCreateCallbackChannel, null /*innerException*/));
                }
                else
                {
                    //Successfully created a callback channel. 
                    WebConversation webConversation = this.WebConversationManager.CreateNewWebConversation(request, conversationCallback, OperationContext.Current.Channel);
                    response = new CreateConversationResponse(request, webConversation);
                }
            }

            return response;
        }

        /// <summary>
        /// Establishes an instant messaging call.
        /// </summary>
        /// <param name="request">establish call request.</param>
        /// <param name="asyncCallback">User callback.</param>
        /// <param name="state">User state.</param>
        /// <returns>IAsyncresult.</returns>
        public IAsyncResult BeginEstablishInstantMessagingCall(EstablishInstantMessagingCallRequest request, AsyncCallback asyncCallback, object state)
        {
            if (request == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidRequest, "request"));
            }
            else if (request.Conversation == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NullConversation, "request"));
            }
            else
            {
                //First try to map the right conversation.
                WebConversation webConversation = this.WebConversationManager.GetWebConversationFromId(request.Conversation.Id);


                if (webConversation == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidConversation, "request"));
                }
                else
                {
                    //Create IM call.
                    InstantMessagingCall imCall = new InstantMessagingCall(webConversation.Conversation);
                    //Wrap Im call as a web imcall
                    WebImCall webImcall = new WebImCall(imCall, webConversation);
                    this.RegisterInstantMessagingCallEventHandlers(imCall);

                    //Set IM app context.
                    imCall.ApplicationContext = new InstantMessagingCallContext(webImcall);

                    //Get custom MIME part.
                    IContextInformationProvider contextInformationProvider = this.ContextInformationProvider;
                    MimePartContentDescription customMimePart = null;
                    if (contextInformationProvider != null)
                    {
                        customMimePart = contextInformationProvider.GenerateContextMimePartContentDescription(webConversation.ConversationContext);
                    }

                    string destinationUri = this.GetUriFromQueueName(request.Destination);

                    //Create a new async result. 
                    var establishImCallAsyncResult = new EstablishInstantMessagingCallAsyncResult(request, webConversation, destinationUri, imCall, customMimePart, asyncCallback, state);
                    establishImCallAsyncResult.Process();

                    return establishImCallAsyncResult;
                }
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        /// <returns>EstablishInstantMessagingCallResponse.</returns>
        public EstablishInstantMessagingCallResponse EndEstablishInstantMessagingCall(IAsyncResult asyncResult)
        {
            EstablishInstantMessagingCallResponse response = null;
            if (asyncResult == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
            }
            else
            {
                EstablishInstantMessagingCallAsyncResult establishAsyncResult = asyncResult as EstablishInstantMessagingCallAsyncResult;
                if (establishAsyncResult == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
                }
                else
                {
                    response = establishAsyncResult.EndInvoke();
                }
            }
            return response;
        }


        /// <summary>
        /// Establishes an audio video call.
        /// </summary>
        /// <param name="request">establish call request.</param>
        /// <param name="asyncCallback">User callback.</param>
        /// <param name="state">User state.</param>
        /// <returns>IAsyncresult.</returns>
        public IAsyncResult BeginEstablishAudioVideoCall(EstablishAudioVideoCallRequest request, AsyncCallback asyncCallback, object state)
        {
            if (request == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidRequest, "request"));
            }
            else if (request.Conversation == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NullConversation, "request"));
            }
            else
            {
                //First try to map the right conversation.
                WebConversation webConversation = this.WebConversationManager.GetWebConversationFromId(request.Conversation.Id);


                if (webConversation == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidConversation, "request"));
                }
                else
                {

                    AudioVideoCall avCall = null;
                    AsyncResultWithProcess<EstablishAudioVideoCallResponse> asyncResult = null;

                    if (!string.IsNullOrEmpty(request.CallbackPhoneNumber))
                    {
                        //Click to call case.
                        //Create callback call.
                        Conversation tempConversation = new Conversation(m_applicationEndpoint);
                        AudioVideoCall callbackAvCall = new AudioVideoCall(tempConversation);

                        var establishClickToCallAsyncResult = new EstablishClickToCallAsyncResult(request, callbackAvCall, webConversation, this.ClickToCallAsyncResultCompleting /*action delegate*/, asyncCallback, state);
                        //Store this async result for self transfer cases.
                        lock (m_pendingClickToCallAsyncResults)
                        {
                            m_pendingClickToCallAsyncResults.Add(establishClickToCallAsyncResult);
                        }
                        asyncResult = establishClickToCallAsyncResult;
                    }
                    else
                    {
                        //Direct av call case.
                        //Create AV call.
                        avCall = new AudioVideoCall(webConversation.Conversation);

                        //Create web av call and stamp as app context.
                        WebAvCall webAvCall = new WebAvCall(avCall, webConversation);

                        //Set AV app context.
                        avCall.ApplicationContext = new AudioVideoCallContext(webAvCall);

                        //Get custom MIME part.
                        IContextInformationProvider contextInformationProvider = this.ContextInformationProvider;
                        MimePartContentDescription customMimePart = null;
                        if (contextInformationProvider != null)
                        {
                            customMimePart = contextInformationProvider.GenerateContextMimePartContentDescription(webConversation.ConversationContext);
                        }
                        string destinationUri = this.GetUriFromQueueName(request.Destination);
                        asyncResult = new EstablishAudioVideoCallAsyncResult(request, avCall, destinationUri, webConversation, customMimePart, asyncCallback, state);
                    }

                    asyncResult.Process();

                    return asyncResult;
                }
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        /// <returns>EstablishAudioVideoCallResponse.</returns>
        public EstablishAudioVideoCallResponse EndEstablishAudioVideoCall(IAsyncResult asyncResult)
        {
            EstablishAudioVideoCallResponse response = null;
            if (asyncResult == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
            }
            else
            {
                AsyncResult<EstablishAudioVideoCallResponse> establishAsyncResult = asyncResult as AsyncResult<EstablishAudioVideoCallResponse>;
                if (establishAsyncResult == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
                }
                else
                {
                    response = establishAsyncResult.EndInvoke();
                }
            }
            return response;
        }


        /// <summary>
        /// Sets local composing state.
        /// </summary>
        /// <param name="request">Request.</param>
        public void SetLocalComposingState(LocalComposingStateRequest request)
        {
            //First try to map the right conversation.
            WebConversation webConversation = this.WebConversationManager.GetWebConversationFromId(request.Conversation.Id);

            if (webConversation == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidConversation, "request"));
            }
            else if (webConversation.WebImCall == null || webConversation.WebImCall.ImCall == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NoImCall, "request"));
            }
            else
            {
                InstantMessagingFlow imFlow = webConversation.WebImCall.ImCall.Flow;

                if (imFlow == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.ImFlowNotActive, "request"));
                }
                else
                {
                    imFlow.LocalComposingState = ComposingState.Composing;
                }
            }
        }

        /// <summary>
        /// Sends an instant message on the active instant messaging call on the given conversation.
        /// </summary>
        /// <param name="request">Send im message request.</param>
        /// <param name="asyncCallback">User callback.</param>
        /// <param name="state">User state.</param>
        /// <returns>IAsyncresult.</returns>
        public IAsyncResult BeginSendInstantMessage(SendInstantMessageRequest request, AsyncCallback asyncCallback, object state)
        {
            if (request == null || request.Message == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidRequest, "request"));
            }
            else if (request.Conversation == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NullConversation, "request"));
            }
            else
            {
                //First try to map the right conversation.
                WebConversation webConversation = this.WebConversationManager.GetWebConversationFromId(request.Conversation.Id);

                if (webConversation == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidConversation, "request"));
                }
                else if (webConversation.WebImCall == null || webConversation.WebImCall.ImCall == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NoImCall, "request"));
                }
                else
                {
                    InstantMessagingFlow imFlow = webConversation.WebImCall.ImCall.Flow;

                    if (imFlow == null)
                    {
                        throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.ImFlowNotActive, "request"));
                    }
                    else
                    {
                        //Create a new async result. 
                        var sendImMessageAsyncResult = new SendInstantMessageAsyncResult(request, imFlow, request.Message, asyncCallback, state);
                        sendImMessageAsyncResult.Process();
                        return sendImMessageAsyncResult;
                    }
                }
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        /// <returns>SendInstantMessageResponse.</returns>
        public SendInstantMessageResponse EndSendInstantMessage(IAsyncResult asyncResult)
        {
            SendInstantMessageResponse response = null;
            if (asyncResult == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
            }
            else
            {
                SendInstantMessageAsyncResult sendMessageAsyncResult = asyncResult as SendInstantMessageAsyncResult;
                if (sendMessageAsyncResult == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
                }
                else
                {
                    response = sendMessageAsyncResult.EndInvoke();
                }
            }
            return response;
        }



        /// <summary>
        /// Terminates a conversation.
        /// </summary>
        /// <param name="request">conversation termination.</param>
        /// <param name="asyncCallback">User callback.</param>
        /// <param name="state">User state.</param>
        /// <returns>IAsyncresult.</returns>
        public IAsyncResult BeginTerminateConversation(TerminateConversationRequest request, AsyncCallback asyncCallback, object state)
        {
            if (request == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidRequest, "request"));
            }
            else if (request.Conversation == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.NullConversation, "request"));
            }
            else
            {
                //First try to map the right conversation.
                WebConversation webConversation = this.WebConversationManager.GetWebConversationFromId(request.Conversation.Id);


                if (webConversation == null)
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidConversation, "request"));
                }
                else
                {
                    Conversation ucmaConversation = webConversation.Conversation;
                    Debug.Assert(null != ucmaConversation, "Ucma conversation cannot be null");
                    AsyncResult<TerminateConversationResponse> tempAsyncResult = new AsyncResult<TerminateConversationResponse>(asyncCallback, state);
                    Pair<WebConversation, AsyncResult<TerminateConversationResponse>> tempState = new Pair<WebConversation, AsyncResult<TerminateConversationResponse>>(webConversation, tempAsyncResult);
                    return webConversation.BeginTerminate(request, this.WebConversationTerminatedInternal, tempState/*state*/);
                }
            }
        }

        /// <summary>
        /// Internal callback method.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void WebConversationTerminatedInternal(IAsyncResult asyncResult)
        {
            Pair<WebConversation, AsyncResult<TerminateConversationResponse>> tempState = asyncResult.AsyncState as Pair<WebConversation, AsyncResult<TerminateConversationResponse>>;
            try
            {
                TerminateConversationResponse response = tempState.First.EndTerminate(asyncResult);
                tempState.Second.SetAsCompleted(response, false);
            }
            catch (RealTimeException rte)
            {
                tempState.Second.SetAsCompleted(rte, false);
            }
            catch (Exception e)
            {
                tempState.Second.SetAsCompleted(e, false);
            }
        }

        /// <summary>
        /// Waits for corresponding begin operation to complete.
        /// </summary>
        /// <param name="asyncResult">Async result from the corresponding begin method.</param>
        /// <returns>TerminateConversationResponse.</returns>
        public TerminateConversationResponse EndTerminateConversation(IAsyncResult asyncResult)
        {
            TerminateConversationResponse response = null;
            if (asyncResult == null)
            {
                throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
            }
            else
            {
                AsyncResult<TerminateConversationResponse> tempAsyncResult = asyncResult as AsyncResult<TerminateConversationResponse>;
                if (tempAsyncResult != null)
                {
                    response = tempAsyncResult.EndInvoke();
                }
                else
                {
                    throw new FaultException<ArgumentFault>(FaultHelper.CreateArgumentFault(FailureStrings.GenericFailures.InvalidAsyncResult, "asyncResult"));
                }
            }
            return response;
        }

        /// <summary>
        /// Gets presence for a specific queue.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns>Presence of specific queue name.</returns>
        public ContactCenterEntityPresenceInformation GetQueuePresence(string queueName)
        {

            ContactCenterEntityPresenceInformation retVal = null;

            string destinationUri = this.GetUriFromQueueName(queueName);
            UcPresenceCache presenceCache = m_presenceCache;
            if (!String.IsNullOrEmpty(destinationUri) && presenceCache != null)
            {
                var presenceInformation = presenceCache.GetPresence(destinationUri);
                if (presenceInformation != null)
                {
                    retVal = new ContactCenterEntityPresenceInformation(queueName, presenceInformation);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Gets presence for all available queues.
        /// </summary>
        /// <returns>Presence of all available queues.</returns>
        public List<ContactCenterEntityPresenceInformation> GetAvailableQueuePresence()
        {

            List<ContactCenterEntityPresenceInformation> presenceInformationList = new List<ContactCenterEntityPresenceInformation>();

            var poller = m_servicePoller;
            UcPresenceCache presenceCache = m_presenceCache;
            if (poller != null && presenceCache != null)
            {
                ContactCenterInformation ccInfo = poller.ContactCenterInformation;
                if (ccInfo != null)
                {
                    List<string> allQueueNames = ccInfo.GetAllAvailableQueueNames();
                    foreach (string queueName in allQueueNames)
                    {
                        string destinationUri = this.GetUriFromQueueName(queueName);
                        if (!String.IsNullOrEmpty(destinationUri))
                        {
                            var presenceInformation = presenceCache.GetPresence(destinationUri);
                            if (presenceInformation != null)
                            {
                                presenceInformationList.Add(new ContactCenterEntityPresenceInformation(queueName, presenceInformation));
                            }
                        }
                    }
                }
            }

            return presenceInformationList.OrderBy(a => a.Availability).ThenBy(n => n.EntityName).ToList();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Get uri from queue name.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <returns>Uri value corresponding to the queue name, if available.</returns>
        private string GetUriFromQueueName(string queueName)
        {
            var poller = m_servicePoller;
            string retVal = queueName;
            if (poller != null)
            {
                retVal = poller.GetUriFromQueueName(queueName);
            }
            return retVal;
        }

        /// <summary>
        /// Conversation termination completed.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ConversationTerminationCompleted(IAsyncResult asyncResult)
        {
            var asyncState = asyncResult.AsyncState as Triple<Conversation, AsyncResult<TerminateConversationResponse>, TerminateConversationRequest>;
            Debug.Assert(null != asyncState, "Async state is null");

            Conversation conversation = asyncState.First;
            AsyncResult<TerminateConversationResponse> terminateConversationAsyncResult = asyncState.Second;
            TerminateConversationRequest request = asyncState.Third;

            bool exceptionEncountered = true;
            try
            {
                conversation.EndTerminate(asyncResult);
                exceptionEncountered = false;
            }
            finally
            {
                if (exceptionEncountered)
                {
                    OperationFault operationFault = FaultHelper.CreateServerOperationFault(FailureStrings.GenericFailures.UnexpectedException, null /*innerException*/);
                    terminateConversationAsyncResult.SetAsCompleted(new FaultException<OperationFault>(operationFault), false /*completedSynchronously*/);
                }
                else
                {
                    TerminateConversationResponse response = new TerminateConversationResponse(request);
                    terminateConversationAsyncResult.SetAsCompleted(response, false /*completedSynchronously*/);
                }
            }
        }

        /// <summary>
        /// Callback method for success IMDN.
        /// </summary>
        /// <param name="result">Async result.</param>
        private void SendSuccessDeliveryNotificationCompleted(IAsyncResult result)
        {
            bool unhandledExceptionOccured = true;
            try
            {
                InstantMessagingFlow imFlow = result.AsyncState as InstantMessagingFlow;
                Debug.Assert(null != imFlow, "Async state is null");
                imFlow.EndSendSuccessDeliveryNotification(result);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    Helper.Logger.Error("Unhandled exception occured while completing success IMDN operation.");
                }
            }
        }

        /// <summary>
        /// Callback method for failure IMDN.
        /// </summary>
        /// <param name="result">Async result.</param>
        private void SendFailureDeliveryNotificationCompleted(IAsyncResult result)
        {
            bool unhandledExceptionOccured = true;
            try
            {
                InstantMessagingFlow imFlow = result.AsyncState as InstantMessagingFlow;
                Debug.Assert(null != imFlow, "Async state is null");
                imFlow.EndSendFailureDeliveryNotification(result);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    Helper.Logger.Error("Unhandled exception occured while completing failure IMDN operation.");
                }
            }
        }

        /// <summary>
        /// Action delegate method when click to call async result is completing.
        /// </summary>
        private void ClickToCallAsyncResultCompleting(EstablishClickToCallAsyncResult asyncResult)
        {
            lock (m_pendingClickToCallAsyncResults)
            {
                m_pendingClickToCallAsyncResults.Remove(asyncResult);
            }
        }


        /// <summary>
        /// Register b2b call event handlers.
        /// </summary>
        /// <param name="b2bCall">Back to back call.</param>
        private void RegisterBackToBackCallEventHandlers(BackToBackCall b2bCall)
        {
            Debug.Assert(null != b2bCall, "B2B call is not expected to be null here");
            b2bCall.StateChanged += this.B2BCall_StateChanged;
        }


        /// <summary>
        /// Unregsiter b2b call event handlers.
        /// </summary>
        /// <param name="avCall">Audio Video call.</param>
        private void UnregisterBackToBackCallEventHandlers(BackToBackCall b2bCall)
        {
            Debug.Assert(null != b2bCall, "B2B call is not expected to be null here");
            b2bCall.StateChanged -= this.B2BCall_StateChanged;
        }

        /// <summary>
        /// Register im call event handlers.
        /// </summary>
        /// <param name="imCall">Instant messaging call.</param>
        private void RegisterInstantMessagingCallEventHandlers(InstantMessagingCall imCall)
        {
            Debug.Assert(null != imCall, "Instant messaging call is not expected to be null here");
            imCall.StateChanged += this.ImCall_StateChanged;
            imCall.InstantMessagingFlowConfigurationRequested += this.ImCall_InstantMessagingFlowConfigurationRequested;
        }


        /// <summary>
        /// Unregsiter im call event handlers.
        /// </summary>
        /// <param name="imCall">Instant messaging call.</param>
        private void UnregisterInstantMessagingCallEventHandlers(InstantMessagingCall imCall)
        {
            Debug.Assert(null != imCall, "Instant messaging call is not expected to be null here");
            imCall.StateChanged -= this.ImCall_StateChanged;
            imCall.InstantMessagingFlowConfigurationRequested -= this.ImCall_InstantMessagingFlowConfigurationRequested;
        }

        /// <summary>
        /// Register im flow event handlers.
        /// </summary>
        /// <param name="imFlow">Instant messaging flow.</param>
        private void RegisterInstantMessagingFlowEventHandlers(InstantMessagingFlow imFlow)
        {
            Debug.Assert(null != imFlow, "Instant messaging flow is not expected to be null here");
            imFlow.StateChanged += this.ImFlow_StateChanged;
            imFlow.MessageReceived += this.ImFlow_MessageReceived;
            imFlow.RemoteComposingStateChanged += this.ImFlow_RemoteComposingStateChanged;
        }

        /// <summary>
        /// Unregsiter im flow event handlers.
        /// </summary>
        /// <param name="imFlow">Instant messaging flow.</param>
        private void UnregisterInstantMessagingFlowEventHandlers(InstantMessagingFlow imFlow)
        {
            Debug.Assert(null != imFlow, "Instant messaging flow is not expected to be null here");
            imFlow.StateChanged -= this.ImFlow_StateChanged;
            imFlow.MessageReceived -= this.ImFlow_MessageReceived;
            imFlow.RemoteComposingStateChanged -= this.ImFlow_RemoteComposingStateChanged;

        }

        #endregion

        #region ucma event handlers

        #region incoming av call handler
        /// <summary>
        /// Handles incoming audio video call.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="eventArgs">Event args.</param>
        private void HandleIncomingAudioVideoCall(object sender, CallReceivedEventArgs<AudioVideoCall> eventArgs)
        {
            //This method handles only incoming self transfer calls.
            bool declineCall = true;
            if (eventArgs.CallToBeReplaced != null)
            {
                string replacesCallId = eventArgs.CallToBeReplaced.CallId;
                EstablishClickToCallAsyncResult pendingAsyncResult = null;

                //Check if we have the self transfer context.
                lock (m_pendingClickToCallAsyncResults)
                {
                    foreach (EstablishClickToCallAsyncResult asyncResult in m_pendingClickToCallAsyncResults)
                    {
                        if (asyncResult.CallbackCall.CallId.Equals(replacesCallId, StringComparison.Ordinal))
                        {
                            pendingAsyncResult = asyncResult;
                            break;
                        }
                    }
                }

                if (pendingAsyncResult != null)
                {
                    AudioVideoCall incomingAvCall = eventArgs.Call;
                    WebConversation webConversation = pendingAsyncResult.WebConversation;
                    Debug.Assert(null != webConversation, "Web conversation is null");

                    //Create b2b call.
                    BackToBackCallSettings incomingAvCallSettings = new BackToBackCallSettings(incomingAvCall);

                    //Create web av call as app context.
                    WebAvCall webAvCall = new WebAvCall(incomingAvCall, webConversation);

                    //Stamp the context.
                    incomingAvCall.ApplicationContext = new AudioVideoCallContext(webAvCall);

                    //Create an idle av call.
                    AudioVideoCall idleAvCall = new AudioVideoCall(webConversation.Conversation);
                    string destinationUri = this.GetUriFromQueueName(pendingAsyncResult.DestinationFromRequest);
                    BackToBackCallSettings idleAvCallSettings = new BackToBackCallSettings(idleAvCall, destinationUri);
                    BackToBackCall b2bCall = new BackToBackCall(incomingAvCallSettings, idleAvCallSettings);
                    this.RegisterBackToBackCallEventHandlers(b2bCall);

                    pendingAsyncResult.EstablishBackToBackCall(b2bCall);
                    declineCall = false;
                }
            }

            if (declineCall)
            {
                try
                {
                    eventArgs.Call.Decline();
                }
                catch (InvalidOperationException ioe)
                {
                    Helper.Logger.Info("IOE while declining incoming call {0}", EventLogger.ToString(ioe));
                }
                catch (RealTimeException rte)
                {
                    Helper.Logger.Info("RTE while declining incoming call {0}", EventLogger.ToString(rte));
                }
            }
        }
        #endregion

        #region im call event handlers

        /// <summary>
        /// Im call flow configuration requested event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImCall_InstantMessagingFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            this.RegisterInstantMessagingFlowEventHandlers(e.Flow);
        }
        /// <summary>
        /// Im call state changed event handlers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            if (e.State == CallState.Terminating)
            {
                InstantMessagingCall imCall = sender as InstantMessagingCall;

                //Find the right callback.
                InstantMessagingCallContext imCallContext = imCall.ApplicationContext as InstantMessagingCallContext;
                WebImCall webImCall = imCallContext.WebImcall;

                InstantMessageCallTerminationNotification imCallTerminationNotification = new InstantMessageCallTerminationNotification();
                imCallTerminationNotification.ImCall = webImCall;

                IConversationCallback convCallback = WebConversationManager.GetActiveConversationCallback(webImCall.WebConversation);
                if (convCallback != null)
                {
                    try
                    {
                        convCallback.InstantMessageCallTerminated(imCallTerminationNotification);
                    }
                    catch (System.TimeoutException)
                    {
                    }

                }

                this.UnregisterInstantMessagingCallEventHandlers(imCall);
            }
        }

        /// <summary>
        /// B2B call state changed event handlers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void B2BCall_StateChanged(object sender, BackToBackCallStateChangedEventArgs e)
        {
            if (e.State == BackToBackCallState.Terminating)
            {
                BackToBackCall b2bCall = sender as BackToBackCall;

                //Find the right callback.
                AudioVideoCallContext avCallContext = b2bCall.Call1.ApplicationContext as AudioVideoCallContext;
                WebAvCall webAvCall = avCallContext.WebAvcall;

                AudioVideoCallTerminationNotification avCallTerminationNotification = new AudioVideoCallTerminationNotification();
                avCallTerminationNotification.AvCall = webAvCall;

                IConversationCallback convCallback = WebConversationManager.GetActiveConversationCallback(webAvCall.WebConversation);
                if (convCallback != null)
                {
                    try
                    {
                        convCallback.AudioVideoCallTerminated(avCallTerminationNotification);
                    }
                    catch (System.TimeoutException)
                    {
                    }
                }

                this.UnregisterBackToBackCallEventHandlers(b2bCall);
            }
        }
        #endregion

        #region im flow event handlers


        /// <summary>
        /// Instant message received handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImFlow_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            //Find the right callback.
            InstantMessagingFlow imFlow = sender as InstantMessagingFlow;
            InstantMessagingCall imCall = imFlow.Call;
            InstantMessagingCallContext imCallContext = imCall.ApplicationContext as InstantMessagingCallContext;
            WebImCall webImCall = imCallContext.WebImcall;

            InstantMessageReceivedNotification imReceivedNotification = new InstantMessageReceivedNotification();
            imReceivedNotification.ImCall = webImCall;
            string senderName = null;
            if (!String.IsNullOrEmpty(e.Sender.DisplayName))
            {
                senderName = e.Sender.DisplayName;
            }
            else
            {
                senderName = e.Sender.UserAtHost;
            }
            imReceivedNotification.MessageSender = senderName;
            imReceivedNotification.MessageReceived = e.TextBody;

            bool successfullyReportedMessageToCustomer = false;
            int failureResponseCodeToSend = ResponseCode.ServiceUnavailable;
            IConversationCallback convCallback = WebConversationManager.GetActiveConversationCallback(webImCall.WebConversation);
            if (convCallback != null)
            {
                try
                {
                    convCallback.InstantMessageReceived(imReceivedNotification);
                    successfullyReportedMessageToCustomer = true;
                }
                catch (System.TimeoutException)
                {
                    failureResponseCodeToSend = ResponseCode.ServerTimeout;
                }
            }

            bool unhandledExceptionOccured = true;
            try
            {
                if (successfullyReportedMessageToCustomer)
                {
                    //Send success IMDN.
                    imFlow.BeginSendSuccessDeliveryNotification(e.MessageId, this.SendSuccessDeliveryNotificationCompleted, imFlow /*asyncState*/);
                }
                else
                {
                    //Send failure IMDN.
                    imFlow.BeginSendFailureDeliveryNotification(e.MessageId, failureResponseCodeToSend, this.SendFailureDeliveryNotificationCompleted, imFlow /*asyncState*/);
                }
                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(ioe));
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    Helper.Logger.Error("Unhandled exception occured while completing IMDN operation.");
                }
            }
        }


        /// <summary>
        /// Instant messaging flow state changed event handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Terminated)
            {
                InstantMessagingFlow imFlow = sender as InstantMessagingFlow;
                this.UnregisterInstantMessagingFlowEventHandlers(imFlow);
            }
        }


        /// <summary>
        /// Keyboard notification
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ImFlow_RemoteComposingStateChanged(object sender, ComposingStateChangedEventArgs e)
        {
            //Find the right callback.
            InstantMessagingFlow imFlow = sender as InstantMessagingFlow;
            InstantMessagingCall imCall = imFlow.Call;
            InstantMessagingCallContext imCallContext = imCall.ApplicationContext as InstantMessagingCallContext;
            WebImCall webImCall = imCallContext.WebImcall;

            RemoteComposingStatusNotification remoteComposingStatusNotification = new RemoteComposingStatusNotification();
            remoteComposingStatusNotification.ImCall = webImCall;

            //Populate composing status.
            RemoteComposingStatus remoteComposingStatus = RemoteComposingStatus.Idle;
            if (e.ComposingState == ComposingState.Composing)
            {
                remoteComposingStatus = RemoteComposingStatus.Active;
            }
            else if (e.ComposingState == ComposingState.Idle)
            {
                remoteComposingStatus = RemoteComposingStatus.Idle;
            }

            remoteComposingStatusNotification.RemoteComposingStatus = remoteComposingStatus;

            //Populate participant details.
            string remoteParticipantStr = string.Empty;
            ConversationParticipant remoteParticipant = e.Participant;
            if (remoteParticipant != null)
            {
                if (!String.IsNullOrEmpty(remoteParticipant.DisplayName))
                {
                    remoteParticipantStr = remoteParticipant.DisplayName;
                }
                else
                {
                    remoteParticipantStr = remoteParticipant.Uri;
                }
            }
            remoteComposingStatusNotification.Participant = remoteParticipantStr;


            IConversationCallback convCallback = WebConversationManager.GetActiveConversationCallback(webImCall.WebConversation);
            if (convCallback != null)
            {
                try
                {
                    convCallback.RemoteComposingStatus(remoteComposingStatusNotification);
                }
                catch (System.TimeoutException)
                {
                }
            }

        }

        #endregion
        #endregion
    }
}
