Imports System.Net.Mail

<Serializable> Public Class CL_ErrManager

  Private m_sExceptions() As String
  Private m_sDesc As String
  Private m_sTemp As String
  Private m_sServerName As String

  Public Property ServerName() As String
    Get
      ServerName = m_sServerName
    End Get
    Set(ByVal Value As String)
      m_sServerName = Value
    End Set
  End Property

  Public Sub AddError(ByVal pException As Exception, Optional ByVal bTopLevel As Boolean = False)
    If m_sExceptions Is Nothing Then
      ReDim m_sExceptions(0)
      If Not HttpContext.Current.Session Is Nothing Then
        m_sDesc = "Session: " & HttpContext.Current.Session.SessionID & vbCrLf
        If Not HttpContext.Current.Session("DeviceBasketRef") Is Nothing Then
          m_sDesc += "Basket Ref:: " & CStr(HttpContext.Current.Session("DeviceBasketRef")) & vbCrLf
        End If

        If Not HttpContext.Current.Session("bl_Client") Is Nothing Then
          Dim pbl_Client As BL_Client = CType(HttpContext.Current.Session("bl_Client"), BL_Client)
          m_sDesc += "Client: " & pbl_Client.ClientID & ", " & pbl_Client.CompanyName & vbCrLf
        End If
        If Not HttpContext.Current.Session("bl_Operator") Is Nothing Then
          Dim pbl_Op As BL_Operator = CType(HttpContext.Current.Session("bl_Operator"), BL_Operator)
          m_sDesc += "Operator: " & pbl_Op.ID & ", " & pbl_Op.Name & vbCrLf
        End If
      Else
        m_sDesc = "Session: Nothing" & vbCrLf
      End If
      m_sDesc += "IP: " & HttpContext.Current.Request.UserHostAddress & vbCrLf
      m_sDesc += "Raw URL: " & HttpContext.Current.Request.RawUrl & vbCrLf

      If Not HttpContext.Current.Request.Browser Is Nothing Then
        Dim bcClient As HttpBrowserCapabilities = HttpContext.Current.Request.Browser

        m_sDesc += "User Agent: " & HttpContext.Current.Request.UserAgent & vbCrLf
        If HttpContext.Current.Request.UserAgent <> "" Then
          m_sDesc += "Operating System: " & GetOS() & vbCrLf
        End If
      End If

      m_sDesc += "-----------------------------------------------" & vbCrLf



      m_sDesc += pException.Message
    Else
      ReDim Preserve m_sExceptions(m_sExceptions.GetUpperBound(0) + 1)
    End If
    m_sExceptions(m_sExceptions.GetUpperBound(0)) = pException.StackTrace
    If bTopLevel Then
      LogError("---------------------------------------------------------------------------------------------")
      LogError("Error: " & m_sDesc)
      Dim sExp As String
      Dim sAllMsg As String = ""
      For Each sExp In m_sExceptions
        If Not sExp Is Nothing Then
          Dim sMsg As String
          Dim iLoc As Int32
          iLoc = InStr(sExp, vbCrLf) + 2
          If iLoc > 0 Then
            sMsg = sExp.Substring(iLoc).Trim
          Else
            sMsg = sExp.Trim
          End If
          LogError(sMsg)
          sAllMsg += sMsg & vbCrLf & vbCrLf
        End If
      Next
      sAllMsg = m_sDesc & vbCrLf & sAllMsg
      If m_sServerName <> "localhost" Then
        zSendEmail("streamline@relay.horizon-it.co.uk", "paul.craven@horizon-it.co.uk", "", "", "Orbis StreamLine Error (" & m_sServerName & ").... ", sAllMsg, False)
      Else
        zSendEmail("streamline@relay.horizon-it.co.uk", "paul.craven@horizon-it.co.uk", "", "", "Orbis StreamLine Error (" & m_sServerName & ").... ", sAllMsg, False)
      End If
    End If
  End Sub
  Public Function ErrorDetails() As String
    'Output the exception stack as string....
    Dim sOut As String
    Dim sExp As String

    sOut = "Error: " & Replace(m_sDesc, vbCrLf, "<br/>") & "<br/>"
    If m_sExceptions IsNot Nothing Then
      For Each sExp In m_sExceptions
        If Not sExp Is Nothing Then
          Dim sMsg As String
          Dim iLoc As Int32
          iLoc = InStr(sExp, vbCrLf) + 2
          If iLoc > 0 Then
            sMsg = sExp.Substring(iLoc).Trim
          Else
            sMsg = sExp.Trim
          End If
          sOut += sMsg & "<br/>"
        End If
      Next
    End If

    Return sOut
  End Function
  Public Sub LogError(ByVal message As String, Optional ByVal pInnerException As Exception = Nothing)

    ' Get the current HTTPContext
    Dim context As HttpContext = HttpContext.Current
    Dim sProcessID As String

    sProcessID = System.Diagnostics.Process.GetCurrentProcess.Id.ToString

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
    Dim errorDateTime As String = _
      DateTime.Now.Year.ToString & "." & DateTime.Now.Month.ToString("00") & "." & DateTime.Now.Day.ToString("00") _
      & " @ " & _
      DateTime.Now.Hour.ToString("00") & ":" & DateTime.Now.Minute.ToString("00") & ":" & DateTime.Now.Second.ToString("00") _
      & "." & DateTime.Now.Millisecond.ToString("000") _
      & " (GMT " & gmtPrefix & gmtOffset.ToString & ")"

    ' Write message to error file
    Dim sw As System.IO.StreamWriter = Nothing
    Try
      sw = New System.IO.StreamWriter(filePath, True)
      sw.WriteLine(errorDateTime & " - " & sProcessID & " - " & message)
      If Not pInnerException Is Nothing Then
        With pInnerException
          sw.WriteLine(errorDateTime & " - " & .StackTrace())
        End With
      End If
    Catch ex As Exception
      ' If error writing to file, simply continue
      Dim a As Int32 = 0
      Diagnostics.Debug.WriteLine("Error Writing to Log File:" & filePath & ", Error: " & ex.Message)

    Finally
      If Not sw Is Nothing Then
        sw.Close()
      End If

    End Try
  

  End Sub
  Public Function zSendEmail(ByVal sFrom As String, ByVal sTo As String, ByVal sCC As String, _
             ByVal sBCC As String, ByVal sSubject As String, ByVal sBody As String, _
             Optional ByVal bIsHTML As Boolean = False, Optional ByVal sEmailID As String = "", _
             Optional ByVal dicParms As eDictionary = Nothing, Optional ByVal sAttachFile As String = "") As Boolean

    Dim objEmail As New System.Net.Mail.MailMessage()
    Dim oSmtpClient As SmtpClient
    Dim sDests() As String = sTo.Split(";".ToCharArray)
    Dim bOK As Boolean = False

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

    If m_sServerName = "localhost" Then
      oSmtpClient = New SmtpClient("ds3b.horizon-it.co.uk", 587)
      oSmtpClient.Credentials = New System.Net.NetworkCredential("streamline@relay.horizon-it.co.uk", "Swimmer1024!")
      oSmtpClient.EnableSsl = False
    Else
      oSmtpClient = New SmtpClient("ds3b.horizon-it.co.uk", 587)
      oSmtpClient.Credentials = New System.Net.NetworkCredential("streamline@relay.horizon-it.co.uk", "Swimmer1024!")
      oSmtpClient.EnableSsl = False
    End If
    Try
      oSmtpClient.Send(objEmail)
      Diagnostics.Debug.WriteLine("Email send to: " & sTo)

      bOK = True

    Catch localexception As Exception
      LogError("Email Send Error: " & localexception.Message)
      If Not localexception.InnerException Is Nothing Then
        LogError("Email Send Error: " & localexception.InnerException.Message)
      End If
      Diagnostics.Debug.WriteLine("Email failed to send to: " & sTo)
      bOK = False
    End Try

    Return bOK

  End Function
  Public Sub ClearError()
    'Reset the display ready for the next error, as if there would ever be two errors!!!

    m_sExceptions = Nothing
    m_sDesc = ""
    m_sTemp = ""

  End Sub


  Public Function GetOS() As String
    Dim MyAgent As String = LCase(HttpContext.Current.Request.UserAgent)
    If MyAgent.IndexOf("unknown") >= 0 Then
      Return "Unknown"
    ElseIf MyAgent.IndexOf("windows 10.0.16299") >= 0 Then
      Return "Windows 10 (1709)"
    ElseIf MyAgent.IndexOf("windows 10.0.14393") >= 0 Then
      Return "Windows 10 (1607)"
    ElseIf MyAgent.IndexOf("windows 10.0.10586") >= 0 Then
      Return "Windows 10 (1511)"
    ElseIf MyAgent.IndexOf("windows 10.0.10240") >= 0 Then
      Return "Windows 10"
    ElseIf MyAgent.IndexOf("windows nt 6.3") >= 0 Then
      Return "Windows 8.1"
    ElseIf MyAgent.IndexOf("windows nt 6.2") >= 0 Then
      Return "Windows 8"
    ElseIf MyAgent.IndexOf("windows nt 6.1") >= 0 Then
      Return "Windows 7"
    ElseIf MyAgent.IndexOf("windows nt 6.0") >= 0 Then
      Return "Windows Vista"
    ElseIf MyAgent.IndexOf("windows nt 5.2") >= 0 Then
      Return "Windows Server 2003"
    ElseIf MyAgent.IndexOf("windows nt 5.1") >= 0 Then
      Return "Windows XP"
    ElseIf MyAgent.IndexOf("windows nt 5.01") >= 0 Then
      Return "Windows 2000 (SP1)"
    ElseIf MyAgent.IndexOf("windows nt 5.0") >= 0 Then
      Return "Windows 2000"
    ElseIf MyAgent.IndexOf("windows nt 4.5") >= 0 Then
      Return "Windows nt 4.5"
    ElseIf MyAgent.IndexOf("windows nt 4.0") >= 0 Then
      Return "Windows nt 4.0"
    ElseIf MyAgent.IndexOf("win 9x 4.90") >= 0 Then
      Return "Windows ME"
    ElseIf MyAgent.IndexOf("windows 98") >= 0 Then
      Return "Windows 98"
    ElseIf MyAgent.IndexOf("windows 95") >= 0 Then
      Return "Windows 95"
    ElseIf MyAgent.IndexOf("windows CE") >= 0 Then
      Return "Windows CE"
    ElseIf (MyAgent.Contains("ipad")) Then
      Return String.Format("iPad OS {0}", GetMobileVersion(MyAgent, "os"))
    ElseIf (MyAgent.Contains("iphone")) Then
      Return String.Format("iPhone OS {0}", GetMobileVersion(MyAgent, "os"))
    ElseIf (MyAgent.Contains("linux") AndAlso MyAgent.Contains("kfapwi")) Then
      Return "Kindle Fire"
    ElseIf (MyAgent.Contains("rim tablet") OrElse (MyAgent.Contains("gg") AndAlso MyAgent.Contains("mobile"))) Then
      Return "Black Berry"
    ElseIf (MyAgent.Contains("windows phone")) Then
      Return String.Format("Windows Phone {0}", GetMobileVersion(MyAgent, "windows phone"))
    ElseIf (MyAgent.Contains("mac os")) Then
      Return "Mac OS"
    ElseIf MyAgent.IndexOf("android") >= 0 Then
      Return String.Format("Android {0}", GetMobileVersion(MyAgent, "android"))
    Else
      Return "OS is unknown: " & MyAgent
    End If
  End Function

  Private Function GetMobileVersion(userAgent As String, device As String) As String
    Dim ReturnValue As String = String.Empty
    Dim RawVersion As String = userAgent.Substring(userAgent.IndexOf(device) + device.Length).TrimStart()
    For Each character As Char In RawVersion
      If IsNumeric(character) Then
        ReturnValue &= character
      ElseIf (character = "." OrElse character = "_") Then
        ReturnValue &= "."
      Else
        Exit For
      End If
    Next
    Return ReturnValue
  End Function

End Class
