using Lync.Enum;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Lync.Model
{
	public class VideoAudioConversation : LyncConversation
	{

		private ILog _log = LogManager.GetLog(typeof(VideoAudioConversation));

		private string _sipUriOfRealPerson;


		private AudioChannel _audioChannel;
		private VideoChannel _videoChannel;

		private AVModality _avModality;


		private ObservableCollection<ParticipantVideoModel> _participantVideoModels = new ObservableCollection<ParticipantVideoModel>();

		public ObservableCollection<ParticipantVideoModel> ParticipantVideoModels
		{
			get
			{
				return _participantVideoModels;
			}
			set
			{
				Set("ParticipantVideoModels", ref _participantVideoModels, value);
			}
		}

		/// <summary>
		/// The Application sharing modality of the local participant.
		/// </summary>
		ParticipantVideoModel _localParticipantVideoModel;


		public VideoAudioConversation()
		{
			Type = ConversationType.Audio;
		}

		public void Init(string sipUriOfRealPerson)
		{
			_sipUriOfRealPerson = sipUriOfRealPerson;
			//CreateConversation();
		}


		protected override void HandleAddedInternal()
		{
			//saves the AVModality, AudioChannel and VideoChannel, just for the sake of readability
			_avModality = (AVModality)Conversation.Modalities[ModalityTypes.AudioVideo];
			_audioChannel = _avModality.AudioChannel;
			_videoChannel = _avModality.VideoChannel;

			//subscribes to modality action availability events (all audio button except DTMF)
			_avModality.ActionAvailabilityChanged += OnAvModalityActionAvailabilityChanged;

			//subscribes to the modality state changes so that the status bar gets updated with the new state
			_avModality.ModalityStateChanged += OnAvModalityModalityStateChanged;


			//subscribes to the audio channel action availability events (DTMF only)
			_audioChannel.ActionAvailabilityChanged += OnAudioChannelActionAvailabilityChanged;

			//subscribes to the video channel state changes so that the status bar gets updated with the new state
			_audioChannel.StateChanged += OnAudioChannelStateChanged;


			//subscribes to the video channel action availability events
			_videoChannel.ActionAvailabilityChanged += OnVideoChannelActionAvailabilityChanged;

			//subscribes to the video channel state changes so that the video feed can be presented
			_videoChannel.StateChanged += OnVideoChannelStateChanged;


			AddParticipant();
		}


		protected override void CloseInternal()
		{
			//subscribes to modality action availability events (all audio button except DTMF)
			_avModality.ActionAvailabilityChanged -= OnAvModalityActionAvailabilityChanged;

			//subscribes to the modality state changes so that the status bar gets updated with the new state
			_avModality.ModalityStateChanged -= OnAvModalityModalityStateChanged;


			//subscribes to the audio channel action availability events (DTMF only)
			_audioChannel.ActionAvailabilityChanged -= OnAudioChannelActionAvailabilityChanged;

			//subscribes to the video channel state changes so that the status bar gets updated with the new state
			_audioChannel.StateChanged -= OnAudioChannelStateChanged;


			//subscribes to the video channel action availability events
			_videoChannel.ActionAvailabilityChanged -= OnVideoChannelActionAvailabilityChanged;

			//subscribes to the video channel state changes so that the video feed can be presented
			_videoChannel.StateChanged -= OnVideoChannelStateChanged;
		}

		protected void AddParticipant()
		{
			var contact = ContactService.GetContactByUri(_sipUriOfRealPerson);
			if (contact != null)
			{
				Conversation.AddParticipant(contact);
			}
		}


		#region Modality
		private void OnModalityEndConnect(IAsyncResult ar)
		{
			Object[] asyncState = (Object[])ar.AsyncState;

			if (ar.IsCompleted == true)
			{
				((AVModality)asyncState[0]).EndConnect(ar);
			}
		}

		private void OnAvModalityActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
		{

			_log.Debug("OnAvModalityActionAvailabilityChanged");

			RunAtUI
				(() =>
				{

					//each action is mapped to a button in the UI
					switch (e.Action)
					{
						//case ModalityAction.Connect:
						// buttonConnectAudio.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Disconnect:
						// buttonDisconnectAudio.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Hold:
						// buttonHold.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Retrieve:
						// buttonRetrieve.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.LocalTransfer:
						// buttonTransfer.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.ConsultAndTransfer:
						// buttonConsultTransfer.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Forward:
						// buttonForward.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Accept:
						// buttonAccept.Enabled = e.IsAvailable;
						// break;

						//case ModalityAction.Reject:
						// buttonReject.Enabled = e.IsAvailable;
						// break;
					}
				}

			   );
		}

		private void OnAvModalityModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
		{
			try
			{
				_log.Debug("OnAvModalityModalityStateChanged  NewState:{0}", e.NewState.ToString());

				if (e.NewState == ModalityState.Connected)
				{
					var videoChannel = ((AVModality)Conversation.Modalities[ModalityTypes.AudioVideo]).VideoChannel;

					//wire up to state changes to control the UI display of the control.
					//wire up to action availability events to know when the channel is ready to be started.
					//videoChannel.StateChanged += OnVideoChannelStateChanged;
					//videoChannel.ActionAvailabilityChanged += OnVideoChannelActionAvailabilityChanged;

				}
			}
			catch (Exception ex)
			{
				_log.ErrorException("", ex);
			}
		}

		#endregion


		#region AudioChannel

		private void OnAudioChannelStateChanged(object sender, ChannelStateChangedEventArgs e)
		{
			_log.Debug("OnAudioChannelStateChanged  NewState:{0}", e.NewState.ToString());
			////posts the execution into the UI thread
			//this.BeginInvoke(new MethodInvoker(delegate ()
			//{
			//	//updates the status bar with the video channel state
			//	toolStripStatusLabelAudioChannel.Text = e.NewState.ToString();
			//}));
			if (e.NewState == ChannelState.Inactive)
			{

			}

		}

		private void OnAudioChannelActionAvailabilityChanged(object sender, ChannelActionAvailabilityEventArgs e)
		{
			_log.Debug("OnAudioChannelActionAvailabilityChanged  NewState:{0}", e.Action.ToString());
			////posts the execution into the UI thread
			//this.BeginInvoke(new MethodInvoker(delegate ()
			//{

			//	//only SendDtmf is used here since the button are already mapped
			//	//to the action availability of the modality itself
			//	if (e.Action == ChannelAction.SendDtmf)
			//	{
			//		buttonSendDTMF.Enabled = e.IsAvailable;
			//	}

			//}));

			try
			{
				_log.Debug("OnVideoChannelActionAvailabilityChanged");
				if (e.Action == ChannelAction.Start && e.IsAvailable == true)
				{
					var audioChannel = (AudioChannel)sender;
					if (audioChannel.CanInvoke(ChannelAction.Start))
					{

						//even though the Action IsAvailable is set to true *AND* CanInvoke is true, sometimes the channel isn't ready. There's no good
						//way of knowing when it'll become ready, and if you try and call it when it isn't ready, it won't error. However, the call back (videoChannelEndStart
						//in this case) never gets hit. The only thing I can think of in this situation is to wait..

						audioChannel.BeginStart(OnAudioChannelEndStart, audioChannel);

					}
				}
			}
			catch (Exception ex)
			{
				_log.ErrorException("", ex);
			}
		}

		private void OnAudioChannelEndStart(IAsyncResult result)
		{
			try
			{
				_log.Debug("OnAudioChannelEndStart");

				AudioChannel channel = (AudioChannel)result.AsyncState;
				channel.EndStart(result);
				//RaiseVideoAvailable(channel.CaptureVideoWindow, VideoDirection.Outgoing);
			}
			catch (Exception ex)
			{
				_log.ErrorException("", ex);
			}
		}

		#endregion

		#region VideoChannel

		private void OnVideoChannelStateChanged(object sender, ChannelStateChangedEventArgs e)
		{
			//posts the execution into the UI thread
			RunAtUI(() =>
		   {

			   //*****************************************************************************************
			   //                              Video Content
			   //
			   // The video content is only available when the Lync client is running in UISuppressionMode.
			   //
			   // The video content is not directly accessible as a stream. It's rather available through
			   // a video window that can de drawn in any panel or window.
			   //
			   // The outgoing video is accessible from videoChannel.CaptureVideoWindow
			   // The window will be available when the video channel state is either Send or SendReceive.
			   // 
			   // The incoming video is accessible from videoChannel.RenderVideoWindow
			   // The window will be available when the video channel state is either Receive or SendReceive.
			   //
			   //*****************************************************************************************

			   _log.Debug("OnVideoChannelStateChanged  OldState:{0} NewState:{1} channel:{2}  channelCode:{3}"
					, e.OldState.ToString()
					, e.NewState.ToString()
					, sender.ToString()
					, sender.GetHashCode()
				   );


			   //if the outgoing video is now active, show the video (which is only available in UI Suppression Mode)
			   if ((e.NewState == ChannelState.Send
				  || e.NewState == ChannelState.SendReceive) && _videoChannel.CaptureVideoWindow != null)
			   {
				   SetParticipantVideoWindow(_videoChannel, _videoChannel.CaptureVideoWindow);
				   //presents the video in the panel
				   //  ShowVideo(panelOutgoingVideo, _videoChannel.CaptureVideoWindow);
			   }

			   //if the incoming video is now active, show the video (which is only available in UI Suppression Mode)
			   if ((e.NewState == ChannelState.Receive
				  || e.NewState == ChannelState.SendReceive) && _videoChannel.RenderVideoWindow != null)
			   {
				   //presents the video in the panel
				   // SetParticipantVideoWindow(_videoChannel, _videoChannel.RenderVideoWindow);
			   }

		   });
		}



		private void OnVideoChannelActionAvailabilityChanged(object sender, ChannelActionAvailabilityEventArgs e)
		{
			//posts the execution into the UI thread
			RunAtUI(() =>
		   {

			   //each action is mapped to a button in the UI
			   switch (e.Action)
			   {
				   case ChannelAction.Start:
					   // buttonStartVideo.Enabled = e.IsAvailable;
					   break;

				   case ChannelAction.Stop:
					   // buttonStopVideo.Enabled = e.IsAvailable;
					   break;
			   }

		   });
		}

		#endregion


		#region Video channel related actions

		//*****************************************************************************************
		//                              VideoChannel related actions
		//
		// The video channel action will behave differently depending on whether the audio is already
		// connected.
		//
		// If audio is not connected, starting video is equivalent to connecting the modality. If the
		// conversation already has audio, starting video will start the outgoing video stream. The 
		// other participants in the conversation also need to start their own video.
		//
		// Stopping the video channel will stop both outgoing and incoming video. It will remove video
		// from the conversation.
		//
		//*****************************************************************************************

		/// <summary>
		/// Starts the video channel: VideoChannel.BeginStart()
		/// </summary>
		private void StartVideo()
		{
			_log.Debug("StartVideo");

			//starts a video call or the video stream in a audio call
			AsyncCallback callback = new AsyncOperationHandler(_videoChannel.EndStart).Callback;
			try
			{
				_videoChannel.BeginStart(callback, null);
			}
			catch (LyncClientException lyncClientException)
			{
				Console.WriteLine(lyncClientException);
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

		/// <summary>
		/// Starts the video channel: VideoChannel.BeginStop()
		/// </summary>
		private void StopVideo()
		{
			//removes video from the conversation
			AsyncCallback callback = new AsyncOperationHandler(_videoChannel.EndStop).Callback;
			try
			{
				_videoChannel.BeginStop(callback, null);
			}
			catch (LyncClientException lyncClientException)
			{
				Console.WriteLine(lyncClientException);
			}
			catch (SystemException systemException)
			{
				if (LyncModelExceptionHelper.IsLyncException(systemException))
				{
					// Log the exception thrown by the Lync Model API.
					Console.WriteLine("Error: " + systemException);
				}
				else
				{
					// Rethrow the SystemException which did not come from the Lync Model API.
					throw;
				}
			}
		}

		#endregion


		#region Participant

		protected override void ConversationParticipantAddedInternal(Participant participant)
		{

			var partAVModality = (AVModality)participant.Modalities[ModalityTypes.AudioVideo];
			partAVModality.ActionAvailabilityChanged += OnParticipantActionAvailabilityChanged;
			partAVModality.ModalityStateChanged += OnParticipantModalityStateChanged;

			var partVideoChannel = partAVModality.VideoChannel;
			partVideoChannel.StateChanged += OnParticipantVideoChannelStateChanged;

			var displayName = (string)participant.Contact.GetContactInformation(ContactInformationType.DisplayName);


			if (participant.IsSelf)
			{

				var localpPartAVModality = (AVModality)participant.Modalities[ModalityTypes.AudioVideo];

				_localParticipantVideoModel = new ParticipantVideoModel()
				{
					Modality = localpPartAVModality
					,
					Id = participant.Contact.Uri
				};

				var partModel = new ParticipantVideoModel()
				{
					Modality = partAVModality,
					Id = participant.Contact.Uri,
					VideoChannel = partVideoChannel,
					Participant = participant,
					DisplayName = displayName,
				};


				ParticipantVideoModels.Add(partModel);

			}

			else
			{
				//var partAVModality = (AVModality)participant.Modalities[ModalityTypes.AudioVideo];
				//partAVModality.ActionAvailabilityChanged += OnParticipantActionAvailabilityChanged;
				//partAVModality.ModalityStateChanged += OnParticipantModalityStateChanged;

				//var partVideoChannel = partAVModality.VideoChannel;
				//partVideoChannel.StateChanged += OnParticipantVideoChannelStateChanged;

				var partModel = new ParticipantVideoModel()
				{
					Modality = partAVModality
					,
					Id = participant.Contact.Uri
					,
					VideoChannel = partVideoChannel
					,
					Participant = participant,
					DisplayName = displayName,
				};


				ParticipantVideoModels.Add(partModel);



				if (Type == ConversationType.Audio)
				{
					ConnectAudio();
				}
				else
				{
					StartVideo();
				}
			}

		}

		protected override void ConversationParticipantRemovedInternal(Participant participant)
		{

			var model = ParticipantVideoModels.Where(p => p.Id == participant.Contact.Uri).SingleOrDefault();
			//get the application sharing modality of the removed participant out of the class modalty dicitonary
			AVModality removedModality = model.Modality;

			//Un-register for modality events on this participant's application sharing modality.
			removedModality.ActionAvailabilityChanged -= OnParticipantActionAvailabilityChanged;
			removedModality.ModalityStateChanged -= OnParticipantModalityStateChanged;

			//Remove the modality from the dictionary.
			ParticipantVideoModels.Remove(model);
		}


		private void OnParticipantVideoChannelStateChanged(object sender, ChannelStateChangedEventArgs e)
		{
			//posts the execution into the UI thread
			RunAtUI(() =>
			{
				_log.Debug("OnParticipantVideoChannelStateChanged  OldState:{0} NewState:{1} Channel:{2} ChannelCode:{3}"
					, e.OldState.ToString()
					, e.NewState.ToString()
					, sender.ToString()
					, sender.GetHashCode()
					);

				var channel = sender as VideoChannel;

				//if the outgoing video is now active, show the video (which is only available in UI Suppression Mode)
				if ((e.NewState == ChannelState.Send
				   || e.NewState == ChannelState.SendReceive) && _videoChannel.CaptureVideoWindow != null)
				{
					//SetParticipantVideoWindow(channel, _videoChannel.CaptureVideoWindow);
				}

				//if the incoming video is now active, show the video (which is only available in UI Suppression Mode)
				if ((e.NewState == ChannelState.Receive
				   || e.NewState == ChannelState.SendReceive) && _videoChannel.RenderVideoWindow != null)
				{
					SetParticipantVideoWindow(channel, _videoChannel.RenderVideoWindow);
				}

			});
		}

		private void OnParticipantModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
		{
		}

		private void OnParticipantActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
		{
		}

		#endregion

		/// <summary>
		/// Connects the modality (audio): AvModality.BeginConnect()
		/// </summary>
		private void ConnectAudio()
		{
			//starts an audio call or conference by connecting the AvModality
			try
			{
				_log.Debug("ConnectAudio  BeginConnect");

				AsyncCallback callback = new AsyncOperationHandler(_avModality.EndConnect).Callback;
				_avModality.BeginConnect(callback, null);
			}
			catch (LyncClientException lyncClientException)
			{
				_log.ErrorException("", lyncClientException);
			}
			catch (Exception systemException)
			{
				_log.ErrorException("", systemException);
			}
		}


		#region helper

		private List<ParticipantVideoModel> _HasSetWindowParticipants = new List<ParticipantVideoModel>();

		private void SetParticipantVideoWindow(VideoChannel channel, VideoWindow window)
		{
			var model = ParticipantVideoModels.Where(p => p.IsMatch(channel)).SingleOrDefault();
			if (model != null)
			{
				if (_HasSetWindowParticipants.Contains(model))
				{
					return;
				}
				model.View = window;
				_HasSetWindowParticipants.Add(model);
			}
		}

		#endregion

	}
}
