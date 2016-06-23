using Lync.Enum;
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

		private ContactService _contactService;

		internal ConversationManager ConversationManager
		{
			get
			{
				return LyncService.Instance.Client.ConversationManager;
			}
		}

		private Conversation _currentConversation;

		public ConversationStatus _status;

		private string _sipUriOfRealPerson;


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
			_contactService = ContactService.Instance;
		}

		private void Init()
		{
			ConversationManager.ConversationAdded += OnConversationManagerConversationAdded;
			ConversationManager.ConversationRemoved += OnConversationManagerConversationRemoved;
			_sipUriOfRealPerson = string.Empty;
		}

		#region Conversation and  Participant

		private void OnConversationManagerConversationRemoved(object sender, ConversationManagerEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void OnConversationManagerConversationAdded(object sender, ConversationManagerEventArgs e)
		{
			var addedByThisProcess = true;

			try
			{
				//Suspend hosting new conversations until this conversation is ended
				ConversationManager.ConversationAdded -= OnConversationManagerConversationAdded;
			}

			catch (Exception ex)
			{
				_log.Error("Exception on de-register for ConversationAdded: " + ex.Message);
			}

			//If this class field is null then the new conversation was not started by this running process. It was
			//Started by the Lync client or by a remote user.
			if (_currentConversation == null)
			{
				addedByThisProcess = false;
				_currentConversation = e.Conversation;
			}

			if (_status == ConversationStatus.CallInProgress)
			{
				HandelrCall(addedByThisProcess);
			}

		}


		private void OnConversationStateChanged(object sender, ConversationStateChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void OnConversationParticipantAdded(object source, ParticipantCollectionChangedEventArgs data)
		{
			if (data.Participant.IsSelf != true)
			{
				if (((Conversation)source).Modalities[ModalityTypes.AudioVideo].CanInvoke(ModalityAction.Connect))
				{
					object[] asyncState = { ((Conversation)source).Modalities[ModalityTypes.AudioVideo], "CONNECT" };

					((Conversation)source).Modalities[ModalityTypes.AudioVideo].ModalityStateChanged += OnAvModalityModalityStateChanged;
					((Conversation)source).Modalities[ModalityTypes.AudioVideo].BeginConnect(OnModalityEndConnect, asyncState);
				}
			}
		}
		#endregion


		#region Modality
		private void OnModalityEndConnect(IAsyncResult ar)
		{
			Object[] asyncState = (Object[])ar.AsyncState;

			if (ar.IsCompleted == true)
			{
				((AVModality)asyncState[0]).EndConnect(ar);
			}
		}

		private void OnAvModalityModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
		{
			try
			{
				if (e.NewState == ModalityState.Connected)
				{
					var videoChannel = ((AVModality)_currentConversation.Modalities[ModalityTypes.AudioVideo]).VideoChannel;

					//wire up to state changes to control the UI display of the control.
					//wire up to action availability events to know when the channel is ready to be started.
					videoChannel.StateChanged += OnVideoChannelStateChanged;
					videoChannel.ActionAvailabilityChanged += OnVideoChannelActionAvailabilityChanged;

				}
			}
			catch (Exception ex)
			{
				_log.ErrorException("", ex);
			}
		}

		#endregion


		#region AudioVideo

		public void StartCall(string sipUriOfRealPerson)
		{
			_sipUriOfRealPerson = sipUriOfRealPerson;
			_currentConversation = ConversationManager.AddConversation();
			_status = ConversationStatus.CallInProgress;

		}

		private void HandelrCall(bool addedByThisProcess)
		{
			var avModality = (AVModality)_currentConversation.Modalities[ModalityTypes.AudioVideo];

			_log.Debug(string.Format("avMod state is {0}", avModality.State));

			if (avModality.State == ModalityState.Notified)
			{
				//incoming call

				avModality.ModalityStateChanged += OnAvModalityModalityStateChanged;
				avModality.Accept();
				OnCallAccepted();
			}
			else
			{
				_log.Debug(string.Format("else avMod state is {0}", avModality.State));
				//outgoing call

				_currentConversation.ParticipantAdded += OnConversationParticipantAdded;
				_currentConversation.StateChanged += OnConversationStateChanged;

				if (_currentConversation.CanInvoke(ConversationAction.AddParticipant))
				{
					var contact = _contactService.GetContactByUri(_sipUriOfRealPerson);
					_currentConversation.AddParticipant(contact);
				}

			}
		}

		private void OnCallAccepted()
		{
			//CallAccepted(null, EventArgs.Empty);
		}

		private void OnVideoChannelActionAvailabilityChanged(object sender, ChannelActionAvailabilityEventArgs e)
		{
			try
			{
				if (e.Action == ChannelAction.Start && e.IsAvailable == true)
				{
					var videoChannel = (VideoChannel)sender;
					if (videoChannel.CanInvoke(ChannelAction.Start))
					{

						//even though the Action IsAvailable is set to true *AND* CanInvoke is true, sometimes the channel isn't ready. There's no good
						//way of knowing when it'll become ready, and if you try and call it when it isn't ready, it won't error. However, the call back (videoChannelEndStart
						//in this case) never gets hit. The only thing I can think of in this situation is to wait..
						System.Threading.Thread.Sleep(2000);

						videoChannel.BeginStart(OnVideoChannelEndStart, videoChannel);

					}
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException("", ex);
			}
		}


		private static void OnVideoChannelEndStart(IAsyncResult result)
		{
			try
			{
				VideoChannel channel = (VideoChannel)result.AsyncState;
				channel.EndStart(result);
				//RaiseVideoAvailable(channel.CaptureVideoWindow, VideoDirection.Outgoing);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		private void OnVideoChannelStateChanged(object sender, ChannelStateChangedEventArgs e)
		{
			//_log(string.Format("Video Channel state change from {0} to {1}", e.OldState, e.NewState));
			//var videoChannel = (VideoChannel)sender;



			//if (_incomingChannelStates.Contains(e.NewState) && !_incomingChannelStates.Contains(e.OldState))    // Incoming newly available
			//{
			//	RaiseVideoAvailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}
			//else if (!_incomingChannelStates.Contains(e.NewState) && _incomingChannelStates.Contains(e.OldState))    // Incoming newly unavailable
			//{
			//	RaiseVideoUnavailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}

			//if (e.NewState == ChannelState.Send)
			//{
			//	// If outgoing is newly available, then raise the Available event
			//	if (!_outgoingChannelStates.Contains(e.OldState))
			//		RaiseVideoAvailable(videoChannel.CaptureVideoWindow, VideoDirection.Outgoing);

			//	// If incoming was previously available, then raise the Unavailable event
			//	if (_incomingVideoStreamStarted && _incomingChannelStates.Contains(e.OldState))
			//		RaiseVideoUnavailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}
			//else if (e.NewState == ChannelState.Receive)
			//{
			//	// If outgoing was previously available, then raise the Unavailable event
			//	if (_outgoingChannelStates.Contains(e.OldState))
			//		RaiseVideoUnavailable(videoChannel.CaptureVideoWindow, VideoDirection.Outgoing);

			//	// If incoming is newly available, then raise the Available event
			//	if (_incomingVideoStreamStarted && !_incomingChannelStates.Contains(e.OldState))
			//		RaiseVideoAvailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}
			//else if (e.NewState == ChannelState.SendReceive)
			//{
			//	// If outgoing is newly available, then raise the Available event
			//	if (!_outgoingChannelStates.Contains(e.OldState))
			//		RaiseVideoAvailable(videoChannel.CaptureVideoWindow, VideoDirection.Outgoing);

			//	// If incoming is newly available, then raise the Available event
			//	if (_incomingVideoStreamStarted && !_incomingChannelStates.Contains(e.OldState))
			//		RaiseVideoAvailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}
			//else
			//{
			//	// If outgoing was previously available, then raise the Unavailable event
			//	if (_outgoingChannelStates.Contains(e.OldState))
			//		RaiseVideoUnavailable(videoChannel.CaptureVideoWindow, VideoDirection.Outgoing);

			//	// If incoming was previously available, then raise the Unavailable event
			//	if (_incomingVideoStreamStarted && _incomingChannelStates.Contains(e.OldState))
			//		RaiseVideoUnavailable(videoChannel.RenderVideoWindow, VideoDirection.Incoming);
			//}
		}

		#endregion
	}
}
