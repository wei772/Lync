Title: AudioVideoFlow - Basic ApplyChanges Demo.

Description:

The application places an AudioVideo call to the designated target, after initalizing platform and endpoints.
Once the call has been connected, the application waits for the audio video flow to go active, start to play a file, then applies a configuration change to the flow to change the sampling rate.

After that, the platform is shut down normally.

Features:

	- Basic AudioVideo call placement.
	- Platform/Endpoint initialization.
	- AudioVideoFlow handling and control.
	- ApplySettings/Shifting the configuration of a currently active AudioVideoFlow.

Prerequisites:
	•	Microsoft Lync Server.
	•	Two users capable of sending/receiving Voice calls.
	•	The credentials for those users, and a client capable of logging in to Microsoft Lync Server.
	•	A currently logged-in client on Microsoft Lync Server.

Running the sample:
•	Replace the credentials in the variables at the beginning of the code sample with the credentials and server of the users from your Microsoft Lync Server topology.
•	Substitute the address of the called user in the code snippet with the address of a valid, currently signed-in user capable of receiving audio calls.
•	Open the project in Visual Studio, and hit F5.

