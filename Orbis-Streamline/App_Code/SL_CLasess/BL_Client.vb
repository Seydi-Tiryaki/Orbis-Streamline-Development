Imports System.IO

<Serializable> Public Class BL_Client

  Private m_pdl_Manager As DL_Manager
  Private m_pcl_ErrManager As CL_ErrManager
  'Private m_drClient As DataRow
  Private m_iClient As Int32
  Private m_sMsg As String = ""

  Public Enum enClientStatus
    enCreated = 0
    enActive = 1
    enBlocked = 2
    enDeactivated = 3
    enOverDue = 4

    enReset = 99
  End Enum
#Region "Private Class Variables"
  Private m_lClient As Int32
  Private m_sCompanyName As String
  Private m_sContactName As String
  Private m_sExternalID As String


  Private m_sClientPath As String
  Private m_sClientConnString As String
  Private m_sClientsTopPath As String
  Private m_sClientsTopUrl As String
  Private m_sTimeStamp As String
  Private m_sTempFolderFullPath As String
  Private m_sTempPublicFolderFullPath As String
  Private m_sHttpUrl As String
  Private m_enStatus As enClientStatus
  Private m_sEmail As String


#End Region

  Public Sub New(ByVal pDL_manager As DL_Manager, ByVal pCL_ErrManager As CL_ErrManager)
    m_pdl_Manager = pDL_manager
    m_pcl_ErrManager = pCL_ErrManager
  End Sub
#Region "Properties"
  Public ReadOnly Property sMsg() As String
    Get
      sMsg = m_sMsg
    End Get
  End Property
  Public Property pdl_Manager() As DL_Manager
    Get
      pdl_Manager = m_pdl_Manager
    End Get
    Set(ByVal Value As DL_Manager)
      m_pdl_Manager = Value
      'If Not m_pdl_Manager Is Nothing Then
      '  m_pDb_Layer = New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
      'Else
      '  m_pDb_Layer = Nothing
      'End If
    End Set
  End Property
  Public Property pCL_ErrManager() As CL_ErrManager
    Get
      pCL_ErrManager = m_pcl_ErrManager
    End Get
    Set(ByVal Value As CL_ErrManager)
      m_pcl_ErrManager = Value
    End Set
  End Property
  Public Property ClientID() As Int32
    Get
      ClientID = m_lClient
    End Get
    Set(ByVal Value As Int32)
      m_lClient = Value
    End Set
  End Property
  Public Property CompanyName() As String
    Get
      CompanyName = m_sCompanyName
    End Get
    Set(ByVal Value As String)
      m_sCompanyName = Value
    End Set
  End Property
  Public Property ContactName() As String
    Get
      ContactName = m_sContactName
    End Get
    Set(ByVal Value As String)
      m_sContactName = Value
    End Set
  End Property
  Public Property ClientStatus() As enClientStatus
    Get
      ClientStatus = m_enStatus
    End Get
    Set(ByVal Value As enClientStatus)
      m_enStatus = Value
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

  Public Property HttpUrl() As String
    Get
      HttpUrl = m_sHttpUrl
    End Get
    Set(ByVal Value As String)
      m_sHttpUrl = Value
    End Set
  End Property
  Public ReadOnly Property TempFolderFullPath() As String
    Get
      TempFolderFullPath = m_sTempFolderFullPath
    End Get
  End Property
  Public ReadOnly Property TempPublicFolderFullPath() As String
    Get
      TempPublicFolderFullPath = m_sTempPublicFolderFullPath
    End Get
  End Property

  'Public ReadOnly Property drClient() As DataRow
  '  Get
  '    drClient = m_drClient
  '  End Get
  'End Property
#End Region

  Public Function GetClientIDFromHostname(ByVal sUrl As String) As Int32
    'Try
    Dim iRet As Int32
    Dim sKey As String = "SopHost2Store_" & sUrl.ToLower

    If Not System.Configuration.ConfigurationManager.AppSettings(sKey) Is Nothing Then
      iRet = CInt(System.Configuration.ConfigurationManager.AppSettings(sKey))
    Else
      iRet = -1
    End If

    Return iRet

    'Catch ex As Exception
    '  m_pcl_ErrManager.AddError(ex)
    '  Throw ex
    'End Try
  End Function
  Public Function GetClientIDFromInvPrefix(ByVal sPrefix As String) As Int32
    'Try
    Dim iRet As Int32
    Dim sKey As String = "SopInv2Client_" & sPrefix.ToLower

    If Not System.Configuration.ConfigurationManager.AppSettings(sKey) Is Nothing Then
      iRet = CInt(System.Configuration.ConfigurationManager.AppSettings(sKey))
    Else
      iRet = -1
    End If

    Return iRet

    'Catch ex As Exception
    '  m_pcl_ErrManager.AddError(ex)
    '  Throw ex
    'End Try
  End Function

  Public Function ValidClientID() As Boolean
    'Validate the Client ID

    Dim drClient As DataRow = Nothing
    Dim sSql As String
    Dim pDb_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    Dim bRet As Boolean

    'Select by client ID
    'Dim sSqlParams As SqlParameter() = {New SqlParameter("@Client_ID", m_lClient)}
    sSql = "SELECT c1.* FROM sop_clients as c1 "
    sSql += "WHERE c1.Client_ID = " & m_lClient

    drClient = pDb_Layer.RunSqlToDataRow(sSql)
    If Not drClient Is Nothing Then
      'We have a client...
      If IsDBNull(drClient!Client_ExternalID) Then
        'We need to allocate a new external ID!
        Dim dicParms As New eDictionary
        Dim sSqlUpdate As String
        Dim iRows As Int32
        dicParms.Add("Client_ExternalID", "S:" & Guid.NewGuid.ToString)
        sSqlUpdate = pDb_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_clients", dicParms)
        sSqlUpdate += " Where client_id=" & CStr(drClient!Client_id)
        pDb_Layer.RunSqlNonQuery(sSqlUpdate, iRows, False, "", dicParms)

        'Reread the data...
        drClient = pDb_Layer.RunSqlToDataRow(sSql)

      End If

      m_sMsg = ""
      'm_drClient = drClient
      zPopulateFromRow(drClient)

      'Get the site Settings....




      bRet = True
    Else
      'Unknown Client...
      m_sMsg = "Unknown Client: " & m_lClient
      bRet = False
    End If

    pDb_Layer.Dispose()
    pDb_Layer = Nothing


    Return bRet

  End Function
  Private Sub zPopulateFromRow(drClient As DataRow)
    Dim oDB_Layer As New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)
    With drClient
      m_lClient = CInt(.Item("Client_ID"))
      m_sHttpUrl = oDB_Layer.CheckDBNullStr(.Item("Client_HttpUrl"))
      m_sCompanyName = oDB_Layer.CheckDBNullStr(.Item("Client_Company_Name"))
      m_sContactName = oDB_Layer.CheckDBNullStr(.Item("Client_Contact_Name"))
      m_sExternalID = oDB_Layer.CheckDBNullStr(.Item("Client_ExternalID"))
      m_enStatus = CType(.Item("Client_status"), BL_Client.enClientStatus)
      m_sEmail = oDB_Layer.CheckDBNullStr(.Item("Client_Email"))

      m_sTimeStamp = Format(oDB_Layer.CheckDBNullDate(.Item("Client_timestamp")), "yyyy-MM-dd HH:mm:ss")


    End With

    zCheckClientFolders()

    oDB_Layer.Dispose()

  End Sub
  Private Sub zCheckClientFolders()

    'Check temp folder....
    m_sTempFolderFullPath = m_pdl_Manager.sFilePath & "App_Data\Temp\" & Format(m_lClient, "00000") & "-" & m_sExternalID & "\"
    Dim diTemp1 As New DirectoryInfo(m_sTempFolderFullPath)
    If Not diTemp1.Exists Then
      diTemp1.Create()
    End If
    diTemp1 = Nothing


    'Check Client Top Folder....
    m_sClientsTopPath = m_pdl_Manager.sFilePath & "clients\" & Format(m_lClient, "00000") & "-" & m_sExternalID & "\"
    m_sClientsTopUrl = "clients/" & Format(m_lClient, "00000") & "-" & m_sExternalID & "/"
    Dim diTemp3 As New DirectoryInfo(m_sClientsTopPath)
    If Not diTemp3.Exists Then
      diTemp3.Create()
    End If
    diTemp3 = Nothing

  End Sub

End Class
