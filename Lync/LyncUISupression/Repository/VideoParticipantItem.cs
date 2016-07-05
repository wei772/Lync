using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Repository
{
	public class VideoParticipantItem
	{
		public string Id { get; set; }


		public string DisplayName { get; set; }
	

		public bool IsPined { get; set; }
	
	
		public VideoWindow CaptureVideoWindow { get; set; }

		public int CaptureVideoWindowOriginWidth { get; set; }

		public int CaptureVideoWindowOriginHeight { get; set; }


		public VideoWindow RenderVideoWindow { get; set; }

		public int RenderVideoWindowOriginWidth { get; set; }

		public int RenderVideoWindowOriginHeight { get; set; }



		public AVModality Modality { get; set; }

		public VideoChannel VideoChannel { get; set; }


		public Participant Participant { get; set; }



		public bool IsMatch(VideoChannel channel)
		{
			return channel.GetHashCode() == VideoChannel.GetHashCode();
		}
	}
}
