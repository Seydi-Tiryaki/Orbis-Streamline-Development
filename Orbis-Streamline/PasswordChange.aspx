<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="PasswordChange.aspx.vb" Inherits="Orbis_Streamline.PasswordChange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <div class="row PanelUpperHeader">
    <h1>Change Password</h1>
  </div>
  <div class="row PanelHeader" style="">
    <div class="DivSpace10"></div>
  </div>
  <div class="row MaintenanceEdit" style="max-width: 600px;margin-left:0px;">
    <div class="row">
      <div class="col-sm-2 DivEditLabel">Current Password:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtCurrent" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
      </div>
    </div>

    <div class="row">
      <div class="col-sm-2 DivEditLabel">New Password:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtNewPassword1" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-2 DivEditLabel">Confirm New Password:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtNewPassword2" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-2 DivEditLabel"></div>
      <div class="col-sm-10 DivEditField">
        <asp:Button ID="cmdChangePassword" runat="server" Text="Change Password" CssClass="form-control AccountFormButton" />
      </div>
    </div>



  </div>



</asp:Content>
