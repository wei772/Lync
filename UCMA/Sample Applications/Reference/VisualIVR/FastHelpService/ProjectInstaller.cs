/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpService
{
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.DirectoryServices;
    using System;
    using System.ServiceProcess;

    /// <summary>
    /// Installer for Service
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInstaller"/> class.
        /// </summary>
        public ProjectInstaller()
        {
            this.InitializeComponent();
            BeforeInstall += new InstallEventHandler(ProjectInstaller_BeforeInstall);
        }

        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            // fetches the service account credentials from the command line
            if (Context != null && Context.Parameters.ContainsKey("PASSWORD") && Context.Parameters.ContainsKey("USER"))
            {
                string[] Account = Context.Parameters["USER"].Split(new char[] { '\\' });
                if (2 == Account.Length)
                {
                    string Domain = Account[0];
                    string User = Account[1];
                    fastHelpProcessInstaller.Username = Context.Parameters["USER"];
                    fastHelpProcessInstaller.Password = Context.Parameters["PASS"];

                    AddToLocalGroup(User, Domain, "Administrators");
                }
                else throw new ArgumentException("Domain user must be in the \"DOMAIN\\USERNAME\" form.");
            }
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            var sc = new ServiceController(this.fastHelpInstaller.ServiceName);
            sc.Start();
        }

        private static bool AddToLocalGroup(string User, string Domain, string Group)
        {
            try
            {
                DirectoryEntry Computer = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry oUser = new DirectoryEntry("WinNT://" + Domain + "/" + User);
                DirectoryEntry oGroup;
                oGroup = Computer.Children.Find(Group, "group");
                if (oGroup != null && oUser != null)
                {
                    oGroup.Invoke("Add", new object[] { oUser.Path.ToString() });
                    oGroup.CommitChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
