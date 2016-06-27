Title: SpeechSynthesisConnector

Description:

The application initalizes the platform and calls out to the user specified, uses speech synthesis connector for a small period of time, stops, then tears down the speech synthesis connector and platform. The application prints logging to the console, and then quits; shutting the platform down normally.


Features:

	- Call and Conversation cleanup and use.
	- Basic Call placement.
	- Basic SpeechSynthesisConnector usage.

Prerequisites:
	•	Microsoft Lync Server.
	•	Microsoft Speech Platform Runtime.
	•	Microsoft Speech Platform Software Development Kit.
	•	Microsoft Speech Language Packs
	•	One user capable of receiving Voice calls.
	•	The credentials for the user, and a client capable of logging in to Microsoft Lync Server.
	•	A currently logged-in client on Microsoft Lync Server.

Running the sample:
•	Replace the credentials in the variables at the beginning of the code sample with the credentials and server of the users from your Microsoft Lync Server topology.
•	Substitute the address of the called user in the code snippet with the address of a valid, currently signed-in user capable of receiving audio calls.
•	Open the project in Visual Studio, and hit F5.
