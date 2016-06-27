Copyright (c) Microsoft Corporation.  All rights reserved.

AudioVideo Conversation Sample 	

Uses the conversation and conversation.audiovideo namespaces from the Lync Model API to implement a conversation window.


Features
- Implements a fully functional audio-video conversation window.
- Shows how to register and handle conversation manager and audio-video conversation events.
- Uses the most common features of Conversation, AvModality, AudioChannel and VideoChannel.


Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2010 or above.
- Microsoft Lync Client SDK 2010 or above.

Running the sample
------------------
1. Copy the AudioVideoConversation folder to a folder outside of Program Files.
	- The sample should run in the same privilege level of Lync. 
	- Compiling it from Program Files prevents Lync from being able to draw video in the conversation window of this sample.
2. Open AudioVideoConversation.csproj.
3. Have Lync running or (preferably) set UISuppressionMode 
	- See the registry files in the root folder of this sample.
	- video will only work With UISuppressionMode.
	- More information is available in MainWindow.cs	
4. Hit F5 in Visual Studio.


How to use the sample application
---------------------------------
1. Type a SIP URI or Phone number in the input box and hit the "Create a conversation" button.
2. Use the conversation window button to make actions on the Conversation, AvModality, Audio and Video channels.
   The application also creates conversation windows for incoming calls.
	
