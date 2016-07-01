/*=====================================================================
  This file is part of the Microsoft Unified Communications Code Samples.

  Copyright (C) 2012 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/
using System;

using System.Windows;

using System.Windows.Threading;

// Used by this sample.
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.Sharing;
using Lync.Service;

namespace Lync.Model
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public class ContentSharingConversation : LyncConversation
	{

		#region private fields

		private ILog _log = LogManager.GetLog(typeof(ContentSharingConversation));

		private Conversation _conversation;
		private ContentSharingModality _contentModality;
		private ShareableContent _whiteBoardContent;
		private ShareableContent _pptContent;

		private string _title = "Share";

		private string _sharePPTFilePath = @"C:\Users\li772\Downloads\20151116  Windows 10  Adaptive UI  .pptx";

		#endregion


		private void ShareWhiteBoard()
		{

			try
			{
				_log.Debug("ShareWhiteBoard");
				if (_contentModality.CanInvoke(ModalityAction.CreateShareableWhiteboardContent))
				{
					// This way of calling will block the app until Content is created. To create a more responsive app you can
					// use an AsyncCallback Delegate to end this asynchoronous opertation. More information about this C# concept
					// can be found on MSDN
					IAsyncResult result = _contentModality.BeginCreateContent(ShareableContentType.Whiteboard, _title, null, null);
					_whiteBoardContent = _contentModality.EndCreateContent(result);
					_whiteBoardContent.StateChanged += OnWhiteBoardContentStateChanged;

					// The newly created content is only present on local computer. Lets upload it to conference
					// where other users can also see the content listed in their content bin. 
					int reason; // if can invoke returns false, you can get the reason from here.
					if (_whiteBoardContent.CanInvoke(ShareableContentAction.Upload, out reason))
					{
						_whiteBoardContent.Upload();
						// above call will start uploading the content to conference. Once the content is ready its 
						// state will change from Offline to Online thats when Present() can be called.
					}
				}
			}
			catch (ContentTitleExistException ce)
			{
				_log.Error("Title already exists. Please enter a unique title and try again.", ce);
			}
			catch (NullReferenceException ne)
			{
				_log.Error("Null Reference Exception: Did you create the conversation first? If yes, you can try restarting the sample.", ne);
			}
		}

		private void SharePPT()
		{
			try
			{
				_log.Debug("SharePPT");
				if (_contentModality.CanInvoke(ModalityAction.CreateShareablePowerPointContent))
				{
					// This way of calling will block the app until Content is created. To create a more responsive app you can
					// use an AsyncCallback Delegate to end this asynchoronous opertation. More information about this C# concept
					// can be found on MSDN
					IAsyncResult result = _contentModality.BeginCreateContentFromFile(ShareableContentType.PowerPoint, _title, _sharePPTFilePath, true, null, null);
					_pptContent = _contentModality.EndCreateContentFromFile(result);
					_pptContent.StateChanged += OnpptContentStateChanged;

					// The newly created content is only present on local computer. Lets upload it to conference
					// where other users can also see the content listed in their content bin. 
					int reason; // if can invoke returns false, you can get the reason from here.
					if (_pptContent.CanInvoke(ShareableContentAction.Upload, out reason))
					{
						_pptContent.Upload();
						// above call will start uploading the content to conference. Once the content is ready its 
						// state will change from Offline to Online thats when Present() can be called.
					}
				}
				else
				{
					_log.Debug("Can not sharePPT");
				}
			}
			catch (NullReferenceException ne)
			{
				_log.ErrorException("Null Reference Exception: Did you create the conversation first? If yes, you can try restarting the sample.", ne);
			}
			catch (Exception excep)
			{
				_log.ErrorException("", excep);
			}
		}

		private void Forward()
		{
			if (_pptContent != null)
			{
				if (_pptContent.State == ShareableContentState.Active)
				{
					PowerPointContent pptC = (PowerPointContent)_pptContent;
					pptC.GoForward();
				}
			}
		}

		private void Backward()
		{
			if (_pptContent != null)
			{
				if (_pptContent.State == ShareableContentState.Active)
				{
					PowerPointContent pptC = (PowerPointContent)_pptContent;
					pptC.GoBackward();
				}
			}
		}

		private void Disconnect()
		{
			if (_contentModality.State == ModalityState.Connected)
			{
				IAsyncResult result = _contentModality.BeginDisconnect(ModalityDisconnectReason.None, null, null);
				_contentModality.EndDisconnect(result);
			}
		}

		private void Accept()
		{
			if (_contentModality != null)
			{
				if (_contentModality.CanInvoke(ModalityAction.Accept))
				{
					_contentModality.Accept();
				}
			}
		}


		#region Lync Event Handlers.

		protected override void HandleAddedInternal()
		{
			base.HandleAddedInternal();


			if (Conversation.Modalities[ModalityTypes.ContentSharing].State == ModalityState.Notified)
			{
				// Conversation orignated with remote SIP user. Lets set the modality.
				// Accept will happen when user clicks Accept button in UI.
				_contentModality = (ContentSharingModality)_conversation.Modalities[ModalityTypes.ContentSharing];

			}

		}


		protected override void ConversationParticipantAddedInternal(Participant participant)
		{
			base.ConversationParticipantAddedInternal(participant);

			if (participant.IsSelf != true)
			{
				//if (_contentModality == null)
				//{
				//	_contentModality = (ContentSharingModality)(Conversation.Modalities[ModalityTypes.ContentSharing]);
				//}

				_contentModality = (ContentSharingModality)(Conversation.Modalities[ModalityTypes.ContentSharing]);

				if (_contentModality.CanInvoke(ModalityAction.Connect))
				{
					try
					{
						_contentModality.BeginConnect(HandleCallBacks, "Connect");
					}
					catch (Exception ec)
					{
						_log.ErrorException("", ec);
					}
				}
				else
				{
					_log.Debug("can not Connect");
				}

			}
		}

		void OnWhiteBoardContentStateChanged(object sender, ShareableContentStateChangedEventArgs e)
		{
			if ((e.OldState == ShareableContentState.Connecting) && (e.NewState == ShareableContentState.Online))
			{
				// Lets make the content visible.
				_whiteBoardContent.Present();
			}
		}

		void OnpptContentStateChanged(object sender, ShareableContentStateChangedEventArgs e)
		{
			if ((e.OldState == ShareableContentState.Connecting) && (e.NewState == ShareableContentState.Online))
			{
				// Lets make the content visible.
				_pptContent.Present();
			}
		}


		#endregion

		public void HandleCallBacks(IAsyncResult result)
		{
			string callBackFor = (string)result.AsyncState;

			switch (callBackFor)
			{

				// Content Modality
				case "CreateContent":
					break;
				case "CreateContentFromFile":
					SharePPT();
					break;
				case "Connect":
					_contentModality.EndConnect(result);
					SharePPT();
					break;

				default:
					break;
			}
		}


	}
}
