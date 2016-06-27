/*=====================================================================
  File:      SupervisorDashboardChannel.cs

  Summary:   The model for supervisor dashboard view. 

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    /// <summary>
    /// The model for the supervisor dashboard view.
    /// </summary>
    public class SupervisorDashboardChannel
    {
        #region Private Fields

        private readonly Dictionary<string, agentType> _agents;
        private SupervisorMonitoringChannel _monitoringChannel;
        private readonly RequestProcessor _requestProcessor;

        #endregion

        #region Public Properties

        public Conversation Conversation { get; private set; }
        public string ApplicationId { get; private set; }

        public Collection<agentType> Agents
        {
            get 
            {
                lock (_agents)
                {
                    return new Collection<agentType>(_agents.Values.ToArray());
                }
            }
        }

        #endregion

        #region Public Events

        public event EventHandler<AgentsChangedEventArgs> AgentsChanged;

        #endregion

        #region Public Constructors

        public SupervisorDashboardChannel(string guid)
        {
            Conversation = (Conversation)LyncClient.GetHostingConversation();
            ApplicationId = guid;
            _requestProcessor = new RequestProcessor(null);
            _agents = new Dictionary<string, agentType>();

            Conversation.ContextDataReceived += ConversationContextDataReceived;
        }

        #endregion

        #region Public Methods

        public IAsyncResult BeginStartMonitoringSession(Uri uri,
            AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.StartMonitoring);

            request.startmonitoringsession = new startmonitoringsessionType
            {
                uri = uri.ToString(),
                sessionId = Guid.NewGuid().ToString()
            };


            SupervisorMonitoringChannel monitoringChannel = new SupervisorMonitoringChannel(this, request.startmonitoringsession.sessionId);
            _monitoringChannel = monitoringChannel;

            ProcessRequestAsyncResult requestAsyncResult =
                new StartMonitoringProcessRequestAsyncResult(monitoringChannel, request, _requestProcessor,
                userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public SupervisorMonitoringChannel EndStartMonitoringSession(IAsyncResult result)
        {
            StartMonitoringProcessRequestAsyncResult asyncResult = result as StartMonitoringProcessRequestAsyncResult;

            if (asyncResult != null)
            {
                asyncResult.EndInvoke();
            }
            else
            {
                throw new Exception("Invalid async result");
            }

            return asyncResult.SupervisorMonitoringChannel;
        }

        public IAsyncResult BeginStopMonitoringSession(
            AsyncCallback userCallback, object state)
        {
            SupervisorMonitoringChannel monitoringChannel = _monitoringChannel;

            if (monitoringChannel == null)
            {
                AsyncResultNoResult result = new AsyncResultNoResult(userCallback, state);
                result.SetAsCompleted(null, true);
                return result;
            }
            _monitoringChannel = null;

            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.StopMonitoring);

            request.terminatemonitoringsession = new terminatemonitoringsessionType();
            request.terminatemonitoringsession.sessionId = monitoringChannel.SessionId;

            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, Conversation, ApplicationId,
                                              userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public void EndStopMonitoringSession(IAsyncResult result)
        {
            AsyncResultNoResult asyncResult = result as AsyncResultNoResult;

            if (asyncResult != null)
            {
                asyncResult.EndInvoke();
            }
            else
            {
                throw new Exception("Invalid async result");
            }

        }

        #endregion

        #region Private Methods

        void ConversationContextDataReceived(object sender, ContextEventArgs e)
        {
            if (e.ContextDataType.Equals(WireHelpers.Response, StringComparison.OrdinalIgnoreCase))
            {
                ProcessResponse(WireHelpers.ParseResponse(e.ContextData));
            }
            else if (e.ContextDataType.Equals(WireHelpers.Notification, StringComparison.OrdinalIgnoreCase))
            {
                ProcessNotification(WireHelpers.ParseNotification(e.ContextData));
            }
        }

        private void ProcessResponse(responseType response)
        {
            SupervisorMonitoringChannel monitoringChannel = _monitoringChannel;

            if (!string.IsNullOrEmpty(response.sessionId) &&
                monitoringChannel != null &&
                response.sessionId.Equals(monitoringChannel.SessionId))
            {
                monitoringChannel.ProcessResponse(response);
            }
            else
            {
                _requestProcessor.ProcessResponse(response);
            }
        }

        private void ProcessNotification(notificationType notification)
        {
            SupervisorMonitoringChannel monitoringChannel = _monitoringChannel;

            if (!string.IsNullOrEmpty(notification.sessionId) &&
                monitoringChannel != null &&
                notification.sessionId.Equals(monitoringChannel.SessionId))
            {
                monitoringChannel.ProcessNotification(notification);
            }
            else
            {
                if (notification.agentinfos.Length > 0 || notification.agentsremoved.Length > 0)
                {
                    lock (_agents)
                    {
                        foreach (agentType notificationAgentinfo in notification.agentinfos)
                        {
                            _agents[notificationAgentinfo.uri] = notificationAgentinfo;
                        }

                        foreach (string uri in notification.agentsremoved)
                        {
                            _agents.Remove(uri);
                        }

                        AgentsChangedEventArgs e = new AgentsChangedEventArgs(notification.agentinfos, notification.agentsremoved);

                        EventHandler<AgentsChangedEventArgs> agentsChangedHandler = AgentsChanged;

                        if (agentsChangedHandler != null)
                        {
                            agentsChangedHandler(this, e);
                        }

                    }
                }
            }
        }

        #endregion

    }
}
