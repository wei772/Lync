Copyright (c) Microsoft Corporation.  All rights reserved.

Docking Conversation Window Sample

This application demonstrates the ability to start a conversation, pick a local resource to share, and manage the control of the shared resource.


Features
- Get all contacts in user contact list.
- Starts a new conversation.
- Gets all locally shareable resources.
- Shares one of the locally shareable resources in the new conversation.
- Grants control of the shared resource to another participant
- Revokes control of the shared resource
- Accepts or declines a request to control the shared resource
- Requests control of a resource shared by another user
- Releases control of a resource shared by another user
- Shows a remotely shared application resource in an ApplicationSharingView
- Re-sizes sample application window in response to resizing shared application

Warnings:
- The sample runs in Visual Studio 2010 but the project file included in it is for Visual Studio 2012.
- Copy the ShareResources folder to a user folder, outside of Program Files.
- Both Lync and the ShareResources must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2010- or 2012
- Microsoft Lync 2013 SDK + cumulative update for the Lync 2013 SDK: July 2013

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2013 cumulative update - May 2013 must be installed.

Running the sample
------------------
1. Open ShareResources.csproj file.
2. Go to ShareResources.cs and edit the ShareResources_form_Load method and change the _LyncClient.BeginSignIn() method arguments to provide valid sign in credential strings.
3. Hit F5
4. Choose one or more contacts from the contact list and click the Start Conversation button.
5. Choose a resource from the resource list and click the Share Resources button.
6. Choose a conversation participant from the contact list and click the Grant button to grant control of the shared resource.
7. Choose the Stop Sharing button to remove the shared resource from the conversation window content stage.
8. Choose the End button to end the conversation.
