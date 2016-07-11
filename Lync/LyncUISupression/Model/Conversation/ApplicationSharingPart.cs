/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

#define DEBUG

using System;
using System.Windows.Forms;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.Sharing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GalaSoft.MvvmLight.Command;

namespace Lync.Model
{

	public partial class ApplicationSharingPart : ConversationPart
	{

		#region class field declarations

		private ILog _log = LogManager.GetLog(typeof(ApplicationSharingPart));


		/// <summary>
		/// A Contact instance representing the participant selected to be granted control of a resource
		/// </summary>
		Contact _resourceControllingContact;

		/// <summary>
		/// Dictionary of all contacts selected from the multi-select enabled contact list on UI
		/// </summary>
		Dictionary<string, Contact> _selectedContacts = new Dictionary<string, Contact>();

		/// <summary>
		/// Collection of all participants application sharing modalities, keyed by Contact.Uri.
		/// See _controllingContact class field...
		/// </summary>
		Dictionary<string, ApplicationSharingModality> _participantSharingModalities = new Dictionary<string, ApplicationSharingModality>();

		/// <summary>
		/// The Application sharing modality of the conversation itself
		/// </summary>
		ApplicationSharingModality _sharingModality;

		/// <summary>
		/// The Application sharing modality of the local participant.
		/// </summary>
		ApplicationSharingModality _LocalParticipantSharingModality;

		private ApplicationSharingView _sharingView;
		public ApplicationSharingView SharingView
		{
			get
			{
				return _sharingView;
			}
			set
			{
				Set("SharingView", ref _sharingView, value);
				RaisePropertyChanged("SharingModality");
			}
		}


		public ApplicationSharingModality SharingModality
		{
			get
			{
				return _sharingModality;
			}
			set
			{
				Set("SharingModality", ref _sharingModality, value);
			}
		}


		private SharingResourceType _currentSharingResourceType;

		private bool _isSharingResource;

		public bool IsSharingResource
		{
			get
			{
				return _isSharingResource;
			}
			set
			{
				Set("IsSharingResource", ref _isSharingResource, value);
			}
		}


		private bool _canStartSharingDesktop;

		public bool CanStartSharingDesktop
		{
			get
			{
				return _canStartSharingDesktop;
			}
			set
			{
				Set("CanStartSharingDesktop", ref _canStartSharingDesktop, value);
			}
		}

		private RelayCommand _startSharingDesktopCommand;

		public RelayCommand StartSharingDesktopCommand
		{
			get
			{
				return _startSharingDesktopCommand ??
					(_startSharingDesktopCommand =
						new RelayCommand(
							() =>
								{
									ShareSelectedResource(SharingResourceType.Desktop);
								}
							)
					);
			}

		}


		#endregion

		public ApplicationSharingPart()
		{

		}



		internal override void CloseInternal()
		{

			if (_sharingModality != null)
			{
				//Unregister for events on the terminated conversation's sharing modality events.
				_sharingModality.ModalityStateChanged -= OnSharingModalityModalityStateChanged;
				_sharingModality.ControlRequestReceived -= OnSharingModalityControlRequestReceived;
				_sharingModality.LocalSharedResourcesChanged -= OnSharingModalityLocalSharedResourcesChanged;
				_sharingModality.ControllerChanged -= OnSharingModalityControllerChanged;
				_sharingModality.ActionAvailabilityChanged -= OnSharingModalityActionAvailabilityChanged;

			}

			//Unregister for the application sharing events on each and every participant in the terminated 
			//conversation
			foreach (string contactUri in _selectedContacts.Keys)
			{
				if (_participantSharingModalities.ContainsKey(contactUri))
				{
					ApplicationSharingModality participantSharingModality = (ApplicationSharingModality)_participantSharingModalities[contactUri];
					participantSharingModality.ActionAvailabilityChanged -= OnSharingModalityActionAvailabilityChanged;
				}
			}

			_resourceControllingContact = null;
			_sharingModality = null;
			_participantSharingModalities.Clear();

		}



		internal override void HandleAddedInternal()
		{

			_log.Debug("HandleAddedInternal");

			//Register for the application sharing modality event on the conversation itself
			_sharingModality = (ApplicationSharingModality)Conversation.Modalities[ModalityTypes.ApplicationSharing];

			if (_sharingModality.ShareableResources == null || _sharingModality.ShareableResources.Count == 0)
			{
				_log.Debug("_sharingModality.ShareableResources is null");
			}
			else
			{
				_log.Debug("_sharingModality.ShareableResources is not null");
			}

			//Register for state changes like connecting->connected
			_sharingModality.ModalityStateChanged += OnSharingModalityModalityStateChanged;

			//Register to catch requests from other participants for control of the locally owned sharing resource.
			_sharingModality.ControlRequestReceived += OnSharingModalityControlRequestReceived;
			_sharingModality.ControllerChanged += OnSharingModalityControllerChanged;


			//Register to catch changes in the list of local sharable resources such as a process that starts up or terminates.
			_sharingModality.LocalSharedResourcesChanged += OnSharingModalityLocalSharedResourcesChanged;

			//Register for changes in the availbility of resource controlling actions such as grant and revoke.
			_sharingModality.ActionAvailabilityChanged += OnSharingModalityActionAvailabilityChanged;

			//Register for changes in the local participant's mode of sharing participation
			// such as Viewing->Sharing, Requesting Control->Controlling.
			_sharingModality.ParticipationStateChanged += OnSharingModalityParticipationStateChanged;


			if (((Modality)Conversation.Modalities[ModalityTypes.InstantMessage]).CanInvoke(ModalityAction.SendInstantMessage))
			{
				((InstantMessageModality)Conversation.Modalities[ModalityTypes.InstantMessage]).BeginSendMessage("hi", (ar) =>
				{
					if (((InstantMessageModality)Conversation.Modalities[ModalityTypes.InstantMessage]).CanInvoke(ModalityAction.SendInstantMessage))
					{
						((InstantMessageModality)Conversation.Modalities[ModalityTypes.InstantMessage]).EndSendMessage(ar);
					}
				}, null);
			}

			base.HandleAddedInternal();
		}




		#region Conversation event handlers


		/// <summary>
		/// Handles the participant added event. Registers for events on the application sharing modality for the 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal override void ConversationParticipantAddedInternal(Participant participant)
		{
			base.ConversationParticipantAddedInternal(participant);

			ApplicationSharingModality participantSharingModality = (ApplicationSharingModality)participant.Modalities[ModalityTypes.ApplicationSharing];
			_participantSharingModalities.Add(participant.Contact.Uri, participantSharingModality);

			//register for important events on the application sharing modality of the new participant.
			participantSharingModality.ActionAvailabilityChanged += OnParticipantSharingModalityActionAvailabilityChanged;
			participantSharingModality.ModalityStateChanged += OnParticipantSharingModalityModalityStateChanged;


			try
			{

				//Is this added participant the local user?
				if (participant.IsSelf)
				{
					//Store the application sharing modality of the local user so that
					//the user can request or release control of a remotely owned and shared resource.
					_LocalParticipantSharingModality = (ApplicationSharingModality)participant.Modalities[ModalityTypes.ApplicationSharing];

					//Enable or disable the Start Resource Sharing button according to the role of the local participant.
					//Roles can be Presenter or Attendee.

					if (participant.Properties[ParticipantProperty.IsPresenter] != null)
					{

					}
					//Register for the particpant property changed event to be notified when the role of the local user changes.
					participant.PropertyChanged += OnParticipantPropertyChanged;


				}
				else
				{

				}



			}
			catch (ArgumentException ae)
			{
				_log.ErrorException("argument exception: ", ae);
			}

		}



		/// <summary>
		/// Handles the event raised when a particpant is removed from the conversation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal override void ConversationParticipantRemovedInternal(Participant participant)
		{

			//get the application sharing modality of the removed participant out of the class modalty dicitonary
			ApplicationSharingModality removedModality = _participantSharingModalities[participant.Contact.Uri];

			//Un-register for modality events on this participant's application sharing modality.
			removedModality.ActionAvailabilityChanged -= OnSharingModalityActionAvailabilityChanged;

			//Remove the modality from the dictionary.
			_participantSharingModalities.Remove(participant.Contact.Uri);
		}
		#endregion

		#region Participant event handlers
		/// <summary>
		/// Called when a participant property is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnParticipantPropertyChanged(object sender, ParticipantPropertyChangedEventArgs e)
		{
			if (e.Property == ParticipantProperty.IsPresenter)
			{
				//Enable or disable the Start Sharing Resource button according to the participant role
				//  this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { StartSharingResource_Button, (Boolean)e.Value });

			}
		}

		#endregion

		#region application sharing modality event handlers


		void OnSharingModalityLocalSharedResourcesChanged(object sender, LocalSharedResourcesChangedEventArgs e)
		{
			_log.Debug("OnSharingModalityLocalSharedResourcesChanged");

			//Update the shareable resources list box with the currently available shareable resources.
			if (e.ResourceList.Count > 0)
			{
			}
			else
			{
			}
		}


		private void OnParticipantSharingModalityModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
		{
			_log.Debug("OnSharingModalityModalityStateChanged  ModalityState:{0}", e.NewState.ToString());

			//Modality will be connected for each particpant whethere they have accepted the sharing invite or not.
			RunAtUI(() =>
			{
				var thisModality = sender as ApplicationSharingModality;

				if (thisModality.View != null)
				{
					SharingView = thisModality.View;
					//   this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Hide sharing stage" });
				}

				if (e.NewState == ModalityState.Connected)
				{

					//ShowStage_Button
					//If the local user is not resource sharer, then dock the view to see
					//the resource shared by a remote user
					if (thisModality.View != null)
					{
						SharingView = thisModality.View;
						//   this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Hide sharing stage" });
					}
					else
					{
						//    this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Stop sharing" });
					}

				}
				if (e.NewState == ModalityState.Disconnected)
				{
					if (thisModality == Conversation.Modalities[ModalityTypes.ApplicationSharing])
					{
					}

				}

			});
		}

		private void OnParticipantSharingModalityActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
		{
			try
			{
				_log.Debug("OnParticipantSharingModalityActionAvailabilityChanged");

				ApplicationSharingModality thisModality = (ApplicationSharingModality)sender;
				Button buttonToUpdate = null;

				//Enable or disable a UI action button that corresponds to the action whose availability has changed.
				switch (e.Action)
				{
					case ModalityAction.Accept:
						//  buttonToUpdate = AcceptSharing_Button;
						break;
					case ModalityAction.Reject:
						//  buttonToUpdate = RejectSharing_Button;
						break;
					case ModalityAction.AcceptSharingControlRequest:
						//  buttonToUpdate = Accept_Button;
						break;
					case ModalityAction.DeclineSharingControlRequest:
						//   buttonToUpdate = Decline_Button;
						break;
					case ModalityAction.GrantSharingControl:
						//   buttonToUpdate = Grant_Button;
						break;
					case ModalityAction.ReleaseSharingControl:
						//  buttonToUpdate = Release_Button;
						break;
					case ModalityAction.RequestSharingControl:
						//    buttonToUpdate = Request_Button;
						break;
					case ModalityAction.RevokeSharingControl:
						//     buttonToUpdate = Revoke_Button;
						break;
					case ModalityAction.Disconnect:
						//     buttonToUpdate = Disconnect_Button;
						break;
				}

				//Not all possible cases of ActionAvailability are represented in the previous switch statement. 
				//For this reason, buttonToUpdate may be null.
				if (buttonToUpdate != null)
				{
					//   this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { buttonToUpdate, e.IsAvailable });
				}
			}
			catch (Exception) { }
		}

		void OnSharingModalityModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
		{
			_log.Debug("OnSharingModalityModalityStateChanged  ModalityState:{0}", e.NewState.ToString());

			//Modality will be connected for each particpant whethere they have accepted the sharing invite or not.
			RunAtUI(() =>
			{
				ApplicationSharingModality thisModality = sender as ApplicationSharingModality;
				if (e.NewState == ModalityState.Connected)
				{

					//ShowStage_Button
					//If the local user is not resource sharer, then dock the view to see
					//the resource shared by a remote user
					if (thisModality.View != null)
					{
						SharingView = thisModality.View;
						//   this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Hide sharing stage" });
					}
					else
					{
						//    this.Invoke(new ChangeButtonTextDelegate(ChangeButtonText), new object[] { Disconnect_Button, "Stop sharing" });
					}

				}
				if (e.NewState == ModalityState.Disconnected)
				{
					if (thisModality == Conversation.Modalities[ModalityTypes.ApplicationSharing])
					{
					}

				}

			});
		}

		/// <summary>
		/// Handles the event raised when the participant that is controlling the shared conversation application resource
		/// changes. This event is raised locally even when the shared resource is not locally owned.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSharingModalityControllerChanged(object sender, ControllerChangedEventArgs e)
		{

			_log.Debug("OnSharingModalityControllerChanged");
			//Store the Contact object for the conversation participant that now controls the shared conversation resource.
			if (((ApplicationSharingModality)sender).Controller != null)
			{
				_resourceControllingContact = ((ApplicationSharingModality)sender).Controller.Contact;
			}

		}

		/// <summary>
		/// Handles the event raised when a conversation participant requests control of a locally owned resource.
		/// These requests always go to the resource owner, and not the current controller of the resource.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSharingModalityControlRequestReceived(object sender, ControlRequestReceivedEventArgs e)
		{
			_log.Debug("OnSharingModalityControlRequestReceived");

			//Get the name of the participant that is requesting control of the locally owned resource.
			string displayRequesterName = e.Requester.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();


			//Store the Contact object for the requesting participant.
			_resourceControllingContact = e.Requester.Contact;
		}

		/// <summary>
		/// Handles the event raised when a participant participation state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSharingModalityParticipationStateChanged(object sender, ParticipationStateChangedEventArgs e)
		{
			_log.Debug("OnSharingModalityParticipationStateChanged");

			if (((ApplicationSharingModality)sender) == _sharingModality)
			{
			}
			ApplicationSharingModality participantModality = (ApplicationSharingModality)sender;
			if (participantModality.Controller != null)
			{
				string userName = participantModality.Controller.Contact.GetContactInformation(ContactInformationType.DisplayName).ToString();
			}
		}


		/// <summary>
		/// Event handler for sharing modality action availability change
		/// This method enables or disables the modality control action buttons on the UI according to
		/// the availability of a given action.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSharingModalityActionAvailabilityChanged(object sender, ModalityActionAvailabilityChangedEventArgs e)
		{
			try
			{
				_log.Debug("OnSharingModalityActionAvailabilityChanged");

				ApplicationSharingModality thisModality = (ApplicationSharingModality)sender;
				Button buttonToUpdate = null;

				//Enable or disable a UI action button that corresponds to the action whose availability has changed.
				switch (e.Action)
				{
					case ModalityAction.Accept:
						//  buttonToUpdate = AcceptSharing_Button;
						break;
					case ModalityAction.Reject:
						//  buttonToUpdate = RejectSharing_Button;
						break;
					case ModalityAction.AcceptSharingControlRequest:
						//  buttonToUpdate = Accept_Button;
						break;
					case ModalityAction.DeclineSharingControlRequest:
						//   buttonToUpdate = Decline_Button;
						break;
					case ModalityAction.GrantSharingControl:
						//   buttonToUpdate = Grant_Button;
						break;
					case ModalityAction.ReleaseSharingControl:
						//  buttonToUpdate = Release_Button;
						break;
					case ModalityAction.RequestSharingControl:
						//    buttonToUpdate = Request_Button;
						break;
					case ModalityAction.RevokeSharingControl:
						//     buttonToUpdate = Revoke_Button;
						break;
					case ModalityAction.Disconnect:
						//     buttonToUpdate = Disconnect_Button;
						break;
				}

				//Not all possible cases of ActionAvailability are represented in the previous switch statement. 
				//For this reason, buttonToUpdate may be null.
				if (buttonToUpdate != null)
				{
					//   this.Invoke(new EnableDisableButtonDelegate(EnableDisableButton), new object[] { buttonToUpdate, e.IsAvailable });
				}
			}
			catch (Exception) { }
		}
		#endregion


		#region Operation


		/// <summary>
		///Accept another participants request to control locally owned and shared resource.
		///The Accept button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
		///with the event argument that specifies the ModalityAction.AcceptSharingControlRequest action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Accept()
		{

			//_selectedContact is set to the Contact object of the participant who requested control of the resource. 
			//see the _sharingModality_ControlRequestReceived method in the application sharing modality event handlers region.
			ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_resourceControllingContact.Uri];

			//If the requesting participant application sharing modality is available and the AcceptSharingControlRequest action can be invoked
			if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.AcceptSharingControlRequest))
			{
				//Accept sharing control request.
				sharingModality.BeginAcceptControlRequest((ar) => { sharingModality.EndAcceptControlRequest(ar); }, null);
			}
		}

		/// <summary>
		///Decline another participants request to control locally owned and shared resource.
		///The Decline button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
		///called with the event argument that specifies the ModalityAction.DeclineSharingControlRequest action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Decline()
		{
			//_selectedContact is set to the Contact object of the participant who requested control of the resource. 
			//see the _sharingModality_ControlRequestReceived method in the application sharing modality event handlers region.
			ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_resourceControllingContact.Uri];
			if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.DeclineSharingControlRequest))
			{
				sharingModality.BeginDeclineControlRequest((ar) => { sharingModality.EndDeclineControlRequest(ar); }, null);
			}

		}

		/// <summary>
		///Grant another participant control of a locally owned and shared resource.
		///The Grant button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
		///called with the event argument that specifies the ModalityAction.GrantSharingControl action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Grant()
		{

			//Get the sharing modality of the participant which the local user has selected to control the locally owned resource.
			try
			{
				ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities["TODO"];

				//If the application sharing modality is available and the resource can still be granted then grant
				//control of the resource.
				if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.GrantSharingControl))
				{
					sharingModality.BeginGrantControl((ar) => { sharingModality.EndGrantControl(ar); }
						, null);
				}
			}
			catch (KeyNotFoundException keyExp)
			{
				_log.ErrorException("Chosen participant does not have an application sharing modality.", keyExp);
			}
			catch (NullReferenceException exp)
			{
				_log.ErrorException("Chosen participant does not have an application sharing modality.", exp);

			}

		}

		/// <summary>
		///Release control of a remotely owned resource and shared resource.
		///The Release button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is
		///called with the event argument that specifies the ModalityAction.ReleaseSharingControl action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Release()
		{
			if (_LocalParticipantSharingModality != null && _LocalParticipantSharingModality.CanInvoke(ModalityAction.ReleaseSharingControl))
			{
				_LocalParticipantSharingModality.BeginReleaseControl
					(
						(ar) => { _LocalParticipantSharingModality.EndReleaseControl(ar); }
						, null
					);
			}
		}

		/// <summary>
		///Request control of a remotely owned resource and shared resource.
		///The Request button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
		///with the event argument that specifies the ModalityAction.RequestSharingControl action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Request()
		{
			if (_LocalParticipantSharingModality != null && _LocalParticipantSharingModality.CanInvoke(ModalityAction.RequestSharingControl))
			{
				_LocalParticipantSharingModality.BeginRequestControl((ar) =>
				{
					try
					{
						_LocalParticipantSharingModality.EndRequestControl(ar);
					}
					catch (LyncClientException lce)
					{
						_log.ErrorException("Lync client exception on request control: ", lce);
					}
				}
				, null);
			}
		}

		/// <summary>
		///Revoke control of a remotely controlled resource and locally owned shared resource.
		///The Revoke button is enabled when the _sharingModality_ActionAvailabilityChanged event handler is called
		///with the event argument that specifies the ModalityAction.RevokeSharingControl action is available.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Revoke()
		{
			if (_resourceControllingContact == null)
			{
				return;
			}
			ApplicationSharingModality sharingModality = (ApplicationSharingModality)_participantSharingModalities[_resourceControllingContact.Uri];
			if (sharingModality != null && sharingModality.CanInvoke(ModalityAction.RevokeSharingControl))
			{
				sharingModality.BeginRevokeControl((ar) =>
				{
					try
					{
						sharingModality.EndRevokeControl(ar);
					}
					catch (OperationException oe)
					{
						_log.ErrorException("Operation exception ", oe);
					}
				}
				, null);
			}

		}




		/// <summary>
		/// Disconnects the conversation application sharing modality so that the user
		/// is no longer sharing or viewing a resource.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StopSharing()
		{
			if (_sharingModality != null && _sharingModality.CanInvoke(ModalityAction.Disconnect))
			{
				_sharingModality.BeginDisconnect(ModalityDisconnectReason.None, (ar) =>
				{
					_sharingModality.EndDisconnect(ar);
				}, null);

			}
		}


		/// <summary>
		/// Accepts an invitation to connect to a resource sharing modality
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AcceptSharing()
		{
			if (_sharingModality != null)
			{
				//Register for the application sharing modality event on the conversation itself
				_sharingModality = (ApplicationSharingModality)Conversation.Modalities[ModalityTypes.ApplicationSharing];
			}
			_sharingModality.Accept();

		}

		/// <summary>
		/// Rejects an invitiation to connect to a resource sharing modality
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RejectSharing()
		{
			_sharingModality.Reject(ModalityDisconnectReason.Decline);
		}



		#endregion


		#region API Operation callback methods


		/// <summary>
		/// Completes the desktop sharing operation.
		/// </summary>
		/// <param name="ar"></param>
		private void ShareDesktopCallback(System.IAsyncResult ar)
		{
			try
			{
				ApplicationSharingModality sharingModality = (ApplicationSharingModality)ar.AsyncState;
				sharingModality.EndShareDesktop(ar);
			}
			catch (LyncClientException) { }
			catch (InvalidCastException) { };
		}

		/// <summary>
		/// Completes the process sharing operation
		/// </summary>
		/// <param name="ar"></param>
		private void ShareResourcesCallback(System.IAsyncResult ar)
		{
			try
			{
				((ApplicationSharingModality)ar.AsyncState).EndShareResources(ar);
			}
			catch (OperationException) { }
			catch (LyncClientException) { }
			catch (InvalidCastException) { };

		}

		/// <summary>
		/// Completes the monitor sharing operation
		/// </summary>
		/// <param name="ar"></param>
		private void ShareMonitorCallback(System.IAsyncResult ar)
		{
			try
			{
				ApplicationSharingModality sharingModality = (ApplicationSharingModality)ar.AsyncState;
			}
			catch (InvalidCastException) { }
			catch (LyncClientException) { }

		}


		#endregion


		#region UI update helper methods and their delegates

		/// <summary>
		/// Shares the resource selected by the user.
		/// </summary>
		private void ShareSelectedResource(SharingResourceType selectedResourceType)
		{

			//If there is no active conversation to share this resource in, return from handler
			if (Conversation == null)
			{
				return;
			}
			_log.Debug("StartSharingResource");

			//If there is no sharing modality stored locally on the active conversation, get it from the active conversation and store it.
			if (_sharingModality == null)
			{
				_sharingModality = Conversation.Modalities[ModalityTypes.ApplicationSharing] as ApplicationSharingModality;
			}



			SharingResource sharingResource = null;

			if (_sharingModality.ShareableResources == null)
			{
				_log.Debug("ShareableResources is null");
				return;
			}

			//foreach (SharingResource s in _sharingModality.ShareableResources)
			//{
			//    //if (s.Id == selectedResource.ResourceId)
			//    //{
			//    //    //Get the type of resource selected by the user
			//    //    selectedResourceType = s.Type;
			//    //    sharingResource = s;
			//    //    break;
			//    //}
			//}

			CanShareDetail sharingDetail;
			if (!_sharingModality.CanShare(selectedResourceType, out sharingDetail))
			{
				_log.Debug("sharingDetail:{0}", sharingDetail);
				switch (sharingDetail)
				{
					case CanShareDetail.DisabledByOrganizerPolicy:
						MessageBox.Show("The conversation organizer has disallowed sharing");
						break;
					case CanShareDetail.DisabledByPolicy:
						MessageBox.Show("Sharing resources is not allowed ");
						break;
					case CanShareDetail.DisabledByRole:
						MessageBox.Show("Conference attendees cannot share resources");
						break;
				}
				return;
			}

			_log.Debug("selectedResourceType: {0}", selectedResourceType.ToString());

			if (selectedResourceType == SharingResourceType.Desktop)
			{
				_sharingModality.BeginShareDesktop((ar) =>
				{
					_sharingModality.EndShareDesktop(ar);
				}
					, null);

			}
			else if (selectedResourceType == SharingResourceType.Monitor)
			{
				_sharingModality.BeginShareResources(sharingResource, (ar) =>
				{
					_sharingModality.EndShareResources(ar);
				}, null);

			}
			else
			{
				_sharingModality.BeginShareResources(sharingResource, (ar) =>
				{
					try
					{
						_sharingModality.EndShareResources(ar);

					}
					catch (OperationException oe) { throw oe; }
					catch (LyncClientException lce) { throw lce; }
				}
				, null);

			}
		}

		private delegate void NoParamDelegate();
		private delegate void TwoIntParamDelegate(int height, int width);
		private delegate void EnableDisableButtonDelegate(Button buttonToUpdate, Boolean newButtonEnableState);

		/// <summary>
		/// Enables or disables a UI button based on the actionAvailability
		/// </summary>
		/// <param name="buttonToUpdate"></param>
		/// <param name="newButtonEnableState"></param>
		private void EnableDisableButton(Button buttonToUpdate, Boolean newButtonEnableState)
		{
			buttonToUpdate.Enabled = newButtonEnableState;
		}


		private delegate void ChangeButtonTextDelegate(Button buttonToChange, string newText);
		private void ChangeButtonText(Button buttonToChange, string newText)
		{
			buttonToChange.Text = newText;
		}


		private delegate void ChangeLabelTextDelegate(Label labelToUpdate, string newText);
		/// <summary>
		/// Replaces the text of any label control on the UI with new text
		/// </summary>
		/// <param name="labelToUpdate"></param>
		/// <param name="newText"></param>
		private void ChangeLabelText(Label labelToUpdate, string newText)
		{
			labelToUpdate.Text = newText;
		}

		#endregion

	}
}
