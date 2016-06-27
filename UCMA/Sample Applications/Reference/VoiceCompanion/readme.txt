				Voice Companion - A Sample UCMA Application
Voice Companion is one of the UCMA core API sample applications. The application serves PSTN phone users who would like to leverage unified communication capabilities like conferencing, presence and contact lists.

Main scenario
==============
1.	A PSTN phone user dials the application phone number.
2.	The application recognizes the phone number and prompts the user to enter his specific pin number.
3.	If successfully authenticated, the user is presented with several voice services.

Available Voice Services
========================
1. Contact list service
Using this service, the user can be connected to users from his/her contact list via speech recognition. Before connecting the user to the contact, Voice Companion checks the contact presence status. If not available, the user is prompted with the option of setting a callback when the contact becomes available.
2. International number dial up service
The PSTN phone user can dial an international phone number through her corporate network and thus potentially leveraging cheaper rates for business calls.
3. Conferencing service
The PSTN phone user can setup an ad-hoc conference and invite users to this conference either by selecting them from her contact list or entering a phone number. The PSTN user can do so without interrupting the conference (the PSTN user is isolated from the conference while being prompted).

Setting up the application
===========================
An application with ID "urn:application:voicecompanion" needs to be configured in the topology. Please check UCMA provisioning documentation to learn how to provision a UCMA application. 
Power Shell Commands
The power shell commands relevant for setting up the application are:
•	New-CsTrustedApplication
This helps to create a new application in the topology. You need to assign a listening port for the application using this command.
•	New-CsTrustedApplicationPool
This helps to create a pool for this application. This can help to run many instances of the application as the demand increases for the application.
•	New-CsTrustedApplicationComputer
This helps to create a computer in the pool created above.
•	new-cstrustedapplicationendpoint
This helps to create the application endpoint for the application. Note that the same instance of the application endpoint can be run from different computers. However, any number of endpoints could be assigned to this application to allow different numbers to be used for contacting the application.

Firewall Configuration
----------------------
The application needs to listen for incoming connections from the Microsoft Lync Server. This requires the port assigned to the application using New-CsTrustedApplication power shell command. The port specified in this command should be opened on the application computer by creating an inbound rule in the firewall configuration.

Reverse Number Lookup
----------------------
This sample program uses an xml file that stores the <callerid,sipuri> mappings. Initially, this contains a sample entry. The reason for this design is to make user experience simpler by not having to enter the telephone number of the user to map. To simplify administration of this file, the sample uses subscription notification as hint for adding the telephone numbers assigned to the user who added the application in his/her contact list. Users can also use IM call with the application to add/remove/list the telephone numbers. The application will update the file periodically (every hour) and when the application is shutdown. If the application needs to be run on many computers, this file should obviously be stored in a database so that any application can access it. This can be easily done by adding necessary code to the sample.

Logging
--------
The application supports a simple logging mechanism. Please see Program.cs file to see how it is used. 

User Provisioning
-----------------
In order to use the application, users must know the PIN to authenticate themselves. Normally, a web site is used for setting up the PIN. Users can also use the Microsoft Lync client to update their PIN. An administrator can use power shell to update the pin as well. For example, the following command shows an sample command.
Set-CSClientPin -identity sip:user1@contoso.com -Pin 23567
It is possible to add the ability for user to set the pin using IM call with the application. The application can use UserEndpoint to update the PIN. It is very important for the application to authenticate the user before updating the PIN. 

Program Design
==============
This application is written with the goal of making the code very readable and flexible. It uses a simple implementation of utility classes to make it possible to manage asynchronous operations without having to write similar looking try-catch blocks all over the code. This section will describe the basic infrastructure before explaining the architecture.

Component Base
---------------
Every component in the application derives from this base class that provides mechanism to start and stop the component. The base class provides API to start and stop the component. The derived classes need to implement StartupCore and ShutdownCore methods. The CompleteStartup and CompleteShutdown methods are provided by default for the class to indicate the corresponding operations are complete. It is the responsibility of the derived classes to call these methods when the startup or shut down operations are complete. Every component can be given a unique name for identification purpose in log files.

AsyncResult
------------
There are a couple of classes that provide asynchronous implementation but these are primarily used by the component base. Derived classes need not use them. Instead, it is much easier to use AsyncTask class which is described below.

AsyncTask
----------
This represents an asynchronous task. For example, establishing an endpoint or subscribing to presence, or joining a conference are all asynchronous tasks. The actual work is performed by a method with signature given by AsyncTaskMethod which is similar to AsyncCallback class in .Net framework. This method takes the owner task and a state. When the method completes the operation, it should complete the owner task. 
A task can be broken down further into smaller steps. This makes it easier to organize the program by focusing on macro tasks and letting each task manage smaller steps. For example, establishing an endpoint involves calling BeginEstablish and when the callback is invoked, caling EndEstablish. Execution of any step needs to handle exceptions properly. To make this simpler to deal with, AsyncTask class provides DoOneStep and DoFinalStep methods. Each of them takes a delegate without parameter that will perform the step. DoOneStep will NOT complete the task after calling the method. DoFinalStep WILL complete the task after calling the method. Thus, an asynchronous task can use several DoOneStep methods followed by one final DoFinalStep method. Note that is is simple to add if-then-else syntax to AsyncTask implementation but was not needed for sample implementation.

AsyncTaskSequence
-----------------
Typically, a program needs to perform several asynchronous tasks to accomplish a scenario. These tasks can be executed serially for some scenarios or in parallel for others scenarios. For example, an endpoint has to be established first before joining a conference and hence calls for sequential execution. When the application is shutdown, it can terminate all endpoints in parallel or terminate all customer sessions in parallel since they are independent tasks. The framework provides AsyncTaskSequenceSerial and AsyncTaskSequenceParallel classes to accomplish these sequences.

For further understanding of these concepts, please see the sample application.

The use of AsyncTask and AsyncTaskSequence classes makes the code much simpler to follow. It should only take a few hours to get learn and become comfortable with these concepts. These concepts provide a lot of flexibility to easily rearrange the way different scenarios are handled.

Architecture
============
The application uses auto provisioning to boot strap itself based on topology settings. The application platform is created first that encapsulated the collaboration platform. Once it is created, an application front end for each Application Endpoint in the topology is created. The application front end encapsulates an ApplicationEndpoint in UCMA. It is responsible for handling incoming calls.

AppFrontEnd
------------
When the application front end is started, it will start the music provider component that can play music files for users and the callback manager that is responsible for maintaining the callback requests setup by the customers. Finally, the application front end will establish the application endpoint.

Customer Session
----------------
Each incoming audio call or a callback session is handled by the customer session. The incoming call is used to determine the caller id (phone number) so that the application can determine the sip uri of the user. This is done by using reverse number lookup from a dictionary that is populated by loading the rnl.xml file. If the mapping cannot be determined, the call is rejected. Else, the user is asked to authenticate by entering PIN. If the PIN validates the user, the user is given options.

Service Hub
-----------
For speech/Dtmf recognition of the customer while in a conference, it is necessary to create a pipe only with the customer in the conference. This is done by creating a service channel (An audio call for the purpose of intercepting speech/dtmf commands from the user). The service hub is a trusted conversation created from the application endpoint. The application joins an ad-hoc created conference created on the fly. In addition, it creates a primary call (First class participation in the conference is needed by the AV MCU to carry out some trusted operations such as MuteAll or Remove user from default mix) followed by a service channel call. The service channel is wired with the customer.

Customer Endpoint
-----------------
The application is written to optimize the use of UserEndpoint to serve the customer. Only one endpoint is used so that users can use other endpoints (out of 8 allowed endpoints per user in Microsoft Lync Server) for their own use. The customer session uses the endpoint to publish online presence, followed by “InACall” presence. In addition, the user endpoint is used to create a conversation for joining the conference. This makes is possible to issue commands such as “conference invitations” to other contacts using the identity of the customer. 

Customer Contact Manager
------------------------
This component is used to determine the list of contacts of the customer and their presence. For simplicity, it does not handle contacts added/removed dynamically (since user is on the phone anyway).

Voice Services
--------------
There are there voice services available for the customer.
a.	Get Contact Service
This service will look up the presence of contact and offer callback option to the customer if the contact is not immediately available. 
b.	International Dial In Service
This service provides a way to connect the customer with another phone number. When the call is business related, it makes it possible to make the call as if the customer made the call from within the corporate network. Note that the user identity is used to make the call.
c.	Conference Service
This service is used to make a conference call. The customer call many contacts and phone numbers into the conference. The customer can press #1 and #2 to perform these tasks. While performing these tasks, the application has the ability to interact only with the customer (others will not hear any of the interactions with the application).

Workflows
----------
There are various workflows used in the application to accomplish the user experience. The main work flow offers the 3 choices above. The authentication work flow is used initially to welcome the customer and ask for the pin. The callback workflow is used to make the call to the customer to connect to the contact who just came online. There are many other workflows which can be seen from the application solution. 

Disclaimer
----------
This is just a sample and is not meant to be deployed as such for business use. There are many improvements possible in the application before it can be deployed for a real production. The purpose of this sample is to illustrate the usage and power of UCMA and not for the purpose of running an application for real customer use.
