using Lync.Enum;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lync.Model
{
	public class AudioConversation : LyncConversation
	{

		private ILog _log = LogManager.GetLog(typeof(AudioConversation));

		private string _sipUriOfRealPerson;

		private AVModality _aVModality;

		private AudioChannel _audioChannel;
		private AVModality _avModality;

		public AudioConversation()
		{
			Type = ConversationType.Audio;
		}

		public void Start(string sipUriOfRealPerson)
		{
			_sipUriOfRealPerson = sipUriOfRealPerson;
			CreateConversation();
		}


		protected override void HandleAddedCore()
		{
			//saves the AVModality, AudioChannel and VideoChannel, just for the sake of readability
			_avModality = (AVModality)Conversation.Modalities[ModalityTypes.AudioVideo];
			_audioChannel = _avModality.AudioChannel;


			//subscribes to modality action availability events (all audio button except DTMF)
			_avModality.ActionAvailabilityChanged += OnAvModalityActionAvailabilityChanged;

			//subscribes to the modality state changes so that the status bar gets updated with the new state
			_avModality.ModalityStateChanged += OnAvModalityModalityStateChanged;


			//subscribes to the audio channel action availability events (DTMF only)
			_audioChannel.ActionAvailabilityChanged += OnAudioChannelActionAvailabilityChanged;

			//subscribes to the video channel state changes so that the status bar gets updated with the new state
			_audioChannel.StateChanged += OnAudioChannelStateChanged;

			ConnectAudio();
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
						System.Threading.Thread.Sleep(2000);

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

				VideoChannel channel = (VideoChannel)result.AsyncState;
				channel.EndStart(result);
				//RaiseVideoAvailable(channel.CaptureVideoWindow, VideoDirection.Outgoing);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
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
				_log.ErrorException("",lyncClientException);
			}
			catch (SystemException systemException)
			{
				_log.ErrorException("", systemException);
			}
		}


	}
}
