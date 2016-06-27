Title
-----
InstantMessagingCall

Description
-----------
The application initializes the platform, and logs in on behalf of a user. The
sample then sends an instant message, with text of "Hello World", to another
user that is logged in to a client, such as Microsoft Lync. The sample echoes
instant messages sent by the user logged into Microsoft Lync. When the user logged
into Microsoft Lync responds with a message of text 'bye', or closes the instant
messaging window, the sample shuts down the platform and exits.

Features
--------
	• Call and Conversation use and cleanup.
	• Basic Call placement.
	• Instant Messaging message use, including replying to incoming messages.
	• InstantMessagingFlow handling and control.

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
	3. The sample will send a "Hello World" message to the called party.
	4. You may login as the called party in Microsoft Lync and send an instant
	   message to the user the sample is logged in as.
	5. You can terminate the sample by the called party to send a message with
	   text "bye" to the user that the sample is logged in as.
