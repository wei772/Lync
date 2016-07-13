using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Lync.Enum;
using Lync.Service;
using Microsoft.Lync.Model;
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

		public string ExternalUrl { get; set; }
		public Guid ExternalId { get; set; }


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

		private ParticipantCollection _participantCollection;
		public ParticipantCollection ParticipantCollection
		{
			get
			{
				return _participantCollection;
			}
			set
			{
				Set("ParticipantCollection", ref _participantCollection, value);
			}
		}

		public Action<Action> RunAtUI = DispatcherHelper.CheckBeginInvokeOnUI;

		private RelayCommand<ParticipantItem> _removeParticipantCommand;

		public RelayCommand<ParticipantItem> RemoveParticipantCommand
		{
			get
			{
				return _removeParticipantCommand ??
					(_removeParticipantCommand = new RelayCommand<ParticipantItem>(
						(part) =>
							{
								if (part != null)
								{
									if (Conversation.CanInvoke(ConversationAction.RemoveParticipant))
									{
										Conversation.RemoveParticipant(part.Participant);
									}
								}
							}
						)
					);
			}
		}

		#region Part

		private List<ConversationPart> _conversationParts = new List<ConversationPart>();

		private ApplicationSharingPart _applicationSharingPart;
		public ApplicationSharingPart ApplicationSharingPart
		{
			get
			{
				return _applicationSharingPart;
			}
			set
			{
				_applicationSharingPart = value;
			}
		}

		private VideoAudioPart _videoAudioPart;
		public VideoAudioPart VideoAudioPart
		{
			get
			{
				return _videoAudioPart;
			}
			set
			{
				_videoAudioPart = value;
			}
		}

		public Func<Participant, ParticipantItem> CreateParticipantModel { get; set; }

		#endregion


		public LyncConversation()
		{
			ContactService = ContactService.Instance;
			ConversationService = ConversationService.Instance;
			VideoAudioPart = new VideoAudioPart();
			ApplicationSharingPart = new ApplicationSharingPart();
			_conversationParts.Add(VideoAudioPart);
			_conversationParts.Add(ApplicationSharingPart);


		}

		public void CreateConversation()
		{
			ConversationService.AddConversation(this);
		}


		public void HandleAdded()
		{
			ParticipantCollection = ParticipantCollection.Instance;
			ParticipantCollection.Clear();


			Conversation.ParticipantAdded += OnConversationParticipantAdded;
			Conversation.ParticipantRemoved += OnConversationParticipantRemoved;
			Conversation.StateChanged += OnConversationStateChanged;

			//subscribes to the conversation action availability events (for the ability to add/remove participants)
			Conversation.ActionAvailabilityChanged += OnConversationActionAvailabilityChanged;
			Conversation.PropertyChanged += OnConversationPropertyChanged;

			InitPart();
			InvokePartHandleAdded();

			AddParticipant();
		}


		public string SipUriOfRealPerson { get; set; }

		public void AddParticipant()
		{
			if (string.IsNullOrEmpty(SipUriOfRealPerson))
			{
				return;
			}

			var contact = ContactService.GetContactByUri(SipUriOfRealPerson);
			if (contact != null)
			{
				Conversation.AddParticipant(contact);
			}
		}



		/// <summary>
		/// Ends the conversation if the user closes the window.
		/// </summary>
		public void Close()
		{
			if (Conversation == null)
			{
				return;
			}

			_log.Debug("Close");
			//need to remove event listeners otherwide events may be received after the form has been unloaded
			Conversation.StateChanged -= OnConversationStateChanged;
			Conversation.ParticipantAdded -= OnConversationParticipantAdded;
			Conversation.ParticipantRemoved -= OnConversationParticipantRemoved;
			Conversation.ActionAvailabilityChanged -= OnConversationActionAvailabilityChanged;

			InvokePartClose();

			//if the conversation is active, will end it
			if (Conversation.State != ConversationState.Terminated)
			{
				//ends the conversation which will disconnect all modalities
				try
				{
					Conversation.End();
				}
				catch (LyncClientException lyncClientException)
				{
					_log.ErrorException("Close", lyncClientException);
				}
				catch (SystemException systemException)
				{
					if (LyncModelExceptionHelper.IsLyncException(systemException))
					{
						// Log the exception thrown by the Lync Model API.
						_log.ErrorException("Error: ", systemException);
					}
					else
					{
						// Rethrow the SystemException which did not come from the Lync Model API.
						throw;
					}
				}
			}

			Conversation = null;
		}


		private void OnConversationParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
		{
			RunAtUI
				(() =>
				{
					var newPart = e.Participant;
					var newModel = CreateAndInitParticipant(newPart);
					ParticipantCollection.AddItem(newModel);
					_log.Debug("OnConversationParticipantAdded  uri:{0}", newPart.Contact.Uri);
					InvokePartConversationParticipantAdded(newModel);
				}
				);
		}

		private void OnConversationParticipantRemoved(object sender, ParticipantCollectionChangedEventArgs e)
		{
			RunAtUI
				(() =>
				{

					var removePart = ParticipantCollection.Remove(e.Participant.Contact.Uri);

					InvokePartConversationParticipantRemoved(removePart);

					_log.Debug("OnConversationParticipantRemoved  uri:{0}", removePart.Participant.Contact.Uri);
				}
				);
		}

		private void OnConversationPropertyChanged(object sender, ConversationPropertyChangedEventArgs e)
		{
			Conversation conference = (Conversation)sender;

			switch (e.Property)
			{
				case ConversationProperty.ConferenceAcceptingParticipant:
					Contact acceptingContact = (Contact)e.Value;
					break;
				case ConversationProperty.ConferencingUri:
					break;
				case ConversationProperty.ConferenceAccessInformation:

					try
					{

						var conferenceKey = CreateConferenceKey();
					}
					catch (NullReferenceException nr)
					{
						_log.ErrorException("Null ref Eception on ConferenceAccessInformation changed ", nr);
					}
					catch (LyncClientException lce)
					{
						_log.ErrorException("Exception on ConferenceAccessInformation changed ", lce);
					}
					break;
			}

		}

		private void OnConversationStateChanged(object sender, ConversationStateChangedEventArgs e)
		{
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

		/// <summary>
		/// Returns the meet-now meeting access key as a string
		/// </summary>
		/// <returns></returns>
		private string CreateConferenceKey()
		{
			string returnValue = string.Empty;
			try
			{

				//These properties are used to invite people by creating an email (or text message, or IM)
				//and adding the dial in number, external Url, internal Url, and conference Id
				ConferenceAccessInformation conferenceAccess =
					(ConferenceAccessInformation)Conversation.Properties[ConversationProperty.ConferenceAccessInformation];

				if (conferenceAccess == null)
				{
					if (!Conversation.CanSetProperty(ConversationProperty.ConferenceAccessInformation))
					{
						return string.Empty;
					}

					//Conversation.BeginSetProperty(
					//	ConversationProperty.ConferenceAccessInformation,
					//	 new ConferenceAccessInformation() , (ar) =>
					//	{
					//		Conversation.EndSetProperty(ar);
					//	},
					//	null);
				}

				StringBuilder MeetingKey = new StringBuilder();

				if (conferenceAccess.Id.Length > 0)
				{
					MeetingKey.Append("Meeting Id: " + conferenceAccess.Id);
					MeetingKey.Append(System.Environment.NewLine);
				}

				if (conferenceAccess.AdmissionKey.Length > 0)
				{
					MeetingKey.Append(conferenceAccess.AdmissionKey);
					MeetingKey.Append(System.Environment.NewLine);
				}

				string[] attendantNumbers = (string[])conferenceAccess.AutoAttendantNumbers;

				StringBuilder sb2 = new StringBuilder();
				sb2.Append(System.Environment.NewLine);
				foreach (string aNumber in attendantNumbers)
				{
					sb2.Append("\t\t" + aNumber);
					sb2.Append(System.Environment.NewLine);
				}
				if (sb2.ToString().Trim().Length > 0)
				{
					MeetingKey.Append("Auto attendant numbers:" + sb2.ToString());
					MeetingKey.Append(System.Environment.NewLine);
				}

				if (conferenceAccess.ExternalUrl.Length > 0)
				{
					MeetingKey.Append("External Url: " + conferenceAccess.ExternalUrl);
					MeetingKey.Append(System.Environment.NewLine);
				}

				if (conferenceAccess.InternalUrl.Length > 0)
				{
					MeetingKey.Append("Inner Url: " + conferenceAccess.InternalUrl);
					MeetingKey.Append(System.Environment.NewLine);
				}

				MeetingKey.Append("Meeting access type: " + (
					(ConferenceAccessType)Conversation.Properties[ConversationProperty.ConferencingAccessType])
					.ToString());

				MeetingKey.Append(System.Environment.NewLine);
				returnValue = MeetingKey.ToString();

			}
			catch (System.NullReferenceException nr)
			{
				System.Diagnostics.Debug.WriteLine(
					"Null ref Eception on ConferenceAccessInformation changed " + nr.Message);
			}
			catch (LyncClientException lce)
			{
				System.Diagnostics.Debug.WriteLine(
					"Exception on ConferenceAccessInformation changed " + lce.Message);
			}
			return returnValue;
		}


		#region parts method

		private void InitPart()
		{
			foreach (var part in _conversationParts)
			{
				part.LyncConversation = this;
				part.ContactService = ContactService;
				part.ParticipantCollection = ParticipantCollection;
				part.Conversation = Conversation;
			}
		}

		private void InvokePartHandleAdded()
		{
			foreach (var part in _conversationParts)
			{
				part.HandleAddedInternal();
			}
		}

		private void InvokePartClose()
		{
			foreach (var part in _conversationParts)
			{
				part.CloseInternal();
			}
		}

		private void InvokePartConversationParticipantAdded(ParticipantItem participant)
		{
			foreach (var part in _conversationParts)
			{
				part.ConversationParticipantAddedInternal(participant);
			}
		}

		private void InvokePartConversationParticipantRemoved(ParticipantItem participant)
		{
			foreach (var part in _conversationParts)
			{
				part.ConversationParticipantRemovedInternal(participant);
			}
		}

		private ParticipantItem CreateAndInitParticipant(Participant participant)
		{
			var part = CreateParticipantModel(participant);
			var displayName = (string)participant.Contact.GetContactInformation(ContactInformationType.DisplayName);
			part.DisplayName = displayName;
			part.Id = participant.Contact.Uri;

			return part;
		}

		#endregion

	}



}
