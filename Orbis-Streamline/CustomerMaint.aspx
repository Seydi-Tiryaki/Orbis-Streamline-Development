<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="CustomerMaint.aspx.vb" Inherits="Orbis_Streamline.CustomerMaint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <style>
    html {
      height: 100%
    }

    body {
      height: 100%
    }

    form {
      height: 100%
    }

    .DivPageContent {
      height: calc(100% - 150px)
    }
  </style>
  <div style="height: 100%">
    <div class="row PanelUpperHeader">
      <h1>Customer and Location Maintenance</h1>
    </div>
    <div class="row PanelHeader" style="">

      <div class="DivSpace10"></div>
    </div>
    <div class="row">
      <div class="col-sm-12 col-lg-3">
        <div class="col-sm-12 DivEditLabel">
          <asp:Label ID="Label3" runat="server" Text="Organization:" CssClass="CustListHeader"></asp:Label>
        </div>
        <div class="col-sm-12 DivEditField">
          <asp:DropDownList ID="ddOrganizations" runat="server" CssClass="form-control"></asp:DropDownList>
        </div>
      </div>
    </div>
    <div class="row" style="height: calc(100% - 200px)">
      <div class="col-sm-3" style="height: 100%">
        <!-- Customer Panel -->

        <div class="col-sm-12">
          <asp:Label ID="Label0" runat="server" Text="Customers" CssClass="CustListHeader"></asp:Label>
        </div>
        <div class="col-sm-12" style="height: calc(100% - 50px)">
          <asp:ListBox ID="lstCustomers" runat="server" AutoPostBack="True" CssClass="form-control CustEditListbox"></asp:ListBox>
        </div>
        <div class="col-sm-12 CustEditIcon">
          <asp:LinkButton runat="server" ID="lnkNewCust" Text="<i class='fas fa-plus-square'></i>" CssClass="CustEditIcon" ToolTip="New Customer" />
          <asp:LinkButton runat="server" ID="lnkEditCust" Text="<i class='fas fa-edit'></i>" CssClass="CustEditIcon" ToolTip="Edit Customer" />
        </div>
      </div>

      <div class="col-sm-3" style="height: 100%">
        <asp:Panel ID="pnlLocations" runat="server" CssClass="CustListPanel">
          <!-- Location Panel -->
          <div class="col-sm-12">
            <asp:Label ID="Label1" runat="server" Text="Locations" CssClass="CustListHeader"></asp:Label>
          </div>
          <div class="col-sm-12" style="height: calc(100% - 50px)">
            <asp:ListBox ID="lstLocations" runat="server" AutoPostBack="True" CssClass="form-control CustEditListbox"></asp:ListBox>
          </div>
          <div class="col-sm-12 CustEditIcon">
            <asp:LinkButton runat="server" ID="lnkNewLocation" Text="<i class='fas fa-plus-square'></i>" CssClass="CustEditIcon" ToolTip="New Location" />
            <asp:LinkButton runat="server" ID="lnkEditLocation" Text="<i class='fas fa-edit'></i>" CssClass="CustEditIcon" ToolTip="Edit Location" />
          </div>
        </asp:Panel>
      </div>

      <div class="col-sm-3" style="height: 100%">
        <asp:Panel ID="pnlSubLocations" runat="server" CssClass="CustListPanel">
          <!-- Location Panel -->
          <div class="col-sm-12">
            <asp:Label ID="Label2" runat="server" Text="Sub Locations" CssClass="CustListHeader"></asp:Label>
          </div>
          <div class="col-sm-12" style="height: calc(100% - 50px)">
            <asp:ListBox ID="lstSubLocations" runat="server" AutoPostBack="True" CssClass="form-control CustEditListbox"></asp:ListBox>
          </div>
          <div class="col-sm-12 CustEditIcon">
            <asp:LinkButton runat="server" ID="lnkNewSubLoc" Text="<i class='fas fa-plus-square'></i>" CssClass="CustEditIcon" ToolTip="New Sub-location" />
            <asp:LinkButton runat="server" ID="lnkEditSubLoc" Text="<i class='fas fa-edit'></i>" CssClass="CustEditIcon" ToolTip="Edit Sub-location" />
          </div>
        </asp:Panel>
      </div>
      <div class="col-sm-3" style="height: 100%">
        <asp:Panel ID="pnlCustomerEdit" runat="server" CssClass="col-sm-12 CustEditPanel">
          <div class="col-sm-12">
            <asp:Label ID="lblEditCustHdr" runat="server" Text="" CssClass="CustListHeader"></asp:Label>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label4" runat="server" Text="Customer Name:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtEditCustName" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:TextBox ID="txtEditCustID" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
            <asp:TextBox ID="txtEditCustTS" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label5" runat="server" Text="Short Code (5 Chars):*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtEditCustShortCode" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label10" runat="server" Text="Notification Email:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtNotifyEmail" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtNotifyEmail" EnableClientScript="False" ErrorMessage="Invalid Email" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Visible="False"></asp:RegularExpressionValidator>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label12" runat="server" Text="Acoustic Leak Trigger Level:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtNotifyAcousticLevel" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label13" runat="server" Text="Acoustic Trigger Count:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtNotifyAcousticCount" runat="server" CssClass="form-control"></asp:TextBox>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label6" runat="server" Text="Customer Active" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:CheckBox ID="chkEditCustActive" runat="server" CssClass="form-control" />
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Button ID="cmdUpdateCustomer" runat="server" Text="Update Customer" CssClass="form-control AccountFormButton" />
          </div>

        </asp:Panel>
        <asp:Panel ID="pnlLocationEdit" runat="server" CssClass="col-sm-12 CustEditPanel">
          <div class="col-sm-12">
            <asp:Label ID="lblEditLocHdr" runat="server" Text="" CssClass="CustListHeader"></asp:Label>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label8" runat="server" Text="Location Name:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtEditLocName" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:TextBox ID="txtEditLocID" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
            <asp:TextBox ID="txtEditLocTS" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
          </div>

          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label9" runat="server" Text="General Location:" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:DropDownList ID="ddEditLocGeneralLoc" runat="server" CssClass="form-control"></asp:DropDownList>
          </div>

          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label7" runat="server" Text="Location Active:" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:CheckBox ID="chkEditLocActive" runat="server" CssClass="form-control" />
          </div>

          <div class="col-sm-12 EditCustRow">
            <asp:Button ID="cmdUpdateLocation" runat="server" Text="Update Location" CssClass="form-control AccountFormButton" />
          </div>

        </asp:Panel>
        <asp:Panel ID="pnlSubLocationEdit" runat="server" CssClass="col-sm-12 CustEditPanel">
          <div class="col-sm-12">
            <asp:Label ID="lblEditSubLocHdr" runat="server" Text="" CssClass="CustListHeader"></asp:Label>
          </div>
          <div class="col-sm-12 EditCustRow">
            <asp:Label ID="Label11" runat="server" Text="Sub-Location Name:*" CssClass="LabelSelectMini"></asp:Label><br />
            <asp:TextBox ID="txtEditSubLocName" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:TextBox ID="txtEditSubLocID" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
            <asp:TextBox ID="txtEditSubLocTS" runat="server" CssClass="form-control" BackColor="Lime" Visible="false"></asp:TextBox>
          </div>

          <div class="col-sm-12 EditCustRow">
            <asp:Button ID="cmdUpdateSubLoc" runat="server" Text="Update Sub Location" CssClass="form-control AccountFormButton" />
          </div>

        </asp:Panel>

      </div>

    </div>
  </div>

</asp:Content>
