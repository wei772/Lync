Title
-----
SubscribePresenceView

Description
-----------
The application initializes the platform and endpoint and subscribes to a target
user. The user application uses two RemotePresenceView objects; each configured
with different SubscriptionModes -- Persistent and Polling.

Once the subscription is complete, the application will listen for incoming
notifications from user logged into Microsoft Lync and display the notifications
in the console.

Features
--------
	• Creation and configuration of RemotePresenceView objects
	• Presence Subscription using RemotePresenceViews
	• Monitoring of presence-related notifications

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
	3. Change the presence of the user logged in on Microsoft Lync, and see the
	   presence change in the console.
