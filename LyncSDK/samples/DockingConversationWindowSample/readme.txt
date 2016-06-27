Copyright (c) Microsoft Corporation.  All rights reserved.

Docking Conversation Window Sample

This application demonstrates the ability to dock a conversation window within another application.

Features
- Docks a conversation window into a WPF application.
- Demonstrates how to use a WindowsFormsHost to dock a Conversation Window.
- Shows how to handle the different events fired by a Conversation Window.
- Shows how to Flash the window when the conversation Window requires attention.

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Both Lync and the DockingConversationWindowSample must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2010 or above.
- Microsoft Lync SDK 2010 or above.

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2010 or above must be installed and running.

Running the sample
------------------
1. Copy the DockingConversationWindowSample folder to a user folder, outside of Program Files.
2. Open DockingConversationWindowSample.csproj file.
3. Go to MainWindow.xaml and change "Source" property of the StartInstantMessagingButton (_myStartIMButton) to point to a valid SIP URI.
4. Hit F5
5. Click on the StartInstantMessagingButton and see the conversation window docking in the space below.
