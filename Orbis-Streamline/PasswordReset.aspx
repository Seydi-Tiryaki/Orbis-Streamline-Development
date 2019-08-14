<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="PasswordReset.aspx.vb" Inherits="Orbis_Streamline.PasswordReset" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
    <div class="row" style="min-height: 500px; padding-top: 5%;">
    <div class="col-xs-12 col-lg-3"></div>
    <div class="col-xs-12 col-lg-6">
      <asp:Panel ID="pnlReset" runat="server">
        <div class="col-lg-12 DivLogonCont">
          <div class="col-lg-12 AccountHeaderText">
            Reset Your Password
          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Label ID="Label1" runat="server" Text="Enter New Password:"></asp:Label>
            <asp:TextBox ID="txtPassword1" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
            <asp:RegularExpressionValidator ID="rev1" runat="server" ControlToValidate="txtPassword1" ErrorMessage="Minimum 8 characters at least 1 Alphabetic, 1 Number and 1 Special" ValidationExpression="^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@$!%*#?&])[A-Za-z\d$@$!%*#?&]{8,}$" CssClass="FormValidError"/>
            <asp:TextBox ID="txtResetID" runat="server" Visible="false" BackColor="lime"></asp:TextBox>
            <asp:TextBox ID="txtOperatorID" runat="server" Visible="false" BackColor="lime"></asp:TextBox>
          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Label ID="Label2" runat="server" Text="Confirm New Password:"></asp:Label>
            <asp:TextBox ID="txtPassword2" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Button ID="cmdReset" runat="server" Text="Reset Your Password" CssClass="form-control AccountFormButton" />
          </div>
        </div>
      </asp:Panel>
      <asp:Panel ID="pnlError" runat="server" Visible="false">
        <div class="col-lg-12 DivLogonCont">
          <div class="col-lg-12 AccountHeaderText">
            Reset Your Password Failure
          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Label ID="lblFailureMsg" runat="server" Text="" CssClass="label-warning"></asp:Label>
            
          </div>
        </div>

      </asp:Panel>
    </div>
    <div class="col-xs-12 col-lg-3"></div>
  </div>




</asp:Content>
