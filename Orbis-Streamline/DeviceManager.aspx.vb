Option Strict On
Imports System.Drawing
Imports Telerik.Web.UI
Imports MySql.Data

Public Class DeviceManager
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DevManager As BL_DeviceManager
  Private m_oBL_CustMgr As BL_CustomerManager


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then
      zLoadDropdowns()
      'pnlDeviceList.Visible = True
      'pnlDeviceEdit.Visible = False

    End If
  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '

    If m_oBL_CustMgr IsNot Nothing Then
      m_oBL_CustMgr.Dispose()
      m_oBL_CustMgr = Nothing
    End If
    If m_oBL_DevManager IsNot Nothing Then
      m_oBL_DevManager.Dispose()
      m_oBL_DevManager = Nothing
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

    m_oBL_DevManager = New BL_DeviceManager(m_pdl_Manager, m_pcl_ErrManager, m_oDB_Layer)
    m_oBL_CustMgr = New BL_CustomerManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    Return True

  End Function

  Private Sub zLoadDropdowns()
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'organizations
    ddorganizations.Items.Clear()
    ddorganizations.Items.Add(New ListItem("All organizations....", "-1"))


    dtTemp = m_oBL_CustMgr.GetAllOrganizations(True, True, m_pbl_Operator, m_pbl_Operator.ID)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!org_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!org_name)
      ddorganizations.Items.Add(liNew)
    Next

    If ddorganizations.Items.Count > 1 Then
      If ddorganizations.Items.Count = 2 Then
        ddorganizations.SelectedIndex = 1
      Else
        ddorganizations.SelectedIndex = 0
      End If
      ddorganizations_SelectedIndexChanged(Nothing, Nothing)
    End If


  End Sub

  Private Sub zLoadGrid()
    'Load Dashboard controls.
    Dim dtDevices As DataTable

    dtDevices = m_oBL_DevManager.GetDevicesForList(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), False)

    gvDevices.DataSource = dtDevices
    gvDevices.DataBind()

  End Sub

  Private Sub zloadEditDropDowns()
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Device Hardware Type
    ddDevModel.Items.Clear()
    ddDevModel.Items.Add(New ListItem("Unknown", "-1"))
    dtTemp = m_oBL_DevManager.GetAllDeviceHardwareTypes()
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!devh_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!devh_Description)
      ddDevModel.Items.Add(liNew)
    Next


    'Device Types
    ddDevType.Items.Clear()
    ddDevType.Items.Add(New ListItem("Select Device Type....", "-1"))
    dtTemp = m_oBL_DevManager.GetAllDeviceTypes()
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!devtype_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!devtype_name)
      ddDevType.Items.Add(liNew)
    Next

    ''Device model
    'ddEditOrganisation.Items.Clear()
    'ddEditOrganisation.Items.Add(New ListItem("Select Device Type....", "-1"))
    'dtTemp = m_oBL_DevManager.GetAllDeviceTypes()
    'For iRow = 0 To dtTemp.Rows.Count - 1
    '  Dim liNew As New ListItem
    '  liNew.Value = CStr(dtTemp.Rows(iRow)!org_id)
    '  liNew.Text = CStr(dtTemp.Rows(iRow)!org_name)
    '  ddEditOrganisation.Items.Add(liNew)
    'Next

    'organizations
    ddEditOrganisation.Items.Clear()
    ddEditOrganisation.Items.Add(New ListItem("Select Organisation....", "-1"))
    dtTemp = m_oBL_CustMgr.GetAllOrganizations(True, True, m_pbl_Operator, m_pbl_Operator.ID)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!org_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!org_name)
      ddEditOrganisation.Items.Add(liNew)
    Next

    ''Load Customers based on org
    'ddEditCustomer.Items.Clear()
    'ddEditCustomer.Items.Add(New ListItem("Select Customer....", "-1"))
    'dtTemp = m_oBL_CustMgr.GetAllCustomers(True, m_pbl_Operator, m_pbl_Operator.ID)
    'For iRow = 0 To dtTemp.Rows.Count - 1
    '  Dim liNew As New ListItem
    '  liNew.Value = CStr(dtTemp.Rows(iRow)!Cust_id)
    '  liNew.Text = CStr(dtTemp.Rows(iRow)!Cust_name)
    '  ddEditCustomer.Items.Add(liNew)
    'Next


    ''Load Locations based on Customer
    'ddEditLocation.Items.Clear()
    'ddEditLocation.Items.Add(New ListItem("Select Location....", "-1"))
    'dtTemp = m_oBL_CustMgr.GetAllLocations(True, m_pbl_Operator, m_pbl_Operator.ID, CInt(ddCustomers.SelectedItem.Value))
    'For iRow = 0 To dtTemp.Rows.Count - 1
    '  Dim liNew As New ListItem
    '  liNew.Value = CStr(dtTemp.Rows(iRow)!Loc_id)
    '  liNew.Text = CStr(dtTemp.Rows(iRow)!loc_name)
    '  ddEditLocation.Items.Add(liNew)
    'Next


    ''Load Sub-Locations based on Location
    'ddEditSubLocation.Items.Clear()
    'ddEditSubLocation.Items.Add(New ListItem("Select Sub-locations....", "-1"))
    'dtTemp = m_oBL_CustMgr.GetAllsubLocationsByLocation(CInt(ddLocations.SelectedItem.Value), Nothing)
    'For iRow = 0 To dtTemp.Rows.Count - 1
    '  Dim liNew As New ListItem
    '  liNew.Value = CStr(dtTemp.Rows(iRow)!subloc_ID)
    '  liNew.Text = CStr(dtTemp.Rows(iRow)!subloc_name)
    '  ddEditSubLocation.Items.Add(liNew)
    'Next

    'Load Pipe Orientation
    ddPipeOrient.Items.Clear()
    ddPipeOrient.Items.Add(New ListItem("Select Pipe Orientation....", "-1"))
    dtTemp = m_oBL_DevManager.GetAllPipeOrientations()
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!pipeo_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!pipeo_Orientation)
      ddPipeOrient.Items.Add(liNew)
    Next

    'Load Pipe Diameter
    ddPipeDiameter.Items.Clear()
    ddPipeDiameter.Items.Add(New ListItem("Select Pipe Diameter....", "-1"))
    dtTemp = m_oBL_DevManager.GetAllPipeDiameters()
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!piped_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!piped_Diameter)
      ddPipeDiameter.Items.Add(liNew)
    Next

  End Sub

  Private Function zFormValid() As Boolean
    Dim bOK As Boolean = False


    If Trim(txtAssetTag.Text) <> "" Then
      If ddDevType.SelectedIndex <> 0 Then
        If ddEditOrganisation.SelectedIndex <> 0 Then
          If ddEditLocation.Items.Count > 0 Then
            If ddEditSubLocation.Items.Count > 0 Then
              If ddPipeOrient.SelectedIndex <> 0 Then
                If ddPipeDiameter.SelectedIndex <> 0 Then
                  bOK = True
                Else
                  m_pcl_Utils.DisplayMessage("Please select the Pipe Diameter", ddPipeDiameter.ClientID)
                End If
              Else
                m_pcl_Utils.DisplayMessage("Please select the Pipe Orientation", ddPipeOrient.ClientID)
              End If
            Else
              m_pcl_Utils.DisplayMessage("Please select the Sub-Location", ddsubLocations.ClientID)
            End If
          Else
            m_pcl_Utils.DisplayMessage("Please select the Location", ddLocations.ClientID)
          End If
        Else
          m_pcl_Utils.DisplayMessage("Please select the Organisation", ddorganizations.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please select the Device Type", ddDevType.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter the device Asset Tag", txtAssetTag.ClientID)
    End If

    If bOK Then
      bOK = False

      If Trim(txtLatitute.Text) <> "" Then
        If IsNumeric(txtLatitute.Text) Then
          If Trim(txtLongitude.Text) <> "" Then
            If IsNumeric(txtLongitude.Text) Then
              bOK = True
            Else
              m_pcl_Utils.DisplayMessage("Please enter a numberic Longitude.", txtLongitude.ClientID)
            End If
          Else
            'Blank Long is ok, nothing set.
          End If
        Else
          m_pcl_Utils.DisplayMessage("Please enter a numberic Latitude.", txtLatitute.ClientID)
        End If
      Else
        'Blank Lat is ok, nothing set.
        bOK = True
      End If



    End If


    Return bOK
  End Function

#End Region
#Region "Control Routines"
  Protected Sub ddorganizations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddorganizations.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Customers based on organisation
    ddCustomers.Items.Clear()
    ddCustomers.Items.Add(New ListItem("All Customers....", "-1"))


    dtTemp = m_oBL_CustMgr.GetAllCustomers(True, m_pbl_Operator, m_pbl_Operator.ID)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!Cust_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!Cust_name)
      ddCustomers.Items.Add(liNew)
    Next

    If ddCustomers.Items.Count > 1 Then
      If ddCustomers.Items.Count = 2 Then
        ddCustomers.SelectedIndex = 1
      Else
        ddCustomers.SelectedIndex = 0
      End If

      ddCustomers_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub

  Protected Sub ddCustomers_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddCustomers.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Locations based on Customer
    ddLocations.Items.Clear()
    ddLocations.Items.Add(New ListItem("All Locations....", "-1"))


    dtTemp = m_oBL_CustMgr.GetAllLocations(True, m_pbl_Operator, m_pbl_Operator.ID, CInt(ddCustomers.SelectedItem.Value))
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!Loc_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!loc_name)
      ddLocations.Items.Add(liNew)
    Next

    If ddLocations.Items.Count > 1 Then
      If ddLocations.Items.Count = 2 Then
        ddLocations.SelectedIndex = 1
      Else
        ddLocations.SelectedIndex = 0
      End If

      ddLocations_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub
  Protected Sub ddLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddLocations.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Locations based on Customer
    ddsubLocations.Items.Clear()
    ddsubLocations.Items.Add(New ListItem("All Sub-locations....", "-1"))


    dtTemp = m_oBL_CustMgr.GetAllsubLocationsByLocation(CInt(ddLocations.SelectedItem.Value), m_pbl_Operator, m_pbl_Operator.ID, CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value))

    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!subloc_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!subloc_name)
      ddsubLocations.Items.Add(liNew)
    Next

    If ddsubLocations.Items.Count > 1 Then
      If ddsubLocations.Items.Count = 2 Then
        ddsubLocations.SelectedIndex = 1
      Else
        ddsubLocations.SelectedIndex = 0
      End If

      ddsubLocations_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub

  Protected Sub ddsubLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddsubLocations.SelectedIndexChanged

    zLoadGrid()
  End Sub

  Private Sub gvDevices_RowCommand(sender As Object, e As GridViewCommandEventArgs) Handles gvDevices.RowCommand
    Dim iDev As Int32 = CInt(e.CommandArgument)
    Dim commandName As String = e.CommandName
    Dim drDevice As DataRow

    'Edit...

    drDevice = m_oBL_DevManager.GetDeviceByID(iDev)

    'Load the screen fields...
    zloadEditDropDowns()
    lblEditTitle.Text = "Edit - Asset: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_AssetTag) & ", IMEI: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_IMEI)
    txtDevID.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_id)
    txtDevTS.Text = Format(m_oDB_Layer.CheckDBNullDate(drDevice!dev_Timestamp), "yyyy-MM-dd HH:mm:ss")
    txtAssetTag.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_AssetTag)
    m_pcl_Utils.DropDownSelectByValue(ddDevType, m_oDB_Layer.CheckDBNullStr(drDevice!devtype_id))

    m_pcl_Utils.DropDownSelectByValue(ddDevModel, m_oDB_Layer.CheckDBNullStr(drDevice!devh_id))

    'Select the device customer and location details.
    m_pcl_Utils.DropDownSelectByValue(ddEditOrganisation, m_oDB_Layer.CheckDBNullStr(drDevice!org_id))
    ddEditOrganisation_SelectedIndexChanged(Nothing, Nothing)

    m_pcl_Utils.DropDownSelectByValue(ddEditCustomer, m_oDB_Layer.CheckDBNullStr(drDevice!cust_id))
    ddEditCustomer_SelectedIndexChanged(Nothing, Nothing)

    m_pcl_Utils.DropDownSelectByValue(ddEditLocation, m_oDB_Layer.CheckDBNullStr(drDevice!loc_id))
    ddEditLocation_SelectedIndexChanged(Nothing, Nothing)

    m_pcl_Utils.DropDownSelectByValue(ddEditSubLocation, m_oDB_Layer.CheckDBNullStr(drDevice!subloc_id))

    txtLocationRef.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_LocationRef)
    m_pcl_Utils.DropDownSelectByValue(ddPipeOrient, m_oDB_Layer.CheckDBNullStr(drDevice!pipeo_ID))
    m_pcl_Utils.DropDownSelectByValue(ddPipeDiameter, m_oDB_Layer.CheckDBNullStr(drDevice!piped_ID))
    txtSerialNo.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_SerialNumber)
    txtInternalNote.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_InternalNotes)



    'txtOtaAttempts.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_LastOtaAttempts)
    'txtOtaSuccess.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_LastOtaSuccess)

    If m_oDB_Layer.CheckDBNullBool(drDevice!dev_Live) Then
      chkDevLive.Checked = True
    Else
      chkDevLive.Checked = False
    End If

    If m_oDB_Layer.CheckDBNullDec(drDevice!dev_Latitude) <> -999 Then
      txtLatitute.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_Latitude)
    Else
      txtLatitute.Text = ""
    End If

    If m_oDB_Layer.CheckDBNullDec(drDevice!dev_Longitude) <> -999 Then
      txtLongitude.Text = m_oDB_Layer.CheckDBNullStr(drDevice!dev_Longitude)
    Else
      txtLongitude.Text = ""
    End If


    zShowPopUp(True)



  End Sub

  Protected Sub cmdUpdate_Click(sender As Object, e As EventArgs) Handles cmdUpdate.Click
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return

    'Validate form.
    If zFormValid() Then
      'Update the device....
      dicParms.Add("org_id", "N:" & ddEditOrganisation.SelectedItem.Value)
      dicParms.Add("cust_ID", "N:" & ddEditCustomer.SelectedItem.Value)
      dicParms.Add("loc_id", "N:" & ddEditLocation.SelectedItem.Value)
      dicParms.Add("subloc_id", "N:" & ddEditSubLocation.SelectedItem.Value)
      dicParms.Add("dev_LocationRef", "S:" & Trim(m_pcl_Utils.Strip2Alphanumeric(txtLocationRef.Text, "",, True, True,, True, True, True, True, True,, True, True)))
      dicParms.Add("pipeo_ID", "N:" & ddPipeOrient.SelectedItem.Value)
      dicParms.Add("piped_ID", "N:" & ddPipeDiameter.SelectedItem.Value)
      dicParms.Add("devtype_id", "N:" & ddDevType.SelectedItem.Value)
      dicParms.Add("dev_SerialNumber", "S:" & Trim(m_pcl_Utils.Strip2Alphanumeric(txtSerialNo.Text, "",, True, True,, True, True, True, True, True,, True, True)))
      dicParms.Add("dev_AssetTag", "S:" & Trim(m_pcl_Utils.Strip2Alphanumeric(txtAssetTag.Text, "",, True, True,, True, True, True, True, True,, True, True)))
      dicParms.Add("dev_InternalNotes", "S:" & Trim(m_pcl_Utils.Strip2Alphanumeric(txtInternalNote.Text, "",, True, True,, True, True, True, True, True,, True, True)))
      If chkDevLive.Checked Then
        dicParms.Add("dev_Live", "N:" & "-1")
      Else
        dicParms.Add("dev_Live", "N:" & "0")
      End If

      If IsNumeric(txtLatitute.Text) Then
        dicParms.Add("dev_Latitude", "N:" & Trim(txtLatitute.Text))
      Else
        dicParms.Add("dev_Latitude", "N:" & "-999")
      End If

      If IsNumeric(txtLongitude.Text) Then
        dicParms.Add("dev_Longitude", "N:" & Trim(txtLongitude.Text))
      Else
        dicParms.Add("dev_Longitude", "N:" & "-999")
      End If


      dicParms.Add("dev_timestamp", "T:" & txtDevTS.Text)

      dbRet = m_oBL_DevManager.UpdateDevice(CInt(txtDevID.Text), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        zLoadGrid()
        zShowPopUp(False)
      Else
        m_pcl_Utils.DisplayMessage("It appears that the device data has been updated by another person, please close and reapply the changes.")
      End If


    End If


  End Sub

  Private Sub zShowPopUp(bShow As Boolean)
    pnlOverlay.Visible = bShow
    pnlPopUpPanel.Visible = bShow
  End Sub
  Protected Sub cmdClosePopUp_Click(sender As Object, e As ImageClickEventArgs) Handles cmdClosePopUp.Click
    zShowPopUp(False)
  End Sub
  Private Sub gvDevices_RowEditing(sender As Object, e As GridViewEditEventArgs) Handles gvDevices.RowEditing

  End Sub
  Protected Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click
    zLoadGrid()
    zShowPopUp(False)
  End Sub

  Private Sub gvDevices_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvDevices.RowDataBound
    Dim pGridViewRow As GridViewRow

    Dim hypDeviceData As HyperLink = CType(e.Row.FindControl("hypDeviceData"), HyperLink)
    Dim lblLastData As Label = CType(e.Row.FindControl("lblLastData"), Label)
    Dim ChkLive As CheckBox = CType(e.Row.FindControl("ChkLive"), CheckBox)
    Dim drvInput As DataRowView
    Dim tsAge As TimeSpan
    Dim sTemp As String
    Dim tcLastData As TableCell
    Dim tcLive As TableCell


    pGridViewRow = e.Row

    Select Case e.Row.RowType
      Case DataControlRowType.Header

      Case DataControlRowType.DataRow
        drvInput = CType(pGridViewRow.DataItem, DataRowView)

        hypDeviceData.NavigateUrl = "DeviceDataAnalysis.aspx?devid=" & m_pcl_Utils.EncryptStringOld(Format(CInt(drvInput.Row!dev_id), "000000"))

        tsAge = Now.ToUniversalTime() - CDate(drvInput.Row!ddata_DateTimeUtc)
        sTemp = ""
        If tsAge.Days > 0 Then
          sTemp = tsAge.Days & "d "
        End If
        If tsAge.Hours > 0 Then
          sTemp += tsAge.Hours & "h "
        End If
        sTemp += tsAge.Minutes & "m "
        lblLastData.Text = sTemp

        tcLastData = CType(lblLastData.Parent, TableCell)
        tcLive = CType(ChkLive.Parent, TableCell)


        Select Case True
          Case (tsAge.Duration.Days >= 7)
            tcLastData.BackColor = Color.LightCoral

          Case (tsAge.Duration.Days >= 1)
            tcLastData.BackColor = Color.LightGoldenrodYellow

          Case Else
            tcLastData.BackColor = Color.LightGreen

        End Select

        If m_oDB_Layer.CheckDBNullBool(drvInput!dev_Live) Then
          ChkLive.Checked = True
          tcLive.BackColor = Color.LightGreen
        Else
          chkDevLive.Checked = False
          'tcLive.BackColor = Color.Pink
        End If





    End Select


  End Sub

  Protected Sub ddEditOrganisation_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddEditOrganisation.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Customers based on organisation
    ddEditCustomer.Items.Clear()

    dtTemp = m_oBL_CustMgr.GetAllCustomers(True, m_pbl_Operator, m_pbl_Operator.ID)
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!Cust_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!Cust_name)
      ddEditCustomer.Items.Add(liNew)
    Next

    If ddEditCustomer.Items.Count > 0 Then
      If ddEditCustomer.Items.Count = 1 Then
        ddEditCustomer.SelectedIndex = 0
      End If

      ddEditCustomer_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub

  Protected Sub ddEditCustomer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddEditCustomer.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Locations based on Customer
    ddEditLocation.Items.Clear()

    dtTemp = m_oBL_CustMgr.GetAllLocations(True, m_pbl_Operator, m_pbl_Operator.ID, CInt(ddEditCustomer.SelectedItem.Value))
    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!Loc_id)
      liNew.Text = CStr(dtTemp.Rows(iRow)!loc_name)
      ddEditLocation.Items.Add(liNew)
    Next

    If ddEditLocation.Items.Count > 0 Then
      If ddEditLocation.Items.Count = 1 Then
        ddEditLocation.SelectedIndex = 0
      End If

      ddEditLocation_SelectedIndexChanged(Nothing, Nothing)
    End If
  End Sub

  Protected Sub ddEditLocation_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddEditLocation.SelectedIndexChanged
    Dim dtTemp As DataTable
    Dim iRow As Int32

    'Load Locations based on Customer
    ddEditSubLocation.Items.Clear()


    dtTemp = m_oBL_CustMgr.GetAllsubLocationsByLocation(CInt(ddLocations.SelectedItem.Value), m_pbl_Operator, m_pbl_Operator.ID, CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value))

    For iRow = 0 To dtTemp.Rows.Count - 1
      Dim liNew As New ListItem
      liNew.Value = CStr(dtTemp.Rows(iRow)!subloc_ID)
      liNew.Text = CStr(dtTemp.Rows(iRow)!subloc_name)
      ddEditSubLocation.Items.Add(liNew)
    Next

    If ddEditSubLocation.Items.Count > 1 Then
      If ddEditSubLocation.Items.Count = 2 Then
        ddEditSubLocation.SelectedIndex = 1
      Else
        ddEditSubLocation.SelectedIndex = 0
      End If
    End If

  End Sub





#End Region
End Class