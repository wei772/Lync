Title: AudioVideoCall - Basic Outbound call and basic AVCall handling.

Description:

The application places an AudioVideo call to the designated target, after initalizing platform and endpoints.
Once the call has been connected, the application subscribes to events to see audio video flow's transitions and states.
The application prints logging to the console, and then quits; shutting the platform down normally.

The sample uses two users capable of sending/receiving voice calls. The sample logs in as one of the users, called AVCall Sample User, 
and calls the other user that is logged on a client (such as, Microsoft Lync) capable of logging in to Microsoft Lync Server.
There is no media exchanged between the users when the call is established, but the console logging will provide indication of each of
the steps as they are executed. After the incoming call is picked up by the user logged on the client (Microsoft Lync), the sample terminates
the call and waits for a key press to terminate the sample. This way, the console stays up until the user terminates the sample and the user
can inspect all the logging to trace through the execution of the sample.

Features:

                - Basic Call placement.
                - Basic Audio Video call use.
                - AudioVideoFlow handling and control.

Prerequisites:
                •             Microsoft Lync Server.
                •             Two users capable of sending/receiving Voice calls.
                •             The credentials for those users.
                •             A client (such as, Microsoft Lync) capable of logging in to Microsoft Lync Server.


Running the sample:
•             You may either supply the user credentials in the accompanying app.config file, or you will be prompted for them when you run the sample.
•             Open the project in Visual Studio, and hit F5.
