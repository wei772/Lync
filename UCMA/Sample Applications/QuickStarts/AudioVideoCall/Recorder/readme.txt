Title: Recorder

Description:

The application initalizes the platform and calls out to the user specified, records what the user says, then tears down the platform. The application prints logging to the console, and then quits; shutting the platform down normally.


Features:

	- Call and Conversation cleanup and use.
	- Basic Call placement.
	- Basic Recorder and WmaFileSink usage.
	- Voice activity detection.

Prerequisites:
	•	Microsoft Lync Server.
	•	One user capable of sending/receiving Voice calls.
	•	The credentials for the user, and a client capable of logging in to Microsoft Lync Server.
	•	A currently logged-in client on Microsoft Lync Server.

Running the sample:
•	Replace the credentials in the variables at the beginning of the code sample with the credentials and server of the users from your Microsoft Lync Server topology.
•	Substitute the address of the called user in the code snippet with the address of a valid, currently signed-in user capable of receiving audio calls.
•	Open the project in Visual Studio, and hit F5.
