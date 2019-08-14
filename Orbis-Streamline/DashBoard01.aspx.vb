Option Strict On
Imports System.Drawing
Imports Telerik.Web.UI
Imports MySql.Data

Public Class DashBoard01
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DevManager As BL_DeviceManager
  Private m_oBL_CustMgr As BL_CustomerManager
  Private m_dtAlerts As DataTable
  Private m_dtMapPoints As DataTable

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then
      zLoadDropdowns()

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

  Private Sub zLoadDashboard()
    'Load Dashboard controls.

    Dim dtPieData01 As New DataTable()
    Dim dtPieData02 As New DataTable()
    Dim dtPieData03 As New DataTable()
    Dim dtAlerts As DataTable

    dtPieData01 = m_oBL_DevManager.getDeviceDataSummaryByType(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), True)
    pieUnitsByType.DataSource = dtPieData01
    pieUnitsByType.DataBind()

    dtPieData02 = m_oBL_DevManager.getDeviceCommsSummary(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), True)
    pieCommsStatus.DataSource = dtPieData02
    pieCommsStatus.DataBind()

    dtPieData03 = m_oBL_DevManager.getDeviceLocationSummary(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), True)
    pieDevByLocation.DataSource = dtPieData03
    pieDevByLocation.DataBind()

    dtAlerts = zBuildAlertsTable()
    pieAlerts.DataSource = dtAlerts
    pieAlerts.DataBind()

    gvAlerts.DataSource = m_dtAlerts
    gvAlerts.DataBind()

    mapDevices.DataSource = m_dtMapPoints
    mapDevices.DataBind()

  End Sub

  Private Function zBuildAlertsTable() As DataTable
    Dim dtRet As New DataTable
    Dim dtDevStatus As DataTable
    Dim i As Int32
    Dim drDevice As DataRow
    Dim iAlertCount As Int32 = 0
    Dim iWarning As Int32 = 0
    Dim tsAge As TimeSpan
    Dim sTempTime As String
    Dim bOK As Boolean = False

    Diagnostics.Debug.WriteLine("Build Alerts Table Start: " & Format(Now(), "yyyy-MM-dd HH:mm:ss.ffff"))

    dtRet.Columns.Add("DevStatus", Type.[GetType]("System.String"))
    dtRet.Columns.Add("Kounter", Type.[GetType]("System.Int32"))
    dtRet.Columns.Add("AlertColour", Type.[GetType]("System.String"))

    dtDevStatus = m_oBL_DevManager.GetDevicesForStatusList(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), True, 24)
    dtDevStatus.Columns.Add("HasAlert", Type.[GetType]("System.Int32"))
    dtDevStatus.Columns.Add("MapShape", Type.[GetType]("System.String"))
    dtDevStatus.Columns.Add("MapTitle", Type.[GetType]("System.String"))
    dtDevStatus.Columns.Add("MapToolTip", Type.[GetType]("System.String"))


    Diagnostics.Debug.WriteLine("Build Alerts Table Process Device Statuses: " & Format(Now(), "yyyy-MM-dd HH:mm:ss.ffff"))


    m_dtAlerts = dtDevStatus.Clone
    m_dtMapPoints = dtDevStatus.Clone

    For i = 0 To dtDevStatus.Rows.Count - 1
      drDevice = dtDevStatus.Rows(i)

      bOK = False

      'If m_oDB_Layer.CheckDBNullInt(drDevice!FlowEventCount) = 0 Then
      If m_oDB_Layer.CheckDBNullInt(drDevice!ddata_AmbientTemperature) < 60 Then
        If m_oDB_Layer.CheckDBNullInt(drDevice!ddata_AdmBatteryVoltage) > 300 Then
          'Comms Status
          tsAge = Now.ToUniversalTime() - CDate(drDevice!ddata_DateTimeUtc)
          sTempTime = ""
          If tsAge.Days > 0 Then
            sTempTime = tsAge.Days & "d "
          End If
          If tsAge.Hours > 0 Then
            sTempTime += tsAge.Hours & "h "
          End If
          sTempTime += tsAge.Minutes & "m "

          Select Case True
            Case (tsAge.Duration.Days >= 7)
              iAlertCount += 1

            Case (tsAge.Duration.Days >= 1)
              iWarning += 1
              bOK = True

            Case Else
              'Normal no problem
              bOK = True

          End Select

        Else
          iAlertCount += 1
        End If
      Else
        iAlertCount += 1
      End If
      'Else
      '  iAlertCount += 1
      'End If

      If bOK Then
        drDevice!HasAlert = 0
        drDevice!MapShape = "my-custom-shape-ok"
      Else
        drDevice!HasAlert = -1
        'drDevice!MapShape = "my-custom-shape-alert"
        drDevice!MapShape = "my-custom-shape-ok"
      End If

      drDevice!MapTitle = "Asset Tag: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_AssetTag)
      drDevice!MapToolTip = m_oDB_Layer.CheckDBNullStr(drDevice!cust_Name) & ", " & m_oDB_Layer.CheckDBNullStr(drDevice!loc_name) & ", " & m_oDB_Layer.CheckDBNullStr(drDevice!subloc_name)

      If m_oDB_Layer.CheckDBNullDec(drDevice!dev_Latitude) <> -999 Then
        m_dtMapPoints.ImportRow(drDevice)
      End If


      If bOK = False Then
        m_dtAlerts.ImportRow(drDevice)
      End If


    Next

    If iAlertCount > 0 Then
      'shift to percentages
      dtRet.Rows.Add("OK", (dtDevStatus.Rows.Count - iAlertCount - iWarning) / dtDevStatus.Rows.Count * 100, "#90ee90")
      dtRet.Rows.Add("Warning", iWarning / dtDevStatus.Rows.Count * 100, "#fafad2")
      dtRet.Rows.Add("Alert", iAlertCount / dtDevStatus.Rows.Count * 100, "#F08080")

    End If

    Diagnostics.Debug.WriteLine("Build Alerts Table Done: " & Format(Now(), "yyyy-MM-dd HH:mm:ss.ffff"))

    Return dtRet

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

      zLoadDashboard()
    End If
  End Sub

  Protected Sub ddsubLocations_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddsubLocations.SelectedIndexChanged
    zLoadDashboard()
  End Sub

  Private Sub gvAlerts_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvAlerts.RowDataBound
    Dim pGridViewRow As GridViewRow

    Dim hypDeviceData As HyperLink = CType(e.Row.FindControl("hypDeviceData"), HyperLink)
    Dim lblDeviceStatus As Label = CType(e.Row.FindControl("lblDeviceStatus"), Label)

    Dim drvInput As DataRowView
    Dim tsAge As TimeSpan
    Dim sTempTime As String

    pGridViewRow = e.Row

    Select Case e.Row.RowType
      Case DataControlRowType.Header

      Case DataControlRowType.DataRow
        drvInput = CType(pGridViewRow.DataItem, DataRowView)

        hypDeviceData.NavigateUrl = "DeviceDataAnalysis.aspx?devid=" & m_pcl_Utils.EncryptStringOld(Format(CInt(drvInput.Row!dev_id), "000000"))

        ''Flow Thermal
        'Select Case m_oDB_Layer.CheckDBNullInt(drvInput!FlowEventCount)
        '  Case 0

        '  Case Else
        '    lblDeviceStatus.Text += "Flow Thermal Detected" & "<br/>"
        'End Select

        'Board Temp
        If Not IsDBNull(drvInput!ddata_AmbientTemperature) Then

          Select Case m_oDB_Layer.CheckDBNullInt(drvInput!ddata_BoardTemperature)
            Case > 60
              lblDeviceStatus.Text += "Flow Thermal Detected" & "<br/>"
            Case > 45


            Case > 10


            Case Else

          End Select

        Else

        End If


        'Leak Conductive
        If m_oDB_Layer.CheckDBNullDec(drvInput!ddata_CordLeak) > 0 Then
          lblDeviceStatus.Text += "Leak Conductive Detected" & "<br/>"
        Else

        End If


        'Battery Level
        Select Case m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage)
          Case > 400
          Case > 330

          Case > 300

          Case Else
            lblDeviceStatus.Text += "Low Battery" & "<br/>"
        End Select



        'Comms Status
        tsAge = Now.ToUniversalTime() - CDate(drvInput.Row!ddata_DateTimeUtc)
        sTempTime = ""
        If tsAge.Days > 0 Then
          sTempTime = tsAge.Days & "d "
        End If
        If tsAge.Hours > 0 Then
          sTempTime += tsAge.Hours & "h "
        End If
        sTempTime += tsAge.Minutes & "m "

        Select Case True
          Case (tsAge.Duration.Days >= 7)
            lblDeviceStatus.Text += "Device Comms Alert: " & sTempTime & "<br/>"

          Case (tsAge.Duration.Days >= 1)

          Case Else

        End Select





    End Select


  End Sub


#End Region
End Class