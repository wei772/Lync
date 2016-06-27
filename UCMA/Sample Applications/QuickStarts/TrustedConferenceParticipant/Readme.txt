Title
-----
TrustedConferenceParticipant

Description
-----------
The application establishes an application endpoint and impersonates a user who
creates and joins an ad-hoc conference. The application endpoint then joins the
conference as a trusted participant. The trusted participant then monitors the
roster of the MCU for the conference, when a new participant is detected, a new
AV call is created. This audio/videocall is then configured to route DTMF from
the new participant to the application and audio from the application to the
participant. The application can also send an invitation to a specified user to
show how the impersonated and trusted joins are surfaced. For help provioning
trusted applications and endpoints in Microsoft Lync Server, see the UCMA 4.0 Core SDK help file
by clicking on Start -> All Programs -> Microsoft Unified Communications Managed
API 4.0, SDK -> Core SDK Help and navigating to the "Activating a UCMA 4.0
Trusted Application". Steps from both the "General Application Activation" and
"Activating an Manually-Provisioned Application" will need to be used.

Features
--------
	• Ad hoc conference creation
	• Conversation impersonation
	• Joining a conference as a trusted participant (endpoint will not be
	 visible on the roster)
	• Audio/video call route configuration

Prerequisites
-------------
	• Microsoft Lync Server
	• Provisioned trusted application endpoint
	• A currently-logged-in client on Microsoft Lync Server


Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
	3. When the conference is established the sample will prompt for URIs to
	   send invitations to the conference. (Pressing ENTER when prompted will
	   skip this step.) This URI should be for a user that is currently signed
	   into Microsoft Lync so they can join the conference to observe how the
	   other participants are presented and see that the entered DTMF or the
	   response audio is not heard by the other participants of the conference.
