using Lync.Enum;
using Lync.Service;
using Microsoft.Lync.Model.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class LyncConversation
	{
		private ILog _log = LogManager.GetLog(typeof(LyncConversation));

		public Conversation Conversation { get; set; }

		public ConversationType Type;
		protected ConversationService ConversationService { get; set; }

		protected ContactService ContactService { get; set; }

		public LyncConversation()
		{
			ContactService = ContactService.Instance;
			ConversationService = ConversationService.Instance;
		}

		public void CreateConversation()
		{
			ConversationService.AddConversation(Type);
		}

		public virtual void HandleAdded()
		{

		}

		internal void End()
		{
			try
			{
				if (Conversation != null)
				{
					Conversation.End();
				}
			}

			catch (Exception ex)
			{
				_log.Error("OnConversationManagerConversationAdded: " + ex.Message);
			}

		}

		internal void Terminate()
		{
			throw new NotImplementedException();
		}
	}
}
