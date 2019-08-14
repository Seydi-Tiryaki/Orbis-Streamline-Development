Imports Microsoft.VisualBasic

Public Class BL_CustomerManager
  Implements IDisposable

  Private m_pbl_Client As BL_Client
  Private m_pdl_Manager As DL_Manager
  Private m_pcl_Utils As New CL_Utils()
  Private m_pcl_ErrManager As CL_ErrManager
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
      m_pDB_Layer = Nothing
      m_pbl_Client = Nothing
      m_pcl_ErrManager = Nothing
    End If
  End Sub
#End Region

  Public Function GetCustomerByExternalID(iClient As Int32, sID As String, ifeed As Int32) As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim drOut As DataRow = Nothing

    dicParms.Add("client_id", "N:" & iClient)
    dicParms.Add("cust_ExternalID", "S:" & sID)
    dicParms.Add("feed_id", "N:" & ifeed)


    sSql = "Select * from sop_customers where "
    sSql += " client_id=@client_id and cust_ExternalID=@cust_ExternalID and feed_id=@feed_id"

    drOut = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)

    Return drOut
  End Function
  Public Function GetCustomerByPwdResetID(iClient As Int32, sID As String, ifeed As Int32) As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim drOut As DataRow = Nothing

    dicParms.Add("client_id", "N:" & iClient)
    dicParms.Add("cust_PwdResetID", "S:" & sID)
    dicParms.Add("feed_id", "N:" & ifeed)


    sSql = "Select * from sop_customers where "
    sSql += " client_id=@client_id and cust_PwdResetID=@cust_PwdResetID and feed_id=@feed_id"

    drOut = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)

    Return drOut
  End Function

  Public Function GetCustomerByPaymentEmail(iClient As Int32, sEmail As String, iFeedID As Int32) As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim drOut As DataRow = Nothing

    dicParms.Add("client_id", "N:" & iClient)
    dicParms.Add("cust_Email", "S:" & sEmail)
    dicParms.Add("feed_id", "N:" & iFeedID)


    sSql = "Select * from sop_customers where "
    sSql += " client_id=@client_id and cust_PayerEmail=@cust_Email and feed_id=@feed_id"

    drOut = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)

    Return drOut
  End Function
  Public Function GetCustomerByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String

    sSql = "Select * from customers where cust_ID=" & iID

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql)
    Return drTemp

  End Function
  Public Function GetCustomerByNameAndOrg(sName As String, ByVal iOrgID As Int32, iExcludeCustID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("cust_Name", "S:" & sName)
    dicParms.Add("org_ID", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iExcludeCustID)

    sSql = "Select * from customers where org_ID=@org_ID and cust_Name=@cust_Name"
    If iExcludeCustID <> -1 Then
      sSql += " and cust_id<> @cust_id"
    End If

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function InsertCustomer(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "customers", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "customers", dicParms)
    Return iID
  End Function
  Public Function UpdateCustomer(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return


    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "customers", dicParms)
    sSql += " Where cust_id=" & iID
    sSql += " and cust_Timestamp=@cust_Timestamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet
  End Function
  Public Function GetAllOrganizations(bSortByname As Boolean, ByVal bActiveOnly As Boolean, pbl_Operator As BL_Operator, iOpFilterID As Int32) As DataTable
    Try
      Dim sSql As String
      Dim dicParms As New eDictionary
      Dim dtList As DataTable
      Dim sWhere As String = ""

      sSql = "SELECT o1.*, o2of.* from organizations as o1 "
      sSql += " LEFT JOIN operator2organization as o2o on o2o.org_id = o1.org_id and o2o.operator_id=" & pbl_Operator.ID
      sSql += " LEFT JOIN operator2organization as o2of on o2of.org_id = o1.org_id and o2of.operator_id=" & iOpFilterID
      If bActiveOnly Then
        sWhere += " WHERE org_active=-1"
      End If

      If pbl_Operator.bIsSuperUser = False Then
        'We must filter the Orgs based of access....
        If sWhere = "" Then
          sWhere += " WHERE op2org_Timestamp is not NULL"
        Else
          sWhere += " and o2o.op2org_Timestamp is not NULL"
        End If
      End If

      If sWhere <> "" Then
        sSql += sWhere
      End If

      If bSortByname Then
        sSql += " order by org_Name"
      Else
        sSql += " order by org_id"
      End If

      dtList = m_pDB_Layer.RunSqlToTable(sSql)
      Return dtList

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex

    End Try
  End Function
  Public Function GetAllGeneralLocations(bSortByname As Boolean) As DataTable
    Try
      Dim sSql As String
      Dim dicParms As New eDictionary
      Dim dtList As DataTable
      Dim sWhere As String = ""

      sSql = "SELECT * from generallocations as g1 "
      sSql += " LEFT JOIN timezones as tz on g1.timezone_ID = tz.timezone_ID"

      If bSortByname Then
        sSql += " order by genloc_name"
      Else
        sSql += " order by genloc_ID"
      End If

      dtList = m_pDB_Layer.RunSqlToTable(sSql)
      Return dtList

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex

    End Try
  End Function
  Public Function GetAllCustomers(ByVal bActiveOnly As Boolean, pbl_operator As BL_Operator, iOpFilterID As Int32) As DataTable
    Try
      Dim sSql As String
      Dim dicParms As New eDictionary
      Dim dtList As DataTable
      Dim sWhere As String = ""

      sSql = "SELECT c1.*, o1.*, o2cf.* from customers as c1"
      sSql += " LEFT JOIN organizations as o1 on o1.org_id = c1.org_id"
      sSql += " LEFT JOIN operator2organization as o2o on o2o.org_id = c1.org_id and o2o.operator_id=" & pbl_operator.ID
      sSql += " LEFT JOIN operator2customer as o2c on o2c.cust_id = c1.cust_id and o2c.operator_id=" & pbl_operator.ID
      sSql += " LEFT JOIN operator2customer as o2cf on o2cf.cust_id = c1.cust_id and o2cf.operator_id=" & iOpFilterID
      If bActiveOnly Then
        sWhere += " WHERE cust_active=-1"
      End If

      'Exclude customers for org we do not have access to.
      If sWhere = "" Then
        sWhere += " WHERE op2org_Timestamp is not null"
      Else
        sWhere += " and op2org_Timestamp is not null"
      End If
      sSql += " "

      If pbl_operator.bIsSuperUser = False Then
        'We must filter the customers based of access....
        If sWhere = "" Then
          sWhere += " WHERE "
        Else
          sWhere += " and "
        End If
        sWhere += " o2c.op2cust_Timestamp is not NULL"
      End If

      If sWhere <> "" Then
        sSql += sWhere
      End If

      sSql += " order by org_name, cust_Name"

      dtList = m_pDB_Layer.RunSqlToTable(sSql)

      Return dtList

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex

    End Try
  End Function
  Public Function GetAllLocations(ByVal bActiveOnly As Boolean, pbl_operator As BL_Operator, iOpFilterID As Int32, iCustomerFilter As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""

    sSql = "Select l1.*, c1.*,o1.*,g1.*,tz.*,o2lf.* from locations as l1 "
    sSql += " LEFT JOIN customers as c1 on c1.cust_id = l1.cust_id"
    sSql += " LEFT JOIN organizations as o1 on o1.org_id = c1.org_id"
    sSql += " LEFT JOIN generallocations as g1 on l1.genloc_ID = g1.genloc_ID"
    sSql += " LEFT JOIN timezones as tz on g1.timezone_ID = tz.timezone_ID"
    sSql += " LEFT JOIN operator2organization as o2o on o2o.org_id = c1.org_id and o2o.operator_id=" & pbl_operator.ID
    sSql += " LEFT JOIN operator2customer as o2c on o2c.cust_id = c1.cust_id and o2c.operator_id=" & pbl_operator.ID
    sSql += " LEFT JOIN operator2locations as o2l on o2l.loc_id = l1.loc_id and o2l.operator_id=" & pbl_operator.ID
    sSql += " LEFT JOIN operator2locations as o2lf on o2lf.loc_id = l1.loc_id and o2lf.operator_id=" & iOpFilterID
    If bActiveOnly Then
      sWhere += " WHERE loc_active=-1"
    End If

    If iCustomerFilter <> -1 Then
      If sWhere = "" Then
        sWhere += " WHERE c1.cust_id = " & iCustomerFilter
      Else
        sWhere += " and c1.cust_id = " & iCustomerFilter
      End If

    End If

    'Exclude customers for org we do not have access to.
    If sWhere = "" Then
      sWhere += " WHERE op2cust_Timestamp is not null"
    Else
      sWhere += " and op2cust_Timestamp is not null"
    End If
    sSql += " "

    If pbl_operator.bIsSuperUser = False Then
      'We must filter the customers based of access....
      If sWhere = "" Then
        sWhere += " WHERE "
      Else
        sWhere += " and "
      End If
      sWhere += " o2l.op2loc_Timestamp is not NULL"
    End If

    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " order by org_name, cust_Name, loc_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn


  End Function
  Public Function GetAllSubLocations(ByVal bActiveOnly As Boolean, pbl_operator As BL_Operator, iOpFilterID As Int32, iLocationFilter As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""

    sSql = "Select * from sublocations as sl1,locations as l1 "
    sSql += " LEFT JOIN customers as c1 on c1.cust_id = l1.cust_id"
    sSql += " LEFT JOIN organizations as o1 on o1.org_id = c1.org_id"
    sSql += " LEFT JOIN operator2organization as o2o on o2o.org_id = c1.org_id and o2o.operator_id=" & iOpFilterID
    sSql += " LEFT JOIN operator2customer as o2c on o2c.cust_id = c1.cust_id and o2c.operator_id=" & iOpFilterID
    sSql += " LEFT JOIN operator2locations as o2l on o2l.loc_id = l1.loc_id and o2l.operator_id=" & iOpFilterID

    sWhere += " Where sl1.loc_id = l1.loc_id"


    If iLocationFilter <> -1 Then
      sWhere += " and l1.loc_id = " & iLocationFilter
    End If

    If bActiveOnly Then
      sWhere += " and loc_active=-1"
    End If

    'Exclude customers for org we do not have access to.
    If sWhere = "" Then
      sWhere += " WHERE op2cust_Timestamp is not null"
    Else
      sWhere += " and op2cust_Timestamp is not null"
    End If
    sSql += " "

    If pbl_operator.bIsSuperUser = False Then
      'We must filter the customers based of access....
      If sWhere = "" Then
        sWhere += " WHERE "
      Else
        sWhere += " and "
      End If
      sWhere += " op2loc_Timestamp is not NULL"
    End If

    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " order by org_name, cust_Name, loc_name, subloc_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn


  End Function
  Public Function GetLocationByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String

    sSql = "Select * from locations where Loc_ID=" & iID

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql)
    Return drTemp

  End Function
  Public Function GetLocationByNameAndCust(sName As String, ByVal iCustID As Int32, iExcludeLocationID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("loc_name", "S:" & sName)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_ID", "N:" & iExcludeLocationID)

    sSql = "Select * from locations where cust_id=@cust_id and loc_name=@loc_name"
    If iExcludeLocationID <> -1 Then
      sSql += " and loc_ID<> @loc_ID"
    End If

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function InsertLocation(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "locations", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "locations", dicParms)
    Return iID
  End Function
  Public Function UpdateLocation(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return


    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "locations", dicParms)
    sSql += " Where loc_id=" & iID
    sSql += " and loc_Timestamp=@loc_Timestamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet
  End Function
  Public Function GetSubLocationByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String

    sSql = "Select * from sublocations where subloc_ID=" & iID

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql)
    Return drTemp

  End Function
  Public Function GetSubLocationByNameAndLoc(sName As String, ByVal iLocID As Int32, iExcludeSubLocationID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("subloc_name", "S:" & sName)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_ID", "N:" & iExcludeSubLocationID)

    sSql = "Select * from sublocations where loc_id=@loc_id and subloc_name=@subloc_name"
    If iExcludeSubLocationID <> -1 Then
      sSql += " and subloc_ID<> @subloc_ID"
    End If

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function InsertSubLocation(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "sublocations", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "sublocations", dicParms)
    Return iID
  End Function
  Public Function UpdateSubLocation(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return


    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "sublocations", dicParms)
    sSql += " Where subloc_id=" & iID
    sSql += " and subloc_Timestamp=@subloc_Timestamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet
  End Function

  Public Function GetAllCustomersByOrgNoSec(iSelOrg As Int32, dicAvailableOrg As eDictionary) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from customers as c1 "
    If iSelOrg <> -1 Then
      sSql += " Where c1.org_id = " & iSelOrg.ToString
    End If
    sSql += " order by cust_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetAllLocationsByCustomerNoSec(iSelCust As Int32, dicAvailableLoc As eDictionary) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from locations as l1 "
    If iSelCust <> -1 Then
      sSql += " Where l1.cust_id = " & iSelCust.ToString
    End If
    sSql += " order by loc_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetAllsubLocationsByLocation(iSelLoc As Int32, pbl_operator As BL_Operator, iOpFilterID As Int32, iCustomerFilter As Int32, iLocationFilter As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""

    sSql = "Select * from sublocations as sl1 "
    sSql += " left join locations as l1 on l1.loc_id = sl1.loc_id"
    sSql += " LEFT JOIN customers as c1 on c1.cust_id = l1.cust_id"
    sSql += " LEFT JOIN organizations as o1 on o1.org_id = c1.org_id"
    sSql += " LEFT JOIN operator2organization as o2o on o2o.org_id = c1.org_id and o2o.operator_id=" & iOpFilterID
    sSql += " LEFT JOIN operator2customer as o2c on o2c.cust_id = c1.cust_id and o2c.operator_id=" & iOpFilterID
    sSql += " LEFT JOIN operator2locations as o2l on o2l.loc_id = l1.loc_id and o2l.operator_id=" & iOpFilterID

    If iSelLoc <> -1 Then
      sWhere += " Where sl1.loc_ID = " & iSelLoc.ToString
    End If

    If iCustomerFilter <> -1 Then
      If sWhere = "" Then
        sWhere += " WHERE c1.cust_id = " & iCustomerFilter
      Else
        sWhere += " and c1.cust_id = " & iCustomerFilter
      End If

    End If

    If iLocationFilter <> -1 Then
      If sWhere = "" Then
        sWhere += " WHERE l1.loc_id = " & iLocationFilter
      Else
        sWhere += " and l1.loc_id = " & iLocationFilter
      End If

    End If

    'Exclude customers without access
    If sWhere = "" Then
      sWhere += " WHERE op2cust_Timestamp is not null"
    Else
      sWhere += " and op2cust_Timestamp is not null"
    End If

    'Exclude Locs without access
    If sWhere = "" Then
      sWhere += " WHERE op2loc_Timestamp is not null"
    Else
      sWhere += " and op2loc_Timestamp is not null"
    End If

    'Exclude orgs we don't have access....
    If sWhere = "" Then
      sWhere += " WHERE op2org_Timestamp is not null"
    Else
      sWhere += " and op2org_Timestamp is not null"
    End If


    sSql += " "

    If pbl_operator.bIsSuperUser = False Then
      'We must filter the location  based of access....
      If sWhere = "" Then
        sWhere += " WHERE "
      Else
        sWhere += " and "
      End If
      sWhere += " op2loc_Timestamp is not NULL"
    End If


    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " order by subloc_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function

  Public Function InsertOp2Org(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "operator2organization", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "operator2organization", dicParms)
    Return iID
  End Function


  Public Function DeleteOp2Org(iOp As Int32, iOrg As Int32) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("operator_id", "N:" & iOp)
    dicParms.Add("org_id", "N:" & iOrg)

    'Fields....
    sSql = "delete from operator2organization where operator_id=@operator_id and org_id=@org_id"

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, "", dicParms)
    Return iRowsAffected

  End Function

  Public Function InsertOp2Cust(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "operator2customer", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "operator2customer", dicParms)
    Return iID
  End Function


  Public Function DeleteOp2Cust(iOp As Int32, iCust As Int32) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("operator_id", "N:" & iOp)
    dicParms.Add("cust_id", "N:" & iCust)

    'Fields....
    sSql = "delete from operator2customer where operator_id=@operator_id and cust_id=@cust_id"

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, "", dicParms)
    Return iRowsAffected

  End Function

  Public Sub DeleteOrphanedOp2Cust(iOpID As Int32)
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim iRowsAffected As Int32 = 0

    dicParms.Add("operator_id", "N:" & iOpID)

    sSql = "delete operator2customer from customers "
    sSql += " left join operator2customer  on operator2customer.cust_id = customers.cust_id and operator2customer.operator_id = @operator_id"
    sSql += " left join operator2organization on operator2organization.org_id = customers.org_id and operator2organization.operator_id = @operator_id"
    sSql += " where op2org_Timestamp is null and op2cust_Timestamp IS NOT null"

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, "", dicParms)



  End Sub

  Public Function InsertOp2Loc(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "operator2locations", dicParms)

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "operator2locations", dicParms)
    Return iID
  End Function


  Public Function DeleteOp2Loc(iOp As Int32, iLoc As Int32) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("operator_id", "N:" & iOp)
    dicParms.Add("loc_id", "N:" & iLoc)

    'Fields....
    sSql = "delete from operator2locations where operator_id=@operator_id and loc_id=@loc_id"

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, "", dicParms)
    Return iRowsAffected

  End Function

  Public Sub DeleteOrphanedOp2Loc(iOpID As Int32)
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim iRowsAffected As Int32 = 0

    dicParms.Add("operator_id", "N:" & iOpID)

    sSql = "delete operator2locations from Locations "
    sSql += " left join operator2customer on operator2customer.cust_id = Locations.cust_id and operator2customer.operator_id = @operator_id"
    sSql += " left join operator2locations on operator2locations.loc_id = locations.loc_id and operator2locations.operator_id = @operator_id"
    sSql += " where op2loc_Timestamp is not null and op2cust_Timestamp is null"

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False, "", dicParms)



  End Sub

End Class
