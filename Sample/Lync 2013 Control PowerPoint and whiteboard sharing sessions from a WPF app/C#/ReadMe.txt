Introduction
=============
This application demonstrates the ability to start and accept Whiteboard and PowerPoint sharing using ContentShaingModality.


Sample location
================
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\ContentModalitySample
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\ContentModalitySample


Features
========
- Create a conversation with a remote party.
- Share whiteboard with another user.
- Share PowerPoint presentation with another user over Lync.
- Accept an incoming sharing session. 


Warnings
========
- Project file included in it is for Visual Studio 2010.
- Copy the ShareResources folder to a user folder, outside of Program Files.
- Both Lync and the the sample must run with the same priviliges.
- To reduce UI related code, sample doesn't enable and disable buttons. Please follow the sequence and restart the sample
  after each conversation. 


Prerequisites (for compiling and running in Visual Studio)
===========================================================
- .Net Framework 4.0 or above.
- Visual Studio 2010
- Microsoft Lync 15 Technical Preview SDK


Prerequisites (for running installed sample on client machines)
================================================================
- Microsoft Lync 15 Technical preview must be installed and running.


Running the sample
==================
1. Open "ContentModalitySample.csproj".
2. Confirm Microsoft.Lync.Model reference is pointing to the location where your Microsoft.Lync.Model.dll file is.
3. Hit F5
4. Once the sample app starts up, enter sip URI of a user who you want to contact and press Create Conversation button.
5. Accept the toast on remote user's side. Ensure Lync Converation Windows are open before clicking next button.
6. To start Whiteboard, enter title and press Share Whiteboard.
7. To start PowerPoint, enter title and full file path then press Share PowerPoint.
8. Please ensure you enter unique title for each time.
9. To Accept a call, wait for the toast to show up and then click Accept Toast button.

