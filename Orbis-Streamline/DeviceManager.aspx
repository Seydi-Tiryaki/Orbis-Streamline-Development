<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="DeviceManager.aspx.vb" Inherits="Orbis_Streamline.DeviceManager" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <script src="js/ChartCustom.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <telerik:RadScriptManager ID="RadScriptManager1" runat="server"></telerik:RadScriptManager>
  <div class="row">
    <h1>Device Management</h1>
  </div>
  <asp:Panel ID="pnlDeviceList" runat="server">
    <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px">
      <div class="col-sx-12 col-sm-3">
        <asp:Label ID="Label1" runat="server" Text="Organisation" CssClass="LabelSelectMini"></asp:Label><br />
        <asp:DropDownList ID="ddorganizations" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
      </div>
      <div class="col-sx-3 col-sm-3">
        <asp:Label ID="Label2" runat="server" Text="Customer" CssClass="LabelSelectMini"></asp:Label><br />
        <asp:DropDownList ID="ddCustomers" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
      </div>
      <div class="col-sx-3 col-sm-3">
        <asp:Label ID="Label3" runat="server" Text="Location" CssClass="LabelSelectMini"></asp:Label><br />
        <asp:DropDownList ID="ddLocations" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
      </div>
      <div class="col-sx-3 col-sm-3">
        <asp:Label ID="Label4" runat="server" Text="Sub-Location" CssClass="LabelSelectMini"></asp:Label><br />
        <asp:DropDownList ID="ddsubLocations" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
      </div>
    </div>

    <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px">
      <asp:GridView ID="gvDevices" runat="server" AutoGenerateColumns="False" CellPadding="5" BorderColor="GhostWhite" CssClass="table table-responsive table-striped table-hover" GridLines="Horizontal">
        <HeaderStyle BackColor="#CCCCCC" HorizontalAlign="Left" />
        <AlternatingRowStyle CssClass="ReportRow" />
        <Columns>
          <asp:TemplateField HeaderText="Device ID" Visible="false">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblDevID" runat="server" Text='<%# Bind("dev_id")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Asset #">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblAssetTag" runat="server" Text='<%# Bind("dev_AssetTag")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="IMEI">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:LinkButton CommandName="Edit" CommandArgument='<%# Bind("dev_id")%>' CssClass="hypSmall" Text='<%# Bind("dev_IMEI")%>' runat="server" ID="lnkEmeiEdit" CausesValidation="False" />
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Customer" >
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblCustomer" runat="server" Text='<%# Bind("cust_name")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Location">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblLocation" runat="server" Text='<%# Bind("loc_name")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Sub-Location">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblSubLocation" runat="server" Text='<%# Bind("subloc_name")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Location Ref.">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblLocationRef" runat="server" Text='<%# Bind("dev_LocationRef")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Notes">
            <ItemStyle HorizontalAlign="Left" />
            <ItemTemplate>
              <asp:Label ID="lblNotes" runat="server" Text='<%# Bind("dev_InternalNotes")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="PIC18 Ver" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:Label ID="lblPic18" runat="server" Text='<%# Bind("dev_PIC18Version")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="PIC32 Ver" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:Label ID="lblPic32" runat="server" Text='<%# Bind("dev_PIC32Version")%>'></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Last Data" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:Label ID="lblLastData" runat="server" Text=''></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Device Live" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:CheckBox ID="ChkLive" runat="server" Enabled="false"  />
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Device Data" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:HyperLink ID="hypDeviceData" runat="server" ImageUrl="~/Images/GraphButton50.png" CssClass="GridIconLink" Target="_blank">Device Data</asp:HyperLink>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:LinkButton CommandName="Edit" CommandArgument='<%# Bind("dev_id")%>' CssClass="hypSmall" Text="Edit" runat="server" ID="lnkV2fEdit" CausesValidation="False" />

            </ItemTemplate>
          </asp:TemplateField>

        </Columns>

        <RowStyle CssClass="ReportRow" />

      </asp:GridView>
    </div>
  </asp:Panel>
  <asp:Panel ID="pnlDeviceEdit" runat="server" Height="100%">
    <asp:Panel ID="pnlOverlay" runat="server" class="Overlay" Visible="false">
      <asp:Panel ID="pnlPopUpPanel" runat="server" class="PopUpPanel row" Visible="false">
        <asp:Panel ID="pnlPopUpTitle" runat="server" Style="width: 100%;" CssClass="PopUpTitle col-sm-12">
          <div style="text-align: left" class="col-sm-10">
            <asp:Label ID="lblEditTitle" runat="server" Text="Label" CssClass="h2"></asp:Label>
          </div>
          <div style="text-align: right" class="col-sm-2">
            <asp:ImageButton ID="cmdClosePopUp" runat="server" ImageUrl="~/Images/CloseCross.png"></asp:ImageButton>
          </div>
        </asp:Panel>

        <div class="col-lg-12" style="margin-top: 15px;">
          
          <div class="row RowEdit" style="display:none">
            <div class="col-sm-5">
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtDevID" runat="server" Visible="false"></asp:TextBox>
              <asp:TextBox ID="txtDevTS" runat="server" Visible="false"></asp:TextBox>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label9" runat="server" CssClass=" form-label">Asset Tag:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtAssetTag" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label12" runat="server" CssClass=" form-label">Device Model:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddDevModel" runat="server" CssClass="form-control" Enabled="false"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label13" runat="server" CssClass=" form-label">Device Type:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddDevType" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label35" runat="server" CssClass=" form-label">Organisation:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddEditOrganisation" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label5" runat="server" CssClass=" form-label">Customer:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddEditCustomer" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label6" runat="server" CssClass=" form-label">Location:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddEditLocation" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label7" runat="server" CssClass=" form-label">Sub-location:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddEditSubLocation" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label8" runat="server" CssClass=" form-label">Location Ref:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtLocationRef" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
          </div>


          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label10" runat="server" CssClass=" form-label">Pipe Orientation:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddPipeOrient" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label11" runat="server" CssClass=" form-label">Pipe Diameter:*</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:DropDownList ID="ddPipeDiameter" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label14" runat="server" CssClass=" form-label">Serial No.:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtSerialNo" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label19" runat="server" CssClass=" form-label">Device is Live:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:CheckBox ID="chkDevLive" runat="server" CssClass="form-control" />
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label20" runat="server" CssClass=" form-label">Notes:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtInternalNote" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"></asp:TextBox>
            </div>
          </div>


          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label15" runat="server" CssClass=" form-label">Latitude:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtLatitute" runat="server" CssClass="form-control" ></asp:TextBox>
            </div>
          </div>

          <div class="row RowEdit">
            <div class="col-sm-5">
              <asp:Label ID="Label16" runat="server" CssClass=" form-label">Longitude:</asp:Label>
            </div>
            <div class="col-sm-7">
              <asp:TextBox ID="txtLongitude" runat="server" CssClass="form-control" ></asp:TextBox>
            </div>
          </div>



          <div class="col-sm-12" style="border-top: solid 1px  #17A3E1; margin-top: 10px; margin-bottom: 10px;"></div>
          <div class="row RowEdit">
            <div class="col-sm-6">
            </div>
            <div class="col-sm-3">
              <asp:Button ID="cmdCancel" runat="server" Text="Cancel" CssClass="form-control cmdButton" />
            </div>
            <div class="col-sm-3">
              <asp:Button ID="cmdUpdate" runat="server" Text="Update" CssClass="form-control cmdButton" />
            </div>
          </div>



        </div>

      </asp:Panel>
    </asp:Panel>
  </asp:Panel>


</asp:Content>
