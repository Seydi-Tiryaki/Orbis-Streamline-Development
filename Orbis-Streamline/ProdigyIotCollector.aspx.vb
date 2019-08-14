Public Class ProdigyIotCollector
  Inherits System.Web.UI.Page

  Dim m_odl_Manager As DL_Manager
  Dim m_ocl_ErrManager As CL_ErrManager
  Dim m_oDB_Layer As DB_LayerGeneric
  Dim m_ocl_Utils As CL_Utils
  Dim m_lBufferID As Long



  Dim m_bTest As Boolean

  Dim m_dtStartTime As Date = Now()
  Dim m_dtUtc As Date = DateTime.UtcNow


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Diagnostics.Debug.WriteLine("Test IOT Start: " & Format(m_dtStartTime, "HH:mm:ss.fff"))

    zSetupObjects()

    m_ocl_ErrManager.LogError("Start ProdigyIotCollector request, From: " & Request.UserHostAddress.ToString)

    If Not Page.IsPostBack Then

      If Request.QueryString("pinger") Is Nothing Then

        lblText.Text = ""
        Label1.Text = ""

        zInsertDataToBuffer()

        Dim sFrom As String
        Dim sTo As String
        Dim sCC As String
        Dim sSubject As String
        Dim sBody As String = " Host: " & Request.Url.Host & vbCrLf
        Dim dicProdigyParams As New eDictionary
        Dim sVarName As String
        Dim sVarValue As String

        dicProdigyParams.Add("DN", "PRM_REQ_COMPLETE")
        dicProdigyParams.Add("FE", "PRM_FLOW_EVENT")
        dicProdigyParams.Add("FT", "PRM_FLOW_EVENT_TRIGGER")
        dicProdigyParams.Add("FM", "PRM_FLOW_MEAS")
        dicProdigyParams.Add("FC", "PRM_FLOW_CONF")
        dicProdigyParams.Add("AD", "PRM_AUD_LEAK")
        dicProdigyParams.Add("AC", "PRM_AUD_LEAK_CONF")
        dicProdigyParams.Add("AP", "PRM_AUD_PIPECOND")
        dicProdigyParams.Add("PC", "PRM_AUD_PIPECOND_CONF")
        dicProdigyParams.Add("BT", "PRM_BOARD_TEMP")
        dicProdigyParams.Add("AT", "PRM_AMB_TEMP")
        dicProdigyParams.Add("VR", "PRM_FW_VERSION")
        dicProdigyParams.Add("TR", "PRM_TRIGGER_TYPE")
        dicProdigyParams.Add("CL", "PRM_CORD_LEAK")
        dicProdigyParams.Add("A0", "ADM_TYPE")
        dicProdigyParams.Add("A1", "ADM_IMEI")
        dicProdigyParams.Add("A2", "ADM_DATE")
        dicProdigyParams.Add("A3", "ADM_TIME")
        dicProdigyParams.Add("A4", "ADM_RSSI")
        dicProdigyParams.Add("A5", "ADM_BATTERY")
        dicProdigyParams.Add("A6", "ADM_TEMPERATURE")
        dicProdigyParams.Add("A7", "ADM_PIC18_FWVERS")
        dicProdigyParams.Add("A8", "ADM_PIC32_FWVERS")
        dicProdigyParams.Add("A9", "ADM_MSG_COUNT")
        dicProdigyParams.Add("B0", "ADM_OTA_ATTEMPTS")
        dicProdigyParams.Add("B1", "ADM_OTA_SUCCESS")
        dicProdigyParams.Add("RF", "PRM_DATA_FILE")
        dicProdigyParams.Add("TN", "PRM_TRIGGER_NUM")
        dicProdigyParams.Add("EOL", "ADM_END_OF_LINE")

        sBody += " Querystring: " & Request.QueryString.ToString & vbCrLf
        sBody += " From IP: " & Request.UserHostAddress.ToString & vbCrLf
        sBody += " Buffer ID: " & m_lBufferID & vbCrLf
        sBody += " Call Time: " & Format(m_dtUtc, "yyyy-MM-dd HH:mm:ss.ff") & " (UTC)" & vbCrLf & vbCrLf

        sFrom = "streamline@relay.horizon-it.co.uk"
        If InStr(Request.Url.ToString, "localhost") = 0 Then
          sTo = "peto@orbis-sys.com;prodigy1@orbis-sys.com"
          sCC = "paul.craven@horizon-it.co.uk"
          sSubject = "IOT ProdigyIotCollector page Called: " & Format(m_dtUtc, "yyyy-MM-dd HH:mm:ss") & " (UTC)"
        Else
          sTo = "paul.craven@horizon-it.co.uk"
          sCC = ""
          sSubject = "IOT ProdigyIotCollector TEST page Called: " & Format(m_dtUtc, "yyyy-MM-dd HH:mm:ss") & " (UTC)"
        End If

        If Request.QueryString.Count = 0 Then
          sBody += " No Querystring data on request."
          m_ocl_ErrManager.LogError("No Querystring data on request.")
        Else
          'sBody += " Querystring: " & Request.QueryString.ToString
          m_ocl_ErrManager.LogError("  Querystring: " & Request.QueryString.ToString)

          'Parse the query strings....
          sBody += " Parse QueryString data:" & vbCrLf

          For Each nvKey In Request.QueryString
            sVarValue = ""
            sVarName = ""

            If nvKey IsNot Nothing Then
              If dicProdigyParams.KeyExists(UCase(nvKey)) Then
                sVarName = "  " & dicProdigyParams(nvKey) & " (" & nvKey & "): "
              Else
                sVarName = "  " & nvKey & ": " & vbTab
              End If

              sVarName += "                                 "
              sVarName = Left(sVarName, 30)
              sBody += sVarName

              sVarValue = Request.QueryString.Item(nvKey)
              sVarValue += "                                "
              sVarValue = Left(sVarValue, 30)

              sBody += sVarValue

              Select Case True
                Case Len(Trim(Request.QueryString(nvKey))) = 0
                  sBody += vbTab & vbTab & "(Empty)"

                Case IsNumeric(Request.QueryString(nvKey))
                  sBody += vbTab & vbTab & "(Numeric)"

                Case Else
                  sBody += vbTab & vbTab & "(String)"

              End Select

              sBody += vbCrLf

            Else

              sBody += "Missing Querystring Key " & vbCrLf
            End If


          Next


        End If

        lblText.Text = Replace(sBody, vbCrLf, "<br/>")

        m_odl_Manager.SendEmail(sFrom, sTo, sCC, "", sSubject, sBody)

        m_ocl_ErrManager.LogError("---------------------------------------------------------------------------------------------------")
      Else
        'This is a ping check!!
        zProcessPingCheck()

      End If

    End If
  End Sub
  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    'Shutdown the data layer...

    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    Else
      m_oDB_Layer = Nothing
    End If
    System.Diagnostics.Debug.WriteLine("Order List Created in " & Format(Now().Subtract(m_dtStartTime).TotalMilliseconds / 1000, "###0.00") & " seconds")

  End Sub
#Region "Private Routines"
  Private Sub zSetupObjects()
    Dim Var1 As String
    Dim Var2 As String
    Dim sConn As String
    Dim Var3 As String
    Dim Var4 As String

    Dim sLocation As String
    Dim bTest As Boolean

    'Create Utils...
    m_ocl_Utils = New CL_Utils()
    m_ocl_Utils.pPage = Page


    sLocation = CStr(System.Configuration.ConfigurationManager.AppSettings("Location")).ToLower
    bTest = CStr(System.Configuration.ConfigurationManager.AppSettings("TestOrLive")) = "test"

    If sLocation = "horizon" Then
      If bTest Then
        'Selvatico Test Database....
        Var1 = "SERVER=hit-server05;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
        Var2 = "DATABASE=OrbisStreamLine;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))
        Var3 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var3")))
        Var4 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var4")))
      Else
        'Live from locahost dev!!!!
        Var1 = "SERVER=88.208.217.104;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
        Var2 = "DATABASE=OrbisStreamLine;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

        Var3 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3ML")))
        Var4 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4ML")))
      End If
    Else
      If bTest Then
        'Remote and test
        Var1 = "SERVER=88.208.217.104;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
        Var2 = "DATABASE=OrbisStreamLine-test;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

        Var3 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3MT")))
        Var4 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4MT")))
      Else
        'Live!!!
        Var1 = "SERVER=127.0.0.1;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
        Var2 = "DATABASE=OrbisStreamLine;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

        Var3 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3ML")))
        Var4 = m_ocl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4ML")))

      End If
    End If

    sConn = Var1 & Var2 & Var3 & "; " & Var4 & ";" & "pooling=true;Max Pool Size=1000; CharSet=utf8mb4"


    'Restore dl_manager
    m_odl_Manager = New DL_Manager()
    m_ocl_ErrManager = New CL_ErrManager()
    m_oDB_Layer = New DB_LayerGeneric(sConn, m_ocl_ErrManager)

  End Sub

  Private Sub zInsertDataToBuffer()
    Dim dicParms As New eDictionary
    Dim sSQL As String
    Dim iRowsAffected As Int32

    If Request.QueryString("OriginatorIP") IsNot Nothing Then
      dicParms.Add("buf_IpAddress", "S:" & Request.QueryString("OriginatorIP"))
    Else
      dicParms.Add("buf_IpAddress", "S:" & Request.UserHostAddress)
    End If



    dicParms.Add("buf_InsertDateTimeUtc", "D:" & Format(m_dtUtc, "yyyy-MM-dd HH:mm:ss"))
    dicParms.Add("buf_CalledUrl", "S:" & Request.Url.Host)
    dicParms.Add("buf_CalledPage", "S:" & Request.Path)
    dicParms.Add("buf_QueryString", "S:" & Request.QueryString.ToString)

    sSQL = m_oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "iotbuffer", dicParms)

    m_lBufferID = m_oDB_Layer.RunSqlNonQuery(sSQL, iRowsAffected, True, "iotbuffer", dicParms)
    If iRowsAffected > 0 Then
      'All ok

    Else
      'Insert failed.
      m_ocl_ErrManager.LogError("ProdigyIotCollector, zInsertDataToBuffer Failed: Rows Affected 0")

    End If

  End Sub

  Private Sub zProcessPingCheck()
    Dim sSql As String
    Dim drTemp As DataRow

    sSql = "Select count(*) as kounter from devicedata"

    drTemp = m_oDB_Layer.RunSqlToDataRow(sSql)

    lblText.Text = "Ping OK! Counter: " & m_oDB_Layer.CheckDBNullStr(drTemp!kounter)

  End Sub

#End Region




#Region "Public Routines"


#End Region

End Class