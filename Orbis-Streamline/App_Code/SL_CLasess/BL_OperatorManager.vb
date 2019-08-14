Option Strict On

Imports System.Data

Public Class BL_OperatorManager
  Implements IDisposable

  Private m_pbl_Client As BL_Client
  Private m_pdl_Manager As DL_Manager
  Private m_pcl_Utils As New CL_Utils()
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_sMsg As String
  Private m_pDB_Layer As DB_LayerGeneric
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



  Public Sub New(ByVal pDL_manager As DL_Manager, ByVal pCL_ErrManager As CL_ErrManager, ByVal pBL_Client As BL_Client, pDB_Layer As DB_LayerGeneric)
    m_pdl_Manager = pDL_manager
    m_pcl_ErrManager = pCL_ErrManager
    m_pbl_Client = pBL_Client
    m_pDB_Layer = pDB_Layer
  End Sub
  Public ReadOnly Property sMsg() As String
    Get
      sMsg = m_sMsg
    End Get
  End Property

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
      m_pDB_Layer = Nothing
      m_pbl_Client = Nothing
      m_pcl_ErrManager = Nothing
    End If
  End Sub


  Public Function GetOperatorByID(ByVal iID As Int32) As DataRow
    'Get the user information.....
    Dim drTemp As DataRow
    Dim sSql As String

    Dim dicParms As New eDictionary

    dicParms.Add("@ID", "N:" & iID)
    dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)
    sSql = " SELECT * FROM sop_operators WHERE "
    sSql += " Operator_ID = @ID"
    sSql += " and Client_ID=@client"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetOperatorByEmail(ByVal sUserID As String, ByVal iClientID As Int32) As DataRow
    'Get the user information.....	
    Dim drTemp As DataRow
    Dim sSql As String

    Dim dicParms As New eDictionary

    dicParms.Add("@operator_email", "S:" & sUserID)
    dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)
    sSql = " SELECT * FROM sop_operators WHERE "
    sSql += " operator_email = @operator_email"
    sSql += " and Client_ID=@client"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetOperatorByPwdResetID(iClient As Int32, sID As String) As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim drOut As DataRow = Nothing

    dicParms.Add("client_id", "N:" & iClient)
    dicParms.Add("operator_PwdResetID", "S:" & sID)

    sSql = "Select * from sop_operators where "
    sSql += " client_id=@client_id and operator_PwdResetID=@operator_PwdResetID"

    drOut = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)

    Return drOut
  End Function
  Public Function GetOperators(ByVal bSortByLogon As Boolean, ByVal bActiveOnly As Boolean) As DataTable
    'Get the user information.....
    Dim dtTemp As DataTable
    Dim sSql As String

    Dim dicParms As New eDictionary

    dicParms.Add("@client", "N:" & m_pbl_Client.ClientID)

    sSql = "Select * from sop_operators "
    sSql += " where client_id=@client"
    If bActiveOnly Then
      sSql += " and Operator_Start_Date <= '" & Format(Now(), "yyyy-MM-dd HH:mm:ss") & "' "
      sSql += " and Operator_End_Date >= '" & Format(Now(), "yyyy-MM-dd HH:mm:ss") & "' "
    End If


    If bSortByLogon Then
      sSql += " Order by operator_email"
    Else
      sSql += " Order by Operator_ID"
    End If

    dtTemp = m_pDB_Layer.RunSqlToTable(sSql, , , dicParms)
    Return dtTemp

  End Function
  Public Function UpdateOperator(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return


    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sop_operators", dicParms)
    sSql += " Where operator_id=" & iID
    sSql += " and Operator_TimeStamp=@Operator_TimeStamp "

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet
  End Function
  Public Function GetAvailablePerms(ByVal iLoggedOnOpID As Int32, ByVal iEditOpID As Int32) As DataTable
    Dim dtPerms As DataTable
    Dim sSql As String

    Dim dicParms As New eDictionary

    dicParms.Add("@iLoggedOnOpID", "N:" & iLoggedOnOpID)
    dicParms.Add("@iEditOpID", "N:" & iEditOpID)

    sSql = "SELECT op1.permgroup_id, op1.perm_id, pgd.permgroup_desc, pd.perm_desc, op2.opPerm_level"
    sSql += " FROM sop_permgroupdesc AS pgd, sop_permdesc AS pd, sop_operatorperm AS op1"
    sSql += " LEFT JOIN sop_operatorperm AS op2 ON op1.permgroup_id = op2.permgroup_id AND op1.perm_id = op2.perm_id AND op2.operator_id=@iEditOpID"
    sSql += " WHERE(op1.operator_id=@iLoggedOnOpID)"
    sSql += " AND op1.opPerm_level=1 AND op1.permgroup_id = pgd.permgroup_id AND (op1.perm_id = pd.perm_id and op1.permgroup_id = pd.permgroup_id)"
    sSql += " ORDER BY op1.permgroup_id, op1.perm_id"

    dtPerms = m_pDB_Layer.RunSqlToTable(sSql, , , dicParms)
    Return dtPerms


  End Function
  Public Function GetMenuItems(iLevel As Int32) As DataTable
    'Get the user information.....
    Dim dtTemp As DataTable
    Dim sSql As String

    Dim dicParms As New eDictionary
    dicParms.Add("menu_Menuid", "N:" & iLevel)

    sSql = "SELECT * FROM sop_menus "
    sSql += " where menu_Menuid=@menu_Menuid"
    sSql += " Order by menu_MenuID, menu_SeqNo"

    dtTemp = m_pDB_Layer.RunSqlToTable(sSql, , , dicParms)
    Return dtTemp

  End Function
  Public Function bIgnoreOperatorSecurity() As Boolean
    Return CBool(System.Configuration.ConfigurationManager.AppSettings("DisableMenuSecurity"))
  End Function
  Public Function GetAllMenuItems() As DataTable
    'Get the user information.....
    Dim dtTemp As DataTable
    Dim sSql As String

    Dim dicParms As New eDictionary

    sSql = "SELECT * FROM sop_menus "
    sSql += " Order by menu_MenuID, menu_SeqNo"

    dtTemp = m_pDB_Layer.RunSqlToTable(sSql, , , dicParms)
    Return dtTemp

  End Function
  Public Function CheckAccess(ByVal sPage As String, ByVal dtMenu As DataTable, pBL_OP As BL_Operator) As Boolean
    Dim drMenu() As DataRow
    Dim bRequiresLogon As Boolean
    Dim bRet As Boolean = False
    drMenu = dtMenu.Select("menu_OutLink = '" & sPage & "'")

    If drMenu.GetUpperBound(0) > -1 Then
      'Permission is present....
      If m_pDB_Layer.CheckDBNullBool(drMenu(0)!Menu_requires_login) Then
        bRequiresLogon = True
      Else
        bRequiresLogon = False
      End If
    Else
      'No permission, no access.....
      bRequiresLogon = True
    End If



    If pBL_OP.LoggedOn Then
      If bRequiresLogon Then
        If drMenu.GetUpperBound(0) > -1 Then
          If m_pDB_Layer.CheckDBNullInt(drMenu(0)!PermGroup_ID) <> -1 Then
            If Not drMenu(0).IsNull("PermGroup_ID") Then
              bRet = pBL_OP.HasAccess(m_pDB_Layer.CheckDBNullInt(drMenu(0)!PermGroup_ID), m_pDB_Layer.CheckDBNullInt(drMenu(0)!Perm_ID))
            Else
              bRet = True
            End If
          Else
            'Auto allow...
            bRet = True
          End If
        Else
          'No permission, no access!!!
          bRet = False
        End If
      Else
        bRet = True
      End If
    Else
      If bRequiresLogon Then
        bRet = False
      Else
        bRet = True
      End If
    End If

    Return bRet



  End Function
  Public Sub CheckPageAccess(ByVal oDB_Layer As DB_LayerGeneric, pBL_Op As BL_Operator)
    Try
      Dim dtMenuItems As DataTable

      If HttpContext.Current.Session("MenuItems") Is Nothing Then
        Dim sSql As String

        'Get the initial menu options...
        sSql = "SELECT * FROM sop_Menus Order by Menu_ID, Seq_No"
        dtMenuItems = oDB_Layer.RunSqlToTable(sSql)
        HttpContext.Current.Session("MenuItems") = dtMenuItems
      Else
        dtMenuItems = CType(HttpContext.Current.Session("MenuItems"), DataTable)
      End If

      Dim sPage As String
      Dim uURI As Uri
      uURI = HttpContext.Current.Request.Url

      sPage = uURI.Segments(uURI.Segments.GetUpperBound(0))
      If Not CheckAccess(sPage, dtMenuItems, pBL_Op) Then
        HttpContext.Current.Response.Redirect("Default.aspx", True)
      End If

    Catch ex As Threading.ThreadAbortException
      'Do nothing..



    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex
    End Try
  End Sub


End Class
