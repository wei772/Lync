Copyright (c) Microsoft Corporation.  All rights reserved.

Proposal Tracker sample application

The ProposalTracker application demonstrates the use of Microsoft Lync controls in 
a Silverlight application.  The application is a demo, representing a tool used by a fictitious 
company called Fabrikam Inc. It tracks a list of proposals and sales people in the company.

Features
- Demonstrates how to use the following Lync controls in a Silverlight application.
	- MyStatusArea Control 
	- PresenceIndicator Control 
	- ContactCard Control 
	- ContactList Control 
	- CustomContactList Control 
	- ContactSearchInputBox Control 
	- ContactSearchResultList Control 
- Demonstrates how to use ContextualConversation between two Conversation Windows with the 
  companion application "MiniProposalTracker" which runs in a Lync Extensibility Window.


Prerequisites (for compiling and running in Visual Studio)
- Silverlight 4.0 or above.
- Visual Studio 2010 or above.
- Microsoft Lync Client SDK 2010 or above. 


Prerequisites (for running installed sample on client machines)
- Microsoft Lync must be installed and running.


Running the sample
------------------
1. Open ProposalTracker.csproj file.
2. Open the SalesTeam.cs file and change the sip uri’s to point to valid users in your organization.
3. Run the project (F5) and you should see the different Lync controls in use.
4. Select a proposal from the list of proposals in the upper left corner, click More...
5. View the proposal details page, with more examples of Lync controls.
6. The Lync controls on the proposal details page have contextual conversation data embedded in them.

To demonstrate the features of contextual conversations, and the use of the Lync Extensibility Window,
please refer to the MiniProposalTracker folder located within this sample.
