<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="SystemLogon.aspx.vb" Inherits="Orbis_Streamline.WebForm_test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title></title>
</head>
<body>
  <form id="form1" runat="server">
    <div>
      <div class="row" style="min-height: 500px; padding-top: 5%;">
        <div class="col-xs-12 col-lg-3"></div>
        <div class="col-xs-12 col-lg-6">
          <div class="col-lg-12 DivLogonCont">
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
            <div class="col-lg-12 AccountSubHeaderText">
              Forgot your password?<br />
              Enter your email below to request a new password.
            </div>
            <div class="col-lg-12 AccountFormField">
              <asp:Label ID="Label3" runat="server" Text="Your Email Address:"></asp:Label>
              <asp:TextBox ID="txtReminderEmail" runat="server" CssClass="form-control"></asp:TextBox>
              <asp:RegularExpressionValidator ID="revReminder" runat="server" ControlToValidate="txtReminderEmail" ErrorMessage="Email" Display="None" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" EnableClientScript="False"></asp:RegularExpressionValidator>

            </div>
            <div class="col-lg-12 AccountFormField">
              <asp:Button ID="cmdRemind" runat="server" Text="Request New Password" CssClass="form-control AccountFormButton" />
            </div>
          </div>
        </div>
        <div class="col-xs-12 col-lg-3"></div>
      </div>




    </div>
  </form>
</body>
</html>
