
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using System.Collections.ObjectModel;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    public class AgentContextChannel
    {
        public AgentContextChannel(Conversation conversation, ParticipantEndpoint remoteEndpoint);

        public IAsyncResult BeginEstablish(Guid applicationId,
                ProductInfo productInfo,
                IEnumerable<Skill> skills,
                AsyncCallback userCallback, object state);
        public void EndEstablish(IAsyncResult result);

        public IAsyncResult BeginTerminate(AsyncCallback userCallback, object state);
        public void EndTerminate(IAsyncResult result);

        public ConversationContextChannelState State { get; }


        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;
        public event EventHandler<ConversationContextChannelStateChangedEventArgs> StateChanged;
    }

    public class SupervisorContextChannel
    {
        public SupervisorContextChannel(Conversation conversation, ParticipantEndpoint remoteEndpoint);

        public IAsyncResult BeginEstablish(Guid applicationId,
                    AsyncCallback userCallback, object state);
        public void EndEstablish(IAsyncResult result);

        public IAsyncResult BeginTerminate(AsyncCallback userCallback, object state);
        public void EndTerminate(IAsyncResult result);

        public ConversationContextChannelState State { get; }

        public event EventHandler<ConversationContextChannelStateChangedEventArgs> StateChanged;
        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;

        public IAsyncResult BeginUpdateAgents(IEnumerable<AgentInfo> agentsAddedOrModified,
            IEnumerable<AgentInfo> agentsRemoved,
            AsyncCallback callback, object state);
        public void EndUpdateAgents(IAsyncResult result);

    }

    public class MonitoringChannel
    {
        public Guid Id { get; }

        public Uri Uri { get; }

        public IAsyncResult BeginUpdateParticipants(IEnumerable<ParticipantInfo> participantsAddedOrModified,
            IEnumerable<ParticipantInfo> participantsRemoved,
            AsyncCallback callback, object state);
        public void EndUpdateParticipants(IAsyncResult result);

        public IAsyncResult BeginUpdateAgents(IEnumerable<AgentInfo> agentsAddedOrModified,
            IEnumerable<AgentInfo> agentsRemoved,
            AsyncCallback callback, object state);
        public void EndUpdateAgents(IAsyncResult result);


        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;
    }


    public enum AgentStatus : byte
    {
        Idle,
        Allocated
    }

    [Flags]
    public enum MediaTypes : byte
    {
        Chat,
        Audio,
        Video
    }

    public class AgentInfo
    {
        public AgentInfo(string uri);
        public string DisplayName { get; set; }
        public DateTime StatusChangeTime { get; set; }
        public AgentStatus Status { get; set; }
        public MediaTypes MediaTypes { get; set; }
    }

    public class ParticipantInfo
    {
        public ParticipantInfo(string uri);
        public string DisplayName { get; set; }
        public MediaTypes MediaTypes { get; set; }
    }

    public class ProductInfo
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }

    public class Skill
    {
        public string SkillName { get; set; }
        public Collection<string> SkillValues { get; set; }
    }

    public class AgentSkill
    {
        public string SkillName { get; set; }
        public string SkillValue { get; set; }
    }

    public enum ContextChannelRequestType
    {
        Hold,
        Retrieve,
        Escalate,
        Whisper,
        StartMonitoring,
        StopMonitoring
    }

    public class ContextChannelRequest
    {
        public ContextChannelRequestType RequestType;

        public void SendResponse(string responseCode);
    }

    public class ContextChannelRequestReceivedEventArgs
    {
        public ContextChannelRequestType RequestType;
        public ContextChannelRequest Request;
    }

    public class WhisperRequest : ContextChannelRequest
    {
        public string Uri { get; }
    }

    public class EscalateRequest : ContextChannelRequest
    {
        public Collection<AgentSkill> Skills { get; }
    }

    public class MonitoringRequest : ContextChannelRequest
    {
        public MonitoringChannel MonitoringChannel { get; }
    }

}
