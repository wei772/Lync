using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Factory of message handlers.
    /// </summary>
    public class MessageHandlerFactory
    {

        /// <summary>
        /// List of message handlers to be injected into this factory via MEF.
        /// </summary>
        [ImportMany(typeof(MessageHandler))]
        private List<MessageHandler> messageHandlers = null;

        /// <summary>
        /// Initializes the message handlers.
        /// </summary>
        /// <returns></returns>
        internal static List<MessageHandler> InitializeMessageHandlers()
        {
            MessageHandlerFactory factory = new MessageHandlerFactory();
            factory.ComposeMessageHandlers();
            return factory.messageHandlers;
        }
        

        /// <summary>
        /// Composes the message handlers.
        /// </summary>
        private void ComposeMessageHandlers()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog("."));

            Assembly entryAssembly = Assembly.GetEntryAssembly();
        
            if (entryAssembly != null)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(entryAssembly));
            }

            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

    }
}
