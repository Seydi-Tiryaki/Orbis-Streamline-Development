Imports System.Data

<Serializable> Public Class BL_Operator
  Implements IDisposable
  Private m_sPWord As String
  Private m_sNewPWord As String
  Private m_bLoggedOn As Boolean

  Private m_pbl_Client As BL_Client
  Private m_drOperator As DataRow
  Private m_pdl_Manager As DL_Manager

  Private m_sMsg As String
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_iOperator As Int32 = -1
  Private m_iClientID As Int32
  Private m_dtPerms As DataTable
  Private m_dicPerms As New eDictionary()
  Private m_sName As String
  Private m_sEmail As String
  Private m_sTelephone As String
  Private m_sJobTitle As String
  Private m_bSuspended As Boolean
  Private m_sTimestamp As String

  Private m_dtOrgs As DataTable
  Private m_dtCusts As DataTable
  Private m_dtLocs As DataTable

  Private m_bSuperUser As Boolean

  'Private oDB_Layer As DB_LayerGeneric
  Private m_bCloseDB As Boolean = False
  Private Const m_PassLetters As String = "!@abcdefghijklmnopqrstuvwxyz@!ABCDEFGHIJKLMNOPQRSTUVWXY@!Z1234567890!@"


  Public Sub New(ByVal pDL_manager As DL_Manager, ByVal pCL_ErrManager As CL_ErrManager, ByVal pBL_Client As BL_Client)
    m_pdl_Manager = pDL_manager
    m_pcl_ErrManager = pCL_ErrManager
    m_pbl_Client = pBL_Client

  End Sub

#Region "Dispose"
  Public Overloads Sub Dispose() Implements IDisposable.Dispose
    zDispose(True)
    GC.SuppressFinalize(Me)
  End Sub
  Protected Overrides Sub Finalize()
    zDispose(False)
    MyBase.Finalize()
  End Sub
  Private Sub zDispose(ByVal bCloseDown As Boolean)
    If bCloseDown Then
      'Shutdown the SQL Connection....
      m_pbl_Client = Nothing
      m_pcl_ErrManager = Nothing
      m_pdl_Manager = Nothing
    End If
  End Sub
#End Region


  Public Enum enLogonResponse
    lrOK
    lrInvalidUserID
    lrInvalidPassword
    lrSuspended
    lrPasswordExpired
    lrConfirmPassword
    lrInvRepeatPassword
    lrUserExpired
    lrNotActive
    lrDeactivated
  End Enum
  Public Property ID() As Int32
    Get
      ID = m_iOperator
    End Get
    Set(ByVal Value As Int32)
      m_iOperator = Value
    End Set
  End Property
  Public Property ClientID() As Int32
    Get
      ClientID = m_iClientID
    End Get
    Set(ByVal Value As Int32)
      m_iClientID = Value
    End Set
  End Property
  'Public Property drOperator(bAsSuperUser As Boolean) As DataRow
  '  Get
  '    drOperator = m_drOperator
  '  End Get
  '  Set(ByVal Value As DataRow)
  '    m_drOperator = Value
  '    zLoadVarsFromRow(m_drOperator)
  '  End Set
  'End Property
  Public Property PWord() As String
    Get
      PWord = m_sPWord
    End Get
    Set(ByVal Value As String)
      m_sPWord = Value
    End Set
  End Property
  Public Property NewPWord() As String
    Get
      NewPWord = m_sNewPWord
    End Get
    Set(ByVal Value As String)
      m_sNewPWord = Value
    End Set
  End Property
  Public Property JobTitle() As String
    Get
      JobTitle = m_sJobTitle
    End Get
    Set(ByVal Value As String)
      m_sJobTitle = Value
    End Set
  End Property
  Public Property Suspended() As Boolean
    Get
      Suspended = m_bSuspended
    End Get
    Set(ByVal Value As Boolean)
      m_bSuspended = Value
    End Set
  End Property
  Public Property Email() As String
    Get
      Email = m_sEmail
    End Get
    Set(ByVal Value As String)
      m_sEmail = Value
    End Set
  End Property
  Public Property Telephone() As String
    Get
      Telephone = m_sTelephone
    End Get
    Set(ByVal Value As String)
      m_sTelephone = Value
    End Set
  End Property
  Public Property Name() As String
    Get
      Name = m_sName
    End Get
    Set(ByVal Value As String)
      m_sName = Value
    End Set
  End Property
  Public Property Timestamp() As String
    Get
      Timestamp = m_sTimestamp
    End Get
    Set(ByVal Value As String)
      m_sTimestamp = Value
    End Set
  End Property
  Public Property pbl_Client() As BL_Client
    Get
      pbl_Client = m_pbl_Client
    End Get
    Set(ByVal Value As BL_Client)
      m_pbl_Client = Value
    End Set
  End Property
  Public Property pdl_Manager() As DL_Manager
    Get
      pdl_Manager = m_pdl_Manager
    End Get
    Set(ByVal Value As DL_Manager)
      m_pdl_Manager = Value
    End Set
  End Property
  Public Property pcl_ErrManager() As CL_ErrManager
    Get
      pcl_ErrManager = m_pcl_ErrManager
    End Get
    Set(ByVal Value As CL_ErrManager)
      m_pcl_ErrManager = Value
    End Set
  End Property
  Public ReadOnly Property sMsg() As String
    Get
      sMsg = m_sMsg
    End Get
  End Property
  Public Property LoggedOn() As Boolean
    Get
      LoggedOn = m_bLoggedOn
    End Get
    Set(ByVal value As Boolean)
      m_bLoggedOn = value
    End Set
  End Property
  'Public Property drOperator() As DataRow
  '  Get
  '    drOperator = drOperator
  '  End Get
  '  Set(ByVal Value As DataRow)
  '    drOperator = Value
  '    zLoadVarsFromRow()
  '  End Set
  'End Property
  Public Property dicOpPerms() As eDictionary
    Get
      dicOpPerms = m_dicPerms
    End Get
    Set(ByVal Value As eDictionary)
      m_dicPerms = Value
    End Set
  End Property

  Public Property bIsSuperUser() As Boolean
    Get
      bIsSuperUser = m_bSuperUser
    End Get
    Set(ByVal value As Boolean)
      m_bSuperUser = value
    End Set
  End Property

  Public Property dtOrganizations() As DataTable
    Get
      dtOrganizations = m_dtOrgs
    End Get
    Set(ByVal value As DataTable)
      m_dtOrgs = value
    End Set
  End Property

  Public Property dtCustomers() As DataTable
    Get
      dtCustomers = m_dtCusts
    End Get
    Set(ByVal value As DataTable)
      m_dtCusts = value
    End Set
  End Property


  Public Property dtLocations() As DataTable
    Get
      dtLocations = m_dtLocs
    End Get
    Set(ByVal value As DataTable)
      m_dtLocs = value
    End Set
  End Property


  Public Function UserLogon(sUid As String, sPwd As String) As enLogonResponse
    'Validate the user information.....
    Dim sSql As String = ""
    Dim enRet As enLogonResponse
    Dim drOperator As DataRow
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    Dim pcl_Utils As New CL_Utils

    Dim dicParms As New eDictionary



    dicParms.Add("@user", "S:" & sUid)
    dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)
    'sSql = "  Declare @user varchar(200) declare @client integer"
    sSql += " SELECT * FROM sop_operators WHERE "
    sSql += " operator_email = @user"
    sSql += " and Client_ID=@client"
    drOperator = oDB_Layer.RunSqlToDataRow(sSql, dicParms)



    m_bLoggedOn = False
    If Not drOperator Is Nothing Then
      'Valid Operator ID....
      System.Diagnostics.Debug.Write("operator logon attempt: " & CStr(drOperator!Operator_name) & vbCrLf)
      Dim stemp As String = pcl_Utils.DecryptFromBase64String(CStr(drOperator!operator_password))
      m_sEmail = oDB_Layer.CheckDBNullStr(drOperator!operator_email)
      If stemp = sPwd Then
        'valid Password
        If Not CBool(drOperator!Operator_suspended) Then
          'Operator is Active
          'Operator is not expired
          Select Case m_pbl_Client.ClientStatus
            Case BL_Client.enClientStatus.enActive
              'Clear everything!!
              Reset()

              m_bLoggedOn = True
              m_iOperator = CInt(drOperator.Item("Operator_ID"))
              enRet = enLogonResponse.lrOK
              zLoadVarsFromRow(drOperator, False)
            Case Else
              enRet = enLogonResponse.lrDeactivated
              m_sMsg = "Client is not active"

          End Select
        Else
          'Operator Suspended
          m_sMsg = "User as been suspended"
          enRet = enLogonResponse.lrSuspended
        End If
      Else
        'Invalid Password
        m_sMsg = "User or Password is invalid"
        enRet = enLogonResponse.lrInvalidPassword
      End If
    Else
      'Invalid Operator ID
      m_sMsg = "User Name or Password is invalid"
      enRet = enLogonResponse.lrInvalidUserID
    End If

    oDB_Layer.Dispose()

    Return enRet

  End Function
  Public Function GetOperator(ByVal sUid As String, bAsSuperUser As Boolean) As Boolean
    'Get the user information.....
    Return zGetOperator(sUid, ,, bAsSuperUser)
  End Function
  Public Function GetOperatorByID(ByVal iUid As Int32, bAsSuperUser As Boolean) As Boolean
    'Get the user information.....
    Return zGetOperator("", iUid,, bAsSuperUser)
  End Function
  'Public Function GetOperatorByEmail(ByVal sEmail As String) As DataRow
  '  'Get the user information.....
  '  If zGetOperator("", -1, sEmail) Then
  '    Return drOperator

  '  Else
  '    Return Nothing
  '  End If

  'End Function
  Public Function HasAccess(ByVal PermGroup As Int32, ByVal PermID As Int32) As Boolean

    Dim bOk As Boolean
    Try
      Dim drRows() As DataRow

      If Not m_dtPerms Is Nothing Then
        drRows = m_dtPerms.Select("PermGroup_ID = " & PermGroup.ToString & " and Perm_ID = " & PermID.ToString)


        Dim drPerm As DataRow
        If drRows.Length = 0 Then
          'No permissions
          bOk = False
        Else
          drPerm = drRows(0)
          If Not drPerm Is Nothing Then
            'Permission is present....
            If Not IsDBNull(drPerm!OpPerm_Level) Then
              If CBool(drPerm!OpPerm_Level) Then
                bOk = True
              Else
                bOk = False
              End If
            Else
              bOk = False
            End If
          Else
            'No permission, no access.....
            bOk = False
          End If
        End If
      Else
        bOk = False
      End If

      Return bOk


    Catch localException As Exception
      m_pcl_ErrManager.AddError(localException)
      Throw localException


    End Try


  End Function

  Public Function SendReminder(ByVal sUser As String, ByVal bNew As Boolean, sResetID As String, pcl_Utils As CL_Utils) As Boolean
    'Get the user information.....
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    Dim bok As Boolean = False


    If zGetOperator(sUser) Then
      Dim sSubject As String = ""
      Dim sBody As String
      Dim sFrom As String
      Dim sBCC As String = ""
      Dim dicSubs As New eDictionary()
      Dim sHtmlLink As String
      Dim sLinkText As String
      Dim sResetIDEnc As String


      dicSubs.Add("%EMAIL%", sUser)
      dicSubs.Add("%PASSWORD%", m_sPWord)
      dicSubs.Add("%COMPANY%", oDB_Layer.CheckDBNullStr(m_pbl_Client.CompanyName))
      dicSubs.Add("%FOOTER%", m_pdl_Manager.GetMessage(15)) 'Text footer.
      sBody = m_pdl_Manager.GetMessage(10) 'HTML Email Header

      'sBody += m_pdl_Manager.GetMessage(12) 'HTML email invalid body

      If bNew Then
        sSubject = "Welcome to " & oDB_Layer.CheckDBNullStr(m_pbl_Client.CompanyName) & " Streamline Portal, here are your user login details..."
        sBody += m_pdl_Manager.GetMessage(13) 'HTML Email new user

      Else
        'Reset password email
        sResetIDEnc = pcl_Utils.EncryptStringOld(sResetID)

        If m_pdl_Manager.ServerName = "localhost" Then
          sHtmlLink = "<a href='http://localhost:59504/passwordreset.aspx?key=" & sResetIDEnc & "'>Click Here To Reset Password</a>"
          sLinkText = "http://localhost:59504/passwordreset.aspx?key=" & sResetIDEnc
        Else
          sHtmlLink = "<a href='http://streamline.orbis-sys.com/passwordreset.aspx?key=" & sResetIDEnc & "'>Click Here To Reset Password</a>"
          sLinkText = "http://streamline.orbis-sys.com/passwordreset.aspx?key=" & sResetIDEnc
        End If
        dicSubs.Add("%LINK%", sHtmlLink)
        dicSubs.Add("%LINKTEXT%", sLinkText)

        sSubject = "Password reset for your " & oDB_Layer.CheckDBNullStr(m_pbl_Client.CompanyName) & " Streamline Portal account..."
        sBody += m_pdl_Manager.GetMessage(14) 'HTML Reset user password.
      End If

      sBody += m_pdl_Manager.GetMessage(11) 'HTML Email Footer

      sFrom = m_pdl_Manager.GetSetting("EmailFrom")

      bok = m_pdl_Manager.SendEmail(sFrom, sUser, "", ConfigurationManager.AppSettings("LoginFailEmailCc"), sSubject, sBody, True, dicSubs)

      'm_pdl_Manager.SendEmail(sFrom, m_sEmail, "", sBCC, sSubject, sBody, , dicSubs)
    Else
      bok = False
      Diagnostics.Debug.WriteLine("SendReminder, Could not find user: " & sUser)

    End If
    oDB_Layer.Dispose()

    Return vbOK

  End Function
  Public Function SendInvalidLoginEmail(sEmail As String, pDBLayer As DB_LayerGeneric, request As HttpRequest) As Boolean
    Dim sFrom As String
    Dim sBody As String
    Dim sSubject As String
    Dim bOK As Boolean = False
    Dim dicEmailSubs As New eDictionary

    sFrom = CStr(System.Configuration.ConfigurationManager.AppSettings("EmailFrom"))
    sSubject = pDBLayer.CheckDBNullStr(m_pbl_Client.CompanyName) & "Orbis StreamLine Invalid login attempt!"

    sBody = m_pdl_Manager.GetMessage(10) 'HTML Email Header
    sBody += m_pdl_Manager.GetMessage(12) 'HTML email invalid body
    sBody += m_pdl_Manager.GetMessage(11) 'HTML Email Footer
    dicEmailSubs.Add("%EMAIL%", pDBLayer.CleanStringForSql(sEmail))
    dicEmailSubs.Add("%IP%", request.UserHostAddress)
    dicEmailSubs.Add("%FOOTER%", m_pdl_Manager.GetMessage(1))
    dicEmailSubs.Add("%COMPANY%", pDBLayer.CheckDBNullStr(m_pbl_Client.CompanyName))

    Try
      dicEmailSubs.Add("%PC%", System.Net.Dns.GetHostEntry(request.UserHostAddress).HostName)
    Catch
      dicEmailSubs.Add("%PC%", "Unknown Host Name")
    End Try
    bOK = m_pdl_Manager.SendEmail(sFrom, sEmail, "", ConfigurationManager.AppSettings("LoginFailEmailCc"), sSubject, sBody, True, dicEmailSubs)
    Return bOK

  End Function

  Public Sub UpdatePassword()
    'Update the row....
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    Dim pcl_Utils As New CL_Utils
    Dim dicParms As New eDictionary


    With m_pbl_Client
      Dim irows As Int32
      Dim sSql As String

      dicParms.Add("Operator_Password", "S:" & pcl_Utils.EncryptToBase64String(m_sPWord))
      dicParms.Add("operator_timestamp", "T:" & m_sTimestamp)

      sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_operators", dicParms)
      sSql += " Where Client_ID = " & m_pbl_Client.ClientID
      sSql += " and Operator_ID = " & m_iOperator
      sSql += " and operator_timestamp = @operator_timestamp"

      oDB_Layer.RunSqlNonQuery(sSql, irows, False, , dicParms)

    End With
    oDB_Layer.Dispose()

  End Sub
  Public Sub InsertOperator()
    'Insert the row....
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    Dim pcl_Utils As New CL_Utils

    Dim iRowsAffected As Int32
    Dim sSql As String
    m_sPWord = zGenerateNewPassword()

    Dim dicParms As New eDictionary()

    dicParms.Add("client_id", "N:" & m_pbl_Client.ClientID)
    dicParms.Add("Operator_Email", "S:" & m_sEmail)
    dicParms.Add("Operator_name", "S:" & m_sName)
    dicParms.Add("Operator_JobTitle", "S:" & m_sJobTitle)
    dicParms.Add("Operator_Telephone", "S:" & m_sTelephone)
    dicParms.Add("Operator_password", "S:" & pcl_Utils.EncryptToBase64String(m_sPWord))
    If m_bSuspended Then
      dicParms.Add("Operator_Suspended", "N:" & 1)
    Else
      dicParms.Add("Operator_Suspended", "N:" & 0)
    End If

    sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "sop_operators", dicParms)
    m_iOperator = oDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "sop_operators", dicParms)

    zUpdateOpPerms(oDB_Layer)

    GetOperatorByID(m_iOperator, False)

    oDB_Layer.Dispose()

  End Sub
  Public Function UpdateOperator() As DL_Manager.DB_Return
    'Update the row....
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return

    If m_sPWord = "" Then
      m_sPWord = zGenerateNewPassword()
    End If

    Dim dicParms As New eDictionary()

    dicParms.Add("client_id", "N:" & m_pbl_Client.ClientID)
    dicParms.Add("Operator_Email", "S:" & m_sEmail)
    dicParms.Add("Operator_name", "S:" & m_sName)
    dicParms.Add("Operator_Telephone", "S:" & m_sTelephone)
    dicParms.Add("Operator_JobTitle", "S:" & m_sJobTitle)
    'dicParms.Add("Operator_password", "S:" & m_pcl_Utils.EncryptString(m_sPWord))
    If m_bSuspended Then
      dicParms.Add("Operator_Suspended", "N:" & 1)
    Else
      dicParms.Add("Operator_Suspended", "N:" & 0)
    End If
    dicParms.Add("operator_timestamp", "T:" & m_sTimestamp)

    sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_operators", dicParms)
    sSql += " where operator_id = " & m_iOperator
    sSql += " and operator_timestamp=@operator_timestamp"


    oDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, , dicParms)

    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    If dbRet = DL_Manager.DB_Return.ok Then
      dbRet = zUpdateOpPerms(oDB_Layer)
    End If

    oDB_Layer.Dispose()

    Return dbRet

  End Function
  Public Sub Reset()
    'Let's clear some data...
    m_iOperator = -1
    m_sJobTitle = ""
    m_sMsg = ""
    m_sName = ""
    m_sNewPWord = ""
    m_sPWord = ""
    m_bSuspended = False
    m_bLoggedOn = False
    If Not m_dtPerms Is Nothing Then
      m_dtPerms.Rows.Clear()
    End If
    m_dtPerms = Nothing
    m_dicPerms.Clear()

  End Sub
  Public Sub CreateInitialPerms()
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    zGetOpPermissions(oDB_Layer)
    oDB_Layer.Dispose()
  End Sub


  Private Function zGenerateNewPassword() As String
    '
    Dim iLen As Int32
    Dim sPassword As String = ""
    Dim iLetter As Int32
    Dim sLetter As String

    iLen = CInt(m_pdl_Manager.GetSetting("Default--PasswordLength"))
    Randomize()
    For iLetter = 1 To iLen
      sLetter = Mid(m_PassLetters, CInt((Val(m_PassLetters.Length)) * Rnd() + 1), 1)
      sPassword += sLetter
    Next
    Return sPassword


  End Function
  Public Function zGetOperator(ByVal sUser As String, Optional ByVal iID As Int32 = -1, Optional ByVal sEmail As String = "", Optional bAsSuperUser As Boolean = False) As Boolean
    Dim sSql As String = ""
    Dim drOperator As DataRow
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    Dim dicParms As New eDictionary

    If sUser <> "" Then
      'Select by user....
      dicParms.Add("@user", "S:" & sUser)
      dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)
      sSql = "SELECT * FROM sop_operators WHERE "
      sSql += " operator_email = @user"
      sSql += " and Client_ID=@client"
    ElseIf iID <> -1 Then
      'Select by ID....
      dicParms.Add("@OpID", "N:" & iID)
      sSql = "SELECT * FROM sop_operators WHERE "
      sSql += "Operator_ID = @opID"
    Else
      'Select by Email....
      dicParms.Add("@Email", "S:" & sEmail)
      dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)
      sSql = "SELECT * FROM sop_operators WHERE "
      sSql += "Operator_Email = @email"
      sSql += " and Client_ID=@client"
    End If
    drOperator = oDB_Layer.RunSqlToDataRow(sSql, dicParms)

    If Not drOperator Is Nothing Then
      'Valid Operator ID....
      zLoadVarsFromRow(drOperator, bAsSuperUser)
      Return True
    Else
      'Invalid Operator ID
      m_sMsg = m_pdl_Manager.GetMessage(109)
      Return False
    End If

    oDB_Layer.Dispose()

  End Function
  Private Sub zGetOpPermissions(pDB_Layer As DB_LayerGeneric)

    Dim i As Int32
    Dim sSql As String
    Dim pcl_Utils As New CL_Utils

    If m_dtPerms Is Nothing Then
      'Load the Perms Table
      sSql = "SELECT pg2.*, op1.opperm_level,OpPerm_Timestamp FROM "
      sSql += " (SELECT pg1.permgroup_desc, pd1.* "
      sSql += " FROM  sop_permdesc AS pd1,  sop_permgroupdesc AS pg1 "
      sSql += " WHERE pd1.permgroup_id = pg1.permgroup_id) AS pg2"
      sSql += " LEFT JOIN sop_operatorperm AS op1 ON (op1.permgroup_id = pg2.permgroup_id AND op1.perm_id = pg2.perm_id "
      sSql += " and op1.operator_id = " & m_iOperator & ")"
      sSql += " ORDER BY pg2.permgroup_id, pg2.perm_id"
      m_dtPerms = pDB_Layer.RunSqlToTable(sSql)
    End If
    m_dicPerms.Clear()
    For i = 0 To m_dtPerms.Rows.Count - 1
      Dim sKey As String
      Dim drPerm As DataRow

      drPerm = m_dtPerms.Rows(i)
      Dim oOpPerm As New BL_OpPermission()
      With oOpPerm
        .PermGroupID = CInt(drPerm!PermGroup_ID)
        .PermGroupDesc = CStr(drPerm!PermGroup_Desc)
        .PermID = CInt(drPerm!Perm_ID)
        .PermDesc = CStr(drPerm!perm_desc)
        If IsDBNull(drPerm!opperm_Level) Then
          'Level not set, take default
          .PermLevel = 0
          .OpPermExists = False
          .OriginalLevel = .PermLevel
          .TimeStamp = Nothing
        Else
          'Perm set, so use it!!!
          .PermLevel = CInt(drPerm!OpPerm_Level)
          .OriginalLevel = .PermLevel
          .OpPermExists = True
          .TimeStamp = Format(CDate(drPerm!OpPerm_Timestamp), "yyyy-MM-dd HH:mm:ss")
        End If

        sKey = "key-" & Format(.PermGroupID, "0000")
        sKey += "-" & Format(.PermID, "0000")
        m_dicPerms.Add(sKey, oOpPerm)

      End With
      oOpPerm = Nothing
    Next


  End Sub
  Private Function zUpdateOpPerms(oDB_Layer As DB_LayerGeneric) As DL_Manager.DB_Return
    'Update the tables from the dictionary....
    Try
      Dim iPerm As Int32
      Dim iRowsAffected As Int32
      Dim iNew As Int32
      Dim sSql As String
      Dim dbRet As DL_Manager.DB_Return = DL_Manager.DB_Return.ok



      For iPerm = 0 To m_dicPerms.Count - 1
        Dim pOpPerm As BL_OpPermission
        pOpPerm = CType(m_dicPerms.Item(iPerm), BL_OpPermission)
        With pOpPerm
          If pOpPerm.PermLevel <> pOpPerm.OriginalLevel Then
            'The level has changed....
            Dim dicParms As New eDictionary()
            If .OpPermExists Then
              'update
              dicParms.Add("opPerm_level", "N:" & .PermLevel)
              dicParms.Add("OpPerm_Timestamp", "T:" & .TimeStamp)
              sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_operatorperm", dicParms)
              sSql += " Where operator_id=" & m_iOperator
              sSql += " and PermGroup_id =" & .PermGroupID
              sSql += " and Perm_id =" & .PermID
              sSql += " and OpPerm_Timestamp=@OpPerm_Timestamp"
              oDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, , dicParms)
              If iRowsAffected > 0 Then
                dbRet = DL_Manager.DB_Return.ok
              Else
                dbRet = DL_Manager.DB_Return.TimeStampMismatch
                Exit For
              End If
            Else
              'Insert...
              dicParms.Add("operator_id", "N:" & m_iOperator)
              dicParms.Add("PermGroup_id", "N:" & .PermGroupID)
              dicParms.Add("Perm_ID", "N:" & .PermID)
              dicParms.Add("opPerm_level", "N:" & .PermLevel)
              Dim drChecker As DataRow
              sSql = "Select * from sop_operatorperm where operator_id=@operator_id and PermGroup_id=@PermGroup_id and Perm_ID=@Perm_ID "
              drChecker = oDB_Layer.RunSqlToDataRow(sSql, dicParms)
              If drChecker Is Nothing Then
                sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "sop_operatorperm", dicParms)
                iNew = oDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
              Else
                dicParms.Add("OpPerm_Timestamp", "T:" & .TimeStamp)
                sSql = oDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_operatorperm", dicParms)
                sSql += " Where operator_id=" & m_iOperator
                sSql += " and PermGroup_id =" & .PermGroupID
                sSql += " and Perm_id =" & .PermID
                sSql += " and OpPerm_Timestamp=@OpPerm_Timestamp"
                oDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, , dicParms)
                If iRowsAffected > 0 Then
                  dbRet = DL_Manager.DB_Return.ok
                Else
                  dbRet = DL_Manager.DB_Return.TimeStampMismatch
                  Exit For
                End If
              End If
            End If
          End If
        End With
      Next
      Return dbRet

    Catch localException As Exception
      m_pcl_ErrManager.AddError(localException)
      Throw localException

    End Try
  End Function
  Private Sub zLoadVarsFromRow(drOperator As DataRow, bAsSuperUser As Boolean)

    Dim pcl_Utils As New CL_Utils
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)


    m_sPWord = CStr(drOperator!operator_password)
    m_sPWord = pcl_Utils.DecryptFromBase64String(m_sPWord)
    m_iClientID = CInt(drOperator!Client_ID)
    m_iOperator = CInt(drOperator!Operator_id)
    m_sName = CStr(drOperator!Operator_name)
    m_sEmail = CStr(drOperator!Operator_Email)
    m_sTelephone = oDB_Layer.CheckDBNullStr(drOperator!Operator_Telephone)
    m_bSuspended = CBool(drOperator!Operator_suspended)
    If IsDBNull(drOperator!operator_jobtitle) Then
      m_sJobTitle = ""
    Else
      m_sJobTitle = CStr(drOperator!operator_jobtitle)
    End If
    m_sTimestamp = Format(CDate(drOperator!Operator_Timestamp), "yyyy-MM-dd HH:mm:ss")

    m_bSuperUser = oDB_Layer.CheckDBNullBool(drOperator!Operator_SuperUser)

    zGetOpPermissions(oDB_Layer)

    zGetOpAllocations(oDB_Layer, bAsSuperUser)

    oDB_Layer.Dispose()

  End Sub

  Private Sub zGetOpAllocations(pdb_Layer As DB_LayerGeneric, bAsSuperUser As Boolean)
    Dim sSql As String

    If m_bSuperUser = False And bAsSuperUser = False Then
      'Normal user Apply Restrinctions.
      sSql = "Select * From operator2organization as o2o"
      sSql += " LEFT JOIN organizations as o1 on o1.org_id = o2o.org_id "
      m_dtOrgs = pdb_Layer.RunSqlToTable(sSql)

      sSql = "Select * From operator2customer as o2c"
      sSql += " LEFT JOIN customers as c1 on c1.cust_id = o2c.cust_id "
      m_dtCusts = pdb_Layer.RunSqlToTable(sSql)

      sSql = "Select * From operator2locations as o2l"
      sSql += " LEFT JOIN locations as l1 on l1.loc_id = o2l.loc_id "
      m_dtCusts = pdb_Layer.RunSqlToTable(sSql)

    Else
      'Super user, run wild!!!
      sSql = "Select * From operator2organization as o2o"
      sSql += " RIGHT JOIN organizations as o1 on o1.org_id = o2o.org_id "
      m_dtOrgs = pdb_Layer.RunSqlToTable(sSql)

      sSql = "Select * From operator2customer as o2c"
      sSql += " RIGHT JOIN customers as c1 on c1.cust_id = o2c.cust_id "
      m_dtCusts = pdb_Layer.RunSqlToTable(sSql)

      sSql = "Select * From operator2locations as o2l"
      sSql += " RIGHT JOIN locations as l1 on l1.loc_id = o2l.loc_id "
      m_dtCusts = pdb_Layer.RunSqlToTable(sSql)

    End If




  End Sub


End Class
