Option Strict On
Public Class ScheduledDaily
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DeviceManager As BL_DeviceManager
  Private m_dtFrom As Date
  Private m_dtTo As Date



  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load


    zProcessQueryStrings()

    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then

      zGenDeviceDataCsv()

      m_pcl_Utils.CloseBroswerWindow()

    End If

  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '
    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    End If

    If Not m_oBL_DeviceManager Is Nothing Then
      m_oBL_DeviceManager.Dispose()
      m_oBL_DeviceManager = Nothing
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


    m_oBL_DeviceManager = New BL_DeviceManager(m_pdl_Manager, m_pcl_ErrManager, m_oDB_Layer)


    Return True

  End Function
  Private Sub zProcessQueryStrings()
    Dim sDateFrom As String
    Dim sDateTo As String

    If Not Request.QueryString("datefrom") Is Nothing Then
      sDateFrom = CStr(Request.QueryString("datefrom"))
      If IsDate(sDateFrom) Then
        m_dtFrom = CDate(Format(CDate(sDateFrom), "yyyy-MM-dd 00:00:00"))
      Else
        m_dtFrom = Now().AddDays(-1)
        m_dtFrom = CDate(Format(m_dtFrom, "yyyy-MM-dd 00:00:00"))
      End If
    Else
      m_dtFrom = Now().AddDays(-1)
      m_dtFrom = CDate(Format(m_dtFrom, "yyyy-MM-dd 00:00:00"))
    End If

    If Not Request.QueryString("dateto") Is Nothing Then
      sDateTo = CStr(Request.QueryString("dateto"))
      If IsDate(sDateTo) Then
        m_dtTo = CDate(Format(CDate(sDateTo), "yyyy-MM-dd 23:59:59"))
      Else
        m_dtTo = Now().AddDays(-1)
        m_dtTo = CDate(Format(m_dtTo, "yyyy-MM-dd 23:59:59"))
      End If
    Else
      m_dtTo = Now().AddDays(-1)
      m_dtTo = CDate(Format(m_dtTo, "yyyy-MM-dd 23:59:59"))
    End If





  End Sub

  Private Sub zGenDeviceDataCsv()
    Try
      Dim dtDeviceData As DataTable
      Dim drDeviceData As DataRow
      Dim sQ As String = Chr(34)
      Dim sLine As String


      Dim sFileName As String = m_pbl_Client.TempFolderFullPath & "StreamLine_DeviceData_" & Format(Now(), "yyyy_MM_dd HH_mm_ss") & ".csv"
      Dim fiCSV As New System.IO.FileInfo(sFileName)
      If fiCSV.Exists Then fiCSV.Delete()
      Dim sw As New System.IO.StreamWriter(sFileName, True)

      'Header line... 
      sLine = "IMEI,"
      sLine += "Asset Tag,"
      sLine += "Customer,"
      sLine += "Location,"
      sLine += "Sub-Location,"
      sLine += "Location Ref,"
      sLine += "Pipe Diameter,"
      sLine += "Pipe Orientation,"
      sLine += "Message Count,"
      sLine += "Date Time UTC ,"
      sLine += "Date Time Local,"
      sLine += "Local Timezone,"
      sLine += "RSSI,"
      sLine += "Battery Level,"
      sLine += "Flow Event,"
      sLine += "Flow Measurement (fps),"
      sLine += "Flow Event Confidence,"
      sLine += "Flow Trigger Type,"
      sLine += "Unit Temperature (C),"
      sLine += "Pipe Temperature,"
      sLine += "Leak Cord Detect,"
      sLine += "Audio Leak Detect,"
      sLine += "Audio Leak Confidence,"
      sLine += "Audio Ping Condition,"
      sLine += "Audio Ping Confidence,"
      sLine += "PIC18 Version,"
      sLine += "PIC32 Version,"
      sLine += "FW Upload Attempts,"
      sLine += "FW Upload Successes,"
      sLine += "Buffer ID,"
      sLine += "Trigger Number,"
      sLine += "Ambient Temp,"
      sLine += "Raw Filename,"



      sw.WriteLine(sLine)




      dtDeviceData = m_oBL_DeviceManager.GetDeviceDataForExport(m_dtFrom, m_dtTo)

      For iSale = 0 To dtDeviceData.Rows.Count - 1
        drDeviceData = dtDeviceData.Rows(iSale)
        sLine = sQ & "IMEI: " & m_oDB_Layer.CheckDBNullStr(drDeviceData!dev_IMEI) & sQ & ","                        'IMEI
        sLine += sQ & m_oDB_Layer.CheckDBNullStr(drDeviceData!dev_AssetTag) & sQ & ","                              'Asset Tag

        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!cust_Name)) & sQ & ","                'Customer
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!loc_name)) & sQ & ","                 'Location
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!subloc_name)) & sQ & ","              'Sub-Location
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!dev_LocationRef)) & sQ & ","          'Location Ref
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!piped_Diameter)) & sQ & ","           'Pipe Diameter
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!pipeo_Orientation)) & sQ & ","        'Pipe Orientation

        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmMsgCount)) & ","                  'Message Count
        sLine += Format(m_oDB_Layer.CheckDBNullDate(drDeviceData!ddata_DateTimeUtc), "yyyy-MM-dd HH:mm:ss") & ","   'UTC Time
        sLine += Format(m_oDB_Layer.CheckDBNullDate(drDeviceData!ddata_LocalDateTime), "yyyy-MM-dd HH:mm:ss") & "," 'Local Time
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!timezone_Abbreviation)) & sQ & ","    'TZ Abbr
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmRssi)) & ","                      'RSSI
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmBatteryVoltage)) & ","            'TZ Abbr
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_FlowEvent)) & ","                    'Flow Event

        If Not IsDBNull(drDeviceData!ddata_FlowEvent) Then
          sLine += ""
        End If

        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_FlowMeasurement)) & ","              'Flow Measure
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_FlowConfidence)) & ","               'Flow Confidence
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_FlowEventTrigger)) & ","             'Flow Trigger
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_BoardTemperature)) & ","             'Unit Temperature
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AmbientTemperature)) & ","           'Pipe Temperature
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_CordLeak)) & ","                     'Leak Cord
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AudioLeakDetect)) & ","              'Audio Leak
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AudioLeakConfidence)) & ","          'Audio Leak Conf
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_PingPipeCondition)) & ","            'Ping Condition
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_PingPipeConfidence)) & ","           'Ping Confidence
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmPic18Version)) & sQ & ","    'Pic18
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmPic32Version)) & sQ & ","    'Pic32
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmOtaAttempts)) & sQ & ","     'OTA Attempts
        sLine += sQ & zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AdmOtaSuccess)) & sQ & ","      'OTA Success
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!buf_id)) & ","                             'Buffer ID
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataTriggerNumber)) & ","         'Trigger Number
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AmbientTempC)) & ","                 'Ambient Temp
        sLine += zCleanStringCSV(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName)) & ","              'Raw Filename


        sw.WriteLine(sLine)
      Next


      lblText.Text = "Rows Processed:" & dtDeviceData.Rows.Count

      sw.Close()

      'Send an email....
      Dim sBody As String
      Dim sFrom As String
      Dim sTo As String
      Dim sCC As String
      Dim sSubject As String

      sBody = "Orbis Streamline CSV Export for: " & Format(m_dtFrom, "yyyy-MM-dd") & " to " & Format(m_dtTo, "yyyy-MM-dd") & vbCrLf & vbCrLf

      sFrom = "streamline@relay.horizon-it.co.uk"
      If InStr(Request.Url.ToString, "localhost") = 0 Then
        sTo = "peto@orbis-sys.com;danny@orbis-sys.com;prodigy1@orbis-sys.com"
        sCC = "paul.craven@horizon-it.co.uk"
      Else
        sTo = "paul.craven@horizon-it.co.uk"
        sCC = ""
      End If

      sSubject = "Orbis Streamline CSV Export for: " & Format(m_dtFrom, "yyyy-MM-dd") & " to " & Format(m_dtTo, "yyyy-MM-dd")

      m_pdl_Manager.SendEmail(sFrom, sTo, sCC, "", sSubject, sBody,,, sFileName)



    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("ExceptionPage.aspx")
    End Try




  End Sub
  Private Function zCleanStringCSV(ByVal sIn As String) As String
    sIn = Replace(sIn, vbCrLf, " ")
    sIn = Replace(sIn, Chr(34), Chr(34) & Chr(34))
    Return sIn
  End Function

#End Region


End Class