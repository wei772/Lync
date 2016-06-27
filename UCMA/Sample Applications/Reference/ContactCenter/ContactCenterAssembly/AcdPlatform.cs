/*=====================================================================
  File:      AcdPlatform.cs

  Summary:   Manages the platform and for the system, and bootstraps portal
             instances.
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Signaling;
using System.Xml.Linq;
using System.Xml;
using System.Linq;
using Microsoft.Rtc.Collaboration.Samples.ApplicationSharing;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Net.Mime;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Utilities;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// Implements the core of the application, including the management of the platform and the 
    /// application endpoint. This class listens for incoming calls, and delegates processing to
    /// a number of handler classes, such as ConferenceHandler and LocateAvailableAgentHandler.
    /// On shutdown, a message is send to all active parties notifying them of impending shutdown.
    /// After a certain period of time, the calls are then written a final message and forcefully 
    /// terminated. This class is a singleton.
    /// </summary>
    public class AcdPlatform
    {
        //private AcdConfiguration config;
        private AcdPlatformConfiguration _configuration;
        internal AcdAgentMatchMakerConfiguration _matchMakerConfiguration;
        private CollaborationPlatform _platform;
        internal List<AcdPortal> _portals;
        internal AcdAgentMatchMaker _matchMaker;
        private object _syncRoot = new object();
        private AcdLogger _logger;
        internal int _numberOfPortals = 0;
        private AcdPlatformState _acdPlatformState;
        private List<ShutdownAsyncResult> _listOfShutdownAsyncResults = new List<ShutdownAsyncResult>();
        private ApplicationEndpoint _defaultRoutingEndpoint;
        private string _wcfAnonymousSubscriberUri;
        

        #region constructor
        public AcdPlatform()
        {
            _portals = new List<AcdPortal>();
            _acdPlatformState = AcdPlatformState.Created;
        }
        #endregion

        #region events
        internal event EventHandler<AcdPlatformStateChangedEventArgs> StateChanged;
        private EventHandler<AcdPlatformAnonymousSubscriberUriChangedEventArgs> _delegateForAnonymousSubscriber;
        internal event EventHandler<AcdPlatformAnonymousSubscriberUriChangedEventArgs> AnonymousSubscriberUriChanged
        {
            add
            {
                lock (_syncRoot)
                {
                    _delegateForAnonymousSubscriber += value;

                    //fire up the event immediately
                    if(!String.IsNullOrEmpty(_wcfAnonymousSubscriberUri))
                    {
                      value(this, new AcdPlatformAnonymousSubscriberUriChangedEventArgs(_wcfAnonymousSubscriberUri));
                    }
                }
            }

            remove
            {
                lock (_syncRoot)
                {
                    _delegateForAnonymousSubscriber -= value;
                }
            
            }

        }
        #endregion events

        #region Properties
        internal  AcdAgentMatchMaker MatchMaker
        {
            get {return _matchMaker;}
        }

        internal CollaborationPlatform CollaborationPlatform
        {
            get {return _platform;}
        }

        internal AcdPlatformState AcdPlatformState
        {
            get {return _acdPlatformState;}    
        }
        #endregion properties

        #region methods

        /// <summary>
        /// Updates the platform state to the new state and fires an event to all listeners informing them of the state change
        /// </summary>
        private void UpdateAcdPlatformState(AcdPlatformState newState)
        {
            AcdPlatformState oldState = _acdPlatformState;


            lock (_syncRoot)
            {

                switch (newState)
                { 
                
                    case ContactCenter.AcdPlatformState.Created:
                        _acdPlatformState = newState;
                        break;
                    case ContactCenter.AcdPlatformState.Starting:
                        if (_acdPlatformState == ContactCenter.AcdPlatformState.Created)
                        {
                            _acdPlatformState = newState;
                        }
                        break;

                    case ContactCenter.AcdPlatformState.Started:
                        if (_acdPlatformState == ContactCenter.AcdPlatformState.Starting)
                        {
                            _acdPlatformState = newState;
                        }
                        break;

                    case ContactCenter.AcdPlatformState.Terminating:
                        if (_acdPlatformState == ContactCenter.AcdPlatformState.Started)
                        {
                            _acdPlatformState = newState;
                        }
                        break;

                    case ContactCenter.AcdPlatformState.Terminated:
                        if (_acdPlatformState == ContactCenter.AcdPlatformState.Terminating)
                        {
                            _acdPlatformState = newState;
                        }
                        break;
                }
                             
            }

            _acdPlatformState = newState;
            if (StateChanged != null)
            {
                StateChanged(this, new AcdPlatformStateChangedEventArgs
                    (oldState, newState));
            }
        }

        private void UpdateAcdPlatformAnonymousSubscriberUri(string anonymousSubsriberUri)
        {
            if (_delegateForAnonymousSubscriber != null)
            {
                _delegateForAnonymousSubscriber(this, new AcdPlatformAnonymousSubscriberUriChangedEventArgs(anonymousSubsriberUri));
            }
        
        }

        public IAsyncResult BeginStartUp(string ConfigXMLFile, AsyncCallback userCallback, object state)
        {
            var asyncResult = new StartupAsyncResult(userCallback, state, ConfigXMLFile, this);

            lock (_syncRoot)
            {
                if (_acdPlatformState == AcdPlatformState.Created)
                {
                    this.UpdateAcdPlatformState(AcdPlatformState.Starting);
                    ThreadPool.QueueUserWorkItem((waitState) =>
                    {
                        var tempAr = waitState as StartupAsyncResult;
                        tempAr.Process();
                    }, asyncResult);
                }
                else
                {
                    throw new InvalidOperationException("AcdPlatform has already been started");
                }
            }

            return asyncResult;
        }

        public void EndStartUp(IAsyncResult ar)
        {
            var result = ar as StartupAsyncResult;

            result.EndInvoke();
        }

        public IAsyncResult BeginShutdown(AsyncCallback userCallBack, object state)
        {
            var asyncResult = new ShutdownAsyncResult(userCallBack, state, this);
            bool firstTime = false;

            lock (_syncRoot)
            {
                if (_acdPlatformState < AcdPlatformState.Terminating)
                {
                   this.UpdateAcdPlatformState(AcdPlatformState.Terminating);
                   firstTime = true;
                }
                else if (_acdPlatformState == AcdPlatformState.Terminating)
                {
                    _listOfShutdownAsyncResults.Add(asyncResult);
                  
                }
                else if (_acdPlatformState == AcdPlatformState.Terminated)
                {
                    asyncResult.SetAsCompleted(null, true);
                    return asyncResult;
                }
            }

            if (firstTime == true)
            {
                UnregisterForPlatformAutoProvisioningEvents();
                
                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as ShutdownAsyncResult;
                    tempAr.Process();
                }, asyncResult);
            }
            
            return asyncResult;
        }

        public void EndShutdown(IAsyncResult ar)
        {
            var result = ar as ShutdownAsyncResult;
            result.EndInvoke();
        }

        /// <summary>
        /// RegisterForPlatformAutoProvisioningEvents is the method used to register to applicationEndpoint 
        /// creation events. The generic routing and trust settings must be 
        /// </summary>
        private void RegisterForPlatformAutoProvisioningEvents()
        {
            _platform.RegisterForApplicationEndpointSettings(OnNewPortalFound);
        }

        private void UnregisterForPlatformAutoProvisioningEvents()
        {
            if (_platform != null)
            {
                _platform.UnregisterForApplicationEndpointSettings(OnNewPortalFound);
                _platform.ProvisioningFailed -= OnPlatformProvisioningFailed;
            }
        }

        //Auto Provisioning
        private void OnNewPortalFound(object sender, ApplicationEndpointSettingsDiscoveredEventArgs args)
        {
            string entityUri = args.ApplicationEndpointSettings.OwnerUri;
  
            // The endpoint created is not a matchmaker, verify the endpoint created has been provisioned as an Acdportal
            _configuration.PortalConfigurations.ForEach(delegate(AcdPortalConfiguration pConfig)
            {
                if (SipUriCompare.Equals(entityUri, pConfig.Uri))
                {
                    lock (_syncRoot)
                    {
                        if (_acdPlatformState < AcdPlatformState.Terminating)
                        {
                            AcdPortal portal = new AcdPortal(this, pConfig, args.ApplicationEndpointSettings);
                            _portals.Add(portal);
                            _logger.Log(String.Format("AcdPlatform Added AcdPortal {0} to the cache ", entityUri));

                            portal.BeginStartUp(this.OnPortalStartUpComplete, portal);
                            _logger.Log(String.Format("AcdPlatform Starting AcdPortal {0}", entityUri));
                        }
                    }

                    return;
                }
            });
        }

        private void OnPortalStartUpComplete(IAsyncResult ar)
        {
            AcdPortal portal = ar.AsyncState as AcdPortal;

            try
            {
                portal.EndStartUp(ar);
            }
            catch (OperationFailureException ofex)
            {
                _logger.Log(String.Format("AcdPlatform could not start the portal {0}, exception: {1}", portal.Uri, ofex));
            }
        }

        private void OnPlatformProvisioningFailed(object sender, ProvisioningFailedEventArgs args)
        {
            _logger.Log("AcdPlatform auto provisioning failed", args.Exception);
        }

        #endregion

        #region StartUp Async Result

        /// <summary>
        /// Start up the platform, then cascade to starting the endpoint.
        /// </summary>
        private class StartupAsyncResult : AsyncResultNoResult
        {
            private AcdPlatform _acdPlatform;
            private string _configXMLDoc;


            private readonly ContentType DiscoveryContentType = new ContentType("application/ContactCenterDiscovery+xml");


            internal StartupAsyncResult(AsyncCallback userCallback, object state, string configXMLDoc, AcdPlatform platform)
                : base(userCallback, state)
            {
                _acdPlatform = platform;
                _configXMLDoc = configXMLDoc;
            }

            internal void Process()
            {
                _acdPlatform._logger = new AcdLogger();

                if (false == _acdPlatform.ProcessConfigurationFile(_configXMLDoc))
                {
                  this.SetAsCompleted( new InvalidOperationException("AcdPlatform configuration cannot be parsed"), false);
                }

                //Create the platform settings
                ProvisionedApplicationPlatformSettings platformSettings = new ProvisionedApplicationPlatformSettings(_acdPlatform._configuration.ApplicationUserAgent,
                                                                                                                     _acdPlatform._configuration.ApplicationUrn);
                       
                //Create the CollaborationPlatform             
                _acdPlatform._platform = new CollaborationPlatform(platformSettings);

                //Mark that we only support text/plain
                _acdPlatform._platform.InstantMessagingSettings.SupportedFormats = InstantMessagingFormat.PlainText;

                //Extend the support of the CollaborationPlatform for ApplicationSharing
                _acdPlatform._platform.RegisterPlatformExtension(new ApplicationSharingCallFactory());
                _acdPlatform._platform.RegisterPlatformExtension(new ApplicationSharingMcuSessionFactory());
                _acdPlatform._platform.RegisterPlatformExtension(new ApplicationSharingMcuSessionNotificationProcessorFactory());

                //Register a separate event handler for the AcdAgentMatchMaker as this is meant to be processed
                //by the async result
                _acdPlatform._platform.RegisterForApplicationEndpointSettings(OnMatchMakerFound);
                _acdPlatform._platform.ProvisioningFailed += _acdPlatform.OnPlatformProvisioningFailed;

                //starts the CollaborationPlatform
                try
                {
                    _acdPlatform._platform.BeginStartup(this.OnPlatformStartupComplete, null);
                }
                catch (InvalidOperationException ivoex)
                {
                    _acdPlatform._logger.Log("AcdPortal the collaboration platform may already be started", ivoex);
                }
            }
    
            /// <summary>
            /// userCallback invoked by the CollaborationPlatform upon completion of the platform start up operation.
            /// </summary>
            /// <param name="ar"></param>
            private void OnPlatformStartupComplete(IAsyncResult ar)
            {
                try
                {
                    _acdPlatform._platform.EndStartup(ar);

                    // The CollaborationPlatform is started and is listening on the port supplied in the configuration

                    // We are now establishing the default routing endpoint to receive messages sent from the Web WCF Contact
                    // Center service and targeting the Contact Center pool GRUU

                    // cooking up a URI for default routing endpoint.
                    string defaultRoutingEndpointUri = "sip:" + new SipUriParser(_acdPlatform._platform.ActiveGruu).User;
                    
                    ApplicationEndpointSettings defaultRoutingEndpointSettings = new ApplicationEndpointSettings(defaultRoutingEndpointUri);
                    defaultRoutingEndpointSettings.IsDefaultRoutingEndpoint = true;
                    defaultRoutingEndpointSettings.ProvisioningDataQueryDisabled = true;

                    _acdPlatform._defaultRoutingEndpoint = new ApplicationEndpoint(_acdPlatform._platform, defaultRoutingEndpointSettings);

                    _acdPlatform._defaultRoutingEndpoint.InnerEndpoint.MessageReceived += OnDefaultRoutingEndpointMessageReceived;

                    try
                    {
                        _acdPlatform._defaultRoutingEndpoint.BeginEstablish(dre =>
                          {
                              try
                              {
                                  _acdPlatform._defaultRoutingEndpoint.EndEstablish(dre);
                              }
                              catch (RealTimeException rtex)
                              {
                                  _acdPlatform._logger.Log("AcdPlatform failed to establish the default routing endpoint", rtex);
                                  this.SetAsCompleted(rtex, false);

                              }
                          },
                          null);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        _acdPlatform._logger.Log("AcdPlatform failed to begin establish the default routing endpoint", ivoex);
                        this.SetAsCompleted(new OperationFailureException("AcdPlatform failed to begin establish the default routing endpoint", ivoex), false);
                      
                    }
                }
                catch (ConnectionFailureException ex)
                {
                    _acdPlatform._logger.Log("AcdPlatform the listening port might be used by another application", ex);
                    this.SetAsCompleted(ex, false);
                }
                catch (RealTimeException rtex)
                {
                    _acdPlatform._logger.Log("AcdPlatform auto-provisioning may have failed", rtex);
                    this.SetAsCompleted(rtex, false);
                }
            }

            private void OnDefaultRoutingEndpointMessageReceived(object sender, MessageReceivedEventArgs args)
            { 
            
              //First verify the origin to see whether it is a legitimate source (i.e. Contact Center WCF Service)
              List<SignalingHeader> listOfHeaders = new List<SignalingHeader>(args.RequestData.SignalingHeaders);
              
              listOfHeaders.ForEach(header=> 
              {
                if (header.Name.Equals("Contact", StringComparison.OrdinalIgnoreCase))
                {
                    
                    // check whether the contact address is a trusted GRUU and thus a legitimate source
                    if (_acdPlatform._platform.TopologyConfiguration.IsTrusted(args.Participant.ContactUri))
                    {
                        //check that the verb and content-type of the incoming request
                        if (   args.MessageType == MessageType.Service 
                            && args.ContentType != null
                            && args.ContentType.Equals(this.DiscoveryContentType))
                        {
                            // this is a legitimate request, let's return the list of portals to the requestor.
                            // the information needed consists of a collection of token representing the portal and its SIP URI
                            _acdPlatform._wcfAnonymousSubscriberUri = args.Participant.Uri;

                            _acdPlatform.UpdateAcdPlatformAnonymousSubscriberUri(_acdPlatform._wcfAnonymousSubscriberUri);

                            try
                            {
                                // construct the list of Portals that need to be sent back.
                                queueUriMappingListType contactCenterDiscoveryInfo = new queueUriMappingListType();

                                contactCenterDiscoveryInfo.queueUriMapping = new queueUriMappingType[_acdPlatform._configuration.PortalConfigurations.Count];

                                int i = 0;

                                _acdPlatform._configuration.PortalConfigurations.ForEach(portalConfig =>
                                 {
                                     queueUriMappingType portalDiscoveryInfo = new queueUriMappingType();
                                     portalDiscoveryInfo.queueName = portalConfig.Token;
                                     portalDiscoveryInfo.uriValue = portalConfig.Uri;

                                     contactCenterDiscoveryInfo.queueUriMapping[i++] = portalDiscoveryInfo;
 
                                 });


                                //Serialize using XmlSerializer.

                                XmlSerializer serializer = new XmlSerializer(typeof(queueUriMappingListType));
                                string discoveryResponse = SerializerHelper.SerializeObjectFragment(contactCenterDiscoveryInfo, serializer);
                        

                                args.SendResponse(ResponseCode.Success,
                                                  new ContentType("application/ContactCenterDiscovery+xml"),
                                                  discoveryResponse, 
                                                  null);

                            }


                            catch (XmlException xe)
                            {

                                _acdPlatform._logger.Log("AcdPlatform experienced an issue serializing the Contact Center discovery info", xe);

                            }  
                            catch (InvalidOperationException ivoex)
                            {
                                _acdPlatform._logger.Log("AcdPlatform failed to respond to a portal discovery request", ivoex);

                            }
                            catch (RealTimeException rtex)
                            {
                                _acdPlatform._logger.Log("AcdPlatform failed to respond to a portal discovery request", rtex);
                            }

                        }
                                   
                    }
                }
             
                

              });

             
            
            }

            private void OnMatchMakerFound(object sender, ApplicationEndpointSettingsDiscoveredEventArgs args)
            {               
                string entityUri = args.ApplicationEndpointSettings.OwnerUri;
                if (SipUriCompare.Equals(entityUri, _acdPlatform._matchMakerConfiguration.Uri))
                {
                    _acdPlatform._logger.Log(String.Format("AcdPlatform new AcdAgentMatchMaker found for contact uri {0}, display name {1}", entityUri, args.ApplicationEndpointSettings.OwnerDisplayName));

                    if (_acdPlatform._acdPlatformState != AcdPlatformState.Terminating
                        && _acdPlatform._acdPlatformState != AcdPlatformState.Terminated)
                    {
                        _acdPlatform._matchMaker = new AcdAgentMatchMaker(_acdPlatform, _acdPlatform._matchMakerConfiguration, args.ApplicationEndpointSettings, _acdPlatform._logger);
                        _acdPlatform._matchMaker.BeginStartup(this.OnMatchMakerStartUpComplete, _acdPlatform._matchMaker);
                    }
                }
            }

            /// <summary>
            /// To complete the start up operation it is essential that the match maker be started correctly.
            /// </summary>
            /// <param name="ar"></param>
            private void OnMatchMakerStartUpComplete(IAsyncResult ar)
            {
                AcdAgentMatchMaker matchMaker = ar.AsyncState as AcdAgentMatchMaker;

                try
                {
                    matchMaker.EndStartup(ar);

                    _acdPlatform._platform.UnregisterForApplicationEndpointSettings(OnMatchMakerFound);
                    _acdPlatform.RegisterForPlatformAutoProvisioningEvents();

                    _acdPlatform.UpdateAcdPlatformState(AcdPlatformState.Started);
                    this.SetAsCompleted(null, false);
                }
                catch (RealTimeException ex)
                {
                    _acdPlatform._logger.Log("AcdPlatform could not start the match maker", ex);
                    _acdPlatform.BeginShutdown(_acdPlatform.OnPlatformShutdownComplete, null);
                    this.SetAsCompleted(ex, false);
                    return;
                }
                catch (InvalidOperationException ivoex)
                {
                    _acdPlatform._logger.Log("AcdPlatform could not start the match maker", ivoex);
                    _acdPlatform.BeginShutdown(_acdPlatform.OnPlatformShutdownComplete, null);
                    this.SetAsCompleted(new OperationFailureException("AcdPlatform could not start the match maker", ivoex), false);
                    return;            
                }
            }
        }

        /// <summary>
        /// Parses the configuration file
        /// </summary>
        /// <param name="configXMLDoc"></param>
        /// <returns>true if the configuration file could be parsed successfully, false otherwise</returns>
        internal bool ProcessConfigurationFile(string configXMLDoc)
        {
            try
            {
                //Parse the config xml doc into an Xdocument
                XDocument configDoc = XDocument.Parse(configXMLDoc);

                //Use Linq to XML to get the platform 
                _configuration = (from platform in configDoc.Descendants("platform")
                          select new AcdPlatformConfiguration
                          {
                              ApplicationUserAgent = platform.Element("applicationUserAgent").Value,
                              ApplicationUrn = platform.Element("applicationUrn").Value,
                              PortalConfigurations = (from portal in platform.Element("portals").Descendants("portal")
                                                      select new AcdPortalConfiguration
                                            {

                                                Uri = portal.Element("uri").Value,
                                                Token = portal.Element("token").Value,
                                                VoiceXmlEnabled = (portal.Element("voiceXmlEnabled") != null && portal.Element("voiceXmlEnabled").Value.Equals("true")) ? true : false,
                                                VoiceXmlPath = portal.Element("voiceXmlPath") != null ? portal.Element("voiceXmlPath").Value : null,
                                                WelcomeMessage = portal.Element("welcomeMessage").Value,
                                                ContextualWelcomeMessage = portal.Element("contextualWelcomeMessage").Value,
                                                ImBridgingMessage = portal.Element("imBridgingMessage").Value, 
                                                ImPleaseHoldMessage = portal.Element("imPleaseHoldMessage").Value,
                                                FinalMessage = portal.Element("finalMessage").Value,
                                                TimeOutNoAgentAvailableMessage = portal.Element("timeOutNoAgentAvailableMessage").Value,
                                                Skills = (from skill in portal.Element("portalSkills").Descendants("portalSkill").Attributes()
                                                          select (string)skill.Value).ToList<string>()
                                            }).ToList()
                          }).First();

                //Retrieve the matchmaker configuration
                 _matchMakerConfiguration = 
                    (from matchMaker in configDoc.Element("platform").Descendants("matchMaker")
                     select new AcdAgentMatchMakerConfiguration
                    {
                       MaxWaitTimeOut = int.Parse(matchMaker.Element("maxWaitTimeOut").Value),
                       Uri = matchMaker.Element("uri").Value,
                       AgentDashboardGuid = new Guid(matchMaker.Element("agentDashboardGuid").Value),
                       SupervisorDashboardGuid = new Guid(matchMaker.Element("supervisorDashboardGuid").Value),
                       FinalMessageToAgent = matchMaker.Element("agentPrompts").Element("finalMessageToAgent").Value,
                       OfferToAgentMainPrompt = matchMaker.Element("agentPrompts").Element("mainPrompt").Value,
                       OfferToAgentNoRecoPrompt = matchMaker.Element("agentPrompts").Element("noRecoPrompt").Value,
                       OfferToAgentSilencePrompt = matchMaker.Element("agentPrompts").Element("silencePrompt").Value,
                       MusicOnHoldFilePath = matchMaker.Element("musicOnHoldFilePath").Value,
                       AgentMatchMakingPrompt = matchMaker.Element("agentMatchMakingPrompt").Value,
                       FinalMessageToSupervisor = matchMaker.Element("supervisorPrompts").Element("finalMessageToSupervisor").Value,
                       SupervisorWelcomePrompt = matchMaker.Element("supervisorPrompts").Element("welcomePrompt").Value,
                       Skills = (from skill in matchMaker.Element("skills").Descendants("skill")
                                 select new Skill()
                                 {
                                     Name = skill.Attribute("name").Value,
                                     MainPrompt = skill.Element("mainPrompt").Value,
                                     NoRecoPrompt = skill.Element("noRecoPrompt").Value,
                                     SilencePrompt = skill.Element("silencePrompt").Value,
                                     RecognizedSkillPrompt = skill.Element("skillRecognizedMainPrompt").Value,
                                     Values = (from skillValue in skill.Descendants("skillValues").Elements()
                                               select skillValue.Value).ToList()
                                 }).ToList(),
                   }).First();

                 //add the supervisors. 
                 _matchMakerConfiguration.Supervisors =
                 (from supervisor in configDoc.Descendants("supervisors").Elements("supervisor")
                  select new Supervisor()
                  {
                      SignInAddress = supervisor.Element("signInAddress").Value,
                      PublicName = supervisor.Element("publicName").Value,
                      InstantMessageColor = supervisor.Element("instantMessageColor").Value,
                 }).ToList();
         
                //Add the agents.  This must be done separately from the previous line since we need the skills
                //from the matchMakerConfiguration object.
                _matchMakerConfiguration.Agents = 
                (from agent in configDoc.Descendants("agents").Elements("agent")
                  select new Agent(_logger)
                  {
                     PublicName = agent.Element("publicName").Value,
                     SignInAddress = agent.Element("signInAddress").Value,
                     SupervisorUri = agent.Element("supervisorUri").Value,
                     InstantMessageColor = agent.Element("instantMessageColor").Value,
                     Skills = (from agentSkill in agent.Descendants("agentSkills").Elements()
                                select new AgentSkill(Skill.FindSkill(agentSkill.Attribute("name").Value,
                                                                      _matchMakerConfiguration.Skills),
                                                                      agentSkill.Value)).ToList(),
                  }).ToList();
                
                //Assign agent to supervisor and vice versa
                _matchMakerConfiguration.Agents.ForEach(agent =>
                {
                    _matchMakerConfiguration.Supervisors.ForEach(sup =>
                    {
                        if (SipUriCompare.Equals(sup.SignInAddress, agent.SupervisorUri))
                        {
                            agent.Supervisor = sup;
                            sup.Agents.Add(agent);
                        }
                    });
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.Log("AcdPlatform failed parsing the configuration file",ex);
                return false;
            }        
        }

        /// <summary>
        /// OnPlatformShutdownComplete finishes shutting down the platform.
        /// </summary>
        internal void OnPlatformShutdownComplete(IAsyncResult result)
        {
            CollaborationPlatform platform = result.AsyncState as CollaborationPlatform;

            platform.EndShutdown(result);
        }

        #endregion

        #region Shutdown AsyncResult

        private class ShutdownAsyncResult : AsyncResultNoResult
        { 
            private AcdPlatform _acdPlatform;

            internal ShutdownAsyncResult(AsyncCallback userCallBack, object state, AcdPlatform platform):base(userCallBack, state)
            {
                _acdPlatform = platform;
            }

            internal void Process()
            {
                //Call each portal and ask them to drain and terminate.
                if (_acdPlatform._portals.Count != 0)
                {
                    _acdPlatform._numberOfPortals = _acdPlatform._portals.Count;

                    foreach (AcdPortal portal in _acdPlatform._portals)
                    {
                        portal.BeginShutdown(OnPortalShutdownComplete, portal);
                    }
                }
                else
                {
                    if (_acdPlatform._matchMaker != null)
                    {
                        _acdPlatform._matchMaker.BeginShutdown(OnMatchMakerShutdownComplete, null);
                    }
                    else
                    {

                        //Log all our events to the event log.
                        if (_acdPlatform._logger != null)
                        {
                            _acdPlatform._logger.ShutDown();
                        }

                        if (_acdPlatform.CollaborationPlatform != null)
                        {
                            _acdPlatform.CollaborationPlatform.BeginShutdown(OnCollabPlatformShutdownComplete, null);
                        }
                        else
                        {
                            _acdPlatform.UpdateAcdPlatformState(AcdPlatformState.Terminated);
                            _acdPlatform._listOfShutdownAsyncResults.ForEach(stdwnar => stdwnar.SetAsCompleted(null, false));


                            this.SetAsCompleted(null, false);
                        }
                    }
                }
            }

            void OnPortalShutdownComplete(IAsyncResult ar)
            {
                //given that the number of portals is fixed, we should have as many 
                //callback completions as portals.
                var portal = ar.AsyncState as AcdPortal;

                portal.EndShutdown(ar);

                if (0 == Interlocked.Decrement(ref _acdPlatform._numberOfPortals))
                {
                    if (_acdPlatform._matchMaker != null)
                    {
                        //shut down the match maker once the portals are terminated
                        _acdPlatform._matchMaker.BeginShutdown(OnMatchMakerShutdownComplete, null);
                    }
                }
            }

            void OnMatchMakerShutdownComplete(IAsyncResult ar)
            {
                _acdPlatform._matchMaker.EndShutdown(ar);

                //Log all our events to the event log.
                _acdPlatform._logger.ShutDown();
                _acdPlatform.CollaborationPlatform.BeginShutdown(OnCollabPlatformShutdownComplete, null);
            }

            void OnCollabPlatformShutdownComplete(IAsyncResult ar)
            {
                _acdPlatform.CollaborationPlatform.EndShutdown(ar);

                _acdPlatform.UpdateAcdPlatformState(AcdPlatformState.Terminated);   
                _acdPlatform._listOfShutdownAsyncResults.ForEach(stdwnar => stdwnar.SetAsCompleted(null, false));
         
           
                this.SetAsCompleted(null, false);
            }
        }
    }
    #endregion

    internal enum AcdPlatformState 
    { 
        Created, 
        Starting, 
        Started, 
        Terminating, 
        Terminated 
    };

    internal class AcdPlatformStateChangedEventArgs : EventArgs
    {
        private AcdPlatformState _previousState;
        private AcdPlatformState _newState;

        internal AcdPlatformStateChangedEventArgs(AcdPlatformState previousState, AcdPlatformState newState)
        {
            _previousState = previousState;
            _newState = newState;
        }

        internal AcdPlatformState PreviousState
        {
            get { return _previousState; }
        }

        internal AcdPlatformState NewState
        {
            get { return _newState; }
        }
    }

    internal class AcdPlatformAnonymousSubscriberUriChangedEventArgs : EventArgs
    {
        private string _anonymousSubscriberUri;

        internal AcdPlatformAnonymousSubscriberUriChangedEventArgs(string anonymousSubscriberUri)
        {
            SipUriParser parser;
            if(SipUriParser.TryParse(anonymousSubscriberUri, out parser))
            {
            _anonymousSubscriberUri = anonymousSubscriberUri;
            }
            else
            {
              throw new ArgumentException("AcdPlatformAnonymousSubscriberUriChangedEventArgs requires a valid Uri");
            }
        }

        internal string AnonymousSubscriberUri
        {
            get { return _anonymousSubscriberUri; }
        }
     
    }
}
        