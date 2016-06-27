<%@ Page Title="Contoso Brands - Products" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
	CodeBehind="Default.aspx.cs" Inherits="Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web.Products" %>

<%@ Import Namespace="Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <link href="Styles/Default.css" rel="stylesheet" type="text/css" />
	<script src="Scripts/ChatLauncher.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="TitlePlaceholder" runat="server">
	<img src="Images/best-sellers.png" />
</asp:Content>

<asp:Content ContentPlaceHolderID="DescriptionPlaceholder" runat="server">
	Results:
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceholder" runat="server">
	<div class="page" style="text-align: center">
		<asp:Repeater ID="ProductRepeater" runat="server">
			<ItemTemplate>
				<div class="result vertical-rule-right solid-rule">
					<div class="horizontal-rule-bottom dashed-rule">
						<div class="phone-column">
							<img id="Image" class="phone-image" runat="server" />
						</div>
						<div class="text-column">
							<h2>
								<%#((Product)Container.DataItem).Title%>
							</h2>
							<h3>
								Your Price: <span class="price"><%#((Product)Container.DataItem).DiscountedPrice.ToString("$#.00")%></span></h3>
							<div>
								<span>Compare at: </span><span class="slashed-price"><%#((Product)Container.DataItem).Price.ToString("$#.00")%></span>
							</div>
							<div>
								<span>You Save: </span><span><%#GetDiscountAmount((Product)Container.DataItem).ToString("$#.00")%> (<%#GetDiscountPercent((Product)Container.DataItem).ToString("#%")%>)</span>
							</div>
							<div style="margin-top:10px;">
								<span>Review: <span class="icon-4-stars"></span></span>
							</div>
							<a id="ChatLink" style="margin-top:10px;" runat="server">
								<span class="detail" style="vertical-align:middle"><span class="chat-icon"></span>Click to Chat with an Expert</span>
							</a>
						</div>
					</div>
				</div>
			</ItemTemplate>
		</asp:Repeater>
	</div>


</asp:Content>
