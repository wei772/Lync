Title
-----
CallTransferSupervised

Description
-----------
The application represents three users, who we'll refer to as transferee,
transferor and transfer target for the sake of simplicity. The transferor places
audio calls to both transferee and transfer target, After the calls are accepted
and connect successfully, the transferor executes a supervised transfer from
transferee to the transfer target. In doing so, the original calls that the
transferor had with the transferee and transfer target are terminated, and new
call directly between transferee and transfer target is established. The
transferor then drops out of the communication. The application prints then
quits; shutting the platform down normally.

Features
--------
	• Call transfer, and established call activities
	• Basic Call placement
	• Basic audio/video call incoming use
	• AudioVideoFlow handling and control

Prerequisites
-------------
	• Microsoft Lync Server
	• Three users, enabled to use the Microsoft Lync Server
	• The credentials for those users, and a client capable of logging into Microsoft Lync Server
	• A currently-logged-in client on Microsoft Lync Server

Running the sample
------------------
	1. You may either supply the configuration settings to be used by the sample
	   in the accompanying App.config file, or you will be prompted for any
	   settings when you run the sample.
	2. Open the project in Visual Studio, and hit F5.