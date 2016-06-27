Title
-----
ConferenceManagement

Description
-----------
The application initializes a server platform, four UserEndpoints and one
ApplicationEndpoint. The four UserEndpoints act as the conference organizer and
participants. The ApplicationEndpoint impersonates a phone user in the
conference simulating someone joining from a PSTN gateway. The organizer
endpoint schedules an audio/video conference using the conference scheduling
APIs, with the following conference scheduling options:

	• Conference access level Invited; only participants on the invited list can
	  be admitted
	• Participant A is in the invited list of the conference
	• Participants that are not in the invited list should land in the lobby
	  when joining the conference
	• Automatic leadership assignment (automatic promotion to leader) is enabled
	  for users of the same company
	• Participants joining the conference as a gateway participant bypass the
	  lobby

The sample proceeds in this order:

	1. Participant A, invited to the conference, joins the conference
	   successfully and is automatically promoted to leader.
	2. Participants B and C, not invited to the conference upon schedule, join
	   the conference and land in the conference lobby.
	3. Participant A is notified of the presence of Participants B and C in the
	   lobby.
	4. The ApplicationEndpoint, impersonating a phone user, joins the conference
	   successfully as a gateway participant.
	5. The ApplicationEndpoint is notified of the presence of Participant A in
	   the conference and the presence of Participants B and C in the conference
	   lobby.
	6. Organizer joins the conference successfully and receives notification
	   that Participants B and C are in the lobby and participants A and phone
	   user are in the conference.
	7. Organizer admits participant B from the lobby and denies access to
	   participant C.
	8. Organizer modifies the conference access level to SameEnterprise (opens
	   the conference) and participant C joins the conference successfully.

For help provisioning trusted applications and endpoints in Microsoft Lync Server, see the
UCMA 4.0 Core SDK help file by clicking on Start -> All Programs -> Microsoft
Unified Communications Managed API 4.0, SDK -> Core SDK Help and
navigating to Managing UCMA 4.0 Applications ->Activating a UCMA 4.0 Trusted Application. Steps from both the
"General Application Activation" and "Activating a Manually-Provisioned
Application" will need to be used.

Features
--------
	• Conference scheduling and initialization
	• Conference joining using the default join mode
	• Conference joining as a gateway participant
	• Conference lobby operation (Admission and denial of lobby participants)
	• ConferenceSession notifications
	• AudioVideoMcuSession notifications
	• Automatic promotion to leader for participants in the same company where
	  the conference is scheduled
	• Lobby bypass for gateway participants
	• Conversation impersonation

Prerequisites
-------------
	• Microsoft Lync Server
	• Provisioned trusted application endpoint
	• Two users, enabled to use the Microsoft Lync Server
	• The credentials for those users, and a client capable of logging into Microsoft Lync Server
	• A currently-logged-in client on Microsoft Lync

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.