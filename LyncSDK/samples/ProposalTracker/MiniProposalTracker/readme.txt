Copyright (c) Microsoft Corporation.  All rights reserved.

Mini Proposal Tracker sample extensibility window application

This application is used in conjunction with the ProposalTracker application to demonstrate the 
use of contextual conversations and the Microsoft Lync Extensibility Window feature.

Lync controls support the capability of sending contextual information within a conversation.
This information can include links to an application that runs in a frame within the existing
Lync ConversationWindow.  Participants in the conversation can exchange additional data via this
application using the Contextual Conversations API features.


Features
- When called from the contextual conversation, this project gives the users the option to change 
  different chart layouts as seen in the extensibility window.
- The changes made by one client are visible for the client immediately, via the exchange of context data.

Warnings:
- This application is not meant to be run independently.  It is intended for use in the Lync Extensibility Window, 
  when the conversation is launched from the Proposal Details page of the ProposalTracker sample application.

Prerequisites (for compiling and running in Visual Studio)
- Silverlight 4.0 or above.
- Visual Studio 2010 or above.
- Microsoft Lync Client SDK 2010 or above.

Prerequisites (for running installed sample on client machines)
- Microsoft Lync must be installed and running.
- Both ProposalTracker and MiniProposalTracker should be compiled and installed in a
  commonly accesible location.

Running the sample
------------------
1. Open MiniProposalTracker.csproj file.
2. Build the project (F6).
3. Copy the the bin folder to a shared location that is accessible to the people with whom 
   you are planning to have a contextual conversation.
4. Open the MiniProposalTracker.reg file and make the following changes

   a. change the values of InternalURL, ExternalURL and InstallLink variables to point to the shared folder you created above.
   b. change the value of the ServerName using the following rules for Lync 2010:
	If you're sharing your files from a network share server, the ServerName should be the name of the machine you have
        shared the files on. If your server name is MySharedServer, your line would look like 
	
	[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\MySharedServer]
	"file"=dword:00000002
   
   	If you're sharing your files from a web server, you have to add the server to the trusted sites list. To do that change
   	the ServerName to the address of the website domain name. If your website is www.mysite.com, change it to 
	
	[HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\mysite.com\www]
	"http"=dword:00000002

      For Lync 2013, do the corresponding changes in the reg file.	
	
5. Run the MiniProposalTracker.reg file to register this extensibility application with Lync.
5. Close the project, then open the ProposalTracker application.  Build and run the ProposalTracker application (F5).
6. Select one of the proposals and click "More". A child window should open, showing proposal details, and several Lync controls.
7. Start a conversation with one of the contacts on the CustomContactList on the right side of the Proposal Details window.
8. The conversation will be launched, and the remote participant should be given instructions for
   how to install the extensibility window, as well as how to run the the ProposalTracker application.
   There will also be a link for launching the extensibility window when installation is complete.
9. Once the extensibility window has been registered by both parties, it will open automatically in the future.
10. Use the extensibility window to exchange data.  This feature is demonstrated by the chart picking feature
   of the MiniProposalTracker.  Changes made by one participant should be visible to the other participant.

