/*=====================================================================
  File:      Proposal.cs

  Summary:   Model class for creating Proposals.

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
using Microsoft.Lync.Controls;

namespace ProposalTracker.Models
{
    /// <summary>
    /// Class that models a proposal
    /// </summary>
    public class Proposal
    {
        private ConversationContextualInfo _conversationContextualInfo;
        public String ProjectName { get; set; }
        public String Description { get; set; }
        public String ClosedDate { get; set; }
        public List<String> Team { get; set; }
        public String SponsorUri { get; set; }

        //Field to create and return a ContextualInfo of a proposal.
        //This is used in the MiniProposalProject to initiate a Contextual Conversation.
        public ConversationContextualInfo ContextualInfo { 
            get
            {
                //If _conversationContextualInfo is null, create a new one and return it.
                //If not return itself. 
                return _conversationContextualInfo ??
                       (_conversationContextualInfo = new ConversationContextualInfo{
                                                       //Description is taken from the proposal details
                                                       ApplicationData = Description, 
                                                       //This should be the same GUID you're using in the reg file.
                                                       ApplicationId = "{AFCFD912-E1B7-4CB4-92EE-174D5E7A35DD}",
                                                       //Project name is the current proposal's projectname.
                                                       Subject = ProjectName,
                                                       //This is a link to the main page. It is optional but good to have.
                                                       ContextualLink = "\\\\SharedFilesPath\\ProposalTracker\\ProposalTrackerTestPage.html"
                                                      }
                       );
            }
        
        }
    }
}
