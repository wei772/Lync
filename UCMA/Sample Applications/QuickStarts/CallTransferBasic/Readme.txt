Title
-----
CallTransferBasic

Description
-----------
This sample involves communication between three users; a transferor, a transferee
and a transfer target. The sample logs in as the transferor, who is the user who
receives an incoming audiovideo call from the transferee, logged onto Microsoft Lync.
The transferor accepts the call from the transferee, and initiates a transfer to
the transfer target. This sample demonstrates the ATTENDED transfer-type; hence
after the transfer completes, the initial incoming call is disconnected.
 
The user has the option whether to opt for attended or unattended transfers
by changing the transfer-type in the code.

The difference between attended and unattended transfer is that unattended 
transfers begin the transfer (send the REFER to the caller) and terminate the 
initial call on receipt of the transfer request response (202-Reply) from the 
caller. Attended transfers, on the other hand, wait to terminate the call until 
the transfer either succeeds or fails. The application prints logging to the 
console, and then quits; shutting the platform down normally.


Features
--------
	• Call transfer, and established call activities.
	• Basic Call placement.
	• Handling of an incoming audio video call.
	• AudioVideoFlow handling and control.

Prerequisites
-------------
	• Microsoft Lync Server
	• Three users capable of sending/receiving Voice calls.
	• The credentials for those users, and a client (such as, Microsoft Lync) 
	capable of logging in to Microsoft Lync Server.
	• A currently logged-in user on Microsoft Lync, using one of the three users 
	above, that will initiate the call sequence.

Running the sample
------------------
    1. You may either supply the user credentials in the accompanying app.config
	   file, or you will be prompted for them when you run the sample.
    2. Open the project in Visual Studio, and hit F5.
    3. Initiate a voice call to the user whose credentials the endpoint is using.