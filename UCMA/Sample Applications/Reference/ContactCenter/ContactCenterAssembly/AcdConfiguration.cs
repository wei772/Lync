/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    internal class AcdPlatformConfiguration
    {
        internal string ApplicationUserAgent { get; set; }
        internal string ApplicationUrn { get; set; }
        internal List<AcdPortalConfiguration> PortalConfigurations { get; set; }
    }

    internal class AcdPortalConfiguration
    {
        internal string Uri { get; set; }
        internal string Token { get; set; }
        internal string VoiceXmlPath { get; set; }
        internal bool VoiceXmlEnabled { get; set; }
        internal List<string> Skills { get; set; }
        internal string WelcomeMessage { get; set; }
        internal string ContextualWelcomeMessage { get; set; }
        internal string ImBridgingMessage { get; set; }
        internal string ImPleaseHoldMessage { get; set; }
        internal string FinalMessage { get; set; }
        internal string TimeOutNoAgentAvailableMessage { get; set; }
    }

    internal class AcdAgentMatchMakerConfiguration
    {
        internal string Uri { get; set; }
        internal string MusicOnHoldFilePath { get; set; }
        internal Guid AgentDashboardGuid { get; set; }
        internal Guid SupervisorDashboardGuid { get; set; }
        internal int MaxWaitTimeOut { get; set; }
        internal List<Skill> Skills { get; set; }
        internal List<Agent> Agents { get; set; }
        internal List<Supervisor> Supervisors { get; set; }
        internal string FinalMessageToAgent { get; set; }
        internal string OfferToAgentMainPrompt { get; set; }
        internal string OfferToAgentNoRecoPrompt { get; set; }
        internal string OfferToAgentSilencePrompt { get; set; }
        internal string AgentMatchMakingPrompt { get; set; }
        internal string FinalMessageToSupervisor { get; set; }
        internal string SupervisorWelcomePrompt { get; set; }
    }
}
