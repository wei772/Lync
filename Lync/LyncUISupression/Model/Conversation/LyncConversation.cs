using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using Lync.Enum;
using Lync.Service;
using Microsoft.Lync.Model.Conversation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class LyncConversation : ObservableObject
	{
		private ILog _log = LogManager.GetLog(typeof(LyncConversation));

		protected ConversationService ConversationService { get; set; }
		protected ContactService ContactService { get; set; }

		public Conversation Conversation { get; set; }

		public ConversationType Type;


		private ObservableCollection<ParticipantModel> _participants;
		public ObservableCollection<ParticipantModel> Participants
		{
			get { return _participants; }
			set
			{
				Set("Participants", ref _participants, value);
			}

		}

		private bool _isCanAddParticipant;
		public bool IsCanAddParticipant
		{
			get
			{
				return _isCanAddParticipant;
			}
			set
			{
				Set("IsCanAddParticipant", ref _isCanAddParticipant, value);
			}
		}

		private bool _isCanRemoveParticipant;
		public bool IsCanRemoveParticipant
		{
			get
			{
				return _isCanRemoveParticipant;
			}
			set
			{
				Set("IsCanRemoveParticipant", ref _isCanRemoveParticipant, value);
			}
		}


		public Action<Action> RunAtUI = DispatcherHelper.CheckBeginInvokeOnUI;


		public LyncConversation()
		{
			ContactService = ContactService.Instance;
			ConversationService = ConversationService.Instance;
			Participants = new ObservableCollection<ParticipantModel>();
		}

		public void CreateConversation()
		{
			ConversationService.AddConversation(this);
		}

		public void HandleAdded()
		{
			//registers for participant events
			foreach (var participant in Conversation.Participants)
			{
				Participants.Add(new ParticipantModel(participant));
			}
			Conversation.ParticipantAdded += OnConversationParticipantAdded;
			Conversation.ParticipantRemoved += OnConversationParticipantRemoved;
			Conversation.StateChanged += OnConversationStateChanged;

			//subscribes to the conversation action availability events (for the ability to add/remove participants)
			Conversation.ActionAvailabilityChanged += OnConversationActionAvailabilityChanged;


			HandleAddedCore();
		}

		protected virtual void HandleAddedCore()
		{

		}

		private void OnConversationParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
		{
			RunAtUI
				(() =>
					{
						var newPart = new ParticipantModel(e.Participant);
						Participants.Add(newPart);
						_log.Debug("OnConversationParticipantAdded  {0}", newPart);
						OnConversationParticipantAddedCore(e.Participant);
					}
				);
		}

		protected virtual void OnConversationParticipantAddedCore(Participant participant)
		{

		}

		private void OnConversationParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs e)
		{
			RunAtUI
				(() =>
					{

						var removePart = Participants.Where(p => p.Participant.Equals(e.Participant)).SingleOrDefault();
						if (removePart != null)
						{
							Participants.Remove(removePart);
						}
						_log.Debug("OnConversationParticipantRemoved  {0}", removePart);
					}
				);
		}

		private void OnConversationStateChanged(object sender, ConversationStateChangedEventArgs e)
		{
			if (e.NewState == ConversationState.Terminated)
			{
				Terminate();
			}

			_log.Debug("OnConversationStateChanged  NewState:{0}", e.NewState.ToString());
		}

		private void OnConversationActionAvailabilityChanged(object sender, ConversationActionAvailabilityEventArgs e)
		{
			//posts the execution into the UI thread
			RunAtUI
				(() =>
					{
						//each action is mapped to a button in the UI
						switch (e.Action)
						{
							case ConversationAction.AddParticipant:
								IsCanAddParticipant = e.IsAvailable;
								break;

							case ConversationAction.RemoveParticipant:
								IsCanRemoveParticipant = e.IsAvailable;
								break;
						}
						_log.Debug("OnConversationActionAvailabilityChanged  Action: {0}", e.Action.ToString());
					}
			);
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
				_log.Error("End: " + ex.Message);
			}

		}

		internal void Terminate()
		{
		}
	}
}
