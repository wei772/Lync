using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UCWALib.Demo
{
    public partial class MainForm : Form
    {
        private SkypeClient client = new SkypeClient();
        public MainForm()
        {
            InitializeComponent();
        }

        private void UpdateStatus(string status)
        {
            lblStatus.Text = status;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            UpdateStatus("Initializing...");
            if (await client.StartUp())
            {
                UpdateStatus("Ready");
                linkToken.Visible = true;
                linkToken.Click += (a, b) =>
                {
                    using (var form = new MicrosoftLoginForm())
                    {
                        form.SetUrl(client.GetTokenUri("535f4d29-3bc7-4fe1-be19-d12a45004aca", "http://localhost/skypetest.html", chkAdmin.Checked));
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            txtToken.Text = form.AccessToken;
                        }
                    }
                    //Process.Start(client.GetTokenUri("535f4d29-3bc7-4fe1-be19-d12a45004aca", "http://localhost/skypetest.html"));
                };
                btnStartMeeting.Enabled = true;
            }
        }

        private async void btnStartMeeting_Click(object sender, EventArgs e)
        {
            // Sign In
            try
            {
                UpdateStatus("Sigining In...");
                if (!await client.SignIn(txtToken.Text.Trim()))
                {
                    throw new Exception("Unkown Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to Sign In with current token. Please update token and try it again.\n\nInternal Error:" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get Meeting URI
            try
            {
                UpdateStatus("Startting Meeting...");
                var meeting = await client.StartMeeting(txtSubject.Text.Trim());
                txtMeetingURI.Text = meeting.OnlineMeetingUri;
                linkMeeting.Text = meeting.JoinUrl;
                UpdateStatus("Meeting Started.");
                //await client.AddMessaging(meeting);
                await client.TerminateMeeting(meeting);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to Start Meeting.\n\nInternal Error:" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkMeeting_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkMeeting.Text);
        }

        private async void linkToken2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                txtToken.Text = await client.GetAccessToken("tonyxia@o365ms.com", "Abcd1234!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Failed to Get AccessToken.\n\nInternal Error:" +
                    ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}
