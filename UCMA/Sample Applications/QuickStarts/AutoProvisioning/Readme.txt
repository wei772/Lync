Title
-----
AutoProvisioning

Description
-----------
Based on the ApplicationID specificed, the sample initalizes the collaboration
platform and the application endpoint corresponding to the provisioned 
application. The quickstart will establish as many ApplicationEndpoints as have
been provisioned on the particular trusted application within the Microsoft Lync Server deployment.

For help on provisioning trusted applications and endpoints in Microsoft Lync Server, see the 
UCMA 4.0 Core SDK help file by clicking on Start -> All Programs -> Microsoft 
Unified Communications Managed API 4.0, SDK -> Core SDK Help and navigating
to Managing UCMA 4.0 Applications ->Activating a UCMA 4.0 Trusted Application. Steps from both the
"General Application Activation" and "Activating a Manually-Provisioned
Application" will need to be used.

Throughout the lifetime of the process, the sample application will listen for 
StateChanged and ProvisioningDataChanged notifications on the application endpoint
and reflect the same in the console. Developers can modify the provisioning 
information relevant to ApplicationEndpoints used in the quickstart (such as 
updating contact object properties such as displayName) to see how the 
ProvisioningData changes are surfaced.

Features
--------
	• ApplicationEndpointSettings creation from RegisterForApplicationEndpointSettings
	• ApplicationEndpoint establishment
	• Endpoint StateChanged notification handling
	• ProvisioningDataChanged notification handling

Prerequisites
-------------
	• Microsoft Lync Server
	• Provisioned trusted application endpoint

Running the sample
------------------
	1. You may either supply the provisioned Application ID to be used by the 
	   sample in the accompanying app.config file, or you will be prompted for 
	   this value when you run the sample.
	2. Open the project in Visual Studio, and hit F5.
