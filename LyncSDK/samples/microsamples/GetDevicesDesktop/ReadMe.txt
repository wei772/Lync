Introduction
=============
This application demonstrates the ability to get all audio and video devices and set a particular one as the Active device. 


Sample location
================
64Bit Operating System: %PROGRAMFILES(X86)%\Microsoft Lync\SDK\Samples\GetDevicesDesktop
32Bit Operating System: %PROGRAMFILES%\Microsoft Lync\SDK\Samples\GetDevicesDesktop


Features
========
- List all audio devices connected to the computer.
- List all video devices connected to the computer.
- See which audio and video device is currently the active device. 
- Choose a device and set is as active device. 
- Play an audio file on the communication device. 


Warnings
========
- Project file included in it is for Visual Studio 2010.
- Copy the ShareResources folder to a user folder, outside of Program Files.
- Both Lync and the the sample must run with the same priviliges.


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
1. Open "Devices Scenarios.csproj".
2. Confirm Microsoft.Lync.Model reference is pointing to the location where your Microsoft.Lync.Model.dll file is.
3. Hit F5
4. Once the sample app starts up, choose whether you want view audio or video devices by clicking the right radio button.
5. Choose one device and click "Set As Active Device" button. 
6. For playing the Audio file, pass the full path to the *.wav file. (hint: several wav files are installed with Lync)
