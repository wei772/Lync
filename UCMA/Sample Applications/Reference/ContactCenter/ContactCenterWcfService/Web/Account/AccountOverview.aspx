<%@ Page Title="Contoso Brands - Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountOverview.aspx.cs" Inherits="Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web.AccountOverview" %>

<%@ Import Namespace="Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common" %>

<asp:Content ContentPlaceHolderID="TitlePlaceholder" runat="server">
	<img src="../Images/account.png" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="DescriptionPlaceholder" runat="server">
	Chat, Manage Your Account, View Order Status.
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
    <link href="../Styles/AccountOverview.css" rel="stylesheet" type="text/css" />
    <script src="../Scripts/ChatLauncher.js" type="text/javascript"></script>
    <!-- Presence Control includes -->
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxCore.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxComponentModel.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxSerialization.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxTemplates.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxNetwork.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxWebServices.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxAdoNet.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxDataContext.debug.js" type="text/javascript"></script>
    <script src="../Scripts/MicrosoftAjax/MicrosoftAjaxApplicationServices.debug.js" type="text/javascript"></script>
    <script src="../Scripts/presence.timer.js" type="text/javascript"></script>
    <script src="../Scripts/presence.helper.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="ContentPlaceholder" runat="server">
	
	<div class="page">
		<div class="inline-block left-column">
			<div class="content">
				<div class="section">
					<h2>Available Agents</h2>
					<h3>Chat with an Expert Now!</h3>
					<h4>Select an available agent:</h4>
				</div>
				<div>
                    <!-- Presence Control -->
                    <!-- new Function('sender','args','alert(args.get_error().get_message());') -->
                    <div id="contacts-wrapper">
                        <div id="presence-error"></div>
                        <ul id="contacts" 
                            class="contacts sys-template"
                            sys:attach="dataview"
                            dataview:dataprovider="{{ PresenceHelper.dataContext }}"
                            dataview:autofetch="true"
                            dataview:fetchoperation="GetPresence"
                            dataview:fetchparameters="{{ PresenceHelper.webContext }}"
                            dataview:onfetchfailed="{{ PresenceHelper.fetchFailed }}"
                            dataview:onfetchsucceeded="{{ PresenceHelper.fetchSuccess }}" >     
                            <li>
                                <div class="name-status-wrapper">
                                    <span sys:class="{ binding Availability,convert={{ PresenceHelper.getTicTac }} }" ></span>
                                    <span class="name" sys:innertext="{ binding EntityName }"></span>
                                    <span class="dept" sys:innertext="{ binding ActivityStatus }"></span>
                                </div>
                                <button sys:codeafter="$addHandler($element, 'click', function() {PresenceHelper.loadChatWindow($dataItem); return false;})" id="ChatNowButton" type="button" class="short inline-block" style="float: right;">
								    <div>
									    Chat Now <span class="chevron"></span>
								    </div>
							    </button>
                            </li>
                        </ul>
                    </div>
                <asp:TextBox ID="txtUserName" runat="server" style="display:none;" />
                <asp:TextBox ID="txtUserPhone" runat="server" style="display:none;" />
				</div>
			</div>
		</div>
		<div class="inline-block solid-rule vertical-rule right-column" style="vertical-align: top">
			<div>
				<div style="margin-left:10px;">
					<div class="section" style="vertical-align: bottom">
					<div class="settings-left-column inline-block">
						<h2>
							Account Settings</h2>
						<h3>
							Update your Information at any time!</h3>
						<h4>
							Account overview shown below: </h4>
					</div>
					<div class="inline-block">

						<ul class="horiz-menu label form-detail">
							<li style="margin-right:50px">
								<a href="AccountEditor.aspx">Edit Your Account <span class="chevron"></span></a>
							</li>
							<li>
								<a href="#">Add User to Your Account <span class="chevron"></span></a>
							</li>
						</ul>
						
					</div>
				</div>
					<div class="dashed-rule horizontal-rule" />
					<div class="section">
						<div class="inline-block account-settings settings-left-column">
							<div class="form-row">
								<span class="form-label">First Name: </span><span id="FirstNameLabel" class="value" runat="server"></span>
							</div>
							<div class="form-row">
								<span class="form-label">Last Name: </span><span id="LastNameLabel" class="value" runat="server"></span>
							</div>
						</div>
						<div class="inline-block account-settings" style="vertical-align: top">
							<div class="form-row">
								<span class="form-label">Email Address: </span><span id="EmailLabel" class="value" runat="server"></span>
							</div>
							<div class="form-row">
								<span class="form-label">Phone Number: </span><span id="PhoneLabel" class="value" runat="server"></span>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div class="solid-rule horizontal-rule">
			</div>
			<div>
				<div style="margin-left:10px;">
					<div style="vertical-align: bottom">
						<div class="section">
							<h2>
								Order History</h2>
							<h3>
								Thank you! View past and recent orders.</h3>
							<h4>Most recent orders shown below: </h4>
						</div>
					</div>
					<div class="dashed-rule horizontal-rule">
						<table class="data">
							<thead>
								<tr>
									<th class="spacer-column"></th>
									<th>
										<span class="label">Date</span>
									</th>
									<th>
										<span class="label">Order #</span>
									</th>
									<th>
										<span class="label">Product Name</span>
									</th>
									<th>
										<span class="label">Status</span>
									</th>
									<th>
										<span class="label">Price</span>
									</th>
									<th>
									</th>
								</tr>
							</thead>
							<tfoot>
							</tfoot>
							<tbody>
								<tr class="odd">
									<td class="spacer-column"></td>
									<td>
										<span>04/26/2012</span>
									</td>
									<td>
										<span>8777</span>
									</td>
									<td>
										<span>Nokia Lumia 900 Cyan</span>
									</td>
									<td>
										<span>Back Ordered</span>
									</td>
									<td>
										<span>$99.99</span>
									</td>
									<td>
										<span>View Details</span>
									</td>
								</tr>
								<tr>
									<td class="spacer-column"></td>
									<td>
										<span>05/25/2010</span>
									</td>
									<td>
										<span>4734</span>
									</td>
									<td>
										<span>Screen Protector (3pk)</span>
									</td>
									<td>
										<span>Returned</span>
									</td>
									<td>
										<span>19.99</span>
									</td>
									<td>
										<span>View Details</span>
									</td>
								</tr>
								<tr class="odd">
									<td class="spacer-column"></td>
									<td>
										<span>05/15/2010</span>
									</td>
									<td>
										<span>4168</span>
									</td>
									<td>
										<span>HTC Car Charger</span>
									</td>
									<td>
										<span>Delivered</span>
									</td>
									<td>
										<span>$29.99</span>
									</td>
									<td>
										<span>View Details</span>
									</td>
								</tr>
								<tr>
									<td class="spacer-column"></td>
									<td>
										<span>04/14/2010</span>
									</td>
									<td>
										<span>9856</span>
									</td>
									<td>
										<span>T-Mobile HTC HD7</span>
									</td>
									<td>
										<span>Delivered</span>
									</td>
									<td>
										<span>$199.99</span>
									</td>
									<td>
										<span>View Details</span>
									</td>
								</tr>
							</tbody>
						</table>
					</div>
				</div>
			</div>
		</div>
	</div>

	<script type="text/javascript">

		function ChatNowButton_OnClick(sender, e) {
			var sipAddress = sender.getAttribute("sip-address");
			// TODO: launch chat
		}

	</script>

</asp:Content>
