Public Class CustomerMaint
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_obl_OpMgr As BL_OperatorManager
  Private m_obl_custMgr As BL_CustomerManager

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then
      zHideEditPanels()
      zloadOrganizations()

    End If
  End Sub



  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.


    If Not m_obl_custMgr Is Nothing Then
      m_obl_custMgr.Dispose()
      m_obl_custMgr = Nothing
    End If '

    If Not m_obl_OpMgr Is Nothing Then
      m_obl_OpMgr.Dispose()
      m_obl_OpMgr = Nothing
    End If

    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    End If

    m_pbl_Client = Nothing
    m_pbl_Operator = Nothing
    m_pcl_ErrManager = Nothing
    m_pdl_Manager = Nothing
    m_pcl_Utils = Nothing


  End Sub

#Region "Private Routines"
  Private Function zSetupObjects() As Boolean

    m_pcl_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)

    'Restore dl_manager
    If Session("dl_Manager") Is Nothing Then
      Return False
    End If
    m_pdl_Manager = CType(Session("dl_Manager"), DL_Manager)

    'Create Utils...
    m_pcl_Utils = New CL_Utils()
    m_pcl_Utils.pPage = Page

    m_oDB_Layer = New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    'Restore Client
    m_pbl_Client = CType(Session("bl_Client"), BL_Client)

    'Restore Operator
    m_pbl_Operator = CType(Session("bl_Operator"), BL_Operator)

    m_obl_OpMgr = New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)
    m_obl_custMgr = New BL_CustomerManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    Return True

  End Function

  Private Sub zloadOrganizations()
    Dim dtTemp As DataTable

    'organizations
    ddOrganizations.Items.Clear()


    dtTemp = m_obl_custMgr.GetAllOrganizations(True, True, m_pbl_Operator, m_pbl_Operator.ID)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!org_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!org_name)
      ddOrganizations.Items.Add(liNew)
    Next

    If ddOrganizations.Items.Count > 0 Then
      ddOrganizations.SelectedIndex = 0
      ddOrganizations_SelectedIndexChanged(Nothing, Nothing)
    Else
    End If

  End Sub


  Private Sub zloadGeneralLocations()
    Dim dtTemp As DataTable

    'General Locations
    ddEditLocGeneralLoc.Items.Clear()
    ddEditLocGeneralLoc.Items.Add(New ListItem("Select General Location", "-1"))

    dtTemp = m_obl_custMgr.GetAllGeneralLocations(True)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!genloc_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!genloc_name)
      ddEditLocGeneralLoc.Items.Add(liNew)
    Next

  End Sub


  Private Sub zHideEditPanels()
    pnlCustomerEdit.Visible = False
    pnlLocationEdit.Visible = False
    pnlSubLocationEdit.Visible = False
    'Reset the hidden IDs and TSs
    txtEditCustID.Text = "null"
    txtEditCustTS.Text = "null"

    txtEditLocID.Text = "null"
    txtEditLocTS.Text = "null"

    txtEditSubLocID.Text = "null"
    txtEditSubLocTS.Text = "null"


  End Sub

#End Region

#Region "Control Events"
  Protected Sub ddOrganizations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddOrganizations.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    zHideEditPanels()

    'Load Customers based on organisation
    lstCustomers.Items.Clear()
    lstLocations.Items.Clear()
    lstSubLocations.Items.Clear()

    If m_pbl_Operator.bIsSuperUser Then
      dtTemp = m_obl_custMgr.GetAllCustomers(False, m_pbl_Operator, m_pbl_Operator.ID)
    Else
      dtTemp = m_obl_custMgr.GetAllCustomers(True, m_pbl_Operator, m_pbl_Operator.ID)

    End If

    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!Cust_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!Cust_name)
      If m_oDB_Layer.CheckDBNullBool(dtTemp.Rows(iRow)!cust_active) = False Then
        liNew.Text += " (inactive)"
      End If
      lstCustomers.Items.Add(liNew)
    Next

    If lstCustomers.Items.Count > 1 Then
      If lstCustomers.Items.Count = 1 Then
        lstCustomers.SelectedIndex = 0
      Else
      End If

      lstCustomers_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub
  Protected Sub lstCustomers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCustomers.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32


    zHideEditPanels()

    If lstCustomers.SelectedIndex <> -1 Then

      'Load locations based on customer
      lstLocations.Items.Clear()
      lstSubLocations.Items.Clear()

      dtTemp = m_obl_custMgr.GetAllLocations(False, m_pbl_Operator, m_pbl_Operator.ID, CInt(lstCustomers.SelectedItem.Value))
      For iRow = 0 To dtTemp.Rows.Count - 1
        Dim liNew As New ListItem
        liNew.Value = CStr(dtTemp.Rows(iRow)!loc_id)
        liNew.Text = CStr(dtTemp.Rows(iRow)!Loc_name) & " (" & m_oDB_Layer.CheckDBNullStr(dtTemp.Rows(iRow)!timezone_Abbreviation) & ")"
        If m_oDB_Layer.CheckDBNullBool(dtTemp.Rows(iRow)!Loc_active) = False Then
          liNew.Text += "(inactive)"
        End If
        lstLocations.Items.Add(liNew)
      Next

      If lstLocations.Items.Count > 1 Then
        If lstLocations.Items.Count = 1 Then
          lstLocations.SelectedIndex = 0
        Else
        End If

        lstLocations_SelectedIndexChanged(Nothing, Nothing)
      End If


    End If
    pnlSubLocations.Visible = False

  End Sub
  Protected Sub lstLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstLocations.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    zHideEditPanels()

    If lstLocations.SelectedIndex <> -1 Then

      'Load sub-locations based on organisation
      lstSubLocations.Items.Clear()

      dtTemp = m_obl_custMgr.GetAllSubLocations(False, m_pbl_Operator, m_pbl_Operator.ID, CInt(lstLocations.SelectedItem.Value))
      For iRow = 0 To dtTemp.Rows.Count - 1
        Dim liNew As New ListItem
        liNew.Value = CStr(dtTemp.Rows(iRow)!subloc_id)
        liNew.Text = CStr(dtTemp.Rows(iRow)!subloc_name)
        lstSubLocations.Items.Add(liNew)
      Next

      pnlSubLocations.Visible = True

    End If
  End Sub
  Protected Sub lstSubLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSubLocations.SelectedIndexChanged
    zHideEditPanels()
  End Sub
  Protected Sub lnkNewCust_Click(sender As Object, e As EventArgs) Handles lnkNewCust.Click

    lblEditCustHdr.Text = "New Customer"

    zHideEditPanels()
    pnlCustomerEdit.Visible = True

    txtEditCustID.Text = -1
    txtEditCustName.Text = ""
    txtEditCustShortCode.Text = ""
    txtEditCustTS.Text = ""
    txtNotifyEmail.Text = ""
    txtNotifyAcousticLevel.Text = "1.9"
    txtNotifyAcousticCount.Text = "4"
    chkEditCustActive.Checked = False


  End Sub

  Protected Sub lnkEditCust_Click(sender As Object, e As EventArgs) Handles lnkEditCust.Click
    Dim drCust As DataRow

    zHideEditPanels()

    If lstCustomers.SelectedIndex > -1 Then
      pnlCustomerEdit.Visible = True
      lblEditCustHdr.Text = "Edit Customer"

      drCust = m_obl_custMgr.GetCustomerByID(CInt(lstCustomers.SelectedItem.Value))

      txtEditCustID.Text = lstCustomers.SelectedItem.Value
      txtEditCustTS.Text = Format(CDate(drCust!cust_timestamp), "yyyy-MM-dd HH:mm:ss")
      txtEditCustName.Text = m_oDB_Layer.CheckDBNullStr(drCust!cust_name)
      txtEditCustShortCode.Text = m_oDB_Layer.CheckDBNullStr(drCust!cust_ShortCode)
      chkEditCustActive.Checked = m_oDB_Layer.CheckDBNullBool(drCust!cust_active)
      txtNotifyEmail.Text = m_oDB_Layer.CheckDBNullStr(drCust!cust_NotifyEmails)
      txtNotifyAcousticLevel.Text = m_oDB_Layer.CheckDBNullStr(drCust!cust_NotifyAcousticTrigger)
      txtNotifyAcousticCount.Text = m_oDB_Layer.CheckDBNullStr(drCust!cust_NotifyAcousticCount)


    Else
      m_pcl_Utils.DisplayMessage("Please select a customer to edit.", lstCustomers.ClientID)
    End If
  End Sub


  Protected Sub cmdUpdateCustomer_Click(sender As Object, e As EventArgs) Handles cmdUpdateCustomer.Click
    Dim bOK As Boolean = False
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return
    Dim iNewID As Int32
    Dim drChecker As DataRow


    If Trim(txtEditCustName.Text) <> "" Then
      If Trim(txtEditCustShortCode.Text) <> "" Then
        If Len(txtEditCustShortCode.Text) <= 5 Then
          drChecker = m_obl_custMgr.GetCustomerByNameAndOrg(Trim(txtEditCustName.Text), CInt(ddOrganizations.SelectedItem.Value), CInt(txtEditCustID.Text))
          If drChecker Is Nothing Then
            'Not a duplicate....
            bOK = True
          Else
            m_pcl_Utils.DisplayMessage("The Customer Name already exists, you may not have permission to access it.", txtEditCustName.ClientID)
          End If
        Else
          m_pcl_Utils.DisplayMessage("The Customer Short Code can be a maximum of 5 characters.", txtEditCustName.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please enter the Customer Short Code", txtEditCustName.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter the Customer Name", txtEditCustName.ClientID)
    End If

    If bOK Then
      bOK = False
      If txtNotifyEmail.Text.Trim <> "" Then
        revEmail.Visible = True
        revEmail.Validate()
        revEmail.Visible = False
        If revEmail.IsValid Then
          'Email OK
          If txtNotifyAcousticLevel.Text.Trim <> "" Then
            If IsNumeric(txtNotifyAcousticLevel.Text.Trim) Then
              If CDec(txtNotifyAcousticLevel.Text.Trim) <= 10 Then
                'Acoustic ok.
                If txtNotifyAcousticCount.Text.Trim <> "" Then
                  If IsNumeric(txtNotifyAcousticCount.Text.Trim) Then
                    If CDec(txtNotifyAcousticCount.Text.Trim) > 1 Then
                      'Acoustic ok.
                      bOK = True
                    Else
                      m_pcl_Utils.DisplayMessage("Please enter an Acoustic Leak Trigger Count must be greater than 1", txtNotifyAcousticCount.ClientID)
                    End If
                  Else
                    m_pcl_Utils.DisplayMessage("Please enter a numeric Acoustic Leak Trigger Count", txtNotifyAcousticCount.ClientID)
                  End If
                Else
                  m_pcl_Utils.DisplayMessage("Please enter the Acoustic Leak Trigger Count", txtNotifyAcousticCount.ClientID)
                End If
              Else
                m_pcl_Utils.DisplayMessage("Please enter an Acoustic Leak Trigger Level less than 19", txtNotifyAcousticLevel.ClientID)
              End If
            Else
              m_pcl_Utils.DisplayMessage("Please enter a numeric Acoustic Leak Trigger Level", txtNotifyAcousticLevel.ClientID)
            End If
          Else
            m_pcl_Utils.DisplayMessage("Please enter the Acoustic Leak Trigger Level", txtNotifyAcousticLevel.ClientID)
          End If
        Else
          m_pcl_Utils.DisplayMessage("Please enter the A valid email address", txtNotifyEmail.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please enter the A valid email address", txtNotifyEmail.ClientID)
      End If

    End If

    If bOK Then
      dicParms.Add("cust_Name", "S:" & Trim(txtEditCustName.Text))
      dicParms.Add("cust_ShortCode", "S:" & Trim(txtEditCustShortCode.Text))
      If chkEditCustActive.Checked Then
        dicParms.Add("cust_active", "N:" & -1)
      Else
        dicParms.Add("cust_active", "N:" & 0)
      End If
      dicParms.Add("cust_NotifyEmails", "S:" & Trim(txtNotifyEmail.Text))
      dicParms.Add("cust_NotifyAcousticTrigger", "N:" & Format(CDec(txtNotifyAcousticLevel.Text), "#####.00"))
      dicParms.Add("cust_NotifyAcousticCount", "N:" & CInt(txtNotifyAcousticCount.Text))

      If txtEditCustID.Text = "-1" Then
        'Insert
        dicParms.Add("org_id", "N:" & CInt(ddOrganizations.SelectedItem.Value))
        iNewID = m_obl_custMgr.InsertCustomer(dicParms)
        'Need to give this operator permission to see the new customer.
        dicParms.Clear()
        dicParms.Add("operator_id", "N:" & m_pbl_Operator.ID)
        dicParms.Add("cust_id", "N:" & iNewID)
        dicParms.Add("op2cust_AllLoctions", "N:" & "0")
        m_obl_custMgr.InsertOp2Cust(dicParms)

        dbRet = DL_Manager.DB_Return.ok
      Else
        'Update
        dicParms.Add("cust_Timestamp", "T:" & Format(CDate(txtEditCustTS.Text), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_obl_custMgr.UpdateCustomer(CInt(txtEditCustID.Text), dicParms)
      End If

      If dbRet = DL_Manager.DB_Return.ok Then
        zHideEditPanels()
        'Reload the Customer list..
        ddOrganizations_SelectedIndexChanged(Nothing, Nothing)

        lstLocations.Items.Clear()
        lstSubLocations.Items.Clear()
      Else
        m_pcl_Utils.DisplayMessage("Could not update the customer details, another user my have changed them.\nPlease reload the data.")
      End If

    End If
  End Sub
  Protected Sub lnkNewLocation_Click(sender As Object, e As EventArgs) Handles lnkNewLocation.Click

    lblEditLocHdr.Text = "New Location"
    zHideEditPanels()
    pnlLocationEdit.Visible = True
    zloadGeneralLocations()

    txtEditLocID.Text = -1
    txtEditLocName.Text = ""
    ddEditLocGeneralLoc.SelectedIndex = 0
    chkEditLocActive.Checked = False
    txtEditLocTS.Text = ""

  End Sub
  Protected Sub lnkEditLocation_Click(sender As Object, e As EventArgs) Handles lnkEditLocation.Click
    Dim drLoc As DataRow

    zHideEditPanels()
    zloadGeneralLocations()

    If lstLocations.SelectedIndex > -1 Then
      pnlLocationEdit.Visible = True
      lblEditLocHdr.Text = "Edit Location"

      drLoc = m_obl_custMgr.GetLocationByID(CInt(lstLocations.SelectedItem.Value))

      txtEditLocID.Text = m_oDB_Layer.CheckDBNullStr(drLoc!loc_id)
      txtEditLocTS.Text = Format(CDate(drLoc!loc_timestamp), "yyyy-MM-dd HH:mm:ss")
      txtEditLocName.Text = m_oDB_Layer.CheckDBNullStr(drLoc!loc_name)
      m_pcl_Utils.DropDownSelectByValue(ddEditLocGeneralLoc, m_oDB_Layer.CheckDBNullStr(drLoc!genloc_id))
      chkEditLocActive.Checked = m_oDB_Layer.CheckDBNullBool(drLoc!loc_active)


    Else
      m_pcl_Utils.DisplayMessage("Please select a Location to edit.", lstLocations.ClientID)
    End If
  End Sub
  Protected Sub cmdUpdateLocation_Click(sender As Object, e As EventArgs) Handles cmdUpdateLocation.Click
    Dim bOK As Boolean = False
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return
    Dim iNewID As Int32
    Dim drChecker As DataRow

    'Update Location!!

    If Trim(txtEditLocName.Text) <> "" Then
      If Len(txtEditLocName.Text) <= 250 Then
        If ddEditLocGeneralLoc.SelectedIndex > 0 Then
          drChecker = m_obl_custMgr.GetLocationByNameAndCust(Trim(txtEditLocName.Text), CInt(lstCustomers.SelectedItem.Value), CInt(txtEditLocID.Text))
          If drChecker Is Nothing Then
            'Not a duplicate....
            bOK = True
          Else
            m_pcl_Utils.DisplayMessage("The Location Name already exists for this customer, you may not have permission to access it.", txtEditLocName.ClientID)
          End If
        Else
          m_pcl_Utils.DisplayMessage("Please select a General Location.", ddEditLocGeneralLoc.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("The Location Name maximum of 250 characters.", txtEditLocName.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter the Location Name", txtEditLocName.ClientID)
    End If

    If bOK Then
      dicParms.Add("loc_Name", "S:" & Trim(txtEditLocName.Text))
      dicParms.Add("genloc_ID", "S:" & ddEditLocGeneralLoc.SelectedItem.Value)
      If chkEditLocActive.Checked Then
        dicParms.Add("loc_active", "N:" & -1)
      Else
        dicParms.Add("loc_active", "N:" & 0)
      End If

      If txtEditLocID.Text = "-1" Then
        'Insert
        dicParms.Add("cust_id", "N:" & CInt(lstCustomers.SelectedItem.Value))
        iNewID = m_obl_custMgr.InsertLocation(dicParms)
        dicParms.Clear()
        dicParms.Add("operator_id", "N:" & m_pbl_Operator.ID)
        dicParms.Add("loc_id", "N:" & iNewID)
        m_obl_custMgr.InsertOp2Loc(dicParms)

        dbRet = DL_Manager.DB_Return.ok
      Else
        'Update
        dicParms.Add("loc_Timestamp", "T:" & Format(CDate(txtEditLocTS.Text), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_obl_custMgr.UpdateLocation(CInt(txtEditLocID.Text), dicParms)
      End If

      If dbRet = DL_Manager.DB_Return.ok Then
        zHideEditPanels()
        'Reload the Location list..
        lstCustomers_SelectedIndexChanged(Nothing, Nothing)
        lstSubLocations.Items.Clear()

      Else
        m_pcl_Utils.DisplayMessage("Could not update the location details, another user my have changed them.\nPlease reload the data.")
      End If

    End If

  End Sub
  Protected Sub lnkNewSubLoc_Click(sender As Object, e As EventArgs) Handles lnkNewSubLoc.Click

    lblEditSubLocHdr.Text = "New Sub-Location"
    zHideEditPanels()
    pnlSubLocationEdit.Visible = True

    txtEditSubLocID.Text = -1
    txtEditSubLocName.Text = ""
    txtEditSubLocTS.Text = ""

  End Sub

  Protected Sub lnkEditSubLoc_Click(sender As Object, e As EventArgs) Handles lnkEditSubLoc.Click
    Dim drSubLoc As DataRow

    zHideEditPanels()

    If lstSubLocations.SelectedIndex > -1 Then
      pnlSubLocationEdit.Visible = True
      lblEditLocHdr.Text = "Edit Sub-Location"

      drSubLoc = m_obl_custMgr.GetSubLocationByID(CInt(lstSubLocations.SelectedItem.Value))

      txtEditSubLocID.Text = m_oDB_Layer.CheckDBNullStr(drSubLoc!subloc_id)
      txtEditSubLocTS.Text = Format(CDate(drSubLoc!subloc_timestamp), "yyyy-MM-dd HH:mm:ss")
      txtEditSubLocName.Text = m_oDB_Layer.CheckDBNullStr(drSubLoc!subloc_name)


    Else
      m_pcl_Utils.DisplayMessage("Please select a Sub-Location to edit.", lstSubLocations.ClientID)
    End If
  End Sub


  Protected Sub cmdUpdateSubLoc_Click(sender As Object, e As EventArgs) Handles cmdUpdateSubLoc.Click
    Dim bOK As Boolean = False
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return
    Dim iNewID As Int32
    Dim drChecker As DataRow

    'Update Location!!

    If Trim(txtEditSubLocName.Text) <> "" Then
      If Len(txtEditSubLocName.Text) <= 250 Then
        drChecker = m_obl_custMgr.GetSubLocationByNameAndLoc(Trim(txtEditSubLocName.Text), CInt(lstLocations.SelectedItem.Value), CInt(txtEditSubLocID.Text))
        If drChecker Is Nothing Then
          'Not a duplicate....
          bOK = True
        Else
          m_pcl_Utils.DisplayMessage("The Sub-Location Name already exists for this location, you may not have permission to access it.", txtEditSubLocName.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("The Sub-Location Name maximum of 250 characters.", txtEditSubLocName.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter the Sub-Location Name", txtEditSubLocName.ClientID)
    End If

    If bOK Then
      dicParms.Add("subloc_Name", "S:" & Trim(txtEditSubLocName.Text))

      If txtEditSubLocID.Text = "-1" Then
        'Insert
        dicParms.Add("loc_id", "N:" & CInt(lstLocations.SelectedItem.Value))
        iNewID = m_obl_custMgr.InsertSubLocation(dicParms)
        dbRet = DL_Manager.DB_Return.ok
      Else
        'Update
        dicParms.Add("Subloc_Timestamp", "T:" & Format(CDate(txtEditSubLocTS.Text), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_obl_custMgr.UpdateSubLocation(CInt(txtEditSubLocID.Text), dicParms)
      End If

      If dbRet = DL_Manager.DB_Return.ok Then
        zHideEditPanels()
        'Reload the Location list..
        lstLocations_SelectedIndexChanged(Nothing, Nothing)

      Else
        m_pcl_Utils.DisplayMessage("Could not update the Sub-Location details, another user my have changed them.\nPlease reload the data.")
      End If

    End If

  End Sub
#End Region



End Class