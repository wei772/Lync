/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterService
{
    partial class ContactCenterInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AcdServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            this.ACDServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // AcdServiceInstaller
            // 
            this.AcdServiceInstaller.DisplayName = "ContactCenter";
            this.AcdServiceInstaller.ServiceName = "ContactCenter";
            // 
            // ACDServiceProcessInstaller
            // 
            this.ACDServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ACDServiceProcessInstaller.Password = null;
            this.ACDServiceProcessInstaller.Username = null;
            // 
            // ACDInstallerInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.AcdServiceInstaller,
            this.ACDServiceProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller AcdServiceInstaller;
        private System.ServiceProcess.ServiceProcessInstaller ACDServiceProcessInstaller;

    }
}