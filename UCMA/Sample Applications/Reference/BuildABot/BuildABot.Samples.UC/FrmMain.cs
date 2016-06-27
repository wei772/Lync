using System;
using System.Configuration;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using BuildABot.UC;
using BuildABotMessage = BuildABot.Core.MessageHandlers.Message;
using BuildABot.Core.MessageHandlers;

namespace BuildABot.Samples.UC
{
    public partial class FrmMain : Form
    {
        private UCBotHost ucBotHost;
        Sender sender;

        public FrmMain()
        {
            InitializeComponent();
            sender = new Sender("Home OS", null, SenderKind.System);
        }

        private void btnStartUCHost_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(StartUC));
            thread.IsBackground = true;
            btnStartUCHost.Enabled = false;
            thread.Start();
        }

        public void StartUC()
        {
      
            string applicationUserAgent = ConfigurationManager.AppSettings["applicationuseragent"];
            string applicationUrn = ConfigurationManager.AppSettings["applicationurn"]; 
            ucBotHost = new UCBotHost(applicationUserAgent, applicationUrn);         
            ucBotHost.Run();
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            try
            {

                if (ucBotHost != null)
                {
                    ucBotHost.StartConversation(txtSipUri.Text, new BuildABotMessage(txtMessage.Text, this.sender), new Reply(txtReply.Text));
                }
                else
                {
                    MessageBox.Show("Please start the UC Bot host", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (StartConversationFailedException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
