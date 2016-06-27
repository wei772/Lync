/*=====================================================================

 File   :  GetBuddyDialog.cs

 Summary:  Gets the name of contact the user wants to contact with
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.GetContactService.GetBuddyDialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Speech.Recognition;
    using System.Globalization;
    using Microsoft.Rtc.Signaling;
    using Microsoft.Speech.Recognition.SrgsGrammar;
    using System.Diagnostics;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    class GetBuddyDialog : DialogBase
    {

        #region Public properties for inputs to this dialog
        public AudioVideoCall avCall { get; set; }
        public GetBuddyConfiguration Configuration { get; set; }
        public CustomerSession CustomerSession { get; set; }
        #endregion

        /// <summary>
        /// Output for this dialog, the selected contact
        /// </summary>
        private ContactInformation selectedContactInfo;


        private List<Grammar> speechGrammar, dtmfGrammar;
        private IDictionary<string, ContactInformation> contacts;
        private string selectedContactUri;



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="customerSession"></param>
        /// <param name="configuration"></param>
        public GetBuddyDialog(CustomerSession customerSession, GetBuddyConfiguration configuration)
        {

            this.Configuration = configuration;
            this.CustomerSession = customerSession;
            this.avCall = this.CustomerSession.CustomerServiceChannel.ServiceChannelCall;
            speechGrammar = new List<Grammar>();
            dtmfGrammar = new List<Grammar>();
        }

        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.Configuration = inputParameters["Configuration"] as GetBuddyConfiguration;
            this.CustomerSession = inputParameters["CustomerSession"] as CustomerSession;
            this.avCall = inputParameters["Call"] as AudioVideoCall;
        }

        /// <summary>
        /// Execute activities of the dialog one by one.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {
            //Load contacts from user's contact list.
            Task<ActivityResult> loadContacts = this.CreateCodeActivity_LoadContacts();
            yield return loadContacts;
            if (contacts.Count == 0)
            {               
                //Speak as "No contatcts are loaded".
                Task<ActivityResult> stmtNoContacts = Create_SpeechStmtNoContacts();
                yield return stmtNoContacts;
            }
            else
            {

                //1. Prepare grammar for contact names.
                Task<ActivityResult> setUpGrammar = this.CreateCodeActivity_SetUpGrammar();
                yield return setUpGrammar;
                //2. Ask for name of the contact.
                Task<ActivityResult> speechQAGetContactName = this.Create_SpeechQuestionAnswerActivity();
                yield return speechQAGetContactName.ContinueWith((task) =>
                    {
                        if (task.Exception == null)
                        {
                            RecognitionResult getContactNameResult = speechQAGetContactName.Result.Output["Result"] as RecognitionResult;
                            this.selectedContactUri = (string)getContactNameResult.Semantics.Value;
                        }
                    });

                //3. Parse result from customer's respnose.
                Task<ActivityResult> parseResult = this.CreateCodeActivity_ParseResult();
                yield return parseResult;
            }
        }

        /// <summary>
        /// Creates a task of speech statement activity to speak statement as no contacts are loaded.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechStmtNoContacts()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity stmtSorryNoContacts = new SpeechStatementActivity(this.avCall, this.Configuration.NoContactsStatement.MainPrompt);
                return stmtSorryNoContacts.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;

        }



        /// <summary>
        /// Creates a task of code activity to load contacts from contact list of customer.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_LoadContacts()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(Code_LoadContacts);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates a task of a code activity to set up a grammar from the loaded contact list.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_SetUpGrammar()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(Code_PrepareGrammar);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates a task of a code activity to parse result of question-answer(get the contact name).
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_ParseResult()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeGetContactAcitvity = new CodeActivity();
                codeGetContactAcitvity.ExecuteCode += new EventHandler(Code_GetContactInfo);
                return codeGetContactAcitvity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates a task of a speech question answer activity to get the contact name user wants to contact with.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechQuestionAnswerActivity speechQAGetContactName = new SpeechQuestionAnswerActivity(this.avCall, this.Configuration.GetContactQa.MainPrompt, speechGrammar, null, null, null);

                speechQAGetContactName.NoRecognitionPrompt = this.Configuration.GetContactQa.NoRecognitionPrompt;
                speechQAGetContactName.CanBargeIn = true;
                speechQAGetContactName.PreFlushDtmf = false;

                return speechQAGetContactName.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Loads contacts from the contact list of the customer
        /// </summary>
        private void Code_LoadContacts(object sender, EventArgs e)
        {
            CustomerContactManager contactManager = this.CustomerSession.ContactManager;
            if (contactManager != null)
            {
                contacts = contactManager.GetContacts();
            }
            else
            {
                contacts = new Dictionary<string, ContactInformation>();
            }
        }
        /// <summary>
        /// Create Grammars for the contact names of customer's contact list 
        /// </summary>
        private void Code_PrepareGrammar(object sender, EventArgs e)
        {

            SrgsOneOf oneOf = new SrgsOneOf();

            foreach (var contact in contacts)
            {
                SrgsItem item = new SrgsItem(contact.Value.DisplayName);

                SrgsSemanticInterpretationTag tag =
                    new SrgsSemanticInterpretationTag("$._value = \"" + contact.Key + "\";");

                SrgsItem nameItem = new SrgsItem();
                nameItem.Add(item);
                nameItem.Add(tag);

                oneOf.Add(nameItem);
            }

            SrgsRule nameRule = new SrgsRule("name");
            nameRule.Scope = SrgsRuleScope.Public;
            nameRule.Elements.Add(oneOf);

            SrgsDocument grammarDoc = new SrgsDocument();
            grammarDoc.Rules.Add(nameRule);
            grammarDoc.Root = nameRule;

            speechGrammar.Add(new Grammar(grammarDoc));

        }
        /// <summary>
        /// Gets the contact info of selected contact name
        /// </summary>
        private void Code_GetContactInfo(object sender, EventArgs e)
        {
            // Debug.Assert(!string.IsNullOrEmpty(selectedContactUri), "m_selectedContact should not be null or empty");
            if (!string.IsNullOrEmpty(selectedContactUri))
                selectedContactInfo = contacts[selectedContactUri];
        }

        /// <summary>
        /// Raises the dialog completion event 
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {
            //sets the output dictionary
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("selectedContact", selectedContactInfo);
            //raise the dialog complete event
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));

        }
        /// <summary>
        /// Dialog complete handler
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }

    }
}
