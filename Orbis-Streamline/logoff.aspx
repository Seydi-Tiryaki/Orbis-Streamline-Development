<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="logoff.aspx.vb" Inherits="Orbis_Streamline.logoff" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <meta http-equiv="Refresh" content="3; url=default.aspx" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <div class="row">
    <h1>Device Management</h1>
  </div>
  <div class="row">
    <asp:Label ID="Label1" runat="server" Text="You have been logged off." CssClass="AccountHeaderText"></asp:Label>
  </div>

</asp:Content>
