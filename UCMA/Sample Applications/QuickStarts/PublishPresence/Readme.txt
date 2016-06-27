Title
-----
PublishPresence

Description
-----------
The application initializes the platform and a user endpoint and subscribes to
the sample user's presence (self-presence). The sample then publishes the user
state, machine state, note, and contact card. The note is published using raw
XML, whereas all other categories are published using strongly-typed objects.
The sample then deletes the presence categories that it published, terminates
the endpoint, and shuts down the platform, exiting normally.

You may log in to the same user as the sample using a client (such as
Microsoft Lync), to see the categories being published as well.

Features
--------
	• Presence Publication using the grammar and strongly-typed categories

Prerequisites
-------------
	• Microsoft Lync Server
	• One user, enabled to use the Microsoft Lync Server
	• The credentials for that user

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
	3. Change the presence of the user logged in on Microsoft Lync, and see the
	   presence change in the console.
