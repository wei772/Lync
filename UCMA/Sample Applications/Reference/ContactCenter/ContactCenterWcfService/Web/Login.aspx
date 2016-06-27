<%@ Page Title="Contoso Brands - Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitlePlaceholder" runat="server">
<img src="Images/login.png" />
</asp:Content>

<asp:Content ContentPlaceHolderID="head" runat="server">
	<link href="Styles/Site.css" rel="stylesheet" type="text/css" />
    <link href="Styles/Login.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="DescriptionPlaceholder" runat="server">
	Register an account, update your profile, manage an order, or view order status.
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="ContentPlaceholder" runat="server">
	<div class="page">
		<div class="left-column solid-rule vertical-rule-right  inline-block">
			<div class="content">
				<div style="margin-left:40px">
					<div class="section">
						<h2>New Customer</h2>
						<h3>Register an account with us today!</h3>
						<h4>Please enter your information below:</h4>
					</div>
					<div class="horizontal-rule dashed-rule"></div>
					<div class="section">
                        <asp:Panel id="panelRegisterUser" runat="server">
	                        <asp:CreateUserWizard ID="RegisterUser" runat="server" EnableViewState="false" OnCreatedUser="RegisterUser_CreatedUser">
                                <WizardSteps>
                                    <asp:CreateUserWizardStep ID="RegisterUserWizardStep" runat="server">
                                        <ContentTemplate>
                                            <span class="failureNotification">
                                                <asp:Literal ID="ErrorMessage" runat="server"></asp:Literal>
                                            </span>
                                            <asp:ValidationSummary ID="RegisterUserValidationSummary" runat="server" CssClass="failureNotification" ValidationGroup="RegisterUserValidationGroup"/>
                                            <div class="accountInfo">
						                        <div class="form-row">
                                                    <span class="form-label">First Name:</span><asp:TextBox ID="FirstName" runat="server"></asp:TextBox>
                                                </div>
						                        <div class="form-row">
                                                    <span class="form-label">Last Name:</span><asp:TextBox ID="LastName" runat="server"></asp:TextBox>
                                                </div>
                                                <div class="form-row">
                                                    <span class="form-label">Username:</span><asp:TextBox ID="UserName" runat="server"></asp:TextBox>
                                                </div>
                                                <div class="form-row">
                                                    <span class="form-label">Email Address:</span><asp:TextBox ID="Email" runat="server"></asp:TextBox>
                                                </div>
						                        <div class="form-row">
							                        <span class="form-label">Phone Number:</span><asp:TextBox ID="Phone" runat="server"></asp:TextBox>
						                        </div>
						                        <div class="form-break"></div>
                                                <div class="form-row">
                                                    <span class="form-label">Create Password:</span><asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
                                                </div>
                                                <div class="form-row">
                                                    <span class="form-label">Confirm Password:</span><asp:TextBox ID="ConfirmPassword" runat="server" TextMode="Password"></asp:TextBox>
                                                </div>
						                        <div class="detail form-foot" >
							                        Questions? Prefer to order by Phone or Chat? Our Customer Loyalty Team is available 24 hours a day, call 1.800.000.000 to talk with an Expert Now!
						                        </div>
                                                <p class="submitButton">
                                                    <asp:Button ID="CreateUserButton" CssClass="register-button" runat="server" CommandName="MoveNext" Text="Register" ValidationGroup="RegisterUserValidationGroup"/>
                                                </p>
                                            </div>
                                        </ContentTemplate>
                                        <CustomNavigationTemplate>
                                        </CustomNavigationTemplate>
                                    </asp:CreateUserWizardStep>
                                </WizardSteps>
                            </asp:CreateUserWizard>
						</asp:Panel>
					</div>
				</div>
			</div>
		</div>
		<div class="right-column inline-block">
			<div class="content">
				<div style="margin-left:40px">
					<div class="section">
						<h2>Existing Customer</h2>
						<h3>Please enter your credentials below:</h3>
					</div>
					<div class="horizontal-rule dashed-rule"></div>
					<div class="section">
						<asp:Login ID="LoginUser" FailureAction="Refresh" DestinationPageUrl="~/Default.aspx" runat="server" EnableViewState="false" >
							<LayoutTemplate>
                                <asp:Panel id="panelLogin" DefaultButton="LoginButton" runat="server">
								    <asp:Label ID="FailureText" CssClass="failureNotification" runat="server"></asp:Label>
								    <div class="form-row">
									    <span class="form-label">Username:</span><asp:TextBox ID="UserName" runat="server" ValidationGroup=""></asp:TextBox>
								    </div>
								    <div class="form-row">
									    <span class="form-label">Password:</span><asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
								    </div>
								    <div>
									    <span class="form-label"></span><span class="detail">(Remember, password is case sensitive)</span>
								    </div>

								    <div class="detail form-foot">
									    Forget your password? Do you have questions about an order or your account? Call 1.800.000.000 to talk with an Expert Now!
								    </div>
								    <div class="form-foot">
									    <asp:Button ID="LoginButton" CssClass="login-button" runat="server" CommandName="Login" Text="Log In" ValidationGroup="LoginUserValidationGroup"/>
								    </div>
                                </asp:Panel>
							</LayoutTemplate>
						</asp:Login>
					</div>
				</div>
				
			</div>
			<div style="clear:both; margin-top:100px">
					<ul class="horiz-menu">
						<li style="margin-right:10px;margin-left:20px">
							<a href="#">Read our Privacy Policy <span class="chevron"></span></a>
						</li>
						<li>
							<a href="#">Read our Shipping and Return Policy <span class="chevron"></span></a>
						</li>
					</ul>
				</div>
		</div>
	</div>

</asp:Content>
