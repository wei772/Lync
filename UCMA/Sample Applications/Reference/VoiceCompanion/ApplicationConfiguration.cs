/*=====================================================================
  File:      ApplicationConfiguration.cs

  Summary:   Abstracts application specific configuration.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    #region MainMenuConfiguration classes

    public class MainMenuConfiguration
    {
        public MainMenuConfiguration(IList<MainMenuOption> options)
        {
            m_mainMenuOptions = new ReadOnlyCollection<MainMenuOption>(options);
        }
        
        private ReadOnlyCollection<MainMenuOption> m_mainMenuOptions;

        public ReadOnlyCollection<MainMenuOption> Options
        {
            get
            {
                return m_mainMenuOptions;
            }
        }
    }

    public class MainMenuOption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dtmf")]
        public MainMenuOption(
            int index,
            string dtmfCode,
            string serviceId,
            string prompt)
        {
            this.Index = index;
            this.DtmfCode = dtmfCode;
            this.ServiceId = serviceId;
            this.Prompt = prompt;
        }

        public int Index { get; private set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtmf")]
        public string DtmfCode { get; private set; }

        public string ServiceId { get; private set; }
        public string Prompt { get; private set; }
    }

    #endregion 

    #region GetBuddyConfiguration 

    public class GetBuddyConfiguration
    {
        public VoiceQuestionAnswerConfiguration GetContactQa
        {
            get;
            set;
        }

        public VoiceStatementConfiguration NoContactsStatement
        {
            get;
            set;
        }
    }

    #endregion

    #region CallbackGreetingConfiguration

    public class CallbackGreetingConfiguration
    {
        public VoiceStatementConfiguration GreetingStatement
        {
            get;
            set;
        }

        public VoiceStatementConfiguration CannotReachUser
        {
            get;
            set;
        }

        public VoiceStatementConfiguration UserDeclined
        {
            get;
            set;
        }
    }

    #endregion

    #region SetupCallbackConfiguration

    public class SetupCallbackConfiguration
    {
        public VoiceStatementConfiguration ConnectingToUserStatement
        {
            get;
            set;
        }

        public VoiceQuestionAnswerConfiguration CallbackQa
        {
            get;
            set;
        }

        public VoiceStatementConfiguration SetCallbackSucceededStatement
        {
            get;
            set;
        }

        public VoiceStatementConfiguration SetCallbackFailedStatement
        {
            get;
            set;
        }

        public AvailabilityPrompts AvailabilityPrompts
        {
            get;
            set;
        }
    }


    #endregion

    #region DialupConfiguration

    public class DialupConfiguration
    {
        public VoiceQuestionAnswerConfiguration GetNumberQA { get; set; }

        public VoiceStatementConfiguration NumberConfirmationStatement { get; set; }

        public VoiceStatementConfiguration UserDeclinedStatement { get; set; }

        public VoiceStatementConfiguration DialupFailedStatement { get; set; }
    }

    #endregion

    #region Voice Dialog configuration classes

    public class VoiceStatementConfiguration
    {
        public string MainPrompt
        {
            get;
            internal set;
        }
    }

    public class VoiceQuestionAnswerConfiguration
    {
        public string MainPrompt
        {
            get;
            internal set;
        }

        public string NoRecognitionPrompt
        {
            get;
            internal set;
        }
    }

    public class VoiceServiceInformation
    {
        public VoiceServiceInformation(
            string id,
            string type,
            string assembly)
        {
            this.Id = id;
            this.VoiceServiceType = type;
            this.Assembly = assembly;
        }
        
        public string Id { get; private set; }

        public string VoiceServiceType { get; private set; }

        public string Assembly { get; private set; }
    }

    #endregion

    #region AvailabilityPrompts

    public class AvailabilityPrompts
    {
        public string OnlinePrompt { get; set; }
        public string AwayPrompt { get; set; }
        public string OfflinePrompt { get; set; }
        public string BeRightBackPrompt { get; set; }
        public string BusyPrompt { get; set; }
        public string DoNotDisturb { get; set; }
        public string OtherPrompt { get; set; }
    }

    #endregion

    #region MusicOnHoldConfiguration

    public class MusicOnHoldConfiguration
    {
        public string FilePath { get; set; }
    }

    #endregion

    #region ConferenceServiceConfiguration

    public class ConferenceServiceConfiguration
    {
        public VoiceStatementConfiguration InstructionsStatement { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtmf")]
        public DtmfMenuConfiguration DtmfMenuConfiguration { get; set; }

        public VoiceStatementConfiguration InviteUserStatement { get; set; }

        public VoiceStatementConfiguration HelpStatement { get; set; }
    }

    #endregion

    #region AuthenticationConfiguration

    public class AuthenticationConfiguration
    {
        public VoiceStatementConfiguration WelcomeStatement { get; set; }

        public VoiceQuestionAnswerConfiguration GetPinQa { get; set; }

        public VoiceStatementConfiguration InvalidPinStatement { get; set; }

        public VoiceStatementConfiguration PinValidatedStatement { get; set; }

        public VoiceStatementConfiguration DisconnectStatement { get; set; }
    }

    #endregion

    #region DtmfMenuConfiguration

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dtmf")]
    public class DtmfMenuConfiguration
    {
        public Dictionary<string, string> Menu
        {
            get;
            set;
        }

    }

    #endregion

    #region Application configuration

    internal static class ApplicationConfiguration
    {
        #region Private fields

        private static Logger s_logger;
        private static string s_mainMenuFileConfigFile;
        private static string s_voiceServicesConfigFile;
        private static string s_getBuddyConfigFile;
        private static string s_mohFilePath;
        private static string s_setupCallbackConfigFile;
        private static string s_callbackGreetingConfigFile;
        private static string s_authenticationConfigFile;
        private static string s_dialupConfigFile;
        private static string s_conferenceServiceConfigFile;
        private static string s_rnlFile;

        private static Dictionary<string, VoiceServiceInformation> s_voiceServices;
        private static MainMenuConfiguration s_mainMenuConfiguration;
        private static GetBuddyConfiguration s_getBuddyConfig;
        private static MusicOnHoldConfiguration s_mohConfiguration;
        private static SetupCallbackConfiguration s_setupCallbackConfig;
        private static CallbackGreetingConfiguration s_callbackGreetingConfig;
        private static DialupConfiguration s_dialupConfig;
        private static ConferenceServiceConfiguration s_conferenceServiceConfig;
        private static AuthenticationConfiguration s_authenticatioConfig;

        #endregion

        public static GetBuddyConfiguration GetBuddyConfiguration()
        {
            if (s_getBuddyConfig == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }

            return s_getBuddyConfig;
        }

        public static MusicOnHoldConfiguration GetMusicOnHoldConfiguration()
        {
            if (s_mohConfiguration == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }
                     
            return s_mohConfiguration;
        }

        public static string RnlFile
        {
            get
            {
                return s_rnlFile;
            }
        }

        public static MainMenuConfiguration GetMainMenuConfiguration()
        {
            if (s_mainMenuConfiguration == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }
            return s_mainMenuConfiguration;
        }

        public static CallbackGreetingConfiguration GetCallbackGreetingConfiguration()
        {
            if (s_callbackGreetingConfig == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }

            return s_callbackGreetingConfig;
        }

        public static AuthenticationConfiguration GetAuthenticationConfiguration()
        {
            if (s_authenticatioConfig == null)
            {
                throw new InvalidOperationException("Configuration file has not been loaded yet");                
            }

            return s_authenticatioConfig;
        }

        public static ConferenceServiceConfiguration GetConferenceServiceConfiguration()
        {
            if (s_conferenceServiceConfig == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }

            return s_conferenceServiceConfig;
        }


        public static SetupCallbackConfiguration GetSetupCallbackConfiguration()
        {
            if (s_setupCallbackConfig == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }

            return s_setupCallbackConfig;
        }

        public static DialupConfiguration GetDialupConfiguration()
        {
            if (s_dialupConfig == null)
            {
                throw new InvalidOperationException("Configuration has not been loaded yet");
            }

            return s_dialupConfig;
        }

        public static bool LoadConfiguration()
        {

            try
            {
                s_logger = new Logger();
                s_mainMenuFileConfigFile = ConfigurationManager.AppSettings["MainMenuConfigFile"];
                s_voiceServicesConfigFile = ConfigurationManager.AppSettings["VoiceServicesFile"];
                s_getBuddyConfigFile = ConfigurationManager.AppSettings["GetBuddyConfigFile"];
                s_mohFilePath = ConfigurationManager.AppSettings["MusicOnHoldFile"];
                s_setupCallbackConfigFile = ConfigurationManager.AppSettings["CallbackConfigFile"];
                s_callbackGreetingConfigFile = ConfigurationManager.AppSettings["CallbackGreetingConfigFile"];
                s_dialupConfigFile = ConfigurationManager.AppSettings["DialupConfigFile"];
                s_conferenceServiceConfigFile = ConfigurationManager.AppSettings["ConferenceServiceConfigFile"];
                s_rnlFile = ConfigurationManager.AppSettings["RnlFile"];
                s_authenticationConfigFile = ConfigurationManager.AppSettings["AuthenticationConfigFile"];

                if (string.IsNullOrEmpty(s_mainMenuFileConfigFile) ||
                    string.IsNullOrEmpty(s_voiceServicesConfigFile) ||
                    string.IsNullOrEmpty(s_getBuddyConfigFile) ||
                    string.IsNullOrEmpty(s_mohFilePath) ||
                    string.IsNullOrEmpty(s_setupCallbackConfigFile) ||
                    string.IsNullOrEmpty(s_callbackGreetingConfigFile) ||
                    string.IsNullOrEmpty(s_dialupConfigFile) ||
                    string.IsNullOrEmpty(s_conferenceServiceConfigFile) ||                    
                    string.IsNullOrEmpty(s_rnlFile) ||
                    string.IsNullOrEmpty(s_authenticationConfigFile))
                {
                    return false;
                }

            }
            
            catch (ConfigurationErrorsException cex)
            {
                s_logger.Log(cex);
                return false;
            }
            
            
            if (LoadVoiceServicesInformation() &&
                LoadMainMenuConfiguration() &&
                LoadGetBuddyConfiguration() &&
                LoadMusicOnholdConfiguration() &&
                LoadSetupCallbackConfiguration() &&
                LoadCallbackGreeting() &&
                LoadDialupConfiguration() &&
                LoadConferenceServiceConfiguration() &&
                LoadAuthenticationConfiguration())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool LoadConferenceServiceConfiguration()
        {
            var root = XElement.Load(s_conferenceServiceConfigFile);
            if (root == null)
            {
                return false;
            }

            s_conferenceServiceConfig = new ConferenceServiceConfiguration();
            s_conferenceServiceConfig.InstructionsStatement = GetVoiceStatementConfiguration(root, "stmtInstruction");

            var pairs =
                (from item in root.Descendants("command")
                 select new { Key = item.Attribute("purpose").Value, Value = item.Attribute("dtmfKey").Value }
                );

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var item in pairs)
            {
                dictionary.Add(item.Key, item.Value);
            }

            s_conferenceServiceConfig.DtmfMenuConfiguration = new DtmfMenuConfiguration { Menu = dictionary };

            s_conferenceServiceConfig.InviteUserStatement = GetVoiceStatementConfiguration(root, "stmtInviteUser");

            s_conferenceServiceConfig.HelpStatement = GetVoiceStatementConfiguration(root, "stmtHelp");
            

            return true;
        }

        private static bool LoadDialupConfiguration()
        {
            var root = XElement.Load(s_dialupConfigFile);
            if (root == null)
            {
                return false;
            }

            s_dialupConfig = new DialupConfiguration();
            s_dialupConfig.GetNumberQA = GetVoiceQaConfiguration(root, "qaGetNumber");
            s_dialupConfig.NumberConfirmationStatement = GetVoiceStatementConfiguration(root, "stmtNumberConfirmation");
            s_dialupConfig.UserDeclinedStatement = GetVoiceStatementConfiguration(root, "stmtUserDeclined");
            s_dialupConfig.DialupFailedStatement = GetVoiceStatementConfiguration(root, "stmtDialupFailure");
            
            return true;
        }

        private static bool LoadAuthenticationConfiguration()
        {
            var root = XElement.Load(s_authenticationConfigFile);
            if (root == null)
            {
                return false;
            }

            s_authenticatioConfig = new AuthenticationConfiguration();
            s_authenticatioConfig.WelcomeStatement = GetVoiceStatementConfiguration(root, "stmtWelcome");
            s_authenticatioConfig.GetPinQa = GetVoiceQaConfiguration(root, "qaPin");
            s_authenticatioConfig.InvalidPinStatement = GetVoiceStatementConfiguration(root, "stmtInvalidPin");
            s_authenticatioConfig.PinValidatedStatement = GetVoiceStatementConfiguration(root, "stmtPinValidated");
            s_authenticatioConfig.DisconnectStatement = GetVoiceStatementConfiguration(root, "stmtDisconnect");

            return true;
        }
        
        private static bool LoadCallbackGreeting()
        {
            var root = XElement.Load(s_callbackGreetingConfigFile);
            if (root == null)
            {
                return false;
            }

            s_callbackGreetingConfig = new CallbackGreetingConfiguration();
            s_callbackGreetingConfig.GreetingStatement = GetVoiceStatementConfiguration(root, "stmtCallbackGreeting");
            s_callbackGreetingConfig.CannotReachUser = GetVoiceStatementConfiguration(root, "stmtCannotReachUser");
            s_callbackGreetingConfig.UserDeclined = GetVoiceStatementConfiguration(root, "stmtUserDeclined");

            return true;
        }

        private static bool LoadSetupCallbackConfiguration()
        {           
            var root = XElement.Load(s_setupCallbackConfigFile);
            if (root == null)
            {
                return false;
            }

            s_setupCallbackConfig = new SetupCallbackConfiguration();

            s_setupCallbackConfig.SetCallbackSucceededStatement = GetVoiceStatementConfiguration(root, "stmtSetCallbackSucceeded");
            s_setupCallbackConfig.SetCallbackFailedStatement = GetVoiceStatementConfiguration(root, "stmtSetCallbackFailed");
            s_setupCallbackConfig.ConnectingToUserStatement = GetVoiceStatementConfiguration(root, "stmtConnectingToUser");
            s_setupCallbackConfig.CallbackQa = GetVoiceQaConfiguration(root, "qaCallback");
            s_setupCallbackConfig.AvailabilityPrompts = GetAvailabilityPrompts(root);

            return true;

        }

        private static bool LoadMusicOnholdConfiguration()
        {
            s_mohConfiguration = new MusicOnHoldConfiguration();
            s_mohConfiguration.FilePath = s_mohFilePath;

            return true;
        }

        public static bool TryGetVoiceServiceInformation(string serviceId, out VoiceServiceInformation info)
        {
            if (s_voiceServices == null)
            {
                throw new InvalidOperationException("The configuration has not been loaded");
            }

            info = null;
            return s_voiceServices.TryGetValue(serviceId, out info);
        }

        private static bool LoadVoiceServicesInformation()
        {            
            var root = XElement.Load(s_voiceServicesConfigFile);
            if (root == null)
            {
                return false;
            }

            s_voiceServices = 
                          (from item in root.Descendants("VoiceService")
                           select new VoiceServiceInformation(
                               item.Attribute("Id").Value,
                               item.Element("Type").Value,
                               item.Element("Assembly").Value)
                           ).ToDictionary(item => item.Id,StringComparer.OrdinalIgnoreCase);

            return s_voiceServices.Count != 0;
        }

        private static bool LoadMainMenuConfiguration()
        {        
            var root = XElement.Load(s_mainMenuFileConfigFile);
            if (root == null)
            {
                return false;
            }

            var options = (from item in root.Descendants("Option")
                           orderby int.Parse(item.Attribute("index").Value, CultureInfo.InvariantCulture)
                           select new MainMenuOption(
                               int.Parse(item.Attribute("index").Value,CultureInfo.InvariantCulture),
                               item.Element("DtmfCode").Value,
                               item.Element("ServiceId").Value,
                               item.Element("Prompt").Value
                           )).ToList();

            if (options.Count == 0)
            {
                return false;
            }

            foreach (var option in options)
            {
                if (!s_voiceServices.ContainsKey(option.ServiceId))
                {
                    s_logger.Log(Logger.LogLevel.Warning, "The main menu option is linked to an invalid service id");
                    return false;
                }
            }

            s_mainMenuConfiguration = new MainMenuConfiguration(options);
            return true;
        }

        private static bool LoadGetBuddyConfiguration()
        {
            var root = XElement.Load(s_getBuddyConfigFile);
            if (root == null)
            {
                return false;
            }

            var config = new GetBuddyConfiguration();
            config.NoContactsStatement = GetVoiceStatementConfiguration(root, "stmtNoContacts");

            config.GetContactQa = GetVoiceQaConfiguration(root, "qaGetContact");

            s_getBuddyConfig = config;
            
            return true;
        }

        private static AvailabilityPrompts GetAvailabilityPrompts(XElement root)
        {
            var node = root.Element("availabilityPrompts");

            var prompts = new AvailabilityPrompts();
            prompts.AwayPrompt = node.Element("away").Value;
            prompts.BeRightBackPrompt = node.Element("berightback").Value;
            prompts.BusyPrompt = node.Element("busy").Value;
            prompts.OfflinePrompt = node.Element("offline").Value;
            prompts.OnlinePrompt = node.Element("online").Value;
            prompts.OtherPrompt = node.Element("other").Value;
            prompts.DoNotDisturb = node.Element("donotdisturb").Value;

            return prompts;
        }

        private static VoiceQuestionAnswerConfiguration GetVoiceQaConfiguration(XElement root, string xmlTag)
        {
            var config =
                from item in root.Descendants(xmlTag)
                select new VoiceQuestionAnswerConfiguration
                {
                    MainPrompt = item.Element("mainPrompt").Value,
                    NoRecognitionPrompt = item.Element("noRecognitionPrompt").Value
                };
            return config.ToArray()[0];   
        }

        private static VoiceStatementConfiguration GetVoiceStatementConfiguration(XElement root, string xmlTag)
        {
            var config =
                from item in root.Descendants(xmlTag)
                select new VoiceStatementConfiguration
                {
                    MainPrompt = item.Element("mainPrompt").Value
                };

            return config.ToArray()[0];  
        }
    }


    #endregion
}
