Copyright (c) Microsoft Corporation.  All rights reserved.

Persistent Chat: Chat window add-in sample

This application demonstrates the ability to filter outgoing room messages using an add-in.

Sample location:
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\PersistentChat_FilterMessageAddIn
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\PersistentChat_FilterMessageAddIn

Features
- Catches outgoing messages before they are sent to a chat room and then cancels, reformats, or passes unchanged messages to the room.

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Copy the PersistentChat_FilterMessageAddIn folder to a user folder, outside of Program Files.
- Both Lync and the PersistentChat_FilterMessageAddIn project must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2008 or 2010
- Microsoft Lync 15 Technical Preview SDK

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 15 Technical preview must be installed and running.

Running the sample
------------------
1. Open PersistentChat_FilterMessageAddIn.csproj file.
2. Hit F5
