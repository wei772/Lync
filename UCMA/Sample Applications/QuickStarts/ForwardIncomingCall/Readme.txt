Title
-----
ForwardIncomingCall

Description
-----------
The application initializes the platform and logs in on behalf of a user. The
sample then waits for an incoming audio/video call, forwards the incoming
audio/video call on to the designated target. After the forwarding, the sample
then terminates the call, conversation, and platform. The sample will then exit.

Features
--------
	• Call and Conversation use and cleanup
	• Call forwarding
	• AudioVideoFlow handling and control

Prerequisites
-------------
	• Microsoft Lync Server
	• Three users, enabled to use the Microsoft Lync Server
	• The credentials for those users, and a client capable of logging into Microsoft Lync Server
	• A currently-logged-in client on Microsoft Lync Server

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
	3. Make a voice call from the user logged into Microsoft Lync to the URI of
	   the user whose credentials the sample is using.
