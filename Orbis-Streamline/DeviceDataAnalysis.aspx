<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="DeviceDataAnalysis.aspx.vb" Inherits="Orbis_Streamline.DeviceDataAnalysis" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <script type="text/javascript">

    window.onresize = function () {
      $find("<%=lineDailyFlowEvents.ClientID%>").get_kendoWidget().resize();
      $find("<%=lineuSonicFlow.ClientID%>").get_kendoWidget().resize();
      $find("<%=LineLeakageDetailed.ClientID%>").get_kendoWidget().resize();

      $find("<%=LineVibration.ClientID%>").get_kendoWidget().resize();
      $find("<%=linePipeCondition.ClientID%>").get_kendoWidget().resize();
      $find("<%=linePressue.ClientID%>").get_kendoWidget().resize();

      $find("<%=lineDeviceCheckins.ClientID%>").get_kendoWidget().resize();
      $find("<%=lineDailyAvgBattery.ClientID%>").get_kendoWidget().resize();
      $find("<%=lineDailyAvgBoardTemp.ClientID%>").get_kendoWidget().resize();
    }

    $(document).ready(function () {
      $("#ctl00_contentBody_LineLeakage").attr('style', "background-image:url('Images/AlgoEnhancedWatermark-01.jpg');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");
      /*$("#ctl00_contentBody_LineVibration").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");
      $("#ctl00_contentBody_lineuSonicFlow").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");
      $("#ctl00_contentBody_linePressue").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");*/
      $("#ctl00_contentBody_linePipeCondition").attr('style', "background-image:url('Images/AlgoEnhancedWatermark-01.jpg');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");

      /* $("#ctl00_contentBody_colPressure2").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");
       $("#ctl00_contentBody_linePipeCondition").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");
       $("#ctl00_contentBody_lineLegionella").attr('style', "background-image:url('Images/SampleWatermark.png');background-size: 100% 100%;; background-repeat: no-repeat; background-color:#ffffff");*/
    });

  </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <telerik:RadScriptManager ID="RadScriptManager1" runat="server"></telerik:RadScriptManager>
  <div class="row">
    <h1>Device Data Analysis</h1>
    <asp:TextBox ID="txtDevID" runat="server" Visible="false"></asp:TextBox>
  </div>
  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 10px; padding-top: 10px">
    <div class="col-sx-12 col-sm-8">
      <asp:Label ID="lblDeviceDetails" runat="server" Text="" CssClass="DeviceDataHeader"></asp:Label><br />
    </div>

    <div class="col-sx-12 col-sm-4">
      <asp:Label ID="Label1" runat="server" Text="Select Date Range:" CssClass="LabelSelectMini"></asp:Label><br />
      <asp:DropDownList ID="ddDateRange" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
    </div>
  </div>

  <div class="col-sx-12 col-sm-4 ChartHeight ChartPanel" style=" display: none">
    <telerik:RadHtmlChart runat="server" ID="lineDailyFlowEvents" Width="100%" CssClass="ChartHeight" BackColor="#ffffff">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Events" DataFieldX="DateSeq" DataFieldY="FlowEventCount" VisibleInLegend="true">
            <TooltipsAppearance Color="White" DataFormatString="{1} -- {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false">
            </LabelsAppearance>
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis>
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="No. of Events" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Flow Activity">
      </ChartTitle>
    </telerik:RadHtmlChart>
    <div class="col-sx-12">

    </div>
  </div>
  <div class="col-sx-12 col-sm-4 ChartPanel">
    <telerik:RadHtmlChart runat="server" ID="lineuSonicFlow" Width="100%" BackColor="#ffffff" CssClass="ChartHeight ">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Ft/s" DataFieldX="dates" DataFieldY="est_flow" VisibleInLegend="true">
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="1" />
            <MarkersAppearance Size="3" />
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" BaseUnitStep="1" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis MinValue="0" MaxValue="10">
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="ft/s" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Ultrasonic Flow">
      </ChartTitle>
      <Legend>
        <Appearance Position="Bottom" Align="Center">
        </Appearance>
      </Legend>
    </telerik:RadHtmlChart>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label3" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label4" runat="server" CssClass="GraphInfoText" Text="Leak Acoustic: Green: Non-leak- threshold not broken;  Orange: Elevated level- threshold broken first time <br/> Red: Alert– values above threshold" Visible="false"></asp:Label>
    </div>

  </div>

  <div class="col-sx-12 col-sm-4 ChartPanel ">

    <asp:HyperLink ID="hypLeakDetails" runat="server" Target="DataProcessed">
      <telerik:RadHtmlChart runat="server" ID="LineLeakage" Width="100%"  BackColor="#ffffff" Visible="false" CssClass="ChartHeight">
        <PlotArea>
          <Series>
            <telerik:LineSeries Name="LeakProbability" VisibleInLegend="false">
              <Appearance FillStyle-BackgroundColor="DarkBlue"></Appearance>
              <TooltipsAppearance Color="White" DataFormatString="Acoustic Probability: {0}"></TooltipsAppearance>
              <LabelsAppearance Visible="false">
              </LabelsAppearance>
            </telerik:LineSeries>
            <telerik:LineSeries Name="CordLeak" VisibleInLegend="false" AxisName="Yes/No">
              <Appearance FillStyle-BackgroundColor="Red"></Appearance>
              <TooltipsAppearance Color="White" DataFormatString="Cord Leak Detected: {0}"></TooltipsAppearance>
              <LabelsAppearance Visible="false">
              </LabelsAppearance>
            </telerik:LineSeries>
            <telerik:LineSeries Name="LeakThreshold" VisibleInLegend="false">
              <Appearance FillStyle-BackgroundColor="orange"></Appearance>
              <LineAppearance Width="2" />
              <MarkersAppearance Visible="false" />
              <TooltipsAppearance Color="White" DataFormatString="Leak Threshold: {0}"></TooltipsAppearance>
              <LabelsAppearance Visible="false">
              </LabelsAppearance>
            </telerik:LineSeries>
          </Series>
          <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
            <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="45" />
            <TitleAppearance Text="" />
            <AxisCrossingPoints>
              <telerik:AxisCrossingPoint Value="0" />
              <telerik:AxisCrossingPoint Value="9999999" />
            </AxisCrossingPoints>
          </XAxis>
          <YAxis MinValue="0" MaxValue="10" Color="DarkBlue">
            <LabelsAppearance DataFormatString="{0}" />
            <TitleAppearance Text="Acoustic Probability" />
          </YAxis>
          <AdditionalYAxes>
            <telerik:AxisY Name="Yes/No" Color="Red" MinValue="0" MaxValue="1.2" Step="1">
              <TitleAppearance Text="Cord Leak Y/N" />
            </telerik:AxisY>
          </AdditionalYAxes>
        </PlotArea>
        <ChartTitle Text="Leakage (Acoustic and/or Conductive)">
        </ChartTitle>
      </telerik:RadHtmlChart>
        <!-- ToolTip="Leak Acoustic &#013; Green: Non-leak- threshold not broken &#013; Orange: Elevated level- threshold broken first time &#013; Red: Alert– values above threshold" -->
      <telerik:RadHtmlChart runat="server" ID="LineLeakageDetailed" Width="100%" CssClass="ChartHeight " BackColor="#ffffff" >
        <PlotArea>
          <Series>
            <telerik:ScatterLineSeries Name="Q Factor" DataFieldX="LeakDate" DataFieldY="LeakProb" VisibleInLegend="true" ColorField="LeakColor">
              <Appearance FillStyle-BackgroundColor="Green"></Appearance>
              <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy HH:mm}"></TooltipsAppearance>
              <LabelsAppearance Visible="false"></LabelsAppearance>
              <MarkersAppearance Size="5" MarkersType="Circle"/>
              <LineAppearance Width="0" />
            </telerik:ScatterLineSeries>
            <telerik:ScatterLineSeries Name="Conductive" DataFieldX="LeakDate" DataFieldY="LeakConductive" VisibleInLegend="true" ColorField="LeakColorCond">
              <Appearance FillStyle-BackgroundColor="Blue"></Appearance>
              <TooltipsAppearance Color="White" DataFormatString="Conductive Leak - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
              <LabelsAppearance Visible="false"></LabelsAppearance>
              <MarkersAppearance Size="10" MarkersType="Square"/>
              <LineAppearance Width="2"  />
            </telerik:ScatterLineSeries>
          </Series>
          <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
            <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
            <TitleAppearance Text="" />
          </XAxis>
          <YAxis MinValue="0" MaxValue="12">
            <LabelsAppearance DataFormatString="{0}" />
            <TitleAppearance Text="Acoustic Propability" />
          </YAxis>
        </PlotArea>
        <ChartTitle Text="Leakage (Acoustic and/or Conductive)">
        </ChartTitle>
        <Legend>
          <Appearance Position="Bottom" Align="Center">
          </Appearance>
        </Legend>
      </telerik:RadHtmlChart>
    </asp:HyperLink>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="lblLeakTooltip" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label2" runat="server" CssClass="GraphInfoText" Text="Leak Acoustic: Green: Non-leak- threshold not broken;  Orange: Elevated level- threshold broken first time <br/> Red: Alert– values above threshold"></asp:Label>
    </div>
  </div>


  <div class="col-sx-12 col-sm-4  " style=" display: none">
    <telerik:RadHtmlChart runat="server" ID="LineVibration" Width="100%" BackColor="#ffffff" CssClass="ChartHeight ChartPanel">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Events" DataFieldX="DateSeq" DataFieldY="FlowEventCount" VisibleInLegend="False">
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false">
            </LabelsAppearance>
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis>
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="No. of Events" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Vibration">
      </ChartTitle>
    </telerik:RadHtmlChart>
  </div>
  <div class="col-sx-12 col-sm-4  ChartPanel">
    <asp:HyperLink ID="hypPipeCondDetails" runat="server" Target="DataProcessed">
      <telerik:RadHtmlChart runat="server" ID="linePipeCondition" Width="100%" CssClass="ChartHeight " BackColor="#ffffff">
        <PlotArea>
          <Series>
            <telerik:ScatterLineSeries Name="Resonant Frequency" DataFieldX="dates" DataFieldY="Cond" VisibleInLegend="False">
              <TooltipsAppearance Color="White" DataFormatString="{1:##0.000} - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
              <LabelsAppearance Visible="false">
              </LabelsAppearance>
            </telerik:ScatterLineSeries>
          </Series>
          <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
            <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
            <TitleAppearance Text="" />
          </XAxis>
          <YAxis MinValue="0" MaxValue="10">
            <LabelsAppearance DataFormatString="{0}" />
            <TitleAppearance Text="Resonant Frequency" />
          </YAxis>
        </PlotArea>
        <ChartTitle Text="Pipe Condition">
        </ChartTitle>
      </telerik:RadHtmlChart>
    </asp:HyperLink>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label5" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label6" runat="server" CssClass="GraphInfoText" Text="Leak Acoustic: Green: Non-leak- threshold not broken;  Orange: Elevated level- threshold broken first time <br/> Red: Alert– values above threshold" Visible="false"></asp:Label>
    </div>
  </div>

  <div class="col-sx-12 col-sm-4 ChartPanel ">
    <telerik:RadHtmlChart runat="server" ID="linePressue" Width="100%" cssclass="ChartHeight " BackColor="#ffffff">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Stress" DataFieldX="DateGroup" DataFieldY="avgStrain" VisibleInLegend="false">
            <Appearance FillStyle-BackgroundColor="orange"></Appearance>
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2" />
            <MarkersAppearance Size="10" BackgroundColor="white" BorderColor="orange" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="Moving Average" DataFieldX="DateGroup" DataFieldY="_7dayMovingAvg" VisibleInLegend="true">
            <Appearance FillStyle-BackgroundColor="green"></Appearance>
            <TooltipsAppearance Color="white" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="1" />
            <MarkersAppearance Size="0" BackgroundColor="Green" BorderColor="Green" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="+Trigger" DataFieldX="DateGroup" DataFieldY="_7dayMovingAvgPlus50" VisibleInLegend="true" ColorField="ColourMid">
            <Appearance FillStyle-BackgroundColor="Yellow"></Appearance>
            <TooltipsAppearance Color="Black" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2"></LineAppearance>
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="-Trigger" DataFieldX="DateGroup" DataFieldY="_7dayMovingAvgMinus50" VisibleInLegend="true" ColorField="ColourMid">
            <Appearance FillStyle-BackgroundColor="Yellow"></Appearance>
            <TooltipsAppearance Color="Black" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2" />
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="+Warning" DataFieldX="DateGroup" DataFieldY="_7dayMovingAvgPlus95" VisibleInLegend="true" ColorField="ColourHigh">
            <Appearance FillStyle-BackgroundColor="Red"></Appearance>
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="1" />
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="-Warning" DataFieldX="DateGroup" DataFieldY="_7dayMovingAvgMinus95" VisibleInLegend="true" ColorField="ColourHigh">
            <Appearance FillStyle-BackgroundColor="Red"></Appearance>
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="1" />
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" BaseUnitStep="1" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis>
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="Hoop Stress" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Pipe Pressure">
      </ChartTitle>

       <Legend>
       <Appearance Position="Bottom" Align="Center">
       </Appearance>
       </Legend>

    </telerik:RadHtmlChart>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label7" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label8" runat="server" CssClass="GraphInfoText" Text="Leak Acoustic: Green: Non-leak- threshold not broken;  Orange: Elevated level- threshold broken first time <br/> Red: Alert– values above threshold" Visible="false"></asp:Label>
    </div>
  </div>

  <div class="col-sx-12 col-sm-4  ChartPanel" >
    <telerik:RadHtmlChart runat="server" ID="lineDeviceCheckins" Width="100%" CssClass="ChartHeight " BackColor="#ffffff">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Checkins" DataFieldX="DateGroup" DataFieldY="CheckInCount" VisibleInLegend="true">
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false">
            </LabelsAppearance>
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis>
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="Reports / Day" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Device Communications">
      </ChartTitle>
      <Legend>
        <Appearance Position="Bottom" Align="Center">
        </Appearance>
      </Legend>
    </telerik:RadHtmlChart>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label9" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label10" runat="server" CssClass="GraphInfoText" Text="How many times per day the device has communicated with StreamLine Portal" ></asp:Label>
    </div>
  </div>

  <div class="col-sx-12 col-sm-4  ChartPanel" >
    <telerik:RadHtmlChart runat="server" ID="lineDailyAvgBattery" Width="100%" CssClass="ChartHeight " BackColor="#ffffff">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Avg Battery Level" DataFieldX="DateGroup" DataFieldY="avgBattery" VisibleInLegend="true">
            <TooltipsAppearance Color="White" DataFormatString="{1} - {0}"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="7DMA Upper" DataFieldX="DateGroup" DataFieldY="avgBattery7DMAwarnUpper" VisibleInLegend="false">
            <Appearance FillStyle-BackgroundColor="Yellow"></Appearance>
            <TooltipsAppearance Color="Black" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2"></LineAppearance>
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="7DMA Lower" DataFieldX="DateGroup" DataFieldY="avgBattery7DMAwarnLower" VisibleInLegend="false">
            <Appearance FillStyle-BackgroundColor="Yellow"></Appearance>
            <TooltipsAppearance Color="Black" DataFormatString="{1} - {0:ddd dd MMM yyyy}" Visible="False"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2"></LineAppearance>
            <MarkersAppearance Size="0" />
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" BaseUnitStep="1" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis MaxValue="600" MinValue="300">
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="Unit" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Battery">
      </ChartTitle>
                   <Legend>
          <Appearance Position="Bottom" Align="Center">
          </Appearance>
        </Legend>
    </telerik:RadHtmlChart>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label11" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label12" runat="server" CssClass="GraphInfoText" Text="Nominal battery Level of the device" Visible="true"></asp:Label>
    </div>
  </div>
  <div class="col-sx-12 col-sm-4 ChartPanel " >
    <telerik:RadHtmlChart runat="server" ID="lineBoardTempMA" Width="100%" CssClass="ChartHeight " BackColor="#ffffff">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Pipe Temperature" DataFieldX="DateGroup" DataFieldY="avgPipeTemp" VisibleInLegend="true" Visible="true">
            <Appearance FillStyle-BackgroundColor="blue"></Appearance>
            <TooltipsAppearance Color="White" DataFormatString="{1:N1}&#176;C - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2" />
            <MarkersAppearance Size="5" BackgroundColor="blue" BorderColor="blue" />
          </telerik:ScatterLineSeries>
          <telerik:ScatterLineSeries Name="Board Temperature" DataFieldX="DateGroup" DataFieldY="avgBoardTemp" VisibleInLegend="true">
            <Appearance FillStyle-BackgroundColor="orange"></Appearance>
            <TooltipsAppearance Color="White" DataFormatString="{1:N1}&#176;C - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false"></LabelsAppearance>
            <LineAppearance Width="2" />
            <MarkersAppearance Size="5" BackgroundColor="orange" BorderColor="orange" />
          </telerik:ScatterLineSeries>

        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" BaseUnitStep="1" Step="7">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis>
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="c" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Average Device Temperature">
      </ChartTitle>
      <Legend>
        <Appearance Position="Bottom" Align="Center">
        </Appearance>
      </Legend>
    </telerik:RadHtmlChart>
    <div class="GraphInfoPanel">
      <asp:label runat="server" ID="Label13" Text="<i class='fas fa-info-circle'></i>" CssClass="GraphInfoIcon" />
      <asp:Label ID="Label14" runat="server" CssClass="GraphInfoText" Text="Pipe and device board temperatures" ></asp:Label>
    </div>
    <telerik:RadHtmlChart runat="server" ID="lineDailyAvgBoardTemp" Width="100%" CssClass="ChartHeight ChartPanel" BackColor="#ffffff" Visible="false">
      <PlotArea>
        <Series>
          <telerik:ScatterLineSeries Name="Amb" DataFieldX="DateGroup" DataFieldY="avgTemp" VisibleInLegend="false">
            <TooltipsAppearance Color="White" DataFormatString="{1}&#176;C - {0:ddd dd MMM yyyy}"></TooltipsAppearance>
            <LabelsAppearance Visible="false">
            </LabelsAppearance>
          </telerik:ScatterLineSeries>
        </Series>
        <XAxis Type="Date" MajorTickSize="10px" StartAngle="0" BaseUnit="Days" MinorTickType="Outside" Step="3">
          <LabelsAppearance DataFormatString="yyyy-MM-dd" RotationAngle="25" />
          <TitleAppearance Text="" />
        </XAxis>
        <YAxis MinValue="10" MaxValue="40">
          <LabelsAppearance DataFormatString="{0}" />
          <TitleAppearance Text="&#176;C" />
        </YAxis>
      </PlotArea>
      <ChartTitle Text="Average Temperature">
      </ChartTitle>
      <Legend>
        <Appearance Position="Bottom" Align="Center">
        </Appearance>
      </Legend>
    </telerik:RadHtmlChart>
  </div>
</asp:Content>
