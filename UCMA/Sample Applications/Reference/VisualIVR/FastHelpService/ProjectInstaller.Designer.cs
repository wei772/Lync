namespace FastHelpService
{
    /// <summary>
    /// Designer class for Project installer
    /// </summary>
    public partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///   Instance of FastHelpProcessInstaller.
        /// </summary>
        private System.ServiceProcess.ServiceProcessInstaller fastHelpProcessInstaller;

        /// <summary>
        /// Instance of   FastHelpInstaller.
        /// </summary>
        private System.ServiceProcess.ServiceInstaller fastHelpInstaller;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.fastHelpProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.fastHelpInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // fastHelpProcessInstaller
            // 
            this.fastHelpProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.fastHelpProcessInstaller.Password = null;
            this.fastHelpProcessInstaller.Username = null;
            // 
            // fastHelpInstaller
            // 
            this.fastHelpInstaller.Description = "A Windows Service for FastHelp UCMA BOT";
            this.fastHelpInstaller.DisplayName = "FastHelpService";
            this.fastHelpInstaller.ServiceName = "FastHelpService";
            this.fastHelpInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.fastHelpProcessInstaller,
            this.fastHelpInstaller});

        }

        #endregion
    }
}