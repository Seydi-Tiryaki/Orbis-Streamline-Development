Option Strict On
Imports System.Drawing
Imports Telerik.Web.UI
Imports MySql.Data

Public Class DeviceDataAnalysis
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DevManager As BL_DeviceManager


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Dim sTemp As String

    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then
      If Request.QueryString("devid") IsNot Nothing Then
        Try
          sTemp = m_pcl_Utils.DecryptStringOld(Request.QueryString("devid"))
          If IsNumeric(sTemp) Then
            txtDevID.Text = CStr(sTemp)


          End If

        Catch ex As Exception
          Server.ClearError()
          Response.Redirect("DashBoard01.aspx")
        End Try

        zLoadDropdowns

        zLoadDeviceDetails()

      Else
        Response.Redirect("DashBoard01.aspx")
      End If

      If CInt(txtDevID.Text) <> -1 Then
                zLoadGraphs(CInt(ddDateRange.SelectedValue))
            End If
    Else

    End If

  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '

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

    Return True

  End Function

    Private Sub zLoadGraphs(Days As Int32)
        'Load Dashboard controls.

        Try
            Dim iDev_id As Int32 = CInt(txtDevID.Text)

            Dim i As Int32
            Dim drData As DataRow
            Dim drNew As DataRow
            Dim iTempMin As Int32 = 0
            Dim iTempMax As Int32 = 0
            Dim drTemp As DataRow

            Dim dtLineDailyAvgs01 As New DataTable()
            Dim dtTemperature As New DataTable()
            Dim dtDailyFlowEvents As New DataTable()
            Dim dtLineLeakDaily As New DataTable()
            Dim dtVibrationCount As New DataTable()
            Dim dtLineData05 As New DataTable()
            Dim dtDeviceDetailed As New DataTable
            Dim dtFlowUsonicSample As New DataTable
            Dim dtStrainSample As New DataTable
            Dim dtLeakEnhanced As DataTable
            Dim dtStrainDailyAvg7dma As New DataTable

            dtLineDailyAvgs01 = m_oBL_DevManager.GetDeviceDataDailyAvg(CInt(iDev_id), Days)
            dtDailyFlowEvents = m_oBL_DevManager.GetDeviceFlowDailyAvg(CInt(iDev_id), Days, False)
            'dtLineLeakDaily = m_oBL_DevManager.GetDeviceLeakCountDaily(CInt(iDev_id), Days)
            dtVibrationCount = m_oBL_DevManager.GetDeviceFlowDailyAvg(CInt(iDev_id), Days, True)
            dtLineData05 = m_oBL_DevManager.GetDeviceZeroesDaily(CInt(iDev_id), Days)
            dtDeviceDetailed = m_oBL_DevManager.GetDeviceDetailed(CInt(iDev_id), Days)
            dtFlowUsonicSample = m_oBL_DevManager.GetFlowUsonicSample(Days)
            dtStrainSample = m_oBL_DevManager.GetStrainSample(Days)
            dtLeakEnhanced = m_oBL_DevManager.GetDeviceLeakDetailed(CInt(iDev_id), Days)
            dtStrainDailyAvg7dma = m_oBL_DevManager.GetDeviceDataStrainDailyAvg(CInt(iDev_id), Days)


            '1. Flow Activity
            '==================
            lineDailyFlowEvents.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
            lineDailyFlowEvents.DataSource = dtDailyFlowEvents
            lineDailyFlowEvents.DataBind()


            '2. Ultrasonic Flow
            '==================
            lineuSonicFlow.DataSource = dtFlowUsonicSample 'Now data zeroed!
            lineuSonicFlow.DataBind()


            '3. Leak Enhanced
            '======================
            'For i = 0 To dtLineLeakDaily.Rows.Count - 1
            '  drData = dtLineLeakDaily.Rows(i)

            '  LineLeakage.PlotArea.XAxis.Items.Add(m_oDB_Layer.CheckDBNullStr(drData!DateSeq))

            '  'Leak Probability
            '  Dim itemNewS0 As New SeriesItem
            '  itemNewS0.YValue = m_oDB_Layer.CheckDBNullDec(drData!LeakProbability)

            '  Select Case itemNewS0.YValue
            '    Case > 1.9
            '      itemNewS0.BackgroundColor = Color.Red
            '    Case > 1.4
            '      itemNewS0.BackgroundColor = Color.Orange
            '    Case Else
            '      itemNewS0.BackgroundColor = Color.Green
            '  End Select
            '  LineLeakage.PlotArea.Series(0).Items.Add(itemNewS0)

            '  'Cord Leak
            '  Dim itemNewS1 As New SeriesItem
            '  itemNewS1.YValue = m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
            '  Select Case m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
            '    Case 1
            '      itemNewS1.BackgroundColor = Color.Red
            '    Case 0
            '      itemNewS1.BackgroundColor = Color.Green

            '  End Select
            '  LineLeakage.PlotArea.Series(1).Items.Add(itemNewS1)

            '  'Threshold
            '  Dim itemNewS2 As New SeriesItem
            '  itemNewS2.YValue = CDec(1.9)
            '  LineLeakage.PlotArea.Series(2).Items.Add(itemNewS2)

            'Next

            'Populate Line Leakage Detailed
            '--------------------------------
            Dim dtLeakDetailed As New DataTable
            dtLeakDetailed.Columns.Add("LeakDate", GetType(DateTime))
            dtLeakDetailed.Columns.Add("LeakProb", GetType(Decimal))
            dtLeakDetailed.Columns.Add("LeakColor", GetType(String))
            dtLeakDetailed.Columns.Add("LeakConductive", GetType(Decimal))
            dtLeakDetailed.Columns.Add("LeakColorCond", GetType(String))
            Dim iPrevCount As Int32 = 0

            For i = 0 To dtDeviceDetailed.Rows.Count - 1
                drData = dtDeviceDetailed.Rows(i)
                If CDec(drData!ddata_LeakProbability) <> 0 Or (Not IsDBNull(drData!ddata_CordLeak)) Then


                    Dim drNewLeakRow As DataRow
                    drNewLeakRow = dtLeakDetailed.NewRow
                    drNewLeakRow!LeakDate = Format(drData!ddata_LocalDateTime, "yyyy-MM-dd HH:mm:ss")
                    If Not IsDBNull(drData!ddata_LeakProbability) AndAlso CDec(drData!ddata_LeakProbability) <> 0 Then
                        drNewLeakRow!LeakProb = m_oDB_Layer.CheckDBNullDec(drData!ddata_LeakProbability)
                    Else
                        drNewLeakRow!LeakProb = DBNull.Value
                    End If
                    If Not IsDBNull(drNewLeakRow!LeakProb) Then

                        Select Case True
              'Case CDbl(drData!ddata_LeakProbability) > 4.0
              'drNewLeakRow!LeakColor = System.Drawing.ColorTranslator.ToHtml(Color.Red)
              'iPrevCount += 1
                            Case CDbl(drData!ddata_LeakProbability) > CDbl(drData!cust_NotifyAcousticTrigger) And iPrevCount >= CInt(drData!cust_NotifyAcousticCount)
                                drNewLeakRow!LeakColor = System.Drawing.ColorTranslator.ToHtml(Color.Red)
                                iPrevCount += 1
                            Case CDbl(drData!ddata_LeakProbability) > 1.99 And iPrevCount < CInt(drData!cust_NotifyAcousticCount)
                                drNewLeakRow!LeakColor = System.Drawing.ColorTranslator.ToHtml(Color.Orange)
                                iPrevCount += 1
                            Case Else
                                iPrevCount = 0
                                drNewLeakRow!LeakColor = System.Drawing.ColorTranslator.ToHtml(Color.Green)
                        End Select
                    End If

                    If Not IsDBNull(drData!ddata_CordLeak) AndAlso CDec(drData!ddata_CordLeak) <> 0 Then

                        If CInt(drData!ddata_CordLeak) <> 0 Then
                            drNewLeakRow!LeakConductive = 10
                        Else
                            drNewLeakRow!LeakConductive = 0
                        End If
                        'Select Case CDbl(drData!ddata_CordLeak)
                        '  Case > 0
                        '    drNewLeakRow!LeakColorCond = System.Drawing.ColorTranslator.ToHtml(Color.Purple)
                        '  Case Else
                        drNewLeakRow!LeakColorCond = System.Drawing.ColorTranslator.ToHtml(Color.Blue)
                        'End Select

                    End If

                    dtLeakDetailed.Rows.Add(drNewLeakRow)

                    ''Cord Leak
                    'Dim itemNewS1 As New SeriesItem
                    'itemNewS1.YValue = m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
                    'Select Case m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
                    '  Case 1
                    '    itemNewS1.BackgroundColor = Color.Red
                    '  Case 0
                    '    itemNewS1.BackgroundColor = Color.Green

                    'End Select
                    'LineLeakage.PlotArea.Series(1).Items.Add(itemNewS1)

                    ''Threshold
                    'Dim itemNewS2 As New SeriesItem
                    'itemNewS2.YValue = CDec(1.9)
                    'LineLeakage.PlotArea.Series(2).Items.Add(itemNewS2)
                End If
            Next

            LineLeakageDetailed.DataSource = dtLeakDetailed
            LineLeakageDetailed.DataBind()


            '4. Vibration Graph
            '===================
            LineVibration.DataSource = dtVibrationCount
            LineVibration.DataBind()


            '5. Pipe Condition
            '====================

            'Let's create some pipe condition data....
            Dim dtPipeCondition As New DataTable
            Dim dNudge As Decimal = CDec((Rnd() * 0.05)) + 0
            Dim dlast As Decimal = 1 + CDec((Rnd() * 2)) + 0
            If CInt(txtDevID.Text) = 3 Then
                dlast = CDec(0.75)
                dNudge = CDec(0.8)
            End If

            dtPipeCondition.Columns.Add("dates", GetType(DateTime))
            dtPipeCondition.Columns.Add("Cond", GetType(Decimal))

            For i = CInt(ddDateRange.SelectedItem.Value) To 0 Step -1
                If i Mod 7 = 0 Then
                    drNew = dtPipeCondition.NewRow
                    drNew!dates = Now.AddDays(i * -1)
                    drNew!Cond = dlast
                    drNew!Cond = 0 'Force a zero entry
                    dlast = dlast + dNudge

                    If CInt(txtDevID.Text) = 3 Then
                        dNudge = CDec(0.8) + CDec((Rnd() * 0.3))
                    End If

                    dtPipeCondition.Rows.Add(drNew)
                End If
            Next

            linePipeCondition.DataSource = dtPipeCondition
            linePipeCondition.DataBind()


            '6. Pipe Pressue
            '=================
            'Dim dt4 As New DataTable
            'dt4.Columns.Add("dates", GetType(DateTime))
            'dt4.Columns.Add("strn", GetType(Int32))
            'dt4.Columns.Add("_7dayMovingAvg", GetType(Decimal))
            'dt4.Columns.Add("_7dayMovingAvgPlus50", GetType(Decimal))
            'dt4.Columns.Add("_7dayMovingAvgMinus50", GetType(Decimal))
            'dt4.Columns.Add("_7dayMovingAvgPlus95", GetType(Decimal))
            'dt4.Columns.Add("_7dayMovingAvgMinus95", GetType(Decimal))
            'dt4.Columns.Add("ColourMid", GetType(String))
            'dt4.Columns.Add("ColourHigh", GetType(String))

            'For i = 0 To dtStrainSample.Rows.Count - 1
            '  drTemp = dtStrainSample.Rows(i)

            '  drNew = dt4.NewRow
            '  drNew!dates = CDate(Format(drTemp!dates, "dd MMM yyyy HH:mm"))
            '  drNew!strn = CInt(drTemp!strn)
            '  drNew!_7DayMovingAvg = CDec(drTemp!_7DayMovingAvg)
            '  drNew!_7dayMovingAvgPlus50 = CDec(drTemp!_7dayMovingAvgPlus50)
            '  drNew!_7dayMovingAvgMinus50 = CDec(drTemp!_7dayMovingAvgMinus50)
            '  drNew!_7dayMovingAvgPlus95 = CDec(drTemp!_7dayMovingAvgPlus95)
            '  drNew!_7dayMovingAvgMinus95 = CDec(drTemp!_7dayMovingAvgMinus95)
            '  drNew!ColourMid = System.Drawing.ColorTranslator.ToHtml(Color.Yellow)
            '  drNew!ColourHigh = System.Drawing.ColorTranslator.ToHtml(Color.Red)

            '  'drNew!MarkerColour = "#" & Mid(Lerp(Color.Red, Color.Green, dPercent).Name, 3)
            '  dt4.Rows.Add(drNew)

            'Next

            For i = 0 To dtStrainDailyAvg7dma.Rows.Count - 1
                Diagnostics.Debug.WriteLine("------------------------")
                Diagnostics.Debug.WriteLine("DateGroup: " & m_oDB_Layer.CheckDBNullStr(dtStrainDailyAvg7dma.Rows(i)!DateGroup))
                Diagnostics.Debug.WriteLine("avgStrain: " & m_oDB_Layer.CheckDBNullStr(dtStrainDailyAvg7dma.Rows(i)!avgStrain))
                Diagnostics.Debug.WriteLine("_7dayMovingAvg: " & m_oDB_Layer.CheckDBNullStr(dtStrainDailyAvg7dma.Rows(i)!_7dayMovingAvg))
                Diagnostics.Debug.WriteLine("_7dayMovingAvgPlus50: " & m_oDB_Layer.CheckDBNullStr(dtStrainDailyAvg7dma.Rows(i)!_7dayMovingAvgPlus50))
                Diagnostics.Debug.WriteLine("_7dayMovingAvgMinus50: " & m_oDB_Layer.CheckDBNullStr(dtStrainDailyAvg7dma.Rows(i)!_7dayMovingAvgMinus50))
            Next
            Diagnostics.Debug.WriteLine("------------------------")

            linePressue.DataSource = dtStrainDailyAvg7dma
            linePressue.DataBind()


            '7. Device Check ins
            '======================
            lineDeviceCheckins.DataSource = dtLineDailyAvgs01
            lineDeviceCheckins.DataBind()


            '8. Load the Battery Graph
            '==========================
            Dim dtBatteryData As New DataTable
            dtBatteryData.Columns.Add("DateGroup")
            dtBatteryData.Columns.Add("avgBattery")
            dtBatteryData.Columns.Add("avgBattery7DMA")
            dtBatteryData.Columns.Add("avgBattery7DMAwarnUpper")
            dtBatteryData.Columns.Add("avgBattery7DMAwarnLower")
            dtBatteryData.Columns.Add("additional1")
            dtBatteryData.Columns.Add("additional2")
            dtBatteryData.Columns.Add("additional3")

            iTempMin = 999
            iTempMax = 0
            Dim dWarnAdjust As Decimal

            For i = 0 To dtLineDailyAvgs01.Rows.Count - 1
                drData = dtLineDailyAvgs01.Rows(i)

                dWarnAdjust = m_oDB_Layer.CheckDBNullDec(m_oDB_Layer.CheckDBNullDec(drData!avgBattery7DMA) * 0.1)
                drNew = dtBatteryData.NewRow
                drNew!DateGroup = Format(drData!dategroup, "dd MMM yyyy")
                drNew!avgBattery = drData!avgBattery
                drNew!avgBattery7DMA = drData!avgBattery7DMA
                drNew!avgBattery7DMAwarnUpper = m_oDB_Layer.CheckDBNullDec(drData!avgBattery7DMA) + dWarnAdjust
                drNew!avgBattery7DMAwarnLower = m_oDB_Layer.CheckDBNullDec(drData!avgBattery7DMA) - dWarnAdjust
                drNew!additional1 = 0
                drNew!additional2 = 0
                drNew!additional3 = 0

                dtBatteryData.Rows.Add(drNew)

                If iTempMin > m_oDB_Layer.CheckDBNullInt(drNew!avgBattery7DMAwarnLower) Then
                    iTempMin = m_oDB_Layer.CheckDBNullInt(drNew!avgBattery7DMAwarnLower)
                End If
                If iTempMax < m_oDB_Layer.CheckDBNullInt(drNew!avgBattery7DMAwarnUpper) Then
                    iTempMax = m_oDB_Layer.CheckDBNullInt(drNew!avgBattery7DMAwarnUpper)
                End If
            Next

            If iTempMin > 0 Then
                iTempMin = CInt(iTempMin * 0.7)
            End If
            If iTempMax > 0 Then
                iTempMax = CInt(iTempMax * 1.3)
            End If

            lineDailyAvgBattery.PlotArea.YAxis.MinValue = iTempMin
            lineDailyAvgBattery.PlotArea.YAxis.MaxValue = iTempMax

            lineDailyAvgBattery.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
            lineDailyAvgBattery.DataSource = dtBatteryData
            lineDailyAvgBattery.DataBind()



            '9. Board Temperatures
            '=========================
            Dim dtBoardTemps As New DataTable
            dtBoardTemps.Columns.Add("DateGroup", GetType(DateTime))
            dtBoardTemps.Columns.Add("avgPipeTemp", GetType(Decimal))
            dtBoardTemps.Columns.Add("avgBoardTemp", GetType(Decimal))


            Diagnostics.Debug.WriteLine("--------------------------------------------------------------------------------------------------------------")

            For i = 0 To dtLineDailyAvgs01.Rows.Count - 1
                drTemp = dtLineDailyAvgs01.Rows(i)

                drNew = dtBoardTemps.NewRow
                drNew!DateGroup = CDate(Format(drTemp!DateGroup, "dd MMM yyyy HH:mm"))
                Diagnostics.Debug.WriteLine(CStr(drNew!DateGroup) & ": " & m_oDB_Layer.CheckDBNullStr(drTemp!avgPipeTemp) & " - " & m_oDB_Layer.CheckDBNullStr(drTemp!avgBoardTemp))

                If Not IsDBNull(drTemp!avgPipeTemp) Then
                    drNew!avgPipeTemp = CDec(drTemp!avgPipeTemp)
                Else
                    drNew!avgPipeTemp = DBNull.Value
                End If

                If Not IsDBNull(drTemp!avgBoardTemp) Then
                    drNew!avgBoardTemp = CDec(drTemp!avgBoardTemp)
                Else
                    drNew!avgBoardTemp = DBNull.Value
                End If

                dtBoardTemps.Rows.Add(drNew)
            Next

            lineBoardTempMA.DataSource = dtBoardTemps
            lineBoardTempMA.DataBind()


            'Format the graphs.....
            '==========================

            'Force the dates on the graphs to be uniform.
            Dim datMinDate As Date
            Dim datMaxDate As Date

            datMaxDate = DateAdd(DateInterval.Day, 2, Now())
            datMinDate = DateAdd(DateInterval.Day, (CInt(ddDateRange.SelectedItem.Value) + 2) * -1, Now())


            LineLeakageDetailed.PlotArea.XAxis.MinDateValue = datMinDate
            LineLeakageDetailed.PlotArea.XAxis.MaxDateValue = datMaxDate

            lineDailyFlowEvents.PlotArea.XAxis.MinDateValue = datMinDate
            lineDailyFlowEvents.PlotArea.XAxis.MaxDateValue = datMaxDate
            lineuSonicFlow.PlotArea.XAxis.MinDateValue = datMinDate
            lineuSonicFlow.PlotArea.XAxis.MaxDateValue = datMaxDate
            LineVibration.PlotArea.XAxis.MinDateValue = datMinDate
            LineVibration.PlotArea.XAxis.MaxDateValue = datMaxDate
            linePipeCondition.PlotArea.XAxis.MinDateValue = datMinDate
            linePipeCondition.PlotArea.XAxis.MaxDateValue = datMaxDate
            linePressue.PlotArea.XAxis.MinDateValue = datMinDate
            linePressue.PlotArea.XAxis.MaxDateValue = datMaxDate
            lineDeviceCheckins.PlotArea.XAxis.MinDateValue = datMinDate
            lineDeviceCheckins.PlotArea.XAxis.MaxDateValue = datMaxDate
            lineDailyAvgBattery.PlotArea.XAxis.MinDateValue = datMinDate
            lineDailyAvgBattery.PlotArea.XAxis.MaxDateValue = datMaxDate
            lineBoardTempMA.PlotArea.XAxis.MinDateValue = datMinDate
            lineBoardTempMA.PlotArea.XAxis.MaxDateValue = datMaxDate
            lineDeviceCheckins.PlotArea.XAxis.MinDateValue = datMinDate
            lineDeviceCheckins.PlotArea.XAxis.MaxDateValue = datMaxDate


            'Setup Hyperlinks
            If CBool(System.Configuration.ConfigurationManager.AppSettings("DisableRawFileProcessing")) = False Then
                hypLeakDetails.NavigateUrl = "DeviceDataProcessed.aspx?devid=" & m_pcl_Utils.EncryptStringOld(CStr(iDev_id)) & "&graphtype=LD"
                hypPipeCondDetails.NavigateUrl = "DeviceDataProcessed.aspx?devid=" & m_pcl_Utils.EncryptStringOld(CStr(iDev_id)) & "&graphtype=PC"
            End If


        Catch ex As Exception
            m_pcl_ErrManager.AddError(ex)
            Throw ex
        End Try

    End Sub

    Private Sub zLoadDeviceDetails()
    Dim drDevice As DataRow

    drDevice = m_oBL_DevManager.GetDeviceByID(CInt(txtDevID.Text))

    lblDeviceDetails.Text = m_oDB_Layer.CheckDBNullStr(drDevice!org_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!cust_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!loc_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!subloc_name)
    lblDeviceDetails.Text += ", Location Ref: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_LocationRef)
    lblDeviceDetails.Text += ", Asset Tag: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_AssetTag)
    'lblDeviceDetails.Text += ", IMEI: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_IMEI)
  End Sub

  Private Sub zLoadDropdowns()
    ddDateRange.Items.Clear()
    ddDateRange.Items.Add(New ListItem("28 Days", "28"))
    ddDateRange.Items.Add(New ListItem("1 week", "7"))
    ddDateRange.Items.Add(New ListItem("1 Day", "1"))
    ddDateRange.Items.Add(New ListItem("4 hours", "-4"))

    ddDateRange.ClearSelection()
    m_pcl_Utils.DropDownSelectByValue(ddDateRange, "28")

  End Sub

  Protected Sub ddDateRange_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddDateRange.SelectedIndexChanged
        'Force 28 days for now!  "28"
        m_pcl_Utils.DropDownSelectByValue(ddDateRange, ddDateRange.SelectedValue)
        zLoadGraphs(CInt(ddDateRange.SelectedValue))
    End Sub

  Private Function zLerp(ByVal color1 As Color, ByVal color2 As Color, ByVal amount As Single) As Color
    'The function accept a Single value in the range 0.0 (0%) to 1.0 (100%).
    Const bitmask As Single = 65536.0!
    Dim n As UInteger = CUInt(Math.Round(CDbl(Math.Max(Math.Min((amount * bitmask), bitmask), 0.0!))))
    Dim r As Integer = (CInt(color1.R) + (((CInt(color2.R) - CInt(color1.R)) * CInt(n)) >> 16))
    Dim g As Integer = (CInt(color1.G) + (((CInt(color2.G) - CInt(color1.G)) * CInt(n)) >> 16))
    Dim b As Integer = (CInt(color1.B) + (((CInt(color2.B) - CInt(color1.B)) * CInt(n)) >> 16))
    Dim a As Integer = (CInt(color1.A) + (((CInt(color2.A) - CInt(color1.A)) * CInt(n)) >> 16))
    Return Color.FromArgb(a, r, g, b)
  End Function

#End Region
#Region "Control Routines"





#End Region



  'Battery detailed testing...  

  'Dim dt2 As New DataTable
  '    dt2.Columns.Add("LocalDateTime", GetType(DateTime))
  '    dt2.Columns.Add("BatteryValue", GetType(Int32))
  '    dt2.Columns.Add("MarkerColour", GetType(String))

  '    Dim iMax As Int32 = 550
  'Dim iMin As Int32 = 520
  'Dim dPercent As Decimal
  'Dim dTemp As Decimal
  'Dim dTempMax As Decimal


  'Diagnostics.Debug.WriteLine("Battery detailed dump.....")
  'For i = 0 To dtBatteryDetailed.Rows.Count - 1
  '  drTemp = dtBatteryDetailed.Rows(i)

  '  drNew = dt2.NewRow
  '  drNew!LocalDateTime = CDate(Format(drTemp!ddata_LocalDateTime, "dd MMM yyyy HH:mm"))
  '  drNew!BatteryValue = CInt(drTemp!ddata_AdmBatteryVoltage)

  '  If CInt(drTemp!ddata_AdmBatteryVoltage) > iMax Then
  '    dPercent = 1
  '  ElseIf CInt(drTemp!ddata_AdmBatteryVoltage) < iMin Then
  '    dPercent = 0
  '  Else
  '    dTemp = CInt(drTemp!ddata_AdmBatteryVoltage) - iMin
  '    dTempMax = iMax - iMin
  '    dPercent = dTemp / dTempMax
  '  End If

  '  drNew!MarkerColour = "#" & Mid(Lerp(Color.Red, Color.Green, dPercent).Name, 3)

  '  Diagnostics.Debug.WriteLine("Date: " & Format(drTemp!ddata_LocalDateTime, "yyyy-MM-dd HH:mm:ss") & ", Value: " & CInt(drTemp!ddata_AdmBatteryVoltage) & ", dPercent: " & dPercent & ", Colour: " & CStr(drNew!MarkerColour))

  '  dt2.Rows.Add(drNew)

  'Next

  'lineBatteryDetail.DataSource = dt2
  'lineBatteryDetail.DataBind()

  'LineVibration.DataBind()


  'PRESSURE
  '---------------------------

  'For i = 0 To dtLineData04.Rows.Count - 1
  '  drData = dtLineData04.Rows(i)


  '  colPressure2.PlotArea.XAxis.Items.Add(m_oDB_Layer.CheckDBNullStr(drData!DateSeq))
  '  colPressure2.PlotArea.Series(0).Items.Add(m_oDB_Layer.CheckDBNullInt(drData!PlaciboCount))

  'Next

  'colPressure2.DataSource = dtLineData04
  'colPressure2.DataBind()

  'linePipeCondition.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
  'linePipeCondition.DataSource = dtLineData04
  'linePipeCondition.DataBind()

  'lineLegionella.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
  'lineLegionella.DataSource = dtLineData05
  'lineLegionella.DataBind()


  '-------------------------------------------------
  'Board Temp Moving Average
  '-------------------------------------------------
  'dtTemperature = m_oBL_DevManager.GetDeviceDataTempMA(CInt(iDev_id), 28)

  'Dim dt5 As New DataTable
  'dt5.Columns.Add("DateGroup", GetType(DateTime))
  'dt5.Columns.Add("avgPipeTemp", GetType(Decimal))
  'dt5.Columns.Add("avgBoardTemp", GetType(Decimal))
  'dt5.Columns.Add("avgTemp7MA", GetType(Decimal))
  'dt5.Columns.Add("_7dayMovingAvgPlus50", GetType(Decimal))
  'dt5.Columns.Add("_7dayMovingAvgMinus50", GetType(Decimal))
  'dt5.Columns.Add("_7dayMovingAvgPlus95", GetType(Decimal))
  'dt5.Columns.Add("_7dayMovingAvgMinus95", GetType(Decimal))
  'dt5.Columns.Add("ColourMid", GetType(String))
  'dt5.Columns.Add("ColourHigh", GetType(String))



  'For i = 0 To dtTemperature.Rows.Count - 1
  '  drTemp = dtTemperature.Rows(i)

  '  If Not IsDBNull(drTemp!avgTemp7MA) And Not IsDBNull(drTemp!_7dayMovingAvgPlus50) Then
  '    drNew = dt5.NewRow
  '    drNew!DateGroup = CDate(Format(drTemp!DateGroup, "dd MMM yyyy HH:mm"))
  '    If Not IsDBNull(drTemp!avgPipeTemp) Then
  '      drNew!avgPipeTemp = CDec(drTemp!avgPipeTemp)
  '    Else
  '      drNew!avgPipeTemp = DBNull.Value
  '    End If

  '    If Not IsDBNull(drTemp!avgPipeTemp) Then
  '      drNew!avgBoardTemp = CDec(drTemp!avgBoardTemp)
  '    Else
  '      drNew!avgBoardTemp = DBNull.Value
  '    End If

  '    drNew!avgTemp7MA = CDec(drTemp!avgTemp7MA)
  '    drNew!_7dayMovingAvgPlus50 = CDec(drTemp!_7dayMovingAvgPlus50)
  '    drNew!_7dayMovingAvgMinus50 = CDec(drTemp!_7dayMovingAvgMinus50)
  '    drNew!_7dayMovingAvgPlus95 = CDec(drTemp!_7dayMovingAvgPlus95)
  '    drNew!_7dayMovingAvgMinus95 = CDec(drTemp!_7dayMovingAvgMinus95)
  '    drNew!ColourMid = System.Drawing.ColorTranslator.ToHtml(Color.Yellow)
  '    drNew!ColourHigh = System.Drawing.ColorTranslator.ToHtml(Color.Red)

  '    'drNew!MarkerColour = "#" & Mid(Lerp(Color.Red, Color.Green, dPercent).Name, 3)

  '    dt5.Rows.Add(drNew)
  '  End If
  'Next





  'lineDailyAvgBoardTemp.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
  'lineDailyAvgBoardTemp.DataSource = dtLineData01
  'lineDailyAvgBoardTemp.DataBind()








  'LineCordLeak.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
  'LineCordLeak.DataSource = dtLineData03
  'LineCordLeak.DataBind()

  'Diagnostics.Debug.WriteLine("Leak Data Cols---------------------------------------------")

  'For i = 0 To dtLineLeakDaily.Rows.Count - 1
  '  drData = dtLineLeakDaily.Rows(i)
  '  'Diagnostics.Debug.WriteLine("row: " & i & ", X:" & m_oDB_Layer.CheckDBNullStr(drData!DateSeq) & ", Y1:" & m_oDB_Layer.CheckDBNullInt(drData!CordLeakCount))

  '  colLeakage.PlotArea.XAxis.Items.Add(m_oDB_Layer.CheckDBNullStr(drData!DateSeq))

  '  'Leak Probability
  '  Dim itemNewS0 As New SeriesItem
  '  itemNewS0.YValue = m_oDB_Layer.CheckDBNullDec(drData!LeakProbability)

  '  Select Case itemNewS0.YValue
  '    Case > 1.9
  '      itemNewS0.BackgroundColor = Color.Red
  '    Case > 1.4
  '      itemNewS0.BackgroundColor = Color.Orange
  '    Case Else
  '      itemNewS0.BackgroundColor = Color.Green
  '  End Select
  '  colLeakage.PlotArea.Series(0).Items.Add(itemNewS0)

  '  'Cord Leak
  '  Dim itemNewS1 As New SeriesItem
  '  itemNewS1.YValue = m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
  '  Select Case m_oDB_Layer.CheckDBNullInt(drData!CordLeakTafficLight)
  '    Case 1
  '      itemNewS1.BackgroundColor = Color.Red
  '    Case 0
  '      itemNewS1.BackgroundColor = Color.Green

  '  End Select
  '  colLeakage.PlotArea.Series(1).Items.Add(itemNewS1)

  'Next

  ''lineLeakEnhanced.DataSource = dtLeakEnhanced
  ''lineLeakEnhanced.DataBind()


  'Diagnostics.Debug.WriteLine("Leak Data Line---------------------------------------------")


  '    Diagnostics.Debug.WriteLine("----------------------------------------------------")

  'colLeakage.PlotArea.XAxis.MaxDateValue = Today().AddDays(2)
  ''colLeakage.DataSource = dtLineData03
  'colLeakage.DataBind()

  Private Function zConvertToJavaScriptDateTime(fromDate As DateTime) As Decimal
    Return CDec(fromDate.Subtract(New DateTime(1970, 1, 1)).TotalMilliseconds)
  End Function

End Class