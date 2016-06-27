Title
-----
BasicConferencing

Description
-----------
The application initializes a platform and two endpoints, caller and callee, 
each on behalf of a different user. The caller endpoint schedules an IM 
conference using the conference scheduling APIs, and then both the caller and
the callee join this scheduled conference. This sample also expects a third user
to be logged into a client (Microsoft Lync) who will be extended a conference
invite to participate in. This way, you can get a visual representation of the
IM Conference and the roster. The end result is that you have three users in an 
IM conference with each other.

The caller endpoint sends an IM on the conference, which all 3 endpoints get. The
callee endpoint always echoes all messages sent by caller endpoint. All messages
sent by the caller endpoint and callee endpoint are always logged to console.
The third endpoint, logged onto Microsoft Lync, can terminate the Conference by 
sending a "bye" message, which in turn shuts down the platform normally.


Features
--------
	• Conference scheduling and initialization
	• Conference joining to a scheduled conference
	• Conference invitation
	• Instant Messaging call conference use
	• InstantMessagingFlow handling and control

Prerequisites
-------------
	•	Microsoft Lync Server
	•	Three users capable of sending/receiving Voice calls
	•	The credentials for these users
	•	The third user logged onto Microsoft Lync

Running the sample
------------------
     1. You may either supply the user credentials in the accompanying app.config
	    file or you will be prompted for them when you run the sample.
	 2. Log-in as the third user in Microsoft Lync.
     3. Open the project in Visual Studio, and hit F5.
	 4. Terminate the sample by sending a text message of "bye" from the user 
	    logged into Microsoft Lync.
