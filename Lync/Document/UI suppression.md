[UI suppression](https://msdn.microsoft.com/en-us/library/office/jj933224.aspx)

UI suppression limitations


The running UI suppressed Lync process gives your application access to the same Lync client endpoint, 
SIP processing, and all media processing that an unsuppressed Lync client uses with the following limitations. 
The visible components of the SDK are not available except for the video window. 
Components such as the Microsoft Lync 2013 Controls for Silverlight browser or WPF applications 
and the meeting content and resource sharing modalities cannot be used in your application when UI suppression is enabled. 
In addition, you cannot use the objects in the Microsoft.Lync.Model.Extensibility namespace to automate starting conversations or meetings.


The Microsoft.Lync.Model.Conversation.AudioVideo.VideoWindow is a special exception to the previously described rule. 
In this case, the VideoWindow is designed to replace the Lync video window that is lost when the Lync client UI is suppressed.
 If you implement the video window in your application, you can provide the full range of basic communication modes, 
from IM to audio/video and even Persistent Chat.
