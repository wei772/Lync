Title
-----
DeclineIncomingCall

Description
-----------
The sample initializes the platform, and logs in on behalf of a user, rejects
the first incoming instant messaging call, then tears down the call,
conversation, and platform. The application then quits.

The sample expects a user logged into a client (such as Microsoft Lync) to send an
instant message to the user that the sample is logged in as.

Features
--------
	• Call and Conversation cleanup and use
	• Handling of incoming instant messages
	• InstantMessagingFlow handling and control

Prerequisites
-------------
	• Microsoft Lync Server
	• Two users, enabled to use the Microsoft Lync Server
	• The credentials for those users, and a client capable of logging into Microsoft Lync Server
	• A currently-logged-in client on Microsoft Lync Server

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
	3. Send an instant message to the user whose credentials the sample is
	   using.
