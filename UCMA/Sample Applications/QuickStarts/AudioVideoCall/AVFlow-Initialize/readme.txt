
Title: AudioVideoFlow - Initialize.

Description:

The application places an AudioVideo call to the designated target, after initalizing platform and endpoints.
Once the call has been connected, the application uses initialize to change the settings of the currently active audio video flow, before the flow goes active. A moment later, the flow goes active, and the changes take effect. (In this case, the UCMA endpoint is set to allow audio channel direction only Inactive). The application prints the step-by-step actions it performs to the console, and then quits; shutting the platform down normally.

Features:

	- Changing the settings on an audio video call/flow at runtime to support/remove support for codecs/extra data types.
	- Basic AudioVideo call placement.
	- AudioVideoFlow handling and control.

Prerequisites:
	•	Microsoft Lync Server.
	•	Two users capable of sending/receiving Voice calls.
	•	The credentials for those users, and a client capable of logging in to Microsoft Lync Server.
	•	A currently logged-in client on Microsoft Lync Server.

Running the sample:
•	Replace the credentials in the variables at the beginning of the code sample with the credentials and server of the users from your Microsoft Lync Server topology.
•	Substitute the address of the called user in the code snippet with the address of a valid, currently signed-in user capable of receiving audio calls.
•	Open the project in Visual Studio, and hit F5.