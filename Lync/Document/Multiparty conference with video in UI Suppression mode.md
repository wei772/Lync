[Multiparty conference with video in UI Suppression mode](https://social.msdn.microsoft.com/Forums/lync/en-US/960018cc-5768-437b-b87e-ee0f9b47ba42/multiparty-conference-with-video-in-ui-suppression-mode?forum=communicatorsdk)

In a conference video scenario, local endpoints receive one video stream from the conference resource itself. 
The conference resource uses pin/lock/active speaker/gallery modes to determine the contents of the video stream. 
In the 2013 release of the API, we allow an endpoint to set pin and lock modes for the video of individual participants. 
A conference presenter can override individual pin preferences by locking the video stream on an individual.

At the API platform level, the conference resource is represented by a participant object and conference video 
is obtained from that participant. The Participant.Contact.Uri property for the conference resource returns a 
SIP addy formatted as a conference Url instead of a person's SIP Url.

In addition, there are participant objects for each person who is in the conference. Wei stated earlier in the 
thread that the Participant object exposes Modalities collection. In the collection is an AVModality object. 
That object has a VideoChannel object. The VideoChannel object provides capture and render video windows. 
This is generally true for all participant objects. However, a conference scenario is a bit different. 
In this case, the video of the conference resource participant is active. I suspect that the other participant video is not active. 

Your question is about the video capabilities of the Participant objects that represent people in the conference 
(rather than from the conference resource Participant). So in the scenario where I've pinned the video of John Doe (participant 2)
, does his video stream come from the conference resource Participant object or does it come from the John Doe participant object?
  Good question. My hunch is that it still comes from the conference resource Participant where the video stream is shown in 
"gallery mode".  I'll poke around with one of my conference samples to see if I can find the facts in this matter. 

Stay tuned. I'll post an answer in the next couple of days.

Side note: The conference video stream that is broadcast  out to all conference participants is a composite of
 the individual video streams sent from endpoints into conference resource. The conference resource acts as a sort of 
switch that picks from among the individual inbound video streams and sends the picked video stream outbound to all 
conference resources. 

The server video picking algorithm uses the "active speaker" mode which can be individually overridden by pinning 
the video of a conference participant. In this case, the conference resource honors the pin request from an endpoint 
and substitutes the inbound video of the pinned participant for the active speaker's video. When a conference presenter
 locks the video on a single participant, the active speaker mode is disabled and every endpoint gets the video of the
 locked participant from the conference resource.

In "gallery mode", pinning still works as does locking. I do not know (yet) if the gallery view is composed of 5 individual
 streams from people participant objects, or 1 composite stream from the conference resource object. 
