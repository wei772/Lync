Readme file for the VoiceXMLSample application

1. Problem- Files referenced from within a VoiceXML document may result in the browser object exiting with error.badfetch.
Solution- To reference files from within a VoiceXML document, insure that one of the following is true:

a) The base directory is set using a xml:base attribute inside the <vxml ..> element, e.g. xml:base="C:\Grammars", and the referenced files are in that directory
b) The fully qualified path to the file is given.
c) The file is referenced through a web server.

2. Problem - Cannot access VoiceXML documents or other files from a web server.
Solution: To access VoiceXml documents, grammar files, or other files from a web server do the following:
a) Using the IIS Manager set up a virtual directory and copy the files to that directory
b) Using the IIS Manager set MIME types for all file exensions used. Types used in typical VoiceXML applications include:
	.vxml	text/xml
	.grxml	text/xml
	.cfg	application/srgs+xml
	.ssml	application/ssml+xml

3. There are two VoiceXML files and one grammar file included in the subfolder VoiceXMLSampleData below the folder of the main source files for use with the sample. You may add other files to this folder to use in the sample using a relative path such as VoiceXMLSampleData\filename.vxml. After adding files to the folder be sure to add them to the project by clicking on the VoiceXMLSampleData node in Solution Explorer and right clicking on the files to be added. Also make sure that the "Copy to Output Directory" property for each added file is set to "Copy always".

4. Other notes- 
 
a) As this sample illustrates, the VoiceXML browser object can be used multiple times (sessions) as long as it is bound to a UCMA AudioVideoCall object for each session. To reuse a browser instance after a session completes the application must call SetAudioVideoCall to bind the AudioVideoCall object to the broswer object.
b) Unlike many VoiceXML interpreters in the industry, the VoiceXML browser object that ships in the Microsoft Speech Platform and UCMA 4.0 does not automatically disconnect the associated call on session completion unless the VoiceXML interpreter processes a <disconnect> or <transfer> element. Thus, an application should check the state of the AudioVideoCall object on session completion and take appropriate action if it still active, as noted in the comments in this sample.

5. For other release notes related to VoiceXML see the Speech SDK release notes.
	
 