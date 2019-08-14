<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ExceptionPage.aspx.vb" Inherits="Orbis_Streamline.ExceptionPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div class="ContentPanel">
      <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/Bathroom-traders-logo-263.jpg" />
      <div class="DivBlock">
        <p style="color:red;font-weight:bold;font-size:2.0em">Web Site Problem</p>
      </div>

      <div class="DivSpace100"></div>
      <div class="DivBlock">
        <asp:Table ID="tblDetails" runat="server" HorizontalAlign="Center" Width="95%" CssClass="LabelBold"></asp:Table>
      </div>
    </div>

    </form>
</body>
</html>
