Title
-----
PublishAlwaysOnline

Description
-----------
Based on the ApplicationID specified, the sample initializes the platform and
any trusted application endpoint(s) corresponding to the provisioned
application. Upon establishment, the endpoint(s) publishes static "always
online" presence. The sample will establish as many ApplicationEndpoints as have
been provisioned on the particular trusted application within the Microsoft Lync
Server deployment.

For help provisioning trusted applications and endpoints in Microsoft Lync Server, see the UCMA
4.0 Core SDK help file by clicking on Start -> All Programs -> Microsoft Unified
Communications Managed API 4.0, SDK -> Core SDK Help and navigating to
the "Activating a UCMA 4.0 Trusted Application". Steps from both the "General
Application Activation" and "Activating an Auto-Provisioned Application" will
need to be used.

Throughout the lifetime of the process, the sample application will listen for
StateChanged on the application endpoint and reflect the endpoint state changes
in the console.

Features
--------
	• Custom presence publishing
	• ApplicationEndpoint establishment
	• ApplicationEndpointSettings creation by registering a delegate with
	  RegisterForApplicationEndpointSettings
	• Endpoint state change notification handling

Prerequisites
-------------
	• Microsoft Lync Server
	• Provisioned trusted application endpoint

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
