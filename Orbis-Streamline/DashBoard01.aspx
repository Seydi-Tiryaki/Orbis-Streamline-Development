<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="DashBoard01.aspx.vb" Inherits="Orbis_Streamline.DashBoard01" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

  <script type="text/javascript">

    window.onresize = function () {
      $find("<%=pieUnitsByType.ClientID%>").get_kendoWidget().resize();
      $find("<%=pieCommsStatus.ClientID%>").get_kendoWidget().resize();
      $find("<%=pieDevByLocation.ClientID%>").get_kendoWidget().resize();
    }

    function toHyphens(str) {
    return str.replace(/([a-z][A-Z])/g, function (g) {
        return g.charAt(0) + '-' + g.charAt(1).toLowerCase();
    });
}

  </script>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <telerik:RadScriptManager ID="RadScriptManager1" runat="server"></telerik:RadScriptManager>
  <div class="row">
    <h1>Dashboard</h1>
  </div>

  <div class="row">
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

  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px; height: 100%">

    <div class="col-sx-12 col-sm-3" style="height: 400px">
      <telerik:RadHtmlChart ID="pieUnitsByType" runat="server" Width="100%" Height="100%" Transitions="true" CssClass="DashPie">

        <ChartTitle Text="Number of Devices by Type">
          <Appearance Align="Center" Position="Top">
            <TextStyle FontSize="1.4em" Bold="true" />
          </Appearance>
        </ChartTitle>
        <Legend>
          <Appearance Position="Right" Visible="true">
            <TextStyle FontSize="1.2em" Bold="true" Color="#090909" />
          </Appearance>
        </Legend>


        <PlotArea>
          <Series>
            <telerik:PieSeries DataFieldY="TypeCount" NameField="devtype_Name" ColorField="devtype_GraphColour" StartAngle="90">
              <LabelsAppearance Position="OutsideEnd">
                <TextStyle Bold="true" />
              </LabelsAppearance>
              <TooltipsAppearance visible="false" Color="black"></TooltipsAppearance>
              <SeriesItems>
              </SeriesItems>
            </telerik:PieSeries>
          </Series>
        </PlotArea>

      </telerik:RadHtmlChart>
    </div>

    <div class="col-sx-12 col-sm-3" style="height: 400px">
      <telerik:RadHtmlChart ID="pieCommsStatus" runat="server" Width="100%" Height="100%" Transitions="true">

        <ChartTitle Text="Device Last Comms - Number of Devices">
          <Appearance Align="Center" Position="Top">
            <TextStyle FontSize="1.4em" Bold="true" />
          </Appearance>
        </ChartTitle>
        <Legend>
          <Appearance Position="Right" Visible="true">
            <TextStyle FontSize="1.2em" Bold="true" Color="#090909" />
          </Appearance>
        </Legend>


        <PlotArea>
          <Series>
            <telerik:PieSeries DataFieldY="DevCount" NameField="AgeCheck" ColorField="AgeColour" StartAngle="90">
              <LabelsAppearance Position="OutsideEnd">
                <TextStyle Bold="true" />
              </LabelsAppearance>
              <TooltipsAppearance  visible="false" Color="black"></TooltipsAppearance>
              <SeriesItems>
              </SeriesItems>
            </telerik:PieSeries>
          </Series>
        </PlotArea>

      </telerik:RadHtmlChart>
    </div>
    <div class="col-sx-12 col-sm-3" style="height: 400px">
      <telerik:RadHtmlChart ID="pieDevByLocation" runat="server" Width="100%" Height="100%" Transitions="true">

        <ChartTitle Text="Devices By Location - Number of Devices">
          <Appearance Align="Center" Position="Top">
            <TextStyle FontSize="1.4em" Bold="true" />
          </Appearance>
        </ChartTitle>
        <Legend>
          <Appearance Position="right" Visible="true">
            <TextStyle FontSize="1.2em" Bold="true" Color="#090909" />
          </Appearance>
        </Legend>


        <PlotArea>
          <Series>
            <telerik:PieSeries DataFieldY="DevCount" NameField="loc_name" StartAngle="90">
              <LabelsAppearance Position="OutsideEnd">
                <TextStyle Bold="true" />
              </LabelsAppearance>
              <TooltipsAppearance  visible="false" Color="black"></TooltipsAppearance>
              <SeriesItems>
              </SeriesItems>
            </telerik:PieSeries>
          </Series>
        </PlotArea>

      </telerik:RadHtmlChart>
    </div>
    <div class="col-sx-12 col-sm-3" style="height: 400px">
      <a href="DeviceStatusList.aspx">
        <telerik:RadHtmlChart ID="pieAlerts" runat="server" Width="100%" Height="100%" Transitions="true">

          <ChartTitle Text="Device Status - Last 24 Hours">
            <Appearance Align="Center" Position="Top">
              <TextStyle FontSize="1.4em" Bold="true" />
            </Appearance>
          </ChartTitle>
          <Legend>
            <Appearance Position="Right" Visible="true">
              <TextStyle FontSize="1.2em" Bold="true" Color="#090909" />
            </Appearance>
          </Legend>


          <PlotArea>
            <Series>
              <telerik:PieSeries DataFieldY="Kounter" NameField="DevStatus" ColorField="AlertColour" StartAngle="90">
                <LabelsAppearance Position="OutsideEnd" DataFormatString='{0}%'>
                  <TextStyle Bold="true" />
                  
                </LabelsAppearance>
                <TooltipsAppearance  visible="false" Color="black" DataFormatString="{0}%"></TooltipsAppearance>
                <SeriesItems>
                </SeriesItems>
              </telerik:PieSeries>
            </Series>
          </PlotArea>

        </telerik:RadHtmlChart>
      </a>
    </div>
  </div>
  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px; height: 100%">
    <div class="col-lg-12" style="font-size: 2.0em">
      Alerts
    </div>
    <div class="col-sx-12 col-sm-6" style="min-height: 600px">
      <div class="col-lg-12">
        <asp:GridView ID="gvAlerts" runat="server" AutoGenerateColumns="False" CellPadding="5" CssClass="table table-responsive table-striped table-hover" GridLines="Horizontal">
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
            <asp:TemplateField HeaderText="Alert Details" HeaderStyle-HorizontalAlign="Left">
              <ItemStyle HorizontalAlign="Left" CssClass="" />
              <ItemTemplate>
                <asp:Label ID="lblDeviceStatus" runat="server" Text=''></asp:Label>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Device Data" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="gridHeaderCenter">
              <ItemStyle HorizontalAlign="center" />
              <ItemTemplate>
                <asp:HyperLink ID="hypDeviceData" runat="server" ImageUrl="~/Images/GraphButton50.png" CssClass="GridIconLink" Target="_blank">Device Data</asp:HyperLink>
              </ItemTemplate>
            </asp:TemplateField>

          </Columns>

          <RowStyle CssClass="ReportRow" />

        </asp:GridView>

      </div>
    </div>
    <div class="col-sx-12 col-sm-6" style="min-height: 600px">
      <telerik:RadMap RenderMode="Lightweight" runat="server" ID="mapDevices" Zoom="4" CssClass="MyMap">
        <CenterSettings Latitude="39.25" Longitude="-100.5" />
        <LayersCollection>
          <telerik:MapLayer Type="Tile" Subdomains="a,b,c"
            UrlTemplate="https://#= subdomain #.tile.openstreetmap.org/#= zoom #/#= x #/#= y #.png"
            Attribution="&copy; <a href='http://osm.org/copyright' title='OpenStreetMap contributors' target='_blank'>OpenStreetMap contributors</a>.">
          </telerik:MapLayer>
        </LayersCollection>


        <DataBindings>
          <MarkerBinding
            DataShapeField="MapShape"
            DataTitleField="MapTitle"
            DataLocationLatitudeField="dev_Latitude"
            DataLocationLongitudeField="dev_Longitude"
            DataTooltipContentField="MapToolTip" /> 
        </DataBindings>


      </telerik:RadMap>

    </div>

  </div>



</asp:Content>
