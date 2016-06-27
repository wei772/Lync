Title
-----
CallModalityAddition

Description
-----------
The application initalizes the platform to log in a pair of users and initiates
an instant messaging call between them. After the instant messaging call is 
established, the application re-uses the existing conversation to send an 
AudioVideo call between the two users as part of the same dialog. After the
AudioVideo call is established, it prints logging to the console, and then quits; 
shutting the platform down normally. Please note, no audio/video media or instant
messages are actually exchanged between the users.

Features
--------
	• Modality addition/Call and conversation
	• Basic Call placement
	• Conversation re-use

Prerequisites
-------------
	• Microsoft Lync Server
	• Two users capable of sending/receiving Voice calls
	• The credentials for those users

Running the sample
------------------
    1. You may either supply the user credentials in the accompanying app.config
	   file, or you will be prompted for them when you run the sample.
    2. Open the project in Visual Studio, and hit F5.
