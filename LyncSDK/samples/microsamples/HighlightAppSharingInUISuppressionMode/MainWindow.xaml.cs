using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace HighlightAppSharingInUISuppressionMode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LyncClient _client;
        Conversation _conversation;
        Contact _remoteContact;
        ApplicationSharingModality _sharingModality;
        SharingResource _sharingResource;
        Highlighter[] _appsharingHighlighters;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                _client = LyncClient.GetClient();
                _client.StateChanged += client_StateChanged;
                StatusTextBlock.Text = _client.State.ToString();
                if (_client.InSuppressedMode)
                {
                    _client.BeginInitialize((ar) =>
                    {
                        _client.EndInitialize(ar);
                    }, null);
                }
                _client.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Exception: " + ex.Message;
            }
        }

        void client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusTextBlock.Text = e.NewState.ToString();
            }), null);
        }

        void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusTextBlock.Text = "A conversation is added";
            }), null);

            if (_conversation != null && e.Conversation.GetHashCode() == _conversation.GetHashCode())
            {
                _sharingModality = (ApplicationSharingModality)_conversation.Modalities[ModalityTypes.ApplicationSharing];
                _sharingModality.ModalityStateChanged += sharingModality_ModalityStateChanged;

                _conversation.AddParticipant(_remoteContact);
            }

            // populate the ShareableListBox with ShareableResources
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusTextBlock.Text = "Populating shareable resources...";
                ShareableResourcesListBox.Items.Clear();
                for (int i = 0; i < _sharingModality.ShareableResources.Count; i++)
                {
                    SharingResource sr = _sharingModality.ShareableResources[i];
                    ShareableResourcesListBox.Items.Add(sr.Name);
                }
                ShareableResourcesListBox.SelectedIndex = 0;
                StatusTextBlock.Text = "Shareable resources are populated.";
            }), null);
        }

        void sharingModality_ModalityStateChanged(object sender, ModalityStateChangedEventArgs e)
        {
            if (e.NewState == ModalityState.Connected)
            {
                // We show all the highlighters when the application sharing modality is connected
                foreach (Highlighter hl in _appsharingHighlighters)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {

                        hl.Highlight();
                        hl.Show();
                    }), null);
                }
            }
            else if (e.NewState == ModalityState.Disconnected)
            {
                // We hide all the highlighters when the application sharing modality is connected
                foreach (Highlighter hl in _appsharingHighlighters)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        hl.Hide();
                        hl.Close();
                    }), null);
                }
            }
        }

        /// <summary>
        /// Start a conversation with the remote participant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartConversationButton_Click(object sender, RoutedEventArgs e)
        {
            _remoteContact = _client.ContactManager.GetContactByUri(RemoteParticipantUriTextBox.Text);
            _conversation = _client.ConversationManager.AddConversation();
        }

        /// <summary>
        /// Start application sharing with the remote participant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartSharingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_conversation != null && _sharingModality != null)
            {
                SharingResource sr = _sharingModality.ShareableResources[ShareableResourcesListBox.SelectedIndex];

                switch (sr.Type)
                {
                    case SharingResourceType.Desktop:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _appsharingHighlighters = new Highlighter[Screen.AllScreens.Length];
                            for (int i = 0; i < Screen.AllScreens.Length; i++)
                            {
                                _appsharingHighlighters[i] = new Highlighter(HighLighterMode.Screen, i);
                            }
                        }), null);
                        _sharingModality.BeginShareDesktop((ar) =>
                        {
                            _sharingModality.EndShareDesktop(ar);
                        }, null);
                        break;
                    case SharingResourceType.Monitor:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _appsharingHighlighters = new Highlighter[1];
                            _appsharingHighlighters[0] = new Highlighter(HighLighterMode.Screen, sr.Id - 1);
                        }), null);
                        _sharingModality.BeginShareResources(sr, (ar) =>
                        {
                            _sharingModality.EndShareResources(ar);
                        }, null);
                        break;
                    case SharingResourceType.Process:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _appsharingHighlighters = new Highlighter[1];
                            _appsharingHighlighters[0] = new Highlighter(HighLighterMode.Process, sr.Id);
                        }), null);
                        _sharingModality.BeginShareResources(sr, (ar) =>
                        {
                            _sharingModality.EndShareResources(ar);
                        }, null);
                        break;
                    case SharingResourceType.Window:
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _appsharingHighlighters = new Highlighter[1];
                            _appsharingHighlighters[0] = new Highlighter(HighLighterMode.Window, sr.Id);
                        }), null);
                        _sharingModality.BeginShareResources(sr, (ar) =>
                        {
                            _sharingModality.EndShareResources(ar);
                        }, null);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sign in the Lync client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (_client.State != ClientState.SignedIn)
            {
                _client.BeginSignIn("johns@contoso.com", "domain\\johns", "password", (ar) =>
                {
                    _client.EndSignIn(ar);
                }, null);
            }
        }

        /// <summary>
        /// Stop the application sharing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sharingModality.BeginDisconnect(ModalityDisconnectReason.None, (ar) =>
            {
                _sharingModality.EndDisconnect(ar);
            }, null);
        }
    }
}
