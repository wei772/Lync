using Lync.Enum;
using Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Service
{
	public class ConversationService
	{
		private ILog _log = LogManager.GetLog(typeof(ConversationService));

		private LyncService _lyncService;


		internal ConversationManager ConversationManager
		{
			get
			{
				return LyncService.Instance.Client.ConversationManager;
			}
		}


		private Dictionary<Guid, LyncConversation> _LyncConversationDictionary;

		private LyncConversation _currentLyncConversation;

		public ConversationType Type;


		private static ConversationService _instance;
		public static ConversationService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ConversationService();
				}
				return _instance;
			}
		}

		private ConversationService()
		{
			_lyncService = LyncService.Instance;
	
		}

		public void InitializeConversationEvent()
		{
			ConversationManager.ConversationAdded += OnConversationManagerConversationAdded;
			ConversationManager.ConversationRemoved += OnConversationManagerConversationRemoved;
		}


		internal void AddConversation(ConversationType type)
		{
			Type = type;
			ConversationManager.AddConversation();
		}




		private void OnConversationManagerConversationRemoved(object sender, ConversationManagerEventArgs e)
		{
			_currentLyncConversation = null;
		}

		private void OnConversationManagerConversationAdded(object sender, ConversationManagerEventArgs e)
		{
			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.End();
			}

			var conversation = e.Conversation;

			_currentLyncConversation.HandleAdded();

			_log.Debug("OnConversationManagerConversationAdded  Type:{0}", Type.ToString());
		}


		private void OnConversationStateChanged(object sender, ConversationStateChangedEventArgs e)
		{
			if (e.NewState == ConversationState.Terminated)
			{
				_currentLyncConversation.Terminate();
			}
		}






	}
}
