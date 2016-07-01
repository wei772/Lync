Copyright (c) Microsoft Corporation.  All rights reserved.

Lock Video sample

This application demonstrates the ability to Lock and unLock a participant's video in the video gallery

Sample location:
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\LockVideo
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\LockVideo

Features
- Select a participant in a conversation and Lock their video in the video gallery

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Copy the LockVideo folder to a user folder, outside of Program Files.
- Both Lync and the LockVideo project must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2008 or 2010
- Microsoft Lync 2013 SDK

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2013 must be installed and running.

Running the sample
------------------
1. Open LockVideo.csproj file.
2. Click F5
3. Start a 3 person video conference or a meet now with an additional participant
4. Click on Get Conversations
5. Select a conversation in the listbox and click Get Participants
6. Select a participant from the participant listbox and click Get Participant (do not select self)
7. Click on Lock and UnLock to Lock and unLock the video in the video gallery.  
