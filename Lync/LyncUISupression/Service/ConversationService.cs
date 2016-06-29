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

        public void CreateConversationUseExternalUrl(string url, LyncConversation lyncConversation)
        {
            Process.Start(url);
       
            if (_currentLyncConversation != null)
            {
                _currentLyncConversation.End();
            }
            _currentLyncConversation = lyncConversation;

            ConversationManager.ConversationAdded += OnConversationManagerConversationAdded;
            ConversationManager.ConversationRemoved += OnConversationManagerConversationRemoved;
            _currentLyncConversation = lyncConversation;

           // var conversation = ConversationManager.JoinConference(url);
          //  conversation.StateChanged += OnConversationStateChanged;
        }


        internal void AddConversation(LyncConversation lyncConversation)
		{
			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.End();
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
				_currentLyncConversation.Conversation = null;
			}
		}

		private void OnConversationManagerConversationAdded(object sender, ConversationManagerEventArgs e)
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

			var conversation = e.Conversation;
			if (_currentLyncConversation == null)
			{
				addedByThisProcess = false;
				_currentLyncConversation = ConversationFactory.CreateLyncConversation(conversation);
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

		public void Close()
		{
			if (_currentLyncConversation != null)
			{
				_currentLyncConversation.Close();
			}
		}


	}
}
