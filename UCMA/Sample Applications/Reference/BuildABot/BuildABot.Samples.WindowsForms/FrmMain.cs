namespace BuildABot.Samples.WindowsForms
{
    using System;
    using System.Windows.Forms;
    using BuildABot.Core;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// Main form that hosts a sample bot.
    /// </summary>
    public partial class FrmMain : Form
    {
        private Bot bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmMain"/> class.
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();
            bot = new Bot();
            bot.Replied += new ReplyEventHandler(bot_Replied);
            bot.FailedToUnderstand += new MessageEventHandler(bot_FailedToUnderstand);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            AddToOutput("User: " + txtInput.Text);
            bot.ProcessMessage(txtInput.Text);
            txtInput.Text = string.Empty;
            txtInput.Focus();
        }

        void bot_Replied(object sender, ReplyEventArgs e)
        {
            foreach (ReplyMessage replyMessage in e.Reply.Messages)
            {
                AddToOutput("Bot: " + replyMessage.Content);
            }
        }

        void bot_FailedToUnderstand(object sender, MessageEventArgs e)
        {
            AddToOutput("Bot: sorry, I didn't get you");
        }

        private void AddToOutput(string message)
        {
            txtBotOutput.AppendText(message);
            txtBotOutput.AppendText(Environment.NewLine);
        }
    }
}
