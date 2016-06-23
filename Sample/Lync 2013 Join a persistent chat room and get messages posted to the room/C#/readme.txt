Copyright (c) Microsoft Corporation.  All rights reserved.

Persistent chat: Get room messages

This application demonstrates the ability get all of the messages that have been posted to a room.

Sample location:
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\PersistentChat_GetMessages
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\PersistentChat_GetMessages
Features
- Get messages posted to a persistent chat room.

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Copy the PersistentChat_GetMessages folder to a user folder, outside of Program Files.
- Both Lync and the PersistentChat_GetMessages project must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2008 or 2010
- Microsoft Lync 15 Technical Preview SDK

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 15 Technical preview must be installed and running.

Running the sample
------------------
1. Open PersistentChat_GetMessages.csproj file.
2. Hit F5
3. Select a chat room from the Followed Room list
4. Press the Retrieve Additional Messages button to get the next set of older messages.
5. Repeat until the oldest messages are displayed.
