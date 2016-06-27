Title
-----
ManualProvisioning

Description
-----------
In some situations, administrators may wish to deploy Unified Communications
Managed API (UCMA) 4.0-based applications that are trusted by Microsoft
Lync Server, yet avoid installing a Central Management Store
(CMS) replica on the server hosting the application. In these situations,
the application must make use of manual provisioning.

When compared with automatic provisioning, manual provisioning has some
limitations. Specifically, manually provisioned applications must be configured
with a Globally Routable User Agent URI (GRUU), trusted application endpoint
owner URI and port, all of which must be configured in the CMS when the trusted
application endpoint is configured. Additionally, it is typically necessary to
configure manually provisioned endpoints with the registrar FQDN and port. It is
also necessary to determine the certificate the application will use if TLS
transport is being employed. Additionally, changes to the trusted application
endpoint owner properties will not be updated from CMS into the application.

For help provisioning trusted applications and endpoints in Microsoft Lync Server, see the
UCMA 4.0 Core SDK help file by clicking on Start -> All Programs -> Microsoft
Unified Communications Managed API 4.0, SDK -> Core SDK Help and
navigating to Managing UCMA 4.0 Applications -> Activating a UCMA 4.0 Trusted Application. Steps from both the
"General Application Activation" and "Activating a Manually-Provisioned
Application" will need to be used.

This sample demonstrates how to create a CollaborationPlatform using the
ServerPlatformSettings constructor, establish a single ApplicationEndpoint, and
monitoring the state changes on the endpoint.

Features
--------
	• CollaborationPlatform creation from ServerPlatformSettings
	• ApplicationEndpoint establishment
	• Endpoint StateChanged notification handling

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
