Imports System.Net.Mail

Public Class TestIOT
  Inherits System.Web.UI.Page

  Dim m_dtStartTime As Date = Now()

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Diagnostics.Debug.WriteLine("Test IOT Start: " & Format(m_dtStartTime, "HH:mm:ss.fff"))

    LogError("Start IOT Test request, From: " & Request.UserHostAddress.ToString)


    If Not Page.IsPostBack Then

      lblText.Text = ""
      Label1.Text = ""

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



      sBody += " Querystring: " & Request.QueryString.ToString & vbCrLf
      sBody += " From IP: " & Request.UserHostAddress.ToString & vbCrLf
      sBody += " Call Time: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss.ff") & " (UTC)" & vbCrLf & vbCrLf



      sFrom = "streamline@relay.horizon-it.co.uk"
      If InStr(Request.Url.ToString, "localhost") = 0 Then
        sTo = "peto@orbis-sys.com;prodigy1@orbis-sys.com"
        sCC = "paul.craven@horizon-it.co.uk"
      Else
        sTo = "paul.craven@horizon-it.co.uk"
        sCC = ""
      End If

      sSubject = "IOT Test Page Called: " & Format(DateTime.UtcNow, "yyyy-MM-dd HH:mm:ss") & " (UTC)"
      If Request.QueryString.Count = 0 Then
        sBody += " No Querystring data on request."
        LogError(" No Querystring data on request.")
      Else
        'sBody += " Querystring: " & Request.QueryString.ToString

        LogError("Querystring: " & Request.QueryString.ToString)

        'Parse the query strings....
        sBody += " Parse QueryString data:" & vbCrLf

        For Each nvKey In Request.QueryString

          If dicProdigyParams.KeyExists(UCase(nvKey)) Then
            sVarName = "  " & dicProdigyParams(nvKey) & " (" & nvKey & "): "
          Else
            sVarName = "  " & nvKey & ": " & vbTab & vbTab
          End If

          sVarName += "                                 "
          sVarName = Left(sVarName, 30)
          sBody += sVarName

          sVarValue = Request.QueryString(nvKey)
          sVarValue += "                                "
          sVarValue = Left(sVarValue, 20)

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






        Next



      End If

      lblText.Text = Replace(sBody, vbCrLf, "<br/>")

      zSendEmail(sFrom, sTo, sCC, "", sSubject, sBody)

      LogError("---------------------------------------------------------------------------------------------------")

    End If
  End Sub



  Public Function zSendEmail(ByVal sFrom As String, ByVal sTo As String, ByVal sCC As String,
             ByVal sBCC As String, ByVal sSubject As String, ByVal sBody As String,
             Optional ByVal bIsHTML As Boolean = False, Optional ByVal sEmailID As String = "",
             Optional ByVal dicParms As eDictionary = Nothing, Optional ByVal sAttachFile As String = "") As Boolean

    Dim objEmail As New System.Net.Mail.MailMessage()
    Dim oSmtpClient As SmtpClient
    Dim sDests() As String = sTo.Split(";".ToCharArray)

    If sAttachFile.Trim <> "" Then
      objEmail.Attachments.Add(New Attachment(sAttachFile))
    End If
    Dim i As Int32
    With objEmail
      For i = 0 To sDests.GetUpperBound(0)
        .To.Add(New MailAddress(sDests(i)))
      Next
      .From = New MailAddress(sFrom)
      If sCC <> "" Then
        .CC.Add(New MailAddress(sCC))
      End If
      If sBCC <> "" Then
        .Bcc.Add(New MailAddress(sBCC))
      End If
      .Subject = sSubject
      .Body = sBody
      .Priority = MailPriority.Normal
      .IsBodyHtml = bIsHTML
    End With

    oSmtpClient = New SmtpClient("ds3b.horizon-it.co.uk", 25)
    oSmtpClient.Credentials = New System.Net.NetworkCredential("streamline@relay.horizon-it.co.uk", "Swimmer1024!")

    Try
      oSmtpClient.Send(objEmail)
      Return True

    Catch localexception As Exception
      Diagnostics.Debug.WriteLine("Email error send problem: " & localexception.Message)
      If Not IsDBNull(localexception.InnerException) Then
        LogError("Email error send inner problem: " & localexception.InnerException.Message)
      End If
      Return False

    End Try


  End Function

  Public Sub LogError(ByVal message As String, Optional ByVal pInnerException As Exception = Nothing)

    ' Get the current HTTPContext
    Dim context As HttpContext = HttpContext.Current

    ' Get location of ErrorLogFile from Web.config fie
    Dim sPath As String = CStr(System.Configuration.ConfigurationManager.AppSettings("ErrorLogFile"))
    sPath = sPath.Replace("%date%", Format(Now, "yyyy-MM-dd"))
    Dim filePath As String = context.Server.MapPath("~") & "\" & sPath

    'Dim filePath As String = context.Server.MapPath(sPath)

    ' Calculate GMT offset
    Dim gmtOffset As Integer = DateTime.Compare(DateTime.Now, DateTime.UtcNow)

    Dim gmtPrefix As String
    If gmtOffset > 0 Then
      gmtPrefix = "+"
    Else
      gmtPrefix = ""
    End If

    ' Create DateTime string
    Dim errorDateTime As String =
      DateTime.Now.Year.ToString & "." & DateTime.Now.Month.ToString("00") & "." & DateTime.Now.Day.ToString("00") _
      & " @ " &
      DateTime.Now.Hour.ToString("00") & ":" & DateTime.Now.Minute.ToString("00") & ":" & DateTime.Now.Second.ToString("00") _
      & "." & DateTime.Now.Millisecond.ToString("000") _
      & " (GMT " & gmtPrefix & gmtOffset.ToString & ")"

    ' Write message to error file
    Dim sw As System.IO.StreamWriter = Nothing
    Try
      sw = New System.IO.StreamWriter(filePath, True)
      sw.WriteLine(errorDateTime & " - " & message)
      If Not pInnerException Is Nothing Then
        With pInnerException
          sw.WriteLine(errorDateTime & " - " & .StackTrace())
        End With
      End If
    Catch ex As Exception
      ' If error writing to file, simply continue
      Dim a As Int32 = 0

    Finally
      If Not sw Is Nothing Then
        sw.Close()
      End If

    End Try


  End Sub



End Class
