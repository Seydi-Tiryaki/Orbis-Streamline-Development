Option Strict On
Imports System.IO

Public Class ProcessBufferToData
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DeviceManager As BL_DeviceManager
  Private m_iMaxAutoRun As Int32 = -1
  Private m_iRunCount As Int32 = 0


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Try

      zProcessQueryStrings()

      If Not zSetupObjects() Then
        Exit Sub
      End If

      If Not Page.IsPostBack Then

        Diagnostics.Debug.WriteLine("Ckeck Close, Max Run:" & m_iMaxAutoRun & ", Run #: " & m_iRunCount)
        lblText.Text += "<br/>Max Run:" & m_iMaxAutoRun & ", Run #: " & m_iRunCount

        If m_iRunCount >= m_iMaxAutoRun Then
          lblText.Text += "<br/>Send Close Window Command....."
          m_pcl_Utils.CloseBroswerWindow()
        Else
          zProcessBufferToData()
          zProcessRawDataFiles()
          zProcessPostProcessing()
        End If

      End If

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("ExceptionPage.aspx")

    End Try
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

    If Session("AutoRunCount") IsNot Nothing Then
      m_iRunCount = CInt(Session("AutoRunCount")) + 1
      Session("AutoRunCount") = m_iRunCount
      Diagnostics.Debug.WriteLine("Session Run Count:" & m_iRunCount)
    Else
      m_iRunCount = 0
      Session("AutoRunCount") = 0
      Diagnostics.Debug.WriteLine("Init Run Count:" & m_iRunCount)
    End If



    Return True

  End Function
  Private Sub zProcessQueryStrings()


    If Not Request.QueryString("MaxAutoRun") Is Nothing Then
      m_iMaxAutoRun = CInt(Request.QueryString("MaxAutoRun"))
    Else
      If InStr(Request.Url.ToString, "localhost") = 0 Then
        'Live run, so don't run....
        m_iMaxAutoRun = -1
      Else
        'Test run, just one.
        m_iMaxAutoRun = 1
      End If
    End If


  End Sub
  Private Sub zProcessBufferToData()
    Dim dtBuffer As DataTable
    Dim drBuffer As DataRow
    Dim drDevice As DataRow = Nothing
    Dim iBuffer As Int32
    Dim bOK As Boolean
    Dim dicBufferVars As New eDictionary
    Dim dicParms As New eDictionary
    Dim iID As Int32
    Dim iDataID As Int32
    Dim iProcID As Int32
    Dim iFailID As Int32
    Dim dbRet As DL_Manager.DB_Return
    Dim sRetMessage As String = ""
    Dim sRowMessage As String = ""
    Dim sIMEI As String = ""
    Dim iRowsAffected As Int32
    Dim sBody As String
    Dim sSubject As String
    Dim sTempMsg As String = ""
    Dim iDevHardType As Int32
    Dim drLastDataRow As DataRow


    dtBuffer = m_oBL_DeviceManager.GetAllBufferRows()

    For iBuffer = 0 To dtBuffer.Rows.Count - 1
      drBuffer = dtBuffer.Rows(iBuffer)

      sRowMessage = ""
      sTempMsg = ""
      sIMEI = ""

      Diagnostics.Debug.Write(Format(CInt(drBuffer!buf_id), "00000") & " - ")

      dicBufferVars = zParseBufferToDic(drBuffer)

      bOK = False

      'dicProdigyParams.Add("DN", "PRM_REQ_COMPLETE")
      'dicProdigyParams.Add("FE", "PRM_FLOW_EVENT")
      'dicProdigyParams.Add("FT", "PRM_FLOW_EVENT_TRIGGER")
      'dicProdigyParams.Add("FM", "PRM_FLOW_MEAS")
      'dicProdigyParams.Add("FC", "PRM_FLOW_CONF")
      'dicProdigyParams.Add("AD", "PRM_AUD_LEAK")
      'dicProdigyParams.Add("AC", "PRM_AUD_LEAK_CONF")
      'dicProdigyParams.Add("AP", "PRM_AUD_PIPECOND")
      'dicProdigyParams.Add("PC", "PRM_AUD_PIPECOND_CONF")
      'dicProdigyParams.Add("BT", "PRM_BOARD_TEMP")
      'dicProdigyParams.Add("AT", "PRM_AMB_TEMP")
      'dicProdigyParams.Add("VR", "PRM_FW_VERSION")
      'dicProdigyParams.Add("TR", "PRM_TRIGGER_TYPE")
      'dicProdigyParams.Add("CL", "PRM_CORD_LEAK")
      'dicProdigyParams.Add("A0", "ADM_TYPE")
      'dicProdigyParams.Add("A1", "ADM_IMEI")
      'dicProdigyParams.Add("A2", "ADM_DATE")
      'dicProdigyParams.Add("A3", "ADM_TIME")
      'dicProdigyParams.Add("A4", "ADM_RSSI")
      'dicProdigyParams.Add("A5", "ADM_BATTERY")
      'dicProdigyParams.Add("A6", "ADM_TEMPERATURE")
      'dicProdigyParams.Add("A7", "ADM_PIC18_FWVERS")
      'dicProdigyParams.Add("A8", "ADM_PIC32_FWVERS")
      'dicProdigyParams.Add("A9", "ADM_MSG_COUNT")
      'dicProdigyParams.Add("B0", "ADM_OTA_ATTEMPTS")
      'dicProdigyParams.Add("B1", "ADM_OTA_SUCCESS")


      If dicBufferVars.KeyExists("A1") Then
        drDevice = m_oBL_DeviceManager.GetDeviceByImei(CStr(dicBufferVars("A1")))
      Else
        drDevice = Nothing
      End If


      'Check to see if the device exists....
      If zValidateData(dicBufferVars, sTempMsg, sIMEI, m_oDB_Layer.CheckDBNullStr(drBuffer!buf_QueryString), iDevHardType, drDevice) Then
        'We have an IMEI Number....


        If drDevice Is Nothing Then
          dicParms.Clear()
          dicParms.Add("org_ID", "N:" & "1") 'Default to Orbis
          dicParms.Add("cust_ID", "N:" & "2") 'Orbis Default
          dicParms.Add("loc_ID", "N:" & "2") 'Orbis Systems
          dicParms.Add("subloc_ID", "N:" & "4") 'Orbis New Device
          dicParms.Add("devtype_id", "N:" & "7") 'Type Not Set
          dicParms.Add("devh_ID", "N:" & iDevHardType) 'Device Hardware Type
          dicParms.Add("dev_IMEI", "S:" & CStr(dicBufferVars("A1")))


          'PIC18
          If dicBufferVars.KeyExists("A7") Then
            dicParms.Add("dev_PIC18Version", "S:" & CStr(dicBufferVars("A7")))
          Else
            dicParms.Add("dev_PIC18Version", "S:" & "N/A")
          End If

          'PIC32
          If dicBufferVars.KeyExists("A8") Then
            dicParms.Add("dev_PIC32Version", "S:" & CStr(dicBufferVars("A8")))
          Else
            dicParms.Add("dev_PIC32Version", "S:" & "N/A")
          End If

          'OTA Attempts
          If dicBufferVars.KeyExists("B0") Then
            dicParms.Add("dev_LastOtaAttempts", "S:" & CStr(dicBufferVars("B0")))
          Else
            dicParms.Add("dev_LastOtaAttempts", "S:" & "0")
          End If

          'OTA Success
          If dicBufferVars.KeyExists("B1") Then
            dicParms.Add("dev_LastOtaSuccess", "S:" & CStr(dicBufferVars("B1")))
          Else
            dicParms.Add("dev_LastOtaSuccess", "S:" & "0")
          End If


          dicParms.Add("dev_History", "S:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss") & "(UTC) - Device data First processed." & vbCrLf)

          dicParms.Add("dev_ExternalGuid", "S:" & Guid.NewGuid().ToString())


          iID = m_oBL_DeviceManager.InsertDevice(dicParms)

          drDevice = m_oBL_DeviceManager.GetDeviceByID(iID)
          Diagnostics.Debug.Write(", Insert")

        Else


          dicParms.Clear()
          'update the device with the latest information.

          'Device Hardware Type
          If iDevHardType <> m_oDB_Layer.CheckDBNullInt(drDevice!devh_ID) Then
            dicParms.Add("devh_ID", "S:" & iDevHardType)
          End If

          'PIC18
          If dicBufferVars.KeyExists("A7") Then
            If CStr(dicBufferVars("A7")) <> m_oDB_Layer.CheckDBNullStr(drDevice!dev_PIC18Version) Then
              dicParms.Add("dev_PIC18Version", "S:" & CStr(dicBufferVars("A7")))
            End If
          End If

          'PIC18
          If dicBufferVars.KeyExists("A8") Then
            If CStr(dicBufferVars("A8")) <> m_oDB_Layer.CheckDBNullStr(drDevice!dev_PIC32Version) Then
              dicParms.Add("dev_PIC32Version", "S:" & CStr(dicBufferVars("A8")))
            End If
          End If

          'OTA Attempts
          If dicBufferVars.KeyExists("B0") Then
            If CStr(dicBufferVars("B0")) <> m_oDB_Layer.CheckDBNullStr(drDevice!dev_LastOtaAttempts) Then
              dicParms.Add("dev_LastOtaAttempts", "S:" & CStr(dicBufferVars("B0")))
            End If
          End If

          'OTA Success
          If dicBufferVars.KeyExists("B1") Then
            If CStr(dicBufferVars("B1")) <> m_oDB_Layer.CheckDBNullStr(drDevice!dev_LastOtaSuccess) Then
              dicParms.Add("dev_LastOtaSuccess", "S:" & CStr(dicBufferVars("B1")))
            End If
          End If



          If dicParms.Count > 0 Then
            dicParms.Add("dev_TimeStamp", "T:" & Format(CDate(drDevice!dev_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
            dbRet = m_oBL_DeviceManager.UpdateDevice(CInt(drDevice!dev_id), dicParms)
            If dbRet = DL_Manager.DB_Return.ok Then
              Diagnostics.Debug.Write(", Updated")
              'sRetMessage += Format(CInt(drDevice!dev_id), "000000") & ": Device Update OK." & vbCrLf
            Else
              'sRetMessage += Format(CInt(drDevice!dev_id), "000000") & ": Device Update FAILED!!!!!!!!." & vbCrLf
              sRowMessage += Format(CInt(drDevice!dev_id), "000000") & ": Device Update FAILED!!!!!!!!." & vbCrLf
            End If
          End If

        End If


        'Process the Device Data

        'Check the last row, flag up missing data.
        drLastDataRow = m_oBL_DeviceManager.GetLastDeviceData(m_oDB_Layer.CheckDBNullInt(drDevice!dev_ID))
        If drLastDataRow IsNot Nothing Then
          If m_oDB_Layer.CheckDBNullInt(drLastDataRow!ddata_AdmMsgCount) + 1 <> CInt(dicBufferVars("A9")) Then
            'Email sequence error.....
            sSubject = "Orbis Streamline buffer processing Data Sequence IMEI: " & CStr(dicBufferVars("A1")) & ", " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & " (UTC)"
            sBody = "Orbis Streamline buffer processing Data Sequence IMEI: " & CStr(dicBufferVars("A1")) & vbCrLf & vbCrLf
            sBody += "Last Seq: " & m_oDB_Layer.CheckDBNullInt(drLastDataRow!ddata_AdmMsgCount) & vbCrLf
            sBody += "This Seq: " & CInt(dicBufferVars("A9")) & vbCrLf

            m_pcl_ErrManager.LogError("zProcessBufferToData: Data Sequence Missing IMEI: " & CStr(dicBufferVars("A1")) & ", Last Seq: " & m_oDB_Layer.CheckDBNullInt(drLastDataRow!ddata_AdmMsgCount) & ",This Seq: " & CInt(dicBufferVars("A9")))

            ' zSendErrorEmail(sSubject, sBody)

          Else
            'All ok, next one in seq....
          End If
        Else
          'all ok!

        End If


        dicParms.Clear()
        dicParms.Add("dev_ID", "N:" & m_oDB_Layer.CheckDBNullStr(drDevice!dev_ID))
        dicParms.Add("buf_id", "N:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_ID))

        'ADM Date
        If dicBufferVars.KeyExists("A2") Then
          dicParms.Add("ddata_AdmDate", "S:" & CStr(dicBufferVars("A2")))
        End If

        'ADM Time
        If dicBufferVars.KeyExists("A3") Then
          dicParms.Add("ddata_AdmTime", "S:" & CStr(dicBufferVars("A3")))
        End If

        'ADM RSSI
        If dicBufferVars.KeyExists("A4") Then
          dicParms.Add("ddata_AdmRssi", "N:" & CStr(dicBufferVars("A4")))
        End If

        'ADM Battery
        If dicBufferVars.KeyExists("A5") Then
          dicParms.Add("ddata_AdmBatteryVoltage", "N:" & CStr(dicBufferVars("A5")))
        End If

        'ADM PIC18
        If dicBufferVars.KeyExists("A7") Then
          dicParms.Add("ddata_AdmPic18Version", "S:" & CStr(dicBufferVars("A7")))
        End If

        'ADM PIC32
        If dicBufferVars.KeyExists("A8") Then
          dicParms.Add("ddata_AdmPic32Version", "S:" & CStr(dicBufferVars("A8")))
        End If

        'ADM Msg Count
        If dicBufferVars.KeyExists("A9") Then
          dicParms.Add("ddata_AdmMsgCount", "N:" & CStr(dicBufferVars("A9")))
        End If

        'ADM OTA Attempts
        If dicBufferVars.KeyExists("B0") Then
          dicParms.Add("ddata_AdmOtaAttempts", "N:" & CStr(dicBufferVars("B0")))
        End If

        'ADM OTA Success
        If dicBufferVars.KeyExists("B1") Then
          dicParms.Add("ddata_AdmOtaSuccess", "N:" & CStr(dicBufferVars("B1")))
        End If

        'Data Flow Event
        If dicBufferVars.KeyExists("FE") Then
          dicParms.Add("ddata_FlowEvent", "N:" & CStr(dicBufferVars("FE")))
        End If

        'Data Flow Event Trigger
        If dicBufferVars.KeyExists("TR") Then
          dicParms.Add("ddata_FlowEventTrigger", "N:" & CStr(dicBufferVars("TR")))
        End If

        'Data Flow Measurement
        If dicBufferVars.KeyExists("FM") Then
          dicParms.Add("ddata_FlowMeasurement", "N:" & CStr(dicBufferVars("FM")))
        End If

        'Data Flow Confidence
        If dicBufferVars.KeyExists("FC") Then
          dicParms.Add("ddata_FlowConfidence", "N:" & CStr(dicBufferVars("FC")))
        End If

        'Data Audit Detect
        If dicBufferVars.KeyExists("AD") Then
          dicParms.Add("ddata_AudioLeakDetect", "N:" & CStr(dicBufferVars("AD")))
        End If

        'Data Audio Detect Confidence
        If dicBufferVars.KeyExists("AC") Then
          dicParms.Add("ddata_AudioLeakConfidence", "N:" & CStr(dicBufferVars("AC")))
        End If

        'Data Audio Pipe Condition
        If dicBufferVars.KeyExists("AP") Then
          dicParms.Add("ddata_PingPipeCondition", "N:" & CStr(dicBufferVars("AP")))
        End If

        'Data Audio Pipe Condition Confidence
        If dicBufferVars.KeyExists("PC") Then
          dicParms.Add("ddata_PingPipeConfidence", "N:" & CStr(dicBufferVars("PC")))
        End If

        'Data Board Temp
        If dicBufferVars.KeyExists("BT") Then
          dicParms.Add("ddata_BoardTemperature", "N:" & CStr(dicBufferVars("BT")))
        End If

        'Data Ambient Temp
        If dicBufferVars.KeyExists("AT") Then
          dicParms.Add("ddata_AmbientTemperature", "N:" & CStr(dicBufferVars("AT")))

          Dim iTemp As Int32
          Dim dAmbientTempC As Decimal

          iTemp = CInt(dicBufferVars("AT"))
          'Calc a real temperature!!
          dAmbientTempC = m_oBL_DeviceManager.ConvertPatchTempToC(iTemp)

          dicParms.Add("ddata_AmbientTempC", "N:" & Format(dAmbientTempC, "000.00"))


        End If


        'Data Cord Leak
        If dicBufferVars.KeyExists("CL") Then
          dicParms.Add("ddata_CordLeak", "N:" & CStr(dicBufferVars("CL")))
        End If


        'Raw File Data
        If dicBufferVars.KeyExists("RF") Then
          dicParms.Add("ddata_RawDataFileName", "S:" & CStr(dicBufferVars("RF")))
        End If



        'Let's do some work on the date time!!!

        dicParms.Add("ddata_DateTimeUtc", "D:" & CStr(drBuffer!buf_InsertDateTimeUtc))
        dicParms.Add("timezone_ID", "N:" & CStr(drDevice!timezone_ID))


        Dim tmUtc As Date = CDate(drBuffer!buf_InsertDateTimeUtc)
        Try
          Dim tzLocal As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(CStr(drDevice!timezone_Name))
          Dim tmLocal As Date = TimeZoneInfo.ConvertTimeFromUtc(tmUtc, tzLocal)

          dicParms.Add("ddata_LocalDateTime", "D:" & Format(tmLocal, "yyyy-MM-dd HH:mm:ss"))
          Diagnostics.Debug.WriteLine(", UTC: {0} Local:{1} {2}.", tmUtc, tmLocal, If(tzLocal.IsDaylightSavingTime(tmLocal), tzLocal.DaylightName, tzLocal.StandardName))

        Catch e1 As TimeZoneNotFoundException
          sRowMessage += "The registry does not define " & CStr(drDevice!timezone_Name) & "zone." & vbCrLf

        Catch e2 As InvalidTimeZoneException
          sRowMessage += "Registry data on the  " & CStr(drDevice!timezone_Name) & "zone has been corrupted." & vbCrLf

        End Try


        'Raw File Trigger
        If dicBufferVars.KeyExists("TN") Then
          dicParms.Add("ddata_RawDataTriggerNumber", "N:" & CInt(dicBufferVars("TN")))
        End If

        iDataID = m_oBL_DeviceManager.InsertDeviceData(dicParms)

        bOK = True

      Else
        sRowMessage += "QueryString: " & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_QueryString) & vbCrLf & vbCrLf
        sRowMessage += "Invalid data:" & sTempMsg & vbCrLf
        sRowMessage += "-----------------------" & vbCrLf


        'Email validation error.....
        sBody = "Orbis Streamline buffer processing Validation Error: " & sTempMsg & vbCrLf & vbCrLf

        sSubject = "Orbis Streamline buffer Test processing Validation Error: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & " (UTC)"

        zSendErrorEmail(sSubject, sBody)

      End If

      If bOK Then
        'move buffer to Processed...
        dicParms.Clear()

        dicParms.Add("buf_id", "N:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_id))
        dicParms.Add("ddata_ID", "N:" & iDataID)
        dicParms.Add("processed_InsertDateTimeUtc", "D:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_InsertDateTimeUtc))
        dicParms.Add("processed_IpAddress", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_IpAddress))
        dicParms.Add("processed_CalledUrl", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_CalledUrl))
        dicParms.Add("processed_CalledPage", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_CalledPage))
        dicParms.Add("processed_QueryString", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_QueryString))

        iProcID = m_oBL_DeviceManager.InsertIotProcessed(dicParms)
        If iProcID > 0 Then
          iRowsAffected = m_oBL_DeviceManager.DeleteIotBuffer(CInt(drBuffer!buf_id))
          If iRowsAffected > 0 Then
            'success
          Else
            sRowMessage += "Problem clearing IotBuffer row for success:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_id)
          End If
        End If

      Else
        'Move buffer to failed....



        dicParms.Clear()

        dicParms.Add("buf_id", "N:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_id))
        dicParms.Add("failed_InsertDateTimeUtc", "D:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_InsertDateTimeUtc))
        dicParms.Add("failed_IpAddress", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_IpAddress))
        dicParms.Add("failed_CalledUrl", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_CalledUrl))
        dicParms.Add("failed_CalledPage", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_CalledPage))
        dicParms.Add("failed_QueryString", "S:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_QueryString))

        iFailID = m_oBL_DeviceManager.InsertIotFailed(dicParms)
        If iFailID > 0 Then
          iRowsAffected = m_oBL_DeviceManager.DeleteIotBuffer(CInt(drBuffer!buf_id))
          If iRowsAffected > 0 Then
            'success
            Diagnostics.Debug.WriteLine("Invalid data from buffer.")
          Else
            sRowMessage += "Problem clearing IotBuffer row for fail:" & m_oDB_Layer.CheckDBNullStr(drBuffer!buf_id)
          End If
        End If


      End If

      If sRowMessage <> "" Then
        sRetMessage += "Buffer ID: " & Format(CLng(drBuffer!buf_id), "##,###,###,###,###,###,000") & "; IMEI: " & sIMEI & ", " & vbCrLf & sRowMessage & vbCrLf
      End If

    Next

    If sRetMessage <> "" Then
      sRetMessage += vbCrLf & vbCrLf & "EOF"
    End If

    lblText.Text += "<br/>Buffer to Device data Completed: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & " (UTC), Row Count: " & dtBuffer.Rows.Count

    If sRetMessage <> "" Then

      sBody = "Orbis Streamline buffer processing errors:" & vbCrLf & vbCrLf
      sBody += sRetMessage
      m_pcl_ErrManager.LogError("Send BufferToData email, body length: " & Len(sBody))


      sSubject = "Orbis Streamline buffer processing: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & " (UTC)"

      zSendErrorEmail(sSubject, sBody)
    End If

  End Sub

  Private Function zParseBufferToDic(drBuffer As DataRow) As eDictionary
    Dim dicReturn As New eDictionary
    Dim nvQueryList As NameValueCollection
    Dim i As Int32
    Dim sKey As String = ""
    Dim sValue As String = ""


    nvQueryList = HttpUtility.ParseQueryString(m_oDB_Layer.CheckDBNullStr(drBuffer!buf_QueryString))


    For i = 0 To nvQueryList.Count - 1
      sKey = UCase(nvQueryList.Keys(i))
      sValue = nvQueryList.Item(i)

      'Diagnostics.Debug.WriteLine(Format(i, "000") & " - QS: " & sKey & ", " & sValue)
      dicReturn.Add(sKey, sValue)

    Next

    Return dicReturn
  End Function
  Private Function zValidateData(dicBufferVars As eDictionary, ByRef sMsg As String, ByRef sIMEI As String, sQueryString As String, ByRef iDevHardID As Int32, drDevice As DataRow) As Boolean
    Dim bOK As Boolean = False
    Dim drDevHardType As DataRow
    'Dim dtTriggerCheck As DataTable


    If dicBufferVars.KeyExists("A0") Then
      drDevHardType = m_oBL_DeviceManager.GetDeviceHardwareTypeByCode(CStr(dicBufferVars("A0")))
      If drDevHardType IsNot Nothing Then
        iDevHardID = m_oDB_Layer.CheckDBNullInt(drDevHardType!devh_ID)
      Else
        sMsg += "Unknown Device Hardware Type: " & CStr(dicBufferVars("A0"))
      End If
    Else
      sMsg += "Missing Device Hardware Type."
    End If



    If dicBufferVars.KeyExists("A1") Then
      If Trim(CStr(dicBufferVars("A1"))) <> "" Then
        Diagnostics.Debug.Write("IMEI: " & CStr(dicBufferVars("A1")))
        sIMEI = CStr(dicBufferVars("A1"))
      Else
        Diagnostics.Debug.Write("IMEI: Missing!! ")
        sIMEI = ""
      End If

      'Validate Buffer row!!!!
      If Trim(CStr(dicBufferVars("A1"))) <> "" And m_pcl_Utils.ValidIMEI(CStr(dicBufferVars("A1"))) Then

        bOK = False
        If dicBufferVars.KeyExists("AT") Then
          If IsNumeric(dicBufferVars("AT")) Then
            bOK = True
          Else
            sMsg = "Invalid AT: " & CStr(dicBufferVars("AT"))
          End If
        Else
          bOK = True
        End If

        If bOK Then
          'Next validation....
          bOK = False
          If dicBufferVars.KeyExists("FC") Then
            If IsNumeric(dicBufferVars("FC")) Then
              bOK = True
            Else
              sMsg += " ,Invalid FC: " & CStr(dicBufferVars("FC"))
            End If
          Else
            bOK = True
          End If

        End If

      Else
        sMsg += "Invalid IMEI: " & Trim(CStr(dicBufferVars("A1")))
      End If
    Else
      sMsg += "Missing IMEI."
    End If

    If dicBufferVars.KeyExists("A7") Then
      bOK = False
      If Trim(CStr(dicBufferVars("A7"))) >= "V1_02" Then 'PIC 18 Version, additional checks.
        If dicBufferVars.KeyExists("EOL") Then
          'Data should have an EOL marker
          bOK = True
        Else
          bOK = False
          sMsg += ", Missing EOL."
        End If
      Else
        'Early FW with no EOL checking.
        bOK = True
      End If
    Else
      bOK = False
      sMsg = ", Missing A7 - PIC18 Version."
    End If

    'Dim sTr As String = "&TR="
    'Dim iTrCount As Int32 = CInt((sQueryString.Length - sQueryString.Replace(sTr, "").Length) / sTr.Length)

    'If iTrCount > 1 Then
    '  'Error!!
    '  bOK = False
    '  sMsg = ", Too many TRs: " & iTrCount
    'End If

    ''ok we need to validate the trigger number for duplicates....
    ''ok, due to duplicate triggers we need to check to see if the trigger already exists.
    ''If unprocessed dups exist we have to discard this data, no easy option I'm afraid... :(

    'If drDevice IsNot Nothing Then

    '  dtTriggerCheck = m_oBL_DeviceManager.GetDeviceDataByDevIDandTriggerUnprocessed(m_oDB_Layer.CheckDBNullInt(drDevice!dev_ID), CInt(dicBufferVars("TN")))

    '  If dtTriggerCheck.Rows.Count > 0 Then
    '    'Let's get creative and bump the trigger out of usable range.
    '    bOK = False
    '    sMsg += ", Duplicate Trigger ID: " & CInt(dicBufferVars("TN"))
    '  End If

    'End If

    If sMsg = "" Then
      bOK = True
    Else
      bOK = False
    End If

    Return bOK

  End Function

  Private Sub zProcessRawDataFiles()
    Dim dtDataRawFilesUnprocessed As DataTable
    Dim drDatarow As DataRow
    Dim iRow As Int32
    Dim sMsg As String = ""
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return
    Dim sNewFileWithDateStamp As String = ""

    Try
      m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles Start...")
      If CBool(System.Configuration.ConfigurationManager.AppSettings("DisableRawFileProcessing")) = False Then

        m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles Start FTPs")

        dtDataRawFilesUnprocessed = m_oBL_DeviceManager.GetDeviceDataForDownload()

        For iRow = 0 To dtDataRawFilesUnprocessed.Rows.Count - 1
          drDatarow = dtDataRawFilesUnprocessed.Rows(iRow)
          sMsg = ""

          ' we need to process the raw data.
          m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles File for Download: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName))

          'OK do we have a file name?
          If zDownloadFileFromFTPtoWebSite(drDatarow, sMsg, sNewFileWithDateStamp) Then
            dicParms.Clear()
            dicParms.Add("ddata_RawDataFileDownloaded", "N:" & "-1")
            dicParms.Add("ddata_RawDataFileName", "S:" & sNewFileWithDateStamp)
            dicParms.Add("ddata_Timestamp", "T:" & Format(CDate(drDatarow!ddata_timestamp), "yyyy-MM-dd HH:mm:ss"))
            dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDatarow!ddata_id), dicParms)
            If dbRet = DL_Manager.DB_Return.ok Then
              'all good
              lblText.Text += "<br/>File downloaded: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & ": " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName)
            Else
              Diagnostics.Debug.WriteLine("DeviceData row Update failed, possible locking issue: " & CStr(drDatarow!ddata_id) & ", Process Raw data file")
              m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles 1 ERROR!!!")

            End If
          Else
            If Left(sMsg, 1) = "2" Then
              'File not found
              dicParms.Clear()
              dicParms.Add("ddata_RawDataFileDownloaded", "N:" & "-2")
              dicParms.Add("ddata_Timestamp", "T:" & Format(CDate(drDatarow!ddata_timestamp), "yyyy-MM-dd HH:mm:ss"))
              dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDatarow!ddata_id), dicParms)
              If dbRet = DL_Manager.DB_Return.ok Then
                'all good
                lblText.Text += "<br/>File download Missing!!: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & ": " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName)
              Else
                Diagnostics.Debug.WriteLine("DeviceData row Update failed, possible locking issue: " & CStr(drDatarow!ddata_id) & ", Process Raw data file")
                m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles 2 ERROR!!!")

              End If

              'flag up a probable error.
              Dim sSubject As String
              Dim sBody As String

              sSubject = "StreamLine Error: FTP File Missing on server: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName)
              sBody = "StreamLine Error: FTP File Missing on server: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName) & vbCrLf & vbCrLf
              sBody += "DeviceData ID: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_ID) & vbCrLf

              zSendErrorEmail(sSubject, sBody)
            End If
            If Left(sMsg, 1) = "1" Then
              'FTP Comms Error
              lblText.Text += "<br/>FTP Download Comms error!!: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & ": " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName)

              'flag up a probable error.
              Dim sSubject As String
              Dim sBody As String

              sSubject = "StreamLine Error: FTP Download Comms error: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName)
              sBody = "StreamLine Error: FTP Download Comms error: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName) & vbCrLf & vbCrLf
              sBody += "StreamLine Error: " & sMsg & vbCrLf & vbCrLf
              sBody += "DeviceData ID: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_ID) & vbCrLf

              zSendErrorEmail(sSubject, sBody)
            End If
          End If
        Next

        m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles Start Extracts")

        dtDataRawFilesUnprocessed = m_oBL_DeviceManager.GetDeviceDataForExtract()

        For iRow = 0 To dtDataRawFilesUnprocessed.Rows.Count - 1
          drDatarow = dtDataRawFilesUnprocessed.Rows(iRow)
          sMsg = ""

          ' we need to process the raw data.
          m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles File for Extract: " & m_oDB_Layer.CheckDBNullStr(drDatarow!ddata_RawDataFileName))

          'OK do we have a file name?
          If zExtractRawDataFile(drDatarow, sMsg) Then
            dicParms.Clear()
            dicParms.Add("ddata_RawDataFileExtracted", "N:" & "-1")
            dicParms.Add("ddata_Timestamp", "T:" & Format(CDate(drDatarow!ddata_timestamp), "yyyy-MM-dd HH:mm:ss"))
            dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDatarow!ddata_id), dicParms)
            If dbRet = DL_Manager.DB_Return.ok Then
              'all good
            Else
              Diagnostics.Debug.WriteLine("DeviceData row Update failed, possible locking issue: " & CStr(drDatarow!ddata_id) & ", Process Raw data file")
              m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles ERROR!!!")

            End If
          Else

          End If
        Next


      Else
        m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles: Processing disabled!")
      End If


      m_pcl_ErrManager.LogError("Buffer2Data, zProcessRawDataFiles End")

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex
    End Try


  End Sub

  Private Function zDownloadFileFromFTPtoWebSite(drDeviceData As DataRow, ByRef sMsg As String, ByRef sNewFileWithDateStamp As String) As Boolean
    Dim bOK As Boolean = False
    Dim drDevice As DataRow = Nothing
    Dim dicParms As New eDictionary
    Dim dbRet As DL_Manager.DB_Return
    Dim sFolderPath As String
    Dim sDataFileFolderPath As String
    Dim sDataFilePath As String
    Dim sTempFilename As String
    Dim sDataFileNoExt As String
    'Dim sCustomer As String
    Dim sDevice As String
    Dim iLooper As Int32 = 0
    Dim bRenameOK As Boolean = False
    Dim dtFileModDate As Date

    Try
      drDevice = m_oBL_DeviceManager.GetDeviceByID(CInt(drDeviceData!dev_id))

      If IsDBNull(drDeviceData!dev_ExternalGuid) Then
        'We need to get and update the GUID...
        dicParms.Clear()
        dicParms.Add("dev_ExternalGuid", "S:" & Guid.NewGuid().ToString())
        dicParms.Add("dev_TimeStamp", "S:" & Format(CDate(drDevice!dev_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateDevice(CInt(drDevice!dev_id), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drDevice = m_oBL_DeviceManager.GetDeviceByID(CInt(drDeviceData!dev_id))
        Else
          'This should not occur, so just smash it out!! Ha Ha!! 
          Dim ex As New Exception("Error updating device Externl GUID")
          Throw ex
        End If
      End If

      'locate Folder for storing raw device data.
      sFolderPath = m_pdl_Manager.sFilePath & "DeviceRawData\" & Format(CInt(drDeviceData!dev_id), "00000000") & "\"

      Dim diDevFolder As New DirectoryInfo(sFolderPath)
      If Not diDevFolder.Exists Then
        diDevFolder.Create()
      End If

      If m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName) <> "" Then


        'ok, device data folder is in place....
        'Let's try and get the file from FTP Site....

        Dim oFTP As DL_FTP
        Dim sFiles() As String = Nothing

        If m_pdl_Manager.ServerName = "localhost" Then
          oFTP = New DL_FTP("prodigyftptest.orbis-sys.com", "ProdigyRawData", "prodigyftptest7", "dhdh732hDHDje", 21, m_pcl_ErrManager)
        Else
          oFTP = New DL_FTP("prodigyftp.orbis-sys.com", "ProdigyRawData", "orbisftptestds5", "DK239awdnad2dff23", 21, m_pcl_ErrManager)
        End If



        sFiles = oFTP.GetFileList(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName))

        If sFiles.Length() > 0 Then
          'File found on FTP Server

          dtFileModDate = oFTP.GetFileModDate(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName))
          sNewFileWithDateStamp = Format(dtFileModDate, "yyyy_MM_dd__HH_mm_ss") & "_" & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName)
          sTempFilename = m_pbl_Client.TempFolderFullPath() & sNewFileWithDateStamp


          oFTP.DownloadFile(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName), sTempFilename)

          If oFTP.RetValue = 226 Then
            'OK download complete, move it to Processed.
            sFiles = oFTP.GetFileList("Processed")
            If sFiles.Length = 0 Then
              'Create Processed
              oFTP.CreateDirectory("Processed")
            End If

            'sCustomer = Format(m_oDB_Layer.CheckDBNullInt(drDevice!cust_id), "000000")
            'sFiles = oFTP.GetFileList("Processed/" & sCustomer)
            'If sFiles.Length = 0 Then
            '  oFTP.CreateDirectory("Processed/" & sCustomer)
            'End If

            sDevice = Format(m_oDB_Layer.CheckDBNullInt(drDevice!dev_id), "00000000")
            sFiles = oFTP.GetFileList("Processed/" & sDevice)
            If sFiles.Length = 0 Then
              oFTP.CreateDirectory("Processed/" & sDevice)
            End If



            sFiles = oFTP.GetFileList("Processed/" & sDevice & "/" & sNewFileWithDateStamp)
            If sFiles.Length > 0 Then
              'the file already exists in the processed folder.....
              Dim sRenameFile As String
              iLooper = 0
              bRenameOK = False
              Do While bRenameOK = False And iLooper < 500
                Try
                  If iLooper > 0 Then
                    sRenameFile = "Processed/" & sDevice & "/" & sNewFileWithDateStamp & "(" & iLooper & ")"
                  Else
                    sRenameFile = "Processed/" & sDevice & "/" & sNewFileWithDateStamp
                  End If

                  oFTP.RenameFile("Processed/" & sDevice & "/" & sNewFileWithDateStamp, sRenameFile)
                  'oFTP.CreateDirectory("Processed/" & sDevice)

                  bRenameOK = True

                Catch ex As Exception
                  If oFTP.RetValue = 550 Then
                    bRenameOK = False
                    iLooper += 1
                  Else
                    Throw New Exception("Rename FTP Failure A1: " & sNewFileWithDateStamp & " " & oFTP.RetMsg)
                  End If
                End Try

              Loop

              If iLooper > 500 Then
                Throw New Exception("Rename FTP Failure A2: " & sNewFileWithDateStamp)
              End If

            End If


            'oFTP.DeleteFile("Processed/" & sDevice & "/" & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName))
            iLooper = 0
            bRenameOK = False
            Do While bRenameOK = False And iLooper < 500
              Try
                If iLooper > 0 Then
                  oFTP.RenameFile(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName), "Processed/" & sDevice & "/" & sNewFileWithDateStamp & "(" & iLooper & ")")
                Else
                  oFTP.RenameFile(m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName), "Processed/" & sDevice & "/" & sNewFileWithDateStamp)
                End If

                bRenameOK = True

              Catch ex As Exception
                If oFTP.RetValue = 550 Then
                  bRenameOK = False
                  iLooper += 1
                Else
                  Throw New Exception("Rename FTP Failure B1: " & sNewFileWithDateStamp & " " & oFTP.RetMsg)
                End If
              End Try

            Loop

            If iLooper > 500 Then
              Throw New Exception("Rename FTP Failure B2: " & sNewFileWithDateStamp)
            End If

            sDataFilePath = sFolderPath & sNewFileWithDateStamp
            Dim fiDownloadFile As New FileInfo(sTempFilename)
            sDataFileNoExt = Replace(fiDownloadFile.Name, fiDownloadFile.Extension, "")

            Dim fiDestFile As New FileInfo(sDataFilePath)
            If fiDestFile.Exists Then
              Dim sRenameFile As String
              sRenameFile = sFolderPath & sNewFileWithDateStamp
              iLooper = 0
              bRenameOK = False

              Do While bRenameOK = False And iLooper < 500
                Dim fiDestCheck As New FileInfo(sRenameFile)
                If fiDestCheck.Exists = False Then
                  bRenameOK = True
                Else
                  iLooper += 1
                  sRenameFile = sFolderPath & sNewFileWithDateStamp & "(" & iLooper & ")"
                End If
              Loop



              fiDestFile.MoveTo(sRenameFile)
            End If
            fiDestFile = Nothing

            Diagnostics.Debug.WriteLine("Downloaded file: " & fiDownloadFile.Length)
            fiDownloadFile.MoveTo(sDataFilePath)

            'Remove 

            sDataFileFolderPath = sFolderPath & Format(CInt(drDeviceData!ddata_id), "000000000000") & "-" & Format(CDate(drDeviceData!ddata_DateTimeUtc), "yyyy-MM-dd HH-mm-ss")
            Dim diDataFileFolder As New DirectoryInfo(sDataFileFolderPath)
            If Not diDataFileFolder.Exists Then
              diDataFileFolder.Create()
            Else
              'Clear any existing files...
              For Each deleteFile In diDataFileFolder.GetFiles()
                deleteFile.Delete()
              Next
            End If

            bOK = True

          Else
            sMsg = "1. Problem FTP Transfer: " & sNewFileWithDateStamp & ",  Ret Code: " & oFTP.RetValue & ": " & oFTP.RetMsg
            bOK = False
          End If
        Else
          'File no found on FTP server.
          sMsg = "2. File Not Found: " & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName)
        End If
      End If

      diDevFolder = Nothing

      Return bOK

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex
    End Try

  End Function



  Private Function zExtractRawDataFile(drDeviceData As DataRow, ByRef sMsg As String) As Boolean
    Dim iTrigger As Long = 0
    Dim sType As String = ""
    Dim iLDataLength As Int32 = 0
    Dim iSampleCount As Int32
    Dim iInt As Int64
    Dim sExt As String = ""
    Dim iSampleLen As Int32 = 0
    Dim iCol As Int32 = 0
    Dim iBytesRead As Int32 = 0
    Dim fiInput As FileInfo
    Dim sDetailedProcessing As String = ""
    Dim dtDeviceData4Trigger As DataTable = Nothing
    Dim drDeviceData4Trigger As DataRow = Nothing
    Dim iID As Int32
    Dim sFolderPath As String
    Dim sDataFilePath As String
    Dim sDataFileNoExt As String
    Dim sFilenameRelativePath As String
    Dim sDataFileFolderPath As String
    Dim bRet As Boolean = False
    Dim iPerdKounter As Int32 = 0
    Dim sLastFileWritten As String = ""
    Dim iLastRawDataFileID As Int32

    Dim sSubject As String = ""
    Dim sBody As String = ""

    sFolderPath = m_pdl_Manager.sFilePath & "DeviceRawData\" & Format(CInt(drDeviceData!dev_id), "00000000") & "\"
    sDataFileFolderPath = sFolderPath & Format(CInt(drDeviceData!ddata_id), "000000000000") & "-" & Format(CDate(drDeviceData!ddata_DateTimeUtc), "yyyy-MM-dd HH-mm-ss")
    sDataFilePath = sFolderPath & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName)

    fiInput = New FileInfo(sDataFilePath)

    m_pcl_ErrManager.LogError("zExtractRawDataFile: " & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName))

    sDataFileNoExt = Replace(fiInput.Name, fiInput.Extension, "")
    sFilenameRelativePath = "\DeviceRawData\" & Format(CInt(drDeviceData!dev_id), "00000000") & "\"
    sFilenameRelativePath += Format(CInt(drDeviceData!ddata_id), "000000000000") & "-" & Format(CDate(drDeviceData!ddata_DateTimeUtc), "yyyy-MM-dd HH-mm-ss") & "\"

    Diagnostics.Debug.WriteLine("Read File: " & sDataFilePath)

    Try
      Using strm As New FileStream(sDataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Using readerInput As New BinaryReader(strm)
          sDetailedProcessing += "Open file: " & sDataFilePath & vbCrLf
          sDetailedProcessing += "Length: " & fiInput.Length & vbCrLf

          Do While readerInput.PeekChar > 0
            'Read integer and byte vaules from the file
            sDetailedProcessing += "---------------" & vbCrLf
            sDetailedProcessing += "Read Tag..." & vbCrLf

            Dim sTagArray() As Char = readerInput.ReadChars(4)
            Dim sTag As New String(sTagArray)
            iBytesRead += 4
            sDetailedProcessing += "Tag: " & sTag & vbCrLf
            sDetailedProcessing += "Type: " & sType & vbCrLf

            Dim iSize As Int32 = readerInput.ReadInt32()
            iBytesRead += 4
            sDetailedProcessing += "Tag Size/Trig: " & Format(iSize, "###,###,###,##0") & vbCrLf

            Diagnostics.Debug.WriteLine("Read Tag: " & sTag & ", Type: " & sType & ", Size: " & iSize & " bytes, Bytes Read: " & iBytesRead & " bytes")

            Select Case sTag
              Case "AUDO"
                sExt = "_" & sType & "_" & Format(iTrigger, "000000") & "_AUDO.csv"
                iSampleLen = 2

              Case "PING"
                sExt = "_" & sType & "_" & Format(iTrigger, "000000") & "_PING.csv"
                iSampleLen = 2
                If fiInput.Length = 12008 Then
                  'length is incorrect, should be 12016, tweak the data to allow processing, ignoring the missing 8 bytes.
                  iSize = iSize - 8
                  sDetailedProcessing += "Tag PERD Size changed: " & Format(iSize, "###,###,###,##0") & vbCrLf

                End If

              Case "STRN"
                sExt = "_" & sType & "_" & Format(iTrigger, "000000") & "_STRN.csv"
                iSampleLen = 2

              Case "TMPS"
                sExt = "_" & sType & "_" & Format(iTrigger, "000000") & "_TMPS.csv"
                iSampleLen = 4

              Case "TRIG"
                'Set Trigger ID and then prep for the data collection sub-blocks.
                sExt = ""
                iTrigger = iSize
                drDeviceData4Trigger = Nothing
                sType = "T"

                If iTrigger = 260 Then
                  sType = sType
                End If

              Case "PERD"
                sExt = ""
                sType = "P"
                iPerdKounter = iPerdKounter - 1 'Ensure periodics are unique!!!!
                iTrigger = iPerdKounter

              Case Else
                'Expected TAG....
                'Most likely caused by file corruption, so tidy it up and mop it out.


                sBody += "-------------------------------------------------------------------------------------------" & vbCrLf
                sBody += "StreamLine Error: Unexpected TAG: " & sTag & vbCrLf & vbCrLf
                sBody += "Data File: " & sDataFileNoExt & vbCrLf
                sBody += "Device ID: " & CInt(drDeviceData!dev_id) & vbCrLf
                sBody += "Trigger: " & iTrigger & vbCrLf
                sBody += "Last File Deleted: " & sLastFileWritten & vbCrLf
                sBody += "Delete Raw File Row: " & iLastRawDataFileID & vbCrLf

                sType = ""
                iTrigger = -1

                'We need to nuke the last file and the rawdata file.
                If sLastFileWritten <> "" Then
                  Dim fiLastFile As New FileInfo(sLastFileWritten)
                  fiLastFile.Delete()
                  fiLastFile = Nothing

                  m_oBL_DeviceManager.DeleteRawDataFileRow(iLastRawDataFileID)

                End If


                Exit Do

            End Select
            m_pcl_ErrManager.LogError("zExtractRawDataFile: Tag Found: " & sTag & ", Size: " & iSize)


            If sType <> "" Then
              If sExt <> "" Then
                'We have data, lets us export to CSV
                iSampleCount = CInt(iSize / iSampleLen)
                sDetailedProcessing += "Sample Len: " & iSampleLen & vbCrLf
                sDetailedProcessing += "Sample Count: " & iSampleCount & vbCrLf

                Dim diOutput As New DirectoryInfo(sDataFileFolderPath)
                If diOutput.Exists = False Then
                  diOutput.Create()
                End If
                diOutput = Nothing

                m_pcl_ErrManager.LogError("zExtractRawDataFile: Write File: " & sDataFileFolderPath & "\" & sDataFileNoExt & sExt)


                Dim swOutput As System.IO.StreamWriter = Nothing
                swOutput = New System.IO.StreamWriter(sDataFileFolderPath & "\" & sDataFileNoExt & sExt, False)
                sDetailedProcessing += "Output File: " & sDataFileFolderPath & "\" & sDataFileNoExt & sExt & vbCrLf
                sLastFileWritten = sDataFileFolderPath & "\" & sDataFileNoExt & sExt

                iCol = 0

                For i = 1 To iSampleCount
                  iCol += 1


                  Diagnostics.Debug.WriteLine("Write: i: " & i & ", bytes Read: " & iBytesRead)
                  'Get select 
                  Select Case iSampleLen
                    Case 2
                      iInt = readerInput.ReadInt16()
                      iBytesRead += 2

                    Case 4
                      iInt = readerInput.ReadInt32()
                      iBytesRead += 4

                  End Select

                  Select Case sTag
                    Case "AUDO", "PING"
                      swOutput.WriteLine(Format(iInt, "##########0"))

                    Case "STRN"
                      swOutput.WriteLine(Format(iInt, "##########0"))

                    Case "TMPS"
                      If iCol = 1 Then
                        swOutput.Write(Format(iInt, "##########0"))
                        swOutput.Write(",")
                      Else
                        swOutput.WriteLine(Format(iInt, "##########0"))
                        iCol = 0
                      End If

                    Case Else

                  End Select


                Next

                m_pcl_ErrManager.LogError("zExtractRawDataFile: Close File: ")

                swOutput.Close()
                sDetailedProcessing += "Output Complete " & vbCrLf

                'OK we have a data file to process, let's insert some details into the DB for later....
                Dim dicParms As New eDictionary
                Dim sSQL As String = ""
                Dim iRowsAffected As Int32

                If InStr(Request.Url.ToString, "localhost") > 0 Then
                  'We are running on dev PC, remove any existing rows in the DB....
                  'NOT TO RUN ON LIVE!!!

                  dicParms.Add("dev_id", "N:" & CStr(drDeviceData!dev_id))
                  dicParms.Add("rdf_TriggerNumber", "N:" & iTrigger)
                  dicParms.Add("rdf_Tag", "S:" & sTag)
                  sSQL = "DELETE from rawdatafiles where dev_id = @dev_id and rdf_TriggerNumber=@rdf_TriggerNumber and rdf_Tag=@rdf_Tag"
                  m_oDB_Layer.RunSqlNonQuery(sSQL, iRowsAffected, False,, dicParms)
                End If

                dicParms.Clear()
                If sType = "T" Then
                  'Trigger
                  If drDeviceData4Trigger IsNot Nothing Then
                    dicParms.Add("ddata_id", "N:" & CStr(drDeviceData4Trigger!ddata_id))
                  Else
                    dicParms.Add("ddata_id", "N:" & "-1")
                  End If
                Else
                  'Periodic - match to this ddata_id.
                  dicParms.Add("ddata_id", "N:" & CStr(drDeviceData!ddata_id))
                End If

                dicParms.Add("dev_ID", "N:" & CStr(drDeviceData!dev_id))
                dicParms.Add("rdf_TriggerNumber", "N:" & iTrigger)
                dicParms.Add("rdf_Type", "S:" & sType)
                dicParms.Add("rdf_Tag", "S:" & sTag)
                dicParms.Add("rdf_DateExtracted", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
                dicParms.Add("rdf_AudioLeak1Processed", "X:")
                dicParms.Add("rdf_AudioLeak1Status", "N:" & "0")
                dicParms.Add("rdf_FilenameIncoming", "S:" & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_RawDataFileName))
                dicParms.Add("rdf_FilenamePath", "S:" & sFilenameRelativePath)
                dicParms.Add("rdf_ExtractedFilename", "S:" & sDataFileNoExt & sExt)
                Dim fiTemp As New FileInfo(sDataFileFolderPath & "\" & sDataFileNoExt & sExt)
                dicParms.Add("rdf_SizeBytes", "N:" & fiTemp.Length)
                fiTemp = Nothing

                iID = m_oBL_DeviceManager.InsertRawDataFile(dicParms)
                iLastRawDataFileID = iID

                lblText.Text += "<br/>Extacted : " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & ": " & sDataFileNoExt & sExt

                If fiInput.Length - iBytesRead <= 8 Then
                  'Abort, this is junk chars on the end!!
                  Exit Do
                End If

              Else
                If sTag = "TRIG" Then
                  'This Is a trigger header, we need to find the DeviceData for this trigger....
                  dtDeviceData4Trigger = m_oBL_DeviceManager.GetDeviceDataByDevIDandTriggerUnprocessed(CInt(drDeviceData!dev_id), iTrigger)
                  If dtDeviceData4Trigger.Rows.Count = 1 Then
                    drDeviceData4Trigger = dtDeviceData4Trigger.Rows(0)
                  Else
                    'Throw New Exception("zExtractRawDataFile, multiple Device Data 4 Trigger rows located, Dev: " & CInt(drDeviceData!dev_id) & ", Trigger: " & iTrigger)

                    sBody += "-------------------------------------------------------------------------------------------" & vbCrLf
                    sBody += "StreamLine Error: Missing Device data Trigger." & vbCrLf & vbCrLf
                    sBody += "Data File: " & sDataFileNoExt & vbCrLf
                    sBody += "Device ID: " & CInt(drDeviceData!dev_id) & vbCrLf
                    sBody += "Trigger: " & iTrigger & vbCrLf

                  End If
                Else
                  'PERD
                  drDeviceData4Trigger = Nothing
                End If

              End If
            End If
          Loop

        End Using
      End Using

      bRet = True

      Return bRet


    Catch ex As Exception
      If ex.GetType Is GetType(System.IO.EndOfStreamException) Or ex.GetType Is GetType(System.ArgumentException) Then
        sBody += "-------------------------------------------------------------------------------------------" & vbCrLf
        sBody += "-------------------------------------------------------------------------------------------" & vbCrLf
        sBody += "StreamLine Error: ProcessBufferToData - ExtractRawDataFile: " & sDataFileNoExt & vbCrLf & vbCrLf
        sBody += "Data File Path: " & sDataFilePath & vbCrLf
        sBody += "Data Folder Path: " & sDataFileFolderPath & vbCrLf
        sBody += "File Length: " & fiInput.Length & vbCrLf
        sBody += "Bytes Read: " & iBytesRead & vbCrLf
        sBody += "----------------------------------------" & vbCrLf
        sBody += "Error: " & ex.Message & vbCrLf
        sBody += "----------------------------------------" & vbCrLf & vbCrLf
        sBody += "Detailed processing: " & vbCrLf
        sBody += sDetailedProcessing & vbCrLf

        sBody += "-------------------------------------------------------------------------------------------" & vbCrLf
        sBody += "-------------------------------------------------------------------------------------------" & vbCrLf

        bRet = True

      Else
        Throw ex
        bRet = False
      End If

    Finally
      If sBody <> "" Then
        'We have to send an email!!!!!
        sSubject = "StreamLine Errors: ExtractRawDataFile: " & sDataFileNoExt
        zSendErrorEmail(sSubject, sBody)
      End If

      fiInput = Nothing
      zExtractRawDataFile = bRet
    End Try

  End Function

  Private Sub zSendErrorEmail(sSubject As String, sBody As String)
    Dim sFrom As String
    Dim sTo As String
    Dim sCC As String

    sFrom = "streamline@relay.horizon-it.co.uk"
    If InStr(Request.Url.ToString, "localhost") = 0 Then
      sTo = "peto@orbis-sys.com;prodigy1@orbis-sys.com"
      sCC = "paul.craven@horizon-it.co.uk"
    Else
      sTo = "paul.craven@horizon-it.co.uk"
      sCC = ""
    End If

    m_pdl_Manager.SendEmail(sFrom, sTo, sCC, "", sSubject, sBody)

  End Sub
  Private Sub zProcessPostProcessing()
    'ok here we take all this lovely raw data and make something meaningful out of it!

    Dim dtRawFiles As DataTable
    Dim drRawFile As DataRow
    Dim iRawFile As Int32
    Dim sTempAlgoFolder As String
    Dim sTempAlgoSessionFolder As String
    Dim sSessionFolderStub As String
    Dim iTemp As Int32
    Dim iRowKounter As Int32 = 0

    'Check / Create the Temp Algo folder
    sTempAlgoFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"

    sSessionFolderStub = "Session_" & Session.SessionID
    sTempAlgoSessionFolder = sTempAlgoFolder & sSessionFolderStub
    Dim diAlgoSession As New DirectoryInfo(sTempAlgoSessionFolder)
    If diAlgoSession.Exists = False Then
      diAlgoSession.Create()
    End If
    diAlgoSession = Nothing

    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Start")
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Session: " & sTempAlgoSessionFolder)


    'Pass 1, process any audio energies first, as these are needed in Pass 2
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: Audio Averages")
    Diagnostics.Debug.WriteLine("Pass1: Audio Averages")
    dtRawFiles = m_oBL_DeviceManager.getRawDataFiles4Processing("AUDIOAVERAGE")
    Diagnostics.Debug.WriteLine("Rows: " & dtRawFiles.Rows.Count)
    iTemp = 0
    For iRawFile = 0 To dtRawFiles.Rows.Count - 1
      drRawFile = dtRawFiles.Rows(iRawFile)

      'Let's Process Audio Averages
      'drRawFile = zProcessAudioAverage(drRawFile)

      drRawFile = zProcessAudioEnergy(drRawFile, sTempAlgoFolder, sSessionFolderStub, sTempAlgoSessionFolder)


      iTemp += 1
      If iTemp Mod 100 = 0 Then
        Diagnostics.Debug.WriteLine(".")
      Else
        Diagnostics.Debug.Write(".")
      End If
    Next
    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")

    'Pass 2, Process leak data, and maybe determine leak baseline, using audio averages, if one does not exist....
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: Leak1 Processing")
    Diagnostics.Debug.WriteLine("Pass2: Leak1 Processing")
    iTemp = 0
    dtRawFiles = m_oBL_DeviceManager.getRawDataFiles4Processing("AUDIOLEAK1")
    Diagnostics.Debug.WriteLine("Rows: " & dtRawFiles.Rows.Count)
    For iRawFile = 0 To dtRawFiles.Rows.Count - 1
      drRawFile = dtRawFiles.Rows(iRawFile)


      'Let's Process Audio Leak1
      drRawFile = zProcessAudioLeak1(drRawFile, sTempAlgoFolder, sSessionFolderStub, sTempAlgoSessionFolder)
      If drRawFile Is Nothing Then
        m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: Leak1 Processing Jump Out Failure")
        Exit For
      End If
      iTemp += 1
      If iTemp Mod 100 = 0 Then
        Diagnostics.Debug.WriteLine(".")
        Diagnostics.Debug.Write(iTemp & ": ")
      Else
        Diagnostics.Debug.Write(".")
      End If


    Next
    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")

    'Pass 3, Process temperature files, basic min,avg & max
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass3: Temp1 Processing")
    Diagnostics.Debug.WriteLine("Pass3: Temp1 Processing")
    iTemp = 0
    dtRawFiles = m_oBL_DeviceManager.getRawDataFiles4Processing("TEMPERATURE1")
    Diagnostics.Debug.WriteLine("Rows: " & dtRawFiles.Rows.Count)
    For iRawFile = 0 To dtRawFiles.Rows.Count - 1
      drRawFile = dtRawFiles.Rows(iRawFile)

      'Let's Process Audio Leak1
      drRawFile = zProcessTemperature1(drRawFile)

      iTemp += 1
      If iTemp Mod 100 = 0 Then
        Diagnostics.Debug.WriteLine(".")
      Else
        Diagnostics.Debug.Write(".")
      End If


    Next
    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")

    'Pass 4, Process Strain files, basic min,avg & max
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass4: Strain Processing")
    Diagnostics.Debug.WriteLine("Pass4: Strain1 Processing")
    iTemp = 0
    dtRawFiles = m_oBL_DeviceManager.getRawDataFiles4Processing("STRAIN1")
    Diagnostics.Debug.WriteLine("Rows: " & dtRawFiles.Rows.Count)
    For iRawFile = 0 To dtRawFiles.Rows.Count - 1
      drRawFile = dtRawFiles.Rows(iRawFile)

      'Let's Process Audio Leak1
      drRawFile = zProcessStrain1(drRawFile)

      iTemp += 1
      If iTemp Mod 100 = 0 Then
        Diagnostics.Debug.WriteLine(".")
      Else
        Diagnostics.Debug.Write(".")
      End If


    Next
    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")



    'Pass 6, Process Pipe Condition 1 data 
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass6: Pipe Condition 1 Processing")
    Diagnostics.Debug.WriteLine("Pass6: Pipe Condition 1 Processing")
    iTemp = 0
    dtRawFiles = m_oBL_DeviceManager.getRawDataFiles4Processing("PIPECONDITION1")
    Diagnostics.Debug.WriteLine("Rows: " & dtRawFiles.Rows.Count)
    For iRawFile = 0 To dtRawFiles.Rows.Count - 1
      drRawFile = dtRawFiles.Rows(iRawFile)


      'Let's Process Audio Leak1
      drRawFile = zProcessPipeCondition1(drRawFile, sTempAlgoFolder, sSessionFolderStub, sTempAlgoSessionFolder)
      If drRawFile Is Nothing Then
        m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass7: Pipe Condition1 Processing Jump Out Failure")
        Exit For
      End If
      iTemp += 1
      If iTemp Mod 100 = 0 Then
        Diagnostics.Debug.WriteLine(".")
      Else
        Diagnostics.Debug.Write(".")
      End If


    Next
    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")









    'Pass 99, Process 7 Day Moving Average (7DMA)
    '=============================================================================================================
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass99: 7DMA")
    Diagnostics.Debug.WriteLine("Pass99: 7DMA: " & Format(Now(), "HH:mm:ss.ffff"))
    iTemp = 0
    iRowKounter = m_oBL_DeviceManager.UpdateDeviceData47dmaCalc()
    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass99 Updated:" & iRowKounter)
    Diagnostics.Debug.WriteLine("Pass99:  Updated:" & iRowKounter & ", " & Format(Now(), "HH:mm:ss.ffff"))

    Diagnostics.Debug.WriteLine("")
    Diagnostics.Debug.WriteLine("---------------")


    m_oBL_DeviceManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)

    Dim diAlgoSession2 As New DirectoryInfo(sTempAlgoSessionFolder)
    If diAlgoSession2.Exists Then
      diAlgoSession2.Delete()
    End If
    diAlgoSession2 = Nothing

    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: End")



  End Sub
  Private Function zProcessAudioAverage(drRawFile As DataRow) As DataRow
    Dim drRet As DataRow
    Dim bok As Boolean = False
    Dim sRawFile As String
    Dim sValue As String
    Dim iTotal As Int32 = 0
    Dim iKounter As Int32 = 0
    Dim iRow As Int32 = 0
    Dim dicParms As New eDictionary
    Dim dAvg As Decimal
    Dim dMin As Decimal = 99999
    Dim dMax As Decimal = -99999
    Dim dbRet As DL_Manager.DB_Return
    Dim bBusted As Boolean = False

    Const c_AUDIO_VALUE As Int32 = 0

    sRawFile = m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)

    'Diagnostics.Debug.Write("zProcessAudioAverage: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))


    Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(sRawFile, Text.Encoding.GetEncoding("ISO-8859-1"))
      MyReader.TextFieldType = FileIO.FieldType.Delimited
      MyReader.SetDelimiters(",")
      Dim currentRow As String()
      While Not MyReader.EndOfData
        Try
          currentRow = MyReader.ReadFields()
          iRow += 1

          sValue = currentRow(c_AUDIO_VALUE)
          If IsNumeric(sValue) Then
            iTotal += CInt(sValue)
            iKounter += 1

            If CInt(sValue) > dMax Then
              dMax = CInt(sValue)
            End If
            If CInt(sValue) < dMin Then
              dMin = CInt(sValue)
            End If

            If CInt(sValue) > 10000 Or CInt(sValue) < -2 Then
              bBusted = True
            End If

          End If


        Catch ex As Exception
          Throw ex
        End Try
      End While
    End Using

    'If bBusted = False Then
    'Normal readings
    dAvg = CDec(iTotal / iKounter)
    'Diagnostics.Debug.WriteLine(", rows: " & iKounter & ", Avg:" & dAvg)
    dicParms.Add("rdf_AudioAverage", "N:" & dAvg)
    'Else
    '  dicParms.Add("rdf_AudioAverage", "N:" & -2)
    'End If
    If dMin = 99999 Then
      dMin = 0
    End If
    If dMax = -99999 Then
      dMax = 0
    End If
    dicParms.Add("rdf_AudioMin", "N:" & dMin)
    dicParms.Add("rdf_AudioMax", "N:" & dMax)



    dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))

    dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
    If dbRet = DL_Manager.DB_Return.ok Then
      drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
    Else
      Throw New Exception("Error Updating RawDeviceFile: " & CInt(drRawFile!rdf_ID))
    End If

    Return drRet
  End Function
  Private Function zProcessAudioLeak1(drRawFile As DataRow, sTempAlgoFolder As String, sSessionFolderStub As String, sTempAlgoSessionFolder As String) As DataRow
    Dim drRet As DataRow = Nothing
    Dim bok As Boolean = False
    Dim sBaseLineFile As String
    Dim drLowest As DataRow
    Dim dbRet As DL_Manager.DB_Return
    Dim dtStartTime As DateTime = Now()
    Dim dicParms As New eDictionary
    Dim sDestFileName As String

    Dim bSetBaseline As Boolean = False

    sBaseLineFile = m_pdl_Manager.sFilePath & "DeviceRawData\" & Format(CInt(drRawFile!dev_id), "00000000") & "\a_AudioLeak1BaseLine.csv"

    If CDec(drRawFile!rdf_AudioEnergy) <> 0 Then

      'Do we need a base line file or a new lower one?
      drLowest = m_oBL_DeviceManager.getRawDataFilesLowestAudioPERD(CInt(drRawFile!dev_id))
      If Not IsDBNull(drLowest!dev_LowestAudioEnergy) Then 'drLowest has the latest version of the device dev_LowestAudioEnergy
        If CDec(drLowest!rdf_AudioEnergy) < CDec(drLowest!dev_LowestAudioEnergy) Then
          bSetBaseline = True
        End If
      Else
        'Null for force update....
        bSetBaseline = True
      End If

      If bSetBaseline Then
        'remove if exists
        Dim fiBaseLine As New FileInfo(sBaseLineFile)
        If fiBaseLine.Exists Then
          fiBaseLine.Delete()
        End If
        fiBaseLine = Nothing

        m_pcl_ErrManager.LogError("Set Audio Baseline: " & m_pdl_Manager.sFilePath & "DeviceRawData\" & Format(CInt(drRawFile!dev_id), "00000000") & "\a_AudioLeak1BaseLine.csv")

        If Not drLowest Is Nothing Then
          m_pcl_ErrManager.LogError("Copy Baseline: " & m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drLowest!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drLowest!rdf_ExtractedFilename))
          Dim fiBaseLineCopy As New FileInfo(m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drLowest!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drLowest!rdf_ExtractedFilename))
          fiBaseLineCopy.CopyTo(sBaseLineFile)
          fiBaseLineCopy = Nothing

          dicParms.Clear()
          dicParms.Add("dev_LowestAudioEnergy", "N:" & CStr(drLowest!rdf_AudioEnergy))
          dicParms.Add("dev_LowestAudioEnergyRdfID", "N:" & CStr(drLowest!rdf_ID))
          dicParms.Add("dev_TimeStamp", "T:" & CStr(drRawFile!dev_TimeStamp))

          dbRet = m_oBL_DeviceManager.UpdateDevice(CInt(drRawFile!dev_id), dicParms)
          If dbRet = DL_Manager.DB_Return.ok Then
            'Re-get the RawDataFile
            drRawFile = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_id))
          Else
            Throw New Exception("Cannot update device lowest Audio Energy: " & dbRet.ToString)
          End If
        Else
          'This shouldn't be possible, so let's cause a fuss!
          Throw New Exception("Cannot locate Leak Baseline: " & sBaseLineFile)
        End If
      End If

      'm_pcl_ErrManager.LogError("Audio Baseline missing: " & m_pdl_Manager.sFilePath & "DeviceRawData\" & Format(CInt(drRawFile!dev_id), "00000000") & "\a_AudioLeak1BaseLine.csv")

      'Ok, let's Check the session folder and clear it required....
      m_pcl_ErrManager.LogError("zProcessAudioLeak1, sTempAlgoSessionFolder: " & sTempAlgoSessionFolder)
      m_oBL_DeviceManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)


      If CInt(drRawFile!ddata_id) <> -1 And CInt(drRawFile!rdf_AudioEnergy) <> 0 Then
        'OK, we have matching device data, let's process.....

        'Copy Files into folder....
        'Leak BaseLine
        Dim fiBaseLineLeak1 As New FileInfo(sBaseLineFile)
        fiBaseLineLeak1.CopyTo(sTempAlgoSessionFolder & "\" & fiBaseLineLeak1.Name)

        'File to Compare
        Dim fiAudioCompare As New FileInfo(m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))
        fiAudioCompare.CopyTo(sTempAlgoSessionFolder & "\" & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))

        m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))


        Dim sFolder As String
        Dim sbatchFileLocation As String

        sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
        sbatchFileLocation = sFolder & "RunAudioLeak1.bat"


        Dim oProcess As New Process
        oProcess.StartInfo.FileName = sbatchFileLocation
        '    oProcess.StartInfo.Arguments = " .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

        'oProcess.StartInfo.UserName = "53FF8F5\AdministratorDS"
        'oProcess.StartInfo.Password = m_pcl_Utils.ConvertToSecureString("Lancster!1979")
        oProcess.StartInfo.Arguments = " .\" & sSessionFolderStub & "\ " & "a_AudioLeak1BaseLine.csv" & " " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)

        oProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(sbatchFileLocation)
        oProcess.StartInfo.UseShellExecute = False

        oProcess.Start()
        oProcess.WaitForExit()

        Dim fiCheckErrorLevel As New FileInfo(sTempAlgoSessionFolder & "\ErrorLevel.Log")
        If fiCheckErrorLevel.Exists Then
          fiCheckErrorLevel = Nothing
          Dim sfileCheckErrorLevel As String = sTempAlgoSessionFolder & "\ErrorLevel.Log"
          Dim srCheckErrorLevel As StreamReader
          Dim sErrorLevel As String

          srCheckErrorLevel = File.OpenText(sfileCheckErrorLevel)
          'Now, read the entire file into a string
          sErrorLevel = srCheckErrorLevel.ReadToEnd()
          sErrorLevel = Replace(sErrorLevel, vbCrLf, "")
          'There is an odd char at the end of the error level file, just removing it.
          sErrorLevel = Left(sErrorLevel, Len(sErrorLevel) - 1)

          srCheckErrorLevel.Close()

          If sErrorLevel = "ErrorLevel:0" Then
            'we have a winner!!!!
            'Check the post processed folder....
            Dim diProcessedDeviceFolder As New DirectoryInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/")
            If diProcessedDeviceFolder.Exists = False Then
              diProcessedDeviceFolder.Create()
            End If
            diProcessedDeviceFolder = Nothing

            'copy the PNG...
            Dim fiLeak1Png As New FileInfo(sTempAlgoSessionFolder & "\" & Replace(m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename), "csv", "png"))

            sDestFileName = Format(CLng(drRawFile!ddata_id), "0000000000")
            sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullDate(drRawFile!ddata_DateTimeUtc), "yyyy-MM-dd--HH-mm-ss")
            sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullInt(drRawFile!ddata_RawDataTriggerNumber), "00000000")
            sDestFileName += "_LD1.png"
            Dim fiLeak1PngDest As New FileInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/" & sDestFileName)

            If fiLeak1PngDest.Exists Then
              fiLeak1PngDest.Delete()
            End If
            fiLeak1PngDest = Nothing

            fiLeak1Png.CopyTo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/" & sDestFileName)
            fiLeak1Png = Nothing

            ' we need the Algo score.
            Dim sAlgoScoreFile As String = sTempAlgoSessionFolder & "\AudioScore.csv"
            Dim sAlgoValue As String
            Dim dAlgovalue As Decimal

            Using readerAlgoScore As New Microsoft.VisualBasic.FileIO.TextFieldParser(sAlgoScoreFile, Text.Encoding.GetEncoding("ISO-8859-1"))
              readerAlgoScore.TextFieldType = FileIO.FieldType.Delimited
              readerAlgoScore.SetDelimiters(",")
              Dim sCurrentRow() As String
              Const c_ALGO_NAME As Int32 = 0
              Const c_ALGO_VALUE As Int32 = 1

              While Not readerAlgoScore.EndOfData
                Try
                  sCurrentRow = readerAlgoScore.ReadFields()

                  If sCurrentRow(c_ALGO_NAME) = "getMetricsv2" Then
                    sAlgoValue = sCurrentRow(c_ALGO_VALUE)
                    If IsNumeric(sAlgoValue) Then
                      dAlgovalue = CDec(sAlgoValue)

                      If CInt(drRawFile!ddata_id) <> -1 Then

                        Dim drDevicedata As DataRow
                        drDevicedata = m_oBL_DeviceManager.GetDeviceDataByID(CInt(drRawFile!ddata_id))
                        dicParms.Clear()
                        dicParms.Add("ddata_LeakProbability", "N:" & dAlgovalue)
                        dicParms.Add("ddata_LeakDetectionGraphic", "S:" & sDestFileName)
                        dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDevicedata!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
                        dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDevicedata!ddata_id), dicParms)
                        If dbRet <> DL_Manager.DB_Return.ok Then
                          Throw New Exception("Error updating Device data with Leak Probability")


                        End If

                      End If
                      'Update the rawdata row....
                      Dim dElapsed As Decimal
                      dElapsed = CDec((DateTime.Now - dtStartTime).TotalMilliseconds / 1000)

                      dicParms.Clear()
                      dicParms.Add("rdf_AudioLeak1Status", "N:" & 1)
                      dicParms.Add("rdf_AudioLeak1RunTime", "N:" & dElapsed)
                      dicParms.Add("rdf_AudioLeak1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
                      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
                      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
                      If dbRet = DL_Manager.DB_Return.ok Then
                        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
                      Else
                        Throw New Exception("Error Updating RawDeviceFile Leak1: " & CInt(drRawFile!rdf_ID))
                      End If
                      m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: SUCCESS!!: " & sDestFileName)


                    Else
                      m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: Fail Algo Invalid: " & sAlgoValue)

                    End If


                  End If
                Catch ex As Exception
                  Throw ex
                End Try
              End While
            End Using

          Else
            If sErrorLevel = "ErrorLevel:99" Then
              'corrupt data
              dicParms.Clear()
              dicParms.Add("rdf_AudioLeak1Status", "N:" & 4) 'Invalid data from algo
              dicParms.Add("rdf_AudioLeak1RunTime", "N:" & 0)
              dicParms.Add("rdf_AudioLeak1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
              dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
              dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
              If dbRet = DL_Manager.DB_Return.ok Then
                drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
              End If

            Else
              'Normal error
              m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: Fail ErrorLevel: " & sErrorLevel)
              Dim fiCheckErrorLog As New FileInfo(sTempAlgoSessionFolder & "\Error.Log")
              If fiCheckErrorLog.Exists Then
                fiCheckErrorLevel = Nothing
                Dim srCheckErrorLog As StreamReader
                Dim sErrorLog As String
                'Now, read the entire file into a string
                srCheckErrorLog = File.OpenText(sTempAlgoSessionFolder & "\Error.Log")
                sErrorLog = srCheckErrorLog.ReadToEnd()
                Throw New Exception("zProcessAudioLeak1:" & sErrorLog)
              Else
                Throw New Exception("zProcessAudioLeak1:" & sErrorLevel)
              End If
              fiCheckErrorLevel = Nothing
            End If

          End If

        Else
          m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass2: Fail ErrorLevel Missing ")
        End If

      Else
        'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
        dicParms.Clear()
        If CInt(drRawFile!ddata_id) = -1 Then
          dicParms.Add("rdf_AudioLeak1Status", "N:" & 2) 'Missing DeviceData
        ElseIf CInt(drRawFile!rdf_AudioEnergy) = 0 Then
          dicParms.Add("rdf_AudioLeak1Status", "N:" & 3) 'Audio energy too low.
        Else
          dicParms.Add("rdf_AudioLeak1Status", "N:" & 999) 'Unknown
        End If


        dicParms.Add("rdf_AudioLeak1RunTime", "N:" & 0)
        dicParms.Add("rdf_AudioLeak1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
        dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
        End If
      End If


    Else
      ' zero Audio Energy...
      dicParms.Clear()
      dicParms.Add("rdf_AudioLeak1Status", "N:" & 3) 'Audio energy too low.

      dicParms.Add("rdf_AudioLeak1RunTime", "N:" & 0)
      dicParms.Add("rdf_AudioLeak1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
      End If


    End If






    Return drRet
  End Function
  Private Function zProcessTemperature1(drRawFile As DataRow) As DataRow
    Dim dtStartTime As DateTime = Now()
    Dim drRet As DataRow = Nothing
    Dim bok As Boolean = False
    Dim sRawFile As String
    Dim dTotal1 As Decimal = 0
    Dim dTotal2 As Decimal = 0
    Dim iKounter1 As Int32 = 0
    Dim iKounter2 As Int32 = 0
    Dim iRow As Int32 = 0
    Dim dicParms As New eDictionary
    Dim dAvg1 As Decimal
    Dim dMin1 As Decimal = 999999999
    Dim dMax1 As Decimal = -999999999
    Dim dAvg2 As Decimal
    Dim dMin2 As Decimal = 999999999
    Dim dMax2 As Decimal = -999999999
    Dim dbRet As DL_Manager.DB_Return
    Dim bBusted As Boolean = False
    Dim sTemp As String
    Dim dTemp1 As Decimal
    Dim dTemp2 As Decimal

    Const c_TEMP_1 As Int32 = 0
    Const c_TEMP_2 As Int32 = 1



    If CInt(drRawFile!ddata_id) <> -1 Then

      sRawFile = m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)
      'Diagnostics.Debug.Write("zProcessAudioAverage: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))

      Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(sRawFile, Text.Encoding.GetEncoding("ISO-8859-1"))
        MyReader.TextFieldType = FileIO.FieldType.Delimited
        MyReader.SetDelimiters(",")
        Dim currentRow As String()
        While Not MyReader.EndOfData
          Try
            currentRow = MyReader.ReadFields()
            iRow += 1

            'Get Temperature 1
            sTemp = currentRow(c_TEMP_1)
            If IsNumeric(sTemp) Then
              dTemp1 = CDec(sTemp)
              dTotal1 += dTemp1
              iKounter1 += 1

              If dTemp1 > dMax1 Then
                dMax1 = dTemp1
              End If
              If dTemp1 < dMin1 Then
                dMin1 = dTemp1
              End If

              'range: -43c to +99c
              If dTemp1 > 6000000 Or dTemp1 < -7000000 Then
                bBusted = True
              End If

            End If

            'Get Temperature 2
            sTemp = currentRow(c_TEMP_2)
            If IsNumeric(sTemp) Then
              dTemp2 = CDec(sTemp)
              dTotal2 += dTemp2
              iKounter2 += 1

              If dTemp2 > dMax2 Then
                dMax2 = dTemp2
              End If
              If dTemp2 < dMin2 Then
                dMin2 = dTemp2
              End If

              'range: -43c to +99c
              If dTemp2 > 6000000 Or dTemp2 < -7000000 Then
                bBusted = True
              End If
            End If

            If bBusted Then
              Exit While
            End If

          Catch ex As Exception
            Throw ex
          End Try
        End While
      End Using

      If bBusted = False Then
        'Normal readings
        dAvg1 = CDec(dTotal1 / iKounter1)
        dAvg2 = CDec(dTotal2 / iKounter2)

        If dMin1 = 999999999 Then
          dMin1 = 0
        End If
        If dMin2 = 999999999 Then
          dMin2 = 0
        End If
        If dMax1 = -999999999 Then
          dMax1 = 0
        End If
        If dMax2 = -999999999 Then
          dMax2 = 0
        End If


        Dim drDevicedata As DataRow
        drDevicedata = m_oBL_DeviceManager.GetDeviceDataByID(CInt(drRawFile!ddata_id))
        dicParms.Clear()
        dicParms.Add("ddata_Temp1Min1", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dMin1)))
        dicParms.Add("ddata_Temp1Avg1", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dAvg1)))
        dicParms.Add("ddata_Temp1Max1", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dMax1)))
        dicParms.Add("ddata_Temp1Min2", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dMin2)))
        dicParms.Add("ddata_Temp1Avg2", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dAvg2)))
        dicParms.Add("ddata_Temp1Max2", "N:" & m_oBL_DeviceManager.ConvertPatchTempToC(CInt(dMax2)))

        dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDevicedata!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDevicedata!ddata_id), dicParms)
        If dbRet <> DL_Manager.DB_Return.ok Then
          Throw New Exception("Error updating Device data with Temp1 Data")
        End If


        'Update the rawdata row....
        Dim dElapsed As Decimal
        dElapsed = CDec((DateTime.Now - dtStartTime).TotalMilliseconds / 1000)

        dicParms.Clear()
        dicParms.Add("rdf_Temp1Status", "N:" & 1)
        dicParms.Add("rdf_Temp1RunTime", "N:" & dElapsed)
        dicParms.Add("rdf_Temp1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
        dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
        Else
          Throw New Exception("Error Updating RawDeviceFile Temp1: " & CInt(drRawFile!rdf_ID))
        End If

      Else
        'Data is out of Range
        dicParms.Clear()
        dicParms.Add("rdf_Temp1Status", "N:" & 3) 'Data out of range 
        dicParms.Add("rdf_Temp1RunTime", "N:" & 0)
        dicParms.Add("rdf_Temp1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
        dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
        End If

      End If
    Else
      'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
      dicParms.Clear()
      If CInt(drRawFile!ddata_id) = -1 Then
        dicParms.Add("rdf_Temp1Status", "N:" & 2) 'Missing DeviceData
      Else
        dicParms.Add("rdf_Temp1Status", "N:" & 999) 'Unknown
      End If
      dicParms.Add("rdf_Temp1RunTime", "N:" & 0)
      dicParms.Add("rdf_Temp1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
      End If

    End If




    Return drRet
  End Function
  Private Function zProcessStrain1(drRawFile As DataRow) As DataRow
    Dim dtStartTime As DateTime = Now()
    Dim drRet As DataRow = Nothing
    Dim bok As Boolean = False
    Dim sRawFile As String
    Dim dTotal1 As Decimal = 0
    Dim dTotal2 As Decimal = 0
    Dim iKounter1 As Int32 = 0
    Dim iKounter2 As Int32 = 0
    Dim iRow As Int32 = 0
    Dim dicParms As New eDictionary
    Dim dAvg As Decimal = 0
    Dim dMin As Decimal = 999999999
    Dim dMax As Decimal = -999999999

    Dim dbRet As DL_Manager.DB_Return
    Dim bBusted As Boolean = False
    Dim sStrain As String
    Dim iStrain As Int32

    Const c_Strain_1 As Int32 = 0


    If CInt(drRawFile!ddata_id) <> -1 Then

      sRawFile = m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)
      'Diagnostics.Debug.Write("zProcessAudioAverage: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))

      Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(sRawFile, Text.Encoding.GetEncoding("ISO-8859-1"))
        MyReader.TextFieldType = FileIO.FieldType.Delimited
        MyReader.SetDelimiters(",")
        Dim currentRow As String()
        While Not MyReader.EndOfData
          Try
            currentRow = MyReader.ReadFields()
            iRow += 1

            'Get Stain 1
            sStrain = currentRow(c_Strain_1)
            If IsNumeric(sStrain) Then
              iStrain = CInt(sStrain)
              dTotal1 += iStrain
              iKounter1 += 1

              If iStrain > dMax Then
                dMax = iStrain
              End If
              If iStrain < dMin Then
                dMin = iStrain
              End If

              ''range: -43c to +99c
              'If dTemp1 > 6000000 Or dTemp1 < -7000000 Then
              '  bBusted = True
              'End If

            End If



            If bBusted Then
              Exit While
            End If

          Catch ex As Exception
            Throw ex
          End Try
        End While
      End Using

      If bBusted = False Then
        'Normal readings
        dAvg = CDec(dTotal1 / iKounter1)

        If dMin = 999999999 Then
          dMin = 0
        End If
        If dMax = -999999999 Then
          dMax = 0
        End If


        Dim drDevicedata As DataRow
        drDevicedata = m_oBL_DeviceManager.GetDeviceDataByID(CInt(drRawFile!ddata_id))
        dicParms.Clear()
        dicParms.Add("ddata_StrainOrg", "N:" & CInt(dAvg))
        dicParms.Add("ddata_StrainMin", "N:" & CInt(dMin) + 32768)
        dicParms.Add("ddata_StrainAvg", "N:" & CInt(dAvg) + 32768)
        dicParms.Add("ddata_StrainMax", "N:" & CInt(dMax) + 32768)

        dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDevicedata!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDevicedata!ddata_id), dicParms)
        If dbRet <> DL_Manager.DB_Return.ok Then
          Throw New Exception("Error updating Device data with Strain1 Data")
        End If


        'Update the rawdata row....
        Dim dElapsed As Decimal
        dElapsed = CDec((DateTime.Now - dtStartTime).TotalMilliseconds / 1000)

        dicParms.Clear()
        dicParms.Add("rdf_Strain1Status", "N:" & 1)
        dicParms.Add("rdf_Strain1RunTime", "N:" & dElapsed)
        dicParms.Add("rdf_Strain1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
        dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
        Else
          Throw New Exception("Error Updating RawDeviceFile Strain1: " & CInt(drRawFile!rdf_ID))
        End If

      Else
        'Data is out of Range
        dicParms.Clear()
        dicParms.Add("rdf_Strain1Status", "N:" & 3) 'Data out of range 
        dicParms.Add("rdf_Strain1RunTime", "N:" & 0)
        dicParms.Add("rdf_Strain1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
        dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
        If dbRet = DL_Manager.DB_Return.ok Then
          drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
        End If

      End If
    Else
      'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
      dicParms.Clear()
      If CInt(drRawFile!ddata_id) = -1 Then
        dicParms.Add("rdf_Strain1Status", "N:" & 2) 'Missing DeviceData
      Else
        dicParms.Add("rdf_Strain1Status", "N:" & 999) 'Unknown
      End If
      dicParms.Add("rdf_Strain1RunTime", "N:" & 0)
      dicParms.Add("rdf_Strain1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
      End If

    End If

    Return drRet
  End Function

  Private Function zProcessPipeCondition1(drRawFile As DataRow, sTempAlgoFolder As String, sSessionFolderStub As String, sTempAlgoSessionFolder As String) As DataRow
    Dim drRet As DataRow = Nothing
    Dim bok As Boolean = False
    Dim dbRet As DL_Manager.DB_Return
    Dim dtStartTime As DateTime = Now()
    Dim dicParms As New eDictionary
    Dim sDestFileName As String


    'Ok, let's Check the session folder and clear it required....
    m_pcl_ErrManager.LogError("zProcessAudioLeak1, sTempAlgoSessionFolder: " & sTempAlgoSessionFolder)
    m_oBL_DeviceManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)


    If CInt(drRawFile!ddata_id) <> -1 Then
      'OK, we have matching device data, let's process.....

      'Copy Files into folder....
      'Leak BaseLine

      'File to Compare
      Dim fiAudioCompare As New FileInfo(m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))
      fiAudioCompare.CopyTo(sTempAlgoSessionFolder & "\" & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))

      m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass6: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))


      Dim sFolder As String
      Dim sbatchFileLocation As String

      sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
      sbatchFileLocation = sFolder & "RunPipeCondition1.bat"


      Dim oProcess As New Process
      oProcess.StartInfo.FileName = sbatchFileLocation
      '    oProcess.StartInfo.Arguments = " .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

      'oProcess.StartInfo.UserName = "53FF8F5\AdministratorDS"
      'oProcess.StartInfo.Password = m_pcl_Utils.ConvertToSecureString("Lancster!1979")
      oProcess.StartInfo.Arguments = " .\" & sSessionFolderStub & "\ " & " " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)

      oProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(sbatchFileLocation)
      oProcess.StartInfo.UseShellExecute = False

      oProcess.Start()
      oProcess.WaitForExit()

      Dim fiCheckErrorLevel As New FileInfo(sTempAlgoSessionFolder & "\ErrorLevel.Log")
      If fiCheckErrorLevel.Exists Then
        fiCheckErrorLevel = Nothing
        Dim sfileCheckErrorLevel As String = sTempAlgoSessionFolder & "\ErrorLevel.Log"
        Dim srCheckErrorLevel As StreamReader
        Dim sErrorLevel As String

        srCheckErrorLevel = File.OpenText(sfileCheckErrorLevel)
        'Now, read the entire file into a string
        sErrorLevel = srCheckErrorLevel.ReadToEnd()
        sErrorLevel = Replace(sErrorLevel, vbCrLf, "")
        'There is an odd char at the end of the error level file, just removing it.
        sErrorLevel = Left(sErrorLevel, Len(sErrorLevel) - 1)

        srCheckErrorLevel.Close()

        If sErrorLevel = "ErrorLevel:0" Then
          'we have a winner!!!!
          'Check the post processed folder....
          Dim diProcessedDeviceFolder As New DirectoryInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/")
          If diProcessedDeviceFolder.Exists = False Then
            diProcessedDeviceFolder.Create()
          End If
          diProcessedDeviceFolder = Nothing

          'copy the PNG...
          Dim fiLeak1Png As New FileInfo(sTempAlgoSessionFolder & "\" & Replace(m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename), "csv", "png"))

          sDestFileName = Format(CLng(drRawFile!ddata_id), "0000000000")
          sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullDate(drRawFile!ddata_DateTimeUtc), "yyyy-MM-dd--HH-mm-ss")
          sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullInt(drRawFile!ddata_RawDataTriggerNumber), "00000000")
          sDestFileName += "_PC1.png"
          Dim fiLeak1PngDest As New FileInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/" & sDestFileName)

          If fiLeak1PngDest.Exists Then
            fiLeak1PngDest.Delete()
          End If
          fiLeak1PngDest = Nothing

          fiLeak1Png.CopyTo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drRawFile!dev_id), "00000000") & "/" & sDestFileName)
          fiLeak1Png = Nothing




          Dim drDevicedata As DataRow
          drDevicedata = m_oBL_DeviceManager.GetDeviceDataByID(CInt(drRawFile!ddata_id))
          dicParms.Clear()
          'dicParms.Add("ddata_LeakProbability", "N:" & dAlgovalue)
          dicParms.Add("ddata_PipeConditionGraphic", "S:" & sDestFileName)
          dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDevicedata!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
          dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDevicedata!ddata_id), dicParms)
          If dbRet <> DL_Manager.DB_Return.ok Then
            Throw New Exception("Error updating Device data with Pipe Condition Graphic")
          End If

          'Update the rawdata row....
          Dim dElapsed As Decimal
          dElapsed = CDec((DateTime.Now - dtStartTime).TotalMilliseconds / 1000)

          dicParms.Clear()
          dicParms.Add("rdf_PipeCond1Status", "N:" & 1)
          dicParms.Add("rdf_PipeCond1RunTime", "N:" & dElapsed)
          dicParms.Add("rdf_PipeCond1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
          dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
          dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
          If dbRet = DL_Manager.DB_Return.ok Then
            drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
          Else
            Throw New Exception("Error Updating RawDeviceFile Leak1: " & CInt(drRawFile!rdf_ID))
          End If
          m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass6 - zProcessPipeCondition1: SUCCESS!!: " & sDestFileName)


        Else
          m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass6 - zProcessPipeCondition1: Fail ErrorLevel: " & sErrorLevel)
          Dim fiCheckErrorLog As New FileInfo(sTempAlgoSessionFolder & "\Error.Log")
          If fiCheckErrorLog.Exists Then
            fiCheckErrorLevel = Nothing
            Dim srCheckErrorLog As StreamReader
            Dim sErrorLog As String
            'Now, read the entire file into a string
            srCheckErrorLog = File.OpenText(sTempAlgoSessionFolder & "\Error.Log")
            sErrorLog = srCheckErrorLog.ReadToEnd()
            Throw New Exception("zProcessPipeCondition1:" & sErrorLog)

          Else
            Throw New Exception("zProcessPipeCondition1:" & sErrorLevel)
          End If
          fiCheckErrorLevel = Nothing

        End If

      Else
        m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass6 - zProcessPipeCondition1: Fail ErrorLevel Missing ")
      End If

    Else
      'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
      dicParms.Clear()
      If CInt(drRawFile!ddata_id) = -1 Then
        dicParms.Add("rdf_PipeCond1Status", "N:" & 2) 'Missing DeviceData
      Else
        dicParms.Add("rdf_PipeCond1Status", "N:" & 999) 'Unknown
      End If


      dicParms.Add("rdf_PipeCond1RunTime", "N:" & 0)
      dicParms.Add("rdf_PipeCond1Processed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
      End If

    End If


    Return drRet
  End Function

  Private Function zProcessAudioEnergy(drRawFile As DataRow, sTempAlgoFolder As String, sSessionFolderStub As String, sTempAlgoSessionFolder As String) As DataRow
    Dim drRet As DataRow = Nothing
    Dim bok As Boolean = False
    Dim dbRet As DL_Manager.DB_Return
    Dim dtStartTime As DateTime = Now()
    Dim dicParms As New eDictionary


    'Ok, let's Check the session folder and clear it required....
    m_pcl_ErrManager.LogError("zProcessAudioEnergy, sTempAlgoSessionFolder: " & sTempAlgoSessionFolder)
    m_oBL_DeviceManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)


    If CInt(drRawFile!ddata_id) <> -1 And Not IsDBNull(drRawFile!rdf_ExtractedFilename) Then
      'OK, we have matching device data, let's process.....

      'File to Compare
      Dim fiAudioEnergy As New FileInfo(m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))
      fiAudioEnergy.CopyTo(sTempAlgoSessionFolder & "\" & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))

      m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename))


      Dim sFolder As String
      Dim sbatchFileLocation As String

      sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
      sbatchFileLocation = sFolder & "RunAudioEnergy.bat"


      Dim oProcess As New Process
      oProcess.StartInfo.FileName = sbatchFileLocation
      '    oProcess.StartInfo.Arguments = " .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

      oProcess.StartInfo.Arguments = "  .\" & sSessionFolderStub & "\ " & m_oDB_Layer.CheckDBNullStr(drRawFile!rdf_ExtractedFilename)

      oProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(sbatchFileLocation)
      oProcess.StartInfo.UseShellExecute = False

      oProcess.Start()
      oProcess.WaitForExit()

      Dim fiCheckErrorLevel As New FileInfo(sTempAlgoSessionFolder & "\ErrorLevel.Log")
      If fiCheckErrorLevel.Exists Then
        fiCheckErrorLevel = Nothing
        Dim sfileCheckErrorLevel As String = sTempAlgoSessionFolder & "\ErrorLevel.Log"
        Dim srCheckErrorLevel As StreamReader
        Dim sErrorLevel As String

        srCheckErrorLevel = File.OpenText(sfileCheckErrorLevel)
        'Now, read the entire file into a string
        sErrorLevel = srCheckErrorLevel.ReadToEnd()
        sErrorLevel = Replace(sErrorLevel, vbCrLf, "")
        'There is an odd char at the end of the error level file, just removing it.
        sErrorLevel = Left(sErrorLevel, Len(sErrorLevel) - 1)

        srCheckErrorLevel.Close()

        If sErrorLevel = "ErrorLevel:0" Then
          'we have a winner!!!!

          ' we need the Algo score.
          Dim sAlgoScoreFile As String = sTempAlgoSessionFolder & "\energyvalues.csv"
          Dim sAlgoValue As String
          Dim dAlgovalue As Decimal

          Using readerAlgoScore As New Microsoft.VisualBasic.FileIO.TextFieldParser(sAlgoScoreFile, Text.Encoding.GetEncoding("ISO-8859-1"))
            readerAlgoScore.TextFieldType = FileIO.FieldType.Delimited
            readerAlgoScore.SetDelimiters(",")
            Dim sCurrentRow() As String
            Const c_ALGO_NAME As Int32 = 0
            Const c_ALGO_VALUE As Int32 = 1

            While Not readerAlgoScore.EndOfData
              Try
                sCurrentRow = readerAlgoScore.ReadFields()

                If sCurrentRow(c_ALGO_NAME) = "energyValues.py" Then
                  sAlgoValue = sCurrentRow(c_ALGO_VALUE)
                  If IsNumeric(sAlgoValue) Then
                    dAlgovalue = CDec(sAlgoValue)


                    'Update the rawdata row....
                    Dim dElapsed As Decimal
                    dElapsed = CDec((DateTime.Now - dtStartTime).TotalMilliseconds / 1000)

                    dicParms.Clear()
                    dicParms.Add("rdf_AudioEnergy", "N:" & sAlgoValue)
                    dicParms.Add("rdf_AudioEnergyStatus", "N:" & 1)
                    dicParms.Add("rdf_AudioEnergyRunTime", "N:" & dElapsed)
                    dicParms.Add("rdf_AudioEnergyProcessed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
                    dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
                    dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
                    If dbRet = DL_Manager.DB_Return.ok Then
                      drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
                    Else
                      Throw New Exception("Error Updating RawDeviceFile Leak1: " & CInt(drRawFile!rdf_ID))
                    End If
                    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: SUCCESS!!: ")


                  Else
                    m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: Fail Algo Invalid: " & sAlgoValue)

                  End If


                End If
              Catch ex As Exception
                Throw ex
              End Try
            End While
          End Using

        Else
          m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: Fail ErrorLevel: " & sErrorLevel)
          Dim fiCheckErrorLog As New FileInfo(sTempAlgoSessionFolder & "\Error.Log")
          If fiCheckErrorLog.Exists Then
            fiCheckErrorLevel = Nothing
            Dim srCheckErrorLog As StreamReader
            Dim sErrorLog As String
            'Now, read the entire file into a string
            srCheckErrorLog = File.OpenText(sTempAlgoSessionFolder & "\Error.Log")
            sErrorLog = srCheckErrorLog.ReadToEnd()
            Throw New Exception("zProcessAudioEnergy:" & sErrorLog)

          Else
            Throw New Exception("zProcessAudioEnergy:" & sErrorLevel)
          End If
          fiCheckErrorLevel = Nothing

        End If

      Else
        m_pcl_ErrManager.LogError("ProcessBufferToData > zProcessPostProcessing: Pass1: Fail ErrorLevel Missing ")
      End If

    Else
      'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
      dicParms.Clear()
      If CInt(drRawFile!ddata_id) = -1 Then
        dicParms.Add("rdf_AudioEnergyStatus", "N:" & 2) 'Missing DeviceData
      ElseIf IsDBNull(drRawFile!rdf_ExtractedFilename) Then
        dicParms.Add("rdf_AudioEnergyStatus", "N:" & 3) '-1 file!!
      Else
        dicParms.Add("rdf_AudioEnergyStatus", "N:" & 999) 'Unknown
      End If
      dicParms.Add("rdf_AudioEnergyRunTime", "N:" & 0)
      dicParms.Add("rdf_AudioEnergyProcessed", "D:" & Format(Now().ToUniversalTime, "yyyy-MM-dd HH:mm:ss"))
      dicParms.Add("rdf_TimeStamp", "T:" & Format(CDate(drRawFile!rdf_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
      dbRet = m_oBL_DeviceManager.UpdateRawDeviceFile(CInt(drRawFile!rdf_ID), dicParms)
      If dbRet = DL_Manager.DB_Return.ok Then
        drRet = m_oBL_DeviceManager.GetRawDeviceFileByID(CInt(drRawFile!rdf_ID))
      End If

    End If


    Return drRet
  End Function










#End Region

End Class