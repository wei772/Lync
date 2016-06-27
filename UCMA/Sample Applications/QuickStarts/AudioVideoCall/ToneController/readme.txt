Title: ToneController Demo.

Description:

The application places an AudioVideo call to the designated target, after initalizing platform and endpoints.
Once the call has been connected, the application waits for the audio video flow to go active, after that it attaches a ToneController and subscribe a handler to receive tones and fax tones.

For each tone received it will reply sending the same tone back to the remote endpoint, except zero and fax tone which will shutdown the platform and close the demo.

Features:

	- Basic AudioVideo call placement.
	- Platform/Endpoint initialization.
	- AudioVideoFlow handling and control.
	- Attaching an AudioVideoFlow to a ToneController.
	- ToneController handling
	- Receiving tones from a ToneController
	- Receiving fax tones from a ToneController
	- Sending tones

Prerequisites:
	•	Microsoft Lync Server.
	•	Two users capable of sending/receiving Voice calls.
	•	The credentials for those users, and a client capable of logging in to Microsoft Lync Server.
	•	A currently logged-in client on Microsoft Lync Server.

Running the sample:
•	Replace the credentials in the variables at the beginning of the code sample with the credentials and server of the users from your Microsoft Lync Server topology.
•	Substitute the address of the called user in the code snippet with the address of a valid, currently signed-in user capable of receiving audio calls.
•	Open the project in Visual Studio, and hit F5.