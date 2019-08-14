Public Class OperatorMaintenance
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  'Private m_bNewOP As Boolean
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oEditOperator As BL_Operator
  Private m_bDisplayDetails As Boolean
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_dtStartTime As Date = Now()
  Private m_oBL_CustMgr As BL_CustomerManager

  Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

    Try
      Diagnostics.Debug.WriteLine("Operator Maint load: " & Format(m_dtStartTime, "HH:mm:ss.fff"))

      'Set Default button!!!
      zSetupObjects()
      Me.Title = "Operator Maintenance"
      If Not Page.IsPostBack Then
        m_pdl_Manager.UpdateControlLabels(Page.Controls)
        zLoadMainGrid()
      End If

    Catch localexception As Exception
      m_pcl_ErrManager.AddError(localexception, True)
      Server.Transfer("exceptionpage.aspx")
    End Try
  End Sub
  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload

    If m_oBL_CustMgr IsNot Nothing Then
      m_oBL_CustMgr.Dispose()
      m_oBL_CustMgr = Nothing
    End If

    If m_oEditOperator IsNot Nothing Then
      m_oEditOperator.Dispose()
      m_oEditOperator = Nothing
    End If

    'Shutdown the data layer...
    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    Else
      m_oDB_Layer = Nothing
    End If

    System.Diagnostics.Debug.WriteLine("Operator Maint Created in " & Format(Now().Subtract(m_dtStartTime).TotalMilliseconds / 1000, "###0.00") & " seconds")

  End Sub

#Region "Control Events"
  Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
    Debug.Write("Cancel Click" & vbCrLf)
    m_oEditOperator.Reset()
    'Reset Form back to search....
    zSetupDetailsPanel(False, False)
  End Sub
  Private Sub cmdUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdUpdate.Click
    Try
      Dim dbret As DL_Manager.DB_Return
      If cmdUpdate.Enabled Then

        'Let's validate and update image.....
        If zbformValid() Then
          zStoreValuesToOperator()
          If m_oEditOperator.ID = -1 Then
            m_oEditOperator.InsertOperator()
            'Send a password reminder....
            m_oEditOperator.SendReminder(m_oEditOperator.Email, True, "", m_pcl_Utils)
            zSetupDetailsPanel(False, False)
            zLoadMainGrid()
          Else
            dbret = m_oEditOperator.UpdateOperator()
            If dbret = DL_Manager.DB_Return.ok Then
              m_oEditOperator.Reset()
              'Go back to search mode.....
              zSetupDetailsPanel(False, False)
              zLoadMainGrid()
            Else
              m_pcl_Utils.DisplayMessage("There was a problem updating the user details, please re-load and try again.")
            End If
          End If
        End If
      End If


    Catch localexception As Exception
      m_pcl_ErrManager.AddError(localexception, True)
      Server.Transfer("ExceptionPage.aspx")
    End Try
  End Sub

  Protected Sub lnkNewOp_Click(sender As Object, e As EventArgs) Handles lnkNewOp.Click
    'Let's Display the details for entry...
    Debug.Write("New Click" & vbCrLf)
    m_oEditOperator.Reset()
    m_oEditOperator.CreateInitialPerms()
    zSetupDetailsPanel(True, True)
    m_pcl_Utils.SetFocusControl(txtEmail.ClientID)
    'zSetupDetailsPanel(True, True)
  End Sub

  Protected Sub gvSearch_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvSearch.RowDataBound
    Try
      Dim chkActive As CheckBox = CType(e.Row.FindControl("chkActive"), CheckBox)
      Dim lblOpEmail As Label = CType(e.Row.FindControl("lblOpEmail"), Label)
      Dim drvInput As DataRowView
      Dim pGridViewRow As GridViewRow
      Dim i As Int32

      pGridViewRow = e.Row

      Select Case e.Row.RowType
        Case DataControlRowType.Header
          For i = 0 To pGridViewRow.Cells.Count - 1
            pGridViewRow.Cells(i).CssClass = "DataGridColHdr"
          Next

        Case DataControlRowType.DataRow
          drvInput = CType(pGridViewRow.DataItem, DataRowView)

          If m_oDB_Layer.CheckDBNullBool(drvInput!Operator_Suspended) Then
            lblOpEmail.ForeColor = Drawing.Color.DarkRed
          End If

          For i = 0 To pGridViewRow.Cells.Count - 1
            pGridViewRow.Cells(i).CssClass = "DataGridCol"

          Next

      End Select

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("Exceptionpage.aspx")
    End Try
  End Sub


  Private Sub gvSearch_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvSearch.RowCommand
    Try
      'Edit/view click...
      Dim sTmp As String
      Dim bEdit As Boolean
      Dim lnkClick As LinkButton = CType(e.CommandSource, LinkButton)

      If UCase(lnkClick.Text) = "VIEW" Then
        bEdit = False
      Else
        bEdit = True
      End If
      'Get Operator ID
      sTmp = CStr(e.CommandArgument)
      'Display Details
      zSetupDetailsPanel(True, bEdit, CInt(Val(sTmp)))
      m_pcl_Utils.SetFocusControl(txtName.ClientID)

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("exceptionpage.aspx")
    End Try
  End Sub


  Protected Sub gvSearch_EditCommand(sender As Object, e As GridViewEditEventArgs) Handles gvSearch.RowEditing

  End Sub
  Protected Sub dgPerms_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgPerms.ItemDataBound
    Try
      Dim drvInput As DataRowView
      Dim pDataGridItem As DataGridItem
      Dim i As Int32

      pDataGridItem = CType(e.Item, DataGridItem)

      Select Case e.Item.ItemType

        Case ListItemType.Header
          For i = 0 To pDataGridItem.Cells.Count - 1
            pDataGridItem.Cells(i).CssClass = "DataGridColHdr"

          Next

        Case ListItemType.AlternatingItem, ListItemType.Item
          drvInput = CType(e.Item.DataItem, DataRowView)

          For i = 0 To pDataGridItem.Cells.Count - 1
            pDataGridItem.Cells(i).CssClass = "DataGridCol"

          Next
      End Select

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("ExceptionPage.aspx")
    End Try
  End Sub



  Protected Sub lnkAddOrg_Click(sender As Object, e As EventArgs) Handles lnkAddOrg.Click
    'Add the org to the xref

    Dim dicParms As New eDictionary

    If lstAvailOrgs.SelectedIndex > -1 Then
      dicParms.Add("operator_id", "N:" & m_oEditOperator.ID)
      dicParms.Add("org_id", "N:" & lstAvailOrgs.SelectedItem.Value)
      m_oBL_CustMgr.InsertOp2Org(dicParms)

      zLoadOrgs()
      zLoadCustomers()
      zLoadLocations()
    Else
      m_pcl_Utils.DisplayMessage("Please select an organization to add.", lstAvailOrgs.ClientID)
    End If


  End Sub

  Protected Sub lnkRemoveOrg_Click(sender As Object, e As EventArgs) Handles lnkRemoveOrg.Click

    If lstSelectedOrgs.SelectedIndex > -1 Then
      m_oBL_CustMgr.DeleteOp2Org(m_oEditOperator.ID, CInt(lstSelectedOrgs.SelectedItem.Value))

      'We now need to remove any customers that no longer are allocation to the organization....
      m_oBL_CustMgr.DeleteOrphanedOp2Cust(m_oEditOperator.ID)
      m_oBL_CustMgr.DeleteOrphanedOp2Loc(m_oEditOperator.ID)

      zLoadOrgs()
      zLoadCustomers()
      zLoadLocations()

    Else
      m_pcl_Utils.DisplayMessage("Please select an organization to remove.", lstAvailOrgs.ClientID)
    End If


  End Sub


  Protected Sub lnkAddCustomer_Click(sender As Object, e As EventArgs) Handles lnkAddCustomer.Click
    'Add the cust to the xref

    Dim dicParms As New eDictionary

    If lstAvailCusts.SelectedIndex > -1 Then
      dicParms.Add("operator_id", "N:" & m_oEditOperator.ID)
      dicParms.Add("cust_id", "N:" & lstAvailCusts.SelectedItem.Value)
      m_oBL_CustMgr.InsertOp2cust(dicParms)

      zLoadCustomers()
      zLoadLocations()

    Else
      m_pcl_Utils.DisplayMessage("Please select a Customer to add.", lstAvailOrgs.ClientID)
    End If


  End Sub

  Protected Sub lnkRemoveCustomer_Click(sender As Object, e As EventArgs) Handles lnkRemoveCustomer.Click
    If lstSelectedCusts.SelectedIndex > -1 Then
      m_oBL_CustMgr.DeleteOp2Cust(m_oEditOperator.ID, CInt(lstSelectedCusts.SelectedItem.Value))
      m_oBL_CustMgr.DeleteOrphanedOp2Loc(m_oEditOperator.ID)

      'We now need to remove any customers that no longer are allocation to the organization....

      zLoadCustomers()
      zLoadLocations()

    Else
      m_pcl_Utils.DisplayMessage("Please select a Customer to remove.", lstAvailOrgs.ClientID)
    End If


  End Sub

  Protected Sub lnkAddLoc_Click(sender As Object, e As EventArgs) Handles lnkAddLoc.Click
    'Add the cust to the xref

    Dim dicParms As New eDictionary

    If lstAvailLocs.SelectedIndex > -1 Then
      dicParms.Add("operator_id", "N:" & m_oEditOperator.ID)
      dicParms.Add("Loc_id", "N:" & lstAvailLocs.SelectedItem.Value)
      m_oBL_CustMgr.InsertOp2Loc(dicParms)

      zLoadLocations()

    Else
      m_pcl_Utils.DisplayMessage("Please select a Location to add.", lstAvailOrgs.ClientID)
    End If


  End Sub

  Protected Sub lnkRemoveLoc_Click(sender As Object, e As EventArgs) Handles lnkRemoveLoc.Click
    If lstSelectedLocs.SelectedIndex > -1 Then
      m_oBL_CustMgr.DeleteOp2Loc(m_oEditOperator.ID, CInt(lstSelectedLocs.SelectedItem.Value))

      'We now need to remove any customers that no longer are allocation to the organization....

      zLoadLocations()

    Else
      m_pcl_Utils.DisplayMessage("Please select a Location to remove.", lstAvailOrgs.ClientID)
    End If


  End Sub




#End Region

#Region "Private Routines"
  Private Sub zSetupObjects()
    'Restore dl_manager
    m_pdl_Manager = CType(Session("dl_Manager"), DL_Manager)
    m_pcl_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)
    m_oDB_Layer = New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    'Create Utils...
    m_pcl_Utils = New CL_Utils()
    m_pcl_Utils.pPage = Page

    'Restore Client
    m_pbl_Client = CType(Session("bl_Client"), BL_Client)

    'Restore Operator
    m_pbl_Operator = CType(Session("bl_Operator"), BL_Operator)

    m_oBL_CustMgr = New BL_CustomerManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    If Session("EditOperator") Is Nothing Then
      m_oEditOperator = New BL_Operator(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client)
      Session("EditOperator") = m_oEditOperator
    Else
      m_oEditOperator = CType(Session("EditOperator"), BL_Operator)
      m_oEditOperator.pbl_Client = m_pbl_Client
      m_oEditOperator.pcl_ErrManager = m_pcl_ErrManager
      m_oEditOperator.pdl_Manager = m_pdl_Manager
    End If
  End Sub
  Private Sub zLoadMainGrid()
    Dim oBL_OpMgr As New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)
    Dim dtMain As DataTable
    dtMain = oBL_OpMgr.GetOperators(True, False)
    gvSearch.DataSource = dtMain
    gvSearch.DataBind()

  End Sub
  Private Function zbformValid() As Boolean
    'Validate the form and the image.....
    Dim oBL_OpMgr As New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)
    Dim sMsg As String = ""
    Dim pcontrol As Control = Nothing
    Dim bOk As Boolean
    Dim drOpCheck As DataRow
    Dim bOpOk As Boolean = False

    bOk = False
    If txtEmail.Text.Trim <> "" Then
      drOpCheck = oBL_OpMgr.GetOperatorByEmail(txtEmail.Text.Trim, m_pbl_Client.ClientID)
      If drOpCheck Is Nothing Then
        bOpOk = True
      Else
        If CInt(drOpCheck!operator_id) = CInt(txtID.Text) Then
          bOpOk = True
        End If
      End If

      If bOpOk Then
        If txtEmail.Text.Trim <> "" Then
          revEmail.Visible = True
          revEmail.Validate()
          revEmail.Visible = False
          If revEmail.IsValid Then

            'Email OK
            If txtName.Text.Trim <> "" Then
              'Op Name OK
              If txtJob.Text.Trim <> "" Then
                'Job OK
                bOk = True
              Else
                'No job
                sMsg = "Please enter user's Job Title"
                pcontrol = txtJob
              End If
            Else
              'No Name
              sMsg = "Please enter the user's Name"
              pcontrol = txtName
            End If
          Else
            'Invalid Email
            sMsg = "The email is not valid, please correct"
            pcontrol = txtEmail
          End If
        Else
          'No Email
          sMsg = "Please Enter the Operator's Email"
          pcontrol = txtEmail
        End If
      Else
        'Duplicate User
        sMsg = "The User with this email has already been created, please correct."
        pcontrol = txtEmail
      End If
    Else
      'No UID
      sMsg = "Please enter the User ID Field."
      pcontrol = txtEmail
    End If



    If sMsg <> "" Then
      m_pcl_Utils.DisplayMessage(sMsg, pcontrol.ClientID)
      bOk = False
    End If

    Return bOk

  End Function
  Private Sub zSetupDetailsPanel(ByVal bShowDetails As Boolean, ByVal bAllowEdit As Boolean, Optional ByVal lOpID As Int32 = -1)
    'Display Image detail fields
    pnlDetails.Visible = bShowDetails
    cmdUpdate.Enabled = bAllowEdit And bShowDetails
    pnlDetails.Enabled = bShowDetails
    dgPerms.Enabled = bAllowEdit

    pnlHeader.Visible = Not bShowDetails

    If m_pbl_Operator.bIsSuperUser Then
      pnlSuperUser1.Visible = True
    Else
      pnlSuperUser1.Visible = False
    End If

    'Set the screen controls into readonly mode....
    m_pcl_Utils.FieldEditMode(True, pnlDetails.Controls)
    lnkNewOp.Visible = Not bShowDetails
    If bShowDetails Then
      zPopulateDetailsFields(lOpID)

    End If

  End Sub
  Private Sub zPopulateDetailsFields(ByVal iOpID As Int32)
    Dim dtAvailablePerms As DataTable
    Dim oBL_OpMgr As New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    'Populate the screen fields with the requested data....
    m_bDisplayDetails = True
    Session("bDisplayDets") = m_bDisplayDetails
    If iOpID <> -1 Then
      'Existing Image, let's go find it!!!!
      If m_oEditOperator.GetOperatorByID(iOpID, m_pbl_Operator.bIsSuperUser) Then
        'Populate the data fields
        With m_oEditOperator
          txtID.Text = .ID.ToString
          txtID.ReadOnly = True
          txtName.Text = .Name
          txtEmail.Text = .Email
          txtTelephone.Text = .Telephone
          txtJob.Text = .JobTitle
          'Popluate the Check boxes
          chkSuspended.Checked = .Suspended
          txtTS.Text = .Timestamp
          chkSuperUser.Checked = .bIsSuperUser

          PnlOrgs.Visible = True

        End With
      Else
        PnlOrgs.Visible = False
        m_pcl_Utils.DisplayMessage(m_pdl_Manager.GetMessage(103))
      End If
    Else
      'Set-up new details...
      txtID.Text = "-1"
      txtID.ReadOnly = True
      txtEmail.Text = ""
      txtName.Text = ""
      txtTelephone.Text = ""
      txtEmail.Text = ""
      txtJob.Text = ""
      chkSuspended.Checked = False
      txtTS.Text = ""
      chkSuperUser.Checked = False

      PnlOrgs.Visible = False

    End If
    dtAvailablePerms = oBL_OpMgr.GetAvailablePerms(m_pbl_Operator.ID, m_oEditOperator.ID)
    dgPerms.DataSource = dtAvailablePerms
    dgPerms.DataBind()

    If PnlOrgs.Visible = True Then
      zLoadOrgs()
      zLoadCustomers()
      zLoadLocations()

    End If

    'Display seuper user stuff!!
    pnlSuperUser1.Visible = m_pbl_Operator.bIsSuperUser

  End Sub
  Private Sub zStoreValuesToOperator()
    'Take the values on the form and post to the Edit object.
    Dim iRow As Int32
    Dim dgItem As DataGridItem
    Dim chkAccess As CheckBox
    Dim iGroup As Int32
    Dim iPerm As Int32
    Dim sKey As String
    Dim lblTemp As Label

    With m_oEditOperator
      .Email = txtEmail.Text.Trim
      .Name = txtName.Text.Trim
      .Telephone = txtTelephone.Text.Trim
      .JobTitle = txtJob.Text.Trim
      .Suspended = chkSuspended.Checked

      'Copy the perms back to the Operator...
      For iRow = 0 To dgPerms.Items.Count - 1
        dgItem = dgPerms.Items(iRow)
        chkAccess = CType(dgItem.Cells(3).FindControl("chkAccess"), CheckBox)

        lblTemp = CType(dgItem.Cells(3).FindControl("lblPermGroup_ID"), Label)
        iGroup = CInt(lblTemp.Text)

        lblTemp = CType(dgItem.Cells(3).FindControl("lblperm_id"), Label)
        iPerm = CInt(lblTemp.Text)

        'sParse = chkAccess.ToolTip.Split(CChar("-"))
        'iGroup = CInt(sParse(0))
        'iPerm = CInt(sParse(1))

        sKey = "key-" & Format(iGroup, "0000") & "-" & Format(iPerm, "0000")
        If chkAccess.Checked Then
          CType(m_oEditOperator.dicOpPerms.Item(sKey), BL_OpPermission).PermLevel = -1
        Else
          CType(m_oEditOperator.dicOpPerms.Item(sKey), BL_OpPermission).PermLevel = 0
        End If
      Next

    End With

  End Sub


  Private Sub zLoadOrgs()
    Dim dtOrgs As DataTable
    Dim iOrg As Int32
    Dim drOrg As DataRow

    lstAvailOrgs.Items.Clear()
    lstSelectedOrgs.Items.Clear()

    dtOrgs = m_oBL_CustMgr.GetAllOrganizations(True, True, m_pbl_Operator, m_oEditOperator.ID)

    For iOrg = 0 To dtOrgs.Rows.Count - 1
      drOrg = dtOrgs.Rows(iOrg)
      Dim liNew As New ListItem
      liNew.Text = CStr(drOrg!org_Name)
      liNew.Value = CStr(drOrg!org_ID)

      If IsDBNull(drOrg!op2org_Timestamp) Then
        lstAvailOrgs.Items.Add(liNew)
      Else
        lstSelectedOrgs.Items.Add(liNew)
      End If

    Next


  End Sub




  Private Sub zLoadCustomers()
    Dim dtCusts As DataTable
    Dim iCust As Int32
    Dim drCust As DataRow

    lstAvailCusts.Items.Clear()
    lstSelectedCusts.Items.Clear()

    If m_pbl_Operator.bIsSuperUser Then
      dtCusts = m_oBL_CustMgr.GetAllCustomers(False, m_pbl_Operator, m_oEditOperator.ID)
    Else
      dtCusts = m_oBL_CustMgr.GetAllCustomers(True, m_pbl_Operator, m_oEditOperator.ID)

    End If

    For iCust = 0 To dtCusts.Rows.Count - 1
      drCust = dtCusts.Rows(iCust)
      Dim liNew As New ListItem
      liNew.Text = m_oDB_Layer.CheckDBNullStr(drCust!org_ShortCode) & ":" & CStr(drCust!cust_Name)
      If CBool(drCust!cust_active) = False Then
        liNew.Text += " (Inactive)"
      End If
      liNew.Value = CStr(drCust!cust_ID)

      If IsDBNull(drCust!op2cust_Timestamp) Then
        lstAvailCusts.Items.Add(liNew)
      Else
        lstSelectedCusts.Items.Add(liNew)
      End If

    Next

  End Sub



  Private Sub zLoadLocations()
    Dim dtLocs As DataTable
    Dim iLoc As Int32
    Dim drLoc As DataRow

    lstAvailLocs.Items.Clear()
    lstSelectedLocs.Items.Clear()

    If m_pbl_Operator.bIsSuperUser Then
      dtLocs = m_oBL_CustMgr.GetAllLocations(False, m_pbl_Operator, m_oEditOperator.ID, -1)
    Else
      dtLocs = m_oBL_CustMgr.GetAllLocations(True, m_pbl_Operator, m_oEditOperator.ID, -1)
    End If


    For iLoc = 0 To dtLocs.Rows.Count - 1
      drLoc = dtLocs.Rows(iLoc)
      Dim liNew As New ListItem
      liNew.Text = m_oDB_Layer.CheckDBNullStr(drLoc!org_ShortCode) & ":" & m_oDB_Layer.CheckDBNullStr(drLoc!cust_ShortCode) & ":" & CStr(drLoc!Loc_Name)
      liNew.Value = CStr(drLoc!Loc_ID)
      If CBool(drLoc!loc_active) = False Then
        liNew.Text += " (Inactive)"
      End If
      If IsDBNull(drLoc!op2loc_Timestamp) Then
        lstAvailLocs.Items.Add(liNew)
      Else
        lstSelectedLocs.Items.Add(liNew)
      End If

    Next

  End Sub



#End Region

End Class