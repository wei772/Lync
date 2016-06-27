Title
-----
BasicCallHandling

Description
-----------
The application initalizes the platform to start listening on behalf of a user, 
accepts the first incoming instant messaging call, and then terminates the call. 
This is followed by tearing down the conversation, and the platform. The 
application prints logging to the console, and then quits, shutting the platform
down normally. Please note that the sample does not send a response to the 
instant message that it receives.

Features
--------
	• Basic Call placement
	• Handling of an incoming instant messaging call
	• Call and Conversation cleanup and use
	• Call termination on an established call

Prerequisites
-------------
	•	Microsoft Lync Server
	•	Two users capable of sending/receiving Voice calls
	•	The credentials for those users and a client (such as Microsoft Lync) capable of logging in to Microsoft Lync Server

Running the sample
------------------
    1. You may either supply the user credentials in the accompanying app.config
	   file, or you will be prompted for them when you run the sample.
    2. Open the project in Visual Studio, and hit F5.
	3. Log in to a client (Microsoft Lync) using the second user's credentials and 
	   send an instant message to the user above that the sample is logged in as.
