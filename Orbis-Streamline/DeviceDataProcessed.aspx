<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="DeviceDataProcessed.aspx.vb" Inherits="Orbis_Streamline.DeviceDataProcessed" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <telerik:RadScriptManager ID="RadScriptManager1" runat="server"></telerik:RadScriptManager>
  <div class="row">
    <h1>Device Data Processed Reports</h1>
  </div>
  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 10px; padding-top: 10px">
    <div class="col-sm-12">
      <asp:TextBox runat="server" BackColor="Lime" CssClass="miniTextBox" TabIndex="-1" Width="75px" ID="txtFormRef" Visible="False"></asp:TextBox>

      <asp:Label ID="lblDeviceDetails" runat="server" Text="" CssClass="DeviceDataHeader"></asp:Label>
      <asp:TextBox runat="server" BackColor="Lime" CssClass="miniTextBox" TabIndex="-1" Width="75px" ID="txtDevID" Visible="False"></asp:TextBox>

      <br />
    </div>

    <div class="col-sm-2">
      <asp:Label ID="Label1" runat="server" Text="Select Data Type:" CssClass="LabelSelectMini"></asp:Label><br />
      <asp:DropDownList ID="ddDataType" runat="server" CssClass="form-control" AutoPostBack="True" ></asp:DropDownList>
    </div>
    <div class="col-sm-8"></div>
    <div class="col-sm-2">
      <asp:Label ID="Label2" runat="server" Text="Select Date Range:" CssClass="LabelSelectMini"></asp:Label><br />
      <asp:DropDownList ID="ddDateRange" runat="server" CssClass="form-control" AutoPostBack="True"></asp:DropDownList>
    </div>

  </div>

  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px;">
    <div class="col-sx-12 col-sm-3" >
      <asp:ListBox ID="lstDeviceEvents" runat="server" AutoPostBack="True" CssClass="form-control" style="font-size:1.1em;font-family:monospace" Rows="30"></asp:ListBox>
    </div>

    <div class="col-sx-12 col-sm-6" style="min-height: 400px">
      <asp:Panel ID="pnlImages" runat="server" CssClass="col-sm-12"></asp:Panel>
    </div>
    <div class="col-sx-12 col-sm-3" style="padding-top: 30px" >
      <asp:Button ID="cmdGenerateAudio" runat="server" Text="Prepare Audio" CssClass="form-control AccountFormButton" />
      <asp:Panel ID="pnlPlayAudio" runat="server" Visible="false"  >
        Audio File: <br />
        <audio controls="controls"  id="audioPlayer" runat="server" style="width:100%">
            Your browser does not support the audio element.
        </audio>
      </asp:Panel>
      <div class="AlgoDescription">
        <asp:Literal ID="litDesciption" runat="server" ></asp:Literal>
        </div>
    </div>

  </div>
  <div class="row" style="border-top: solid 1px lightgrey; margin-top: 20px;">
    <div class="col-sx-12 col-sm-3">
    </div>
    <div class="col-sx-12 col-sm-6" style="text-align: center">
      <asp:Panel ID="pnlConcatCont" runat="server">
        <span style="font-size: 1.5em;">Concatenated Pipe Impluse Spectrograms</span>
        <asp:Panel ID="pnlConcat" runat="server">
        </asp:Panel>
        <span style="font-size: 1.3em;">Frequency (Hz)</span>
      </asp:Panel>
    </div>
    <div class="col-sx-12 col-sm-3" style="padding-top: 30px" >
      <div class="AlgoDescription">
        <asp:Literal ID="litConcatsDesc" runat="server" ></asp:Literal>
        </div>
    </div>

  </div>

</asp:Content>
