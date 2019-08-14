<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine01.Master" CodeBehind="default.aspx.vb" Inherits="Orbis_Streamline._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="row" style="min-height: 500px; padding-top: 5%;">
    <div class="col-xs-12 col-lg-1"></div>
    <div class="col-xs-12 col-lg-10 DivLogonCont">
      <div class="col-xs-12  col-lg-8 ">
        <div class="col-lg-12 AccountHeaderText">
          Login To Your Account
        </div>
        <div class="col-lg-12 AccountFormField">
          <asp:Label ID="Label1" runat="server" Text="Your Email Address:"></asp:Label>
          <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control"></asp:TextBox>
          <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtUsername" ErrorMessage="Email" Display="None" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" EnableClientScript="False"></asp:RegularExpressionValidator>
        </div>
        <div class="col-lg-12 AccountFormField">
          <asp:Label ID="Label2" runat="server" Text="Your Password:"></asp:Label>
          <asp:TextBox ID="txtPword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
        </div>
        <div class="col-lg-12 AccountFormField">
          <asp:Button ID="cmdLogin" runat="server" Text="Login To Your Account" CssClass="form-control AccountFormButton" />
        </div>
        <hr class="col-xs-12" />
        <asp:Panel ID="pnlReminder" runat="server" Visible="true">
          <div class="col-lg-12 AccountSubHeaderText">
            Forgot your password?<br />
            Enter your email below to request a password reset email.
          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Label ID="Label3" runat="server" Text="Your Email Address:"></asp:Label>
            <asp:TextBox ID="txtReminderEmail" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RegularExpressionValidator ID="revReminder" runat="server" ControlToValidate="txtReminderEmail" ErrorMessage="Email" Display="None" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" EnableClientScript="False"></asp:RegularExpressionValidator>

          </div>
          <div class="col-lg-12 AccountFormField">
            <asp:Button ID="cmdRemind" runat="server" Text="Reset Password" CssClass="form-control AccountFormButton" />
          </div>
        </asp:Panel>
      </div>
      <div class="col-xs-12  col-lg-4">
        <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/Orbis-Logo-01.jpg" Width="100%" />
      </div>
    <div class="col-xs-12 col-lg-1"></div>
    </div>
  </div>
</asp:Content>
