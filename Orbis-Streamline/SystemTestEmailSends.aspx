<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="SystemTestEmailSends.aspx.vb" Inherits="Orbis_Streamline.SystemTestEmailSends" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <div class="row">
    <div class="col-sm-2">
      Select email to send:
    </div>
    <div class="col-sm-2">
      <asp:DropDownList ID="ddEmailType" runat="server" CssClass="form-control"></asp:DropDownList>
    </div>
        <div class="col-sm-1">
          To:
</div>
    <div class="col-sm-2">
      <asp:TextBox ID="txtToEmail" runat="server" Text="paul.craven@horizon-it.co.uk" CssClass="form-control"></asp:TextBox>
    </div>
    <div class="col-sm-2">
      <asp:Button ID="cmdSend" runat="server" Text="Send Email" CssClass="form-control AccountFormButton" />
    </div>

  </div>
  <hr />
  <div class="row">
    <div class="col-sm-2">
      <asp:Button ID="cmdFixDupTriggers" runat="server" Text="Fix Duplicate Trigger IDs" CssClass="form-control AccountFormButton"  />
    </div>
  </div>
  <div class="row">
    <div class="col-sm-2">
      <asp:Button ID="cmdProcessRawData" runat="server" Text="Process Raw Data File" CssClass="form-control AccountFormButton"  />
    </div>
  </div>
  <div class="row">
    <div class="col-sm-2">
      <asp:Button ID="cmdCreateDir" runat="server" Text="Test directory creation and deletion" CssClass="form-control AccountFormButton"  />
    </div>
  </div>
  <div class="row">
    <div class="col-sm-2">
      <asp:Button ID="cmdCallAlgo" runat="server" Text="Call Algo Processing" CssClass="form-control AccountFormButton"  />
    </div>
  </div>



</asp:Content>
