/********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Configuration;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Net.Mime;
using System.Xml;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using System.Data.SqlClient;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.ContactCenterWcfService.ContextInformation
{
    /// <summary>
    /// Db product.
    /// </summary>
    public class DbProduct 
    {
        public string Id { get;set;}

        public string Title {get; set;}

        public double Price { get; set;}

        public double DiscountedPrice { get; set; }

        public short Rating { get; set; }

        public string Image { get; set; }
    }

    public class DbAgentSkill 
    {
        public string Id { get; set;}

        public string SkillName { get; set;} 

        public string SkillValue { get; set;}
    }

    public class DbAgentSkillMapping 
    {
        public string Id { get; set; }
        public string ProductId { get; set; }

        public string AgentSkillId { get; set; }
    }

  
    /// <summary>
    /// Represents context information interface.
    /// </summary>
    public class ProductContextContextInformationProvider : IContextInformationProvider
    {

        #region private variables

        /// <summary>
        /// Context key to look for.
        /// </summary>
        private const string ContextKey = "ProductId";

        /// <summary>
        /// Connnection string key.
        /// </summary>
        private const string ConnectionStringKey = "ProductStoreConnectionString";

        /// <summary>
        /// Connection string.
        /// </summary>
        private string m_connectionString;

        #endregion


        #region constructor

        /// <summary>
        /// Creates a new product context information.
        /// </summary>
        public ProductContextContextInformationProvider()
        {
            //Figure out the connection string.
            ConnectionStringSettings connSettings = ConfigurationManager.ConnectionStrings[ProductContextContextInformationProvider.ConnectionStringKey];
            if (connSettings != null)
            {
                m_connectionString = connSettings.ConnectionString;
            }
        }
        #endregion

        #region private variables

        /// <summary>
        /// Gets product type from product id.
        /// </summary>
        /// <param name="productId">Product id.</param>
        /// <returns>productType. Can be null.</returns>
        private productType GetProductTypeFromProductId(string productId)
        {
            Product dbProduct = null;
            string connectionString = m_connectionString;
            List<AgentSkill> agentSkills = null;
            productType retVal = null;

            if (!String.IsNullOrEmpty(productId) && !String.IsNullOrEmpty(connectionString))
            {
                using (ProductStore dataStore = new ProductStore(SqlHelper.Current.GetConnection(connectionString)))
                {
                    try
                    {
                        dbProduct = dataStore.Product.SingleOrDefault(p => p.Id.ToString().Equals(productId));
                        if (dbProduct != null)
                        {
                            //If we have product info. Query for agent skills associated with this product.
                            var allAgentSkillMappings = from a in dataStore.AgentSkillMapping
                                                        where a.ProductId == dbProduct.Id
                                                        select a;

                            agentSkills = new List<AgentSkill>();
                            foreach (var agentSkillMapping in allAgentSkillMappings)
                            {
                                if (agentSkillMapping.AgentSkillId.HasValue)
                                {
                                    AgentSkill agentSkill = dataStore.AgentSkill.SingleOrDefault(ask => ask.Id.ToString().Equals(agentSkillMapping.AgentSkillId));
                                    if (agentSkill != null)
                                    {
                                        agentSkills.Add(agentSkill);
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        Helper.Logger.Error("Sql exception occured {0}", EventLogger.ToString(sqlException));
                    }
                } // end of data store.
            }

            if (dbProduct != null)
            {
                retVal = new productType();
                retVal.productGuid = dbProduct.Id.ToString();
                retVal.productImage = dbProduct.Image;
                retVal.productPrice = dbProduct.Price.ToString();
                retVal.productTitle = dbProduct.Title;
                if (agentSkills != null && agentSkills.Count > 0)
                {
                    retVal.agentSkillsList = new agentSkillType[agentSkills.Count];
                    for (int i = 0; i < agentSkills.Count; i++)
                    {
                        agentSkillType agentSkillType = new agentSkillType();
                        agentSkillType.name = agentSkills[i].SkillName;
                        agentSkillType.Value = agentSkills[i].SkillValue;
                        retVal.agentSkillsList[i] = agentSkillType;
                    }
                }
            }

            return retVal;
        }

        #endregion

        #region interface implementation
        /// <summary>
        /// Returns MimePartContentDescription based on incoming context.
        /// </summary>
        /// <param name="incomingContext">Incoming context. </param>
        /// <returns>MimePartContextDescription.</returns>
        public MimePartContentDescription GenerateContextMimePartContentDescription(IDictionary<string, string> incomingContext)
        {
            MimePartContentDescription mimePartContentDescription = null;

            //Check if we have context key.
            if(incomingContext != null && incomingContext.ContainsKey(ProductContextContextInformationProvider.ContextKey))  
            {
                productType productType = this.GetProductTypeFromProductId(incomingContext[ProductContextContextInformationProvider.ContextKey]);
                if (productType != null)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(productType));
                    //Try to serialize
                    try
                    {
                        byte[] contentAsByteArray = CommonHelper.SerializeObjectToByteArray(productType, xmlSerializer);
                        ContentType contentType = new ContentType();
                        mimePartContentDescription = new MimePartContentDescription(contentType, contentAsByteArray);
                    }
                    catch (XmlException xmlException)
                    {
                        Helper.Logger.Error("Xml exception occured {0}", EventLogger.ToString(xmlException));
                    }
                }
            }

            return mimePartContentDescription;
        }

        #endregion
    }
    
}
