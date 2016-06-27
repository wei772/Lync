<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccountEditor.aspx.cs" Inherits="Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web.Account.AccountEditor" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<link href="../Styles/AccountOverview.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="TitlePlaceholder" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="PageHeaderPlaceholder" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="DescriptionPlaceholder" runat="server">
</asp:Content>
<asp:Content ID="Content5" ContentPlaceHolderID="ContentPlaceholder" runat="server">

<div style=" width:500px">
		<div class="content">
				<div style="margin-left:40px">
					<div class="section">
						<h2>Update Account</h2>
						<h3>Update your account info</h3>
						<h4>Please update your information below:</h4>
					</div>
					<div class="horizontal-rule dashed-rule"></div>
					<div class="section">
                        <span class="failureNotification">
                            <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                        </span>
                        <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification" ValidationGroup="UpdateUserValidationGroup"/>
                        <div class="accountInfo">
						    <div class="form-row">
                                <span class="form-label">First Name:</span><asp:TextBox ID="FirstName" runat="server"></asp:TextBox>
                            </div>
						    <div class="form-row">
                                <span class="form-label">Last Name:</span><asp:TextBox ID="LastName" runat="server"></asp:TextBox>
                            </div>
                            <div class="form-row">
                                <span class="form-label">Email Address:</span><asp:TextBox ID="Email" runat="server"></asp:TextBox>
                            </div>
						    <div class="form-row">
							    <span class="form-label">Phone Number:</span><asp:TextBox ID="Phone" runat="server"></asp:TextBox>
						    </div>
                            <p class="submitButton" style="margin-top:50px">
                                <asp:Button ID="UpdateUserButton" runat="server" Text="Update" 
									onclick="UpdateUserButton_Click"/>
                            </p>
                        </div>
					</div>
				</div>
			</div>
		</div>
</asp:Content>
