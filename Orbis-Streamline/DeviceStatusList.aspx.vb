Option Strict On
Imports System.Drawing
Imports Telerik.Web.UI
Imports MySql.Data

Public Class DeviceStatusList
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

    dtDevices = m_oBL_DevManager.GetDevicesForStatusList(CInt(ddorganizations.SelectedItem.Value), CInt(ddCustomers.SelectedItem.Value), CInt(ddLocations.SelectedItem.Value), CInt(ddsubLocations.SelectedItem.Value), True, 24)

    gvDevices.DataSource = dtDevices
    gvDevices.DataBind()

  End Sub

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

    drDevice = m_oBL_DevManager.GetDeviceByID(iDev)


  End Sub


  Private Sub gvDevices_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvDevices.RowDataBound
    Dim pGridViewRow As GridViewRow

    Dim hypDeviceData As HyperLink = CType(e.Row.FindControl("hypDeviceData"), HyperLink)

    Dim lblFlowThermal As Label = CType(e.Row.FindControl("lblFlowThermal"), Label)
    Dim lblFlowUSonic As Label = CType(e.Row.FindControl("lblFlowUSonic"), Label)
    Dim lblFlowAcoustic As Label = CType(e.Row.FindControl("lblFlowAcoustic"), Label)
    Dim lblPipeTemp As Label = CType(e.Row.FindControl("lblPipeTemp"), Label)
    Dim lblBoardTemp As Label = CType(e.Row.FindControl("lblBoardTemp"), Label)
    Dim lblPipeCond As Label = CType(e.Row.FindControl("lblPipeCond"), Label)
    Dim lblLeakAcoustic As Label = CType(e.Row.FindControl("lblLeakAcoustic"), Label)
    Dim lblLeakConductive As Label = CType(e.Row.FindControl("lblLeakConductive"), Label)
    Dim lblVibration As Label = CType(e.Row.FindControl("lblVibration"), Label)
    Dim lblPressure As Label = CType(e.Row.FindControl("lblPressure"), Label)
    Dim lblLegionella As Label = CType(e.Row.FindControl("lblLegionella"), Label)


    Dim lblBattery As Label = CType(e.Row.FindControl("lblBattery"), Label)
    Dim lblComms As Label = CType(e.Row.FindControl("lblComms"), Label)

    Dim drvInput As DataRowView
    Dim tsAge As TimeSpan
    Dim tcFlowThermal As TableCell
    Dim tcFlowuSonic As TableCell
    Dim tcFlowAcoustic As TableCell
    Dim tcPipeTemp As TableCell
    Dim tcBoardTemp As TableCell
    Dim tcPipeCond As TableCell
    Dim tcLeakAcoustic As TableCell
    Dim tcLeakConductive As TableCell
    Dim tcVibration As TableCell
    Dim tcPressure As TableCell
    Dim tcLegionella As TableCell

    Dim tcLastData As TableCell
    Dim tcBattery As TableCell
    Dim sTempTime As String
    Dim sAlertToolTip As String = ""
    Dim colorAlert As System.Drawing.Color = Color.Red
    Dim colorAlertFore As System.Drawing.Color = Color.White
    Dim colorWarn As System.Drawing.Color = Color.Yellow
    Dim colorOK As System.Drawing.Color = Color.Green
    Dim colorOKFore As System.Drawing.Color = Color.White


    pGridViewRow = e.Row


    Select Case e.Row.RowType
      Case DataControlRowType.Header

      Case DataControlRowType.DataRow
        drvInput = CType(pGridViewRow.DataItem, DataRowView)

        hypDeviceData.NavigateUrl = "DeviceDataAnalysis.aspx?devid=" & m_pcl_Utils.EncryptStringOld(Format(CInt(drvInput.Row!dev_id), "000000"))


        tcFlowThermal = CType(lblFlowThermal.Parent, TableCell)
        tcFlowuSonic = CType(lblFlowUSonic.Parent, TableCell)
        tcFlowAcoustic = CType(lblFlowAcoustic.Parent, TableCell)
        tcPipeTemp = CType(lblPipeTemp.Parent, TableCell)
        tcBoardTemp = CType(lblBoardTemp.Parent, TableCell)
        tcPipeCond = CType(lblPipeCond.Parent, TableCell)
        tcLeakAcoustic = CType(lblLeakAcoustic.Parent, TableCell)
        tcLeakConductive = CType(lblLeakConductive.Parent, TableCell)
        tcVibration = CType(lblVibration.Parent, TableCell)
        tcPressure = CType(lblPressure.Parent, TableCell)
        tcLegionella = CType(lblLegionella.Parent, TableCell)

        tcBattery = CType(lblBattery.Parent, TableCell)
        tcLastData = CType(lblComms.Parent, TableCell)

        ''Flow Events
        'Select Case m_oDB_Layer.CheckDBNullInt(drvInput!FlowEventCount)
        '  Case 0
        '    tcFlowThermal.BackColor = colorOK
        '  Case Else
        '    tcFlowThermal.BackColor = colorAlert
        '    lblFlowThermal.ForeColor = colorAlertFore
        '    sAlertToolTip += "Flow Event Count: " & m_oDB_Layer.CheckDBNullInt(drvInput!FlowEventCount) & Environment.NewLine
        'End Select

        ''Vibration
        'tcVibration.BackColor = colorOK
        'Select Case m_oDB_Layer.CheckDBNullInt(drvInput!FlowVibrationCount)
        '  Case 0
        '    tcVibration.BackColor = colorOK
        '  Case Else
        '    tcVibration.BackColor = colorAlert
        '    lblVibration.ForeColor = colorAlertFore
        '    sAlertToolTip += "Flow Vibration Count: " & m_oDB_Layer.CheckDBNullInt(drvInput!FlowVibrationCount) & Environment.NewLine
        'End Select

        ''Flow Acoustic
        'tcFlowAcoustic.BackColor = colorOK
        'Select Case m_oDB_Layer.CheckDBNullInt(drvInput!FlowAcousticCount)
        '  Case 0
        '    tcFlowAcoustic.BackColor = colorOK
        '  Case Else
        '    tcFlowAcoustic.BackColor = colorAlert
        '    lblFlowAcoustic.ForeColor = colorAlertFore
        '    sAlertToolTip += "Flow Acoustic Count: " & m_oDB_Layer.CheckDBNullInt(drvInput!FlowAcousticCount) & Environment.NewLine
        'End Select




        'Flow UltraSonic
        tcFlowuSonic.BackColor = colorOK


        'Pipe Temp
        tcPipeTemp.BackColor = colorOK
        lblPipeTemp.ForeColor = colorAlertFore


        'Board Temp
        If Not IsDBNull(drvInput!ddata_AmbientTemperature) Then
          Select Case m_oDB_Layer.CheckDBNullInt(drvInput!ddata_BoardTemperature)
            Case > 60
              tcBoardTemp.BackColor = colorAlert
              sAlertToolTip += "Board Temp: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_BoardTemperature) & Environment.NewLine
              lblBoardTemp.ForeColor = colorAlertFore

            Case > 45
              tcBoardTemp.BackColor = colorWarn
              sAlertToolTip += "Board Temp: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_BoardTemperature) & Environment.NewLine


            Case > 10
              tcBoardTemp.BackColor = colorOK

            Case Else
              tcBoardTemp.BackColor = colorWarn
              sAlertToolTip += "Board Temp: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_BoardTemperature) & Environment.NewLine

          End Select

        Else
          tcBoardTemp.BackColor = colorOK
        End If

        'Pipe Cond
        If CInt(drvInput!dev_id) <> 3 Then
          tcPipeCond.BackColor = colorOK
        Else
          tcPipeCond.BackColor = colorWarn
          lblPipeCond.Text = "5.349"
        End If

        'Leak Acoustic
        If Not IsDBNull(drvInput!ddata_LeakProbability) Then
          If CDec(drvInput!ddata_LeakProbability) <> 0 Then
            lblLeakAcoustic.Text = m_oDB_Layer.CheckDBNullStr(drvInput!ddata_LeakProbability)
            Select Case m_oDB_Layer.CheckDBNullDec(drvInput!ddata_LeakProbability)

              Case > 5
                tcLeakAcoustic.BackColor = colorAlert
                lblLeakAcoustic.ForeColor = colorAlertFore

              Case > CDec(1.9)
                tcLeakAcoustic.BackColor = colorWarn

              Case Else
                tcLeakAcoustic.BackColor = colorOK
                lblLeakAcoustic.ForeColor = colorOKFore

            End Select
          Else
            tcLeakAcoustic.BackColor = colorOK
          End If
        Else
          tcLeakAcoustic.BackColor = colorOK
        End If


        'Leak Conductive
        If m_oDB_Layer.CheckDBNullDec(drvInput!ddata_CordLeak) > 0 Then
          tcLeakConductive.BackColor = colorAlert
          lblLeakConductive.ForeColor = colorAlertFore
          sAlertToolTip += "Conductive Leak Detected. " & Environment.NewLine
        Else
          tcLeakConductive.BackColor = colorOK
          lblLeakConductive.ForeColor = colorOKFore
        End If




        'Pressure
        tcPressure.BackColor = colorOK
        If Not IsDBNull(drvInput!PressureAvg) Then
          'we have pressure!!
          If CInt(drvInput!PressureAvg) <> 0 Then
            lblPressure.Text = Format(CInt(drvInput!PressureAvg), "###,##0")
            lblPressure.ForeColor = colorAlertFore
          End If



        End If

          'Legionella
          tcLegionella.BackColor = colorOK

        'Battery Level

        tcBattery.BackColor = colorOK
        lblBattery.ForeColor = colorOKFore
        'Select Case m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage)
        '  Case > 600
        '    tcBattery.BackColor = colorWarn
        '    sAlertToolTip += "Battery: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage) & Environment.NewLine
        '  Case > 330
        '    tcBattery.BackColor = colorOK
        '    lblBattery.ForeColor = colorOKFore

        '  Case > 300
        '    tcBattery.BackColor = colorWarn
        '    sAlertToolTip += "Battery: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage) & Environment.NewLine

        '  Case Else
        '    tcBattery.BackColor = colorAlert
        '    lblBattery.ForeColor = colorAlertFore
        '    sAlertToolTip += "Battery: " & m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage) & Environment.NewLine
        'End Select



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
            tcLastData.BackColor = colorAlert
            lblComms.ForeColor = colorAlertFore
            sAlertToolTip += "Last Comms: " & sTempTime & Environment.NewLine

          Case (tsAge.Duration.Days >= 1)
            tcLastData.BackColor = colorWarn
            sAlertToolTip += "Last Comms: " & sTempTime & Environment.NewLine

          Case Else
            tcLastData.BackColor = colorOK
            lblComms.ForeColor = colorOKFore

        End Select



        If chkDetailedData.Checked Then
          'lblFlowThermal.Text = m_oDB_Layer.CheckDBNullStr(drvInput!FlowEventCount)
          'lblVibration.Text = m_oDB_Layer.CheckDBNullStr(drvInput!FlowVibrationCount)
          'lblFlowAcoustic.Text = m_oDB_Layer.CheckDBNullStr(drvInput!FlowAcousticCount)
          If Not IsDBNull(drvInput!PipeTempAvg) Then
            lblPipeTemp.Text = Format(m_oDB_Layer.CheckDBNullDec(drvInput!PipeTempAvg), "##0.0")
          End If

          lblBoardTemp.Text = m_oDB_Layer.CheckDBNullStr(drvInput!ddata_BoardTemperature)
          If m_oDB_Layer.CheckDBNullStr(drvInput!ddata_CordLeak) <> "0" Then
            lblLeakConductive.Text = m_oDB_Layer.CheckDBNullStr(drvInput!ddata_CordLeak)
          End If

          lblBattery.Text = CStr(m_oDB_Layer.CheckDBNullInt(drvInput!ddata_AdmBatteryVoltage))
          lblComms.Text = sTempTime

        End If



          pGridViewRow.ToolTip = sAlertToolTip



    End Select


  End Sub

  Protected Sub chkDetailedData_CheckedChanged(sender As Object, e As EventArgs) Handles chkDetailedData.CheckedChanged
    zLoadGrid()
  End Sub





#End Region
End Class