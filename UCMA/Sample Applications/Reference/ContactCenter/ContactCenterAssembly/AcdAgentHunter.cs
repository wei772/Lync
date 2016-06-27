/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.Utilities;
using Microsoft.Rtc.Signaling;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Diagnostics;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Dialogs;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    internal class AcdAgentHunter
    {
        #region Private Members
        private AcdPortal _portal;
        private AcdAgentMatchMaker _matchMaker;
        private AcdLogger _logger;
        /// <summary>
        /// List Stores CommitAgentDialog Instance Ids
        /// </summary>
        private List<Guid> _dialogList = new List<Guid>();
        private Dictionary<Guid, TryCommitAgentAsyncResult> _dictionaryOfTryCommitAsyncResults = new Dictionary<Guid,TryCommitAgentAsyncResult>();
        private object _syncRoot = new object();
        private AcdAverageQueueTime _averageQueueTime;

        #endregion 

        #region Constructor
        internal AcdAgentHunter(AcdPortal portal, AcdAgentMatchMaker matchMaker, AcdLogger logger)
        {
            _portal = portal;
            _matchMaker = matchMaker;
            _logger = logger;
            _averageQueueTime = new AcdAverageQueueTime(portal);
        }
        
 

        #endregion

        #region Properties

        internal AcdAgentMatchMaker Matchmaker
        {
            get
            {
                return _matchMaker;
            }
        }

        internal AcdAverageQueueTime AverageQueueTime
        {
            get { return _averageQueueTime; }
        }

        internal AcdPortal Portal
        {
            get
            {
                return _portal;
            }
        }

        #endregion

        #region Internal Methods


        internal IAsyncResult BeginHuntForAgent(AcdCustomerSession session, List<AgentSkill> requestedSkills,AsyncCallback callback, object state)
        {
            HuntForAgentAsyncResult ar = new HuntForAgentAsyncResult(this, session, requestedSkills, callback, state);

            ThreadPool.QueueUserWorkItem((waitState)=>
                {
                    var tempAr = waitState as HuntForAgentAsyncResult;
                    tempAr.Process(true);
                }, ar);
            
            return ar;
        }

        internal AgentHuntResult EndHuntForAgent(IAsyncResult ar)
        {
            HuntForAgentAsyncResult result = ar as HuntForAgentAsyncResult;

            return result.EndInvoke();
        }

        /// <summary>
        /// Commit Agent Dialog Complete Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void AgentHuntCompleteHandler(object sender, DialogCompletedEventArgs e)
        {
            Dictionary<string, object> output = e.Output;
            TryCommitAgentAsyncResult ar;
            bool agentApproval = output.ContainsKey("QaAgentOfferApproval") ? Convert.ToBoolean(output["QaAgentOfferApproval"]) : false;
            InstantMessagingCall imCall = output.ContainsKey("Call") ? output["Call"] as InstantMessagingCall : null;

            _dictionaryOfTryCommitAsyncResults.TryGetValue((Guid)output["InstanceId"], out ar);
            _dictionaryOfTryCommitAsyncResults.Remove((Guid)output["InstanceId"]);

            if (ar == null)
                Debug.Assert(false, "failed to get TryCommitAgentAsyncResult");

            if (agentApproval)
                ar.SetAsCompleted(imCall, false);

            else
            {
                if (null != imCall)
                {
                    imCall.BeginTerminate(ter => { imCall.EndTerminate(ter); }, null);
                    ar.SetAsCompleted(null, false);
                }
                else
                    ar.SetAsCompleted(new OperationFailureException("AcdAgentHunter: DialogTerminated is getting hit"), false);
            }
        }

        /// <summary>
        /// Update collection og Dialog instance
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="ar"></param>
        private void AddDialogInstance(Guid instanceId, TryCommitAgentAsyncResult ar)
        {
            lock (_syncRoot)
            {
                _dialogList.Add(instanceId);
                _dictionaryOfTryCommitAsyncResults.Add(instanceId, ar);
            }
        }

        #endregion

        private class HuntForAgentAsyncResult : AsyncResult<AgentHuntResult>
        {
            private AcdAgentHunter _agentHunter;
            private AcdCustomerSession _session;
            private List<AgentSkill> _requestedSkills;
            private Agent _tentativeAgent;
            private InstantMessagingCall _imCall;
            private Conversation _huntingConversation;
            private DateTime _huntStartTime;

            #region constructor
            internal HuntForAgentAsyncResult(AcdAgentHunter hunter, AcdCustomerSession session, List<AgentSkill> requestedSkills, AsyncCallback callback, object state)
                : base(callback, state)
            {
                _agentHunter = hunter;
                _session = session;
                _requestedSkills = requestedSkills;

            }
            #endregion

            #region Properties

            internal AcdAgentHunter AgentHunter
            {
                get
                {
                    return _agentHunter;
                }
            }

            internal DateTime HuntStartTime
            {
                get
                {
                    return _huntStartTime;
                }
            }
            #endregion

            #region Internal Methods

            internal void Process(bool firstTime)
            {

                //Take the start time of the operation to measure its duration and compute the average queue time
                _huntStartTime = DateTime.UtcNow;

                //Refresh the average queue time timer to account for call rate. When incoming calls dry out, the timer
                //expires and the average queue time is reset to its initial value
                _agentHunter.AverageQueueTime.RefreshTimerOnMatchRequested();

                try
                {

                    _agentHunter.Matchmaker.BeginFindAgent(_session,
                                                           _session.ExclusionListOfAgents,
                                                           _requestedSkills,
                                                           this.FindAgentCompleted,
                                                           _agentHunter.Matchmaker);
                }
                catch (InvalidOperationException ivoex)
                {
                    this.SetAsCompleted(new OperationFailureException("AcdAgentHunter matchmaker is not in the correct state", ivoex), false);
                }
            }
            #endregion Internal Methods

            #region Private Methods

            private void TryCommitAgentCompleted(IAsyncResult result)
            {
                try
                {
                    _imCall = this.EndTryCommitAgent(result);
                    if (null != _imCall)
                    {
                        _huntingConversation = _imCall.Conversation;
                        bool success = true;

                        try
                        {
                            _session.Anchor.AuthorizeParticipant(_tentativeAgent.SignInAddress);

                            _tentativeAgent.AllocationStatus = AgentAllocationStatus.EscalatingTheAgent;
                            _tentativeAgent.AsyncResult = this as AsyncResult<AgentHuntResult>;

                            _huntingConversation.ConferenceSession.BeginJoin(_session.Anchor.ConferenceUri,
                                                                             null,
                                                                             this.JoinCompleted,
                                                                             null);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            success = false;
                            this.SetAsCompleted(new OperationFailureException("AcdAgentHunter failed to join the conference", ivoex), false);
                        }
                        finally
                        {
                            if (!success)
                            {
                                //First Deallocate/Disallow the agent
                                _tentativeAgent.Deallocate(_session);
                                _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                                _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);

                                try
                                {
                                    _imCall.Flow.BeginSendInstantMessage(
                                        _agentHunter.Matchmaker.Configuration.FinalMessageToAgent,
                                        sm =>
                                        {
                                            try
                                            {
                                                _imCall.Flow.EndSendInstantMessage(sm);
                                                _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
                                            }
                                            catch (RealTimeException rtex)
                                            {
                                                _agentHunter._logger.Log("AcdAgenHunter failed to end send the final message to Agent", rtex);
                                            }
                                        },
                                         null);
                                }
                                catch (InvalidOperationException ivoex)
                                {
                                    _agentHunter._logger.Log("AcdAgenHunter failed to end send the final message to Agent", ivoex);

                                }
                            }
                        }
                    }
                    else
                    {
                        _tentativeAgent.Deallocate(_session);
                        _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                        _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);
                        ThreadPool.QueueUserWorkItem((waitState) =>
                        {
                            var tempAr = waitState as HuntForAgentAsyncResult;
                            tempAr.Process(false);
                        }, this);
                    }
                }
                catch (Exception ex)
                {
                    this.SetAsCompleted(ex, false);
                    _tentativeAgent.Deallocate(_session);
                    _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                    _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);
                }
            }

            private void EscalateToConferenceCompleted(IAsyncResult result)
            {
                bool success = true;

                try
                {
                    _huntingConversation.EndEscalateToConference(result);
                    _session.Anchor.AuthorizeParticipant(_tentativeAgent.SupervisorUri);
                    TimeSpan matchDuration = DateTime.UtcNow.Subtract(this._huntStartTime);
                    _agentHunter.AverageQueueTime.ReEvaluate(matchDuration);

                    AgentHuntResult agentResult = new AgentHuntResult(_tentativeAgent, _huntingConversation.Id);
                    this.SetAsCompleted(agentResult, false);

                }
                catch (RealTimeException rtex)
                {
                    success = false;
                    this.SetAsCompleted(rtex, false);
                }
                finally
                {
                    if (!success)
                    {
                        _tentativeAgent.Deallocate(_session);
                        _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                        _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);
                        try
                        {
                            _imCall.Flow.BeginSendInstantMessage(
                                _agentHunter.Matchmaker.Configuration.FinalMessageToAgent,
                                sm =>
                                {
                                    try
                                    {
                                        _imCall.Flow.EndSendInstantMessage(sm);
                                        _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
                                    }
                                    catch (RealTimeException rtex)
                                    {
                                        _agentHunter._logger.Log("AcdAgenHunter failed to end send the final message to Agent", rtex);
                                        _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
                                    }
                                },
                                 null);
                        }
                        catch (InvalidOperationException ivoex)
                        {
                            _agentHunter._logger.Log("AcdAgenHunter failed to end send the final message to Agent", ivoex);
                            _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
                        }
                    }
                    else
                    {
                        _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
                    }
                }
            }

            private void JoinCompleted(IAsyncResult result)
            {
                bool success = true;

                try
                {
                    _huntingConversation.ConferenceSession.EndJoin(result);
                    try
                    {
                        _huntingConversation.BeginEscalateToConference(this.EscalateToConferenceCompleted, null /*state*/);
                    }
                    catch (InvalidOperationException ivoex)
                    {
                        success = false;
                        this.SetAsCompleted(new OperationFailureException("AcdAgentHunter failed to begin escalate the conference", ivoex), false);
                    }
                }
                catch (RealTimeException rtex)
                {
                    this.SetAsCompleted(rtex, false);
                    success = false;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            _tentativeAgent.Deallocate(_session);
                            _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                            _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);
                        }
                        catch (InvalidOperationException)
                        { }

                        try
                        {
                            _imCall.Flow.BeginSendInstantMessage(
                                _agentHunter.Matchmaker.Configuration.FinalMessageToAgent,
                                sm =>
                                {
                                    try
                                    {
                                        _imCall.Flow.EndSendInstantMessage(sm);
                                        _huntingConversation.BeginTerminate(ter => { _huntingConversation.EndTerminate(ter); }, null);
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
                }
            }

            private void FindAgentCompleted(IAsyncResult result)
            {
                AcdAgentMatchMaker matchMaker = result.AsyncState as AcdAgentMatchMaker;
                try
                {
                    if (_tentativeAgent != null
                        && _tentativeAgent.Owner == _session
                        && _tentativeAgent.IsAllocated)
                    {
                        _tentativeAgent.Deallocate(_session);
                        _agentHunter._logger.Log("AcdAgentHunter failed to deallocate the previous agent of the hunt");

                    }
                    _tentativeAgent = matchMaker.EndFindAgent(result);

                    _session.AddAgentToExclusionList(_tentativeAgent);

                    if (   _session.State != CustomerSessionState.Terminating
                        && _session.State != CustomerSessionState.Terminated)
                    {

                        this.BeginTryCommitAgent(_tentativeAgent,
                                                 this.TryCommitAgentCompleted,
                                                 null);

                    }
                    else
                    {
                        _tentativeAgent.Deallocate(_session);
                        _agentHunter.Matchmaker.HandleNewAvailableAgent(_tentativeAgent);
                        _session.Anchor.DisallowParticipant(_tentativeAgent.SignInAddress);
                        this.SetAsCompleted(new TimeoutException("AcdAgentHunter realized that the customer session ended"), false);
                        return;

                    }
                }
                catch (TimeoutException toex)
                {
                    // We are re-evaluating the average queue time by taking into account this time out.
                    _agentHunter.AverageQueueTime.ReEvaluate(new TimeSpan(0, 0, 0, _agentHunter.Matchmaker.Configuration.MaxWaitTimeOut));

                    this.SetAsCompleted(toex, false);

                }
                catch (OperationFailureException ofex)
                {
                    this.SetAsCompleted(ofex, false);
                }
            }



            private IAsyncResult BeginTryCommitAgent(Agent tentativeAgent, AsyncCallback userCallback, object state)
            {
                TryCommitAgentAsyncResult ar = new TryCommitAgentAsyncResult(this, tentativeAgent, userCallback, state);

                ThreadPool.QueueUserWorkItem((waitState) =>
                {
                    var tempAr = waitState as TryCommitAgentAsyncResult;
                    tempAr.Process();
                }, ar);

                return ar;
            }

            private InstantMessagingCall EndTryCommitAgent(IAsyncResult ar)
            {
                TryCommitAgentAsyncResult result = ar as TryCommitAgentAsyncResult;
                return result.EndInvoke();

            }

            internal string GetAgentSkillPrompt()
            {
                string agentSkillPrompt = String.Empty;

                _requestedSkills.ForEach(ask => { agentSkillPrompt += ask.Value + " "; });

                return agentSkillPrompt;
            }
            #endregion
        }

        private class TryCommitAgentAsyncResult : AsyncResult<InstantMessagingCall>
        {
            private HuntForAgentAsyncResult _huntForAgentAsyncResult;
            private Agent _agent;
            private AcdAgentMatchMaker _matchMaker;


            internal TryCommitAgentAsyncResult(HuntForAgentAsyncResult huntForAgentAsyncResult, Agent agent, AsyncCallback callBack, object state)
                : base(callBack, state)
            {
                _huntForAgentAsyncResult = huntForAgentAsyncResult;
                _agent = agent;
            }

            internal void Process()
            {
                _matchMaker = _huntForAgentAsyncResult.AgentHunter.Matchmaker;

                AcdPortal portal = _huntForAgentAsyncResult.AgentHunter.Portal;

                _agent.AllocationStatus = AgentAllocationStatus.CommittingTheAgent;

                Dictionary<string, object> inputParameters = new Dictionary<string, object>();

                string agentSkillPrompt = _huntForAgentAsyncResult.GetAgentSkillPrompt();

                inputParameters.Add("AgentUri", _agent.SignInAddress);
                inputParameters.Add("QaAgentOfferMainPrompt", String.Format(_matchMaker.Configuration.OfferToAgentMainPrompt, _agent.PublicName, agentSkillPrompt));
                inputParameters.Add("QaAgentOfferNoRecognitionPrompt", _matchMaker.Configuration.OfferToAgentNoRecoPrompt);
                inputParameters.Add("QaAgentOfferSilencePrompt", _matchMaker.Configuration.OfferToAgentNoRecoPrompt);
                inputParameters.Add("Owner", this);
                inputParameters.Add("ApplicationEndPoint", portal.Endpoint);
                try
                {
                    //Start commit agent dialog
                    CommitAgentDialog commitAgent = new CommitAgentDialog();
                    Guid instanceId = commitAgent.InstanceId;
                    commitAgent.InitializeParameters(inputParameters);
                    commitAgent.Completed += _huntForAgentAsyncResult.AgentHunter.AgentHuntCompleteHandler;
                    commitAgent.Run();
                    _huntForAgentAsyncResult.AgentHunter.AddDialogInstance(instanceId, this);
                }
                catch (Exception ex)
                {
                    this.SetAsCompleted(ex, false);
                }
            }
        }
    }



    internal struct AgentHuntResult
    {
        private string _conversationId;
        private Agent _agent;

        internal AgentHuntResult(Agent agent, string ConversationId)
        {
            _conversationId = ConversationId;
            _agent = agent;
        }

        internal string ConversationId
        {
            get { return _conversationId; }
        }

        internal Agent Agent
        {
            get { return _agent; }

        }
    
    }



}
