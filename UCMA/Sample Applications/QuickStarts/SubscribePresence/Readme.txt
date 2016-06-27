Title
-----
SubscribePresence

Description
-----------
The application initializes the platform and endpoint and subscribes to the
presence of a target user. Throughout the process, it will listen for incoming
notifications from a user logged into Microsoft Lync and reflect the updated state
of that user in the console.

Features
--------
	• Presence Subscription
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
