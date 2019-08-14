Option Explicit On
Imports Microsoft.VisualBasic
Imports System
Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Net.Sockets



Public Class DL_FTP
#Region "Class Variable Declarations"
  Private m_sRemoteHost, m_sRemotePath, m_sRemoteUser As String
  Private m_sRemotePassword, m_sMess As String
  Private m_iRemotePort, m_iBytes As Int32
  Private m_objClientSocket As Socket

  Private m_iRetValue As Int32
  Private m_bLoggedIn As Boolean
  Private m_sMes, m_sReply As String
  Private m_pclErrManager As CL_ErrManager

  Private m_bDebugLog As Boolean


  'Set the size of the packet that is used to read and to write data to the FTP server
  'to the following specified size.
  Public Const BLOCK_SIZE As Int32 = 512
  Private m_aBuffer(BLOCK_SIZE) As Byte
  Private ASCII As Encoding = Encoding.ASCII
  Public flag_bool As Boolean
  'General variable declaration
  Private m_sMessageString As String
#End Region
#Region "Class Constructors"

  ' Parameterized constructor
  Public Sub New(ByVal sRemoteHost As String,
                 ByVal sRemotePath As String,
                 ByVal sRemoteUser As String,
                 ByVal sRemotePassword As String,
                 ByVal iRemotePort As Int32, pcl_ErrManager As CL_ErrManager)
    m_sRemoteHost = sRemoteHost
    m_sRemotePath = sRemotePath
    m_sRemoteUser = sRemoteUser
    m_sRemotePassword = sRemotePassword
    m_sMessageString = ""
    m_iRemotePort = 21
    m_bLoggedIn = False
    m_pclErrManager = pcl_ErrManager

    m_bDebugLog = CBool(System.Configuration.ConfigurationManager.AppSettings("FtpDebugLogging"))

  End Sub
#End Region
#Region "Public Properties"

  'Set or Get the name of the FTP server that you want to connect.
  Public Property RemoteHostFTPServer() As String
    'Get the name of the FTP server.
    Get
      Return m_sRemoteHost
    End Get
    'Set the name of the FTP server.
    Set(ByVal Value As String)
      m_sRemoteHost = Value
    End Set
  End Property

  'Set or Get the FTP Port Number of the FTP server that you want to connect.
  Public Property RemotePort() As Int32
    'Get the FTP Port Number.
    Get
      Return m_iRemotePort
    End Get
    'Set the FTP Port Number.
    Set(ByVal Value As Int32)
      m_iRemotePort = Value

    End Set
  End Property

  'Set or Get the remote path of the FTP server that you want to connect.
  Public Property RemotePath() As String
    'Get the remote path.
    Get
      Return m_sRemotePath
    End Get
    'Set the remote path.
    Set(ByVal Value As String)
      m_sRemotePath = Value
    End Set
  End Property

  'Set or Get the remote password of the FTP server that you want to connect.
  Public Property RemotePassword() As String
    Get
      Return m_sRemotePassword
    End Get
    Set(ByVal Value As String)
      m_sRemotePassword = Value
    End Set
  End Property

  'Set or Get the remote user of the FTP server that you want to connect.
  Public Property RemoteUser() As String
    Get
      Return m_sRemoteUser
    End Get
    Set(ByVal Value As String)
      m_sRemoteUser = Value
    End Set
  End Property

  'Set the class MessageString.
  Public Property MessageString() As String
    Get
      Return m_sMessageString
    End Get
    Set(ByVal Value As String)
      m_sMessageString = Value
    End Set
  End Property

  Public ReadOnly Property RetValue() As Int32
    Get
      Return m_iRetValue
    End Get
  End Property
  Public ReadOnly Property RetMsg() As String
    Get
      Return m_sReply
    End Get
  End Property



#End Region
#Region "Public Subs and Functions"
  Public Function GetFileList(ByVal sMask As String) As String()
    'Return a list of files in a string() array from the file system.
    Dim cSocket As Socket
    Dim bytes As Int32
    Dim seperator As Char = ControlChars.Lf
    Dim mess() As String

    m_sMes = ""
    'Check if you are logged on to the FTP server.
    If (Not (m_bLoggedIn)) Then
      Login()
    End If

    cSocket = CreateDataSocket()
    'Send an FTP command,
    SendCommand("NLST " & sMask)

    If m_iRetValue <> 550 Then

      If (Not (m_iRetValue = 150 Or m_iRetValue = 125)) Then
        MessageString = m_sReply
        Throw New IOException(m_sReply.Substring(4))
      End If

      m_sMes = ""
      Do While (True)
        Array.Clear(m_aBuffer, 0, m_aBuffer.Length)
        bytes = cSocket.Receive(m_aBuffer, m_aBuffer.Length, 0)
        m_sMes += ASCII.GetString(m_aBuffer, 0, bytes)

        If (bytes < m_aBuffer.Length) Then
          Exit Do
        End If
      Loop

      mess = m_sMes.Split(seperator)
      cSocket.Close()
      ReadReply()

      If (m_iRetValue <> 226) Then
        MessageString = m_sReply
        Throw New IOException(m_sReply.Substring(4))
      End If
    Else
      ReDim mess(-1)
    End If

    Return mess
  End Function
  Public Function GetFileSize(ByVal sFileName As String) As Long
    ' Get the size of the file on the FTP server.
    Dim size As Long

    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command.
    SendCommand("SIZE " & sFileName)
    size = 0

    If (m_iRetValue = 213) Then
      size = Int64.Parse(m_sReply.Substring(4))
    Else
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

    Return size
  End Function
  Public Function GetFileModDate(ByVal sFileName As String) As Date
    ' Get the size of the file on the FTP server.
    Dim dtRet As Date = Nothing
    Dim sTemp As String

    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command.
    SendCommand("MDTM " & sFileName)

    'Return: 213 20190703131022
    'chars   123456789012345678
    '        RET yyyyMMddhhmmss
    If (m_iRetValue = 213) Then
      sTemp = Mid(m_sReply, 5, 4) & "-" & Mid(m_sReply, 9, 2) & "-" & Mid(m_sReply, 11, 2)
      sTemp += " " & Mid(m_sReply, 13, 2) & ":" & Mid(m_sReply, 15, 2) & ":" & Mid(m_sReply, 17, 2)

      dtRet = CDate(sTemp)
    Else
      MessageString = m_sReply
      Throw New IOException(m_sReply)
    End If

    Return dtRet
  End Function
  Public Function Login() As Boolean
    'Log on to the FTP server.
    System.Diagnostics.Debug.Write(Now & " - Creating Socket" & vbCrLf)
    m_objClientSocket = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

    System.Diagnostics.Debug.Write(Now & " - Create End Point" & vbCrLf)
    Try
      'Dim ep As New IPEndPoint(Dns.Resolve(m_sRemoteHost).AddressList(0), m_iRemotePort)
      Dim ep As New IPEndPoint(Dns.GetHostEntry(m_sRemoteHost).AddressList(0), m_iRemotePort)

      System.Diagnostics.Debug.Write(Now & " - Connecting Sockect" & vbCrLf)
      m_objClientSocket.Connect(ep)
    Catch ex As Exception
      'ReadReply()
      'MessageString = m_sReply
      m_bLoggedIn = False
      Throw New IOException("Cannot connect to remote server", 667)
      m_pclErrManager.LogError("DL_FTP Login: Failed to connect to port.")
    End Try

    ReadReply()
    If (m_iRetValue <> 220) Then
      CloseConnection()
      MessageString = m_sReply
      m_pclErrManager.LogError("DL_FTP Login: " & m_sReply.Substring(4))
      Throw New IOException(m_sReply.Substring(4))
    End If
    'Send an FTP command to send a user logon ID to the server.
    SendCommand("USER " & m_sRemoteUser)
    If (Not (m_iRetValue = 331 Or m_iRetValue = 230)) Then
      Cleanup()
      MessageString = m_sReply
      m_pclErrManager.LogError("DL_FTP Login: " & m_sReply.Substring(4))
      Throw New IOException(m_sReply.Substring(4))
    End If

    If (m_iRetValue <> 230) Then
      'Send an FTP command to send a user logon password to the server.
      SendCommand("PASS " & m_sRemotePassword)
      If (Not (m_iRetValue = 230 Or m_iRetValue = 202)) Then
        Cleanup()
        MessageString = m_sReply
        m_pclErrManager.LogError("DL_FTP Login: " & m_sReply.Substring(4))
        Throw New IOException(m_sReply.Substring(4))
      End If
    End If

    m_bLoggedIn = True
    'Call the ChangeDirectory user-defined function to change the folder to the
    'remote FTP folder that is mapped.
    ChangeDirectory(m_sRemotePath)

    'Return the final result.
    Return m_bLoggedIn
  End Function
  Public Sub SetBinaryMode(ByVal bMode As Boolean)
    'If the value of mode is true, set the binary mode for downloads. Otherwise, set ASCII mode.
    If (bMode) Then
      'Send the FTP command to set the binary mode.
      '(TYPE is an FTP command that is used to specify representation type.)
      SendCommand("TYPE I")
    Else
      'Send the FTP command to set ASCII mode.
      '(TYPE is a FTP command that is used to specify representation type.)
      SendCommand("TYPE A")
    End If

    If (m_iRetValue <> 200) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If
  End Sub
  Public Sub DownloadFile(ByVal sFileName As String)
    ' Download a file to the local folder of the assembly, and keep the same file name.
    DownloadFile(sFileName, "", False)
  End Sub
  Public Sub DownloadFile(ByVal sFileName As String, ByVal bResume As Boolean)
    ' Download a remote file to the local folder of the assembly, and keep the same file name.
    DownloadFile(sFileName, "", bResume)
  End Sub
  Public Sub DownloadFile(ByVal sFileName As String, ByVal sLocalFileName As String)
    DownloadFile(sFileName, sLocalFileName, False)
  End Sub
  Public Sub DownloadFile(ByVal sFileName As String,
                          ByVal sLocalFileName As String,
                          ByVal bResume As Boolean)
    Dim st As Stream
    Dim output As FileStream
    Dim cSocket As Socket
    Dim offset, npos As Long

    If (Not (m_bLoggedIn)) Then
      Login()
    End If

    SetBinaryMode(True)

    If (sLocalFileName.Equals("")) Then
      sLocalFileName = sFileName
    End If

    If (Not (File.Exists(sLocalFileName))) Then
      st = File.Create(sLocalFileName)
      st.Close()
    End If

    output = New FileStream(sLocalFileName, FileMode.Open)
    cSocket = CreateDataSocket()
    offset = 0

    If (bResume) Then
      offset = output.Length

      If (offset > 0) Then
        'Send an FTP command to restart.
        SendCommand("REST " & offset)
        If (m_iRetValue <> 350) Then
          offset = 0
        End If
      End If

      If (offset > 0) Then
        npos = output.Seek(offset, SeekOrigin.Begin)
      End If
    End If
    'Send an FTP command to retrieve a file.
    SendCommand("RETR " & sFileName)

    If (Not (m_iRetValue = 150 Or m_iRetValue = 125 Or m_iRetValue = 226 Or m_iRetValue = 250)) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

    Do While (True)
      Array.Clear(m_aBuffer, 0, m_aBuffer.Length)
      m_iBytes = cSocket.Receive(m_aBuffer, m_aBuffer.Length, 0)
      output.Write(m_aBuffer, 0, m_iBytes)

      If (m_iBytes <= 0) Then
        Exit Do
      End If
    Loop

    output.Close()
    If (cSocket.Connected) Then
      cSocket.Close()
    End If

    ReadReply()
    If (Not (m_iRetValue = 226 Or m_iRetValue = 250)) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

  End Sub
  Public Sub UploadFile(ByVal sFileName As String,
                        ByVal sTofile As String,
                        Optional ByVal bResume As Boolean = False)

    Dim cSocket As Socket
    Dim offset As Long
    Dim input As FileStream
    Dim bFileNotFound As Boolean

    If (Not (m_bLoggedIn)) Then
      Login()
    End If

    cSocket = CreateDataSocket()
    offset = 0

    If (bResume) Then
      Try
        SetBinaryMode(True)
        offset = GetFileSize(sFileName)
      Catch ex As Exception
        m_bLoggedIn = False
        offset = 0
      End Try
    End If

    If (offset > 0) Then
      SendCommand("REST " & offset)
      If (m_iRetValue <> 350) Then

        'The remote server may not support resuming.
        offset = 0
      End If
    End If
    'Send an FTP command to store a file.
    SendCommand("STOR " & Path.GetFileName(sTofile))
    If (Not (m_iRetValue = 125 Or m_iRetValue = 150)) Then
      MessageString = m_sReply
      Dim iNo As Integer
      iNo = CInt(Int(m_sReply.Substring(0, 3)))
      m_pclErrManager.LogError("FTP Upload Error 1: " & m_sReply.Substring(4) & ", No.: " & iNo)
      Throw New IOException(m_sReply.Substring(4), iNo)
    End If

    'Check to see if the file exists before the upload.
    bFileNotFound = False
    If (File.Exists(sFileName)) Then
      ' Open the input stream to read the source file.
      Dim oFileInfo As New System.IO.FileInfo(sFileName)
      Dim iSize As Int32
      Dim iXfer As Int32

      iSize = CInt(oFileInfo.Length)

      input = New FileStream(sFileName, FileMode.Open, FileAccess.Read)
      If (offset <> 0) Then
        input.Seek(offset, SeekOrigin.Begin)
      End If

      'Upload the file.
      m_iBytes = input.Read(m_aBuffer, 0, m_aBuffer.Length)
      System.Diagnostics.Debug.Write(Now & " - Xfer Data: " & oFileInfo.Length.ToString)
      Try
        Do While (m_iBytes > 0)

          'System.Diagnostics.Debug.Write(".")
          cSocket.Send(m_aBuffer, m_iBytes, 0)
          iXfer += m_iBytes
          m_iBytes = input.Read(m_aBuffer, 0, m_aBuffer.Length)
        Loop
        System.Diagnostics.Debug.Write("Complete" & vbCrLf)

        input.Close()
      Catch
        input.Close()
        m_iRetValue = Err.Number
        m_sReply = Err.Description
        m_pclErrManager.LogError("FTP Upload Error 2: " & m_sReply & ", No.: " & m_iRetValue)

        System.Diagnostics.Debug.Write(Now & " - Fail: " & m_iRetValue.ToString & "  " & m_sReply & vbCrLf)
        m_bLoggedIn = False
        MessageString = m_sReply
        'Throw New IOException(m_sReply)
        Exit Sub
      End Try
    Else
      bFileNotFound = True
    End If

    If (cSocket.Connected) Then
      cSocket.Close()
    End If

    'Check the return value if the file was not found.
    If (bFileNotFound) Then
      MessageString = m_sReply
      Throw New IOException("The file: " & sFileName & " was not found." &
      " Cannot upload the file to the FTP site.")

    End If

    ReadReply()
    If (Not (m_iRetValue = 226 Or m_iRetValue = 250)) Then
      MessageString = m_sReply
      Exit Sub
    Else
      'All OK!!!
      m_iRetValue = 0
    End If
  End Sub
  Public Function DeleteFile(ByVal sFileName As String) As Boolean
    ' Delete a file from the remote FTP server.
    Dim bResult As Boolean

    bResult = True
    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command to delete a file.
    SendCommand("DELE " & sFileName)
    If (m_iRetValue <> 250) Then
      bResult = False
      MessageString = m_sReply
    End If

    ' Return the final result.
    Return bResult
  End Function
  Public Function RenameFile(ByVal sOldFileName As String,
                             ByVal sNewFileName As String) As Boolean
    Dim bResult As Boolean

    bResult = True
    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command to rename a file.
    SendCommand("RNFR " & sOldFileName)
    If (m_iRetValue <> 350) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

    'Send an FTP command to rename a file to a file name.
    'It will overwrite if newFileName exists.
    SendCommand("RNTO " & sNewFileName)
    If (m_iRetValue <> 250) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If
    ' Return the final result.
    Return bResult
  End Function
  Public Function CreateDirectory(ByVal sDirName As String) As Boolean
    Dim bResult As Boolean

    bResult = True
    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command to make a folder on the FTP server.
    SendCommand("MKD " & sDirName)
    If (m_iRetValue <> 257) Then
      bResult = False
      MessageString = m_sReply
    End If

    ' Return the final result.
    Return bResult
  End Function
  Public Function RemoveDirectory(ByVal sDirName As String) As Boolean
    Dim bResult As Boolean

    bResult = True
    'Check if you are logged on to the FTP server.
    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command to remove a folder on the FTP server.
    SendCommand("RMD " & sDirName)
    If (m_iRetValue <> 250) Then
      bResult = False
      MessageString = m_sReply
    End If

    ' Return the final result.
    Return bResult
  End Function
  Public Function ChangeDirectory(ByVal sDirName As String) As Boolean
    Dim bResult As Boolean

    bResult = True
    'Check if you are in the root directory.
    If (sDirName.Equals(".")) Then
      Return False 'Not sure that this is about, need to check code operation!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
    End If
    'Check if you are logged on to the FTP server.
    If (Not (m_bLoggedIn)) Then
      Login()
    End If
    'Send an FTP command to change the folder on the FTP server.
    SendCommand("CWD " & sDirName)
    If (m_iRetValue <> 250) Then
      bResult = False
      MessageString = m_sReply
    End If

    Me.m_sRemotePath = sDirName

    ' Return the final result.
    Return bResult
  End Function
  Public Function GetWorkingDirectory() As String
    'Return a list of files in a string() array from the file system.
    Dim cSocket As Socket
    Dim sMess As String = ""

    m_sMes = ""
    'Check if you are logged on to the FTP server.
    If (Not (m_bLoggedIn)) Then
      Login()
    End If

    cSocket = CreateDataSocket()

    'Send an FTP command,
    SendCommand("PWD")

    If m_iRetValue <> 257 Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

    sMess = Mid(m_sMes, InStr(m_sMes, Chr(34)) + 1)
    sMess = Left(sMess, InStr(sMess, Chr(34)) - 1)


    cSocket.Close()

    Return sMess
  End Function


  Public Sub CloseConnection()
    If (Not (m_objClientSocket Is Nothing)) Then
      'Send an FTP command to end an FTP server system.
      SendCommand("QUIT")
    End If

    Cleanup()
  End Sub

#End Region
#Region "Private Subs and Functions"
  ' Read the reply from the FTP server.
  Private Sub ReadReply()
    m_sMes = ""
    m_sReply = ReadLine()
    m_iRetValue = Int32.Parse(m_sReply.Substring(0, 3))
    System.Diagnostics.Debug.Write(Now & " - Reply:" & m_sReply)
  End Sub

  Private Sub Cleanup()
    ' Clean up some variables.
    If Not (m_objClientSocket Is Nothing) Then
      m_objClientSocket.Close()
      m_objClientSocket = Nothing
    End If
    m_bLoggedIn = False
  End Sub
  ' Read a line from the FTP server.
  Private Function ReadLine(Optional ByVal bClearMes As Boolean = False) As String
    Dim seperator As Char = ControlChars.Lf
    Dim mess() As String

    If (bClearMes) Then
      m_sMes = ""
    End If
    Do While (True)
      Array.Clear(m_aBuffer, 0, BLOCK_SIZE)
      m_iBytes = m_objClientSocket.Receive(m_aBuffer, m_aBuffer.Length, 0)
      m_sMes += ASCII.GetString(m_aBuffer, 0, m_iBytes)
      If (m_iBytes < m_aBuffer.Length) Then
        Exit Do
      End If
    Loop

    mess = m_sMes.Split(seperator)
    If (m_sMes.Length > 2) Then
      m_sMes = mess(mess.Length - 2)
    Else
      m_sMes = mess(0)
    End If

    If (Not (m_sMes.Substring(3, 1).Equals(" "))) Then
      Return ReadLine(True)
    End If

    Return m_sMes
  End Function
  ' This is a function that is used to send a command to the FTP server that you are connected to.
  Private Sub SendCommand(ByVal sCommand As String)
    System.Diagnostics.Debug.Write(Now & " - Send:" & sCommand & vbCrLf)
    If m_bDebugLog Then
      m_pclErrManager.LogError("SendCommand:" & sCommand)
    End If
    sCommand = sCommand & ControlChars.CrLf
    Try
      Dim cmdbytes As Byte() = ASCII.GetBytes(sCommand)
      m_objClientSocket.Send(cmdbytes, cmdbytes.Length, 0)
      ReadReply()
      If m_bDebugLog Then
        m_pclErrManager.LogError("Reply:" & m_sReply)
      End If


    Catch ex As Exception
      m_bLoggedIn = False
      System.Diagnostics.Debug.Write(Now & " - Send Failed:" & ex.Message & vbCrLf)
      If m_bDebugLog Then
        m_pclErrManager.LogError("SendCommand Error:" & ex.Message)
      End If
      Throw ex
    End Try
  End Sub
  ' Create a data socket.
  Private Function CreateDataSocket() As Socket
    Dim index1, index2, len As Int32
    Dim partCount, i, port As Int32
    Dim ipData, buf, ipAddress As String
    Dim parts(6) As Int32
    Dim ch As Char
    Dim s As Socket
    Dim ep As IPEndPoint
    'Send an FTP command to use a passive data connection.
    SendCommand("PASV")
    If (m_iRetValue <> 227) Then
      MessageString = m_sReply
      Throw New IOException(m_sReply.Substring(4))
    End If

    index1 = m_sReply.IndexOf("(")
    index2 = m_sReply.IndexOf(")")
    ipData = m_sReply.Substring(index1 + 1, index2 - index1 - 1)

    len = ipData.Length
    partCount = 0
    buf = ""

    For i = 0 To ((len - 1)) 'And partCount <= 6)
      ch = Char.Parse(ipData.Substring(i, 1))
      If (Char.IsDigit(ch)) Then
        buf += ch
      ElseIf (ch <> ",") Then
        MessageString = m_sReply
        Throw New IOException("Malformed PASV reply: " & m_sReply)
      End If

      If ((ch = ",") Or (i + 1 = len)) Then
        Try
          parts(partCount) = Int32.Parse(buf)
          partCount += 1
          buf = ""
        Catch ex As Exception
          MessageString = m_sReply
          m_bLoggedIn = False
          Throw New IOException("Malformed PASV reply: " & m_sReply)
        End Try
      End If
    Next

    ipAddress = parts(0) & "." & parts(1) & "." & parts(2) & "." & parts(3)

    ' Make this call in Visual Basic .NET 2002.  You want to
    ' bitshift the number by 8 bits. Therefore, in Visual Basic .NET 2002, you must
    ' multiply the number by 2 to the power of 8.
    port = CInt(parts(4) * (2 ^ 8))

    ' Make this call and then comment out the previous line for Visual Basic .NET 2003.
    'port = parts(4) << 8

    ' Determine the data port number.
    port = port + parts(5)

    s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    'ep = New IPEndPoint(Dns.GetHostEntry(ipAddress).AddressList(0), port)

    Dim ipAddr As System.Net.IPAddress
    ipAddr = System.Net.IPAddress.Parse(ipAddress)
    ep = New IPEndPoint(ipAddr, port)

    Try
      s.Connect(ep)
    Catch ex As Exception
      System.Diagnostics.Debug.Write(Now & " - Failed to connect to data port." & ipAddress & ":" & port.ToString & " - " & Err.Description)
      MessageString = m_sReply
      Throw New IOException("Cannot connect to remote server")
      'If you cannot connect to the FTP
      'server that is specified, make the boolean variable false.
      flag_bool = False
      m_bLoggedIn = False
    End Try
    'If you can connect to the FTP server that is specified, make the boolean variable true.
    flag_bool = True
    Return s
  End Function

#End Region
End Class

