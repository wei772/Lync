using Lync.Enum;
using Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private string GetConferenceUrl(string meetingUrl)
		{
			var id = meetingUrl.Split('/').Last();

			return string.Format("sip:jrtonyxia@o365ms.com;gruu;opaque=app:conf:focus:id:{0}?", id);
		}
		public void CreateConversationUseExternalUrl(string url, LyncConversation lyncConversation)
		{

			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.Close();
			}
			_currentLyncConversation = lyncConversation;

			ConversationManager.ConversationAdded += OnConversationManagerConversationAdded;
			ConversationManager.ConversationRemoved += OnConversationManagerConversationRemoved;
			_currentLyncConversation = lyncConversation;

			var conferUrl = GetConferenceUrl(url);

			_log.Info("ConferenceUrl:{0}  ExternalUrl:{1}", conferUrl, url);


			Conversation conversation = null;
			var existConversations = ConversationManager.Conversations;

			foreach (var existConversation in existConversations)
			{
				_log.Debug((string)existConversation.Properties[ConversationProperty.ConferencingUri]);
				if ((string)existConversation.Properties[ConversationProperty.ConferencingUri]+"?" == conferUrl)
				{
					conversation = existConversation;
					ConversationManagerConversationAdded(conversation);
				}
			}
			if (conversation == null)
			{
				conversation = ConversationManager.JoinConference(conferUrl);
			}

			conversation.StateChanged += OnConversationStateChanged;
		}


		public void AddConversation(LyncConversation lyncConversation)
		{
			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.Close();
			}
			_currentLyncConversation = lyncConversation;

			ConversationManager.ConversationAdded += OnConversationManagerConversationAdded;
			ConversationManager.ConversationRemoved += OnConversationManagerConversationRemoved;

			var conversation = ConversationManager.AddConversation();
			conversation.StateChanged += OnConversationStateChanged;

		}

		private void OnConversationStateChanged(object sender, ConversationStateChangedEventArgs e)
		{
			if (e.NewState == ConversationState.Terminated)
			{
			}
			_log.Debug("OnConversationStateChanged  NewState:{0}", e.NewState.ToString());
		}

		private void OnConversationManagerConversationRemoved(object sender, ConversationManagerEventArgs e)
		{
			if (_currentLyncConversation != null)
			{
				//	_currentLyncConversation.Close();
			}
		}

		private void ConversationManagerConversationAdded(Conversation newConversation)
		{
			Boolean addedByThisProcess = true;

			try
			{
				//Suspend hosting new conversations until this conversation is ended
				ConversationManager.ConversationAdded -= OnConversationManagerConversationAdded;
			}

			catch (Exception ex)
			{
				_log.Error("ConversationAdded", ex);
			}

			var conversation = newConversation;
			if (_currentLyncConversation == null)
			{
				addedByThisProcess = false;
				//	_currentLyncConversation = ConversationFactory.CreateLyncConversation(conversation);
			}

			_log.Debug("OnConversationManagerConversationAdded  addedByThisProcess:{0}", addedByThisProcess);

			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.Conversation = conversation;
				_currentLyncConversation.HandleAdded();

			}
			else
			{
				_log.Debug("OnConversationManagerConversationAdded  none conversationType");
			}
		}

		private void OnConversationManagerConversationAdded(object sender, ConversationManagerEventArgs e)
		{
			ConversationManagerConversationAdded(e.Conversation);
		}

		public void Close()
		{
			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.Close();
			}
		}


	}
}
