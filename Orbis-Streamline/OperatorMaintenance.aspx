<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/StreamLine02.Master" CodeBehind="OperatorMaintenance.aspx.vb" Inherits="Orbis_Streamline.OperatorMaintenance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="contentBody" runat="server">
  <div class="row PanelUpperHeader">
    <h1>User Maintenance</h1>
  </div>
  <asp:Panel ID="pnlHeader" runat="server">
    <div class="row PanelHeader" style="">
      <asp:LinkButton runat="server" ID="lnkNewOp" Text="<i class='fas fa-user-plus'></i>" CssClass="HeaderPanelIcon" ToolTip="New User" />
      <div class="DivSpace10"></div>
    </div>

    <div class="row" style="padding-top: 20px;">
        <asp:GridView ID="gvSearch" runat="server" AutoGenerateColumns="False" CellPadding="5" BorderColor="GhostWhite" CssClass="table table-responsive table-striped table-hover MaintenanceGrid" GridLines="Horizontal">
          <HeaderStyle HorizontalAlign="Left" />
          <AlternatingRowStyle CssClass="ReportRow" />
          <Columns>

            <asp:TemplateField HeaderText="Operator ID" Visible="true">
              <ItemStyle HorizontalAlign="Left" />
              <ItemTemplate>
                <asp:Label ID="lblOpID" runat="server" Text='<%# Bind("Operator_id")%>'></asp:Label>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Email">
              <ItemTemplate>
                <asp:Label ID="lblOpEmail" runat="server" Text='<%# Bind("Operator_Email")%>'></asp:Label>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Name">
              <ItemTemplate>
                <asp:Label ID="lblOpName" runat="server" Text='<%# Bind("Operator_Name")%>'></asp:Label>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Job Title">
              <ItemTemplate>
                <asp:Label ID="lblOpJobTitle" runat="server" Text='<%# Bind("Operator_JobTitle")%>'></asp:Label>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Suspended">
              <ItemTemplate>
                <asp:CheckBox runat="server" ID="chkActive" Checked='<%# Eval("Operator_Suspended") %>' Enabled="false"></asp:CheckBox>
              </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="">
              <ItemTemplate>
                <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-user-edit'></i>" runat="server" ID="cmdEdit1" CausesValidation="False" CssClass="GridViewEditIcon" ToolTip="Edit User"></asp:LinkButton>
              </ItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>
    </div>
  </asp:Panel>

  <asp:Panel ID="pnlDetails" runat="server" Width="100%" HorizontalAlign="Center" BorderWidth="0px" Visible="False" CssClass="MaintenanceEdit">
    <div class="row">
      <div class="col-sm-2 DivEditLabel">
        Email:         
        <asp:TextBox ID="txtID" runat="server" Visible="false" Width="50px" BackColor="lime"></asp:TextBox><asp:TextBox ID="txtTS" runat="server" Visible="false" Width="50px" BackColor="lime"></asp:TextBox>
      </div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>
        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" EnableClientScript="False" ErrorMessage="Invalid Email" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Visible="False"></asp:RegularExpressionValidator>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-2 DivEditLabel">Name:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtName" runat="server" CssClass="form-control"></asp:TextBox>
      </div>
    </div>

    <div class="row">
      <div class="col-sm-2 DivEditLabel">Job Title:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtJob" runat="server" CssClass="form-control"></asp:TextBox>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-2 DivEditLabel">Telephone:</div>
      <div class="col-sm-10 DivEditField">
        <asp:TextBox ID="txtTelephone" runat="server" CssClass="form-control"></asp:TextBox>
      </div>
    </div>
    <div class="row">
      <div class="col-sm-2 DivEditLabel">Suspended:</div>
      <div class="col-sm-10 DivEditField">
        <asp:CheckBox ID="chkSuspended" runat="server" Checked="false"></asp:CheckBox>
      </div>
    </div>
    <asp:Panel ID="pnlSuperUser1" runat="server">
      <div class="row">
        <div class="col-sm-2 DivEditLabel">SuperUser:</div>
        <div class="col-sm-10 DivEditField">
          <asp:CheckBox ID="chkSuperUser" runat="server" Checked="false"></asp:CheckBox> 
        </div>
      </div>
    </asp:Panel>
    <asp:Panel ID="PnlOrgs" runat="server">
      <div class="row">
        <div class="col-sm-2 DivEditLabel">Organizations:</div>
        <div class="col-sm-10 DivEditField">
          <div class="col-sm-5">
            Available Organizations:
          <asp:ListBox ID="lstAvailOrgs" runat="server" CssClass="form-control" Width="100%" Rows="3"></asp:ListBox>

          </div>
          <div class="col-sm-2 EditArrowDiv">
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-right'></i>" runat="server" ID="lnkAddOrg" CausesValidation="False" CssClass="EditArrow" ToolTip="Add Organization"></asp:LinkButton>
            <br />
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-left'></i>" runat="server" ID="lnkRemoveOrg" CausesValidation="False" CssClass="EditArrow" ToolTip="Remove Organization"></asp:LinkButton>
          </div>
          <div class="col-sm-5">
            Selected Organizations:
          <asp:ListBox ID="lstSelectedOrgs" runat="server" CssClass="form-control" Width="100%" Rows="3"></asp:ListBox>
          </div>
        </div>
      </div>


      <div class="row">
        <div class="col-sm-2 DivEditLabel">Customers:</div>
        <div class="col-sm-10 DivEditField">
          <div class="col-sm-5">
            Available Customers:
          <asp:ListBox ID="lstAvailCusts" runat="server" CssClass="form-control" Width="100%" Rows="6"></asp:ListBox>

          </div>
          <div class="col-sm-2 EditArrowDiv">
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-right'></i>" runat="server" ID="lnkAddCustomer" CausesValidation="False" CssClass="EditArrow" ToolTip="Add Customer"></asp:LinkButton>
            <br />
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-left'></i>" runat="server" ID="lnkRemoveCustomer" CausesValidation="False" CssClass="EditArrow" ToolTip="Remove Customer"></asp:LinkButton>
          </div>
          <div class="col-sm-5">
            Selected Customers:
          <asp:ListBox ID="lstSelectedCusts" runat="server" CssClass="form-control" Width="100%" Rows="6"></asp:ListBox>
          </div>


        </div>

      </div>
      <div class="row">
        <div class="col-sm-2 DivEditLabel">Locations:</div>
        <div class="col-sm-10 DivEditField">
          <div class="col-sm-5">
            Available Locations:
          <asp:ListBox ID="lstAvailLocs" runat="server" CssClass="form-control" Width="100%" Rows="9"></asp:ListBox>

          </div>
          <div class="col-sm-2 EditArrowDiv">
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-right'></i>" runat="server" ID="lnkAddLoc" CausesValidation="False" CssClass="EditArrow" ToolTip="Add Location"></asp:LinkButton>
            <br />
            <asp:LinkButton CommandArgument='<%# Eval("Operator_ID") %>' CommandName="Edit" Text="<i class='fas fa-angle-left'></i>" runat="server" ID="lnkRemoveLoc" CausesValidation="False" CssClass="EditArrow" ToolTip="Remove Location"></asp:LinkButton>
          </div>
          <div class="col-sm-5">
            Selected Locations:
          <asp:ListBox ID="lstSelectedLocs" runat="server" CssClass="form-control" Width="100%" Rows="9"></asp:ListBox>
          </div>


        </div>

      </div>

    </asp:Panel>

    <div class="row">
      <div class="col-sm-2 DivEditLabel">Permissions:</div>
      <div class="col-sm-10 DivEditField">
        <div class="DataGridRound1">
          <asp:DataGrid ID="dgPerms" runat="server" AutoGenerateColumns="False"  CellPadding="5" BorderColor="GhostWhite" CssClass="table table-responsive table-striped table-hover MaintenanceGrid" GridLines="Horizontal">
            <EditItemStyle CssClass="DataGrid1" />
            <AlternatingItemStyle CssClass="ReportRow" />
            <ItemStyle CssClass="ReportRow" />
            <HeaderStyle CssClass="DataGridHeader1" />
            <Columns>
              <asp:TemplateColumn HeaderText="ID" Visible="false">
                <HeaderStyle HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" />
                <ItemTemplate>
                  <asp:Label ID="lblPermGroup_ID" runat="server" Text='<%#Eval("PermGroup_ID")%>'></asp:Label>
                  <asp:Label ID="lblperm_id" runat="server" Text='<%#Eval("perm_id")%>'></asp:Label>
                </ItemTemplate>
              </asp:TemplateColumn>
              <asp:TemplateColumn HeaderText="Group">
                <HeaderStyle HorizontalAlign="Left" />
                <ItemStyle HorizontalAlign="Left" VerticalAlign="top" />
                <ItemTemplate>
                  <%#Eval("permgroup_desc")%>
                </ItemTemplate>
              </asp:TemplateColumn>
              <asp:TemplateColumn HeaderText="Permission">
                <HeaderStyle HorizontalAlign="Left" />
                <ItemStyle HorizontalAlign="Left" VerticalAlign="top" />
                <ItemTemplate>
                  <%#Eval("perm_desc")%>
                </ItemTemplate>
              </asp:TemplateColumn>
              <asp:TemplateColumn HeaderText="Access">
                <HeaderStyle HorizontalAlign="center" />
                <ItemStyle HorizontalAlign="center" VerticalAlign="top" />
                <ItemTemplate>
                  <asp:CheckBox ID="chkAccess" runat="server" Checked='<%#IIf(IsDBNull(Eval("opPerm_level")), False, Eval("opPerm_level"))  %>' />
                </ItemTemplate>
              </asp:TemplateColumn>
            </Columns>
          </asp:DataGrid>
        </div>
      </div>
    </div>
    <div class="DivSpace10"></div>

    <div class="DivBlock">
      <div class="DivFormLabel"></div>
      <div class="DivFormField">
        <asp:Button ID="cmdUpdate" runat="server" CssClass="minibutton" Enabled="False" Text="Update" Width="150px" />
        &nbsp;<asp:Button ID="cmdCancel" runat="server" CssClass="minibutton" Text="Cancel" Width="150px" />
      </div>
    </div>
  </asp:Panel>



</asp:Content>
