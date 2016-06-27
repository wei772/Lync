Copyright (c) Microsoft Corporation.  All rights reserved.

Conversation Translator Sample 	

Uses the conversation namespace from the Lync Model API to intercept instant messages and provide translation using Bing Web Services.


Warnings:
- Requires to apply for an AppId from Microsoft Bing Translator (http://www.bing.com/developer).
- Requires internet access for the translation content service from Bing Translation.
- Please ignore the Visual Studio warning about Web Services without Web Projects. 
  Since this project uses an external Web Service, there are no issues.


Features
- Provides a sample architecture for registering for and handling asynchronous Lync SDK events in Silverlight.
- Register for two Conversation related events: ParticipantAdded, InstantMessageReceived.
- Use the InstantMessageModality.BeginSendMessage() method and callback.
- Uses the Bing Translator Web Service.


Prerequisites (for compiling and running in Visual Studio)
- Visual Studio 2010 or above. 
- Silverlight 4 SDK or above.
- Microsoft Lync Client SDK (2010 or above).

Prerequisites (for running installed sample on client machines)
- Microsoft Lync 2010 or above. 


Running the sample
------------------
1. Preferably copy the Conversation Translator folder to a folder outside of Program Files.
2. Open ConversationTranslator.csproj
3. Have Lync running with exactly one conversation window.
	- For debugging purposes, the Conversation Translator will associate itself with the first element in the conversation collection
4. Update Services\TranslationService.cs. Replace "xxxxx" in the following line of code with a valid AppId you apply from Microsoft Bing. Save All.
       private const string AppID = "xxxxx";

5. Hit F5 in Visual Studio.
6. Verification: the sample will be ready and working when the language combo boxes are filled with the languages.


How to use the sample application
---------------------------------
1. Select your language in the Me: combo box.
2. Select your target audience's language in the Them: combo box.
3. Received messages will automatically be translated to your language
4. To send a message:
	a. Type the message in the input area and hit Enter or click the Translate button
	b. When the translation arrives, edit it if necessary
	c. After verifying the translator hit Enter again or click the Send button
5. You may use the Cancel button to return back to typing the original message


How to install the Conversation Translator into the conversation window extension area
--------------------------------------------------------------------------------------
1. Build the sample project in Visual Studio
2. Create and execute a registry file (.reg) with content similar to the sample ones provided in this sample:

    RegisterTranslatorLocal_changeme.reg
    RegisterTranslatorNetworkShare_changeme.reg
    RegisterTranslatorWebsite_changeme.reg

Before using them, please update them based on your server names, domain names, and/or file locations.

3. Within a conversation window, open the extension menu 
For Lync 2010: (double arrows on the right of the conversation buttons bar) and choose Conversation Translator.
For Lync 2013: Click the overflow button (...) at the right bottom of the conversation window, and choose Conversation Translator.

Please refer to the SDK documentation for more details.
	
