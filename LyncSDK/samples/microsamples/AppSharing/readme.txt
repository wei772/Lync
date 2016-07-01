Copyright (c) Microsoft Corporation.  All rights reserved.

Share Resources: Resource sharing sample

This application demonstrates the ability to share a desktop, monitor, or running process in a conversation window. The sample is supported
by the Lync client conversation window and cannot be run in a UI suppression scenario.

Sample location:

64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\ShareResources
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\ShareResources

Features
- Starts a conversation and presents a list of local shareable resources to be selected and shared in conversation.
- Control of shared resources can be granted and revoked to another user
- Local user can accept or decline an invitation to control a resource shared by another user
- Local user can release control of a resource that is shared by another user

Warnings:
- The sample runs in Visual Studio 2008 but the project file included in it is for Visual Studio 2010.
- Copy the ShareResources folder to a user folder, outside of Program Files.
- Both Lync and the ShareResources project must run with the same priviliges.

Prerequisites (for compiling and running in Visual Studio)
- .Net Framework 3.5 or above.
- Visual Studio 2008 or 2010
- Microsoft Lync 2013 SDK

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2013 must be installed and running.

Running the sample
------------------
1. Open ShareResources.csproj file.
2. Click F5
