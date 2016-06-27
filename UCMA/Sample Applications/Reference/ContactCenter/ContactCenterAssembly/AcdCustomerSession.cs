/*=====================================================================
  File:      AcdCustomerSession.cs

  Summary:   Handles the call interaction of a single Client session.

/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.ConferenceManagement;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;
using System.Net.Mime;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.VoiceXml.Common;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Dialogs;
using Microsoft.Rtc.Collaboration.Samples.Utilities;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    class AcdCustomerSession
    {
        #region Private members

        private Conversation _frontOfficeConversation;
        private Conversation _backOfficeConversation;


        private Call _initialCustomerCall;
        private InstantMessagingCall _agentBackChannel;
        private AgentContextChannel _agentControlChannel;

        private AcdAgentMatchMaker _matchMaker;
        private AcdPortal _portal;
        private AcdLogger _logger;

        private AcdConferenceServicesAnchor _customerCallAnchor;

        private List<AgentSkill> _requestedSkillsByCustomer;
        private productType _contextualProductInformation;
        private CustomerSessionState _sessionState;
        private List<Agent> _exclusionList;
        private BackToBackCall _audioVideoCallB2BUA;
        private BackToBackCall _applicationSharingB2BUA;

        private AcdServiceChannel _customerPrivateAudioChannel;
        private bool _welcomePromptSynchronizationCondition1 = false;
        private bool _welcomePromptSynchronizationCondition2 = false;

        private object _syncRoot = new object();

        private List<ShutdownAsyncResult> _listOfShutdownAsyncResults = new List<ShutdownAsyncResult>();

        #endregion

        #region Properties
        internal event EventHandler<CustomerSessionStateChangedEventArgs> CustomerSessionStateChanged;

        internal List<AgentSkill> RequestedSkills
        {
            get { return _requestedSkillsByCustomer; }
        }
        internal Browser VXmlBrowser { get; set; }
        internal Agent RespondingAgent { get; set; }
        internal List<Agent> AdditionalAgents { get; set; }
        internal AcdConferenceServicesAnchor Anchor
        {
            get { return _customerCallAnchor; }
        }
        internal bool IsRespondingAgentAllocated
        {
            get
            {
                if (null != this.RespondingAgent)
                    return RespondingAgent.IsAllocated;
                else
                    return false;
            }
        }

        internal CustomerSessionState State
        {
            get { return _sessionState; }
        }


        #endregion

        #region constructor
        /// <summary>
        /// Instantiates a new Acd Customer Session
        /// </summary>
        internal AcdCustomerSession(AcdLogger logger, AcdAgentMatchMaker matchMaker, AcdPortal portal)
        {
            _logger = logger;
            this.UpdateState(CustomerSessionState.Incoming);
            _logger = logger;
            _matchMaker = matchMaker;
            _portal = portal;
            _requestedSkillsByCustomer = new List<AgentSkill>();
            this.AdditionalAgents = new List<Agent>();
            _exclusionList = new List<Agent>();
        }
        #endregion

        #region internal methods

        internal void AddAgentToExclusionList(Agent agentToExclude)
        {
            lock (_syncRoot)
            {
                _exclusionList.Add(agentToExclude);

            }
        }

        internal List<Agent> ExclusionListOfAgents
        {
            get
            {
                List<Agent> listOfAgents = new List<Agent>(_exclusionList);

                return listOfAgents;
            }
        }

        /// <summary>
        /// Processes the first incoming call of this AcdCustomerSession
        /// </summary>
        internal void HandleInitialCall(Call call, IEnumerable<AgentSkill> listOfSkills, productType product)
        {
            _logger.Log("AcdCustomerSession receives first incoming Call from " + call.RemoteEndpoint.Participant.Uri);
            Debug.Assert((call is InstantMessagingCall) || (call is AudioVideoCall));

            _contextualProductInformation = product;

            //sets the initial customer call
            _initialCustomerCall = call;

            Agent caller = new Agent(_logger);
            caller.SignInAddress = call.RemoteEndpoint.Participant.Uri;

            lock (_syncRoot)
            {
                _exclusionList.Add(caller);
            }

            //monitors the customer-facing conversation state
            SetFrontOfficeConversationProperties(call);

            //validates the state of the session
            lock (_syncRoot)
            {
                if (_sessionState == CustomerSessionState.Incoming)
                {
                    RealTimeAddress addr = new RealTimeAddress(_initialCustomerCall.RemoteEndpoint.Participant.Uri, _portal.Endpoint.DefaultDomain, _portal.Endpoint.PhoneContext);
                    SipUriParser localParticipantAddress = new SipUriParser(addr.Uri);
                    localParticipantAddress.RemoveParameter(new SipUriParameter("user", "phone"));
                    string Uri = localParticipantAddress.ToString();

                    string displayName = _initialCustomerCall.RemoteEndpoint.Participant.DisplayName;
                    string phoneUri = _initialCustomerCall.RemoteEndpoint.Participant.PhoneUri;

                    //create the back office Conversation where-in the incoming initial call will be back to backed.
                    _backOfficeConversation = new Conversation(_portal.Endpoint);

                    _backOfficeConversation.Impersonate(Uri, phoneUri, displayName);

                    this.SetBackOfficeConversationProperties();

                    //prepare the conference where the incoming audio call will be anchored and processed.
                    _customerCallAnchor = new AcdConferenceServicesAnchor(this, _portal.Endpoint, _logger);

                    if (null != listOfSkills)
                    {
                        _requestedSkillsByCustomer = new List<AgentSkill>(listOfSkills);
                    }

                    //we are treating voice calls differently from instant messaging calls. Instant Messaging dialogs are 
                    //processed directly whereas voice calls will be back to backed into an audio conference first and after
                    // an IVR or MOH service channel was created.

                    if (call is InstantMessagingCall)
                    {

                        this.UpdateState(CustomerSessionState.CollectingData);


                        //Create a dialog instance.
                        Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                        if (null != listOfSkills
                            && _requestedSkillsByCustomer.Count == _portal.Configuration.Skills.Count)
                        {
                            inputParameters.Add("PortalSkills", new List<string>());

                            string skillsRequested = null;
                            List<AgentSkill> list = new List<AgentSkill>(listOfSkills);
                            list.ForEach(sk => { skillsRequested += sk.Value + " "; });

                            inputParameters.Add("WelcomeMessage", String.Format(_portal.Configuration.ContextualWelcomeMessage, skillsRequested));
                        }
                        else
                        {
                            inputParameters.Add("PortalSkills", _portal.Configuration.Skills);
                            inputParameters.Add("WelcomeMessage", _portal.Configuration.WelcomeMessage);
                        }

                        inputParameters.Add("MatchMakerSkills", _matchMaker.Configuration.Skills);
                        inputParameters.Add("PleaseHoldPrompt", _matchMaker.Configuration.AgentMatchMakingPrompt);
                        inputParameters.Add("Call", this._initialCustomerCall);
                        try
                        {
                            //Start IM dialog to gather skills requested
                            SkillGatheringInstantMessagingDialog skillGathering = new SkillGatheringInstantMessagingDialog(inputParameters);
                            skillGathering.Completed += this.DialogCompleted;
                            skillGathering.Run();
                        }
                        catch (Exception e)
                        {
                            _logger.Log("AcdCustomerSession failed to start the instant messaging dialog to gather skill gathering.", e);
                            this.BeginShutdown(this.OnShutdownComplete, null);
                        }

                    }
                    //else, this is a voice call
                    else
                    {
                        try
                        {
                            _customerCallAnchor.BeginStartUp(_backOfficeConversation.LocalParticipant.Uri,
                                                             OnCustomerCallAnchor_StartupCompleted,
                                                             _customerCallAnchor);

                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _logger.Log(String.Format("AcdCustomerSession failed to start the conference services anchor for {0}: exception msg {1}", _backOfficeConversation.LocalParticipant.Uri, ivoex.ToString()));
                            this.BeginShutdown(this.OnShutdownComplete, null);
                            return;
                        }
                    }
                }
            }
        }

        private void OnCustomerCallAnchor_StartupCompleted(IAsyncResult ar)
        {
            try
            {
                AcdConferenceServicesAnchor anchor = ar.AsyncState as AcdConferenceServicesAnchor;
                anchor.EndStartup(ar);

                anchor.AuthorizeParticipant(_backOfficeConversation.LocalParticipant.Uri);

                _customerPrivateAudioChannel = new AcdServiceChannel(anchor, _logger, true);

                try
                {
                    _customerPrivateAudioChannel.BeginStartUp(MediaType.Audio,
                                                              ServiceChannelType.DialIn,
                                                              OnCustomerPrivateAudioChannel_StartUpCompleted,
                                                              _customerPrivateAudioChannel);
                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log(String.Format("AcdCustomerSession failed to establish a private audio channel for {0}", _backOfficeConversation.LocalParticipant.Uri), ivoex);
                    this.BeginShutdown(this.OnShutdownComplete, null);
                }
            }
            catch (RealTimeException rtex)
            {
                _logger.Log(String.Format("AcdCustomerSession failed to start the conference services anchor for {0}: exception msg {1}", _backOfficeConversation.LocalParticipant.Uri, rtex.ToString()));
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }
        }

        private void OnCustomerPrivateAudioChannel_StartUpCompleted(IAsyncResult suar)
        {
            AcdServiceChannel channel = suar.AsyncState as AcdServiceChannel;

            try
            {
                channel.EndStartUp(suar);
            }
            catch (RealTimeException)
            {
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }

            try
            {

                _backOfficeConversation.ConferenceSession.BeginJoin(_customerCallAnchor.ConferenceUri,
                                                                    null,
                                                                    OnBackOfficeConversation_ConferenceSessionJoinCompleted,
                                                                    _backOfficeConversation.ConferenceSession);
            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdCustomerSession failed to begin back to back the customer call into the anchor", ivoex);

                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }
        }

        private void OnBackOfficeConversation_ConferenceSessionJoinCompleted(IAsyncResult jar)
        {
            ConferenceSession confSession = jar.AsyncState as ConferenceSession;
            try
            {
                confSession.EndJoin(jar);

            }
            catch (RealTimeException)
            {
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }

            try
            {
                _customerPrivateAudioChannel.BeginEstablishPrivateAudioChannel(
                    _backOfficeConversation.LocalParticipant.Uri,
                    false,
                    OnCustomerPrivateAudioChannel_EstablishPrivateAudioChannelCompleted,
                    null);
            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdCustomerSession failed to begin start a private audio channel", ivoex);
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }

            this.CreateBackToBackAudioCall(confSession);
        }

        private void OnCustomerPrivateAudioChannel_EstablishPrivateAudioChannelCompleted(IAsyncResult par)
        {
            try
            {
                _customerPrivateAudioChannel.EndEstablishPrivateAudioChannel(par);

                //Signal that the private audio channel is wired to the customer
                _welcomePromptSynchronizationCondition2 = true;

                _logger.Log("AcdCustomerSession detected that the customer was wired to the private audio channel");

                this.StartDataCollection();
            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdCustomerSession failed to end start a private audio channel", ivoex);

                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }
            catch (RealTimeException rtex)
            {
                _logger.Log("AcdCustomerSession failed to end start a private audio channel", rtex);

                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }
        }

        private void CreateBackToBackAudioCall(ConferenceSession confSession)
        {
            AudioVideoCall avCall = new AudioVideoCall(confSession.Conversation);
            try
            {
                _audioVideoCallB2BUA = new BackToBackCall(new BackToBackCallSettings(_initialCustomerCall), new BackToBackCallSettings(avCall));
            }
            catch (ArgumentException argex)
            {
                //handle the case where the incoming call may have been terminated
                _logger.Log("AcdCustomerSession failed to begin start back-to-back call.", argex);
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }

            _audioVideoCallB2BUA.RemoteMediaFlowStateChanged += new EventHandler<RemoteMediaFlowStateChangedEventArgs>(this.RemoteMediaFlowStateChanged);
            _audioVideoCallB2BUA.BeginEstablish(this.OnAudioVideoCallB2BUA_EstablishCompleted, this._audioVideoCallB2BUA);
        }

        private void OnAudioVideoCallB2BUA_EstablishCompleted(IAsyncResult bar)
        {
            BackToBackCall b2bUA = bar.AsyncState as BackToBackCall;

            try
            {
                b2bUA.EndEstablish(bar);
            }
            catch (RealTimeException rtex)
            {
                _logger.Log("AcdCustomerSession failed to end back to back the customer call into the anchor", rtex);
                this.BeginShutdown(this.OnShutdownComplete, null);
                return;
            }
        }


        private static bool DoesRemoteSupportHtmlText(InstantMessagingFlow flow)
        {
            bool supportsHtml = false;
            if (null != flow.SupportedRemoteMediaCapabilities)
            {
                if ((flow.SupportedRemoteMediaCapabilities.SupportedFormats & InstantMessagingFormat.HtmlText) == InstantMessagingFormat.HtmlText)
                {
                    supportsHtml = true;
                }
            }
            return supportsHtml;
        }

        private void StartDataCollection()
        {

            //this is the check to avoid Media clipping
            bool areWelcomePromptSynchronizationConditionsMet = false;

            lock (_syncRoot)
            {
                if ((_sessionState == CustomerSessionState.Incoming) &&
                    _welcomePromptSynchronizationCondition1 &&
                    _welcomePromptSynchronizationCondition2)
                {
                    this.UpdateState(CustomerSessionState.CollectingData);
                    areWelcomePromptSynchronizationConditionsMet = true;
                }
            }

            if (areWelcomePromptSynchronizationConditionsMet)
            {
                // handle VoiceXml portal
                if (_portal.Configuration.VoiceXmlEnabled == true)
                {
                    Microsoft.Rtc.Collaboration.AudioVideo.AudioVideoCall avcall = (Microsoft.Rtc.Collaboration.AudioVideo.AudioVideoCall)_customerPrivateAudioChannel.Call;
                    this.VXmlBrowser = new Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml.Browser();
                    this.VXmlBrowser.SetAudioVideoCall(avcall);
                    this.VXmlBrowser.SessionCompleted += new EventHandler<SessionCompletedEventArgs>(HandleVoiceXmlSessionCompleted);

                    try
                    {
                        this.VXmlBrowser.RunAsync(new Uri("file:///" + Path.GetFullPath(_portal.Configuration.VoiceXmlPath)), null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdCustomerSession failed to start the browser", ivoex);
                        this.BeginShutdown(this.OnShutdownComplete, null);
                        return;

                    }
                    catch (DirectoryNotFoundException dnfex)
                    {
                        _logger.Log("AcdCustomerSession failed to find the directory for a voiceXML file", dnfex);
                        this.BeginShutdown(this.OnShutdownComplete, null);
                        return;
                    }

                }
                // handle dialog-based portal
                else
                {

                    //Create a dialog instance to gather the skills
                    Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                    if (null != _requestedSkillsByCustomer
                        && _requestedSkillsByCustomer.Count == _portal.Configuration.Skills.Count)
                    {

                        inputParameters.Add("PortalSkills", new List<string>());

                        string skillsRequested = null;
                        List<AgentSkill> list = new List<AgentSkill>(_requestedSkillsByCustomer);
                        list.ForEach(sk => { skillsRequested += sk.Value + " "; });

                        inputParameters.Add("WelcomeMessage", String.Format(_portal.Configuration.ContextualWelcomeMessage, skillsRequested));

                    }
                    else
                    {
                        inputParameters.Add("PortalSkills", _portal.Configuration.Skills);
                        inputParameters.Add("WelcomeMessage", _portal.Configuration.WelcomeMessage);
                    }

                    inputParameters.Add("MatchMakerSkills", _matchMaker.Configuration.Skills);
                    inputParameters.Add("PleaseHoldPrompt", _matchMaker.Configuration.AgentMatchMakingPrompt);
                    inputParameters.Add("Call", _customerPrivateAudioChannel.Call);
                    try
                    {
                        //Start audio dialog to gather the skills
                        SkillGatheringAudioDialog audioDialog = new SkillGatheringAudioDialog(inputParameters);
                        audioDialog.Completed += this.DialogCompleted;
                        audioDialog.Run();
                    }
                    catch (Exception)
                    {
                        _logger.Log("AcdCustomerSession failed to start the skill gathering dialog for audio.");
                        this.BeginShutdown(this.OnShutdownComplete, this);

                    }
                }
            }
        }

        void RemoteMediaFlowStateChanged(object sender, RemoteMediaFlowStateChangedEventArgs e)
        {
            if (e.PreviousState == RemoteMediaFlowState.Connecting
                && e.State == RemoteMediaFlowState.Connected)
            {
                //Signal that the ICE connectivity checks are complete
                _welcomePromptSynchronizationCondition1 = true;

                _logger.Log("AcdCustomerSession detected that the customer call media ICE renegotiation completed");

                this.StartDataCollection();
            }
        }

        /// <summary>
        /// Handles the completion of a voicexml session, and calls HandleDataCollectionComplete()
        /// to bring an agent on-line
        /// </summary>
        /// 
        private void HandleVoiceXmlSessionCompleted(object sender, SessionCompletedEventArgs e)
        {
            try
            {
                string message = string.Format("Vxml session completed.  Error={0}, Reason={1}, UnhandledThrow Event={2}, UnhandledThrow Name={3}",
                    e.Error == null ? "null" : e.Error.ToString(),
                    e.Result.Reason.ToString(),
                    e.Result.UnhandledThrowElement == null ? "null" : e.Result.UnhandledThrowElement.Event,
                    e.Result.UnhandledThrowElement == null ? "null" : e.Result.UnhandledThrowElement.Message);

                this._logger.Log(message);

                Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml.Browser voiceXmlBrowser = sender as Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml.Browser;

                ThreadPool.QueueUserWorkItem(state => { voiceXmlBrowser.Dispose(); });

                List<AgentSkill> skills = new List<AgentSkill>();

                if (e.Result != null && e.Result.Namelist != null)
                {
                    foreach (string key in e.Result.Namelist.Keys)
                    {
                        _matchMaker.Configuration.Skills.ForEach(s =>
                        {
                            if (s.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                AgentSkill requestedSkill;

                                try
                                {
                                    requestedSkill = new AgentSkill(s, (string)(e.Result.Namelist[key]));
                                    skills.Add(requestedSkill);
                                }
                                catch (ArgumentException argException)
                                {
                                    _logger.Log(String.Format("AcdCustomerSession: the skill value {0} does not map to any values of skill {1}.", (string)(e.Result.Namelist[key]), s.Name), argException);
                                }
                            }

                        });
                    }
                }


                HandleDataCollectionComplete(skills);
            }
            finally
            {
                var browser = sender as Browser;

                browser.SessionCompleted -= this.HandleVoiceXmlSessionCompleted;
            }
        }

        /// <summary>
        /// Handles the completion of the dialog and determines if the AcdCustomerSession could gather
        /// the required skills and proceed with finding an available agent with the appropriate skillset.
        /// </summary>
        internal void DialogCompleted(object sender, DialogCompletedEventArgs e)
        {
            if (e.Exception == null)
            {
                Dictionary<string, object> output = e.Output;
                List<AgentSkill> agentSkill = output["AgentSkill"] as List<AgentSkill>;
                if (null != agentSkill && agentSkill.Count > 0)
                {
                    this.HandleDataCollectionComplete(agentSkill);
                }
                else
                {
                    //if skills are collected before asking user requested skills(when users chat through website, skills are collected while we receive call)
                    if (null != _requestedSkillsByCustomer && (_requestedSkillsByCustomer.Count == _portal.Configuration.Skills.Count))
                    {
                        agentSkill = _requestedSkillsByCustomer;
                        this.HandleDataCollectionComplete(agentSkill);
                    }
                    else
                        this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
                }
            }
            else if (e.Exception is ArgumentNullException)
            {
                _logger.Log("AcdCustomerSession: Exception occured in dialog:" + e.Exception.Message + " cannot be null");
                this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
            }
            else
            {
                _logger.Log("AcdCustomerSession: Exception occured in dialog:" + e.Exception.Message );
                this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
            }
        }

        /// <summary>
        /// Handles the completion of the dialog and determines if the AcdCustomerSession could gather
        /// the required skills and proceed with finding an available agent with the appropriate skillset.
        /// </summary>
        internal void HandleDataCollectionComplete(List<AgentSkill> requestedSkillsByCustomer)
        {

            //Caching the requested skills

            if (_requestedSkillsByCustomer.Count == 0)
            {
                _requestedSkillsByCustomer = requestedSkillsByCustomer;
            }

            if (_requestedSkillsByCustomer.Count == _portal.Configuration.Skills.Count)
            {
                // We only update the state of the AcdCustomerSession if we are looking for the responding agent (first agent to serve a customer)
                this.UpdateState(CustomerSessionState.AgentMatchMaking);

                if (_initialCustomerCall is AudioVideoCall)
                {

                    _matchMaker.MusicOnHoldServer.BeginEstablishMohChannel(_customerPrivateAudioChannel,
                    ar =>
                    {
                        AcdMusicOnHoldServer mohServer = ar.AsyncState as AcdMusicOnHoldServer;

                        try
                        {
                            mohServer.EndEstablishMohChannel(ar);

                            try
                            {

                                _portal.AgentHunter.BeginHuntForAgent(this, _requestedSkillsByCustomer, OnFindAgentComplete, null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("AcdCustomerSession failed to begin hunt for an agent", ivoex);
                                this.BeginShutdown(this.OnShutdownComplete, null);
                            }
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _logger.Log("AcdCustomerSession failed to end establish Moh Channel", ivoex);
                            this.BeginShutdown(this.OnShutdownComplete, null);
                            return;
                        }

                        catch (RealTimeException rtex)
                        {
                            _logger.Log("AcdCustomerSession failed to end establish Moh Channel", rtex);

                            this.BeginShutdown(this.OnShutdownComplete, null);
                            return;
                        }

                    },
                                                                         _matchMaker.MusicOnHoldServer);

                }
                else
                {
                    try
                    {
                        _customerCallAnchor.BeginStartUp(_backOfficeConversation.LocalParticipant.Uri,
                        ar =>
                        {
                            try
                            {
                                _customerCallAnchor.EndStartup(ar);

                                _customerCallAnchor.AuthorizeParticipant(_backOfficeConversation.LocalParticipant.Uri);

                                try
                                {
                                    _backOfficeConversation.ConferenceSession.BeginJoin(_customerCallAnchor.ConferenceUri,
                                                                                        null,
                                    jar =>
                                    {
                                        try
                                        {
                                            _backOfficeConversation.ConferenceSession.EndJoin(jar);
                                        }
                                        catch (RealTimeException rtex)
                                        {
                                            _logger.Log("AcdCustomerSession: customer failed to join the conference (IM)", rtex);
                                            this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
                                            return;
                                        }

                                        _agentBackChannel = new InstantMessagingCall(_backOfficeConversation);
                                        _agentBackChannel.InstantMessagingFlowConfigurationRequested += this.OnAgentBackChannelFlowCreated;

                                        try
                                        {
                                            _agentBackChannel.BeginEstablish(ear =>
                                            {
                                                try
                                                {
                                                    _agentBackChannel.EndEstablish(ear);
                                                    this.BridgeInstantMessagingCalls((InstantMessagingCall)_initialCustomerCall);
                                                    try
                                                    {

                                                        _portal.AgentHunter.BeginHuntForAgent(this, _requestedSkillsByCustomer, OnFindAgentComplete, null);
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        _logger.Log("AcdCustomerSession failed to begin hunt for an agent", ivoex);
                                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                                    }

                                                }
                                                catch (RealTimeException rtex)
                                                {
                                                    _logger.Log("AcdCustomerSession failed to end establish an agent back channel", rtex);
                                                    this.BeginShutdown(this.OnShutdownComplete, null);
                                                    return;

                                                }
                                            },
                                                                                                                            null);
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            this.BeginShutdown(this.OnShutdownComplete, null);
                                            return;
                                        }
                                    },
                                                                                        null);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    _logger.Log("AcdCustomerSession failed to begin join im anchor", ivoex);
                                    this.BeginShutdown(this.OnShutdownComplete, null);
                                    return;
                                }

                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdCustomerSession failed to end start up the anchor", rtex);
                                this.BeginShutdown(this.OnShutdownComplete, null);
                                return;
                            }
                        },
                                                        _customerCallAnchor);

                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log(String.Format("AcdCustomerSession failed to start the conference services anchor for {0}: exception msg {1}", _backOfficeConversation.LocalParticipant.Uri, ivoex.ToString()));
                        this.BeginShutdown(this.OnShutdownComplete, null);
                    }
                }

            }
            else
            {
                _logger.Log(String.Format("Skills Requested {0}. Skills in Portal {1}", _requestedSkillsByCustomer.Count, _portal.Configuration.Skills.Count));
                _logger.Log("Customer Requested Skills:");
                foreach (AgentSkill skill in _requestedSkillsByCustomer)
                {
                    _logger.Log(skill.Value);
                    this.BeginShutdown(OnShutdownComplete, null);
                }
            }

        }



        /// <summary>
        /// Handles modality escalation by the customer when she is connected to the agent. Else decline.
        /// </summary>
        internal void HandleNewModality(Call call)
        {
            lock (_syncRoot)
            {
                //Handles modality escalation only if the state of the AcdCustomerSession is connected
                if (_sessionState == CustomerSessionState.ConnectedToAgent
                    || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
                {
                    if (call is InstantMessagingCall)
                    {
                        this.HandleNewImCall(call);
                    }
                    else if (call is AudioVideoCall)
                    {
                        this.HandleNewAudioVideoCall(call);
                    }
                    else if (call is ApplicationSharingCall)
                    {
                        //accepts the call and bridges it with the agent call
                        this.HandleNewApplicationSharingCall(call);
                    }
                    else
                    {
                        try
                        {
                            call.Decline();
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _logger.Log("AcdCustomerSession failed declining a modality escalation", ivoex);

                        }
                        catch (RealTimeException ex)
                        {
                            _logger.Log("AcdCustomerSession failed declining a modality escalation", ex);
                        }

                    }

                }
                else
                {
                    Exception declineEx = null;
                    try
                    {
                        _logger.Log("AcdCustomerSession is declining an incoming call as it is not in the connected state");
                        call.Decline();
                    }
                    catch (InvalidOperationException ivo)
                    {
                        declineEx = ivo;
                    }
                    catch (RealTimeException ex)
                    {
                        declineEx = ex;
                    }

                    if (declineEx != null)
                    {
                        _logger.Log("AcdCustomerSession failed declining a modality escalation", declineEx);
                    }
                }
            }


        }

        private void HandleNewImCall(Call call)
        {
            //accepts the call and bridges it with the agent call
            var imCall = call as InstantMessagingCall;

            //override the default behavior so that the first message sent by the customer is received and not eaten up
            imCall.InstantMessagingFlowConfigurationRequested += this.OnCustomerInstantMessagingAdditionFlowCreated;

            try
            {

                imCall.BeginAccept(new AsyncCallback(
                                       delegate(IAsyncResult result)
                                       {
                                           try
                                           {
                                               imCall.EndAccept(result);
                                               //the call was accepted successfully, bridge the two calls together.
                                               this.BridgeInstantMessagingCalls(imCall);
                                               this.SendIMWelcomeMessages(null, _portal.Configuration.ImBridgingMessage, imCall);

                                           }
                                           catch (RealTimeException ex)
                                           {
                                               _logger.Log("AcdCustomerSession failed accepting the IM escalation", ex);
                                           }
                                       }),
                                   imCall);

            }
            catch (InvalidOperationException ex)
            {
                _logger.Log("AcdCustomerSession failed accepting the IM escalation", ex);
            }
        }

        private void HandleNewAudioVideoCall(Call call)
        {
            if (_sessionState == CustomerSessionState.ConnectedToAgentHeld)
            {
                try
                {
                    this.BeginPutCustomerOnHold(pcoh =>
                                                {
                                                    try
                                                    {
                                                        this.EndPutCustomerOnHold(pcoh);
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        _logger.Log("AcdCustomerSession failed to end put customer on hold in handle new modality", ivoex);
                                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                                    }
                                                    catch (RealTimeException rtex)
                                                    {
                                                        _logger.Log("AcdCustomerSession failed to end put customer on hold in handle new modality", rtex);
                                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                                    }
                                                },
                                                null);
                }
                catch (InvalidOperationException ivoex)
                {
                    _logger.Log("AcdCustomerSession failed to start put customer on hold in handle new modality", ivoex);
                    this.BeginShutdown(this.OnShutdownComplete, null);
                }
            }

            //accepts the call and bridges it with the agent call
            var avCall = call as AudioVideoCall;

            AudioVideoCall audioVideoCall = null;

            if (call.Conversation == _backOfficeConversation)
            {
                //creates an AudioVideoCall in the back office
                audioVideoCall = new AudioVideoCall(this._frontOfficeConversation);
            }
            else if (call.Conversation == _frontOfficeConversation)
            {
                //creates an AudioVideoCall in the back office
                audioVideoCall = new AudioVideoCall(_backOfficeConversation);
            }
            //creates a B2BUA
            _audioVideoCallB2BUA = new BackToBackCall(new BackToBackCallSettings(avCall), new BackToBackCallSettings(audioVideoCall));

            try
            {
                _audioVideoCallB2BUA.BeginEstablish(HandleNewAudioVideoCall_AudioVideoCallB2BUA_EstablishCompleted, _audioVideoCallB2BUA);

            }
            catch (RealTimeException ex)
            {
                _logger.Log("AcdCustomerSession failed adding audio", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log("AcdCustomerSession failed adding audio most likely because the customer hung up", ex);

            }
        }

        private void HandleNewApplicationSharingCall(Call call)
        {
            //accepts the call and bridges it with the agent call
            var asCall = call as ApplicationSharingCall;

            //creates an AudioVideoCall in the back office
            var applicationSharingCall = new ApplicationSharingCall(this._backOfficeConversation);

            //creates a B2BUA
            _applicationSharingB2BUA = new BackToBackCall(new BackToBackCallSettings(asCall), new BackToBackCallSettings(applicationSharingCall));

            try
            {
                _applicationSharingB2BUA.BeginEstablish(
                    HandleNewApplicationSharingCall_ApplicationSharingB2BUA_EstablishCompleted,
                    _applicationSharingB2BUA);

            }
            catch (RealTimeException ex)
            {
                _logger.Log("AcdCustomerSession failed adding application sharing", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log("AcdCustomerSession failed adding application sharing most likely because the customer hung up", ex);

            }
        }

        private void HandleNewAudioVideoCall_AudioVideoCallB2BUA_EstablishCompleted(IAsyncResult result)
        {
            var b2bUA = result.AsyncState as BackToBackCall;
            try
            {
                b2bUA.EndEstablish(result);
            }
            catch (RealTimeException ex)
            {
                _logger.Log("AcdCustomerSession failed adding audio or video", ex);
            }
        }

        private void HandleNewApplicationSharingCall_ApplicationSharingB2BUA_EstablishCompleted(IAsyncResult result)
        {
            var b2bUA = result.AsyncState as BackToBackCall;
            try
            {
                b2bUA.EndEstablish(result);
            }
            catch (RealTimeException ex)
            {
                _logger.Log("AcdCustomerSession failed adding application sharing", ex);
            }
        }


        /// <summary>
        /// Terminates and cleans up the AcdCustomerSession
        /// </summary>
        internal IAsyncResult BeginShutdown(AsyncCallback userCallBack, object state)
        {
            ShutdownAsyncResult asyncResult = new ShutdownAsyncResult(userCallBack, state, this);

            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_sessionState == CustomerSessionState.Terminated)
                {
                    // complete the operation synchronously as the session is already being terminated or terminated
                    asyncResult.SetAsCompleted(null, true);
                }
                else if (_sessionState == CustomerSessionState.Terminating)
                {
                    _listOfShutdownAsyncResults.Add(asyncResult);
                }
                else
                {
                    this.UpdateState(CustomerSessionState.Terminating);
                    _logger.Log("AcdCustomerSession is shutting down");
                    firstTime = true;
                }
            }
            if (firstTime)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    ShutdownAsyncResult tempAr = waitState as ShutdownAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }

            return asyncResult;

        }

        internal void EndShutdown(IAsyncResult result)
        {
            ShutdownAsyncResult sdResult = result as ShutdownAsyncResult;

            sdResult.EndInvoke();
        }
        #endregion internal methods

        #region Private methods
        /// <summary>
        ///  Sets the front office Conversation characteristics and register event handlers to monitor
        ///  the Acd customer session state.
        /// </summary>
        private void SetFrontOfficeConversationProperties(Call incomingCall)
        {
            _frontOfficeConversation = incomingCall.Conversation;
            _frontOfficeConversation.ApplicationContext = this;
            _frontOfficeConversation.StateChanged += HandleFrontAndBackOfficeConversationStateChanged;

        }

        /// <summary>
        /// Register event handlers to monitor the the Acd customer session state.
        /// </summary>
        private void SetBackOfficeConversationProperties()
        {
            _backOfficeConversation.ApplicationContext = this;
            _backOfficeConversation.StateChanged += HandleFrontAndBackOfficeConversationStateChanged;
            _backOfficeConversation.RemoteParticipantAttendanceChanged += HandleAttendanceChanged;
            _backOfficeConversation.PropertiesChanged += HandleModalityChanges;
        }

        private void OnCustomerInstantMessagingAdditionFlowCreated(object sender, InstantMessagingFlowConfigurationRequestedEventArgs args)
        {
            var imFlow = args.Flow;
            var imCall = sender as InstantMessagingCall;

            var template = new InstantMessagingFlowTemplate();
            template.ToastFormatSupport = CapabilitySupport.UnSupported;

            imFlow.Initialize(template);


            imCall.InstantMessagingFlowConfigurationRequested -= this.OnCustomerInstantMessagingAdditionFlowCreated;

        }


        /// <summary>
        /// Monitors the state of the Conversation with the customer, and terminate the session when it terminates
        /// Special cases the self-transfer case when the state is connecting an agent.
        /// Note that the front office Conversation is P2P only, which makes it relatively simpler to monitor.
        /// </summary>
        private void HandleFrontAndBackOfficeConversationStateChanged(object sender, StateChangedEventArgs<ConversationState> args)
        {
            if (args.State == ConversationState.Terminating)
            {
                // the first of the back office or front office Conversation that gets terminated terminates the entire 
                // session.
                _logger.Log("AcdCustomerSession is shutting down because the front or back office conversation is getting terminated");
                this.BeginShutdown(OnShutdownComplete, null);
            }

            if (args.State == ConversationState.Terminated)
            {

                Conversation conversation = sender as Conversation;
                if (conversation.Equals(_backOfficeConversation))
                {
                    _backOfficeConversation.StateChanged -= HandleFrontAndBackOfficeConversationStateChanged;
                    _backOfficeConversation.RemoteParticipantAttendanceChanged -= HandleAttendanceChanged;
                }
                else
                {
                    _frontOfficeConversation.StateChanged -= HandleFrontAndBackOfficeConversationStateChanged;
                    _frontOfficeConversation.PropertiesChanged -= HandleModalityChanges;
                }
            }

        }

        /// <summary>
        /// Monitors the back office agent roster. When the responding agent is no longer in the roster
        /// the session gets terminated.
        /// </summary>
        private void HandleAttendanceChanged(object sender, ParticipantAttendanceChangedEventArgs e)
        {
            if (e.Removed.Count > 0)
            {
                Conversation conversation = sender as Conversation;

                Agent agent = null;

                foreach (ConversationParticipant participant in e.Removed)
                {
                    if (_sessionState == CustomerSessionState.ConnectedToAgent
                        || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
                    {
                        if (SipUriCompare.Equals(participant.Uri, this.RespondingAgent.SignInAddress))
                        {
                            //the responding agent is no longer part of the roster, shutting down the session
                            //which includes the deallocation of all agents remaining in the conversation.
                            _logger.Log("AcdCustomerSession detected that the responding agent left the call, shutting down the session");
                            this.BeginShutdown(OnShutdownComplete, this);
                        }
                        else if ((agent = _matchMaker.LookupAgent(participant.Uri)) != null)
                        {
                            lock (_syncRoot)
                            {
                                if (this.AdditionalAgents.Contains(agent))
                                {
                                    this.AdditionalAgents.Remove(agent);
                                }
                            }

                            _logger.Log("De-allocating Agent " + agent.SignInAddress);
                            try
                            {
                                agent.Deallocate(this);
                                _logger.Log("Agent " + agent.SignInAddress + " is deallocated.");

                                _matchMaker.HandleNewAvailableAgent(agent);
                            }
                            catch (InvalidOperationException ex)
                            {
                                //eat the exception
                                _logger.Log("AcdCustomerSession tries to deallocate an unallocated agent in HandleAttendanceChanged", ex);
                            }

                        }
                    }
                }
            }
        }


        /// <summary>
        /// Monitors the back office modality support. When a modality is added in the back office, some
        /// logic determines whether to add the modality on the customer side.
        /// </summary>
        private void HandleModalityChanges(object sender, PropertiesChangedEventArgs<ConversationProperties> e)
        {
            lock (_syncRoot)
            {

                if (_sessionState == CustomerSessionState.ConnectedToAgent
                    || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
                {
                    Conversation conversation = sender as Conversation;
                    ParticipantEndpoint localEndpoint = conversation.LocalParticipant.GetEndpoints()[0];
                    var options = new McuDialOutOptions();

                    foreach (string key in e.ChangedPropertyNames)
                    {
                        if (key.Equals("ConversationActiveMediaTypes", StringComparison.OrdinalIgnoreCase))
                        {

                            // Enumerate all modalities of the conference and evaluate those where the local participant
                            // does not participate. Add this modality if not already available.
                            foreach (string modality in e.Properties.ActiveMediaTypes)
                            {
                                if (!localEndpoint.GetActiveMediaTypes().Contains(modality))
                                {
                                    if (modality.Equals(MediaType.Audio, StringComparison.OrdinalIgnoreCase))
                                    {
                                        options.Media.Add(new McuMediaChannel(MediaType.Audio, McuMediaChannelStatus.SendReceive));
                                        if (_sessionState == CustomerSessionState.ConnectedToAgentHeld)
                                        {
                                            try
                                            {
                                                this.BeginPutCustomerOnHold(pcoh =>
                                                {
                                                    try
                                                    {
                                                        this.EndPutCustomerOnHold(pcoh);
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        _logger.Log("AcdCustomerSession failed to end put customer on hold in handle modality changes", ivoex);
                                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                                    }
                                                    catch (RealTimeException rtex)
                                                    {
                                                        _logger.Log("AcdCustomerSession failed to end put customer on hold in handle modality changes", rtex);
                                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                                    }
                                                },
                                                null);
                                            }
                                            catch (InvalidOperationException ivoex)
                                            {
                                                _logger.Log("AcdCustomerSession failed to start put customer on hold in handle modality changes", ivoex);
                                                this.BeginShutdown(this.OnShutdownComplete, null);
                                            }
                                        }

                                    }

                                    if (modality.Equals(MediaType.Video, StringComparison.OrdinalIgnoreCase))
                                    {
                                        var mainVideoChannel = new McuMediaChannel(MediaType.Video, McuMediaChannelStatus.SendReceive);
                                        mainVideoChannel.Label = "main-video";

                                        var panoramicVideoChannel = new McuMediaChannel(MediaType.Video, McuMediaChannelStatus.SendReceive);
                                        panoramicVideoChannel.Label = "panoramic-video";

                                        options.Media.Add(mainVideoChannel);
                                        options.Media.Add(panoramicVideoChannel);

                                    }

                                }
                            }

                        }

                    }

                    //see if we need to dial out for audio and video
                    if (options.Media.Count > 0)
                    {
                        var avMcuSession = _backOfficeConversation.ConferenceSession.AudioVideoMcuSession;

                        try
                        {
                            avMcuSession.BeginDialOut(_backOfficeConversation.LocalParticipant.GetEndpoints()[0].Uri,
                                                      options,
                            delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    avMcuSession.EndDialOut(ar);
                                }
                                catch (RealTimeException ex)
                                {
                                    _logger.Log("AcdCustomerSession failed to dial out to the local endpoint Uri", ex);
                                }
                            },
                                                    avMcuSession);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Log("AcdCustomerSession failed to dial out to the local endpoint Uri", ex);
                        }
                    }
                }
            }
        }



        ///<summary>
        ///  This method is called by the matchMaker when an agent has been allocated or 
        ///  when the operation times out or fails for other reasons. 
        /// </summary>
        private void OnFindAgentComplete(IAsyncResult result)
        {
            try
            {
                AgentHuntResult agentResult = _portal.AgentHunter.EndHuntForAgent(result);

                Agent agent = agentResult.Agent;
                string contextualConversationId = agentResult.ConversationId;

                _logger.Log("AcdCustomerSession AgentFound :" + agent.SignInAddress);

                if (_sessionState != CustomerSessionState.Terminating && _sessionState != CustomerSessionState.Terminated)
                {
                    if (RespondingAgent == null)  //If no agent was allocated to this session
                    {
                        RespondingAgent = agent;
                        RespondingAgent.AllocationStatus = AgentAllocationStatus.AssigningTheAgentToItsOwner;

                        this.InitializeAgentControlChannel(contextualConversationId);

                        if (_initialCustomerCall is InstantMessagingCall)
                        {
                            InstantMessagingCall imCall = (InstantMessagingCall)_initialCustomerCall;

                            this.UpdateState(CustomerSessionState.ConnectedToAgent);
                            this.SendIMWelcomeMessages(null, _portal.Configuration.ImBridgingMessage, imCall);
                        }
                        else if (_initialCustomerCall is AudioVideoCall)
                        {
                            _agentBackChannel = new InstantMessagingCall(_backOfficeConversation);
                            _agentBackChannel.InstantMessagingFlowConfigurationRequested += this.OnAgentBackChannelFlowCreated;

                            try
                            {
                                _agentBackChannel.BeginEstablish(ear =>
                                {
                                    try
                                    {
                                        _agentBackChannel.EndEstablish(ear);

                                        _backOfficeConversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged += this.CancelMusicOnHoldOnAgentJoiningAudioConference;

                                        List<ParticipantEndpoint> _listOfAudioVideoParticipantEndpoints = new List<ParticipantEndpoint>(_backOfficeConversation.ConferenceSession.AudioVideoMcuSession.GetRemoteParticipantEndpoints());

                                        bool agentAudioEndpointFound = false;
                                        _listOfAudioVideoParticipantEndpoints.ForEach(pe =>
                                        {

                                            if (SipUriCompare.Equals(pe.Participant.Uri, agent.SignInAddress))
                                            {
                                                agentAudioEndpointFound = true;
                                            }

                                        });


                                        if (agentAudioEndpointFound)
                                        {
                                            if (_sessionState < CustomerSessionState.ConnectedToAgent)
                                            {
                                                this.UpdateState(CustomerSessionState.ConnectedToAgent);
                                                _matchMaker.MusicOnHoldServer.BeginTerminateMohChannel(_customerPrivateAudioChannel,
                                                ter =>
                                                {
                                                    _matchMaker.MusicOnHoldServer.EndTerminateMohChannel(ter);


                                                    try
                                                    {
                                                        _customerPrivateAudioChannel.BeginBringAllChannelParticipantsInMainAudioMix(bacpimam =>
                                                        {
                                                            try
                                                            {
                                                                _customerPrivateAudioChannel.EndBringAllChannelParticipantsInMainAudioMix(bacpimam);
                                                            }
                                                            catch (RealTimeException rtex)
                                                            {
                                                                _logger.Log("AcdCustomerSession failed bringing the customer into the main mix", rtex);
                                                                this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
                                                                return;

                                                            }
                                                        },
                                                                                                                                    null);
                                                    }
                                                    catch (InvalidOperationException ivoex)
                                                    {
                                                        _logger.Log("AcdCustomerSession detected that the customer private audio channel was torn doww making it impossible to bring the customer out of the box", ivoex);
                                                        this.BeginShutdown(sd => { this.EndShutdown(sd); }, null);
                                                        return;
                                                    }

                                                },
                                                                                                            null);


                                                _backOfficeConversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged -= this.CancelMusicOnHoldOnAgentJoiningAudioConference;

                                            }
                                        }
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _logger.Log("AcdCustomerSession failed to end establish an agent back channel (initial call is audio)", rtex);
                                        this.BeginShutdown(this.OnShutdownComplete, null);
                                        return;

                                    }
                                },
                                                                    null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("AcdCustomerSession failed to begin establish an agent back channel (initial call is audio)", ivoex);
                                this.BeginShutdown(this.OnShutdownComplete, null);
                                return;
                            }

                        }

                    }
                    else
                    {
                        lock (_syncRoot)
                        {
                            this.AdditionalAgents.Add(agent);
                        }

                    }

                }
                else
                {

                    //Deallocate the agent as the session got terminated.
                    _logger.Log("De-allocating Agent " + agent.SignInAddress + "the session got terminated.");
                    try
                    {
                        agent.Deallocate(this);

                        _logger.Log("Agent " + agent.SignInAddress + " is deallocated");

                        //signal to the match maker that an agent is available again
                        _matchMaker.HandleNewAvailableAgent(agent);
                    }
                    catch (InvalidOperationException ex)
                    {
                        //eat the exception
                        _logger.Log("AcdCustomerSession tries to deallocate an unallocated agent in EndFindAgent", ex);
                    }
                }
            }
            catch (TimeoutException ex)
            {
                _logger.Log("AcdCustomerSession failed to get an agent match in time, abandon the session", ex);
                if (_sessionState == CustomerSessionState.AgentMatchMaking)
                {
                    //Announce to the customer that no match could be made in time
                    if (_initialCustomerCall is AudioVideoCall)
                    {

                        this.BeginShutdown(OnShutdownComplete, null);

                    }
                    else
                    {
                        InstantMessagingCall customerIMCall = _initialCustomerCall as InstantMessagingCall;
                        try
                        {
                            customerIMCall.Flow.BeginSendInstantMessage(_portal.Configuration.TimeOutNoAgentAvailableMessage,
                            delegate(IAsyncResult ar)
                            {
                                try
                                {
                                    var flow = ar.AsyncState as InstantMessagingFlow;
                                    flow.EndSendInstantMessage(ar);
                                }
                                catch (RealTimeException)
                                {
                                }
                            },
                                                                 customerIMCall.Flow);

                        }
                        catch (InvalidOperationException)
                        {
                            //best effort
                        }
                        finally
                        {
                            //clean up and terminate the session
                            this.BeginShutdown(this.OnShutdownComplete, null);
                        }

                    }

                }
            }
            catch (OperationFailureException ex)
            {
                _logger.Log("AcdCustomerSession failed to get an agent match as the match maker got terminated", ex);
                this.BeginShutdown(OnShutdownComplete, null);

            }
            catch (OperationTimeoutException otex)
            {
                _logger.Log("AcdCustomerSession failed to escalate the agent in time", otex);
                this.BeginShutdown(OnShutdownComplete, null);

            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdCustomerSession failed to get an agent match as the conference join operation failed ", ivoex);
                this.BeginShutdown(OnShutdownComplete, null);

            }
            catch (RealTimeException rtex)
            {
                _logger.Log("AcdCustomerSession failed to get an agent match as the conference join operation failed ", rtex);
                this.BeginShutdown(OnShutdownComplete, null);

            }

        }

        private void InitializeAgentControlChannel(string ContextualConversationId)
        {
            List<ParticipantEndpoint> listOfEndpoints = new List<ParticipantEndpoint>(_backOfficeConversation.ConferenceSession.GetRemoteParticipantEndpoints());

            ParticipantEndpoint targetAgentEndpoint = null;

            foreach (ParticipantEndpoint pep in listOfEndpoints)
            {
                if (SipUriCompare.Equals(pep.Participant.Uri, RespondingAgent.SignInAddress))
                {
                    targetAgentEndpoint = pep;
                    break;

                }
            }

            if (null == targetAgentEndpoint)
            {
                //the responding agent must have left the conference
                _logger.Log("AcdCustomerSession cannot establish the control channel with the responding Agent, agent cannot be found.");
                return;

            }

            _agentControlChannel = new AgentContextChannel(_backOfficeConversation, targetAgentEndpoint);

            _agentControlChannel.RequestReceived += ProcessAgentControlChannelRequestReceived;
            _agentControlChannel.StateChanged += ProcessAgentControlChannelStateChanged;

            List<skillType> listOfAvailableSkills = new List<skillType>();

            _portal.Platform.MatchMaker.Configuration.Skills.ForEach(sk =>
             {
                 listOfAvailableSkills.Add(Skill.Convert(sk));
             });


            try
            {

                _agentControlChannel.BeginEstablish(ContextualConversationId,
                                                    _portal.Platform.MatchMaker.Configuration.AgentDashboardGuid,
                                                    _contextualProductInformation,
                                                    listOfAvailableSkills,
                acest =>
                {
                    try
                    {
                        _agentControlChannel.EndEstablish(acest);
                    }
                    catch (RealTimeException rtex)
                    {
                        _logger.Log("AcdCustomerSession fails to end establish the control channel with the responding Agent.", rtex);
                    }
                },
                                                    null);

            }
            catch (InvalidOperationException ivoex)
            {
                _logger.Log("AcdCustomerSession fails to begin establish the control channel with the responding Agent.", ivoex);

            }
        }


        private void CancelMusicOnHoldOnAgentJoiningAudioConference(object sender, ParticipantEndpointAttendanceChangedEventArgs<AudioVideoMcuParticipantEndpointProperties> args)
        {

            Collection<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> participantEndpointKeyValues = args.Joined;

            List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>> listOfEndpoints = new List<KeyValuePair<ParticipantEndpoint, AudioVideoMcuParticipantEndpointProperties>>(participantEndpointKeyValues);

            ParticipantEndpoint participantEndpoint = null;

            listOfEndpoints.ForEach(endpoint =>
            {
                if (SipUriCompare.Equals(endpoint.Key.Participant.Uri, RespondingAgent.SignInAddress))
                {
                    participantEndpoint = endpoint.Key;
                }
            });

            if (null == participantEndpoint)
            {
                return;
            }

            this.UpdateState(CustomerSessionState.ConnectedToAgent);
            _matchMaker.MusicOnHoldServer.BeginTerminateMohChannel(_customerPrivateAudioChannel,
            ter =>
            {
                _matchMaker.MusicOnHoldServer.EndTerminateMohChannel(ter);

                try
                {
                    _customerPrivateAudioChannel.BeginBringAllChannelParticipantsInMainAudioMix(bacpimam =>
                    {
                        try
                        {
                            _customerPrivateAudioChannel.EndBringAllChannelParticipantsInMainAudioMix(bacpimam);
                        }
                        catch (RealTimeException rtex)
                        {
                            _logger.Log("AcdCustomerSession failed end bringing the customer in the main mix", rtex);
                        }
                    },
                                                                                                null);
                }
                catch (InvalidOperationException ivoex)
                {

                    _logger.Log("AcdCustomerSession failed bringing the customer in the main mix", ivoex);
                }
            },
                                                                    null);

            _backOfficeConversation.ConferenceSession.AudioVideoMcuSession.ParticipantEndpointAttendanceChanged -= this.CancelMusicOnHoldOnAgentJoiningAudioConference;
        }

        /// <summary>
        /// Bridges the customer's and the agent's Instant Messaging calls 
        /// </summary>
        private void BridgeInstantMessagingCalls(InstantMessagingCall customerIMCall)
        {
            //Wire up our BackToBack Flow events
            customerIMCall.Flow.StateChanged += OnCustomerIMFlowTerminated;
            customerIMCall.Flow.MessageReceived += OnCustomerMessageReceived;
            customerIMCall.Flow.RemoteComposingStateChanged += OnTypingNotificationReceived;
        }


        /// <summary>
        /// Relay typing notification back and forth once the communication is established
        /// </summary>
        private void OnTypingNotificationReceived(object sender, ComposingStateChangedEventArgs args)
        {
            lock (_syncRoot)
            {
                if (_sessionState == CustomerSessionState.ConnectedToAgent)
                {

                    if (args.Participant.Equals(_initialCustomerCall.RemoteEndpoint.Participant))
                    {
                        _agentBackChannel.Flow.LocalComposingState = args.ComposingState;
                    }
                    else if (!this.ScreenSupervisorMessage(args.Participant))
                    {
                        InstantMessagingCall imCall = null;
                        foreach (Call call in _frontOfficeConversation.Calls)
                        {
                            imCall = call as InstantMessagingCall;
                            if (imCall != null)
                            {
                                break;
                            }
                        }

                        if (null != imCall)
                        {
                            imCall.Flow.LocalComposingState = args.ComposingState;
                        }

                    }
                }
            }
        }


        /// <summary>
        /// Initializes the agent back channel flow in such a way we can delay sending the success/failure response
        /// to the conference. It is important that agents be aware of whether a message they sent was actually received 
        /// by the customer. Hence the need to configure the IMFlow appropriately before call establishment.
        /// </summary>
        private void OnAgentBackChannelFlowCreated(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            e.Flow.MessageReceived += this.OnMessageReceivedFromAgent;
            e.Flow.StateChanged += this.OnAgentBackChannelTerminated;
            e.Flow.RemoteComposingStateChanged += this.OnTypingNotificationReceived;

            InstantMessagingFlowTemplate template = new InstantMessagingFlowTemplate();
            template.MessageConsumptionMode = InstantMessageConsumptionMode.ProxiedToRemoteEntity;

            e.Flow.Initialize(template);
        }


        /// <summary>
        /// Handles messages received from agents 
        /// </summary>
        private void OnMessageReceivedFromAgent(object sender, InstantMessageReceivedEventArgs e)
        {
            lock (_syncRoot)
            {
                InstantMessageId MessageId = e.MessageId; ;
                if (this.ScreenSupervisorMessage(e.Sender) || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
                {
                    try
                    {
                        //acknowledge the successful reception of the message (this needs to be done as we selected
                        //a delayed answer mode for messages so that agents get notified when the ACD fails to deliver message
                        //to the customer
                        _agentBackChannel.Flow.EndSendSuccessDeliveryNotification(_agentBackChannel.Flow.BeginSendSuccessDeliveryNotification(MessageId, null, null));
                    }
                    catch (InvalidOperationException)
                    {
                        //eat the exception this may happen when the imflow just got terminated. Let the 
                        //flow state changed event take care of terminating the session
                    }
                }
                else if (_sessionState == CustomerSessionState.ConnectedToAgent)
                {
                    //This is not a command but a message that can be tentatively forwarded the customer if customer supports IM.
                    //Retrieve the customer call if it exists. If it does not, then do nothing.
                    //if it exists forward the IM to the the customer.
                    InstantMessagingCall imCall = null;

                    foreach (Call call in _frontOfficeConversation.Calls)
                    {
                        if (call is InstantMessagingCall)
                        {
                            imCall = (InstantMessagingCall)call;
                            break;
                        }
                    }

                    if (null != imCall && imCall.State == CallState.Established)
                    {
                        bool fallbackToText = !DoesRemoteSupportHtmlText(imCall.Flow);

                        string message = FormatHtmlCustomerMessage(e, fallbackToText);

                        try
                        {
                            ContentType ct = null;
                            if (fallbackToText)
                            {
                                ct = new ContentType("text/plain");
                            }
                            else
                            {
                                ct = new ContentType("text/html");
                            }
                            imCall.Flow.BeginSendInstantMessage(ct, Encoding.UTF8.GetBytes(message),
                            delegate(IAsyncResult result)
                            {
                                try
                                {
                                    imCall.Flow.EndSendInstantMessage(result);

                                    try
                                    {
                                        _agentBackChannel.Flow.BeginSendSuccessDeliveryNotification(MessageId,
                                        ssdn =>
                                        {
                                            try
                                            {
                                                _agentBackChannel.Flow.EndSendSuccessDeliveryNotification(ssdn);
                                            }
                                            catch (RealTimeException)
                                            {

                                            }
                                        },
                                        null);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                    }
                                }
                                catch (RealTimeException ex)
                                {
                                    int respCode;

                                    if (ex is FailureResponseException)
                                    {
                                        var fre = ex as FailureResponseException;
                                        respCode = fre.ResponseData.ResponseCode;
                                    }
                                    else
                                    {
                                        respCode = ResponseCode.TemporarilyUnavailable;
                                    }
                                    try
                                    {
                                        _agentBackChannel.Flow.BeginSendFailureDeliveryNotification(MessageId,
                                                                                                    respCode,
                                                                                                    sfdn =>
                                                                                                    {
                                                                                                        try
                                                                                                        {
                                                                                                            _agentBackChannel.Flow.EndSendFailureDeliveryNotification(sfdn);
                                                                                                        }
                                                                                                        catch (RealTimeException)
                                                                                                        { }
                                                                                                    },
                                            null);
                                    }
                                    catch (InvalidOperationException)
                                    { }

                                    _logger.Log("AcdCustomerSession failed forwarding message to customer", ex);
                                }
                            },
                                                         null);
                        }
                        catch (RealTimeException)
                        {
                            _logger.Log("AcdCustomerSession failed forwarding message to customer");

                        }
                        catch (InvalidOperationException)
                        {
                            _logger.Log("AcdCustomerSession failed forwarding message to customer");

                        }
                    }
                    else
                    {
                        if (imCall == null)
                        {
                            imCall = new InstantMessagingCall(_frontOfficeConversation);
                            imCall.InstantMessagingFlowConfigurationRequested += this.OnCustomerInstantMessagingAdditionFlowCreated;
                            try
                            {
                                imCall.BeginEstablish(ar =>
                                  {
                                      try
                                      {
                                          imCall.EndEstablish(ar);

                                          bool fallbackToText = !DoesRemoteSupportHtmlText(imCall.Flow);
                                          //the call was accepted successfully, bridge the two calls together.
                                          this.BridgeInstantMessagingCalls(imCall);

                                          try
                                          {
                                              ContentType ct = null;
                                              if (fallbackToText)
                                              {
                                                  ct = new ContentType("text/plain");
                                              }
                                              else
                                              {
                                                  ct = new ContentType("text/html");
                                              }
                                              imCall.Flow.BeginSendInstantMessage(ct,
                                                                                  Encoding.UTF8.GetBytes(FormatHtmlCustomerMessage(e, fallbackToText)), sim =>
                                              {
                                                  try
                                                  {
                                                      imCall.Flow.EndSendInstantMessage(sim);
                                                  }
                                                  catch (RealTimeException)
                                                  { }
                                              },
                                              null);
                                          }
                                          catch (InvalidOperationException)
                                          {
                                          }

                                          try
                                          {
                                              _agentBackChannel.Flow.BeginSendSuccessDeliveryNotification(MessageId,
                                                                                                          ssdn =>
                                                                                                          {
                                                                                                              try
                                                                                                              {
                                                                                                                  _agentBackChannel.Flow.EndSendSuccessDeliveryNotification(ssdn);
                                                                                                              }
                                                                                                              catch (RealTimeException)
                                                                                                              { }
                                                                                                          },
                                              null);
                                          }
                                          catch (InvalidOperationException)
                                          { }
                                      }
                                      catch (RealTimeException)
                                      {
                                          try
                                          {

                                              _agentBackChannel.Flow.BeginSendFailureDeliveryNotification(MessageId,
                                                                                                          ResponseCode.TemporarilyUnavailable,
                                                sfdn =>
                                                {
                                                    try
                                                    {
                                                        _agentBackChannel.Flow.EndSendFailureDeliveryNotification(sfdn);
                                                    }
                                                    catch (RealTimeException)
                                                    { }
                                                },
                                              null);
                                          }

                                          catch (InvalidOperationException)
                                          { }

                                      }


                                  },
                                  null);
                            }
                            catch (InvalidOperationException ivoex)
                            {

                                _logger.Log("AcdCustomerSession failed to start the modality escalation on the caller side", ivoex);

                            }



                        }
                        else
                        {
                            //don't forget to acknowledge the message in this case.

                            try
                            {

                                _agentBackChannel.Flow.BeginSendFailureDeliveryNotification(MessageId,
                                                                                            ResponseCode.TemporarilyUnavailable,
                                sfdn =>
                                {
                                    try
                                    {
                                        _agentBackChannel.Flow.EndSendFailureDeliveryNotification(sfdn);
                                    }
                                    catch (RealTimeException)
                                    { }

                                },
                                                                                             null);
                            }
                            catch (InvalidOperationException)
                            {

                            }
                        }
                    }
                }

            }
        }
        /// <summary>
        /// This helper method takes care of formatting a message received from an Agent, Supervisor, or Expert into
        /// a message to the customer. This will take care of the anonymity of identity.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string FormatHtmlCustomerMessage(InstantMessageReceivedEventArgs e, bool fallbackToText)
        {
            //looks up the agent that sent the message
            Agent agent = _matchMaker.LookupAgent("sip:" + e.Sender.UserAtHost);
            string publicName = null;
            string color = null;

            if (null != agent)
            {
                publicName = agent.PublicName;
                color = agent.InstantMessageColor;
            }
            else
            {
                // this is not an agent we can give away the name; application can change this behavior
                publicName = e.Sender.DisplayName;
                color = "black";
            }
            //prepends the message with the agent's name
            string message = null;

            if (fallbackToText)
            {
                message = String.Format("{0}: {1}", publicName, e.TextBody);
            }
            else
            {
                message = String.Format("<html><body><span style=\"color:{0};bold\">{1}:</span> {2}</body></html>", color, publicName, e.TextBody);
            }
            return message;
        }

        /// <summary>
        /// Copies instant messages received from the customer to the agent conference.
        /// </summary>
        private void OnCustomerMessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {
            if (_sessionState == CustomerSessionState.ConnectedToAgent
                || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
            {

                try
                {
                    _agentBackChannel.Flow.BeginSendInstantMessage(e.TextBody,
                    new AsyncCallback(delegate(IAsyncResult result)
                    {
                        try
                        {
                            _agentBackChannel.Flow.EndSendInstantMessage(result);
                        }
                        catch (RealTimeException ex)
                        {
                            _logger.Log("AcdCustomerSession failed to forward IM to agents", ex);
                        }
                    }),
                                                            null);
                }
                catch (RealTimeException)
                {
                    _logger.Log("AcdCustomerSession failed to send an IM to the agents");
                }
                catch (InvalidOperationException)
                {
                    _logger.Log("AcdCustomerSession failed to send an IM to the agents");

                }

            }
        }

        /// <summary>
        /// Unregisters the event handlers when the agent back channel gets terminated.
        /// </summary>
        private void OnAgentBackChannelTerminated(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Terminated)
            {
                _logger.Log("AcdCustomerSession agent back channel terminated");

                InstantMessagingFlow flow = sender as InstantMessagingFlow;
                flow.MessageReceived -= OnMessageReceivedFromAgent;
                flow.StateChanged -= OnAgentBackChannelTerminated;
                flow.RemoteComposingStateChanged -= OnTypingNotificationReceived;

                //terminate the session
                this.BeginShutdown(OnShutdownComplete, null);

            }
        }

        /// <summary>
        /// Unregisters the event handlers when the customer IM call gets terminated.
        /// </summary>
        private void OnCustomerIMFlowTerminated(object sender, MediaFlowStateChangedEventArgs e)
        {
            if (e.State == MediaFlowState.Terminated)
            {
                InstantMessagingFlow flow = sender as InstantMessagingFlow;
                flow.MessageReceived -= OnCustomerMessageReceived;
                flow.StateChanged -= OnCustomerIMFlowTerminated;
                flow.RemoteComposingStateChanged -= OnTypingNotificationReceived;
            }
        }

        /// <summary>
        /// Sends a generic welcome message to the client
        /// </summary>
        private void SendIMWelcomeMessages(ContentType bridgingMessageContentType, string imBridgingMessage, InstantMessagingCall imCall)
        {
            try
            {
                InstantMessagingCall imAgentBackChannel = _agentBackChannel;

                if (null != imAgentBackChannel.Flow)
                {
                    try
                    {
                        imAgentBackChannel.Flow.BeginSendInstantMessage(imBridgingMessage,
                        delegate(IAsyncResult result)
                        {
                            try
                            {
                                imAgentBackChannel.Flow.EndSendInstantMessage(result);
                            }
                            catch (RealTimeException ex)
                            {
                                _logger.Log("AcdCustomerSession failed sending the welcome message to agent", ex);
                            }
                        },
                                                                 imAgentBackChannel);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdCustomerSession could not send message to the agent backchannel as the call may have been torn down", ivoex);
                    }
                }

                if (null != imCall)
                {

                    ContentType contentType = new ContentType("text/plain");
                    if (null != bridgingMessageContentType)
                    {
                        contentType = bridgingMessageContentType;
                    }

                    try
                    {
                        imCall.Flow.BeginSendInstantMessage(contentType,
                                                     UTF8Encoding.UTF8.GetBytes(imBridgingMessage),
                        delegate(IAsyncResult result)
                        {
                            try
                            {
                                imCall.Flow.EndSendInstantMessage(result);
                            }
                            catch (RealTimeException ex)
                            {
                                _logger.Log("AcdCustomerSession failed sending the welcome message to customer", ex);
                            }
                        },
                                                     imCall);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdCustomerSession could not send initial message as the initial IM call was torn down", ivoex);

                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                _logger.Log("AcdCustomerSession failed sending the welcome message to agent or customer", ex);
            }
        }


        /// <summary>
        /// Checks whether the message received is from a supervisor to determine whether the IM needs to be screened.
        /// </summary>
        /// <param name="textBody">The text to be screened for a command.</param>
        /// <param name="sender">The sender of the IM in the back office</param>
        /// <returns>True if a command was found in the text.</returns>
        private bool ScreenSupervisorMessage(ConversationParticipant sender)
        {
            bool isSupervisor = false;

            //check if the message is from supervisor
            if (null != this.RespondingAgent)
            {
                if (SipUriCompare.Equals(sender.Uri, this.RespondingAgent.SupervisorUri))
                {
                    isSupervisor = true;
                    return isSupervisor;
                }

            }

            lock (_syncRoot)
            {

                foreach (Agent agent in this.AdditionalAgents)
                {
                    if (SipUriCompare.Equals(sender.Uri, agent.SupervisorUri))
                    {
                        isSupervisor = true;
                        break;

                    }
                }
            }

            return isSupervisor;
        }

        /// <summary>
        /// Sends an Instant Message on the agent back channel
        /// </summary>
        private void SendIMOnAgentBackChannel(string message)
        {
            if (_sessionState == CustomerSessionState.ConnectedToAgent
                || _sessionState == CustomerSessionState.ConnectedToAgentHeld)
            {
                try
                {
                    _agentBackChannel.Flow.BeginSendInstantMessage(message,
                    delegate(IAsyncResult result)
                    {
                        try
                        {
                            _agentBackChannel.Flow.EndSendInstantMessage(result);
                        }
                        catch (RealTimeException ex)
                        {
                            _logger.Log("AcdCustomerSession could not send a response on the agent back channel", ex);
                        }
                    },
                                                                null);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Log("AcdCustomerSession could not send a response on the agent back channel", ex);
                }
            }
        }


        private void ProcessAgentControlChannelRequestReceived(object sender, ContextChannelRequestReceivedEventArgs args)
        {
            AgentContextChannel controlChannel = sender as AgentContextChannel;

            _logger.Log("REQUEST RECEIVED:: AcdCustomerSession received the following request " + args.RequestType);


            switch (args.RequestType)
            {
                case ContextChannelRequestType.Hold:

                    try
                    {
                        this.BeginPutCustomerOnHold(pcoh =>
                        {
                            try
                            {
                                this.EndPutCustomerOnHold(pcoh);
                                args.Request.SendResponse("Success");

                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("AcdCustomerSession fails to end put customer on hold in process request received", ivoex);
                                args.Request.SendResponse("Failure");
                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdCustomerSession fails to end put customer on hold in process request received", rtex);
                                args.Request.SendResponse("Failure");

                            }

                        },
                                                    null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdCustomerSession fails to begin put customer on hold in process request received", ivoex);
                        args.Request.SendResponse("Failure");
                    }
                    break;

                case ContextChannelRequestType.Escalate:
                    EscalateRequest escalationRequest = args.Request as EscalateRequest;
                    List<AgentSkill> listOfRequestedSkills = new List<AgentSkill>();
                    List<agentSkillType> listOfAgentSkillTypes = new List<agentSkillType>(escalationRequest.Skills);

                    listOfAgentSkillTypes.ForEach(aSkillType =>
                    {
                        Skill skillMatch = null;

                        skillMatch = Skill.FindSkill(aSkillType.name, _matchMaker.Configuration.Skills);

                        if (null != skillMatch)
                        {
                            AgentSkill agentSkill = null;

                            try
                            {
                                agentSkill = new AgentSkill(skillMatch, aSkillType.Value);
                                listOfRequestedSkills.Add(agentSkill);

                            }
                            catch (ArgumentException aex)
                            {
                                _logger.Log("AcdCustomerSession failed to create an agent skill for an escalation to expert", aex);
                            }

                        }
                    });

                    if (listOfAgentSkillTypes.Count == 0)
                    {
                        _logger.Log("AcdCustomerSession could not find matching agent skills for the escalation");
                        escalationRequest.SendResponse("Failure");

                    }
                    try
                    {
                        this.BeginEscalateToExpert(listOfRequestedSkills,
                                                   ete =>
                                                   {
                                                       try
                                                       {
                                                           this.EndEscalateToExpert(ete);
                                                           args.Request.SendResponse("Success");

                                                       }
                                                       catch (Exception)
                                                       {
                                                           args.Request.SendResponse("Failure");

                                                       }

                                                   },
                                                      null);
                    }
                    catch (InvalidOperationException)
                    {
                        args.Request.SendResponse("Failure");
                    }

                    break;

                case ContextChannelRequestType.Retrieve:
                    try
                    {
                        this.BeginRetrieveCustomerFromHold(rcfh =>
                        {
                            try
                            {
                                this.EndRetrieveCustomerFromHold(rcfh);
                                args.Request.SendResponse("Success");

                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _logger.Log("AcdCustomerSession fails to end retrieve customer from hold in process request received", ivoex);
                                args.Request.SendResponse("Failure");
                            }
                            catch (RealTimeException rtex)
                            {
                                _logger.Log("AcdCustomerSession fails to end retrieve customer from hold in process request received", rtex);
                                args.Request.SendResponse("Failure");

                            }

                        },
                                                    null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _logger.Log("AcdCustomerSession fails to begin retrieve customer from hold in process request received", ivoex);
                        args.Request.SendResponse("Failure");
                    }
                    break;

                default:
                    _logger.Log(String.Format("AcdCustomerSession received an unknown request {0} from Agent", args.Request.RequestType.ToString()));
                    args.Request.SendResponse("Failure");
                    break;

            }

        }


        private void ProcessAgentControlChannelStateChanged(object sender, ConversationContextChannelStateChangedEventArgs args)
        {

            if (args.State == ConversationContextChannelState.Terminating
                && (args.PreviousState == ConversationContextChannelState.Established || args.PreviousState == ConversationContextChannelState.Recovering))
            {
                //only kill if the session was ever established.
                _logger.Log("AcdCustomerSession detected that the responding Agent's control channel was closed by the agent; previous state:" + args.PreviousState);
                this.BeginShutdown(this.OnShutdownComplete, null);

            }

        }


        private IAsyncResult BeginPutCustomerOnHold(AsyncCallback userCallback, Object state)
        {
            PutCustomerOnHoldAsyncResult asyncResult = new PutCustomerOnHoldAsyncResult(userCallback, state, this);

            bool process = false;

            lock (_syncRoot)
            {
                if (_sessionState == CustomerSessionState.ConnectedToAgent)
                {
                    this.UpdateState(CustomerSessionState.ConnectedToAgentHeld);

                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdCustomerSession is an invalid state to put the customer on hold");
                }

            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    PutCustomerOnHoldAsyncResult tempAr = waitState as PutCustomerOnHoldAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }

            return asyncResult;

        }

        private void EndPutCustomerOnHold(IAsyncResult ar)
        {
            PutCustomerOnHoldAsyncResult asyncResult = ar as PutCustomerOnHoldAsyncResult;

            asyncResult.EndInvoke();
        }

        private IAsyncResult BeginRetrieveCustomerFromHold(AsyncCallback userCallback, Object state)
        {
            RetrieveCustomerFromHoldAsyncResult asyncResult = new RetrieveCustomerFromHoldAsyncResult(userCallback, state, this);

            bool process = false;

            lock (_syncRoot)
            {
                if (_sessionState == CustomerSessionState.ConnectedToAgentHeld)
                {
                    this.UpdateState(CustomerSessionState.ConnectedToAgent);

                    process = true;
                }
                else
                {
                    throw new InvalidOperationException("AcdCustomerSession is an invalid state to retrieve the customer from hold");
                }

            }

            if (process)
            {
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    RetrieveCustomerFromHoldAsyncResult tempAr = waitState as RetrieveCustomerFromHoldAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }

            return asyncResult;

        }

        private void EndRetrieveCustomerFromHold(IAsyncResult ar)
        {
            RetrieveCustomerFromHoldAsyncResult asyncResult = ar as RetrieveCustomerFromHoldAsyncResult;

            asyncResult.EndInvoke();
        }


        private IAsyncResult BeginEscalateToExpert(List<AgentSkill> requestedSkills, AsyncCallback userCallback, Object state)
        {
            EscalateToExpertAsyncResult asyncResult = new EscalateToExpertAsyncResult(userCallback, state, requestedSkills, this);

            ThreadPool.QueueUserWorkItem((waitState) =>
            {
                EscalateToExpertAsyncResult tempAr = waitState as EscalateToExpertAsyncResult;
                tempAr.Process();
            }, asyncResult);

            return asyncResult;


        }

        private void EndEscalateToExpert(IAsyncResult ar)
        {

            EscalateToExpertAsyncResult asyncResult = ar as EscalateToExpertAsyncResult;
            asyncResult.EndInvoke();

        }


        /// <summary>
        /// Updates the state of the current AcdCustomerSession
        /// </summary>
        private void UpdateState(CustomerSessionState state)
        {
            CustomerSessionState previousState = _sessionState;

            lock (_syncRoot)
            {
                switch (state)
                {
                    case CustomerSessionState.Incoming:
                        _sessionState = state;
                        break;

                    case CustomerSessionState.CollectingData:
                        if (previousState == CustomerSessionState.Incoming)
                        {
                            _sessionState = state;
                        }
                        break;

                    case CustomerSessionState.AgentMatchMaking:
                        if (previousState == CustomerSessionState.CollectingData)
                        {
                            _sessionState = state;
                        }
                        break;

                    case CustomerSessionState.ConnectedToAgent:
                        if (previousState == CustomerSessionState.AgentMatchMaking
                            || previousState == CustomerSessionState.ConnectedToAgentHeld)
                        {
                            _sessionState = state;
                        }
                        break;

                    case CustomerSessionState.ConnectedToAgentHeld:
                        if (previousState == CustomerSessionState.ConnectedToAgent)
                        {
                            _sessionState = state;
                        }
                        break;

                    case CustomerSessionState.Terminating:
                        if (previousState != CustomerSessionState.Terminating
                            && previousState != CustomerSessionState.Terminated)
                        {
                            _sessionState = state;
                        }
                        break;

                    case CustomerSessionState.Terminated:
                        if (previousState == CustomerSessionState.Terminating)
                        {
                            _sessionState = state;
                        }
                        break;

                }
            }

            EventHandler<CustomerSessionStateChangedEventArgs> customerSessionStateChanged = this.CustomerSessionStateChanged;

            if (customerSessionStateChanged != null)
                customerSessionStateChanged(this, new CustomerSessionStateChangedEventArgs(previousState, state));
        }
        /// <summary>
        /// Complete the termination of the customer session
        /// </summary>
        private void OnShutdownComplete(IAsyncResult ar)
        {
            this.EndShutdown(ar);

        }
        #endregion Private methods

        #region Shutdown Async Result
        /// <summary>
        /// Represents the Async result to shut down the current acd customer session
        /// </summary>
        private class ShutdownAsyncResult : AsyncResultNoResult
        {
            private AcdCustomerSession _customerSession;

            internal ShutdownAsyncResult(AsyncCallback userCallBack, object state, AcdCustomerSession session)
                : base(userCallBack, state)
            {
                _customerSession = session;
            }

            internal void Process()
            {


                //Deallocating remaining agents.
                Agent respondingAgent = _customerSession.RespondingAgent;
                if (null != respondingAgent)
                {
                    try
                    {
                        _customerSession._logger.Log("De-allocating Agent " + respondingAgent.SignInAddress);

                        respondingAgent.Deallocate(_customerSession);
                        _customerSession._logger.Log("Agent " + respondingAgent.SignInAddress + "is de-allocated.");


                        //signal to the match maker that an agent is available
                        _customerSession._matchMaker.HandleNewAvailableAgent(respondingAgent);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _customerSession._logger.Log("AcdCustomerSession fails to deallocate the responding agent while terminating", ex);
                    }

                    foreach (Agent agent in _customerSession.AdditionalAgents)
                    {
                        try
                        {
                            _customerSession._logger.Log("DeAllocating Agent " + agent.SignInAddress);

                            agent.Deallocate(_customerSession);
                            _customerSession._logger.Log("Agent " + agent.SignInAddress + "is de-allocated.");


                            //signal to the match maker that an agent is available
                            _customerSession._matchMaker.HandleNewAvailableAgent(agent);

                        }
                        catch (InvalidOperationException ex)
                        {
                            _customerSession._logger.Log("AcdCustomerSession fails to deallocate additional agent while terminating the session", ex);
                        }
                    }
                }
                if ((null != _customerSession._agentBackChannel) &&
                    (null != _customerSession._agentBackChannel.Flow))
                {
                    //Send a message indicating that the session is over
                    try
                    {
                        _customerSession._agentBackChannel.Flow.BeginSendInstantMessage(_customerSession._matchMaker.Configuration.FinalMessageToAgent,
                        delegate(IAsyncResult ar)
                        {
                            InstantMessagingCall agentBackChannel = ar.AsyncState as InstantMessagingCall;
                            try
                            {
                                agentBackChannel.Flow.EndSendInstantMessage(ar);
                            }
                            catch (RealTimeException)
                            {
                                // eat the exception.
                            }
                        },
                                                                                     _customerSession._agentBackChannel);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _customerSession._logger.Log("AcdCustomerSession failed to send the final message to the agents", ivoex);
                    }
                }

                //Send a final meesage to the customer indicating that the session is over (UX)
                foreach (Call call in _customerSession._frontOfficeConversation.Calls)
                {
                    if (call is InstantMessagingCall)
                    {
                        InstantMessagingCall imCall = call as InstantMessagingCall;

                        if (null != imCall.Flow)
                        {

                            //Send a message indicating that the session is over
                            try
                            {
                                imCall.Flow.BeginSendInstantMessage(_customerSession._portal.Configuration.FinalMessage,
                                delegate(IAsyncResult ar)
                                {
                                    try
                                    {
                                        imCall.Flow.EndSendInstantMessage(ar);
                                    }
                                    catch (RealTimeException)
                                    {
                                        // eat the exception.
                                    }
                                },
                                                               imCall);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _customerSession._logger.Log("AcdCustomerSession failed to send the final message to the customer", ivoex);
                            }
                        }
                    }
                }

                //Unregister the event handlers for the Conversations and then terminate
                if (null != _customerSession._frontOfficeConversation)
                {
                    _customerSession._frontOfficeConversation.StateChanged -= _customerSession.HandleFrontAndBackOfficeConversationStateChanged;
                    _customerSession._frontOfficeConversation.PropertiesChanged -= _customerSession.HandleModalityChanges;
                }

                if (null != _customerSession._backOfficeConversation)
                {
                    _customerSession._backOfficeConversation.StateChanged -= _customerSession.HandleFrontAndBackOfficeConversationStateChanged;
                    _customerSession._backOfficeConversation.RemoteParticipantAttendanceChanged -= _customerSession.HandleAttendanceChanged;

                    _customerSession._backOfficeConversation.BeginTerminate(OnBackOfficeConversationTerminateComplete, _customerSession._backOfficeConversation);
                    if (null != _customerSession._agentControlChannel)
                    {
                        _customerSession._agentControlChannel.RequestReceived -= _customerSession.ProcessAgentControlChannelRequestReceived;
                        _customerSession._agentControlChannel.StateChanged -= _customerSession.ProcessAgentControlChannelStateChanged;
                    }
                }
                else if (null != _customerSession._frontOfficeConversation)
                {
                    _customerSession._frontOfficeConversation.BeginTerminate(OnFrontOfficeConversationTerminateComplete, _customerSession._frontOfficeConversation);
                }
            }

            /// <summary>
            /// Finishes the termination of the back office conversation and then 
            /// terminates the front office conversation.
            /// </summary>
            void OnBackOfficeConversationTerminateComplete(IAsyncResult ar)
            {
                var conversation = ar.AsyncState as Conversation;
                conversation.EndTerminate(ar);

                if (null != _customerSession._frontOfficeConversation)
                {
                    _customerSession._frontOfficeConversation.BeginTerminate(OnFrontOfficeConversationTerminateComplete, _customerSession._frontOfficeConversation);
                }
                else
                {
                    this.FinishTermination();
                }
            }

            /// <summary>
            /// Finishes the termination of the front office Conversation
            /// </summary>
            void OnFrontOfficeConversationTerminateComplete(IAsyncResult ar)
            {
                var conversation = ar.AsyncState as Conversation;
                conversation.EndTerminate(ar);
                FinishTermination();
            }

            void FinishTermination()
            {
                //Deallocating remaining agents.

                if (null != _customerSession._customerCallAnchor)
                {
                    _customerSession._customerCallAnchor.BeginShutDown(sd =>
                     {
                         _customerSession._customerCallAnchor.EndShutDown(sd);
                         //Update the state 

                         _customerSession.UpdateState(CustomerSessionState.Terminated);

                         //completes the operation
                         this.SetAsCompleted(null, false);

                         //completes any other pending requests if any
                         foreach (ShutdownAsyncResult result in _customerSession._listOfShutdownAsyncResults)
                         {
                             result.SetAsCompleted(null, false);
                         }
                     },
                                                                      null);

                }
                else
                {
                    _customerSession.UpdateState(CustomerSessionState.Terminated);

                    //completes the operation
                    this.SetAsCompleted(null, false);

                    //completes any other pending requests if any
                    foreach (ShutdownAsyncResult result in _customerSession._listOfShutdownAsyncResults)
                    {
                        result.SetAsCompleted(null, false);
                    }
                }
            }

        }
        #endregion Shutdown Async Result

        #region PutCustomerOnHoldAsyncResult
        private class PutCustomerOnHoldAsyncResult : AsyncResultNoResult
        {
            private AcdCustomerSession _customerSession;

            internal PutCustomerOnHoldAsyncResult(AsyncCallback userCallBack, object state, AcdCustomerSession session)
                : base(userCallBack, state)
            {
                _customerSession = session;

            }

            internal void Process()
            {
                //Let the customer know that s/he is on hold if IM is used.
                if (_customerSession._frontOfficeConversation.GetActiveMediaTypes().Contains(MediaType.Message))
                {
                    InstantMessagingCall imCall = null;
                    List<Call> listOfCalls = new List<Call>(_customerSession._frontOfficeConversation.Calls);
                    listOfCalls.ForEach(call =>
                    {
                        if (call is InstantMessagingCall)
                        { imCall = call as InstantMessagingCall; }
                    });

                    _customerSession.SendIMWelcomeMessages(null, _customerSession._portal.Configuration.ImPleaseHoldMessage, imCall);

                }

                if (_customerSession._frontOfficeConversation.GetActiveMediaTypes().Contains(MediaType.Audio))
                {
                    if (_customerSession._customerPrivateAudioChannel == null)
                    {
                        _customerSession._customerPrivateAudioChannel = new AcdServiceChannel(_customerSession._customerCallAnchor, _customerSession._logger, true);

                        try
                        {
                            _customerSession._customerPrivateAudioChannel.BeginStartUp(MediaType.Audio,
                                                                                       ServiceChannelType.DialIn,
                            suar =>
                            {
                                try
                                {
                                    _customerSession._customerPrivateAudioChannel.EndStartUp(suar);

                                    try
                                    {
                                        this.BeginPlaybackMusicOnHold(pmoh =>
                                        {
                                            try
                                            {
                                                this.EndPlaybackMusicOnhold(pmoh);
                                                this.SetAsCompleted(null, false);
                                            }
                                            catch (InvalidOperationException ivoex)
                                            {
                                                this.SetAsCompleted(new OperationFailureException("AcdCustomerSession end moh play back", ivoex), false);

                                            }
                                            catch (RealTimeException rtex)
                                            {
                                                this.SetAsCompleted(rtex, false);
                                            }
                                        },
                                        null,
                                        _customerSession);
                                    }
                                    catch (InvalidOperationException ivoex)
                                    {
                                        this.SetAsCompleted(new OperationFailureException("AcdCustomerSession begin moh play back", ivoex), false);
                                    }

                                }
                                catch (RealTimeException rtex)
                                {
                                    _customerSession._logger.Log("AcdCustomerSession is unable to start a service channel", rtex);
                                    this.SetAsCompleted(rtex, false);
                                }
                            },
                            null);

                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _customerSession._logger.Log("AcdCustomerSession is unable to start a service channel", ivoex);
                            this.SetAsCompleted(new OperationFailureException("AcdCustomerSession is unable to start a service channel", ivoex), false);
                        }
                    }
                    else
                    {
                        try
                        {
                            this.BeginPlaybackMusicOnHold(pmoh =>
                            {
                                try
                                {
                                    this.EndPlaybackMusicOnhold(pmoh);
                                    this.SetAsCompleted(null, false);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    this.SetAsCompleted(new OperationFailureException("AcdCustomerSession is unable to start a service channel", ivoex), false);

                                }
                                catch (RealTimeException rtex)
                                {
                                    this.SetAsCompleted(rtex, false);
                                }
                            },
                            null,
                            _customerSession);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            this.SetAsCompleted(new OperationFailureException("AcdCustomerSession is unable to play back MOH", ivoex), false);

                        }

                    }

                }
                else
                {
                    this.SetAsCompleted(null, false);
                }

            }

            private IAsyncResult BeginPlaybackMusicOnHold(AsyncCallback userCallback, Object state, AcdCustomerSession session)
            {
                PlaybackMusicOnHoldAsyncResult asyncResult = new PlaybackMusicOnHoldAsyncResult(userCallback, state, session);

                if (null != session._customerPrivateAudioChannel)
                {
                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        PlaybackMusicOnHoldAsyncResult tempAr = waitState as PlaybackMusicOnHoldAsyncResult;
                        tempAr.Process();
                    }, asyncResult);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                return asyncResult;

            }

            private void EndPlaybackMusicOnhold(IAsyncResult ar)
            {
                PlaybackMusicOnHoldAsyncResult asyncResult = ar as PlaybackMusicOnHoldAsyncResult;
                asyncResult.EndInvoke();
            }



            private class PlaybackMusicOnHoldAsyncResult : AsyncResultNoResult
            {
                private AcdCustomerSession _customerSession;

                internal PlaybackMusicOnHoldAsyncResult(AsyncCallback userCallback, Object state, AcdCustomerSession session)
                    : base(userCallback, state)
                {
                    _customerSession = session;
                }

                internal void Process()
                {
                    try
                    {
                        _customerSession._customerPrivateAudioChannel.BeginEstablishPrivateAudioChannel(_customerSession._backOfficeConversation.LocalParticipant.Uri,
                                                                                                        true,
                        eap =>
                        {
                            try
                            {
                                _customerSession._customerPrivateAudioChannel.EndEstablishPrivateAudioChannel(eap);
                                _customerSession._matchMaker.MusicOnHoldServer.BeginEstablishMohChannel(_customerSession._customerPrivateAudioChannel,
                                emoh =>
                                {
                                    try
                                    {
                                        _customerSession._matchMaker.MusicOnHoldServer.EndEstablishMohChannel(emoh);
                                        this.SetAsCompleted(null, false);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _customerSession._logger.Log("AcdCustomerSession failed to end establish the Music On Hold Channel", rtex);
                                        this.SetAsCompleted(rtex, false);
                                    }
                                },
                                                                                                        null);
                            }
                            catch (InvalidOperationException ivoex)
                            {
                                _customerSession._logger.Log("AcdCustomerSession failed to begin establish the Music On Hold Channel", ivoex);
                                this.SetAsCompleted(new OperationFailureException("AcdCustomerSession failed to begin establish the Music On Hold Channel", ivoex), false);
                            }
                            catch (RealTimeException rtex)
                            {
                                _customerSession._logger.Log("AcdCustomerSession failed to end establish the Private Audio Channel", rtex);
                                this.SetAsCompleted(rtex, false);

                            }
                        },
                                                                                    null);

                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _customerSession._logger.Log("AcdCustomerSession is unable to begin establish a private channel", ivoex);
                        this.SetAsCompleted(new OperationFailureException("AcdCustomerSession is unable to begin establish a private channel", ivoex), false);
                    }
                }
            }
        }
        #endregion PutCustomerOnHoldAsyncResult

        #region RetrieveCustomerFromHoldAsyncResult

        private class RetrieveCustomerFromHoldAsyncResult : AsyncResultNoResult
        {
            private AcdCustomerSession _customerSession;

            internal RetrieveCustomerFromHoldAsyncResult(AsyncCallback userCallback, Object state, AcdCustomerSession session)
                : base(userCallback, state)
            {
                _customerSession = session;
            }

            internal void Process()
            {

                if (_customerSession._frontOfficeConversation.GetActiveMediaTypes().Contains(MediaType.Message))
                {
                    InstantMessagingCall imCall = null;
                    List<Call> listOfCalls = new List<Call>(_customerSession._frontOfficeConversation.Calls);
                    listOfCalls.ForEach(call =>
                    {
                        if (call is InstantMessagingCall)
                        { imCall = call as InstantMessagingCall; }
                    });

                    _customerSession.SendIMWelcomeMessages(null, _customerSession._portal.Configuration.ImBridgingMessage, imCall);

                }

                if (_customerSession._customerPrivateAudioChannel != null)
                {
                    try
                    {
                        _customerSession._matchMaker.MusicOnHoldServer.BeginTerminateMohChannel(_customerSession._customerPrivateAudioChannel,
                                                                                                tmoh =>
                                                                                                {
                                                                                                    try
                                                                                                    {
                                                                                                        _customerSession._matchMaker.MusicOnHoldServer.EndTerminateMohChannel(tmoh);
                                                                                                    }
                                                                                                    catch (RealTimeException rtex)
                                                                                                    {
                                                                                                        _customerSession._logger.Log("AcdCustomerSession failed to end terminate the Moh Channel", rtex);
                                                                                                        this.SetAsCompleted(rtex, false);
                                                                                                        return;
                                                                                                    }

                                                                                                    try
                                                                                                    {
                                                                                                        _customerSession._customerPrivateAudioChannel.BeginBringAllChannelParticipantsInMainAudioMix(bacpimam =>
                                                                                                        {
                                                                                                            try
                                                                                                            {
                                                                                                                _customerSession._customerPrivateAudioChannel.EndBringAllChannelParticipantsInMainAudioMix(bacpimam);
                                                                                                                this.SetAsCompleted(null, false);
                                                                                                            }
                                                                                                            catch (RealTimeException rtex)
                                                                                                            {
                                                                                                                _customerSession._logger.Log("AcdCustomerSession failed to end bring all participants in main mix", rtex);
                                                                                                                this.SetAsCompleted(rtex, false);
                                                                                                                return;
                                                                                                            }
                                                                                                        },
                                                                                                                                                                                                     null);
                                                                                                    }
                                                                                                    catch (InvalidOperationException ivoex)
                                                                                                    {
                                                                                                        _customerSession._logger.Log("AcdCustomerSession failed to begin bring all participants in main mix", ivoex);
                                                                                                        this.SetAsCompleted(new OperationFailureException("AcdCustomerSession failed to begin bring all participants in main mix", ivoex), false);
                                                                                                        return;
                                                                                                    }

                                                                                                },
                                                                                                null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _customerSession._logger.Log("AcdCustomerSession failed to begin terminate the Moh Channel", ivoex);
                        this.SetAsCompleted(new OperationFailureException("AcdCustomerSession failed to begin terminate the Moh Channel", ivoex), false);
                    }

                }
                else
                {
                    this.SetAsCompleted(null, false);
                }

            }
        }
        #endregion RetrieveCustomerFromHoldAsyncResult

        #region EscalateToExpertAsyncResult

        private class EscalateToExpertAsyncResult : AsyncResultNoResult
        {
            List<AgentSkill> _requestedSkills;
            AcdCustomerSession _acdCustomerSession;

            internal EscalateToExpertAsyncResult(AsyncCallback userCallback, Object state, List<AgentSkill> requestedSkills, AcdCustomerSession session)
                : base(userCallback, state)
            {
                _acdCustomerSession = session;
                _requestedSkills = new List<AgentSkill>(requestedSkills);

            }

            internal void Process()
            {

                try
                {

                    _acdCustomerSession._portal.AgentHunter.BeginHuntForAgent(_acdCustomerSession,
                                                                              _requestedSkills,
                                                                              hfa =>
                                                                              {
                                                                                  try
                                                                                  {
                                                                                      AgentHuntResult agentResult = _acdCustomerSession._portal.AgentHunter.EndHuntForAgent(hfa);

                                                                                      Agent agent = agentResult.Agent;


                                                                                      lock (_acdCustomerSession._syncRoot)
                                                                                      {

                                                                                          if (_acdCustomerSession._sessionState != CustomerSessionState.Terminating && _acdCustomerSession._sessionState != CustomerSessionState.Terminated)
                                                                                          {

                                                                                              _acdCustomerSession.AdditionalAgents.Add(agent);
                                                                                              this.SetAsCompleted(null, false);
                                                                                          }
                                                                                          else
                                                                                          {

                                                                                              //Deallocate the agent as the session got terminated.
                                                                                              _acdCustomerSession._logger.Log("De-allocating Agent " + agent.SignInAddress + "the session got terminated.");
                                                                                              try
                                                                                              {
                                                                                                  agent.Deallocate(this);

                                                                                                  _acdCustomerSession._logger.Log("Agent " + agent.SignInAddress + " is deallocated");

                                                                                                  //signal to the match maker that an agent is available again
                                                                                                  _acdCustomerSession._matchMaker.HandleNewAvailableAgent(agent);
                                                                                              }
                                                                                              catch (InvalidOperationException ivoex)
                                                                                              {
                                                                                                  //eat the exception
                                                                                                  _acdCustomerSession._logger.Log("AcdCustomerSession tries to deallocate an unallocated agent iN Escalate to Expert", ivoex);
                                                                                              }
                                                                                              this.SetAsCompleted(null, false);
                                                                                          }

                                                                                      }
                                                                                  }
                                                                                  catch (TimeoutException toex)
                                                                                  {
                                                                                      _acdCustomerSession._logger.Log("AcdCustomerSession failed to get an expert in time", toex);
                                                                                      this.SetAsCompleted(toex, false);
                                                                                  }
                                                                                  catch (OperationFailureException ofex)
                                                                                  {
                                                                                      _acdCustomerSession._logger.Log("AcdCustomerSession failed to get an agent match as the match maker got terminated", ofex);
                                                                                      this.SetAsCompleted(ofex, false);

                                                                                  }
                                                                                  catch (OperationTimeoutException otex)
                                                                                  {
                                                                                      _acdCustomerSession._logger.Log("AcdCustomerSession failed to escalate to an expert in time", otex);
                                                                                      this.SetAsCompleted(otex, false);
                                                                                  }
                                                                                  catch (InvalidOperationException ivoex)
                                                                                  {
                                                                                      _acdCustomerSession._logger.Log("AcdCustomerSession failed to escalate an expert as the conference join operation failed ", ivoex);
                                                                                      this.SetAsCompleted(new OperationFailureException("AcdCustomerSession failed to escalate an expert as the conference join operation failed ", ivoex), false);

                                                                                  }
                                                                                  catch (RealTimeException rtex)
                                                                                  {
                                                                                      _acdCustomerSession._logger.Log("AcdCustomerSession failed to get an expert h as the conference join operation failed ", rtex);
                                                                                      this.SetAsCompleted(rtex, false);
                                                                                  }

                                                                              },
                                                                              null);
                }
                catch (InvalidOperationException ivoex)
                {
                    _acdCustomerSession._logger.Log("AcdCustomerSession failed to begin hunt for an agent", ivoex);
                    this.SetAsCompleted(new OperationFailureException("AcdCustomerSession failed to begin hunt for an agent", ivoex), false);
                }
            }


        }

        #endregion EscalateToExpertAsyncResult
    }

    internal enum CustomerSessionState
    {
        Incoming = 0,
        CollectingData = 1,
        AgentMatchMaking = 2,
        ConnectedToAgent = 3,
        ConnectedToAgentHeld = 4,
        Terminating = 5,
        Terminated = 6
    };

    internal class CustomerSessionStateChangedEventArgs : EventArgs
    {
        private CustomerSessionState _previousState;
        private CustomerSessionState _newState;

        internal CustomerSessionStateChangedEventArgs(CustomerSessionState previousState, CustomerSessionState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal CustomerSessionState PreviousState
        {
            get { return _previousState; }
        }

        internal CustomerSessionState NewState
        {
            get { return _newState; }
        }
    }
}


