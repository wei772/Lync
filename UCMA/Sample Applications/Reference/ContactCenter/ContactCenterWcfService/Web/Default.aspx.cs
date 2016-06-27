
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.Common;
using System.Web.UI.HtmlControls;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web
{
	public partial class Products : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				DataBindProducts();
			}
		}

		protected void DataBindProducts()
		{
			ProductRepeater.ItemDataBound += new RepeaterItemEventHandler(ProductRepeater_ItemDataBound);
			string connectionString = ConfigurationManager.ConnectionStrings["ProductStoreConnectionString"].ConnectionString;
			DbConnection connection = SqlHelper.Current.GetConnection(connectionString);
            using (ProductStore dataStore = new ProductStore(connection))
			{
				var products = from p in dataStore.Product
							   select p;

				ProductRepeater.DataSource = products;
				ProductRepeater.DataBind();
			}
		}

		private void ProductRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
            Product product = ((Product)e.Item.DataItem);

			HtmlImage image = e.Item.FindControl("Image") as HtmlImage;
			image.Src = product.Image;
			image.Alt = product.Title;

			string userName = String.Empty;
			string userPhone = String.Empty;

			UserProfile profile = UserProfile.GetUserProfile();
			if (profile != null)
			{
				userName = profile.FirstName + " " + profile.LastName;
				userPhone = profile.Phone;
			}

			HtmlAnchor chatLink = e.Item.FindControl("ChatLink") as HtmlAnchor;
			chatLink.Attributes.Add("href", String.Format("javascript:ChatLauncher.launch('queueName=Sales&id={0}&un={1}&up={2}');", product.Id, userName, userPhone));
		}

		protected Decimal GetDiscountAmount(Product product)
		{
			return (product.Price - product.DiscountedPrice);
		}

		protected Decimal GetDiscountPercent(Product product)
		{
			return ((product.Price - product.DiscountedPrice) / product.Price);
		}
	}
}