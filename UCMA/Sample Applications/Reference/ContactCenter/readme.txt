Title: Contact center reference application

Description
************
The Contact Center application is an end-to-end client and server sample application that demonstrates advanced conferencing and contact center scenarios. 



Scenarios
**********
The Contact Center application demonstrates the following scenarios:

	The customer visits a website, configures their account, and browses products for sale.

	The customer has a question about a product and clicks a link to start a Microsoft Lync 2013 contextual web chat.

	The customer can escalate the web chat into a voice call with the click of a button, directing the service to start a call between the agent and the customer’s phone.

	The agent has a dashboard view of the customer’s contextual data associated with the product in question.

	The agent can escalate the call to any available product expert, and put the customer on hold.

	The call center supervisor can monitor all calls, and bargein as necessary.

The server components, which are included in the UCMA 4.0 Core SDK, run on the application server:

	The Contact Center is a console application that handles all call routing and data flow between users and call center representatives. The Contact Center has two automatic call distributors (ACDs) that handle routing calls to available agents. One ACD is for the Sales portal, and the other ACD is for the Helpdesk portal. Calls get routed to agents based on their skill sets and availability.

	The Microsoft Windows Communication Foundation (WCF) service runs the website, which sells mobile devices, and the web chat interface. Internet Information Services (IIS) is used for website deployment.

	The Contact Center Provisioner is an application that is used one time to set up IIS and run the WCF service.


Please note: The client components, which are included in the Lync SDK, include the following Microsoft Lync 2013 Conversation Window Extension applications: 

	The agent dashboard, which is used to view customer data that is provided by the web portal.

	The supervisor dashboard, which is used to monitor all conversations in progress.




Prerequisites
**************
	•	Windows 2k8 Server R2 (or higher).
        •	Configuration of WebServer (IIS 7.x or higher) and Application server roles.
	•       Contact center application running on a separate or same machine.
	•	Installation of visual studio 2012 or higher with visual web developer feature.
	•	Installation of Silverlight 4 Toolkit
	•	Installation of Blend SDK for Silverlight 4
	•	Installation of UCMA 4.0 sdk.
	•	Installation of Lync Client 2013 sdk for the Conversation Window Extension.
	•	Installation of required certificates to communicate with Microsoft Lync Server.
	•	SQL Server 2008 Express (or greater) edition.





Setting up the topology
************************
Set up the following trusted applications in your topology. (See Help documentation for more information.)

Trusted application		Recommended application ID
================================================================
Contact Center application	contactcenter
Web service application		webstore


Set up the following trusted application endpoints in your topology. (See Help documentation for more information.)

Endpoint			Recommended SIP address			Recommended display name	Application ID
======================================================================================================================== 
Contact Center application	sip:contactcenter@yourdomain.com	ContactCenter			contactcenter
Help desk portal		sip:Helpdesk@yourdomain.com		Helpdesk			contactcenter
Sales portal			sip:Sales@yourdomain.com		Sales				contactcenter
Web chat			sip:webuser@yourdomain.com		WebUser				webstore
 



Setting up files
*****************
•	Make sure all the pre-requisites are satisfied.
•	Copy the Samples folder to a new location.
•	Open visual studio solution and build the solution.




Title: Installing ContactCenterWcfService

Description:
This sample provides a web interface to the contact center application for the external customers to initiate communication with the contact center application.

Features:
This sample provides following features

	- Initiate IM call to the contact center (Click to chat)
	- Initiate Audio call to the contact center (Click to call)


Prerequisites:
	•	Windows 2008 Server R2.
        •	Configuration of WebServer (IIS 7.x or higher) and Application server roles.
	•       Contact center application running on a separate or same machine.
	•	Installation of visual studio 2012 or higher with visual web developer feature.
	•	Installation of UCMA 4.0 sdk.
	•	Installation of Silverlight 4.0 or higher SDK.
	•	Installation of Expression Blend SDK for Silverlight.
	•	Installation of Visual studio tools for silverlight. 
	•	Installation of required certificates to communicate with Microsoft Lync Server.
	•	Creation of contact objects in Lync topology for this application. (See above)
	•	SQL Server 2008 Express (or higher) edition.

Deploying the sample:

Setting up files
*****************
•	Make sure all the pre-requisites are satisfied.
•	Open visual studio solution as administrator and build the solution.
•	Make sure your Windows 2008 server (or higher) has "web server" role enabled.
•	Make sure "Application Development” role with Asp.NET is enabled.
•	Make sure you have c:\inetpub\wwwroot as the root directory for IIS and the current user has write access to this directory.
•	Build the contact center solution as administrator so that all necessary files are copied to appropriate folders.
•	Configure web.config file to point to the application id of the contact object created for this Wcf service.
•	Run the contact center .exe file located in "<contact center folder>\ContactCenterExe\bin\Debug"


Hosting WCF service in IIS
****************************
•	Copy the entire folder under c:\inetpub\wwwroot\contactcenter to a suitable machine where you are planning to host this WCF service.
•	Run ContactCenterWcfServiceProvisioner application on the machine where this WCF service will be hosted by providing appropriate user name and password.
•	Configure SqlExpress database to run under the same credentials provided in the previous step.
•	In the IIS Manager, stop the default web site and start the ContactCenter web site.
•	Open the ContactCenterWcfService.svc file in a web browser and make sure you see instructions to create test client.


Setting up the Conversation Window Extension
**********************************************
•	Copy ContactCenterExtension.xap and TestPage.html from the output folder of the ContactCenterExtension project to a location which is accessible by the client. It might be a folder on the hard disk or an IIS web site.
•	Update Registry.reg in the ContactCenterExtension project folder with the location of these files, and install the registry file on the client machine


Firewall issues
****************
•	Make sure to open up firewall ports for the test port given earlier (example: 80) for external access. 

Accessing the service
*********************
You should be able to access http://<machine_name> for the webpage.

Debugging the service
************************
Contact center web application service provides basic debugging help using event logs. It logs exceptions under ContactCenterWcfService application name. An administrator can use these event logs to debug any issues with the web service.
If the service page returns 500.21 (Handler “PageHandlerFactory-Integrated” has a bad module “ManagedPipelineHandler” in its module list) when running the application please run %windir%\Microsoft.NET\Framework64\<.Net version of the applications app pool>\aspnet_regiis.exe -i


Security disclaimer
********************
This is just a sample application. For production applications, WCF service should be secured using transport + message level security using certificates. See http://msdn.microsoft.com/en-us/library/ms735093.aspx for more details.



