Title: FastHelp reference application

Description
************
The FastHelp application is an end-to-end client and server sample application that response to user with
 both IM and AV call. 



Scenarios
**********
The HelpDesk application demonstrates the following scenarios:

  The customer call the contact or user of the sample with IM, after several input the third part agent will
   make call to customer to provide more information.

The server components, which are included in the UCMA 4.0 Core SDK, run on the application server:

  The HelpDesk is a console application that handles all call routing and data flow between users and call 
  center representatives. The HelpDesk  has an automatic call distributors (ACDs) that handle routing calls
   to specified agents. 


Prerequisites
**************
  • Windows 2k8 Server R2 (or higher).
        • Configuration of WebServer (IIS 7.x or higher) and Application server roles.
  • HelpDesktop application running on a separate or same machine.
  • Installation of visual studio 2012 or higher with visual web developer feature.
  • Installation of Silverlight 4 Toolkit
  • Installation of Blend SDK for Silverlight 4
  • Installation of UCMA 4.0 sdk.
  • Installation of Lync Client 2013 sdk for the Conversation Window Extension.
  • Installation of required certificates to communicate with Microsoft Lync Server.


Setting up the topology
************************
Set up the following trusted applications in your topology. (See Help documentation for more information.)


Set up the following trusted application endpoints in your topology. (See Help documentation for more 
 information.)

Endpoint                      Recommended SIP address             Recommended display name  Application ID
======================================================================================================================== 
HelpDesk application          sip:HelpDesk@yourdomain.com         HelpDesk                  HelpDesk
Help desk portal              sip:HelpDesk@yourdomain.com         Helpdesk                  HelpDesk

or update with user information from user node section from config file.


Setting up files
*****************
• Make sure all the pre-requisites are satisfied.
• Copy the Samples folder to a new location.
• Open visual studio solution and build the solution.






To deploy the application on new machine do the following :

Make sure that the new machine has UCMA 4.0 SDK installed. The bot works with Microsoft Lync Server 2013.


The deployment consists of 3 steps, explained further below:
1) Deploy the Lync CWE application on IIS - the CWE (Lync Conversation Window Extension) is a Silverlight
    application that runs in the extension window of the Lync client.
2) Deploy the Rest Service on IIS 
3) Provision and run the bot as a Windows Service or a Console Application. You can leverage Visual Studio 
    to do this.


The first two steps are to prepare the two IIS web sites:

a) Open IIS manager from Start -> Admininstrative Tools

b) In the LHS tree node.expand the root node. Expand Sites node.

c) Right click Sites node -> Create new website
	   Create a website for CWE Silverlight app. Select a new path/folder on the harddrive.(empty folder)
 	   Make sure you assign a unique port number.
d) Right click Sites node -> Create new website
	   Create a website for RestService. Select a new path/folder and assign a unique port number. 


After the websites are created, update the following configuration parameters to point to their URLs:
   - RestServiceUrl in Constants.cs - point to the REST service web site
   - InternalURL and ExternalURL in FastHelp_CWE.reg - point to the web site hosting the CWE launch page.
   - ServerName in TrustedSites in FastHelp_CWE.reg - the host name of the server running these two web sites
   - CWERegistryFilePath in the respective app.config for the Console Application or the Windows Service - 
      point to the location where the FastHelp_CWE.reg file can be accessed from.
   - Image file URLs in IVRMenu.xml - point to the web site hosting the CWE web site.

Once the configuration steps are done, you can build the solution. After building, copy the build output to
the root directories of the web sites:

    .\FastHelpRestService to the root of the REST service web site,
    .\CWE\FastHelp.Web to the root of the web site hosting the CWE.


After the web sites are deployed, the next step is to provision, configure and run the bot, hosted in a Windows 
service or in a console application:

   - As a user that is a member of the RTCAdministrator security group, create a user account or a trusted 
      application in your Lync Server deployment that the bot will be using (for instructions how to do that, 
      please consult the MSDN Library or Channel 9).
   - Update the .config file of the host application with the login details for the account:
      - UseUserEndPoint = true/false - to log in as a user or as an application correspondingly
      - PoolFQDN = the pool FQDN of the user or application
      - For a user account, set up the login name/password/domain appropriately. 
      - For a trusted application, set up the corresponding parameters or turn on auto-provisioning.

Now, run the console application or the service, and try calling the bot, or sending it an IM message.

Troubleshooting:
 - If the bot does not respond to IM or calls, verify that its account settings are correct and that it was 
    able to sign in.
 - If the conversation window extension doesn't appear, make sure that the registry file has been installed
    on the caller's machine, and that the URLs in it are correct.
 - If the CWE loads but shows a JavaScript error message, check that the trusted web site parameter in the 
    registry file is correct.
 - If there CWE tiles don't display images, check the URLs in the REST service configuration file (IVRMenu.xml).
    The service only reads the file at startup, so make sure to restart it after making changes to it.