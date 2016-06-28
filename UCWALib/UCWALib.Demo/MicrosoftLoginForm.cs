using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UCWALib.Demo
{
    public partial class MicrosoftLoginForm : Form
    {
        public string AccessToken { get; set; }
        public MicrosoftLoginForm()
        {
            InitializeComponent();
        }

        private void MicrosoftLoginForm_Load(object sender, EventArgs e)
        {
            
        }

        public void SetUrl(string url)
        {
            webBrowser.Navigate(url);
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            

        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var url = e.Url.ToString();
            var accessTokenIdx = url.IndexOf("#access_token=");
            if (accessTokenIdx > -1)
            {
                accessTokenIdx += "#access_token=".Length;
                var endIdx = url.IndexOf("&token_type=", accessTokenIdx);
                if (endIdx > -1)
                {
                    AccessToken = url.Substring(accessTokenIdx, endIdx - accessTokenIdx);
                    this.DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
