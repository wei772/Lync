Title
-----
ConversationContextChannel

Description
-----------
The application establishes an application endpoint who sends an invitation to a specified user and creates
a conversation Contextchannel for the remote user to display a webpage. For help provisioning
trusted applications and endpoints in Microsoft Lync Server, see the UCMA 4.0 Core SDK help file
by clicking on Start -> All Programs -> Microsoft Unified Communications Managed
API 4.0, SDK -> Core SDK Help and navigating to the "Activating a UCMA 4.0
Trusted Application". Steps from both the "General Application Activation" and
"Activating a Manually-Provisioned Application" will need to be used.

Features
--------
	• Conversation ContextChannel establishment
	• Sending context data to the remote user on the Conversation ContextChannel
	• Receiving context data to the remote user on the Conversation ContextChannel

Prerequisites
-------------
	• Microsoft Lync Server
	• Microsoft Lync SDK installed on the machine where the user is signed on in Lync
	• Autoprovisioned application with one provisioned application endpoint. Note that this feature can also be used with a UserEndpoint.
	• A currently-logged-in client on Microsoft Lync Server
	• For more information on this topic please refer to help documentation topics below
          1.UCMA 4.0 Core Overview -> Contextual Communication
          2.UCMA 4.0 Core Details -> Conversation Context Channel
    • Also see Lync SDK help topic ‘Walkthrough: Perform Install Registration for an Extensibility Application’


Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	   Please ensure that TargetUrl is contained within the trusted sites for the default browser
	   explorer on the machine where you are deploying the sample.
	2. Log in to a client (Microsoft Lync) using the second user's credentials.
	3. Open the project in Visual Studio, and hit F5.
