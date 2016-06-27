/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastHelp.Logging;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Signaling;

namespace FastHelpServer
{

    /// <summary>
    /// Represents an anchor for customer calls
    /// </summary>
    public class CustomerCallAnchor
    {

        #region private variables

        /// <summary>
        /// Customer conversation.
        /// </summary>
        private Conversation customerConversation;

        /// <summary>
        /// Customer session.
        /// </summary>
        private CustomerSession customerSession;

        /// <summary>
        /// Remote participant.
        /// </summary>
        private ConversationParticipant customerRemoteParticipant;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Pending establish async result.
        /// </summary>
        private EstablishAsyncResult pendingEstablishAsyncResult;

        /// <summary>
        /// Trusted conversation.
        /// </summary>
        private Conversation trustedConversation;

        /// <summary>
        /// Lock object.
        /// </summary>
        private readonly object syncRoot = new object();
        #endregion

        #region constructors

        /// <summary>
        /// To create a new customer call anchor.
        /// </summary>
        /// <param name="customerSession">Customer session.</param>
        /// <param name="customerConversation">Customer conversation.</param>
        public CustomerCallAnchor(CustomerSession customerSession, ILogger logger, Conversation customerConversation)
        {
            this.customerConversation = customerConversation;
            this.customerRemoteParticipant = customerConversation.RemoteParticipants[0];
            this.customerSession = customerSession;
            this.logger = logger;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the customer uri.
        /// </summary>
        public string CustomerUri
        {
            get { return this.customerSession.CustomerUri; }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Establishes the call anchor for a customer session.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginEstablish(RealTimeAddress helpdeskNumber, AsyncCallback callback, object state)
        {
            EstablishAsyncResult establishAsyncResult = null;

            if (helpdeskNumber == null)
            {
                throw new ArgumentNullException("helpdeskNumber");
            }
            lock (this.syncRoot)
            {
                if (this.pendingEstablishAsyncResult != null)
                {
                    throw new InvalidOperationException("Already a pending async result is in progress");
                }
                if (this.customerRemoteParticipant == null)
                {
                    throw new InvalidOperationException("Customer has already left the conversation");
                }

                if (this.trustedConversation == null)
                {
                    ConversationSettings settings = new ConversationSettings(ConversationPriority.Normal, "Dialout", "<" + this.CustomerUri + ">");
                    this.trustedConversation = new Conversation(this.customerConversation.Endpoint, settings);

                    // Config change: We cannot impersonate with user endpoint.
                    if (!Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["UseUserEndPoint"]))
                        this.trustedConversation.Impersonate(this.customerRemoteParticipant.Uri, this.customerRemoteParticipant.PhoneUri, this.customerRemoteParticipant.DisplayName);
                    this.RegisterTrustedConversationEventHandlers(this.trustedConversation);
                }

                establishAsyncResult = new EstablishAsyncResult(this, this.trustedConversation, this.customerConversation, helpdeskNumber, this.logger, callback, state);
                this.pendingEstablishAsyncResult = establishAsyncResult;
            }

            establishAsyncResult.Process();

            return establishAsyncResult;
        }

        /// <summary>
        /// Ends the establishment process.
        /// </summary>
        /// <param name="asyncResult"></param>
        public void EndEstablish(IAsyncResult asyncResult)
        {
            EstablishAsyncResult establishAsyncResult = asyncResult as EstablishAsyncResult;
            establishAsyncResult.EndInvoke();
        }

        /// <summary>
        /// Terminates the call anchor.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginTerminate(AsyncCallback callback, object state)
        {
            TerminateAsyncResult terminateAsyncResult = new TerminateAsyncResult(this, this.trustedConversation, this.logger, callback, state);
            terminateAsyncResult.Process();

            return terminateAsyncResult;
        }

        /// <summary>
        /// Completes the termination operation.
        /// </summary>
        /// <param name="asyncResult"></param>
        public void EndTerminate(IAsyncResult asyncResult)
        {
            TerminateAsyncResult terminateAsyncResult = asyncResult as TerminateAsyncResult;
            terminateAsyncResult.EndInvoke();
        }

        /// <summary>
        /// Processes incoming dialout call.
        /// </summary>
        /// <param name="avCall"></param>
        public void ProcessIncomingDialOutCall(AudioVideoCall avCall)
        {
            lock (this.syncRoot)
            {
                if (this.pendingEstablishAsyncResult != null)
                {
                    this.pendingEstablishAsyncResult.ProcessIncomingDialOutCall(avCall);
                }
                else
                {
                    try
                    {
                        Console.WriteLine("No pending estalblishment process. Declining call");
                        this.logger.Log("No pending estalblishment process. Declining call");
                        avCall.Decline();
                    }
                    catch (RealTimeException rte)
                    {
                        Console.WriteLine("Decline failed with {0}", rte);
                        this.logger.Log("Decline failed with {0}", rte);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        Console.WriteLine("Decline failed with {0}", ioe);
                        this.logger.Log("Decline failed with {0}", ioe);
                    }
                }
            }
        }

        /// <summary>
        /// Clears pending estalbish async result.
        /// </summary>
        internal void HandleEstablishmentCompletion(Exception e)
        {
            lock (this.syncRoot)
            {
                this.pendingEstablishAsyncResult = null;
                if (e != null)
                {
                    this.BeginTerminate((asyncResult) =>
                    {
                        this.EndTerminate(asyncResult);
                    },
                    null);
                }
            }
        }

        /// <summary>
        /// Handle termination completion.
        /// </summary>
        internal void HandleTerminationCompletion()
        {
            lock (this.syncRoot)
            {
                this.trustedConversation = null;
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Register event handlers.
        /// </summary>
        /// <param name="conversation"></param>
        private void RegisterTrustedConversationEventHandlers(Conversation conversation)
        {
            conversation.StateChanged += this.TrustedConversation_StateChanged;
            conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged += this.AudioVideoMcuSession_ParticipantEndpointAttendanceChanged;
        }

        /// <summary>
        /// Unregister event handlers.
        /// </summary>
        /// <param name="conversation"></param>
        private void UnregisterTrustedConversationEventHandlers(Conversation conversation)
        {
            conversation.StateChanged -= this.TrustedConversation_StateChanged;
            conversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged -= this.AudioVideoMcuSession_ParticipantEndpointAttendanceChanged;
        }

        // avmcu attendance changed.
        private void AudioVideoMcuSession_ParticipantEndpointAttendanceChanged(object sender, ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> e)
        {
            // If any participant leaves we tear down the conversation.
            if (e.Left.Count > 0)
            {
                bool needTermination = false;
                foreach (var kv in e.Left)
                {
                    if(kv.Key.Participant.RosterVisibility == ConferencingRosterVisibility.Visible)
                    {
                        needTermination = true;
                        break;
                    }
                }

                if (needTermination)
                {
                    this.BeginTerminate((asyncResult) =>
                    {
                        this.EndTerminate(asyncResult);
                    },
                    null);
                }
            }
        }

        /// <summary>
        /// State changed handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrustedConversation_StateChanged(object sender, StateChangedEventArgs<ConversationState> e)
        {
            Conversation conversation = sender as Conversation;
            if (e.State == ConversationState.Terminated)
            {
                this.UnregisterTrustedConversationEventHandlers(conversation);
            }
        }

        /// <summary>
        /// Attendance changed monitor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrustedConversation_RemoteParticipantAttendanceChanged(object sender, ParticipantAttendanceChangedEventArgs e)
        {
            // If any participant leaves we tear down the conversation.
            if (e.Removed.Count > 0)
            {
                bool needTermination = false;
                foreach (var participant in e.Removed)
                {
                    if (participant.RosterVisibility == ConferencingRosterVisibility.Visible)
                    {
                        needTermination = true;
                        break;
                    }
                }

                if (needTermination)
                {
                    this.BeginTerminate((asyncResult) =>
                    {
                        this.EndTerminate(asyncResult);
                    },
                    null);
                }
            }
        }


        #endregion
    }

    /// <summary>
    /// Terminate async result.
    /// </summary>
    internal class TerminateAsyncResult : AsyncResultWithProcess<int>
    {
        #region private variables


        /// <summary>
        /// Trusted conversation to use.
        /// </summary>
        private Conversation trustedConversation;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Parent object.
        /// </summary>
        private CustomerCallAnchor parent;
        #endregion

        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public TerminateAsyncResult(CustomerCallAnchor parent,
            Conversation trustedConversation,
            ILogger logger,
            AsyncCallback asyncCallback,
            object state)
            : base(asyncCallback, state)
        {
            this.trustedConversation = trustedConversation;
            this.logger = logger;
            this.parent = parent;
        }

        #endregion

        #region methods

        /// <summary>
        /// Overridden process method.
        /// </summary>
        public override void Process()
        {
            var conversation = this.trustedConversation;
            if (conversation != null)
            {
                // Eject all visible participants.
                conversation.ConferenceSession.BeginTerminateConference(this.ConferenceTerminated, conversation.ConferenceSession);
            }
            else
            {
                this.CompleteTermination();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Callback for conference termination.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConferenceTerminated(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionOccured = true;
            try 
            {
                ConferenceSession confSession = asyncResult.AsyncState as ConferenceSession;
                confSession.EndTerminateConference(asyncResult);

                this.trustedConversation.BeginTerminate(this.ConversationTerminationCompleted, this.trustedConversation);
                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during termination {0}", ioe);
                this.logger.Log("Exception during termination {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during termination {0}", rte);
                this.logger.Log("Exception during termination {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                    Console.WriteLine("Unhandled Exception during termination");
                    this.logger.Log("Unhandled Exception during termination");
                }

                if (caughtException != null)
                {
                    this.CompleteTermination();
                }
            }
        }

        /// <summary>
        /// Callback for conversation termination.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConversationTerminationCompleted(IAsyncResult asyncResult)
        {
            Exception caughtException = null;
            bool unhandledExceptionOccured = true;
            try
            {
                Conversation conversation = asyncResult.AsyncState as Conversation;
                conversation.EndTerminate(asyncResult);
                this.CompleteTermination();
                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during termination {0}", ioe);
                this.logger.Log("Exception during termination {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during termination {0}", rte);
                this.logger.Log("Exception during termination {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                    Console.WriteLine("Unhandled Exception during termination");
                    this.logger.Log("Unhandled Exception during termination");
                }

                if (caughtException != null)
                {
                    this.CompleteTermination();
                }
            }
        }

        /// <summary>
        /// Completes this async result.
        /// </summary>
        /// <param name="exception"></param>
        private void CompleteTermination()
        {
            try
            {
                this.parent.HandleTerminationCompletion();
            }
            finally
            {
                this.Complete(1);
            }
        }

        #endregion

    }

    /// <summary>
    /// Establish async result.
    /// </summary>
    internal class EstablishAsyncResult : AsyncResultWithProcess<int> 
    {
        #region private variables


        /// <summary>
        /// Trusted conversation to use.
        /// </summary>
        private Conversation trustedConversation;

        /// <summary>
        /// Customer conversation.
        /// </summary>
        private Conversation customerConversation;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Parent object.
        /// </summary>
        private CustomerCallAnchor parent;

        /// <summary>
        /// Help desk number.
        /// </summary>
        private RealTimeAddress helpdeskNumber;
        #endregion

        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public EstablishAsyncResult(CustomerCallAnchor parent, 
            Conversation trustedConversation,
            Conversation customerConversation,
            RealTimeAddress helpdeskNumber,
            ILogger logger,
            AsyncCallback asyncCallback, 
            object  state) 

            : base(asyncCallback, state)
        {
            if (helpdeskNumber == null)
            {
                throw new ArgumentNullException("helpdeskNumber");
            }
            this.trustedConversation = trustedConversation;
            this.customerConversation = customerConversation;
            this.logger = logger;
            this.parent = parent;
            this.helpdeskNumber = helpdeskNumber;
        }

        #endregion

        #region methods

        /// <summary>
        /// Overridden process method.
        /// </summary>
        public override void Process()
        {
            // Schedule the conference.
            ConferenceServices conferenceManagement = this.customerConversation.Endpoint.ConferenceServices;

            //Create a conference to anchor the incoming customer call
            ConferenceScheduleInformation conferenceScheduleInfo = new ConferenceScheduleInformation();
            conferenceScheduleInfo.AutomaticLeaderAssignment = AutomaticLeaderAssignment.Everyone;
            conferenceScheduleInfo.LobbyBypass = LobbyBypass.EnabledForGatewayParticipants;
            conferenceScheduleInfo.AccessLevel = ConferenceAccessLevel.Everyone;
            conferenceScheduleInfo.PhoneAccessEnabled = false;
            conferenceScheduleInfo.ExpiryTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(60));

            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                conferenceManagement.BeginScheduleConference(conferenceScheduleInfo,
                    this.ConferenceScheduled,
                    conferenceManagement);
                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during scheduling conference {0}", ioe);
                this.logger.Log("Exception during scheduling conference {0}", ioe);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }

        /// <summary>
        /// Process incoming audio video call.
        /// </summary>
        /// <param name="avCall"></param>
        public void ProcessIncomingDialOutCall(AudioVideoCall avCall)
        {
            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                BackToBackCallSettings incomingCallSettings = new BackToBackCallSettings(avCall);

                AudioVideoCall customerCall = new AudioVideoCall(this.customerConversation);
                BackToBackCallSettings idleCallSettings = new BackToBackCallSettings(customerCall);
                BackToBackCall b2bCall = new BackToBackCall(incomingCallSettings, idleCallSettings);

                b2bCall.BeginEstablish(this.BackToBackCallEstablished, b2bCall);

                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during back to back call establish {0}", ioe);
                this.logger.Log("Exception during back to back call establish {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during back to back call establish {0}", rte);
                this.logger.Log("Exception during back to back call establish {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Completes this async result.
        /// </summary>
        /// <param name="exception"></param>
        private void CompleteEstablishment(Exception exception)
        {
            try
            {
                this.parent.HandleEstablishmentCompletion(exception);
            }
            finally
            {
                this.Complete(exception);
            }
        }

        /// <summary>
        /// Completes this async result.
        /// </summary>
        /// <param name="result"></param>
        private void CompleteEstablishment(int result)
        {
            try
            {
                this.parent.HandleEstablishmentCompletion(null);
            }
            finally
            {
                this.Complete(result);
            }
        }

        /// <summary>
        /// Conference scheduled callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void BackToBackCallEstablished(IAsyncResult asyncResult)
        {
            BackToBackCall b2bCall = asyncResult.AsyncState as BackToBackCall;

            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                b2bCall.EndEstablish(asyncResult);

                AudioVideoMcuSession avmcuSession = this.trustedConversation.ConferenceSession.AudioVideoMcuSession;

                // Dial out to the helpdesk number.
                avmcuSession.BeginDialOut(this.helpdeskNumber.Uri, this.HelpDeskDialOutCompleted, avmcuSession);

                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during back to back call establish {0}", ioe);
                this.logger.Log("Exception during back to back call establish {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during back to back call establish {0}", rte);
                this.logger.Log("Exception during back to back call establish {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }


        /// <summary>
        /// Conference scheduled callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConferenceScheduled(IAsyncResult asyncResult)
        {
            ConferenceServices conferenceManagement = asyncResult.AsyncState as ConferenceServices;

            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                conferenceManagement.EndScheduleConference(asyncResult);

                ConferenceJoinOptions options = new ConferenceJoinOptions();
                this.trustedConversation.ConferenceSession.BeginJoin(options, this.ConferenceJoinCompleted, null);

                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during scheduling conference {0}", ioe);
                this.logger.Log("Exception during scheduling conference {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during scheduling conference {0}", rte);
                this.logger.Log("Exception during scheduling conference {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }

        /// <summary>
        /// Conference join callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ConferenceJoinCompleted(IAsyncResult asyncResult)
        {
            
            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                this.trustedConversation.ConferenceSession.EndJoin(asyncResult);

                var options = new AudioVideoMcuDialOutOptions();
                options.Media.Add(new McuMediaChannel(MediaType.Audio, McuMediaChannelStatus.SendReceive));
                options.ParticipantUri = this.parent.CustomerUri;

                this.trustedConversation.ConferenceSession.AudioVideoMcuSession.BeginDialOut(
                                    this.trustedConversation.Endpoint.EndpointUri,
                                    options,
                                    this.SelfDialOutCompleted,
                                    null);

                unhandledExceptionOccured = false;
            }
            catch (InvalidOperationException ioe)
            {
                caughtException = ioe;
                Console.WriteLine("Exception during join conference {0}", ioe);
                this.logger.Log("Exception during join conference {0}", ioe);
                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during join conference {0}", rte);
                this.logger.Log("Exception during join conference {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }

        /// <summary>
        /// Avmcu dialout callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void SelfDialOutCompleted(IAsyncResult asyncResult)
        {

            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                this.trustedConversation.ConferenceSession.AudioVideoMcuSession.EndDialOut(asyncResult);

                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during dial out {0}", rte);
                this.logger.Log("Exception during dial out {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
            }
        }

        /// <summary>
        /// Avmcu dialout callback.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void HelpDeskDialOutCompleted(IAsyncResult asyncResult)
        {

            bool unhandledExceptionOccured = true;
            Exception caughtException = null;
            try
            {
                AudioVideoMcuSession avmcuSession = asyncResult.AsyncState as AudioVideoMcuSession;
                avmcuSession.EndDialOut(asyncResult);

                unhandledExceptionOccured = false;
            }
            catch (RealTimeException rte)
            {
                caughtException = rte;
                Console.WriteLine("Exception during dial out {0}", rte);
                this.logger.Log("Exception during dial out {0}", rte);
                unhandledExceptionOccured = false;
            }
            finally
            {
                if (unhandledExceptionOccured)
                {
                    caughtException = new OperationFailureException();
                }

                if (caughtException != null)
                {
                    this.CompleteEstablishment(caughtException);
                }
                else
                {
                    /// Complete the async result.
                    this.CompleteEstablishment(1);
                }
            }
        }

        #endregion
    }
}
