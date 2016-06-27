Title
-----
ConferenceEscalation

Description
-----------
The application initializes a platform and two endpoints, each on behalf of a
different user. The endpoints establish a peer-to-peer instant messaging call
between themselves. One of the endpoints escalates the instant messaging call
into a conference. After escalation, the conversation is used for basic command
and control, including user privilege elevation, conference locking, and
ejection from conferences. Once the last of the command and control activities
complete, the application quits; shutting the platform down normally.

Features
--------
	• Ad-Hoc conference initialization
	• Conference command and control through conference services
	• AudioVideoCall conference use

Prerequisites
-------------
	• Microsoft Lync Server
	• Two users, enabled to use the Microsoft Lync Server
	• The credentials for those users

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
