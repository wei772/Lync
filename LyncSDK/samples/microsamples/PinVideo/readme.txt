Copyright (c) Microsoft Corporation.  All rights reserved.

Pin Video sample

This application demonstrates the ability to pin and unpin a participant's video in the video gallery

Sample location:
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\PinVideo
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\PinVideo

Features
- Select a participant in a conversation and pin their video in the video gallery

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Copy the PinVideo folder to a user folder, outside of Program Files.
- Both Lync and the PinVideo project must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2008 or 2010
- Microsoft Lync 2013 SDK

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2013 must be installed and running.

Running the sample
------------------
1. Open PinVideo.csproj file.
2. Click F5
3. Start a 3 person video conference or a meet now with an additional participant
4. Click on Get Conversations
5. Select a conversation in the listbox and click Get Participants
6. Select a participant from the participant listbox and click Get Participant (do not select self)
7. Click on Pin and Unpin to pin and unpin the video in the video gallery.  
