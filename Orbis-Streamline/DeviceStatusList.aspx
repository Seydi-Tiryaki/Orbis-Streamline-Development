<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="DeviceStatusList.aspx.vb" Inherits="Orbis_Streamline.DeviceStatusList" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <script src="js/ChartCustom.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <telerik:RadScriptManager ID="RadScriptManager1" runat="server"></telerik:RadScriptManager>
  <div class="row">
    <h1>Device Status List - Last 24 Hours</h1>
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
      <asp:GridView ID="gvDevices" runat="server" AutoGenerateColumns="False" CellPadding="5" CssClass="table table-responsive table-striped table-hover" GridLines="Horizontal">
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
          <asp:TemplateField HeaderText="Type">
            <ItemStyle HorizontalAlign="Center" />
            <ItemTemplate>
              <asp:Label ID="lblDeviceType" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Customer">
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
          <asp:TemplateField HeaderText="Flow Events" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter" Visible="false">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblFlowThermal" runat="server" Text=''></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Flow Vibration" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter" Visible="false">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblVibration" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Flow Acoustic" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter" Visible="false">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblFlowAcoustic" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Flow uSonic" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblFlowUSonic" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Pipe Temp Avg" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblPipeTemp" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Board Temp" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter" Visible="false">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblBoardTemp" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Pipe Condition" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblPipeCond" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Leak Acoustic" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblLeakAcoustic" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Leak Conductive" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblLeakConductive" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Pressure" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblPressure" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Legionella" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter" Visible="false">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblLegionella" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Battery" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblBattery" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Comms" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" CssClass="gridColStatus" />
            <ItemTemplate>
              <asp:Label ID="lblComms" runat="server" Text=""></asp:Label>
            </ItemTemplate>
          </asp:TemplateField>


          <asp:TemplateField HeaderText="Device Data" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
            <ItemStyle HorizontalAlign="center" />
            <ItemTemplate>
              <asp:HyperLink ID="hypDeviceData" runat="server" ImageUrl="~/Images/GraphButton50.png" CssClass="GridIconLink" target="DeviceDataTab">Device Data</asp:HyperLink>
            </ItemTemplate>
          </asp:TemplateField>

        </Columns>

        <RowStyle CssClass="ReportRow" />

      </asp:GridView>
    </div>
    <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px; padding-top: 10px; padding-bottom: 50px">
      <div class="col-sx-12 col-sm-3">
        <asp:CheckBox ID="chkDetailedData" runat="server" Text="Show detailed data" AutoPostBack="True" CssClass="form-control" Checked="true" />
      </div>

    </div>

  </asp:Panel>

</asp:Content>
